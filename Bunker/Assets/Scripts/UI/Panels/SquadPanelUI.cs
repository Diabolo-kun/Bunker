// ============================================================================
// SquadPanelUI.cs — Panel de cuadrillas activas
// GDD Sección 2.7: UI del Mapa — Panel de cuadrillas
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Bunker.UI.Panels
{
    /// <summary>
    /// Panel que muestra las cuadrillas activas con su estado,
    /// destino, barra de progreso y botón de cancelar.
    /// Se actualiza cada tick del reloj de simulación.
    /// </summary>
    public class SquadPanelUI : MonoBehaviour
    {
        [Header("── Panel ──")]
        [SerializeField] private GameObject panelRaiz;

        [Header("── Contenido ──")]
        [Tooltip("Contenedor donde se instancian las filas de cuadrilla")]
        [SerializeField] private Transform contenedorFilas;

        [Tooltip("Prefab para cada fila de cuadrilla")]
        [SerializeField] private GameObject filaSquadPrefab;

        [Header("── Info General ──")]
        [SerializeField] private TextMeshProUGUI contadorTexto;

        // ── Estado ───────────────────────────────────────────────────────
        private Dictionary<int, GameObject> filasInstanciadas = new Dictionary<int, GameObject>();

        // ── Inicialización ───────────────────────────────────────────────

        private void Start()
        {
            // Suscribirse a eventos
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock != null)
                clock.OnTick += ActualizarPanel;

            var squadManager = Squads.SquadManager.Instance;
            if (squadManager != null)
            {
                squadManager.OnSquadDeparted += OnCuadrillaEnviada;
                squadManager.OnSquadReturned += OnCuadrillaFinalizada;
                squadManager.OnSquadCancelled += OnCuadrillaFinalizada;
            }
        }

        private void OnDestroy()
        {
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock != null)
                clock.OnTick -= ActualizarPanel;

            var squadManager = Squads.SquadManager.Instance;
            if (squadManager != null)
            {
                squadManager.OnSquadDeparted -= OnCuadrillaEnviada;
                squadManager.OnSquadReturned -= OnCuadrillaFinalizada;
                squadManager.OnSquadCancelled -= OnCuadrillaFinalizada;
            }
        }

        // ── Handlers ─────────────────────────────────────────────────────

        private void OnCuadrillaEnviada(Squads.Squad squad)
        {
            CrearFilaCuadrilla(squad);
            ActualizarContador();
        }

        private void OnCuadrillaFinalizada(Squads.Squad squad)
        {
            EliminarFilaCuadrilla(squad.Id);
            ActualizarContador();
        }

        // ── Creación/Eliminación de filas ─────────────────────────────────

        private void CrearFilaCuadrilla(Squads.Squad squad)
        {
            if (contenedorFilas == null || filaSquadPrefab == null) return;
            if (filasInstanciadas.ContainsKey(squad.Id)) return;

            var fila = Instantiate(filaSquadPrefab, contenedorFilas);
            fila.name = $"Squad_{squad.Id}";
            filasInstanciadas[squad.Id] = fila;

            // Configurar botón cancelar si existe
            var btnCancelar = fila.GetComponentInChildren<Button>();
            if (btnCancelar != null)
            {
                int squadId = squad.Id;
                btnCancelar.onClick.AddListener(() => CancelarCuadrilla(squadId));
            }

            ActualizarFilaCuadrilla(squad, fila);
        }

        private void EliminarFilaCuadrilla(int squadId)
        {
            if (filasInstanciadas.TryGetValue(squadId, out var fila))
            {
                Destroy(fila);
                filasInstanciadas.Remove(squadId);
            }
        }

        // ── Actualización ────────────────────────────────────────────────

        private void ActualizarPanel()
        {
            var squadManager = Squads.SquadManager.Instance;
            if (squadManager == null) return;

            foreach (var squad in squadManager.CuadrillasActivas)
            {
                if (filasInstanciadas.TryGetValue(squad.Id, out var fila))
                {
                    ActualizarFilaCuadrilla(squad, fila);
                }
            }
        }

        private void ActualizarFilaCuadrilla(Squads.Squad squad, GameObject fila)
        {
            if (fila == null) return;

            var textos = fila.GetComponentsInChildren<TextMeshProUGUI>();
            if (textos.Length >= 2)
            {
                textos[0].text = $"Cuadrilla #{squad.Id}";
                textos[1].text = squad.GetEstadoFormateado();
            }

            // Barra de progreso
            var barras = fila.GetComponentsInChildren<Image>();
            foreach (var barra in barras)
            {
                if (barra.type == Image.Type.Filled)
                {
                    barra.fillAmount = squad.Progreso;
                }
            }
        }

        private void ActualizarContador()
        {
            if (contadorTexto == null) return;

            var squadManager = Squads.SquadManager.Instance;
            if (squadManager == null) return;

            contadorTexto.text = $"Cuadrillas: {squadManager.CuadrillasActivasCount}";
        }

        // ── Acciones ─────────────────────────────────────────────────────

        private void CancelarCuadrilla(int squadId)
        {
            var squadManager = Squads.SquadManager.Instance;
            if (squadManager == null) return;

            var squad = squadManager.GetCuadrillaPorId(squadId);
            if (squad != null)
                squadManager.CancelarExpedicion(squad);
        }
    }
}
