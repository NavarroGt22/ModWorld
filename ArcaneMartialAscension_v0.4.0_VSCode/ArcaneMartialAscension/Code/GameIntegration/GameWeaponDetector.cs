using System;
using System.Reflection;
using ArcaneMartialAscension.Core;

namespace ArcaneMartialAscension.GameIntegration
{
    // Best-effort runtime inspection. No hardcoded unstable WorldBox 0.51.x equipment members.
    public static class GameWeaponDetector
    {
        private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
        private static object Read(object o,string name)
        {
            if(o==null)return null;
            try
            {
                PropertyInfo p=o.GetType().GetProperty(name,Flags);
                if(p!=null && p.GetIndexParameters().Length==0)return p.GetValue(o,null);
                FieldInfo f=o.GetType().GetField(name,Flags);
                if(f!=null)return f.GetValue(o);
            }
            catch(Exception) { }
            return null;
        }
        private static object GetWeapon(object equipment)
        {
            if(equipment==null)return null;
            object found=Read(equipment,"weapon")??Read(equipment,"main_weapon")??Read(equipment,"item_weapon");
            if(found!=null)return found;
            foreach(MethodInfo m in equipment.GetType().GetMethods(Flags))
            {
                if(m.Name!="getItem" && m.Name!="getEquipment" && m.Name!="getWeapon")continue;
                ParameterInfo[] p=m.GetParameters();
                try
                {
                    if(p.Length==0 && m.Name=="getWeapon")return m.Invoke(equipment,null);
                    if(p.Length==1 && p[0].ParameterType==typeof(string))
                    {
                        object value=m.Invoke(equipment,new object[]{"weapon"});
                        if(value!=null)return value;
                    }
                }
                catch(Exception) { }
            }
            return null;
        }
        public static bool TryDetect(Actor actor,out WeaponType type)
        {
            type=WeaponType.Sword;
            if(actor==null)return false;
            object equipment=Read(actor,"equipment")??Read(Read(actor,"data"),"equipment");
            object item=GetWeapon(equipment);
            if(item==null)return false;
            object asset=Read(item,"asset")??Read(item,"equipmentAsset")??item;
            object id=Read(asset,"id")??Read(asset,"name")??Read(item,"id")??Read(item,"name");
            if(id==null)return false;
            string name=id.ToString().ToLowerInvariant();
            if(name.Contains("spear") || name.Contains("lance")) {type=WeaponType.Spear;return true;}
            if(name.Contains("bow") || name.Contains("arco")) {type=WeaponType.Bow;return true;}
            if(name.Contains("axe") || name.Contains("machad")) {type=WeaponType.Axe;return true;}
            if(name.Contains("sword") || name.Contains("espada")) {type=WeaponType.Sword;return true;}
            return false;
        }
    }
}
