using System;
using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Pulo ÚNICO da Bola: pode ser usado no chão OU no ar, e só volta a estar disponível ao tocar o chão.
    /// (Se o player cair de uma plataforma sem pular, ele AINDA tem o pulo para usar no ar.)
    /// Só funciona nas formas com canJump = true (a Bola). Pressionar espaço cedo demais (até
    /// jumpBufferTime antes de poder pular) fica "guardado" por um instante.
    ///
    /// ONDE COLOCAR: GameObject raiz do Player.
    /// COM QUEM CONVERSA: GameInput (tecla), PlayerFormController (pode pular? quão alto?), GroundCheck (reset).
    ///
    /// EVENTO: JumpPerformed é disparado quando o pulo acontece (use para som/partículas).
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(PlayerFormController), typeof(GroundCheck))]
    public class PlayerJump : MonoBehaviour
    {
        [Tooltip("Tempo (s) em que o espaço fica 'guardado' se apertado antes de poder pular.")]
        [SerializeField, Min(0f)] private float jumpBufferTime = 0.1f;

        [Tooltip("Depois de pular, ignora o chão por este tempo (s) para o pulo não resetar no mesmo instante.")]
        [SerializeField, Min(0f)] private float groundResetDelay = 0.15f;

        private Rigidbody _body;
        private PlayerFormController _forms;
        private GroundCheck _ground;
        private GameInput _input;

        private bool _jumpAvailable = true;
        private float _bufferTimer;
        private float _lastJumpTime = float.NegativeInfinity;

        /// <summary>True se o pulo ainda não foi gasto (útil para UI).</summary>
        public bool JumpAvailable => _jumpAvailable;

        /// <summary>Disparado quando um pulo acontece.</summary>
        public event Action JumpPerformed;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _forms = GetComponent<PlayerFormController>();
            _ground = GetComponent<GroundCheck>();
        }

        private void OnEnable()
        {
            _input = GameInput.Current;
            if (_input == null)
            {
                Debug.LogWarning("PlayerJump: não existe GameInput na cena.", this);
                return;
            }

            _input.JumpPressed += OnJumpPressed;
        }

        private void OnDisable()
        {
            if (_input == null) return;

            _input.JumpPressed -= OnJumpPressed;
            _input = null;
        }

        private void OnJumpPressed() => _bufferTimer = jumpBufferTime;

        private void FixedUpdate()
        {
            // 1) Tocou o chão (e não acabou de pular)? O pulo volta.
            bool justJumped = Time.time - _lastJumpTime < groundResetDelay;
            if (_ground.IsGrounded && !justJumped) _jumpAvailable = true;

            // 2) Tem um pedido de pulo guardado?
            if (_bufferTimer <= 0f) return;
            _bufferTimer -= Time.fixedDeltaTime;

            PlayerFormData data = _forms.CurrentData;
            if (data == null || !data.CanJump || !_jumpAvailable) return;

            Jump(data);
        }

        private void Jump(PlayerFormData data)
        {
            // Define a velocidade vertical (em vez de somar), assim o pulo tem a mesma altura
            // mesmo se o player estiver caindo rápido.
            Vector3 velocity = _body.linearVelocity;
            velocity.y = data.JumpSpeed;
            _body.linearVelocity = velocity;

            _jumpAvailable = false;
            _bufferTimer = 0f;
            _lastJumpTime = Time.time;

            JumpPerformed?.Invoke();
        }
    }
}
