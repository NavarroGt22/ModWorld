using System;
using System.Collections.Generic;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Learning;
using Weapon=ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.Martial
{
    public sealed class TechniqueKnowledge
    {
        public int SuccessfulUses {get;internal set;}
        public int Level {get{return SuccessfulUses>=20?3:SuccessfulUses>=5?2:1;}}
        public double LastUsed {get;internal set;}=double.NegativeInfinity;
        public string Rank {get{return Level==3?"III":Level==2?"II":"I";}}
    }
    public sealed class TechniqueProgression
    {
        private readonly Dictionary<string,TechniqueKnowledge> known=new Dictionary<string,TechniqueKnowledge>(StringComparer.Ordinal);
        internal IEnumerable<KeyValuePair<string,TechniqueKnowledge>> Known {get{return known;}}
        public TechniqueKnowledge Get(string id)
        {TechniqueKnowledge result;return id!=null && known.TryGetValue(id,out result)?result:null;}
        internal bool Learn(TechniqueEntry entry)
        {if(known.ContainsKey(entry.Id))return false;known.Add(entry.Id,new TechniqueKnowledge());return true;}
    }
    public static class TechniqueRules
    {
        public static int LearnFromTraining(CharacterProgression progress,Weapon weapon,KnowledgeSource source,double now,long actorId)
        {
            if(progress==null || (int)weapon<0 || (int)weapon>3 || actorId<=0 || now<0 || double.IsNaN(now) || double.IsInfinity(now))return 0;
            int learned=0;
            foreach(TechniqueEntry entry in TechniqueCatalog.ForWeapon(weapon))
                if(entry.HasRuntime && progress.GetWeaponLevel(weapon)>=entry.RequiredLevel && progress.Techniques.Learn(entry))
                {progress.RecordOrigin(entry.Id,new KnowledgeOrigin(source,now,actorId));learned++;}
            return learned;
        }
        public static bool CanUse(CharacterProgression progress,TechniqueEntry entry,Weapon? equipped,double now)
        {
            if(progress==null || entry==null || !entry.HasRuntime || equipped!=entry.Weapon ||
                now<0 || double.IsNaN(now) || double.IsInfinity(now) || progress.GetWeaponLevel(entry.Weapon)<entry.RequiredLevel)return false;
            TechniqueKnowledge knowledge=progress.Techniques.Get(entry.Id);
            return knowledge!=null && now-knowledge.LastUsed>=entry.Cooldown;
        }
        public static float Damage(CharacterProgression progress,TechniqueEntry entry)
        {
            TechniqueKnowledge knowledge=entry==null || progress==null?null:progress.Techniques.Get(entry.Id);
            return knowledge==null?0:entry.Damage*(1+.25f*(knowledge.Level-1));
        }
        public static bool ConfirmHit(CharacterProgression progress,TechniqueEntry entry,Weapon? equipped,double now,int healthLost)
        {
            if(healthLost<=0 || !CanUse(progress,entry,equipped,now))return false;
            TechniqueKnowledge knowledge=progress.Techniques.Get(entry.Id);
            knowledge.LastUsed=now;knowledge.SuccessfulUses=Math.Min(20,knowledge.SuccessfulUses+1);
            return true;
        }
    }
}
