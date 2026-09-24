namespace Saitynai.Models;

public class Session
{
    public long Id { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required int PlayerCount {get; set; }
    public required string City {get; set; }
    public required string Adress {get; set; }
    public string? Description {get; set;}
    
    public long UserId { get; set; }

    public User User { get; set;}
}