using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.FavouriteDtos;
using MiniApi.Application.DTOs.OrderDtos;
using MiniApi.Application.DTOs.PasswordDtos;
using MiniApi.Application.DTOs.ProductDtos;
using MiniApi.Application.DTOs.ReviewDtos;
using MiniApi.Application.DTOs.UserDtos;
using MiniApi.Application.DTOs.Users;
using MiniApi.Application.Shared;
using MiniApi.Application.Shared.Settings;
using MiniApi.Domain.Entities;
using MiniApi.Persistence.Repositories;

namespace MiniApi.Persistence.Services;

public class UserService : IUserService
{
    private UserManager<AppUser> _userManager { get; }
    private IEmailService _mailService { get; }

    private readonly RoleManager<IdentityRole> _roleManager;
    private SignInManager<AppUser> _singInManager { get; }
    private JWTSettings _jwtSetting { get; }
    private readonly IOrderRepository _orderRepository;
    private readonly IFavouriteRepository _favouriteRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;

    public UserService(UserManager<AppUser> userManager,
        SignInManager<AppUser> singInManager,
        IOptions<JWTSettings> jWTSettings,
        RoleManager<IdentityRole> roleManager,
        IEmailService mailService,
        IOrderRepository orderRepository,
        IFavouriteRepository favouriteRepository,
        IReviewRepository reviewRepository,
        IProductRepository productRepository)
    {
        _userManager = userManager;
        _singInManager = singInManager;
        _jwtSetting = jWTSettings.Value;
        _roleManager = roleManager;
        _mailService = mailService;
        _orderRepository = orderRepository;
        _favouriteRepository = favouriteRepository;
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }
    public async Task<BaseResponse<string>> Register(UserRegisterDto dto)
    {
        var existedEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existedEmail is not null)
        {
            return new BaseResponse<string>("This account already exist", HttpStatusCode.BadRequest);
        }
        AppUser newUser = new()
        {
            Email = dto.Email,
            FullName = dto.FullName,
            UserName = dto.Email,
        };

        IdentityResult identityResult = await _userManager.CreateAsync(newUser, dto.Password);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors;
            StringBuilder errorMassege = new();
            foreach (var error in errors)
            {
                errorMassege.Append(error.Description + ";");
            }
            return new(errorMassege.ToString(), HttpStatusCode.BadRequest);
        }
        var roleName = dto.Role.ToString();
        await _userManager.AddToRoleAsync(newUser, roleName);
        string confirmEmailLink = await GetEmailConfirmLink(newUser);
        await _mailService.SendEmailAsync(new List<string> { newUser.Email }, "Email Confitmation",
        confirmEmailLink);

        return new("Successfully created", true, HttpStatusCode.Created);
    }

    public async Task<BaseResponse<TokenResponse>> Login(UserLoginDto dto)
    {
        var existedUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existedUser is null)
        {
            return new("Email or password os wrong.", HttpStatusCode.NotFound);
        }

        if (existedUser.EmailConfirmed)
        {
            return new("Please Confirm your email", HttpStatusCode.BadRequest);
        }

        SignInResult signInResult = await _singInManager.PasswordSignInAsync
            (dto.Email, dto.Password, true, true);

        if (!signInResult.Succeeded)
        {
            return new("Email or password os wrong.", null, HttpStatusCode.NotFound);
        }
        var token = await GenerateTokensAsync(existedUser);

        return new("Token generated", token, HttpStatusCode.OK);

    }


    public async Task<BaseResponse<string>> AddRole(UserAddRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null)
        {
            return new BaseResponse<string>("User not found", HttpStatusCode.NotFound);
        }

        var roleNames = new List<string>();

        foreach (var roleId in dto.RoleId.Distinct())
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                return new BaseResponse<string>($"Role With Id'{roleId}' not found", HttpStatusCode.NotFound);
            }

            if (!await _userManager.IsInRoleAsync(user, role.Name!))
            {
                var result = await _userManager.AddToRoleAsync(user, role.Name!);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new BaseResponse<string>($"Failed to add role '{role.Name}'to user:{errors}", HttpStatusCode.BadRequest);
                }
                roleNames.Add(role.Name!);
            }

        }
        return new BaseResponse<string>($"Succesfuly added roles:{string.Join(", ", roleNames)}", HttpStatusCode.OK);

    }

    public async Task<BaseResponse<string>> ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new BaseResponse<string>("User not found", null, HttpStatusCode.NotFound);

        // Bura da decode yazsan, 100% əmin olarsan!
        token = WebUtility.UrlDecode(token);

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            return new BaseResponse<string>($"Email confirmation failed: {errorMessages}", null, HttpStatusCode.BadRequest);
        }

        return new BaseResponse<string>("Email confirmed successfully", null, HttpStatusCode.OK);
    } 

    public async Task<BaseResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return new("Invalid access token", null, HttpStatusCode.BadRequest);

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
            return new("User not faund", null, HttpStatusCode.NotFound);



        if (user.RefreshToken is null || user.RefreshToken != request.RefreshToken ||
            user.ExpireDate < DateTime.UtcNow)
            return new("Invalid refresh token", null, HttpStatusCode.BadRequest);


        //Generate new tokens
        var tokenResponse = await GenerateTokensAsync(user);
        return new("Refreshed", tokenResponse, HttpStatusCode.OK);
    }


    private async Task<string> GetEmailConfirmLink(AppUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = $"https://localhost:7071/api/Accounts/ConfirmEmail?userId={user.Id}&token={HttpUtility.UrlEncode(token)}";
        Console.WriteLine("Confirm Email Link" + token);
        return link;
    }
    public async Task<BaseResponse<string>> ForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            return new("User not found or email not confirmed", HttpStatusCode.BadRequest);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"https://localhost:7071/api/Accounts/ResetPassword?userId={user.Id}&token={HttpUtility.UrlEncode(token)}";

        await _mailService.SendEmailAsync(new[] { user.Email }, "Reset Password", resetLink);

        return new("Reset link sent to your email.", HttpStatusCode.OK);
    }

    public async Task<BaseResponse<string>> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return new("User not found", HttpStatusCode.NotFound);
        var decodedToken = Uri.UnescapeDataString(dto.Token);
        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
            return new(errorMessage, HttpStatusCode.BadRequest);
        }

        return new("Password reset successfully", HttpStatusCode.OK);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // expired token üçün bu false olmalıdır

            ValidIssuer = _jwtSetting.Issuer,
            ValidAudience = _jwtSetting.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.SecretKey))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken &&
                jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<TokenResponse> GenerateTokensAsync(AppUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSetting.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!)
        };
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                var permissionClaims = roleClaims.Where(c => c.Type == "Permission").Distinct();

                foreach (var permissionClaim in permissionClaims)
                {
                    claims.Add(new Claim("Permission", permissionClaim.Value));
                }
            }
        }
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSetting.ExpiryMinutes),
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);


        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiryDate = DateTime.UtcNow.AddHours(2);
        user.RefreshToken = refreshToken;
        user.ExpireDate = refreshTokenExpiryDate;
        await _userManager.UpdateAsync(user);

        return new TokenResponse
        {
            Token = jwt,
            RefreshToken = refreshToken,
            ExpireDate = tokenDescriptor.Expires!.Value,
        };
    }
    public async Task<BaseResponse<UserProfileDto>> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new BaseResponse<UserProfileDto>("User not found", HttpStatusCode.NotFound);

        var userProfile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            // Lazım olsa əlavə sahələr əlavə et
        };

        return new BaseResponse<UserProfileDto>("Success", userProfile, HttpStatusCode.OK);
    }
    public async Task<BaseResponse<List<UserProfileDto>>> GetAllUsersAsync()
    {
        var users = _userManager.Users.ToList();

        var userDtos = users.Select(user => new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName
            // Burada lazım olsa əlavə sahələr əlavə edə bilərsən
        }).ToList();

        return new BaseResponse<List<UserProfileDto>>("Success", userDtos, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<UserProfileDto>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new BaseResponse<UserProfileDto>("User not found", HttpStatusCode.NotFound);

        var userProfile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName
            // Əlavə sahələr əlavə etmək olar
        };

        return new BaseResponse<UserProfileDto>("Success", userProfile, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<UserFullProfileDto>> GetFullProfileAsync(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return new("Invalid token", HttpStatusCode.BadRequest);

        var jwtToken = handler.ReadJwtToken(token);
        var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid")?.Value;
        if (userId is null)
            return new("User not found", HttpStatusCode.NotFound);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new("User not found", HttpStatusCode.NotFound);

        var roles = (await _userManager.GetRolesAsync(user)).ToList();

        // Məhsullar (əgər sellerdirsə)
        List<ProductGetDto> products = new();
        if (roles.Contains("Seller"))
        {
            products = await _productRepository.GetByFiltered(x => x.OwnerId == userId)
                .Select(x => new ProductGetDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Title = x.Title,
                    Price = x.Price,
                    // digər mappinglər
                }).ToListAsync();
        }

        // Favoritlər
        var favourites = await _favouriteRepository.GetByFiltered(x => x.UserId == userId, new[] { (Expression<Func<Favourite, object>>)(x => x.Product) })
            .Select(x => new FavouriteGetDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Title
            }).ToListAsync();

        var orders = await _orderRepository.GetByFiltered(x => x.BuyerId == userId, new[] { (Expression<Func<Order, object>>)(x => x.OrderProducts) })
            .Select(x => new OrderGetDto
            {
                 Id = x.Id,
                 OrderDate = x.OrderDate,
                 Status = x.Status,
                 Products = x.OrderProducts.Select(op => new OrderProductDetailDto
                 {
                    ProductId = op.ProductId,
                    ProductCount = op.ProductCount,
                    Price = op.Product != null ? op.Product.Price : 0,
                    TotalPrice = op.TotalPrice
                 }).ToList(),
                 TotalPrice = x.OrderProducts.Sum(op => op.TotalPrice) // <--- BURADA ƏLAVƏ ET!
            }).ToListAsync();

        // Review-lar (əgər əlavə etmək istəyirsənsə)
        var reviews = await _reviewRepository.GetByFiltered(x => x.UserId == userId, new[] { (Expression<Func<Review, object>>)(x => x.Product) })
            .Select(x => new ReviewGetDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                Content = x.Content,
                Rating = x.Rating
            }).ToListAsync();

        var ordersTotalPrice = orders.Sum(x => x.TotalPrice);

        var response = new UserFullProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles,
            Products = products,
            Orders = orders,
            OrdersTotalPrice = ordersTotalPrice,
            Favourites = favourites,
            Reviews = reviews
        };

        return new BaseResponse<UserFullProfileDto>("Success", response, HttpStatusCode.OK);
    }

}
