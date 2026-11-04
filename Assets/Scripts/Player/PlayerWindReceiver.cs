using Game.Wind;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Aplica o vento global no player. Quanto ele é afetado depende da forma atual
    /// (windResponse e turbulence do PlayerFormData). O cálculo em si está em WindPhysics.
    ///
    /// ONDE COLOCAR: GameObject raiz do Player.
    /// COM QUEM CONVERSA: WindController.Current (lê o vento) e PlayerFormController (lê os dados da forma).
    ///
    /// Este é o equivalente do WindAffectedBody, mas para o player (que muda de comportamento por forma).
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(PlayerFormController))]
    public class PlayerWindReceiver : MonoBehaviour
    {
        private Rigidbody _body;
        private PlayerFormController _forms;
        private float _noiseSeed;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _forms = GetComponent<PlayerFormController>();
            _noiseSeed = Random.value * 100f;
        }

        private void FixedUpdate()
        {
            WindController wind = WindController.Current;
            PlayerFormData data = _forms.CurrentData;
            if (wind == null || data == null || data.WindResponse <= 0f) return;

            Vector3 windVelocity = WindPhysics.AddTurbulence(
                wind.Velocity, data.Turbulence, data.TurbulenceFrequency, _noiseSeed);

            Vector3 force = WindPhysics.ComputeDragForce(
                windVelocity, _body.linearVelocity, data.WindResponse, _body.mass, Time.fixedDeltaTime);

            _body.AddForce(force, ForceMode.Force);
        }
    }
}
