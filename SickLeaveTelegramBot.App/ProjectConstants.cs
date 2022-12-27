namespace SickLeaveTelegramBot.App;

public static class ProjectConstants
{
    public const string UsageText = """
Команды для работы с ботом
/start_polling - запустить отправку опроса по таймеру
/start_polling __dayDiff__ - вместо __dayDiff__ указать разницу по дням для опроса , но не более 14 дней. К примеру, /start_polling 4 запустит опрос раньше на 4 дня
/stop_polling - выключает опрос по таймеру
/poll_now - отправляет опрос сейчас
/help - показывает справку о командах
""";

    public const string StartText = """
/start

Привет!
Я бот, который помогает менеджерам команды автоматизировать опросники сотрудников о больничных.

Чтобы узнать, что я умею - просто добавь меня в чат команды и напиши /help
""";
    
}