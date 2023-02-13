using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IProfileStore
{
    Task UpsertProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task DeleteProfile(string username);
}
