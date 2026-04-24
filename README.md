# Distribuciones YCT — E-commerce

Plataforma e-commerce de lácteos para las marcas YCT, Mamá Sara y Lácteos Ideal
(Cesar, Magdalena, La Guajira).

- **Frontend:** Angular 19 (standalone + signals)
- **Backend:** .NET 9 Minimal API + MediatR CQRS
- **DB:** SQL Server (EF Core migrations)

---

## Setup en máquina nueva

### 1. Prerrequisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/) y npm
- SQL Server (Express sirve) + SSMS
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### 2. Clonar

```bash
git clone https://github.com/EduardoZ-dev/yct-ecommerce.git
cd yct-ecommerce
```

### 3. Backend

**a) Ajustar connection string** en `Back/YCT.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=YctDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Cambia `Server=` al nombre del SQL Server local (ej. `.\SQLEXPRESS`, `localhost`, o el nombre del equipo).

**b) Crear la BD con migraciones:**

```bash
cd Back/YCT.API
dotnet ef database update --project ../YCT.Infrastructure
```

**c) Cargar datos (productos, categorías, usuario admin):**

```bash
sqlcmd -S .\SQLEXPRESS -d YctDb -E -i ../db-scripts/seed.sql
```

(Ajusta `-S` al servidor. Alternativa: abrir `Back/db-scripts/seed.sql` en SSMS y ejecutar.)

**d) Correr:**

```bash
dotnet run
```

→ http://localhost:5088

### 4. Frontend

```bash
cd Front/yct-frontend
npm install
npm start
```

→ http://localhost:4200

### 5. Credenciales admin

- Usuario: `admin`
- Password: `admin123` (solo dev)

---

## Scripts rápidos

- `dev.bat` (Windows) / `dev.sh` (Unix) — arrancan backend + frontend juntos.
