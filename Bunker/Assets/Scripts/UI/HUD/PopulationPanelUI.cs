// ============================================================================
// PopulationPanelUI.cs — Panel de UI para la población y moral del búnker
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bunker.Bunker.Population;

namespace Bunker.UI.HUD
{
    /// <summary>
    /// Muestra la información de población, moral y estado de carencias.
    /// </summary>
    public class PopulationPanelUI : MonoBehaviour
    {
        [Header("── Población ──")]
        [SerializeField] private TextMeshProUGUI poblacionTexto;
        [SerializeField] private Image poblacionBarra;

        [Header("── Moral ──")]
        [SerializeField] private TextMeshProUGUI moralTexto;
        [SerializeField] private Image moralBarra;
        [SerializeField] private Gradient moralColorGradient;

        [Header("── Alertas ──")]
        [SerializeField] private GameObject alertaSinAgua;
        [SerializeField] private GameObject alertaSinComida;
        [SerializeField] private TextMeshProUGUI alertaMuertesTexto;

        private void Start()
        {
            SuscribirEventos();
            ActualizarTodo();
        }

        private void OnDestroy()
        {
            DesuscribirEventos();
        }

        private void SuscribirEventos()
        {
            var pop = PopulationManager.Instance;
            if (pop != null)
            {
                pop.OnPopulationChanged += OnPoblacionChanged;
                pop.OnMoralChanged += OnMoralChanged;
                pop.OnDeaths += OnMuertes;
            }
        }

        private void DesuscribirEventos()
        {
            var pop = PopulationManager.Instance;
            if (pop != null)
            {
                pop.OnPopulationChanged -= OnPoblacionChanged;
                pop.OnMoralChanged -= OnMoralChanged;
                pop.OnDeaths -= OnMuertes;
            }
        }

        private void OnPoblacionChanged(int anterior, int nueva)
        {
            ActualizarPoblacion();
        }

        private void OnMoralChanged(float anterior, float nueva)
        {
            ActualizarMoral();
        }

        private void OnMuertes(int cantidad, string causa)
        {
            if (alertaMuertesTexto != null)
            {
                alertaMuertesTexto.text = $"⚠ {cantidad} muertes por {causa}";
                alertaMuertesTexto.gameObject.SetActive(true);
                // Ocultar después de unos segundos
                CancelInvoke(nameof(OcultarAlertaMuertes));
                Invoke(nameof(OcultarAlertaMuertes), 5f);
            }
        }

        private void OcultarAlertaMuertes()
        {
            if (alertaMuertesTexto != null)
                alertaMuertesTexto.gameObject.SetActive(false);
        }

        private void Update()
        {
            // Actualizar alertas de carencias en tiempo real
            var pop = PopulationManager.Instance;
            if (pop == null) return;

            if (alertaSinAgua != null)
                alertaSinAgua.SetActive(pop.HorasSinAgua > 0);

            if (alertaSinComida != null)
                alertaSinComida.SetActive(pop.HorasSinComida > 0);
        }

        private void ActualizarTodo()
        {
            ActualizarPoblacion();
            ActualizarMoral();
        }

        private void ActualizarPoblacion()
        {
            var pop = PopulationManager.Instance;
            if (pop == null) return;

            if (poblacionTexto != null)
                poblacionTexto.text = $"{pop.PoblacionActual} / {pop.PoblacionMaxima}";

            if (poblacionBarra != null)
                poblacionBarra.fillAmount = (float)pop.PoblacionActual / pop.PoblacionMaxima;
        }

        private void ActualizarMoral()
        {
            var pop = PopulationManager.Instance;
            if (pop == null) return;

            float ratio = pop.MoralActual / pop.MoralMaxima;

            if (moralTexto != null)
                moralTexto.text = $"Moral: {Mathf.FloorToInt(pop.MoralActual)}%";

            if (moralBarra != null)
            {
                moralBarra.fillAmount = ratio;

                if (moralColorGradient != null)
                    moralBarra.color = moralColorGradient.Evaluate(ratio);
            }
        }
    }
}
