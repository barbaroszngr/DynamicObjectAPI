using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicObjectAPI.Common.DTOs;
using DynamicObjectAPI.Domain.Entities;

namespace DynamicObjectAPI.Services.Interfaces
{
    public interface IObjectService
    {
        Task<Guid> CreateObjectAsync(CreateObjectRequest request);
        Task<object> GetObjectAsync(string objectType, Guid id);
        Task<IEnumerable<object>> GetObjectsAsync(string objectType, Dictionary<string, string> filters);
        Task UpdateObjectAsync(string objectType, Guid id, string data);
        Task DeleteObjectAsync(string objectType, Guid id);
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId);
    }
}