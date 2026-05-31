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
- **.NET 10** (C#) — ASP.NET Core MVC
- **Entity Framework Core 10** — ORM with PostgreSQL (Neon)
- **ASP.NET Core Identity** — Authentication, authorization, role management
- **Stripe** — Payment processing API

### Frontend
- **Razor Views** — Server-rendered pages
- **Tailwind CSS** — Utility-first styling
- **Bootstrap 5.3** — Grid & components (CDN)
- **jQuery 3.7 + jQuery Validate** — Client-side form validation (CDN)
- **Chart.js** — Admin dashboard charts
- **Font Awesome 6** — Icons

### DevOps & Hosting
- **Hugging Face Spaces** — Docker-based hosting
- **Docker** — Containerization
- **Neon** — Serverless PostgreSQL
- **GitHub Actions** — CI/CD (auto-sync to HF Spaces)
- **UptimeRobot** — Keep-alive pings

## Architecture

The application follows a **three-layer architecture** built on top of ASP.NET Core MVC. Each layer has a single responsibility and communicates only with the layer directly below it.

```
┌──────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                        │
│                                                                  │
│   ┌──────────────┐    ┌──────────────┐    ┌──────────────┐      │
│   │  Controllers  │───▶│  ViewModels  │───▶│     Views    │      │
│   │  (MVC)        │    │  (DTOs)      │    │  (Razor .cshtml) │  │
│   └──────┬───────┘    └──────────────┘    └──────────────┘      │
│          │                                                       │
│          │ HTTP Session, Cookies, User.Identity                  │
│          ▼                                                       │
│   ┌──────────────────────────────────────────────────────┐       │
│   │  AccountController  │  CartController                │       │
│   │  HomeController     │  CheckoutController            │       │
│   │  Admin/Orders       │  Admin/Products                │       │
│   └──────────────────────────────────────────────────────┘       │
└──────────────────────────┬───────────────────────────────────────┘
                           │
                           ▼ Delegates business logic
┌──────────────────────────────────────────────────────────────────┐
│                       BUSINESS LOGIC LAYER                        │
│                     (ECommerce.Presenter)                         │
│                                                                  │
│   ┌──────────────────────────────────────────────────────┐       │
│   │  ProductListPresenter  │  CartPresenter              │       │
│   │  CheckoutPresenter     │  OrderPresenter             │       │
│   │  AdminDashboardPresenter                              │       │
│   └──────────────────────────────────────────────────────┘       │
│                                                                  │
│   Responsibilities:                                              │
│   • Calculations (subtotal, tax, revenue aggregation)            │
│   • Validation (stock check, data integrity)                     │
│   • Orchestration (call multiple repos, combine results)         │
│   • Mapping (entities → view models)                             │
│   • No dependency on HTTP, sessions, or controllers              │
└──────────────────────────┬───────────────────────────────────────┘
                           │
                           ▼ Calls Repository
┌──────────────────────────────────────────────────────────────────┐
│                        DATA ACCESS LAYER                          │
│                       (ECommerce.Model)                           │
│                                                                  │
│   ┌────────────┐    ┌──────────────┐    ┌──────────────────┐     │
│   │ Repositories│───▶│  AppDbContext│───▶│  PostgreSQL (EF) │     │
│   │ IRepository │    │  Migrations │    │  (Neon)          │     │
│   │ ICustomer   │    │  SeedData   │    │                  │     │
│   └────────────┘    └──────────────┘    └──────────────────┘     │
│                                                                  │
│   Entities: Product, Order, Cart, CartItem, Review,              │
│              Category, Customer, OrderItem                        │
└──────────────────────────────────────────────────────────────────┘
```

### Request Lifecycle Example — Checkout

```
Browser ──GET /checkout──▶ Routing ──▶ [Authorize] ──▶ CheckoutController.Index()
                                                              │
                                                              ▼
                                              _cartPresenter.GetCheckoutData(userId)
                                                              │
                                                              ▼
                                              _cartRepo.GetCart(userId)  ──▶  PostgreSQL
                                                              │
                                                              ▼
                                              Calculates totals, validates stock
                                                              │
                                                              ▼
                                              Returns CheckoutViewModel
                                                              │
                                                              ▼
                                              View → Renders HTML → Browser
```

### Key Design Decisions

- **Cart persistence**: Guest items stored in `Session`; authenticated users' carts in DB via `Cart`/`CartItem` tables. Cart data survives login/logout via `GetEffectiveCartAsync()`.
- **Email verification skipped**: `RequireConfirmedAccount = false`. Users auto-confirmed and signed in immediately on register. All email calls go through `NoopEmailSender` (logs silently).
- **Role-based redirect**: On login, admins go to `/admin/dashboard`, customers go to `/`.
- **Revenue aggregation**: Includes `Paid` + `Shipped` + `Delivered` order statuses. Chart supports daily and monthly views.
- **Toast notifications**: Auto-dismiss after 2 seconds with fade-out animation. Dark theme (`#2a2a3d`).

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


