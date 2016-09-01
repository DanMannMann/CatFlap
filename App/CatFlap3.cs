﻿//This file was generated by the CatFlap item template

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CatFlap;

namespace App
{
    public class CatFlap3 : Modeler<DomainModel.northwindEntities>
    {
        public CatFlap3()
            : base(() => new DomainModel.northwindEntities())
        {
            Categories = CreateAccessor<DomainModel.Category>();
            CustomerDemographics = CreateAccessor<DomainModel.CustomerDemographic>();
            Customers = CreateAccessor<DomainModel.Customer>();
            Employees = CreateAccessor<DomainModel.Employee>();
            Orders = CreateAccessor<DomainModel.Order>();
            Products = CreateAccessor<DomainModel.Product>();
            Regions = CreateAccessor<DomainModel.Region>();
            Shippers = CreateAccessor<DomainModel.Shipper>();
            Suppliers = CreateAccessor<DomainModel.Supplier>();
            Territories = CreateAccessor<DomainModel.Territory>();
        }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Category> Categories { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.CustomerDemographic> CustomerDemographics { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Customer> Customers { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Employee> Employees { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Order> Orders { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Product> Products { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Region> Regions { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Shipper> Shippers { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Supplier> Suppliers { get; private set; }

        public SetAccessor<DomainModel.northwindEntities, DomainModel.Territory> Territories { get; private set; }


    }
}