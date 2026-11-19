namespace Saitynai.DTO;

public class SessionParticipantDto
{
    public required long SessionId { get; set; }

    public required string UserId { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
}

public class CreateSessionParticipantDto
{
    public required string UserId { get; set; }
}