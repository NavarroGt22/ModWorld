using System;
using System.Collections.Generic;
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
        private Sprite boltSprite;
        private Sprite impactSprite;
        private Sprite[] slashFrames,beamFrames;
        private readonly List<FxSpriteAnimation> pool=new List<FxSpriteAnimation>();
        private int activeEffects;
        private const int MaxEffects = 48;

        private void Awake()
        {
            circleSprite = MakeSprite(96, 0);
            swordSprite = MakeSprite(96, 1);
            auraSprite = MakeSprite(96, 2);
            boltSprite = MakeSprite(64, 3);
            impactSprite=MakeSprite(64,5);
            slashFrames=new[]{MakeSprite(96,1,0),MakeSprite(96,1,1),MakeSprite(96,1,2)};
            beamFrames=new[]{MakeSprite(96,4,0),MakeSprite(96,4,1),MakeSprite(96,4,2)};
        }

        public Transform SpawnBolt(Vector3 point,Color color)
        {
            FxSpriteAnimation animation=Spawn(boltSprite,point,color,.6f,float.MaxValue,260f,0f);
            if(animation==null)return null;
            animation.LeaveTrail=true;return animation.transform;
        }
        public void Release(Transform effect)
        {if(effect!=null){FxSpriteAnimation animation=effect.GetComponent<FxSpriteAnimation>();if(animation!=null)animation.Finish();}}
        public void ClearEffects()
        {foreach(FxSpriteAnimation animation in pool)if(animation!=null)animation.Finish();}
        internal void SpawnTrail(Vector3 point,Color color)
        {Spawn(boltSprite,point,new Color(color.r,color.g,color.b,.45f),.22f,.24f,0,-.6f);}
        public void SpawnImpact(Vector3 point,Color color)
        {ScreenEffects.Emit(point,3,color,"",false);Spawn(impactSprite,point,color,.45f,.32f,30,2.2f);}
        public void SpawnLightning(Vector3 from,Vector3 to)
        {
            Vector3 delta=to-from;
            float distance=delta.magnitude;
            if(distance<.01f)return;
            ScreenEffects.Lightning(from,to);
            Color color=new Color(.5f,.8f,1f);
            FxSpriteAnimation beam=Spawn(beamFrames[0],(from+to)*.5f,color,1,.28f,0,0);
            if(beam!=null)
            {
                beam.transform.localScale=new Vector3(distance/(96f/48f),.7f,1);
                beam.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
                beam.Frames=beamFrames;
            }
            SpawnImpact(to,color);
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
        {SpawnSwordArc(center,center+Vector3.right);}
        public void SpawnSwordArc(Vector3 from,Vector3 target)
        {
            Vector3 direction=(target-from).normalized;
            if(direction.sqrMagnitude<.01f)direction=Vector3.right;
            Vector3 center=from+direction*.45f;
            ScreenEffects.Slash(from,target,new Color(1f,.8f,.2f));
            FxSpriteAnimation slash=Spawn(slashFrames[0],center,new Color(1f,.79f,.31f),1.4f,.4f,95f,.16f);
            if(slash!=null){slash.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-65);slash.Frames=slashFrames;}
        }

        public void SpawnAuraPulse(Vector3 center)
        {
            ScreenEffects.Emit(center, 2, new Color(0.2f,0.9f,1f), "AURA", false);
            Spawn(auraSprite, center, new Color(0.3f, 0.94f, 1f, 0.95f), 1.3f, 0.9f, 15f, 0.8f);
        }

        private FxSpriteAnimation Spawn(Sprite sprite, Vector3 point, Color color, float scale,
            float lifetime, float angularSpeed, float growth)
        {
            if (sprite == null || activeEffects >= MaxEffects) return null;
            FxSpriteAnimation animation=null;
            foreach(FxSpriteAnimation candidate in pool)if(candidate!=null && !candidate.IsRunning){animation=candidate;break;}
            if(animation==null)
            {
                GameObject created=new GameObject("AMA_VFX_Pooled");created.transform.SetParent(transform,false);
                created.AddComponent<SpriteRenderer>();animation=created.AddComponent<FxSpriteAnimation>();pool.Add(animation);
            }
            GameObject fx=animation.gameObject;
            fx.SetActive(true);
            fx.transform.position = new Vector3(point.x, point.y, point.z - 0.02f);
            fx.transform.rotation=Quaternion.identity;
            fx.transform.localScale = Vector3.one * scale;
            SpriteRenderer renderer = fx.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = 32000;
            animation.Initialize(this, renderer, lifetime, angularSpeed, growth, color);
            activeEffects++;
            return animation;
        }

        internal void Finished()
        {
            activeEffects = Mathf.Max(0, activeEffects - 1);
        }

        private Sprite MakeSprite(int size, int kind,int frame=1)
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
                        bool inSweep = angle > -.72f && angle < (frame==0?.25f:frame==1?1.82f:2.2f);
                        float edge = 27f + 4f * Mathf.Sin((angle + 0.72f) * 1.2f);
                        visible = inSweep && Mathf.Abs(distance - edge) < 3.5f;
                    }
                    else if(kind==2)
                    {
                        // Expanding aura wave, with four spokes.
                        visible = Mathf.Abs(distance - 33f) < 2.4f ||
                            (Mathf.Abs(distance - 25f) < 1.3f &&
                             Mathf.Abs(Mathf.Sin(angle * 4f)) < 0.25f);
                    }
                    else if(kind==4)
                    {
                        float jagged=Mathf.Sin((dx+frame*7)*.35f)*5+Mathf.Sin((dx-frame*5)*.8f)*2;
                        visible=Mathf.Abs(dy-jagged)<1.3f && Mathf.Abs(dx)<mid-1;
                    }
                    else if(kind==5)visible=distance<5 || (distance<25 && Mathf.Abs(Mathf.Sin(angle*6))<.11f);
                    else visible=distance<8f || (Mathf.Abs(dx)<2.5f && Mathf.Abs(dy)<25f) ||
                        (Mathf.Abs(dy)<2.5f && Mathf.Abs(dx)<20f);
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
            if (boltSprite != null) { Destroy(boltSprite.texture); Destroy(boltSprite); }
            if(impactSprite!=null){Destroy(impactSprite.texture);Destroy(impactSprite);}
            if(slashFrames!=null)foreach(Sprite frame in slashFrames){Destroy(frame.texture);Destroy(frame);}
            if(beamFrames!=null)foreach(Sprite frame in beamFrames){Destroy(frame.texture);Destroy(frame);}
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
        public bool IsRunning {get;private set;}
        public bool LeaveTrail {get;set;}
        public Sprite[] Frames {get;set;}
        private double lastWorldTime;
        private float trailClock;

        public void Initialize(CombatVisualController parent, SpriteRenderer target,
            float lifetime, float angularSpeed, float growth, Color color)
        {
            owner = parent;
            IsRunning=true;LeaveTrail=false;Frames=null;age=trailClock=0;
            lastWorldTime=World.world!=null?World.world.getCurWorldTime():0;
            renderer = target;
            length = lifetime;
            speed = angularSpeed;
            expansion = growth;
            baseColor = color;
        }

        private void Update()
        {
            if(!IsRunning || (World.world!=null && World.world.isPaused()))return;
            double now=World.world!=null?World.world.getCurWorldTime():0;
            float dt=World.world!=null?(float)Math.Max(0,now-lastWorldTime):Time.deltaTime;
            lastWorldTime=now;
            age += dt;
            float fraction = Mathf.Clamp01(age / length);
            transform.Rotate(0f, 0f, speed * dt);
            transform.localScale *= 1f + expansion * dt;
            if(Frames!=null && Frames.Length>0)renderer.sprite=Frames[Mathf.Min(Frames.Length-1,(int)(fraction*Frames.Length))];
            if(LeaveTrail && owner!=null && (trailClock+=dt)>=.08f){trailClock=0;owner.SpawnTrail(transform.position,baseColor);}
            Color fade = baseColor;
            fade.a = baseColor.a * (1f - fraction);
            if (renderer != null) renderer.color = fade;
            if (age >= length) Finish();
        }
        public void Finish()
        {
            if(!IsRunning)return;
            IsRunning=false;
            if(owner!=null)owner.Finished();
            gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            if(IsRunning && owner!=null)owner.Finished();
            IsRunning=false;
        }
    }
}
