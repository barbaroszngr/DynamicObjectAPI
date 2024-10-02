using DynamicObjectAPI.Common.DTOs;
using DynamicObjectAPI.Common.Exceptions;
using DynamicObjectAPI.Data.Repositories;
using DynamicObjectAPI.Services.Interfaces;
using DynamicObjectAPI.Services.Validators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using DynamicObjectAPI.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Services
{
    public class DynamicObjectService : IDynamicObjectService
    {
        private readonly IRepository _repository;
        private readonly IValidatorFactory _validatorFactory;
        private readonly ILogger<DynamicObjectService> _logger;

        public DynamicObjectService(IRepository repository, IValidatorFactory validatorFactory, ILogger<DynamicObjectService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> CreateAsync(string objectType, JObject data)
        {
            try
            {
                await ValidateDataAsync(objectType, data);
                return await _repository.CreateAsync(objectType, data);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, $"Validation failed for {objectType} creation");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while creating {objectType}");
                throw new ServiceException($"Failed to create {objectType}", ex);
            }
        }

        public async Task<DynamicResponse> GetByIdAsync(string objectType, int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(objectType, id);
                if (result == null)
                    throw new NotFoundException(objectType, id);

                return new DynamicResponse { Data = result };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting {objectType} with id {id}");
                throw new ServiceException($"Failed to get {objectType} with id {id}", ex);
            }
        }

        public async Task<DynamicListResponse> GetAllAsync(string objectType, JObject filters = null)
        {
            try
            {
                var results = await _repository.GetAllAsync(objectType, filters);
                return new DynamicListResponse { Data = results };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting all {objectType}");
                throw new ServiceException($"Failed to get all {objectType}", ex);
            }
        }

        public async Task<bool> UpdateAsync(string objectType, int id, JObject data)
        {
            try
            {
                await ValidateDataAsync(objectType, data);
                return await _repository.UpdateAsync(objectType, id, data);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, $"Validation failed for {objectType} update");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating {objectType} with id {id}");
                throw new ServiceException($"Failed to update {objectType} with id {id}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string objectType, int id)
        {
            try
            {
                return await _repository.DeleteAsync(objectType, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting {objectType} with id {id}");
                throw new ServiceException($"Failed to delete {objectType} with id {id}", ex);
            }
        }

        private async Task ValidateDataAsync(string objectType, JObject data)
        {
            var validator = _validatorFactory.GetValidator(objectType);
            if (validator == null)
            {
                _logger.LogWarning($"No validator found for object type: {objectType}");
                throw new InvalidOperationException($"No validator found for object type: {objectType}");
            }

            var validationResult = await validator.ValidateAsync(data);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors.ConvertAll(error => error.ErrorMessage));
            }
        }
    }
}