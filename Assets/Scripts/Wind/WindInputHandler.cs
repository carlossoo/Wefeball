using Game.Core;
using UnityEngine;

namespace Game.Wind
{
    /// <summary>
    /// Traduz o INPUT do jogador em mudanças no vento:
    ///  - Segurar o botão esquerdo do mouse + mover o mouse = gira o vento em 3D, RELATIVO À CÂMERA:
    ///    mouse para a direita = a seta vira para a direita da tela; mouse para cima = a seta sobe na tela.
    ///    Ao soltar o botão, a direção fica fixa (não precisamos fazer nada, o estado já está salvo).
    ///  - Shift = alterna a força (fraca > média > forte > fraca).
    ///
    /// ONDE COLOCAR: GameObject "_Systems" (junto com GameInput e WindController).
    /// COM QUEM CONVERSA: GameInput (lê input) e WindController (altera o estado).
    ///
    /// A câmera congela enquanto o botão esquerdo está segurado (isso é feito no OrbitCamera).
    /// </summary>
    public class WindInputHandler : MonoBehaviour
    {
        [Tooltip("Opcional. Se vazio, usa o WindController.Current da cena.")]
        [SerializeField] private WindController wind;

        [Tooltip("Câmera que serve de referência para mirar o vento. Se vazio, usa a Main Camera (tag MainCamera).")]
        [SerializeField] private Transform viewCamera;

        private WindController Wind => wind != null ? wind : WindController.Current;

        private GameInput _subscribedInput; // guardamos para desinscrever do MESMO objeto

        private void OnEnable()
        {
            _subscribedInput = GameInput.Current;
            if (_subscribedInput == null)
            {
                Debug.LogWarning("WindInputHandler: não existe GameInput na cena.", this);
                return;
            }

            _subscribedInput.CycleWindStrengthPressed += HandleCycleStrength;
        }

        private void OnDisable()
        {
            if (_subscribedInput == null) return;

            _subscribedInput.CycleWindStrengthPressed -= HandleCycleStrength;
            _subscribedInput = null;
        }

        private void Update()
        {
            GameInput input = GameInput.Current;
            WindController controller = Wind;
            if (input == null || controller == null || controller.Settings == null) return;
            if (!input.WindAimHeld) return;

            Vector2 look = input.Look;
            if (look == Vector2.zero) return;

            float sensitivity = controller.Settings.MouseSensitivity;
            Transform view = GetViewTransform();

            if (view == null)
            {
                // Sem câmera: cai no modo antigo, em eixos fixos do mundo.
                controller.AddRotation(look.x * sensitivity, look.y * sensitivity);
                return;
            }

            // MODO RELATIVO À CÂMERA: o vento gira como a seta é vista NA TELA.
            //  - mouse para a direita  => seta vira para a direita da tela
            //  - mouse para cima       => seta sobe na tela
            // Funciona para qualquer direção atual, inclusive apontando para a câmera.
            Quaternion turn =
                Quaternion.AngleAxis(look.x * sensitivity, view.up) *
                Quaternion.AngleAxis(-look.y * sensitivity, view.right);

            controller.SetDirection(turn * controller.Direction);
        }

        /// <summary>Câmera usada como referência da mira (a do Inspector, ou a Main Camera).</summary>
        private Transform GetViewTransform()
        {
            if (viewCamera != null) return viewCamera;

            Camera main = Camera.main;
            return main != null ? main.transform : null;
        }

        private void HandleCycleStrength()
        {
            WindController controller = Wind;
            if (controller != null) controller.CycleStrength();
        }
    }
}
