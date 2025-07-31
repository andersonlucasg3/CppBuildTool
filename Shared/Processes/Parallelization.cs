namespace Shared.Processes;

public static class Parallelization
{
    public static void ForEach<T>(T[] InSourceArray, Action<T> Action)
    {
        if (InSourceArray.Length == 0) return;

        ThreadWorker.ExecuteOnExclusiveThread(() =>
        {
            Lock Locker = new();

            int Count = InSourceArray.Length;

            foreach (T Element in InSourceArray)
            {
                ThreadWorker.Execute(() =>
                {
                    Action.Invoke(Element);

                    lock (Locker) Count--;
                });
            }

            bool bRelease = false;
            do
            {
                Thread.Sleep(1);

                lock (Locker)
                {
                    bRelease = Count == 0;
                }
            }
            while (!bRelease);
        });
    }
}