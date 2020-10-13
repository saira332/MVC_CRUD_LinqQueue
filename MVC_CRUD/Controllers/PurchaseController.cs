using MVC_CRUD.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;

namespace MVC_CRUD.Controllers
{
    public class PurchaseController : Controller
    {
        TestDbEntities db;
        public PurchaseController()
        {
            db = new TestDbEntities();
        }
        // GET: Country
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetAllCountries()
        {
            //if (User != null)
            //{


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                    + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //var HallName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            //string whereCondition = "";
            string sorting = "";
            if (!(string.IsNullOrEmpty(sortColumn) && !(string.IsNullOrEmpty(sortColumnDir))))
            {
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    sorting = " Order by " + sortColumn + " " + sortColumnDir + "";
                }
            }
            else
            {
                sorting = " Order by s.CountryId asc";
            }
            //if (!(string.IsNullOrEmpty(HallName)))
            //{
            //    whereCondition = " LOWER(s.HallName) like ('%" + HallName + "%')";
            //}
            //else
            //{
            //    whereCondition = " LOWER(s.HallName) like ('%%')";
            //}
            List<clsCountry> listsub = new List<clsCountry>();
            DataTableReader dtr = clsSqlCountry.getCountryListCount();
            while (dtr.Read())
            {
                recordsTotal = Convert.ToInt32(dtr["MyRowCount"]);
            }
            DataTableReader dt = clsSqlCountry.getCountryList(start, length, sorting);
            //     int i = 0;
            while (dt.Read())
            {
                listsub.Add(new clsCountry()
                {
                    CountryId = Convert.ToInt32(dt["CountryId"]),
                    CountryName = dt["CountryName"].ToString()

                });
            }

            var data = listsub.ToList();


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddUpdateCountry(int id = 0)
        {
            clsCountry country = new clsCountry();
            if (id > 0)
            {
                country = (from c in db.tblCountries
                           where c.CountryId == id
                           select new clsCountry
                           {
                               CountryId = c.CountryId,
                               CountryName = c.CountryName
                           }).FirstOrDefault();

            }
            else
            {
                country = new clsCountry
                {
                    CountryId = 0,
                    CountryName = ""
                };
            }

            return PartialView(country);
        }
        [HttpPost]
        public ActionResult AddUpdateCountry(clsCountry country)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                if (country.CountryId > 0)
                {
                    insertUpdateStatus = "Update";

                }
                else
                {
                    insertUpdateStatus = "Save";

                }
                returnId = InsertUpdateCountryDb(country, insertUpdateStatus);
                if (returnId == "Success")
                {
                    status = true;
                    message = "User Type Successfully Updated";
                }
                else
                {
                    status = false;
                    message = returnId;
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message.ToString();
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }

        private string InsertUpdateCountryDb(clsCountry st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCountry", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@CountryId", SqlDbType.Int).Value = st.CountryId;
                        cmd.Parameters.Add("@CountryName", SqlDbType.NVarChar).Value = st.CountryName;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.NVarChar, 300).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        returnId = cmd.Parameters["@CheckReturn"].Value.ToString();
                        cmd.Dispose();
                    }
                    con.Close();
                    con.Dispose();
                }
                catch (Exception ex)
                {
                    returnId = ex.Message.ToString();
                }
            }
            return returnId;
        }
        [HttpPost]
        public ActionResult DeleteCountry(int id)
        {
            string message = "";
            bool status = false;

            clsCountry st = new clsCountry();
            st.CountryId = id;
            string returnId = InsertUpdateCountryDb(st, "Delete");
            if (returnId == "Success")
            {
                ModelState.Clear();
                status = true;
                message = "User Type Successfully Deleted";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = returnId;
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
    }
}