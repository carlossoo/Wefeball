using Game.Wind;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Seta 3D no canto da tela que mostra PARA ONDE o vento sopra, do ponto de vista da câmera.
    /// A cor e o tamanho mudam com o nível de força (fraca, média, forte).
    ///
    /// COMO FUNCIONA: a seta é um objeto 3D pequeno, posto bem longe do cenário (hiddenPosition),
    /// fotografado por uma câmera própria que escreve numa textura. A textura é desenhada no canto da tela.
    /// A rotação da seta é a direção do vento vista pela câmera principal; por isso, quando o jogador gira
    /// a câmera, a seta gira junto e sempre mostra para onde o vento empurra o que está na tela.
    ///
    /// ONDE COLOCAR: num GameObject vazio qualquer da cena (ex.: "WindArrowIndicator").
    /// O menu Game > Setup > Create Wind Arrow cria o objeto e o material.
    /// CONFIGURAR: arrowMaterial (material "Unlit"; o menu cria um). O resto é opcional.
    /// COM QUEM CONVERSA: WindController.Current (direção e força) e a Main Camera (referência de visão).
    ///
    /// ATENÇÃO: o Far Clip Plane da câmera principal precisa ser MENOR que a altura de hiddenPosition
    /// (padrão 5000), senão a seta escondida pode aparecer no cenário.
    /// </summary>
    [DefaultExecutionOrder(100)] // depois da câmera se mover, para a seta usar a rotação atual
    public class WindArrowIndicator : MonoBehaviour
    {
        public enum ScreenCorner { TopLeft, TopRight, BottomLeft, BottomRight }

        // Valores técnicos da câmera que fotografa a seta.
        private const float CameraDistance = 2.5f;
        private const float CameraFieldOfView = 30f;
        private const float CameraFarClip = 10f;
        private const int CameraDepth = -50;

        [Header("Posição na tela")]
        [SerializeField] private ScreenCorner corner = ScreenCorner.TopRight;

        [Tooltip("Tamanho da área da seta na tela (pixels).")]
        [SerializeField, Min(32)] private int sizePixels = 160;

        [Tooltip("Distância da borda da tela (pixels).")]
        [SerializeField, Min(0)] private int margin = 24;

        [Header("Aparência")]
        [Tooltip("Material 'Unlit' (sem luz) da seta. O menu Game > Setup > Create Wind Arrow cria um.")]
        [SerializeField] private Material arrowMaterial;

        [SerializeField] private Color weakColor = new Color(0.45f, 0.85f, 0.5f);
        [SerializeField] private Color mediumColor = new Color(0.95f, 0.85f, 0.3f);
        [SerializeField] private Color strongColor = new Color(0.95f, 0.35f, 0.3f);

        [Tooltip("Tamanho da seta em cada nível (1 = ocupa a área toda).")]
        [SerializeField, Range(0.3f, 1.2f)] private float weakScale = 0.6f;
        [SerializeField, Range(0.3f, 1.2f)] private float mediumScale = 0.8f;
        [SerializeField, Range(0.3f, 1.2f)] private float strongScale = 1f;

        [Header("Avançado")]
        [Tooltip("Resolução da textura da seta (pixels).")]
        [SerializeField, Min(64)] private int textureSize = 256;

        [Tooltip("Onde a seta fica no mundo, longe de tudo. Mantenha acima do Far Clip da câmera principal.")]
        [SerializeField] private Vector3 hiddenPosition = new Vector3(0f, 5000f, 0f);

        private WindController _wind;
        private Transform _view;
        private Transform _arrowRoot;
        private Camera _arrowCamera;
        private RenderTexture _texture;
        private Material _materialInstance;
        private Mesh _mesh;

        private void Awake()
        {
            BuildArrowRig();
        }

        private void OnEnable()
        {
            if (_arrowCamera != null) _arrowCamera.enabled = true;

            _wind = WindController.Current;
            if (_wind == null)
            {
                Debug.LogWarning("WindArrowIndicator: não existe WindController na cena.", this);
                return;
            }

            _wind.Changed += RefreshLook;
            RefreshLook();
        }

        private void OnDisable()
        {
            if (_arrowCamera != null) _arrowCamera.enabled = false;
            if (_wind == null) return;

            _wind.Changed -= RefreshLook;
            _wind = null;
        }

        private void LateUpdate()
        {
            if (_wind == null || _arrowRoot == null) return;

            if (_view == null && Camera.main != null) _view = Camera.main.transform;
            if (_view == null) return;

            // Direção do vento vista da câmera principal: a seta usa essa direção na "câmera da seta" (que não gira).
            Vector3 viewDirection = Quaternion.Inverse(_view.rotation) * _wind.Direction;
            _arrowRoot.rotation = Quaternion.FromToRotation(Vector3.forward, viewDirection);
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || _texture == null) return;

            GUI.DrawTexture(GetScreenRect(), _texture, ScaleMode.ScaleToFit, true);
        }

        private void OnDestroy()
        {
            if (_arrowRoot != null) Destroy(_arrowRoot.gameObject);
            if (_arrowCamera != null) Destroy(_arrowCamera.gameObject);
            if (_texture != null)
            {
                _texture.Release();
                Destroy(_texture);
            }
            if (_materialInstance != null) Destroy(_materialInstance);
            if (_mesh != null) Destroy(_mesh);
        }

        /// <summary>Cria a seta 3D, a câmera que a fotografa e a textura onde ela aparece.</summary>
        private void BuildArrowRig()
        {
            _mesh = WindArrowMeshFactory.Build();
            _materialInstance = CreateMaterialInstance();

            var arrowObject = new GameObject("[WindArrow 3D]");
            arrowObject.transform.position = hiddenPosition;
            arrowObject.AddComponent<MeshFilter>().sharedMesh = _mesh;

            var meshRenderer = arrowObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _materialInstance;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            _arrowRoot = arrowObject.transform;

            _texture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32)
            {
                name = "WindArrowTexture",
                antiAliasing = 4
            };
            _texture.Create();

            var cameraObject = new GameObject("[WindArrow Camera]");
            cameraObject.transform.position = hiddenPosition + Vector3.back * CameraDistance;
            cameraObject.transform.rotation = Quaternion.identity; // olha sempre para +Z

            _arrowCamera = cameraObject.AddComponent<Camera>();
            _arrowCamera.clearFlags = CameraClearFlags.SolidColor;
            _arrowCamera.backgroundColor = Color.clear; // fundo transparente
            _arrowCamera.fieldOfView = CameraFieldOfView;
            _arrowCamera.nearClipPlane = 0.1f;
            _arrowCamera.farClipPlane = CameraFarClip;
            _arrowCamera.depth = CameraDepth;
            _arrowCamera.targetTexture = _texture;
        }

        private Material CreateMaterialInstance()
        {
            if (arrowMaterial != null) return new Material(arrowMaterial);

            // Plano B: procura um shader sem luz. Pode falhar em builds; por isso o ideal é atribuir o material.
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            Debug.LogWarning("WindArrowIndicator: arrowMaterial vazio. Use Game > Setup > Create Wind Arrow.", this);
            return new Material(shader);
        }

        /// <summary>Atualiza cor e tamanho conforme o nível de força.</summary>
        private void RefreshLook()
        {
            if (_wind == null || _materialInstance == null || _arrowRoot == null) return;

            Color color;
            float scale;
            switch (_wind.Strength)
            {
                case WindStrength.Medium:
                    color = mediumColor;
                    scale = mediumScale;
                    break;
                case WindStrength.Strong:
                    color = strongColor;
                    scale = strongScale;
                    break;
                default:
                    color = weakColor;
                    scale = weakScale;
                    break;
            }

            _materialInstance.color = color;
            _arrowRoot.localScale = Vector3.one * scale;
        }

        private Rect GetScreenRect()
        {
            bool left = corner == ScreenCorner.TopLeft || corner == ScreenCorner.BottomLeft;
            bool top = corner == ScreenCorner.TopLeft || corner == ScreenCorner.TopRight;

            float x = left ? margin : Screen.width - sizePixels - margin;
            float y = top ? margin : Screen.height - sizePixels - margin; // no OnGUI o Y cresce para baixo
            return new Rect(x, y, sizePixels, sizePixels);
        }
    }
}
