// ============================================================================
// PopulationManager.cs — Gestión de la población del búnker
// GDD Sección 6: Gestión Interna del Búnker — Asignación de Roles y Consumo
// ============================================================================
using System;
using UnityEngine;

namespace Bunker.Bunker.Population
{
    /// <summary>
    /// Gestiona la población total del búnker, el consumo de recursos vitales,
    /// la moral y las consecuencias por falta de suministros.
    /// </summary>
    public class PopulationManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static PopulationManager Instance { get; private set; }

        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cuando la población cambia (nacimiento, muerte, reclutamiento).</summary>
        public event Action<int, int> OnPopulationChanged;
        // Parámetros: poblaciónAnterior, poblaciónNueva

        /// <summary>Se dispara cuando hay muertes por falta de recursos.</summary>
        public event Action<int, string> OnDeaths;
        // Parámetros: cantidad, causa

        /// <summary>Se dispara cuando la moral cambia.</summary>
        public event Action<float, float> OnMoralChanged;
        // Parámetros: moralAnterior, moralNueva

        // ── Configuración ────────────────────────────────────────────────
        [Header("── Población ──")]
        [Tooltip("Población inicial del búnker")]
        [SerializeField] private int poblacionInicial = 20;

        [Tooltip("Población máxima del búnker (sin ampliaciones)")]
        [SerializeField] private int poblacionMaxima = 100;

        [Header("── Consumo por Tick ──")]
        [Tooltip("Agua purificada consumida por persona por hora de juego")]
        [SerializeField] private float aguaPorPersonaPorHora = 0.5f;

        [Tooltip("Comida limpia consumida por persona por hora de juego")]
        [SerializeField] private float comidaPorPersonaPorHora = 0.3f;

        [Header("── Consecuencias ──")]
        [Tooltip("Moral que se pierde por hora sin agua")]
        [SerializeField] private float moralPerdidaSinAgua = 5f;

        [Tooltip("Moral que se pierde por hora sin comida")]
        [SerializeField] private float moralPerdidaSinComida = 3f;

        [Tooltip("Horas sin agua antes de que empiecen las muertes")]
        [SerializeField] private int horasSinAguaParaMuertes = 6;

        [Tooltip("Horas sin comida antes de que empiecen las muertes")]
        [SerializeField] private int horasSinComidaParaMuertes = 12;

        [Tooltip("Porcentaje de la población que muere por hora sin recurso vital")]
        [Range(0.01f, 0.2f)]
        [SerializeField] private float porcentajeMuertesPorHora = 0.05f;

        [Header("── Moral ──")]
        [Tooltip("Moral inicial")]
        [SerializeField] private float moralInicial = 75f;

        [Tooltip("Moral máxima")]
        [SerializeField] private float moralMaxima = 100f;

        [Tooltip("Moral que se recupera por hora cuando todo va bien")]
        [SerializeField] private float recuperacionMoralPorHora = 1f;

        // ── Estado Interno ───────────────────────────────────────────────
        private int horasSinAgua;
        private int horasSinComida;

        // ── Propiedades Públicas ─────────────────────────────────────────
        public int PoblacionActual { get; private set; }
        public int PoblacionMaxima => poblacionMaxima;
        public float MoralActual { get; private set; }
        public float MoralMaxima => moralMaxima;
        public int HorasSinAgua => horasSinAgua;
        public int HorasSinComida => horasSinComida;

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

            PoblacionActual = poblacionInicial;
            MoralActual = moralInicial;
        }

        private void OnEnable()
        {
            // Suscribirse al reloj de simulación
            if (Core.Managers.SimulationClock.Instance != null)
            {
                Core.Managers.SimulationClock.Instance.OnHourPassed += ProcesarConsumoHora;
            }
        }

        private void OnDisable()
        {
            if (Core.Managers.SimulationClock.Instance != null)
            {
                Core.Managers.SimulationClock.Instance.OnHourPassed -= ProcesarConsumoHora;
            }
        }

        private void Start()
        {
            // Reintentar suscripción en Start por si SimulationClock se crea después
            if (Core.Managers.SimulationClock.Instance != null)
            {
                Core.Managers.SimulationClock.Instance.OnHourPassed -= ProcesarConsumoHora;
                Core.Managers.SimulationClock.Instance.OnHourPassed += ProcesarConsumoHora;
            }
        }

        // ── Lógica de Consumo por Hora ───────────────────────────────────
        private void ProcesarConsumoHora()
        {
            if (PoblacionActual <= 0) return;

            var rm = Core.Managers.ResourceManager.Instance;
            if (rm == null) return;

            ProcesarConsumoAgua(rm);
            ProcesarConsumoComida(rm);
            ProcesarRecuperacionMoral();
        }

        private void ProcesarConsumoAgua(Core.Managers.ResourceManager rm)
        {
            float aguaNecesaria = PoblacionActual * aguaPorPersonaPorHora;
            float aguaConsumida = rm.RemoveResource(Core.Data.ResourceType.AguaPurificada, aguaNecesaria);

            if (aguaConsumida < aguaNecesaria)
            {
                // No hay suficiente agua
                horasSinAgua++;
                CambiarMoral(-moralPerdidaSinAgua);

                if (horasSinAgua >= horasSinAguaParaMuertes)
                {
                    int muertes = Mathf.Max(1, Mathf.FloorToInt(PoblacionActual * porcentajeMuertesPorHora));
                    MatarPoblacion(muertes, "deshidratación");
                }
            }
            else
            {
                horasSinAgua = 0;
            }
        }

        private void ProcesarConsumoComida(Core.Managers.ResourceManager rm)
        {
            float comidaNecesaria = PoblacionActual * comidaPorPersonaPorHora;
            float comidaConsumida = rm.RemoveResource(Core.Data.ResourceType.ComidaLimpia, comidaNecesaria);

            if (comidaConsumida < comidaNecesaria)
            {
                // No hay suficiente comida
                horasSinComida++;
                CambiarMoral(-moralPerdidaSinComida);

                if (horasSinComida >= horasSinComidaParaMuertes)
                {
                    int muertes = Mathf.Max(1, Mathf.FloorToInt(PoblacionActual * porcentajeMuertesPorHora));
                    MatarPoblacion(muertes, "inanición");
                }
            }
            else
            {
                horasSinComida = 0;
            }
        }

        private void ProcesarRecuperacionMoral()
        {
            // Si todo va bien (no hay carencias), la moral se recupera lentamente
            if (horasSinAgua == 0 && horasSinComida == 0)
            {
                CambiarMoral(recuperacionMoralPorHora);
            }
        }

        // ── API Pública ──────────────────────────────────────────────────

        /// <summary>Añade población (reclutamiento, refugiados).</summary>
        public void AddPoblacion(int cantidad)
        {
            if (cantidad <= 0) return;
            int anterior = PoblacionActual;
            PoblacionActual = Mathf.Min(PoblacionActual + cantidad, poblacionMaxima);
            OnPopulationChanged?.Invoke(anterior, PoblacionActual);
        }

        /// <summary>Elimina población (muerte, expulsión).</summary>
        public void MatarPoblacion(int cantidad, string causa)
        {
            if (cantidad <= 0 || PoblacionActual <= 0) return;
            int anterior = PoblacionActual;
            int muertesReales = Mathf.Min(cantidad, PoblacionActual);
            PoblacionActual -= muertesReales;
            OnPopulationChanged?.Invoke(anterior, PoblacionActual);
            OnDeaths?.Invoke(muertesReales, causa);

            // Las muertes afectan la moral
            CambiarMoral(-muertesReales * 2f);
        }

        /// <summary>Modifica la moral del búnker.</summary>
        public void CambiarMoral(float cantidad)
        {
            float anterior = MoralActual;
            MoralActual = Mathf.Clamp(MoralActual + cantidad, 0f, moralMaxima);

            if (Mathf.Abs(anterior - MoralActual) > 0.01f)
            {
                OnMoralChanged?.Invoke(anterior, MoralActual);
            }
        }

        /// <summary>Amplía la capacidad máxima de población.</summary>
        public void AmpliaPoblacionMaxima(int incremento)
        {
            poblacionMaxima += incremento;
        }
    }
}
