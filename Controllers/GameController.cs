using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;

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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Game>>> GetGames()
    {
        var games = await _context.Games.ToListAsync();

        return Ok(games);
    }

    // GET: api/Games/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Game>> GetGame(long id)
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
    public async Task<ActionResult<Game>> AddGame(GameDto dto)
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
    public async Task<IActionResult> UpdateGame(long id, GameDto dto)
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