// See https://aka.ms/new-console-template for more information

using Marten;
using Marten.Testing.Harness;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weasel.Core;


var host =
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((builder, services) =>
        {

            services.AddSingleton<IHostedService, TimedHostedService>();
            // This is the absolute, simplest way to integrate Marten into your
// .NET application with Marten's default configuration
            services.AddMarten(options =>
            {
                // Establish the connection string to your Marten database
                options.Connection(ConnectionSource.ConnectionString);

                // Specify that we want to use STJ as our serializer
                options.UseSystemTextJsonForSerialization();
                options.AutoCreateSchemaObjects = AutoCreate.None; // Have to set this manually in testing
                options.Schema.For<TestDocument>();
                // options.ConfigurePolly(builder =>
                // {
                //     builder
                //         .AddRetry(new RetryStrategyOptions
                //         {
                //             ShouldHandle = new PredicateBuilder()
                //                 .Handle<NpgsqlException>()
                //                 .Handle<MartenCommandException>(),
                //             Delay = TimeSpan.FromMilliseconds(100),
                //             MaxRetryAttempts = 3,
                //             UseJitter = true,
                //             OnRetry = args =>
                //             {
                //                 Console.WriteLine($"Retry Attempt Number : {args.AttemptNumber} after {args.Duration.TotalMilliseconds} ms.");
                //                 return default;
                //             }
                //         })
                //         .Build();
                // });
            }).UseLightweightSessions();
        });

await host.RunConsoleAsync();

public class TestDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
public class TimedHostedService(ILogger<TimedHostedService> logger, ISessionFactory sessionFactory)
    : IHostedService, IDisposable
{
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger = logger;
    private Task _task;
    private CancellationTokenSource _cancellationTokenSource = new();

    public Task StartAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Timed Hosted Service running.");

        _task = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await DoWork(_cancellationTokenSource.Token);
                await Task.Delay(1000, _cancellationTokenSource.Token);
            }
        });


        return Task.CompletedTask;
    }

    private async Task DoWork(CancellationToken ct)
    {
        Console.WriteLine("do work");
        var count = Interlocked.Increment(ref executionCount);

        try
        {
            var items = await sessionFactory.OpenSession().Query<TestDocument>()
                .ToListAsync(ct);

            Console.WriteLine($"Found {items.Count} items");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Console.WriteLine(
            $"Timed Hosted Service is working. Count: {count}");
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Timed Hosted Service is stopping.");
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        await _task;
    }

    public void Dispose()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
