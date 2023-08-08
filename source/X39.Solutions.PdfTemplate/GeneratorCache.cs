using X39.Util.Threading;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Cache for the generator.
/// </summary>
/// <remarks>
/// This class is not thread safe. Make sure to implement locking if you want to use it in a multi-threaded environment.
/// </remarks>
[Obsolete("Obsolete, unused code", error: true)]
public sealed class GeneratorCache : IAsyncDisposable, IDisposable
{
    private readonly IServiceProvider                             _serviceProvider;
    private readonly ReaderWriterLockSlim                         _readerWriterLockSlim = new();
    private readonly Dictionary<(Type type, string key), object?> _data                 = new();

    public GeneratorCache(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Add<TCacheSource, TValue>(string key, TValue value)
    {
        _readerWriterLockSlim.WriteLocked(
            () => { _data.Add((typeof(TCacheSource), key), value); });
    }

    public bool TryGet<TCacheSource, TValue>(string key, out TValue value)
    {
        TValue tmpValue = default!;
        var flag = _readerWriterLockSlim.ReadLocked(
            () =>
            {
                if (!_data.TryGetValue((typeof(TCacheSource), key), out var tmp) || tmp is not TValue val)
                    return false;
                tmpValue = val;
                return true;
            });
        value = tmpValue;
        return flag;
    }

    public TValue Get<TCacheSource, TValue>(string key)
    {
        if (!TryGet<TCacheSource, TValue>(key, out var value))
            throw new KeyNotFoundException(
                $"The key '{key}' was not found in the cache for {typeof(TCacheSource).FullName()}.");
        return value;
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public async Task ClearAsync()
    {
        foreach (var (_, value) in _data)
        {
            switch (value)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _data.Clear();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await ClearAsync();
        _readerWriterLockSlim.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var (_, value) in _data)
        {
            switch (value)
            {
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _data.Clear();
        _readerWriterLockSlim.Dispose();
    }
}