using Hangfire;
using Hangfire.MemoryStorage;
using Newtonsoft.Json;
using SickLeaveTelegramBot.App;
using SickLeaveTelegramBot.App.Contracts;
using SickLeaveTelegramBot.App.Services;
using SickLeaveTelegramBot.App.Workers;
using Telegram.Bot;
using Telegram.Bot.Examples.Polling;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(context.Configuration.GetSection(BotConfiguration.Configuration));
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetConfiguration<BotConfiguration>();
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });
        services.AddHangfire(x => x.UseMemoryStorage().UseSerializerSettings(new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto // For storing interface implementation types in json.
        }));
        services.AddHangfireServer();
        services.AddScoped<IReceiverService, ReceiverService>();
        services.AddScoped<UpdateHandler>();
        services.AddTransient<TgCommandHandler>();
        services.AddHostedService<PollingWorkerService>();
    })
    .Build();

await host.RunAsync();

#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable RCS1110 // Declare type inside namespace.
namespace Telegram.Bot.Examples.Polling
{
    public class BotConfiguration
#pragma warning restore RCS1110 // Declare type inside namespace.
#pragma warning restore CA1050 // Declare types in namespaces
    {
        public static readonly string Configuration = "BotConfiguration";

        public string BotToken { get; set; } = "";
    }
}