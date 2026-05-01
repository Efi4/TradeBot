namespace TradeBot.Base;

public static class Constants
{
    public static class EnvironmentVariables
    {
        public const string AzureStorageConnectionStringEnvironmentVariableName = "CUSTOMCONNSTR_tradingStorageAccount";
        public const string AzureSqlConnectionStringEnvironmentVariableName = "SQLAZURECONNSTR_tradingDatabase";
        public const string LocalSqlConnectionStringEnvironmentVariableName = "ConnectionStrings__tradingDatabase";
    }
    public static class AzureStorageConfiguration
    {
        public const string TradeDealsQueueName = "trade-deals";
        public const string BlobContainerName = "deals";
    }
    public static class WeaponStatRanges
    {
        public const int MinSniperAttack = 101;
        public const int MaxSniperAttack = 130;
        public const int MinTankAttack = 141;
        public const int MaxTankAttack = 170;
        public const int MinJetAttack = 221;
        public const int MaxJetAttack = 300;
        public const int MinSniperCrit = 16;
        public const int MaxSniperCrit = 20;
        public const int MinTankCrit = 26;
        public const int MaxTankCrit = 35;
        public const int MinJetCrit = 41;
        public const int MaxJetCrit = 50;
    }
    public static class EquipmentLookup
    {
        public static Dictionary<string, string> Mapping = new()
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
            {"gloves3", "Blue Gloves3"},
            {"gloves4", "Purple Gloves3"},
            {"gloves5", "Legendary Gloves3"},
            {"gloves6", "Mythic Gloves3"},
            {"pants3", "Blue Pants3"},
            {"pants4", "Purple Pants3"},
            {"pants5", "Legendary Pants3"},
            {"pants6", "Mythic Pants3"}
        };
    }
}
