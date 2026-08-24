// ============================================================================
// ResourcePanelUI.cs — Panel de UI que muestra los recursos del búnker
// y gestiona los botones de comando que abren un panel lateral informativo.
// GDD Sección 6: Oficina del Supervisor
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace Bunker.UI.HUD
{
    /// <summary>
    /// Controla los indicadores de recursos pre-colocados en la barra del supervisor
    /// y gestiona los botones de comando que abren un panel lateral con información
    /// detallada de la categoría seleccionada.
    /// </summary>
    public class ResourcePanelUI : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────────
        // Indicadores de recursos
        // ─────────────────────────────────────────────────────────────────
        [Header("── Indicadores ──")]
        [Tooltip("Lista de filas de recurso pre-colocadas en la escena (en orden)")]
        [SerializeField] private List<ResourceRowUI> indicadores;

        [Tooltip("Definiciones de recursos (mismo orden que los indicadores)")]
        [SerializeField] private List<Core.Data.ResourceDataSO> recursosAMostrar;

        // ─────────────────────────────────────────────────────────────────
        // Botones de comando → panel lateral
        // ─────────────────────────────────────────────────────────────────
        [Header("── Botones de Comando ──")]
        [SerializeField] private Button btnDecisiones;
        [SerializeField] private Button btnInvestigacion;
        [SerializeField] private Button btnDiplomacia;
        [SerializeField] private Button btnElectricidad;
        [SerializeField] private Button btnComidaAgua;
        [SerializeField] private Button btnConsumo;

        // ─────────────────────────────────────────────────────────────────
        // Panel lateral de información
        // ─────────────────────────────────────────────────────────────────
        [Header("── Panel Lateral ──")]
        [Tooltip("GameObject raíz del panel lateral que se muestra/oculta")]
        [SerializeField] private GameObject panelLateral;

        [Tooltip("Título del panel lateral")]
        [SerializeField] private TextMeshProUGUI tituloPanel;

        [Tooltip("Contenido descriptivo del panel lateral")]
        [SerializeField] private TextMeshProUGUI contenidoPanel;

        [Tooltip("Contenedor donde se listan los recursos de la categoría")]
        [SerializeField] private Transform contenedorDetalles;

        [Tooltip("Prefab para cada fila de detalle dentro del panel lateral")]
        [SerializeField] private GameObject detalleRowPrefab;

        [Tooltip("Botón para cerrar el panel lateral")]
        [SerializeField] private Button btnCerrarPanel;

        // ─────────────────────────────────────────────────────────────────
        // Estado interno
        // ─────────────────────────────────────────────────────────────────
        private Dictionary<Core.Data.ResourceType, ResourceRowUI> indicadoresPorTipo
            = new Dictionary<Core.Data.ResourceType, ResourceRowUI>();

        private string seccionActiva = "";

        // ═════════════════════════════════════════════════════════════════
        // Ciclo de vida
        // ═════════════════════════════════════════════════════════════════
        private void Start()
        {
            InicializarIndicadores();
            ConfigurarBotones();
            SuscribirEventos();
            ActualizarTodosLosIndicadores();

            // Panel lateral oculto por defecto
            if (panelLateral != null)
                panelLateral.SetActive(false);
        }

        private void OnDestroy()
        {
            DesuscribirEventos();
            DesconfigurarBotones();
        }

        // ═════════════════════════════════════════════════════════════════
        // Indicadores
        // ═════════════════════════════════════════════════════════════════
        private void InicializarIndicadores()
        {
            int count = Mathf.Min(
                indicadores != null ? indicadores.Count : 0,
                recursosAMostrar != null ? recursosAMostrar.Count : 0);

            for (int i = 0; i < count; i++)
            {
                var fila = indicadores[i];
                var def = recursosAMostrar[i];
                if (fila == null || def == null) continue;

                fila.Inicializar(def);
                indicadoresPorTipo[def.tipo] = fila;
            }
        }

        private void SuscribirEventos()
        {
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm != null)
                rm.OnResourceChanged += OnRecursoChanged;
        }

        private void DesuscribirEventos()
        {
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm != null)
                rm.OnResourceChanged -= OnRecursoChanged;
        }

        private void OnRecursoChanged(Core.Data.ResourceType tipo, float anterior, float nuevo)
        {
            if (indicadoresPorTipo.TryGetValue(tipo, out var fila))
            {
                var rm = Core.Managers.ResourceManager.Instance;
                fila.Actualizar(nuevo, rm.GetMaxCapacity(tipo));
            }
        }

        private void ActualizarTodosLosIndicadores()
        {
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm == null) return;

            foreach (var kvp in indicadoresPorTipo)
                kvp.Value.Actualizar(rm.GetAmount(kvp.Key), rm.GetMaxCapacity(kvp.Key));
        }

        // ═════════════════════════════════════════════════════════════════
        // Botones de comando
        // ═════════════════════════════════════════════════════════════════
        private void ConfigurarBotones()
        {
            if (btnDecisiones != null)
                btnDecisiones.onClick.AddListener(() => ToggleSeccion("Decisiones"));
            if (btnInvestigacion != null)
                btnInvestigacion.onClick.AddListener(() => ToggleSeccion("Investigación"));
            if (btnDiplomacia != null)
                btnDiplomacia.onClick.AddListener(() => ToggleSeccion("Diplomacia"));
            if (btnElectricidad != null)
                btnElectricidad.onClick.AddListener(() => ToggleSeccion("Electricidad"));
            if (btnComidaAgua != null)
                btnComidaAgua.onClick.AddListener(() => ToggleSeccion("Comida y Agua"));
            if (btnConsumo != null)
                btnConsumo.onClick.AddListener(() => ToggleSeccion("Consumo"));
            if (btnCerrarPanel != null)
                btnCerrarPanel.onClick.AddListener(CerrarPanel);
        }

        private void DesconfigurarBotones()
        {
            if (btnDecisiones != null) btnDecisiones.onClick.RemoveAllListeners();
            if (btnInvestigacion != null) btnInvestigacion.onClick.RemoveAllListeners();
            if (btnDiplomacia != null) btnDiplomacia.onClick.RemoveAllListeners();
            if (btnElectricidad != null) btnElectricidad.onClick.RemoveAllListeners();
            if (btnComidaAgua != null) btnComidaAgua.onClick.RemoveAllListeners();
            if (btnConsumo != null) btnConsumo.onClick.RemoveAllListeners();
            if (btnCerrarPanel != null) btnCerrarPanel.onClick.RemoveAllListeners();
        }

        // ═════════════════════════════════════════════════════════════════
        // Panel lateral
        // ═════════════════════════════════════════════════════════════════

        /// <summary>
        /// Alterna la visibilidad del panel lateral. Si se pulsa el mismo botón
        /// que ya está activo, cierra el panel. Si se pulsa otro, cambia el contenido.
        /// </summary>
        private void ToggleSeccion(string seccion)
        {
            if (panelLateral == null) return;

            // Si ya está abierto con la misma sección, cerrar
            if (panelLateral.activeSelf && seccionActiva == seccion)
            {
                CerrarPanel();
                return;
            }

            // Abrir o cambiar sección
            seccionActiva = seccion;
            MostrarContenidoSeccion(seccion);
            panelLateral.SetActive(true);
        }

        /// <summary>Cierra el panel lateral.</summary>
        public void CerrarPanel()
        {
            if (panelLateral != null)
                panelLateral.SetActive(false);

            seccionActiva = "";
        }

        /// <summary>
        /// Rellena el panel lateral con información según la sección seleccionada.
        /// Filtra los recursos por categoría relevante y muestra sus valores actuales.
        /// </summary>
        private void MostrarContenidoSeccion(string seccion)
        {
            if (tituloPanel != null)
                tituloPanel.text = seccion;

            // Limpiar contenido anterior del contenedor de detalles
            if (contenedorDetalles != null)
            {
                for (int i = contenedorDetalles.childCount - 1; i >= 0; i--)
                    Destroy(contenedorDetalles.GetChild(i).gameObject);
            }

            // Determinar qué recursos mostrar y qué descripción dar
            var categoriasFiltro = ObtenerCategoriasPorSeccion(seccion);
            string descripcion = ObtenerDescripcionSeccion(seccion);

            if (contenidoPanel != null)
                contenidoPanel.text = descripcion;

            // Crear filas de detalle para los recursos relevantes
            if (contenedorDetalles != null && detalleRowPrefab != null && recursosAMostrar != null)
            {
                var rm = Core.Managers.ResourceManager.Instance;

                foreach (var def in recursosAMostrar)
                {
                    if (def == null) continue;
                    if (categoriasFiltro != null && !categoriasFiltro.Contains(def.categoria)) continue;

                    var fila = Instantiate(detalleRowPrefab, contenedorDetalles);
                    var rowUI = fila.GetComponent<ResourceRowUI>();
                    if (rowUI != null)
                    {
                        rowUI.Inicializar(def);
                        if (rm != null)
                            rowUI.Actualizar(rm.GetAmount(def.tipo), rm.GetMaxCapacity(def.tipo));
                    }
                }
            }
        }

        /// <summary>
        /// Devuelve las categorías de recurso asociadas a cada sección de botón.
        /// Si devuelve null, no se filtra (se muestran todos).
        /// </summary>
        private List<Core.Data.ResourceCategory> ObtenerCategoriasPorSeccion(string seccion)
        {
            switch (seccion)
            {
                case "Electricidad":
                    return new List<Core.Data.ResourceCategory>
                        { Core.Data.ResourceCategory.Tecnologico };

                case "Comida y Agua":
                    return new List<Core.Data.ResourceCategory>
                        { Core.Data.ResourceCategory.Vital };

                case "Consumo":
                    return new List<Core.Data.ResourceCategory>
                        { Core.Data.ResourceCategory.Basico };

                case "Decisiones":
                case "Investigación":
                case "Diplomacia":
                    return new List<Core.Data.ResourceCategory>
                        { Core.Data.ResourceCategory.HumanoSocial };

                default:
                    return null; // Mostrar todos
            }
        }

        /// <summary>Devuelve una descripción contextual para cada sección.</summary>
        private string ObtenerDescripcionSeccion(string seccion)
        {
            switch (seccion)
            {
                case "Decisiones":
                    return "Gestiona las decisiones críticas del búnker. " +
                           "Las elecciones afectan la moral y la influencia.";
                case "Investigación":
                    return "Investiga nuevas tecnologías y mejoras. " +
                           "Requiere componentes electrónicos y mano de obra.";
                case "Diplomacia":
                    return "Negocia con facciones externas. " +
                           "La influencia determina tu poder de negociación.";
                case "Electricidad":
                    return "Controla la producción y distribución eléctrica. " +
                           "Fundamental para mantener los sistemas del búnker.";
                case "Comida y Agua":
                    return "Supervisa las reservas vitales de comida y agua. " +
                           "Gestiona la purificación de recursos contaminados.";
                case "Consumo":
                    return "Administra los materiales básicos de construcción. " +
                           "Madera, chatarra, minerales y combustible.";
                default:
                    return "";
            }
        }
    }
}
