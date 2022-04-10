[![MIT License](http://img.shields.io/badge/license-MIT-yellow.svg?style=flat)](https://github.com/zhmpng/Common.Cache/blob/trunk/LICENSE)
# Common.Cache
This library was created specifically to simplify and automate the storage of data cached in a json file.

# Examples of using

To implement a cache file instance, the corresponding field in the class follows:
```csharp
public static readonly SerializableCache<string, string> CacheFile = new SerializableCache<string, string>($"{Directory.GetCurrentDirectory()}\\cacheFile.json");
```

It can be given different data types for keys and values.
```csharp
SerializableCache<Guid, long>
SerializableCache<string, int>
SerializableCache<long, YourDto>
//and other options
```

In this use case, the cache file will be initialized along with the class. After that, you can freely use it and are not afraid of losing your melons.
```csharp
CacheFile.AddOrUpdate("", "");
CacheFile.Delete("");
CacheFile.Clear();
CacheFile.Count();
```

