namespace Cleverence.LogProcessor;




/// <summary>
/// Сущность "Запись лога"
/// </summary>
public class LogRecord
{
    public enum LoggingLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }

    private const string DefaultCallerMethodName = "DEFAULT";
    public DateOnly Date;
    public string TimeString;
    public LoggingLevel LogLevel;
    public string Message;
    
    public TimeOnly Time => TimeOnly.Parse(TimeString);

    private string _callerMethodName;
    public string CallerMethodName
    {
        get => string.IsNullOrEmpty(_callerMethodName) ? DefaultCallerMethodName : _callerMethodName;
        set => _callerMethodName=value;
    }
}