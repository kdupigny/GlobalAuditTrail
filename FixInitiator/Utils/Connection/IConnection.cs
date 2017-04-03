namespace GATUtils.Connection
{
    public interface IConnection
    {
        bool Connect();
        bool Disconnect();

        bool IsAlive { get; }
    }
}
