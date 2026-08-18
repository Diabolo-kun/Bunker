// ============================================================================
// ResourceDataSO.cs — ScriptableObject que define las propiedades de un recurso
// GDD Sección 5: Matriz de Recursos del Juego
// ============================================================================
using UnityEngine;

namespace Bunker.Core.Data
{
    /// <summary>
    /// Define las propiedades estáticas de un tipo de recurso.
    /// Crear un asset por cada recurso (Madera, Chatarra, Agua Contaminada, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "NuevoRecurso", menuName = "Bunker/Recursos/Definición de Recurso")]
    public class ResourceDataSO : ScriptableObject
    {
        [Header("── Identificación ──")]
        [Tooltip("Tipo de recurso que representa este asset")]
        public ResourceType tipo;

        [Tooltip("Nombre para mostrar en la UI")]
        public string nombreMostrado;

        [Tooltip("Descripción breve del recurso")]
        [TextArea(2, 4)]
        public string descripcion;

        [Tooltip("Categoría del recurso")]
        public ResourceCategory categoria;

        [Header("── Contaminación ──")]
        [Tooltip("¿Este recurso está contaminado y requiere purificación?")]
        public bool esContaminado;

        [Tooltip("Si está contaminado, ¿en qué recurso limpio se convierte?")]
        public ResourceType recursoLimpioResultante;

        [Tooltip("Ratio de conversión: cuántas unidades contaminadas se necesitan para 1 limpia")]
        [Range(0.1f, 10f)]
        public float ratioConversion = 1f;

        [Header("── Almacenamiento ──")]
        [Tooltip("Capacidad máxima de almacenamiento por defecto en el búnker")]
        public int capacidadMaximaPorDefecto = 500;

        [Tooltip("¿Se puede almacenar este recurso? (Moral e Influencia son instantáneos)")]
        public bool esAlmacenable = true;

        [Header("── Visual ──")]
        [Tooltip("Icono del recurso para la UI")]
        public Sprite icono;

        [Tooltip("Color representativo del recurso en la UI")]
        public Color colorUI = Color.white;
    }
}
