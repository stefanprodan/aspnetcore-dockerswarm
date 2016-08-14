using RethinkDb.Driver.Net;

namespace RethinkDbLogProvider
{
    public interface IRethinkDbConnectionFactory
    {
        Connection CreateConnection();
        void CloseConnection();
        RethinkDbOptions GetOptions();
    }
}