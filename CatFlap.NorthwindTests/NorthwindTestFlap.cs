﻿//This file was generated by the CatFlap item template

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FelineSoft.CatFlap;
using DomainModel;

namespace FelineSoft.CatFlap.NorthwindTests
{
    public class NorthwindTestFlap : CatFlap<DomainModel.northwindEntities>
    {
        public NorthwindTestFlap()
            : base(() => new DomainModel.northwindEntities())
        {
            Categories = CreateAccessor<Category>();
            CustomerDemographics = CreateAccessor<CustomerDemographic>();
            Customers = CreateAccessor<Customer>(x => x.Where(i => !i.CustomerID.StartsWith("H")));
            Employees = CreateAccessor<Employee>();
            Order_Details = CreateAccessor<Order_Detail>();
            Orders = CreateAccessor<Order>(x => x.Where(i => i.Deleted == false).Where(i => i.ShipVia != null));
            DeletedOrders = CreateAccessor<Order>(x => x.Where(i => i.Deleted == true).Where(i => i.ShipVia != null));
            AllOrders = CreateAccessor<Order>();
            Products = CreateAccessor<Product>();
            Regions = CreateAccessor<Region>();
            Shippers = CreateAccessor<Shipper>();
            Suppliers = CreateAccessor<Supplier>();
            Territories = CreateAccessor<Territory>();
        }

        public ISetAccessor<northwindEntities, Category> Categories { get; private set; }

        public ISetAccessor<northwindEntities, CustomerDemographic> CustomerDemographics { get; private set; }

        public ISetAccessor<northwindEntities, Customer> Customers { get; private set; }

        public ISetAccessor<northwindEntities, Employee> Employees { get; private set; }

        public ISetAccessor<northwindEntities, Order_Detail> Order_Details { get; private set; }

        [Default]
        public ISetAccessor<northwindEntities, Order> Orders { get; private set; }

        public ISetAccessor<northwindEntities, Order> DeletedOrders { get; private set; }

        public ISetAccessor<northwindEntities, Order> AllOrders { get; private set; }

        public ISetAccessor<northwindEntities, Product> Products { get; private set; }

        public ISetAccessor<northwindEntities, Region> Regions { get; private set; }

        public ISetAccessor<northwindEntities, Shipper> Shippers { get; private set; }

        public ISetAccessor<northwindEntities, Supplier> Suppliers { get; private set; }

        public ISetAccessor<northwindEntities, Territory> Territories { get; private set; }


    }
}