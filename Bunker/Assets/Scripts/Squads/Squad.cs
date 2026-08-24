// ============================================================================
// Squad.cs — Cuadrilla de exploración y recolección
// GDD Sección 7: Logística, Expediciones y Puestos de Control
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bunker.Squads
{
    /// <summary>
    /// Representa una cuadrilla de 5 personas enviada a recolectar
    /// recursos de un nodo natural. Gestiona las fases de viaje,
    /// recolección y regreso, avanzando con cada tick del reloj.
    /// </summary>
    public class Squad
    {
        // ── Enums ────────────────────────────────────────────────────────

        /// <summary>Fases de una expedición.</summary>
        public enum EstadoCuadrilla
        {
            Preparando,
            ViajeIda,
            Recolectando,
            ViajeVuelta,
            EnBase
        }

        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cuando la cuadrilla cambia de estado.</summary>
        public event Action<Squad, EstadoCuadrilla> OnEstadoCambiado;

        /// <summary>Se dispara cuando la cuadrilla regresa a base con recursos.</summary>
        public event Action<Squad> OnRegresado;

        // ── Propiedades ──────────────────────────────────────────────────

        /// <summary>Identificador único de la cuadrilla.</summary>
        public int Id { get; private set; }

        /// <summary>Número de miembros en la cuadrilla.</summary>
        public int Miembros { get; private set; }

        /// <summary>Estado actual de la expedición.</summary>
        public EstadoCuadrilla Estado { get; private set; }

        /// <summary>Nodo destino de la expedición.</summary>
        public Nodes.NaturalNode NodoDestino { get; private set; }

        /// <summary>Distancia en tiles al nodo destino.</summary>
        public float DistanciaTiles { get; private set; }

        /// <summary>Progreso actual de la fase (0-1).</summary>
        public float Progreso { get; private set; }

        /// <summary>Ticks totales necesarios para el viaje de ida.</summary>
        public int TicksViaje { get; private set; }

        /// <summary>Ticks totales de recolección en el nodo.</summary>
        public int TicksRecoleccion { get; private set; }

        /// <summary>Ticks transcurridos en la fase actual.</summary>
        public int TicksEnFase { get; private set; }

        /// <summary>Recursos recolectados durante la expedición.</summary>
        public Dictionary<Core.Data.ResourceType, float> CargaRecolectada { get; private set; }

        // ── Configuración ────────────────────────────────────────────────

        /// <summary>Ticks de viaje por tile de distancia.</summary>
        private const int TICKS_POR_TILE = 2;

        /// <summary>Ticks de recolección base en el nodo.</summary>
        private const int TICKS_RECOLECCION_BASE = 30;

        // ── Constructor ──────────────────────────────────────────────────

        public Squad(int id, int miembros, Nodes.NaturalNode destino, Vector2Int posicionBunker)
        {
            Id = id;
            Miembros = miembros;
            NodoDestino = destino;
            CargaRecolectada = new Dictionary<Core.Data.ResourceType, float>();

            DistanciaTiles = destino.DistanciaATiles(posicionBunker);
            TicksViaje = Mathf.Max(1, Mathf.CeilToInt(DistanciaTiles * TICKS_POR_TILE));
            TicksRecoleccion = TICKS_RECOLECCION_BASE;

            Estado = EstadoCuadrilla.Preparando;
            TicksEnFase = 0;
            Progreso = 0f;
        }

        // ── Ciclo de vida ────────────────────────────────────────────────

        /// <summary>
        /// Inicia la expedición. Cambia el estado a ViajeIda.
        /// </summary>
        public void IniciarExpedicion()
        {
            CambiarEstado(EstadoCuadrilla.ViajeIda);
        }

        /// <summary>
        /// Avanza un tick de simulación. Gestiona el progreso según la fase.
        /// </summary>
        public void ProcesarTick()
        {
            TicksEnFase++;

            switch (Estado)
            {
                case EstadoCuadrilla.ViajeIda:
                    Progreso = (float)TicksEnFase / TicksViaje;
                    if (TicksEnFase >= TicksViaje)
                    {
                        CambiarEstado(EstadoCuadrilla.Recolectando);
                        NodoDestino.MarcarEnExplotacion();
                    }
                    break;

                case EstadoCuadrilla.Recolectando:
                    Progreso = (float)TicksEnFase / TicksRecoleccion;

                    // Recolectar cada tick
                    var recolectado = NodoDestino.RecolectarTick();
                    foreach (var kvp in recolectado)
                    {
                        if (!CargaRecolectada.ContainsKey(kvp.Key))
                            CargaRecolectada[kvp.Key] = 0f;
                        CargaRecolectada[kvp.Key] += kvp.Value;
                    }

                    // ¿Terminar recolección?
                    if (TicksEnFase >= TicksRecoleccion || NodoDestino.EstaAgotado)
                    {
                        CambiarEstado(EstadoCuadrilla.ViajeVuelta);
                    }
                    break;

                case EstadoCuadrilla.ViajeVuelta:
                    Progreso = (float)TicksEnFase / TicksViaje;
                    if (TicksEnFase >= TicksViaje)
                    {
                        CambiarEstado(EstadoCuadrilla.EnBase);
                        OnRegresado?.Invoke(this);
                    }
                    break;
            }
        }

        /// <summary>
        /// Cancela la expedición. La cuadrilla vuelve inmediatamente
        /// (pierde los recursos no entregados si estaba recolectando).
        /// </summary>
        public void Cancelar()
        {
            CargaRecolectada.Clear();
            CambiarEstado(EstadoCuadrilla.EnBase);
        }

        // ── Utilidades ───────────────────────────────────────────────────

        private void CambiarEstado(EstadoCuadrilla nuevoEstado)
        {
            Estado = nuevoEstado;
            TicksEnFase = 0;
            Progreso = 0f;
            OnEstadoCambiado?.Invoke(this, nuevoEstado);
        }

        /// <summary>Devuelve descripción de estado para la UI.</summary>
        public string GetEstadoFormateado()
        {
            switch (Estado)
            {
                case EstadoCuadrilla.Preparando:
                    return "Preparando expedición...";
                case EstadoCuadrilla.ViajeIda:
                    return $"En camino → {NodoDestino.Plantilla.nombreMostrado} ({Progreso:P0})";
                case EstadoCuadrilla.Recolectando:
                    return $"Recolectando en {NodoDestino.Plantilla.nombreMostrado} ({Progreso:P0})";
                case EstadoCuadrilla.ViajeVuelta:
                    return $"Regresando al búnker ({Progreso:P0})";
                case EstadoCuadrilla.EnBase:
                    return "En base";
                default:
                    return "Desconocido";
            }
        }
    }
}
