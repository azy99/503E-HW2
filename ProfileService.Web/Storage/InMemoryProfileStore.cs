using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public class InMemoryProfileStore : IProfileStore
{
    private readonly Dictionary<string, Profile> _profiles = new();
        
    public Task UpsertProfile(Profile profile)
    {
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.username) ||
            string.IsNullOrWhiteSpace(profile.firstName) ||
            string.IsNullOrWhiteSpace(profile.lastName)
           )
        {
            throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
        }
        
        _profiles[profile.username] = profile;
        return Task.CompletedTask;
    }

    public Task<Profile?> GetProfile(string username)
    {
        if (!_profiles.ContainsKey(username)) return Task.FromResult<Profile?>(null);
        return Task.FromResult<Profile?>(_profiles[username]);
    }

    public Task DeleteProfile(string username)
    {
        _profiles.Remove(username);
        return Task.CompletedTask;
    }
}