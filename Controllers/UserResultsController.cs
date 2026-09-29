using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class UserResultController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserResultController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/UserResult
    [HttpGet(Name = "GetUserResults")]
    public async Task<ActionResult<IEnumerable<UserResultDto>>> GetUserResults(
        [FromQuery] PageParameters pageParameters,
        LinkGenerator linkGenerator)
    {
        var userResults = _context.UserResults
            .Select(u => new UserResultDto
            {
                Id = u.Id,
                Score = u.Score,
                Placement = u.Placement,
                GameId = u.GameId,
                UserId = u.UserId
            })
            .OrderBy(u => u.Score);

        var pagedUserResults = await PagedList<UserResultDto>.CreateAsync(
            userResults,
            pageParameters.PageNumber!.Value,
            pageParameters.PageSize!.Value);

        var paginationMetadata = pagedUserResults.CreatePaginationMetadata(
            linkGenerator,
            HttpContext,
            "GetUserResults");

        HttpContext.Response.Headers.Append(
            "Pagination",
            JsonSerializer.Serialize(paginationMetadata));

        return Ok(pagedUserResults);
    }

    // GET: api/UserResult/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserResultDto>> GetUserResult(long id)
    {
        var userResult = await _context.UserResults
            .Where(u => u.Id == id)
            .Select(u => new UserResultDto
            {
                Id = u.Id,
                Score = u.Score,
                Placement = u.Placement,
                GameId = u.GameId,
                UserId = u.UserId
            })
            .FirstOrDefaultAsync();

        if (userResult == null)
        {
            return NotFound();
        }

        return Ok(userResult);
    }

    // POST: api/UserResult
    [HttpPost]
    public async Task<ActionResult<UserResultDto>> AddUserResult(
        CreateUserResultDto dto)
    {
        // Check that the game exists
        var game = await _context.Games.FindAsync(dto.GameId);

        if (game == null)
        {
            return NotFound(new
            {
                message = "Game not found."
            });
        }

        // Check that the user exists
        var user = await _context.Users.FindAsync(dto.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        // Create ENTITY, not DTO
        var userResult = new UserResult
        {
            Score = dto.Score,
            Placement = dto.Placement,
            GameId = dto.GameId,
            UserId = dto.UserId
        };

        _context.UserResults.Add(userResult);
        await _context.SaveChangesAsync();

        // Return DTO
        var result = new UserResultDto
        {
            Id = userResult.Id,
            Score = userResult.Score,
            Placement = userResult.Placement,
            GameId = userResult.GameId,
            UserId = userResult.UserId
        };

        return CreatedAtAction(
            nameof(GetUserResult),
            new { id = userResult.Id },
            result
        );
    }

    // PUT: api/UserResult/5
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateUserResult(
        long id,
        CreateUserResultDto dto)
    {
        var existingUserResult = await _context.UserResults.FindAsync(id);

        if (existingUserResult == null)
        {
            return NotFound();
        }

        // Check that the game exists
        var game = await _context.Games.FindAsync(dto.GameId);

        if (game == null)
        {
            return NotFound(new
            {
                message = "Game not found."
            });
        }

        // Check that the user exists
        var user = await _context.Users.FindAsync(dto.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        existingUserResult.Score = dto.Score;
        existingUserResult.Placement = dto.Placement;
        existingUserResult.GameId = dto.GameId;
        existingUserResult.UserId = dto.UserId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/UserResult/5
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