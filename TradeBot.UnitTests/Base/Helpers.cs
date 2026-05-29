using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Services;
using TradeBot.Data.Contexts;
using TradeBot.Data.Helpers;
using TradeBot.Data.Models;
using TradeBot.Base.Models;
using TradeBot.Base.Configuration;
using TradeBot.Base.Objects;
using System.Threading.Tasks;

namespace TradeBot.UnitTests.Base
{
    /// <summary>
    /// Fixture for TradeBot.Core services with all necessary mocks and registrations.
    /// Provides mocks for: Loggers, HttpClient, DbContext, Azure Storage Helper, and Options.
    /// </summary>
    public class CoreServicesFixture : IDisposable
    {
        #region Logger Mocks
        public Mock<ILogger<CalculateAveragePriceService>> MockCalculateAveragePriceServiceLogger { get; private set; } = null!;
        public Mock<ILogger<CheckThePricesService>> MockCheckThePricesServiceLogger { get; private set; } = null!;
        public Mock<ILogger<DiscordIntegrationService>> MockDiscordIntegrationServiceLogger { get; private set; } = null!;
        #endregion

        #region DbContext Mock
        public Mock<TradingDbContext> MockTradingDbContext { get; private set; } = null!;
        #endregion

        #region HttpClient Mocks
        public Mock<HttpClient> MockHttpClientForCheckPrices { get; private set; } = null!;
        public Mock<HttpClient> MockHttpClientForDiscord { get; private set; } = null!;
        #endregion

        #region Azure Storage Helper Mock
        public Mock<IAzureStorageHelper> MockAzureStorageHelper { get; private set; } = null!;
        #endregion

        #region Options Mocks
        public Mock<IOptions<StatRangeOptions>> MockStatRangeOptions { get; private set; } = null!;
        public Mock<IOptions<RequestDataOptions>> MockRequestDataOptions { get; private set; } = null!;
        public Mock<IOptions<DiscordIntegrationOptions>> MockDiscordIntegrationOptions { get; private set; } = null!;
        #endregion

        #region Service Instances
        public ICalculateAveragePriceService CalculateAveragePriceService { get; private set; } = null!;
        public ICheckThePricesService CheckThePricesService { get; private set; } = null!;
        public IDiscordIntegrationService DiscordIntegrationService { get; private set; } = null!;
        #endregion

        public CoreServicesFixture()
        {
            InitializeMocks();
            RegisterServices();
        }

        private void InitializeMocks()
        {
            // Initialize Logger Mocks
            MockCalculateAveragePriceServiceLogger = new Mock<ILogger<CalculateAveragePriceService>>();
            MockCheckThePricesServiceLogger = new Mock<ILogger<CheckThePricesService>>();
            MockDiscordIntegrationServiceLogger = new Mock<ILogger<DiscordIntegrationService>>();

            // Initialize DbContext Mock
            MockTradingDbContext = new Mock<TradingDbContext>();

            // Initialize HttpClient Mocks
            MockHttpClientForCheckPrices = new Mock<HttpClient>();
            MockHttpClientForDiscord = new Mock<HttpClient>();

            // Initialize Azure Storage Helper Mock
            MockAzureStorageHelper = new Mock<IAzureStorageHelper>();

            // Initialize Options Mocks with Default Values
            MockStatRangeOptions = CreateStatRangeOptionsMock();
            MockRequestDataOptions = CreateRequestDataOptionsMock();
            MockDiscordIntegrationOptions = CreateDiscordIntegrationOptionsMock();
        }

        private Mock<IOptions<StatRangeOptions>> CreateStatRangeOptionsMock()
        {
            var mock = new Mock<IOptions<StatRangeOptions>>();
            var value = new StatRangeOptions
            {
                ArmorStatRanges = new List<ArmorStatRange>()
            };
            mock.Setup(x => x.Value).Returns(value);
            return mock;
        }

        private Mock<IOptions<RequestDataOptions>> CreateRequestDataOptionsMock()
        {
            var mock = new Mock<IOptions<RequestDataOptions>>();
            var value = new RequestDataOptions
            {
                BaseUrl = "http://localhost:7071",
                BaseBatchUrl = "http://localhost:7071",
                HttpHeadersDictionary = new Dictionary<string, string>
                {
                    { "User-Agent", "TradeBot/1.0" },
                    { "Accept", "application/json" }
                }
            };
            mock.Setup(x => x.Value).Returns(value);
            return mock;
        }

        private Mock<IOptions<DiscordIntegrationOptions>> CreateDiscordIntegrationOptionsMock()
        {
            var mock = new Mock<IOptions<DiscordIntegrationOptions>>();
            var value = new DiscordIntegrationOptions
            {
                WebHookUrl = "https://discord.com/api/webhooks/test",
                NotificationsWebHookUrl = "https://discord.com/api/webhooks/notifications"
            };
            mock.Setup(x => x.Value).Returns(value);
            return mock;
        }

        private void RegisterServices()
        {
            // Register CalculateAveragePriceService
            CalculateAveragePriceService = new CalculateAveragePriceService(
                MockCalculateAveragePriceServiceLogger.Object,
                MockTradingDbContext.Object,
                MockStatRangeOptions.Object
            );

            // Register CheckThePricesService
            CheckThePricesService = new CheckThePricesService(
                MockCheckThePricesServiceLogger.Object,
                MockHttpClientForCheckPrices.Object,
                MockRequestDataOptions.Object,
                MockTradingDbContext.Object,
                MockAzureStorageHelper.Object
            );

            // Register DiscordIntegrationService
            DiscordIntegrationService = new DiscordIntegrationService(
                MockDiscordIntegrationServiceLogger.Object,
                MockHttpClientForDiscord.Object,
                MockDiscordIntegrationOptions.Object
            );
        }

        /// <summary>
        /// Resets all mocks to their default state.
        /// </summary>
        public void ResetAllMocks()
        {
            MockCalculateAveragePriceServiceLogger.Reset();
            MockCheckThePricesServiceLogger.Reset();
            MockDiscordIntegrationServiceLogger.Reset();
            MockTradingDbContext.Reset();
            MockHttpClientForCheckPrices.Reset();
            MockHttpClientForDiscord.Reset();
            MockAzureStorageHelper.Reset();
            MockStatRangeOptions.Reset();
            MockRequestDataOptions.Reset();
            MockDiscordIntegrationOptions.Reset();
        }

        /// <summary>
        /// Reinitializes all mocks and services. Useful after resetting mocks.
        /// </summary>
        public void Reinitialize()
        {
            ResetAllMocks();
            RegisterServices();
        }

        public void Dispose()
        {
            // Cleanup if needed
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Helper methods for CheckThePricesService unit tests.
    /// Provides utilities for setting up mocks and test data.
    /// </summary>
    public static class CheckThePricesServiceHelpers
    {
        /// <summary>
        /// Creates a configured mock for RequestDataOptions used by CheckThePricesService.
        /// </summary>
        public static Mock<IOptions<RequestDataOptions>> CreateMockRequestDataOptions()
        {
            var mock = new Mock<IOptions<RequestDataOptions>>();
            var value = new RequestDataOptions
            {
                BaseUrl = "http://localhost:7071",
                BaseBatchUrl = "http://localhost:7071/batch",
                HttpHeadersDictionary = new Dictionary<string, string>
                {
                    { "User-Agent", "TradeBot/1.0" },
                    { "Accept", "application/json" },
                    { "Cookie", "test_cookie=value123" }
                }
            };
            mock.Setup(x => x.Value).Returns(value);
            return mock;
        }

        /// <summary>
        /// Creates a mock TradingDbContext with basic setup for CheckThePricesService operations.
        /// </summary>
        public static Mock<TradingDbContext> CreateMockTradingDbContext()
        {
            var mock = new Mock<TradingDbContext>();
            
            // Setup DbSet mocks
            var mockWeaponPrices = new Mock<DbSet<WeaponPrice>>();
            var mockArmorPrices = new Mock<DbSet<ArmorPrice>>();
            var mockWeapons = new Mock<DbSet<Weapon>>();
            var mockArmors = new Mock<DbSet<Armor>>();

            mock.Setup(x => x.WeaponPrices).Returns(mockWeaponPrices.Object);
            mock.Setup(x => x.ArmorPrices).Returns(mockArmorPrices.Object);
            mock.Setup(x => x.Weapons).Returns(mockWeapons.Object);
            mock.Setup(x => x.Armors).Returns(mockArmors.Object);

            return mock;
        }

        /// <summary>
        /// Creates a mock IAzureStorageHelper for CheckThePricesService tests.
        /// </summary>
        public static Mock<IAzureStorageHelper> CreateMockAzureStorageHelper()
        {
            var mock = new Mock<IAzureStorageHelper>();
            
            // Setup queue operations to succeed by default
            mock.Setup(x => x.PushToNotificationsQueueEncodedAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            mock.Setup(x => x.PushToTradeDealsQueueEncodedAsync(It.IsAny<EquipmentQueueMessageModel>()))
                .Returns(Task.CompletedTask);

            return mock;
        }

        /// <summary>
        /// Creates a mock ILogger for CheckThePricesService tests.
        /// </summary>
        public static Mock<ILogger<CheckThePricesService>> CreateMockCheckThePricesLogger()
        {
            return new Mock<ILogger<CheckThePricesService>>();
        }

        /// <summary>
        /// Creates a complete test fixture with all necessary mocks for CheckThePricesService.
        /// </summary>
        public static (
            Mock<ILogger<CheckThePricesService>> Logger,
            Mock<IOptions<RequestDataOptions>> RequestOptions,
            Mock<TradingDbContext> DbContext,
            Mock<IAzureStorageHelper> StorageHelper,
            HttpClient HttpClient
        ) CreateCheckThePricesServiceTestFixture()
        {
            var logger = CreateMockCheckThePricesLogger();
            var requestOptions = CreateMockRequestDataOptions();
            var dbContext = CreateMockTradingDbContext();
            var storageHelper = CreateMockAzureStorageHelper();
            var httpClient = new HttpClient();

            return (logger, requestOptions, dbContext, storageHelper, httpClient);
        }
    }
}
