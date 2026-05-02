# TradeBot.IntegrationTests

A .NET 8 test project containing integration tests for Azure Functions and external API interactions.

## Overview

`TradeBot.IntegrationTests` validates the interaction between Azure Functions, external services, and the database in an integrated environment. These tests:
- Test HTTP-triggered functions with realistic requests
- Verify end-to-end workflows
- Validate Azure Function configuration
- Test external API integrations
- Ensure database persistence works correctly

## Project Structure

```
TradeBot.IntegrationTests/
├── HttpTriggerFunctionTests.cs    # HTTP trigger function tests
└── TradeBot.IntegrationTests.csproj
```

## Test Categories

### HTTP Trigger Function Tests

Located in: [HttpTriggerFunctionTests.cs](HttpTriggerFunctionTests.cs)

Tests the `HttpTriggerFunction` endpoint that responds to HTTP GET/POST requests.

**Test Scenarios:**
- Valid HTTP requests return proper response format
- Response includes required fields (message, timestamp, method, path)
- Different HTTP methods (GET, POST) are handled correctly
- Response timestamps are accurate
- Request path and method are captured correctly

**Example Test Structure:**
```csharp
[TestFixture]
public class HttpTriggerFunctionTests
{
    private readonly HttpClient _httpClient = new();
    private const string FunctionUrl = "http://localhost:7071/api/hello";

    [Test]
    public async Task GetRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, FunctionUrl);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsAsync<dynamic>();
        content.message.Should().NotBeNullOrEmpty();
        content.method.Should().Be("GET");
    }

    [Test]
    public async Task PostRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, FunctionUrl)
        {
            Content = new StringContent("test", Encoding.UTF8, "application/json")
        };

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsAsync<dynamic>();
        content.method.Should().Be("POST");
    }

    [Test]
    public async Task Response_IncludesTimestamp()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync(FunctionUrl);
        var content = await response.Content.ReadAsAsync<dynamic>();

        // Assert
        content.timestamp.Should().NotBeNull();
        DateTime.TryParse(content.timestamp.ToString(), out var parsedDate)
            .Should().BeTrue();
    }
}
```

## Prerequisites

### Local Setup Requirements
- .NET 8 SDK
- Azure Functions Core Tools v4+
- SQL Server or LocalDB (for database tests)
- Visual Studio Code or Visual Studio 2022

### Running the Tests

1. **Start Azure Functions locally:**
```bash
cd AzureFunctionApp
func start
```

The functions should be running on `http://localhost:7071` before tests execute.

2. **Run all integration tests:**
```bash
dotnet test
```

3. **Run specific test class:**
```bash
dotnet test --filter ClassName=TradeBot.IntegrationTests.HttpTriggerFunctionTests
```

4. **Run with verbose output:**
```bash
dotnet test --logger:"console;verbosity=detailed"
```

## Test Configuration

### Function App Requirements

Before running integration tests, ensure:
1. Azure Functions are running locally: `func start`
2. All required services are configured in `local.settings.json`
3. Connection strings and API keys are properly set

### Test Isolation

Each test should:
- Be independent of other tests
- Clean up any created resources
- Not rely on test execution order
- Handle timeouts gracefully

## Test Patterns

### Arranging a Test Request

```csharp
[Test]
public async Task MyIntegrationTest()
{
    // Arrange - Set up test data and HTTP request
    var testData = new { message = "test" };
    var request = new HttpRequestMessage(HttpMethod.Post, FunctionUrl)
    {
        Content = JsonContent.Create(testData)
    };

    // Act - Execute the function
    var response = await _httpClient.SendAsync(request);

    // Assert - Verify results
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadAsAsync<dynamic>();
    result.message.Should().Equal("test");
}
```

### Validating Response Structure

```csharp
[Test]
public async Task ResponseHasRequiredProperties()
{
    // Act
    var response = await _httpClient.GetAsync(FunctionUrl);
    var content = await response.Content.ReadAsAsync<JObject>();

    // Assert
    content.Should().NotBeNull();
    content.Properties().Select(p => p.Name)
        .Should().Contain(new[] { "message", "timestamp", "method", "path" });
}
```

### Testing with Database

```csharp
[TestFixture]
public class DatabaseIntegrationTests
{
    private TradingDbContext _dbContext = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        // Initialize database context
        var options = new DbContextOptionsBuilder<TradingDbContext>()
            .UseSqlServer(GetConnectionString())
            .Options;
        
        _dbContext = new TradingDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [Test]
    public async Task FunctionStoresDataInDatabase()
    {
        // Arrange
        var initialCount = await _dbContext.Weapons.CountAsync();

        // Act
        await _httpClient.PostAsync(FunctionUrl, new StringContent("{}"));

        // Assert
        var finalCount = await _dbContext.Weapons.CountAsync();
        finalCount.Should().BeGreaterThan(initialCount);
    }

    [OneTimeTearDown]
    public async Task Cleanup()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _dbContext.Dispose();
    }
}
```

## Common Issues

### Functions Not Running
- Ensure `func start` is running before tests
- Check that `local.settings.json` is properly configured
- Verify connection strings are correct

### Timeout Errors
- Increase `HttpClient.Timeout` for slow operations
- Ensure Azure Emulator or real services are accessible
- Check network connectivity

### Database Access Issues
- Verify SQL Server/LocalDB is running
- Check connection string in `local.settings.json`
- Ensure migrations have been applied: `dotnet ef database update`

## Dependencies

- **.NET 8**: Target framework
- **Microsoft.NET.Test.Sdk**: Test framework support
- **NUnit**: Testing framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking framework (if needed)
- **Azure.Storage.Queues**: For testing queue operations
- **Microsoft.EntityFrameworkCore.SqlServer**: Database testing

## Best Practices

### Test Naming
```csharp
// Good: Describes what is being tested and the expected outcome
[Test]
public async Task GetWeapon_WithValidId_ReturnsWeaponData()

// Avoid: Vague naming
[Test]
public async Task TestWeaponFunction()
```

### Assertions
```csharp
// Use FluentAssertions for readable assertions
response.StatusCode.Should().Be(HttpStatusCode.OK);
weapons.Should().NotBeEmpty();
deals.Should().HaveCount(5);

// Avoid: Hard to read assertions
Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
Assert.IsNotNull(weapons);
```

### Async/Await
```csharp
// Always use async for HTTP calls
public async Task MyTest()
{
    var response = await _httpClient.GetAsync(url);
}

// Avoid: Blocking calls
var response = _httpClient.GetAsync(url).Result;
```

## Continuous Integration

For CI/CD pipelines, run tests with:

```bash
# Unit tests only (faster)
dotnet test TradeBot.UnitTests.csproj

# Integration tests only (requires services running)
dotnet test TradeBot.IntegrationTests.csproj

# All tests
dotnet test

# With code coverage
dotnet test /p:CollectCoverage=true
```

## Debugging Tests

Run tests in debug mode:
```bash
# In Visual Studio: Set breakpoints and press F5
# In VS Code: Use .NET debugger extension

# Or with command line
dotnet test --logger "console;verbosity=normal" --no-build
```

## Related Projects

- [AzureFunctionApp](../AzureFunctionApp/README.md) - Functions being tested
- [TradeBot.UnitTests](../TradeBot.UnitTests/README.md) - Unit tests for individual services
- [TradeBot.Core](../TradeBot.Core/README.md) - Core services
- [TradeBot.Data](../TradeBot.Data/README.md) - Data layer

## See Also

- [Root README](../Readme.md) - Project overview
- [AzureFunctionApp/README.md](../AzureFunctionApp/README.md) - Function documentation
