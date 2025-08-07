# Copilot Instructions for Vault Project

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is an Encrypted Vault - Secure Notes & Password Manager built with ASP.NET Core 8 Web API and Blazor WebAssembly.

## Technology Stack
- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: Blazor WebAssembly
- **Database**: Entity Framework Core with SQLite
- **Authentication**: JWT Bearer tokens
- **Encryption**: AES-256 for all secrets

## Architecture
- **Vault.API**: Web API controllers, authentication, and business logic
- **Vault.Web**: Blazor WebAssembly frontend with secure UI components
- **Vault.Core**: Shared models, DTOs, interfaces, and encryption utilities
- **Vault.Infrastructure**: Entity Framework DbContext, repositories, and data access
- **Vault.Tests**: Unit tests with xUnit

## Security Requirements
- All secrets must be encrypted using AES-256 before storage
- JWT authentication for all API endpoints
- HTTPS for all communication
- Input validation on all endpoints
- Secure password hashing

## Key Features
- User registration and authentication
- CRUD operations for encrypted secrets
- Secret encryption/decryption on demand
- Auto-logout after inactivity
- Dark mode UI theme
- PIN-based secondary authentication simulation

## Coding Guidelines
- Use async/await patterns for all database operations
- Implement proper error handling and logging
- Follow SOLID principles and dependency injection
- Use DTOs for API communication
- Implement repository pattern for data access
- Include comprehensive unit tests
