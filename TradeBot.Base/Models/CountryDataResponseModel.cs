using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TradeBot.Base.Models
{
    /// <summary>
    /// Represents a country's state including finances, inventory, and market info
    /// </summary>
    public class CountryDataContainerModel
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("managers")]
        public List<string> Managers { get; set; }

        [JsonPropertyName("money")]
        public decimal Money { get; set; }

        [JsonPropertyName("items")]
        public ItemsData Items { get; set; }

        [JsonPropertyName("market")]
        public MarketData Market { get; set; }

        [JsonPropertyName("estimatedValues")]
        public EstimatedValues EstimatedValues { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("__v")]
        public int Version { get; set; }
    }

    /// <summary>
    /// Contains inventory categorized by type
    /// </summary>
    public class ItemsData
    {
        [JsonPropertyName("basics")]
        public Dictionary<string, long> Basics { get; set; }

        [JsonPropertyName("weapons")]
        public List<WeaponItem> Weapons { get; set; }

        [JsonPropertyName("equipments")]
        public List<EquipmentItem> Equipments { get; set; }
    }

    /// <summary>
    /// Market data including item prices and locked money
    /// </summary>
    public class MarketData
    {
        [JsonPropertyName("basics")]
        public Dictionary<string, int> Basics { get; set; }

        [JsonPropertyName("lockedMoney")]
        public decimal LockedMoney { get; set; }
    }

    /// <summary>
    /// Estimated monetary values of country assets
    /// </summary>
    public class EstimatedValues
    {
        [JsonPropertyName("money")]
        public decimal Money { get; set; }

        [JsonPropertyName("basicItems")]
        public decimal BasicItems { get; set; }

        [JsonPropertyName("weapons")]
        public decimal Weapons { get; set; }

        [JsonPropertyName("equipments")]
        public decimal Equipments { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Represents congressional data including members and government positions
    /// </summary>
    public class CongressDataContainerModel
    {
        [JsonPropertyName("dates")]
        public CongressDates Dates { get; set; }

        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("congressMembers")]
        public List<string> CongressMembers { get; set; }

        [JsonPropertyName("president")]
        public string President { get; set; }

        [JsonPropertyName("vicePresident")]
        public string VicePresident { get; set; }

        [JsonPropertyName("minOfDefense")]
        public string MinOfDefense { get; set; }

        [JsonPropertyName("minOfForeignAffairs")]
        public string MinOfForeignAffairs { get; set; }

        [JsonPropertyName("minOfEconomy")]
        public string MinOfEconomy { get; set; }

        [JsonPropertyName("__v")]
        public int Version { get; set; }
    }

    /// <summary>
    /// Congress-related dates
    /// </summary>
    public class CongressDates
    {
        [JsonPropertyName("announcementCreatedAts")]
        public List<DateTime> AnnouncementCreatedAts { get; set; }
    }

    /// <summary>
    /// Represents a collection of laws/proposals with pagination
    /// </summary>
    public class CountryLawsDataContainerModel
    {
        [JsonPropertyName("items")]
        public List<LawItem> ItemsModel { get; set; }

        [JsonPropertyName("nextCursor")]
        public string NextCursor { get; set; }
    }

    /// <summary>
    /// Represents a single law/proposal with voting information
    /// </summary>
    public class LawItem
    {
        [JsonPropertyName("votes")]
        public VotesData Votes { get; set; }

        [JsonPropertyName("data")]
        public LawData Data { get; set; }

        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("endAt")]
        public DateTime EndAt { get; set; }

        [JsonPropertyName("vicePresidentVoted")]
        public bool VicePresidentVoted { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("additionalCost")]
        public decimal? AdditionalCost { get; set; }

        [JsonPropertyName("__v")]
        public int Version { get; set; }
    }

    /// <summary>
    /// Voting information for a law
    /// </summary>
    public class VotesData
    {
        [JsonPropertyName("accept")]
        public List<string> Accept { get; set; }

        [JsonPropertyName("abstain")]
        public List<string> Abstain { get; set; }

        [JsonPropertyName("reject")]
        public List<string> Reject { get; set; }
    }

    /// <summary>
    /// Base class for law data - can be various types like declare_war, define_enemy_country, etc.
    /// </summary>
    public class LawData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("taxType")]
        public string TaxType { get; set; }

        [JsonPropertyName("discordUrl")]
        public string DiscordUrl { get; set; }

        [JsonPropertyName("targetCountry")]
        public string TargetCountry { get; set; }

        [JsonPropertyName("targetRegion")]
        public string TargetRegion { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }
    }

    /// <summary>
    /// Represents a weapon item in inventory
    /// </summary>
    public class WeaponItem
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("quality")]
        public int? Quality { get; set; }

        [JsonPropertyName("condition")]
        public decimal? Condition { get; set; }
    }

    /// <summary>
    /// Represents an equipment item in inventory
    /// </summary>
    public class EquipmentItem
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("quality")]
        public int? Quality { get; set; }

        [JsonPropertyName("condition")]
        public decimal? Condition { get; set; }
    }
}
