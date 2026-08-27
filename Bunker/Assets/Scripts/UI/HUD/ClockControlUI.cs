// ============================================================================
// ClockControlUI.cs — Controles de velocidad del reloj de simulación
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bunker.UI.HUD
{
    /// <summary>
    /// Controles de UI para pausar, reanudar y cambiar la velocidad del reloj.
    /// Muestra el día y hora actuales.
    /// </summary>
    public class ClockControlUI : MonoBehaviour
    {
        [Header("── Textos ──")]
        [SerializeField] private TextMeshProUGUI tiempoTexto;
        [SerializeField] private TextMeshProUGUI velocidadTexto;

        [Header("── Botones ──")]
        [SerializeField] private Button botonPausa;
        [SerializeField] private Button botonX1;
        [SerializeField] private Button botonX2;
        [SerializeField] private Button botonX4;

        [Header("── Visual ──")]
        [SerializeField] private Color colorBotonActivo = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color colorBotonInactivo = new Color(0.5f, 0.5f, 0.5f);

        private void Start()
        {
            // Asignar listeners a botones
            if (botonPausa != null) botonPausa.onClick.AddListener(OnPausaClick);
            if (botonX1 != null) botonX1.onClick.AddListener(OnX1Click);
            if (botonX2 != null) botonX2.onClick.AddListener(OnX2Click);
            if (botonX4 != null) botonX4.onClick.AddListener(OnX4Click);

            // Suscribirse a cambios de estado
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock != null)
            {
                clock.OnClockStateChanged += ActualizarEstadoVisual;
            }

            ActualizarEstadoVisual(false, 1f);
        }

        private void OnDestroy()
        {
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock != null)
            {
                clock.OnClockStateChanged -= ActualizarEstadoVisual;
            }
        }

        private void Update()
        {
            // Actualizar texto de tiempo cada frame
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock != null && tiempoTexto != null)
            {
                tiempoTexto.text = clock.GetTiempoFormateado();
            }
        }

        // ── Handlers de Botones ──────────────────────────────────────────
        private void OnPausaClick()
        {
            Core.Managers.SimulationClock.Instance?.TogglePausa();
        }

        private void OnX1Click()
        {
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock == null) return;
            clock.SetVelocidadNormal();
            if (clock.EstaPausado) clock.Reanudar();
        }

        private void OnX2Click()
        {
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock == null) return;
            clock.SetVelocidadRapida();
            if (clock.EstaPausado) clock.Reanudar();
        }

        private void OnX4Click()
        {
            var clock = Core.Managers.SimulationClock.Instance;
            if (clock == null) return;
            clock.SetVelocidadMuyRapida();
            if (clock.EstaPausado) clock.Reanudar();
        }

        // ── Visual ───────────────────────────────────────────────────────
        private void ActualizarEstadoVisual(bool pausado, float velocidad)
        {
            // Actualizar texto de velocidad
            if (velocidadTexto != null)
            {
                if (pausado)
                    velocidadTexto.text = "PAUSADO";
                else
                    velocidadTexto.text = $"> x{velocidad:F0}";
            }

            // Resaltar botón activo
            ActualizarColorBoton(botonPausa, pausado);
            ActualizarColorBoton(botonX1, !pausado && Mathf.Approximately(velocidad, 1f));
            ActualizarColorBoton(botonX2, !pausado && Mathf.Approximately(velocidad, 2f));
            ActualizarColorBoton(botonX4, !pausado && Mathf.Approximately(velocidad, 4f));
        }

        private void ActualizarColorBoton(Button boton, bool activo)
        {
            if (boton == null) return;
            var colors = boton.colors;
            colors.normalColor = activo ? colorBotonActivo : colorBotonInactivo;
            boton.colors = colors;
        }
    }
}
