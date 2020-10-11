using MVC_CRUD.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_CRUD.Controllers
{
    public class CountryController : Controller
    {
        TestDbEntities db;
        public CountryController()
        {
            db = new TestDbEntities();
        }
        // GET: Country
        public ActionResult Index()
        {
            var countries = (from row in db.tblCountries select row).ToList();

            return View(countries);
        }
        public ActionResult GetAllBanks(int? id)
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var code = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var name = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                int? c_id = clsGetSchoolId.GetSchoolId(User.UserId);
                List<clsBank> Banks = (from b in db.tblBanks
                                       orderby b.BankId descending
                                       where b.SchoolId == c_id
                                       select new clsBank
                                       {
                                           BankId = b.BankId,
                                           BankName = b.BankName,
                                           Code = b.Code,
                                           Status = b.Status
                                       }).ToList();
                decimal? Total = 0;
                foreach (clsBank b in Banks)
                {
                    b.StatementBalance = getBankAccountBalance(b.BankId);
                    b.StatementBalanceForShow = b.StatementBalance.ToString() + "_normal_" + b.BankId.ToString();
                    b.IdForShow = b.BankId + "-Transaction";
                    Total += b.StatementBalance;
                }
                clsBank bb = new clsBank();
                bb.BankId = 0;
                bb.BankName = "";
                bb.Code = "";
                bb.Status = "";
                bb.IdForShow = "0-Total";
                bb.StatementBalance = 0;
                bb.StatementBalanceForShow = Total.ToString() + "_bold_0";
                Banks.Add(bb);
                if (id > 0)
                {
                    Banks = Banks.Where(c => c.BankId.Equals(id)).ToList();
                }
                if (!string.IsNullOrEmpty(code))
                {
                    Banks = Banks.Where(c => c.Code.ToLower().Contains(code.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    Banks = Banks.Where(c => c.BankName.ToLower().Contains(name.ToLower())).ToList();
                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    Banks = Banks.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }
                ViewBag.Status = new SelectList(getStatus(), "Value", "Text", "Active");
                recordsTotal = Banks.Count();
                var data = Banks.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("login", "account");
            }
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