using Cleverence.ThreadsafeServer;

namespace Cleverence.TasksConsoleApp;

class MockCountingClientsPool
{
    public MockCountingClientsPool(int clientsCount)
    {
        for (int i = 0; i < clientsCount; i++)
        {
            new Thread(AccessServer).Start(i);
        }
    }

    private void AccessServer(object id)
    {
        Thread.Sleep((int)id);
        Console.WriteLine($"{DateTime.Now.TimeOfDay}\tПоток {id} пытается отправить данные серверу...");
        ThreadsafeCountingServer.AddToCount(1);
        Console.WriteLine($"{DateTime.Now.TimeOfDay}\tПоток {id} отправил данные.");
        Thread.Sleep((int)id*2);      
        Console.WriteLine($"{DateTime.Now.TimeOfDay}\tПоток {id} запрашивает данные с сервера...");
        var i = ThreadsafeCountingServer.GetCount();
        Console.WriteLine($"{DateTime.Now.TimeOfDay}\tПоток {id} получил ответ: {i}.");
        
    }


}