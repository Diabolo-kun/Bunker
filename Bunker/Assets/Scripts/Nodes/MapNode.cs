// ============================================================================
// MapNode.cs — Clase base para todos los nodos del mapa
// GDD Sección 9B: Jerarquía de Nodos — MapNode (Clase Base)
// ============================================================================
using System;
using UnityEngine;

namespace Bunker.Nodes
{
    /// <summary>
    /// Clase base abstracta para cualquier nodo posicionado en el mapa.
    /// Contiene posición en el grid, tipo de terreno, estado y lógica de selección.
    /// </summary>
    public abstract class MapNode : MonoBehaviour
    {
        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cuando el nodo cambia de estado.</summary>
        public event Action<MapNode, Core.Data.NodeState> OnStateChanged;

        /// <summary>Se dispara cuando el nodo es seleccionado por el jugador.</summary>
        public event Action<MapNode> OnSelected;

        // ── Datos ────────────────────────────────────────────────────────
        [Header("── Posición ──")]
        [Tooltip("Posición del nodo en el grid del mapa (en tiles)")]
        [SerializeField] private Vector2Int posicionGrid;

        [Header("── Configuración ──")]
        [Tooltip("Plantilla de datos de este nodo")]
        [SerializeField] private Core.Data.NodeDataSO plantilla;

        [Tooltip("Estado actual del nodo")]
        [SerializeField] private Core.Data.NodeState estado = Core.Data.NodeState.SinExplorar;

        // ── Propiedades Públicas ─────────────────────────────────────────

        /// <summary>Posición en el grid del mapa (en tiles de 500m).</summary>
        public Vector2Int PosicionGrid => posicionGrid;

        /// <summary>Tipo de terreno de este nodo.</summary>
        public Core.Data.TerrainType TipoTerreno => plantilla != null ? plantilla.tipoTerreno : Core.Data.TerrainType.Erial;

        /// <summary>Plantilla de datos del nodo.</summary>
        public Core.Data.NodeDataSO Plantilla => plantilla;

        /// <summary>Estado actual del nodo.</summary>
        public Core.Data.NodeState Estado
        {
            get => estado;
            protected set
            {
                if (estado != value)
                {
                    estado = value;
                    OnStateChanged?.Invoke(this, estado);
                }
            }
        }

        // ── Configuración ────────────────────────────────────────────────

        /// <summary>
        /// Inicializa el nodo con una posición y plantilla.
        /// Llamado por el MapManager tras la generación procedural.
        /// </summary>
        public virtual void Configurar(Vector2Int pos, Core.Data.NodeDataSO datos)
        {
            posicionGrid = pos;
            plantilla = datos;
            estado = Core.Data.NodeState.SinExplorar;
            gameObject.name = $"Nodo_{datos.nombreMostrado}_{pos.x}_{pos.y}";
        }

        /// <summary>Marca el nodo como descubierto (visible en el mapa).</summary>
        public void Descubrir()
        {
            if (estado == Core.Data.NodeState.SinExplorar)
                Estado = Core.Data.NodeState.Descubierto;
        }

        // ── Selección ────────────────────────────────────────────────────

        /// <summary>Llamado cuando el jugador hace click en este nodo.</summary>
        public void Seleccionar()
        {
            OnSelected?.Invoke(this);
        }

        // ── Distancia ────────────────────────────────────────────────────

        /// <summary>
        /// Calcula la distancia en tiles hasta otra posición del grid.
        /// Usa distancia euclídea para más realismo.
        /// </summary>
        public float DistanciaATiles(Vector2Int otraPosicion)
        {
            return Vector2Int.Distance(posicionGrid, otraPosicion);
        }

        /// <summary>
        /// Calcula la distancia en metros hasta otra posición del grid.
        /// Cada tile = 500 metros.
        /// </summary>
        public float DistanciaAMetros(Vector2Int otraPosicion)
        {
            return DistanciaATiles(otraPosicion) * 500f;
        }

        /// <summary>
        /// Devuelve información formateada del nodo para la UI.
        /// </summary>
        public virtual string GetInfoFormateada()
        {
            if (plantilla == null) return "Nodo desconocido";
            return $"{plantilla.nombreMostrado}\nTerreno: {plantilla.tipoTerreno}\nEstado: {estado}";
        }
    }
}
