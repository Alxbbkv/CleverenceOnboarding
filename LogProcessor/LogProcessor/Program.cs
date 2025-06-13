namespace Cleverence.LogProcessor;

class Program
{
    static int Main(string[] args)
    {
       
        
        if (!CheckCommandlineArgumentsCount(args)) return -1;

        var filenames = TryGetFilenamesFromArgs(args);
        if (filenames is null) return -1;
        
        Console.WriteLine("log2log is starting...");
        Console.WriteLine($"Source file: {filenames.SourceFileName}");
        Console.WriteLine($"Target file: {filenames.TargetFileName}");
        Console.WriteLine($"Errors report file: {filenames.ErrorsFileName}");
        Console.WriteLine("Please wait...");
        
        var fileParser = new FileParser();
        FileParser.Statistics stat;
        try
        {
            stat = fileParser.Process(filenames.SourceFileName, filenames.TargetFileName, filenames.ErrorsFileName);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {e.Message}");
            return -1;
        }

        Console.WriteLine($"Done.\n\rLines read: {stat.SourceLinesRead}\n\rLines recognized: {stat.TargetLinesWritten}\n\r Lines not recognized: {stat.ErroneousLinesWritten}");
        
        return 0;
    }

    private static FilesRequirementsChecker TryGetFilenamesFromArgs(string[] args)
    {
        var sourceFile = args[0];
        var targetFile = args[1];
        var errorsFile = (args.Length == 3) ? args[2] : string.Empty;

        FilesRequirementsChecker filesChecker;
        try
        {
            filesChecker = new FilesRequirementsChecker(sourceFile, targetFile, errorsFile);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error:");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            return null;
        }

        return filesChecker;
    }
    
    private static bool CheckCommandlineArgumentsCount(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return false;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("Error: Required command-line argument is missing!\n\rRun the utility with no arguments to see manual.");
            return false;
        }

        if (args.Length > 3)
        {
            Console.WriteLine("Error: Too many command-line arguments!\n\rRun the utility with no arguments to see manual.");
            return false;
        }

        return true;
    }

    static void PrintHelp()
    {
        var text =  """
                    
                        The log2log utility converts CleverenceSoft log-file of obsolete formats into log-file of brand new CleverenceSoft format.
                        Sucessfully parsed lines from SourceFile will be written into TargetFile, and not recognized ones comes to ErrorsReportFile.
                        
                    Usage:      log2log   sourcefile  targetfile  [errorsreportfile]
                        
                    where:  sourcefile - name (incl. extention) of existing log-file to be parsed.
                            targetfile - name (incl. ext) of target log-file to be created. 
                            errorsreportfile - name (incl. ext) of file for not recognized lines. Default name is: problems.txt
                                
                              note 1: if sourcefile name does not contain path, the file is considered to be located in current directory.
                              note 2: if targetfile / errorsreportfile name does not contain path, it will be created at the same place as sourcefile.
                              note 3: if file to be written already exists, it will be overwritten.
                          
                    """;
        Console.WriteLine(text);
    }
}