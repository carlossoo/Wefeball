using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Movimento por WASD, relativo à câmera (W = para onde a câmera olha).
    /// IMPORTANTE: o WASD aplica FORÇA, não define velocidade. Por isso o vento e o WASD se somam:
    /// com vento fraco o jogador anda contra ele, com vento forte o vento vence.
    /// Os números (força, velocidade máxima) vêm do PlayerFormData da forma atual.
    /// Na forma Peso, canMove = false, então nada acontece.
    ///
    /// ONDE COLOCAR: GameObject raiz do Player.
    /// CONFIGURAR: "Camera Transform" (opcional). Se vazio, usa a Main Camera (tag MainCamera).
    /// COM QUEM CONVERSA: GameInput (WASD), PlayerFormController (dados), GroundCheck (freio no chão).
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(PlayerFormController), typeof(GroundCheck))]
    public class PlayerMovement : MonoBehaviour
    {
        [Tooltip("Câmera que define a direção do WASD. Se vazio, usa a Main Camera.")]
        [SerializeField] private Transform cameraTransform;

        private Rigidbody _body;
        private PlayerFormController _forms;
        private GroundCheck _ground;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _forms = GetComponent<PlayerFormController>();
            _ground = GetComponent<GroundCheck>();
        }

        private void Start()
        {
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (cameraTransform == null)
                Debug.LogWarning("PlayerMovement: nenhuma câmera encontrada. O WASD usará direções do mundo.", this);
        }

        private void FixedUpdate()
        {
            GameInput input = GameInput.Current;
            PlayerFormData data = _forms.CurrentData;
            if (input == null || data == null || !data.CanMove) return;

            Vector2 raw = input.Move;
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(_body.linearVelocity, Vector3.up);

            if (raw.sqrMagnitude < 0.01f)
            {
                ApplyGroundBrake(horizontalVelocity, data);
                return;
            }

            Vector3 direction = GetCameraRelativeDirection(raw);

            // Só empurra enquanto a velocidade NESSA direção estiver abaixo do máximo.
            // (O vento pode levar o player além desse limite; o WASD só não ajuda mais.)
            float speedAlongDirection = Vector3.Dot(horizontalVelocity, direction);
            if (speedAlongDirection < data.MaxMoveSpeed)
                _body.AddForce(direction * data.MoveForce, ForceMode.Force);
        }

        /// <summary>Converte o WASD em direção no mundo, usando o "frente" da câmera sem inclinação.</summary>
        private Vector3 GetCameraRelativeDirection(Vector2 raw)
        {
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up); // câmera olhando reto para baixo
                forward.Normalize();
                right = Vector3.Cross(Vector3.up, forward);
            }

            Vector3 direction = forward * raw.y + right * raw.x;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        /// <summary>Sem input e no chão: aplica um freio proporcional à velocidade (para não deslizar para sempre).</summary>
        private void ApplyGroundBrake(Vector3 horizontalVelocity, PlayerFormData data)
        {
            if (!_ground.IsGrounded || data.GroundBrake <= 0f) return;

            _body.AddForce(-horizontalVelocity * data.GroundBrake, ForceMode.Force);
        }
    }
}
