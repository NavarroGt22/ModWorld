using System;
using System.Collections.Generic;
using UnityEngine;
using ArcaneMartialAscension.GameIntegration;

namespace ArcaneMartialAscension.VisualEffects
{
    // Screen-space fallback: IMGUI already renders our control panel on WorldBox 0.51.2.
    // Unlike SpriteRenderer, this does not rely on the map camera's sorting layers.
    public static class ScreenEffects
    {
        private sealed class Effect
        {
            public Vector2 MouseScreen; // GUI coordinate system (top-left)
            public Vector3 World;
            public bool HasWorld;
            public int Kind;
            public Color Tint;
            public float Started;
            public float Duration;
            public string Label;
        }
        private const int Limit = 32;
        private static readonly List<Effect> live = new List<Effect>();
        private static Texture2D circle, slash, aura, bolt;
        private static float lastCombatFx;

        private static Texture2D Make(int kind)
        {
            const int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx=x-63.5f, dy=y-63.5f;
                float r=Mathf.Sqrt(dx*dx+dy*dy);
                float a=Mathf.Atan2(dy,dx);
                bool ink;
                if(kind==0)
                {
                    ink = Mathf.Abs(r-48)<2 || Mathf.Abs(r-38)<1.5f || Mathf.Abs(r-29)<1.3f ||
                        (r>52 && r<57 && Mathf.Abs(Mathf.Sin(a*8))<0.18f) ||
                        (Mathf.Abs(dy+22f)<1.5f && Mathf.Abs(dx)<38f) ||
                        (Mathf.Abs(dy-1.35f*Mathf.Abs(dx)+20f)<1.6f && dy<34 && dy>-20);
                }
                else if(kind==1) ink = a>-.85f && a<1.8f && Mathf.Abs(r-(45+3*Mathf.Sin(a*2f)))<4;
                else if(kind==2) ink = Mathf.Abs(r-47)<3 || Mathf.Abs(r-32)<1.6f ||
                        (r>31 && r<51 && Mathf.Abs(Mathf.Sin(a*6))<.12f);
                else ink = r<6 || (Mathf.Abs(dx)<2 && Mathf.Abs(dy)<38);
                pixels[y*size+x] = ink ? new Color32(255,255,255,255) : new Color32(0,0,0,0);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false,false);
            return tex;
        }
        private static void Ensure()
        {
            if(circle!=null) return;
            circle=Make(0); slash=Make(1); aura=Make(2); bolt=Make(3);
        }
        public static void Emit(object source, int kind, Color tint, string label, bool combat)
        {
            if(combat && Time.time-lastCombatFx<.14f) return;
            if(combat) lastCombatFx=Time.time;
            Ensure();
            if(live.Count>=Limit) live.RemoveAt(0);
            Vector3 pos;
            bool has=GameCompat.GetPosition(source,out pos);
            live.Add(new Effect {
                MouseScreen=new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y),
                World=pos, HasWorld=has, Kind=kind, Tint=tint,
                Started=Time.time, Duration=kind==0?1.5f:kind==1?.65f:.95f,
                Label=label
            });
        }
        // Unconditional UI-only test; useful to isolate map coordinate/sorting failures.
        public static void Test()
        {
            Emit(null,0,new Color(1f,.7f,.18f,1f),"VFX GUI TESTE",false);
        }
        private static Vector2 Location(Effect e)
        {
            if(e.HasWorld)
            {
                Camera c=Camera.main;
                if(c==null && Camera.allCamerasCount>0) c=Camera.allCameras[0];
                if(c!=null)
                {
                    Vector3 p=c.WorldToScreenPoint(e.World);
                    if(p.z>=0 && p.x>=20 && p.x<=Screen.width-20 &&
                        p.y>=20 && p.y<=Screen.height-20)
                        return new Vector2(p.x, Screen.height-p.y);
                }
            }
            return e.MouseScreen;
        }
        public static void Draw()
        {
            if(live.Count==0) return;
            Ensure();
            for(int i=live.Count-1;i>=0;i--)
            {
                Effect e=live[i];
                float t=(Time.time-e.Started)/e.Duration;
                if(t>=1f) {live.RemoveAt(i);continue;}
                Vector2 center=Location(e);
                float sz=e.Kind==0?130f:e.Kind==1?108f:116f;
                sz*=.80f+.30f*t;
                Color col=e.Tint;
                col.a=(1f-t)*.97f;
                Texture2D tex=e.Kind==0?circle:e.Kind==1?slash:e.Kind==2?aura:bolt;
                Color old=GUI.color;
                GUI.color=col;
                Matrix4x4 previous=GUI.matrix;
                GUIUtility.RotateAroundPivot((e.Kind==0?70f:180f)*t,center);
                GUI.DrawTexture(new Rect(center.x-sz/2,center.y-sz/2,sz,sz),tex);
                GUI.matrix=previous;
                GUI.color=old;
                if(t<.6f && !string.IsNullOrEmpty(e.Label))
                {
                    GUI.Label(new Rect(center.x-110,center.y+sz/2+1,220,25),e.Label);
                }
            }
        }
        public static void Clear()
        {
            live.Clear();
            if(circle!=null) UnityEngine.Object.Destroy(circle);
            if(slash!=null) UnityEngine.Object.Destroy(slash);
            if(aura!=null) UnityEngine.Object.Destroy(aura);
            if(bolt!=null) UnityEngine.Object.Destroy(bolt);
            circle=null;slash=null;aura=null;bolt=null;
        }
    }
}
