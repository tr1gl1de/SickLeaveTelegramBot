using SickLeaveTelegramBot.App.Contracts;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace SickLeaveTelegramBot.App.Services;

public class ReceiverService : IReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandlers;
    private readonly ILogger<ReceiverService> _logger;

    public ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandlers, ILogger<ReceiverService> logger)
    {
        _botClient = botClient;
        _updateHandlers = updateHandlers;
        _logger = logger;
    }

    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            ThrowPendingUpdates = true,
        };

        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

        // Start receiving updates
        await _botClient.ReceiveAsync(
            updateHandler: _updateHandlers,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }
}