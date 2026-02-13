using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Inventory;
using SurvivalGame.Items;

namespace SurvivalGame.UI
{
    /// <summary>
    /// IMGUI-based inventory UI. Toggle with TAB.
    /// Shows a grid of slots, item names, amounts, weight.
    /// Supports: Click to select, Drop (G), Split (middle-click), Move.
    /// 
    /// Will be replaced with proper Unity UI (Canvas) later.
    /// For now: functional and testable.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.Tab;
        [SerializeField] private KeyCode _dropKey = KeyCode.G;

        [Header("Layout")]
        [SerializeField] private int _columns = 6;
        [SerializeField] private float _slotSize = 60f;
        [SerializeField] private float _padding = 4f;

        private bool _isOpen;
        private int _selectedSlot = -1;
        private int _dragFromSlot = -1;
        private InventoryController _inventoryController;
        private InventoryModel _model;

        public bool IsOpen => _isOpen;

        private void Start()
        {
            _inventoryController = FindFirstObjectByType<InventoryController>();
            if (_inventoryController != null)
                _model = _inventoryController.Model;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                ToggleInventory();
            }

            if (!_isOpen) return;

            // Drop selected item with G
            if (Input.GetKeyDown(_dropKey) && _selectedSlot >= 0)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    // Shift+G = drop 1
                    _inventoryController.DropFromSlot(_selectedSlot, 1);
                }
                else
                {
                    // G = drop entire stack
                    _inventoryController.DropSlot(_selectedSlot);
                    _selectedSlot = -1;
                }
            }
        }

        private void ToggleInventory()
        {
            _isOpen = !_isOpen;
            _selectedSlot = -1;
            _dragFromSlot = -1;

            // Show/hide cursor
            if (_isOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            GameEvents.RaiseDebugMessage(_isOpen ? "Inventory opened" : "Inventory closed");
        }

        private void OnGUI()
        {
            if (!_isOpen || _model == null) return;

            int slotCount = _model.SlotCount;
            int rows = Mathf.CeilToInt((float)slotCount / _columns);

            float gridW = _columns * (_slotSize + _padding) + _padding;
            float gridH = rows * (_slotSize + _padding) + _padding;
            float panelW = gridW + 20f;
            float panelH = gridH + 100f; // Extra space for header + footer

            // Center the panel
            float panelX = (Screen.width - panelW) / 2f;
            float panelY = (Screen.height - panelH) / 2f;

            // Background
            GUI.Box(new Rect(panelX, panelY, panelW, panelH), "");

            // Header
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 16
            };
            GUI.Label(new Rect(panelX, panelY + 5, panelW, 25), "INVENTORY", headerStyle);

            // Weight info
            GUIStyle weightStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12
            };
            float curWeight = _model.CurrentWeight;
            float maxWeight = _model.MaxWeight;
            string weightText = maxWeight > 0
                ? $"Weight: {curWeight:F1} / {maxWeight:F1} kg ({_model.UsedSlots}/{_model.SlotCount} slots)"
                : $"Weight: {curWeight:F1} kg ({_model.UsedSlots}/{_model.SlotCount} slots)";

            if (_model.IsOverWeight)
                GUI.color = Color.red;
            GUI.Label(new Rect(panelX, panelY + 28, panelW, 20), weightText, weightStyle);
            GUI.color = Color.white;

            // Slot grid
            float gridStartX = panelX + 10f;
            float gridStartY = panelY + 55f;

            for (int i = 0; i < slotCount; i++)
            {
                int col = i % _columns;
                int row = i / _columns;

                float slotX = gridStartX + col * (_slotSize + _padding);
                float slotY = gridStartY + row * (_slotSize + _padding);
                Rect slotRect = new Rect(slotX, slotY, _slotSize, _slotSize);

                DrawSlot(i, slotRect);
            }

            // Footer – controls
            float footerY = gridStartY + rows * (_slotSize + _padding) + 5f;
            GUIStyle footerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11
            };
            footerStyle.normal.textColor = Color.gray;

            string controls = "Click=Select | RClick=Move | G=Drop | Shift+G=Drop 1 | MMB=Split | TAB=Close";
            GUI.Label(new Rect(panelX, footerY, panelW, 20), controls, footerStyle);

            // Selected item tooltip
            if (_selectedSlot >= 0)
            {
                var stack = _model.GetSlot(_selectedSlot);
                if (stack != null)
                {
                    DrawTooltip(stack, panelX, footerY + 22f, panelW);
                }
            }
        }

        private void DrawSlot(int index, Rect rect)
        {
            var stack = _model.GetSlot(index);
            bool isEmpty = stack == null;
            bool isSelected = index == _selectedSlot;

            // Slot background color
            Color bgColor;
            if (isSelected)
                bgColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
            else if (!isEmpty)
                bgColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
            else
                bgColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);

            GUI.color = bgColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Border
            Color borderColor = isSelected ? Color.yellow : new Color(0.4f, 0.4f, 0.4f);
            DrawBorder(rect, borderColor);

            if (!isEmpty)
            {
                // ── Item Icon ──
                GUI.color = Color.white;
                var icon = ItemIconGenerator.GetIcon(stack.ItemDef.id);
                if (icon != null)
                {
                    float iconSize = rect.width - 8f;
                    float iconX = rect.x + 4f;
                    float iconY = rect.y + 2f;
                    GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), icon, ScaleMode.ScaleToFit);
                }

                // Item name (colored by rarity, smaller, at bottom)
                GUI.color = stack.ItemDef.GetRarityColor();
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.LowerCenter,
                    fontSize = 9,
                    wordWrap = false,
                    clipping = TextClipping.Clip
                };
                nameStyle.normal.textColor = stack.ItemDef.GetRarityColor();

                string displayName = stack.ItemDef.displayName;
                if (displayName.Length > 8) displayName = displayName[..8];
                GUI.Label(new Rect(rect.x + 2, rect.y + rect.height - 16, rect.width - 4, 14), displayName, nameStyle);

                // Amount (bottom-right, with shadow)
                if (stack.Amount > 1)
                {
                    GUIStyle amountStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.LowerRight,
                        fontSize = 13,
                        fontStyle = FontStyle.Bold
                    };
                    // Shadow
                    amountStyle.normal.textColor = Color.black;
                    GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 17),
                        stack.Amount.ToString(), amountStyle);
                    // Text
                    amountStyle.normal.textColor = Color.white;
                    GUI.Label(new Rect(rect.x, rect.y, rect.width - 3, rect.height - 18),
                        stack.Amount.ToString(), amountStyle);
                }

                // Durability bar (bottom)
                if (stack.HasDurability)
                {
                    float maxDur = stack.ItemDef is ToolDef tool ? tool.durabilityMax : 100f;
                    float pct = Mathf.Clamp01(stack.Durability / maxDur);
                    Color durColor = pct > 0.5f ? Color.green : (pct > 0.2f ? Color.yellow : Color.red);

                    Rect barBg = new Rect(rect.x + 3, rect.yMax - 8, rect.width - 6, 4);
                    GUI.color = Color.black;
                    GUI.DrawTexture(barBg, Texture2D.whiteTexture);
                    GUI.color = durColor;
                    GUI.DrawTexture(new Rect(barBg.x, barBg.y, barBg.width * pct, barBg.height), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
            }

            // Handle clicks
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.button == 0) // Left click – select
                {
                    if (_selectedSlot >= 0 && _selectedSlot != index)
                    {
                        // Move from selected to this slot
                        _model.MoveSlot(_selectedSlot, index);
                        _selectedSlot = -1;
                    }
                    else
                    {
                        _selectedSlot = isEmpty ? -1 : index;
                    }
                    Event.current.Use();
                }
                else if (Event.current.button == 1) // Right click – quick action
                {
                    if (!isEmpty)
                    {
                        _selectedSlot = index;
                    }
                    Event.current.Use();
                }
                else if (Event.current.button == 2) // Middle click – split half
                {
                    if (!isEmpty && stack.Amount > 1)
                    {
                        _model.SplitSlot(index, stack.Amount / 2);
                    }
                    Event.current.Use();
                }
            }
        }

        private void DrawTooltip(ItemStack stack, float x, float y, float width)
        {
            GUIStyle tooltipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.UpperCenter
            };
            tooltipStyle.normal.textColor = stack.ItemDef.GetRarityColor();

            string tooltip = $"{stack.ItemDef.displayName} x{stack.Amount}";
            tooltip += $"\nWeight: {stack.TotalWeight:F1}kg | {stack.ItemDef.rarity}";
            if (!string.IsNullOrEmpty(stack.ItemDef.description))
                tooltip += $"\n{stack.ItemDef.description}";
            if (stack.HasDurability)
                tooltip += $"\nDurability: {stack.Durability:F0}";

            GUI.Label(new Rect(x, y, width, 60), tooltip, tooltipStyle);
        }

        private void DrawBorder(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1), Texture2D.whiteTexture); // Top
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1, rect.width, 1), Texture2D.whiteTexture); // Bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y, 1, rect.height), Texture2D.whiteTexture); // Left
            GUI.DrawTexture(new Rect(rect.xMax - 1, rect.y, 1, rect.height), Texture2D.whiteTexture); // Right
            GUI.color = Color.white;
        }
    }
}
