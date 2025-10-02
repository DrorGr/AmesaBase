# Frontend-Backend Integration Guide

## Overview

This document provides a comprehensive guide for integrating the Angular frontend with the .NET backend API. The integration includes authentication, data management, real-time communication, and error handling.

## Table of Contents

1. [Setup and Configuration](#setup-and-configuration)
2. [Authentication Integration](#authentication-integration)
3. [API Service Integration](#api-service-integration)
4. [Real-time Communication](#real-time-communication)
5. [Error Handling](#error-handling)
6. [Data Flow Examples](#data-flow-examples)
7. [Testing Integration](#testing-integration)
8. [Deployment Considerations](#deployment-considerations)

## Setup and Configuration

### 1. Environment Configuration

Update your environment files to include the backend URL:

```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  logLevel: 'debug',
  backendUrl: 'http://localhost:5000/api'
};

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  logLevel: 'info',
  backendUrl: 'https://api.yourdomain.com/api'
};
```

### 2. Install Required Dependencies

```bash
npm install @microsoft/signalr
npm install @angular/common
npm install @angular/forms
```

### 3. Update Angular Module

Add the required modules to your `app.module.ts`:

```typescript
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule
  ],
  // ... other configurations
})
export class AppModule { }
```

## Authentication Integration

### 1. Update Auth Service

The `AuthService` has been updated to integrate with the backend API. Key features:

- JWT token management
- Automatic token refresh
- User profile management
- Social login support (placeholder)

### 2. Authentication Flow

```typescript
// Login example
this.authService.login(email, password).subscribe({
  next: (success) => {
    if (success) {
      // User is logged in, redirect to dashboard
      this.router.navigate(['/dashboard']);
    }
  },
  error: (error) => {
    // Handle login error
    console.error('Login failed:', error);
  }
});

// Register example
this.authService.register(registerData).subscribe({
  next: (success) => {
    if (success) {
      // Registration successful, redirect to dashboard
      this.router.navigate(['/dashboard']);
    }
  },
  error: (error) => {
    // Handle registration error
    console.error('Registration failed:', error);
  }
});
```

### 3. Route Guards

Create route guards to protect authenticated routes:

```typescript
// src/guards/auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    } else {
      this.router.navigate(['/login']);
      return false;
    }
  }
}
```

## API Service Integration

### 1. Generic API Service

The `ApiService` provides a generic interface for all HTTP operations:

```typescript
// Example usage
this.apiService.get<HouseDto[]>('houses').subscribe({
  next: (response) => {
    if (response.success && response.data) {
      this.houses = response.data;
    }
  },
  error: (error) => {
    console.error('Error fetching houses:', error);
  }
});
```

### 2. Service Integration Examples

#### Lottery Service
```typescript
// Get houses with pagination
this.lotteryService.getHousesFromApi({
  page: 1,
  limit: 10,
  status: 'active'
}).subscribe({
  next: (pagedResponse) => {
    this.houses = pagedResponse.items;
    this.totalCount = pagedResponse.totalCount;
  }
});

// Purchase tickets
this.lotteryService.purchaseTicket({
  houseId: 'house-123',
  quantity: 5,
  paymentMethodId: 'pm-456'
}).subscribe({
  next: (result) => {
    console.log('Tickets purchased:', result.ticketNumbers);
  }
});
```

#### Payment Service
```typescript
// Get payment methods
this.paymentService.getPaymentMethods().subscribe({
  next: (methods) => {
    this.paymentMethods = methods;
  }
});

// Process payment
this.paymentService.processPayment({
  paymentMethodId: 'pm-123',
  amount: 250,
  currency: 'USD',
  description: 'Lottery tickets purchase'
}).subscribe({
  next: (response) => {
    if (response.success) {
      console.log('Payment successful:', response.transactionId);
    }
  }
});
```

## Real-time Communication

### 1. SignalR Integration

The `RealtimeService` provides real-time communication with the backend:

```typescript
// Initialize connection
this.realtimeService.startConnection().then(() => {
  console.log('Connected to real-time service');
});

// Subscribe to lottery updates
this.realtimeService.lotteryUpdates$.subscribe(update => {
  console.log('Lottery update:', update);
  // Update UI with real-time data
});

// Subscribe to notifications
this.realtimeService.notifications$.subscribe(notification => {
  console.log('New notification:', notification);
  // Show notification to user
});
```

### 2. Join Groups

```typescript
// Join lottery group for real-time updates
this.realtimeService.joinLotteryGroup('house-123');

// Join user group for personal notifications
this.realtimeService.joinUserGroup('user-456');
```

## Error Handling

### 1. Global Error Handler

The `ErrorHandlingService` provides centralized error handling:

```typescript
// Error handling is automatically applied through interceptors
// Custom error handling in components
this.lotteryService.getHousesFromApi().subscribe({
  next: (response) => {
    // Handle success
  },
  error: (error) => {
    // Error is automatically logged and handled
    // You can add custom UI error handling here
    this.showErrorMessage('Failed to load houses');
  }
});
```

### 2. HTTP Interceptors

The `RoutePerformanceInterceptor` automatically handles:
- Request/response logging
- Performance monitoring
- Error handling

## Data Flow Examples

### 1. Complete User Registration Flow

```typescript
// 1. User fills registration form
const registrationData = {
  username: 'john_doe',
  email: 'john@example.com',
  password: 'securePassword',
  firstName: 'John',
  lastName: 'Doe',
  authProvider: 'email'
};

// 2. Submit registration
this.authService.register(registrationData).subscribe({
  next: (success) => {
    if (success) {
      // 3. User is automatically logged in
      // 4. Redirect to dashboard
      this.router.navigate(['/dashboard']);
    }
  },
  error: (error) => {
    // Handle registration error
    this.showError('Registration failed. Please try again.');
  }
});
```

### 2. Complete Ticket Purchase Flow

```typescript
// 1. User selects house and ticket quantity
const purchaseRequest = {
  houseId: 'house-123',
  quantity: 5,
  paymentMethodId: 'pm-456'
};

// 2. Purchase tickets
this.lotteryService.purchaseTicket(purchaseRequest).subscribe({
  next: (result) => {
    // 3. Show success message
    this.showSuccess(`Successfully purchased ${result.ticketsPurchased} tickets`);
    
    // 4. Update UI with new ticket count
    this.updateTicketCount();
    
    // 5. Redirect to tickets page
    this.router.navigate(['/my-tickets']);
  },
  error: (error) => {
    // Handle purchase error
    this.showError('Failed to purchase tickets. Please try again.');
  }
});
```

### 3. Real-time Lottery Updates

```typescript
// 1. Component initialization
ngOnInit() {
  // 2. Start real-time connection
  this.realtimeService.startConnection();
  
  // 3. Join lottery group for specific house
  this.realtimeService.joinLotteryGroup(this.houseId);
  
  // 4. Subscribe to updates
  this.realtimeService.lotteryUpdates$.subscribe(update => {
    // 5. Update UI with real-time data
    this.updateLotteryStats(update);
  });
}

// 6. Update UI method
updateLotteryStats(update: LotteryUpdateEvent) {
  this.ticketsSold = update.ticketsSold;
  this.participationPercentage = update.participationPercentage;
  this.timeRemaining = update.timeRemaining;
}
```

## Testing Integration

### 1. Unit Tests

```typescript
// Example test for AuthService
describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should login user successfully', () => {
    const mockResponse = {
      success: true,
      data: {
        accessToken: 'mock-token',
        user: { id: '1', email: 'test@example.com' }
      }
    };

    service.login('test@example.com', 'password').subscribe(result => {
      expect(result).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.backendUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });
});
```

### 2. Integration Tests

```typescript
// Example integration test
describe('Lottery Integration', () => {
  it('should complete full ticket purchase flow', async () => {
    // 1. Login user
    await authService.login('test@example.com', 'password');
    
    // 2. Get houses
    const houses = await lotteryService.getHousesFromApi().toPromise();
    expect(houses.items.length).toBeGreaterThan(0);
    
    // 3. Purchase tickets
    const result = await lotteryService.purchaseTicket({
      houseId: houses.items[0].id,
      quantity: 1,
      paymentMethodId: 'test-pm'
    }).toPromise();
    
    expect(result.ticketsPurchased).toBe(1);
  });
});
```

## Deployment Considerations

### 1. Environment Variables

Ensure environment variables are properly configured:

```bash
# Production environment
export BACKEND_URL=https://api.yourdomain.com/api
export FRONTEND_URL=https://yourdomain.com
```

### 2. CORS Configuration

The backend is configured to allow requests from the frontend domain. Update the CORS policy in `Program.cs` if needed.

### 3. SSL/TLS

Ensure both frontend and backend use HTTPS in production.

### 4. API Rate Limiting

The backend implements rate limiting. Monitor API usage and adjust limits as needed.

### 5. Monitoring

Set up monitoring for:
- API response times
- Error rates
- Real-time connection stability
- User authentication success rates

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure backend CORS policy allows frontend domain
2. **Authentication Failures**: Check JWT token configuration
3. **Real-time Connection Issues**: Verify SignalR hub configuration
4. **API Timeouts**: Check network connectivity and server performance

### Debug Mode

Enable debug logging in development:

```typescript
// In environment.ts
export const environment = {
  production: false,
  logLevel: 'debug',
  backendUrl: 'http://localhost:5000/api'
};
```

## Security Considerations

1. **Token Storage**: JWT tokens are stored in localStorage (consider httpOnly cookies for production)
2. **Input Validation**: All user inputs are validated on both frontend and backend
3. **HTTPS**: Always use HTTPS in production
4. **Rate Limiting**: Implement rate limiting to prevent abuse
5. **Error Handling**: Avoid exposing sensitive information in error messages

## Performance Optimization

1. **Lazy Loading**: Implement lazy loading for routes and modules
2. **Caching**: Use HTTP caching for static data
3. **Real-time Optimization**: Limit real-time updates to necessary data only
4. **Bundle Size**: Monitor and optimize bundle size
5. **API Optimization**: Use pagination and filtering to reduce data transfer

This integration guide provides a comprehensive foundation for connecting your Angular frontend with the .NET backend. Follow the examples and best practices outlined here to ensure a robust and scalable integration.
