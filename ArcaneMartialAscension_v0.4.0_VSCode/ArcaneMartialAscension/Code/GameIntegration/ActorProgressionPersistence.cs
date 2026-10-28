using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ArcaneMartialAscension.GameIntegration
{
    public static class ActorProgressionPersistence
    {
        public const string Key="ama.progression.v1";
        private static Harmony harmony;
        private static float nextWarning;
        public static void Register()
        {
            if(harmony!=null)return;
            MethodInfo method=typeof(Actor).GetMethod("prepareForSave",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,Type.EmptyTypes,null);
            if(method==null)throw new MissingMethodException("Actor.prepareForSave/0");
            var patcher=new Harmony("com.arcane.martial.ascension.persistence");
            try
            {
                patcher.Patch(method,prefix:new HarmonyMethod(typeof(ActorProgressionPersistence).GetMethod("BeforeSave",BindingFlags.Static|BindingFlags.NonPublic)));
                harmony=patcher;Debug.Log("[AMA Save] Persistência por Actor registrada antes de prepareForSave.");
            }
            catch{patcher.UnpatchSelf();throw;}
        }
        private static void BeforeSave(Actor __instance)
        {
            try {ActorAbilities.SaveForActor(__instance);}
            catch(Exception error)
            {
                if(Time.unscaledTime<nextWarning)return;
                nextWarning=Time.unscaledTime+5;
                Debug.LogError("[AMA Save] Progresso não atualizado; salvamento nativo continua: "+error);
            }
        }
        public static void Unregister(){if(harmony!=null)harmony.UnpatchSelf();harmony=null;}
    }
}
