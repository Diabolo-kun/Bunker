// ============================================================================
// TerrainType.cs — Tipos de terreno y estados de nodo para el sistema de mapa
// GDD Sección 3: Sistema de Mapa Geoespacial
// ============================================================================
namespace Bunker.Core.Data
{
    /// <summary>
    /// Tipos de terreno que determinan qué recursos genera un nodo.
    /// Basados en las capas geográficas del GDD (landuse, natural, etc.)
    /// </summary>
    public enum TerrainType
    {
        /// <summary>Zonas residenciales/comerciales: Chatarra, Componentes, Medicinas.</summary>
        Urbano,

        /// <summary>Masas forestales: Madera, Comida Contaminada (caza).</summary>
        Forestal,

        /// <summary>Ríos, lagos, pozos: Agua Contaminada, potencial energético.</summary>
        Acuatico,

        /// <summary>Terrenos agrícolas/praderas: Comida Contaminada, bonus futuro.</summary>
        Agricola,

        /// <summary>Zonas industriales/fábricas: Chatarra, Combustible, Componentes.</summary>
        Industrial,

        /// <summary>Terreno baldío sin recursos significativos.</summary>
        Erial
    }

    /// <summary>
    /// Estado del ciclo de vida de un nodo en el mapa.
    /// </summary>
    public enum NodeState
    {
        /// <summary>El jugador aún no ha descubierto este nodo.</summary>
        SinExplorar,

        /// <summary>Visible en el mapa pero sin cuadrilla asignada.</summary>
        Descubierto,

        /// <summary>Hay una cuadrilla recolectando activamente.</summary>
        EnExplotacion,

        /// <summary>Todos los recursos del nodo se han agotado.</summary>
        Agotado
    }
}
