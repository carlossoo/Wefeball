using System;
using UnityEngine;

namespace Game.Wind
{
    /// <summary>
    /// Dono do ESTADO do vento global: direção (3D) e nível de força (fraca/média/forte).
    /// Este script NÃO lê input e NÃO move nada. Ele só guarda o estado e avisa quem quiser ouvir.
    ///
    /// ONDE COLOCAR: GameObject "_Systems" da cena (um WindController por fase).
    /// CONFIGURAR: arraste o asset WindSettings. Defina a direção e a força INICIAIS desta fase.
    ///
    /// QUEM MUDA O ESTADO: WindInputHandler (a partir do input do jogador).
    /// QUEM LÊ O ESTADO: PlayerWindReceiver, WindAffectedBody, WindArrowIndicator.
    ///
    /// EXCEÇÃO DE SINGLETON: WindController.Current existe porque props em prefabs não conseguem
    /// referenciar objetos de cena. É a única forma permitida de "achar" o vento.
    ///
    /// DIREÇÃO: guardamos yaw (giro horizontal) e pitch (inclinação, + = para cima).
    /// Direction é a direção PARA ONDE o vento sopra, normalizada.
    /// </summary>
    [DefaultExecutionOrder(-100)] // roda antes dos outros scripts, para o Current já existir
    public class WindController : MonoBehaviour
    {
        /// <summary>Vento ativo na cena atual (pode ser null).</summary>
        public static WindController Current { get; private set; }

        [Header("Referências")]
        [Tooltip("Asset com os 3 valores de força (Create > Game > Wind Settings).")]
        [SerializeField] private WindSettings settings;

        [Header("Estado inicial desta fase")]
        [Tooltip("Giro horizontal inicial em graus. 0 = sopra para +Z (frente do mundo), 90 = para +X.")]
        [SerializeField] private float initialYaw = 0f;

        [Tooltip("Inclinação inicial em graus. 0 = horizontal, positivo = para cima.")]
        [SerializeField, Range(-89f, 89f)] private float initialPitch = 0f;

        [SerializeField] private WindStrength initialStrength = WindStrength.Weak;

        private float _yaw;
        private float _pitch;

        /// <summary>Disparado sempre que direção ou força mudam.</summary>
        public event Action Changed;

        public WindSettings Settings => settings;
        public WindStrength Strength { get; private set; }

        /// <summary>Direção para onde o vento sopra (vetor unitário).</summary>
        public Vector3 Direction { get; private set; } = Vector3.forward;

        /// <summary>Velocidade atual do vento em m/s (depende do nível de força).</summary>
        public float Speed => settings != null ? settings.GetSpeed(Strength) : 0f;

        /// <summary>Velocidade do vento como vetor (Direction * Speed).</summary>
        public Vector3 Velocity => Direction * Speed;

        private float MaxPitch => settings != null ? settings.MaxPitch : 80f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;

        private void Awake()
        {
            if (Current != null && Current != this)
                Debug.LogWarning("Existe mais de um WindController na cena. Deixe apenas um.", this);

            Current = this;

            if (settings == null)
                Debug.LogError("WindController sem WindSettings! Arraste o asset no Inspector.", this);

            Strength = initialStrength;
            _yaw = initialYaw;
            _pitch = Mathf.Clamp(initialPitch, -MaxPitch, MaxPitch);
            RefreshDirection();
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        /// <summary>Gira o vento. yawDelta = horizontal (graus), pitchDelta = vertical (graus, + = para cima).</summary>
        public void AddRotation(float yawDelta, float pitchDelta)
        {
            _yaw = Mathf.Repeat(_yaw + yawDelta, 360f);
            _pitch = Mathf.Clamp(_pitch + pitchDelta, -MaxPitch, MaxPitch);
            RefreshDirection();
        }

        /// <summary>Aponta o vento para uma direção específica (útil para fases e testes).</summary>
        public void SetDirection(Vector3 worldDirection)
        {
            if (worldDirection.sqrMagnitude < 0.0001f) return;

            worldDirection.Normalize();
            _yaw = Mathf.Atan2(worldDirection.x, worldDirection.z) * Mathf.Rad2Deg;
            _pitch = Mathf.Clamp(Mathf.Asin(Mathf.Clamp(worldDirection.y, -1f, 1f)) * Mathf.Rad2Deg, -MaxPitch, MaxPitch);
            RefreshDirection();
        }

        /// <summary>Fraca > Média > Forte > Fraca...</summary>
        public void CycleStrength()
        {
            SetStrength((WindStrength)(((int)Strength + 1) % 3));
        }

        public void SetStrength(WindStrength strength)
        {
            if (Strength == strength) return;

            Strength = strength;
            Changed?.Invoke();
        }

        private void RefreshDirection()
        {
            // Euler(-pitch, yaw, 0): o sinal negativo faz pitch positivo apontar PARA CIMA.
            Direction = Quaternion.Euler(-_pitch, _yaw, 0f) * Vector3.forward;
            Changed?.Invoke();
        }

        /// <summary>
        /// Desenha uma seta do vento na Scene view (e na Game view com Gizmos ligado), saindo deste objeto.
        /// Serve só para DEPURAR enquanto a seta 3D da interface não existe. Cor: verde, amarelo, vermelho.
        /// </summary>
        private void OnDrawGizmos()
        {
            Vector3 direction = Application.isPlaying
                ? Direction
                : Quaternion.Euler(-initialPitch, initialYaw, 0f) * Vector3.forward;

            WindStrength strength = Application.isPlaying ? Strength : initialStrength;
            Gizmos.color = strength == WindStrength.Weak ? Color.green
                : strength == WindStrength.Medium ? Color.yellow
                : Color.red;

            float length = 2f + (int)strength * 1.5f; // maior = mais forte
            Vector3 start = transform.position;
            Vector3 end = start + direction * length;

            Gizmos.DrawLine(start, end);

            // Ponta da seta: duas linhas curtas voltando a partir do fim.
            Vector3 side = Vector3.Cross(direction, Vector3.up);
            if (side.sqrMagnitude < 0.001f) side = Vector3.Cross(direction, Vector3.right);
            side.Normalize();
            Gizmos.DrawLine(end, end - direction * 0.5f + side * 0.25f);
            Gizmos.DrawLine(end, end - direction * 0.5f - side * 0.25f);
        }
    }
}
