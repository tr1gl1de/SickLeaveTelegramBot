﻿using Hangfire;
using Telegram.Bot;
using Telegram.Bot.Types;

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

    public async Task<Message> SendSicknessPollReportAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendPollAsync(
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

    public Message SendCurrentTime(Message message, CancellationToken cancellationToken, int jobNum = 0)
    {
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Job num -> {jobNum} [{DateTime.Now}]",
            cancellationToken: cancellationToken).Result;
    }
    
    public Message SendSicknessPollReport(Message message, CancellationToken cancellationToken)
    {
        return SendSicknessPollReportAsync(message, cancellationToken).Result;
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
                if (dayDiff > 14) dayDiff = 0;
            }
        }

        _dayDiffs.TryAdd(firstJobId.ToString(), dayDiff);
        _dayDiffs.TryAdd(secondJobId.ToString(), dayDiff);
        
        RecurringJob.AddOrUpdate($"{message.Chat.Id}",
            () => SendSicknessPollReport(message, cancellationToken),
            $"17 11 {FirstDay - _dayDiffs[firstJobId.ToString()]} * *",
            TimeZoneInfo.Local
        );
        RecurringJob.AddOrUpdate($"{message.Chat.Id+1}",
            () => SendSicknessPollReport(message, cancellationToken),
            $"17 11 {LastDay - _dayDiffs[firstJobId.ToString()]} * *",
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
        const string usage = """
            Команды для работы с ботом
            /start_polling - запустить отправку опроса по таймеру
            /start_polling dayDiff - вместо dayDiff указать разницу по дням для опроса , но неболее 14 дней. К примеру /start_polling 4 запустит порос раньше на 4 дня
            /stop_polling - выключает опрос по таймеру
            /poll_now - отправляет опрос сейчас
            /usage - показывает справку о командах
            """;
        
        _logger.LogInformation($"Send message with id {message.MessageId}");
        return await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: usage,
            cancellationToken: cancellationToken);
    }
}