// ============================================================================
// NodeDataSO.cs — ScriptableObject que define plantillas de nodos de recurso
// GDD Sección 4: Tipología y Ciclo de Vida de los Nodos
// ============================================================================
using UnityEngine;

namespace Bunker.Core.Data
{
    /// <summary>
    /// Define la plantilla estática de un tipo de nodo en el mapa.
    /// Cada tipo de terreno tiene una o más plantillas que determinan
    /// qué recursos genera, en qué cantidades y a qué velocidad.
    /// </summary>
    [CreateAssetMenu(fileName = "NuevoNodo", menuName = "Bunker/Mapa/Plantilla de Nodo")]
    public class NodeDataSO : ScriptableObject
    {
        [Header("── Identificación ──")]
        [Tooltip("Nombre para mostrar en la UI")]
        public string nombreMostrado;

        [Tooltip("Tipo de terreno que representa este nodo")]
        public TerrainType tipoTerreno;

        [Tooltip("Descripción breve del nodo")]
        [TextArea(2, 4)]
        public string descripcion;

        [Header("── Recursos ──")]
        [Tooltip("Tipos de recurso que ofrece este nodo")]
        public ResourceType[] recursosDisponibles;

        [Tooltip("Cantidad base de cada recurso (mismo orden que recursosDisponibles)")]
        public float[] cantidadBase;

        [Tooltip("Unidades de recurso recolectadas por tick de trabajo")]
        [Range(0.1f, 10f)]
        public float tasaRecoleccionPorTick = 1f;

        [Header("── Visual ──")]
        [Tooltip("Icono del nodo para el mapa")]
        public Sprite iconoNodo;

        [Tooltip("Color representativo en el mapa")]
        public Color colorMapa = Color.white;

        [Header("── Variación ──")]
        [Tooltip("Multiplicador mínimo para la cantidad de recursos (variación procedural)")]
        [Range(0.5f, 1f)]
        public float multiplicadorMin = 0.7f;

        [Tooltip("Multiplicador máximo para la cantidad de recursos (variación procedural)")]
        [Range(1f, 2f)]
        public float multiplicadorMax = 1.3f;
    }
}
