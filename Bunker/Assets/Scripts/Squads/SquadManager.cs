// ============================================================================
// SquadManager.cs — Gestor de cuadrillas de exploración
// GDD Sección 7: Logística, Expediciones y Puestos de Control
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bunker.Squads
{
    /// <summary>
    /// Singleton que gestiona todas las cuadrillas activas.
    /// Coordina el envío, progreso y regreso de expediciones.
    /// Se suscribe al SimulationClock para avanzar cada tick.
    /// </summary>
    public class SquadManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static SquadManager Instance { get; private set; }

        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cuando una cuadrilla parte hacia un nodo.</summary>
        public event Action<Squad> OnSquadDeparted;

        /// <summary>Se dispara cuando una cuadrilla llega al nodo.</summary>
        public event Action<Squad> OnSquadArrived;

        /// <summary>Se dispara cuando una cuadrilla regresa al búnker.</summary>
        public event Action<Squad> OnSquadReturned;

        /// <summary>Se dispara cuando una cuadrilla es cancelada.</summary>
        public event Action<Squad> OnSquadCancelled;

        // ── Configuración ────────────────────────────────────────────────
        [Header("── Cuadrillas ──")]
        [Tooltip("Miembros por defecto en una cuadrilla")]
        [SerializeField] private int miembrosPorDefecto = 5;

        [Tooltip("Máximo de cuadrillas activas simultáneamente")]
        [SerializeField] private int maxCuadrillasActivas = 5;

        // ── Estado ───────────────────────────────────────────────────────
        private List<Squad> cuadrillasActivas = new List<Squad>();
        private int siguienteId = 1;

        // ── Propiedades ──────────────────────────────────────────────────

        /// <summary>Lista de cuadrillas activas (solo lectura).</summary>
        public IReadOnlyList<Squad> CuadrillasActivas => cuadrillasActivas.AsReadOnly();

        /// <summary>Número de cuadrillas activas actualmente.</summary>
        public int CuadrillasActivasCount => cuadrillasActivas.Count;

        /// <summary>¿Se pueden enviar más cuadrillas?</summary>
        public bool PuedeEnviarMas => cuadrillasActivas.Count < maxCuadrillasActivas;

        /// <summary>Miembros por defecto en una cuadrilla.</summary>
        public int MiembrosPorDefecto => miembrosPorDefecto;

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

        private void Start()
        {
            // Suscribirse al reloj de simulación
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock != null)
                clock.OnTick += ProcesarTickCuadrillas;
        }

        private void OnDestroy()
        {
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock != null)
                clock.OnTick -= ProcesarTickCuadrillas;
        }

        // ── API Pública ──────────────────────────────────────────────────

        /// <summary>
        /// Envía una cuadrilla al nodo destino.
        /// Valida disponibilidad de personal y radio operativo.
        /// </summary>
        /// <param name="destino">Nodo al que enviar la cuadrilla.</param>
        /// <param name="miembros">Número de miembros (0 = usar valor por defecto).</param>
        /// <returns>La cuadrilla creada, o null si no se pudo enviar.</returns>
        public Squad EnviarCuadrilla(Nodes.NaturalNode destino, int miembros = 0)
        {
            if (destino == null)
            {
                Debug.LogWarning("[SquadManager] No se puede enviar cuadrilla: destino nulo.");
                return null;
            }

            if (!PuedeEnviarMas)
            {
                Debug.LogWarning("[SquadManager] Máximo de cuadrillas activas alcanzado.");
                return null;
            }

            // Verificar radio operativo
            var mapManager = Map.MapManager.Instance;
            if (mapManager != null && !mapManager.EstaDentroDelRadio(destino.PosicionGrid))
            {
                Debug.LogWarning("[SquadManager] El nodo está fuera del radio operativo.");
                return null;
            }

            // Verificar que el nodo no está agotado
            if (destino.EstaAgotado)
            {
                Debug.LogWarning("[SquadManager] El nodo está agotado.");
                return null;
            }

            // Verificar que el nodo está descubierto
            if (destino.Estado == Core.Data.NodeState.SinExplorar)
            {
                Debug.LogWarning("[SquadManager] El nodo no ha sido descubierto.");
                return null;
            }

            int numMiembros = miembros > 0 ? miembros : miembrosPorDefecto;

            // Verificar mano de obra disponible
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm != null && !rm.HasEnough(Core.Data.ResourceType.ManoDeObra, numMiembros))
            {
                Debug.LogWarning("[SquadManager] No hay suficiente mano de obra.");
                return null;
            }

            // Descontar mano de obra
            if (rm != null)
                rm.RemoveResource(Core.Data.ResourceType.ManoDeObra, numMiembros);

            // Crear y enviar cuadrilla
            Vector2Int posBunker = mapManager != null ? mapManager.PosicionBunker : Vector2Int.zero;
            var squad = new Squad(siguienteId++, numMiembros, destino, posBunker);

            squad.OnRegresado += OnCuadrillaRegresada;
            squad.IniciarExpedicion();

            cuadrillasActivas.Add(squad);

            Debug.Log($"[SquadManager] Cuadrilla #{squad.Id} enviada a {destino.Plantilla.nombreMostrado} " +
                      $"({numMiembros} miembros, {squad.TicksViaje} ticks de viaje)");

            OnSquadDeparted?.Invoke(squad);
            return squad;
        }

        /// <summary>Cancela una expedición activa.</summary>
        public void CancelarExpedicion(Squad squad)
        {
            if (squad == null || !cuadrillasActivas.Contains(squad)) return;

            squad.Cancelar();
            FinalizarCuadrilla(squad, cancelada: true);
        }

        /// <summary>Obtiene una cuadrilla por su ID.</summary>
        public Squad GetCuadrillaPorId(int id)
        {
            return cuadrillasActivas.Find(s => s.Id == id);
        }

        // ── Procesamiento de ticks ───────────────────────────────────────

        private void ProcesarTickCuadrillas()
        {
            // Iterar en copia para evitar modificar la lista durante iteración
            var copia = new List<Squad>(cuadrillasActivas);

            foreach (var squad in copia)
            {
                squad.ProcesarTick();
            }
        }

        // ── Handlers ─────────────────────────────────────────────────────

        private void OnCuadrillaRegresada(Squad squad)
        {
            // Transferir recursos recolectados al inventario
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm != null)
            {
                foreach (var kvp in squad.CargaRecolectada)
                {
                    float añadido = rm.AddResource(kvp.Key, kvp.Value);
                    Debug.Log($"[SquadManager] Cuadrilla #{squad.Id} entregó {añadido:F0} {kvp.Key}");
                }
            }

            FinalizarCuadrilla(squad, cancelada: false);
        }

        private void FinalizarCuadrilla(Squad squad, bool cancelada)
        {
            // Devolver mano de obra
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm != null)
                rm.AddResource(Core.Data.ResourceType.ManoDeObra, squad.Miembros);

            squad.OnRegresado -= OnCuadrillaRegresada;
            cuadrillasActivas.Remove(squad);

            if (cancelada)
            {
                Debug.Log($"[SquadManager] Cuadrilla #{squad.Id} cancelada.");
                OnSquadCancelled?.Invoke(squad);
            }
            else
            {
                Debug.Log($"[SquadManager] Cuadrilla #{squad.Id} regresó al búnker.");
                OnSquadReturned?.Invoke(squad);
            }
        }
    }
}
