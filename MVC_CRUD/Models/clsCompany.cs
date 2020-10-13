using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Web;

namespace MVC_CRUD.Models
{
    public class clsCompany
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }

    }
}