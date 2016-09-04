# CatFlap

Makes Entity Framework suck less. Draws together the concept of projection (Project<T>().To<T>()) as it appears in AutoMapper with some funky expression rewriting to allow filtered collections of related entities to be retreived in a single query and to allow query results to be limited implicitly to the data needed to populate a given viewmodel.

## I don't believe you

Say you need to get any employee from Northwind who has at least one order that isn't deleted, and you need to get that employees territories and also those non-deleted orders, but not the deleted ones. For maximum efficiency and accounting for the need to filter the related order entities you'd usually you do it a bit like this, with a judicious combination of .Include() for the territories and re-querying for the orders collection:

```csharp
using (var db = new northwindContext())
{
    db.Context.Configuration.LazyLoadingEnabled = false;
    results = db.Context.Employees.Where(x => x.Orders.Any(y => !y.Deleted)).Include(t => t.Territories).ToList();
    foreach (var x in results)
    {
        x.Orders = db.Context.Orders.Where(y => !y.Deleted && y.EmployeeID == x.EmployeeID).Include(t => t.Order_Details).ToList();
    }
    mappedResult = AutoMapper.Mapper.Map<List<EmployeeVM>>(results);
}
```

The example above returns a single employee and therefore queries for a single collection of non-deleted orders. Here's some stats:

 | Time Connected | Execution Time | Bytes Sent | Bytes Recvd |
| --- | --- | --- |
 | 16 | 17 | 49163 | 1624467 |
 
 Here's how you'd handle that with CatFlap:
 ```csharp
 mappedResult = new northwindCatFlap().Employees
                .With(x => x.Orders, x => x.Where(y => !y.Deleted))
                .Where<EmployeeVM>(x => x.Orders.Any(y => !y.Deleted))
                .ToList();
 ```
 
And some stats:
 --------------------------------------------------------------------- 
 | Time Connected | Execution Time | Bytes Sent     | Bytes Recvd    |
 --------------------------------------------------------------------- 
 | 39             | 9              | 10000          | 428888         |
 --------------------------------------------------------------------- 
 
 At tht expense of a longer time connected (spent prepping the expression whilst the connection is open, which unfortunately seems unavoidable) the execution time is almost halved, the amount of data sent is reduced by 80% and the amount of data received is reduced by nearly 75%. What's happening is that the viewmodel (EmployeeVM) is being used to automatically decide which data columns to bring back and which related entity collections to eagerly load. On top of this the .Using() call specifies a filter which is applied to the related collection *in the SQL query* so that only the needed data is returned *in a single query*.
 
 CatFlap reduces the amount of data moving around, reduces the number of queries required to statisfy most types of complex operation, makes it easier to define exactly what data to return (it is implicit in the viewmodel) and includes a load of other toys that are well worth playing with.
 
 Mappings are also done on-the-fly (unlike in AutoMapper) and saved to a static collection as valid C# code so they can be copied out and done pre-emptively whenever you want. Pulling the captured mappings out after a regression test and adding them to the build can help to speed things up significantly by eagerly doing a load of that work during intialisation.
