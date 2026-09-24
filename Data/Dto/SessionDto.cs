namespace Saitynai.DTO;

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