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
    public async Task<ActionResult<IEnumerable<GameDto>>> GetGames(
     [FromQuery]PageParameters pageParameters,
     [FromQuery] GameFilterParameters filter,
     LinkGenerator linkGenerator)
    {

        var games = _context.Games.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
        games = games.Where(s =>
            s.Name == filter.Name);
        }
        if (filter.MinPlayers.HasValue)
    {
        games = games.Where(s =>
            s.PlayerCount >= filter.MinPlayers.Value);
    }

    if (filter.MaxPlayers.HasValue)
    {
        games = games.Where(s =>
            s.PlayerCount <= filter.MaxPlayers.Value);
    }

  

    var gamesDtos = games
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

        var pagedGames = await PagedList<GameDto>.CreateAsync(
            gamesDtos,
            pageParameters.PageNumber!.Value, 
            pageParameters.PageSize!.Value);

        var resources = pagedGames.Select(game =>
        {
            var links = CreateLinksForSingleGame(
                game.Id,
                linkGenerator,
                HttpContext
            ).ToArray();

            return new ResourceDto<GameDto>(
                game,
                links);
        }).ToList();

        var links = CreateLinksForGames(
            linkGenerator,
            HttpContext,
            pagedGames.GetPreviousPageLink(
                linkGenerator,
                HttpContext,
                "GetGames"),
            pagedGames.GetNextPageLink(
                linkGenerator,
                HttpContext,
                "GetGames")
        ).ToArray();
            

        var paginationMetadata = pagedGames.CreatePaginationMetadata(
        linkGenerator,
        HttpContext,
        "GetGames");

        HttpContext.Response.Headers.Append(
            "Pagination", 
            JsonSerializer.Serialize(paginationMetadata));

        return Ok(new
        {
            resources,
            links
        });
    }

    // GET: api/Games/5
    [HttpGet("{id:long}", Name = "GetGame")]
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
    [HttpPost(Name = "CreateGames")]
    public async Task<ActionResult<GameDto>> CreateGame(CreateGameDto dto)
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
    [HttpPut("{id:long}", Name = "UpdateGame")]
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
    [HttpDelete("{id:long}", Name = "RemoveGame")]
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
    static IEnumerable<LinkDto> CreateLinksForSingleGame(long gameId, LinkGenerator linkGenerator, HttpContext httpContext)
{
    yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetGame", new {id = gameId}), "self", "GET");
    yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "UpdateGame", new {id = gameId}), "edit", "PUT");
    yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "RemoveGame", new {id = gameId}), "remove", "DELETE");

}
static IEnumerable<LinkDto> CreateLinksForGames(LinkGenerator linkGenerator, HttpContext httpContext, string? previousPageLink, string? nextPageLink)
{
    yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetGames"), "self", "GET");
    
    if (previousPageLink != null)
        {
            yield return new LinkDto(previousPageLink, "previousPage", "GET");
        }
    if (nextPageLink != null)
        {
            yield return new LinkDto(nextPageLink, "nextPage", "GET");

        }

}
}