using Microsoft.Data.SqlClient;

namespace MultipleDbContextsTransaction
{
    public interface IConnectionWrapper : IDisposable
    {
        SqlConnection Connection { get; }
    }

    internal class ConnectionWrapper : IConnectionWrapper
    {
        public SqlConnection Connection { get; }

        public ConnectionWrapper(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
        }

        public void Dispose()
        {
            Connection.Dispose();
            Console.WriteLine("Disposed ConnectionWrapper");
        }
    }
}
