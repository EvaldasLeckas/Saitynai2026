using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }


    [HttpGet(Name = "GetUsers")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery]PageParameters pageParameters, LinkGenerator linkGenerator)
    {
        var users = _context.Users
    .Select(u => new UserDto
    {
        Id = u.Id,
        Name = u.Name,
        Surname = u.Surname,
        Bio = u.Bio,
        Email = u.Email,
        BirthDate = u.BirthDate
    })
    .OrderBy(u => u.Name);

        var pagedUsers = await PagedList<UserDto>.CreateAsync(users, pageParameters.PageNumber!.Value, pageParameters.PageSize!.Value);

        var paginationMetadata = pagedUsers.CreatePaginationMetadata(linkGenerator, HttpContext, "GetUsers");

        HttpContext.Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(pagedUsers);
    }


    // GET: api/users/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDto>> GetUser(long id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var userDto = new UserDto
    {
        Id = user.Id,
        Name = user.Name,
        Surname = user.Surname,
        Bio = user.Bio,
        Email = user.Email,
        BirthDate = user.BirthDate
    };

        return Ok(userDto);
    }

    // POST: api/users
[HttpPost]
public async Task<ActionResult<UserDto>> AddUser(CreateUserDto dto)
{
    var user = new User
    {
        Name = dto.Name,
        Surname = dto.Surname,
        Bio = dto.Bio,
        Email = dto.Email,
        BirthDate = dto.BirthDate
    };

    try
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Bio = user.Bio,
            Email = user.Email,
            BirthDate = user.BirthDate
        };

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            userDto
        );
    }
    catch (DbUpdateException)
    {
        return BadRequest(new
        {
            message = "Unable to create the user."
        });
    }
    catch
    {
        return StatusCode(503, new
        {
            message = "Unable to access the database."
        });
    }
}

    // PUT: api/users/5
[HttpPut("{id:long}")]
public async Task<IActionResult> UpdateUser(long id, CreateUserDto dto)
{
    var existingUser = await _context.Users.FindAsync(id);

    if (existingUser == null)
    {
        return NotFound();
    }

    existingUser.Name = dto.Name;
    existingUser.Surname = dto.Surname;
    existingUser.Bio = dto.Bio;
    existingUser.Email = dto.Email;
    existingUser.BirthDate = dto.BirthDate;

    await _context.SaveChangesAsync();

    return NoContent();
}

    // DELETE: api/users/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}