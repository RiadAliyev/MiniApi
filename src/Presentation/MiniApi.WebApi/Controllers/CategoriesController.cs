using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.CategoryDtos;
using MiniApi.Application.Shared;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MiniApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var response = await _categoryService.SearchByNameAsync(search);
        return StatusCode((int)response.StatusCode, response);
    }

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("Get-All")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAll()
    {
        var response = await _categoryService.GetAllAsync();
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("GetById")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _categoryService.GetByIdAsync(id);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize  (Policy =Permissions.Category.Create)]
    [HttpPost("Create")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var response = await _categoryService.CreateAsync(dto);
        return StatusCode((int)response.StatusCode, response);
    }

    // DELETE api/categories/{id}
    [Authorize(Policy = Permissions.Category.Create)]
    [HttpDelete("Delete")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _categoryService.DeleteAsync(id);
        return StatusCode((int)response.StatusCode, response);
    }

    // PUT api/categories/{id}
    [Authorize(Policy = Permissions.Category.Create)]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateDto dto)
    {
        var response = await _categoryService.UpdateAsync(id, dto);
        return StatusCode((int)response.StatusCode, response);
    }
}


