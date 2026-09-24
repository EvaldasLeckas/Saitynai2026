using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;

[ApiController]
[Route("api/[controller]")]
public class UserResultController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserResultController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/UserResults
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResult>>> GetUserResults()
    {
        var userResults = await _context.UserResults.ToListAsync();

        return Ok(userResults);
    }

    // GET: api/UserResults/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserResult>> GetUserResult(long id)
    {
        var userResult = await _context.UserResults.FindAsync(id);

        if (userResult == null)
        {
            return NotFound();
        }

        return Ok(userResult);
    }

    // POST: api/UserResults
    [HttpPost]
    public async Task<ActionResult<UserResult>> AddGame(UserResultDto dto)
    {
        var game = await _context.Games.FindAsync(dto.GameId);

        if (game == null)
        {
            return NotFound(new
            {
                message = "Game not found."
            });
        }

        var userResult = new UserResult
        {
            Score = dto.Score,
            Placement = dto.Placement,
            GameId = dto.GameId
        };

        _context.UserResults.Add(userResult);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUserResult),
            new { id = userResult.Id },
            userResult
        );
    }

    // PUT: api/UserResults/5
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateUserResult(long id, UserResultDto userResult)
    {


        var existingUserResult = await _context.UserResults.FindAsync(id);

        if (existingUserResult == null)
        {
            return NotFound();
        }

        var game = await _context.Games.FindAsync(userResult.UserId);

        if (game == null)
        {
            return NotFound(new
            {
                message = "Game not found."
            });
        }

        existingUserResult.Score = userResult.Score;
        existingUserResult.Placement = userResult.Placement;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/UserResults/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteUserResult(long id)
    {
        var userResult = await _context.UserResults.FindAsync(id);

        if (userResult == null)
        {
            return NotFound();
        }

        _context.UserResults.Remove(userResult);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}