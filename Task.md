# E‑Commerce Web App – Full Development Tasks (with Admin, Customer Panel, Stripe, Render + Neon).. in ASP.NET

## 1. Project Setup & Solution Structure
- [ ] Create a new solution: `EcommerceSolution`
- [ ] Create projects (all targeting .NET 8 or later):
  - `ECommerce.Domain` (Class Library)
  - `ECommerce.Application` (Class Library)
  - `ECommerce.Infrastructure` (Class Library)
  - `ECommerce.WebAPI` (ASP.NET Core Web API) – backend
  - `ECommerce.AdminPanel` (ASP.NET Core MVC / Razor Pages) – separate frontend for admin
  - `ECommerce.WebUI` (ASP.NET Core MVC or Blazor Server) – customer frontend
- [ ] Set project dependencies:
  - `Application` → `Domain`
  - `Infrastructure` → `Application`
  - `WebAPI`, `AdminPanel`, `WebUI` → `Application` and `Infrastructure`
- [ ] Install NuGet packages:
  - **Domain**: none
  - **Application**: `FluentValidation.DependencyInjectionExtensions`, `MediatR.Extensions.Microsoft.DependencyInjection`
  - **Infrastructure**: `Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL` (for Neon), `SendGrid.Extensions.DependencyInjection`, `Stripe.net`
  - **WebAPI**: `Swashbuckle.AspNetCore`, `Serilog.AspNetCore`
  - **AdminPanel & WebUI**: `Microsoft.AspNetCore.Identity.UI`, `Microsoft.EntityFrameworkCore.Tools`

## 2. Domain Layer (Entities)
- [ ] Create `Entities` folder:
  - `BaseEntity` (Id, CreatedAt, UpdatedAt)
  - `Product`, `Category`, `Customer` (linked to Identity UserId)
  - `Cart`, `CartItem`
  - `Order`, `OrderItem`
  - `Payment` (OrderId, StripePaymentIntentId, Status, Amount)
- [ ] Add `Enums`: `OrderStatus`, `PaymentStatus`
- [ ] Add `ValueObjects`: `Address`, `Money`

## 3. Application Layer (Use Cases)
- [ ] Create `Abstractions` with repository interfaces (`IGenericRepository<T>`, `IProductRepository`, `IOrderRepository`, `ICartRepository`)
- [ ] Create `DTOs` (ProductDto, OrderDto, CheckoutDto, etc.)
- [ ] Implement MediatR Commands/Queries:
  - `AddToCartCommand`, `CheckoutCommand`, `CreateOrderCommand`
  - `GetCartQuery`, `GetProductListQuery`
- [ ] Add validation with FluentValidation
- [ ] Add `IEmailSender` and `IStripePaymentService` interfaces
- [ ] Add `Result<T>` wrapper

## 4. Infrastructure Layer
- [ ] **Database**: Use PostgreSQL (compatible with Neon)
  - `AppDbContext` with `UseNpgsql`
  - `DesignTimeDbContextFactory`
  - Add migrations and seeding
- [ ] **Repositories**: Implement generic and specific repositories
- [ ] **Email**: Implement `SendGridEmailSender`
- [ ] **Payment**: Implement `StripePaymentService` with methods:
  - `CreatePaymentIntentAsync(amount, currency, metadata)`
  - `ConfirmPaymentAsync(paymentIntentId)`
  - `HandleWebhookAsync(eventJson)`
- [ ] Register services in `AddInfrastructure()`

## 5. Backend API (ECommerce.WebAPI)
- [ ] Controllers:
  - `ProductsController` (public GET, admin POST/PUT/DELETE)
  - `CartController` (GET, POST, DELETE)
  - `CheckoutController` (POST – creates Stripe PaymentIntent, returns client secret)
  - `OrdersController` (GET customer orders)
  - `WebhookController` (POST /stripe/webhook – to confirm payment and finalize order)
- [ ] Authentication: ASP.NET Core Identity + JWT
- [ ] Email verification endpoint (using SendGrid)
- [ ] Swagger with JWT support

## 6. Customer Panel (ECommerce.WebUI) – MVC / Razor Pages
- [ ] **Pages / Views**:
  - Home (product listing with pagination & categories)
  - Product detail (add to cart button)
  - Cart page (list items, update quantity, remove)
  - Checkout page (collect shipping address, payment via Stripe Elements)
- [ ] **Login / Register at Checkout**:
  - Guest adds items to cart (cart stored in session or cookie)
  - When user clicks “Proceed to Checkout”, if not logged in → redirect to login/register page.
  - After login, merge guest cart into logged-in user’s cart.
  - Redirect back to checkout.
- [ ] **Stripe Integration in UI**:
  - Include Stripe.js
  - On checkout page, call backend to create PaymentIntent, get clientSecret.
  - Use Stripe Elements or PaymentElement to collect card details.
  - Confirm payment; on success, redirect to order confirmation.
- [ ] **Order history** page for logged-in customers

## 7. Admin Panel (ECommerce.AdminPanel)
- [ ] Separate area (or separate project) with `[Authorize(Roles = "Admin")]`
- [ ] **Dashboard** with stats (total orders, revenue, top products)
- [ ] **Product management** (CRUD, upload images)
- [ ] **Category management**
- [ ] **Order management** (list all orders, view details, update status: Pending → Paid → Shipped → Delivered)
- [ ] **Customer management** (view customers, disable accounts)
- [ ] **Inventory alerts** (low stock products)
- [ ] Use AdminLTE or similar UI theme

## 8. Stripe Integration – Detailed Steps
- [ ] Sign up at Stripe, get Publishable & Secret keys (test mode first)
- [ ] Store keys in `appsettings.Production.json` (or environment variables)
- [ ] Backend:
  - `StripePaymentService.CreatePaymentIntent` – called from `CheckoutController`
  - Return `clientSecret` to frontend
  - Webhook endpoint: listen to `payment_intent.succeeded` event
  - On success, create Order from cart, clear cart, send email confirmation
- [ ] Frontend (customer panel):
  - Use `@stripe/stripe-js` and `@stripe/react-stripe-js` (if React) or vanilla JS for Razor
  - Confirm payment with `stripe.confirmPayment`
- [ ] Test with Stripe test card `4242 4242 4242 4242`

## 9. Login / Register at Checkout – Technical Flow
- [ ] Cart stored in database for logged-in users, in session for guests
- [ ] Action: `AddToCart` checks if user is authenticated; if not, uses `HttpContext.Session` to store cart items (serialized)
- [ ] When guest clicks “Checkout” → redirect to `/Identity/Account/Login?returnUrl=/checkout`
- [ ] After successful login, in the `OnGet` of checkout page, check for guest cart in session; if exists, add items to user’s cart and clear session.
- [ ] Ensure no duplication: if product already in user’s cart, increase quantity

## 10. Database – Neon PostgreSQL
- [ ] Create a free Neon account (neon.tech)
- [ ] Create a new project, get the **connection string** (starts with `postgresql://...`)
- [ ] In `appsettings.Production.json` (or environment variable):
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-...;Database=neondb;Username=...;Password=..."
  }


