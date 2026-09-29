namespace Saitynai.DTO;

public class CreateUserDto
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string? Bio {get; set;}
    public required string Email {get; set; }
    public required DateTime BirthDate {get; set; }
}
public class UserDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string? Bio { get; set; }
    public required string Email { get; set; }
    public required DateTime BirthDate { get; set; }
}