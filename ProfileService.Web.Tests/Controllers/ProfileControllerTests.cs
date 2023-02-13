using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Tests.Controllers;

public class ProfileControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileStore> _profileStoreMock = new();
    private readonly HttpClient _httpClient;

    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        // DRY: Don't repeat yourself
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); });
        }).CreateClient();
    }

    [Fact]
    public async Task GetProfile()
    {
        var profile = new Profile("foobar", "Foo", "Bar");
        _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);

        var response = await _httpClient.GetAsync($"/Profile/{profile.username}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(json));
    }

    [Fact]
    public async Task GetProfile_NotFound()
    {
        _profileStoreMock.Setup(m => m.GetProfile("foobar"))
            .ReturnsAsync((Profile?)null);

        var response = await _httpClient.GetAsync($"/Profile/foobar");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddProfile()
    {
        var profile = new Profile("foobar", "Foo", "Bar");
        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("http://localhost/Profile/foobar", response.Headers.GetValues("Location").First());
        
        _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Once);
    }

    [Fact]
    public async Task AddProfile_Conflict()
    {
        var profile = new Profile("foobar", "Foo", "Bar");
        _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);

        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        _profileStoreMock.Verify(m => m.UpsertProfile(profile), Times.Never);
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
    public async Task AddProfile_InvalidArgs(string username, string firstName, string lastName)
    {
        var profile = new Profile(username, firstName, lastName);
        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Never);
    }

    [Fact]
    public async Task UpdateProfile()
    {
        var profile = new Profile("foobar", "Foo", "Bar");
        _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);

        var updatedProfile = profile with { firstName = "Foo2" };
        
        var response = await _httpClient.PutAsync($"/Profile/{profile.username}",
            new StringContent(JsonConvert.SerializeObject(updatedProfile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.UpsertProfile(updatedProfile));
    }
    
    [Fact]
    public async Task UpdateProfile_NotFound()
    {
        var updatedProfile = new Profile("foobar","b","c");
        var json = JsonConvert.SerializeObject(updatedProfile);
        var content =
            new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/Profile/{updatedProfile.username}", content);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _profileStoreMock.Verify(mock=> mock.UpsertProfile(updatedProfile), Times.Never);
    
    }
    
    [Theory]
    [InlineData(null, "Bar")]
    [InlineData( " ", "Bar")]
    [InlineData("Foo", null)]
    [InlineData("Foo", " ")]
    public async Task UpdateProfile_InvalidArgs(string firstName, string lastName)
    {
        var profile = new Profile("foobar", "Foo", "Bar");
        _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);

        
        var updatedProfile = profile with { firstName = firstName, lastName = lastName };
        
        var json = JsonConvert.SerializeObject(updatedProfile);
        var content =
            new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/Profile/{profile.username}", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _profileStoreMock.Verify(mock=> mock.UpsertProfile(updatedProfile), Times.Never);
    }
}