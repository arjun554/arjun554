# FoodDash - Food Delivery Management System

A comprehensive food delivery system built with .NET 8, featuring a Blazor WebAssembly web app, .NET MAUI mobile app, and microservices architecture.

## 📁 Project Structure

```
FoodDash/
├── src/
│   ├── FoodDash.Shared/                # Shared DTOs, Models, Enums
│   ├── FoodDash.Database/              # Entity Framework Core Models & Context
│   ├── FoodDash.API/                   # Web API with Controllers & Services
│   ├── FoodDash.WebApp/                # Blazor WebAssembly App
│   ├── FoodDash.MobileApp/             # .NET MAUI Blazor Hybrid App
│   └── Microservices/
│       ├── FoodDash.OrderService/      # Order Processing Microservice
│       ├── FoodDash.PaymentService/    # Payment Processing Microservice
│       └── FoodDash.NotificationService/ # Notification Microservice
├── tests/
│   ├── FoodDash.API.Tests/             # API Unit Tests
│   └── FoodDash.WebApp.Tests/          # Blazor Component Tests
└── docs/                               # Documentation
```

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code
- Node.js (for frontend dependencies)

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-repo/fooddash.git
   cd fooddash
   ```

2. **Database Setup**
   ```bash
   cd src/FoodDash.API
   dotnet ef database update
   ```

3. **Run the API**
   ```bash
   cd src/FoodDash.API
   dotnet run
   ```

4. **Run the Web App**
   ```bash
   cd src/FoodDash.WebApp
   dotnet run
   ```

5. **Run the Mobile App**
   ```bash
   cd src/FoodDash.MobileApp
   dotnet build -t:Run -f net8.0-android
   ```

## 🏗️ Architecture Overview

### Core Components

1. **FoodDash.Shared** - Common DTOs and Models
2. **FoodDash.Database** - Entity Framework Core with SQL Server
3. **FoodDash.API** - REST API with JWT authentication
4. **FoodDash.WebApp** - Blazor WebAssembly client
5. **FoodDash.MobileApp** - MAUI Blazor Hybrid app

### Key Features

- **Multi-role Authentication**: Customer, Restaurant Owner, Delivery Partner, Admin
- **Real-time Updates**: SignalR for order tracking and notifications
- **Payment Integration**: ESewa, IME Pay, Cash on Delivery
- **Multi-language Support**: English, Nepali, Tharu
- **Geolocation**: Google Maps integration for delivery tracking
- **Responsive Design**: Mobile-first approach

## 🔐 Authentication & Authorization

### JWT Configuration
- **Token Expiration**: 24 hours
- **Refresh Token**: Supported
- **Role-based Access**: Different permissions for each user role

### User Roles
1. **Customer**: Browse restaurants, place orders, track deliveries
2. **Restaurant Owner**: Manage restaurant, menu, and orders
3. **Delivery Partner**: Accept deliveries, update delivery status
4. **Admin**: Manage all users, restaurants, and system settings

## 📊 Database Schema

### Core Entities

- **Users**: Identity-based user management
- **Restaurants**: Restaurant information and settings
- **MenuCategories & MenuItems**: Restaurant menu structure
- **Orders & OrderItems**: Order processing and tracking
- **Customers**: Customer-specific data and loyalty points
- **DeliveryPartners**: Delivery partner information and earnings
- **Payments**: Payment tracking and transaction logs
- **Reviews**: Restaurant and menu item reviews

### Relationships
- One-to-Many: Restaurant → MenuItems, User → Orders
- Many-to-Many: Orders ↔ MenuItems (through OrderItems)
- One-to-One: User → Customer/DeliveryPartner

## 🌐 API Endpoints

### Authentication Endpoints
```
POST /api/auth/login
POST /api/auth/register
POST /api/auth/change-password
POST /api/auth/forgot-password
POST /api/auth/reset-password
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me
```

### Restaurant Endpoints
```
GET    /api/restaurants
GET    /api/restaurants/{id}
POST   /api/restaurants
PUT    /api/restaurants/{id}
DELETE /api/restaurants/{id}
GET    /api/restaurants/owner/{ownerId}
```

### Menu Endpoints
```
GET    /api/menu/restaurant/{restaurantId}
GET    /api/menu/item/{id}
POST   /api/menu/item
PUT    /api/menu/item/{id}
DELETE /api/menu/item/{id}
```

### Order Endpoints
```
GET    /api/orders
GET    /api/orders/{id}
POST   /api/orders
PUT    /api/orders/{id}/status
GET    /api/orders/customer/{customerId}
GET    /api/orders/restaurant/{restaurantId}
```

## 🔧 Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FoodDashDb;..."
  },
  "JWT": {
    "Key": "your-secret-key",
    "Issuer": "FoodDashAPI",
    "Audience": "FoodDashUsers"
  },
  "ExternalServices": {
    "GoogleMapsApiKey": "your-api-key",
    "ESewa": { ... },
    "IMEPay": { ... }
  }
}
```

## 📱 Mobile App Features

### Platform Support
- **Android**: API 21+ (Android 5.0)
- **iOS**: iOS 11.0+

### Key Features
- Native performance with .NET MAUI
- Shared UI components with Blazor
- Offline capabilities
- Push notifications
- Camera integration for profile pictures
- GPS tracking for delivery

## 🧪 Testing

### Unit Tests
```bash
cd tests/FoodDash.API.Tests
dotnet test
```

### Integration Tests
```bash
cd tests/FoodDash.WebApp.Tests
dotnet test
```

### Test Coverage
- API Controllers and Services
- Blazor Components
- Business Logic
- Database Operations

## 🌍 Localization

### Supported Languages
- **English** (default)
- **Nepali** (नेपाली)
- **Tharu** (थारू)

### Resource Files
- `Resources/en.json`
- `Resources/ne.json`
- `Resources/th.json`

## 🚀 Deployment

### Development Environment
```bash
dotnet run --environment Development
```

### Production Environment
```bash
dotnet publish -c Release
```

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "FoodDash.API.dll"]
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow
- Automated testing on pull requests
- Build verification
- Deployment to staging environment
- Production deployment on main branch

## 📈 Performance Considerations

### Optimization Strategies
- Entity Framework query optimization
- Response caching
- Image compression and CDN usage
- SignalR connection management
- Database indexing

### Monitoring
- Application Insights integration
- Performance metrics tracking
- Error logging with Serilog
- Health checks

## 🤝 Contributing

### Development Guidelines
1. Follow clean architecture principles
2. Write unit tests for new features
3. Use meaningful commit messages
4. Follow code review process
5. Update documentation

### Code Standards
- Use C# coding conventions
- Follow SOLID principles
- Implement proper error handling
- Use async/await patterns

## 📞 Support

### Documentation
- API documentation available at `/swagger`
- Postman collection included
- Component documentation in Storybook

### Contact
- **Client**: Ajay Singh Thakur
- **Location**: Gulariya, Bardiya, Nepal
- **Project Date**: February 16, 2025

## 🛣️ Roadmap

### Phase 1 (Current)
- ✅ Basic authentication and user management
- ✅ Restaurant and menu management
- ✅ Order processing
- ✅ Payment integration setup

### Phase 2 (Coming Soon)
- 🔲 Advanced order tracking
- 🔲 Delivery partner app features
- 🔲 Analytics dashboard
- 🔲 Loyalty program implementation

### Phase 3 (Future)
- 🔲 AI-powered recommendations
- 🔲 Farm-to-table partnerships
- 🔲 NGO meal planning integration
- 🔲 Advanced reporting features

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

*FoodDash - Bringing delicious food to your doorstep with cutting-edge technology.*