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

**.NET 10** — ASP.NET Core MVC • **EF Core 10** — PostgreSQL (Neon) • **Identity** — Auth & roles • **Stripe** — Payments

**Razor Views** • **Tailwind CSS** • **Bootstrap 5.3** • **jQuery 3.7 + Validate** • **Chart.js** • **Font Awesome 6**

**Hugging Face Spaces** (Docker) • **Neon** • **GitHub Actions** • **UptimeRobot**

## Architecture

Three-layer architecture on ASP.NET Core MVC:

```
┌──────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                        │
│                                                                  │
│   Controllers (HTTP) ──▶ ViewModels (DTOs) ──▶ Views (Razor)   │
│                                                                  │
│   AccountController  │  CartController  │  HomeController       │
│   CheckoutController │  Admin/*                                  │
└──────────────────────────┬───────────────────────────────────────┘
                           │ Delegates business logic
                           ▼
┌──────────────────────────────────────────────────────────────────┐
│                       BUSINESS LOGIC LAYER                        │
│                     (ECommerce.Presenter)                         │
│                                                                  │
│   ProductListPresenter  │  CartPresenter  │  CheckoutPresenter  │
│   OrderPresenter  │  AdminDashboardPresenter                     │
│                                                                  │
│   → Calculations, validation, orchestration, entity→DTO mapping │
│   → No dependency on HTTP, sessions, or controllers             │
└──────────────────────────┬───────────────────────────────────────┘
                           │ Calls Repository
                           ▼
┌──────────────────────────────────────────────────────────────────┐
│                        DATA ACCESS LAYER                          │
│                       (ECommerce.Model)                           │
│                                                                  │
│   Repositories ──▶ AppDbContext ──▶ PostgreSQL (EF Core)         │
│                                                                  │
│   Entities: Product, Order, Cart, CartItem, Review,              │
│              Category, Customer, OrderItem                        │
└──────────────────────────────────────────────────────────────────┘
```

### Request Lifecycle (Checkout Example)

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

### Key Decisions

- **Cart persistence**: Guest items in `Session`, logged-in users in DB. Survives login/logout.
- **Email skipped**: Auto-confirm on register. `NoopEmailSender` logs silently (HF blocks SMTP).
- **Role redirect**: Admin → `/admin/dashboard`, Customer → `/`.
- **Revenue**: Includes `Paid` + `Shipped` + `Delivered`. Daily/monthly chart toggle.
- **Toasts**: Auto-dismiss after 2s. Dark theme (`#2a2a3d`).

## Project Structure

```
Ecommerce-web-project/
├── Controllers/                  # MVC controllers
│   ├── Admin/                    # Admin panel
│   ├── AccountController.cs      # Auth (login, register, profile)
│   ├── CartController.cs         # Shopping cart
│   ├── CheckoutController.cs     # Stripe checkout
│   └── HomeController.cs         # Pages & reviews
├── Views/                        # Razor views
│   ├── Home/  Account/  Cart/  Checkout/  Shared/
├── Areas/Admin/Views/            # Admin panel views
├── ECommerce.Model/              # Data layer
│   ├── Entities/  Data/  Repositories/
├── ECommerce.Presenter/          # Business logic layer
│   ├── Presenters/  ViewModels/
├── wwwroot/                      # Static files (css, images, js)
├── Program.cs                    # Entry point, DI, middleware
├── Dockerfile                    # Container build
└── .github/workflows/            # CI/CD (sync-to-hf.yml)
```

## Setup

### Prerequisites
- .NET 10 SDK
- PostgreSQL database (Neon or local)
- Stripe account

### Local Development

```
dotnet ef database update
dotnet run
Visit https://localhost:5289
```

### Configuration

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Stripe:PublishableKey` | Stripe publishable key |
| `Stripe:SecretKey` | Stripe secret key |

### Deployment (Hugging Face Spaces)

| Key | Value |
|---|---|
| `CONNECTIONSTRINGS__DEFAULTCONNECTION` | Neon PostgreSQL connection string |
| `STRIPE__PUBLISHABLEKEY` | Stripe publishable key |
| `STRIPE__SECRETKEY` | Stripe secret key |
| `ADMIN__EMAIL` | Admin email |
| `ADMIN__PASSWORD` | Admin password |
