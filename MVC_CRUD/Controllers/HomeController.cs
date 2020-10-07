using MVC_CRUD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

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
            List<clsCity> listcity = new List<clsCity>();
            listcity = ( from c in db.tblCities
                         join cc in db.tblCountries on c.CountryId equals cc.CountryId
                         select new clsCity{
                         CityId = c.CityId,
                         CityName = c.CityName,
                         CountryId = c.CountryId,
                         CountryName = cc.CountryName
                         }).ToList();
            return View(listcity);
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
                            CountryId = c.CountryId
                        }).FirstOrDefault();

            }
            else
            {
                city = new clsCity
                        {
                            CityId = 0,
                            CityName ="",
                            CountryId = 0
                        };
            }
            ViewBag.Countries = new SelectList(db.tblCountries.OrderBy(x=>x.CountryName).ToList(),"CountryId","CountryName",city.CountryId);

            return PartialView(city);
        }
        [HttpPost]
        public ActionResult AddUpdateCity(clsCity ccity)
        {
            string message = "";
            bool status = false;
            clsCity city = new clsCity();
            try
            {
                if (ccity.CityId > 0)
                {
                    var res = db.tblCities.Where(x => x.CityId == ccity.CityId).FirstOrDefault();
                    res.CityName = ccity.CityName;
                    res.CountryId = ccity.CountryId;
                    db.SaveChanges();
                }
                else
                {

                    tblCity cityy = new tblCity();
                    cityy.CityName = ccity.CityName;
                    cityy.CountryId = ccity.CountryId;
                    db.tblCities.Add(cityy);
                    db.SaveChanges();
                }
                status = true;

            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }
        
        //public ActionResult DeleteCity(int? id)
        //{
        //    var result = db.tblCities.Single(city => city.CityId == id);
        //    return PartialView(result);
        //}
        [HttpPost]
        public ActionResult DeleteCity(int id)
        {
            string message = "";
            bool status = false;
            try
            {
                var result = db.tblCities.Single(city => city.CityId == id);
                db.tblCities.Remove(result);
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                message = ex.Message.ToString();
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }

    }
}