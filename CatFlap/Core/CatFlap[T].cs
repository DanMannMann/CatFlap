using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FelineSoft.CatFlap.Extensions;
using AutoMapper.QueryableExtensions;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using FelineSoft.CatFlap.Utils;

namespace FelineSoft.CatFlap
{
    public abstract class CatFlap<TContext> : CatFlap
        where TContext : DbContext, new()
    {
        private Func<TContext> _contextFactory;
        private bool _longRunningContext = false;
        private List<Tuple<Type, Type>> _alreadyMapped = new List<Tuple<Type, Type>>();

        public MonitoredDbContextContainer<TContext> MonitoredContext()
        {
            var context = _contextFactory.Invoke();
            var conn = InitStats(context);
            return new MonitoredDbContextContainer<TContext>(() => CompleteStats(conn, () => "[RAW TEST]", typeof(TContext), _longRunningContext)) { Context = context };
        }

        protected CatFlap(Func<TContext> contextFactory, bool longRunningContext = false)
        {
            _longRunningContext = longRunningContext;
            _contextFactory = contextFactory;
        }

        private void AddrelatedEntityFiltersIfApplicable<T>(Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships, ref IQueryable<T> query)
        {
            var filters = relatedEntityFilters.Clone();
            var rootFilters = new Dictionary<Type, Expression>();

            var props = this.GetType().GetProperties().Where(x => 
                x.PropertyType.GetGenericTypeDefinition() == typeof(ISetAccessor) ||
                typeof(ISetAccessor).IsAssignableFrom(x.PropertyType));

            //group the properties by entity type, & exclude any type for which accessors are explicitly specified in a .Using() call
            var groupedProps = props.GroupBy(x => x.PropertyType.GetGenericArguments()[1]).Where(x => !usingRelationships.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Select(i => i).ToList());

            //Add the filters specified in .Using() calls
            foreach (var key in usingRelationships.Keys)
            {
                if (usingRelationships[key].Count == 1)
                {
                    var prop = usingRelationships[key].Single();
                    var setAccessor = prop.GetValue(this);
                    var filter = (Expression)typeof(ISetAccessor).GetProperty("FilterExpression").GetValue(setAccessor);
                    if (filter != null && filter.NodeType == ExpressionType.Lambda)
                    {
                        if ((filter as LambdaExpression).Body != (filter as LambdaExpression).Parameters[0])
                        {  //filter expression actually does something so use it.
                            var entityType = prop.PropertyType.GetGenericArguments()[1];
                            rootFilters.Add(entityType, filter);
                        }
                    }
                }
                else if (usingRelationships[key].Count > 1)
                {
                    throw new CatFlapException("Only one .Using() can be specified per entity type per data retrieval operation. Multiple usings specified for: " + key.FullName);
                }
                else
                {
                    throw new CatFlapException("usingRelationships collection cannot be empty");
                }
            }

            foreach(var group in groupedProps)
            {
                PropertyInfo prop = null;

                if (group.Value.Count() == 1)
                {
                    prop = group.Value.Single();
                }
                else if (group.Value.Count() > 1)
                {
                    var multiple = group.Value.Count(x => x.GetCustomAttributes<DefaultAttribute>().Count() == 1) > 1;
                    if (multiple)
                    {
                        throw new CatFlapSetAccessorException("Ambiguous choice for relationship collection filter.\r\nMore than one SetAccessor<" + this.GetType().BaseType.GetGenericArguments()[0].FullName + "," + group.Key.FullName + "> property of " + this.GetType().FullName + " is decorated with the [RelationshipFilter] attribute.");
                    }

                    var none = group.Value.Count(x => x.GetCustomAttributes<DefaultAttribute>().Count() == 1) == 0;
                    if (none)
                    {
                        throw new CatFlapSetAccessorException("Ambiguous choice for relationship collection filter.\r\nMore than one SetAccessor<" + this.GetType().BaseType.GetGenericArguments()[0].FullName + "," + group.Key.FullName + "> property exists on " + this.GetType().FullName + " but none is decorated with the [RelationshipFilter] attribute.");
                    }

                    prop = group.Value.Single(x => x.GetCustomAttributes<DefaultAttribute>().Count() == 1);
                }

                if (prop != null)
                {
                    var setAccessor = prop.GetValue(this);
                    var filter = (Expression)typeof(ISetAccessor).GetProperty("FilterExpression").GetValue(setAccessor);
                    if (filter != null && filter.NodeType == ExpressionType.Lambda)
                    {
                        if ((filter as LambdaExpression).Body != (filter as LambdaExpression).Parameters[0])
                        { //filter expression actually does something so use it.
                            var entityType = group.Key;
                            rootFilters.Add(entityType, filter);
                        }
                    }
                }
            }

            if (filters.Count > 0 || rootFilters.Count > 0)
            {
                //inject the navigation property filters
                var resultingExpression = new QueryInjectorExpressionVisitor(filters, rootFilters).Visit((query as IQueryable).Expression);
                query = query.Provider.CreateQuery<T>(resultingExpression);
            }
        }

        private IEnumerable<T> ExecuteCollection<T>(Func<TContext, IQueryable<T>> function, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
        {
            var db = _contextFactory.Invoke();
            SqlConnection connection = InitStats(db);

            var query = function.Invoke(db);
            AddrelatedEntityFiltersIfApplicable<T>(relatedEntityFilters, usingRelationships, ref query);
            var results = query.ToList();

            foreach (var result in results)
            {
                Track(db, result);
            }

            CompleteStats(connection, () => query.ToString(), typeof(TContext), _longRunningContext);

            if (!_longRunningContext)
            {
                db.Dispose();
            }

            return results;
        }

        private T Execute<T>(Func<TContext, IQueryable<T>> function, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
        {
            var db = _contextFactory.Invoke();

            SqlConnection connection = InitStats(db);

            var query = function.Invoke(db);
            AddrelatedEntityFiltersIfApplicable<T>(relatedEntityFilters, usingRelationships, ref query);
            var result = query.Single();

            Track(db, result);

            CompleteStats(connection, () => query.ToString(), typeof(TContext), _longRunningContext);

            if (!_longRunningContext)
            {
                db.Dispose();
            }

            return result;
        }

        private void Track(DbContext db, object value)
        {
            TrackingDictionary dict = new TrackingDictionary();
            bool trackThisInstance = typeof(ITrackable).IsAssignableFrom(value.GetType());
            foreach (var property in value.GetType().GetProperties())
            {
                if (property.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    var elementType = property.PropertyType.GetElementTypeIfCollection();
                    if (trackThisInstance) //track related entities of this instance
                    {
                        TrackedIdentities entities = new TrackedIdentities();
                        Type entityType = elementType.TrackAs();

                        if (entityType == null)
                        {
                            var entityTypeCandidates = AutoMapper.Mapper.GetAllTypeMaps().Where(x => x.SourceType == elementType).Select(x => x.DestinationType)
                                .Union(AutoMapper.Mapper.GetAllTypeMaps().Where(x => x.DestinationType == elementType).Select(x => x.SourceType)).ToList();

                            if (entityTypeCandidates.Count == 0)
                            {
                                throw new CatFlapException("Found no entity type to track against for type " + elementType.FullName);
                            }
                            else if (entityTypeCandidates.Count > 1)
                            {
                                throw new CatFlapException("Ambiguous choice of entity type to track against for type " + elementType.FullName + ". Add a [TrackAs] attribute to the class definition to specify which of the mapped types to track against.");
                            }
                            else
                            {
                                entityType = entityTypeCandidates.Single();
                            }
                        }

                        var keyNames = GetKeyNames(db, entityType);
                        var collection = (IEnumerable)property.GetValue(value);
                        var propertyNameTransforms = GetPropertyNameTransforms(elementType, entityType);

                        foreach (var item in collection)
                        {
                            TrackedIdentity entity = new TrackedIdentity();
                            foreach (var key in keyNames)
                            {
                                entity.Add(new TrackedPrimaryKey() { PropertyName = key, Value = elementType.GetProperty(propertyNameTransforms[key]).GetValue(item) });
                            }
                            entities.Add(entity);
                        }
                        dict.Add(property.Name, entities);
                    }

                    if (typeof(ITrackable).IsAssignableFrom(elementType))
                    {
                        IEnumerable items = (IEnumerable)property.GetValue(value);
                        foreach (var item in items)
                        {
                            Track(db, item); //be all recursive and shit
                        }
                    }
                }
                else if (typeof(ITrackable).IsAssignableFrom(property.PropertyType))
                {
                    Track(db, property.GetValue(value)); //be all recursive and shit
                }
            }

            if (trackThisInstance) //track related entities of this instance
            {
                typeof(ITrackable).GetProperty("TrackingToken").SetValue(value, dict.ToBase64());
            }
        }

        private string[] GetKeyNames(DbContext db, Type entityType)
        {
            var method = typeof(CatFlap).GetMethod("GetPrimaryKeyNames", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(DbContext) }, null);
            var generic = method.MakeGenericMethod(entityType);
            var keyNames = (string[])generic.Invoke(this, new object[] { db });
            return keyNames;
        }

        private static Dictionary<string, string> GetPropertyNameTransforms(Type modelType, Type entityType)
        {
            var propertyNameTransforms = new Dictionary<string, string>();
      
            var map = AutoMapper.Mapper.FindTypeMapFor(entityType, modelType);
            if (map == null)
            {
                map = AutoMapper.Mapper.FindTypeMapFor(modelType, entityType);
                propertyNameTransforms = map.GetPropertyMaps().Where(x => x.SourceMember != null && x.DestinationProperty != null).ToDictionary(x => x.DestinationProperty.Name, x => x.SourceMember.Name);
            }
            else
            {
                propertyNameTransforms = map.GetPropertyMaps().Where(x => x.SourceMember != null && x.DestinationProperty != null).ToDictionary(x => x.SourceMember.Name, x => x.DestinationProperty.Name);
            }
            return propertyNameTransforms;
        }

        private int Count<T>(Func<TContext, IQueryable<T>> function)
        {
            var db = _contextFactory.Invoke();

            SqlConnection connection = InitStats(db);

            var query = function.Invoke(db);
            var result = query.Count();

            CompleteStats(connection, () => "[COUNT] " + query.ToString(), typeof(TContext), _longRunningContext);

            if (!_longRunningContext)
            {
                db.Dispose();
            }

            return result;
        }

        internal IEnumerable<Tout> GetCollection<Tout, Tin>(System.Linq.Expressions.Expression<Func<Tin, bool>> predicate, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
            where Tin : class
        {
            MapTypes<Tin, Tout>();
            return ExecuteCollection<Tout>(x =>
                (x.Set<Tin>() as IQueryable<Tin>).Where(predicate).Project().To<Tout>(),
                relatedEntityFilters,
                usingRelationships);
        }

        internal IEnumerable<Tout> GetCollection<Tout, Tin>(Func<IQueryable<Tin>, IQueryable<Tin>> linq, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
            where Tin : class
        {
            MapTypes<Tin, Tout>();
            return ExecuteCollection<Tout>(x =>
                linq.Invoke((x.Set<Tin>() as IQueryable<Tin>)).Project().To<Tout>(),
                relatedEntityFilters,
                usingRelationships);
        }

        internal IEnumerable<Tout> GetCollection<Tout, Tin, Tselect>(Func<IQueryable<Tin>, IQueryable<Tselect>> linq, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
            where Tin : class
        {
            MapTypes<Tselect, Tout, Tin>();
            return ExecuteCollection<Tout>(x =>
                linq.Invoke((x.Set<Tin>() as IQueryable<Tin>)).Project().To<Tout>(),
                relatedEntityFilters,
                usingRelationships);
        }

        internal Tout Get<Tout, Tin>(System.Linq.Expressions.Expression<Func<Tin, bool>> predicate, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
            where Tin : class
        {
            MapTypes<Tin, Tout>();
            return Execute<Tout>(x =>
                (x.Set<Tin>() as IQueryable<Tin>).Where(predicate).Project().To<Tout>(),
                relatedEntityFilters,
                usingRelationships);
        }

        internal Tout Get<Tout, Tin>(Func<IQueryable<Tin>, IQueryable<Tin>> linq, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
            where Tin : class
        {
            MapTypes<Tin, Tout>();
            return Execute<Tout>(x =>
                linq.Invoke((x.Set<Tin>() as IQueryable<Tin>)).Project().To<Tout>(),
                relatedEntityFilters,
                usingRelationships);
        }

        internal Tout Get<Tout, Tin, Tselect>(Func<IQueryable<Tin>, IQueryable<Tselect>> linq, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
            where Tin : class
        {
            MapTypes<Tselect, Tout, Tin>();
            return Execute<Tout>(x =>
                linq.Invoke((x.Set<Tin>() as IQueryable<Tin>)).Project().To<Tout>(),
                relatedEntityFilters,
                usingRelationships);
        }

        private Dictionary<System.Reflection.PropertyInfo, RelationshipType> GetNavigationProperties(DbContext context, Type entityType)
        {
            Dictionary<System.Reflection.PropertyInfo, RelationshipType> properties = new Dictionary<System.Reflection.PropertyInfo, RelationshipType>();
            MethodInfo method = typeof(System.Data.Entity.Core.Objects.ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes);
            MethodInfo generic = method.MakeGenericMethod(entityType);
            var set = generic.Invoke(context.GetObjectContext(), null);
            PropertyInfo pi = set.GetType().GetProperty("EntitySet");
            System.Data.Entity.Core.Metadata.Edm.EntitySet es = (System.Data.Entity.Core.Metadata.Edm.EntitySet)pi.GetValue(set);
            var entitySetElementType = es.ElementType;
            foreach (var navigationProperty in entitySetElementType.NavigationProperties)
            {
                RelationshipType type = RelationshipType.One;

                if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                        && navigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                {
                    type = RelationshipType.ManyToMany;
                }
                else if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                        && navigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One)
                {
                    type = RelationshipType.OneToMany;
                }

                properties.Add(entityType.GetProperty(navigationProperty.Name), type);
            }
            return properties;
        }

        //TODO Use this (GetForeignKeyProperties) to do cool stuff
        private List<PropertyInfo> GetForeignKeyProperties(DbContext context, Type entityType)
        {
            List<System.Reflection.PropertyInfo> properties = new List<PropertyInfo>();
            MethodInfo method = typeof(ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes);
            MethodInfo generic = method.MakeGenericMethod(entityType);
            var set = generic.Invoke(((IObjectContextAdapter)context).ObjectContext, null);
            PropertyInfo pi = set.GetType().GetProperty("EntitySet");
            EntitySet es = (EntitySet)pi.GetValue(set);

            var fk = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItems<AssociationType>(DataSpace.CSpace).Where(a => a.IsForeignKey);
            var fknames = fk.Where(x => x.ReferentialConstraints[0].ToRole.Name == es.Name).Select(x => x.ReferentialConstraints[0].ToProperties[0].Name).ToList();

            foreach (var property in fknames)
            {
                properties.Add(entityType.GetProperty(property));
            }
            return properties;
        }

        internal void Update<Tmodel, Tdb>(Tmodel input, System.Linq.Expressions.Expression<Func<Tdb, bool>> predicate = null)
            where Tdb : class, new()
        {
            var db = _contextFactory.Invoke();
            SqlConnection connection = null;

            try
            {
                connection = InitStats(db);

                if (predicate != null) //grab the specified existing entity and update it directly
                {
                    var query = db.Set<Tdb>().Where(predicate).Take(1);
                    var target = query.FirstOrDefault();

                    if (target == null)
                    {
                        CompleteStats(connection, () => "[UPDATE(failed - no match for predicate)]", typeof(TContext), _longRunningContext);
                        throw new CatFlapUpdateException("Failed to update from model - no match found in the data set using the specified predicate", null);
                    }

                    //ConfigureMap(typeof(Tmodel), typeof(Tdb), true, false, false, false, true);
                    MapTypesRecursiveForInsert(typeof(Tmodel), typeof(Tdb), null, true, false);
                    AutoMapper.Mapper.Map(input, target);
                    try
                    {
                        db.SaveChanges();
                        CompleteStats(connection, () => query.ToString(), typeof(TContext), _longRunningContext);
                    }
                    catch (Exception ex)
                    {
                        CompleteStats(connection, () => "[UPDATE(failed)]" + query.ToString(), typeof(TContext), _longRunningContext);
                        throw new CatFlapUpdateException("Failed to update from model", ex);
                    }
                    return;
                }

                //No predicate specified, try to do a partial update with a new instance of Tdb.
                //For this to work the viewmodel type Tmodel must contain all of the primary key columns
                //for an existing entity in the database.
                db.Configuration.ValidateOnSaveEnabled = false;
                var set = db.Set<Tdb>();
                var dbInput = Activator.CreateInstance<Tdb>();

                MapTypesRecursiveForInsert(typeof(Tmodel), typeof(Tdb), null, true, false);
                var unmapped = ConfigureMap(typeof(Tmodel), typeof(Tdb), true, true, true, false, true);
                var mapped = typeof(Tdb).GetProperties().Where(x => x.PropertyType.IsPrimitive || x.PropertyType == typeof(string)).Select(x => x.Name).Except(unmapped).ToList();
                AutoMapper.Mapper.Map(input, dbInput);

                try
                {
                    set.Attach(dbInput);
                    mapped.ForEach(x => db.Entry(dbInput).Property(x).IsModified = true);
                }
                catch (InvalidOperationException)
                { //an entity already exists for this db row, just re-use it
                    var names = GetPrimaryKeyNames<Tdb>(db);
                    List<object> keyParams = new List<object>();
                    foreach (var prop in typeof(Tdb).GetProperties().Where(x => names.Contains(x.Name)))
                    {
                        keyParams.Add(prop.GetValue(dbInput, null));
                    }
                    var entity = set.Find(keyParams.ToArray());
                    if (entity == null)
                    {
                        throw new CatFlapUpdateException("Failed to attach entity or find existing entity", null);
                    }
                    else
                    {
                        AutoMapper.Mapper.Map(input, entity);
                        mapped.ForEach(x => db.Entry(entity).Property(x).IsModified = true);
                    }
                }

                try
                {
                    db.SaveChanges();
                    CompleteStats(connection, () => "[UPDATE]", typeof(TContext), _longRunningContext);
                }
                catch (Exception ex)
                {
                    CompleteStats(connection, () => "[UPDATE(failed)]", typeof(TContext), _longRunningContext);
                    if (ex.Message.Contains("update, insert, or delete statement affected an unexpected number of rows (0)"))
                    {
                        throw new CatFlapUpdateException("Failed to update from model - entity does not exist", ex);
                    }
                    throw new CatFlapUpdateException("Failed to update from model", ex);
                }
            }
            catch (Exception ex)
            {
                if (ex is CatFlapUpdateException)
                {
                    throw ex;
                }
                if (connection != null)
                {
                    CompleteStats(connection, () => "[UPDATE(failed)]", typeof(TContext), _longRunningContext);
                }
                throw new CatFlapUpdateException("Failed to update from model - unexpected exception", ex);
            }
            if (!_longRunningContext)
            {
                db.Dispose();
            }
        }

        internal void Add<Tmodel, Tdb>(Tmodel input)
            where Tdb : class, new()
        {
            var db = _contextFactory.Invoke();
            SqlConnection connection = null;

            try
            {
                connection = InitStats(db);

                //For this to work the viewmodel type Tmodel must contain all of the primary key columns
                //and required fields for the entity.
                var set = db.Set<Tdb>();
                var dbInput = Activator.CreateInstance<Tdb>();

                MapTypesRecursiveForInsert(typeof(Tmodel), typeof(Tdb), null, true, true);
                //ConfigureMap(typeof(Tmodel), typeof(Tdb), true, true, true, true, true);
                AutoMapper.Mapper.Map(input, dbInput);
                set.Add(dbInput);
                try
                {
                    db.SaveChanges();
                    CompleteStats(connection, () => "[ADD]", typeof(TContext), _longRunningContext);
                    
                    //Map back onto the input model so that any auto-number IDs are returned to the caller via the input model (if a mapping exists)
                    MapTypes<Tdb, Tmodel>();
                    AutoMapper.Mapper.Map(dbInput, input);
                }
                catch (DbUpdateException ex)
                {
                    //Failed to add, probably a primary key violation.
                    set.Remove(dbInput);
                    CompleteStats(connection, () => "[ADD(failed)]", typeof(TContext), _longRunningContext);

                    Exception test = ex;
                    var primaryKeyException = false;
                    if (test.Message.ToLower().Contains("cannot insert duplicate key in object"))
                    {
                        primaryKeyException = true;
                    }
                    else
                    {
                        while (test.InnerException != null)
                        {
                            test = test.InnerException;
                            if (test.Message.ToLower().Contains("cannot insert duplicate key in object"))
                            {
                                primaryKeyException = true;
                                break;
                            }
                        }
                    }

                    if (primaryKeyException)
                    {
                        throw new CatFlapPrimaryKeyException("Failed to add from model - " + test.Message, ex);
                    }
                    else
                    {
                        throw new CatFlapAddException("Failed to add from model", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is CatFlapAddException)
                {
                    throw ex;
                }
                CompleteStats(connection, () => "[ADD(failed)]", typeof(TContext), _longRunningContext);
                throw new CatFlapAddException("Failed to add from model - unexpected exception", ex);
            }
            if (!_longRunningContext)
            {
                db.Dispose();
            }
        }

        internal void AddOrUpdate<Tmodel, Tdb>(Tmodel input)
            where Tdb : class, new()
        {
            SqlConnection connection = null;

            try
            {
                MapTypesRecursiveForInsert(typeof(Tmodel), typeof(Tdb), null, true, true);
                //ConfigureMap(typeof(Tmodel), typeof(Tdb), true, true, true, true, true);
                var db = _contextFactory.Invoke();

                connection = InitStats(db);

                //For this to work the viewmodel type Tmodel must contain all of the primary key columns
                //and required fields for the entity.
                var set = db.Set<Tdb>();
                var dbInput = Activator.CreateInstance<Tdb>();

                AutoMapper.Mapper.Map(input, dbInput);
                set.Add(dbInput);
                try
                {
                    db.SaveChanges();
                    CompleteStats(connection, () => "[ADD]", typeof(TContext), _longRunningContext);
                }
                catch (DbUpdateException dbEx) //TODO it may be better to ask permission than ask forgiveness here
                {
                    //Failed to add, probably a primary key violation. Try to update
                    set.Remove(dbInput);
                    CompleteStats(connection, () => "[ADD(failed)]", typeof(TContext), _longRunningContext);
                    try
                    {
                        Update<Tmodel, Tdb>(input);
                    }
                    catch (Exception ex)
                    {
                        throw new CatFlapAddOrUpdateException("Failed to add or update from model", ex, dbEx);
                    }
                }
                if (!_longRunningContext)
                {
                    db.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (ex is CatFlapAddOrUpdateException)
                {
                    throw ex;
                }
                throw new CatFlapAddOrUpdateException("Failed to add or update from model - unexpected exception", ex, null);
            }
        }

        protected SetAccessor<TContext, T> CreateAccessor<T>() where T : class, new()
        {
            return new SetAccessor<TContext, T>(this);
        }

        protected SetAccessor<TContext, T> CreateAccessor<T>(Expression<Func<ICollection<T>, IEnumerable<T>>> filterQuery)
            where T : class, new()
        {
            return new SetAccessor<TContext, T>(this, filterQuery);
        }

        //protected SoftDeleteSetAccessor<TContext, T> CreateSoftDeleteAccessor<T>(Expression<Func<T, bool>> softDeleteSelector) where T : class, new()
        //{
        //    return new SoftDeleteSetAccessor<TContext, T>(this, softDeleteSelector);
        //}

        //protected SoftDeleteSetAccessor<TContext, T> CreateSoftDeleteAccessor<T>(Expression<Func<T, bool>> softDeleteSelector, Expression<Func<ICollection<T>, IEnumerable<T>>> filterQuery)
        //    where T : class, new()
        //{
        //    return new SoftDeleteSetAccessor<TContext, T>(this, softDeleteSelector, filterQuery);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource">Recursive map assumes the source is the ORM entity.</typeparam>
        /// <typeparam name="TDest">Recursive map assumes the destination is the ViewModel.</typeparam>
        private void MapTypes<TSource, TDest>()
        {
            MapTypes(typeof(TSource), typeof(TDest), null);
        }

        private void MapTypes<TSource, TDest, Tunderlying>()
        {
            MapTypes(typeof(TSource), typeof(TDest), typeof(Tunderlying));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TSource">Recursive map assumes the source is the ORM entity.</param>
        /// <param name="TDest">Recursive map assumes the destination is the ViewModel.</param>
        private void MapTypes(Type TSource, Type TDest, Type underlying = null)
        {
            MapTypesRecursive(TSource, TDest, underlying);
            AutoMapper.Mapper.AssertConfigurationIsValid(); //throws if the ViewModel type specified does not map cleanly
        }

        private void MapTypesRecursive(Type TSource, Type TDest, Type underlying)
        {
            _alreadyMapped.Add(new Tuple<Type, Type>(TSource, TDest));
            var sourceProps = TSource.GetProperties().Where(x => !x.PropertyType.IsPrimitive && x.PropertyType != typeof(string)).ToList();
            var destProps = TDest.GetProperties().Where(x => !x.PropertyType.IsPrimitive && x.PropertyType != typeof(string)).ToList();

            var viewModelAttributes = TDest.MapToAttributes();

            foreach (var destProp in destProps)
            {
                string mapTo = null;
                var attrs = viewModelAttributes.Where(x => x.Key.Name == destProp.Name).Select(x => x.Value).FirstOrDefault();
                if (attrs != null)
                {
                    var attr = attrs.Where(x => x.EntityType == TSource).FirstOrDefault();
                    if (attr != null)
                    {
                        mapTo = attr.PropertyName;
                    }
                }

                mapTo = mapTo == null ? destProp.Name : mapTo;

                var match = sourceProps.FirstOrDefault(x => mapTo == x.Name);
                if (match != null)
                {
                    var matchType = match.PropertyType.GetElementTypeIfCollection();
                    var destType = destProp.PropertyType.GetElementTypeIfCollection();

                    if (_alreadyMapped.Contains(new Tuple<Type, Type>(matchType, destType)))
                    {
                        //there is a circular reference. We need to configure this map immediately so it is available when needed
                        ConfigureMap(matchType, destType, false, false, false, false, false, underlying);
                    }
                    else if (matchType != destType)
                    {
                        MapTypesRecursive(matchType, destType, null);
                    }
                }
            }

            ConfigureMap(TSource, TDest, false, false, false, false, false, underlying);
        }

        private void MapTypesRecursiveForInsert(Type TSource, Type TDest, Type underlying, bool requireKeys, bool requireRequiredProperties)
        {
            _alreadyMapped.Add(new Tuple<Type, Type>(TSource, TDest));
            var sourceProps = TSource.GetProperties().Where(x => !x.PropertyType.IsPrimitive && x.PropertyType != typeof(string)).ToList();
            var destProps = TDest.GetProperties().Where(x => !x.PropertyType.IsPrimitive && x.PropertyType != typeof(string)).ToList();

            var viewModelAttributes = TSource.MapToAttributes();

            foreach (var sourceProp in sourceProps)
            {
                string mapTo = null;
                var attrs = viewModelAttributes.Where(x => x.Key.Name == sourceProp.Name).Select(x => x.Value).FirstOrDefault();
                if (attrs != null)
                {
                    var attr = attrs.Where(x => x.EntityType == TDest).FirstOrDefault();
                    if (attr != null)
                    {
                        mapTo = attr.PropertyName;
                    }
                }

                mapTo = mapTo == null ? sourceProp.Name : mapTo;

                var match = destProps.FirstOrDefault(x => mapTo == x.Name);
                if (match != null)
                {
                    var matchType = match.PropertyType.GetElementTypeIfCollection();
                    var srcType = sourceProp.PropertyType.GetElementTypeIfCollection();

                    if (_alreadyMapped.Contains(new Tuple<Type, Type>(srcType, matchType)))
                    {
                        //there is a circular reference. We need to configure this map immediately so it is available when needed
                        ConfigureMap(srcType, matchType, true, true, requireKeys, requireRequiredProperties, true, underlying);
                    }
                    else if (srcType != matchType)
                    {
                        MapTypesRecursiveForInsert(srcType, matchType, null, requireKeys, requireRequiredProperties);
                    }
                }
            }

            ConfigureMap(TSource, TDest, true, true, requireKeys, requireRequiredProperties, true, underlying);
        }

        private string[] ConfigureMap(Type source, Type dest, bool sourceIsViewModel, bool ignoreUnmapped, bool requireKeys, bool requireRequiredProperties, bool isAddOrUpdate, Type underlying = null)
        {
            if (requireKeys)
            {
                AssertKeysExist(dest, source);
            }

            var map = AutoMapper.Mapper.FindTypeMapFor(source, dest);

            if (map == null) //no map exists
            {
                List<string> forMemberStatements;
                string[] unmapped = BuildMap(source, dest, sourceIsViewModel, ignoreUnmapped, isAddOrUpdate, out forMemberStatements);
                Collect(source, dest, sourceIsViewModel, forMemberStatements, underlying);
                return unmapped;
            }

            if (requireRequiredProperties)
            {
                AssertRequiredPropertiesExist(dest, source);
            }

            if (ignoreUnmapped) //if already mapped we need to list out which properties are ignored on that map when ignoreUnmapped = true
            {
                return map.GetPropertyMaps()
                        .Where(x => x.IsIgnored())
                        .Select(x => x.DestinationProperty.Name)
                        .ToArray();
            }
            else //caller doesn't care about ignored properties
            {
                return null;
            }
        }

        private string[] BuildMap(Type source, Type dest, bool sourceIsViewModel, bool ignoreUnmapped, bool isAddOrUpdate, out List<string> forMemberStatements)
        {
            AutoMapper.IMappingExpression expression = AutoMapper.Mapper.CreateMap(source, dest);
            IncrementInlineMappingCount(typeof(TContext), _longRunningContext);

            var statements = forMemberStatements = new List<string>();
            //add custom mappings specified as MapTo attributes, if any exist
            if (sourceIsViewModel)
            {
                var mappingDestination = dest;
                var mappings = source.MapToAttributes();

                mappings.ToList().ForEach(x =>
                {
                    var destAttr = x.Value.FirstOrDefault(t => t.EntityType == mappingDestination);
                    if (destAttr != null)
                    {
                        expression.ForMember(destAttr.PropertyName, y => y.MapFrom(x.Key.Name));
                        statements.Add(".ForMember(\"" + destAttr.PropertyName + "\", y => y.MapFrom(\"" + x.Key.Name + "\"))");
                    }

                });
            }
            else
            {
                var mappingSource = source;
                var mappings = dest.MapToAttributes();

                mappings.ToList().ForEach(x =>
                {
                    var destAttr = x.Value.FirstOrDefault(t => t.EntityType == mappingSource);
                    if (destAttr != null)
                    {
                        expression.ForMember(x.Key.Name, y => y.MapFrom(destAttr.PropertyName));
                        statements.Add(".ForMember(\"" + x.Key.Name + "\", y => y.MapFrom(\"" + destAttr.PropertyName + "\"))");
                    }

                });
            }

            var ignored = dest.IgnoredProperties();
            ignored.ForEach(x =>
            {
                expression.ForMember(x, y => y.Ignore());
                statements.Add(".ForMember(\"" + x + "\", y => y.Ignore())");
            });

            if (ignoreUnmapped)
            {
                var map = AutoMapper.Mapper.FindTypeMapFor(source, dest);
                var unmapped = map.GetUnmappedPropertyNames().ToArray();
                List<string> complexProperties = new List<string>();

                if (isAddOrUpdate)
                {
                    complexProperties = source
                        .GetProperties()
                        .Where(x => !(x.PropertyType.IsPrimitive || x.PropertyType == typeof(string)))
                        .Select(x => x.Name)
                        .ToList();
                    foreach (var property in complexProperties.Except(ignored))
                    {
                        expression.ForMember(property, x => x.Ignore());
                        statements.Add(".ForMember(\"" + property + "\", x => x.Ignore()) //Ignore reference properties and collections for add and/or update maps");
                    }
                }

                foreach (var property in unmapped.Except(ignored).Except(complexProperties))
                {
                    expression.ForMember(property, x => x.Ignore());
                    statements.Add(".ForMember(\"" + property + "\", x => x.Ignore())");
                }

                return unmapped;
            }
            else
            {
                return null;
            }
        }

        private void AssertRequiredPropertiesExist(Type dest, Type source)
        {
            MethodInfo method = typeof(CatFlap).GetMethod("AssertRequiredPropertiesExist");
            MethodInfo generic = method.MakeGenericMethod(dest, source);
            generic.Invoke(this, new object[] { _contextFactory, true });
        }

        private void AssertKeysExist(Type dest, Type source)
        {
            MethodInfo method = typeof(CatFlap).GetMethod("AssertKeysExist");
            MethodInfo generic = method.MakeGenericMethod(dest, source);
            generic.Invoke(this, new object[] { _contextFactory, true });
        }

        private void Collect(Type source, Type dest, bool sourceIsViewModel, List<string> forMemberStatements, Type underlying = null)
        {
            var src = source.FullName.Replace("+", ".");
            var dst = dest.FullName.Replace("+", ".");
            string statement = string.Format("//Map for {0} ({1}) -> {2} ({3})\r\n    ",
                source.FullName,
                sourceIsViewModel ? "ViewModel" : "Entity",
                dest.FullName,
                sourceIsViewModel ? "Entity" : "ViewModel");

            statement += "global::AutoMapper.Mapper.CreateMap(typeof(global::" + src + "), typeof(global::" + dst + "))";

            foreach (var forMember in forMemberStatements)
            {
                statement = statement + "\r\n        " + forMember;
            }

            if (!sourceIsViewModel) //query/get uses projection, so when destination is VM do a PreProject to build the expression tree immediately
            {
                if (underlying == null)
                {
                    statement += ";\r\n    global::FelineSoft.CatFlap.CatFlap.PreProject<global::" + src + ",global::" + dst + ">(() => new global::" + typeof(TContext).FullName.Replace("+", ".") + "());\r\n";
                }
                else
                {
                    var udl = underlying.FullName.Replace("+", ".");
                    statement += ";\r\n    global::FelineSoft.CatFlap.CatFlap.PreProject<global::" + udl + ",global::" + src + ",global::" + dst + ">(() => new global::" + typeof(TContext).FullName.Replace("+", ".") + "());\r\n";
                }
            }
            else
            {
                statement += ";\r\n";
            }

            MappingStatements.Add(statement);
        }

        internal int Count<Tset>(Func<IQueryable<Tset>, IQueryable<Tset>> linq)
            where Tset : class, new()
        {
            return Count<Tset>(x =>
                linq.Invoke((x.Set<Tset>() as IQueryable<Tset>)));
        }

        internal int Count<Tset, TQuery>(Func<IQueryable<Tset>, IQueryable<TQuery>> linq)
            where Tset : class, new()
        {
            return Count<TQuery>(x =>
                linq.Invoke((x.Set<Tset>() as IQueryable<Tset>)));
        }
    }
}
