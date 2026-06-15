using Serilog;

namespace Canopy.Dependency;

public static class DependencyContainer
{
    private static readonly Dictionary<Type, object> cache = new();

    /// <summary>
    /// Adds the instance to a type-indexed dependency cache
    /// </summary>
    /// <param name="instance">Instance to add to the cache</param>
    /// <typeparam name="T">Type of the instance</typeparam>
    public static void AddSingleton<T>(T instance)
    {
        var type = typeof(T);
        cache[type] = instance!;
        Log.Verbose("Added {name} to dependency cache", type.Name);
    }

    /// <summary>
    /// Returns an instance of a specified type if present in cache
    /// </summary>
    /// <param name="type">Type of the dependency</param>
    /// <returns>Dependency instance as an 'object'</returns>
    /// <exception cref="ArgumentException">Dependency type isn't found in the dependency cache</exception>
    public static object Get(Type type)
    {
        cache.TryGetValue(type, out var value);
        if (value != null)
            return value;

        var message = $"Dependency Container does not contain {type}";
        Log.Error(message);
        throw new ArgumentException(message, nameof(type));
    }

    /// <summary>
    /// Returns an instance of a specified type if present in cache
    /// </summary>
    /// <typeparam name="T">Type of the dependency</typeparam>
    /// <returns>Dependency instance</returns>
    /// <exception cref="ArgumentException">Dependency type isn't found in the dependency cache</exception>
    public static T Get<T>() where T : class
    {
        return (Get(typeof(T)) as T)!;
    }
}
