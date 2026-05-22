using NUnit.Framework;
using AutoFixture.NUnit3;
using TradeBot.Base.Models;
using FluentAssertions;

namespace TradeBot.UnitTests.TradeBot.Base.Models;

[TestFixture]
public class DiscordIntegrationOptionsTests
{
    [Test]
    [AutoData]
    public void Ctor_PropertiesSet_SamePropertiesGet(string webhookUrl, string notificationsWebhookUrl)
    {
        // Arrange
        // Act

        var sut = new DiscordIntegrationOptions()
        {
            WebHookUrl = webhookUrl,
            NotificationsWebHookUrl = notificationsWebhookUrl
        };

        // Assert
        sut.WebHookUrl.Should().Be(webhookUrl);
        sut.NotificationsWebHookUrl.Should().Be(notificationsWebhookUrl);
    }
}