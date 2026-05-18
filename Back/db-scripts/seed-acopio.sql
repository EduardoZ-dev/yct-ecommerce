SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE [YctDb];
GO

DELETE FROM acopio.Recogidas;
DELETE FROM acopio.Rutas;
DELETE FROM acopio.Granjeros;
DELETE FROM acopio.Asistentes;
DELETE FROM acopio.Conductores;
DELETE FROM acopio.Camiones;
DBCC CHECKIDENT('acopio.Recogidas', RESEED, 0);
DBCC CHECKIDENT('acopio.Rutas', RESEED, 0);
DBCC CHECKIDENT('acopio.Granjeros', RESEED, 0);
DBCC CHECKIDENT('acopio.Asistentes', RESEED, 0);
DBCC CHECKIDENT('acopio.Conductores', RESEED, 0);
DBCC CHECKIDENT('acopio.Camiones', RESEED, 0);
GO

-- Camiones (sin IsActive)
SET IDENTITY_INSERT acopio.Camiones ON;
INSERT INTO acopio.Camiones (Id, Nombre, Placa, Estado, Notas, CreatedAt, UpdatedAt) VALUES
(1, N'DONFENG', N'XYZ-001', N'Activo', N'Camión principal ruta norte', GETDATE(), NULL),
(2, N'NISSAN',  N'XYZ-002', N'Activo', N'Ruta sur', GETDATE(), NULL),
(3, N'HUGO',    N'XYZ-003', N'Activo', N'Ruta oriental', GETDATE(), NULL),
(4, N'JMC',     N'XYZ-004', N'Mantenimiento', N'En taller', GETDATE(), NULL);
SET IDENTITY_INSERT acopio.Camiones OFF;
GO

-- Conductores
SET IDENTITY_INSERT acopio.Conductores ON;
INSERT INTO acopio.Conductores (Id, NombreCompleto, Cedula, Telefono, CamionPreferidoId, UserId, IsActive, CreatedAt, UpdatedAt) VALUES
(1, N'Carlos Ramírez',    N'1067123456', N'3015551122', 1, NULL, 1, GETDATE(), NULL),
(2, N'José Manuel Pérez', N'1067234567', N'3015552233', 2, NULL, 1, GETDATE(), NULL),
(3, N'Andrés Suárez',     N'1067345678', N'3015553344', 3, NULL, 1, GETDATE(), NULL),
(4, N'Luis Martínez',     N'1067456789', N'3015554455', 4, NULL, 1, GETDATE(), NULL);
SET IDENTITY_INSERT acopio.Conductores OFF;
GO

-- Asistentes
SET IDENTITY_INSERT acopio.Asistentes ON;
INSERT INTO acopio.Asistentes (Id, NombreCompleto, Cedula, Telefono, CamionPreferidoId, IsActive, CreatedAt, UpdatedAt) VALUES
(1, N'Miguel Ángel Torres', N'1067567890', N'3015556677', 1, 1, GETDATE(), NULL),
(2, N'Jhon Jairo Castro',   N'1067678901', N'3015557788', 2, 1, GETDATE(), NULL),
(3, N'Yeison Beltrán',      N'1067789012', N'3015558899', 3, 1, GETDATE(), NULL);
SET IDENTITY_INSERT acopio.Asistentes OFF;
GO

-- Granjeros
SET IDENTITY_INSERT acopio.Granjeros ON;
INSERT INTO acopio.Granjeros (Id, Numero, NombreCompleto, Cedula, Telefono, Finca, Vereda, Municipio, PrecioLitro, PromedioDiario, Notas, IsActive, CreatedAt, UpdatedAt) VALUES
(1,  1,  N'Hernando Mejía',     N'77123456', N'3201112233', N'La Esperanza', N'El Carmen',    N'San Diego', 1850, 120.5, NULL, 1, GETDATE(), NULL),
(2,  4,  N'María del Carmen',   N'77234567', N'3201112244', N'Villa Rosa',   N'San Bernardo', N'San Diego', 1850,  85.0, NULL, 1, GETDATE(), NULL),
(3,  7,  N'Rafael Ortega',      N'77345678', N'3201112255', N'El Paraíso',   N'La Loma',      N'San Diego', 1850,  95.5, NULL, 1, GETDATE(), NULL),
(4,  12, N'Pedro Galván',       N'77456789', N'3201112266', N'Los Cerezos',  N'El Carmen',    N'San Diego', 1850, 145.0, NULL, 1, GETDATE(), NULL),
(5,  18, N'Lucía Morales',      N'77567890', N'3201112277', N'San Antonio',  N'El Rincón',    N'San Diego', 1850,  60.5, NULL, 1, GETDATE(), NULL),
(6,  23, N'Juan de Dios Pérez', N'77678901', N'3201112288', N'La Primavera', N'San Bernardo', N'San Diego', 1850, 200.0, NULL, 1, GETDATE(), NULL),
(7,  31, N'Carmen Elena Ríos',  N'77789012', N'3201112299', N'El Vergel',    N'La Loma',      N'San Diego', 1850,  70.5, NULL, 1, GETDATE(), NULL),
(8,  42, N'Alberto Vargas',     N'77890123', N'3201112300', N'Buena Vista',  N'El Carmen',    N'San Diego', 1850, 110.0, NULL, 1, GETDATE(), NULL),
(9,  56, N'Sandra Quintero',    N'77901234', N'3201112311', N'El Recuerdo',  N'San Bernardo', N'San Diego', 1850,  80.0, NULL, 1, GETDATE(), NULL),
(10, 71, N'Ricardo Beltrán',    N'78012345', N'3201112322', N'La Florida',   N'El Rincón',    N'San Diego', 1850, 165.5, NULL, 1, GETDATE(), NULL),
(11, 85, N'Esperanza Núñez',    N'78123456', N'3201112333', N'San Isidro',   N'La Loma',      N'San Diego', 1850,  90.0, NULL, 1, GETDATE(), NULL),
(12, 99, N'Tomás Cárdenas',     N'78234567', N'3201112344', N'El Mirador',   N'El Carmen',    N'San Diego', 1850, 130.0, NULL, 1, GETDATE(), NULL);
SET IDENTITY_INSERT acopio.Granjeros OFF;
GO

-- Rutas (sin IsActive)
SET IDENTITY_INSERT acopio.Rutas ON;
INSERT INTO acopio.Rutas (Id, Codigo, Fecha, CamionId, ConductorId, AsistenteId, HoraSalida, HoraLlegadaPlanta, HoraDescargue, TotalLitrosChofer, TotalLitrosPlanta, DiferenciaTotal, Status, Observaciones, CreatedAt, UpdatedAt) VALUES
(1, N'DONFENG', DATEADD(day, -2, CAST(GETDATE() AS DATE)), 1, 1, 1, '04:30:00', '09:15:00', '09:45:00', 845.5, 843.0, -2.5, N'Conciliada',          N'Sin novedad',  GETDATE(), NULL),
(2, N'NISSAN',  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 2, 2, 2, '04:45:00', '09:30:00', '10:00:00', 612.0, 612.0,  0.0, N'Conciliada',          NULL,            GETDATE(), NULL),
(3, N'HUGO',    DATEADD(day, -1, CAST(GETDATE() AS DATE)), 3, 3, 3, '04:30:00', '09:20:00', NULL,       720.5, NULL,   NULL, N'EsperandoDescargue', NULL,            GETDATE(), NULL),
(4, N'DONFENG', DATEADD(day, -1, CAST(GETDATE() AS DATE)), 1, 1, 1, '04:30:00', '09:10:00', '09:40:00', 880.0, 878.5, -1.5, N'Conciliada',          N'Lluvia leve',  GETDATE(), NULL),
(5, N'NISSAN',  CAST(GETDATE() AS DATE),                   2, 2, 2, '04:45:00', NULL,       NULL,         0.0, NULL,   NULL, N'EnProgreso',         NULL,            GETDATE(), NULL);
SET IDENTITY_INSERT acopio.Rutas OFF;
GO

-- Recogidas (sin IsActive)
SET IDENTITY_INSERT acopio.Recogidas ON;
INSERT INTO acopio.Recogidas (Id, RutaId, GranjeroId, Fecha, CantinasChofer, SaldoChofer, LitrosChofer, CantinasPlanta, SaldoPlanta, LitrosPlanta, DiferenciaLitros, RecogidoAt, DescargadoAt, CreatedAt, UpdatedAt) VALUES
(1,  1, 1,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 3, 0.5,  120.5, 3,    0.0,  120.0, -0.5, GETDATE(), GETDATE(), GETDATE(), NULL),
(2,  1, 4,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 3, 25.0, 145.0, 3,   25.0,  145.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(3,  1, 6,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 5, 0.0,  200.0, 5,    0.0,  200.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(4,  1, 8,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 2, 30.0, 110.0, 2,   30.0,  110.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(5,  1, 10, DATEADD(day, -2, CAST(GETDATE() AS DATE)), 4, 5.5,  165.5, 4,    3.0,  163.0, -2.5, GETDATE(), GETDATE(), GETDATE(), NULL),
(6,  1, 12, DATEADD(day, -2, CAST(GETDATE() AS DATE)), 2, 24.5, 104.5, 2,   25.0,  105.0,  0.5, GETDATE(), GETDATE(), GETDATE(), NULL),
(7,  2, 2,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 2, 5.0,   85.0, 2,    5.0,   85.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(8,  2, 3,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 2, 15.5,  95.5, 2,   15.5,   95.5,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(9,  2, 5,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 1, 20.5,  60.5, 1,   20.5,   60.5,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(10, 2, 7,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 1, 30.5,  70.5, 1,   30.5,   70.5,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(11, 2, 9,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 2, 0.0,   80.0, 2,    0.0,   80.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(12, 2, 11, DATEADD(day, -2, CAST(GETDATE() AS DATE)), 2, 10.0,  90.0, 2,   10.0,   90.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(13, 2, 1,  DATEADD(day, -2, CAST(GETDATE() AS DATE)), 3, 10.5, 130.5, 3,   10.5,  130.5,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(14, 3, 1,  DATEADD(day, -1, CAST(GETDATE() AS DATE)), 3, 0.5,  120.5, NULL, NULL,   NULL, NULL, GETDATE(), NULL,      GETDATE(), NULL),
(15, 3, 4,  DATEADD(day, -1, CAST(GETDATE() AS DATE)), 3, 25.0, 145.0, NULL, NULL,   NULL, NULL, GETDATE(), NULL,      GETDATE(), NULL),
(16, 3, 6,  DATEADD(day, -1, CAST(GETDATE() AS DATE)), 5, 0.0,  200.0, NULL, NULL,   NULL, NULL, GETDATE(), NULL,      GETDATE(), NULL),
(17, 3, 8,  DATEADD(day, -1, CAST(GETDATE() AS DATE)), 2, 30.0, 110.0, NULL, NULL,   NULL, NULL, GETDATE(), NULL,      GETDATE(), NULL),
(18, 3, 10, DATEADD(day, -1, CAST(GETDATE() AS DATE)), 3, 25.0, 145.0, NULL, NULL,   NULL, NULL, GETDATE(), NULL,      GETDATE(), NULL),
(19, 4, 1,  DATEADD(day, -1, CAST(GETDATE() AS DATE)), 3, 5.0,  125.0, 3,    5.0,  125.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(20, 4, 4,  DATEADD(day, -1, CAST(GETDATE() AS DATE)), 4, 0.0,  160.0, 4,    0.0,  160.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(21, 4, 6,  DATEADD(day, -1, CAST(GETDATE() AS DATE)), 5, 10.0, 210.0, 5,   10.0,  210.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(22, 4, 8,  DATEADD(day, -1, CAST(GETDATE() AS DATE)), 2, 35.0, 115.0, 2,   33.5,  113.5, -1.5, GETDATE(), GETDATE(), GETDATE(), NULL),
(23, 4, 10, DATEADD(day, -1, CAST(GETDATE() AS DATE)), 4, 10.0, 170.0, 4,   10.0,  170.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL),
(24, 4, 12, DATEADD(day, -1, CAST(GETDATE() AS DATE)), 2, 20.0, 100.0, 2,   20.0,  100.0,  0.0, GETDATE(), GETDATE(), GETDATE(), NULL);
SET IDENTITY_INSERT acopio.Recogidas OFF;
GO

PRINT 'Acopio seed completado.';
GO
