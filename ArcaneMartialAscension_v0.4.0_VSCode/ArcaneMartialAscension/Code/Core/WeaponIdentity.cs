using System;

namespace ArcaneMartialAscension.Core
{
    public static class WeaponIdentity
    {
        public static bool TryClassify(string id,out WeaponType weapon)
        {
            weapon=WeaponType.Sword;
            if(string.IsNullOrWhiteSpace(id))return false;
            string[] parts=id.ToLowerInvariant().Split(new[]{'_','-','.',' '},StringSplitOptions.RemoveEmptyEntries);
            foreach(string part in parts)
            {
                switch(part)
                {
                    case "sword":case "longsword":case "espada":weapon=WeaponType.Sword;return true;
                    case "spear":case "lance":case "pike":case "lanca":weapon=WeaponType.Spear;return true;
                    case "bow":case "longbow":case "crossbow":case "arco":weapon=WeaponType.Bow;return true;
                    case "axe":case "battleaxe":case "machado":weapon=WeaponType.Axe;return true;
                }
            }
            return false;
        }
    }
}
