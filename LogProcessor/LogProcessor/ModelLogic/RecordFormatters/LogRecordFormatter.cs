using System.Globalization;

namespace Cleverence.LogProcessor;

public abstract class LogRecordFormatter
{
    /// <summary>
    /// Содержит текстовое представление (названия) уровней логирования. Может переопределяться в конкретных реализациях форматтера
    /// </summary>
    public Dictionary<LogRecord.LoggingLevel, string> LogLevelTitles { get; protected set; } = new()
    {
        { LogRecord.LoggingLevel.Debug, "DEBUG" }, { LogRecord.LoggingLevel.Error, "ERROR" }, { LogRecord.LoggingLevel.Info, "INFO" },
        { LogRecord.LoggingLevel.Warning, "WARN" }
    };

    /// <summary>
    /// Формат представления даты. Форматтер считает текстовые строки лога ошибочными, если дата в строке записана не в этом формате.
    /// </summary>
    public string DateFormat { get; protected set; } = "dd-MM-yyyy";
    
    public string TimeFormat { get; protected set; } = "HH:mm:ss.fff";

    public string RecordToString(LogRecord record)
    {
        return record is null ? string.Empty : GetStringView(record);
    }

    /// <summary>
    /// Создает новый объект LogRecord по его строковому представлению в формате, реализуемом данным форматтером. 
    /// </summary>
    /// <param name="s"></param>
    /// <returns>Объект LogRecord, описываемый строкой, ИЛИ null, если строка не соответствует ожидаемому формату</returns>
    public LogRecord StringToRecord(string s)
    {
        return Parse(s);
    }
    /// <summary>
    /// Реализация парсинга строки, описывающей LogRecord, особая для каждого конкретного форматтера
    /// </summary>
    /// <param name="record"></param>
    /// <returns>бъект LogRecord, описываемый строкой, ИЛИ null, если строка не соответствует ожидаемому формату</returns>
    protected abstract LogRecord Parse(string record);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="record"></param>
    /// <returns>Строковое представление объекта LogRecord в конкретном формате</returns>
    protected abstract string GetStringView(LogRecord record);

    protected bool TryParseDate(string input, string requiredFormat, out DateOnly date)
    {
        var result = true;

        try
        {
            date = DateOnly.ParseExact(input, requiredFormat, CultureInfo.InvariantCulture);
        }
        catch (FormatException e)
        {
            date = new DateOnly();
            result = false;
        }

        return result;
    }
    
    protected bool TryParseTime(string input, string requiredFormat, out TimeOnly time)
    {
        var result = true;

        try
        {
            time = TimeOnly.ParseExact(input, requiredFormat, CultureInfo.InvariantCulture);
        }
        catch (FormatException e)
        {
            time = new TimeOnly();
            result = false;
        }

        return result;
    }
    

}