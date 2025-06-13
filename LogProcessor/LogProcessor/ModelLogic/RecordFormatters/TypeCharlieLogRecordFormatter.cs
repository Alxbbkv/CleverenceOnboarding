namespace Cleverence.LogProcessor;

/// <summary>
/// Реализует "Выходной формат" из ТЗ
/// </summary>
public class TypeCharlieLogRecordFormatter : LogRecordFormatter
{
    public string TimeFormatAlter { get; protected set; } = "HH:mm:ss.ffff";
    protected override LogRecord Parse(string record)
    {
        const char separator = '\t';
        const int minFieldsInRecord = 5;
        const int datePositionInRecord = 0;
        const int timePositionInRecord = 1;
        const int levelPositionInRecord = 2;
        const int methodPositionInRecord = 3;        
        const int msgPositionInRecord = 4;


        
        var fields = record.Split(separator);
        
        if (fields.Length < minFieldsInRecord) return null;

        if (!LogLevelTitles.ContainsValue(fields[levelPositionInRecord])) return null;
        var logLevel = LogLevelTitles.FirstOrDefault(x => x.Value == fields[levelPositionInRecord]).Key;
 
        if (!TryParseDate(fields[datePositionInRecord], DateFormat, out var dateOnly)) return null;

        if (!TryParseTime(fields[timePositionInRecord], TimeFormat, out _))
        {
            if (!TryParseTime(fields[timePositionInRecord], TimeFormatAlter, out _))
            {
                return null;
            }
        }

        var time = fields[timePositionInRecord];
        
 
        var msgFields = fields[msgPositionInRecord..^0];
        var msg = string.Join(separator, msgFields);

        
        var result = new LogRecord
        {
            Date = dateOnly,
            TimeString = time,
            LogLevel = logLevel,
            CallerMethodName = fields[methodPositionInRecord],
            Message = msg,
        };

        return result;
    }

    protected override string GetStringView(LogRecord record)
    {
        var date = record.Date.ToString(DateFormat);
        var time = record.TimeString;
        var logLevel = LogLevelTitles[record.LogLevel];
        var result = $"{date}\t{time}\t{logLevel}\t{record.CallerMethodName}\t{record.Message}";
        return result;
    }
}