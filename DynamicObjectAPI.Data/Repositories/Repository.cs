using DynamicObjectAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DynamicObjectAPI.Common.Exceptions;
using System.Collections;

namespace DynamicObjectAPI.Data.Repositories
{
    public class Repository : IRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Repository> _logger;

        public Repository(ApplicationDbContext context, ILogger<Repository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> CreateAsync(string objectType, JObject data)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entityType = GetEntityType(objectType);
                var entity = MapToEntity(entityType, data);

                _context.Add(entity);
                await _context.SaveChangesAsync();

                var entityId = GetEntityId(entity, objectType);

                await ProcessChildEntities(entityType, entity, data);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return entityId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error occurred while creating {objectType}");
                throw new RepositoryOperationException($"Failed to create {objectType}", ex);
            }
        }

        public async Task<JObject> GetByIdAsync(string objectType, int id)
        {
            try
            {
                var entityType = GetEntityType(objectType);
                var entity = await GetEntityById(entityType, id);

                if (entity == null)
                    return null;

                return MapEntityToJObject(entityType, entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting {objectType} with id {id}");
                throw new RepositoryOperationException($"Failed to get {objectType} with id {id}", ex);
            }
        }

        public async Task<IEnumerable<JObject>> GetAllAsync(string objectType, JObject filters = null)
        {
            try
            {
                var entityType = GetEntityType(objectType);
                var query = GetQueryable(entityType);

                query = IncludeChildEntities(query, entityType);

                if (filters != null)
                {
                    var filterString = BuildFilterString(filters);
                    query = query.Where(filterString);
                }

                var toListAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
                    .GetMethod("ToListAsync", new Type[] { typeof(IQueryable<>), typeof(CancellationToken) })
                    .MakeGenericMethod(entityType);

                var task = (Task)toListAsyncMethod.Invoke(null, new object[] { query, CancellationToken.None });
                await task.ConfigureAwait(false);

                
                var resultProperty = task.GetType().GetProperty("Result");
                var entities = (IList)resultProperty.GetValue(task);

                return entities.Cast<object>().Select(e => MapEntityToJObject(entityType, e));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting all {objectType}");
                throw new RepositoryOperationException($"Failed to get all {objectType}", ex);
            }
        }


        public async Task<bool> UpdateAsync(string objectType, int id, JObject data)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entityType = GetEntityType(objectType);
                var entity = await GetEntityById(entityType, id);

                if (entity == null)
                    return false;

                UpdateEntityProperties(entityType, entity, data);
                await UpdateChildEntities(entityType, entity, data);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error occurred while updating {objectType} with id {id}");
                throw new RepositoryOperationException($"Failed to update {objectType} with id {id}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string objectType, int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entityType = GetEntityType(objectType);
                var entity = await GetEntityById(entityType, id);

                if (entity == null)
                    return false;

                _context.Remove(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error occurred while deleting {objectType} with id {id}");
                throw new RepositoryOperationException($"Failed to delete {objectType} with id {id}", ex);
            }
        }

        private Type GetEntityType(string objectType)
        {
            var entityType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name.Equals(objectType, StringComparison.OrdinalIgnoreCase));

            if (entityType == null)
                throw new ArgumentException($"Entity type '{objectType}' not found.", nameof(objectType));

            return entityType;
        }

        private object MapToEntity(Type entityType, JObject data)
        {
            var entity = Activator.CreateInstance(entityType);

            foreach (var property in entityType.GetProperties())
            {
                if (IsCollection(property) || IsNavigationProperty(property))
                    continue;

                var value = data[property.Name];
                if (value != null)
                {
                    try
                    {
                        var convertedValue = ConvertToPropertyType(property.PropertyType, value);
                        property.SetValue(entity, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Error mapping property {property.Name}", ex);
                    }
                }
                else if (property.PropertyType == typeof(DateTime) &&
                    (property.Name.Equals("OrderDate", StringComparison.OrdinalIgnoreCase) ||
                     property.Name.Equals("InvoiceDate", StringComparison.OrdinalIgnoreCase)))
                {
                    property.SetValue(entity, DateTime.UtcNow);
                }
            }

            return entity;
        }

        private async Task ProcessChildEntities(Type parentType, object parentEntity, JObject data)
        {
            foreach (var property in parentType.GetProperties().Where(IsCollection))
            {
                var childDataArray = data[property.Name] as JArray;
                if (childDataArray == null)
                    continue;

                var childEntityType = property.PropertyType.GetGenericArguments()[0];

                foreach (var childData in childDataArray)
                {
                    var childEntity = MapToEntity(childEntityType, (JObject)childData);
                    SetParentEntityReference(parentType, parentEntity, childEntityType, childEntity);
                    _context.Add(childEntity);
                    await ProcessChildEntities(childEntityType, childEntity, (JObject)childData);
                }
            }
        }

        private void SetParentEntityReference(Type parentType, object parentEntity, Type childEntityType, object childEntity)
        {
            var foreignKeyName = $"{parentType.Name}Id";
            var fkProperty = childEntityType.GetProperty(foreignKeyName);

            if (fkProperty != null)
            {
                var parentIdProperty = parentType.GetProperty($"{parentType.Name}Id") ?? parentType.GetProperty("Id");
                var parentIdValue = parentIdProperty.GetValue(parentEntity);
                fkProperty.SetValue(childEntity, parentIdValue);
            }
            else
            {
                var parentProperty = childEntityType.GetProperties()
                    .FirstOrDefault(p => p.PropertyType == parentType);

                if (parentProperty != null)
                {
                    parentProperty.SetValue(childEntity, parentEntity);
                }
                else
                {
                    throw new InvalidOperationException($"No foreign key or navigation property found for {childEntityType.Name} to {parentType.Name}");
                }
            }
        }

        private IQueryable IncludeChildEntities(IQueryable query, Type entityType)
        {
            foreach (var property in entityType.GetProperties().Where(IsCollection))
            {
                var includeMethod = typeof(EntityFrameworkQueryableExtensions)
                    .GetMethods()
                    .First(m => m.Name == "Include" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(entityType, property.PropertyType.GetGenericArguments()[0]);

                query = (IQueryable)includeMethod.Invoke(null, new object[] { query, property.Name });
            }

            return query;
        }


        private JObject MapEntityToJObject(Type entityType, object entity)
        {
            var result = new JObject();

            foreach (var property in entityType.GetProperties())
            {
                if (IsCollection(property))
                {
                    var collection = property.GetValue(entity) as IEnumerable<object>;
                    if (collection != null)
                    {
                        var childArray = new JArray();
                        foreach (var item in collection)
                        {
                            var childType = item.GetType();
                            var childObject = MapEntityToJObject(childType, item);
                            childArray.Add(childObject);
                        }
                        result[property.Name] = childArray;
                    }
                }
                else if (!IsNavigationProperty(property))
                {
                    var value = property.GetValue(entity);
                    result[property.Name] = value != null ? JToken.FromObject(value) : null;
                }
            }

            return result;
        }

        private bool IsCollection(PropertyInfo property)
        {
            return property.PropertyType != typeof(string) &&
                   typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) &&
                   property.PropertyType.IsGenericType &&
                   property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
        }

        private bool IsNavigationProperty(PropertyInfo property)
        {
            return property.PropertyType.IsClass &&
                   property.PropertyType != typeof(string) &&
                   !IsCollection(property) &&
                   !property.PropertyType.IsValueType;
        }

        private object ConvertToPropertyType(Type propertyType, JToken value)
        {
            if (propertyType.IsEnum)
                return Enum.Parse(propertyType, value.ToString());

            if (Nullable.GetUnderlyingType(propertyType) != null)
                return Convert.ChangeType(value, Nullable.GetUnderlyingType(propertyType));

            return Convert.ChangeType(value, propertyType);
        }

        private string BuildFilterString(JObject filters)
        {
            var filterList = new List<string>();

            foreach (var filter in filters)
            {
                var value = filter.Value.Type == JTokenType.String ? $"\"{filter.Value}\"" : filter.Value.ToString();
                filterList.Add($"{filter.Key} == {value}");
            }

            return string.Join(" AND ", filterList);
        }

        private void UpdateEntityProperties(Type entityType, object entity, JObject data)
        {
            foreach (var property in entityType.GetProperties())
            {
                if (IsCollection(property) || IsNavigationProperty(property))
                    continue;

                var value = data[property.Name];
                if (value != null)
                {
                    var convertedValue = ConvertToPropertyType(property.PropertyType, value);
                    property.SetValue(entity, convertedValue);
                }
            }
        }

        private async Task UpdateChildEntities(Type parentType, object parentEntity, JObject data)
        {
            foreach (var property in parentType.GetProperties().Where(IsCollection))
            {
                var childDataArray = data[property.Name] as JArray;
                if (childDataArray == null)
                    continue;

                var childEntityType = property.PropertyType.GetGenericArguments()[0];

                var setMethod = _context.GetType().GetMethod("Set", new Type[] { typeof(Type) });
                var childDbSet = setMethod.Invoke(_context, new object[] { childEntityType });

                var childEntities = (IEnumerable<object>)property.GetValue(parentEntity);
                var existingChildEntities = childEntities?.ToList() ?? new List<object>();

                foreach (var childData in childDataArray)
                {
                    await UpdateOrCreateChildEntity(childEntityType, childDbSet, existingChildEntities, (JObject)childData, parentType, parentEntity);
                }

                await DeleteRemovedChildEntities(childEntityType, existingChildEntities, childDataArray);
            }
        }


        private async Task UpdateOrCreateChildEntity(Type childEntityType, object childDbSet, List<object> existingChildEntities, JObject childData, Type parentType, object parentEntity)
        {
            var childIdProperty = childEntityType.GetProperty($"{childEntityType.Name}Id");
            var childIdValue = childData[childIdProperty.Name]?.Value<int>();

            object childEntity;

            if (childIdValue.HasValue && childIdValue.Value != 0)
            {
                childEntity = existingChildEntities.FirstOrDefault(e => (int)childIdProperty.GetValue(e) == childIdValue.Value);

                if (childEntity == null)
                    throw new InvalidOperationException($"Child entity with ID {childIdValue.Value} not found.");

                UpdateEntityProperties(childEntityType, childEntity, childData);
            }
            else
            {
                childEntity = MapToEntity(childEntityType, childData);
                SetParentEntityReference(parentType, parentEntity, childEntityType, childEntity);
                ((IList<object>)childDbSet).Add(childEntity);
            }

            await UpdateChildEntities(childEntityType, childEntity, childData);
        }

        private async Task DeleteRemovedChildEntities(Type childEntityType, List<object> existingChildEntities, JArray updatedChildData)
        {
            var childIdProperty = childEntityType.GetProperty($"{childEntityType.Name}Id");
            var updatedChildIds = updatedChildData
                .Select(cd => cd[childIdProperty.Name]?.Value<int>() ?? 0)
                .ToList();

            foreach (var existingChild in existingChildEntities)
            {
                var existingChildId = (int)childIdProperty.GetValue(existingChild);

                if (!updatedChildIds.Contains(existingChildId))
                {
                    _context.Remove(existingChild);
                }
            }
        }

        private async Task<object> GetEntityById(Type entityType, int id)
        {
            var setMethod = _context.GetType().GetMethod("Set", new Type[] { typeof(Type) });
            var dbSet = setMethod.Invoke(_context, new object[] { entityType });
            var keyProperty = entityType.GetProperty($"{entityType.Name}Id") ?? entityType.GetProperty("Id");

            if (keyProperty == null)
            {
                throw new InvalidOperationException($"No key property found for entity type {entityType.Name}");
            }

            var parameter = Expression.Parameter(entityType, "e");
            var propertyAccess = Expression.Property(parameter, keyProperty);
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(propertyAccess, constant);
            var lambda = Expression.Lambda(equality, parameter);

            var query = ((IQueryable)dbSet).Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new[] { entityType },
                    ((IQueryable)dbSet).Expression,
                    Expression.Quote(lambda)
                )
            );

            var firstOrDefaultAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
        .GetMethod("FirstOrDefaultAsync", new Type[] { typeof(IQueryable<>).MakeGenericType(entityType), typeof(CancellationToken) })
        .MakeGenericMethod(entityType);

            return await (Task<object>)firstOrDefaultAsyncMethod.Invoke(null, new object[] { query, CancellationToken.None });
        }


        private IQueryable GetQueryable(Type entityType)
        {
            return (IQueryable)_context.GetType()
                .GetMethod("Set", Type.EmptyTypes)
                .MakeGenericMethod(entityType)
                .Invoke(_context, null);
        }

       

        private int GetEntityId(object entity, string objectType)
        {
            var idProperty = entity.GetType().GetProperty($"{objectType}Id") ?? entity.GetType().GetProperty("Id");

            if (idProperty == null)
            {
                throw new InvalidOperationException($"No Id property found for entity type {entity.GetType().Name}");
            }

            var idValue = idProperty.GetValue(entity);

            if (idValue == null)
            {
                throw new InvalidOperationException($"Id value is null for entity type {entity.GetType().Name}");
            }

            return (int)idValue;
        }
    }
}