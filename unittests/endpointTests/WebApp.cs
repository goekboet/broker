using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

public static class WebApp
{
    public static WebApplicationFactory<http.Startup> Get(
        Dictionary<string, string> DbConnection
    )
    {
        return new WebApplicationFactory<http.Startup>()
            .WithWebHostBuilder(opts =>
            {
                opts.ConfigureLogging((hostingContext, logging) =>
                {
                    // Requires `using Microsoft.Extensions.Logging;`
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });

                opts.ConfigureAppConfiguration((ctx, cnf) =>
                {
                    cnf.AddInMemoryCollection(DbConnection);
                });
            });
    }
}