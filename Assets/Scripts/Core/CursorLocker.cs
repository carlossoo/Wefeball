using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
    /// <summary>
    /// Trava e esconde o cursor durante o jogo (necessário para girar a câmera e o vento com o mouse).
    ///
    /// ONDE COLOCAR: no GameObject "_Systems", junto com o GameInput.
    ///
    /// COMO USAR: ESC destrava o cursor (útil no Editor para clicar em outras janelas).
    /// Clicar com o botão esquerdo dentro do jogo trava de novo.
    /// </summary>
    public class CursorLocker : MonoBehaviour
    {
        private void Start() => Lock();

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Unlock();
            }
            else if (Cursor.lockState != CursorLockMode.Locked && mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                Lock();
            }
        }

        private static void Lock()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void Unlock()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
