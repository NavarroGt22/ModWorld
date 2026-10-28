using System;
using ArcaneMartialAscension.Core;
using MartialWeaponType=ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.GameIntegration
{
    public static class GameWeaponDetector
    {
        private static bool warned;
        public static bool TryDetect(Actor actor,out MartialWeaponType type)
        {
            type=MartialWeaponType.Sword;
            if(actor==null)return false;
            try
            {
                // Verified 0.51.2 API: equipment.weapon is a slot, not the item itself.
                Item item=actor.getWeapon();
                EquipmentAsset asset=item!=null?item.getAsset():null;
                return asset!=null && WeaponIdentity.TryClassify(asset.id,out type);
            }
            catch(Exception error)
            {
                if(!warned){warned=true;UnityEngine.Debug.LogWarning("[AMA Learning] Não foi possível ler a arma equipada: "+error);}
                return false;
            }
        }
    }
}