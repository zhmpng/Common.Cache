using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Cache
{
    public class SerializablePool<T> : IDisposable
    {
        private readonly string FilePath;
        public List<T> List { get { return _list.ToList(); } set { _list = value; SerializeCacheToFileAsync(); } }
        private List<T> _list = new List<T>();
        private readonly SerialQueue queue  = new SerialQueue();

        object _locker = new object();

        public delegate void StringEvent(string status);

        public event StringEvent OnLog;

        public SerializablePool(string filePath)
        {
            List = new List<T>();
            FilePath = filePath;

            if (File.Exists(FilePath))
            {
                try
                {
                    var dataList =
                        JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(FilePath));
                    _list =
                        new List<T>(
                            dataList.ToList());
                }
                catch (Exception)
                {
                    var bugFilePath = filePath.Remove(filePath.LastIndexOf(".json"), 5) + $"-{DateTime.Now.ToString("dd-MM-yyyy-HH-mm")}-bug.json";
                    File.Move(FilePath, bugFilePath);
                }
            }

        }

        public void Dispose()
        {
            queue?.Dispose();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public bool Remove(T item)
        {
            if (Contains(item))
            {
                _list.Remove(item);
                SerializeCacheToFileAsync();
                return true;
            }
            return false;
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _list.Add(item);
            SerializeCacheToFileAsync();
        }

        public T Get(Predicate<T> predicate)
        {
            return _list.FirstOrDefault(i => predicate(i));
        }

        public void Clear()
        {
            _list.Clear();
            SerializeCacheToFileAsync();
        }

        private void SerializeCacheToFileAsync()
        {
            lock (_locker)
            {
                var copiedList = new List<T>(_list);
                queue.Enqueue(() =>
                {
                    OnLog?.Invoke($"Begining to write pool to file {FilePath}");
                    try
                    {
                        File.WriteAllText(FilePath, JsonConvert.SerializeObject(copiedList));
                    }
                    catch (Exception ex)
                    {
                        OnLog?.Invoke($"Got exception while trying to write to pool: {ex.Message}");
                    }
                    OnLog?.Invoke($"Ending to write pool to file {FilePath}");                   

                });
            }

        }
    }
}
