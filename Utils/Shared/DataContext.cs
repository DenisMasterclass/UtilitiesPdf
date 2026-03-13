using Microsoft.Data.SqlClient;

namespace Utils.Shared
{
    public class DataContext : IDisposable
    {
        public SqlConnection Connection { get; }
        public SqlTransaction Transaction { get; set; }
        public DataContext(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
            Connection.Open();
        }
        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
