
using System.Data.SqlClient;

namespace InventoryManagement.DAL
{
    public static class Database
    {
        public static string ConnectionString =
            @"Server=.;Database=InventoryDB;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
