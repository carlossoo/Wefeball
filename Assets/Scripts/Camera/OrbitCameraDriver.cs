using Game.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Cameras
{
    /// <summary>
    /// Controla a câmera orbital do Cinemachine 3 com o mouse, lendo SEMPRE o input pelo GameInput
    /// (assim o input continua num lugar só e o botão esquerdo pode congelar a câmera).
    ///  - Mouse livre = gira a câmera ao redor do player (mouse para cima = olha para cima).
    ///  - Botão esquerdo segurado (mirando o vento) = a câmera CONGELA. O zoom continua funcionando.
    ///  - Scroll = zoom, com distância mínima e máxima.
    ///
    /// ONDE COLOCAR: no mesmo objeto da CinemachineCamera (ex.: "CM_PlayerCamera"), que precisa ter
    /// CinemachineOrbitalFollow. O menu Game > Setup > Create Player Camera já monta tudo.
    /// IMPORTANTE: NÃO use CinemachineInputAxisController nesse objeto. Ele leria o mouse por conta própria
    /// e a câmera não congelaria ao mirar o vento.
    ///
    /// COM QUEM CONVERSA: GameInput (mouse e scroll) e CinemachineOrbitalFollow (eixos e raio da órbita).
    /// </summary>
    [RequireComponent(typeof(CinemachineOrbitalFollow))]
    public class OrbitCameraDriver : MonoBehaviour
    {
        [Header("Rotação")]
        [Tooltip("Graus que a câmera gira por pixel de movimento do mouse.")]
        [SerializeField, Min(0.01f)] private float lookSensitivity = 0.15f;

        [Tooltip("Inverte o eixo vertical (mouse para cima = olha para baixo).")]
        [SerializeField] private bool invertY = false;

        [Tooltip("Ângulo mínimo da câmera (graus). Negativo = câmera abaixo do player olhando para cima.")]
        [SerializeField, Range(-89f, 89f)] private float minPitch = -15f;

        [Tooltip("Ângulo máximo da câmera (graus). Positivo = câmera acima do player olhando para baixo.")]
        [SerializeField, Range(-89f, 89f)] private float maxPitch = 70f;

        [Tooltip("Ângulo inicial da câmera (graus).")]
        [SerializeField, Range(-89f, 89f)] private float startPitch = 20f;

        [Header("Zoom (scroll)")]
        [Tooltip("Distância mínima da câmera ao player (metros).")]
        [SerializeField, Min(0.5f)] private float minDistance = 3f;

        [Tooltip("Distância máxima da câmera ao player (metros).")]
        [SerializeField, Min(1f)] private float maxDistance = 10f;

        [Tooltip("Distância inicial (metros).")]
        [SerializeField, Min(0.5f)] private float startDistance = 6f;

        [Tooltip("Quanto cada 'clique' do scroll muda a distância (metros).")]
        [SerializeField, Min(0.1f)] private float zoomStep = 1f;

        [Tooltip("Suavização do zoom (segundos). 0 = instantâneo.")]
        [SerializeField, Min(0f)] private float zoomSmoothTime = 0.12f;

        private CinemachineOrbitalFollow _orbital;
        private float _targetDistance;
        private float _currentDistance;
        private float _zoomVelocity;

        private void Awake()
        {
            _orbital = GetComponent<CinemachineOrbitalFollow>();

            if (GetComponent<CinemachineInputAxisController>() != null)
                Debug.LogWarning("OrbitCameraDriver: remova o CinemachineInputAxisController deste objeto; " +
                                 "ele competiria com o GameInput.", this);

            ConfigureOrbit();
        }

        /// <summary>Deixa a órbita no formato que o jogo precisa (esfera ao redor do player, com limites).</summary>
        private void ConfigureOrbit()
        {
            _orbital.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;

            // Horizontal: gira livremente (volta ao início depois de 360 graus).
            _orbital.HorizontalAxis.Range = new Vector2(-180f, 180f);
            _orbital.HorizontalAxis.Wrap = true;
            _orbital.HorizontalAxis.Center = 0f;

            // Vertical: limitado entre minPitch e maxPitch.
            float pitch = Mathf.Clamp(startPitch, minPitch, maxPitch);
            _orbital.VerticalAxis.Range = new Vector2(minPitch, maxPitch);
            _orbital.VerticalAxis.Wrap = false;
            _orbital.VerticalAxis.Center = pitch;
            _orbital.VerticalAxis.Value = pitch;

            _targetDistance = Mathf.Clamp(startDistance, minDistance, maxDistance);
            _currentDistance = _targetDistance;
            _orbital.Radius = _currentDistance;
        }

        private void Update()
        {
            GameInput input = GameInput.Current;
            if (input == null) return;

            UpdateZoom(input.Zoom);

            // Mirando o vento: a câmera fica parada para servir de referência estável.
            if (!input.WindAimHeld) UpdateLook(input.Look);
        }

        private void UpdateLook(Vector2 look)
        {
            if (look == Vector2.zero) return;

            float yawDelta = look.x * lookSensitivity;
            float pitchDelta = (invertY ? look.y : -look.y) * lookSensitivity;

            // mouse para a direita => a câmera orbita para a esquerda e a visão vira para a direita.
            // mouse para cima => valor vertical diminui => câmera desce e a visão sobe.
            _orbital.HorizontalAxis.Value = _orbital.HorizontalAxis.ClampValue(_orbital.HorizontalAxis.Value + yawDelta);
            _orbital.VerticalAxis.Value = _orbital.VerticalAxis.ClampValue(_orbital.VerticalAxis.Value + pitchDelta);
        }

        private void UpdateZoom(float scroll)
        {
            // Scroll para cima (positivo) aproxima a câmera.
            if (!Mathf.Approximately(scroll, 0f))
                _targetDistance = Mathf.Clamp(_targetDistance - scroll * zoomStep, minDistance, maxDistance);

            _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _zoomVelocity, zoomSmoothTime);
            _orbital.Radius = _currentDistance;
        }
    }
}
