using XInputium;

namespace ControllerToMouseMapper.Input;

/// <summary>
/// Implements a thread safe input loop.
/// </summary>
/// <remarks>
/// <see cref="InputLoop"/> iterates at a specified frequency and calls
/// a provided callback <see cref="Action"/> on every iteration. This
/// callback is called in a thread safe manner -- it is never called
/// concurrently. However, it may be called on different threads each
/// time. The callback is run in a thread from the thread pool.
/// </remarks>
public sealed class InputLoop : IDisposable, IAsyncDisposable
{


    private readonly Action _onUpdate;
    private readonly InputLoopWatch _watch;
    private readonly Timer _timer;
    private bool _disposed, _disposing;
    private int _isUpdating;


    /// <summary>
    /// Initializes a new instance of the <see cref="InputLoop"/> class,
    /// that uses the specified callback for iterations and the specified
    /// number of iterations per second.
    /// </summary>
    /// <param name="onUpdate"><see cref="Action"/> that is invoked
    /// on every iteration of the input loop.</param>
    /// <param name="frequency">Optional. Number of desired iterations
    /// per second. The default is 60.</param>
    /// <param name="watch">Optional. The <see cref="InputLoopWatch"/> instance
    /// used to measure time, or <see langword="null"/> to use the default
    /// implementation. The default value is <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="onUpdate"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="frequency"/> is equal to or less than 0.</exception>
    public InputLoop(Action onUpdate, int frequency = 60, InputLoopWatch? watch = null)
    {
        ArgumentNullException.ThrowIfNull(onUpdate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequency);

        _onUpdate = onUpdate;
        _watch = watch ?? InputLoopWatch.GetDefault();
        _timer = new(OnTick, null, 0, 1000 / frequency);
    }


    /// <summary>
    /// Gets the time elapsed between the two most recent update operations.
    /// This allows you to measure the most recent iteration time.
    /// </summary>
    public TimeSpan IterationTime { get; private set; }


    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed || _disposing)
            return;
        _disposing = true;

        _timer.Dispose();

        _disposed = true;
        _disposing = false;
    }


    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposing = true;

        await _timer.DisposeAsync();

        _disposed = true;
        _disposing = false;
    }


    private void OnTick(object? state)
    {
        if (Interlocked.Exchange(ref _isUpdating, 1) == 0)
        {
            try
            {
                IterationTime = _watch.GetTime();
                _onUpdate();
            }
            finally
            {
                Interlocked.Exchange(ref _isUpdating, 0);
            }
        }

    }


}
