namespace ProObjLogLite;

internal sealed class FileLock : IDisposable
{
    private readonly FileStream _stream;

    private FileLock(FileStream stream)
    {
        _stream = stream;
    }

    public static FileLock Acquire(string lockFilePath, int maxAttempts = 100, int retryDelayMs = 40)
    {
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var stream = new FileStream(lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return new FileLock(stream);
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(retryDelayMs);
            }
        }

        throw new IOException($"Could not acquire lock '{lockFilePath}' after {maxAttempts} attempts.");
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
