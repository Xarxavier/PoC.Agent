# SSO Base Project

Este es el proyecto base de un sistema de Single Sign-On (SSO) construido con .NET Core y Entity Framework Core.

## Arquitectura

El proyecto sigue una arquitectura en capas:

- **SSO.Host**: Capa de presentación con Minimal API endpoints
- **SSO.Application**: Capa de aplicación con lógica de negocio
- **SSO.Domain**: Capa de dominio con entidades y interfaces
- **SSO.Infrastructure.EntityFramework**: Capa de infraestructura con Entity Framework Core y DbContext
- **SSO.Infrastructure.Repository**: Implementación de repositorios genéricos
- **SSO.Infrastructure.QueryServices**: Servicios de consulta especializados

## Tecnologías

- **.NET 9.0**
- **Entity Framework Core 9.0**
- **Pomelo.EntityFrameworkCore.MySql 9.0.0** - Provider MySQL para EF Core
- **DbUp-MySql 6.0.4** - Control de versiones de base de datos
- **Minimal API** - Endpoints HTTP ligeros

## Características

### Base de Datos
- Control de versiones con **DbUp**
- Enfoque **DB First** con Entity Framework Core
- Scripts de migración en la carpeta `SSO.Infrastructure.EntityFramework/Migrations`
- Contexto de base de datos: `SsoContext`

### Estructura de Endpoints
- Los endpoints están organizados en la carpeta `SSO.Host/Endpoints`
- Ejemplo: `UserEndpoints.cs` con operaciones CRUD básicas

### Entidades Base
- `User`: Usuario del sistema
- `Role`: Roles de seguridad
- `UserRole`: Relación muchos a muchos entre usuarios y roles

## Configuración

### Cadena de Conexión

Editar el archivo `appsettings.json` en el proyecto SSO.Host:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=sso_db;User=root;Password=password;"
  }
}
```

### Migraciones de Base de Datos

Las migraciones se ejecutan automáticamente al iniciar si `RunMigrationsOnStartup` está en `true`:

```json
{
  "RunMigrationsOnStartup": true
}
```

O ejecutar manualmente usando el `DatabaseMigrationManager`.

## Cómo Usar

### 1. Restaurar paquetes

```bash
dotnet restore
```

### 2. Compilar la solución

```bash
dotnet build
```

### 3. Ejecutar el proyecto

```bash
cd src/SSO.Host
dotnet run
```

### 4. Acceder a la API

La API estará disponible en `https://localhost:5001` o `http://localhost:5000`

Endpoints disponibles:
- `GET /api/users` - Obtener todos los usuarios activos
- `GET /api/users/{username}` - Obtener usuario por nombre de usuario
- `GET /api/users/email/{email}` - Obtener usuario por email

## Agregar Nuevas Migraciones

1. Crear un nuevo archivo SQL en `SSO.Infrastructure.EntityFramework/Migrations`
2. Nombrar el archivo siguiendo el patrón: `00X_DescriptionOfMigration.sql`
3. Los scripts se ejecutarán automáticamente en orden alfabético

Ejemplo:
```sql
-- 002_AddNewTable.sql
CREATE TABLE IF NOT EXISTS NewTable (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

## Desarrollo

### Agregar Nuevos Endpoints

1. Crear una nueva clase en `SSO.Host/Endpoints`
2. Implementar los métodos de extensión para mapear endpoints
3. Registrar los endpoints en `Program.cs`

### Agregar Nuevas Entidades

1. Crear la entidad en `SSO.Domain/Entities`
2. Agregar el DbSet en `SsoContext`
3. Configurar la entidad en el método `OnModelCreating`
4. Crear el script de migración SQL correspondiente

## Estructura del Proyecto

```
SSO/
├── src/
│   ├── SSO.Host/                           # API Web con Minimal API
│   │   ├── Endpoints/                      # Definición de endpoints
│   │   ├── Program.cs                      # Configuración de la aplicación
│   │   └── appsettings.json                # Configuración
│   ├── SSO.Domain/                         # Entidades de dominio
│   │   ├── Entities/                       # Modelos de datos
│   │   └── Interfaces/                     # Interfaces de repositorio
│   ├── SSO.Application/                    # Lógica de aplicación
│   │   ├── DTOs/                           # Data Transfer Objects
│   │   └── Services/                       # Servicios de aplicación
│   ├── SSO.Infrastructure.EntityFramework/ # EF Core y DbContext
│   │   ├── Migrations/                     # Scripts SQL de DbUp
│   │   ├── SsoContext.cs                   # DbContext
│   │   └── DatabaseMigrationManager.cs     # Gestor de migraciones
│   ├── SSO.Infrastructure.Repository/      # Implementación de repositorios
│   └── SSO.Infrastructure.QueryServices/   # Servicios de consulta
└── SSO.sln                                 # Archivo de solución
```

## Licencia

Este es un proyecto base para desarrollo.