using Saitynai.Models;

namespace Saitynai.DTO;

public class GameDto
{
    public required string Name {get; set; }
    public required string Description {get; set; }
    public required int PlayerCount {get; set; }
    public required GameLenght GameLenght { get; set; }
    public required Difficulty Difficulty {get; set; }

    public required long SessionId { get; set; }
}