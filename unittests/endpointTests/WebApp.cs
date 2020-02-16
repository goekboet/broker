using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

public static class WebApp
{
    private static IEnumerable<KeyValuePair<string, string>> TestConf = new
        Dictionary<string, string>()
    {
        ["Pgres:Host"] = "localhost",
        ["Pgres:Port"] = "5432",
        ["Pgres:Handle"] = "broker",
        ["Pgres:Pwd"] = "trtLAqkGY3nE3DyA",
        ["Pgres:Db"] = "meets"
    };

    public static WebApplicationFactory<http.Startup> Get()
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
                    cnf.AddInMemoryCollection(TestConf);
                });
            });
    }
}