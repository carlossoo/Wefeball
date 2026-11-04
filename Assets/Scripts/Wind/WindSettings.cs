using UnityEngine;

namespace Game.Wind
{
    /// <summary>
    /// Configurações GLOBAIS do vento (asset ScriptableObject). É AQUI que se mudam os 3 valores de força.
    /// Todas as fases usam o mesmo asset, então mudar aqui muda o jogo inteiro.
    ///
    /// COMO CRIAR: botão direito na pasta > Create > Game > Wind Settings.
    /// DICA: você pode editar os valores com o jogo rodando para testar a sensação; no Editor o
    /// valor alterado em Play Mode fica salvo no asset.
    ///
    /// QUEM USA: WindController (lê as velocidades) e WindInputHandler (lê sensibilidade/limite).
    /// </summary>
    [CreateAssetMenu(fileName = "WindSettings", menuName = "Game/Wind Settings")]
    public class WindSettings : ScriptableObject
    {
        [Header("Velocidade do vento em cada nível (metros por segundo)")]
        [Tooltip("Nível FRACO. Brisa leve.")]
        [SerializeField, Min(0f)] private float weakSpeed = 4f;

        [Tooltip("Nível MÉDIO.")]
        [SerializeField, Min(0f)] private float mediumSpeed = 10f;

        [Tooltip("Nível FORTE. Rajada.")]
        [SerializeField, Min(0f)] private float strongSpeed = 20f;

        [Header("Controle da direção com o mouse")]
        [Tooltip("Graus que o vento gira por pixel de movimento do mouse. Aumente para girar mais rápido.")]
        [SerializeField, Min(0.01f)] private float mouseSensitivity = 0.15f;

        [Tooltip("Inclinação máxima do vento para cima/baixo (em graus). Evita direções totalmente verticais.")]
        [SerializeField, Range(1f, 89f)] private float maxPitch = 80f;

        public float MouseSensitivity => mouseSensitivity;
        public float MaxPitch => maxPitch;

        /// <summary>Retorna a velocidade (m/s) correspondente ao nível de força.</summary>
        public float GetSpeed(WindStrength strength)
        {
            switch (strength)
            {
                case WindStrength.Weak: return weakSpeed;
                case WindStrength.Medium: return mediumSpeed;
                case WindStrength.Strong: return strongSpeed;
                default: return weakSpeed;
            }
        }
    }
}
