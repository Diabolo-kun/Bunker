// ============================================================================
// SimulationClock.cs — Reloj de simulación con tick configurable
// GDD Sección 9C: SimulationClock — Tick temporal para consumos, crecimiento, viajes
// ============================================================================
using System;
using UnityEngine;

namespace Bunker.Core.Managers
{
    /// <summary>
    /// Controla el flujo del tiempo en la simulación.
    /// Emite un evento OnTick cada intervalo configurable.
    /// Soporta pausa y múltiples velocidades (x1, x2, x4).
    /// </summary>
    public class SimulationClock : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static SimulationClock Instance { get; private set; }

        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cada tick de simulación.</summary>
        public event Action OnTick;

        /// <summary>Se dispara cada hora de juego (cada horasEnTicks ticks).</summary>
        public event Action OnHourPassed;

        /// <summary>Se dispara cada día de juego (cada 24 horas).</summary>
        public event Action OnDayPassed;

        /// <summary>Se dispara cuando cambia el estado de pausa o la velocidad.</summary>
        public event Action<bool, float> OnClockStateChanged;
        // Parámetros: estaPausado, multiplicadorActual

        // ── Configuración ────────────────────────────────────────────────
        [Header("── Configuración del Tick ──")]
        [Tooltip("Segundos reales entre cada tick de simulación (a velocidad x1)")]
        [SerializeField] private float segundosPorTick = 1f;

        [Tooltip("Cuántos ticks equivalen a 1 hora de juego")]
        [SerializeField] private int ticksPorHora = 60;

        [Header("── Estado Inicial ──")]
        [Tooltip("¿Empieza el juego pausado?")]
        [SerializeField] private bool empezarPausado = false;

        // ── Estado Interno ───────────────────────────────────────────────
        private float acumuladorTiempo;
        private int ticksEnHoraActual;
        private int horasEnDiaActual;

        // ── Propiedades Públicas ─────────────────────────────────────────
        /// <summary>¿Está la simulación pausada?</summary>
        public bool EstaPausado { get; private set; }

        /// <summary>Multiplicador de velocidad actual (1, 2 o 4).</summary>
        public float MultiplicadorVelocidad { get; private set; } = 1f;

        /// <summary>Total de ticks transcurridos desde el inicio.</summary>
        public int TotalTicks { get; private set; }

        /// <summary>Total de horas transcurridas desde el inicio.</summary>
        public int TotalHoras { get; private set; }

        /// <summary>Total de días transcurridos desde el inicio.</summary>
        public int TotalDias { get; private set; }

        /// <summary>Hora actual del día (0-23).</summary>
        public int HoraActual => horasEnDiaActual;

        /// <summary>Día actual (empezando en 1).</summary>
        public int DiaActual => TotalDias + 1;

        // ── Inicialización ───────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EstaPausado = empezarPausado;
        }

        // ── Update Loop ──────────────────────────────────────────────────
        private void Update()
        {
            if (EstaPausado) return;

            acumuladorTiempo += Time.deltaTime * MultiplicadorVelocidad;

            while (acumuladorTiempo >= segundosPorTick)
            {
                acumuladorTiempo -= segundosPorTick;
                EjecutarTick();
            }
        }

        private void EjecutarTick()
        {
            TotalTicks++;
            OnTick?.Invoke();

            // Comprobar si ha pasado una hora
            ticksEnHoraActual++;
            if (ticksEnHoraActual >= ticksPorHora)
            {
                ticksEnHoraActual = 0;
                TotalHoras++;
                horasEnDiaActual++;
                OnHourPassed?.Invoke();

                // Comprobar si ha pasado un día
                if (horasEnDiaActual >= 24)
                {
                    horasEnDiaActual = 0;
                    TotalDias++;
                    OnDayPassed?.Invoke();
                }
            }
        }

        // ── API Pública de Control ───────────────────────────────────────

        /// <summary>Pausa la simulación.</summary>
        public void Pausar()
        {
            EstaPausado = true;
            OnClockStateChanged?.Invoke(EstaPausado, MultiplicadorVelocidad);
        }

        /// <summary>Reanuda la simulación.</summary>
        public void Reanudar()
        {
            EstaPausado = false;
            OnClockStateChanged?.Invoke(EstaPausado, MultiplicadorVelocidad);
        }

        /// <summary>Alterna entre pausado y activo.</summary>
        public void TogglePausa()
        {
            if (EstaPausado) Reanudar();
            else Pausar();
        }

        /// <summary>Establece la velocidad de simulación (x1).</summary>
        public void SetVelocidadNormal()
        {
            MultiplicadorVelocidad = 1f;
            OnClockStateChanged?.Invoke(EstaPausado, MultiplicadorVelocidad);
        }

        /// <summary>Establece la velocidad de simulación (x2).</summary>
        public void SetVelocidadRapida()
        {
            MultiplicadorVelocidad = 2f;
            OnClockStateChanged?.Invoke(EstaPausado, MultiplicadorVelocidad);
        }

        /// <summary>Establece la velocidad de simulación (x4).</summary>
        public void SetVelocidadMuyRapida()
        {
            MultiplicadorVelocidad = 4f;
            OnClockStateChanged?.Invoke(EstaPausado, MultiplicadorVelocidad);
        }

        /// <summary>Devuelve una cadena formateada con el día y hora actuales.</summary>
        public string GetTiempoFormateado()
        {
            return $"Día {DiaActual} — {horasEnDiaActual:D2}:00";
        }
    }
}
