# Image Processing — Remove Background

## Uso

1. **Instalar rembg** (una sola vez):
   ```bash
   pip install "rembg[cpu]"
   ```

2. **Agregar imágenes** a procesar en la carpeta `input/`.
   Acepta `.jpg`, `.jpeg`, `.png`, `.webp`.

3. **Ejecutar** desde esta carpeta:
   ```bash
   ./remove-bg.sh
   ```

4. **Resultado**: PNGs con fondo transparente en `output/`, mismo nombre base.

5. **Mover a destino final** (ejemplo para producto propio YCT):
   ```bash
   mv output/*.png ../Front/yct-frontend/public/assets/products/yct/
   ```

## Modelos disponibles

- `birefnet-general` (default) — mejor calidad general, más lento.
- `isnet-general-use` — rápido, buena calidad en fondos simples.
- `u2net` — básico, muy rápido.

Cambiar modelo:
```bash
./remove-bg.sh isnet-general-use
```

## Notas

- La primera ejecución descarga el modelo (~100–400MB según el elegido).
- No subas esta carpeta ni sus descargas al repo — el `input/` y `output/` son staging local.
