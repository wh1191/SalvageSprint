using System;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SalvageSprint.Editor
{
    // Source-generated scenes make the handoff inspectable without hand-written scene YAML.
    // Generation runs in the Editor. Gameplay does not construct or bake the level at runtime.
    public static class CourseSceneBuilder
    {
        private const string Root = "Assets/SalvageSprint/Generated";
        private const int GeometryLayer = 8, BlockerLayer = 9;
        private static readonly string[] Names =
        { "01_Normal", "02_Cargo", "03_Occlusion", "04_TargetTaken", "05_Unreachable", "06_NoSalvage" };

        [MenuItem("Tools/Salvage Sprint/Create Course Scenes")]
        public static void CreatePreviewScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play mode before creating scenes.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Materials");
            Directory.CreateDirectory(Root + "/Settings");
            AssetDatabase.Refresh();
            ConfigureLayers();
            ConfigureRendering();
            PlayerSettings.companyName = "Futuregames Course";
            PlayerSettings.productName = "Salvage Sprint";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 800;
            for (int i = 0; i < Names.Length; i++)
            {
                string path = Root + "/Scenes/" + Names[i] + ".unity";
                if (File.Exists(path)) continue; // Never overwrite student/teacher scene edits.
                BuildScene(i, path);
            }
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string name in Names)
                scenes.Add(new EditorBuildSettingsScene(Root + "/Scenes/" + name + ".unity", true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Root + "/Scenes/01_Normal.unity");
            Debug.Log("Salvage Sprint: six course scenes created. Press Play. The README contains your next assignment stage.");
        }

        private static void ConfigureLayers()
        {
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = settings.FindProperty("layers");
            SetLayer(layers, GeometryLayer, "CourseGeometry");
            SetLayer(layers, BlockerLayer, "CourseSightBlocker");
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetLayer(SerializedProperty layers, int index, string name)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue != name)
                throw new InvalidOperationException("Layer " + index + " is already occupied. Use the supplied isolated project.");
            layer.stringValue = name;
        }

        private static void ConfigureRendering()
        {
            string rendererPath = Root + "/Settings/CourseRenderer.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }
            string pipelinePath = Root + "/Settings/CourseURP.asset";
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.msaaSampleCount = 4;
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            // Remove quality-level overrides so every course machine uses the same pipeline.
            int previous = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            { QualitySettings.SetQualityLevel(i, false); QualitySettings.renderPipeline = pipeline; }
            QualitySettings.SetQualityLevel(previous, false);
        }

        private static void BuildScene(int scenario, string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.5f, .57f, .65f);
            var lighting = new GameObject("Sun").AddComponent<Light>();
            lighting.type = LightType.Directional;
            lighting.intensity = 1.25f;
            lighting.transform.rotation = Quaternion.Euler(55, -30, 0);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 28, -16);
            camera.transform.rotation = Quaternion.Euler(60, 0, 0);
            camera.orthographic = true;
            camera.orthographicSize = scenario == 4 ? 19 : 17;
            camera.backgroundColor = new Color(.035f, .05f, .08f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<AudioListener>();

            var world = new GameObject("Yard Geometry");
            var surface = world.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = (1 << GeometryLayer) | (1 << BlockerLayer);
            Material floor = MaterialAsset("Floor", new Color(.12f, .17f, .22f));
            Material wall = MaterialAsset("Wall", new Color(.25f, .34f, .4f));
            Material gold = MaterialAsset("Scrap", new Color(1, .65f, .12f));
            Material cyan = MaterialAsset("Player", new Color(.1f, .8f, 1));
            Material coral = MaterialAsset("Rival", new Color(1, .3f, .22f));
            Material line = MaterialAsset("Markings", new Color(.3f, .49f, .58f));
            Box("Floor", new Vector3(0, -.2f, 0), new Vector3(28, .4f, 22), floor, world.transform, GeometryLayer);
            Box("North wall", new Vector3(0, 1.1f, 11), new Vector3(28, 2.2f, .5f), wall, world.transform, BlockerLayer);
            Box("South wall", new Vector3(0, 1.1f, -11), new Vector3(28, 2.2f, .5f), wall, world.transform, BlockerLayer);
            Box("West wall", new Vector3(-14, 1.1f, 0), new Vector3(.5f, 2.2f, 22), wall, world.transform, BlockerLayer);
            // A low east fence in the disconnected case blocks walking, but not eye-height sight.
            Box("East wall", new Vector3(14, scenario == 4 ? .35f : 1.1f, 0),
                new Vector3(.5f, scenario == 4 ? .7f : 2.2f, 22), wall, world.transform,
                scenario == 4 ? GeometryLayer : BlockerLayer);
            Box("West shelving", new Vector3(-4, 1.1f, 0), new Vector3(1.2f, 2.2f, 9), wall, world.transform, BlockerLayer);
            Box("East shelving", new Vector3(4, 1.1f, 0), new Vector3(1.2f, 2.2f, 9), wall, world.transform, BlockerLayer);
            Box("Centre cover", new Vector3(0, 1.1f, 3), new Vector3(3.5f, 2.2f, 1), wall, world.transform, BlockerLayer);
            if (scenario == 2)
                Box("Occlusion test wall", new Vector3(8, 1.1f, 7), new Vector3(.5f, 2.2f, 3), wall, world.transform, BlockerLayer);
            if (scenario == 4)
                Box("Disconnected platform", new Vector3(17, -.2f, 7), new Vector3(3, .4f, 4), floor, world.transform, GeometryLayer);

            // Decorative markings are excluded from navigation and have no colliders.
            for (int z = -8; z <= 8; z += 4)
                VisualBox("Lane marking", new Vector3(0, .01f, z), new Vector3(1.4f, .02f, .12f), line, null);
            Transform playerDepot = Depot("Player depot", new Vector3(-11, 0, -7), cyan);
            Transform rivalDepot = Depot("Rival depot", new Vector3(11, 0, 7), coral);
            var system = new GameObject("Match and HUD");
            MatchController match = system.AddComponent<MatchController>();
            RaceActor player = Actor("Player", new Vector3(-11, 0, -7), cyan, match, 0, playerDepot, gold, false);
            player.gameObject.AddComponent<PlayerDriver>().Configure(match);
            RaceActor rival = Actor("Rival", scenario == 1 ? new Vector3(11, 0, -1) : new Vector3(11, 0, 7),
                coral, match, 1, rivalDepot, gold, true);
            rival.gameObject.AddComponent<Navigator>();
            rival.gameObject.AddComponent<PickupSensor>().Configure(match);

            var points = new List<Transform>();
            foreach (Vector3 p in new[] { new Vector3(10, 0, -7), new Vector3(0, 0, -8),
                new Vector3(-10, 0, -5), new Vector3(-10, 0, 7), new Vector3(0, 0, 8), new Vector3(10, 0, 7) })
            { var point = new GameObject("Survey Point " + points.Count); point.transform.position = p; points.Add(point.transform); }

            Vector3[] positions = PickupPositions(scenario);
            var items = new List<SalvagePickup>();
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject item = VisualBox("Scrap " + i, positions[i] + Vector3.up * .4f,
                    new Vector3(.65f, .65f, .65f), gold, null);
                item.transform.rotation = Quaternion.Euler(0, 25, 0);
                SalvagePickup pickup = item.AddComponent<SalvagePickup>();
                pickup.Configure(i); items.Add(pickup);
            }
            match.Configure(new[] { player, rival }, items.ToArray(), 1 << BlockerLayer,
                Names[scenario], scenario == 5 ? 20 : 120, scenario == 1);
            RivalBrain brain = rival.gameObject.AddComponent<RivalBrain>();
            brain.Configure(match, points.ToArray());
            system.AddComponent<RaceHud>().Configure(match, brain);
            system.AddComponent<ScenarioControls>().Configure(match, brain, scenario == 3);

            // Save first so Unity can resolve scene-local asset references, then persist the bake.
            EditorSceneManager.SaveScene(scene, scenePath);
            Physics.SyncTransforms();
            surface.BuildNavMesh();
            if (surface.navMeshData == null)
                throw new InvalidOperationException("NavMesh bake returned no data for " + scenePath);
            string dataPath = Root + "/Settings/" + Names[scenario] + "_NavMesh.asset";
            AssetDatabase.CreateAsset(surface.navMeshData, dataPath);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static Vector3[] PickupPositions(int scenario)
        {
            if (scenario == 5) return Array.Empty<Vector3>();
            if (scenario == 2) return new[] { new Vector3(6, 0, 7), new Vector3(11, 0, -1) };
            if (scenario == 3) return new[] { new Vector3(5, 0, 8), new Vector3(10, 0, -3), new Vector3(-8, 0, 7) };
            if (scenario == 4) return new[] { new Vector3(17, 0, 7), new Vector3(11, 0, -1) };
            return new[] { new Vector3(-10, 0, -3), new Vector3(-10, 0, 2), new Vector3(-8, 0, 7),
                new Vector3(-6, 0, -7), new Vector3(-1, 0, -7), new Vector3(1, 0, -3), new Vector3(0, 0, 0),
                new Vector3(1, 0, 7), new Vector3(7, 0, -7), new Vector3(10, 0, -3), new Vector3(10, 0, 3),
                new Vector3(6, 0, 7), new Vector3(0, 0, 9), new Vector3(-10, 0, 8) };
        }

        private static RaceActor Actor(string name, Vector3 position, Material material, MatchController match,
            int team, Transform depot, Material cargo, bool navigation)
        {
            var root = new GameObject(name); root.transform.position = position;
            GameObject body = VisualBox("Body", Vector3.zero, new Vector3(.9f, .6f, 1.1f), material, root.transform);
            body.transform.localPosition = Vector3.up * .65f;
            GameObject nose = VisualBox("Front", Vector3.zero, new Vector3(.4f, .22f, .4f), cargo, root.transform);
            nose.transform.localPosition = new Vector3(0, .7f, .65f);
            GameObject carried = VisualBox("Carried scrap", Vector3.zero, Vector3.one * .45f, cargo, root.transform);
            carried.transform.localPosition = new Vector3(0, 1.25f, 0); carried.SetActive(false);
            RaceActor actor = root.AddComponent<RaceActor>(); actor.Configure(match, team, depot, carried);
            if (navigation)
            {
                NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
                agent.radius = .45f; agent.height = 1.5f; agent.baseOffset = 0;
                agent.speed = 3.4f; agent.acceleration = 10; agent.angularSpeed = 360;
                agent.stoppingDistance = .6f;
            }
            else
            {
                CharacterController controller = root.AddComponent<CharacterController>();
                controller.center = new Vector3(0, .75f, 0); controller.height = 1.5f;
                controller.radius = .45f; controller.stepOffset = .25f;
            }
            return actor;
        }

        private static Transform Depot(string name, Vector3 position, Material material)
        {
            GameObject pad = VisualBox(name, position + Vector3.up * .025f, new Vector3(3, .05f, 3), material, null);
            // Use scene names/colours instead of font assets or a text-rendering dependency.
            return pad.transform;
        }

        private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material,
            Transform parent, int layer)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent); go.transform.position = position;
            go.transform.localScale = scale; go.layer = layer;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static GameObject VisualBox(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            GameObject go = Box(name, position, scale, material, parent, 0);
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static Material MaterialAsset(string name, Color colour)
        {
            string path = Root + "/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing. Wait for package import to finish.");
            material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", colour);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
