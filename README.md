# DynamicObjectAPI

## Overview

DynamicObjectAPI is a backend system that stores various objects (like products, orders, customers, etc.) in a single flexible table and manages them through a central gateway for all CRUD operations (Create, Read, Update, Delete). This innovative project provides a powerful API, enabling users to easily create new objects and manage complex transactions involving multiple related objects efficiently.

## Key Features 

### 1. Dynamic Object Creation
- **Flexible Object Creation:** Users can create new objects (e.g., orders, products, customers) and fields through API requests. The system stores these objects in a central dynamic table without the need for physical table creation, making it highly flexible.
- **Dynamic Structures:** The structure and fields for each object are dynamic and can vary depending on the object type. For instance, products may have different fields than customers, allowing each object type to be defined and modified as needed.

### 2. CRUD Operations
- **Unified API Interface:** A single interface manages all Create, Read, Update, and Delete operations for various object types. This API can accept dynamic fields based on the object type, offering a uniform way to manage different objects.
- **CRUD Functionalities:**
    - Create: Adds new records for dynamic objects with varying structures.
    - Read: Retrieves data based on object type and ID, with support for optional filters (e.g., fetching all orders placed by a specific customer).
    - Update: Modifies existing records, adapting to the dynamic structure of each object.
    - Delete: Removes objects and, if applicable, their related sub-objects.

### 3. Transaction Management
- **Atomic Transactions:** The system manages the creation of master objects (e.g., an order) along with their sub-objects (e.g., order products) in a single transaction. This approach ensures that either all objects are created successfully, or none are created if an error occurs, maintaining data consistency.
- **Complex Transactions:** Supports multi-object transactions, allowing multiple related objects to be created or updated together within a single request.

### 4. Error Handling
- **Error Management:** Handles errors such as invalid object structures, missing fields, database connection issues, and validation failures. The API provides clear and meaningful error messages to the consumer, helping them understand the reasons for any failures.

### 5. Dynamic Data Validation
- **Context-Sensitive Validation:** Validates required fields dynamically based on the object type. For example, an order requires a customer ID and at least one product, while a product requires a name and price. The validation logic is adaptable and context-sensitive, ensuring data integrity.
- Custom Validation Rules: Specific rules are enforced per object type, which can be easily extended or modified based on business needs.

## Technology Stack
- **.NET 6 Web API:** The backend system is built using .NET 6, providing modern, scalable, and performant API services.
- **PostgreSQL:** The database is managed by PostgreSQL, offering strong support for handling dynamic data structures.
- **Entity Framework Core:** EF Core is used as the Object-Relational Mapper (ORM) to handle database operations efficiently.

## Project Structure
The project is organized into modular layers to ensure maintainability, scalability, and reduce dependencies between components. This approach follows best practices, allowing for easier updates, testing, and future enhancements while keeping the codebase clean and manageable.

### DynamicObjectAPI.API
- **Controllers:** Handle incoming HTTP requests, routing them to the appropriate services for processing.
- **Middlewares:** Implement custom middleware for processing requests and responses, including error handling and validation.

### DynamicObjectAPI.Common
- **DTOs:** Data Transfer Objects are used for standardizing API requests and responses.
- **Exceptions:** Custom exceptions are defined for handling specific error scenarios within the API.
- **Validations:** Dynamic validation logic is applied for different object types, ensuring that each request meets the required criteria.

### DynamicObjectAPI.Data
- **Repositories and Context:** This layer is responsible for managing all database-related operations, including context setup, migrations, and data access. It uses the generic repository pattern, which defines standard CRUD operations for dynamic objects and handles data persistence efficiently. By centralizing all database interactions within this layer, dependencies on the database are minimized for other modules, enhancing maintainability. The context object is configured here, along with migrations to manage schema changes. The repository pattern ensures that all interactions with the database are streamlined and consistent, reducing the direct dependency of other modules on the database.

### DynamicObjectAPI.Domain
- This layer defines the data structures for the central dynamic table, which is designed to accommodate various object types like products, orders, and customers in a unified format. By using a flexible schema, it allows different types of objects to coexist within a single table, adapting to the dynamic nature of the data. This approach eliminates the need for creating multiple tables for each object type, making the system more adaptable and easier to maintain. The entities are structured to handle varying fields and object relationships, ensuring that the data remains organized and consistent, regardless of the object type being stored or modified.

### DynamicObjectAPI.Services
- The Services layer acts as a bridge between the database operations handled by the repository and the API layer, managing all the business logic in one place. It defines the necessary methods for manipulating objects, such as creating, updating, and managing transactions, through interfaces like ```IObjectService```. The implementation, ObjectService, handles the core logic for these operations, ensuring that all the rules and processes are followed correctly. This setup allows the service layer to coordinate complex tasks, manage data consistency, and keep the code organized and clean by separating the business logic from the direct data access.







