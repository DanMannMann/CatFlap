using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataService;
using FelineSoft.CatFlap;
using FelineSoft.CatFlap.Extensions;
using System.Linq;

namespace FelineSoft.CatFlap.NorthwindTests
{
    [TestClass]
    public class NorthwindQueryTests
    {
        NorthwindTestFlap dataService = new NorthwindTestFlap();

        void ConfigureMaps()
        {
            ////////////////////////////////////////////
            //Entity-ViewModel & ViewModel-Entity Maps//
            ////////////////////////////////////////////

            //Map for DomainModel.Order_Detail (Entity) -> FelineSoft.CatFlap.NorthwindTests.Order_Detail_EmployeeQueryTest (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Order_Detail), typeof(global::FelineSoft.CatFlap.NorthwindTests.Order_Detail_EmployeeQueryTest));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Order_Detail, global::FelineSoft.CatFlap.NorthwindTests.Order_Detail_EmployeeQueryTest>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Order (Entity) -> FelineSoft.CatFlap.NorthwindTests.Order_EmployeeQueryTest (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Order), typeof(global::FelineSoft.CatFlap.NorthwindTests.Order_EmployeeQueryTest));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Order, global::FelineSoft.CatFlap.NorthwindTests.Order_EmployeeQueryTest>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Employee (Entity) -> FelineSoft.CatFlap.NorthwindTests.Employee_EmployeeQueryTest (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Employee), typeof(global::FelineSoft.CatFlap.NorthwindTests.Employee_EmployeeQueryTest));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Employee, global::FelineSoft.CatFlap.NorthwindTests.Employee_EmployeeQueryTest>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Order (Entity) -> FelineSoft.CatFlap.NorthwindTests.Order_CustomerQueryTest (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Order), typeof(global::FelineSoft.CatFlap.NorthwindTests.Order_CustomerQueryTest));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Order, global::FelineSoft.CatFlap.NorthwindTests.Order_CustomerQueryTest>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Customer (Entity) -> FelineSoft.CatFlap.NorthwindTests.Customer_CustomerQueryTest (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Customer), typeof(global::FelineSoft.CatFlap.NorthwindTests.Customer_CustomerQueryTest));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Customer, global::FelineSoft.CatFlap.NorthwindTests.Customer_CustomerQueryTest>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Order_Detail (Entity) -> FelineSoft.CatFlap.NorthwindTests.Order_Detail_OrdersThenOrderDetailsTest (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Order_Detail), typeof(global::FelineSoft.CatFlap.NorthwindTests.Order_Detail_OrdersThenOrderDetailsTest));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Order_Detail, global::FelineSoft.CatFlap.NorthwindTests.Order_Detail_OrdersThenOrderDetailsTest>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Order (Entity) -> FelineSoft.CatFlap.NorthwindTests.Order_OrdersThenOrderDetailsTest (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Order), typeof(global::FelineSoft.CatFlap.NorthwindTests.Order_OrdersThenOrderDetailsTest));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Order, global::FelineSoft.CatFlap.NorthwindTests.Order_OrdersThenOrderDetailsTest>(() => new global::DomainModel.northwindEntities());

            //Map for DomainModel.Employee (Entity) -> FelineSoft.CatFlap.NorthwindTests.Employee_OrdersThenOrderDetailsTest (ViewModel)
            global::AutoMapper.Mapper.CreateMap(typeof(global::DomainModel.Employee), typeof(global::FelineSoft.CatFlap.NorthwindTests.Employee_OrdersThenOrderDetailsTest));
            global::FelineSoft.CatFlap.CatFlap.PreProject<global::DomainModel.Employee, global::FelineSoft.CatFlap.NorthwindTests.Employee_OrdersThenOrderDetailsTest>(() => new global::DomainModel.northwindEntities());



            ////////////////////////////////////////////
            //       AutoMapper Config Assertion      //
            ////////////////////////////////////////////

            global::AutoMapper.Mapper.AssertConfigurationIsValid();
        }

        public NorthwindQueryTests()
        {
            ConfigureMaps();
            CatFlap.CollectMappingStatements = true;
            CatFlap.CollectSQLClientStatistics = true;
        }

        [TestMethod]
        public void EmployeeGet()
        {
            var emp = dataService.Employees.GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 67);
        }

        [TestMethod]
        public void EmployeeGetUsingDeletedOrders()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            emp = dataService.Employees
               .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 56);
        }

        [TestMethod]
        public void EmployeeGetWith()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 23);
        }

        [TestMethod]
        public void EmployeeGetWithTake()
        {
            var emp2 = dataService.Employees
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1).Take(5))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.Orders.Count == 5);
        }

        [TestMethod]
        public void EmployeeGetWithOrderBy()
        {
            var emp2 = dataService.Employees
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1).OrderBy(i => i.OrderDate))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.Orders.Count == 23);
        }

        [TestMethod]
        public void EmployeeGetUsingDeletedOrdersWith()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 15);
        }

        [TestMethod]
        public void EmployeeGetUsingDeletedOrdersWithOrderDetails()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.Order_Details.Any(t => t.Discount > 0.06)))
                .With(x => x.Orders.Then().Order_Details, x => x.Where(i => i.Discount > 0.06))
                .GetWhere<Employee_OrdersThenOrderDetailsTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 14);
            var check = emp.Orders.Sum(x => x.Order_Details.Count);
            Assert.IsTrue(emp.Orders.Sum(x => x.Order_Details.Count) == 36);
        }

        [TestMethod]
        public void EmployeeGetUsingDeletedOrdersWithTake()
        {
            var emp2 = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1).Take(5))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.Orders.Count == 5);
        }

        [TestMethod]
        public void EmployeeGetUsingDeletedOrdersWithTakeOrderBy()
        {
            var emp2 = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1).Take(5).OrderBy(i => i.OrderDate))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.Orders.Count == 5);
        }

        [TestMethod]
        public void EmployeeGetUsingAllOrders()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.AllOrders)
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 123);
        }

        [TestMethod]
        public void CustomersWhereMoreThan15Orders()
        {
            var cust = dataService.Customers.Where<Customer_CustomerQueryTest>(i => i.Orders.Count > 15);
            Assert.IsTrue(cust != null);
            Assert.IsTrue(cust.Count() == 5);
            foreach (var c in cust)
            {
                Assert.IsTrue(c.Orders.Count > 15);
            }
        }

        [TestMethod]
        public void CustomersQueryMoreThan15Orders()
        {
            var cust = dataService.Customers.Query<Customer_CustomerQueryTest>(x => x.Where(i => i.Orders.Count > 15));
            Assert.IsTrue(cust != null);
            Assert.IsTrue(cust.Count() == 5);
            foreach (var c in cust)
            {
                Assert.IsTrue(c.Orders.Count > 15);
            }
        }

        [TestMethod]
        public void RepeatAllTests2()
        {
            DifferentEmployeeGet();
            DifferentEmployeeGetUsingDeletedOrders();
            DifferentEmployeeGetWith();
            DifferentEmployeeGetWithTake();
            DifferentEmployeeGetWithOrderBy();
            DifferentEmployeeGetUsingDeletedOrdersWith();
            DifferentEmployeeGetUsingDeletedOrdersWithOrderDetails();
            DifferentEmployeeGetUsingDeletedOrdersWithTake();
            DifferentEmployeeGetUsingDeletedOrdersWithTakeOrderBy();
            DifferentEmployeeGetUsingAllOrders();
            DifferentCustomersWhereMoreThan15Orders();
            DifferentCustomersQueryMoreThan15Orders();
            
        }

        [TestMethod]
        public void DifferentEmployeeGet()
        {
            var emp = dataService.Employees.GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 67);
            string s = CatFlap.MappingCode; var f = CatFlap.ClientStatistics;
        }

        public void DifferentEmployeeGetUsingDeletedOrders()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 2);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 0);
        }

        [TestMethod]
        public void DifferentEmployeeGetWith()
        {
            var emp = dataService.Employees
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 3);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 36);
        }

        [TestMethod]
        public void DifferentEmployeeGetWithTake()
        {
            var emp2 = dataService.Employees
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 3).Take(2))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 4);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.Orders.Count == 2);
        }

        [TestMethod]
        public void DifferentEmployeeGetWithOrderBy()
        {
            var emp2 = dataService.Employees
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 2).OrderBy(i => i.OrderDate))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 5);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.Orders.Count == 15);
        }

        [TestMethod]
        public void DifferentEmployeeGetUsingDeletedOrdersWith()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 6);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 0);
        }

        [TestMethod]
        public void DifferentEmployeeGetUsingDeletedOrdersWithOrderDetails()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.Order_Details.Any(t => t.Discount > 0.1)))
                .With(x => x.Orders.Then().Order_Details, x => x.Where(i => i.Discount > 0.1))
                .GetWhere<Employee_OrdersThenOrderDetailsTest>(i => i.EmployeeID == 7);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 0);
            var check = emp.Orders.Sum(x => x.Order_Details.Count);
            Assert.IsTrue(emp.Orders.Sum(x => x.Order_Details.Count) == 0);
        }

        [TestMethod]
        public void DifferentEmployeeGetUsingDeletedOrdersWithTake()
        {
            var emp2 = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 2).Take(3))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 4);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.Orders.Count == 0);
        }

        [TestMethod]
        public void DifferentEmployeeGetUsingDeletedOrdersWithTakeOrderBy()
        {
            var emp2 = dataService.Employees
                .Using(dataService, x => x.DeletedOrders)
                .With(x => x.Orders, x => x.Where(i => i.ShipVia == 1).Take(4).OrderBy(i => i.OrderDate))
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 2);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.Orders.Count == 0);
        }

        [TestMethod]
        public void DifferentEmployeeGetUsingAllOrders()
        {
            var emp = dataService.Employees
                .Using(dataService, x => x.AllOrders)
                .GetWhere<Employee_EmployeeQueryTest>(i => i.EmployeeID == 3);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.Orders.Count == 127);
        }

        [TestMethod]
        public void DifferentCustomersWhereMoreThan15Orders()
        {
            var cust = dataService.Customers.Where<Customer_CustomerQueryTest>(i => i.Orders.Count > 20);
            Assert.IsTrue(cust != null);
            Assert.IsTrue(cust.Count() == 3);
            foreach (var c in cust)
            {
                Assert.IsTrue(c.Orders.Count > 20);
            }
        }

        [TestMethod]
        public void DifferentCustomersQueryMoreThan15Orders()
        {
            var cust = dataService.Customers.Query<Customer_CustomerQueryTest>(x => x.Where(i => i.Orders.Count > 20));
            Assert.IsTrue(cust != null);
            Assert.IsTrue(cust.Count() == 3);
            foreach (var c in cust)
            {
                Assert.IsTrue(c.Orders.Count > 20);
            }
        }
    }
}
