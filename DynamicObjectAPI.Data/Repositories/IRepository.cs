using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DynamicObjectAPI.Data.Repositories
{
    public interface IRepository
    {
        Task<int> CreateAsync(string objectType, JObject data);
        Task<JObject> GetByIdAsync(string objectType, int id);
        Task<IEnumerable<JObject>> GetAllAsync(string objectType, JObject filters = null);
        Task<bool> UpdateAsync(string objectType, int id, JObject data);
        Task<bool> DeleteAsync(string objectType, int id);
    }
}
