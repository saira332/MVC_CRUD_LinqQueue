using MVC_CRUD.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;

namespace MVC_CRUD.Controllers
{
    public class HomeController : Controller
    {
        TestDbEntities db;
        public HomeController()
        {
            db = new TestDbEntities();
        }

        // GET: City
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetAllCities()
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
            List<clsCity> listsub = new List<clsCity>();
            DataTableReader dtr = clsSqlCity.getCityListCount();
            while (dtr.Read())
            {
                recordsTotal = Convert.ToInt32(dtr["MyRowCount"]);
            }
            DataTableReader dt = clsSqlCity.getCityList(start, length, sorting);
            //     int i = 0;
            while (dt.Read())
            {
                listsub.Add(new clsCity()
                {
                    CityId = Convert.ToInt32(dt["CityId"]),
                    CityName = dt["CityName"].ToString(),
                    CountryId = Convert.ToInt32(dt["CountryId"]),
                    CountryName = dt["CountryName"].ToString()

                });
            }

            var data = listsub.ToList();


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult AddUpdateCity(int id=0)
        {
            clsCity city = new clsCity();
            if (id > 0)
            {
                city = (from c in db.tblCities
                        where c.CityId == id
                        select new clsCity
                        {
                            CityId = c.CityId,
                            CityName = c.CityName,
                            CountryId = c.CountryId,
                            CountryName = db.tblCountries.Where(x => x.CountryId == c.CountryId).Select(x => x.CountryName).FirstOrDefault()
                        }).FirstOrDefault();

            }
            else
            {
                city = new clsCity
                        {
                            CityId = 0,
                            CityName ="",
                            CountryId = 0,
                            CountryName = ""
                        };
            }
           
            return PartialView(city);
        }
        [HttpPost]
        public ActionResult AddUpdateCity(clsCity ccity)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                if (ccity.CityId > 0)
                {
                    insertUpdateStatus = "Update";

                }
                else
                {
                    insertUpdateStatus = "Save";

                }
                returnId = InsertUpdateCityDb(ccity, insertUpdateStatus);
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

        private string InsertUpdateCityDb(clsCity st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCity", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@CityId", SqlDbType.Int).Value = st.CityId;
                        cmd.Parameters.Add("@CityName", SqlDbType.NVarChar).Value = st.CityName;
                        cmd.Parameters.Add("@CountryId", SqlDbType.Int).Value = st.CountryId;
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
        public ActionResult DeleteCity(int id)
        {
            string message = "";
            bool status = false;

            clsCity st = new clsCity();
            st.CityId = id;
            string returnId = InsertUpdateCityDb(st, "Delete");
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
            //string message = "";
            //bool status = false;
            //try
            //{
            //    var result = db.tblCities.Single(city => city.CityId == id);
            //    db.tblCities.Remove(result);
            //    db.SaveChanges();
            //}
            //catch(Exception ex)
            //{
            //    message = ex.Message.ToString();
            //}

            //return new JsonResult { Data = new { status = status, message = message } };
        }

    }
}