# Backups de la base de datos YCT

## Backup manual

```
backup-db.bat
```

Genera un archivo `.bak` en `./backups/YctDb_YYYY-MM-DD_HH-mm.bak`.
El script conserva los últimos **14 días** y borra los más viejos automáticamente.

## Restauración

```
restore-db.bat                      → restaura el backup más reciente
restore-db.bat ruta\al\backup.bak   → restaura uno específico
```

Pide confirmación antes de sobrescribir. Pone la base en single-user, restaura
y la vuelve a multi-user.

## Programar el backup automático en Windows

1. Abre **Programador de tareas** (`taskschd.msc`)
2. **Crear tarea básica…** → nombre: "YCT backup diario"
3. **Desencadenador**: Diariamente a las **23:00**
4. **Acción**: Iniciar un programa
   - Programa/script: `C:\Users\Eduardo Zequeira\Desktop\YCT\backup-db.bat`
   - Iniciar en: `C:\Users\Eduardo Zequeira\Desktop\YCT`
5. Marcar **"Ejecutar con privilegios más altos"** si SQL Server lo requiere
6. Finalizar

## Recomendación adicional

Mueve la carpeta `backups/` a un **OneDrive** o **Google Drive sincronizado**.
Si el portátil falla, los backups se replican en la nube sin esfuerzo extra.

Ejemplo: cambia en `backup-db.bat` la línea:
```
set "BACKUP_DIR=%~dp0backups"
```
por:
```
set "BACKUP_DIR=%USERPROFILE%\OneDrive\YCT-backups"
```
