using System;
using System.Reflection;
using UnityEngine;

namespace ArcaneMartialAscension.GameIntegration
{
    // Native WorldBox 0.51.2 combat. Success requires a measured change in health.
    public static class CombatBridge
    {
        private const BindingFlags Flags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
        private static float nextTraitWarning;
        public static bool TryDamage(Actor target,float amount,Actor attacker=null)
        {
            if(target==null || !target.isAlive() || amount<=0 || float.IsNaN(amount) || float.IsInfinity(amount))return false;
            try
            {
                int before=target.getHealth();
                // Do not skip a spell just because a normal attack is shaking the victim.
                // Native damage keeps armor, get-hit hooks, death handling and attacker credit.
                target.getHit(amount,true,AttackType.Other,attacker,false,false,true);
                return target.getHealth()<before;
            }
            catch(Exception error)
            {
                Debug.LogError("[AMA Combat] Falha no dano nativo: "+error);
                return false;
            }
        }
        public static bool TryHeal(Actor target,float amount)
        {
            if(target==null || !target.isAlive() || amount<=0 || float.IsNaN(amount) || float.IsInfinity(amount))return false;
            try
            {
                int before=target.getHealth();
                target.restoreHealth(Mathf.CeilToInt(amount));
                return target.getHealth()>before;
            }
            catch(Exception error){Debug.LogError("[AMA Combat] Falha na cura nativa: "+error);return false;}
        }
        public static bool TryStatus(Actor target,string name,float duration)
        {
            if(target==null || !target.isAlive()) return false;
            string[] methodNames={"addStatusEffect","addStatus","addStatusEffectById"};
            foreach(string methodName in methodNames)
            foreach(MethodInfo method in target.GetType().GetMethods(Flags))
            {
                if(method.Name!=methodName) continue;
                ParameterInfo[] ps=method.GetParameters();
                if(ps.Length!=2 || ps[0].ParameterType!=typeof(string)||
                  !(ps[1].ParameterType==typeof(float)||ps[1].ParameterType==typeof(int))) continue;
                try{method.Invoke(target,new object[]{name,Convert.ChangeType(duration,ps[1].ParameterType)});return true;}
                catch(Exception) { }
            }
            return false;
        }
        public static bool RemoveTrait(Actor actor,string id)
        {
            if(actor==null || string.IsNullOrEmpty(id))return false;
            try
            {
                actor.removeTrait(id);
                actor.setStatsDirty();
                return !actor.hasTrait(id);
            }
            catch(Exception error)
            {
                if(Time.unscaledTime>=nextTraitWarning)
                {nextTraitWarning=Time.unscaledTime+10f;Debug.LogWarning("[AMA Traits] Não foi possível remover "+id+": "+error);}
                return false;
            }
        }
    }
}
