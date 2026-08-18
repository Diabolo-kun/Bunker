// ============================================================================
// ResourcePanelUI.cs — Panel de UI que muestra todos los recursos del búnker
// GDD Sección 6: Oficina del Supervisor — Flujo de recursos en tiempo real
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Bunker.UI.HUD
{
    /// <summary>
    /// Panel de UI que muestra los recursos del búnker en tiempo real.
    /// Se actualiza automáticamente cuando cambian los recursos.
    /// </summary>
    public class ResourcePanelUI : MonoBehaviour
    {
        [Header("── Configuración ──")]
        [Tooltip("Prefab de una fila de recurso individual")]
        [SerializeField] private GameObject resourceRowPrefab;

        [Tooltip("Contenedor donde se instancian las filas")]
        [SerializeField] private Transform contenedorFilas;

        [Tooltip("Definiciones de recursos a mostrar (en orden)")]
        [SerializeField] private List<Core.Data.ResourceDataSO> recursosAMostrar;

        // Cache de filas creadas
        private Dictionary<Core.Data.ResourceType, ResourceRowUI> filasCreadas = new Dictionary<Core.Data.ResourceType, ResourceRowUI>();

        private void Start()
        {
            CrearFilas();
            SuscribirEventos();
            ActualizarTodo();
        }

        private void OnDestroy()
        {
            DesuscribirEventos();
        }

        private void CrearFilas()
        {
            if (resourceRowPrefab == null || contenedorFilas == null) return;

            foreach (var def in recursosAMostrar)
            {
                if (def == null || !def.esAlmacenable) continue;

                var fila = Instantiate(resourceRowPrefab, contenedorFilas);
                var rowUI = fila.GetComponent<ResourceRowUI>();

                if (rowUI != null)
                {
                    rowUI.Inicializar(def);
                    filasCreadas[def.tipo] = rowUI;
                }
            }
        }

        private void SuscribirEventos()
        {
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm != null)
            {
                rm.OnResourceChanged += OnRecursoChanged;
            }
        }

        private void DesuscribirEventos()
        {
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm != null)
            {
                rm.OnResourceChanged -= OnRecursoChanged;
            }
        }

        private void OnRecursoChanged(Core.Data.ResourceType tipo, float anterior, float nuevo)
        {
            if (filasCreadas.TryGetValue(tipo, out var fila))
            {
                var rm = Core.Managers.ResourceManager.Instance;
                fila.Actualizar(nuevo, rm.GetMaxCapacity(tipo));
            }
        }

        private void ActualizarTodo()
        {
            var rm = Core.Managers.ResourceManager.Instance;
            if (rm == null) return;

            foreach (var kvp in filasCreadas)
            {
                kvp.Value.Actualizar(rm.GetAmount(kvp.Key), rm.GetMaxCapacity(kvp.Key));
            }
        }
    }
}
