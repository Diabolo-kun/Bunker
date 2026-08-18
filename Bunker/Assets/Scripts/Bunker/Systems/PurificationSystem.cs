// ============================================================================
// PurificationSystem.cs — Sistema de purificación de recursos contaminados
// GDD Sección 6: Cuello de Botella de Purificación
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bunker.Bunker.Systems
{
    /// <summary>
    /// Gestiona la conversión de recursos contaminados en recursos limpios.
    /// Consume personal y energía para funcionar.
    /// </summary>
    public class PurificationSystem : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static PurificationSystem Instance { get; private set; }

        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cuando se completa un ciclo de purificación.</summary>
        public event Action<Core.Data.ResourceType, float> OnPurificationCompleted;
        // Parámetros: recursoLimpioProducido, cantidad

        /// <summary>Se dispara cuando el sistema no puede funcionar (falta personal/energía).</summary>
        public event Action<string> OnPurificationBlocked;

        // ── Configuración ────────────────────────────────────────────────
        [Header("── Líneas de Purificación ──")]
        [Tooltip("Líneas de purificación configuradas")]
        [SerializeField] private List<PurificationLine> lineas = new List<PurificationLine>();

        [Header("── Requisitos ──")]
        [Tooltip("Personal mínimo asignado para que funcione cada línea")]
        [SerializeField] private int personalMinimoPorLinea = 2;

        [Tooltip("Energía consumida por cada ciclo de purificación")]
        [SerializeField] private float energiaPorCiclo = 5f;

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

            // Crear líneas por defecto si no hay configuradas
            if (lineas.Count == 0)
            {
                lineas.Add(new PurificationLine
                {
                    nombre = "Purificadora de Agua",
                    recursoContaminado = Core.Data.ResourceType.AguaContaminada,
                    recursoLimpio = Core.Data.ResourceType.AguaPurificada,
                    cantidadPorCiclo = 10f,
                    ratioConversion = 1.5f, // 1.5 contaminada → 1 limpia
                    ticksPorCiclo = 5,
                    personalAsignado = 0,
                    activa = true
                });

                lineas.Add(new PurificationLine
                {
                    nombre = "Descontaminadora de Alimentos",
                    recursoContaminado = Core.Data.ResourceType.ComidaContaminada,
                    recursoLimpio = Core.Data.ResourceType.ComidaLimpia,
                    cantidadPorCiclo = 8f,
                    ratioConversion = 2f, // 2 contaminada → 1 limpia
                    ticksPorCiclo = 8,
                    personalAsignado = 0,
                    activa = true
                });
            }
        }

        private void OnEnable()
        {
            if (Core.Managers.SimulationClock.Instance != null)
            {
                Core.Managers.SimulationClock.Instance.OnTick += ProcesarTick;
            }
        }

        private void OnDisable()
        {
            if (Core.Managers.SimulationClock.Instance != null)
            {
                Core.Managers.SimulationClock.Instance.OnTick -= ProcesarTick;
            }
        }

        private void Start()
        {
            if (Core.Managers.SimulationClock.Instance != null)
            {
                Core.Managers.SimulationClock.Instance.OnTick -= ProcesarTick;
                Core.Managers.SimulationClock.Instance.OnTick += ProcesarTick;
            }
        }

        // ── Lógica por Tick ──────────────────────────────────────────────
        private void ProcesarTick()
        {
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm == null) return;

            foreach (var linea in lineas)
            {
                if (!linea.activa) continue;

                // Comprobar personal
                if (linea.personalAsignado < personalMinimoPorLinea)
                {
                    OnPurificationBlocked?.Invoke($"{linea.nombre}: personal insuficiente ({linea.personalAsignado}/{personalMinimoPorLinea})");
                    continue;
                }

                // Avanzar progreso
                linea.ticksAcumulados++;

                if (linea.ticksAcumulados >= linea.ticksPorCiclo)
                {
                    linea.ticksAcumulados = 0;

                    // Comprobar energía
                    if (!rm.HasEnough(Core.Data.ResourceType.EnergiaElectrica, energiaPorCiclo))
                    {
                        OnPurificationBlocked?.Invoke($"{linea.nombre}: energía insuficiente");
                        continue;
                    }

                    // Comprobar materia prima contaminada
                    float contaminadaNecesaria = linea.cantidadPorCiclo * linea.ratioConversion;
                    if (!rm.HasEnough(linea.recursoContaminado, contaminadaNecesaria))
                    {
                        // Intentar con lo que haya
                        float disponible = rm.GetAmount(linea.recursoContaminado);
                        if (disponible <= 0) continue;

                        float produccionParcial = disponible / linea.ratioConversion;
                        rm.RemoveResource(linea.recursoContaminado, disponible);
                        rm.RemoveResource(Core.Data.ResourceType.EnergiaElectrica, energiaPorCiclo);
                        rm.AddResource(linea.recursoLimpio, produccionParcial);
                        OnPurificationCompleted?.Invoke(linea.recursoLimpio, produccionParcial);
                        continue;
                    }

                    // Ciclo completo
                    rm.RemoveResource(linea.recursoContaminado, contaminadaNecesaria);
                    rm.RemoveResource(Core.Data.ResourceType.EnergiaElectrica, energiaPorCiclo);
                    rm.AddResource(linea.recursoLimpio, linea.cantidadPorCiclo);
                    OnPurificationCompleted?.Invoke(linea.recursoLimpio, linea.cantidadPorCiclo);
                }
            }
        }

        // ── API Pública ──────────────────────────────────────────────────

        /// <summary>Asigna personal a una línea de purificación.</summary>
        public void AsignarPersonal(int indiceLinea, int cantidad)
        {
            if (indiceLinea < 0 || indiceLinea >= lineas.Count) return;
            lineas[indiceLinea].personalAsignado = Mathf.Max(0, cantidad);
        }

        /// <summary>Activa o desactiva una línea de purificación.</summary>
        public void SetLineaActiva(int indiceLinea, bool activa)
        {
            if (indiceLinea < 0 || indiceLinea >= lineas.Count) return;
            lineas[indiceLinea].activa = activa;
        }

        /// <summary>Devuelve el número de líneas de purificación.</summary>
        public int GetNumeroLineas() => lineas.Count;

        /// <summary>Devuelve la información de una línea.</summary>
        public PurificationLine GetLinea(int indice)
        {
            if (indice < 0 || indice >= lineas.Count) return null;
            return lineas[indice];
        }

        /// <summary>Devuelve el progreso actual (0-1) de una línea.</summary>
        public float GetProgreso(int indiceLinea)
        {
            if (indiceLinea < 0 || indiceLinea >= lineas.Count) return 0f;
            var linea = lineas[indiceLinea];
            if (linea.ticksPorCiclo <= 0) return 0f;
            return (float)linea.ticksAcumulados / linea.ticksPorCiclo;
        }

        /// <summary>Devuelve el personal mínimo necesario por línea.</summary>
        public int GetPersonalMinimo() => personalMinimoPorLinea;
    }

    // ── Clase de datos de una línea de purificación ──────────────────────
    /// <summary>
    /// Representa una línea individual de purificación/descontaminación.
    /// </summary>
    [Serializable]
    public class PurificationLine
    {
        public string nombre;
        public Core.Data.ResourceType recursoContaminado;
        public Core.Data.ResourceType recursoLimpio;

        [Tooltip("Cantidad de recurso limpio producida por ciclo completo")]
        public float cantidadPorCiclo;

        [Tooltip("Cuántas unidades contaminadas se necesitan por cada unidad limpia")]
        public float ratioConversion;

        [Tooltip("Ticks necesarios para completar un ciclo")]
        public int ticksPorCiclo;

        [Tooltip("Personal actualmente asignado a esta línea")]
        public int personalAsignado;

        [Tooltip("¿Está activa esta línea?")]
        public bool activa;

        // Estado interno (no serializado en inspector)
        [HideInInspector] public int ticksAcumulados;
    }
}
