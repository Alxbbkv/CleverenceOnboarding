namespace Cleverence.LogProcessor;

public class OptionalFlagsChecker
{
    public OptionalFlagsChecker(string[] options)
    {
        
    }
}

public class FilesRequirementsChecker
{
    public string SourceFileName { get; private set; }
    public string TargetFileName { get; private set; }
    public string ErrorsFileName { get; private set; }
    public string SourceDirectory { get; private set; }
    
    public FilesRequirementsChecker(string sourceFileName, string targetFileName, string errorsFileName)
    {
        CheckSourceFile(sourceFileName);
        CheckTargetFile(targetFileName);
        CheckErrorsFile(errorsFileName);
    }
    private void CheckSourceFile(string sourceFileName)
    {
        try
        {
            SourceFileName = Path.GetFullPath(sourceFileName);
            SourceDirectory = Path.GetDirectoryName(SourceFileName) ?? string.Empty;
        }
        catch (Exception e)
        {
            throw new Exception("Error in source file name. See inner exception.", e);
        }

        if (!File.Exists(SourceFileName)) throw new FileNotFoundException($"Source file not found: {SourceFileName}.");
    }
    
    private const string DefaultErrorsFileName = "problems.txt";
    private void CheckErrorsFile(string errorsFileName)
    {
        if (string.IsNullOrEmpty(errorsFileName)) errorsFileName = DefaultErrorsFileName;
        try
        {
            string? errorsFileDir = Path.GetDirectoryName(errorsFileName);
            if (errorsFileDir == string.Empty) errorsFileDir = SourceDirectory;
            ErrorsFileName = Path.Combine(errorsFileDir, Path.GetFileName(errorsFileName));
        }
        catch (Exception e)
        {
            throw new Exception($"A problem with errors-report file name: {ErrorsFileName}. See inner exception.", e);
        }
    }


    private void CheckTargetFile(string targetFileName)
    {
        try
        {
            string? targetFileDir = Path.GetDirectoryName(targetFileName);
            if (targetFileDir == string.Empty) targetFileDir = SourceDirectory;
            TargetFileName = Path.Combine(targetFileDir, Path.GetFileName(targetFileName));
        }
        catch (Exception e)
        {
            throw new Exception($"A problem with target file name: {TargetFileName}. See inner exception.", e);
        }
    }


}