using System.Collections.Generic;
using System.IO;
using System.Linq;
using TowerDefense.CameraSystem;
using TowerDefense.Core;
using TowerDefense.Grid;
using TowerDefense.Player;
using TowerDefense.Towers;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace TowerDefense.EditorTools
{
    /// <summary>
    /// 회색 박스 프로토타입 씬과 필요한 에셋을 만든다.
    /// 메뉴: Tools > Tower Defense > Create Prototype Scene
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";
        private const string AssetFolder = "Assets/Prototype";

        [MenuItem("Tools/Tower Defense/Create Prototype Scene")]
        public static void CreateFromMenu()
        {
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                    "프로토타입 씬", $"{ScenePath} 파일이 이미 있습니다. 덮어쓸까요?", "덮어쓰기", "취소"))
            {
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Create();
        }

        public static void Create()
        {
            Directory.CreateDirectory(AssetFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            AssetDatabase.Refresh();

            // 새 씬을 먼저 연다. (씬을 열 때 참조가 없는 에셋이 언로드될 수 있다)
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            const string lit = "Universal Render Pipeline/Lit";
            const string unlit = "Universal Render Pipeline/Unlit";
            Material ground = CreateMaterial("Ground", lit, new Color(0.35f, 0.37f, 0.35f));
            Material towerBody = CreateMaterial("Tower", lit, new Color(0.7f, 0.72f, 0.75f));
            Material playerBody = CreateMaterial("Player", lit, new Color(0.2f, 0.5f, 0.9f));
            Material gridLine = CreateMaterial("GridLine", unlit, new Color(0.75f, 0.75f, 0.75f));
            Material rangeLine = CreateMaterial("RangeLine", unlit, new Color(1f, 0.85f, 0.2f));
            Material valid = CreateMaterial("GhostValid", unlit, new Color(0.2f, 0.9f, 0.3f, 0.45f), true);
            Material invalid = CreateMaterial("GhostInvalid", unlit, new Color(0.95f, 0.2f, 0.2f, 0.45f), true);

            Tower towerPrefab = CreateTowerPrefab(towerBody);
            TowerDefinition basicTower = CreateTowerDefinition(towerPrefab);

            CreateLight();

            var systems = new GameObject("GameSystems");
            var modeController = systems.AddComponent<GameModeController>();
            var hud = systems.AddComponent<DebugHud>();
            Assign(hud, "modeController", modeController);

            var gridObject = new GameObject("BuildGrid");
            var grid = gridObject.AddComponent<BuildGrid>();

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Ground";
            // 기본 Plane은 10x10. 그리드 바깥에 여백을 둔다
            float floorScale = (Mathf.Max(grid.Width, grid.Height) * grid.CellSize + 10f) / 10f;
            floor.transform.localScale = new Vector3(floorScale, 1f, floorScale);
            floor.GetComponent<MeshRenderer>().sharedMaterial = ground;

            var overlayObject = new GameObject("GridOverlay");
            var overlay = overlayObject.AddComponent<GridOverlay>();
            overlayObject.GetComponent<MeshRenderer>().sharedMaterial = gridLine;
            Assign(overlay, "modeController", modeController);
            Assign(overlay, "grid", grid);

            // 플레이어
            var player = new GameObject("Player");
            player.transform.position = new Vector3(1f, 0.1f, -9f);
            var characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.4f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            GameObject body = CreateVisual(PrimitiveType.Capsule, "Body", player.transform,
                new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 0.9f, 0.8f), playerBody);
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var head = new GameObject("Head");
            head.transform.SetParent(player.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var firstPerson = player.AddComponent<FirstPersonController>();
            Assign(firstPerson, "modeController", modeController);
            Assign(firstPerson, "head", head.transform);

            // 카메라
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            var brain = cameraObject.AddComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.8f);

            var rig = new GameObject("TopDownRig");
            CinemachineCamera topDownCamera = CreateVirtualCamera("TopDownCamera", rig.transform, 45f, 0.3f, 20);
            var topDown = rig.AddComponent<TopDownCameraController>();
            Assign(topDown, "modeController", modeController);
            Assign(topDown, "cameraTransform", topDownCamera.transform);
            // 플레이 전에도 탑뷰 구도가 보이도록 초기 위치를 잡는다
            Quaternion topRotation = Quaternion.Euler(60f, 0f, 0f);
            topDownCamera.transform.localRotation = topRotation;
            topDownCamera.transform.localPosition = topRotation * new Vector3(0f, 0f, -28f);
            cameraObject.transform.SetPositionAndRotation(topDownCamera.transform.position, topRotation);

            CinemachineCamera firstPersonCamera = CreateVirtualCamera("FirstPersonCamera", head.transform, 70f, 0.1f, 0);

            var switcher = systems.AddComponent<CameraModeSwitcher>();
            Assign(switcher, "modeController", modeController);
            Assign(switcher, "topDownCamera", topDownCamera);
            Assign(switcher, "firstPersonCamera", firstPersonCamera);

            // 타워 배치
            var rangeObject = new GameObject("RangeIndicator");
            rangeObject.AddComponent<LineRenderer>();
            var rangeIndicator = rangeObject.AddComponent<RangeIndicator>();
            var line = rangeObject.GetComponent<LineRenderer>();
            line.sharedMaterial = rangeLine;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.enabled = false;

            var placerObject = new GameObject("Towers");
            var placer = placerObject.AddComponent<TowerPlacer>();
            Assign(placer, "modeController", modeController);
            Assign(placer, "grid", grid);
            Assign(placer, "viewCamera", mainCamera);
            Assign(placer, "player", player.transform);
            Assign(placer, "rangeIndicator", rangeIndicator);
            Assign(placer, "validMaterial", valid);
            Assign(placer, "invalidMaterial", invalid);
            AssignArray(placer, "towers", basicTower);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"프로토타입 씬 생성 완료: {ScenePath}");
        }

        private static Material CreateMaterial(string name, string shaderName, Color color, bool transparent = false)
        {
            string path = $"{AssetFolder}/{name}.mat";
            Shader shader = Shader.Find(shaderName);
            if (shader == null) throw new System.InvalidOperationException($"셰이더를 찾을 수 없습니다: {shaderName}");

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetColor("_BaseColor", color);
            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Tower CreateTowerPrefab(Material material)
        {
            var root = new GameObject("BasicTower");
            root.AddComponent<Tower>();
            var hitbox = root.AddComponent<BoxCollider>();
            hitbox.center = new Vector3(0f, 1.1f, 0f);
            hitbox.size = new Vector3(1.6f, 2.2f, 1.6f);

            GameObject baseBlock = CreateVisual(PrimitiveType.Cube, "Base", root.transform,
                new Vector3(0f, 0.5f, 0f), new Vector3(1.6f, 1f, 1.6f), material);
            GameObject column = CreateVisual(PrimitiveType.Cylinder, "Column", root.transform,
                new Vector3(0f, 1.45f, 0f), new Vector3(0.8f, 0.45f, 0.8f), material);
            GameObject barrel = CreateVisual(PrimitiveType.Cube, "Barrel", root.transform,
                new Vector3(0f, 1.9f, 0.45f), new Vector3(0.3f, 0.3f, 1.2f), material);
            foreach (GameObject part in new[] { baseBlock, column, barrel })
            {
                Object.DestroyImmediate(part.GetComponent<Collider>());
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{AssetFolder}/{root.name}.prefab");
            Object.DestroyImmediate(root);
            return prefab.GetComponent<Tower>();
        }

        private static TowerDefinition CreateTowerDefinition(Tower prefab)
        {
            string path = $"{AssetFolder}/BasicTower.asset";
            var definition = AssetDatabase.LoadAssetAtPath<TowerDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<TowerDefinition>();
                AssetDatabase.CreateAsset(definition, path);
                var serialized = new SerializedObject(definition);
                serialized.FindProperty("displayName").stringValue = "기본 포탑";
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Assign(definition, "prefab", prefab);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
        }

        private static GameObject CreateVisual(PrimitiveType type, string name, Transform parent,
            Vector3 position, Vector3 scale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position;
            visual.transform.localScale = scale;
            visual.GetComponent<MeshRenderer>().sharedMaterial = material;
            return visual;
        }

        private static CinemachineCamera CreateVirtualCamera(string name, Transform parent,
            float fieldOfView, float nearClip, int priority)
        {
            var cameraObject = new GameObject(name);
            cameraObject.transform.SetParent(parent, false);
            var virtualCamera = cameraObject.AddComponent<CinemachineCamera>();
            LensSettings lens = virtualCamera.Lens;
            lens.FieldOfView = fieldOfView;
            lens.NearClipPlane = nearClip;
            virtualCamera.Lens = lens;
            virtualCamera.Priority = priority;
            return virtualCamera;
        }

        private static void Assign(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignArray(Object target, string propertyName, params Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == path);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
