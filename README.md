# Distribuciones YCT — E-commerce

Plataforma e-commerce de lácteos para las marcas YCT, Mamá Sara y Lácteos Ideal
(Cesar, Magdalena, La Guajira).

- **Frontend:** Angular 19 (standalone + signals)
- **Backend:** .NET 9 Minimal API + MediatR CQRS
- **DB:** SQL Server (EF Core migrations)

---

## Setup en portátil nuevo (1 comando)

### Prerrequisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- SQL Server instalado (cualquier edición)

### Pasos

```bash
git clone https://github.com/EduardoZ-dev/yct-ecommerce.git
cd yct-ecommerce
setup-laptop.bat
```

Ese script hace todo automático:
1. Apunta el backend a `Server=JOSEDAZA`
2. Instala `dotnet-ef` si falta
3. Crea la BD `YctDb` y aplica migraciones
4. Carga datos semilla (9 categorías, 37 productos, admin)
5. Corre `npm install`

### Arrancar los servidores

```bash
dev.bat
```
Abre backend en http://localhost:5088 y frontend en http://localhost:4200

### Credenciales admin
- Usuario: `admin`
- Password: `admin123`

---

### Si el nombre del SQL Server cambia

Edita la línea `set "SQL_SERVER=JOSEDAZA"` al inicio de `setup-laptop.bat` antes de ejecutarlo.
