using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SickLeaveTelegramBot.App.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ILogger<UpdateHandler> _logger;
    private readonly TgCommandHandler _commandHandler;

    public UpdateHandler(ILogger<UpdateHandler> logger, TgCommandHandler commandHandler)
    {
        _logger = logger;
        _commandHandler = commandHandler;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var state = update.Message switch
            {
                { Chat.Type: ChatType.Private } => UpdateResolverAsync(update, cancellationToken),
                { From: not null }  => AdminUpdateResolverAsync(botClient ,update, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update)
            };
            
            await state;
        }
        catch (Exception e)
        {
            _logger.LogError($"{e}");
        }
    }

    private async Task UpdateResolverAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message } => BotOnMessageReceivedAsync(message, cancellationToken),
            { EditedMessage: { } message } => BotOnMessageReceivedAsync(message, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update)
        };
       await handler;
    }

    private async Task AdminUpdateResolverAsync(ITelegramBotClient botClient ,Update update, CancellationToken cancellationToken)
    {
        var currentUserMessage = update.Message.From.Id;
        var userMessage =await botClient.GetChatAdministratorsAsync(update.Message.Chat.Id,cancellationToken);

        if (userMessage.Select(x => x.User.Id).Contains(currentUserMessage))
            await UpdateResolverAsync(update, cancellationToken);
    }

    private async Task BotOnMessageReceivedAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => _commandHandler.SendStartMessageAsync(message, cancellationToken),
            "/start_polling" => _commandHandler.StartSendPoll(message, cancellationToken),
            "/stop_polling" => _commandHandler.StopSendPoll(message),
            "/poll_now" => _commandHandler.SendNowSicknessPollReportAsync(message, cancellationToken),
            "/help" => _commandHandler.SendUsageMessageAsync(message, cancellationToken),
            _ => _commandHandler.UnknownCommand(message, cancellationToken)
            
        };
        var sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
    
    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
