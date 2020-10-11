using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_CRUD.Models;

namespace MVC_CRUD.Controllers
{
    public class ComboListController : Controller
    {
        TestDbEntities db = new TestDbEntities();

        IQueryable<clsList> AllItemsList;


        #region get countries list

        public JsonResult GetCountryList(string searchTerm, int pageSize, int pageNumber)
        {
            AllItemsList = AllCountryLists();
            var select2pagedResult = new Select2PagedResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsList> AllCountryLists()
        {
            //string cacheKey = "Select2Options";
            ////check cache 
            //if (System.Web.HttpContext.Current.Cache[cacheKey] != null)
            //{
            //    return (IQueryable<clsList>)System.Web.HttpContext.Current.Cache[cacheKey];
            //}

            List<clsList> item = new List<clsList>();
            item = (from c in db.tblCountries
                    orderby c.CountryName
                    select new clsList
                    {
                        id = c.CountryId,
                        text = c.CountryName
                    }).ToList();

            var result = item.AsQueryable();

            //cache results
            //System.Web.HttpContext.Current.Cache[cacheKey] = result;

            return result;
        }
        #endregion

        List<clsList> GetPagedListOptions(string searchTerm, int pageSize, int pageNumber, out int totalSearchRecords)
        {
            var allSearchedResults = GetAllSearchResults(searchTerm);
            totalSearchRecords = allSearchedResults.Count;
            return allSearchedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        List<clsList> GetAllSearchResults(string searchTerm)
        {
            //AllItemsList = AllItemsListDetail();
            var resultList = new List<clsList>();

            if (!string.IsNullOrEmpty(searchTerm))
                resultList = AllItemsList.Where(n => n.text.ToLower().Contains(searchTerm.ToLower())).ToList();
            else
                resultList = AllItemsList.ToList();
            return resultList;
        }
    }
}