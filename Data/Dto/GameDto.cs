using Saitynai.Models;

namespace Saitynai.DTO;

public class GameDto
{
    public required long Id {get; set;}
    public required string Name {get; set; }
    public required string Description {get; set; }
    public required int PlayerCount {get; set; }
    public required GameLength GameLength { get; set; }
    public required Difficulty Difficulty {get; set; }

    public required long SessionId { get; set; }
}
public class CreateGameDto
{
    public required string Name {get; set; }
    public required string Description {get; set; }
    public required int PlayerCount {get; set; }
    public required GameLength GameLength { get; set; }
    public required Difficulty Difficulty {get; set; }
    public long SessionId { get; set; }

}

public class GameFilterParameters
{
    public string? Name { get; set; }
    public int? MinPlayers { get; set; }
    public int? MaxPlayers { get; set; }

    public GameLength? GameLength { get; set; }
    public Difficulty? Difficulty { get; set; }
}