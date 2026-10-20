using System.Globalization;
using SkiaSharp;
using YCT.Application.Common;

namespace YCT.Infrastructure.Services.Tarjetas;

/// <summary>Dibuja las tarjetas (PNG) que encabezan los avisos de WhatsApp.</summary>
public interface ITarjetasWhatsApp
{
    /// <summary>Tarjeta del descargue: resultado, conductor, litros y una finca por renglón.</summary>
    byte[] Descargue(WhatsAppDescargueModel modelo);

    /// <summary>Tarjeta de la entrada de un tercero: registrado vs recibido y la nota de planta.</summary>
    byte[] Tercero(WhatsAppTerceroModel modelo);
}

/// <summary>
/// Tarjetas con el diseño de referencia del usuario (verde YCT, secciones con cabecera, cifras a la
/// derecha). Ancho fijo de 1080 px y alto según el contenido. Solo presenta datos: no decide nada.
/// </summary>
public sealed class TarjetasWhatsApp : ITarjetasWhatsApp
{
    private const float Ancho = 1080;
    private const float Margen = 36;
    private const float Espacio = 20;
    private const float Radio = 26;
    private const float AltoCabecera = 200;
    private const float AltoInfo = 132;
    private const float AltoCabeceraSeccion = 92;
    private const float AltoFilaResumen = 74;
    private const float AltoFilaFinca = 62;
    private const float AltoPie = 64;
    private const int MaxFincas = 20;
    private const int MaxRenglonesObservacion = 5;

    private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;

    public byte[] Descargue(WhatsAppDescargueModel m)
    {
        var aportes = m.Aportes ?? Array.Empty<AporteFinca>();
        var filasFincas = Math.Max(1, Math.Min(aportes.Count, MaxFincas) + (aportes.Count > MaxFincas ? 1 : 0));
        var alto = Margen + AltoCabecera + Espacio + AltoInfo + Espacio
                   + AltoSeccion(4) + Espacio
                   + AltoCabeceraSeccion + filasFincas * AltoFilaFinca + 16 + Espacio
                   + AltoPie + Margen;

        return Dibujar(alto, lienzo =>
        {
            var conFaltante = m.Diferencia < 0;
            var y = Margen;
            y = Cabecera(lienzo, y, conFaltante ? Estilo.Alerta : Estilo.Ok,
                conFaltante ? "DESCARGUE CON FALTANTE" : "DESCARGUE OK",
                $"Planilla {m.Codigo} · {Dia(m.Fecha)}");
            y = Informacion(lienzo, y, ("Conductor", m.Conductor, Icono.Persona), ("Vehículo", m.Camion, Icono.Camion));
            y = Resumen(lienzo, y, new[]
            {
                new FilaResumen("Chofer declaró:", $"{Litros(m.LitrosChofer)} L"),
                new FilaResumen("Planta midió:", $"{Litros(m.LitrosPlanta)} L"),
                new FilaResumen("Diferencia:", $"{Litros(m.Diferencia)} L", conFaltante ? Colores.Rojo : null),
            }, m.Estado, conFaltante);
            y = Fincas(lienzo, y, aportes);
            Pie(lienzo, y);
        });
    }

    public byte[] Tercero(WhatsAppTerceroModel m)
    {
        using var fuenteObs = LienzoTarjeta.FuenteRegular(31);
        var observacion = LienzoTarjeta.SoloDibujable(m.Observacion, fuenteObs);
        var renglones = observacion.Length == 0
            ? new List<string> { "Sin observaciones" }
            : LienzoTarjeta.Partir(observacion, fuenteObs, Ancho - 2 * Margen - 60, MaxRenglonesObservacion);
        var alto = Margen + AltoCabecera + Espacio + AltoInfo + Espacio
                   + AltoSeccion(3) + Espacio
                   + AltoCabeceraSeccion + renglones.Count * 44 + 40 + Espacio
                   + AltoPie + Margen;

        return Dibujar(alto, lienzo =>
        {
            var (estilo, resultado) = m.Diferencia switch
            {
                < 0 => (Estilo.Alerta, "Llegó MENOS leche"),
                > 0 => (Estilo.Mas, "Llegó MÁS leche"),
                _ => (Estilo.Ok, "Llegó completa"),
            };
            var y = Margen;
            y = Cabecera(lienzo, y, estilo, "ENTRADA DE TERCERO", $"{resultado} · {Dia(m.Fecha)}");
            y = Informacion(lienzo, y, ("Cliente", m.Cliente, Icono.Persona), ("Recibió", m.RecibidoPor, Icono.Casa));
            var signo = m.Diferencia > 0 ? "+" : string.Empty;
            y = Resumen(lienzo, y, new[]
            {
                new FilaResumen("Registrado en acopio:", $"{Litros(m.LitrosRegistrados)} L"),
                new FilaResumen("Recibido en planta:", $"{Litros(m.LitrosRecibidos)} L"),
                new FilaResumen("Diferencia:", $"{signo}{Litros(m.Diferencia)} L", estilo.Principal),
            }, estado: null, conFaltante: false);
            y = Observacion(lienzo, y, renglones);
            Pie(lienzo, y);
        });
    }

    // ===== Piezas =====

    private enum Icono { Persona, Camion, Casa }

    private sealed record Estilo(SKColor Principal, SKColor Oscuro, SKColor Suave, Action<LienzoTarjeta, float, float, float> Simbolo)
    {
        public static readonly Estilo Ok = new(Colores.Verde, Colores.VerdeOscuro, Colores.VerdeSuave,
            (l, x, y, r) => l.IconoCheck(x, y, r, Colores.Verde));
        public static readonly Estilo Alerta = new(Colores.Rojo, Colores.RojoOscuro, Colores.RojoSuave,
            (l, x, y, r) => l.IconoAlerta(x, y, r, Colores.Rojo));
        public static readonly Estilo Mas = new(Colores.Turquesa, Colores.Turquesa, Colores.TurquesaSuave,
            (l, x, y, r) => l.IconoFlechaArriba(x, y, r, Colores.Turquesa));
    }

    private sealed record FilaResumen(string Etiqueta, string Valor, SKColor? Color = null);

    private static float AltoSeccion(int filas) => AltoCabeceraSeccion + filas * AltoFilaResumen + 16;

    private static byte[] Dibujar(float alto, Action<LienzoTarjeta> dibujo)
    {
        var info = new SKImageInfo((int)Ancho, (int)Math.Ceiling(alto), SKColorType.Rgba8888, SKAlphaType.Premul);
        using var superficie = SKSurface.Create(info)
            ?? throw new InvalidOperationException("No se pudo crear la superficie de dibujo");
        superficie.Canvas.Clear(Colores.Fondo);
        dibujo(new LienzoTarjeta(superficie.Canvas));
        using var imagen = superficie.Snapshot();
        using var png = imagen.Encode(SKEncodedImageFormat.Png, 100);
        return png.ToArray();
    }

    private static float Cabecera(LienzoTarjeta l, float y, Estilo estilo, string titulo, string subtitulo)
    {
        var caja = new SKRect(Margen, y, Ancho - Margen, y + AltoCabecera);
        l.Caja(caja, Radio, estilo.Suave);
        var cx = Margen + 100;
        estilo.Simbolo(l, cx, y + AltoCabecera / 2, 62);
        var xTexto = cx + 92;
        var anchoTexto = caja.Right - xTexto - 28;
        using var fuenteTitulo = LienzoTarjeta.AjustarAncho(titulo, LienzoTarjeta.FuenteNegrita, 58, anchoTexto);
        l.Texto(titulo, xTexto, y + 96, fuenteTitulo, estilo.Oscuro);
        using var fuenteSub = LienzoTarjeta.FuenteRegular(36);
        l.Texto(LienzoTarjeta.Recortar(subtitulo, fuenteSub, anchoTexto), xTexto, y + 152, fuenteSub, Colores.Texto);
        return caja.Bottom + Espacio;
    }

    private static float Informacion(LienzoTarjeta l, float y, (string Etiqueta, string Valor, Icono Icono) izquierda,
        (string Etiqueta, string Valor, Icono Icono) derecha)
    {
        var caja = new SKRect(Margen, y, Ancho - Margen, y + AltoInfo);
        l.Caja(caja, Radio, Colores.Gris);
        var mitad = caja.MidX;
        using var separador = new SKPaint { Color = Colores.Borde, StrokeWidth = 2 };
        l.Canvas.DrawLine(mitad, y + 24, mitad, caja.Bottom - 24, separador);
        Dato(l, caja.Left, mitad, y, izquierda);
        Dato(l, mitad, caja.Right, y, derecha);
        return caja.Bottom + Espacio;
    }

    private static void Dato(LienzoTarjeta l, float desde, float hasta, float y,
        (string Etiqueta, string Valor, Icono Icono) dato)
    {
        var cx = desde + 62;
        var cy = y + AltoInfo / 2;
        switch (dato.Icono)
        {
            case Icono.Camion: l.IconoCamion(cx, cy, 34, Colores.VerdeHoja); break;
            case Icono.Casa: l.IconoCasa(cx, cy, 30, Colores.Verde); break;
            default: l.IconoPersona(cx, cy, 34, Colores.Verde); break;
        }
        var x = desde + 118;
        var anchoMax = hasta - x - 24;
        using var fuenteEtiqueta = LienzoTarjeta.FuenteRegular(27);
        using var fuenteValor = LienzoTarjeta.FuenteNegrita(35);
        l.Texto(dato.Etiqueta, x, y + 54, fuenteEtiqueta, Colores.Etiqueta);
        var valor = LienzoTarjeta.SoloDibujable(dato.Valor, fuenteValor);
        l.Texto(LienzoTarjeta.Recortar(valor, fuenteValor, anchoMax), x, y + 100, fuenteValor, Colores.Texto);
    }

    private static float CabeceraSeccion(LienzoTarjeta l, float y, float altoTotal, string titulo, Action<float, float> icono)
    {
        var caja = new SKRect(Margen, y, Ancho - Margen, y + altoTotal);
        l.Caja(caja, Radio, SKColors.White, Colores.Borde);
        l.CabeceraSeccion(new SKRect(caja.Left, y, caja.Right, y + AltoCabeceraSeccion), Radio, Colores.VerdeSuave);
        icono(Margen + 58, y + AltoCabeceraSeccion / 2);
        using var fuente = LienzoTarjeta.FuenteNegrita(36);
        l.Texto(titulo, Margen + 108, LienzoTarjeta.BaselineCentrada(fuente, y + AltoCabeceraSeccion / 2), fuente, Colores.VerdeOscuro);
        return y + AltoCabeceraSeccion;
    }

    private static float Resumen(LienzoTarjeta l, float y, IReadOnlyList<FilaResumen> filas, string? estado, bool conFaltante)
    {
        var totalFilas = filas.Count + (estado is null ? 0 : 1);
        var alto = AltoSeccion(totalFilas);
        var fila = CabeceraSeccion(l, y, alto, "Resumen de litros", (cx, cy) => l.IconoBarras(cx, cy, 30, Colores.Verde));
        using var fuenteEtiqueta = LienzoTarjeta.FuenteRegular(32);
        using var fuenteValor = LienzoTarjeta.FuenteNegrita(34);
        for (var i = 0; i < filas.Count; i++)
        {
            var centro = fila + AltoFilaResumen / 2;
            l.Texto(filas[i].Etiqueta, Margen + 32, LienzoTarjeta.BaselineCentrada(fuenteEtiqueta, centro), fuenteEtiqueta, Colores.TextoSuave);
            l.Texto(filas[i].Valor, Ancho - Margen - 32, LienzoTarjeta.BaselineCentrada(fuenteValor, centro), fuenteValor,
                filas[i].Color ?? Colores.Texto, SKTextAlign.Right);
            fila += AltoFilaResumen;
            if (i < totalFilas - 1)
                l.Linea(Margen + 32, fila, Ancho - Margen - 32, Colores.Separador);
        }
        if (estado is not null)
            Estado(l, fila, fuenteEtiqueta, estado, conFaltante);
        return y + alto + Espacio;
    }

    private static void Estado(LienzoTarjeta l, float fila, SKFont fuenteEtiqueta, string estado, bool conFaltante)
    {
        var centro = fila + AltoFilaResumen / 2;
        l.Texto("Estado:", Margen + 32, LienzoTarjeta.BaselineCentrada(fuenteEtiqueta, centro), fuenteEtiqueta, Colores.TextoSuave);
        using var fuente = LienzoTarjeta.FuenteMedia(30);
        var ancho = fuente.MeasureText(estado) + 92;
        var pildora = new SKRect(Ancho - Margen - 24 - ancho, centro - 28, Ancho - Margen - 24, centro + 28);
        l.Caja(pildora, 28, conFaltante ? Colores.RojoSuave : Colores.VerdeSuave, conFaltante ? Colores.RojoBorde : Colores.VerdeBorde);
        if (conFaltante)
            l.IconoAlerta(pildora.Left + 34, centro, 17, Colores.Rojo);
        else
            l.IconoCheck(pildora.Left + 34, centro, 17, Colores.Verde);
        l.Texto(estado, pildora.Left + 62, LienzoTarjeta.BaselineCentrada(fuente, centro), fuente,
            conFaltante ? Colores.RojoOscuro : Colores.VerdeOscuro);
    }

    private static float Fincas(LienzoTarjeta l, float y, IReadOnlyList<AporteFinca> aportes)
    {
        var visibles = aportes.Take(MaxFincas).ToList();
        var filas = Math.Max(1, visibles.Count + (aportes.Count > MaxFincas ? 1 : 0));
        var alto = AltoCabeceraSeccion + filas * AltoFilaFinca + 16;
        var fila = CabeceraSeccion(l, y, alto, "Aporte por finca", (cx, cy) => l.IconoCasa(cx, cy, 30, Colores.Verde));
        using var fuenteNombre = LienzoTarjeta.FuenteRegular(31);
        using var fuenteLitros = LienzoTarjeta.FuenteNegrita(32);
        if (visibles.Count == 0)
        {
            l.Texto("Sin recogidas registradas", Margen + 32, LienzoTarjeta.BaselineCentrada(fuenteNombre, fila + AltoFilaFinca / 2),
                fuenteNombre, Colores.Etiqueta);
            return y + alto + Espacio;
        }
        for (var i = 0; i < visibles.Count; i++)
        {
            var centro = fila + AltoFilaFinca / 2;
            l.IconoHoja(Margen + 44, centro, 14, Colores.VerdeHoja);
            var litros = $"{Litros(visibles[i].Litros)} L";
            var anchoNombre = Ancho - 2 * Margen - 100 - fuenteLitros.MeasureText(litros) - 40;
            var nombre = LienzoTarjeta.Recortar(LienzoTarjeta.SoloDibujable(visibles[i].Etiqueta, fuenteNombre), fuenteNombre, anchoNombre);
            l.Texto(nombre, Margen + 78, LienzoTarjeta.BaselineCentrada(fuenteNombre, centro), fuenteNombre, Colores.Texto);
            l.Texto(litros, Ancho - Margen - 32, LienzoTarjeta.BaselineCentrada(fuenteLitros, centro), fuenteLitros, Colores.Texto, SKTextAlign.Right);
            fila += AltoFilaFinca;
            if (i < filas - 1)
                l.Linea(Margen + 32, fila, Ancho - Margen - 32, Colores.Separador);
        }
        if (aportes.Count > MaxFincas)
            l.Texto($"+{aportes.Count - MaxFincas} fincas más en el panel", Margen + 78,
                LienzoTarjeta.BaselineCentrada(fuenteNombre, fila + AltoFilaFinca / 2), fuenteNombre, Colores.Etiqueta);
        return y + alto + Espacio;
    }

    private static float Observacion(LienzoTarjeta l, float y, IReadOnlyList<string> renglones)
    {
        var alto = AltoCabeceraSeccion + renglones.Count * 44 + 40;
        var fila = CabeceraSeccion(l, y, alto, "Observación de planta", (cx, cy) => l.IconoNota(cx, cy, 30, Colores.Verde));
        using var fuente = LienzoTarjeta.FuenteRegular(31);
        var baseline = fila + 50;
        foreach (var renglon in renglones)
        {
            l.Texto(renglon, Margen + 32, baseline, fuente, Colores.TextoSuave);
            baseline += 44;
        }
        return y + alto + Espacio;
    }

    private static void Pie(LienzoTarjeta l, float y)
    {
        using var fuente = LienzoTarjeta.FuenteMedia(26);
        l.Texto("Distribuciones YCT · Acopio", Ancho / 2, y + 38, fuente, Colores.Pie, SKTextAlign.Center);
    }

    private static string Litros(decimal valor) => valor.ToString("0.##", Cultura);

    private static string Dia(DateTime fecha) => fecha.ToString("dd/MM/yyyy", Cultura);
}
