namespace Cleverence.LogProcessor;
/// <summary>
/// Реализует "Формат 1" из ТЗ
/// </summary>
public class TypeAlphaLogRecordFormatter : LogRecordFormatter
{
    public TypeAlphaLogRecordFormatter()
    {
        LogLevelTitles[LogRecord.LoggingLevel.Info] = "INFORMATION";
        LogLevelTitles[LogRecord.LoggingLevel.Warning] = "WARNING";
        DateFormat = "dd.MM.yyyy";
        TimeFormat = "HH:mm:ss.fff";
    }
    protected override LogRecord Parse(string record)
    {
        const char separator = ' ';
        const int minFieldsInRecord = 4;
        const int datePositionInRecord = 0;
        const int timePositionInRecord = 1;
        const int levelPositionInRecord = 2;
        const int msgPositionInRecord = 3;


        
        var fields = record.Split(separator);
        
        if (fields.Length < minFieldsInRecord) return null;

        if (!LogLevelTitles.ContainsValue(fields[levelPositionInRecord])) return null;
        var logLevel = LogLevelTitles.FirstOrDefault(x => x.Value == fields[levelPositionInRecord]).Key;

        if (!TryParseDate(fields[datePositionInRecord], DateFormat, out var dateOnly)) return null;
        
        if (!TryParseTime(fields[timePositionInRecord], TimeFormat, out _)) return null;
        var time = fields[timePositionInRecord];
        
        var msgFields = fields[msgPositionInRecord..^0];
        var msg = string.Join(separator, msgFields);

        var result = new LogRecord
        {
            Date = dateOnly,
            TimeString = time,
            LogLevel = logLevel,
            Message = msg,
        };

        return result;
    }

    protected override string GetStringView(LogRecord record)
    {
        var date = record.Date.ToString(DateFormat);
        var time = record.Time.ToString(TimeFormat);
        var logLevel = LogLevelTitles[record.LogLevel];
        var result = $"{date} {time} {logLevel} {record.Message}";
        return result;
    }
}