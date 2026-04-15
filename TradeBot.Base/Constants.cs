namespace TradeBot.Base;

public static class Constants
{
    public static class EnvironmentVariables
    {
        public const string AzureStorageConnectionStringEnvironmentVariableName = "CUSTOMCONNSTR_tradingStorageAccount";
        public const string AzureSqlConnectionStringEnvironmentVariableName = "SQLAZURECONNSTR_tradingDatabase";
        public const string LocalSqlConnectionStringEnvironmentVariableName = "ConnectionStrings__tradingDatabase";
    }
}
