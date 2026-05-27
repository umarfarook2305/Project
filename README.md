# Layered Architecture .NET Project

This project demonstrates a clean **Layered Architecture** implementation in .NET 10.

## Architecture Overview

The solution is organized into four main layers:

```
LayeredArchitecture/
├── src/
│   ├── LayeredArchitecture.API/          # Presentation Layer
│   ├── LayeredArchitecture.Application/  # Application Layer
│   ├── LayeredArchitecture.Domain/       # Domain Layer
│   └── LayeredArchitecture.Infrastructure/ # Infrastructure Layer
└── LayeredArchitecture.slnx
```

### 1. **Domain Layer** (Core)
- **Purpose**: Contains business entities and core business logic
- **Dependencies**: None (most independent layer)
- **Contains**:
  - `Entities/`: Domain entities (Product)
  - `Interfaces/`: Repository interfaces (IProductRepository)

### 2. **Application Layer** (Business Logic)
- **Purpose**: Contains application-specific business rules and orchestrates domain objects
- **Dependencies**: Domain Layer
- **Contains**:
  - `DTOs/`: Data Transfer Objects
  - `Interfaces/`: Service interfaces (IProductService)
  - `Services/`: Business logic implementation (ProductService)

### 3. **Infrastructure Layer** (Data Access & External Services)
- **Purpose**: Implements interfaces defined in Domain/Application layers
- **Dependencies**: Domain Layer, Application Layer
- **Contains**:
  - `Repositories/`: Data access implementations (ProductRepository)
  - `Extensions/`: Dependency injection configuration

### 4. **Presentation Layer** (API)
- **Purpose**: Exposes application functionality via REST API
- **Dependencies**: Application Layer, Infrastructure Layer
- **Contains**:
  - `Controllers/`: API controllers (ProductsController)
  - `Program.cs`: Application configuration and startup

## Dependency Flow

```
API Layer
  ↓
Application Layer
  ↓
Domain Layer
  ↑
Infrastructure Layer
```

**Key Principle**: Dependencies point inward. The Domain layer has no dependencies, making it the most stable layer.

## Getting Started

### Prerequisites
- .NET 10 SDK or later

### Build the Solution
```bash
dotnet build
```

### Run the API
```bash
dotnet run --project src/LayeredArchitecture.API
```

The API will be available at `https://localhost:5001` (or the port specified in launchSettings.json)

### API Endpoints

#### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create a new product
- `PUT /api/products/{id}` - Update an existing product
- `DELETE /api/products/{id}` - Delete a product

### Example Request

**Create Product:**
```bash
curl -X POST https://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{
	"name": "Laptop",
	"description": "High-performance laptop",
	"price": 999.99,
	"stock": 10
  }'
```

## Benefits of Layered Architecture

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Maintainability**: Changes in one layer have minimal impact on others
3. **Testability**: Layers can be tested independently
4. **Flexibility**: Easy to swap implementations (e.g., change database)
5. **Reusability**: Business logic can be reused across different presentation layers

## Project Structure Details

### Domain Layer Components
- **Product.cs**: Core business entity
- **IProductRepository.cs**: Repository contract

### Application Layer Components
- **ProductDto.cs**: Data transfer objects for API communication
- **IProductService.cs**: Service contract
- **ProductService.cs**: Business logic implementation

### Infrastructure Layer Components
- **ProductRepository.cs**: In-memory data access (can be replaced with EF Core, Dapper, etc.)
- **ServiceCollectionExtensions.cs**: Dependency injection setup

### API Layer Components
- **ProductsController.cs**: REST API endpoints
- **Program.cs**: Application startup and middleware configuration

## Next Steps

### To Add Database Support (Entity Framework Core):
1. Add EF Core packages to Infrastructure layer
2. Create DbContext
3. Update ProductRepository to use DbContext
4. Configure connection string in appsettings.json

### To Add Authentication:
1. Add authentication middleware in Program.cs
2. Add `[Authorize]` attributes to controllers
3. Implement identity in Infrastructure layer

### To Add Validation:
1. Add FluentValidation package to Application layer
2. Create validators for DTOs
3. Register validators in DI container

## Design Patterns Used
- **Repository Pattern**: Abstracts data access
- **Dependency Injection**: Manages dependencies between layers
- **DTO Pattern**: Separates domain models from API contracts
- **Service Layer Pattern**: Encapsulates business logic

## License
This is a sample project for educational purposes.
