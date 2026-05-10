namespace TradeBot.Base.Objects;

/// <summary>
/// Enumeration of available weapon types in the trading system.
/// </summary>
public enum WeaponType
{
    /// <summary>Rifle weapon type.</summary>
    Rifle,
    /// <summary>Sniper rifle weapon type.</summary>
    Sniper,
    /// <summary>Tank-type weapon.</summary>
    Tank,
    /// <summary>Jet-type weapon.</summary>
    Jet
}

/// <summary>
/// Enumeration of available armor types and quality levels in the trading system.
/// Format: [ArmorType][QualityLevel] where QualityLevel ranges from 3 to 6.
/// </summary>
public enum ArmorType
{
    /// <summary>Helmet quality level 3.</summary>
    Helmet3,
    /// <summary>Helmet quality level 4.</summary>
    Helmet4,
    /// <summary>Helmet quality level 5.</summary>
    Helmet5,
    /// <summary>Helmet quality level 6.</summary>
    Helmet6,
    /// <summary>Chest armor quality level 3.</summary>
    Chest3,
    /// <summary>Chest armor quality level 4.</summary>
    Chest4,
    /// <summary>Chest armor quality level 5.</summary>
    Chest5,
    /// <summary>Chest armor quality level 6.</summary>
    Chest6,
    /// <summary>Gloves quality level 3.</summary>
    Gloves3,
    /// <summary>Gloves quality level 4.</summary>
    Gloves4,
    /// <summary>Gloves quality level 5.</summary>
    Gloves5,
    /// <summary>Gloves quality level 6.</summary>
    Gloves6,
    /// <summary>Pants quality level 3.</summary>
    Pants3,
    /// <summary>Pants quality level 4.</summary>
    Pants4,
    /// <summary>Pants quality level 5.</summary>
    Pants5,
    /// <summary>Pants quality level 6.</summary>
    Pants6,
    /// <summary>Boots quality level 3.</summary>
    Boots3,
    /// <summary>Boots quality level 4.</summary>
    Boots4,
    /// <summary>Boots quality level 5.</summary>
    Boots5,
    /// <summary>Boots quality level 6.</summary>
    Boots6
}