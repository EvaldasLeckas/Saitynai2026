using Saitynai.Models;

namespace Saitynai.DTO;

public class UserResultDto
{
    public long Id { get; set; }
    public required float Score { get; set; }
    public int Placement {get; set; }
    public long GameId { get; set; }

    public required long UserId { get; set; }
}
public class CreateUserResultDto
{
    public required float Score { get; set; }
    public int Placement {get; set; }
    public required long UserId { get; set; }
    public required long GameId {get; set; }
}
public class UserResultFilterParameters
{
    public float? MinScore { get; set; }
    public float? MaxScore { get; set; }
    public int? Placement {get; set; }

}