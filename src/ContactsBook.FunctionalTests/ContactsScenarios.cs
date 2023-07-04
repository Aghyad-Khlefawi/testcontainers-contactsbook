using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Driver.Core.Connections;
using Testcontainers.MongoDb;

namespace ContactsBook.FunctionalTests
{
    public class ContactsScenarios : IAsyncLifetime
    {
        private ContactsBookWebFactory? _webFactory;

        private readonly MongoDbContainer _mongoDbContainer;
        public ContactsScenarios()
        {
            _mongoDbContainer = new MongoDbBuilder().Build();
        }

        [Fact]
        public async Task CreateValidContactShouldReturnOkResponse()
        {
            using var client = _webFactory!.CreateClient(); // Create the API client

            var testContact = new Contact // Prepare the test data
            {
                FirstName = "Test",
                LastName = "Test",
                EmailAddress = "Test@mail.copm",
            };

            var response = await client.PostAsJsonAsync("contacts", testContact); // Perform the API call

            Assert.True(response.IsSuccessStatusCode); // Validate the response of the API call
        }

        public Task DisposeAsync()
        {
            return _mongoDbContainer.DisposeAsync().AsTask();
        }

        public async Task InitializeAsync()
        {
            await _mongoDbContainer.StartAsync();
            _webFactory = new ContactsBookWebFactory(_mongoDbContainer.GetConnectionString());
        }
    }
}

public class ContactsBookWebFactory : WebApplicationFactory<Program>
{
    private readonly string _testContainerConnectionString;

    public ContactsBookWebFactory(string testContainerConnectionString)
    {
        _testContainerConnectionString = testContainerConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:MongoDb",_testContainerConnectionString }
            });
        });
    }
}