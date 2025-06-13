namespace Cleverence.ThreadsafeServer;

/// <summary>
/// Решение задачи 2: Реализует "сервер" с методами чтения и записи числа.
/// </summary>
public static class ThreadsafeCountingServer
{
    private static int _count;
    private static object _countWriteLocker = new();
    private static ManualResetEvent _countNotBeingWritten = new(true);
    
    public static int GetCount()
    {
        _countNotBeingWritten.WaitOne();
        return _count;
    }

    public static void AddToCount(int value)
    {
        lock (_countWriteLocker)
        {
            _countNotBeingWritten.Reset();
            try
            {
                _count += value;
            }
            catch (Exception e)
            {
                throw new Exception("Something went wrong. See inner exception.", e);
            }
            _countNotBeingWritten.Set();
        }
    }
    
}