using QShop;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.AddHostedService<Worker>();
        services.AddHttpClient("shop", c =>
        {
            c.BaseAddress = new Uri("https://localhost:44338/");
            c.Timeout= TimeSpan.FromSeconds(120);
        });
    })
    .Build();


host.Run();
