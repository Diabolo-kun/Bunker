// ============================================================================
// NaturalNode.cs — Nodo natural con recursos finitos y agotables
// GDD Sección 4: Nodos Naturales (Generación Procedural)
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bunker.Nodes
{
    /// <summary>
    /// Nodo generado proceduralmente a partir del terreno.
    /// Contiene recursos finitos que se agotan con la recolección.
    /// Soporta nodos híbridos (múltiples tipos de recurso).
    /// </summary>
    public class NaturalNode : MapNode
    {
        // ── Eventos ──────────────────────────────────────────────────────
        /// <summary>Se dispara cuando un recurso del nodo se agota completamente.</summary>
        public event Action<NaturalNode, Core.Data.ResourceType> OnRecursoAgotado;

        /// <summary>Se dispara cuando TODOS los recursos del nodo se agotan.</summary>
        public event Action<NaturalNode> OnNodoAgotado;

        // ── Estado interno ───────────────────────────────────────────────

        /// <summary>Recursos restantes en este nodo.</summary>
        private Dictionary<Core.Data.ResourceType, float> recursosRestantes
            = new Dictionary<Core.Data.ResourceType, float>();

        /// <summary>Recursos máximos originales (para calcular porcentajes).</summary>
        private Dictionary<Core.Data.ResourceType, float> recursosMaximos
            = new Dictionary<Core.Data.ResourceType, float>();

        // ── Propiedades ──────────────────────────────────────────────────

        /// <summary>¿Está completamente agotado?</summary>
        public bool EstaAgotado => Estado == Core.Data.NodeState.Agotado;

        /// <summary>Tasa de recolección por tick de trabajo.</summary>
        public float TasaRecoleccion => Plantilla != null ? Plantilla.tasaRecoleccionPorTick : 0f;

        /// <summary>Devuelve los tipos de recurso disponibles en este nodo.</summary>
        public IReadOnlyDictionary<Core.Data.ResourceType, float> RecursosRestantes => recursosRestantes;

        // ── Configuración ────────────────────────────────────────────────

        /// <summary>
        /// Inicializa el nodo natural con recursos basados en su plantilla.
        /// El multiplicador permite variación procedural.
        /// </summary>
        public override void Configurar(Vector2Int pos, Core.Data.NodeDataSO datos)
        {
            base.Configurar(pos, datos);
            InicializarRecursos(1f);
        }

        /// <summary>
        /// Inicializa con un multiplicador de variación procedural.
        /// </summary>
        public void ConfigurarConVariacion(Vector2Int pos, Core.Data.NodeDataSO datos, float multiplicador)
        {
            base.Configurar(pos, datos);
            InicializarRecursos(multiplicador);
        }

        /// <summary>Marca el nodo como en explotación activa por una cuadrilla.</summary>
        public void MarcarEnExplotacion()
        {
            if (Estado == Core.Data.NodeState.Descubierto)
                Estado = Core.Data.NodeState.EnExplotacion;
        }

        /// <summary>Marca el nodo como descubierto (si estaba en explotación y la cuadrilla se fue).</summary>
        public void MarcarLibre()
        {
            if (Estado == Core.Data.NodeState.EnExplotacion)
                Estado = Core.Data.NodeState.Descubierto;
        }

        private void InicializarRecursos(float multiplicador)
        {
            recursosRestantes.Clear();
            recursosMaximos.Clear();

            if (Plantilla == null) return;

            int count = Mathf.Min(
                Plantilla.recursosDisponibles != null ? Plantilla.recursosDisponibles.Length : 0,
                Plantilla.cantidadBase != null ? Plantilla.cantidadBase.Length : 0);

            for (int i = 0; i < count; i++)
            {
                var tipo = Plantilla.recursosDisponibles[i];
                float cantidad = Plantilla.cantidadBase[i] * multiplicador;
                recursosRestantes[tipo] = cantidad;
                recursosMaximos[tipo] = cantidad;
            }
        }

        // ── Recolección ──────────────────────────────────────────────────

        /// <summary>
        /// Recolecta recursos del nodo. Devuelve lo realmente extraído
        /// (puede ser menor si queda poco). Respeta la tasa de recolección.
        /// </summary>
        /// <param name="tipo">Tipo de recurso a recolectar.</param>
        /// <param name="cantidadSolicitada">Cantidad deseada.</param>
        /// <returns>Cantidad realmente extraída.</returns>
        public float Recolectar(Core.Data.ResourceType tipo, float cantidadSolicitada)
        {
            if (EstaAgotado) return 0f;
            if (!recursosRestantes.ContainsKey(tipo)) return 0f;

            float disponible = recursosRestantes[tipo];
            if (disponible <= 0f) return 0f;

            float extraido = Mathf.Min(cantidadSolicitada, disponible);
            recursosRestantes[tipo] = disponible - extraido;

            // Comprobar si este recurso se ha agotado
            if (recursosRestantes[tipo] <= 0f)
            {
                recursosRestantes[tipo] = 0f;
                OnRecursoAgotado?.Invoke(this, tipo);
            }

            // Comprobar si TODOS los recursos se han agotado
            if (TodoAgotado())
            {
                Estado = Core.Data.NodeState.Agotado;
                OnNodoAgotado?.Invoke(this);
            }

            return extraido;
        }

        /// <summary>
        /// Ejecuta un tick completo de recolección, extrayendo de todos
        /// los recursos disponibles según la tasa de recolección.
        /// Devuelve un diccionario con lo recolectado por tipo.
        /// </summary>
        public Dictionary<Core.Data.ResourceType, float> RecolectarTick()
        {
            var recolectado = new Dictionary<Core.Data.ResourceType, float>();

            if (EstaAgotado || Plantilla == null) return recolectado;

            float tasa = Plantilla.tasaRecoleccionPorTick;

            foreach (var tipo in Plantilla.recursosDisponibles)
            {
                float extraido = Recolectar(tipo, tasa);
                if (extraido > 0f)
                    recolectado[tipo] = extraido;
            }

            return recolectado;
        }

        // ── Consultas ────────────────────────────────────────────────────

        /// <summary>Porcentaje total de recursos restantes (0-1).</summary>
        public float GetPorcentajeRestanteTotal()
        {
            float totalMax = 0f;
            float totalActual = 0f;

            foreach (var kvp in recursosMaximos)
            {
                totalMax += kvp.Value;
                if (recursosRestantes.ContainsKey(kvp.Key))
                    totalActual += recursosRestantes[kvp.Key];
            }

            return totalMax > 0f ? totalActual / totalMax : 0f;
        }

        /// <summary>Porcentaje restante de un recurso específico (0-1).</summary>
        public float GetPorcentajeRestante(Core.Data.ResourceType tipo)
        {
            if (!recursosMaximos.ContainsKey(tipo) || recursosMaximos[tipo] <= 0f) return 0f;
            if (!recursosRestantes.ContainsKey(tipo)) return 0f;
            return recursosRestantes[tipo] / recursosMaximos[tipo];
        }

        /// <summary>Cantidad restante de un recurso.</summary>
        public float GetCantidadRestante(Core.Data.ResourceType tipo)
        {
            if (!recursosRestantes.ContainsKey(tipo)) return 0f;
            return recursosRestantes[tipo];
        }

        private bool TodoAgotado()
        {
            foreach (var kvp in recursosRestantes)
            {
                if (kvp.Value > 0f) return false;
            }
            return true;
        }

        // ── Info ─────────────────────────────────────────────────────────

        public override string GetInfoFormateada()
        {
            if (Plantilla == null) return "Nodo desconocido";

            string info = $"{Plantilla.nombreMostrado}\n" +
                          $"Terreno: {Plantilla.tipoTerreno}\n" +
                          $"Estado: {Estado}\n" +
                          $"Recursos restantes: {GetPorcentajeRestanteTotal():P0}\n";

            foreach (var kvp in recursosRestantes)
            {
                if (kvp.Value > 0f)
                    info += $"  • {kvp.Key}: {Mathf.FloorToInt(kvp.Value)}\n";
            }

            return info;
        }
    }
}
