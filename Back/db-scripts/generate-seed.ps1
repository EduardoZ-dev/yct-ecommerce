param(
    [string]$Server = "EDUARDOZEQUEIRA",
    [string]$Database = "YctDb",
    [string]$OutFile = "$PSScriptRoot\seed.sql"
)

$connStr = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"

function Escape-SqlValue($val) {
    if ($null -eq $val -or $val -is [System.DBNull]) { return "NULL" }
    if ($val -is [bool]) { if ($val) { return "1" } else { return "0" } }
    if ($val -is [int] -or $val -is [long] -or $val -is [decimal] -or $val -is [double] -or $val -is [single]) {
        return $val.ToString([System.Globalization.CultureInfo]::InvariantCulture)
    }
    if ($val -is [datetime]) {
        return "'" + $val.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'"
    }
    $s = $val.ToString().Replace("'", "''")
    return "N'" + $s + "'"
}

function Dump-Table($conn, $table, $columns, $writer) {
    $writer.WriteLine("-- $table")
    $writer.WriteLine("SET IDENTITY_INSERT [$table] ON;")
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT [" + ($columns -join "],[") + "] FROM [$table] ORDER BY Id"
    $reader = $cmd.ExecuteReader()
    $colList = "[" + ($columns -join "],[") + "]"
    while ($reader.Read()) {
        $vals = @()
        for ($i = 0; $i -lt $columns.Count; $i++) {
            $vals += Escape-SqlValue $reader[$i]
        }
        $writer.WriteLine("INSERT INTO [$table] ($colList) VALUES (" + ($vals -join ", ") + ");")
    }
    $reader.Close()
    $writer.WriteLine("SET IDENTITY_INSERT [$table] OFF;")
    $writer.WriteLine("GO")
    $writer.WriteLine("")
}

function Get-Columns($conn, $table) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$table' ORDER BY ORDINAL_POSITION"
    $reader = $cmd.ExecuteReader()
    $cols = @()
    while ($reader.Read()) { $cols += $reader.GetString(0) }
    $reader.Close()
    return $cols
}

Add-Type -AssemblyName "System.Data"
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()

$writer = New-Object System.IO.StreamWriter($OutFile, $false, [System.Text.UTF8Encoding]::new($false))
$writer.WriteLine("-- YCT seed data (autogenerado)")
$writer.WriteLine("-- Ejecutar DESPUES de ``dotnet ef database update``")
$writer.WriteLine("USE [YctDb];")
$writer.WriteLine("GO")
$writer.WriteLine("")
$writer.WriteLine("-- Limpiar tablas en orden inverso a las FKs")
$writer.WriteLine("DELETE FROM OrderDetails; DELETE FROM Orders; DELETE FROM Products; DELETE FROM Categories; DELETE FROM Users;")
$writer.WriteLine("DBCC CHECKIDENT('OrderDetails', RESEED, 0); DBCC CHECKIDENT('Orders', RESEED, 0); DBCC CHECKIDENT('Products', RESEED, 0); DBCC CHECKIDENT('Categories', RESEED, 0); DBCC CHECKIDENT('Users', RESEED, 0);")
$writer.WriteLine("GO")
$writer.WriteLine("")

foreach ($table in @("Categories", "Users", "Products")) {
    $cols = Get-Columns $conn $table
    Dump-Table $conn $table $cols $writer
}

$writer.Close()
$conn.Close()
Write-Output "Seed generado en: $OutFile"
