
public class AppRoles
{
    public const string Admin = nameof(Admin);
    public const string AppUser = nameof(AppUser);
    public const string LobbyOwner = nameof(LobbyOwner);

    public static readonly IReadOnlyCollection<string> All = [Admin, AppUser, LobbyOwner];
}