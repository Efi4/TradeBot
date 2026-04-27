namespace TradeBot.Base;

public static class Constants
{
    public static class EnvironmentVariables
    {
        public const string AzureStorageConnectionStringEnvironmentVariableName = "CUSTOMCONNSTR_tradingStorageAccount";
        public const string AzureSqlConnectionStringEnvironmentVariableName = "SQLAZURECONNSTR_tradingDatabase";
        public const string LocalSqlConnectionStringEnvironmentVariableName = "ConnectionStrings__tradingDatabase";
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
}
