using Hangfire;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SickLeaveTelegramBot.App.Services;

public class TgCommandHandler
{
    private readonly ITelegramBotClient _botClient;
    private const int FirstDay = 15;
    private const int LastDay = 28;
    private readonly ILogger<TgCommandHandler> _logger;
    private readonly Dictionary<string, int> _dayDiffs = new();

    public TgCommandHandler(ITelegramBotClient botClient, ILogger<TgCommandHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<Message> SendNowSicknessPollReportAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendPollAsync(
            chatId: message.Chat.Id,
            question: "Брали ли вы больничный ?",
            isAnonymous: false,
            options: new[]
            {
                "Да",
                "Нет"
            },
            cancellationToken: cancellationToken);
    }

    private async Task<Message> SendSicknessPollReportFirstHalfAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendPollAsync(
            chatId: message.Chat.Id,
            question: "Брали ли вы больничный с 1 по 15 число?",
            isAnonymous: false,
            options: new[]
            {
                "Да",
                "Нет"
            },
            cancellationToken: cancellationToken);
    }

    private async Task<Message> SendSicknessPollReportLastHalfAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendPollAsync(
            chatId: message.Chat.Id,
            question: "Брали ли вы больничный с 16 числа по конец месяца?",
            isAnonymous: false,
            options: new[]
            {
                "Да",
                "Нет"
            },
            cancellationToken: cancellationToken);
    }

    private Message SendSicknessPollReportFirstHalf(Message message, CancellationToken cancellationToken)
    {
        return SendSicknessPollReportFirstHalfAsync(message, cancellationToken).Result;
    }
    
    private Message SendSicknessPollReportLastHalf(Message message, CancellationToken cancellationToken)
    {
        return SendSicknessPollReportLastHalfAsync(message, cancellationToken).Result;
    }

    public Task<Message> StartSendPoll(Message message, CancellationToken cancellationToken)
    {
        var firstJobId = message.Chat.Id;
        var secondJobId = message.Chat.Id + 1;
        
        var dayDiff = 0;
        if (message.Text != null)
        {
            if (message.Text.Split(' ').Length > 1)
            {
                int.TryParse(message.Text.Split(' ')[1], out dayDiff);
                if (dayDiff < 0) dayDiff = 0;
                if (dayDiff > 14) dayDiff = 0;
            }
        }

        _dayDiffs.TryAdd(firstJobId.ToString(), dayDiff);
        _dayDiffs.TryAdd(secondJobId.ToString(), dayDiff);
        
        RecurringJob.AddOrUpdate($"{message.Chat.Id}",
            () => SendSicknessPollReportFirstHalf(message, cancellationToken),
            $"17 11 {FirstDay - _dayDiffs[firstJobId.ToString()]} * *",
            TimeZoneInfo.Local
        );
        
        RecurringJob.AddOrUpdate($"{message.Chat.Id+1}",
            () => SendSicknessPollReportLastHalf(message, cancellationToken),
            $"17 11 {LastDay - _dayDiffs[secondJobId.ToString()]} * *",
            TimeZoneInfo.Local
        );
        
        _logger.LogInformation($"Start job with id -> {firstJobId}");
        _logger.LogInformation($"Start job with id -> {secondJobId}");
        return Task.FromResult(message);
    }

    public Task<Message> StopSendPoll(Message message)
    {
        RecurringJob.RemoveIfExists($"{message.Chat.Id}");
        RecurringJob.RemoveIfExists($"{message.Chat.Id+1}");
        _logger.LogInformation($"Stop job with id -> {message.Chat.Id}");
        _logger.LogInformation($"Stop job with id -> {message.Chat.Id+1}");

        return Task.FromResult(message);
    }

    public Task<Message> UnknownCommand(Message message, CancellationToken cancellationToken)
    {
        return Task.FromResult(message);
    }

    public async Task<Message> SendUsageMessageAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: ProjectConstants.UsageText,
            cancellationToken: cancellationToken);
    }

    public async Task<Message> SendStartMessageAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: ProjectConstants.StartText,
            cancellationToken: cancellationToken);
    }

    public async Task<Message> SendUserIdMessageAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Your id -> {message.From.Id}",
            cancellationToken: cancellationToken);
    }
}