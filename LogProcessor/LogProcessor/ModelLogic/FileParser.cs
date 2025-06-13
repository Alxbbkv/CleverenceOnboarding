using System.Text;

namespace Cleverence.LogProcessor;

public class FileParser
{
    public struct Statistics
    {
        public bool OkDone;
        public int SourceLinesRead;
        public int TargetLinesWritten;
        public int ErroneousLinesWritten;
    }
    
    public Statistics Process(string sourceFileFullName, string targetFileFullName, string errorsFileFullName, Encoding encoding = null)
    {
        var stat = new Statistics() { OkDone = false };
        var targetWriter = new StreamWriter(targetFileFullName);
        StreamWriter errorsWriter = null;
        var recordParser = new RecordParser();
        using (var sourceReader = new StreamReader(sourceFileFullName))
        {
            string? line;
            while ((line = sourceReader.ReadLine()) != null)
            {
                stat.SourceLinesRead++;
                var isLineParsed = recordParser.TryConvert(line, out var formatted);
                if (isLineParsed)
                {
                    targetWriter.WriteLine(formatted);
                    stat.TargetLinesWritten++;
                }
                else
                {
                    errorsWriter ??= new StreamWriter(errorsFileFullName);
                    errorsWriter.WriteLine(line);
                    stat.ErroneousLinesWritten++;
                }
            }
        }

        targetWriter.Close();
        errorsWriter?.Close();

        stat.OkDone = true;

        return stat;
    }
}