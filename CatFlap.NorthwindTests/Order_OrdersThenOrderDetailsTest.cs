﻿//This file was generated by the CatFlap item template

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FelineSoft.CatFlap;

namespace FelineSoft.CatFlap.NorthwindTests
{
    public class Employee_OrdersThenOrderDetailsTest
    {
        public System.Int32 EmployeeID { get; set; }

        public System.Collections.Generic.ICollection<Order_OrdersThenOrderDetailsTest> Orders { get; set; }

        public System.Collections.Generic.ICollection<Territory_OrdersThenOrderDetailsTest> Territories { get; set; }
    }

    public class Territory_OrdersThenOrderDetailsTest
    {
        public string TerritoryID { get; set; }
        public string TerritoryDescription { get; set; }
        public int RegionID { get; set; }
        public Territory_OrdersThenOrderDetailsTest() { RegionID = 1; }
    }

    public class Order_Detail_OrdersThenOrderDetailsTest
    {
        public System.Int32 OrderID { get; set; }

        public System.Int32 ProductID { get; set; }

        public System.Decimal UnitPrice { get; set; }

        public System.Int16 Quantity { get; set; }

        public System.Single Discount { get; set; }

    }

    public class Order_OrdersThenOrderDetailsTest
    {
        public System.Int32 OrderID { get; set; }

        public System.Nullable<System.Int32> ShipVia { get; set; }

        public System.Boolean Deleted { get; set; }

        public System.Collections.Generic.ICollection<Order_Detail_OrdersThenOrderDetailsTest> Order_Details { get; set; }

    }

}