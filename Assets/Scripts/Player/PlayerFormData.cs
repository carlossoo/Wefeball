using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Todos os números que definem COMO uma forma (Bola, Pena ou Peso) se comporta.
    /// É um asset (ScriptableObject): crie um para cada forma e ajuste os valores sem tocar em código.
    ///
    /// COMO CRIAR: menu "Game > Setup > Create Default Assets" (já cria os 3 com valores iniciais)
    /// ou botão direito na pasta > Create > Game > Player Form Data.
    ///
    /// QUEM LÊ ESTES DADOS: PlayerFormController (aplica no Rigidbody), PlayerGravity, PlayerWindReceiver,
    /// PlayerMovement e PlayerJump. Ninguém escreve aqui em tempo de jogo.
    ///
    /// DICA DE BALANCEAMENTO:
    ///  - Força do vento sobre o corpo (N) = (velocidadeVento - velocidadeCorpo) * windResponse.
    ///    Com vento forte (20 m/s) e corpo parado, a força máxima é 20 * windResponse.
    ///  - Compare isso com moveForce: se moveForce for MAIOR, o jogador consegue andar contra o vento;
    ///    se for MENOR, o vento vence.
    ///  - Aceleração = força / massa. Por isso a massa também decide o quanto o vento "empurra".
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerForm_New", menuName = "Game/Player Form Data")]
    public class PlayerFormData : ScriptableObject
    {
        [Header("Identificação")]
        [SerializeField] private PlayerForm form = PlayerForm.Ball;

        [Header("Física do corpo")]
        [Tooltip("Massa do Rigidbody (kg).")]
        [SerializeField, Min(0.01f)] private float mass = 1f;

        [Tooltip("Resistência do ar em todos os eixos. Valor alto = perde velocidade rápido.")]
        [SerializeField, Min(0f)] private float linearDamping = 0.1f;

        [Tooltip("Resistência à rotação. Valor alto = para de girar rápido.")]
        [SerializeField, Min(0f)] private float angularDamping = 0.05f;

        [Tooltip("Multiplicador da gravidade. 1 = normal, menor = flutua, maior = cai rápido.")]
        [SerializeField] private float gravityScale = 1f;

        [Tooltip("Material físico (atrito/quique) aplicado ao colisor nesta forma. Opcional.")]
        [SerializeField] private PhysicsMaterial physicsMaterial;

        [Header("Vento")]
        [Tooltip("Quanto o vento empurra esta forma. 0 = ignora o vento. Veja a fórmula no topo do script.")]
        [SerializeField, Min(0f)] private float windResponse = 0.5f;

        [Tooltip("0 a 1. Variações aleatórias no vento sentido pelo corpo (deixa o movimento 'descontrolado').")]
        [SerializeField, Range(0f, 1f)] private float turbulence = 0f;

        [Tooltip("Quão rápido a turbulência muda (maior = mais nervoso).")]
        [SerializeField, Min(0.1f)] private float turbulenceFrequency = 1.5f;

        [Header("Movimento (WASD)")]
        [Tooltip("Se desmarcado, o WASD não faz nada nesta forma (ex.: Peso).")]
        [SerializeField] private bool canMove = true;

        [Tooltip("Força aplicada pelo WASD (Newtons).")]
        [SerializeField, Min(0f)] private float moveForce = 8f;

        [Tooltip("O WASD para de empurrar quando a velocidade na direção desejada chega a este valor (m/s). O vento pode passar disso.")]
        [SerializeField, Min(0f)] private float maxMoveSpeed = 8f;

        [Tooltip("Freio aplicado no chão quando o jogador NÃO aperta nada. 0 = desliza até parar sozinho.")]
        [SerializeField, Min(0f)] private float groundBrake = 1.5f;

        [Header("Pulo")]
        [Tooltip("Se esta forma pode pular (só a Bola).")]
        [SerializeField] private bool canJump = false;

        [Tooltip("Velocidade vertical definida no pulo (m/s). Altura aproximada = v² / (2 * 9.81 * gravityScale).")]
        [SerializeField, Min(0f)] private float jumpSpeed = 7f;

        public PlayerForm Form => form;
        public float Mass => mass;
        public float LinearDamping => linearDamping;
        public float AngularDamping => angularDamping;
        public float GravityScale => gravityScale;
        public PhysicsMaterial PhysicsMaterial => physicsMaterial;
        public float WindResponse => windResponse;
        public float Turbulence => turbulence;
        public float TurbulenceFrequency => turbulenceFrequency;
        public bool CanMove => canMove;
        public float MoveForce => moveForce;
        public float MaxMoveSpeed => maxMoveSpeed;
        public float GroundBrake => groundBrake;
        public bool CanJump => canJump;
        public float JumpSpeed => jumpSpeed;
    }
}
