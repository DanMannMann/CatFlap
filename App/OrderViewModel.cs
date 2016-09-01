using FelineSoft.CatFlap;
using DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App
{
    public class OrderViewModel
    {
        [MapTo(typeof(Order), "ShipName")]
        public string ShipFucker { get; set; }

        public string CustomerContactName { get; set; }

        public ICollection<OrderDetailViewModel> Order_Details { get; set; }
    }

    public class OrderDetailViewModel
    {
        public ProductViewModel Product { get; set; }

        public short Quantity { get; set; }
    }

    public class ProductViewModel
    {
        public string Product_Name { get; set; }
    }
}
