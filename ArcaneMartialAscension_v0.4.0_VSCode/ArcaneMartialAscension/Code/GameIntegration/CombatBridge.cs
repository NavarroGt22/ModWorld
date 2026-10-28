using System;
using System.Reflection;
using UnityEngine;

namespace ArcaneMartialAscension.GameIntegration
{
    // Isolates unverified game internals instead of referring to removed members at compile time.
    // Damage fallback changes existing current HP but intentionally never forces death/kill attribution.
    public static class CombatBridge
    {
        private const BindingFlags Flags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
        private static object Read(object instance,string name)
        {
            if(instance==null) return null;
            try
            {
                PropertyInfo prop=instance.GetType().GetProperty(name,Flags);
                if(prop!=null && prop.GetIndexParameters().Length==0) return prop.GetValue(instance,null);
                FieldInfo field=instance.GetType().GetField(name,Flags);
                if(field!=null) return field.GetValue(instance);
            }
            catch(Exception) { }
            return null;
        }
        private static bool WriteNumber(object instance,string fieldName,float value)
        {
            if(instance==null) return false;
            try
            {
                FieldInfo f=instance.GetType().GetField(fieldName,Flags);
                if(f!=null && !f.IsInitOnly && IsNumber(f.FieldType))
                {f.SetValue(instance,Convert.ChangeType(value,f.FieldType));return true;}
                PropertyInfo p=instance.GetType().GetProperty(fieldName,Flags);
                if(p!=null && p.CanWrite && IsNumber(p.PropertyType))
                {p.SetValue(instance,Convert.ChangeType(value,p.PropertyType),null);return true;}
            }
            catch(Exception) { }
            return false;
        }
        private static bool IsNumber(Type t)
        {return t==typeof(float)||t==typeof(double)||t==typeof(int)||t==typeof(short);}
        private static bool Number(object obj,out float value)
        {
            value=0f;
            try {if(obj!=null) {value=Convert.ToSingle(obj);return !float.IsNaN(value);}}catch(Exception) { }
            return false;
        }
        public static bool TryDamage(Actor target,float amount)
        {
            if(target==null || !target.isAlive() || amount<=0) return false;
            // Exact, narrow one-parameter calls only; we do NOT blindly invoke arbitrary overloads.
            string[] names={"getHit","takeDamage","receiveDamage","damage"};
            foreach(string n in names)
            {
                MethodInfo[] methods=target.GetType().GetMethods(Flags);
                foreach(MethodInfo method in methods)
                {
                    if(method.Name!=n) continue;
                    ParameterInfo[] p=method.GetParameters();
                    if(p.Length!=1 || !IsNumber(p[0].ParameterType)) continue;
                    try {method.Invoke(target,new object[]{Convert.ChangeType(amount,p[0].ParameterType)});return true;}
                    catch(Exception) { }
                }
            }
            // Conservative fallback: reduce current ActorData.health, stop at 1 HP to avoid bypassing death flow.
            object data=Read(target,"data");
            float health;
            if(Number(Read(data,"health"),out health) && health>1f)
            {
                float remaining=Mathf.Max(1f,health-amount);
                return WriteNumber(data,"health",remaining);
            }
            return false;
        }
        public static bool TryHeal(Actor target,float amount)
        {
            if(target==null || !target.isAlive() || amount<=0) return false;
            object data=Read(target,"data");
            float current;
            if(!Number(Read(data,"health"),out current)) return false;
            float max;
            if(!Number(Read(data,"max_health"),out max))
                if(!Number(Read(data,"health_max"),out max)) return false;
            if(max<=current) return true;
            return WriteNumber(data,"health",Mathf.Min(max,current+amount));
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
            if(actor==null) return false;
            foreach(MethodInfo m in actor.GetType().GetMethods(Flags))
            {
                if(m.Name!="removeTrait" || m.GetParameters().Length!=1 ||
                   m.GetParameters()[0].ParameterType!=typeof(string)) continue;
                try {m.Invoke(actor,new object[]{id}); actor.setStatsDirty();return true;}
                catch(Exception){return false;}
            }
            return false;
        }
    }
}
