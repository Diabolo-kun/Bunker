// ============================================================================
// NodeInfoPanelUI.cs — Panel de información del nodo seleccionado
// GDD Sección 2.7: UI del Mapa — Panel lateral de información
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Bunker.UI.Panels
{
    /// <summary>
    /// Panel lateral que muestra información detallada del nodo seleccionado
    /// en el mapa. Incluye tipo, recursos disponibles, porcentaje restante
    /// y un botón para enviar cuadrilla.
    /// </summary>
    public class NodeInfoPanelUI : MonoBehaviour
    {
        [Header("── Panel ──")]
        [SerializeField] private GameObject panelRaiz;

        [Header("── Información ──")]
        [SerializeField] private TextMeshProUGUI tituloTexto;
        [SerializeField] private TextMeshProUGUI tipoTerrenoTexto;
        [SerializeField] private TextMeshProUGUI estadoTexto;
        [SerializeField] private TextMeshProUGUI descripcionTexto;

        [Header("── Recursos ──")]
        [SerializeField] private Transform contenedorRecursos;
        [SerializeField] private GameObject recursoRowPrefab;

        [Header("── Acciones ──")]
        [SerializeField] private Button btnEnviarCuadrilla;
        [SerializeField] private TextMeshProUGUI btnEnviarTexto;
        [SerializeField] private Button btnCerrar;

        // ── Estado ───────────────────────────────────────────────────────
        private Nodes.NaturalNode nodoActual;

        // ── Inicialización ───────────────────────────────────────────────

        private void Start()
        {
            // Panel oculto al inicio
            if (panelRaiz != null)
                panelRaiz.SetActive(false);

            // Configurar botones
            if (btnEnviarCuadrilla != null)
                btnEnviarCuadrilla.onClick.AddListener(OnEnviarCuadrillaClick);

            if (btnCerrar != null)
                btnCerrar.onClick.AddListener(Cerrar);

            // Suscribirse a eventos del MapManager
            var mapManager = Map.MapManager.Instance;
            if (mapManager != null)
            {
                mapManager.OnNodeSelected += OnNodoSeleccionado;
                mapManager.OnNodeDeselected += OnNodoDeseleccionado;
            }
        }

        private void OnDestroy()
        {
            var mapManager = Map.MapManager.Instance;
            if (mapManager != null)
            {
                mapManager.OnNodeSelected -= OnNodoSeleccionado;
                mapManager.OnNodeDeselected -= OnNodoDeseleccionado;
            }

            if (btnEnviarCuadrilla != null)
                btnEnviarCuadrilla.onClick.RemoveAllListeners();
            if (btnCerrar != null)
                btnCerrar.onClick.RemoveAllListeners();
        }

        // ── Handlers de selección ────────────────────────────────────────

        private void OnNodoSeleccionado(Nodes.MapNode nodo)
        {
            nodoActual = nodo as Nodes.NaturalNode;
            if (nodoActual == null) return;

            MostrarInfo(nodoActual);
        }

        private void OnNodoDeseleccionado()
        {
            Cerrar();
        }

        // ── Mostrar información ──────────────────────────────────────────

        private void MostrarInfo(Nodes.NaturalNode nodo)
        {
            if (panelRaiz == null) return;
            panelRaiz.SetActive(true);

            var plantilla = nodo.Plantilla;
            if (plantilla == null) return;

            // Título y tipo
            if (tituloTexto != null)
                tituloTexto.text = plantilla.nombreMostrado;

            if (tipoTerrenoTexto != null)
                tipoTerrenoTexto.text = $"Terreno: {plantilla.tipoTerreno}";

            if (estadoTexto != null)
                estadoTexto.text = $"Estado: {nodo.Estado}";

            if (descripcionTexto != null)
                descripcionTexto.text = plantilla.descripcion;

            // Limpiar recursos anteriores
            if (contenedorRecursos != null)
            {
                for (int i = contenedorRecursos.childCount - 1; i >= 0; i--)
                    Destroy(contenedorRecursos.GetChild(i).gameObject);
            }

            // Mostrar recursos disponibles
            if (contenedorRecursos != null && recursoRowPrefab != null)
            {
                foreach (var kvp in nodo.RecursosRestantes)
                {
                    var row = Instantiate(recursoRowPrefab, contenedorRecursos);

                    // Intentar encontrar TextMeshPro en la fila
                    var textos = row.GetComponentsInChildren<TextMeshProUGUI>();
                    if (textos.Length >= 2)
                    {
                        textos[0].text = kvp.Key.ToString();
                        textos[1].text = $"{Mathf.FloorToInt(kvp.Value)} ({nodo.GetPorcentajeRestante(kvp.Key):P0})";
                    }

                    // Barra de progreso
                    var barras = row.GetComponentsInChildren<Image>();
                    foreach (var barra in barras)
                    {
                        if (barra.type == Image.Type.Filled)
                        {
                            barra.fillAmount = nodo.GetPorcentajeRestante(kvp.Key);
                        }
                    }
                }
            }

            // Botón de enviar cuadrilla
            ActualizarBotonEnviar(nodo);
        }

        private void ActualizarBotonEnviar(Nodes.NaturalNode nodo)
        {
            if (btnEnviarCuadrilla == null) return;

            var squadManager = Squads.SquadManager.Instance;
            bool puedeEnviar = squadManager != null
                               && squadManager.PuedeEnviarMas
                               && !nodo.EstaAgotado
                               && nodo.Estado != Core.Data.NodeState.SinExplorar;

            btnEnviarCuadrilla.interactable = puedeEnviar;

            if (btnEnviarTexto != null)
            {
                if (nodo.EstaAgotado)
                    btnEnviarTexto.text = "Nodo agotado";
                else if (squadManager != null && !squadManager.PuedeEnviarMas)
                    btnEnviarTexto.text = "Sin cuadrillas disponibles";
                else
                    btnEnviarTexto.text = $"Enviar Cuadrilla ({squadManager?.MiembrosPorDefecto ?? 5} pers.)";
            }
        }

        // ── Acciones ─────────────────────────────────────────────────────

        private void OnEnviarCuadrillaClick()
        {
            if (nodoActual == null) return;

            var squadManager = Squads.SquadManager.Instance;
            if (squadManager == null) return;

            var squad = squadManager.EnviarCuadrilla(nodoActual);
            if (squad != null)
            {
                Debug.Log($"[NodeInfoPanel] Cuadrilla enviada a {nodoActual.Plantilla.nombreMostrado}");
                // Refrescar el panel
                MostrarInfo(nodoActual);
            }
        }

        /// <summary>Cierra el panel.</summary>
        public void Cerrar()
        {
            if (panelRaiz != null)
                panelRaiz.SetActive(false);

            nodoActual = null;
        }
    }
}
