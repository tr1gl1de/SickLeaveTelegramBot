using Hangfire;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SickLeaveTelegramBot.App.Services;

public class TgCommandHandler
{
    private readonly ITelegramBotClient _botClient;
    private const string CronTimer10 = "*/10 * * * * *";
    private const string CronTimer30 = "*/30 * * * * *";
    private readonly ILogger<TgCommandHandler> _logger;

    public TgCommandHandler(ITelegramBotClient botClient, ILogger<TgCommandHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<Message> SendSicknessPollReport(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendPollAsync(
            chatId: message.Chat.Id,
            question: "Да или нет ?",
            isAnonymous: false,
            options: new[]
            {
                "Да",
                "Нет"
            },
            cancellationToken: cancellationToken).WaitAsync(cancellationToken);
    }

    public async Task<Message> SendDice(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendDiceAsync(
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken).WaitAsync(cancellationToken);
    }

    public Task<Message> StartSendPoll(Message message, CancellationToken cancellationToken)
    {
        RecurringJob.AddOrUpdate($"{message.Chat.Id}",
              () => SendSicknessPollReport(
                message,
                cancellationToken),
            CronTimer10
        );
        RecurringJob.AddOrUpdate($"{message.Chat.Id+1}",
            () => SendDice(
                message,
                cancellationToken),
            CronTimer30
        );
        _logger.LogInformation($"Start job with id -> {message.Chat.Id}");
        return Task.FromResult(message);
    }

    public Task<Message> StopSendPoll(Message message)
    {
        RecurringJob.RemoveIfExists($"{message.Chat.Id}");
        RecurringJob.RemoveIfExists($"{message.Chat.Id+1}");
        _logger.LogInformation($"Stop job with id -> {message.Chat.Id}");
        return Task.FromResult(message);
    }
}