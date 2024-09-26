# DynamicObject

## Overview

DynamicObjectAPI is a backend system designed to store all objects (such as products, orders, customers, etc.) in a single dynamic table and manage them through a central gateway for all CRUD operations. This project showcases a robust API that enables users to dynamically create new objects and manage transactions involving multiple related objects.

## Key Features 

### 1. Dynamic Object Creation
- Users can create new objects (e.g., orders and order products) and fields via API requests.
- The system stores soft objects in a central table, eliminating the need for physical table creation.
