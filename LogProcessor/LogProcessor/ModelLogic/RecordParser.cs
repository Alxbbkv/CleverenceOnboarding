namespace Cleverence.LogProcessor;


/// <summary>
/// Извлекает из строк записи логов, применяя для парсинга все определенные в ТЗ форматы. Преобразует строки логов в целевой формат. 
/// </summary>
public class RecordParser
{
    private List<LogRecordFormatter> _formatters;

    public LogRecordFormatter TargetFormatter = new TypeCharlieLogRecordFormatter();

    public RecordParser()
    {
        _formatters =
        [
            new TypeAlphaLogRecordFormatter(),
            new TypeBravoLogRecordFormatter(),
            new TypeCharlieLogRecordFormatter()
        ];
    }

    /// <summary>
    /// Пытается преобразовать строку в объект LogRecord всеми доступными форматтерами.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="record">выход: Объект LogRecord ИЛИ null, если распарсить не получилось</param>
    /// <returns>Получилось ли распарсить.</returns>
    public bool TryParse(string input, out LogRecord record)
    {
        record = null;
        
        foreach (var formatter in _formatters)
        {
            record = formatter.StringToRecord(input);
            if (record is not null) break;
        }

        return (record is not null);
    }

    /// <summary>
    /// Пытается преобразовать текстовую строку-запись лога в строку того формата, который задается в TargetFormatter
    /// </summary>
    /// <param name="input"></param>
    /// <param name="formatted"></param>
    /// <returns></returns>
    public bool TryConvert(string input, out string formatted)
    {
        var ok = TryParse(input, out var record);
        formatted =  ok ? TargetFormatter.RecordToString(record) : input;
        return ok;
    }

    /// <summary>
    /// Получает список строк и пытается распарсить каждую из них, применяя все известные форматтеры. Строки, которые удалось распарсить, преобразуются в целевой формат (определен в TargetFormatter) и возвращаются как результат метода. Строки, которые не получилось распознать, возвращаются в списке erroneousList
    /// </summary>
    /// <param name="sourceStrings"></param>
    /// <param name="erroneousStrings"></param>
    /// <returns></returns>
    public List<string> ConvertMultipleStrings(List<string> sourceStrings, out List<string> erroneousStrings)
    {
        var result = new List<string>();
        erroneousStrings = new List<string>();
        
        foreach (var line in sourceStrings)
        {
            var lineOk = TryConvert(line, out var formatted);
            if (lineOk) result.Add(formatted);
            else erroneousStrings.Add(line);
        }

        return result;
    }
}