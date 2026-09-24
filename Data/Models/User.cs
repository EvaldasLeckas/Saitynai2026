namespace Saitynai.Models;

public class User
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string? Bio {get; set;}
    public required string Email {get; set; }
    public string? PasswordHash {get; set;}
    public required DateTime BirthDate {get; set; }

}