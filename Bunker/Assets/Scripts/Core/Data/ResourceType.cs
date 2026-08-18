// ============================================================================
// ResourceType.cs — Enumeración de todos los tipos de recurso del juego
// Basado en la Matriz de Recursos del GDD (Sección 5)
// ============================================================================
namespace Bunker.Core.Data
{
    /// <summary>
    /// Todos los tipos de recurso disponibles en el juego.
    /// Organizados por categoría según el GDD.
    /// </summary>
    public enum ResourceType
    {
        // ── BÁSICOS ──────────────────────────────────────────────────────
        Madera,
        Chatarra,
        Minerales,
        Combustible,

        // ── VITALES (Consumibles) ────────────────────────────────────────
        AguaContaminada,
        AguaPurificada,
        ComidaContaminada,
        ComidaLimpia,

        // ── HUMANOS Y SOCIALES ───────────────────────────────────────────
        ManoDeObra,
        Moral,
        Influencia,

        // ── TECNOLÓGICOS Y AVANZADOS ─────────────────────────────────────
        ComponentesElectronicos,
        EnergiaElectrica,
        Medicinas
    }

    /// <summary>
    /// Categoría a la que pertenece un recurso.
    /// </summary>
    public enum ResourceCategory
    {
        Basico,
        Vital,
        HumanoSocial,
        Tecnologico
    }
}
