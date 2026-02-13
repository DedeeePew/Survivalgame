using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Items;

namespace SurvivalGame.Inventory
{
    /// <summary>
    /// Creates proper 3D visual representations for WorldItems based on their item type.
    /// Instead of plain colored cubes, items get recognizable shapes and textures.
    /// Attach to the same GameObject as WorldItem, or call SetupVisuals() from WorldItem.
    /// </summary>
    public static class WorldItemVisuals
    {
        /// <summary>
        /// Applies proper visuals (mesh, material, shape) to a WorldItem's GameObject.
        /// Uses SEP worldPrefab when available, falling back to procedural visuals.
        /// Call this after the item is spawned/created.
        /// </summary>
        public static void ApplyVisuals(GameObject go, ItemDef itemDef)
        {
            if (itemDef == null || go == null) return;

            // ── Try SEP worldPrefab first ──
            if (itemDef.worldPrefab != null)
            {
                ApplySEPVisual(go, itemDef);

                var floater = go.GetComponent<WorldItemFloater>();
                if (floater == null)
                    go.AddComponent<WorldItemFloater>();
                return;
            }

            string id = itemDef.id;

            // Remove existing mesh renderer/filter if we're replacing the visual
            var existingRenderer = go.GetComponent<MeshRenderer>();
            var existingFilter = go.GetComponent<MeshFilter>();

            switch (id)
            {
                case "item_wood":
                    SetupWoodVisual(go);
                    break;
                case "item_stone":
                    SetupStoneVisual(go);
                    break;
                case "item_iron_ore":
                    SetupOreVisual(go);
                    break;
                case "item_iron_ingot":
                    SetupIngotVisual(go);
                    break;
                case "item_plank":
                    SetupPlankVisual(go);
                    break;
                case "item_fiber":
                    SetupFiberVisual(go);
                    break;
                case "item_leather":
                    SetupLeatherVisual(go);
                    break;
                case "item_raw_meat":
                    SetupMeatVisual(go, false);
                    break;
                case "item_cooked_meat":
                    SetupMeatVisual(go, true);
                    break;
                case "item_berries":
                    SetupBerriesVisual(go);
                    break;
                case "tool_stone_axe":
                case "tool_iron_axe":
                    SetupAxeVisual(go, id.Contains("iron"));
                    break;
                case "tool_stone_pickaxe":
                case "tool_iron_pickaxe":
                    SetupPickaxeVisual(go, id.Contains("iron"));
                    break;
                case "tool_stone_sword":
                    SetupSwordVisual(go);
                    break;
                case "tool_build_hammer":
                    SetupHammerVisual(go);
                    break;
                default:
                    SetupDefaultVisual(go, itemDef);
                    break;
            }

            // Add a gentle floating/bobbing animation
            var floater2 = go.GetComponent<WorldItemFloater>();
            if (floater2 == null)
                go.AddComponent<WorldItemFloater>();
        }

        // ══════════════════════════════════════
        // RESOURCE VISUALS
        // ══════════════════════════════════════

        private static void SetupWoodVisual(GameObject go)
        {
            // Log shape: elongated cylinder-like cube with wood texture
            go.transform.localScale = new Vector3(0.15f, 0.15f, 0.5f);
            go.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), Random.Range(-10, 10));

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GenerateWoodTexture();
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }
        }

        private static void SetupStoneVisual(GameObject go)
        {
            // Rough stone: slightly irregular scale
            float s = Random.Range(0.2f, 0.3f);
            go.transform.localScale = new Vector3(s * 1.2f, s * 0.8f, s);
            go.transform.localRotation = Quaternion.Euler(Random.Range(-15, 15), Random.Range(0, 360), Random.Range(-15, 15));

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GenerateStoneTexture();
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }
        }

        private static void SetupOreVisual(GameObject go)
        {
            float s = Random.Range(0.2f, 0.3f);
            go.transform.localScale = new Vector3(s * 1.1f, s * 0.9f, s);
            go.transform.localRotation = Quaternion.Euler(Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10));

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GenerateOreTexture();
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }
        }

        private static void SetupIngotVisual(GameObject go)
        {
            // Flat rectangular bar
            go.transform.localScale = new Vector3(0.3f, 0.1f, 0.15f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GenerateMetalTexture();
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }
        }

        private static void SetupPlankVisual(GameObject go)
        {
            // Flat plank shape
            go.transform.localScale = new Vector3(0.4f, 0.05f, 0.15f);
            go.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GeneratePlankTexture();
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }
        }

        private static void SetupFiberVisual(GameObject go)
        {
            // Small bundle
            go.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GenerateFiberTexture();
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }
        }

        private static void SetupLeatherVisual(GameObject go)
        {
            // Flat piece of leather
            go.transform.localScale = new Vector3(0.3f, 0.03f, 0.25f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GenerateLeatherTexture();
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }
        }

        // ══════════════════════════════════════
        // FOOD VISUALS
        // ══════════════════════════════════════

        private static void SetupMeatVisual(GameObject go, bool cooked)
        {
            // Irregular meat chunk
            go.transform.localScale = new Vector3(0.2f, 0.1f, 0.18f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GenerateMeatTexture(64, cooked);
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }
        }

        private static void SetupBerriesVisual(GameObject go)
        {
            // Small cluster - using the main cube as a "bag"
            go.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = ProceduralTextureGenerator.GenerateBerryTexture();
                renderer.material = ProceduralTextureGenerator.CreateMaterial(tex);
            }

            // Add small berry spheres as children
            for (int i = 0; i < 3; i++)
            {
                var berry = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                berry.name = $"Berry_{i}";
                berry.transform.SetParent(go.transform);
                berry.transform.localPosition = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    Random.Range(0.4f, 0.8f),
                    Random.Range(-0.3f, 0.3f)
                );
                berry.transform.localScale = Vector3.one * 0.35f;
                berry.GetComponent<Renderer>().material =
                    ProceduralTextureGenerator.CreateColorMaterial(new Color(0.7f, 0.1f, 0.15f));
                Object.Destroy(berry.GetComponent<Collider>());
            }
        }

        // ══════════════════════════════════════
        // TOOL VISUALS
        // ══════════════════════════════════════

        private static void SetupAxeVisual(GameObject go, bool iron)
        {
            // Clear main object mesh - we'll build from children
            ClearMainMesh(go);
            go.transform.localScale = Vector3.one;

            Color headColor = iron ? new Color(0.6f, 0.6f, 0.65f) : new Color(0.5f, 0.48f, 0.45f);

            // Handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(go.transform);
            handle.transform.localPosition = new Vector3(0, 0, 0);
            handle.transform.localScale = new Vector3(0.04f, 0.04f, 0.4f);
            handle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateWoodTexture());
            Object.Destroy(handle.GetComponent<Collider>());

            // Axe head
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "AxeHead";
            head.transform.SetParent(go.transform);
            head.transform.localPosition = new Vector3(0.08f, 0, 0.15f);
            head.transform.localScale = new Vector3(0.15f, 0.03f, 0.12f);
            head.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateColorMaterial(headColor);
            Object.Destroy(head.GetComponent<Collider>());

            // Ensure main collider exists
            EnsureCollider(go, new Vector3(0.2f, 0.1f, 0.45f));
        }

        private static void SetupPickaxeVisual(GameObject go, bool iron)
        {
            ClearMainMesh(go);
            go.transform.localScale = Vector3.one;

            Color headColor = iron ? new Color(0.6f, 0.6f, 0.65f) : new Color(0.5f, 0.48f, 0.45f);

            // Handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(go.transform);
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localScale = new Vector3(0.04f, 0.04f, 0.4f);
            handle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateWoodTexture());
            Object.Destroy(handle.GetComponent<Collider>());

            // Pickaxe head (curved bar)
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "PickHead";
            head.transform.SetParent(go.transform);
            head.transform.localPosition = new Vector3(0, 0, 0.17f);
            head.transform.localScale = new Vector3(0.25f, 0.03f, 0.05f);
            head.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateColorMaterial(headColor);
            Object.Destroy(head.GetComponent<Collider>());

            EnsureCollider(go, new Vector3(0.3f, 0.1f, 0.45f));
        }

        private static void SetupSwordVisual(GameObject go)
        {
            ClearMainMesh(go);
            go.transform.localScale = Vector3.one;

            // Handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(go.transform);
            handle.transform.localPosition = new Vector3(0, 0, -0.12f);
            handle.transform.localScale = new Vector3(0.035f, 0.035f, 0.12f);
            handle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateColorMaterial(new Color(0.35f, 0.2f, 0.1f));
            Object.Destroy(handle.GetComponent<Collider>());

            // Guard
            var guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guard.name = "Guard";
            guard.transform.SetParent(go.transform);
            guard.transform.localPosition = new Vector3(0, 0, -0.05f);
            guard.transform.localScale = new Vector3(0.12f, 0.03f, 0.02f);
            guard.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateColorMaterial(new Color(0.4f, 0.3f, 0.15f));
            Object.Destroy(guard.GetComponent<Collider>());

            // Blade
            var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(go.transform);
            blade.transform.localPosition = new Vector3(0, 0, 0.15f);
            blade.transform.localScale = new Vector3(0.06f, 0.015f, 0.35f);
            blade.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateColorMaterial(new Color(0.55f, 0.53f, 0.5f));
            Object.Destroy(blade.GetComponent<Collider>());

            EnsureCollider(go, new Vector3(0.12f, 0.05f, 0.55f));
        }

        private static void SetupHammerVisual(GameObject go)
        {
            ClearMainMesh(go);
            go.transform.localScale = Vector3.one;

            // Handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(go.transform);
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localScale = new Vector3(0.04f, 0.04f, 0.35f);
            handle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateWoodTexture());
            Object.Destroy(handle.GetComponent<Collider>());

            // Hammer head
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "HammerHead";
            head.transform.SetParent(go.transform);
            head.transform.localPosition = new Vector3(0, 0, 0.15f);
            head.transform.localScale = new Vector3(0.1f, 0.1f, 0.08f);
            head.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateStoneTexture());
            Object.Destroy(head.GetComponent<Collider>());

            EnsureCollider(go, new Vector3(0.15f, 0.15f, 0.4f));
        }

        private static void SetupDefaultVisual(GameObject go, ItemDef itemDef)
        {
            // Default: textured cube with rarity color tint
            go.transform.localScale = Vector3.one * 0.25f;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = ProceduralTextureGenerator.CreateColorMaterial(itemDef.GetRarityColor());
            }
        }

        // ══════════════════════════════════════
        // SEP PREFAB VISUAL
        // ══════════════════════════════════════

        private static void ApplySEPVisual(GameObject go, ItemDef itemDef)
        {
            // Hide the original cube mesh
            ClearMainMesh(go);
            go.transform.localScale = Vector3.one;

            // Instantiate the SEP prefab as child
            var prefabInstance = Object.Instantiate(itemDef.worldPrefab, go.transform);
            prefabInstance.name = "SEPVisual";
            prefabInstance.transform.localPosition = Vector3.zero;
            prefabInstance.transform.localRotation = Quaternion.identity;
            prefabInstance.transform.localScale = Vector3.one;

            // Remove physics from the prefab child (parent has them)
            foreach (var col in prefabInstance.GetComponentsInChildren<Collider>())
                Object.Destroy(col);
            foreach (var rb in prefabInstance.GetComponentsInChildren<Rigidbody>())
                Object.Destroy(rb);

            // Ensure parent still has a collider for interaction
            if (go.GetComponent<Collider>() == null)
            {
                // Calculate bounds from renderers
                var renderers = prefabInstance.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                        bounds.Encapsulate(renderers[i].bounds);

                    var box = go.AddComponent<BoxCollider>();
                    box.center = go.transform.InverseTransformPoint(bounds.center);
                    box.size = bounds.size;
                }
                else
                {
                    var box = go.AddComponent<BoxCollider>();
                    box.size = Vector3.one * 0.3f;
                }
            }
        }

        // ══════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════

        private static void ClearMainMesh(GameObject go)
        {
            var meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;

            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null) meshFilter.mesh = null;
        }

        private static void EnsureCollider(GameObject go, Vector3 size)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            box.center = new Vector3(0, size.y * 0.5f, 0);
        }
    }

    /// <summary>
    /// Makes a world item gently float/bob up and down and slowly rotate.
    /// Adds a glowing effect so items are easier to spot.
    /// </summary>
    public class WorldItemFloater : MonoBehaviour
    {
        private float _startY;
        private float _bobOffset;
        private float _rotateSpeed;
        private bool _initialized;

        // Glow
        private Light _glowLight;

        private void Start()
        {
            _startY = transform.position.y;
            _bobOffset = Random.Range(0f, Mathf.PI * 2f);
            _rotateSpeed = Random.Range(20f, 40f);

            // Only float after rigidbody settles
            Invoke(nameof(EnableFloating), 2f);

            // Add a subtle point light for item glow
            var lightGO = new GameObject("ItemGlow");
            lightGO.transform.SetParent(transform);
            lightGO.transform.localPosition = Vector3.up * 0.2f;
            _glowLight = lightGO.AddComponent<Light>();
            _glowLight.type = LightType.Point;
            _glowLight.range = 1.5f;
            _glowLight.intensity = 0.4f;
            _glowLight.color = new Color(1f, 0.9f, 0.6f); // Warm glow
        }

        private void EnableFloating()
        {
            _initialized = true;
            _startY = transform.position.y;

            // Disable rigidbody physics once settled
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }

        private void Update()
        {
            if (!_initialized) return;

            // Gentle bob
            float bob = Mathf.Sin(Time.time * 1.5f + _bobOffset) * 0.05f;
            var pos = transform.position;
            pos.y = _startY + 0.15f + bob; // Slight hover
            transform.position = pos;

            // Slow rotation
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime, Space.World);

            // Pulsing glow
            if (_glowLight != null)
            {
                _glowLight.intensity = 0.3f + Mathf.Sin(Time.time * 2f + _bobOffset) * 0.15f;
            }
        }
    }
}
