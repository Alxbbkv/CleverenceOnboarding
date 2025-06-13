namespace Cleverence.TasksConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        TaskOneDemo();
        TaskTwoDemo();
    }

    private static void TaskOneDemo()
    {
        Console.WriteLine("Демонстрация работы по Задаче 1");
        var lines = new string[]
        {
            "aaabbcccdde",
            "bbccaaa",
            "abc",
            "aab",
            "abb",
            "ab",
            "a",
            "",
            "aaaBcc",
            "123"
        };

        var compressor = new StringCompressor.StringCompressor();
        
        Console.WriteLine("{0,15} {1,15} {2,15}", "Исходная", "Сжатая", "Восстановленная");
        foreach (var line in lines)
        {
            string compressed, decompressed;
            try
            {
                compressed = compressor.Compress(line);
            }
            catch (Exception e)
            {
                compressed = e.Message;
            }

            try
            {
                decompressed = compressor.Decompress(compressed);
            }
            catch (Exception e)
            {
                decompressed = e.Message;
            }
            
            Console.WriteLine("{0,15} {1,15} {2,15}", line, compressed, decompressed);
        }
        

    }
    private static void TaskTwoDemo()
    {
        Console.WriteLine("Нажмите Enter для запуска клиент-серверного взаимодействия (Задача 2)");
        Console.ReadLine();
        StartMockClientsForThreadsafeCountingServer();
    }

    static void StartMockClientsForThreadsafeCountingServer()
    {
        var mockClients = new MockCountingClientsPool(1_000);
    }
}