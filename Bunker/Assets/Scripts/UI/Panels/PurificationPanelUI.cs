// ============================================================================
// PurificationPanelUI.cs — Panel de UI del sistema de purificación
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bunker.Bunker.Systems;

namespace Bunker.UI.Panels
{
    /// <summary>
    /// Muestra el estado de las líneas de purificación:
    /// progreso, personal asignado y producción.
    /// </summary>
    public class PurificationPanelUI : MonoBehaviour
    {
        [Header("── Línea de Agua ──")]
        [SerializeField] private TextMeshProUGUI aguaNombreTexto;
        [SerializeField] private Image aguaBarraProgreso;
        [SerializeField] private TextMeshProUGUI aguaPersonalTexto;
        [SerializeField] private TextMeshProUGUI aguaEstadoTexto;
        [SerializeField] private Button aguaMasPersonal;
        [SerializeField] private Button aguaMenosPersonal;

        [Header("── Línea de Comida ──")]
        [SerializeField] private TextMeshProUGUI comidaNombreTexto;
        [SerializeField] private Image comidaBarraProgreso;
        [SerializeField] private TextMeshProUGUI comidaPersonalTexto;
        [SerializeField] private TextMeshProUGUI comidaEstadoTexto;
        [SerializeField] private Button comidaMasPersonal;
        [SerializeField] private Button comidaMenosPersonal;

        [Header("── Alertas ──")]
        [SerializeField] private TextMeshProUGUI alertaTexto;

        private void Start()
        {
            // Botones de personal
            if (aguaMasPersonal != null) aguaMasPersonal.onClick.AddListener(() => CambiarPersonal(0, 1));
            if (aguaMenosPersonal != null) aguaMenosPersonal.onClick.AddListener(() => CambiarPersonal(0, -1));
            if (comidaMasPersonal != null) comidaMasPersonal.onClick.AddListener(() => CambiarPersonal(1, 1));
            if (comidaMenosPersonal != null) comidaMenosPersonal.onClick.AddListener(() => CambiarPersonal(1, -1));

            // Suscribir eventos
            var ps = PurificationSystem.Instance;
            if (ps != null)
            {
                ps.OnPurificationCompleted += OnPurificacionCompletada;
                ps.OnPurificationBlocked += OnPurificacionBloqueada;
            }
        }

        private void OnDestroy()
        {
            var ps = PurificationSystem.Instance;
            if (ps != null)
            {
                ps.OnPurificationCompleted -= OnPurificacionCompletada;
                ps.OnPurificationBlocked -= OnPurificacionBloqueada;
            }
        }

        private void Update()
        {
            ActualizarLinea(0, aguaBarraProgreso, aguaPersonalTexto, aguaEstadoTexto, aguaNombreTexto);
            ActualizarLinea(1, comidaBarraProgreso, comidaPersonalTexto, comidaEstadoTexto, comidaNombreTexto);
        }

        private void ActualizarLinea(int indice, Image barra, TextMeshProUGUI personalTxt,
                                      TextMeshProUGUI estadoTxt, TextMeshProUGUI nombreTxt)
        {
            var ps = PurificationSystem.Instance;
            if (ps == null) return;

            var linea = ps.GetLinea(indice);
            if (linea == null) return;

            if (nombreTxt != null)
                nombreTxt.text = linea.nombre;

            if (barra != null)
                barra.fillAmount = ps.GetProgreso(indice);

            if (personalTxt != null)
                personalTxt.text = $"Personal: {linea.personalAsignado}/{ps.GetPersonalMinimo()}";

            if (estadoTxt != null)
            {
                if (!linea.activa)
                    estadoTxt.text = "DESACTIVADA";
                else if (linea.personalAsignado < ps.GetPersonalMinimo())
                    estadoTxt.text = "<color=#FF6B6B>SIN PERSONAL</color>";
                else
                    estadoTxt.text = "<color=#6BFF6B>OPERATIVA</color>";
            }
        }

        private void CambiarPersonal(int indiceLinea, int cambio)
        {
            var ps = PurificationSystem.Instance;
            if (ps == null) return;

            var linea = ps.GetLinea(indiceLinea);
            if (linea == null) return;

            int nuevoPersonal = Mathf.Max(0, linea.personalAsignado + cambio);
            ps.AsignarPersonal(indiceLinea, nuevoPersonal);
        }

        private void OnPurificacionCompletada(Core.Data.ResourceType tipo, float cantidad)
        {
            // Feedback visual rápido (se podría animar)
            Debug.Log($"[Purificación] Producido {cantidad:F1} de {tipo}");
        }

        private void OnPurificacionBloqueada(string razon)
        {
            if (alertaTexto != null)
            {
                alertaTexto.text = $"⚠ {razon}";
                alertaTexto.gameObject.SetActive(true);
                CancelInvoke(nameof(OcultarAlerta));
                Invoke(nameof(OcultarAlerta), 3f);
            }
        }

        private void OcultarAlerta()
        {
            if (alertaTexto != null)
                alertaTexto.gameObject.SetActive(false);
        }
    }
}
