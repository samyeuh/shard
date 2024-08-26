namespace Shard.Shared.Core;

public interface IShardTimer: IAsyncDisposable, IDisposable
{
    bool Change(int dueTime, int period);
    bool Change(long dueTime, long period);
    bool Change(TimeSpan dueTime, TimeSpan period);
    bool Change(uint dueTime, uint period);
}