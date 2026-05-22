using NUnit.Framework;
using AutoFixture.NUnit3;
using TradeBot.Base.Models;
using FluentAssertions;
using System;

namespace TradeBot.UnitTests.TradeBot.Base.Models;

[TestFixture]
public class EquipmentQueueMessageModelTests
{
    [Test]
    [AutoData]
    public void Ctor_PropertiesSet_SamePropertiesGet(decimal margin, decimal price, DateTime createdAt, ItemModel item)
    {
        // Arrange
        // Act
        var sut = new EquipmentQueueMessageModel()
        {
            Margin = margin,
            Price = price,
            CreatedAt = createdAt,
            Item = item
        };
        // Assert
        sut.Margin.Should().BeApproximately(margin,0.01m);
        sut.Price.Should().BeApproximately(price,0.01m);
        sut.CreatedAt.Should().Be(createdAt);
        sut.Item.Should().BeEquivalentTo(item);
    }
}