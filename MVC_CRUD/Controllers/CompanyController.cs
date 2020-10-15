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
    public class CompanyController : Controller
    {
        TestDbEntities db;
        public CompanyController()
        {
            db = new TestDbEntities();
        }

        // GET: Company
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetAllCompanies()
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
                sorting = " Order by s.CompanyId asc";
            }
            //if (!(string.IsNullOrEmpty(HallName)))
            //{
            //    whereCondition = " LOWER(s.HallName) like ('%" + HallName + "%')";
            //}
            //else
            //{
            //    whereCondition = " LOWER(s.HallName) like ('%%')";
            //}
            List<clsCompany> listsub = new List<clsCompany>();
            DataTableReader dtr = clsSqlCompany.getCompanyListCount();
            while (dtr.Read())
            {
                recordsTotal = Convert.ToInt32(dtr["MyRowCount"]);
            }
            DataTableReader dt = clsSqlCompany.getCompanyList(start, length, sorting);
            //     int i = 0;
            while (dt.Read())
            {
                listsub.Add(new clsCompany()
                {
                    CompanyId = Convert.ToInt32(dt["CompanyId"]),
                    CompanyName = dt["CompanyName"].ToString(),
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
        public ActionResult AddUpdateCompany(int id = 0)
        {
            clsCompany city = new clsCompany();
            if (id > 0)
            {
                city = (from c in db.tblCompanies
                        where c.CompanyId == id
                        select new clsCompany
                        {
                            CompanyId = c.CompanyId,
                            CompanyName = c.CompanyName,
                            CityId = c.CityId,
                            CityName = db.tblCities.Where(x => x.CityId == c.CountryId).Select(x => x.CityName).FirstOrDefault(),
                            CountryId = c.CountryId,
                            CountryName = db.tblCountries.Where(x => x.CountryId == c.CountryId).Select(x => x.CountryName).FirstOrDefault()
                        }).FirstOrDefault();

            }
            else
            {
                city = new clsCompany
                {
                    CompanyId =0,
                    CompanyName ="",
                    CityId = 0,
                    CityName = "",
                    CountryId = 0,
                    CountryName = ""
                };
            }

            return PartialView(city);
        }
        [HttpPost]
        public ActionResult AddUpdateCompany(clsCompany company)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                if (company.CompanyId > 0)
                {
                    insertUpdateStatus = "Update";

                }
                else
                {
                    insertUpdateStatus = "Save";

                }
                returnId = InsertUpdateCompanyDb(company, insertUpdateStatus);
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

        private string InsertUpdateCompanyDb(clsCompany st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCompany", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = st.CompanyId;
                        cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar).Value = st.CityName;
                        cmd.Parameters.Add("@CityId", SqlDbType.Int).Value = st.CityId;
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
        public ActionResult DeleteCompany(int id)
        {
            string message = "";
            bool status = false;

            clsCompany st = new clsCompany();
            st.CompanyId = id;
            string returnId = InsertUpdateCompanyDb(st, "Delete");
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