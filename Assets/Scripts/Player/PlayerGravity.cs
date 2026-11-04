using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Aplica a gravidade do player com o multiplicador da forma atual (Pena cai devagar, Peso cai rápido).
    /// O Rigidbody do Unity não tem "gravity scale" em 3D, então desligamos a gravidade dele
    /// e aplicamos a nossa aqui.
    ///
    /// ONDE COLOCAR: GameObject raiz do Player.
    /// COM QUEM CONVERSA: PlayerFormController (lê GravityScale da forma atual).
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(PlayerFormController))]
    public class PlayerGravity : MonoBehaviour
    {
        private Rigidbody _body;
        private PlayerFormController _forms;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _forms = GetComponent<PlayerFormController>();
            _body.useGravity = false; // a gravidade é aplicada manualmente em FixedUpdate
        }

        private void FixedUpdate()
        {
            PlayerFormData data = _forms.CurrentData;
            if (data == null) return;

            // Acceleration ignora a massa: todos caem com a mesma gravidade base (o arrasto é que diferencia).
            _body.AddForce(Physics.gravity * data.GravityScale, ForceMode.Acceleration);
        }
    }
}
