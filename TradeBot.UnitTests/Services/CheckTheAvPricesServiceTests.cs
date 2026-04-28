using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Models;
using TradeBot.Core.Services;
using TradeBot.Data.Contexts;

namespace TradeBot.UnitTests.Services
{
    [TestFixture]
    public class CheckTheAvPricesServiceTests
    {
        private Mock<ILogger<CheckThePricesService>> _mockLogger = null!;
        private Mock<IOptions<RequestDataOptions>> _mockHttpHeadersOptions = null!;
        private Mock<TradingDbContext> _dbContextMock = null!;
        private CheckThePricesService _sut = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<CheckThePricesService>>();
            
            // Create a real HttpClient for testing (since mocking HttpClient is complex)
            var httpClient = new HttpClient();
            
            _mockHttpHeadersOptions = new Mock<IOptions<RequestDataOptions>>();
            _dbContextMock = new Mock<TradingDbContext>();
            // Setup default HttpHeaders
            var httpHeaders = new RequestDataOptions
            {
                BaseBatchUrl = "http://localhost:7071",
                BaseUrl = "http://localhost:7071",
                HttpHeadersDictionary = new Dictionary<string, string>
                {
                    { "User-Agent", "TradeBot/1.0" },
                    { "Accept", "application/json" }
                }
            };
            _mockHttpHeadersOptions.Setup(x => x.Value).Returns(httpHeaders);

            _sut = new CheckThePricesService(
                _mockLogger.Object,
                httpClient,
                _mockHttpHeadersOptions.Object,
                _dbContextMock.Object
            );
        }

        [Test]
         public async Task CheckWeaponPricesAsync_ShouldReturnSuccessResult()
        {
            // Act
            var result = await _sut.CheckWeaponPricesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Messages.Should().NotBeEmpty();
            result.Messages.Should().Contain("Price check completed successfully");
        }

        [Test]
        public async Task CheckWeaponPricesAsync_ShouldLogInformationMessage()
        {
            // Act
            await _sut.CheckWeaponPricesAsync();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting to check prices")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task CheckWeaponPricesAsync_ShouldHaveCorrectItemsAndDealsCount()
        {
            // Act
            var result = await _sut.CheckWeaponPricesAsync();

            // Assert
            result.ItemsChecked.Should().Be(0);
            result.DealsFound.Should().Be(0);
        }

        [Test]
        public async Task CheckWeaponPricesAsync_ShouldHandleExceptionGracefully()
        {
            // Arrange
            Environment.SetEnvironmentVariable("PRICECHECK_API_URL", "invalid://url");

            // Act
            var result = await _sut.CheckWeaponPricesAsync();

            // Assert
            result.Should().NotBeNull();
            result.CheckedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task CheckWeaponPricesAsync_ResultShouldContainHttpStatusMessage()
        {
            // Act
            var result = await _sut.CheckWeaponPricesAsync();

            // Assert
            result.Messages.Should().Contain(msg => msg.Contains("OK"));
        }
    }
}
