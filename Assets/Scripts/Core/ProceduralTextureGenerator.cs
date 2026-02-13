using UnityEngine;

namespace SurvivalGame.Core
{
    /// <summary>
    /// Generates procedural textures at runtime for terrain, items, and materials.
    /// No external assets needed – pure code-based survival aesthetics.
    /// </summary>
    public static class ProceduralTextureGenerator
    {
        // ══════════════════════════════════════
        // TERRAIN TEXTURES
        // ══════════════════════════════════════

        /// <summary>Generates a grass-like terrain texture with natural variation.</summary>
        public static Texture2D GenerateGrassTexture(int size = 256)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;

            float scale = 0.05f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Multi-octave Perlin noise for natural grass variation
                    float n1 = Mathf.PerlinNoise(x * scale, y * scale);
                    float n2 = Mathf.PerlinNoise(x * scale * 3f + 100, y * scale * 3f + 100) * 0.3f;
                    float n3 = Mathf.PerlinNoise(x * scale * 8f + 200, y * scale * 8f + 200) * 0.1f;
                    float noise = n1 + n2 + n3;

                    // Grass green tones
                    float r = Mathf.Lerp(0.15f, 0.35f, noise) + Random.Range(-0.02f, 0.02f);
                    float g = Mathf.Lerp(0.35f, 0.65f, noise) + Random.Range(-0.03f, 0.03f);
                    float b = Mathf.Lerp(0.08f, 0.18f, noise) + Random.Range(-0.01f, 0.01f);

                    // Occasional darker patches (dirt showing through)
                    float dirtNoise = Mathf.PerlinNoise(x * 0.02f + 50, y * 0.02f + 50);
                    if (dirtNoise > 0.7f)
                    {
                        float dirtAmount = (dirtNoise - 0.7f) * 2f;
                        r = Mathf.Lerp(r, 0.35f, dirtAmount);
                        g = Mathf.Lerp(g, 0.25f, dirtAmount);
                        b = Mathf.Lerp(b, 0.12f, dirtAmount);
                    }

                    tex.SetPixel(x, y, new Color(
                        Mathf.Clamp01(r),
                        Mathf.Clamp01(g),
                        Mathf.Clamp01(b)
                    ));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a dirt/path texture.</summary>
        public static Texture2D GenerateDirtTexture(int size = 128)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float n2 = Mathf.PerlinNoise(x * 0.2f + 50, y * 0.2f + 50) * 0.3f;

                    float r = Mathf.Lerp(0.3f, 0.5f, n + n2);
                    float g = Mathf.Lerp(0.2f, 0.35f, n + n2);
                    float b = Mathf.Lerp(0.1f, 0.2f, n + n2);

                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }
            tex.Apply();
            return tex;
        }

        // ══════════════════════════════════════
        // MATERIAL TEXTURES (for items/world)
        // ══════════════════════════════════════

        /// <summary>Generates a wood bark/log texture.</summary>
        public static Texture2D GenerateWoodTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Wood grain - horizontal lines with noise
                    float grain = Mathf.PerlinNoise(x * 0.03f, y * 0.15f);
                    float detail = Mathf.PerlinNoise(x * 0.1f + 30, y * 0.5f + 30) * 0.2f;
                    float ring = Mathf.Sin(y * 0.3f + grain * 5f) * 0.1f;

                    float v = grain + detail + ring;
                    float r = Mathf.Lerp(0.35f, 0.55f, v);
                    float g = Mathf.Lerp(0.2f, 0.35f, v);
                    float b = Mathf.Lerp(0.08f, 0.15f, v);

                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a stone/rock texture.</summary>
        public static Texture2D GenerateStoneTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n1 = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float n2 = Mathf.PerlinNoise(x * 0.2f + 100, y * 0.2f + 100) * 0.3f;
                    float n3 = Mathf.PerlinNoise(x * 0.5f + 200, y * 0.5f + 200) * 0.1f;

                    float v = n1 + n2 + n3;
                    float grey = Mathf.Lerp(0.35f, 0.7f, v);

                    // Slight color variation
                    float r = grey + Random.Range(-0.02f, 0.02f);
                    float g = grey - 0.02f + Random.Range(-0.01f, 0.01f);
                    float b = grey + 0.01f + Random.Range(-0.01f, 0.01f);

                    tex.SetPixel(x, y, new Color(
                        Mathf.Clamp01(r),
                        Mathf.Clamp01(g),
                        Mathf.Clamp01(b)
                    ));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates an iron/ore texture with metallic speckles.</summary>
        public static Texture2D GenerateOreTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    float baseGrey = Mathf.Lerp(0.3f, 0.5f, n);

                    // Ore speckles
                    float speckle = Mathf.PerlinNoise(x * 0.4f + 150, y * 0.4f + 150);
                    if (speckle > 0.65f)
                    {
                        float intensity = (speckle - 0.65f) * 3f;
                        float r = Mathf.Lerp(baseGrey, 0.7f, intensity);
                        float g = Mathf.Lerp(baseGrey, 0.5f, intensity);
                        float b = Mathf.Lerp(baseGrey, 0.3f, intensity);
                        tex.SetPixel(x, y, new Color(r, g, b));
                    }
                    else
                    {
                        tex.SetPixel(x, y, new Color(baseGrey, baseGrey * 0.95f, baseGrey * 0.9f));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a berry/food texture (reddish-purple).</summary>
        public static Texture2D GenerateBerryTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    float r = Mathf.Lerp(0.5f, 0.8f, n);
                    float g = Mathf.Lerp(0.05f, 0.2f, n);
                    float b = Mathf.Lerp(0.15f, 0.4f, n);

                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a meat texture (pinkish-red).</summary>
        public static Texture2D GenerateMeatTexture(int size = 64, bool cooked = false)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    float r, g, b;

                    if (cooked)
                    {
                        r = Mathf.Lerp(0.4f, 0.6f, n);
                        g = Mathf.Lerp(0.2f, 0.35f, n);
                        b = Mathf.Lerp(0.1f, 0.2f, n);
                    }
                    else
                    {
                        r = Mathf.Lerp(0.6f, 0.85f, n);
                        g = Mathf.Lerp(0.15f, 0.3f, n);
                        b = Mathf.Lerp(0.15f, 0.25f, n);
                    }

                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a metal ingot texture (shiny grey-blue).</summary>
        public static Texture2D GenerateMetalTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                    float highlight = Mathf.PerlinNoise(x * 0.15f + 50, y * 0.15f + 50);

                    float grey = Mathf.Lerp(0.5f, 0.75f, n);
                    if (highlight > 0.7f)
                        grey = Mathf.Lerp(grey, 0.9f, (highlight - 0.7f) * 3f);

                    float r = grey * 0.95f;
                    float g = grey;
                    float b = grey * 1.05f;

                    tex.SetPixel(x, y, new Color(
                        Mathf.Clamp01(r),
                        Mathf.Clamp01(g),
                        Mathf.Clamp01(b)
                    ));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a leaf/foliage texture for trees.</summary>
        public static Texture2D GenerateLeafTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    float n2 = Mathf.PerlinNoise(x * 0.3f + 80, y * 0.3f + 80) * 0.3f;

                    float r = Mathf.Lerp(0.1f, 0.25f, n + n2);
                    float g = Mathf.Lerp(0.3f, 0.6f, n + n2);
                    float b = Mathf.Lerp(0.05f, 0.15f, n + n2);

                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a fiber/rope texture (tan/beige).</summary>
        public static Texture2D GenerateFiberTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float grain = Mathf.PerlinNoise(x * 0.05f, y * 0.3f);
                    float r = Mathf.Lerp(0.55f, 0.75f, grain);
                    float g = Mathf.Lerp(0.5f, 0.65f, grain);
                    float b = Mathf.Lerp(0.3f, 0.4f, grain);

                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a leather texture (brown, slightly bumpy).</summary>
        public static Texture2D GenerateLeatherTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                    float bump = Mathf.PerlinNoise(x * 0.4f + 70, y * 0.4f + 70) * 0.15f;

                    float r = Mathf.Lerp(0.4f, 0.55f, n + bump);
                    float g = Mathf.Lerp(0.22f, 0.35f, n + bump);
                    float b = Mathf.Lerp(0.1f, 0.18f, n + bump);

                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Generates a plank/processed wood texture.</summary>
        public static Texture2D GeneratePlankTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float grain = Mathf.PerlinNoise(x * 0.02f, y * 0.2f);
                    float detail = Mathf.PerlinNoise(x * 0.08f + 40, y * 0.4f + 40) * 0.15f;

                    float v = grain + detail;
                    float r = Mathf.Lerp(0.55f, 0.7f, v);
                    float g = Mathf.Lerp(0.38f, 0.5f, v);
                    float b = Mathf.Lerp(0.18f, 0.28f, v);

                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }
            tex.Apply();
            return tex;
        }

        // ══════════════════════════════════════
        // TREE BARK
        // ══════════════════════════════════════

        /// <summary>Generates a tree bark texture (dark brown, rough).</summary>
        public static Texture2D GenerateBarkTexture(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n1 = Mathf.PerlinNoise(x * 0.06f, y * 0.15f);
                    float n2 = Mathf.PerlinNoise(x * 0.2f + 60, y * 0.4f + 60) * 0.2f;
                    float crack = Mathf.Abs(Mathf.Sin(y * 0.4f + n1 * 4f)) * 0.15f;

                    float v = n1 + n2 - crack;
                    float r = Mathf.Lerp(0.2f, 0.38f, v);
                    float g = Mathf.Lerp(0.12f, 0.22f, v);
                    float b = Mathf.Lerp(0.05f, 0.12f, v);

                    tex.SetPixel(x, y, new Color(
                        Mathf.Clamp01(r),
                        Mathf.Clamp01(g),
                        Mathf.Clamp01(b)
                    ));
                }
            }
            tex.Apply();
            return tex;
        }

        // ══════════════════════════════════════
        // UTILITY: Create Material from Texture
        // ══════════════════════════════════════

        /// <summary>Creates a simple unlit-like material from a texture.</summary>
        public static Material CreateMaterial(Texture2D texture, Color? tint = null)
        {
            // Use Standard shader (available in all render pipelines as fallback)
            var mat = new Material(Shader.Find("Standard"));
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            {
                // Fallback: try Universal Render Pipeline/Lit
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            {
                // Last resort: use default
                mat = new Material(Shader.Find("Diffuse"));
            }

            mat.mainTexture = texture;
            if (tint.HasValue)
                mat.color = tint.Value;

            // Make it less shiny for survival look
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", 0.1f);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", 0f);
            if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", 0.1f);

            return mat;
        }

        /// <summary>Creates a simple colored material without texture.</summary>
        public static Material CreateColorMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Standard"));
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Diffuse"));

            mat.color = color;
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", 0.1f);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", 0f);

            return mat;
        }
    }
}
