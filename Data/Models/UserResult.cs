namespace Saitynai.Models;

public class UserResult
{
    public long Id { get; set; }
    public required float Score { get; set; }
    public int Placement {get; set; }
    public long GameId { get; set; }

    public Game Game {get; set; }

}