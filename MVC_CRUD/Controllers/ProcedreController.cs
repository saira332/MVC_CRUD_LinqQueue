using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_CRUD.Controllers
{
    public class ProcedreController : Controller
    {
        // GET: Procedre
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult InsertUpdateLocationType(clsUser user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? LocationTypeId = user.LocationTypeId;
                if (user.LocationTypeId > 0)
                {
                    insertUpdateStatus = "Update";

                }
                else
                {
                    insertUpdateStatus = "Save";

                }
                returnId = InsertUpdateLocationTypeDb(user, insertUpdateStatus);
                if (returnId == "Success")
                {
                    ModelState.Clear();
                    status = true;
                    message = "User Type Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
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
        private string InsertUpdateLocationTypeDb(clsUser st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateLocationType", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@LocationTypeId", SqlDbType.Int).Value = st.LocationTypeId;
                        cmd.Parameters.Add("@TypeName", SqlDbType.NVarChar).Value = st.TypeName;
                        cmd.Parameters.Add("@ActionByUserId", SqlDbType.Int).Value = User.UserId;
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
    }
}