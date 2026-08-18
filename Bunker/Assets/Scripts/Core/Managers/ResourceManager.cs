// ============================================================================
// ResourceManager.cs — Gestor central del inventario de recursos del búnker
// GDD Sección 9C: Gestores Centrales — ResourceManager
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bunker.Core.Managers
{
    /// <summary>
    /// Gestiona el inventario completo del búnker.
    /// Almacena cantidades, controla límites y emite eventos de cambio.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static ResourceManager Instance { get; private set; }

        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cuando la cantidad de un recurso cambia.</summary>
        public event Action<Data.ResourceType, float, float> OnResourceChanged;
        // Parámetros: tipo, cantidadAnterior, cantidadNueva

        /// <summary>Se dispara cuando un recurso llega a 0.</summary>
        public event Action<Data.ResourceType> OnResourceDepleted;

        /// <summary>Se dispara cuando un recurso alcanza su capacidad máxima.</summary>
        public event Action<Data.ResourceType> OnResourceFull;

        // ── Datos ────────────────────────────────────────────────────────
        [Header("── Configuración ──")]
        [Tooltip("Lista de definiciones de recursos del juego")]
        [SerializeField] private List<Data.ResourceDataSO> definicionesRecursos;

        /// <summary>Cantidades actuales de cada recurso.</summary>
        private Dictionary<Data.ResourceType, float> cantidades = new Dictionary<Data.ResourceType, float>();

        /// <summary>Capacidad máxima de cada recurso.</summary>
        private Dictionary<Data.ResourceType, float> capacidadesMaximas = new Dictionary<Data.ResourceType, float>();

        /// <summary>Referencia rápida a las definiciones por tipo.</summary>
        private Dictionary<Data.ResourceType, Data.ResourceDataSO> definicionesPorTipo = new Dictionary<Data.ResourceType, Data.ResourceDataSO>();

        // ── Inicialización ───────────────────────────────────────────────
        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InicializarRecursos();
        }

        private void InicializarRecursos()
        {
            cantidades.Clear();
            capacidadesMaximas.Clear();
            definicionesPorTipo.Clear();

            foreach (var def in definicionesRecursos)
            {
                if (def == null) continue;

                definicionesPorTipo[def.tipo] = def;
                cantidades[def.tipo] = 0f;
                capacidadesMaximas[def.tipo] = def.capacidadMaximaPorDefecto;
            }
        }

        // ── API Pública ──────────────────────────────────────────────────

        /// <summary>
        /// Añade cantidad de un recurso. Respeta el límite máximo.
        /// Devuelve la cantidad realmente añadida (puede ser menor si se llena).
        /// </summary>
        public float AddResource(Data.ResourceType tipo, float cantidad)
        {
            if (cantidad <= 0) return 0f;
            if (!cantidades.ContainsKey(tipo))
            {
                Debug.LogWarning($"[ResourceManager] Recurso no registrado: {tipo}");
                return 0f;
            }

            float anterior = cantidades[tipo];
            float max = capacidadesMaximas[tipo];
            float espacioDisponible = max - anterior;
            float cantidadReal = Mathf.Min(cantidad, espacioDisponible);

            if (cantidadReal <= 0) return 0f;

            cantidades[tipo] = anterior + cantidadReal;
            OnResourceChanged?.Invoke(tipo, anterior, cantidades[tipo]);

            if (cantidades[tipo] >= max)
            {
                OnResourceFull?.Invoke(tipo);
            }

            return cantidadReal;
        }

        /// <summary>
        /// Resta cantidad de un recurso. No permite valores negativos.
        /// Devuelve la cantidad realmente restada.
        /// </summary>
        public float RemoveResource(Data.ResourceType tipo, float cantidad)
        {
            if (cantidad <= 0) return 0f;
            if (!cantidades.ContainsKey(tipo))
            {
                Debug.LogWarning($"[ResourceManager] Recurso no registrado: {tipo}");
                return 0f;
            }

            float anterior = cantidades[tipo];
            float cantidadReal = Mathf.Min(cantidad, anterior);

            if (cantidadReal <= 0) return 0f;

            cantidades[tipo] = anterior - cantidadReal;
            OnResourceChanged?.Invoke(tipo, anterior, cantidades[tipo]);

            if (cantidades[tipo] <= 0)
            {
                OnResourceDepleted?.Invoke(tipo);
            }

            return cantidadReal;
        }

        /// <summary>
        /// Comprueba si hay suficiente cantidad de un recurso.
        /// </summary>
        public bool HasEnough(Data.ResourceType tipo, float cantidadNecesaria)
        {
            if (!cantidades.ContainsKey(tipo)) return false;
            return cantidades[tipo] >= cantidadNecesaria;
        }

        /// <summary>
        /// Devuelve la cantidad actual de un recurso.
        /// </summary>
        public float GetAmount(Data.ResourceType tipo)
        {
            if (!cantidades.ContainsKey(tipo)) return 0f;
            return cantidades[tipo];
        }

        /// <summary>
        /// Devuelve la capacidad máxima de un recurso.
        /// </summary>
        public float GetMaxCapacity(Data.ResourceType tipo)
        {
            if (!capacidadesMaximas.ContainsKey(tipo)) return 0f;
            return capacidadesMaximas[tipo];
        }

        /// <summary>
        /// Devuelve el porcentaje de llenado (0-1) de un recurso.
        /// </summary>
        public float GetFillPercentage(Data.ResourceType tipo)
        {
            float max = GetMaxCapacity(tipo);
            if (max <= 0) return 0f;
            return GetAmount(tipo) / max;
        }

        /// <summary>
        /// Modifica la capacidad máxima de un recurso (para mejoras del búnker).
        /// </summary>
        public void SetMaxCapacity(Data.ResourceType tipo, float nuevaCapacidad)
        {
            if (!capacidadesMaximas.ContainsKey(tipo)) return;
            capacidadesMaximas[tipo] = Mathf.Max(0, nuevaCapacidad);

            // Si la cantidad actual excede la nueva capacidad, recortar
            if (cantidades[tipo] > nuevaCapacidad)
            {
                float anterior = cantidades[tipo];
                cantidades[tipo] = nuevaCapacidad;
                OnResourceChanged?.Invoke(tipo, anterior, cantidades[tipo]);
            }
        }

        /// <summary>
        /// Devuelve la definición SO de un tipo de recurso.
        /// </summary>
        public Data.ResourceDataSO GetDefinicion(Data.ResourceType tipo)
        {
            definicionesPorTipo.TryGetValue(tipo, out var def);
            return def;
        }

        /// <summary>
        /// Devuelve todas las definiciones registradas.
        /// </summary>
        public IReadOnlyList<Data.ResourceDataSO> GetTodasLasDefiniciones()
        {
            return definicionesRecursos.AsReadOnly();
        }

        /// <summary>
        /// Establece directamente la cantidad de un recurso (para carga de partida).
        /// </summary>
        public void SetAmount(Data.ResourceType tipo, float cantidad)
        {
            if (!cantidades.ContainsKey(tipo)) return;
            float anterior = cantidades[tipo];
            cantidades[tipo] = Mathf.Clamp(cantidad, 0, capacidadesMaximas[tipo]);
            OnResourceChanged?.Invoke(tipo, anterior, cantidades[tipo]);
        }
    }
}
