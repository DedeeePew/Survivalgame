using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Inventory;
using SurvivalGame.Items;
using SurvivalGame.UI;

namespace SurvivalGame.Player
{
    /// <summary>
    /// Displays the currently selected hotbar item in the player's right hand.
    /// Uses SEP prefabs when available (heldPrefab / worldPrefab on ItemDef),
    /// falling back to improved procedural visuals.
    /// Integrates with PlayerVisuals grip system for realistic hand poses.
    /// Items are attached to the hand's GripPoint with per-item offset/rotation.
    /// </summary>
    public class HeldItemDisplay : MonoBehaviour
    {
        [Header("Fallback Position (if no GripPoint)")]
        [SerializeField] private Vector3 _fallbackPos = new Vector3(0.3f, -0.25f, 0.5f);
        [SerializeField] private Vector3 _fallbackRot = new Vector3(10f, -20f, 5f);

        [Header("Animation")]
        [SerializeField] private float _swayAmount = 0.003f;
        [SerializeField] private float _bobAmount = 0.002f;
        [SerializeField] private float _equipSpeed = 8f;

        private Transform _cameraHolder;
        private PlayerVisuals _playerVisuals;
        private GameObject _heldItemRoot;
        private GameObject _currentHeldVisual;
        private string _currentItemId = "";
        private HotbarUI _hotbar;
        private InventoryController _inventoryController;
        private float _bobTimer;

        // Equip animation
        private float _equipLerp = 1f;
        private Vector3 _equipStartPos;
        private Vector3 _equipTargetPos;

        private void Start()
        {
            _cameraHolder = GetComponentInChildren<Camera>()?.transform;
            _playerVisuals = GetComponent<PlayerVisuals>();
            _hotbar = FindFirstObjectByType<HotbarUI>();
            _inventoryController = GetComponent<InventoryController>();

            if (_cameraHolder == null)
            {
                Debug.LogWarning("[HeldItemDisplay] No camera found!");
                return;
            }

            // Subscribe to events
            if (_hotbar != null)
                _hotbar.OnSlotChanged += OnHotbarSlotChanged;

            if (_inventoryController != null && _inventoryController.Model != null)
                _inventoryController.Model.OnChanged += OnInventoryChanged;
        }

        private void OnDestroy()
        {
            if (_hotbar != null)
                _hotbar.OnSlotChanged -= OnHotbarSlotChanged;

            if (_inventoryController != null && _inventoryController.Model != null)
                _inventoryController.Model.OnChanged -= OnInventoryChanged;
        }

        private void LateUpdate()
        {
            CheckForItemChange();

            if (_currentHeldVisual == null) return;

            // Equip animation (item raises into view)
            if (_equipLerp < 1f)
            {
                _equipLerp += Time.deltaTime * _equipSpeed;
                _equipLerp = Mathf.Clamp01(_equipLerp);

                float t = EaseOutBack(_equipLerp);
                _currentHeldVisual.transform.localPosition = Vector3.Lerp(_equipStartPos, _equipTargetPos, t);
            }

            // Subtle sway/bob on the held item itself
            var pc = GetComponent<PlayerController>();
            float speed = pc != null ? pc.CurrentSpeed : 0;
            bool isMoving = speed > 0.5f;

            if (isMoving)
                _bobTimer += Time.deltaTime * (pc.IsSprinting ? 10f : 7f);
            else
                _bobTimer += Time.deltaTime * 1.5f;

            float bobY = Mathf.Sin(_bobTimer) * (isMoving ? _bobAmount * 2f : _bobAmount);
            float swayX = Mathf.Cos(_bobTimer * 0.7f) * _swayAmount;

            if (_equipLerp >= 1f && _currentHeldVisual != null)
            {
                _currentHeldVisual.transform.localPosition = _equipTargetPos + new Vector3(swayX, bobY, 0);
            }
        }

        private void CheckForItemChange()
        {
            if (_hotbar == null)
            {
                _hotbar = FindFirstObjectByType<HotbarUI>();
                if (_hotbar != null)
                    _hotbar.OnSlotChanged += OnHotbarSlotChanged;
            }

            var stack = _hotbar?.SelectedStack;
            string newId = stack?.ItemDef?.id ?? "";

            if (newId != _currentItemId)
            {
                _currentItemId = newId;
                UpdateHeldVisual(stack);
            }
        }

        private void OnHotbarSlotChanged(int slot)
        {
            var stack = _hotbar?.SelectedStack;
            string newId = stack?.ItemDef?.id ?? "";
            _currentItemId = newId;
            UpdateHeldVisual(stack);
        }

        private void OnInventoryChanged()
        {
            var stack = _hotbar?.SelectedStack;
            string newId = stack?.ItemDef?.id ?? "";
            if (newId != _currentItemId)
            {
                _currentItemId = newId;
                UpdateHeldVisual(stack);
            }
        }

        private void UpdateHeldVisual(ItemStack stack)
        {
            // Destroy old visual
            if (_currentHeldVisual != null)
            {
                Destroy(_currentHeldVisual);
                _currentHeldVisual = null;
            }

            // Reset grip to relaxed when no item
            if (stack == null || stack.IsEmpty)
            {
                if (_playerVisuals != null)
                {
                    _playerVisuals.SetRightGrip(PlayerVisuals.GripState.Relaxed);
                    _playerVisuals.SetLeftGrip(PlayerVisuals.GripState.Relaxed);
                }
                return;
            }

            var itemDef = stack.ItemDef;

            // Determine grip type based on item
            var gripType = GetGripType(itemDef);

            // Set hand grip
            if (_playerVisuals != null)
            {
                _playerVisuals.SetRightGrip(gripType);
            }

            // Try to use prefab (heldPrefab > worldPrefab > procedural)
            GameObject visual = null;

            if (itemDef.heldPrefab != null)
            {
                visual = Instantiate(itemDef.heldPrefab);
                visual.name = $"Held_{itemDef.id}";
                StripPhysics(visual);
            }
            else if (itemDef.worldPrefab != null)
            {
                visual = Instantiate(itemDef.worldPrefab);
                visual.name = $"Held_{itemDef.id}";
                StripPhysics(visual);
            }
            else
            {
                // Procedural fallback
                visual = CreateProceduralHeld(itemDef);
            }

            if (visual == null) return;

            // Attach to grip point or fallback position
            Transform attachPoint = _playerVisuals?.RightGripPoint;

            if (attachPoint != null)
            {
                visual.transform.SetParent(attachPoint);
            }
            else
            {
                // Fallback: attach to camera
                if (_heldItemRoot == null)
                {
                    _heldItemRoot = new GameObject("HeldItemFallback");
                    _heldItemRoot.transform.SetParent(_cameraHolder);
                    _heldItemRoot.transform.localPosition = _fallbackPos;
                    _heldItemRoot.transform.localRotation = Quaternion.Euler(_fallbackRot);
                }
                visual.transform.SetParent(_heldItemRoot.transform);
            }

            // Apply per-item grip offset/rotation/scale
            var gripData = GetGripData(itemDef);
            Vector3 posOffset = (itemDef.heldPositionOffset != Vector3.zero) ? itemDef.heldPositionOffset : gripData.posOffset;
            Vector3 rotOffset = (itemDef.heldRotationOffset != Vector3.zero) ? itemDef.heldRotationOffset : gripData.rotOffset;
            float scale = (itemDef.heldScale > 0.01f && itemDef.heldScale != 1f) ? itemDef.heldScale : gripData.scale;

            visual.transform.localRotation = Quaternion.Euler(rotOffset);
            visual.transform.localScale = Vector3.one * scale;

            // Equip animation: item comes from below
            _equipTargetPos = posOffset;
            _equipStartPos = posOffset + new Vector3(0, -0.15f, -0.05f);
            visual.transform.localPosition = _equipStartPos;
            _equipLerp = 0f;

            _currentHeldVisual = visual;
        }

        // ══════════════════════════════════════
        // GRIP DATA PER ITEM
        // ══════════════════════════════════════

        private struct GripData
        {
            public Vector3 posOffset;
            public Vector3 rotOffset;
            public float scale;

            public GripData(Vector3 pos, Vector3 rot, float s)
            {
                posOffset = pos;
                rotOffset = rot;
                scale = s;
            }
        }

        private PlayerVisuals.GripState GetGripType(ItemDef itemDef)
        {
            if (itemDef is ToolDef tool)
            {
                return tool.toolType switch
                {
                    ToolType.Axe => PlayerVisuals.GripState.Fist,
                    ToolType.Pickaxe => PlayerVisuals.GripState.Fist,
                    ToolType.Hammer => PlayerVisuals.GripState.Fist,
                    ToolType.Sword => PlayerVisuals.GripState.Fist,
                    ToolType.Knife => PlayerVisuals.GripState.Fist,
                    _ => PlayerVisuals.GripState.Fist,
                };
            }

            if (itemDef.HasTag("food"))
                return PlayerVisuals.GripState.Palm;

            if (itemDef.HasTag("resource"))
                return PlayerVisuals.GripState.Palm;

            return PlayerVisuals.GripState.Palm;
        }

        private GripData GetGripData(ItemDef itemDef)
        {
            string id = itemDef.id;

            // Per-item grip positioning for SEP prefabs and procedural items
            return id switch
            {
                // ── Tools (held by handle, tilted forward) ──
                "tool_stone_axe" => new GripData(
                    new Vector3(0f, 0f, 0.02f),
                    new Vector3(-30f, 90f, 0f),
                    0.35f),

                "tool_iron_axe" => new GripData(
                    new Vector3(0f, 0f, 0.02f),
                    new Vector3(-30f, 90f, 0f),
                    0.35f),

                "tool_stone_pickaxe" => new GripData(
                    new Vector3(0f, 0f, 0.02f),
                    new Vector3(-30f, 90f, 0f),
                    0.35f),

                "tool_iron_pickaxe" => new GripData(
                    new Vector3(0f, 0f, 0.02f),
                    new Vector3(-30f, 90f, 0f),
                    0.35f),

                "tool_stone_sword" => new GripData(
                    new Vector3(0f, 0.01f, 0.04f),
                    new Vector3(-25f, 90f, 0f),
                    0.35f),

                "tool_build_hammer" => new GripData(
                    new Vector3(0f, 0f, 0.02f),
                    new Vector3(-30f, 90f, 0f),
                    0.35f),

                // ── Resources (held on palm, more flat) ──
                "item_wood" => new GripData(
                    new Vector3(0f, 0.02f, 0.01f),
                    new Vector3(0f, 0f, 15f),
                    0.25f),

                "item_stone" => new GripData(
                    new Vector3(0f, 0.02f, 0f),
                    new Vector3(0f, 0f, 0f),
                    0.3f),

                "item_iron_ore" => new GripData(
                    new Vector3(0f, 0.02f, 0f),
                    new Vector3(0f, 0f, 0f),
                    0.3f),

                "item_iron_ingot" => new GripData(
                    new Vector3(0f, 0.02f, 0f),
                    new Vector3(0f, 0f, 0f),
                    0.25f),

                "item_plank" => new GripData(
                    new Vector3(0f, 0.02f, 0.01f),
                    new Vector3(0f, 0f, 10f),
                    0.25f),

                "item_fiber" => new GripData(
                    new Vector3(0f, 0.02f, 0f),
                    new Vector3(0f, 0f, 0f),
                    0.3f),

                "item_leather" => new GripData(
                    new Vector3(0f, 0.02f, 0f),
                    new Vector3(0f, 0f, 0f),
                    0.3f),

                // ── Food (held casually in palm) ──
                "item_raw_meat" or "item_cooked_meat" => new GripData(
                    new Vector3(0f, 0.02f, 0.01f),
                    new Vector3(0f, 0f, 0f),
                    0.3f),

                "item_berries" => new GripData(
                    new Vector3(0f, 0.02f, 0f),
                    new Vector3(0f, 0f, 0f),
                    0.3f),

                // ── Default ──
                _ => new GripData(
                    new Vector3(0f, 0.01f, 0f),
                    new Vector3(0f, 0f, 0f),
                    0.3f),
            };
        }

        // ══════════════════════════════════════
        // UTILITY
        // ══════════════════════════════════════

        private void StripPhysics(GameObject obj)
        {
            // Remove all colliders and rigidbodies from prefab
            foreach (var col in obj.GetComponentsInChildren<Collider>())
                Destroy(col);
            foreach (var rb in obj.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        // ══════════════════════════════════════
        // PROCEDURAL FALLBACK (improved visuals)
        // ══════════════════════════════════════

        private GameObject CreateProceduralHeld(ItemDef itemDef)
        {
            string id = itemDef.id;

            return id switch
            {
                "item_wood" => CreateWoodHeld(),
                "item_stone" => CreateStoneHeld(),
                "item_iron_ore" => CreateOreHeld(),
                "item_iron_ingot" => CreateIngotHeld(),
                "item_plank" => CreatePlankHeld(),
                "item_fiber" => CreateFiberHeld(),
                "item_leather" => CreateLeatherHeld(),
                "item_raw_meat" or "item_cooked_meat" => CreateMeatHeld(id == "item_cooked_meat"),
                "item_berries" => CreateBerriesHeld(),
                "tool_stone_axe" or "tool_iron_axe" => CreateAxeHeld(id.Contains("iron")),
                "tool_stone_pickaxe" or "tool_iron_pickaxe" => CreatePickaxeHeld(id.Contains("iron")),
                "tool_stone_sword" => CreateSwordHeld(),
                "tool_build_hammer" => CreateHammerHeld(),
                _ => CreateDefaultHeld(itemDef)
            };
        }

        // ── Procedural resource items ──

        private GameObject CreateWoodHeld()
        {
            var root = new GameObject("HeldWood");
            var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.transform.SetParent(root.transform);
            log.transform.localPosition = Vector3.zero;
            log.transform.localScale = new Vector3(0.04f, 0.15f, 0.04f);
            log.transform.localRotation = Quaternion.Euler(0, 0, 90);
            log.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateWoodTexture());
            Destroy(log.GetComponent<Collider>());

            // Add bark ring details
            var ring1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring1.transform.SetParent(root.transform);
            ring1.transform.localPosition = new Vector3(0.08f, 0, 0);
            ring1.transform.localScale = new Vector3(0.042f, 0.005f, 0.042f);
            ring1.transform.localRotation = Quaternion.Euler(0, 0, 90);
            ring1.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateBarkTexture());
            Destroy(ring1.GetComponent<Collider>());

            return root;
        }

        private GameObject CreateStoneHeld()
        {
            var root = new GameObject("HeldStone");
            var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.transform.SetParent(root.transform);
            stone.transform.localScale = new Vector3(0.08f, 0.05f, 0.07f);
            stone.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateStoneTexture());
            Destroy(stone.GetComponent<Collider>());
            return root;
        }

        private GameObject CreateOreHeld()
        {
            var root = new GameObject("HeldOre");
            var ore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ore.transform.SetParent(root.transform);
            ore.transform.localScale = new Vector3(0.08f, 0.06f, 0.07f);
            ore.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateOreTexture());
            Destroy(ore.GetComponent<Collider>());
            return root;
        }

        private GameObject CreateIngotHeld()
        {
            var root = new GameObject("HeldIngot");
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.transform.SetParent(root.transform);
            bar.transform.localScale = new Vector3(0.1f, 0.035f, 0.055f);
            bar.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateMetalTexture());
            Destroy(bar.GetComponent<Collider>());
            return root;
        }

        private GameObject CreatePlankHeld()
        {
            var root = new GameObject("HeldPlank");
            var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.transform.SetParent(root.transform);
            plank.transform.localScale = new Vector3(0.14f, 0.02f, 0.06f);
            plank.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GeneratePlankTexture());
            Destroy(plank.GetComponent<Collider>());
            return root;
        }

        private GameObject CreateFiberHeld()
        {
            var root = new GameObject("HeldFiber");
            var bundle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bundle.transform.SetParent(root.transform);
            bundle.transform.localScale = new Vector3(0.05f, 0.04f, 0.1f);
            bundle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateFiberTexture());
            Destroy(bundle.GetComponent<Collider>());
            return root;
        }

        private GameObject CreateLeatherHeld()
        {
            var root = new GameObject("HeldLeather");
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.transform.SetParent(root.transform);
            piece.transform.localScale = new Vector3(0.1f, 0.015f, 0.08f);
            piece.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateLeatherTexture());
            Destroy(piece.GetComponent<Collider>());
            return root;
        }

        private GameObject CreateMeatHeld(bool cooked)
        {
            var root = new GameObject("HeldMeat");
            var meat = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meat.transform.SetParent(root.transform);
            meat.transform.localScale = new Vector3(0.08f, 0.04f, 0.06f);
            meat.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateMeatTexture(64, cooked));
            Destroy(meat.GetComponent<Collider>());
            return root;
        }

        private GameObject CreateBerriesHeld()
        {
            var root = new GameObject("HeldBerries");
            Color berryColor = new Color(0.7f, 0.1f, 0.15f);
            for (int i = 0; i < 5; i++)
            {
                var berry = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                berry.transform.SetParent(root.transform);
                berry.transform.localPosition = new Vector3(
                    Random.Range(-0.02f, 0.02f),
                    Random.Range(-0.01f, 0.01f),
                    Random.Range(-0.02f, 0.02f)
                );
                berry.transform.localScale = Vector3.one * 0.025f;
                berry.GetComponent<Renderer>().material =
                    ProceduralTextureGenerator.CreateColorMaterial(
                        berryColor + new Color(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.05f, 0.05f)));
                Destroy(berry.GetComponent<Collider>());
            }
            return root;
        }

        // ── Procedural tools ──

        private GameObject CreateAxeHeld(bool iron)
        {
            var root = new GameObject("HeldAxe");
            Color headColor = iron ? new Color(0.6f, 0.6f, 0.65f) : new Color(0.5f, 0.48f, 0.45f);

            // Handle (cylinder for realism)
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(root.transform);
            handle.transform.localPosition = new Vector3(0, 0, 0);
            handle.transform.localScale = new Vector3(0.018f, 0.18f, 0.018f);
            handle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateWoodTexture());
            Destroy(handle.GetComponent<Collider>());

            // Head
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0.04f, 0.16f, 0);
            head.transform.localScale = new Vector3(0.08f, 0.05f, 0.018f);
            head.GetComponent<Renderer>().material = ProceduralTextureGenerator.CreateColorMaterial(headColor);
            Destroy(head.GetComponent<Collider>());

            // Binding wrap
            var wrap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wrap.transform.SetParent(root.transform);
            wrap.transform.localPosition = new Vector3(0, 0.14f, 0);
            wrap.transform.localScale = new Vector3(0.022f, 0.03f, 0.022f);
            wrap.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateFiberTexture());
            Destroy(wrap.GetComponent<Collider>());

            return root;
        }

        private GameObject CreatePickaxeHeld(bool iron)
        {
            var root = new GameObject("HeldPickaxe");
            Color headColor = iron ? new Color(0.6f, 0.6f, 0.65f) : new Color(0.5f, 0.48f, 0.45f);

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(root.transform);
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localScale = new Vector3(0.018f, 0.18f, 0.018f);
            handle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateWoodTexture());
            Destroy(handle.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0, 0.16f, 0);
            head.transform.localScale = new Vector3(0.12f, 0.018f, 0.022f);
            head.GetComponent<Renderer>().material = ProceduralTextureGenerator.CreateColorMaterial(headColor);
            Destroy(head.GetComponent<Collider>());

            // Binding
            var wrap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wrap.transform.SetParent(root.transform);
            wrap.transform.localPosition = new Vector3(0, 0.14f, 0);
            wrap.transform.localScale = new Vector3(0.022f, 0.03f, 0.022f);
            wrap.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateFiberTexture());
            Destroy(wrap.GetComponent<Collider>());

            return root;
        }

        private GameObject CreateSwordHeld()
        {
            var root = new GameObject("HeldSword");

            // Handle (wrapped)
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(root.transform);
            handle.transform.localPosition = new Vector3(0, -0.05f, 0);
            handle.transform.localScale = new Vector3(0.016f, 0.06f, 0.016f);
            handle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateLeatherTexture());
            Destroy(handle.GetComponent<Collider>());

            // Guard
            var guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guard.transform.SetParent(root.transform);
            guard.transform.localPosition = new Vector3(0, 0.01f, 0);
            guard.transform.localScale = new Vector3(0.06f, 0.012f, 0.012f);
            guard.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateColorMaterial(new Color(0.45f, 0.35f, 0.2f));
            Destroy(guard.GetComponent<Collider>());

            // Blade
            var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.transform.SetParent(root.transform);
            blade.transform.localPosition = new Vector3(0, 0.14f, 0);
            blade.transform.localScale = new Vector3(0.035f, 0.22f, 0.008f);
            var bladeMat = ProceduralTextureGenerator.CreateColorMaterial(new Color(0.6f, 0.6f, 0.65f));
            if (bladeMat.HasProperty("_Smoothness"))
                bladeMat.SetFloat("_Smoothness", 0.5f);
            if (bladeMat.HasProperty("_Metallic"))
                bladeMat.SetFloat("_Metallic", 0.4f);
            blade.GetComponent<Renderer>().material = bladeMat;
            Destroy(blade.GetComponent<Collider>());

            return root;
        }

        private GameObject CreateHammerHeld()
        {
            var root = new GameObject("HeldHammer");

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(root.transform);
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localScale = new Vector3(0.018f, 0.15f, 0.018f);
            handle.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateWoodTexture());
            Destroy(handle.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0, 0.13f, 0);
            head.transform.localScale = new Vector3(0.05f, 0.05f, 0.04f);
            head.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateStoneTexture());
            Destroy(head.GetComponent<Collider>());

            // Binding
            var wrap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wrap.transform.SetParent(root.transform);
            wrap.transform.localPosition = new Vector3(0, 0.11f, 0);
            wrap.transform.localScale = new Vector3(0.022f, 0.025f, 0.022f);
            wrap.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateMaterial(ProceduralTextureGenerator.GenerateFiberTexture());
            Destroy(wrap.GetComponent<Collider>());

            return root;
        }

        private GameObject CreateDefaultHeld(ItemDef itemDef)
        {
            var root = new GameObject("HeldDefault");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(root.transform);
            cube.transform.localScale = Vector3.one * 0.07f;
            cube.GetComponent<Renderer>().material =
                ProceduralTextureGenerator.CreateColorMaterial(itemDef.GetRarityColor());
            Destroy(cube.GetComponent<Collider>());
            return root;
        }
    }
}
