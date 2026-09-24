using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();

        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<User>> GetUser(long id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> AddUser(User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.Id },
                user
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
    public async Task<IActionResult> UpdateUser(long id, User user)
    {
        var existingUser = await _context.Users.FindAsync(id);

        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.Name = user.Name;
        existingUser.Surname = user.Surname;
        existingUser.Bio = user.Bio;
        existingUser.Email = user.Email;

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