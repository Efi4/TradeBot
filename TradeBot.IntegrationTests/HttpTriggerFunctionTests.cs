using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using NUnit.Framework;
using FluentAssertions;

namespace TradeBot.IntegrationTests
{
    [TestFixture]
    public class HttpTriggerFunctionTests
    {
        private RestClient _client = null!;
        private const string FunctionAppBaseUrl = "http://localhost:7071";
        private const string HelloFunctionRoute = "/api/hello";

        [OneTimeSetUp]
        public void Setup()
        {
            _client = new RestClient(FunctionAppBaseUrl);
        }

        [Test]
        [Category("Integration")]
        public async Task HttpTriggerFunction_PostRequest_ShouldReturn200()
        {
            // Arrange
            var request = new RestRequest(HelloFunctionRoute, Method.Post);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccessful.Should().BeTrue();
        }

        [Test]
        [Category("Integration")]
        public async Task HttpTriggerFunction_PostRequest_ShouldHaveValidResponse()
        {
            // Arrange
            var request = new RestRequest(HelloFunctionRoute, Method.Post);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();
        }
    }
}
