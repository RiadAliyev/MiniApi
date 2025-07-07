using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.CategoryDtos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MiniApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("Get-All")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _categoryService.GetAllAsync();
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _categoryService.GetByIdAsync(id);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize]
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var response = await _categoryService.CreateAsync(dto);
        return StatusCode((int)response.StatusCode, response);
    }
}
