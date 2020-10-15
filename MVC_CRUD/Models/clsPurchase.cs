using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_CRUD.Models
{
    public class clsPurchase
    {
        public int PurchaseId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int ReferenceNumber { get; set; }
        public int PurchaseLineId { get; set; }
        public string ItemName { get; set; }
        public int Qyt { get; set; }
        public decimal Rate { get; set; }
    }
}