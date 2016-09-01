using DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.QueryableExtensions;
using System.Diagnostics;
using System.Data.Entity;
using FelineSoft.CatFlap.Extensions;
using System.Linq.Expressions;
using FelineSoft.CatFlap;
using System.Collections;
//using DataService;
using AutoMapper;
using System.Runtime.Serialization;
using System.IO;
using FelineSoft.CatFlap.NorthwindTests;

namespace App
{
    class Program
    {
        static DataService.nwFlap dataService = new DataService.nwFlap();

        static void Main(string[] args)
        {
            CatFlap.CollectMappingStatements = true;
            CatFlap.CollectSQLClientStatistics = true;
            List<EmployeeVM> old;
            List<EmployeeVM> @new;
            List<DomainModel.Employee> results;
            ConfigureMaps();

            using (var db = new DataService.nwFlap().MonitoredContext())
            {
                //results = db.Context.Employees.Where(x => x.Orders.Any(y => !y.Deleted)).Include(x => x.Territories).Include(x => x.Orders.Where(y => !y.Deleted)).ToList();
                
                db.Context.Configuration.LazyLoadingEnabled = false;
                results = db.Context.Employees.Where(x => x.Orders.Any(y => !y.Deleted)).Include(t => t.Territories).ToList();
                foreach (var x in results)
                {
                    x.Orders = db.Context.Orders.Where(y => !y.Deleted && y.EmployeeID == x.EmployeeID).Include(t => t.Order_Details).ToList();
                }
                old = AutoMapper.Mapper.Map<List<EmployeeVM>>(results);
            }

            @new = new DataService.nwFlap().Employees
                .With(x => x.Orders, x => x.Where(y => !y.Deleted))
                .Where<EmployeeVM>(x => x.Orders.Any(y => !y.Deleted))
                .ToList();

            @new = new DataService.nwFlap().Employees
               .Using(dataService, x => x.NonDeletedOrders) //using the short-hand
               .Where<EmployeeVM>(x => x.Orders.Any())
               .ToList();

            var emp = dataService.Employees
               .Using(dataService, x => x.NonDeletedOrders)
               .With(x => x.Orders, x => x.Where(i => i.Order_Details.Any(t => t.Discount > 0.06)))
               .With(x => x.Orders.Then().Order_Details, x => x.Where(i => i.Discount > 0.06))
               .Where<Employee_OrdersThenOrderDetailsTest>(i => i.Orders.Any());

            var emp2 = dataService.Employees
              .Using(dataService, x => x.NonDeletedOrders)
              .With(x => x.Orders, x => x.Where(i => i.Order_Details.Any(t => t.Discount > 0.06)))
              .With(x => x.Orders.Then().Order_Details, x => x.Where(i => i.Discount > 0.06))
              .Where<Employee_OrdersThenOrderDetailsTest>(i => i.Orders.Any());

            var emp3 = dataService.Employees
              .Where<Employee_OrdersThenOrderDetailsTest>(i => i.Orders.Any(t => t.Order_Details.Any(g => g.Discount > 0.06)));

            Console.WriteLine(CatFlap.ClientStatistics.ToStatsString());
            //Console.WriteLine(CatFlap.MappingCode);
            Console.ReadKey();
        }

        static void ConfigureMaps()
        {
            ////////////////////////////////////////////
            //Entity-ViewModel & ViewModel-Entity Maps//
            ////////////////////////////////////////////

            //Map for DomainModel.Territory (Entity) -> App.Program+TerritoryVM (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Territory), typeof(global::App.Program.TerritoryVM))
                .ForMember("ID", y => y.MapFrom("TerritoryID"));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Territory, global::App.Program.TerritoryVM>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Order_Detail (Entity) -> App.Program+OrderDetailVM (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Order_Detail), typeof(global::App.Program.OrderDetailVM));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Order_Detail, global::App.Program.OrderDetailVM>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Order (Entity) -> App.Program+OrderVM (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Order), typeof(global::App.Program.OrderVM))
                .ForMember("Name", y => y.MapFrom("ShipName"))
                .ForMember("ShippingAddress", y => y.MapFrom("ShipAddress"))
                .ForMember("TrackingToken", y => y.Ignore());
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Order, global::App.Program.OrderVM>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Employee (Entity) -> App.Program+EmployeeVM (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Employee), typeof(global::App.Program.EmployeeVM))
                .ForMember("ID", y => y.MapFrom("EmployeeID"));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Employee, global::App.Program.EmployeeVM>(() => new global::DomainModel.northwindEntities());



            ////////////////////////////////////////////
            //       AutoMapper Config Assertion      //
            ////////////////////////////////////////////

            global::AutoMapper.Mapper.AssertConfigurationIsValid();
        }

        public class CustomerVM
        {
            [MapTo(typeof(Customer), "CustomerID")]
            public string ID { get; set; }

            [MapTo(typeof(Customer), "CompanyName")]
            public string Name { get; set; }

            public string ContactTitle { get; set; }
            public override string ToString()
            {
                return Name;
            }
            public List<OrderVM> Orders { get; set; }
            public int OrdersCount { get; set; }
        }

        public class OrderVM : TrackableBase
        {
            //TODO support generated/computed columns as primary keys (i.e. don't insist on presence of key on VM for add/update)
            public int OrderID { get; set; }

            public override string ToString()
            {
                return OrderDate.ToString();
            }

            [MapTo(typeof(Order), "ShipName")]
            public string Name { get; set; }

            [MapTo(typeof(Order), "ShipAddress")]
            public string ShippingAddress { get; set; }

            public string ShipperCompanyName { get; set; }

            public DateTime? OrderDate { get; set; }

            public List<OrderDetailVM> Order_Details { get; set; }

            //public EmployeeVM Employee { get; set; }
        }

        public class OrderDetailVM
        {
            public decimal UnitPrice { get; set; }
            public short Quantity { get; set; }
            public float Discount { get; set; }
            public int OrderID { get; set; }
            public int ProductID { get; set; }
        }

        [TrackAs(typeof(Employee))]
        public class EmployeeVM
        {
            [MapTo(typeof(Employee), "EmployeeID")]
            public int ID { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public List<TerritoryVM> Territories { get; set; }
            public List<OrderVM> Orders { get; set; }
        }

        public class TerritoryVM
        {
            [MapTo(typeof(Territory), "TerritoryID")]
            public string ID { get; set; }
            public string TerritoryDescription { get; set; }
            public int RegionID { get; set; }

            public TerritoryVM() { RegionID = 1; }
        }
    }
}
