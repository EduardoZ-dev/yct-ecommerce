-- =========================================================
--  YCT - Script TODO EN UNO para SQL Server
--  Ejecutar en SSMS (New Query), presiona F5
--  Crea la BD YctDb, las tablas y carga los datos semilla
-- =========================================================

IF DB_ID('YctDb') IS NULL
BEGIN
    CREATE DATABASE YctDb;
END
GO

USE YctDb;
GO

-- =========================================================
--  ESQUEMA (tablas, indices, FKs)
-- =========================================================IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(256) NOT NULL,
        [FullName] nvarchar(150) NOT NULL,
        [Email] nvarchar(200) NULL,
        [Phone] nvarchar(20) NULL,
        [Role] nvarchar(30) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Price] decimal(18,2) NOT NULL,
        [Stock] int NOT NULL,
        [ImageUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CategoryId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [OrderNumber] nvarchar(50) NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [ShippingAddress] nvarchar(300) NULL,
        [UserId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderDetails] (
        [Id] int NOT NULL IDENTITY,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_OrderId] ON [OrderDetails] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_ProductId] ON [OrderDetails] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408235822_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260408235822_InitialCreate', N'9.0.14');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Brand] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Calcium] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Calories] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Cholesterol] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [ExpirationInfo] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Ingredients] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Iron] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Protein] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [SaturatedFat] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [ServingSize] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Sodium] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [StorageInstructions] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Sugars] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [TotalCarbs] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [TotalFat] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [VitaminD] decimal(8,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    ALTER TABLE [Products] ADD [Weight] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409004935_AddNutritionalInfo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260409004935_AddNutritionalInfo', N'9.0.14');
END;

COMMIT;
GO


-- =========================================================
--  DATOS SEMILLA
-- =========================================================

-- Categories
SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (1, N'Leches Enteras', N'Leche entera con toda su cremosidad natural', 1, '2026-04-09 01:08:54.006', NULL);
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (2, N'Leches Deslactosadas', N'Leche sin lactosa, facil digestion', 1, '2026-04-09 01:08:54.006', NULL);
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (3, N'Leches Descremadas', N'Leche con 0% grasa, ligera y nutritiva', 1, '2026-04-09 01:08:54.006', NULL);
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (4, N'Yogures', N'Yogures cremosos con frutas naturales', 1, '2026-04-09 01:08:54.006', NULL);
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (5, N'Cremas de Leche', N'Cremas para cocinar y reposteria', 1, '2026-04-09 01:08:54.006', NULL);
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (6, N'Quesos', N'Quesos frescos y madurados', 1, '2026-04-09 01:08:54.006', NULL);
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (7, N'Avenas', N'Bebidas de avena con leche', 1, '2026-04-09 01:08:54.006', NULL);
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (8, N'Postres y Arequipe', N'Dulces, gelatinas y arequipe artesanal', 1, '2026-04-09 01:08:54.006', NULL);
INSERT INTO [Categories] ([Id],[Name],[Description],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (9, N'Bebidas Lacteas', N'Mezclas, chocoleche y bebidas funcionales', 1, '2026-04-09 01:08:54.006', NULL);
SET IDENTITY_INSERT [Categories] OFF;
GO

-- Users
SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id],[Username],[PasswordHash],[FullName],[Email],[Phone],[Role],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (1, N'admin', N'240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', N'Administrador', N'admin@yct.com', N'', N'Admin', 1, '2026-04-08 23:59:00.796', NULL);
SET IDENTITY_INSERT [Users] OFF;
GO

-- Products
SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (32, N'Quesillo Tajado 400g', N'Quesillo tajado tipo mozzarella, elaborado con leche entera. Queso fresco graso, semiduro, ideal para sándwiches, pizzas, horneados y acompañar todo tipo de comidas.', 6000.00, 150, N'assets/products/yct/quesillo-tajado-400g.png', 1, 6, '2026-04-14 20:51:10.673', NULL, N'YCT', 730.00, 392.00, 78.00, N'Producto refrigerado. Ver fecha impresa en el empaque.', N'Leche entera pasteurizada, sal, cuajo y cultivos lácticos.', 0.40, 24.00, 20.00, N'100 g', 638.00, N'Conservar refrigerado entre 2°C y 8°C. Una vez abierto, consumir máximo en 5 días.', 0.00, 1.00, 31.00, NULL, N'400g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (33, N'Quesillo Tipo Cheddar 500g', N'Quesillo tipo cheddar, queso fresco graso y semiduro. Empacado al vacío con 20 tajadas individuales. Perfecto para sándwiches, hamburguesas, pastas y gratinados.', 8500.00, 120, N'assets/products/yct/quesillo-tipo-chedar-500g.png', 1, 6, '2026-04-14 20:51:10.673', NULL, N'YCT', 500.00, 260.00, 60.00, N'Producto refrigerado. Ver fecha impresa en el empaque.', N'Leche pasteurizada, sal, cuajo, cultivos lácticos y colorante natural (annatto).', 0.30, 21.00, 12.00, N'100 g', 800.00, N'Conservar refrigerado entre 2°C y 8°C. Empacado al vacío.', 0.00, 2.00, 19.00, NULL, N'500g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (34, N'Suero Costeño 400g', N'Suero costeño tradicional, ligeramente ácido y cremoso. El acompañante ideal para arepas, yucas, bollos y patacones. Receta auténtica de la costa colombiana, elaborado artesanalmente.', 5000.00, 180, N'assets/products/yct/suero-costenio-400g.png', 1, 5, '2026-04-14 20:51:10.673', NULL, N'YCT', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'400g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (35, N'Quesillo Lácteos Ideal 2500g', N'Quesillo entero de 2,5 kg, ideal para negocios y pedidos al por mayor. Textura firme y sabor fresco tradicional, perfecto para tajar según la necesidad del cliente.', 32000.00, 60, N'assets/products/lacteos-ideal/quesillo-2500g.png', 1, 6, '2026-04-21 12:51:17.100', NULL, N'Lácteos Ideal', 517.00, 293.00, NULL, N'Registro Invima RSA-0036037-2025. Ver fecha de vencimiento impresa en el empaque.', N'Leche entera, sal, cuajo y fermentos lácticos.', NULL, 23.00, 13.00, N'100 g', 373.00, N'Consérvese refrigerado entre 2°C y 8°C. Empacado al vacío. Una vez abierto, consumir en máximo 7 días.', 0.00, 3.10, 21.00, NULL, N'2500g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (36, N'Mozzarella Mamá Sara 2500g', N'Queso mozzarella de 2,5 kg, tipo deli, con textura suave y excelente fundido. Ideal para pizzerías, restaurantes y preparaciones al horno.', 38000.00, 50, N'assets/products/mama-sara/mozzarella-2500g.png', 1, 6, '2026-04-21 12:51:17.100', NULL, N'Mamá Sara', 517.00, 293.00, NULL, N'Registro Invima RSA-0036037-2025. Ver fecha de vencimiento impresa en el empaque.', N'Leche entera, sal, cuajo y fermentos lácticos.', NULL, 23.00, 13.00, N'100 g', 373.00, N'Consérvese refrigerado entre 2°C y 8°C. Empacado al vacío. Una vez abierto, consumir en máximo 7 días.', 0.00, 3.10, 21.00, NULL, N'2500g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (37, N'Queso Mozzarella YCT 2000g', N'Queso mozzarella YCT de 2 kg, frescura garantizada con cadena de frío. Producto estrella para cocina profesional y para familias grandes.', 32000.00, 80, N'assets/products/yct/mozzarella-2000g.png', 1, 6, '2026-04-21 12:51:17.100', NULL, N'YCT', 517.00, 293.00, NULL, N'Registro Invima RSA-0036037-2025. Ver fecha de vencimiento impresa en el empaque.', N'Leche entera, sal, cuajo y fermentos lácticos.', NULL, 23.00, 13.00, N'100 g', 373.00, N'Consérvese refrigerado entre 2°C y 8°C. Empacado al vacío. Una vez abierto, consumir en máximo 7 días.', 0.00, 3.10, 21.00, NULL, N'2000g');
SET IDENTITY_INSERT [Products] OFF;
GO

