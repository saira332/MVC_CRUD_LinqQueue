using MVC_CRUD.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using Newtonsoft.Json;

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
        public ActionResult GetAllPurchases()
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
                sorting = " Order by s.PurchaseId asc";
            }
            //if (!(string.IsNullOrEmpty(HallName)))
            //{
            //    whereCondition = " LOWER(s.HallName) like ('%" + HallName + "%')";
            //}
            //else
            //{
            //    whereCondition = " LOWER(s.HallName) like ('%%')";
            //}
            List<clsPurchase> listsub = new List<clsPurchase>();
            DataTableReader dtr = clsSqlPurchase.getPurchaseListCount();
            while (dtr.Read())
            {
                recordsTotal = Convert.ToInt32(dtr["MyRowCount"]);
            }
            DataTableReader dt = clsSqlPurchase.getPurchaseList(start, length, sorting);
            //     int i = 0;
            while (dt.Read())
            {
                listsub.Add(new clsPurchase()
                {
                    PurchaseId = Convert.ToInt32(dt["PurchaseId"]),
                    PurchaseDate = Convert.ToDateTime(dt["PurchaseDate"]),
                    ReferenceNumber = Convert.ToInt32(dt["ReferenceNumber"])
                });
            }

            var data = listsub.ToList();


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                JsonRequestBehavior.AllowGet);
        }
        public ActionResult getLineDetail(int id)
        {
            List<clsPurchase> detail = new List<clsPurchase>();
            detail = (from c in db.tblPurchaseLines
                      where c.PurchaseId == id
                      orderby c.PurchaseLineId
                      select new clsPurchase
                      {
                          ItemName = c.ItemName,
                          Qyt = c.Qyt,
                          Rate = c.Rate
                      }).ToList();
            var data = detail.ToList();
            return new JsonResult { Data = new { data = data } };
        }
        [HttpGet]
        public ActionResult AddUpdatePurchase(int id = 0)
        {
            clsPurchase country = new clsPurchase();
            if (id > 0)
            {
                country = (from c in db.tblPurchases
                           where c.PurchaseId == id
                           select new clsPurchase
                           {
                               PurchaseId = c.PurchaseId,
                               PurchaseDate = c.PurchaseDate,
                               ReferenceNumber=c.ReferenceNumber,
                               ItemName = db.tblPurchaseLines.Where(x => x.PurchaseId == c.PurchaseId).Select(x => x.ItemName).FirstOrDefault(),
                               Qyt = db.tblPurchaseLines.Where(x => x.PurchaseId == c.PurchaseId).Select(x => x.Qyt).FirstOrDefault(),
                               Rate = db.tblPurchaseLines.Where(x => x.PurchaseId == c.PurchaseId).Select(x => x.Rate).FirstOrDefault()
                           }).FirstOrDefault();

            }
            else
            {
                country = new clsPurchase
                {
                    PurchaseId = 0,
                    PurchaseDate = System.DateTime.Now,
                    ReferenceNumber=0
                };
            }

            return PartialView(country);
        }
        [HttpPost]
        public ActionResult AddUpdatePurchase(clsPurchase purchase,string childData)
        {
            string message = "";
            bool status = false;
            try
            {
                List<clsPurchase> PurchaseList = JsonConvert.DeserializeObject<List<clsPurchase>>(childData);



                //data table for Branch Starts
                DataTable dtPurchase = new DataTable();
                dtPurchase.Columns.Add("Id");
                dtPurchase.Columns.Add("ItemName");
                dtPurchase.Columns.Add("Qyt");
                dtPurchase.Columns.Add("Rate");



                if (PurchaseList.Count != 0)
                {
                    for (int i = 0; i < PurchaseList.Count; i++)
                    {
                        dtPurchase.Rows.Add(new object[] { i + 1, PurchaseList[i].ItemName, PurchaseList[i].Qyt, PurchaseList[i].Rate });
                    }
                }
                else
                {
                    dtPurchase.Rows.Add(new object[] { 0,"",0,0});
                }
                string returnId = "0";
                string insertUpdateStatus = "";
                if (purchase.PurchaseId > 0)
                {
                    insertUpdateStatus = "Update";

                }
                else
                {
                    insertUpdateStatus = "Save";

                }
                returnId = InsertUpdatePurchaseDb(purchase, dtPurchase,insertUpdateStatus);
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

        private string InsertUpdatePurchaseDb(clsPurchase st, DataTable dt,string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdatePurchase", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@PurchaseId", SqlDbType.Int).Value = st.PurchaseId;
                        cmd.Parameters.Add("@PurchaseDate", SqlDbType.DateTime).Value = st.PurchaseDate;
                        cmd.Parameters.Add("@ReferenceNumber", SqlDbType.Int).Value = st.ReferenceNumber;
                        cmd.Parameters.Add("@dtPurchaseLine", SqlDbType.Structured).Value = dt;
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
        public ActionResult DeletePurchase(int id)
        {
            string message = "";
            bool status = false;

            DataTable dtPurchase = new DataTable();
            dtPurchase.Columns.Add("Id");
            dtPurchase.Columns.Add("ItemName");
            dtPurchase.Columns.Add("Qyt");
            dtPurchase.Columns.Add("Rate");



                dtPurchase.Rows.Add(new object[] { 0, "", 0, 0 });

            clsPurchase st = new clsPurchase();
            st.PurchaseId = id;
            string returnId = InsertUpdatePurchaseDb(st, dtPurchase, "Delete");
            if (returnId == "Success")
            {
                ModelState.Clear();
                status = true;
                message = "Successfully Deleted";
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