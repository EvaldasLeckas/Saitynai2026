using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Saitynai.DTO;
using Saitynai.Services;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;
    private readonly IValidator<CreateUserDto> _validator;

    public UsersController(UserService service, IValidator<CreateUserDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    // GET: api/users
    [HttpGet(Name = "GetUsers")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] PageParameters pageParameters,
        LinkGenerator linkGenerator)
    {
        var pagedUsers = await _service.GetAllAsync(pageParameters);

        var paginationMetadata = pagedUsers.CreatePaginationMetadata(
            linkGenerator, HttpContext, "GetUsers");

        Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(pagedUsers);
    }

    // GET: api/users/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDto>> GetUser(long id)
    {
        var user = await _service.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser(CreateUserDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var created = await _service.CreateAsync(dto);

        if (created == null)
            return BadRequest(new { message = "Unable to create the user." });

        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }

    // PUT: api/users/5
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateUser(long id, CreateUserDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();
    }

    // DELETE: api/users/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}