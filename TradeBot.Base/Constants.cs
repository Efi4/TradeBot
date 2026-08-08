namespace TradeBot.Base;

public static class Constants
{
    public static class EnvironmentVariables
    {
        public const string AzureStorageConnectionStringEnvironmentVariableName = "CUSTOMCONNSTR_tradingStorageAccount";
        public const string AzureSqlConnectionStringEnvironmentVariableName = "SQLAZURECONNSTR_tradingDatabase";
        public const string LocalSqlConnectionStringEnvironmentVariableName = "ConnectionStrings__tradingDatabase";
        public const string CookieHeaderEnvironmentVariableName = "RequestDataOptions--HttpHeadersDictionary--Cookie";
    }
    public static class Appsettings
    {
        public const string RequestDataOptionsSectionName = "RequestDataOptions";
        public const string StatRangeOptionsSectionName = "StatRangeOptions";
        public const string DiscordIntegrationOptionsSectionName = "DiscordIntegrationOptions";
        public const string CountryLawsOptionsSectionName = "CountryLawsListOptions";
    }
    public static class AlternativeHeaders
    {
        public const string RequestPathDictionaryKey = "%3Apath";
        public const string BatchRequestPathDictionaryValue = "/trpc/itemOffer.getItemOffers?batch=1";
    }
    public static class AzureStorageConfiguration
    {
        public const string TradeDealsQueueName = "trade-deals";
        public const string NotificationsQueueName = "notifications";
        public const string RegionTransferNotificationsQueueName = "region-transfer-notifications";
        public const string BlobContainerName = "deals";
    }
    public static class WeaponStatRanges
    {
        public const int MaxRifleAttack = 90;
        public const int MinSniperAttack = 101;
        public const int MaxSniperAttack = 130;
        public const int MinTankAttack = 141;
        public const int MaxTankAttack = 170;
        public const int MinJetAttack = 221;
        public const int MaxJetAttack = 300;
        public const int MaxRifleCrit = 15;
        public const int MinSniperCrit = 16;
        public const int MaxSniperCrit = 20;
        public const int MinTankCrit = 26;
        public const int MaxTankCrit = 35;
        public const int MinJetCrit = 41;
        public const int MaxJetCrit = 50;
    }
    public static class RareItemsBrowseLimits
    {
        public const int MinWeaponCrit = 14;
        public const int MinWeaponDmg = 81;
        public const int MinArmorStat = 14;
        public const int MinHelmetStat = 45;
    }
    public static class EquipmentLookup
    {
        public const double ReasonableEquipmentTreshold = 0.2;
        public const string AttackStatName = "attack";
        public const string CritStatName = "criticalChance";
        public static Dictionary<string, string> StatMapping = new()
        {
            {"helmet", "criticalDamages"},
            {"chest", "armor"},
            {"pants","armor"},
            {"boots", "dodge"},
            {"gloves", "precision"}
        };
        public static Dictionary<string, string> NameMapping = new()
        {
            {"helmet3", "Blue Helmet"},
            {"helmet4", "Purple Helmet"},
            {"helmet5", "Legendary Helmet"},
            {"helmet6", "Mythic Helmet"},
            {"boots3", "Blue Boots"},
            {"boots4", "Purple Boots"},
            {"boots5", "Legendary Boots"},
            {"boots6", "Mythic Boots"},
            {"chest3", "Blue Chest"},
            {"chest4", "Purple Chest"},
            {"chest5", "Legendary Chest"},
            {"chest6", "Mythic Chest"},
            {"gloves3", "Blue Gloves"},
            {"gloves4", "Purple Gloves"},
            {"gloves5", "Legendary Gloves"},
            {"gloves6", "Mythic Gloves"},
            {"pants3", "Blue Pants"},
            {"pants4", "Purple Pants"},
            {"pants5", "Legendary Pants"},
            {"pants6", "Mythic Pants"},
            {"rifle", "Blue Rifle"},
            {"sniper", "Purple Sniper Gun"},
            {"tank", "Legendary Tank"},
            {"jet", "Mythic Jet"}
        };
    }
    public static class CountryLookup
    {
        public static Dictionary<string, string> CountryMapping = new()
        {
            {"6813b6d546e731854c7ac865", "Ukraine"},
            {"6813b6d446e731854c7ac7eb", "Turkiye"},
            {"6813b6d446e731854c7ac7b2", "Hungary"},
            {"6813b6d446e731854c7ac7a2", "Italy"},
            {"6813b6d446e731854c7ac7be", "Bulgaria"},
            {"6813b6d446e731854c7ac7b6", "Romania"},
            {"6813b6d446e731854c7ac7ba", "Serbia"},
            {"6813b6d446e731854c7ac7b4", "Slovenia"},
            {"6813b6d446e731854c7ac7bc", "Croatia"},
            {"6813b6d446e731854c7ac7e8", "Greece"},
            {"6873d0ea1758b40e712b5f3d", "Malta"},
            {"6813b6d446e731854c7ac7ae", "Poland"},
            {"6813b6d546e731854c7ac8a6", "Iran"},
            {"6813b6d546e731854c7ac8d1", "Azerbaijan"},
            {"6813b6d446e731854c7ac805", "Slovakia"},
            {"6813b6d546e731854c7ac8c1", "Uzbekistan"},
            {"6813b6d446e731854c7ac7b8", "Lithuania"},
            {"683ddd2c24b5a2e114af15c3", "Iraq"},
            {"6813b6d546e731854c7ac842", "Cyprus"},
            {"6813b6d446e731854c7ac7b0", "Czechia"},
            {"6813b6d446e731854c7ac7ac", "Austria"},
            {"6813b6d446e731854c7ac79a", "France"},
            {"683ddd2c24b5a2e114af1612", "SouthAfrica"},
            {"6813b6d546e731854c7ac83c", "Chile"},
            {"6813b6d546e731854c7ac848", "Morocco"},
            {"6813b6d546e731854c7ac832", "Argentina"},
            {"6813b6d546e731854c7ac85f", "Colombia"},
            {"6813b6d446e731854c7ac7e5", "UnitedStates"},
            {"6813b6d546e731854c7ac84e", "Tunisia"},
            {"6813b6d546e731854c7ac8c4", "Kyrgyzstan"},
            {"6873d0ea1758b40e712b5f1c", "Gunea-Bisau"},
            {"6813b6d546e731854c7ac886", "Cuba"},
            {"6813b6d546e731854c7ac89c", "PuertoRico"},
            {"683ddd2c24b5a2e114af15d7", "Mongolia"},
            {"6873d0ea1758b40e712b5f79", "Liberia"},
            {"696a81da63e2489f47e5a28c", "Liechtenstein"},
            {"6873d0ea1758b40e712b5f59", "Eswatini"},
            {"6873d0ea1758b40e712b5f73", "Zambia"},
            {"6813b6d546e731854c7ac87a", "Belarus"},
            {"6813b6d546e731854c7ac868", "rusia"},
            {"683ddd2c24b5a2e114af15bf", "Georgia"},
            {"6813b6d546e731854c7ac86b", "Moldova"}
        };
        public static string[] Neighbours =
        {
            "6813b6d446e731854c7ac7ae", //"Poland"
            "6813b6d546e731854c7ac8d1", //"Azerbaijan"
            "6813b6d546e731854c7ac868", //"rusia"
            "6813b6d546e731854c7ac87a", //"Belarus"
            "6813b6d546e731854c7ac8a6", //"Iran"
            "6813b6d446e731854c7ac7eb", //"Turkiye"
            "6813b6d446e731854c7ac7b2", //"Hungary"
            "6813b6d446e731854c7ac7b6", //"Romania"
            "683ddd2c24b5a2e114af15bf", //"Georgia"
            "6813b6d546e731854c7ac86b", //"Moldova"
            "6813b6d446e731854c7ac805"  //"Slovakia"
        };
        public static string[] OliveUnion =
        {
            "6813b6d446e731854c7ac79a", //"France"
            "6873d0ea1758b40e712b5f3d", //"Malta"
            "6813b6d446e731854c7ac7eb", //"Turkiye"
            "6813b6d446e731854c7ac7b2", //"Hungary"
            "6813b6d446e731854c7ac7a2", //"Italy"
            "6813b6d446e731854c7ac7be", //"Bulgaria"
            "6813b6d446e731854c7ac7bc", //"Croatia"
            "6813b6d446e731854c7ac7e8", //"Greece"
            "683ddd2c24b5a2e114af1612", //"SouthAfrica
            "6813b6d546e731854c7ac83c", //"Chile"
            "6813b6d546e731854c7ac848", //"Morocco"
            "6813b6d546e731854c7ac832", //"Argentina"
            "6813b6d546e731854c7ac85f", //"Colombia"
            "6813b6d446e731854c7ac7e5", //"UnitedStates"
            "6813b6d546e731854c7ac84e", //"Tunisia"
            "6873d0ea1758b40e712b5f1c", //"Gunea-Bisau"
            "6813b6d546e731854c7ac886", //"Cuba"
            "6813b6d546e731854c7ac89c", //"PuertoRico"
            "683ddd2c24b5a2e114af15d7", //"Mongolia"
            "6873d0ea1758b40e712b5f79", //"Liberia"
            "696a81da63e2489f47e5a28c", //"Liechtenstein"
            "6873d0ea1758b40e712b5f59", //"Eswatini"
            "6873d0ea1758b40e712b5f73" //"Zambia"
        };
    }
}