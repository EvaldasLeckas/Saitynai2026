using Microsoft.Identity.Client;
using Saitynai.Auth;

public class SessionLogin
{
    public Guid Id {get; set; }
    public string LastRefreshToken {get; set;}
    public DateTimeOffset InitiatedAt {get; set;} 
    public DateTimeOffset ExpiresAt {get; set;}
    public bool IsRevoked {get; set;} 
    public required string userId {get; set;} 
    public AppUser User { get; set; } 
}