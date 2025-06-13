using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
namespace MVCTemplate.Models
{
    public class db
    {
        SqlConnection con = new SqlConnection("Data Source=DESKTOP-F7N31EV;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False"); //from server explorer
        public DataTable Getrecord()
        {
            SqlCommand com = new SqlCommand("select * from MVCTemplateDb.dbo.Packages", con); //from server explorer
            SqlDataAdapter da = new SqlDataAdapter(com);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
        //tutorial https://www.youtube.com/watch?v=yW4Yp_J_m6k
        //because of the nature of the query its usability is lower
    }
}
