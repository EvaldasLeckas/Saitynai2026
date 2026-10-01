namespace Saitynai.DTO;

public class SessionDto
{
    public long Id { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required int PlayerCount { get; set; }
    public required string City { get; set; }
    public required string Adress { get; set; }
    public string? Description { get; set; }
    public required long UserId { get; set; }
}
public class CreateSessionDto
{
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required int PlayerCount { get; set; }
    public required string City { get; set; }
    public required string Adress { get; set; }
    public string? Description { get; set; }
    public required long UserId { get; set; }
}

public class SessionFilterParameters
{
    public string? City { get; set; }
    public int? MinPlayers { get; set; }
    public int? MaxPlayers { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
}