# Arquitectura del Proyecto SSO

## Diagrama de Capas

```
┌─────────────────────────────────────────────────────┐
│                    SSO.Host                         │
│              (Presentation Layer)                   │
│  - Minimal API Endpoints                           │
│  - Program.cs (Startup & Configuration)            │
│  - appsettings.json                                │
└──────────────┬──────────────────────────────────────┘
               │ References
               ▼
┌─────────────────────────────────────────────────────┐
│               SSO.Application                       │
│            (Application Layer)                      │
│  - DTOs (Data Transfer Objects)                    │
│  - Application Services                            │
│  - Use Cases                                       │
└──────────────┬──────────────────────────────────────┘
               │ References
               ▼
┌─────────────────────────────────────────────────────┐
│                 SSO.Domain                          │
│              (Domain Layer)                         │
│  - Entities (User, Role, UserRole)                 │
│  - Interfaces (IRepository<T>)                     │
│  - Business Rules                                  │
└──────────────▲──────────────────────────────────────┘
               │ Referenced by
               │
     ┌─────────┴──────────┬──────────────┐
     │                    │              │
     ▼                    ▼              ▼
┌─────────────┐  ┌──────────────┐  ┌────────────────────┐
│Infrastructure│  │Infrastructure│  │  Infrastructure     │
│ .Repository │  │.QueryServices│  │.EntityFramework    │
│             │  │              │  │                    │
│- Generic    │  │- User Query  │  │- SsoContext        │
│  Repository │  │  Service     │  │- Migrations        │
│- CRUD       │  │- Specialized │  │- DbUp Manager      │
│  Operations │  │  Queries     │  │- EF Core Config    │
└─────────────┘  └──────────────┘  └────────────────────┘
```

## Responsabilidades de Cada Capa

### 1. SSO.Host (Presentation Layer)
**Responsabilidad**: Exponer la API y manejar peticiones HTTP

**Contiene**:
- Endpoints organizados en la carpeta `Endpoints/`
- Configuración de la aplicación en `Program.cs`
- Configuración de inyección de dependencias
- Configuración de middleware (CORS, autenticación, etc.)
- appsettings.json para configuración

**Dependencias**: Application, Infrastructure.*

### 2. SSO.Application (Application Layer)
**Responsabilidad**: Lógica de negocio de la aplicación

**Contiene**:
- DTOs para transferencia de datos
- Servicios de aplicación
- Casos de uso
- Validaciones de negocio
- Mappers

**Dependencias**: Domain

### 3. SSO.Domain (Domain Layer)
**Responsabilidad**: Modelo de dominio y reglas de negocio

**Contiene**:
- Entidades del dominio (User, Role, UserRole)
- Interfaces de repositorio
- Value Objects
- Reglas de negocio del dominio

**Dependencias**: Ninguna (capa más interna)

### 4. SSO.Infrastructure.EntityFramework
**Responsabilidad**: Implementación de acceso a datos con EF Core

**Contiene**:
- DbContext (SsoContext)
- Configuración de entidades
- Migraciones SQL (carpeta Migrations/)
- DatabaseMigrationManager (DbUp)

**Dependencias**: Domain

**Tecnologías**:
- Entity Framework Core 9.0
- Pomelo.EntityFrameworkCore.MySql 9.0.0
- DbUp-MySql 6.0.4

### 5. SSO.Infrastructure.Repository
**Responsabilidad**: Implementación de repositorios genéricos

**Contiene**:
- Repository<T> (implementación genérica)
- Operaciones CRUD básicas

**Dependencias**: Domain, Infrastructure.EntityFramework

### 6. SSO.Infrastructure.QueryServices
**Responsabilidad**: Servicios de consulta especializados

**Contiene**:
- Interfaces de query services
- Implementaciones de queries complejas
- Queries optimizadas con Include, Join, etc.

**Dependencias**: Domain, Infrastructure.EntityFramework

## Flujo de una Petición

```
1. HTTP Request
   ↓
2. Endpoint (SSO.Host/Endpoints/UserEndpoints.cs)
   ↓
3. Query Service (SSO.Infrastructure.QueryServices)
   ↓
4. DbContext (SSO.Infrastructure.EntityFramework)
   ↓
5. MySQL Database
   ↓
6. Entity (SSO.Domain/Entities)
   ↓
7. DTO (SSO.Application/DTOs)
   ↓
8. HTTP Response
```

## Ejemplo de Flujo Completo

### GET /api/users/johndoe

```csharp
// 1. Endpoint recibe la petición
// SSO.Host/Endpoints/UserEndpoints.cs
app.MapGet("/api/users/{username}", GetUserByUsername);

// 2. Handler del endpoint
private static async Task<IResult> GetUserByUsername(
    string username, 
    IUserQueryService userQueryService)
{
    var user = await userQueryService.GetUserByUsernameAsync(username);
    return user != null ? Results.Ok(user) : Results.NotFound();
}

// 3. Query Service ejecuta la consulta
// SSO.Infrastructure.QueryServices/Services/UserQueryService.cs
public async Task<User?> GetUserByUsernameAsync(string username)
{
    return await _context.Users
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.Username == username);
}

// 4. EF Core ejecuta la query en MySQL
// 5. Retorna la entidad User del Domain
// 6. Se puede mapear a UserDto si es necesario
// 7. Se retorna como JSON al cliente
```

## Patrones Utilizados

### Repository Pattern
Abstrae el acceso a datos mediante interfaces en Domain y implementaciones en Infrastructure.

```csharp
// Interface en Domain
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
}

// Implementación en Infrastructure.Repository
public class Repository<T> : IRepository<T> where T : class
{
    private readonly SsoContext _context;
    // ... implementación
}
```

### Query Service Pattern
Separa las queries complejas de los repositorios genéricos.

```csharp
public interface IUserQueryService
{
    Task<User?> GetUserByUsernameAsync(string username);
}
```

### Dependency Injection
Todas las dependencias se registran en Program.cs:

```csharp
builder.Services.AddDbContext<SsoContext>(options => ...);
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
```

### Database First
Se utilizan scripts SQL para crear/modificar la base de datos, luego se puede:
1. Crear entidades manualmente basadas en las tablas
2. Usar scaffolding de EF Core para generar entidades

## Principios de Diseño Aplicados

### Clean Architecture
- Dependencias apuntan hacia el centro (Domain)
- Domain no tiene dependencias externas
- Infrastructure depende de Domain, no al revés

### SOLID
- **S**ingle Responsibility: Cada clase tiene una responsabilidad clara
- **O**pen/Closed: Abierto para extensión, cerrado para modificación
- **L**iskov Substitution: Las interfaces se pueden sustituir por implementaciones
- **I**nterface Segregation: Interfaces específicas (IUserQueryService)
- **D**ependency Inversion: Depender de abstracciones, no de implementaciones

### Separation of Concerns
- Cada capa tiene su responsabilidad bien definida
- No hay lógica de negocio en los endpoints
- No hay lógica de acceso a datos en Application

## Convenciones de Código

### Nomenclatura
- Entities: PascalCase (User, Role)
- Interfaces: IPascalCase (IRepository, IUserQueryService)
- DTOs: PascalCaseDto (UserDto)
- Endpoints: Verbos HTTP claros (GetUserByUsername)

### Organización de Archivos
- Una clase por archivo
- Usar carpetas para agrupar funcionalidad relacionada
- Interfaces separadas de implementaciones

### Configuración de Entidades
- Usar Fluent API en OnModelCreating
- No usar Data Annotations para configuración de BD
- Mantener configuración centralizada en SsoContext

## Extensibilidad

### Agregar Nueva Entidad

1. Crear entidad en Domain/Entities
2. Agregar DbSet en SsoContext
3. Configurar en OnModelCreating
4. Crear migración SQL
5. Crear DTO si es necesario
6. Crear endpoints

### Agregar Nuevo Endpoint

1. Crear clase en Host/Endpoints
2. Implementar método de extensión MapXXXEndpoints
3. Registrar en Program.cs

### Agregar Query Especializada

1. Crear interface en Infrastructure.QueryServices/Interfaces
2. Implementar en Infrastructure.QueryServices/Services
3. Registrar en Program.cs
4. Usar en endpoints

## Seguridad

### Consideraciones para Implementar

- [ ] Autenticación (JWT, OAuth, etc.)
- [ ] Autorización basada en roles
- [ ] Validación de entrada
- [ ] Sanitización de datos
- [ ] HTTPS obligatorio
- [ ] Rate limiting
- [ ] CORS configurado apropiadamente
- [ ] Hashing de contraseñas (BCrypt, Argon2)
- [ ] Tokens de sesión seguros
- [ ] Protección CSRF

## Performance

### Optimizaciones Implementadas

- Repositorio genérico con operaciones básicas
- Query Services para consultas complejas con Include
- Indexes en campos frecuentemente consultados (Username, Email)

### Optimizaciones Pendientes

- [ ] Implementar caché (Redis, Memory Cache)
- [ ] Paginación en endpoints que retornan listas
- [ ] Lazy loading vs Eager loading según caso
- [ ] Query optimization con AsNoTracking para lecturas
- [ ] Connection pooling

## Testing

### Estructura Recomendada

```
tests/
├── SSO.Domain.Tests/
├── SSO.Application.Tests/
├── SSO.Infrastructure.Tests/
└── SSO.Host.IntegrationTests/
```

### Tipos de Tests

1. **Unit Tests**: Domain y Application
2. **Integration Tests**: Infrastructure (con base de datos de prueba)
3. **API Tests**: Host (endpoints)

## Recursos y Referencias

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Pomelo MySQL Provider](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
- [DbUp](https://dbup.readthedocs.io/)
- [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
