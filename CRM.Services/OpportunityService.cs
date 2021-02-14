using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CRM.Services
{
    public class OpportunityService : IOpportunityService
    {

        public DataTable OpportunityActivityReport()
        {
            string spName = ConfigurationManager.AppSettings["OpportunityActivityReportSP"];

            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(spName, conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable OpportunityReport()
        {
             DataTable dt = new DataTable();
            string spName = ConfigurationManager.AppSettings["OpportunityReportSP"];
            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(spName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(dt);

                }
            }
            return dt;

        }

        public DataTable OpportunityReportGoods()
        {
            DataTable dt = new DataTable();
            string spName = ConfigurationManager.AppSettings["OpportunityReportGoodsSP"];
            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(spName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(dt);

                }
            }
            return dt;

        }

        public DataTable OpportunityStageProgress()
        {
            DataTable dt = new DataTable();
            string spName = ConfigurationManager.AppSettings["OpportunityReportSP"];
            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(spName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }

            return dt;
        }
    }
}
