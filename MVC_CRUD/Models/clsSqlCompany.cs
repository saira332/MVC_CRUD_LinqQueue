using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
namespace MVC_CRUD.Models
{
    public class clsSqlCompany
    {
        public static DataTableReader getCompanyListCount()
        {
            DataTable tdt = new DataTable();
            string connection = ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "SELECT Count(s.CompanyId) MyRowCount FROM tblCompany s";
                cmd.Connection = con;
                SqlDataReader r = cmd.ExecuteReader();
                tdt.Load(r);
                r.Close();
                con.Close();
                SqlConnection.ClearPool(con);
            }
            return tdt.CreateDataReader();

        }
        public static DataTableReader getCompanyList(string start, string length, string sorting)
        {
            DataTable tdt = new DataTable();
            string connection = ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                if (string.IsNullOrEmpty(start))
                {
                    start = "0";
                }
                if (string.IsNullOrEmpty(length))
                {
                    length = "0";
                }
                int voffset = (Convert.ToInt32(start) / 10) * Convert.ToInt32(length);
                cmd.CommandText = " select isnull(s.CompanyId,0) as 'CompanyId',isnull(s.CompanyName,'') as 'CompanyName', isnull(s.CountryId,'') as 'CountryId', isnull(c.CountryName, '') as 'CountryName' , isnull(s.CityId,0) as CityId, isnull(a.CityName,'') as CityName from tblCompany s  inner join tblCountry c on s.CountryId = c.CountryId inner join tblCity a on s.CityId = a.CityId " + sorting + " OFFSET " + voffset + " ROWS  FETCH NEXT " + length + " ROWS ONLY";
                cmd.Connection = con;
                SqlDataReader r = cmd.ExecuteReader();
                tdt.Load(r);
                r.Close();
                con.Close();
                SqlConnection.ClearPool(con);
            }

            return tdt.CreateDataReader();
        }
    }
}