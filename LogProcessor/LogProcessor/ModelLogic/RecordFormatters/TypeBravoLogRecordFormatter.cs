namespace Cleverence.LogProcessor;

/// <summary>
/// Реализует "Формат 2" из ТЗ
/// </summary>
public class TypeBravoLogRecordFormatter : LogRecordFormatter
{
    public TypeBravoLogRecordFormatter()
    {
        DateFormat = "yyyy-MM-dd";
        TimeFormat = "HH:mm:ss.ffff";
    }
    protected override LogRecord Parse(string record)
    {
        const char separator = '|';
        const int minFieldsInRecord = 5;
        const int datetimePositionInRecord = 0;
        const int levelPositionInRecord = 1;
        const int methodPositionInRecord = 3;           
        const int msgPositionInRecord = 4;
        
        var fields = record.Split(separator);
        for (var i = 0; i < fields.Length; i++)
        {
            fields[i] = fields[i].Trim();
        }
        
        if (fields.Length < minFieldsInRecord) return null;

        if (!LogLevelTitles.ContainsValue(fields[levelPositionInRecord])) return null;
        var logLevel = LogLevelTitles.FirstOrDefault(x => x.Value == fields[levelPositionInRecord]).Key;

        var datetime = fields[datetimePositionInRecord].Split(' ');
        if (datetime.Length != 2) return null;
        
        if (!TryParseDate(datetime[0], DateFormat, out var dateOnly)) return null;
        
        if (!TryParseTime(datetime[1], TimeFormat, out _)) return null;
        var time = datetime[1];
        
        var msgFields = fields[msgPositionInRecord..^0];
        var msg = string.Join(separator, msgFields);

        var result = new LogRecord
        {
            Date = dateOnly,
            TimeString = time,
            LogLevel = logLevel,
            Message = msg,
            CallerMethodName = fields[methodPositionInRecord]
        };

        return result;
    }

    protected override string GetStringView(LogRecord record)
    {
        var date = record.Date.ToString(DateFormat);
        var time = record.Time.ToString(TimeFormat);
        var logLevel = LogLevelTitles[record.LogLevel];
        var result = $"{date} {time}|{logLevel}| |{record.CallerMethodName}|{record.Message}";
        return result;
    }
}