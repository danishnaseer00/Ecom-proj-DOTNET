# DanStore — E-Commerce Web Application

Full-featured e-commerce platform built with ASP.NET Core MVC. Includes product catalog, shopping cart, checkout with Stripe, admin dashboard, user authentication, reviews, and email notifications.

## Live Demo

[https://Danishathugging-danstore.hf.space](https://Danishathugging-danstore.hf.space)

## Features

- **Product Catalog** — Browse, search, filter by category, sort by price/name/newest
- **Shopping Cart** — Persisted for logged-in users; session-based for guests
- **Checkout** — Stripe payment integration with shipping address
- **User Accounts** — Register, login, profile, change password, order history
- **Reviews & Ratings** — Star ratings with reviews on product detail page, review counts shown on product cards
- **Admin Dashboard** — Manage products, categories, orders, customers; update order status with email notification
- **Email Notifications** — Account confirmation, order confirmation, shipped alerts via Resend
- **Responsive Design** — Works on desktop and mobile

## Tech Stack

- **.NET 10** (C#)
- **ASP.NET Core MVC + Razor Pages**
- **Entity Framework Core** with PostgreSQL (Neon)
- **Stripe** — Payment processing
- **Resend** — Email delivery
- **ASP.NET Core Identity** — Authentication & authorization
- **Chart.js** — Admin dashboard charts

## Setup

1. Clone the repo
2. Configure secrets (see below)
3. `dotnet restore`
4. `dotnet ef database update`
5. `dotnet run`
6. Visit `https://localhost:7000`

### Secrets Configuration

The project uses secrets for sensitive data. Right-click project → **Manage User Secrets** and add:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_..."
  },
  "Resend": {
    "ApiKey": "re_...",
    "FromEmail": "danishnaseer000@gmail.com",
    "FromName": "DanStore"
  },
  "Admin": {
    "Email": "danishnaseer000@gmail.com",
    "Password": "YourPassword123"
  }
}
```

An `appsettings.Example.json` is included as a reference template.
