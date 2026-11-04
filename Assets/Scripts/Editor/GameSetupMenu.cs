using System;
using Game.Cameras;
using Game.Core;
using Game.Player;
using Game.UI;
using Game.Wind;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game.EditorTools
{
    /// <summary>
    /// Atalhos do Editor para montar o projeto sem cliques repetitivos. Só existe no Editor (não vai para o build).
    /// Menu: Game > Setup
    ///  - Create Default Assets: cria WindSettings e os 3 PlayerFormData com valores iniciais de teste.
    ///    NUNCA sobrescreve assets que já existem (os seus ajustes ficam salvos).
    ///  - Create Player In Scene: cria o objeto Player na cena aberta, já com todos os scripts,
    ///    os 3 visuais provisórios e os assets das formas ligados.
    ///
    /// Os valores abaixo são só um PONTO DE PARTIDA. O balanceamento de verdade é feito nos assets.
    /// </summary>
    public static class GameSetupMenu
    {
        private const string RootFolder = "Assets/_Project";
        private const string DataFolderName = "ScriptableObjects";
        private const string DataFolder = RootFolder + "/" + DataFolderName;

        private const string WindSettingsName = "WindSettings";
        private const string BallName = "PlayerForm_Ball";
        private const string FeatherName = "PlayerForm_Feather";
        private const string WeightName = "PlayerForm_Weight";

        // Tamanhos dos colisores provisórios (metros).
        private const float BallRadius = 0.5f;
        private const float FeatherRadius = 0.5f;
        private const float WeightSize = 0.7f; // lado da caixa do Peso

        [MenuItem("Game/Setup/Create Default Assets")]
        public static void CreateDefaultAssets()
        {
            EnsureFolder("Assets", "_Project");
            EnsureFolder(RootFolder, DataFolderName);

            // Vento: os valores padrão do próprio script já são o ponto de partida (4 / 10 / 20 m/s).
            string windPath = DataFolder + "/" + WindSettingsName + ".asset";
            if (AssetDatabase.LoadAssetAtPath<WindSettings>(windPath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WindSettings>(), windPath);

            CreateForm(BallName, so =>
            {
                SetEnum(so, "form", (int)PlayerForm.Ball);
                SetFloat(so, "mass", 1f);
                SetFloat(so, "linearDamping", 0.1f);
                SetFloat(so, "angularDamping", 0.05f);
                SetFloat(so, "gravityScale", 1f);
                SetFloat(so, "windResponse", 0.4f);
                SetFloat(so, "turbulence", 0f);
                SetBool(so, "canMove", true);
                SetFloat(so, "moveForce", 6f);
                SetFloat(so, "maxMoveSpeed", 8f);
                SetFloat(so, "groundBrake", 1.5f);
                SetBool(so, "canJump", true);
                SetFloat(so, "jumpSpeed", 7f);
            });

            CreateForm(FeatherName, so =>
            {
                SetEnum(so, "form", (int)PlayerForm.Feather);
                SetFloat(so, "mass", 0.1f);
                SetFloat(so, "linearDamping", 0.1f);
                SetFloat(so, "angularDamping", 2f);
                SetFloat(so, "gravityScale", 0.3f);
                SetFloat(so, "windResponse", 0.15f);
                SetFloat(so, "turbulence", 0.25f);
                SetFloat(so, "turbulenceFrequency", 2f);
                SetBool(so, "canMove", true);
                SetFloat(so, "moveForce", 0.8f);
                SetFloat(so, "maxMoveSpeed", 4f);
                SetFloat(so, "groundBrake", 0.5f);
                SetBool(so, "canJump", false);
            });

            CreateForm(WeightName, so =>
            {
                SetEnum(so, "form", (int)PlayerForm.Weight);
                SetFloat(so, "mass", 5f);
                SetFloat(so, "linearDamping", 0f);
                SetFloat(so, "angularDamping", 1f);
                SetFloat(so, "gravityScale", 2.5f);
                SetFloat(so, "windResponse", 0.05f);
                SetFloat(so, "turbulence", 0f);
                SetBool(so, "canMove", false);
                SetFloat(so, "moveForce", 0f);
                SetFloat(so, "maxMoveSpeed", 0f);
                SetFloat(so, "groundBrake", 0f);
                SetBool(so, "canJump", false);
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Game > Setup: assets padrão prontos em " + DataFolder);
        }

        [MenuItem("Game/Setup/Create Player In Scene")]
        public static void CreatePlayerInScene()
        {
            CreateDefaultAssets();

            var ballData = LoadForm(BallName);
            var featherData = LoadForm(FeatherName);
            var weightData = LoadForm(WeightName);

            var player = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(player, "Create Player");
            player.transform.position = new Vector3(0f, 2f, 0f);

            var forms = player.AddComponent<PlayerFormController>(); // adiciona o Rigidbody sozinho

            // Um colisor por forma. Só o da forma ativa fica ligado (o controlador cuida disso).
            var ballCollider = player.AddComponent<SphereCollider>();
            ballCollider.radius = BallRadius;
            var featherCollider = player.AddComponent<SphereCollider>();
            featherCollider.radius = FeatherRadius;
            var weightCollider = player.AddComponent<BoxCollider>();
            weightCollider.size = Vector3.one * WeightSize;

            player.AddComponent<PlayerGravity>();
            player.AddComponent<PlayerWindReceiver>();
            player.AddComponent<PlayerMovement>(); // adiciona o GroundCheck sozinho
            player.AddComponent<PlayerJump>();

            // Visuais provisórios (só para enxergar a troca de forma). Troque pelos modelos finais depois.
            GameObject ballVisual = CreateVisual(player.transform, "Visual_Ball", PrimitiveType.Sphere,
                Vector3.one, Vector3.zero, true);
            GameObject featherVisual = CreateVisual(player.transform, "Visual_Feather", PrimitiveType.Capsule,
                new Vector3(0.25f, 0.5f, 0.25f), new Vector3(90f, 0f, 0f), false);
            GameObject weightVisual = CreateVisual(player.transform, "Visual_Weight", PrimitiveType.Cube,
                Vector3.one * WeightSize, Vector3.zero, false);

            var so = new SerializedObject(forms);
            so.FindProperty("ball.data").objectReferenceValue = ballData;
            so.FindProperty("ball.visual").objectReferenceValue = ballVisual;
            so.FindProperty("ball.bodyCollider").objectReferenceValue = ballCollider;
            so.FindProperty("feather.data").objectReferenceValue = featherData;
            so.FindProperty("feather.visual").objectReferenceValue = featherVisual;
            so.FindProperty("feather.bodyCollider").objectReferenceValue = featherCollider;
            so.FindProperty("weight.data").objectReferenceValue = weightData;
            so.FindProperty("weight.visual").objectReferenceValue = weightVisual;
            so.FindProperty("weight.bodyCollider").objectReferenceValue = weightCollider;
            so.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = player;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Game > Setup: Player criado na cena.");
        }

        [MenuItem("Game/Setup/Create Player Camera")]
        public static void CreatePlayerCamera()
        {
            // 1) A câmera real da cena (Main Camera) precisa de um CinemachineBrain para obedecer a câmera virtual.
            Camera main = Camera.main;
            if (main == null)
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                main = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Main Camera");
            }

            if (main.GetComponent<CinemachineBrain>() == null)
                Undo.AddComponent<CinemachineBrain>(main.gameObject);

            // 2) A câmera virtual orbital que segue o Player.
            var rig = new GameObject("CM_PlayerCamera");
            Undo.RegisterCreatedObjectUndo(rig, "Create Player Camera");

            var virtualCamera = rig.AddComponent<CinemachineCamera>();
            var orbital = rig.AddComponent<CinemachineOrbitalFollow>();
            var composer = rig.AddComponent<CinemachineRotationComposer>();
            rig.AddComponent<OrbitCameraDriver>(); // configura a órbita em esfera e lê o mouse pelo GameInput

            orbital.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbital.Radius = 6f;
            // WorldSpace: a câmera NÃO gira junto com o Player (a bola rola o tempo todo).
            orbital.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            // Olha um pouco acima do centro do player, para ele não ficar colado no meio da tela.
            composer.TargetOffset = new Vector3(0f, 0.6f, 0f);

            var player = Object.FindFirstObjectByType<PlayerFormController>();
            if (player != null)
                virtualCamera.Target.TrackingTarget = player.transform;
            else
                Debug.LogWarning("Game > Setup: nenhum Player na cena. Crie o Player e arraste-o em " +
                                 "Tracking Target da CM_PlayerCamera.");

            Selection.activeGameObject = rig;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Game > Setup: câmera do player criada.");
        }

        [MenuItem("Game/Setup/Create Wind Arrow")]
        public static void CreateWindArrow()
        {
            EnsureFolder("Assets", "_Project");
            EnsureFolder(RootFolder, "Art");
            EnsureFolder(RootFolder + "/Art", "Materials");

            // Material sem luz para a seta. Não sobrescreve se já existir.
            string materialPath = RootFolder + "/Art/Materials/WindArrow.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Unlit/Color");

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
                AssetDatabase.SaveAssets();
            }

            var arrowObject = new GameObject("WindArrowIndicator");
            Undo.RegisterCreatedObjectUndo(arrowObject, "Create Wind Arrow");
            var indicator = arrowObject.AddComponent<WindArrowIndicator>();

            var so = new SerializedObject(indicator);
            so.FindProperty("arrowMaterial").objectReferenceValue = material;
            so.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = arrowObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Game > Setup: seta do vento criada.");
        }

        // ---------- Ajudantes ----------

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static PlayerFormData LoadForm(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<PlayerFormData>(DataFolder + "/" + fileName + ".asset");
        }

        private static void CreateForm(string fileName, Action<SerializedObject> fill)
        {
            string path = DataFolder + "/" + fileName + ".asset";
            if (AssetDatabase.LoadAssetAtPath<PlayerFormData>(path) != null) return; // não sobrescreve

            var asset = ScriptableObject.CreateInstance<PlayerFormData>();
            AssetDatabase.CreateAsset(asset, path);

            var so = new SerializedObject(asset);
            fill(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static GameObject CreateVisual(Transform parent, string objectName, PrimitiveType type,
            Vector3 scale, Vector3 eulerAngles, bool active)
        {
            GameObject visual = GameObject.CreatePrimitive(type);
            visual.name = objectName;

            // O colisor fica só no objeto raiz. O visual não pode ter colisor próprio.
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localEulerAngles = eulerAngles;
            visual.transform.localScale = scale;
            visual.SetActive(active);
            return visual;
        }

        private static void SetFloat(SerializedObject so, string property, float value) =>
            so.FindProperty(property).floatValue = value;

        private static void SetBool(SerializedObject so, string property, bool value) =>
            so.FindProperty(property).boolValue = value;

        private static void SetEnum(SerializedObject so, string property, int index) =>
            so.FindProperty(property).enumValueIndex = index;
    }
}
