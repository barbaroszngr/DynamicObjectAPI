# DynamicObjectAPI

## Overview

DynamicObjectAPI is a backend system that stores various objects (like products, orders, customers, etc.) in a single flexible table and manages them through a central gateway for all CRUD operations (Create, Read, Update, Delete). This innovative project provides a powerful API, enabling users to easily create new objects and manage complex transactions involving multiple related objects efficiently.

## Key Features 

### 1. Dynamic Object Creation
- Flexible Object Creation: Users can create new objects (e.g., orders, products, customers) and fields through API requests. The system stores these objects in a central dynamic table without the need for physical table creation, making it highly flexible.
- Dynamic Structures: The structure and fields for each object are dynamic and can vary depending on the object type. For instance, products may have different fields than customers, allowing each object type to be defined and modified as needed.

### 2. CRUD Operations
- Unified API Interface: A single interface manages all Create, Read, Update, and Delete operations for various object types. This API can accept dynamic fields based on the object type, offering a uniform way to manage different objects.
- CRUD Functionalities:
