using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using TradeBot.Core.Interfaces;
using TradeBot.Base.Models;
using TradeBot.Base;
using TradeBot.Base.Objects;
using TradeBot.Data.Contexts;
using TradeBot.Data.Models;

namespace TradeBot.Core.Services;

public class CalculateAveragePriceService : ICalculateAveragePriceService
{
    private readonly ILogger<CalculateAveragePriceService> _logger;
    private readonly TradingDbContext _dbContext;
    private readonly IOptions<StatRangeOptions> _statRangeOptions;

    public CalculateAveragePriceService(ILogger<CalculateAveragePriceService> logger, TradingDbContext dbContext, IOptions<StatRangeOptions> statRangeOptions)
    {
        _logger = logger;
        _dbContext = dbContext;
        _statRangeOptions = statRangeOptions;
    }

    public async Task<bool> CalculateAverageWeaponPricesAsync()
    {
        _logger.LogDebug($"{nameof(CalculateAveragePriceService)}: Starting to calculate average weapon prices...");
        var weaponCount = await _dbContext.Weapons.CountAsync();
        if(weaponCount == 0)
        {
            _logger.LogError($"{nameof(CalculateAveragePriceService)}: No weapon items were found in the database. Calculating average price is not possible.");
            return false;
        }
        try
        {
            var weaponList = await _dbContext.Weapons.ToListAsync();
            var averagePriceList = new List<WeaponPrice>();
            for(int crit = Constants.RareItemsBrowseLimits.MinWeaponCrit; crit <= Constants.WeaponStatRanges.MaxJetCrit; crit++)
            {
                var attackRange = GetAttackRangeForCrit(crit);
                if (attackRange is null)    
                {
                    continue;
                }

                var (minAttack, maxAttack) = attackRange.Value;

                for(int attack = minAttack; attack <= maxAttack; attack++)  
                {
                    var weaponPositions = weaponList.Where(w => w.Attack == attack && w.Crit == crit).ToList();
                    if(weaponPositions.Count == 0)
                    {
                        _logger.LogDebug($"{nameof(CalculateAveragePriceService)}: No weapons with stats{attack}-{crit} were found.");
                        continue;
                    }
                    var sortedPrices = weaponPositions.OrderBy(w => w.Price).ToList();
                    var reasonableCount = (int)Math.Ceiling(sortedPrices.Count * 0.3);
                    var averageReasonableWeaponPrice = sortedPrices.Take(reasonableCount).Average(w => (decimal)w.Price);
                    averagePriceList.Add(new WeaponPrice
                    {
                        Type = weaponPositions.First().Type,
                        Attack = attack,
                        Crit = crit,
                        Price = averageReasonableWeaponPrice         
                    });
                }
            }
            await _dbContext.BulkInsertOrUpdateAsync(averagePriceList);
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug($"{nameof(CalculateAveragePriceService)}: Average prices for weapons was updated with relevant values.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(CalculateAveragePriceService)}: an error occurred while calculating average price for weapons.");
            return false;
        }
        return true;
    }

    public async Task<bool> CalculateAverageArmorPricesAsync()
    {
        var armorCount = await _dbContext.Armors.CountAsync();
        if(armorCount == 0)
        {
            _logger.LogError($"{nameof(CalculateAveragePriceService)}: No armor items were found in the database. Calculating average price is not possible.");
            return false;
        }
        _logger.LogDebug($"{nameof(CalculateAveragePriceService)}: Starting to calculate average armor prices...");
        try
        {
            var averagePriceList = new List<ArmorPrice>();   
            foreach(var armorType in Enum.GetValues<ArmorType>())
            {                
                var armorStatRange = _statRangeOptions.Value.ArmorStatRanges.Find(ar => ar.Name == armorType);
                if(armorStatRange == null) 
                {
                    _logger.LogWarning($"{nameof(CalculateAveragePriceService)}: Stat range for armor type {armorType} was not found in configuration. Average price calculation for this position will be skipped.");
                    continue;
                }
                var armorTypeExists = await _dbContext.Armors.AnyAsync(ar => ar.Type == armorType);
                if(!armorTypeExists)
                {
                    _logger.LogWarning($"{nameof(CalculateAveragePriceService)}: Armor items for type {armorType} was not found in database. Average price calculation for this position will be skipped.");
                    continue;
                }

                for(int i = armorStatRange.Stats["Min"]; i<= armorStatRange.Stats["Max"]; i++)
                {
                    var armorItemsPerTypePerStat = await _dbContext.Armors.Where(ar => ar.Type == armorType && ar.Stat == i).ToListAsync();
                    if( armorItemsPerTypePerStat == null || armorItemsPerTypePerStat.Count == 0)
                    {
                        _logger.LogDebug($"{nameof(CalculateAveragePriceService)}: No items of type '{armorType}' was not found for stat '{i}'. Average price calculation for this position will be skipped.");
                         continue;
                    }
                    var sortedArmorPrices = armorItemsPerTypePerStat.OrderBy(w => w.Price).ToList();
                    var reasonableCount = (int)Math.Ceiling(sortedArmorPrices.Count * 0.3);
                    var averageReasonableArmorPrice = sortedArmorPrices.Take(reasonableCount).Average(w => (decimal)w.Price);
                    averagePriceList.Add(new ArmorPrice
                    {
                        Type = armorType,
                        Stat = i,
                        Price = averageReasonableArmorPrice         
                    });
                }
            }
            await _dbContext.BulkInsertOrUpdateAsync(averagePriceList);
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug($"{nameof(CalculateAveragePriceService)}: Average prices for armor items was updated with relevant values.");
         }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(CalculateAveragePriceService)}: An error occurred while calculating average price of armor items.");
            return false;
        }
        return true;
    }

    private (int, int)? GetAttackRangeForCrit(int crit) => crit switch
    {
        <= Constants.WeaponStatRanges.MaxRifleCrit => (Constants.RareItemsBrowseLimits.MinWeaponDmg, Constants.WeaponStatRanges.MaxRifleAttack),
        > Constants.WeaponStatRanges.MaxRifleCrit and <= Constants.WeaponStatRanges.MaxSniperCrit => (Constants.WeaponStatRanges.MinSniperAttack, Constants.WeaponStatRanges.MaxSniperAttack),
        > Constants.WeaponStatRanges.MaxSniperCrit and < Constants.WeaponStatRanges.MinTankCrit => null,
        >= Constants.WeaponStatRanges.MinTankCrit and <= Constants.WeaponStatRanges.MaxTankCrit => (Constants.WeaponStatRanges.MinTankAttack, Constants.WeaponStatRanges.MaxTankAttack),
        > Constants.WeaponStatRanges.MaxTankCrit and < Constants.WeaponStatRanges.MinJetCrit => null,
        >= Constants.WeaponStatRanges.MinJetCrit => (Constants.WeaponStatRanges.MinJetAttack, Constants.WeaponStatRanges.MaxJetAttack)
    };

}