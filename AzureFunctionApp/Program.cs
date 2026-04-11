using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureFunctionApp.Services;
using AzureFunctionApp.Interfaces;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddScoped<ICheckTheAvPricesService, CheckTheAvPricesService>();
    })
    .Build();

await host.RunAsync();

