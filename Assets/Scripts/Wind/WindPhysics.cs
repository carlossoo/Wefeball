using UnityEngine;

namespace Game.Wind
{
    /// <summary>
    /// Fórmulas de física do vento, num lugar só. Todo objeto afetado pelo vento (player e props)
    /// usa estas funções, então o "jeito" do vento é igual para todos e fácil de ajustar.
    ///
    /// COMO FUNCIONA (modelo de arrasto):
    ///   força = (velocidadeDoVento - velocidadeDoCorpo) * windResponse
    /// O vento "puxa" o objeto em direção à velocidade do vento. Quanto maior o windResponse, mais
    /// rápido o objeto acompanha o vento; quanto MAIOR a massa, mais devagar ele acelera.
    /// Isso gera uma velocidade máxima natural (o objeto nunca passa muito da velocidade do vento)
    /// e também funciona como resistência do ar (um objeto caindo é freado por ela).
    ///
    /// Classe estática: não precisa ser colocada em nenhum GameObject.
    /// </summary>
    public static class WindPhysics
    {
        // Margem de segurança para a física não "estourar" com objetos muito leves.
        private const float StabilityMargin = 0.9f;

        /// <summary>
        /// Calcula a força (em Newtons) que o vento aplica num corpo.
        /// </summary>
        /// <param name="windVelocity">Velocidade do vento como vetor (WindController.Velocity).</param>
        /// <param name="bodyVelocity">Velocidade atual do Rigidbody.</param>
        /// <param name="windResponse">Quanto o corpo reage ao vento (coeficiente de arrasto).</param>
        /// <param name="mass">Massa do Rigidbody (usada só para evitar instabilidade).</param>
        /// <param name="deltaTime">Time.fixedDeltaTime.</param>
        public static Vector3 ComputeDragForce(
            Vector3 windVelocity, Vector3 bodyVelocity, float windResponse, float mass, float deltaTime)
        {
            // Se windResponse * dt / massa passar de 1, o corpo "ultrapassaria" o vento e oscilaria.
            // Limitamos o coeficiente para isso nunca acontecer.
            float maxStable = deltaTime > 0f ? (mass / deltaTime) * StabilityMargin : windResponse;
            float response = Mathf.Min(windResponse, maxStable);

            return (windVelocity - bodyVelocity) * response;
        }

        /// <summary>
        /// Adiciona "turbulência" ao vento: variações suaves e aleatórias na direção/intensidade.
        /// Usada para deixar a pena meio descontrolada. amount = 0 desliga.
        /// </summary>
        /// <param name="amount">0 a 1. Quanto da velocidade do vento vira ruído.</param>
        /// <param name="frequency">Quão rápido o ruído muda (maior = mais nervoso).</param>
        /// <param name="seed">Número aleatório fixo por objeto, para cada um ter seu próprio ruído.</param>
        public static Vector3 AddTurbulence(Vector3 windVelocity, float amount, float frequency, float seed)
        {
            if (amount <= 0f) return windVelocity;

            float t = Time.time * frequency;
            Vector3 noise = new Vector3(
                Noise(t, seed),
                Noise(t, seed + 17.3f),
                Noise(t, seed + 41.7f));

            return windVelocity + noise * (windVelocity.magnitude * amount);
        }

        // Ruído suave entre -1 e 1.
        private static float Noise(float t, float seed)
        {
            return Mathf.PerlinNoise(t, seed) * 2f - 1f;
        }
    }
}
