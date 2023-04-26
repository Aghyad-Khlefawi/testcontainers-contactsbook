using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MongoDb;

namespace ContactsBook.FunctionTests
{
    public class ContactsScenarios : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoDbContainer;

        private ContactsBookWebFactory? _webFactory;

        public ContactsScenarios()
        {
            _mongoDbContainer = new MongoDbBuilder().Build();
        }

        [Fact]
        public async Task CreateValidContactShouldReturnOkResponse()
        {
            using var client = _webFactory!.CreateClient();

            var testContact = new Contact
            {
                FirstName = "Test",
                LastName = "Test",
                EmailAddress = "Test@mail.copm",
            };

            var response = await client.PostAsJsonAsync("contacts", testContact);
            
            Assert.True(response.IsSuccessStatusCode); 
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