using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;
using System.Text.Json;


[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly AppDbContext _context;

    public GameController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/games
    [HttpGet(Name = "GetGames")]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetSessions([FromQuery]PageParameters pageParameters, LinkGenerator linkGenerator)
    {
        var games = _context.Games
    .Select(u => new GameDto
    {
        Id = u.Id,
        Name = u.Name,
        PlayerCount = u.PlayerCount,
        Difficulty = u.Difficulty,
        GameLenght = u.GameLenght,
        Description = u.Description,
        SessionId = u.SessionId
    })
    .OrderBy(u => u.Name);

        var pagedSessions = await PagedList<GameDto>.CreateAsync(games, pageParameters.PageNumber!.Value, pageParameters.PageSize!.Value);

        var paginationMetadata = pagedSessions.CreatePaginationMetadata(linkGenerator, HttpContext, "GetSessions");

        HttpContext.Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(pagedSessions);
    }

    // GET: api/Games/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<GameDto>> GetGame(long id)
    {
        var game = await _context.Games.FindAsync(id);

        if (game == null)
        {
            return NotFound();
        }

        return Ok(game);
    }

    // POST: api/Games
    [HttpPost]
    public async Task<ActionResult<GameDto>> AddGame(CreateGameDto dto)
    {
        var session = await _context.Sessions.FindAsync(dto.SessionId);

        if (session == null)
        {
            return NotFound(new
            {
                message = "Session not found."
            });
        }

        var game = new Game
        {
            Name = dto.Name,
            Description = dto.Description,
            PlayerCount = dto.PlayerCount,
            GameLenght = dto.GameLenght,
            Difficulty = dto.Difficulty,
            SessionId = dto.SessionId
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetGame),
            new { id = game.Id },
            game
        );
    }

    // PUT: api/Games/5
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateGame(long id, CreateGameDto dto)
    {

        var existingGame = await _context.Games.FindAsync(id);

        if (existingGame == null)
        {
            return NotFound();
        }
        var session = await _context.Sessions.FindAsync(dto.SessionId);
        if (session == null)
        {
            return NotFound(new
            {
                message = "Session not found."
            });
        }

        existingGame.Name = dto.Name;
        existingGame.Description = dto.Description;
        existingGame.PlayerCount = dto.PlayerCount;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Games/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteGame(long id)
    {
        var Game = await _context.Games.FindAsync(id);

        if (Game == null)
        {
            return NotFound();
        }

        _context.Games.Remove(Game);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}