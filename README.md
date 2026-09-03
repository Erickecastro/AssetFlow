#  AssetFlow

> A modern platform for intelligent IT asset management.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp)
![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql)
![License](https://img.shields.io/badge/License-MIT-success?style=for-the-badge)

# About

**AssetFlow** is a cross-platform IT Asset Management (ITAM) solution designed to centralize and optimize the management of technology assets throughout their entire lifecycle.

The platform enables organizations to efficiently manage equipment, monitor asset movements, maintain complete audit trails, and generate strategic reports through a secure and modern interface.

The project follows software engineering best practices, including Clean Architecture, Domain-Driven Design principles, and scalable application design.

# Features

- Asset registration
- Asset inventory management
- Department management
- User management
- Asset movement tracking
- Complete asset history
- Dashboard and analytics
- Advanced search
- Authentication & Authorization
- REST API
- Cross-platform mobile application

# Tech Stack

### Backend

- ASP.NET Core
- C#
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger / OpenAPI

### Mobile

- .NET MAUI

### Development

- JetBrains Rider
- Git
- GitHub

### Planned

- Docker
- GitHub Actions
- Azure
- Serilog

# Supported Platforms

- Windows
- Android
- iOS
- Mac

# Goals

- Centralize IT asset management
- Improve inventory control
- Track asset lifecycle
- Simplify audits
- Generate business reports
- Increase operational efficiency

# Roadmap

- [ ] User authentication
- [ ] User management
- [ ] Department management
- [ ] Asset registration
- [ ] Inventory management
- [ ] Asset transfers
- [ ] Dashboard
- [ ] Search system

# Experimental API

The current experimental backend supports:

- JWT authentication with a local demo account
- Department creation and listing
- Asset creation and listing
- Asset assignment and transfer between departments
- Return to inventory
- Complete movement history
- A stable QR payload (`assetflow:asset:{id}`) for each asset

## Local setup

1. Start PostgreSQL with `docker compose up -d`.
2. Apply the database schema:
   `dotnet ef database update --project src/AssetFlow.Infrastructure --startup-project src/AssetFlow.Api`
3. Run the API with `dotnet run --project src/AssetFlow.Api`.
4. Use `src/AssetFlow.Api/AssetFlow.Api.http` for example requests.

## Experimental GUI

With PostgreSQL and the API running, start the Windows MAUI client:

`dotnet run --project src/AssetFlow.Mobile -f net10.0-windows10.0.19041.0`

The GUI supports JWT login, department and asset registration, inventory
refresh, asset assignment/transfer, return to inventory, movement history,
and copying the stable QR payload.

The local-only demo credentials are `admin` / `assetflow-demo`. The connection
string, demo credentials, and JWT signing key must be supplied through secure
configuration before any deployment.

## Future Features

- [ ] QR Code support
- [ ] Barcode scanning
- [ ] Asset photos
- [ ] Digital signatures
- [ ] PDF export
- [ ] Excel export
- [ ] Push notifications
- [ ] Maintenance management
- [ ] Web application
- [ ] Audit logs
- [ ] Role-based permissions

# Screenshots

> Coming soon.

# Contributing

Contributions are welcome!

If you have ideas, suggestions, or find any issues, feel free to open an Issue or submit a Pull Request.

# License

This project is licensed under the MIT License.

