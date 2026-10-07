using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;
using System.Text.Json;
using Saitynai.Services;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameService _service;
    private readonly IValidator<CreateGameDto> _validator;


    public GameController(GameService service, IValidator<CreateGameDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    // GET: api/Game
    [HttpGet(Name = "GetGames")]
    public async Task<ActionResult> GetGames(
        [FromQuery] PageParameters pageParameters,
        [FromQuery] GameFilterParameters filter,
        LinkGenerator linkGenerator)
    {
       var pagedGames = await _service.GetAllAsync(pageParameters, filter);

        var resources = pagedGames.Select(game =>
            new ResourceDto<GameDto>(
                game,
                CreateLinksForSingleGame(
                game.Id,
                linkGenerator,
                HttpContext
            ).ToArray())).ToList();



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

    // GET: api/Game/5
    [HttpGet("{id:long}", Name = "GetGame")]
    public async Task<ActionResult<GameDto>> GetGame(long id)
    {
        var games = await _service.GetByIdAsync(id);
        return games == null ? NotFound() : Ok(games);
    }

    // POST: api/Game
    [HttpPost(Name = "CreateGame")]
    public async Task<ActionResult<GameDto>> CreateGame(
        CreateGameDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        var created = await _service.CreateAsync(dto);

        if (created == null)
            return BadRequest(new {message = "Unable to create game"});


        return CreatedAtAction(
            nameof(GetGame),
            new { id = created.Id },
            created);
    }

    // PUT: api/Game/5
    [HttpPut("{id:long}", Name = "UpdateGame")]
    public async Task<IActionResult> UpdateGame(
        long id,
        CreateGameDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var error = await _service.UpdateAsync(id, dto);
        return error == null ? NoContent() : NotFound(new { message = error });


    }

    // DELETE: api/Game/5
    [HttpDelete("{id:long}", Name = "RemoveGame")]
    public async Task<IActionResult> DeleteGame(long id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();

    }

    static IEnumerable<LinkDto> CreateLinksForSingleGame(
        long gameId,
        LinkGenerator linkGenerator,
        HttpContext httpContext)
    {
        yield return new LinkDto(
            linkGenerator.GetUriByName(
                httpContext,
                "GetGame",
                new { id = gameId }),
            "self",
            "GET");

        yield return new LinkDto(
            linkGenerator.GetUriByName(
                httpContext,
                "UpdateGame",
                new { id = gameId }),
            "edit",
            "PUT");

        yield return new LinkDto(
            linkGenerator.GetUriByName(
                httpContext,
                "RemoveGame",
                new { id = gameId }),
            "remove",
            "DELETE");
    }

    static IEnumerable<LinkDto> CreateLinksForGames(
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        string? previousPageLink,
        string? nextPageLink)
    {
        yield return new LinkDto(
            linkGenerator.GetUriByName(
                httpContext,
                "GetGames"),
            "self",
            "GET");

        if (previousPageLink != null)
        {
            yield return new LinkDto(
                previousPageLink,
                "previousPage",
                "GET");
        }

        if (nextPageLink != null)
        {
            yield return new LinkDto(
                nextPageLink,
                "nextPage",
                "GET");
        }
    }
}