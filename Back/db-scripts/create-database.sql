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
-- =========================================================
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
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
-- admin / admin123  (SHA256)
INSERT INTO [Users] ([Id],[Username],[PasswordHash],[FullName],[Email],[Phone],[Role],[IsActive],[CreatedAt],[UpdatedAt]) VALUES (1, N'admin', N'240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', N'Administrador', N'admin@yct.com', N'', N'Admin', 1, '2026-04-08 23:59:00.796', NULL);
SET IDENTITY_INSERT [Users] OFF;
GO

-- Products
SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (1, N'Leche Entera Super Cremosa 1100ml', N'Leche entera con todo el sabor y cremosidad natural. La favorita de toda la familia.', 4.50, 200, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/MEGALITRO_LECHE_SUPERCREMOSA_opt2_f5ec2cc356.png', 0, 1, '2026-04-09 01:08:54.010', NULL, N'YCT', 240.00, 120.00, 20.00, N'15 dias refrigerado sin abrir', N'Leche entera pasteurizada, vitaminas A y D3', 0.00, 6.40, 3.50, N'200 ml', 105.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 3 dias despues de abierto.', 11.00, 11.00, 6.00, 2.50, N'1100 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (2, N'Leche Entera Super Cremosa 900ml', N'Leche entera pasteurizada, fuente natural de calcio y proteinas.', 3.80, 180, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/MEGALITRO_LECHE_SUPERCREMOSA_opt2_f5ec2cc356.png', 0, 1, '2026-04-09 01:08:54.010', NULL, N'YCT', 240.00, 120.00, 20.00, N'12 dias refrigerado sin abrir', N'Leche entera pasteurizada, vitaminas A y D3', 0.00, 6.40, 3.50, N'200 ml', 105.00, N'Mantener refrigerado entre 2-6 C.', 11.00, 11.00, 6.00, 2.50, N'900 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (3, N'Leche Entera Super Cremosa 450ml', N'Presentacion personal, perfecta para llevar.', 2.20, 250, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/MEGALITRO_LECHE_SUPERCREMOSA_opt2_f5ec2cc356.png', 0, 1, '2026-04-09 01:08:54.010', NULL, N'YCT', 240.00, 120.00, 20.00, N'10 dias refrigerado sin abrir', N'Leche entera pasteurizada, vitaminas A y D3', 0.00, 6.40, 3.50, N'200 ml', 105.00, N'Mantener refrigerado entre 2-6 C.', 11.00, 11.00, 6.00, 2.50, N'450 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (4, N'Leche Deslactosada Semidescremada 1100ml', N'Sin lactosa, facil de digerir y con gran sabor cremoso.', 5.20, 150, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Leche_Deslactosada_Megalitro_1100_m_L_28c7e7f0e5.png', 0, 2, '2026-04-09 01:08:54.010', NULL, N'YCT', 250.00, 90.00, 10.00, N'15 dias refrigerado sin abrir', N'Leche semidescremada pasteurizada, enzima lactasa, vitaminas A y D3', 0.00, 6.50, 1.50, N'200 ml', 110.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 3 dias despues de abierto.', 11.00, 11.00, 2.50, 2.50, N'1100 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (5, N'Leche Deslactosada Descremada 1100ml', N'Deslactosada 0% grasa, libre de lactosa y ligera.', 5.50, 120, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Leche_Deslactosada_0_Grasa_Megalitro_1100_m_L_548874bc3a.png', 0, 2, '2026-04-09 01:08:54.010', NULL, N'YCT', 280.00, 70.00, 5.00, N'15 dias refrigerado sin abrir', N'Leche descremada pasteurizada, enzima lactasa, vitaminas A y D3, calcio', 0.00, 7.00, 0.00, N'200 ml', 115.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 3 dias despues de abierto.', 10.00, 10.00, 0.00, 3.00, N'1100 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (6, N'Leche Deslactosada Semidescremada 450ml', N'Presentacion personal deslactosada.', 3.10, 180, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Leche_Deslactosada_Megalitro_1100_m_L_28c7e7f0e5.png', 0, 2, '2026-04-09 01:08:54.010', NULL, N'YCT', 250.00, 90.00, 10.00, N'10 dias refrigerado sin abrir', N'Leche semidescremada pasteurizada, enzima lactasa, vitaminas A y D3', 0.00, 6.50, 1.50, N'200 ml', 110.00, N'Mantener refrigerado entre 2-6 C.', 11.00, 11.00, 2.50, 2.50, N'450 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (7, N'Leche Descremada 0% Grasa 1100ml', N'Sin grasa, ideal para quienes cuidan su alimentacion.', 4.25, 130, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/LECHEALQUERIA_Descremada_0_Grasa_opt_a61ab5b884.png', 0, 3, '2026-04-09 01:08:54.010', NULL, N'YCT', 260.00, 70.00, 5.00, N'15 dias refrigerado sin abrir', N'Leche descremada pasteurizada, vitaminas A y D3, calcio', 0.00, 7.00, 0.00, N'200 ml', 110.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 3 dias despues de abierto.', 10.00, 10.00, 0.00, 2.50, N'1100 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (8, N'Leche Descremada 0% Grasa 450ml', N'Presentacion personal descremada.', 2.50, 160, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/LECHEALQUERIA_Descremada_0_Grasa_opt_a61ab5b884.png', 0, 3, '2026-04-09 01:08:54.010', NULL, N'YCT', 260.00, 70.00, 5.00, N'10 dias refrigerado sin abrir', N'Leche descremada pasteurizada, vitaminas A y D3, calcio', 0.00, 7.00, 0.00, N'200 ml', 110.00, N'Mantener refrigerado entre 2-6 C.', 10.00, 10.00, 0.00, 2.50, N'450 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (9, N'Yogur Vaso Fresa 150g', N'Yogur cremoso sabor fresa con fruta natural. Probioticos para tu digestion.', 1.80, 300, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/RENDER_2023_VASO_YOGURT_FRESA_opt_dac5384aa5.png', 0, 4, '2026-04-09 01:08:54.010', NULL, N'YCT', 180.00, 130.00, 10.00, N'21 dias refrigerado', N'Leche semidescremada, azucar, fresa (8%), cultivos probioticos (Lactobacillus, Bifidobacterium), pectina', 0.20, 5.00, 1.50, N'150 g', 60.00, N'Mantener refrigerado entre 2-6 C.', 18.00, 22.00, 2.50, 0.80, N'150 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (10, N'Yogur Vaso Mora 150g', N'Yogur cremoso sabor mora con frutos naturales. Cremosidad y frescura.', 1.80, 280, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/RENDER_2023_VASO_YOGURT_MORA_opt_b89ff4c815.png', 0, 4, '2026-04-09 01:08:54.010', NULL, N'YCT', 190.00, 135.00, 8.00, N'21 dias refrigerado', N'Leche semidescremada, azucar, mora (6%), cultivos probioticos, pectina, color natural', 0.30, 5.50, 1.20, N'150 g', 65.00, N'Mantener refrigerado entre 2-6 C.', 20.00, 24.00, 2.00, 0.50, N'150 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (11, N'Yogur Vaso Melocoton 150g', N'Yogur cremoso sabor melocoton con trozos de fruta. Delicioso y nutritivo.', 1.80, 260, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/RENDER_2023_VASO_YOGURT_MELOCOTON_opt_c56cdb617e.png', 0, 4, '2026-04-09 01:08:54.010', NULL, N'YCT', 175.00, 128.00, 8.00, N'21 dias refrigerado', N'Leche semidescremada, azucar, melocoton (7%), cultivos probioticos, pectina', 0.20, 5.00, 1.20, N'150 g', 58.00, N'Mantener refrigerado entre 2-6 C.', 17.00, 22.00, 2.00, 0.60, N'150 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (12, N'Yogur Natural 1000ml', N'Yogur natural sin azucar, versatil para recetas, smoothies y bowls.', 4.20, 90, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/RENDER_2023_VASO_YOGURT_FRESA_opt_dac5384aa5.png', 0, 4, '2026-04-09 01:08:54.010', NULL, N'YCT', 250.00, 100.00, 8.00, N'28 dias refrigerado sin abrir', N'Leche semidescremada, cultivos lacticos (Streptococcus thermophilus, Lactobacillus bulgaricus)', 0.10, 7.50, 1.20, N'200 ml', 80.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 5 dias despues de abierto.', 10.00, 12.00, 2.00, 1.00, N'1000 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (13, N'Cremosito Bebible Fresa 200g', N'Alimento lacteo cremoso y bebible sabor fresa. Ideal para la lonchera.', 1.50, 350, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/cremosito_opt_26da03ea67.webp', 0, 4, '2026-04-09 01:08:54.010', NULL, N'YCT', 150.00, 140.00, 5.00, N'21 dias refrigerado', N'Leche descremada, azucar, fresa, almidon modificado, cultivos lacticos', 0.10, 4.00, 1.00, N'200 g', 70.00, N'Mantener refrigerado entre 2-6 C.', 22.00, 26.00, 1.50, 0.50, N'200 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (14, N'Crema de Leche Original 180g', N'Crema de leche natural, perfecta para cocinar, postres y salsas.', 2.80, 150, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Crema_roja_180_g_PEC_PAL_opt_7941725080.webp', 0, 5, '2026-04-09 01:08:54.010', NULL, N'YCT', 20.00, 90.00, 30.00, N'30 dias refrigerado sin abrir', N'Crema de leche pasteurizada, estabilizantes (carragenina, goma guar)', 0.00, 0.80, 5.50, N'30 g', 15.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 5 dias despues de abierto.', 1.00, 1.50, 9.00, 0.30, N'180 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (15, N'Crema de Leche para Cocina 170g', N'Crema especial para preparaciones calientes. No se corta al hervir.', 3.20, 120, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Crema_de_Leche_Preparaciones_Calientes_Film_170g_opt_f19e5b5ca2.webp', 0, 5, '2026-04-09 01:08:54.010', NULL, N'YCT', 25.00, 80.00, 25.00, N'30 dias refrigerado sin abrir', N'Crema de leche pasteurizada, almidon modificado, estabilizantes', 0.00, 1.00, 4.50, N'30 g', 20.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 5 dias despues de abierto.', 1.50, 3.00, 7.50, 0.20, N'170 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (16, N'Crema de Leche Libre 180g', N'Crema con menos grasa, ideal para recetas ligeras sin perder cremosidad.', 3.50, 100, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Crema_Libre180g_8fec9ec9d0.webp', 0, 5, '2026-04-09 01:08:54.010', NULL, N'YCT', 25.00, 50.00, 15.00, N'30 dias refrigerado sin abrir', N'Crema de leche descremada pasteurizada, estabilizantes, enzima lactasa', 0.00, 1.00, 2.80, N'30 g', 20.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 5 dias despues de abierto.', 1.50, 2.00, 4.50, 0.20, N'180 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (17, N'Queso con Bocadillo 50g', N'Combinacion clasica colombiana: queso fresco con bocadillo de guayaba.', 1.50, 400, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Bocadillo_opt_6fc1260380.png', 0, 6, '2026-04-09 01:08:54.010', NULL, N'YCT', 180.00, 145.00, 20.00, N'20 dias refrigerado', N'Queso fresco (leche pasteurizada, cuajo, cloruro de calcio, sal), bocadillo (guayaba, azucar, acido citrico)', 0.30, 7.00, 3.50, N'50 g', 250.00, N'Mantener refrigerado entre 2-6 C.', 12.00, 15.00, 6.00, 0.00, N'50 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (18, N'Queso Campesino 350g', N'Queso fresco campesino, suave y perfecto para acompanar tus comidas.', 6.50, 80, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Bocadillo_opt_6fc1260380.png', 0, 6, '2026-04-09 01:08:54.010', NULL, N'YCT', 190.00, 80.00, 18.00, N'30 dias refrigerado sin abrir', N'Leche pasteurizada, cuajo microbiano, cloruro de calcio, sal', 0.10, 6.50, 3.50, N'30 g', 200.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 7 dias despues de abierto.', 0.50, 1.00, 5.50, 0.20, N'350 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (19, N'Queso Doble Crema 250g', N'Queso doble crema, ideal para fundir en arepas y gratinar.', 5.80, 90, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Bocadillo_opt_6fc1260380.png', 0, 6, '2026-04-09 01:08:54.010', NULL, N'YCT', 170.00, 95.00, 22.00, N'25 dias refrigerado sin abrir', N'Leche pasteurizada, crema de leche, cuajo microbiano, cloruro de calcio, sal', 0.10, 6.00, 4.50, N'30 g', 180.00, N'Mantener refrigerado entre 2-6 C. Ideal para fundir.', 0.30, 0.80, 7.50, 0.20, N'250 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (20, N'Queso Mozzarella 400g', N'Queso mozzarella rallado, perfecto para pizzas, pastas y gratinados.', 7.90, 65, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Bocadillo_opt_6fc1260380.png', 0, 6, '2026-04-09 01:08:54.010', NULL, N'YCT', 200.00, 85.00, 20.00, N'30 dias refrigerado sin abrir', N'Leche pasteurizada, cuajo, cloruro de calcio, sal, acido citrico', 0.10, 7.00, 3.80, N'30 g', 220.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 5 dias despues de abierto.', 0.20, 0.50, 6.00, 0.10, N'400 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (21, N'Avena Pro Autentica 200ml', N'Bebida de avena con leche, cremosa y con sabor a canela. Energia natural.', 1.80, 200, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Avena_Pro_Autentica_opt_907e51e5a2.png', 0, 7, '2026-04-09 01:08:54.010', NULL, N'YCT', 180.00, 150.00, 8.00, N'15 dias refrigerado', N'Leche semidescremada, avena (12%), azucar, canela, saborizante natural', 1.50, 5.00, 1.00, N'200 ml', 90.00, N'Mantener refrigerado entre 2-6 C. Agitar antes de consumir.', 16.00, 26.00, 2.50, 1.00, N'200 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (22, N'Avena Pro Cubana 200ml', N'Bebida de avena estilo cubano, con leche condensada y toque de vainilla.', 1.90, 180, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Avena_Pro_Cubana_opt_994b9ea03c.png', 0, 7, '2026-04-09 01:08:54.010', NULL, N'YCT', 190.00, 165.00, 10.00, N'15 dias refrigerado', N'Leche semidescremada, avena (10%), leche condensada, vainilla, canela', 1.50, 5.50, 1.20, N'200 ml', 95.00, N'Mantener refrigerado entre 2-6 C. Agitar antes de consumir.', 20.00, 28.00, 2.80, 1.00, N'200 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (23, N'Avena Pro Autentica 1000ml', N'Avena familiar, ideal para compartir en el desayuno.', 5.50, 100, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Avena_Pro_Autentica_opt_907e51e5a2.png', 0, 7, '2026-04-09 01:08:54.010', NULL, N'YCT', 180.00, 150.00, 8.00, N'15 dias refrigerado sin abrir', N'Leche semidescremada, avena (12%), azucar, canela, saborizante natural', 1.50, 5.00, 1.00, N'200 ml', 90.00, N'Mantener refrigerado entre 2-6 C. Consumir dentro de 3 dias despues de abierto.', 16.00, 26.00, 2.50, 1.00, N'1000 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (24, N'Arequipe Tradicional 150g', N'Dulce de leche artesanal, cremoso y perfecto para untar o postres.', 3.50, 150, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Render_Arequipe_Tradicional_150g_OK_opt_44ab17c541.png', 0, 8, '2026-04-09 01:08:54.010', NULL, N'YCT', 60.00, 85.00, 8.00, N'90 dias sin abrir', N'Leche entera, azucar, glucosa, bicarbonato de sodio, vanillina', 0.10, 2.00, 1.20, N'30 g', 40.00, N'Mantener en lugar fresco y seco. Refrigerar despues de abierto.', 14.00, 15.00, 2.00, 0.00, N'150 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (25, N'Arequipe Tradicional 50g', N'Porcion individual de arequipe, ideal para la lonchera o snack.', 1.20, 300, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/Render_Arequipe_Tradicional_150g_OK_opt_44ab17c541.png', 0, 8, '2026-04-09 01:08:54.010', NULL, N'YCT', 100.00, 140.00, 12.00, N'90 dias sin abrir', N'Leche entera, azucar, glucosa, bicarbonato de sodio, vanillina', 0.10, 3.50, 2.00, N'50 g', 65.00, N'Mantener en lugar fresco y seco.', 23.00, 25.00, 3.50, 0.00, N'50 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (26, N'Gelatina de Fresa 120g', N'Gelatina de fresa con leche, postre fresco y delicioso.', 1.20, 250, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/gelatina_product_opt_1c021e0108.png', 0, 8, '2026-04-09 01:08:54.010', NULL, N'YCT', 40.00, 80.00, 2.00, N'30 dias refrigerado', N'Agua, leche, azucar, gelatina, saborizante de fresa, color rojo 40, acido citrico', 0.00, 2.50, 0.30, N'120 g', 30.00, N'Mantener refrigerado entre 2-6 C.', 15.00, 17.00, 0.50, 0.00, N'120 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (27, N'Gelatina de Uva 120g', N'Gelatina de uva con leche, textura suave y refrescante.', 1.20, 230, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/gelatina_product_opt_1c021e0108.png', 0, 8, '2026-04-09 01:08:54.010', NULL, N'YCT', 40.00, 80.00, 2.00, N'30 dias refrigerado', N'Agua, leche, azucar, gelatina, saborizante de uva, color violeta, acido citrico', 0.00, 2.50, 0.30, N'120 g', 30.00, N'Mantener refrigerado entre 2-6 C.', 15.00, 17.00, 0.50, 0.00, N'120 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (28, N'AlqueMIX Flips 170g', N'Bebida lactea con cereal crujiente, snack nutritivo y divertido.', 2.50, 200, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/RENDER_ALQUEMIX_FLIPS_170_G_opt_19c007fc8a.png', 0, 9, '2026-04-09 01:08:54.010', NULL, N'YCT', 200.00, 180.00, 10.00, N'15 dias refrigerado', N'Leche semidescremada, cereal de maiz, azucar, saborizante, vitaminas y minerales', 2.50, 6.00, 2.00, N'170 g', 150.00, N'Mantener refrigerado entre 2-6 C.', 18.00, 30.00, 3.50, 1.50, N'170 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (29, N'Fortikids Chocoleche 180ml', N'Leche saborizada con chocolate, enriquecida con vitaminas y calcio. Ideal para ninos.', 1.60, 350, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/TETRAPACK_CHOCOLECHE_FORTIKIDS_180_G_Opt_128396e3e2.png', 0, 9, '2026-04-09 01:08:54.010', NULL, N'YCT', 220.00, 160.00, 10.00, N'120 dias sin abrir', N'Leche semidescremada, azucar, cocoa, lecitina de soya, vitaminas A, D3, B6, B12, hierro, zinc', 2.00, 6.00, 1.80, N'180 ml', 120.00, N'Mantener en lugar fresco y seco. Refrigerar despues de abierto.', 20.00, 25.00, 3.00, 2.00, N'180 ml');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (30, N'ACTIGEST Fresa 140g', N'Yogur probiotico sabor fresa. Cuida tu digestion con cada vaso.', 1.90, 250, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/RENDER_ACTIGEST_VASO_FRESA_140_g_opt_cd7ac4e33c.png', 0, 9, '2026-04-09 01:08:54.010', NULL, N'YCT', 190.00, 110.00, 8.00, N'21 dias refrigerado', N'Leche semidescremada, azucar, fresa, cultivos probioticos (L. acidophilus, B. lactis), inulina, pectina', 0.20, 5.50, 1.00, N'140 g', 55.00, N'Mantener refrigerado entre 2-6 C.', 15.00, 18.00, 2.00, 1.00, N'140 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (31, N'ACTIGEST Melocoton 140g', N'Yogur probiotico sabor melocoton. Favorece tu bienestar digestivo.', 1.90, 220, N'https://alqueriauploads.s3.us-east-1.amazonaws.com/prod/RENDER_ACTIGEST_VASO_FRESA_140_g_opt_cd7ac4e33c.png', 0, 9, '2026-04-09 01:08:54.010', NULL, N'YCT', 185.00, 108.00, 8.00, N'21 dias refrigerado', N'Leche semidescremada, azucar, melocoton, cultivos probioticos (L. acidophilus, B. lactis), inulina', 0.20, 5.50, 1.00, N'140 g', 55.00, N'Mantener refrigerado entre 2-6 C.', 14.00, 17.00, 2.00, 1.00, N'140 g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (32, N'Quesillo Tajado 400g', N'Quesillo tajado tipo mozzarella, elaborado con leche entera. Queso fresco graso, semiduro, ideal para sÃ¡ndwiches, pizzas, horneados y acompaÃ±ar todo tipo de comidas.', 6000.00, 150, N'assets/products/yct/quesillo-tajado-400g.png', 1, 6, '2026-04-14 20:51:10.673', NULL, N'YCT', 730.00, 392.00, 78.00, N'Producto refrigerado. Ver fecha impresa en el empaque.', N'Leche entera pasteurizada, sal, cuajo y cultivos lÃ¡cticos.', 0.40, 24.00, 20.00, N'100 g', 638.00, N'Conservar refrigerado entre 2Â°C y 8Â°C. Una vez abierto, consumir mÃ¡ximo en 5 dÃ­as.', 0.00, 1.00, 31.00, NULL, N'400g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (33, N'Quesillo Tipo Cheddar 500g', N'Quesillo tipo cheddar, queso fresco graso y semiduro. Empacado al vacÃ­o con 20 tajadas individuales. Perfecto para sÃ¡ndwiches, hamburguesas, pastas y gratinados.', 8500.00, 120, N'assets/products/yct/quesillo-tipo-chedar-500g.png', 1, 6, '2026-04-14 20:51:10.673', NULL, N'YCT', 500.00, 260.00, 60.00, N'Producto refrigerado. Ver fecha impresa en el empaque.', N'Leche pasteurizada, sal, cuajo, cultivos lÃ¡cticos y colorante natural (annatto).', 0.30, 21.00, 12.00, N'100 g', 800.00, N'Conservar refrigerado entre 2Â°C y 8Â°C. Empacado al vacÃ­o.', 0.00, 2.00, 19.00, NULL, N'500g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (34, N'Suero CosteÃ±o 400g', N'Suero costeÃ±o tradicional, ligeramente Ã¡cido y cremoso. El acompaÃ±ante ideal para arepas, yucas, bollos y patacones. Receta autÃ©ntica de la costa colombiana, elaborado artesanalmente.', 5000.00, 180, N'assets/products/yct/suero-costenio-400g.png', 1, 5, '2026-04-14 20:51:10.673', NULL, N'YCT', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'400g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (35, N'Quesillo LÃ¡cteos Ideal 2500g', N'Quesillo entero de 2,5 kg, ideal para negocios y pedidos al por mayor. Textura firme y sabor fresco tradicional, perfecto para tajar segÃºn la necesidad del cliente.', 32000.00, 60, N'assets/products/lacteos-ideal/quesillo-2500g.png', 1, 6, '2026-04-21 12:51:17.100', NULL, N'LÃ¡cteos Ideal', 517.00, 293.00, NULL, N'Registro Invima RSA-0036037-2025. Ver fecha de vencimiento impresa en el empaque.', N'Leche entera, sal, cuajo y fermentos lÃ¡cticos.', NULL, 23.00, 13.00, N'100 g', 373.00, N'ConsÃ©rvese refrigerado entre 2Â°C y 8Â°C. Empacado al vacÃ­o. Una vez abierto, consumir en mÃ¡ximo 7 dÃ­as.', 0.00, 3.10, 21.00, NULL, N'2500g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (36, N'Mozzarella MamÃ¡ Sara 2500g', N'Queso mozzarella de 2,5 kg, tipo deli, con textura suave y excelente fundido. Ideal para pizzerÃ­as, restaurantes y preparaciones al horno.', 38000.00, 50, N'assets/products/mama-sara/mozzarella-2500g.png', 1, 6, '2026-04-21 12:51:17.100', NULL, N'MamÃ¡ Sara', 517.00, 293.00, NULL, N'Registro Invima RSA-0036037-2025. Ver fecha de vencimiento impresa en el empaque.', N'Leche entera, sal, cuajo y fermentos lÃ¡cticos.', NULL, 23.00, 13.00, N'100 g', 373.00, N'ConsÃ©rvese refrigerado entre 2Â°C y 8Â°C. Empacado al vacÃ­o. Una vez abierto, consumir en mÃ¡ximo 7 dÃ­as.', 0.00, 3.10, 21.00, NULL, N'2500g');
INSERT INTO [Products] ([Id],[Name],[Description],[Price],[Stock],[ImageUrl],[IsActive],[CategoryId],[CreatedAt],[UpdatedAt],[Brand],[Calcium],[Calories],[Cholesterol],[ExpirationInfo],[Ingredients],[Iron],[Protein],[SaturatedFat],[ServingSize],[Sodium],[StorageInstructions],[Sugars],[TotalCarbs],[TotalFat],[VitaminD],[Weight]) VALUES (37, N'Queso Mozzarella YCT 2000g', N'Queso mozzarella YCT de 2 kg, frescura garantizada con cadena de frÃ­o. Producto estrella para cocina profesional y para familias grandes.', 32000.00, 80, N'assets/products/yct/mozzarella-2000g.png', 1, 6, '2026-04-21 12:51:17.100', NULL, N'YCT', 517.00, 293.00, NULL, N'Registro Invima RSA-0036037-2025. Ver fecha de vencimiento impresa en el empaque.', N'Leche entera, sal, cuajo y fermentos lÃ¡cticos.', NULL, 23.00, 13.00, N'100 g', 373.00, N'ConsÃ©rvese refrigerado entre 2Â°C y 8Â°C. Empacado al vacÃ­o. Una vez abierto, consumir en mÃ¡ximo 7 dÃ­as.', 0.00, 3.10, 21.00, NULL, N'2000g');
SET IDENTITY_INSERT [Products] OFF;
GO


