namespace Shared.Processes;

class ThreadInfo(Thread InThread)
{
    private readonly Lock _lock = new();
    private int _count = 0;

    public readonly Thread Thread = InThread;

    ~ThreadInfo()
    {
        Release();
    }

    public void Wait()
    {
        lock (_lock)
        {
            _count += 1;
        }

        int Count;
        do
        {
            Thread.Sleep(1);

            lock (_lock)
            {
                Count = _count;
            }
        }
        while (Count > 0);
    }

    public void Release()
    {
        lock (_lock)
        {
            _count = Math.Max(0, _count - 1);
        }
    }
}

interface IActionInfo
{
    public void Invoke();
}

class ActionInfo : IActionInfo
{
    public required Action Action;

    public void Invoke()
    {
        Action.Invoke();
    }
}

class ActionInfo<T> : IActionInfo
{
    public required Action<T> Action;
    public required T Object;

    public void Invoke()
    {
        Action.Invoke(Object);
    }
}

public static class ThreadWorker
{
    private static readonly Lock _threadLock = new();
    private static readonly Thread _mainWorkerThread = new(ThreadWorkerRunner);

    private static readonly Queue<ThreadInfo> _availableThreads = [];
    private static readonly Dictionary<Thread, ThreadInfo> _threadInfoMap = [];

    private static readonly Queue<IActionInfo> _actionQueue = [];

    private static bool _shouldKeepRunning = true;
    private static int _preallocatedThreadCount = 1;

    private static bool _bIsSingleThreaded;

    public static void SingleThreaded()
    {
        _bIsSingleThreaded = true;
    }

    public static void PreallocateThreads(int InThreadCount)
    {
        _preallocatedThreadCount = Math.Max(1, InThreadCount);

        _mainWorkerThread.Name = "MainThreadWorker";
        _mainWorkerThread.Start();

        int Count = Math.Max(1, InThreadCount);
        for (int Index = 0; Index < Count; Index++)
        {
            CreateNewWorkerThread();
        }
    }

    public static void Terminate()
    {
        if (_bIsSingleThreaded)
        {
            return;
        }

        ThreadInfo[] ThreadInfos;
        lock (_threadLock) 
        {
            _shouldKeepRunning = false;

            ThreadInfos = [.. _threadInfoMap.Values];
        }

        foreach (ThreadInfo ThreadInfo in ThreadInfos)
        {
            ThreadInfo.Release();
            ThreadInfo.Thread.Join();
        }

        _mainWorkerThread.Join();
    }

    public static void Execute(Action Action)
    {
        if (_bIsSingleThreaded)
        {
            Action.Invoke();
            return;
        }

        lock (_threadLock)
        {
            _actionQueue.Enqueue(new ActionInfo()
            {
                Action = Action
            });
        }
    }

    public static void Execute<T>(Action<T> InAction, T InObject)
    {
        if (_bIsSingleThreaded)
        {
            InAction.Invoke(InObject);
            return;
        }

        lock (_threadLock)
        {
            _actionQueue.Enqueue(new ActionInfo<T>()
            {
                Action = InAction,
                Object = InObject
            });
        }
    }

    public static void ExecuteOnExclusiveThread(Action InAction)
    {
        if (_bIsSingleThreaded)
        {
            InAction.Invoke();
            return;
        }
        
        Thread ExclusiveThread = new(InAction.Invoke)
        {
            Name = "ThreadWorkerOnce",
            IsBackground = true,
        };
        ExclusiveThread.Start();
        ExclusiveThread.Join();
    }

    private static void CreateNewWorkerThread()
    {
        lock (_threadLock)
        {
            ThreadInfo NewThreadInfo = new(new(ThreadRunner)
            {
                Name = "ThreadWorker",
                IsBackground = true,
            });
            _availableThreads.Enqueue(NewThreadInfo);
            _threadInfoMap.Add(NewThreadInfo.Thread, NewThreadInfo);

            NewThreadInfo.Thread.Start();
        }
    }

    private static void ThreadWorkerRunner()
    {
        bool bShouldKeepRunning;
        do
        {
            Thread.Sleep(1);

            bool bReleasedThread = false;

            lock (_threadLock)
            {
                if (_actionQueue.Count > 0 && _availableThreads.TryDequeue(out ThreadInfo? ThreadInfo))
                {
                    bReleasedThread = true;
                    ThreadInfo.Release();
                }

                bShouldKeepRunning = _shouldKeepRunning;
            }

            if (!bReleasedThread && _preallocatedThreadCount <= _actionQueue.Count)
            {
                // if no threads are available, just execute it
                ExecuteAction(false);
            }
        }
        while (bShouldKeepRunning);
    }

    private static void ThreadRunner()
    {
        bool bShouldKeepRunning = true;
        do
        {
            if (!_threadInfoMap.TryGetValue(Thread.CurrentThread, out ThreadInfo? ThreadInfo))
            {
                Thread.Sleep(1);

                continue;
            }

            ThreadInfo.Wait();

            ExecuteAction();

            lock (_threadLock)
            {
                bShouldKeepRunning = _shouldKeepRunning;

                if (_shouldKeepRunning)
                {
                    _availableThreads.Enqueue(ThreadInfo);
                }
            }
        }
        while (bShouldKeepRunning);
    }

    private static void ExecuteAction(bool bShouldLock = true)
    {
        IActionInfo? ActionInfo;
        if (bShouldLock) lock (_threadLock) _actionQueue.TryDequeue(out ActionInfo);
        else _actionQueue.TryDequeue(out ActionInfo);
        ActionInfo?.Invoke();
    }
}