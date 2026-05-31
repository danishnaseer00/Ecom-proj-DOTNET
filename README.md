---
title: DanStore
emoji: 🛒
colorFrom: indigo
colorTo: purple
sdk: docker
pinned: false
---

# DanStore — E-Commerce Web Application

Full-featured e-commerce platform built with ASP.NET Core MVC. Includes product catalog, shopping cart, checkout with Stripe, admin dashboard, user authentication, product reviews, and responsive design.

## Live Demo

[https://Danishathugging-danstore.hf.space](https://Danishathugging-danstore.hf.space)

## Features

- **Product Catalog** — Browse with search, category filter, and sort by price/name/newest; paginated (10/page)
- **Shopping Cart** — Persisted in DB for logged-in users; session-based for guests; items survive login/logout
- **Checkout** — Stripe payment integration with shipping address; order confirmation
- **User Accounts** — Register (auto-confirmed), login, profile editing, change password, order history
- **Reviews & Ratings** — Star ratings with reviews on product detail page; aggregate ratings shown on product cards
- **Admin Dashboard** — Revenue chart (daily/monthly toggle), order status management, product/CRUD, category CRUD, customer list, low stock alerts
- **Role-Based Access** — Admin redirect to dashboard; customers to home page
- **Responsive Design** — Desktop-first with mobile-optimized cart, nav, and admin layouts
- **Toast Notifications** — Auto-dismissing success/error toasts (2s)

## Tech Stack

### Backend
| Layer | Technology |
|---|---|
| Runtime | .NET 10 (C#) |
| Framework | ASP.NET Core MVC |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (Neon serverless) |
| Auth | ASP.NET Core Identity |
| Payments | Stripe SDK |
| Architecture | Repository Pattern + Presenter (Service) Layer |

### Frontend
| Library | Purpose |
|---|---|
| Razor Pages | Server-rendered HTML |
| Tailwind CSS | Utility-first styling |
| Bootstrap 5.3 | Grid, components (via CDN) |
| Chart.js | Dashboard revenue charts |
| Font Awesome 6 | Icons (free) |
| jQuery 3.7 | DOM manipulation (via CDN) |

### DevOps & Hosting
| Service | Role |
|---|---|
| Hugging Face Spaces | Docker-based hosting |
| Docker | Containerization |
| Neon | Serverless PostgreSQL |
| GitHub Actions | CI/CD (auto-sync to HF Spaces) |
| UptimeRobot | Keep-alive pings |

## Architecture

The application follows a **three-layer architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│   Controllers (MVC)  │  Views (Razor)  │  ViewModels   │
│   CartController     │  Index.cshtml   │  CartViewModel │
│   HomeController     │  Shop.cshtml    │  ProductVM     │
│   CheckoutController │  Details.cshtml │  CheckoutVM    │
│   AccountController  │  Profile.cshtml │  OrderListVM   │
└─────────────────────────┬───────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                  Business Logic Layer                    │
│     Presenters (ECommerce.Presenter)                     │
│   ProductListPresenter │  CartPresenter                  │
│   OrderPresenter       │  CheckoutPresenter              │
│   AdminDashboardPresenter                                │
│   Domain services, validation, calculations              │
└─────────────────────────┬───────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                   Data Access Layer                      │
│   Repositories  │  DbContext  │  Entities  │  Migrations  │
│   IRepository<T>│  AppDbContext│  Product   │  SeedData    │
│   ICustomerRepo │              │  Order     │             │
└─────────────────────────────────────────────────────────┘
```

### Key Flows

- **Cart**: Guest items stored in `Session`; on login, CartController reads/writes `Cart`/`CartItem` tables via `IRepository<Cart>`. Cart data survives across authentication boundaries.
- **Checkout**: `[Authorize]`-protected; reads cart from DB for authenticated users (same logic as Cart), processes Stripe payment, creates order + order items, clears cart.
- **Admin Dashboard**: Revenue aggregates across `Paid` / `Shipped` / `Delivered` order statuses; chart supports daily and monthly views.
- **Auth**: Role-based redirect after login — admins go to `/admin/dashboard`, customers to `/`. Email verification is skipped (auto-confirmed on register).

## Project Structure

```
Ecommerce-web-project/
├── Controllers/                  # MVC controllers (presentation)
│   ├── Admin/                    # Admin panel controllers
│   ├── AccountController.cs      # Auth (login, register, profile)
│   ├── CartController.cs         # Shopping cart
│   ├── CheckoutController.cs     # Stripe checkout
│   └── HomeController.cs         # Pages & reviews
│
├── Views/                        # Razor views
│   ├── Home/                     # Index, Shop, Details, Contact
│   ├── Account/                  # Login, Register, Profile, Orders
│   ├── Cart/                     # Shopping cart
│   ├── Checkout/                 # Checkout page
│   └── Shared/                   # Layout, partials, toast scripts
│
├── Areas/Admin/Views/            # Admin panel views (dashboard, products, orders, customers, categories)
│
├── ECommerce.Model/              # Data layer (class library)
│   ├── Entities/                 # Product, Order, Cart, CartItem, Review, Category, Customer, OrderItem
│   ├── Data/                     # AppDbContext, Migrations, SeedData
│   └── Repositories/             # Generic IRepository<T>, ICustomerRepository
│
├── ECommerce.Presenter/          # Business logic layer (class library)
│   ├── Presenters/               # ProductList, Cart, Checkout, AdminDashboard, Order, NoopEmailSender
│   └── ViewModels/               # CartItemSession, CheckoutViewModel, OrderListViewModel, etc.
│
├── wwwroot/                      # Static files
│   ├── css/                      # Custom styles
│   ├── images/                   # Product images (products/, banners/)
│   └── js/                       # Custom JavaScript
│
├── Program.cs                    # App entry point, DI, middleware
├── Dockerfile                    # Docker build for HF Spaces
└── .github/workflows/            # CI/CD pipelines (sync-to-hf.yml)
```

## Setup

### Prerequisites
- .NET 10 SDK
- PostgreSQL database (Neon or local)
- Stripe account (publishable + secret keys)

### Local Development

1. Clone the repo
2. Configure user secrets (see below)
3. Apply migrations:
   ```
   dotnet ef database update
   ```
4. Run the application:
   ```
   dotnet run
   ```
5. Visit `https://localhost:5289`

### Configuration

Set these via `dotnet user-secrets` or environment variables:

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Stripe:PublishableKey` | Stripe publishable key (pk_test_...) |
| `Stripe:SecretKey` | Stripe secret key (sk_test_...) |

### Deployment (Hugging Face Spaces)

Set these environment variables in your Space secrets:

| Key | Value |
|---|---|
| `CONNECTIONSTRINGS__DEFAULTCONNECTION` | Neon PostgreSQL connection string |
| `STRIPE__PUBLISHABLEKEY` | Stripe publishable key |
| `STRIPE__SECRETKEY` | Stripe secret key |
| `ADMIN__EMAIL` | Admin email |
| `ADMIN__PASSWORD` | Admin password |


