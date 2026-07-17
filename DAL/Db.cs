using System.Configuration;
using System.Data.SqlClient;

namespace CRMSistema.DAL
{
    public class Db
    {
        public static SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["EcoSalesCRMContext"].ConnectionString;
            return new SqlConnection(connectionString);
        }
    }
}
