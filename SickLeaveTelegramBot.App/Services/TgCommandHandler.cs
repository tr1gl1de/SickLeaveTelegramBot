using Hangfire;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SickLeaveTelegramBot.App.Services;

public class TgCommandHandler
{
    private readonly ITelegramBotClient _botClient;

    public TgCommandHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task SendSicknessPollReport(Message message, CancellationToken cancellationToken)
    {
        await _botClient.SendPollAsync(
            chatId: message.Chat.Id,
            question: "Вы брали больничный?",
            isAnonymous: false,
            options: new[]
            {
                "Да",
                "Нет"
            },
            cancellationToken: cancellationToken);
    }

    public Task<Message> StartSendPoll(Message message, CancellationToken cancellationToken)
    {
        RecurringJob.AddOrUpdate(nameof(StartSendPoll),
            () => SendSicknessPollReport(
                message,
                cancellationToken),
        Cron.Minutely);
        return Task.FromResult(message);
    }
}