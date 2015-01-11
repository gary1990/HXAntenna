using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HxAntenna.Controllers.Common
{
    public class CommonController : Controller
    {
        public static DataTable GetDateTable(string sql, SqlParameter[] param)
        {
            using (SqlConnection MyConn = new SqlConnection(ConfigurationManager.ConnectionStrings["AntennaDbContext"].ConnectionString))
            {
                MyConn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, MyConn))
                {
                    cmd.Parameters.AddRange(param);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }
	}
}