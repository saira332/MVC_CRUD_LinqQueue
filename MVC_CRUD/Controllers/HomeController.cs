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
            var results = (from row in db.tblCities select row).ToList();
            return View(results);
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
        public ActionResult AddUpdateCity(clsCity a, int id=0)
        {
            clsCity city = new clsCity();
            if (a.CityId> 0)
            {
                var res = db.tblCities.Where(x => x.CityId == a.CityId).FirstOrDefault();
                res.CityName = a.CityName;
                res.CountryId = a.CountryId;
                db.SaveChanges();
            }
            else
            {
                tblCity cityy  = new tblCity();
                cityy.CityName = a.CityName;
                cityy.CountryId = a.CountryId;
                db.tblCities.Add(cityy);
            }

            ViewBag.Countries = new SelectList(db.tblCountries.OrderBy(x => x.CountryName).ToList(), "CountryId", "CountryName", city.CountryId);

            
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public ActionResult DeleteCity(int? id)
        {
            var result = db.tblCities.Single(city => city.CityId == id);
            return PartialView(result);
        }
        [HttpDelete]
        public ActionResult DeleteCity(int id)
        {
            var result = db.tblCities.Single(city => city.CityId == id);
            db.tblCities.Remove(result);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}