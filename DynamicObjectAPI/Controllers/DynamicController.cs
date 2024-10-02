using DynamicObjectAPI.Common.DTOs;
using DynamicObjectAPI.Common.Exceptions;
using DynamicObjectAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace DynamicObjectAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DynamicController : ControllerBase
    {
        private readonly IDynamicObjectService _service;
        private readonly ILogger<DynamicController> _logger;

        public DynamicController(IDynamicObjectService service, ILogger<DynamicController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DynamicCreateRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ObjectType) || request.Data == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var id = await _service.CreateAsync(request.ObjectType, request.Data);
                return Ok(new { Id = id });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating object");
                return StatusCode(500, new { error = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpGet("{objectType}/{id}")]
        public async Task<IActionResult> GetById(string objectType, int id)
        {
            try
            {
                var response = await _service.GetByIdAsync(objectType, id);
                if (response == null)
                    return NotFound($"{objectType} with id {id} not found");

                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting {objectType} with id {id}");
                return StatusCode(500, new { error = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpGet("{objectType}")]
        public async Task<IActionResult> GetAll(string objectType, [FromQuery] string filters)
        {
            try
            {
                JObject filterObj = null;
                if (!string.IsNullOrEmpty(filters))
                {
                    filterObj = JObject.Parse(filters);
                }

                var response = await _service.GetAllAsync(objectType, filterObj);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting all {objectType}");
                return StatusCode(500, new { error = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] DynamicUpdateRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ObjectType) || request.Data == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var success = await _service.UpdateAsync(request.ObjectType, request.Id, request.Data);
                if (!success)
                {
                    return NotFound($"{request.ObjectType} with id {request.Id} not found");
                }

                return Ok();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating {request.ObjectType} with id {request.Id}");
                return StatusCode(500, new { error = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpDelete("{objectType}/{id}")]
        public async Task<IActionResult> Delete(string objectType, int id)
        {
            try
            {
                var result = await _service.DeleteAsync(objectType, id);
                if (result)
                {
                    _logger.LogInformation($"Successfully deleted {objectType} with id {id}");
                    return NoContent();
                }
                else
                {
                    return NotFound($"{objectType} with id {id} not found");
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, $"Error occurred while deleting {objectType} with id {id}");
            //    return StatusCode(500, new { error = "An unexpected error occurred. Please try again later." });
            //}
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating object");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}