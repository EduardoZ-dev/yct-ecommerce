# Distribuciones YCT — E-commerce

Plataforma e-commerce de lácteos para las marcas YCT, Mamá Sara y Lácteos Ideal
(Cesar, Magdalena, La Guajira).

- **Frontend:** Angular 19 (standalone + signals)
- **Backend:** .NET 9 Minimal API + MediatR CQRS
- **DB:** SQL Server (EF Core migrations)

---

## Prerrequisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- SQL Server instalado (cualquier edición)

---

## Setup

```bash
git clone https://github.com/EduardoZ-dev/yct-ecommerce.git
cd yct-ecommerce
```

### 1. Configurar connection string

Edita `Back/YCT.API/appsettings.json` y ajusta `ConnectionStrings.DefaultConnection`
con el nombre de tu instancia local de SQL Server:

```json
"DefaultConnection": "Server=TU_INSTANCIA;Database=YctDb;Trusted_Connection=True;TrustServerCertificate=True"
```

### 2. Instalar dotnet-ef (si no lo tienes)

```bash
dotnet tool install --global dotnet-ef
```

### 3. Crear la BD y aplicar migraciones

```bash
cd Back/YCT.API
dotnet ef database update --project ../YCT.Infrastructure
cd ../..
```

### 4. Cargar datos semilla (9 categorías, 37 productos, admin)

```bash
sqlcmd -S TU_INSTANCIA -d YctDb -E -i Back/db-scripts/seed.sql
```

### 5. Instalar dependencias del frontend

```bash
cd Front/yct-frontend
npm install
cd ../..
```

---

## Arrancar los servidores

```bash
dev.bat
```

Abre backend en http://localhost:5088 y frontend en http://localhost:4200.

## Credenciales admin
- Usuario: `admin`
- Password: `admin123`
