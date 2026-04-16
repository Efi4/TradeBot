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

namespace TradeBot.UnitTests.Services
{
    [TestFixture]
    public class CheckTheAvPricesServiceTests
    {
        private Mock<ILogger<CheckTheAvPricesService>> _mockLogger = null!;
        private Mock<IOptions<HttpHeaders>> _mockHttpHeadersOptions = null!;
        private CheckTheAvPricesService sut = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<CheckTheAvPricesService>>();
            
            // Create a real HttpClient for testing (since mocking HttpClient is complex)
            var httpClient = new HttpClient();
            
            _mockHttpHeadersOptions = new Mock<IOptions<HttpHeaders>>();

            // Setup default HttpHeaders
            var httpHeaders = new HttpHeaders
            {
                HttpHeadersDictionary = new Dictionary<string, string>
                {
                    { "User-Agent", "TradeBot/1.0" },
                    { "Accept", "application/json" }
                }
            };
            _mockHttpHeadersOptions.Setup(x => x.Value).Returns(httpHeaders);

            sut = new CheckTheAvPricesService(
                _mockLogger.Object,
                httpClient,
                _mockHttpHeadersOptions.Object
            );
        }

        [Test]
        public async Task CheckPricesAsync_ShouldReturnSuccessResult()
        {
            // Act
            var result = await sut.CheckPricesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Messages.Should().NotBeEmpty();
            result.Messages.Should().Contain("Price check completed successfully");
        }

        [Test]
        public async Task CheckPricesAsync_ShouldLogInformationMessage()
        {
            // Act
            await sut.CheckPricesAsync();

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
        public async Task CheckPricesAsync_ShouldHaveCorrectItemsAndDealsCount()
        {
            // Act
            var result = await sut.CheckPricesAsync();

            // Assert
            result.ItemsChecked.Should().Be(0);
            result.DealsFound.Should().Be(0);
        }

        [Test]
        public async Task CheckPricesAsync_ShouldHandleExceptionGracefully()
        {
            // Arrange
            Environment.SetEnvironmentVariable("PRICECHECK_API_URL", "invalid://url");

            // Act
            var result = await sut.CheckPricesAsync();

            // Assert
            result.Should().NotBeNull();
            result.CheckedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task CheckPricesAsync_ResultShouldContainHttpStatusMessage()
        {
            // Act
            var result = await sut.CheckPricesAsync();

            // Assert
            result.Messages.Should().Contain(msg => msg.Contains("OK"));
        }
    }
}
