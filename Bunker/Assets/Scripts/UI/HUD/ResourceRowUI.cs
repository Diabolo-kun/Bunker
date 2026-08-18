// ============================================================================
// ResourceRowUI.cs — Fila individual de un recurso en el panel de recursos
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bunker.UI.HUD
{
    /// <summary>
    /// Muestra una fila individual de recurso: icono, nombre, barra y valor numérico.
    /// </summary>
    public class ResourceRowUI : MonoBehaviour
    {
        [Header("── Referencias UI ──")]
        [SerializeField] private Image iconoImagen;
        [SerializeField] private TextMeshProUGUI nombreTexto;
        [SerializeField] private TextMeshProUGUI valorTexto;
        [SerializeField] private Image barraRelleno;
        [SerializeField] private Image fondoBarra;

        // Datos internos
        private Core.Data.ResourceDataSO definicion;

        /// <summary>Configura la fila con la definición de un recurso.</summary>
        public void Inicializar(Core.Data.ResourceDataSO def)
        {
            definicion = def;

            if (nombreTexto != null)
                nombreTexto.text = def.nombreMostrado;

            if (iconoImagen != null && def.icono != null)
                iconoImagen.sprite = def.icono;

            if (barraRelleno != null)
                barraRelleno.color = def.colorUI;

            Actualizar(0, def.capacidadMaximaPorDefecto);
        }

        /// <summary>Actualiza el valor mostrado y la barra de progreso.</summary>
        public void Actualizar(float cantidadActual, float capacidadMaxima)
        {
            if (valorTexto != null)
                valorTexto.text = $"{Mathf.FloorToInt(cantidadActual)} / {Mathf.FloorToInt(capacidadMaxima)}";

            if (barraRelleno != null && capacidadMaxima > 0)
                barraRelleno.fillAmount = cantidadActual / capacidadMaxima;
        }
    }
}
