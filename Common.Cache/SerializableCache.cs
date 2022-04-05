using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Cache
{
    public class SerializableCache<K,V> : IDisposable
    {
        private readonly string FilePath;
        public List<V> Items { get { return _dictionary.Values.ToList(); } }
        public List<K> Keys { get { return _dictionary.Keys.ToList(); } }
        public IDictionary<K, V> KeyValuePair { get { return _dictionary; } }
        private readonly IDictionary<K, V> _dictionary = new ConcurrentDictionary<K, V>();
        private readonly SerialQueue queue = new SerialQueue();

        object _locker = new object();

        public delegate void StringEvent(string status);

        public event StringEvent OnLog;    


        public SerializableCache(string filePath)
        {
            FilePath = filePath;

            if (File.Exists(FilePath))
            {
                try
                {
                    var dataDictionary =
                        JsonConvert.DeserializeObject<Dictionary<K, V>>(File.ReadAllText(FilePath));
                    _dictionary =
                        new Dictionary<K, V>(
                            dataDictionary.ToDictionary(t => t.Key, t => t.Value));
                }
                catch (Exception)
                {
                    var bugFilePath = filePath.Remove(filePath.LastIndexOf(".json"), 5) + $"-{DateTime.Now.ToString("dd-MM-yyyy-HH-mm")}-bug.json";
                    File.Move(FilePath, bugFilePath);
                }
            }
            else
            {
                _dictionary = new Dictionary<K, V>();
                File.Create(FilePath);
                SerializeCacheToFileAsync();
            }

        }

        public void Dispose()
        {
            queue?.Dispose();
        }

        public bool Contains(K id)
        {
            return _dictionary.ContainsKey(id);
        }

        public void Delete(K id)
        {
            if (Contains(id))
            {
                _dictionary.Remove(id);
                SerializeCacheToFileAsync();
            }
        }

        private void Add(K id, V data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _dictionary.Add(id, data);
            SerializeCacheToFileAsync();
        }

        private void Update(K id, V data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _dictionary[id] = data;
            SerializeCacheToFileAsync();
        }

        public void AddOrUpdate(K id, V data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (Contains(id)) Update(id, data);
            else Add(id, data);
        }

        public V GetById(K id)
        {
            if (!Contains(id))
            {
                throw new ArgumentException($"{nameof(id)} with value {id} not exists in cache.");
            }
            return _dictionary[id];
        }

        public bool TryGetById(K id, out V data)
        {
            return _dictionary.TryGetValue(id, out data);
        }

        public void Clear()
        {
            _dictionary.Clear();
            SerializeCacheToFileAsync();
        }

        private void SerializeCacheToFileAsync()
        {
            lock (_locker)
            {
                var copiedDictionary = new ConcurrentDictionary<K, V>(_dictionary);
                queue.Enqueue(() =>
                {
                    OnLog?.Invoke($"Begining to write cache to file {FilePath}");
                    try
                    {
                        File.WriteAllText(FilePath, JsonConvert.SerializeObject(copiedDictionary));
                    }
                    catch (Exception ex)
                    {
                        OnLog?.Invoke($"Got exception while trying to write to cache: {ex.Message}");
                    }
                    OnLog?.Invoke($"Ending to write cache to file {FilePath}");
                });
            }

        }
    }
}
