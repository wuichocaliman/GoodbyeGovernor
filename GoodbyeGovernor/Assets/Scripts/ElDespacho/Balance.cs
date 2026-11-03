using UnityEngine;

namespace ElDespacho
{
    /// <summary>
    /// Números de dificultad del juego, en un solo sitio para poder ajustarlos sin tocar la lógica.
    ///
    /// Valores elegidos con una simulación de 4.000 partidas por tipo de jugador (v0.6):
    /// - Jugador «humano» (acierta la diplomacia el 80 % y elige bien el 70 % de las cartas):
    ///   pierde ~23 % de las partidas (antes ~2 %), sobre todo en los años 3 y 4.
    /// - Jugador que elige al azar: pierde ~98 % (antes ~74 %), con derrotas desde el año 2.
    /// - Jugador perfecto: sigue pudiendo ganar, pero con algún indicador por debajo de 30.
    /// Si se cambian estos números, conviene volver a simular o jugar varias partidas completas.
    /// </summary>
    public static class Balance
    {
        // ── Valores iniciales (antes 60 / 55 / 50) ──
        public const int StartEstabilidad = 50;
        public const int StartSeguridad = 45;
        public const int StartProsperidad = 45;

        // ── Desgaste del cargo: se resta al empezar cada año desde WearFromYear ──
        // El año 1 hace de tutorial y no tiene desgaste.
        public static readonly Effect YearlyWear = new Effect(-3, -4, -3);
        public const int WearFromYear = 2;

        // ── Las cartas pesan más cada año: multiplica SOLO los efectos negativos ──
        // Índice 0 = año 1. Los efectos positivos no cambian.
        static readonly float[] CardCostByYear = { 1f, 1.2f, 1.4f, 1.6f };

        public static float CardCostMultiplier(int year) =>
            CardCostByYear[Mathf.Clamp(year - 1, 0, CardCostByYear.Length - 1)];

        /// <summary>Efecto real de una respuesta de carta en el año dado.</summary>
        public static Effect ScaleCardCost(Effect e, int year)
        {
            float m = CardCostMultiplier(year);
            int Scale(int v) => v < 0 ? Mathf.RoundToInt(v * m) : v;
            return new Effect(Scale(e.estabilidad), Scale(e.seguridad), Scale(e.prosperidad));
        }
    }
}
