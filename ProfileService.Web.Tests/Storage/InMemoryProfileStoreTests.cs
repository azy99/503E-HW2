using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Tests.Storage;

public class InMemoryProfileStoreTests
{
    private readonly InMemoryProfileStore _store = new();
    
    [Fact]
    public async Task AddNewProfile()
    {
        var profile = new Profile(username: "foobar", firstName: "Foo", lastName: "Bar");
        await _store.UpsertProfile(profile);
        Assert.Equal(profile, await _store.GetProfile(profile.username));
    }
    
    [Fact]
    public async Task UpdateExistingProfile()
    {
        var profile = new Profile(username: "foobar", firstName: "Foo", lastName: "Bar");
        await _store.UpsertProfile(profile);

        var updatedProfile = profile with { firstName = "Foo1", lastName = "Foo2" };
        await _store.UpsertProfile(updatedProfile);
        
        Assert.Equal(updatedProfile, await _store.GetProfile(profile.username));
    }

    [Theory]
    [InlineData(null, "Foo", "Bar")]
    [InlineData("", "Foo", "Bar")]
    [InlineData(" ", "Foo", "Bar")]
    [InlineData("foobar", null, "Bar")]
    [InlineData("foobar", "", "Bar")]
    [InlineData("foobar", "   ", "Bar")]
    [InlineData("foobar", "Foo", "")]
    [InlineData("foobar", "Foo", null)]
    [InlineData("foobar", "Foo", " ")]
    public async Task UpsertProfile_InvalidArgs(string username, string firstName, string lastName)
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _store.UpsertProfile(new Profile(username, firstName, lastName));
        });
    }
}