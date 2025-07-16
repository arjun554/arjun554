# FoodDash - New Architecture Overview

## 🏗️ Architecture Transformation

The FoodDash project has been completely restructured to provide separate portals for different user types and convert all REST API communications to high-performance gRPC services.

## 📱 Application Structure

### **Web Portals (Blazor Server)**
1. **Admin Portal** (`FoodDash.AdminPortal`)
   - Complete admin dashboard
   - User management
   - Restaurant approval
   - System analytics
   - Order monitoring

2. **Restaurant Portal** (`FoodDash.RestaurantPortal`)
   - Restaurant management dashboard
   - Menu management
   - Order processing
   - Analytics and reports

3. **Customer Portal** (`FoodDash.CustomerPortal`)
   - Browse restaurants
   - Place orders
   - Order tracking
   - Profile management

### **Mobile Applications (.NET MAUI)**
1. **Customer App** (`FoodDash.CustomerApp`)
   - Native mobile experience for customers
   - Restaurant browsing and ordering
   - Real-time order tracking
   - Push notifications

2. **Delivery App** (`FoodDash.DeliveryApp`)
   - Delivery partner mobile app
   - Order pickup and delivery management
   - GPS tracking integration
   - Earnings tracking

3. **Restaurant App** (`FoodDash.RestaurantApp`)
   - Restaurant mobile management
   - Order notifications
   - Menu quick updates
   - Staff coordination

## 🚀 gRPC Services Architecture

### **Authentication Service** (`FoodDash.AuthService`)
**Port**: 5001
**Proto**: `auth.proto`

**Available Methods**:
- `Login` - User authentication
- `Register` - User registration
- `ChangePassword` - Password management
- `ForgotPassword` - Password recovery
- `ResetPassword` - Password reset
- `RefreshToken` - Token refresh
- `GetProfile` - User profile retrieval
- `UpdateProfile` - Profile updates

**Example Usage**:
```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new AuthService.AuthServiceClient(channel);

var loginRequest = new LoginRequest
{
    Email = "user@example.com",
    Password = "password123"
};

var response = await client.LoginAsync(loginRequest);
if (response.Success)
{
    var token = response.Token;
    var user = response.User;
}
```

### **Restaurant Service** (`FoodDash.RestaurantService`)
**Port**: 5002
**Proto**: `restaurant.proto`

**Available Methods**:
- `GetRestaurants` - Paginated restaurant listing
- `GetRestaurantById` - Single restaurant details
- `CreateRestaurant` - Restaurant creation
- `UpdateRestaurant` - Restaurant updates
- `DeleteRestaurant` - Restaurant deletion
- `GetMyRestaurant` - Owner's restaurant
- `UpdateRestaurantStatus` - Status management

### **Menu Service** (`FoodDash.MenuService`)
**Port**: 5003
**Proto**: `menu.proto`

**Available Methods**:
- `GetMenuByRestaurant` - Restaurant menu retrieval
- `GetMenuItem` - Single menu item details
- `CreateCategory` - Menu category creation
- `UpdateCategory` - Category updates
- `DeleteCategory` - Category deletion
- `CreateMenuItem` - Menu item creation
- `UpdateMenuItem` - Item updates
- `DeleteMenuItem` - Item deletion
- `UpdateMenuItemAvailability` - Availability toggle

### **Order Service** (`FoodDash.OrderService`)
**Port**: 5004
**Proto**: `orders.proto`

**Available Methods**:
- `CreateOrder` - Order placement
- `GetOrderById` - Order details
- `GetMyOrders` - User order history
- `UpdateOrderStatus` - Status updates
- `CancelOrder` - Order cancellation
- `GetRestaurantOrders` - Restaurant orders

### **Payment Service** (`FoodDash.PaymentService`)
**Port**: 5005
**Proto**: `payments.proto`

**Available Methods**:
- `ProcessPayment` - Payment processing
- `GetPaymentStatus` - Payment status check
- `RefundPayment` - Payment refunds
- `GetPaymentHistory` - Payment history

### **Notification Service** (`FoodDash.NotificationService`)
**Port**: 5006
**Proto**: `notifications.proto`

**Available Methods**:
- `SendNotification` - Send notifications
- `SendOrderStatusUpdate` - Order status notifications
- `SendPromotionalNotification` - Marketing notifications
- `GetNotificationHistory` - Notification history
- `MarkAsRead` - Mark notifications as read
- `GetUnreadCount` - Unread notification count

## 🔧 Service Discovery with Consul

All gRPC services register with Consul for service discovery:

```csharp
// Service Registration Example
var registration = new AgentServiceRegistration()
{
    ID = $"auth-service-{Environment.MachineName}",
    Name = "auth-service",
    Address = "localhost",
    Port = 5001,
    Tags = new[] { "grpc", "auth" },
    Check = new AgentServiceCheck()
    {
        HTTP = "http://localhost:5001/health",
        Timeout = TimeSpan.FromSeconds(10),
        Interval = TimeSpan.FromSeconds(30)
    }
};

await consulClient.Agent.ServiceRegister(registration);
```

## 📊 Performance Benefits

### **gRPC vs REST API**
- **Speed**: 7-10x faster than JSON REST APIs
- **Payload Size**: 30-50% smaller messages
- **Type Safety**: Compile-time type checking
- **Streaming**: Real-time bidirectional communication
- **Protocol Buffers**: Efficient binary serialization

### **Separation Benefits**
- **Scalability**: Independent scaling per service
- **Maintenance**: Focused development teams
- **Security**: Service-specific security policies
- **Deployment**: Independent deployment cycles

## 🚀 Getting Started

### **Prerequisites**
```bash
# Install required tools
dotnet tool install --global dotnet-ef
dotnet add package Grpc.AspNetCore
dotnet add package Consul
```

### **Running Services**

1. **Start Consul** (Service Discovery):
```bash
consul agent -dev
```

2. **Start gRPC Services**:
```bash
# Auth Service
cd src/GrpcServices/FoodDash.AuthService
dotnet run

# Restaurant Service
cd src/GrpcServices/FoodDash.RestaurantService
dotnet run

# Menu Service
cd src/GrpcServices/FoodDash.MenuService
dotnet run
```

3. **Start Web Portals**:
```bash
# Admin Portal
cd src/WebPortals/FoodDash.AdminPortal
dotnet run

# Restaurant Portal
cd src/WebPortals/FoodDash.RestaurantPortal
dotnet run

# Customer Portal
cd src/WebPortals/FoodDash.CustomerPortal
dotnet run
```

4. **Run Mobile Apps**:
```bash
# Customer App
cd src/MobileApps/FoodDash.CustomerApp
dotnet build -f net8.0-android

# Delivery App
cd src/MobileApps/FoodDash.DeliveryApp
dotnet build -f net8.0-android

# Restaurant App
cd src/MobileApps/FoodDash.RestaurantApp
dotnet build -f net8.0-android
```

## 🔐 Security Implementation

### **JWT Authentication**
- Each gRPC service validates JWT tokens
- Role-based authorization per service
- Refresh token rotation

### **gRPC Security**
- TLS encryption for all communications
- Service-to-service authentication
- Request/response logging

## 📱 Portal Features

### **Admin Portal Features**
- **Dashboard**: System overview and metrics
- **User Management**: User approval and management
- **Restaurant Management**: Restaurant approval and monitoring
- **Order Monitoring**: Real-time order tracking
- **Analytics**: Business intelligence and reports
- **System Configuration**: Global settings management

### **Restaurant Portal Features**
- **Dashboard**: Restaurant-specific metrics
- **Menu Management**: Full menu CRUD operations
- **Order Management**: Order processing and status updates
- **Analytics**: Restaurant performance metrics
- **Profile Management**: Restaurant profile updates
- **Staff Management**: Employee access control

### **Customer Portal Features**
- **Restaurant Browse**: Search and filter restaurants
- **Menu Viewing**: Browse restaurant menus
- **Cart Management**: Add/remove items
- **Order Placement**: Complete order process
- **Order Tracking**: Real-time order status
- **Profile Management**: Customer profile updates

## 📲 Mobile App Features

### **Customer Mobile App**
- **Native UI**: Platform-specific design
- **Offline Support**: Basic functionality offline
- **Push Notifications**: Order status updates
- **Location Services**: Delivery tracking
- **Payment Integration**: ESewa, IME Pay support

### **Delivery Mobile App**
- **Order Assignment**: Accept/decline orders
- **GPS Navigation**: Route optimization
- **Status Updates**: Real-time delivery status
- **Earnings Tracking**: Payment and earnings
- **Offline Mode**: Basic functionality offline

### **Restaurant Mobile App**
- **Order Notifications**: Instant order alerts
- **Quick Actions**: Rapid order status updates
- **Menu Updates**: Quick availability changes
- **Staff Coordination**: Team communication
- **Analytics**: Mobile dashboard

## 🌐 Multi-Language Support

All applications support:
- **English** (Default)
- **Nepali** (नेपाली)
- **Tharu** (थारू)

## 💳 Payment Integration

Supported payment methods:
- **ESewa**: Nepal's leading digital wallet
- **IME Pay**: Popular mobile payment
- **Cash on Delivery**: Traditional payment method

## 🔧 Development Guidelines

### **Adding New gRPC Methods**
1. Update the `.proto` file
2. Regenerate gRPC classes
3. Implement the service method
4. Update client implementations
5. Add tests

### **Creating New Portals**
1. Create new Blazor Server project
2. Add gRPC client dependencies
3. Implement authentication
4. Create role-specific components
5. Configure routing and authorization

### **Mobile App Development**
1. Create platform-specific implementations
2. Add gRPC client support
3. Implement offline capabilities
4. Add push notification support
5. Test on multiple devices

## 📈 Monitoring and Logging

### **Service Monitoring**
- Consul health checks
- Application metrics
- Performance monitoring
- Error tracking

### **Logging Strategy**
- Structured logging with Serilog
- Correlation IDs across services
- Centralized log aggregation
- Real-time monitoring dashboards

## 🚀 Deployment Strategy

### **Production Deployment**
1. **Containerization**: Docker containers for each service
2. **Orchestration**: Kubernetes for container management
3. **Service Mesh**: Istio for service communication
4. **Load Balancing**: nginx for web portal load balancing
5. **Database**: SQL Server with read replicas
6. **Caching**: Redis for performance optimization

This new architecture provides a scalable, maintainable, and high-performance food delivery platform that can handle enterprise-scale operations while providing excellent user experiences across all platforms.