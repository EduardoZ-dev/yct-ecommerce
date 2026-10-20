using System.Reflection;
using System.Text;
using SkiaSharp;

namespace YCT.Infrastructure.Services.Tarjetas;

/// <summary>Paleta de las tarjetas: los verdes de YCT y los tonos de alerta.</summary>
internal static class Colores
{
    public static readonly SKColor Fondo = SKColors.White;
    public static readonly SKColor Verde = new(0x2E, 0x7D, 0x32);
    public static readonly SKColor VerdeOscuro = new(0x1B, 0x5E, 0x20);
    public static readonly SKColor VerdeHoja = new(0x43, 0xA0, 0x47);
    public static readonly SKColor VerdeSuave = new(0xE8, 0xF3, 0xE8);
    public static readonly SKColor VerdeBorde = new(0xA5, 0xD6, 0xA7);
    public static readonly SKColor Rojo = new(0xC6, 0x28, 0x28);
    public static readonly SKColor RojoOscuro = new(0xB7, 0x1C, 0x1C);
    public static readonly SKColor RojoSuave = new(0xFD, 0xEC, 0xEA);
    public static readonly SKColor RojoBorde = new(0xEF, 0x9A, 0x9A);
    public static readonly SKColor Turquesa = new(0x00, 0x79, 0x6B);
    public static readonly SKColor TurquesaSuave = new(0xE0, 0xF2, 0xF1);
    public static readonly SKColor Gris = new(0xF5, 0xF7, 0xF5);
    public static readonly SKColor Borde = new(0xE3, 0xE9, 0xE3);
    public static readonly SKColor Separador = new(0xEC, 0xEF, 0xEC);
    public static readonly SKColor Texto = new(0x21, 0x21, 0x21);
    public static readonly SKColor TextoSuave = new(0x37, 0x47, 0x4F);
    public static readonly SKColor Etiqueta = new(0x60, 0x7D, 0x8B);
    public static readonly SKColor Pie = new(0x90, 0xA4, 0xAE);
}

/// <summary>
/// Herramientas de dibujo de las tarjetas: tipografía Roboto embebida (el contenedor Linux no trae
/// fuentes), textos que se recortan o se parten para caber, e íconos dibujados con trazos.
/// </summary>
internal sealed partial class LienzoTarjeta
{
    private static readonly Lazy<SKTypeface> Regular = new(() => CargarFuente("Roboto-Regular.ttf"));
    private static readonly Lazy<SKTypeface> Media = new(() => CargarFuente("Roboto-Medium.ttf"));
    private static readonly Lazy<SKTypeface> Negrita = new(() => CargarFuente("Roboto-Bold.ttf"));

    public SKCanvas Canvas { get; }

    public LienzoTarjeta(SKCanvas canvas) => Canvas = canvas;

    public static SKFont FuenteRegular(float tamano) => new(Regular.Value, tamano) { Subpixel = true };
    public static SKFont FuenteMedia(float tamano) => new(Media.Value, tamano) { Subpixel = true };
    public static SKFont FuenteNegrita(float tamano) => new(Negrita.Value, tamano) { Subpixel = true };

    private static SKTypeface CargarFuente(string archivo)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Fuentes.{archivo}")
            ?? throw new InvalidOperationException($"No está embebida la fuente {archivo}");
        return SKTypeface.FromStream(stream)
            ?? throw new InvalidOperationException($"No se pudo leer la fuente {archivo}");
    }

    // ===== Texto =====

    /// <summary>Quita lo que la tipografía no sabe dibujar (emojis…) para no pintar cuadritos vacíos.</summary>
    public static string SoloDibujable(string? texto, SKFont fuente)
    {
        if (string.IsNullOrEmpty(texto))
            return string.Empty;
        var limpio = new StringBuilder(texto.Length);
        foreach (var rune in texto.EnumerateRunes())
        {
            if (Rune.IsWhiteSpace(rune))
                limpio.Append(' ');
            else if (fuente.Typeface.ContainsGlyph(rune.Value))
                limpio.Append(rune.ToString());
        }
        var unido = string.Join(' ', limpio.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        // Lo que quedaba antes de un emoji quitado no debe dejar "camino ." con espacio colgando.
        return EspacioAntesDePuntuacion().Replace(unido, "$1");
    }

    [System.Text.RegularExpressions.GeneratedRegex(@" ([.,;:!?)])")]
    private static partial System.Text.RegularExpressions.Regex EspacioAntesDePuntuacion();

    /// <summary>Recorta con "…" para que el texto no pase de <paramref name="anchoMax"/>.</summary>
    public static string Recortar(string texto, SKFont fuente, float anchoMax)
    {
        if (fuente.MeasureText(texto) <= anchoMax)
            return texto;
        var corte = texto.Length;
        while (corte > 1 && fuente.MeasureText(texto[..corte] + "…") > anchoMax)
            corte--;
        return texto[..corte].TrimEnd() + "…";
    }

    /// <summary>Parte el texto en renglones de hasta <paramref name="anchoMax"/>; como mucho <paramref name="maxRenglones"/>.</summary>
    public static List<string> Partir(string texto, SKFont fuente, float anchoMax, int maxRenglones)
    {
        var renglones = new List<string>();
        var actual = string.Empty;
        foreach (var palabra in texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var prueba = actual.Length == 0 ? palabra : $"{actual} {palabra}";
            if (fuente.MeasureText(prueba) <= anchoMax || actual.Length == 0)
            {
                actual = prueba;
                continue;
            }
            renglones.Add(actual);
            actual = palabra;
        }
        if (actual.Length > 0)
            renglones.Add(actual);
        if (renglones.Count <= maxRenglones)
            return renglones.Select(r => Recortar(r, fuente, anchoMax)).ToList();
        var visibles = renglones.Take(maxRenglones).ToList();
        visibles[^1] = Recortar(visibles[^1] + " …", fuente, anchoMax);
        return visibles;
    }

    /// <summary>Achica la fuente hasta que el texto quepa (para el titular).</summary>
    public static SKFont AjustarAncho(string texto, Func<float, SKFont> crear, float tamano, float anchoMax)
    {
        var fuente = crear(tamano);
        while (tamano > 20 && fuente.MeasureText(texto) > anchoMax)
        {
            fuente.Dispose();
            tamano -= 2;
            fuente = crear(tamano);
        }
        return fuente;
    }

    public void Texto(string texto, float x, float baseline, SKFont fuente, SKColor color, SKTextAlign alineacion = SKTextAlign.Left)
    {
        using var pintura = new SKPaint { Color = color, IsAntialias = true };
        Canvas.DrawText(texto, x, baseline, alineacion, fuente, pintura);
    }

    /// <summary>Baseline para centrar verticalmente un renglón de esa fuente en <paramref name="centroY"/>.</summary>
    public static float BaselineCentrada(SKFont fuente, float centroY) =>
        centroY - (fuente.Metrics.Ascent + fuente.Metrics.Descent) / 2;

    // ===== Formas =====

    public void Caja(SKRect rect, float radio, SKColor relleno, SKColor? borde = null)
    {
        using var pintura = new SKPaint { Color = relleno, IsAntialias = true };
        Canvas.DrawRoundRect(rect, radio, radio, pintura);
        if (borde is null)
            return;
        using var linea = new SKPaint { Color = borde.Value, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
        Canvas.DrawRoundRect(rect, radio, radio, linea);
    }

    /// <summary>Caja con solo las esquinas de arriba redondeadas (cabecera de una sección).</summary>
    public void CabeceraSeccion(SKRect rect, float radio, SKColor relleno)
    {
        using var pintura = new SKPaint { Color = relleno, IsAntialias = true };
        Canvas.DrawRoundRect(rect, radio, radio, pintura);
        Canvas.DrawRect(new SKRect(rect.Left, rect.Bottom - radio, rect.Right, rect.Bottom), pintura);
    }

    public void Linea(float x1, float y, float x2, SKColor color, float grosor = 2)
    {
        using var pintura = new SKPaint { Color = color, IsAntialias = true, StrokeWidth = grosor };
        Canvas.DrawLine(x1, y, x2, y, pintura);
    }

    public void Circulo(float cx, float cy, float radio, SKColor color)
    {
        using var pintura = new SKPaint { Color = color, IsAntialias = true };
        Canvas.DrawCircle(cx, cy, radio, pintura);
    }

    private SKPaint Trazo(SKColor color, float grosor) => new()
    {
        Color = color, IsAntialias = true, Style = SKPaintStyle.Stroke,
        StrokeWidth = grosor, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round
    };

    private SKPaint Relleno(SKColor color) => new() { Color = color, IsAntialias = true };

    // ===== Íconos (todos centrados en cx, cy; r = radio del ícono) =====

    public void IconoCheck(float cx, float cy, float r, SKColor fondo)
    {
        Circulo(cx, cy, r, fondo);
        using var trazo = Trazo(SKColors.White, r * 0.18f);
        using var path = new SKPath();
        path.MoveTo(cx - r * 0.42f, cy + r * 0.02f);
        path.LineTo(cx - r * 0.12f, cy + r * 0.32f);
        path.LineTo(cx + r * 0.45f, cy - r * 0.30f);
        Canvas.DrawPath(path, trazo);
    }

    public void IconoAlerta(float cx, float cy, float r, SKColor fondo)
    {
        Circulo(cx, cy, r, fondo);
        using var trazo = Trazo(SKColors.White, r * 0.18f);
        Canvas.DrawLine(cx, cy - r * 0.48f, cx, cy + r * 0.10f, trazo);
        Circulo(cx, cy + r * 0.40f, r * 0.11f, SKColors.White);
    }

    public void IconoFlechaArriba(float cx, float cy, float r, SKColor fondo)
    {
        Circulo(cx, cy, r, fondo);
        using var trazo = Trazo(SKColors.White, r * 0.16f);
        Canvas.DrawLine(cx, cy + r * 0.45f, cx, cy - r * 0.40f, trazo);
        using var punta = new SKPath();
        punta.MoveTo(cx - r * 0.36f, cy - r * 0.05f);
        punta.LineTo(cx, cy - r * 0.42f);
        punta.LineTo(cx + r * 0.36f, cy - r * 0.05f);
        Canvas.DrawPath(punta, trazo);
    }

    public void IconoPersona(float cx, float cy, float r, SKColor color)
    {
        Circulo(cx, cy - r * 0.38f, r * 0.34f, color);
        using var relleno = Relleno(color);
        Canvas.DrawRoundRect(new SKRect(cx - r * 0.66f, cy + r * 0.06f, cx + r * 0.66f, cy + r * 0.78f), r * 0.42f, r * 0.42f, relleno);
    }

    public void IconoCamion(float cx, float cy, float r, SKColor color)
    {
        using var carga = Relleno(color);
        using var cabina = Relleno(Colores.VerdeOscuro);
        Canvas.DrawRoundRect(new SKRect(cx - r, cy - r * 0.55f, cx + r * 0.25f, cy + r * 0.32f), r * 0.08f, r * 0.08f, carga);
        Canvas.DrawRoundRect(new SKRect(cx + r * 0.30f, cy - r * 0.18f, cx + r, cy + r * 0.32f), r * 0.10f, r * 0.10f, cabina);
        using var ventana = Relleno(new SKColor(0xC8, 0xE6, 0xC9));
        Canvas.DrawRect(new SKRect(cx + r * 0.42f, cy - r * 0.08f, cx + r * 0.82f, cy + r * 0.08f), ventana);
        Circulo(cx - r * 0.55f, cy + r * 0.42f, r * 0.22f, new SKColor(0x26, 0x32, 0x38));
        Circulo(cx + r * 0.62f, cy + r * 0.42f, r * 0.22f, new SKColor(0x26, 0x32, 0x38));
    }

    public void IconoBarras(float cx, float cy, float r, SKColor fondo)
    {
        Circulo(cx, cy, r, fondo);
        using var blanco = Relleno(SKColors.White);
        var ancho = r * 0.22f;
        var piso = cy + r * 0.42f;
        float[] altos = { r * 0.45f, r * 0.80f, r * 0.62f };
        for (var i = 0; i < altos.Length; i++)
        {
            var x = cx - r * 0.42f + i * (ancho + r * 0.09f);
            Canvas.DrawRoundRect(new SKRect(x, piso - altos[i], x + ancho, piso), r * 0.04f, r * 0.04f, blanco);
        }
    }

    public void IconoCasa(float cx, float cy, float r, SKColor fondo)
    {
        Circulo(cx, cy, r, fondo);
        using var blanco = Relleno(SKColors.White);
        using var techo = new SKPath();
        techo.MoveTo(cx - r * 0.55f, cy - r * 0.02f);
        techo.LineTo(cx, cy - r * 0.52f);
        techo.LineTo(cx + r * 0.55f, cy - r * 0.02f);
        techo.Close();
        Canvas.DrawPath(techo, blanco);
        Canvas.DrawRect(new SKRect(cx - r * 0.38f, cy - r * 0.06f, cx + r * 0.38f, cy + r * 0.45f), blanco);
        using var puerta = Relleno(fondo);
        Canvas.DrawRect(new SKRect(cx - r * 0.11f, cy + r * 0.12f, cx + r * 0.11f, cy + r * 0.45f), puerta);
    }

    public void IconoNota(float cx, float cy, float r, SKColor fondo)
    {
        Circulo(cx, cy, r, fondo);
        using var blanco = Relleno(SKColors.White);
        Canvas.DrawRoundRect(new SKRect(cx - r * 0.38f, cy - r * 0.48f, cx + r * 0.38f, cy + r * 0.48f), r * 0.08f, r * 0.08f, blanco);
        using var renglon = Trazo(fondo, r * 0.09f);
        for (var i = 0; i < 3; i++)
            Canvas.DrawLine(cx - r * 0.22f, cy - r * 0.22f + i * r * 0.22f, cx + r * 0.22f, cy - r * 0.22f + i * r * 0.22f, renglon);
    }

    public void IconoHoja(float cx, float cy, float r, SKColor color)
    {
        using var relleno = Relleno(color);
        using var hoja = new SKPath();
        hoja.MoveTo(cx - r * 0.75f, cy + r * 0.75f);
        hoja.QuadTo(cx - r * 0.85f, cy - r * 0.70f, cx + r * 0.80f, cy - r * 0.80f);
        hoja.QuadTo(cx + r * 0.70f, cy + r * 0.85f, cx - r * 0.75f, cy + r * 0.75f);
        hoja.Close();
        Canvas.DrawPath(hoja, relleno);
        using var nervio = Trazo(new SKColor(0xC8, 0xE6, 0xC9), r * 0.10f);
        Canvas.DrawLine(cx - r * 0.55f, cy + r * 0.55f, cx + r * 0.45f, cy - r * 0.45f, nervio);
    }
}
