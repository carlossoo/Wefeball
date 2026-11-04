using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
    /// <summary>
    /// Central de INPUT do jogo (Input System novo). Todos os outros scripts leem o input daqui,
    /// nenhum script deve ler teclado/mouse diretamente.
    ///
    /// ONDE COLOCAR: um GameObject "_Systems" na cena (um único GameInput por cena).
    /// COM QUEM CONVERSA: PlayerMovement, PlayerJump, PlayerFormController, WindInputHandler, OrbitCamera.
    ///
    /// PARA MUDAR UMA TECLA: edite o caminho no método BuildActions() (ex.: "&lt;Keyboard&gt;/q").
    /// As ações são criadas por código para não precisarmos de um arquivo .inputactions
    /// (isso evita conflito de merge no Git entre vocês dois).
    ///
    /// EXCEÇÃO DE SINGLETON: o acesso por GameInput.Current é uma das duas exceções permitidas
    /// (a outra é WindController.Current), porque prefabs não conseguem referenciar objetos de cena.
    /// </summary>
    [DefaultExecutionOrder(-100)] // roda antes dos outros scripts, para o Current já existir
    public class GameInput : MonoBehaviour
    {
        /// <summary>Instância ativa na cena atual (pode ser null se a cena não tiver GameInput).</summary>
        public static GameInput Current { get; private set; }

        // ---------- Eventos (disparados uma vez quando a tecla é apertada) ----------
        public event Action JumpPressed;
        public event Action CycleWindStrengthPressed;
        public event Action CycleFormPressed;
        public event Action<PlayerForm> FormSelected;

        // ---------- Valores contínuos (leia quando precisar) ----------
        /// <summary>WASD como vetor: x = A/D, y = W/S.</summary>
        public Vector2 Move => _move.ReadValue<Vector2>();

        /// <summary>Movimento do mouse neste frame (em pixels). Zero se o cursor não estiver travado.</summary>
        public Vector2 Look => Cursor.lockState == CursorLockMode.Locked ? _look.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>Scroll do mouse neste frame: +1 (para cima), -1 (para baixo) ou 0. Independe do dispositivo.</summary>
        public float Zoom
        {
            get
            {
                float raw = _zoom.ReadValue<float>();
                return Mathf.Approximately(raw, 0f) ? 0f : Mathf.Sign(raw);
            }
        }

        /// <summary>True enquanto o botão esquerdo do mouse está segurado (modo "mirar o vento").</summary>
        public bool WindAimHeld => _windAim.IsPressed();

        private InputAction _move, _look, _zoom, _windAim;
        private InputAction _jump, _cycleWind, _cycleForm;
        private InputAction _selectBall, _selectFeather, _selectWeight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;

        private void Awake()
        {
            if (Current != null && Current != this)
                Debug.LogWarning("Existe mais de um GameInput na cena. Deixe apenas um.", this);

            Current = this;
            BuildActions();
        }

        private void OnEnable() => SetAllEnabled(true);
        private void OnDisable() => SetAllEnabled(false);

        private void OnDestroy()
        {
            if (Current == this) Current = null;
            DisposeAll();
        }

        /// <summary>Define TODAS as teclas do jogo. É aqui que se troca um botão.</summary>
        private void BuildActions()
        {
            _move = new InputAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _look = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            _zoom = new InputAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y");
            _windAim = new InputAction("WindAim", InputActionType.Button, "<Mouse>/leftButton");

            _jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            _jump.performed += _ => JumpPressed?.Invoke();

            _cycleWind = new InputAction("CycleWindStrength", InputActionType.Button, "<Keyboard>/leftShift");
            _cycleWind.AddBinding("<Keyboard>/rightShift");
            _cycleWind.performed += _ => CycleWindStrengthPressed?.Invoke();

            _cycleForm = new InputAction("CycleForm", InputActionType.Button, "<Keyboard>/q");
            _cycleForm.performed += _ => CycleFormPressed?.Invoke();

            // Atalhos diretos para teste: 1 = Bola, 2 = Pena, 3 = Peso
            _selectBall = new InputAction("SelectBall", InputActionType.Button, "<Keyboard>/1");
            _selectBall.performed += _ => FormSelected?.Invoke(PlayerForm.Ball);

            _selectFeather = new InputAction("SelectFeather", InputActionType.Button, "<Keyboard>/2");
            _selectFeather.performed += _ => FormSelected?.Invoke(PlayerForm.Feather);

            _selectWeight = new InputAction("SelectWeight", InputActionType.Button, "<Keyboard>/3");
            _selectWeight.performed += _ => FormSelected?.Invoke(PlayerForm.Weight);
        }

        private InputAction[] AllActions() => new[]
        {
            _move, _look, _zoom, _windAim, _jump, _cycleWind,
            _cycleForm, _selectBall, _selectFeather, _selectWeight
        };

        private void SetAllEnabled(bool enabled)
        {
            if (_move == null) return; // Awake ainda não rodou
            foreach (var action in AllActions())
            {
                if (enabled) action.Enable();
                else action.Disable();
            }
        }

        private void DisposeAll()
        {
            if (_move == null) return;
            foreach (var action in AllActions()) action.Dispose();
        }
    }
}
