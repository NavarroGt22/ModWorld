using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using Weapon=ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.GameIntegration
{
    // Observe health changes after native weapon hits; attack attempts award no XP.
    public static class WeaponHitObserver
    {
        private sealed class Shot
        {
            public long ActorId;
            public Weapon? Weapon;
        }
        private static Harmony harmony;
        private static ConditionalWeakTable<Projectile,Shot> shots=new ConditionalWeakTable<Projectile,Shot>();
        [ThreadStatic] private static Shot currentShot;
        private static float nextWarning;
        public static void Register()
        {
            if(harmony!=null)return;
            var patcher=new Harmony("com.arcane.martial.ascension.weapon_hits");
            try
            {
                MethodInfo getHit=typeof(Actor).GetMethod("getHit",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,
                    new[]{typeof(float),typeof(bool),typeof(AttackType),typeof(BaseSimObject),typeof(bool),typeof(bool),typeof(bool)},null);
                if(getHit==null)throw new MissingMethodException("Actor.getHit/7");
                patcher.Patch(getHit,prefix:Patch("BeforeHit"),postfix:Patch("AfterHit"));
                patcher.Patch(Find(typeof(Projectile),"start"),postfix:Patch("RememberShot"));
                patcher.Patch(Find(typeof(Projectile),"checkHitForUnit"),prefix:Patch("BeforeProjectileHit"),finalizer:Patch("AfterProjectileHit"));
                harmony=patcher;
                Debug.Log("[AMA Martial] Observador de acertos nativos registrado; arma de projéteis capturada no lançamento.");
            }
            catch {patcher.UnpatchSelf();throw;}
        }
        private static MethodInfo Find(Type type,string name)
        {return type.GetMethod(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)??throw new MissingMethodException(type.Name,name);}
        private static HarmonyMethod Patch(string name)
        {return new HarmonyMethod(typeof(WeaponHitObserver).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic));}
        private static void BeforeHit(Actor __instance,out int __state)
        {
            __state=0;
            try {if(__instance!=null && __instance.isAlive())__state=__instance.getHealth();}
            catch(Exception error){Warn(error);}
        }
        private static void AfterHit(Actor __instance,AttackType __2,BaseSimObject __3,int __state)
        {
            if(__2!=AttackType.Weapon || CombatBridge.IsApplyingGeneratedDamage || __state<=0 || __instance==null || __3==null || __3.a==null)return;
            try
            {
                Actor source=__3.a;
                int loss=Math.Max(0,__state-Math.Max(0,__instance.getHealth()));
                if(loss<=0 || source==__instance || !source.isAlive())return;
                Weapon weapon;
                if(currentShot!=null)
                {
                    if(currentShot.ActorId!=source.getID() || !currentShot.Weapon.HasValue)return;
                    weapon=currentShot.Weapon.Value;
                }
                else if(!GameWeaponDetector.TryDetect(source,out weapon))return;
                ActorAbilities.OnConfirmedWeaponHit(source,__instance,weapon,loss);
            }
            catch(Exception error)
            {
                Warn(error);
            }
        }
        private static void RememberShot(Projectile __instance,BaseSimObject __0)
        {
            try
            {
                shots.Remove(__instance);
                Actor source=__0==null?null:__0.a;
                Weapon weapon;
                shots.Add(__instance,new Shot{ActorId=source!=null && source.isAlive()?source.getID():0,
                    Weapon=source!=null && source.isAlive() && GameWeaponDetector.TryDetect(source,out weapon)?(Weapon?)weapon:null});
            }
            catch(Exception error){Warn(error);}
        }
        private static void BeforeProjectileHit(Projectile __instance,out Shot __state)
        {
            __state=currentShot;
            Shot shot;
            currentShot=shots.TryGetValue(__instance,out shot)?shot:new Shot();
        }
        private static Exception AfterProjectileHit(Exception __exception,Shot __state)
        {currentShot=__state;return __exception;}
        private static void Warn(Exception error)
        {
            if(Time.unscaledTime<nextWarning)return;
            nextWarning=Time.unscaledTime+10;Debug.LogError("[AMA Martial] Falha no observador de acertos: "+error);
        }
        public static void ResetWorld(){shots=new ConditionalWeakTable<Projectile,Shot>();currentShot=null;}
        public static void Unregister(){if(harmony!=null)harmony.UnpatchSelf();harmony=null;ResetWorld();}
    }
}
