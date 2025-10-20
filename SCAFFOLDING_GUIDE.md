# Guía de Scaffolding para SSO Project

Este documento describe cómo realizar el scaffolding de la base de datos utilizando el enfoque DB First con Entity Framework Core.

## Prerequisitos

1. MySQL instalado y en ejecución
2. .NET SDK 9.0
3. Herramientas de Entity Framework Core

```bash
dotnet tool install --global dotnet-ef
```

## Configuración de la Base de Datos

### 1. Crear la Base de Datos

Conéctate a MySQL y ejecuta:

```sql
CREATE DATABASE IF NOT EXISTS sso_db
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
```

### 2. Ejecutar Migraciones con DbUp

Las migraciones se pueden ejecutar de dos formas:

#### Opción A: Automático al iniciar la aplicación

En `appsettings.Development.json`, establece:

```json
{
  "RunMigrationsOnStartup": true
}
```

Luego ejecuta la aplicación:

```bash
cd src/SSO.Host
dotnet run
```

#### Opción B: Ejecutar manualmente

Crea un programa de consola temporal o utiliza el `DatabaseMigrationManager` desde código:

```csharp
var connectionString = "Server=localhost;Database=sso_db;User=root;Password=password;";
var migrationManager = new DatabaseMigrationManager(connectionString);
var success = migrationManager.MigrateDatabase();
```

## Scaffolding de la Base de Datos (DB First)

Una vez que la base de datos está creada y las migraciones ejecutadas, puedes hacer scaffolding de las entidades:

### Desde el proyecto Infrastructure.EntityFramework

```bash
cd src/SSO.Infrastructure.EntityFramework

dotnet ef dbcontext scaffold \
  "Server=localhost;Database=sso_db;User=root;Password=password;" \
  Pomelo.EntityFrameworkCore.MySql \
  --output-dir Models \
  --context-dir . \
  --context SsoContext \
  --force \
  --no-onconfiguring
```

### Parámetros del comando:

- `--output-dir Models`: Carpeta donde se generarán las entidades
- `--context-dir .`: Carpeta donde se generará el DbContext
- `--context SsoContext`: Nombre del DbContext
- `--force`: Sobrescribir archivos existentes
- `--no-onconfiguring`: No generar el método OnConfiguring (usaremos configuración desde appsettings.json)

### Notas Importantes

1. **Entidades Existentes**: Si ya tienes entidades definidas en el proyecto Domain, considera si deseas:
   - Mantener las entidades del Domain y no usar las generadas (recomendado)
   - Generar nuevas entidades y moverlas al proyecto Domain
   - Comparar y fusionar las diferencias

2. **Actualizar SsoContext**: Después del scaffolding, revisa y ajusta el `SsoContext` generado para que coincida con tus entidades del proyecto Domain.

3. **Configuración de Navegación**: Asegúrate de configurar correctamente las propiedades de navegación y las relaciones.

## Flujo de Trabajo Recomendado (DB First)

### 1. Diseñar cambios en la base de datos

Crea un nuevo script SQL en `src/SSO.Infrastructure.EntityFramework/Migrations/`:

```sql
-- 002_AddNewFeature.sql
CREATE TABLE IF NOT EXISTS Sessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(255) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_token (Token),
    INDEX idx_user (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 2. Ejecutar migraciones

Ejecuta las migraciones nuevamente para aplicar los cambios:

```bash
cd src/SSO.Host
dotnet run
```

O ejecuta el `DatabaseMigrationManager` manualmente.

### 3. Actualizar entidades

Tienes dos opciones:

#### Opción A: Scaffolding completo (regenerar todo)

```bash
cd src/SSO.Infrastructure.EntityFramework
dotnet ef dbcontext scaffold \
  "Server=localhost;Database=sso_db;User=root;Password=password;" \
  Pomelo.EntityFrameworkCore.MySql \
  --output-dir Models \
  --context-dir . \
  --context SsoContext \
  --force
```

#### Opción B: Crear entidades manualmente

Crea las nuevas entidades en `src/SSO.Domain/Entities/` basándote en la estructura de la tabla:

```csharp
namespace SSO.Domain.Entities;

public class Session : BaseEntity
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    
    public virtual User User { get; set; } = null!;
}
```

### 4. Actualizar DbContext

Agrega el DbSet y la configuración en `SsoContext.cs`:

```csharp
public DbSet<Session> Sessions { get; set; } = null!;

// En OnModelCreating:
modelBuilder.Entity<Session>(entity =>
{
    entity.ToTable("Sessions");
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
    entity.HasIndex(e => e.Token);
    entity.HasOne(e => e.User)
        .WithMany()
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

## Verificar el Modelo

Para verificar que el modelo está correctamente configurado, puedes generar un script de migración de EF Core:

```bash
cd src/SSO.Infrastructure.EntityFramework
dotnet ef migrations add VerifyModel --startup-project ../SSO.Host
```

Esto generará una migración que puedes revisar para ver si EF Core detecta alguna diferencia entre tu modelo y la base de datos.

## Solución de Problemas

### Error de conexión a MySQL

Verifica que:
- MySQL está en ejecución
- Las credenciales son correctas
- El puerto es el correcto (3306 por defecto)
- El usuario tiene permisos en la base de datos

### Versiones de paquetes incompatibles

Asegúrate de que todas las versiones de Entity Framework Core sean consistentes:

```bash
dotnet list package
```

### El scaffolding no genera las entidades correctamente

Verifica que:
- La base de datos existe y tiene tablas
- El usuario tiene permisos de lectura
- La cadena de conexión es correcta
- El provider de Pomelo está instalado

## Recursos Adicionales

- [Documentación de Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
- [Documentación de DbUp](https://dbup.readthedocs.io/)
- [DB First con EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/)
