# GLOWY ORE

Handcrafted ornaments store — a baby-pink, animated ASP.NET Core storefront with a built-in admin studio.

## Features

**Storefront**
- Animated editorial home page, collections, product pages with multi-image galleries
- Session cart, quantity controls, cash-on-delivery checkout
- Order confirmation and delivery note (free delivery over ৳2,000)
- Responsive layout with the GLOWY ORE logo, favicon and social preview

**Admin studio** (`/admin`)
- Cookie-authenticated login
- Dashboard: revenue, orders, pending count, products, recent orders, low stock
- Orders: filter by status and update status
- Products: create, edit, delete, upload multiple photos, stock and visibility
- Categories: create, edit, delete, upload multiple icons/photos (first = cover)

## Stack

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10 + SQLite
- Razor views, custom CSS design system, vanilla JS
- No frontend build step

## Run locally

```bash
dotnet run
```

Open http://localhost:5109. In Development the seeded admin is `admin` / `Glowy@123` (change it with `Admin__Password`, see below).

The SQLite database (`glowyore.db`) is created and seeded on first start.

## Configuration

| Setting | Purpose | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQLite connection string | `Data Source=glowyore.db` |
| `Admin__Password` | Admin password; when set it is applied/updated on startup | generated and logged in Production |
| `Storage__UploadsRoot` | Folder for runtime image uploads, mounted at `/uploads` | `wwwroot/uploads` |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` | `Production` in Docker |

Example for a persistent volume mounted at `/var/data`:

```
ConnectionStrings__DefaultConnection=Data Source=/var/data/glowyore.db
Storage__UploadsRoot=/var/data/uploads
Admin__Password=<a strong password>
```

## Deploy

GitHub Pages cannot host this app — it is a server-rendered .NET application with a database and admin panel, so it needs an application host. GitHub holds the source; a host runs it.

### Render (Docker)

[![Deploy to Render](https://render.com/images/deploy-to-render-button.svg)](https://render.com/deploy?repo=https://github.com/Nibir-Hasan0076/glowy-ore)

1. Sign in to Render with GitHub (free plan, no card).
2. Render reads `render.yaml` and creates the web service.
3. Enter a strong `Admin__Password` when prompted.
4. Open the generated `https://*.onrender.com` URL. The first visit after a period of inactivity can take about a minute while the free service wakes up.

The free plan uses an ephemeral filesystem, so orders, admin edits and uploaded images reset when the service restarts. For a real store, use the paid Starter plan with a 1 GB disk mounted at `/var/data` and the two environment variables shown above.

### Any Docker host

```bash
docker build -t glowy-ore .
docker run -p 8080:8080 \
  -e Admin__Password='change-me' \
  -e ConnectionStrings__DefaultConnection='Data Source=/data/glowyore.db' \
  -e Storage__UploadsRoot=/data/uploads \
  -v glowy-data:/data \
  glowy-ore
```

### Azure App Service

Deploy the container or use `dotnet publish`, set the same app settings, and point `Storage__UploadsRoot` and the database at a persistent path (for example `/home/data`) so uploads and orders survive deployments.

## Notes

- Seeded catalog images live in `wwwroot/images/seed` and ship with the app.
- Runtime uploads are validated (JPG, PNG, WEBP, GIF up to 6 MB) and stored outside the database.
- The brand logo assets are in `wwwroot/images/brand`.
