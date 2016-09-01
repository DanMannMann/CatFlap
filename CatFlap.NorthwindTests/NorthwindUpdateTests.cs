using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace FelineSoft.CatFlap.NorthwindTests
{
    [TestClass]
    public class NorthwindUpdateTests
    {
        NorthwindTestFlap dataService = new NorthwindTestFlap();

        void ConfigureMaps()
        {

        }

        public NorthwindUpdateTests()
        {
            ConfigureMaps();
            CatFlap.CollectMappingStatements = true;
            CatFlap.CollectSQLClientStatistics = true;
        }

        [TestMethod]
        public void EmployeePrimitivePropertyUpdateTest()
        {
            var emp = dataService.Employees.GetWhere<EmployeeUpdateTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp != null);
            Assert.IsTrue(emp.FirstName == "Diane");
            Assert.IsTrue(emp.LastName == "Simmons");
            Assert.IsTrue(emp.Title == "Sales Representative");
            emp.FirstName = "Dave";
            emp.LastName = "Stevens";
            emp.Title = "Janitor";
            dataService.Employees.Update(emp);

            var emp2 = dataService.Employees.GetWhere<EmployeeUpdateTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp2 != null);
            Assert.IsTrue(emp2.FirstName == "Dave");
            Assert.IsTrue(emp2.LastName == "Stevens");
            Assert.IsTrue(emp2.Title == "Janitor");
            emp2.FirstName = "Diane";
            emp2.LastName = "Simmons";
            emp2.Title = "Sales Representative";
            dataService.Employees.Update(emp2);

            var emp3 = dataService.Employees.GetWhere<EmployeeUpdateTest>(i => i.EmployeeID == 1);
            Assert.IsTrue(emp3 != null);
            Assert.IsTrue(emp3.FirstName == "Diane");
            Assert.IsTrue(emp3.LastName == "Simmons");
            Assert.IsTrue(emp3.Title == "Sales Representative");
        }

        [TestMethod]
        public void TerritoryUpdateRelatedEntityTest()
        {
            var territory = dataService.Territories.GetWhere<TerritoryUpdateTest>(i => i.TerritoryID == "01581");
            Assert.IsTrue(territory != null);
            Assert.IsTrue(territory.TerritoryDescription.Trim() == "Westboro");
            Assert.IsTrue(territory.Region != null);
            Assert.IsTrue(territory.Region.RegionDescription.Trim() == "Eastern");

            var oldRegion = territory.Region;
            var newRegion = territory.Region = dataService.Regions.GetWhere<RegionUpdateTest>(i => i.RegionID == 2);
            Assert.IsTrue(newRegion != null);
            Assert.IsTrue(newRegion.RegionDescription.Trim() == "Western");
            dataService.Territories.Update(territory);

            var territory2 = dataService.Territories.GetWhere<TerritoryUpdateTest>(i => i.TerritoryID == "01581");
            Assert.IsTrue(territory2 != null);
            Assert.IsTrue(territory2.TerritoryDescription.Trim() == "Westboro");
            Assert.IsTrue(territory2.Region != null);
            Assert.IsTrue(territory2.Region.RegionDescription.Trim() == "Western");

            territory2.Region = oldRegion;
            dataService.Territories.Update(territory2);

            var territory3 = dataService.Territories.GetWhere<TerritoryUpdateTest>(i => i.TerritoryID == "01581");
            Assert.IsTrue(territory3 != null);
            Assert.IsTrue(territory3.TerritoryDescription.Trim() == "Westboro");
            Assert.IsTrue(territory3.Region != null);
            Assert.IsTrue(territory3.Region.RegionDescription.Trim() == "Eastern");
        }

        [TestMethod]
        public void EmployeeAddTest()
        {
            EmployeeUpdateTest newEmp = new EmployeeUpdateTest()
            {
                BirthDate = DateTime.Now.Subtract(TimeSpan.FromDays(8000)),
                FirstName = "Dave",
                LastName = "Chapelle",
                Notes = "Comedian",
                Title = "Reverend"
            };
            dataService.Employees.Add(newEmp);
            Assert.IsTrue(newEmp.ID != 0);
        }
    }
}
