using System;
using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Controla QUAL forma o player está usando (Bola, Pena ou Peso) e aplica os dados dela no MESMO
    /// Rigidbody. A posição e a velocidade NÃO mudam ao trocar de forma, só massa, resistência,
    /// material físico e o visual.
    ///
    /// ONDE COLOCAR: no GameObject raiz do Player (junto com o Rigidbody e os 3 colisores).
    /// CONFIGURAR no Inspector:
    ///  - Em cada slot (Bola/Pena/Peso): arraste o asset PlayerFormData, o objeto de VISUAL (filho do player)
    ///    e o COLISOR daquela forma (um componente Collider no objeto raiz). Só o colisor da forma
    ///    ativa fica ligado; os outros são desligados na troca.
    ///  - Forma inicial: normalmente Bola.
    ///  - Hoje: Bola = esfera, Pena = esfera, Peso = caixa. Cada forma tem o SEU colisor, então dá para
    ///    mudar tamanho ou tipo de uma sem afetar as outras.
    ///
    /// INPUT: Q alterna Bola > Pena > Peso > Bola. As teclas 1, 2 e 3 escolhem direto (para testes).
    /// A configuração das teclas fica em GameInput.
    ///
    /// QUEM LÊ: PlayerGravity, PlayerWindReceiver, PlayerMovement e PlayerJump consultam CurrentData.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerFormController : MonoBehaviour
    {
        [Serializable]
        private class FormSlot
        {
            [Tooltip("Asset com os números desta forma.")]
            public PlayerFormData data;

            [Tooltip("Objeto filho com o modelo 3D desta forma. É ligado/desligado na troca.")]
            public GameObject visual;

            [Tooltip("Colisor desta forma (no objeto raiz do Player). É ligado/desligado na troca.")]
            public Collider bodyCollider;
        }

        [Header("Formas")]
        [SerializeField] private FormSlot ball = new FormSlot();
        [SerializeField] private FormSlot feather = new FormSlot();
        [SerializeField] private FormSlot weight = new FormSlot();

        [SerializeField] private PlayerForm initialForm = PlayerForm.Ball;

        private Rigidbody _body;
        private GameInput _input;

        /// <summary>Disparado depois de uma troca de forma. Parâmetros: forma nova.</summary>
        public event Action<PlayerForm> FormChanged;

        public PlayerForm CurrentForm { get; private set; }

        /// <summary>Dados da forma atual (nunca null se o Inspector estiver preenchido).</summary>
        public PlayerFormData CurrentData { get; private set; }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();

            // Configurações do Rigidbody que o jogo exige (feitas por código para ninguém esquecer):
            _body.interpolation = RigidbodyInterpolation.Interpolate;          // câmera suave
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Peso rápido não atravessa chão

            SetForm(initialForm);
        }

        private void OnEnable()
        {
            _input = GameInput.Current;
            if (_input == null)
            {
                Debug.LogWarning("PlayerFormController: não existe GameInput na cena.", this);
                return;
            }

            _input.CycleFormPressed += CycleForm;
            _input.FormSelected += SetForm;
        }

        private void OnDisable()
        {
            if (_input == null) return;

            _input.CycleFormPressed -= CycleForm;
            _input.FormSelected -= SetForm;
            _input = null;
        }

        /// <summary>Bola > Pena > Peso > Bola.</summary>
        public void CycleForm()
        {
            SetForm((PlayerForm)(((int)CurrentForm + 1) % 3));
        }

        /// <summary>Troca para uma forma específica.</summary>
        public void SetForm(PlayerForm newForm)
        {
            FormSlot slot = GetSlot(newForm);
            if (slot == null || slot.data == null)
            {
                Debug.LogError($"PlayerFormController: falta o PlayerFormData da forma {newForm}.", this);
                return;
            }

            if (slot.bodyCollider == null)
            {
                Debug.LogError($"PlayerFormController: falta o colisor da forma {newForm}.", this);
                return;
            }

            CurrentForm = newForm;
            CurrentData = slot.data;
            ApplyPhysics(slot);
            ApplyVisualAndCollider(newForm);

            FormChanged?.Invoke(newForm);
        }

        private void ApplyPhysics(FormSlot slot)
        {
            // Não mexemos em position nem velocity: a troca não pode teleportar nem "frear" o player.
            PlayerFormData data = slot.data;
            _body.mass = data.Mass;
            _body.linearDamping = data.LinearDamping;
            _body.angularDamping = data.AngularDamping;
            slot.bodyCollider.sharedMaterial = data.PhysicsMaterial;
        }

        /// <summary>Liga só o visual e o colisor da forma ativa; desliga os das outras.</summary>
        private void ApplyVisualAndCollider(PlayerForm activeForm)
        {
            SetSlotActive(ball, activeForm == PlayerForm.Ball);
            SetSlotActive(feather, activeForm == PlayerForm.Feather);
            SetSlotActive(weight, activeForm == PlayerForm.Weight);
        }

        private static void SetSlotActive(FormSlot slot, bool active)
        {
            if (slot.visual != null) slot.visual.SetActive(active);
            if (slot.bodyCollider != null) slot.bodyCollider.enabled = active;
        }

        private FormSlot GetSlot(PlayerForm form)
        {
            switch (form)
            {
                case PlayerForm.Ball: return ball;
                case PlayerForm.Feather: return feather;
                case PlayerForm.Weight: return weight;
                default: return null;
            }
        }
    }
}
