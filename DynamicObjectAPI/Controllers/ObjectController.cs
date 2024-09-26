using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicObjectAPI.Common.DTOs;
using DynamicObjectAPI.Services.Interfaces;

namespace DynamicObjectAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IObjectService _objectService;

        public ObjectController(IObjectService objectService)
        {
            _objectService = objectService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateObject([FromBody] CreateObjectRequest request)
        {
            var id = await _objectService.CreateObjectAsync(request);
            return Ok(new { Id = id });
        }

        [HttpGet("{objectType}/{id}")]
        public async Task<IActionResult> GetObject(string objectType, Guid id)
        {
            var obj = await _objectService.GetObjectAsync(objectType, id);
            return Ok(obj);
        }

        [HttpGet("{objectType}")]
        public async Task<IActionResult> GetObjects(string objectType, [FromQuery] Dictionary<string, string> filters)
        {
            var objects = await _objectService.GetObjectsAsync(objectType, filters);
            return Ok(objects);
        }
        [HttpGet("orders/customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomerId(Guid customerId)
        {
            var orders = await _objectService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }

        [HttpPut("{objectType}/{id}")]
        public async Task<IActionResult> UpdateObject(string objectType, Guid id, [FromBody] UpdateObjectRequest request)
        {
            await _objectService.UpdateObjectAsync(objectType, id, request.Data);
            return NoContent();
        }

        [HttpDelete("{objectType}/{id}")]
        public async Task<IActionResult> DeleteObject(string objectType, Guid id)
        {
            await _objectService.DeleteObjectAsync(objectType, id);
            return NoContent();
        }
    }
}