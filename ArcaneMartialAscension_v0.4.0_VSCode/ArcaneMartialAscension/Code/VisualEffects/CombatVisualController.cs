using System;
using UnityEngine;

namespace ArcaneMartialAscension.VisualEffects
{
    // SpriteRenderer-based VFX. Unlike LineRenderer, sprites don't rely on
    // a custom runtime line material, and use WorldBox/Unity's 2D pipeline.
    public sealed class CombatVisualController : MonoBehaviour
    {
        private Sprite circleSprite;
        private Sprite swordSprite;
        private Sprite auraSprite;
        private int activeEffects;
        private const int MaxEffects = 48;

        private void Awake()
        {
            circleSprite = MakeSprite(96, 0);
            swordSprite = MakeSprite(96, 1);
            auraSprite = MakeSprite(96, 2);
        }

        public void SpawnCircle(Vector3 center, int circle, Color color)
        {
            ScreenEffects.Emit(center, 0, color, "CIRCULO " + circle, false);
            if (circleSprite == null) return;
            int rings = Mathf.Clamp(1 + circle / 3, 1, 3);
            for (int i = 0; i < rings; i++)
                Spawn(circleSprite, center, color, 1.3f + i * 0.35f, 1.5f + i * 0.25f,
                    (i % 2 == 0 ? 1f : -1f) * (95f - 12f * i), 0.1f);
        }

        public void SpawnSwordArc(Vector3 center)
        {
            ScreenEffects.Emit(center, 1, new Color(1f,0.8f,0.2f), "CORTE", false);
            Spawn(swordSprite, center, new Color(1f, 0.79f, 0.31f, 1f), 1.4f, 0.45f, 285f, 0.16f);
        }

        public void SpawnAuraPulse(Vector3 center)
        {
            ScreenEffects.Emit(center, 2, new Color(0.2f,0.9f,1f), "AURA", false);
            Spawn(auraSprite, center, new Color(0.3f, 0.94f, 1f, 0.95f), 1.3f, 0.9f, 15f, 0.8f);
        }

        private void Spawn(Sprite sprite, Vector3 point, Color color, float scale,
            float lifetime, float angularSpeed, float growth)
        {
            if (sprite == null || activeEffects >= MaxEffects) return;
            GameObject fx = new GameObject("AMA_VFX");
            fx.transform.SetParent(transform, false);
            fx.transform.position = new Vector3(point.x, point.y, point.z - 0.02f);
            fx.transform.localScale = Vector3.one * scale;
            SpriteRenderer renderer = fx.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = 32000;
            FxSpriteAnimation animation = fx.AddComponent<FxSpriteAnimation>();
            animation.Initialize(this, renderer, lifetime, angularSpeed, growth, color);
            activeEffects++;
        }

        internal void Finished()
        {
            activeEffects = Mathf.Max(0, activeEffects - 1);
        }

        private Sprite MakeSprite(int size, int kind)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[size * size];
            float mid = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - mid;
                    float dy = y - mid;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    bool visible = false;
                    if (kind == 0)
                    {
                        // Triple concentric geometry with eight arcane rune ticks.
                        visible = Mathf.Abs(distance - 35) < 1.2f ||
                                  Mathf.Abs(distance - 25) < 0.85f ||
                                  (Mathf.Abs(distance - 42) < 1.2f &&
                                    Mathf.Abs(Mathf.Sin(angle * 8f)) < 0.2f);
                        // Triangle/rune edges inside the circle.
                        if (Mathf.Abs(dy + 13f) < 1.1f && Mathf.Abs(dx) < 21f) visible = true;
                        if (Mathf.Abs(dy - Mathf.Abs(dx) * 1.4f + 15f) < 1.2f && dy < 13f && dy > -17f)
                            visible = true;
                    }
                    else if (kind == 1)
                    {
                        // Crescent sword slash, facing top right.
                        bool inSweep = angle > -0.72f && angle < 1.82f;
                        float edge = 27f + 4f * Mathf.Sin((angle + 0.72f) * 1.2f);
                        visible = inSweep && Mathf.Abs(distance - edge) < 3.5f;
                    }
                    else
                    {
                        // Expanding aura wave, with four spokes.
                        visible = Mathf.Abs(distance - 33f) < 2.4f ||
                            (Mathf.Abs(distance - 25f) < 1.3f &&
                             Mathf.Abs(Mathf.Sin(angle * 4f)) < 0.25f);
                    }
                    pixels[y * size + x] = visible
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(0, 0, 0, 0);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), 48f);
        }

        private void OnDestroy()
        {
            if (circleSprite != null) { Destroy(circleSprite.texture); Destroy(circleSprite); }
            if (swordSprite != null) { Destroy(swordSprite.texture); Destroy(swordSprite); }
            if (auraSprite != null) { Destroy(auraSprite.texture); Destroy(auraSprite); }
        }
    }

    public sealed class FxSpriteAnimation : MonoBehaviour
    {
        private CombatVisualController owner;
        private SpriteRenderer renderer;
        private Color baseColor;
        private float length;
        private float age;
        private float speed;
        private float expansion;
        private bool done;

        public void Initialize(CombatVisualController parent, SpriteRenderer target,
            float lifetime, float angularSpeed, float growth, Color color)
        {
            owner = parent;
            renderer = target;
            length = lifetime;
            speed = angularSpeed;
            expansion = growth;
            baseColor = color;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float fraction = Mathf.Clamp01(age / length);
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
            transform.localScale *= 1f + expansion * Time.deltaTime;
            Color fade = baseColor;
            fade.a = baseColor.a * (1f - fraction);
            if (renderer != null) renderer.color = fade;
            if (age >= length) Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (!done && owner != null) { done = true; owner.Finished(); }
        }
    }
}
