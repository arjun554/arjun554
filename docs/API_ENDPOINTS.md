# FoodDash API Endpoints Documentation

## Authentication Endpoints

### POST /api/auth/login
Login with email and password.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123",
  "rememberMe": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "guid-refresh-token",
    "expiresAt": "2025-02-17T10:00:00Z",
    "user": {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "email": "user@example.com",
      "phoneNumber": "+977-9841234567",
      "role": "Customer",
      "profilePicture": null,
      "isActive": true
    }
  }
}
```

### POST /api/auth/register
Register a new user account.

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "user@example.com",
  "phoneNumber": "+977-9841234567",
  "password": "password123",
  "confirmPassword": "password123",
  "role": "Customer",
  "address": "Kathmandu, Nepal",
  "city": "Kathmandu",
  "state": "Bagmati",
  "zipCode": "44600"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "guid-refresh-token",
    "expiresAt": "2025-02-17T10:00:00Z",
    "user": { /* user object */ }
  }
}
```

### POST /api/auth/change-password
Change user password (requires authentication).

**Request:**
```json
{
  "currentPassword": "oldpassword123",
  "newPassword": "newpassword123"
}
```

### POST /api/auth/forgot-password
Request password reset token.

**Request:**
```json
{
  "email": "user@example.com"
}
```

### POST /api/auth/reset-password
Reset password using token.

**Request:**
```json
{
  "email": "user@example.com",
  "token": "reset-token",
  "newPassword": "newpassword123"
}
```

### POST /api/auth/refresh-token
Refresh JWT token using refresh token.

**Request:**
```json
{
  "refreshToken": "guid-refresh-token"
}
```

### POST /api/auth/logout
Logout user (requires authentication).

### GET /api/auth/me
Get current user information (requires authentication).

## Restaurant Endpoints

### GET /api/restaurants
Get paginated list of restaurants.

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)
- `searchTerm` (string): Search by name or cuisine type

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Pizza Palace",
        "description": "Best pizza in town",
        "address": "New Road, Kathmandu",
        "city": "Kathmandu",
        "state": "Bagmati",
        "zipCode": "44600",
        "phoneNumber": "+977-01-4123456",
        "email": "info@pizzapalace.com",
        "imageUrl": "https://example.com/pizza-palace.jpg",
        "latitude": 27.7172,
        "longitude": 85.3240,
        "status": "Approved",
        "deliveryFee": 50.00,
        "estimatedDeliveryTime": 30,
        "minimumOrderAmount": 500.00,
        "rating": 4.5,
        "reviewCount": 150,
        "isOpen": true,
        "openTime": "10:00:00",
        "closeTime": "22:00:00",
        "cuisineType": "Italian",
        "ownerId": 2,
        "createdAt": "2025-01-01T00:00:00Z",
        "updatedAt": "2025-01-15T12:00:00Z"
      }
    ],
    "totalItems": 25,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### GET /api/restaurants/{id}
Get restaurant by ID.

### POST /api/restaurants
Create new restaurant (requires restaurant owner authentication).

### PUT /api/restaurants/{id}
Update restaurant (requires restaurant owner authentication).

### DELETE /api/restaurants/{id}
Delete restaurant (requires restaurant owner authentication).

### GET /api/restaurants/owner/{ownerId}
Get restaurants by owner ID.

## Menu Endpoints

### GET /api/menu/restaurant/{restaurantId}
Get menu categories and items for a restaurant.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Pizza",
      "description": "Delicious pizzas",
      "imageUrl": "https://example.com/pizza-category.jpg",
      "restaurantId": 1,
      "sortOrder": 1,
      "isActive": true,
      "menuItems": [
        {
          "id": 1,
          "name": "Margherita Pizza",
          "description": "Classic pizza with tomato and mozzarella",
          "price": 800.00,
          "imageUrl": "https://example.com/margherita.jpg",
          "categoryId": 1,
          "categoryName": "Pizza",
          "restaurantId": 1,
          "isAvailable": true,
          "isVegetarian": true,
          "isVegan": false,
          "isGlutenFree": false,
          "isSpicy": false,
          "preparationTime": 15,
          "calories": 250,
          "ingredients": "Tomato sauce, mozzarella, basil",
          "allergens": "Gluten, Dairy",
          "rating": 4.3,
          "reviewCount": 45,
          "createdAt": "2025-01-01T00:00:00Z",
          "updatedAt": "2025-01-10T00:00:00Z"
        }
      ]
    }
  ]
}
```

### GET /api/menu/item/{id}
Get menu item by ID.

### POST /api/menu/item
Create new menu item (requires restaurant owner authentication).

### PUT /api/menu/item/{id}
Update menu item (requires restaurant owner authentication).

### DELETE /api/menu/item/{id}
Delete menu item (requires restaurant owner authentication).

## Order Endpoints

### GET /api/orders
Get user's orders (customer gets their orders, restaurant owners get their restaurant orders).

### GET /api/orders/{id}
Get order by ID.

### POST /api/orders
Create new order (requires customer authentication).

**Request:**
```json
{
  "restaurantId": 1,
  "deliveryAddress": "Thamel, Kathmandu",
  "deliveryInstructions": "Call when you arrive",
  "paymentMethod": "ESewa",
  "couponCode": "WELCOME10",
  "orderItems": [
    {
      "menuItemId": 1,
      "quantity": 2,
      "specialInstructions": "Extra cheese"
    },
    {
      "menuItemId": 2,
      "quantity": 1,
      "specialInstructions": null
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "orderNumber": "ORD-20250216-001",
    "customerId": 1,
    "customerName": "John Doe",
    "customerPhone": "+977-9841234567",
    "restaurantId": 1,
    "restaurantName": "Pizza Palace",
    "deliveryPartnerId": null,
    "deliveryPartnerName": null,
    "status": "Pending",
    "deliveryAddress": "Thamel, Kathmandu",
    "deliveryInstructions": "Call when you arrive",
    "subTotal": 1600.00,
    "taxAmount": 208.00,
    "deliveryFee": 50.00,
    "discountAmount": 160.00,
    "totalAmount": 1698.00,
    "paymentMethod": "ESewa",
    "isPaid": false,
    "createdAt": "2025-02-16T10:00:00Z",
    "confirmedAt": null,
    "readyAt": null,
    "deliveredAt": null,
    "estimatedDeliveryTime": 30,
    "couponCode": "WELCOME10",
    "orderItems": [
      {
        "id": 1,
        "orderId": 1,
        "menuItemId": 1,
        "menuItemName": "Margherita Pizza",
        "quantity": 2,
        "unitPrice": 800.00,
        "totalPrice": 1600.00,
        "specialInstructions": "Extra cheese"
      }
    ]
  }
}
```

### PUT /api/orders/{id}/status
Update order status (requires appropriate role).

### GET /api/orders/customer/{customerId}
Get orders by customer ID.

### GET /api/orders/restaurant/{restaurantId}
Get orders by restaurant ID.

## User Management Endpoints

### GET /api/users/{id}
Get user by ID (admin or self).

### PUT /api/users/{id}
Update user profile (admin or self).

## Payment Endpoints

### POST /api/payments/process
Process payment for an order.

**Request:**
```json
{
  "orderId": 1,
  "paymentMethod": "ESewa"
}
```

## Notification Endpoints

### POST /api/notifications/send
Send notification to user (admin only).

### GET /api/notifications/user/{userId}
Get notifications for user.

## Error Responses

All endpoints return consistent error responses:

```json
{
  "success": false,
  "message": "Error description",
  "errors": [
    "Detailed error message 1",
    "Detailed error message 2"
  ]
}
```

## HTTP Status Codes

- `200` - Success
- `201` - Created
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (authentication required)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `500` - Internal Server Error

## Authentication

Most endpoints require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Rate Limiting

API endpoints are rate-limited based on user role:
- Customers: 100 requests per minute
- Restaurant Owners: 200 requests per minute
- Delivery Partners: 150 requests per minute
- Admins: 500 requests per minute

## Swagger Documentation

Complete API documentation is available at `/swagger` when running in development mode.

## Postman Collection

A Postman collection with all endpoints and sample requests is available in the `docs/postman/` directory.