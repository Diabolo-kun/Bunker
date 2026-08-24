// ============================================================================
// MapManager.cs — Gestor central del mapa, nodos y radio de influencia
// GDD Sección 9C: Gestores Centrales — GeoMapManager
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bunker.Map
{
    /// <summary>
    /// Singleton que gestiona el mapa completo: terreno, nodos instanciados,
    /// radio de influencia del búnker y selección de nodos.
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static MapManager Instance { get; private set; }

        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cuando se genera un nuevo mapa.</summary>
        public event Action OnMapGenerated;

        /// <summary>Se dispara cuando se selecciona un nodo.</summary>
        public event Action<Nodes.MapNode> OnNodeSelected;

        /// <summary>Se dispara cuando se deselecciona.</summary>
        public event Action OnNodeDeselected;

        // ── Configuración ────────────────────────────────────────────────
        [Header("── Generador ──")]
        [SerializeField] private MapGenerator generador;

        [Header("── Plantillas de Nodos ──")]
        [Tooltip("Plantilla para nodos urbanos")]
        [SerializeField] private Core.Data.NodeDataSO plantillaUrbano;

        [Tooltip("Plantilla para nodos forestales")]
        [SerializeField] private Core.Data.NodeDataSO plantillaForestal;

        [Tooltip("Plantilla para nodos acuáticos")]
        [SerializeField] private Core.Data.NodeDataSO plantillaAcuatico;

        [Tooltip("Plantilla para nodos agrícolas")]
        [SerializeField] private Core.Data.NodeDataSO plantillaAgricola;

        [Tooltip("Plantilla para nodos industriales")]
        [SerializeField] private Core.Data.NodeDataSO plantillaIndustrial;

        [Header("── Contenedores ──")]
        [Tooltip("Transform padre para los nodos instanciados")]
        [SerializeField] private Transform contenedorNodos;

        // ── Estado ───────────────────────────────────────────────────────

        /// <summary>Grid de terrenos generado.</summary>
        public Core.Data.TerrainType[,] GridTerreno { get; private set; }

        /// <summary>Posición del búnker en el grid.</summary>
        public Vector2Int PosicionBunker { get; private set; }

        /// <summary>Radio de influencia actual del búnker (en tiles).</summary>
        public int RadioInfluencia { get; private set; }

        /// <summary>Ancho del mapa en tiles.</summary>
        public int AnchoMapa => generador != null ? generador.AnchoTiles : 0;

        /// <summary>Alto del mapa en tiles.</summary>
        public int AltoMapa => generador != null ? generador.AltoTiles : 0;

        /// <summary>Nodo actualmente seleccionado.</summary>
        public Nodes.MapNode NodoSeleccionado { get; private set; }

        // Almacenamiento interno
        private Dictionary<Vector2Int, Nodes.NaturalNode> nodosPorPosicion
            = new Dictionary<Vector2Int, Nodes.NaturalNode>();

        private List<Nodes.NaturalNode> todosLosNodos = new List<Nodes.NaturalNode>();

        // ── Inicialización ───────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Genera el mapa proceduralmente e instancia todos los nodos.
        /// Llamar desde GameManager.Start() o cuando se inicie una partida.
        /// </summary>
        public void GenerarMapa()
        {
            if (generador == null)
            {
                Debug.LogError("[MapManager] No hay MapGenerator asignado.");
                return;
            }

            // Limpiar mapa anterior
            LimpiarMapa();

            // Generar
            var resultado = generador.Generar();
            GridTerreno = resultado.gridTerreno;
            PosicionBunker = resultado.posicionBunker;
            RadioInfluencia = resultado.radioInicial;

            // Instanciar nodos
            foreach (var nodoProcedural in resultado.nodos)
            {
                InstanciarNodo(nodoProcedural);
            }

            // Descubrir nodos dentro del radio inicial
            DescubrirNodosEnRadio(PosicionBunker, RadioInfluencia);

            Debug.Log($"[MapManager] Mapa generado: {AnchoMapa}x{AltoMapa}, " +
                      $"{todosLosNodos.Count} nodos, radio {RadioInfluencia}");

            OnMapGenerated?.Invoke();
        }

        // ── Instanciación de nodos ───────────────────────────────────────

        private void InstanciarNodo(MapGenerator.NodoProcedural datos)
        {
            var plantilla = ObtenerPlantilla(datos.terreno);
            if (plantilla == null)
            {
                Debug.LogWarning($"[MapManager] Sin plantilla para terreno {datos.terreno}");
                return;
            }

            Transform padre = contenedorNodos != null ? contenedorNodos : transform;
            var go = new GameObject();
            go.transform.SetParent(padre);

            var nodo = go.AddComponent<Nodes.NaturalNode>();
            nodo.ConfigurarConVariacion(datos.posicion, plantilla, datos.multiplicadorRecursos);

            nodosPorPosicion[datos.posicion] = nodo;
            todosLosNodos.Add(nodo);

            // Suscribirse a eventos del nodo
            nodo.OnSelected += OnNodoClickeado;
            nodo.OnNodoAgotado += OnNodoAgotadoHandler;
        }

        private Core.Data.NodeDataSO ObtenerPlantilla(Core.Data.TerrainType terreno)
        {
            switch (terreno)
            {
                case Core.Data.TerrainType.Urbano: return plantillaUrbano;
                case Core.Data.TerrainType.Forestal: return plantillaForestal;
                case Core.Data.TerrainType.Acuatico: return plantillaAcuatico;
                case Core.Data.TerrainType.Agricola: return plantillaAgricola;
                case Core.Data.TerrainType.Industrial: return plantillaIndustrial;
                default: return null;
            }
        }

        // ── Radio de influencia ──────────────────────────────────────────

        /// <summary>
        /// Descubre todos los nodos dentro de un radio desde una posición.
        /// </summary>
        public void DescubrirNodosEnRadio(Vector2Int centro, int radio)
        {
            foreach (var nodo in todosLosNodos)
            {
                if (nodo.DistanciaATiles(centro) <= radio)
                {
                    nodo.Descubrir();
                }
            }
        }

        /// <summary>
        /// Amplía el radio de influencia del búnker.
        /// </summary>
        public void AmpliarRadio(int nuevoRadio)
        {
            RadioInfluencia = nuevoRadio;
            DescubrirNodosEnRadio(PosicionBunker, RadioInfluencia);
        }

        /// <summary>
        /// ¿Está una posición dentro del radio de influencia del búnker?
        /// </summary>
        public bool EstaDentroDelRadio(Vector2Int posicion)
        {
            return Vector2Int.Distance(posicion, PosicionBunker) <= RadioInfluencia;
        }

        // ── Selección ────────────────────────────────────────────────────

        private void OnNodoClickeado(Nodes.MapNode nodo)
        {
            if (NodoSeleccionado != null && NodoSeleccionado != nodo)
            {
                // Deseleccionar anterior (visual)
            }

            NodoSeleccionado = nodo;
            OnNodeSelected?.Invoke(nodo);
        }

        /// <summary>Deselecciona el nodo actual.</summary>
        public void DeseleccionarNodo()
        {
            NodoSeleccionado = null;
            OnNodeDeselected?.Invoke();
        }

        // ── Consultas ────────────────────────────────────────────────────

        /// <summary>Obtiene el nodo en una posición del grid.</summary>
        public Nodes.NaturalNode GetNodoEn(Vector2Int posicion)
        {
            nodosPorPosicion.TryGetValue(posicion, out var nodo);
            return nodo;
        }

        /// <summary>Obtiene todos los nodos descubiertos.</summary>
        public List<Nodes.NaturalNode> GetNodosDescubiertos()
        {
            var descubiertos = new List<Nodes.NaturalNode>();
            foreach (var nodo in todosLosNodos)
            {
                if (nodo.Estado != Core.Data.NodeState.SinExplorar)
                    descubiertos.Add(nodo);
            }
            return descubiertos;
        }

        /// <summary>Devuelve todos los nodos del mapa.</summary>
        public IReadOnlyList<Nodes.NaturalNode> GetTodosLosNodos() => todosLosNodos.AsReadOnly();

        /// <summary>Obtiene el tipo de terreno en una celda del grid.</summary>
        public Core.Data.TerrainType GetTerreno(int x, int y)
        {
            if (GridTerreno == null) return Core.Data.TerrainType.Erial;
            if (x < 0 || x >= AnchoMapa || y < 0 || y >= AltoMapa)
                return Core.Data.TerrainType.Erial;
            return GridTerreno[x, y];
        }

        // ── Handlers ─────────────────────────────────────────────────────

        private void OnNodoAgotadoHandler(Nodes.NaturalNode nodo)
        {
            Debug.Log($"[MapManager] Nodo agotado: {nodo.Plantilla.nombreMostrado} en {nodo.PosicionGrid}");
        }

        // ── Limpieza ─────────────────────────────────────────────────────

        private void LimpiarMapa()
        {
            foreach (var nodo in todosLosNodos)
            {
                if (nodo != null)
                {
                    nodo.OnSelected -= OnNodoClickeado;
                    nodo.OnNodoAgotado -= OnNodoAgotadoHandler;
                    Destroy(nodo.gameObject);
                }
            }

            todosLosNodos.Clear();
            nodosPorPosicion.Clear();
            GridTerreno = null;
        }

        private void OnDestroy()
        {
            LimpiarMapa();
        }
    }
}
