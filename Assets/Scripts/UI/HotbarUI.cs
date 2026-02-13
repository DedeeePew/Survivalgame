using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Inventory;
using SurvivalGame.Items;

namespace SurvivalGame.UI
{
    /// <summary>
    /// IMGUI-based hotbar displayed at the bottom of the screen.
    /// Shows the first 9 inventory slots (0-8).
    /// Player can switch with 1-9 keys or scroll wheel.
    /// Selected slot's item is shown in the player's hand.
    /// 
    /// Always visible during gameplay (not just when inventory is open).
    /// </summary>
    public class HotbarUI : MonoBehaviour
    {
        [Header("Hotbar Settings")]
        [SerializeField] private int _slotCount = 9;
        [SerializeField] private float _slotSize = 52f;
        [SerializeField] private float _padding = 3f;
        [SerializeField] private float _bottomMargin = 20f;

        private int _selectedSlot = 0;
        private InventoryController _inventoryController;
        private InventoryModel _model;

        // Public
        public int SelectedSlot => _selectedSlot;
        public int SlotCount => _slotCount;

        /// <summary>Get the ItemStack in the currently selected hotbar slot.</summary>
        public ItemStack SelectedStack
        {
            get
            {
                if (_model == null) return null;
                return _model.GetSlot(_selectedSlot);
            }
        }

        // Events
        public event System.Action<int> OnSlotChanged; // fires when selected slot changes

        private void Start()
        {
            _inventoryController = FindFirstObjectByType<InventoryController>();
            if (_inventoryController != null)
                _model = _inventoryController.Model;
        }

        private void Update()
        {
            // Don't handle hotbar input when inventory UI is open
            var invUI = FindFirstObjectByType<InventoryUI>();
            if (invUI != null && invUI.IsOpen) return;

            HandleNumberKeys();
            HandleScrollWheel();
        }

        private void HandleNumberKeys()
        {
            // Keys 1-9 → slots 0-8
            for (int i = 0; i < _slotCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectSlot(i);
                    return;
                }
            }
        }

        private void HandleScrollWheel()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll == 0) return;

            if (scroll > 0)
            {
                // Scroll up → previous slot
                SelectSlot((_selectedSlot - 1 + _slotCount) % _slotCount);
            }
            else
            {
                // Scroll down → next slot
                SelectSlot((_selectedSlot + 1) % _slotCount);
            }
        }

        private void SelectSlot(int index)
        {
            if (index == _selectedSlot) return;
            _selectedSlot = index;
            OnSlotChanged?.Invoke(_selectedSlot);
            GameEvents.RaiseDebugMessage($"Hotbar Slot {_selectedSlot + 1}");
        }

        private void OnGUI()
        {
            if (_model == null) return;

            // Don't draw hotbar when full inventory is open
            var invUI = FindFirstObjectByType<InventoryUI>();
            if (invUI != null && invUI.IsOpen) return;

            float totalWidth = _slotCount * (_slotSize + _padding) - _padding;
            float startX = (Screen.width - totalWidth) / 2f;
            float startY = Screen.height - _slotSize - _bottomMargin;

            // Background panel
            float bgPadding = 6f;
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(new Rect(
                startX - bgPadding,
                startY - bgPadding,
                totalWidth + bgPadding * 2,
                _slotSize + bgPadding * 2
            ), Texture2D.whiteTexture);
            GUI.color = Color.white;

            for (int i = 0; i < _slotCount; i++)
            {
                float slotX = startX + i * (_slotSize + _padding);
                Rect slotRect = new Rect(slotX, startY, _slotSize, _slotSize);
                DrawHotbarSlot(i, slotRect);
            }

            // Selected slot name (above hotbar)
            var selectedStack = SelectedStack;
            if (selectedStack != null)
            {
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13,
                    fontStyle = FontStyle.Bold
                };
                nameStyle.normal.textColor = selectedStack.ItemDef.GetRarityColor();

                float nameY = startY - 24f;
                // Shadow
                GUI.color = new Color(0, 0, 0, 0.8f);
                GUI.Label(new Rect(startX + 1, nameY + 1, totalWidth, 20), selectedStack.ItemDef.displayName, nameStyle);
                GUI.color = Color.white;
                nameStyle.normal.textColor = selectedStack.ItemDef.GetRarityColor();
                GUI.Label(new Rect(startX, nameY, totalWidth, 20), selectedStack.ItemDef.displayName, nameStyle);
            }
        }

        private void DrawHotbarSlot(int index, Rect rect)
        {
            var stack = _model.GetSlot(index);
            bool isEmpty = stack == null;
            bool isSelected = index == _selectedSlot;

            // Slot background
            Color bgColor;
            if (isSelected)
                bgColor = new Color(0.35f, 0.55f, 0.85f, 0.85f);
            else if (!isEmpty)
                bgColor = new Color(0.2f, 0.2f, 0.2f, 0.75f);
            else
                bgColor = new Color(0.12f, 0.12f, 0.12f, 0.6f);

            GUI.color = bgColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Border (thicker for selected)
            Color borderColor = isSelected ? new Color(1f, 0.85f, 0.2f) : new Color(0.35f, 0.35f, 0.35f);
            int borderThickness = isSelected ? 2 : 1;
            DrawBorder(rect, borderColor, borderThickness);

            // Slot number (top-left)
            GUIStyle numStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 9
            };
            numStyle.normal.textColor = isSelected ? new Color(1f, 0.85f, 0.2f) : new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(rect.x + 2, rect.y + 1, 14, 12), (index + 1).ToString(), numStyle);

            if (!isEmpty)
            {
                // Item icon
                var icon = ItemIconGenerator.GetIcon(stack.ItemDef.id);
                if (icon != null)
                {
                    float iconSize = rect.width - 10f;
                    float iconX = rect.x + 5f;
                    float iconY = rect.y + 3f;
                    GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), icon, ScaleMode.ScaleToFit);
                }

                // Amount (bottom-right with shadow)
                if (stack.Amount > 1)
                {
                    GUIStyle amountStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.LowerRight,
                        fontSize = 12,
                        fontStyle = FontStyle.Bold
                    };
                    amountStyle.normal.textColor = Color.black;
                    GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2),
                        stack.Amount.ToString(), amountStyle);
                    amountStyle.normal.textColor = Color.white;
                    GUI.Label(new Rect(rect.x, rect.y, rect.width - 3, rect.height - 3),
                        stack.Amount.ToString(), amountStyle);
                }

                // Durability bar
                if (stack.HasDurability)
                {
                    float maxDur = stack.ItemDef is ToolDef tool ? tool.durabilityMax : 100f;
                    float pct = Mathf.Clamp01(stack.Durability / maxDur);
                    Color durColor = pct > 0.5f ? Color.green : (pct > 0.2f ? Color.yellow : Color.red);

                    Rect barBg = new Rect(rect.x + 3, rect.yMax - 6, rect.width - 6, 3);
                    GUI.color = Color.black;
                    GUI.DrawTexture(barBg, Texture2D.whiteTexture);
                    GUI.color = durColor;
                    GUI.DrawTexture(new Rect(barBg.x, barBg.y, barBg.width * pct, barBg.height), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
            }
        }

        private void DrawBorder(Rect rect, Color color, int thickness = 1)
        {
            GUI.color = color;
            for (int t = 0; t < thickness; t++)
            {
                GUI.DrawTexture(new Rect(rect.x - t, rect.y - t, rect.width + t * 2, 1), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(rect.x - t, rect.yMax - 1 + t, rect.width + t * 2, 1), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(rect.x - t, rect.y - t, 1, rect.height + t * 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(rect.xMax - 1 + t, rect.y - t, 1, rect.height + t * 2), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }
    }
}
