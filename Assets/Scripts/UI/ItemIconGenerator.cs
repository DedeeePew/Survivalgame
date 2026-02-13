using UnityEngine;
using System.Collections.Generic;

namespace SurvivalGame.UI
{
    /// <summary>
    /// Generates procedural 2D icons for items at runtime.
    /// Each item type gets a unique, recognizable pixel-art style icon.
    /// Icons are cached after first generation.
    /// </summary>
    public static class ItemIconGenerator
    {
        private static readonly Dictionary<string, Texture2D> _iconCache = new();
        private const int ICON_SIZE = 48;

        /// <summary>Get or generate an icon for an item by its id.</summary>
        public static Texture2D GetIcon(string itemId)
        {
            if (_iconCache.TryGetValue(itemId, out var cached))
                return cached;

            var icon = GenerateIcon(itemId);
            _iconCache[itemId] = icon;
            return icon;
        }

        /// <summary>Clear the icon cache (e.g. on scene unload).</summary>
        public static void ClearCache()
        {
            foreach (var tex in _iconCache.Values)
            {
                if (tex != null) Object.Destroy(tex);
            }
            _iconCache.Clear();
        }

        private static Texture2D GenerateIcon(string itemId)
        {
            return itemId switch
            {
                "item_wood" => DrawWoodIcon(),
                "item_stone" => DrawStoneIcon(),
                "item_iron_ore" => DrawOreIcon(),
                "item_iron_ingot" => DrawIngotIcon(),
                "item_plank" => DrawPlankIcon(),
                "item_fiber" => DrawFiberIcon(),
                "item_leather" => DrawLeatherIcon(),
                "item_raw_meat" => DrawMeatIcon(false),
                "item_cooked_meat" => DrawMeatIcon(true),
                "item_berries" => DrawBerriesIcon(),
                "tool_stone_axe" => DrawAxeIcon(false),
                "tool_iron_axe" => DrawAxeIcon(true),
                "tool_stone_pickaxe" => DrawPickaxeIcon(false),
                "tool_iron_pickaxe" => DrawPickaxeIcon(true),
                "tool_stone_sword" => DrawSwordIcon(false),
                "tool_build_hammer" => DrawHammerIcon(),
                _ => DrawDefaultIcon(itemId)
            };
        }

        // ══════════════════════════════════════
        // RESOURCE ICONS
        // ══════════════════════════════════════

        private static Texture2D DrawWoodIcon()
        {
            var tex = CreateBlankIcon();
            Color bark = new Color(0.35f, 0.2f, 0.08f);
            Color barkLight = new Color(0.45f, 0.28f, 0.12f);
            Color inside = new Color(0.65f, 0.45f, 0.25f);
            Color ring = new Color(0.55f, 0.38f, 0.2f);

            // Log shape (cylinder side view)
            FillRect(tex, 8, 12, 32, 24, bark);
            FillRect(tex, 10, 14, 28, 20, barkLight);

            // Wood grain lines
            for (int y = 15; y < 33; y += 4)
                FillRect(tex, 10, y, 28, 1, bark);

            // End circle (cross-section)
            FillCircle(tex, 36, 24, 10, inside);
            FillCircle(tex, 36, 24, 8, ring);
            FillCircle(tex, 36, 24, 5, inside);
            FillCircle(tex, 36, 24, 2, ring);

            // Outline
            DrawRectOutline(tex, 7, 11, 34, 26, new Color(0.2f, 0.1f, 0.05f));

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawStoneIcon()
        {
            var tex = CreateBlankIcon();
            Color stoneLight = new Color(0.6f, 0.58f, 0.55f);
            Color stoneDark = new Color(0.4f, 0.38f, 0.36f);
            Color stoneMid = new Color(0.5f, 0.48f, 0.45f);

            // Irregular stone shape
            FillCircle(tex, 24, 24, 16, stoneMid);
            FillCircle(tex, 22, 22, 14, stoneLight);
            FillCircle(tex, 28, 28, 10, stoneDark);
            FillCircle(tex, 20, 20, 8, stoneLight);

            // Cracks
            FillRect(tex, 18, 20, 12, 1, stoneDark);
            FillRect(tex, 22, 26, 8, 1, stoneDark);

            // Highlight
            FillCircle(tex, 18, 18, 3, new Color(0.7f, 0.68f, 0.65f));

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawOreIcon()
        {
            var tex = CreateBlankIcon();
            Color stoneBase = new Color(0.4f, 0.38f, 0.36f);
            Color oreGold = new Color(0.7f, 0.5f, 0.2f);
            Color oreShine = new Color(0.85f, 0.65f, 0.3f);

            // Rock base
            FillCircle(tex, 24, 24, 16, stoneBase);
            FillCircle(tex, 22, 22, 14, new Color(0.45f, 0.43f, 0.4f));

            // Ore veins / speckles
            FillCircle(tex, 18, 20, 4, oreGold);
            FillCircle(tex, 28, 16, 3, oreGold);
            FillCircle(tex, 30, 26, 4, oreGold);
            FillCircle(tex, 22, 30, 3, oreGold);

            // Shine spots
            FillCircle(tex, 18, 19, 2, oreShine);
            FillCircle(tex, 30, 25, 2, oreShine);

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawIngotIcon()
        {
            var tex = CreateBlankIcon();
            Color metalBase = new Color(0.6f, 0.6f, 0.65f);
            Color metalLight = new Color(0.75f, 0.75f, 0.8f);
            Color metalDark = new Color(0.4f, 0.4f, 0.45f);
            Color outline = new Color(0.3f, 0.3f, 0.35f);

            // Ingot trapezoid shape (top face)
            FillRect(tex, 12, 14, 24, 8, metalLight);
            // Front face
            FillRect(tex, 10, 22, 28, 12, metalBase);
            // Side face (darker)
            FillRect(tex, 34, 18, 6, 16, metalDark);

            // Top highlight
            FillRect(tex, 14, 15, 18, 3, new Color(0.85f, 0.85f, 0.9f));

            // Outline
            DrawRectOutline(tex, 10, 14, 30, 22, outline);

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawPlankIcon()
        {
            var tex = CreateBlankIcon();
            Color plankBase = new Color(0.6f, 0.42f, 0.22f);
            Color plankLight = new Color(0.7f, 0.5f, 0.28f);
            Color plankDark = new Color(0.5f, 0.35f, 0.18f);

            // Rectangular plank
            FillRect(tex, 6, 16, 36, 16, plankBase);
            
            // Wood grain
            for (int y = 18; y < 31; y += 3)
            {
                FillRect(tex, 8, y, 32, 1, plankDark);
            }

            // Light streak
            FillRect(tex, 10, 20, 28, 2, plankLight);

            // Outline
            DrawRectOutline(tex, 5, 15, 38, 18, new Color(0.3f, 0.2f, 0.1f));

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawFiberIcon()
        {
            var tex = CreateBlankIcon();
            Color fiberBase = new Color(0.6f, 0.55f, 0.35f);
            Color fiberDark = new Color(0.45f, 0.4f, 0.25f);
            Color fiberLight = new Color(0.7f, 0.65f, 0.45f);

            // Bundled fibers (wavy lines)
            for (int i = 0; i < 6; i++)
            {
                int startX = 10 + i * 2;
                for (int y = 8; y < 40; y++)
                {
                    int offsetX = (int)(Mathf.Sin(y * 0.3f + i) * 2);
                    Color c = (i % 2 == 0) ? fiberBase : fiberLight;
                    SetPixelSafe(tex, startX + offsetX, y, c);
                    SetPixelSafe(tex, startX + offsetX + 1, y, c);
                }
            }

            // Tie / binding in middle
            FillRect(tex, 8, 22, 20, 4, fiberDark);

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawLeatherIcon()
        {
            var tex = CreateBlankIcon();
            Color leather = new Color(0.5f, 0.3f, 0.15f);
            Color leatherLight = new Color(0.6f, 0.38f, 0.2f);
            Color leatherDark = new Color(0.35f, 0.2f, 0.1f);

            // Hide shape (irregular rectangle)
            FillRect(tex, 10, 10, 28, 28, leather);
            FillRect(tex, 12, 12, 24, 24, leatherLight);

            // Texture dots
            for (int y = 14; y < 34; y += 4)
            {
                for (int x = 14; x < 34; x += 4)
                {
                    SetPixelSafe(tex, x, y, leatherDark);
                }
            }

            // Folded corner
            FillRect(tex, 30, 10, 8, 8, leatherDark);

            DrawRectOutline(tex, 9, 9, 30, 30, leatherDark);

            tex.Apply();
            return tex;
        }

        // ══════════════════════════════════════
        // FOOD ICONS
        // ══════════════════════════════════════

        private static Texture2D DrawMeatIcon(bool cooked)
        {
            var tex = CreateBlankIcon();
            Color meatBase = cooked ? new Color(0.5f, 0.3f, 0.15f) : new Color(0.7f, 0.25f, 0.2f);
            Color meatLight = cooked ? new Color(0.6f, 0.38f, 0.2f) : new Color(0.8f, 0.35f, 0.25f);
            Color bone = new Color(0.9f, 0.88f, 0.8f);
            Color boneDark = new Color(0.7f, 0.68f, 0.6f);

            // Meat chunk (organic blob shape)
            FillCircle(tex, 20, 22, 12, meatBase);
            FillCircle(tex, 24, 20, 10, meatLight);
            FillCircle(tex, 18, 26, 8, meatBase);

            // Fat marbling (if raw)
            if (!cooked)
            {
                FillCircle(tex, 22, 22, 3, new Color(0.9f, 0.8f, 0.7f));
                FillCircle(tex, 18, 18, 2, new Color(0.85f, 0.75f, 0.65f));
            }

            // Bone sticking out
            FillRect(tex, 28, 12, 4, 16, bone);
            FillRect(tex, 30, 10, 6, 4, bone);
            DrawRectOutline(tex, 28, 12, 4, 16, boneDark);

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawBerriesIcon()
        {
            var tex = CreateBlankIcon();
            Color berryRed = new Color(0.7f, 0.1f, 0.15f);
            Color berryDark = new Color(0.5f, 0.05f, 0.1f);
            Color berryShine = new Color(0.9f, 0.4f, 0.4f);
            Color stem = new Color(0.2f, 0.4f, 0.15f);
            Color leaf = new Color(0.15f, 0.5f, 0.1f);

            // Stem
            FillRect(tex, 23, 6, 2, 10, stem);

            // Small leaf
            FillCircle(tex, 27, 10, 4, leaf);

            // Berry cluster
            FillCircle(tex, 18, 24, 7, berryRed);
            FillCircle(tex, 30, 24, 7, berryRed);
            FillCircle(tex, 24, 18, 7, berryRed);
            FillCircle(tex, 24, 30, 7, berryRed);

            // Shine spots
            FillCircle(tex, 16, 22, 2, berryShine);
            FillCircle(tex, 28, 22, 2, berryShine);
            FillCircle(tex, 22, 16, 2, berryShine);

            // Dark bottoms
            FillCircle(tex, 20, 27, 2, berryDark);
            FillCircle(tex, 32, 27, 2, berryDark);

            tex.Apply();
            return tex;
        }

        // ══════════════════════════════════════
        // TOOL ICONS
        // ══════════════════════════════════════

        private static Texture2D DrawAxeIcon(bool iron)
        {
            var tex = CreateBlankIcon();
            Color handle = new Color(0.45f, 0.28f, 0.12f);
            Color handleDark = new Color(0.35f, 0.2f, 0.08f);
            Color head = iron ? new Color(0.6f, 0.6f, 0.65f) : new Color(0.5f, 0.48f, 0.45f);
            Color headLight = iron ? new Color(0.75f, 0.75f, 0.8f) : new Color(0.6f, 0.58f, 0.55f);
            Color headDark = iron ? new Color(0.4f, 0.4f, 0.45f) : new Color(0.35f, 0.33f, 0.3f);

            // Handle (diagonal)
            for (int i = 0; i < 30; i++)
            {
                int x = 10 + i;
                int y = 38 - i;
                FillRect(tex, x, y, 3, 3, handle);
            }
            // Handle outline
            for (int i = 0; i < 30; i++)
            {
                int x = 10 + i;
                int y = 38 - i;
                SetPixelSafe(tex, x, y, handleDark);
                SetPixelSafe(tex, x + 3, y + 3, handleDark);
            }

            // Axe head
            FillRect(tex, 28, 4, 14, 18, head);
            FillRect(tex, 30, 2, 12, 3, headLight);
            FillRect(tex, 32, 6, 8, 14, headLight);

            // Edge (sharp side)
            for (int y = 4; y < 22; y++)
            {
                SetPixelSafe(tex, 42, y, headDark);
                SetPixelSafe(tex, 43, y, headDark);
            }

            // Shine
            FillRect(tex, 33, 7, 3, 8, new Color(headLight.r + 0.1f, headLight.g + 0.1f, headLight.b + 0.1f));

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawPickaxeIcon(bool iron)
        {
            var tex = CreateBlankIcon();
            Color handle = new Color(0.45f, 0.28f, 0.12f);
            Color head = iron ? new Color(0.6f, 0.6f, 0.65f) : new Color(0.5f, 0.48f, 0.45f);
            Color headDark = iron ? new Color(0.4f, 0.4f, 0.45f) : new Color(0.35f, 0.33f, 0.3f);

            // Handle (vertical)
            FillRect(tex, 22, 18, 4, 28, handle);
            DrawRectOutline(tex, 21, 17, 6, 30, new Color(0.3f, 0.18f, 0.06f));

            // Pickaxe head (horizontal)
            FillRect(tex, 6, 8, 36, 6, head);
            // Pointed ends
            for (int i = 0; i < 6; i++)
            {
                FillRect(tex, 4 - i / 2, 9 + i / 3, 4, 3 - i / 2, headDark);
                FillRect(tex, 40 + i / 2, 9 + i / 3, 4, 3 - i / 2, headDark);
            }

            // Shine
            FillRect(tex, 10, 9, 28, 2, new Color(head.r + 0.12f, head.g + 0.12f, head.b + 0.12f));

            // Binding
            FillRect(tex, 20, 12, 8, 6, headDark);

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawSwordIcon(bool iron)
        {
            var tex = CreateBlankIcon();
            Color blade = iron ? new Color(0.7f, 0.7f, 0.75f) : new Color(0.55f, 0.53f, 0.5f);
            Color bladeShine = iron ? new Color(0.85f, 0.85f, 0.9f) : new Color(0.65f, 0.63f, 0.6f);
            Color guard = new Color(0.4f, 0.3f, 0.15f);
            Color handle = new Color(0.35f, 0.2f, 0.1f);
            Color pommel = new Color(0.5f, 0.35f, 0.15f);

            // Blade (vertical, pointing up)
            FillRect(tex, 20, 4, 8, 24, blade);
            FillRect(tex, 22, 2, 4, 3, blade); // Tip
            SetPixelSafe(tex, 23, 1, blade);
            SetPixelSafe(tex, 24, 1, blade);

            // Blade shine
            FillRect(tex, 22, 5, 2, 20, bladeShine);

            // Cross guard
            FillRect(tex, 12, 28, 24, 4, guard);

            // Handle
            FillRect(tex, 20, 32, 8, 10, handle);

            // Pommel
            FillCircle(tex, 24, 44, 4, pommel);

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawHammerIcon()
        {
            var tex = CreateBlankIcon();
            Color handle = new Color(0.45f, 0.28f, 0.12f);
            Color head = new Color(0.5f, 0.48f, 0.45f);
            Color headLight = new Color(0.6f, 0.58f, 0.55f);

            // Handle (vertical)
            FillRect(tex, 22, 20, 4, 26, handle);
            DrawRectOutline(tex, 21, 19, 6, 28, new Color(0.3f, 0.18f, 0.06f));

            // Hammer head (big rectangle)
            FillRect(tex, 10, 4, 28, 16, head);
            FillRect(tex, 12, 6, 24, 12, headLight);

            // Outline
            DrawRectOutline(tex, 9, 3, 30, 18, new Color(0.3f, 0.28f, 0.25f));

            // Face highlights
            FillRect(tex, 14, 7, 8, 8, new Color(0.65f, 0.63f, 0.6f));

            tex.Apply();
            return tex;
        }

        private static Texture2D DrawDefaultIcon(string itemId)
        {
            var tex = CreateBlankIcon();

            // Simple question mark or generic item box
            Color boxColor = new Color(0.4f, 0.4f, 0.5f);
            Color highlight = new Color(0.5f, 0.5f, 0.6f);

            FillRect(tex, 10, 10, 28, 28, boxColor);
            FillRect(tex, 12, 12, 24, 24, highlight);

            // "?" shape
            Color textColor = Color.white;
            FillRect(tex, 18, 16, 12, 3, textColor);
            FillRect(tex, 27, 18, 3, 6, textColor);
            FillRect(tex, 20, 24, 10, 3, textColor);
            FillRect(tex, 20, 27, 3, 4, textColor);
            FillRect(tex, 22, 34, 4, 4, textColor);

            DrawRectOutline(tex, 9, 9, 30, 30, new Color(0.3f, 0.3f, 0.4f));

            tex.Apply();
            return tex;
        }

        // ══════════════════════════════════════
        // DRAWING HELPERS
        // ══════════════════════════════════════

        private static Texture2D CreateBlankIcon()
        {
            var tex = new Texture2D(ICON_SIZE, ICON_SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // Pixel art look
            tex.wrapMode = TextureWrapMode.Clamp;

            // Fill with transparent
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < ICON_SIZE; y++)
                for (int x = 0; x < ICON_SIZE; x++)
                    tex.SetPixel(x, y, clear);

            return tex;
        }

        private static void SetPixelSafe(Texture2D tex, int x, int y, Color color)
        {
            if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                tex.SetPixel(x, y, color);
        }

        private static void FillRect(Texture2D tex, int startX, int startY, int width, int height, Color color)
        {
            for (int y = startY; y < startY + height; y++)
                for (int x = startX; x < startX + width; x++)
                    SetPixelSafe(tex, x, y, color);
        }

        private static void DrawRectOutline(Texture2D tex, int startX, int startY, int width, int height, Color color)
        {
            // Top & bottom
            for (int x = startX; x < startX + width; x++)
            {
                SetPixelSafe(tex, x, startY, color);
                SetPixelSafe(tex, x, startY + height - 1, color);
            }
            // Left & right
            for (int y = startY; y < startY + height; y++)
            {
                SetPixelSafe(tex, startX, y, color);
                SetPixelSafe(tex, startX + width - 1, y, color);
            }
        }

        private static void FillCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            int r2 = radius * radius;
            for (int y = -radius; y <= radius; y++)
                for (int x = -radius; x <= radius; x++)
                    if (x * x + y * y <= r2)
                        SetPixelSafe(tex, cx + x, cy + y, color);
        }
    }
}
