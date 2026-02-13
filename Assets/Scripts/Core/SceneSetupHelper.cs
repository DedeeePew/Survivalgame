#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SurvivalGame.Player;
using SurvivalGame.Interaction;
using SurvivalGame.Inventory;
using SurvivalGame.Items;
using SurvivalGame.UI;

namespace SurvivalGame.Core.Editor
{
    /// <summary>
    /// Editor helper: Creates test scenes using SEP (Survival Environment Pack) assets.
    /// Terrain uses SEP ground textures, trees/rocks/grass are SEP prefabs.
    /// Menu: SurvivalGame → Setup Test Scene (M1) / (M2)
    /// </summary>
    public static class SceneSetupHelper
    {
        // SEP Asset Paths
        private const string SEP = "Assets/PolymindGames/SEP/";
        private const string SEP_TERRAIN = SEP + "Terrain/Textures/";
        private const string SEP_ENV = SEP + "Prefabs/Environment/";
        private const string SEP_DECO = SEP + "Prefabs/Decoration/";
        private const string SEP_SKYBOX = SEP + "Models/Environment/Skybox/Skybox.mat";
        private const string SEP_BARK_MAT = SEP + "Models/Environment/Vegetation/Materials/Bark_PBR.mat";

        [MenuItem("SurvivalGame/Setup Test Scene (M1 - Movement Only)")]
        public static void SetupTestScene()
        {
            // ── 0) Clear existing scene ──
            var allRootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var rootObj in allRootObjects)
                Object.DestroyImmediate(rootObj);

            // ── 1) Terrain with SEP textures ──
            CreateTerrain();

            // ── 2) GameManager ──
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameBootstrapper>();
            gameManager.AddComponent<DebugUI>();

            // ── 3) Player ──
            CreatePlayer();

            // ── 4) Environment with SEP prefabs ──
            CreateEnvironment();

            // ── 5) Directional Light (sun) + Skybox ──
            CreateSunlight();
            ApplySkybox();

            // ── 6) Atmosphere ──
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.55f, 0.7f, 0.85f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 120f;

            var mainCam = Object.FindFirstObjectByType<Camera>();
            if (mainCam != null)
            {
                mainCam.backgroundColor = new Color(0.45f, 0.65f, 0.85f);
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.farClipPlane = 200f;
            }

            Debug.Log("[SceneSetupHelper] ✅ M1 Test Scene created with SEP assets!");
            EditorUtility.DisplayDialog("Scene Setup Complete",
                "M1 Test Scene mit SEP-Assets erstellt!\n\n" +
                "• SEP Terrain-Texturen\n" +
                "• SEP Bäume, Felsen, Gras\n" +
                "• SEP Skybox\n" +
                "• Realistische Hände\n\n" +
                "Press Play!",
                "OK");
        }

        // ══════════════════════════════════════
        // TERRAIN (SEP Textures)
        // ══════════════════════════════════════

        private static void CreateTerrain()
        {
            // Main ground with SEP grass texture
            var grassMat = LoadSEPTerrainMaterial("Ground_Grass", 30f);
            var dirtMat = LoadSEPTerrainMaterial("Ground_DirtWithGrass", 10f);
            var stonesMat = LoadSEPTerrainMaterial("Ground_Stones", 15f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10, 1, 10);
            ground.GetComponent<Renderer>().sharedMaterial = grassMat;

            // Dirt path
            GameObject path = GameObject.CreatePrimitive(PrimitiveType.Plane);
            path.name = "DirtPath";
            path.transform.position = new Vector3(0, 0.01f, 0);
            path.transform.localScale = new Vector3(0.5f, 1, 10);
            path.GetComponent<Renderer>().sharedMaterial = dirtMat;

            // Clearing
            GameObject clearing = GameObject.CreatePrimitive(PrimitiveType.Plane);
            clearing.name = "Clearing";
            clearing.transform.position = new Vector3(0, 0.01f, 5);
            clearing.transform.localScale = new Vector3(2, 1, 2);
            clearing.GetComponent<Renderer>().sharedMaterial = dirtMat;

            // Stone area
            GameObject stoneArea = GameObject.CreatePrimitive(PrimitiveType.Plane);
            stoneArea.name = "StoneArea";
            stoneArea.transform.position = new Vector3(12, 0.01f, -5);
            stoneArea.transform.localScale = new Vector3(1.5f, 1, 1.5f);
            stoneArea.GetComponent<Renderer>().sharedMaterial = stonesMat;

            // Hills (edges)
            CreateHill("Hill_N", new Vector3(0, 0.5f, 48), new Vector3(100, 2, 8), grassMat);
            CreateHill("Hill_S", new Vector3(0, 0.5f, -48), new Vector3(100, 2, 8), grassMat);
            CreateHill("Hill_E", new Vector3(48, 0.5f, 0), new Vector3(8, 2, 100), grassMat);
            CreateHill("Hill_W", new Vector3(-48, 0.5f, 0), new Vector3(8, 2, 100), grassMat);
        }

        private static Material LoadSEPTerrainMaterial(string name, float tiling)
        {
            string albedoPath = SEP_TERRAIN + name + ".png";
            string normalPath = SEP_TERRAIN + name + "_NRM.png";

            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

            if (albedo == null)
            {
                Debug.LogWarning($"[SEP] Terrain texture not found: {albedoPath}");
                return ProceduralTextureGenerator.CreateMaterial(
                    ProceduralTextureGenerator.GenerateGrassTexture());
            }

            // Create proper PBR material
            var mat = new Material(Shader.Find("Standard"));
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            mat.mainTexture = albedo;
            mat.mainTextureScale = new Vector2(tiling, tiling);

            if (normal != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
            }

            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", 0.15f);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", 0f);
            if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", 0.15f);

            mat.name = $"SEP_{name}";
            return mat;
        }

        private static void CreateHill(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var hill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hill.name = name;
            hill.transform.position = pos;
            hill.transform.localScale = scale;
            hill.GetComponent<Renderer>().sharedMaterial = mat;
        }

        // ══════════════════════════════════════
        // PLAYER
        // ══════════════════════════════════════

        private static void CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(0, 1.5f, 0);
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = Vector3.zero;

            GameObject camHolder = new GameObject("CameraHolder");
            camHolder.transform.SetParent(player.transform);
            camHolder.transform.localPosition = new Vector3(0, 0.7f, 0);

            Camera cam = camHolder.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 75f;
            cam.backgroundColor = new Color(0.45f, 0.65f, 0.85f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camHolder.AddComponent<AudioListener>();

            player.AddComponent<PlayerController>();
            player.AddComponent<InteractSystem>();
            player.AddComponent<PlayerVisuals>();
        }

        // ══════════════════════════════════════
        // ENVIRONMENT (SEP Prefabs)
        // ══════════════════════════════════════

        private static void CreateEnvironment()
        {
            GameObject envRoot = new GameObject("Environment");

            // ── SEP Trees ──
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree01.prefab", envRoot.transform,
                new Vector3(8, 0, 5), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree02.prefab", envRoot.transform,
                new Vector3(-6, 0, 8), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree01.prefab", envRoot.transform,
                new Vector3(12, 0, -3), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree02.prefab", envRoot.transform,
                new Vector3(-10, 0, 12), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree01.prefab", envRoot.transform,
                new Vector3(4, 0, 15), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree02.prefab", envRoot.transform,
                new Vector3(-8, 0, -6), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree01.prefab", envRoot.transform,
                new Vector3(15, 0, 10), RandomY(), Vector3.one * 1.2f);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree02.prefab", envRoot.transform,
                new Vector3(-14, 0, 4), RandomY(), Vector3.one * 0.9f);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree01.prefab", envRoot.transform,
                new Vector3(6, 0, -12), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree02.prefab", envRoot.transform,
                new Vector3(-4, 0, -10), RandomY(), Vector3.one * 1.1f);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree01.prefab", envRoot.transform,
                new Vector3(18, 0, -8), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Vegetation/SEP_Tree02.prefab", envRoot.transform,
                new Vector3(-16, 0, -2), RandomY(), Vector3.one * 0.85f);

            // Dead tree + stumps
            SpawnSEPPrefab(SEP_ENV + "Trees/SEP_DeadTree.prefab", envRoot.transform,
                new Vector3(20, 0, 5), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Trees/SEP_TreeStump.prefab", envRoot.transform,
                new Vector3(7, 0, 3), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Trees/SEP_TreeStump.prefab", envRoot.transform,
                new Vector3(-5, 0, -3), RandomY(), Vector3.one);

            // ── SEP Rocks ──
            SpawnSEPPrefab(SEP_ENV + "Rocks/RocksVersion2/SEP_Rock01.prefab", envRoot.transform,
                new Vector3(5, 0, -5), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Rocks/RocksVersion2/SEP_Rock02.prefab", envRoot.transform,
                new Vector3(-7, 0, 3), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Rocks/RocksVersion2/SEP_Rock03.prefab", envRoot.transform,
                new Vector3(10, 0, 8), RandomY(), Vector3.one * 1.3f);
            SpawnSEPPrefab(SEP_ENV + "Rocks/RocksVersion2/SEP_Rock04.prefab", envRoot.transform,
                new Vector3(-3, 0, -8), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Rocks/RocksVersion2/SEP_Rock05.prefab", envRoot.transform,
                new Vector3(14, 0, -6), RandomY(), Vector3.one);

            // Small rocks
            for (int i = 1; i <= 7; i++)
            {
                float rx = Random.Range(-18f, 18f);
                float rz = Random.Range(-18f, 18f);
                if (Mathf.Abs(rx) < 3 && Mathf.Abs(rz) < 3) continue;
                string rockPath = SEP_ENV + $"Rocks/RocksVersion1/SEP_SmallRock0{Mathf.Clamp(i, 1, 7)}.prefab";
                SpawnSEPPrefab(rockPath, envRoot.transform,
                    new Vector3(rx, 0, rz), RandomY(), Vector3.one);
            }

            // ── SEP Grass ──
            for (int i = 0; i < 25; i++)
            {
                float gx = Random.Range(-22f, 22f);
                float gz = Random.Range(-22f, 22f);
                string grassPrefab = (i % 2 == 0)
                    ? SEP_ENV + "Grasses/SEP_Grass01.prefab"
                    : SEP_ENV + "Grasses/SEP_Grass02.prefab";
                SpawnSEPPrefab(grassPrefab, envRoot.transform,
                    new Vector3(gx, 0, gz), RandomY(), Vector3.one);
            }

            // ── SEP Branches (fallen) ──
            SpawnSEPPrefab(SEP_ENV + "Branches/SEP_Branches01.prefab", envRoot.transform,
                new Vector3(6, 0, 4), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Branches/SEP_Branches02.prefab", envRoot.transform,
                new Vector3(-11, 0, 7), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_ENV + "Branches/SEP_Branches03.prefab", envRoot.transform,
                new Vector3(3, 0, -9), RandomY(), Vector3.one);

            // ── SEP Decoration: Logs, Ores ──
            SpawnSEPPrefab(SEP_DECO + "Logs/Static/SEP_Log_Static.prefab", envRoot.transform,
                new Vector3(-8, 0, 5), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_DECO + "Logs/Static/LogPiles/SEP_LogPile01_Static.prefab", envRoot.transform,
                new Vector3(9, 0, -2), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_DECO + "Logs/Static/LogPiles/SEP_LogPile02_Static.prefab", envRoot.transform,
                new Vector3(-12, 0, -5), RandomY(), Vector3.one);

            // Ore nodes
            SpawnSEPPrefab(SEP_DECO + "Ores/SEP_MetalOre.prefab", envRoot.transform,
                new Vector3(13, 0, -5), RandomY(), Vector3.one);
            SpawnSEPPrefab(SEP_DECO + "Ores/SEP_Stone.prefab", envRoot.transform,
                new Vector3(-15, 0, 8), RandomY(), Vector3.one);

            // Decoration
            SpawnSEPPrefab(SEP_DECO + "SEP_Crate.prefab", envRoot.transform,
                new Vector3(2, 0, 8), Quaternion.Euler(0, 15, 0), Vector3.one);
            SpawnSEPPrefab(SEP_DECO + "SEP_NormalBarrel.prefab", envRoot.transform,
                new Vector3(2.5f, 0, 7.5f), Quaternion.identity, Vector3.one);
        }

        private static GameObject SpawnSEPPrefab(string path, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[SEP] Prefab not found: {path}");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = pos;
            instance.transform.rotation = rot;
            instance.transform.localScale = scale;
            // Make static for batching
            instance.isStatic = true;
            return instance;
        }

        private static Quaternion RandomY()
        {
            return Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }

        // ══════════════════════════════════════
        // LIGHTING + SKYBOX
        // ══════════════════════════════════════

        private static void CreateSunlight()
        {
            GameObject light = new GameObject("Sun");
            var dirLight = light.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.intensity = 1.3f;
            dirLight.color = new Color(1f, 0.95f, 0.85f);
            light.transform.rotation = Quaternion.Euler(45, -30, 0);

            RenderSettings.ambientLight = new Color(0.35f, 0.4f, 0.45f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientIntensity = 1f;
        }

        private static void ApplySkybox()
        {
            var skyboxMat = AssetDatabase.LoadAssetAtPath<Material>(SEP_SKYBOX);
            if (skyboxMat != null)
            {
                RenderSettings.skybox = skyboxMat;
                Debug.Log("[SEP] Skybox applied!");

                // Use skybox for camera clear
                var cam = Object.FindFirstObjectByType<Camera>();
                if (cam != null)
                    cam.clearFlags = CameraClearFlags.Skybox;
            }
            else
            {
                Debug.LogWarning("[SEP] Skybox not found at: " + SEP_SKYBOX);
            }
        }

        // ══════════════════════════════════════
        // TEST INTERACTABLES
        // ══════════════════════════════════════

        private static void CreateTestCube(string name, Vector3 pos, Color color, string prompt)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = pos;
            cube.GetComponent<Renderer>().sharedMaterial =
                ProceduralTextureGenerator.CreateColorMaterial(color);

            var interactable = cube.AddComponent<TestInteractable>();
            var so = new SerializedObject(interactable);
            var promptProp = so.FindProperty("_prompt");
            if (promptProp != null)
            {
                promptProp.stringValue = prompt;
                so.ApplyModifiedProperties();
            }
        }

        // ══════════════════════════════════════
        // M2: Full Inventory Scene
        // ══════════════════════════════════════

        [MenuItem("SurvivalGame/Setup Test Scene (M2 - Inventory)")]
        public static void SetupM2Scene()
        {
            SetupTestScene();

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[SceneSetup] Player not found!");
                return;
            }

            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabase>("Assets/ScriptableObjects/ItemDatabase.asset");
            if (itemDatabase == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "ItemDatabase nicht gefunden!\n\n" +
                    "Bitte zuerst: SurvivalGame → Create Example Items (M2)",
                    "OK");
                return;
            }

            // Inventory
            var invController = player.AddComponent<InventoryController>();
            var soController = new SerializedObject(invController);
            var dbProp = soController.FindProperty("_itemDatabase");
            if (dbProp != null)
            {
                dbProp.objectReferenceValue = itemDatabase;
                soController.ApplyModifiedProperties();
            }

            player.AddComponent<HeldItemDisplay>();

            var gameManager = GameObject.Find("GameManager");
            if (gameManager != null)
            {
                gameManager.AddComponent<InventoryUI>();
                gameManager.AddComponent<HotbarUI>();
            }

            // Spawn WorldItems
            SpawnWorldItemInScene("item_wood", 5, new Vector3(7, 0.3f, 5.5f), itemDatabase);
            SpawnWorldItemInScene("item_wood", 3, new Vector3(-5.5f, 0.3f, 8.5f), itemDatabase);
            SpawnWorldItemInScene("item_stone", 3, new Vector3(5.5f, 0.3f, -4.5f), itemDatabase);
            SpawnWorldItemInScene("item_stone", 2, new Vector3(-6.5f, 0.3f, 3.5f), itemDatabase);
            SpawnWorldItemInScene("item_iron_ore", 2, new Vector3(10.5f, 0.3f, 8.5f), itemDatabase);
            SpawnWorldItemInScene("tool_stone_axe", 1, new Vector3(-1, 0.3f, 3), itemDatabase);
            SpawnWorldItemInScene("tool_stone_pickaxe", 1, new Vector3(2, 0.3f, -2), itemDatabase);
            SpawnWorldItemInScene("item_berries", 8, new Vector3(3.5f, 0.3f, 7.5f), itemDatabase);
            SpawnWorldItemInScene("item_berries", 5, new Vector3(-4.5f, 0.3f, 6.5f), itemDatabase);
            SpawnWorldItemInScene("item_raw_meat", 2, new Vector3(1, 0.3f, -5.5f), itemDatabase);
            SpawnWorldItemInScene("item_fiber", 6, new Vector3(-2, 0.3f, 4), itemDatabase);
            SpawnWorldItemInScene("item_plank", 3, new Vector3(4, 0.3f, 1), itemDatabase);

            Debug.Log("[SceneSetupHelper] ✅ M2 Scene mit SEP-Assets erstellt!");
            EditorUtility.DisplayDialog("M2 Scene Setup Complete",
                "M2 Test Scene mit SEP-Assets!\n\n" +
                "• SEP Terrain, Bäume, Felsen, Gras\n" +
                "• SEP Skybox\n" +
                "• SEP 3D-Modelle als World-Items\n" +
                "• Realistische Hände mit Grip\n" +
                "• SEP Sprites als Hotbar-Icons\n\n" +
                "Controls:\n" +
                "• 1-9 = Hotbar Slot\n" +
                "• TAB = Inventory\n" +
                "• E = Aufheben\n" +
                "• G = Drop\n\n" +
                "Press Play!",
                "OK");
        }

        private static void SpawnWorldItemInScene(string itemId, int amount, Vector3 pos, ItemDatabase db)
        {
            var itemDef = db.GetById(itemId);
            if (itemDef == null)
            {
                Debug.LogWarning($"[SceneSetup] Item not found in DB: {itemId}");
                return;
            }

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"WorldItem_{itemId}";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.3f;

            var worldItem = go.AddComponent<WorldItem>();
            var so = new SerializedObject(worldItem);

            var itemDefProp = so.FindProperty("_itemDef");
            if (itemDefProp != null)
                itemDefProp.objectReferenceValue = itemDef;

            var amountProp = so.FindProperty("_amount");
            if (amountProp != null)
                amountProp.intValue = amount;

            so.ApplyModifiedProperties();

            go.GetComponent<Renderer>().sharedMaterial =
                ProceduralTextureGenerator.CreateColorMaterial(itemDef.GetRarityColor());

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
        }
    }
}
#endif
