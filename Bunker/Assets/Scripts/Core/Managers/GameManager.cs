// ============================================================================
// GameManager.cs — Orquestador central del juego
// Inicializa todos los sistemas y proporciona recursos iniciales de prueba
// ============================================================================
using UnityEngine;

namespace Bunker.Core.Managers
{
    /// <summary>
    /// Punto de entrada principal del juego.
    /// Se encarga de la inicialización ordenada de todos los sistemas
    /// y de proporcionar recursos iniciales para testeo.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Estado ───────────────────────────────────────────────────────
        public enum EstadoJuego { Inicializando, Jugando, Pausado, GameOver }
        public EstadoJuego Estado { get; private set; } = EstadoJuego.Inicializando;

        [Header("── Recursos Iniciales (Debug/Prototipo) ──")]
        [SerializeField] private float maderaInicial = 100f;
        [SerializeField] private float chatarraInicial = 50f;
        [SerializeField] private float mineralesInicial = 30f;
        [SerializeField] private float combustibleInicial = 40f;
        [SerializeField] private float aguaContaminadaInicial = 200f;
        [SerializeField] private float aguaPurificadaInicial = 80f;
        [SerializeField] private float comidaContaminadaInicial = 150f;
        [SerializeField] private float comidaLimpiaInicial = 60f;
        [SerializeField] private float componentesInicial = 10f;
        [SerializeField] private float energiaInicial = 100f;
        [SerializeField] private float medicinasInicial = 20f;

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
        }

        private void Start()
        {
            InicializarRecursosIniciales();
            InicializarMapa();
            Estado = EstadoJuego.Jugando;
            Debug.Log("[GameManager] Juego inicializado correctamente.");
        }

        private void InicializarMapa()
        {
            var mapManager = Map.MapManager.Instance;
            if (mapManager != null)
            {
                mapManager.GenerarMapa();
                Debug.Log("[GameManager] Mapa generado.");
            }
            else
            {
                Debug.LogWarning("[GameManager] MapManager no encontrado. El mapa no se generará.");
            }
        }

        private void InicializarRecursosIniciales()
        {
            var rm = ResourceManager.Instance;
            if (rm == null)
            {
                Debug.LogError("[GameManager] ResourceManager no encontrado.");
                return;
            }

            // Recursos básicos
            rm.AddResource(Data.ResourceType.Madera, maderaInicial);
            rm.AddResource(Data.ResourceType.Chatarra, chatarraInicial);
            rm.AddResource(Data.ResourceType.Minerales, mineralesInicial);
            rm.AddResource(Data.ResourceType.Combustible, combustibleInicial);

            // Recursos vitales
            rm.AddResource(Data.ResourceType.AguaContaminada, aguaContaminadaInicial);
            rm.AddResource(Data.ResourceType.AguaPurificada, aguaPurificadaInicial);
            rm.AddResource(Data.ResourceType.ComidaContaminada, comidaContaminadaInicial);
            rm.AddResource(Data.ResourceType.ComidaLimpia, comidaLimpiaInicial);

            // Recursos tecnológicos
            rm.AddResource(Data.ResourceType.ComponentesElectronicos, componentesInicial);
            rm.AddResource(Data.ResourceType.EnergiaElectrica, energiaInicial);
            rm.AddResource(Data.ResourceType.Medicinas, medicinasInicial);

            // Recursos sociales (se establecen directamente)
            rm.AddResource(Data.ResourceType.Moral, 75f);
            rm.AddResource(Data.ResourceType.Influencia, 10f);

            Debug.Log("[GameManager] Recursos iniciales cargados.");
        }

        // ── API Pública ──────────────────────────────────────────────────

        /// <summary>Pausa el juego completamente.</summary>
        public void PausarJuego()
        {
            Estado = EstadoJuego.Pausado;
            SimulationClock.Instance?.Pausar();
        }

        /// <summary>Reanuda el juego.</summary>
        public void ReanudarJuego()
        {
            Estado = EstadoJuego.Jugando;
            SimulationClock.Instance?.Reanudar();
        }
    }
}
