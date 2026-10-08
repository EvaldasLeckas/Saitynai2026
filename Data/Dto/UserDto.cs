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
public class UserProfileDto
{
    // iš User
    public long UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Bio { get; set; }
    public int Age { get; set; }
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public double WinRate { get; set; }
    public double? AveragePlacement { get; set; } 
    public float? BestScore { get; set; }

    public int SessionsJoined { get; set; }

    public List<ProfileSessionDto> UpcomingSessions { get; set; } = new();
    public List<ProfileResultDto> RecentResults { get; set; } = new();
}

public class ProfileSessionDto
{
    public long SessionId { get; set; }
    public string City { get; set; } = "";
    public DateTime StartDate { get; set; }
}

public class ProfileResultDto
{
    public long GameId { get; set; }
    public string GameName { get; set; } = "";
    public float Score { get; set; }
    public int Placement { get; set; }
}