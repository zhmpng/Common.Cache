﻿using System;
using System.Threading.Tasks;

namespace Common.Cache
{
    internal class SerialQueue : IDisposable
    {
        object _locker = new object();
        WeakReference<Task> _lastTask;

        public Task Enqueue(Action action)
        {
            return Enqueue<object>(() => {
                action();
                return null;
            });
        }

        public Task<T> Enqueue<T>(Func<T> function)
        {
            lock (_locker)
            {
                Task lastTask = null;
                Task<T> resultTask = null;

                if (_lastTask != null && _lastTask.TryGetTarget(out lastTask))
                {
                    resultTask = lastTask.ContinueWith(_ => function());
                }
                else
                {
                    resultTask = Task.Run(function);
                }

                _lastTask = new WeakReference<Task>(resultTask);
                return resultTask;
            }
        }

        public void Dispose()
        {
            lock (_locker)
            {
                Task lastTask = null;

                if (_lastTask != null && _lastTask.TryGetTarget(out lastTask))
                {
                    lastTask.Wait();
                }
            }

        }
    }
}
