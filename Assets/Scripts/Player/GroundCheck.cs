using System;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Descobre se o player está TOCANDO O CHÃO, usando os pontos de contato das colisões
    /// (não precisa configurar layers).
    /// Uma superfície conta como chão se não for inclinada demais (maxGroundAngle).
    ///
    /// ONDE COLOCAR: no MESMO GameObject do Rigidbody/colisor (o Unity só envia as colisões para lá).
    /// QUEM USA: PlayerJump (reseta o pulo) e PlayerMovement (freio no chão).
    ///
    /// OBSERVAÇÃO: se o Rigidbody "dormir" (parado), o Unity para de avisar colisões e IsGrounded
    /// pode ficar falso por 1 passo de física. Isso não atrapalha o pulo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GroundCheck : MonoBehaviour
    {
        [Tooltip("Inclinação máxima (graus) que ainda conta como chão. Acima disso é parede.")]
        [SerializeField, Range(0f, 89f)] private float maxGroundAngle = 50f;

        private float _minGroundNormalY;
        private bool _touchedGroundThisStep;

        /// <summary>True se tocou o chão no último passo de física.</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>Disparado no passo em que o player volta a tocar o chão.</summary>
        public event Action Landed;

        private void Awake()
        {
            _minGroundNormalY = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }

        private void FixedUpdate()
        {
            // As colisões do passo anterior já chegaram (OnCollision* roda depois do FixedUpdate).
            bool wasGrounded = IsGrounded;
            IsGrounded = _touchedGroundThisStep;
            _touchedGroundThisStep = false;

            if (!wasGrounded && IsGrounded) Landed?.Invoke();
        }

        private void OnCollisionEnter(Collision collision) => CheckContacts(collision);
        private void OnCollisionStay(Collision collision) => CheckContacts(collision);

        private void CheckContacts(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                // normal.y perto de 1 = superfície plana virada para cima.
                if (collision.GetContact(i).normal.y >= _minGroundNormalY)
                {
                    _touchedGroundThisStep = true;
                    return;
                }
            }
        }
    }
}
