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

- **Product Catalog** — Browse with search, category filter, and sort by price/name/newest
- **Shopping Cart** — Persisted for logged-in users; session-based for guests (survives login/logout)
- **Checkout** — Stripe payment integration with shipping address
- **User Accounts** — Register, login, profile, change password, order history
- **Reviews & Ratings** — Star ratings with reviews on product detail page, review counts on product cards
- **Admin Dashboard** — Manage products, categories, orders, customers; revenue chart (daily/monthly toggle), order status management, low stock alerts
- **Responsive Design** — Desktop-first with mobile-optimized layouts

## Tech Stack

### Backend
- **.NET 10** (C#) — ASP.NET Core MVC
- **Entity Framework Core** — ORM with PostgreSQL (Neon)
- **ASP.NET Core Identity** — Authentication, authorization, role management
- **Stripe** — Payment processing API

### Frontend
- **Razor Views** — Server-rendered pages
- **Chart.js** — Admin dashboard charts (revenue, orders, products)
- **Font Awesome** — Icons
- **CSS** — Custom styles with responsive design

### Hosting
- **Hugging Face Spaces** — Docker deployment
- **Neon** — PostgreSQL database (serverless)
- **UptimeRobot** — Keep-alive pings
- **Brevo** - For email Verification
- **GitHub Actions** — CI/CD pipeline for automated testing and deployment
- **Docker** — Containerization for consistent deployment

## Project Structure

```
Ecommerce-web-project/
├── Controllers/          # MVC controllers
│   ├── Admin/            # Admin panel controllers
│   ├── AccountController  # Auth (login, register, profile)
│   ├── CartController     # Shopping cart
│   ├── CheckoutController # Stripe checkout
│   └── HomeController     # Pages & reviews
├── Views/                # Razor views
│   ├── Home/             # Index, Shop, Details, Contact
│   ├── Account/          # Login, Register, Profile
│   ├── Cart/             # Shopping cart
│   └── Shared/           # Layout, partials
├── Areas/Admin/Views/    # Admin panel views
├── ECommerce.Model/      # Data layer
│   ├── Entities/         # Product, Order, Cart, Review, etc.
│   ├── Data/             # DbContext, Migrations, SeedData
│   └── Repositories/     # Data access
├── ECommerce.Presenter/  # Business logic layer
│   ├── Presenters/       # ProductList, Cart, Checkout, etc.
│   └── ViewModels/       # Data transfer objects
├── wwwroot/              # Static files (CSS, images)
├── Program.cs            # App entry point
└── Dockerfile            # Container build
```

## Setup

### Prerequisites
- .NET 10 SDK
- PostgreSQL database (Neon DB)
- Stripe account

### Local Development

1. Clone the repo
2. Configure secrets (see below)
3. `dotnet restore`
4. `dotnet ef database update`
5. `dotnet run`
6. Visit `https://localhost:5289`


### Deployment (Hugging Face Spaces)

Set these environment variables in your Space secrets:

| Key | Value |
|---|---|
| `CONNECTIONSTRINGS__DEFAULTCONNECTION` | Neon PostgreSQL connection string |
| `STRIPE__PUBLISHABLEKEY` | Stripe publishable key |
| `STRIPE__SECRETKEY` | Stripe secret key |
| `ADMIN__EMAIL` | Admin email |
| `ADMIN__PASSWORD` | Admin password |


