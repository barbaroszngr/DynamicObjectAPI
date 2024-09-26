using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using DynamicObjectAPI.Data.Repositories;
using DynamicObjectAPI.Common.DTOs;
using DynamicObjectAPI.Domain.Entities;
using DynamicObjectAPI.Services.Interfaces;
using DynamicObjectAPI.Common.Exceptions;
using DynamicObjectAPI.Common.Validations;
using Microsoft.EntityFrameworkCore;
using DynamicObjectAPI.Data;


namespace DynamicObjectAPI.Services.Implementations
{
    public class ObjectService : IObjectService
    {
        private readonly IRepository<DynamicObject> _repository;
        private readonly IValidationService _validationService;
        private readonly ApplicationDbContext _context;

        public ObjectService(IRepository<DynamicObject> repository, IValidationService validationService, ApplicationDbContext context)
        {
            _repository = repository;
            _validationService = validationService;
            _context = context;
        }

        public async Task<Guid> CreateObjectAsync(CreateObjectRequest request)
        {
            
            await _validationService.ValidateAsync(request.ObjectType, request.Data);

            
            if (request.ObjectType.Equals("Order", StringComparison.OrdinalIgnoreCase))
            {
                return await CreateOrderAsync(request);
            }

            
            var dynamicObject = new DynamicObject
            {
                Id = Guid.NewGuid(),
                ObjectType = request.ObjectType,
                Data = request.Data,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(dynamicObject);
            await _repository.SaveChangesAsync();

            return dynamicObject.Id;
        }

        public async Task<object> GetObjectAsync(string objectType, Guid id)
        {
            var dynamicObject = await _repository.GetAll(d => d.Id == id && d.ObjectType == objectType)
                                                 .FirstOrDefaultAsync();

            if (dynamicObject == null)
                throw new NotFoundException($"{objectType} with Id {id} not found.");

            var data = JsonDocument.Parse(dynamicObject.Data).RootElement;
            return data;
        }

        public async Task<IEnumerable<object>> GetObjectsAsync(string objectType, Dictionary<string, string> filters)
        {
            
            var results = await _repository.GetAll(d => d.ObjectType == objectType).ToListAsync();

            
            var filteredResults = results.Where(r =>
            {
                var jsonData = JsonDocument.Parse(r.Data).RootElement;

                foreach (var filter in filters)
                {
                    var key = filter.Key;
                    var value = filter.Value;

                    if (!jsonData.TryGetProperty(key, out var jsonValue))
                    {
                        return false;
                    }

                    if (jsonValue.ValueKind == JsonValueKind.String)
                    {
                        if (!string.Equals(jsonValue.GetString(), value, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (jsonValue.ToString() != value)
                        {
                            return false;
                        }
                    }
                }

                return true;
            });

            
            return filteredResults.Select(r => (object)JsonDocument.Parse(r.Data).RootElement);
        }

        public async Task UpdateObjectAsync(string objectType, Guid id, string data)
        {
            
            await _validationService.ValidateAsync(objectType, data);

            var dynamicObject = await _repository.GetAll(d => d.Id == id && d.ObjectType == objectType)
                                                 .FirstOrDefaultAsync();

            if (dynamicObject == null)
                throw new NotFoundException($"{objectType} with Id {id} not found.");

            dynamicObject.Data = data;
            dynamicObject.UpdatedAt = DateTime.UtcNow;

            _repository.Update(dynamicObject);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteObjectAsync(string objectType, Guid id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var dynamicObject = await _repository.GetAll(d => d.Id == id && d.ObjectType == objectType)
                                                         .FirstOrDefaultAsync();

                    if (dynamicObject == null)
                        throw new NotFoundException($"{objectType} with Id {id} not found.");

                    
                    var relatedObjects = await _repository.GetAll(d => d.ParentId == id).ToListAsync();

                    foreach (var relatedObject in relatedObjects)
                    {
                        _repository.Remove(relatedObject);
                    }

                    _repository.Remove(dynamicObject);
                    await _repository.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }


        public async Task<Guid> CreateOrderAsync(CreateObjectRequest request)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var orderDataElement = JsonDocument.Parse(request.Data).RootElement;

                    
                    if (!orderDataElement.TryGetProperty("customerId", out var customerIdElement) ||
                        !Guid.TryParse(customerIdElement.GetString(), out var customerId))
                    {
                        throw new ValidationException("Order için geçerli bir 'customerId' gereklidir.");
                    }

                    
                    var order = new DynamicObject
                    {
                        Id = Guid.NewGuid(),
                        ObjectType = "Order",
                        Data = request.Data,
                        CreatedAt = DateTime.UtcNow,
                        CustomerId = customerId 
                    };

                    await _repository.AddAsync(order);
                    await _repository.SaveChangesAsync();

                    var orderId = order.Id;

                    var products = orderDataElement.GetProperty("products");

                    foreach (var product in products.EnumerateArray())
                    {
                        var productDict = JsonSerializer.Deserialize<Dictionary<string, object>>(product.GetRawText());

                        var orderProduct = new DynamicObject
                        {
                            Id = Guid.NewGuid(),
                            ObjectType = "OrderProduct",
                            Data = JsonSerializer.Serialize(productDict),
                            ParentId = orderId,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _repository.AddAsync(orderProduct);
                    }

                    await _repository.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return orderId;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId)
        {
            
            var customerExists = await _repository.GetAll(c => c.ObjectType.Equals("Customer", StringComparison.OrdinalIgnoreCase) && c.Id == customerId)
                                                  .AnyAsync();

            if (!customerExists)
            {
                throw new NotFoundException($"Customer with ID {customerId} does not exist.");
            }

            
            var orders = await _repository.GetAll(o => o.ObjectType.Equals("Order", StringComparison.OrdinalIgnoreCase) && o.CustomerId == customerId)
                                          .ToListAsync();
            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId.Value,
                CreatedAt = o.CreatedAt,
                Data = o.Data
            });
        }

    }
}