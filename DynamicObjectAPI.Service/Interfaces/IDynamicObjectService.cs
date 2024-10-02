using DynamicObjectAPI.Common.DTOs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Services.Interfaces
{
    public interface IDynamicObjectService
    {
        Task<int> CreateAsync(string objectType, JObject data);
        Task<DynamicResponse> GetByIdAsync(string objectType, int id);
        Task<DynamicListResponse> GetAllAsync(string objectType, JObject filters = null);
        Task<bool> UpdateAsync(string objectType, int id, JObject data);
        Task<bool> DeleteAsync(string objectType, int id);
    }
}

