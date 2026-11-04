using UnityEngine;

namespace Game.Wind
{
    /// <summary>
    /// Faz um objeto QUALQUER do cenário (caixa, folha, plataforma solta...) reagir ao vento global.
    /// O player NÃO usa este script (ele usa PlayerWindReceiver, que depende da forma atual).
    ///
    /// ONDE COLOCAR: no objeto que tem Rigidbody (não-kinematic).
    /// CONFIGURAR:
    ///  - windResponse: quanto o objeto reage. Objetos leves = valor alto, pesados = valor baixo.
    ///  - massa do objeto: ajuste no Rigidbody. Mais massa = acelera mais devagar com o mesmo vento.
    ///
    /// COM QUEM CONVERSA: lê WindController.Current e WindPhysics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class WindAffectedBody : MonoBehaviour
    {
        [Tooltip("Quanto este objeto reage ao vento. 0 = ignora o vento. Referência: bola = 0.4, pena = 0.15.")]
        [SerializeField, Min(0f)] private float windResponse = 0.4f;

        [Tooltip("0 a 1. Variações aleatórias no vento sentido por este objeto (deixa o movimento mais caótico).")]
        [SerializeField, Range(0f, 1f)] private float turbulence = 0f;

        [Tooltip("Quão rápido a turbulência muda.")]
        [SerializeField, Min(0.1f)] private float turbulenceFrequency = 1.5f;

        private Rigidbody _body;
        private float _noiseSeed;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _noiseSeed = Random.value * 100f; // cada objeto tem seu próprio ruído
        }

        private void FixedUpdate()
        {
            WindController wind = WindController.Current;
            if (wind == null || _body.isKinematic || windResponse <= 0f) return;

            Vector3 windVelocity = WindPhysics.AddTurbulence(wind.Velocity, turbulence, turbulenceFrequency, _noiseSeed);
            Vector3 force = WindPhysics.ComputeDragForce(
                windVelocity, _body.linearVelocity, windResponse, _body.mass, Time.fixedDeltaTime);

            _body.AddForce(force, ForceMode.Force);
        }
    }
}
