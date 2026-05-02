# TradeBot.UnitTests

A .NET 8 test project containing unit tests for services, helpers, and business logic in the TradeBot application.

## Overview

`TradeBot.UnitTests` validates individual components in isolation using mocks and stubs. These tests:
- Test service methods independently
- Mock external dependencies (HTTP clients, databases)
- Verify business logic correctness
- Ensure error handling works properly
- Validate calculations and transformations

## Project Structure

```
TradeBot.UnitTests/
├── Services/
│   ├── CheckThePricesServiceTests.cs         # Price checking service tests
│   ├── CalculateAveragePriceServiceTests.cs  # Average price calculation tests
│   └── DiscordIntegrationServiceTests.cs     # Discord notification tests
└── TradeBot.UnitTests.csproj
```

## Test Suites

### CheckThePricesService Tests

Located in: [Services/CheckThePricesServiceTests.cs](Services/CheckThePricesServiceTests.cs)

Tests the `CheckThePricesService` which monitors marketplace prices and detects deals.

**Test Coverage:**
- Service initialization with mocked dependencies
- Successful price checking operations
- Deal detection logic
- Error handling and graceful degradation
- Item and deal counting
- Logging verification

**Example Test Structure:**
```csharp
[TestFixture]
public class CheckThePricesServiceTests
{
    private Mock<ILogger<CheckThePricesService>> _mockLogger = null!;
    private Mock<IOptions<RequestDataOptions>> _mockRequestOptions = null!;
    private Mock<TradingDbContext> _mockDbContext = null!;
    private Mock<IAzureStorageHelper> _mockStorageHelper = null!;
    private HttpClient _httpClient = null!;
    private CheckThePricesService _sut = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        // Initialize mocks
        _mockLogger = new Mock<ILogger<CheckThePricesService>>();
        _mockRequestOptions = new Mock<IOptions<RequestDataOptions>>();
        _mockDbContext = new Mock<TradingDbContext>();
        _mockStorageHelper = new Mock<IAzureStorageHelper>();
        
        _httpClient = new HttpClient();

        // Configure mock options
        var requestData = new RequestDataOptions
        {
            BaseUrl = "https://api.example.com",
            BaseBatchUrl = "https://api.example.com/batch",
            HttpHeadersDictionary = new Dictionary<string, string>
            {
                { "User-Agent", "TradeBot/1.0" }
            }
        };
        _mockRequestOptions.Setup(x => x.Value).Returns(requestData);

        // Create service under test
        _sut = new CheckThePricesService(
            _mockLogger.Object,
            _httpClient,
            _mockRequestOptions.Object,
            _mockDbContext.Object,
            _mockStorageHelper.Object
        );
    }

    [Test]
    public async Task CheckPricesAsync_ReturnsSuccessResult()
    {
        // Act
        var result = await _sut.CheckPricesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task CheckPricesAsync_LogsInformationMessage()
    {
        // Act
        await _sut.CheckPricesAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task CheckPricesAsync_CountsItemsCorrectly()
    {
        // Act
        var result = await _sut.CheckPricesAsync();

        // Assert
        result.ItemsChecked.Should().BeGreaterThanOrEqualTo(0);
        result.DealsFound.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task CheckPricesAsync_HandlesExceptionGracefully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("INVALID_API_URL", "not_a_url");

        // Act
        var result = await _sut.CheckPricesAsync();

        // Assert
        result.Should().NotBeNull();
        result.CheckedAt.Should().NotBe(default);
    }
}
```

### CalculateAveragePriceService Tests

Tests the `CalculateAveragePriceService` which computes average prices for weapons and armor.

**Test Coverage:**
- Average weapon price calculations
- Average armor price calculations
- Stat range filtering
- Database persistence
- Error handling

**Example:**
```csharp
[TestFixture]
public class CalculateAveragePriceServiceTests
{
    private Mock<ILogger<CalculateAveragePriceService>> _mockLogger = null!;
    private Mock<TradingDbContext> _mockDbContext = null!;
    private Mock<IOptions<StatRangeOptions>> _mockStatRangeOptions = null!;
    private CalculateAveragePriceService _sut = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<CalculateAveragePriceService>>();
        _mockDbContext = new Mock<TradingDbContext>();
        _mockStatRangeOptions = new Mock<IOptions<StatRangeOptions>>();

        var statRanges = new StatRangeOptions
        {
            MinWeaponDmg = 50,
            MaxWeaponDmg = 250,
            MinWeaponCrit = 5,
            MaxWeaponCrit = 50
        };
        _mockStatRangeOptions.Setup(x => x.Value).Returns(statRanges);

        _sut = new CalculateAveragePriceService(
            _mockLogger.Object,
            _mockDbContext.Object,
            _mockStatRangeOptions.Object
        );
    }

    [Test]
    public async Task CalculateAverageWeaponPricesAsync_ReturnsTrue_OnSuccess()
    {
        // Act
        var result = await _sut.CalculateAverageWeaponPricesAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task CalculateAverageArmorPricesAsync_ReturnsTrue_OnSuccess()
    {
        // Act
        var result = await _sut.CalculateAverageArmorPricesAsync();

        // Assert
        result.Should().BeTrue();
    }
}
```

### DiscordIntegrationService Tests

Tests the `DiscordIntegrationService` which sends notifications to Discord.

**Test Coverage:**
- Message formatting
- Webhook posting
- Error handling
- Logging

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter ClassName=TradeBot.UnitTests.Services.CheckThePricesServiceTests
```

### Run Specific Test Method
```bash
dotnet test --filter Name=CheckPricesAsync_ReturnsSuccessResult
```

### Run with Verbose Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

## Mocking Patterns

### Mocking Services

```csharp
// Mock an interface
var mockService = new Mock<ICheckThePricesService>();

// Setup a method to return a value
mockService
    .Setup(x => x.CheckPricesAsync())
    .ReturnsAsync(new CheckPricesResult { Success = true });

// Verify a method was called
mockService.Verify(x => x.CheckPricesAsync(), Times.Once);
```

### Mocking HttpClient

```csharp
// Create real HttpClient for testing
var httpClient = new HttpClient();

// Or use HttpClientFactory mock
var mockFactory = new Mock<IHttpClientFactory>();
mockFactory
    .Setup(x => x.CreateClient(It.IsAny<string>()))
    .Returns(httpClient);
```

### Mocking DbContext

```csharp
// Create mock DbContext
var mockContext = new Mock<TradingDbContext>();

// Setup DbSet mock
var mockWeapons = new Mock<DbSet<Weapon>>();
mockContext
    .Setup(x => x.Weapons)
    .Returns(mockWeapons.Object);
```

### Mocking IOptions<T>

```csharp
var mockOptions = new Mock<IOptions<RequestDataOptions>>();
mockOptions
    .Setup(x => x.Value)
    .Returns(new RequestDataOptions 
    { 
        BaseUrl = "https://example.com"
    });
```

## Test Assertions

Using FluentAssertions for readable assertions:

```csharp
// Null/Empty checks
result.Should().NotBeNull();
message.Should().BeEmpty();
items.Should().NotBeEmpty();

// Equality checks
result.Success.Should().BeTrue();
count.Should().Be(5);
price.Should().BeGreaterThan(100);

// Collection checks
items.Should().HaveCount(3);
items.Should().Contain(x => x.Name == "Rifle");
items.Should().AllSatisfy(x => x.Price > 0);

// Date/time checks
timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

// Exception checks
await Assert.ThrowsAsync<ArgumentNullException>(
    () => service.GetWeapon(null!)
);
```

## Test Lifecycle

### OneTimeSetUp vs. SetUp

```csharp
[TestFixture]
public class ServiceTests
{
    // Runs ONCE before all tests in the fixture
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        // Initialize expensive resources
    }

    // Runs BEFORE EACH test
    [SetUp]
    public void Setup()
    {
        // Reset state for each test
    }

    [Test]
    public void MyTest() { }

    // Runs AFTER EACH test
    [TearDown]
    public void Teardown()
    {
        // Clean up after each test
    }

    // Runs ONCE after all tests
    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        // Dispose expensive resources
    }
}
```

## Best Practices

### Test Naming Convention

```csharp
// Good: MethodName_Scenario_ExpectedResult
[Test]
public async Task CheckPricesAsync_WithValidConfiguration_ReturnsSuccessResult()

// Acceptable: What it tests
[Test]
public async Task SuccessfulPriceCheck()

// Avoid: Vague names
[Test]
public async Task TestPriceCheck()
```

### Arrange-Act-Assert Pattern

```csharp
[Test]
public async Task MyTest()
{
    // ARRANGE - Set up test data and mocks
    var mockService = new Mock<IPriceService>();
    mockService.Setup(x => x.GetPrice()).ReturnsAsync(100);

    // ACT - Execute the code under test
    var result = await mockService.Object.GetPrice();

    // ASSERT - Verify the results
    result.Should().Be(100);
}
```

### One Assertion Per Test (Where Possible)

```csharp
// Good: Each test has a single focus
[Test]
public void CalculatePrice_ReturnsCorrectValue()
{
    var result = calculator.Calculate(10, 20);
    result.Should().Be(30);
}

[Test]
public void CalculatePrice_LogsCalculation()
{
    calculator.Calculate(10, 20);
    mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
}

// Avoid: Multiple unrelated assertions
[Test]
public void CalculatePrice()
{
    var result = calculator.Calculate(10, 20);
    result.Should().Be(30);
    mockLogger.Verify(...);
    mockDatabase.Verify(...);
}
```

### Don't Test Implementation Details

```csharp
// Good: Test behavior/output
[Test]
public void GetWeapons_ReturnsNonEmptyList()
{
    var weapons = service.GetWeapons();
    weapons.Should().NotBeEmpty();
}

// Avoid: Testing internal implementation
[Test]
public void GetWeapons_CallsInternalHelper()
{
    service.GetWeapons();
    mockHelper.Verify(x => x.InternalMethod(), Times.Once);
}
```

## Dependencies

- **.NET 8**: Target framework
- **Microsoft.NET.Test.Sdk**: Test framework support
- **NUnit** (3.x): Testing framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking framework
- **Microsoft.EntityFrameworkCore**: For DbContext mocking
- **TradeBot.Core**: Services being tested
- **TradeBot.Data**: Data models
- **TradeBot.Base**: Shared models

## Continuous Integration

In CI/CD pipelines:

```bash
# Run tests with detailed output
dotnet test --logger "console;verbosity=normal"

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Fail on test failure
dotnet test --no-build
```

## Debugging Tests

### In Visual Studio
1. Set a breakpoint in the test
2. Right-click the test and select "Debug Test"
3. Use Debug toolbar to step through

### In VS Code
1. Install C# Extension
2. Set breakpoint
3. Run "Run and Debug" or press `F5`

### Command Line
```bash
dotnet test --no-build --logger:"console;verbosity=detailed"
```

## Coverage Goals

Aim for:
- **>80%** code coverage for business logic services
- **>70%** overall project coverage
- **100%** coverage for critical paths (deal detection, notifications)

## Related Projects

- [TradeBot.Core](../TradeBot.Core/README.md) - Services being tested
- [TradeBot.Data](../TradeBot.Data/README.md) - Data models
- [TradeBot.IntegrationTests](../TradeBot.IntegrationTests/README.md) - Integration tests
- [AzureFunctionApp](../AzureFunctionApp/README.md) - Functions using these services

## See Also

- [Root README](../Readme.md) - Project overview
- Testing Best Practices: https://xunit.net/docs/getting-started/testing-console-app
