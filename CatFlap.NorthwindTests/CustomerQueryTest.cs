﻿//This file was generated by the CatFlap item template

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FelineSoft.CatFlap;

namespace FelineSoft.CatFlap.NorthwindTests
{
    public class Customer_CustomerQueryTest
    {
        public System.String CustomerID { get; set; }

        public System.Collections.Generic.ICollection<Order_CustomerQueryTest> Orders { get; set; }

    }

    public class Order_CustomerQueryTest
    {
        public System.Int32 OrderID { get; set; }

        public System.String CustomerID { get; set; }

        public System.Boolean Deleted { get; set; }

    }

}