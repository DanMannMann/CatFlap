﻿//This file was generated by the CatFlap item template

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FelineSoft.CatFlap;

namespace DataService
{
    public class nwFlap : CatFlap<DomainModel.northwindEntities>
    {
        public nwFlap()
            : base(() => new DomainModel.northwindEntities())
        {
            var date = DateTime.Parse("1997-01-01 00:00:00.000");
            Categories = CreateAccessor<global::DomainModel.Category>();
            CustomerDemographics = CreateAccessor<global::DomainModel.CustomerDemographic>();
            Customers = CreateAccessor<global::DomainModel.Customer>();
            Employees = CreateAccessor<global::DomainModel.Employee>();
            Order_Details = CreateAccessor<global::DomainModel.Order_Detail>();

            NonDeletedOrders = CreateAccessor<global::DomainModel.Order>(x => x.Where(i => i.Deleted == false));
            DeletedOrders = CreateAccessor<global::DomainModel.Order>(x => x.Where(i => i.Deleted == true));
            AllOrders = CreateAccessor<global::DomainModel.Order>();

            Products = CreateAccessor<global::DomainModel.Product>();
            Regions = CreateAccessor<global::DomainModel.Region>();
            Shippers = CreateAccessor<global::DomainModel.Shipper>();
            Suppliers = CreateAccessor<global::DomainModel.Supplier>();
            Territories = CreateAccessor<global::DomainModel.Territory>();
        }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Category> Categories { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.CustomerDemographic> CustomerDemographics { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Customer> Customers { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Employee> Employees { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Order_Detail> Order_Details { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Order> DeletedOrders { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Order> NonDeletedOrders { get; private set; }

        [Default]
        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Order> AllOrders { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Product> Products { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Region> Regions { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Shipper> Shippers { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Supplier> Suppliers { get; private set; }

        public ISetAccessor<global::DomainModel.northwindEntities, global::DomainModel.Territory> Territories { get; private set; }


    }
}