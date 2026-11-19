public class SessionLoginService(AppDbContext dbContext)
{
    public async Task CreateSessionAsync(Guid sessionId, string userId, string refreshToken, DateTime expiresAt)
    {
        dbContext.SessionLogin.Add(new SessionLogin
        {
            Id = sessionId,
            userId = userId,
            InitiatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            LastRefreshToken = refreshToken.ToSHA256()

        });

        await dbContext.SaveChangesAsync();
    }
    public async Task ExtendSessionAsync(Guid sessionId, string refreshToken, DateTime expiresAt)
    {
        var session = await dbContext.SessionLogin.FindAsync(sessionId);
        session.ExpiresAt = expiresAt;
        session.LastRefreshToken = refreshToken.ToSHA256();

        await dbContext.SaveChangesAsync();
    }
    public async Task InvalidateSessionAsync(Guid sessionId)
    {
        var session = await dbContext.SessionLogin.FindAsync(sessionId);
        if (session is null)
        {
            return;
        }
        session.IsRevoked = true;

        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsSessionValidAsync(Guid sessionId, string refreshToken)
    {
        var session = await dbContext.SessionLogin.FindAsync(sessionId);
        return session is not null && session.ExpiresAt > DateTimeOffset.UtcNow && !session.IsRevoked &&
        session.LastRefreshToken == refreshToken.ToSHA256();
    }
}