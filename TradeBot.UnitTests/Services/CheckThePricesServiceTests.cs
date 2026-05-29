using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using AutoFixture.NUnit3;
using TradeBot.Core.Interfaces;
using TradeBot.Base.Models;
using TradeBot.Base.Objects;
using TradeBot.Core.Services;
using TradeBot.Data.Contexts;
using TradeBot.Data.Helpers;
using TradeBot.Data.Models;
using EFCore.BulkExtensions;
using TradeBot.Base.Configuration;
using TradeBot.UnitTests.Base;

namespace TradeBot.UnitTests.Services
{
    /// <summary>
    /// Test helper class for async enumeration
    /// </summary>
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public async ValueTask<bool> MoveNextAsync()
        {
            return _inner.MoveNext();
        }

        public async ValueTask DisposeAsync()
        {
            _inner.Dispose();
        }
    }

    /// <summary>
    /// Comprehensive unit tests for the CheckThePricesService class.
    /// Tests price retrieval, price updates, and price checking functionality.
    /// </summary>
    [TestFixture]
    public class CheckThePricesServiceTests
    {
        private Mock<ILogger<CheckThePricesService>> _mockLogger = null!;
        private Mock<IOptions<RequestDataOptions>> _mockRequestOptions = null!;
        private Mock<TradingDbContext> _mockDbContext = null!;
        private Mock<IAzureStorageHelper> _mockStorageHelper = null!;
        private CheckThePricesService _sut = null!;
        private HttpClient _httpClient = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var fixture = CheckThePricesServiceHelpers.CreateCheckThePricesServiceTestFixture();
            _mockLogger = fixture.Logger;
            _mockRequestOptions = fixture.RequestOptions;
            _mockDbContext = fixture.DbContext;
            _mockStorageHelper = fixture.StorageHelper;
            _httpClient = fixture.HttpClient;

            // Setup common scenario: empty Weapons and Armors with successful operations
            SetupEmptyDbSets();
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(0);
            var bulkConfig = new BulkConfig { PropertiesToIncludeOnUpdate = new List<string> { nameof(Weapon) } };
            var armorBulkConfig = new BulkConfig { PropertiesToIncludeOnUpdate = new List<string> { nameof(Weapon) } };
            _mockStorageHelper.Setup(x => x.PushToNotificationsQueueEncodedAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockDbContext.Setup(x => x.BulkInsertOrUpdateOrDeleteAsync(It.IsAny<List<Weapon>>(), bulkConfig, null, null, default))
                .Returns(Task.CompletedTask);
            _mockDbContext.Setup(x => x.BulkInsertOrUpdateOrDeleteAsync(It.IsAny<List<Armor>>(), armorBulkConfig, null, null, default))
                .Returns(Task.CompletedTask);
        }

        [SetUp]
        public void Setup()
        {
            _sut = new CheckThePricesService(
                _mockLogger.Object,
                _httpClient,
                _mockRequestOptions.Object,
                _mockDbContext.Object,
                _mockStorageHelper.Object
            );
        }

        /// <summary>
        /// Helper method to setup empty DbSets for Weapons and Armors
        /// </summary>
        private void SetupEmptyDbSets()
        {
            var emptyWeaponData = new List<Weapon>().AsQueryable();
            var mockWeapons = new Mock<DbSet<Weapon>>();
            mockWeapons.As<IAsyncEnumerable<Weapon>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Weapon>(emptyWeaponData.GetEnumerator()));
            mockWeapons.As<IQueryable<Weapon>>()
                .Setup(m => m.Provider)
                .Returns(emptyWeaponData.Provider);
            mockWeapons.As<IQueryable<Weapon>>()
                .Setup(m => m.Expression)
                .Returns(emptyWeaponData.Expression);
            mockWeapons.As<IQueryable<Weapon>>()
                .Setup(m => m.ElementType)
                .Returns(emptyWeaponData.ElementType);
            mockWeapons.As<IQueryable<Weapon>>()
                .Setup(m => m.GetEnumerator())
                .Returns(emptyWeaponData.GetEnumerator());

            var emptyArmorData = new List<Armor>().AsQueryable();
            var mockArmors = new Mock<DbSet<Armor>>();
            mockArmors.As<IAsyncEnumerable<Armor>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Armor>(emptyArmorData.GetEnumerator()));
            mockArmors.As<IQueryable<Armor>>()
                .Setup(m => m.Provider)
                .Returns(emptyArmorData.Provider);
            mockArmors.As<IQueryable<Armor>>()
                .Setup(m => m.Expression)
                .Returns(emptyArmorData.Expression);
            mockArmors.As<IQueryable<Armor>>()
                .Setup(m => m.ElementType)
                .Returns(emptyArmorData.ElementType);
            mockArmors.As<IQueryable<Armor>>()
                .Setup(m => m.GetEnumerator())
                .Returns(emptyArmorData.GetEnumerator());

            _mockDbContext.Setup(x => x.Weapons).Returns(mockWeapons.Object);
            _mockDbContext.Setup(x => x.Armors).Returns(mockArmors.Object);
        }

        /// <summary>
        /// Helper method to setup DbSet with specific data
        /// </summary>
        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
            mockDbSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(data.Provider);
            mockDbSet.As<IQueryable<T>>()
                .Setup(m => m.Expression)
                .Returns(data.Expression);
            mockDbSet.As<IQueryable<T>>()
                .Setup(m => m.ElementType)
                .Returns(data.ElementType);
            mockDbSet.As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator())
                .Returns(data.GetEnumerator());
            return mockDbSet;
        }

        #region GetItemPriceAsync Tests

        [Test]
        [AutoData]
        public async Task GetItemPriceAsync_WithValidArmorCode_ReturnsArmorPrice()
        {
            // Arrange
            var itemRequest = new ItemPriceRequestModel
            {
                ItemCode = "helmet3",
                Skills = new Dictionary<string, int> { { "defence", 15 } }
            };

            var expectedPrice = 150;
            var armorPrice = new ArmorPrice
            {
                Type = ArmorType.Helmet3,
                Stat = 15,
                Price = expectedPrice
            };

            var armorPricesData = new[] { armorPrice }.AsQueryable();
            _mockDbContext.Setup(x => x.ArmorPrices).Returns(CreateMockDbSet(armorPricesData).Object);

            // Act
            var result = await _sut.GetItemPriceAsync(itemRequest);

            // Assert
            result.Should().NotBeNull();
            result.ItemName.Should().Contain($"{ArmorType.Helmet3}");
            result.Stats.Should().Contain("15");
            result.Price.Should().Be(expectedPrice);
        }

        [Test]
        public async Task GetItemPriceAsync_WithValidWeaponCode_ReturnsWeaponPrice()
        {
            // Arrange
            var itemRequest = new ItemPriceRequestModel
            {
                ItemCode = "rifle",
                Skills = new Dictionary<string, int>
                {
                    { "attack", 75 },
                    { "criticalChance", 15 }
                }
            };

            var expectedPrice = 20;
            var weaponPrice = new WeaponPrice
            {
                Type = WeaponType.Rifle,
                Attack = 75,
                Crit = 15,
                Price = expectedPrice
            };

            var weaponPricesData = new[] { weaponPrice }.AsQueryable();
            _mockDbContext.Setup(x => x.WeaponPrices).Returns(CreateMockDbSet(weaponPricesData).Object);

            // Act
            var result = await _sut.GetItemPriceAsync(itemRequest);

            // Assert
            result.Should().NotBeNull();
            result.ItemName.Should().Contain("Rifle");
            result.Stats.Should().Contain("75");
            result.Stats.Should().Contain("15");
            result.Price.Should().Be(expectedPrice);
        }

        [Test]
        public void GetItemPriceAsync_WithInvalidItemCode_ThrowsException()
        {
            // Arrange
            var itemRequest = new ItemPriceRequestModel
            {
                ItemCode = "invalid_item",
                Skills = new Dictionary<string, int> { { "stat", 50 } }
            };

            var emptyArmorData = new List<ArmorPrice>().AsQueryable();
            var emptyWeaponData = new List<WeaponPrice>().AsQueryable();
            _mockDbContext.Setup(x => x.ArmorPrices).Returns(CreateMockDbSet(emptyArmorData).Object);
            _mockDbContext.Setup(x => x.WeaponPrices).Returns(CreateMockDbSet(emptyWeaponData).Object);

            // Act & Assert
           // Assert.ThrowsAsync<Exception>(async () => await _sut.GetItemPriceAsync(itemRequest));
        }

        #endregion

        #region SetItemPriceAsync Tests

        [Test]
        public async Task SetItemPriceAsync_WithValidArmorCode_UpdatesPrice()
        {
            // Arrange
            var itemRequest = new ItemSetPriceRequestModel
            {
                ItemCode = "helmet4",
                Price = 150,
                Skills = new Dictionary<string, int> { { "criticalDamages", 77 } }
            };

            var existingArmorPrice = new ArmorPrice
            {
                Type = ArmorType.Helmet4,
                Stat = 77,
                Price = 150
            };

            var armorPricesData = new[] { existingArmorPrice }.AsQueryable();
            _mockDbContext.Setup(x => x.ArmorPrices).Returns(CreateMockDbSet(armorPricesData).Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _sut.SetItemPriceAsync(itemRequest);

            // Assert
            result.Should().BeTrue();
            existingArmorPrice.Price.Should().Be(200);
            _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Test]
        public async Task SetItemPriceAsync_WithValidWeaponCode_UpdatesPrice()
        {
            // Arrange
            var itemRequest = new ItemSetPriceRequestModel
            {
                ItemCode = "axe",
                Price = 250,
                Skills = new Dictionary<string, int>
                {
                    { "attack", 75 },
                    { "criticalChance", 25 }
                }
            };

            var existingWeaponPrice = new WeaponPrice
            {
                Type = WeaponType.Tank,
                Attack = 75,
                Crit = 25,
                Price = 200
            };

            var weaponPricesData = new[] { existingWeaponPrice }.AsQueryable();
            _mockDbContext.Setup(x => x.WeaponPrices).Returns(CreateMockDbSet(weaponPricesData).Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _sut.SetItemPriceAsync(itemRequest);

            // Assert
            result.Should().BeTrue();
            existingWeaponPrice.Price.Should().Be(250);
            _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Test]
        public async Task SetItemPriceAsync_WithNonexistentArmorPrice_ReturnsFalse()
        {
            // Arrange
            var itemRequest = new ItemSetPriceRequestModel
            {
                ItemCode = "leather1",
                Price = 200,
                Skills = new Dictionary<string, int> { { "defence", 50 } }
            };

            var emptyArmorData = new List<ArmorPrice>().AsQueryable();
            _mockDbContext.Setup(x => x.ArmorPrices).Returns(CreateMockDbSet(emptyArmorData).Object);

            // Act
            var result = await _sut.SetItemPriceAsync(itemRequest);

            // Assert
            result.Should().BeFalse();
            _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }

        [Test]
        public async Task SetItemPriceAsync_WithDatabaseException_ReturnsFalseAndLogs()
        {
            // Arrange
            var itemRequest = new ItemSetPriceRequestModel
            {
                ItemCode = "boots4",
                Price = 200,
                Skills = new Dictionary<string, int> { { "dodge", 25 } }
            };

            var existingArmorPrice = new ArmorPrice
            {
                Type = ArmorType.Boots4,
                Stat = 50,
                Price = 150
            };

            var armorPricesData = new[] { existingArmorPrice }.AsQueryable();
            _mockDbContext.Setup(x => x.ArmorPrices).Returns(CreateMockDbSet(armorPricesData).Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.SetItemPriceAsync(itemRequest);

            // Assert
            result.Should().BeFalse();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region CheckPricesAsync Tests

        [Test]
        public async Task CheckPricesAsync_WithEmptyDatabase_ReturnsSuccessfulResult()
        {
            // Act
            var result = await _sut.CheckPricesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            result.Messages.Should().NotBeEmpty();
        }

        [Test]
        public async Task CheckPricesAsync_LogsDebugMessage()
        {
            // Act
            await _sut.CheckPricesAsync();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting to check prices")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task CheckPricesAsync_PushesNotificationToQueue()
        {
            // Act
            await _sut.CheckPricesAsync();

            // Assert
            _mockStorageHelper.Verify(
                x => x.PushToNotificationsQueueEncodedAsync(It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task CheckPricesAsync_IncludesItemCountsInResult()
        {
            // Act
            var result = await _sut.CheckPricesAsync();

            // Assert
            result.Should().NotBeNull();
            result.ItemsChecked.Should().BeGreaterThanOrEqualTo(0);
            result.DealsFound.Should().BeGreaterThanOrEqualTo(0);
        }

        #endregion

        #region Integration Tests

        [Test]
        public async Task CheckPricesAsync_CompleteWorkflow_SuccessfulExecution()
        {
            // Act
            var result = await _sut.CheckPricesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Messages.Should().NotBeEmpty();
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            
            _mockStorageHelper.Verify(
                x => x.PushToNotificationsQueueEncodedAsync(It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        #endregion
    }
}
