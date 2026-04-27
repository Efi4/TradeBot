using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Models;
using TradeBot.Base;
using TradeBot.Base.Objects;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using TradeBot.Core.Objects;
using System.Linq;
using TradeBot.Data.Contexts;
using TradeBot.Data.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using EFCore.BulkExtensions;

namespace TradeBot.Core.Services;

public class CalculateAveragePriceService : ICalculateAveragePriceService
{
    private readonly ILogger<CalculateAveragePriceService> _logger;
    private readonly TradingDbContext _dbContext;
    private List<WeaponObject> _weaponObjects;

    public CalculateAveragePriceService(ILogger<CalculateAveragePriceService> logger, TradingDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _weaponObjects = new List<WeaponObject>();
    }

    public async Task<bool> CalculateAveragePriceAsync()
    {
        _logger.LogInformation("Starting to calculate average price...");
        try
        {
            var weaponList =_dbContext.Weapons.ToList();
            var averagePriceList = new List<WeaponPrice>();
            for(int crit = Constants.WeaponStatRanges.MinSniperCrit; crit <= Constants.WeaponStatRanges.MaxJetCrit; crit++)
            {
                var attackRange = GetAttackRangeForCrit(crit);
                if (attackRange is null)    continue;

                var (minAttack, maxAttack) = attackRange.Value;

                for(int attack = minAttack; attack <= maxAttack; attack++)  
                {
                    _logger.LogInformation($"Calculating average price for Attack: {attack}, Crit: {crit}");
                    var weaponPositions = weaponList.Where(w => w.Attack == attack && w.Crit == crit).ToList();
                    if(weaponPositions.Count == 0)
                    {
                        continue;
                    }
                    var averagePrice = weaponPositions.Sum(w => w.Price) / weaponPositions.Count;
                    averagePriceList.Add(new WeaponPrice
                    {
                        Type = weaponPositions.First().Type,
                        Attack = attack,
                        Crit = crit,
                        Price = averagePrice         
                    });
                }
            }
            await _dbContext.BulkInsertOrUpdateOrDeleteAsync(averagePriceList);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Prices: " + string.Join(" '\n'", _dbContext.WeaponPrices.ToList().Select(wp => $"Attack: {wp.Attack}, Crit: {wp.Crit}, Price: {wp.Price}")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calculating average price.");
            return false;
        }
        return true;
    }

    private (int, int)? GetAttackRangeForCrit(int crit) => crit switch
    {
        <= Constants.WeaponStatRanges.MaxSniperCrit => (Constants.WeaponStatRanges.MinSniperAttack, Constants.WeaponStatRanges.MaxSniperAttack),
        > Constants.WeaponStatRanges.MaxSniperCrit and < Constants.WeaponStatRanges.MinTankCrit => null,
        >= Constants.WeaponStatRanges.MinTankCrit and <= Constants.WeaponStatRanges.MaxTankCrit => (Constants.WeaponStatRanges.MinTankAttack, Constants.WeaponStatRanges.MaxTankAttack),
        >= Constants.WeaponStatRanges.MaxTankCrit and <= Constants.WeaponStatRanges.MinJetCrit => null,
        >= Constants.WeaponStatRanges.MinJetCrit => (Constants.WeaponStatRanges.MinJetAttack, Constants.WeaponStatRanges.MaxJetAttack)
    };

}