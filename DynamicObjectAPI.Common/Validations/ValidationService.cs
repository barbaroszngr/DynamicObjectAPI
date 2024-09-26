using System;
using System.Text.Json;
using System.Threading.Tasks;
using DynamicObjectAPI.Data.Repositories;
using DynamicObjectAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using DynamicObjectAPI.Common.Exceptions;

namespace DynamicObjectAPI.Common.Validations
{
    public class ValidationService : IValidationService
    {
        private readonly IRepository<DynamicObject> _repository;

        public ValidationService(IRepository<DynamicObject> repository)
        {
            _repository = repository;
        }

        public async Task ValidateAsync(string objectType, string data)
        {
            var jsonData = JsonDocument.Parse(data).RootElement;

            
            switch (objectType.ToLower())
            {
                case "product":
                    await ValidateProductAsync(jsonData);
                    break;
                case "customer":
                    await ValidateCustomerAsync(jsonData);
                    break;
                case "order":
                    await ValidateOrderAsync(jsonData);
                    break;
                case "orderproduct":
                    await ValidateOrderProductAsync(jsonData);
                    break;
                default:
                    throw new ValidationException("Geçersiz obje türü.");
            }
        }

        private Task ValidateProductAsync(JsonElement data)
        {
            if (!data.TryGetProperty("name", out var name) || string.IsNullOrEmpty(name.GetString()))
                throw new ValidationException("Product için 'name' alanı gereklidir.");

            if (!data.TryGetProperty("price", out var price) || price.GetDecimal() < 0)
                throw new ValidationException("Product için 'price' alanı sıfır veya pozitif olmalıdır.");

            return Task.CompletedTask;
        }

        private Task ValidateCustomerAsync(JsonElement data)
        {
            if (!data.TryGetProperty("firstName", out var firstName) || string.IsNullOrEmpty(firstName.GetString()))
                throw new ValidationException("Customer için 'firstName' alanı gereklidir.");

            if (!data.TryGetProperty("lastName", out var lastName) || string.IsNullOrEmpty(lastName.GetString()))
                throw new ValidationException("Customer için 'lastName' alanı gereklidir.");

            if (!data.TryGetProperty("email", out var email) || string.IsNullOrEmpty(email.GetString()))
                throw new ValidationException("Customer için 'email' alanı gereklidir.");

            

            return Task.CompletedTask;
        }

        private async Task ValidateOrderAsync(JsonElement data)
        {
            if (!data.TryGetProperty("customerId", out var customerIdElement) || !Guid.TryParse(customerIdElement.GetString(), out var customerId))
                throw new ValidationException("Order için geçerli bir 'customerId' gereklidir.");

            
            var customerExists = await _repository.GetAll(d => d.ObjectType == "Customer" && d.Id == customerId)
                                                  .AnyAsync();

            if (!customerExists)
                throw new ValidationException($"CustomerId {customerId} mevcut değil.");

            if (!data.TryGetProperty("products", out var products) || products.ValueKind != JsonValueKind.Array || products.GetArrayLength() == 0)
                throw new ValidationException("Order için 'products' alanı en az bir ürün içermelidir.");

            
            foreach (var product in products.EnumerateArray())
            {
                await ValidateOrderProductAsync(product);
            }
        }

        private async Task ValidateOrderProductAsync(JsonElement data)
        {
            if (!data.TryGetProperty("productId", out var productIdElement) || !Guid.TryParse(productIdElement.GetString(), out var productId))
                throw new ValidationException("OrderProduct için geçerli bir 'productId' gereklidir.");

            
            var productExists = await _repository.GetAll(d => d.ObjectType == "Product" && d.Id == productId)
                                                 .AnyAsync();

            if (!productExists)
                throw new ValidationException($"ProductId {productId} mevcut değil.");

            if (!data.TryGetProperty("quantity", out var quantity) || quantity.GetInt32() <= 0)
                throw new ValidationException("OrderProduct için 'quantity' sıfırdan büyük olmalıdır.");
        }
    }
}
