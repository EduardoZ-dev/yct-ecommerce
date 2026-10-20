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
    private const float AltoFilaFincaCompacta = 54;
    private const float AltoPie = 64;
    // Hasta 13 fincas: una columna. Más: dos columnas compactas, máximo 30 a la vista. Así la tarjeta
    // nunca pasa de la proporción de una pantalla de celular (9:16) y WhatsApp no la corta en el chat.
    private const int MaxFincasUnaColumna = 13;
    private const int MaxFincasVisibles = 30;
    private const int MaxRenglonesObservacion = 5;

    private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;

    public byte[] Descargue(WhatsAppDescargueModel m)
    {
        var fincas = Distribuir(m.Aportes ?? Array.Empty<AporteFinca>());
        var alto = Margen + AltoCabecera + Espacio + AltoInfo + Espacio
                   + AltoSeccion(4) + Espacio
                   + fincas.AltoSeccion + Espacio
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
            y = Fincas(lienzo, y, fincas);
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

    /// <summary>Cómo se reparten las fincas: cuáles se ven, en cuántas columnas y cuántas quedan fuera.</summary>
    private sealed record DistribucionFincas(IReadOnlyList<AporteFinca> Visibles, int Columnas, int FilasPorColumna, int Restantes)
    {
        public float AltoFila => Columnas == 1 ? AltoFilaFinca : AltoFilaFincaCompacta;
        public int Filas => Math.Max(1, FilasPorColumna + (Restantes > 0 ? 1 : 0));
        public float AltoSeccion => AltoCabeceraSeccion + Filas * AltoFila + 16;
    }

    private static DistribucionFincas Distribuir(IReadOnlyList<AporteFinca> aportes)
    {
        var columnas = aportes.Count > MaxFincasUnaColumna ? 2 : 1;
        // Si no caben todas, se ceden dos huecos al renglón "+N fincas más" para no pasar de 9:16.
        var cuantas = aportes.Count > MaxFincasVisibles ? MaxFincasVisibles - 2 : aportes.Count;
        var visibles = aportes.Take(cuantas).ToList();
        var filasPorColumna = (visibles.Count + columnas - 1) / columnas;
        return new DistribucionFincas(visibles, columnas, filasPorColumna, aportes.Count - visibles.Count);
    }

    private static float Fincas(LienzoTarjeta l, float y, DistribucionFincas d)
    {
        var inicio = CabeceraSeccion(l, y, d.AltoSeccion, "Aporte por finca", (cx, cy) => l.IconoCasa(cx, cy, 30, Colores.Verde));
        using var fuenteNombre = LienzoTarjeta.FuenteRegular(d.Columnas == 1 ? 31 : 27);
        using var fuenteLitros = LienzoTarjeta.FuenteNegrita(d.Columnas == 1 ? 32 : 28);
        var izquierda = Margen + 32;
        var derecha = Ancho - Margen - 32;
        if (d.Visibles.Count == 0)
        {
            l.Texto("Sin recogidas registradas", izquierda, LienzoTarjeta.BaselineCentrada(fuenteNombre, inicio + d.AltoFila / 2),
                fuenteNombre, Colores.Etiqueta);
            return y + d.AltoSeccion + Espacio;
        }

        const float hueco = 36;
        var anchoColumna = (derecha - izquierda - (d.Columnas - 1) * hueco) / d.Columnas;
        for (var fila = 0; fila < d.FilasPorColumna; fila++)
        {
            var centro = inicio + fila * d.AltoFila + d.AltoFila / 2;
            // Orden de lectura: primero baja por la columna izquierda y luego por la derecha.
            for (var col = 0; col < d.Columnas; col++)
            {
                var indice = col * d.FilasPorColumna + fila;
                if (indice < d.Visibles.Count)
                    Finca(l, d.Visibles[indice], izquierda + col * (anchoColumna + hueco), anchoColumna, centro, fuenteNombre, fuenteLitros);
            }
            if (fila < d.Filas - 1)
                l.Linea(izquierda, inicio + (fila + 1) * d.AltoFila, derecha, Colores.Separador);
        }
        if (d.Columnas == 2)
            DivisorVertical(l, (izquierda + derecha) / 2, inicio + 10, inicio + d.FilasPorColumna * d.AltoFila - 10);
        if (d.Restantes > 0)
            l.Texto($"+{d.Restantes} fincas más en el panel", izquierda + 34,
                LienzoTarjeta.BaselineCentrada(fuenteNombre, inicio + d.FilasPorColumna * d.AltoFila + d.AltoFila / 2), fuenteNombre, Colores.Etiqueta);
        return y + d.AltoSeccion + Espacio;
    }

    private static void Finca(LienzoTarjeta l, AporteFinca finca, float x, float ancho, float centro, SKFont fuenteNombre, SKFont fuenteLitros)
    {
        var radioHoja = fuenteNombre.Size * 0.45f;
        l.IconoHoja(x + radioHoja, centro, radioHoja, Colores.VerdeHoja);
        var litros = $"{Litros(finca.Litros)} L";
        var xNombre = x + radioHoja * 2 + 16;
        var anchoNombre = x + ancho - fuenteLitros.MeasureText(litros) - 14 - xNombre;
        var nombre = LienzoTarjeta.Recortar(LienzoTarjeta.SoloDibujable(finca.Etiqueta, fuenteNombre), fuenteNombre, anchoNombre);
        l.Texto(nombre, xNombre, LienzoTarjeta.BaselineCentrada(fuenteNombre, centro), fuenteNombre, Colores.Texto);
        l.Texto(litros, x + ancho, LienzoTarjeta.BaselineCentrada(fuenteLitros, centro), fuenteLitros, Colores.Texto, SKTextAlign.Right);
    }

    private static void DivisorVertical(LienzoTarjeta l, float x, float desde, float hasta)
    {
        using var pintura = new SKPaint { Color = Colores.Separador, StrokeWidth = 2, IsAntialias = true };
        l.Canvas.DrawLine(x, desde, x, hasta, pintura);
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
