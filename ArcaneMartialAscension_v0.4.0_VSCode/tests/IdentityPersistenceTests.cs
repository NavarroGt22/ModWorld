using System;
using System.Linq;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Learning;
using ArcaneMartialAscension.Martial;
using ArcaneMartialAscension.Persistence;
using ArcaneMartialAscension.Spells;
using Weapon=ArcaneMartialAscension.Core.WeaponType;

internal static class IdentityPersistenceTests
{
    private const string Drain="ama.spell.necromancy.1.dreno_de_vigor";
    private static void Assert(bool ok,string error){if(!ok)throw new Exception(error);}
    private static CharacterProgression Trained()
    {
        var p=new CharacterProgression();
        for(int t=0;t<=64;t++)NaturalMagicRules.Observe(p,new FormationContext{ActorId=42,Eligible=true,Available=true,WorldTime=t,ExistingTradition=ArcaneTradition.Warlock});
        p.Arcane.SetPatron("ama.patron.fiend");p.Arcane.SetOrigin(SorcerousOrigin.Draconic);
        p.Arcane.Study(ArcaneTradition.Warlock,100);p.Arcane.Study(ArcaneTradition.Wizard,300);
        p.StudyMagic(MagicSchool.Necromancy,77);p.TrainWeapon(Weapon.Sword,100);p.TrainWeapon(Weapon.Bow,35);
        LearningRules.Learn(p,SpellCatalog.Find(Drain),KnowledgeSource.Pact,80,42,ArcaneTradition.Warlock);
        TechniqueRules.LearnFromTraining(p,Weapon.Sword,KnowledgeSource.Practice,90,42);
        var entry=TechniqueCatalog.ForWeapon(Weapon.Sword)[0];for(int i=0;i<5;i++)TechniqueRules.ConfirmHit(p,entry,Weapon.Sword,100+i*4,3);
        p.GrowMana(p.Arcane.HighestCircle);p.CastSpell(17);p.AwakenAura();p.UseAura(7);return p;
    }
    private static ProgressionSnapshot Snapshot()
    {
        var s=ProgressionCodec.Capture(Trained(),42,117);s.Spells[0].Remaining=1.5;s.PreferredSpell=Drain;
        s.Buffs.Add(new SavedBuff{Id="ama_spell_shield",Remaining=3});return s;
    }
    private static SnapshotStatus Load(ProgressionSnapshot s,out CharacterProgression p,out ProgressionSnapshot decoded)
    {string error;return ProgressionCodec.Load(ProgressionCodec.Encode(s),42,1000,out p,out decoded,out error);}
    private static void Reject(ProgressionSnapshot s)
    {CharacterProgression p;ProgressionSnapshot decoded;Assert(Load(s,out p,out decoded)==SnapshotStatus.Invalid && p==null,"Invalid snapshot accepted or partially published");}
    public static void Run(Action<string,Action> check)
    {
        check("identities: four official patron archetypes and six stable origin profiles have unique IDs",()=>{
            Assert(ArcaneIdentities.All.Count(x=>x.IsPatron)==4 && ArcaneIdentities.All.Count(x=>!x.IsPatron)==6,"Wrong profile count");
            Assert(ArcaneIdentities.All.Select(x=>x.Id).Distinct().Count()==10 && ArcaneIdentities.All.Select(x=>x.TraitId).Distinct().Count()==10,"Duplicate profile ID");
            Assert(ArcaneIdentities.Lineage(SorcerousOrigin.Fey)!=null && ArcaneIdentities.Lineage(SorcerousOrigin.Elemental)!=null,"Legacy origins lost");
        });
        check("identities: every profile curriculum learns four real early spells in its own tradition",()=>{
            foreach(var identity in ArcaneIdentities.All)
            {
                var p=new CharacterProgression();var tradition=identity.IsPatron?ArcaneTradition.Warlock:ArcaneTradition.Sorcerer;
                if(identity.IsPatron)p.Arcane.SetPatron(identity.Id);else p.Arcane.SetOrigin(identity.Origin);
                var context=new LearningContext{ActorId=42,Eligible=true,Available=true,MagicEligible=true,Tradition=tradition};
                for(int i=0;i<=700;i++){context.WorldTime=i;LearningRules.Tick(p,context);}
                Assert(p.Arcane.KnownSpellIds.Count()==4 && identity.Curriculum.All(p.Arcane.Knows),"Wrong curriculum: "+identity.Id);
                Assert(identity.Curriculum.All(id=>SpellCatalog.HasRuntime(SpellCatalog.Find(id)) && SpellCatalog.Find(id).Circle<=1),"Unsupported curriculum");
                Assert(p.Arcane.GetExperience(ArcaneTradition.Wizard)==0,"Borrowed Wizard XP");
            }
        });
        check("identities: natural selections are deterministic and cover all four main paths",()=>{
            var patrons=new System.Collections.Generic.HashSet<string>();var origins=new System.Collections.Generic.HashSet<SorcerousOrigin>();
            for(int id=1;id<10000;id++)
            {
                if(NaturalMagicRules.Vocation(id)==ArcaneTradition.Warlock)patrons.Add(NaturalMagicRules.PatronFor(id));
                if(NaturalMagicRules.Vocation(id)==ArcaneTradition.Sorcerer)origins.Add(NaturalMagicRules.OriginFor(id));
                Assert(NaturalMagicRules.PatronFor(id)==NaturalMagicRules.PatronFor(id) && NaturalMagicRules.OriginFor(id)==NaturalMagicRules.OriginFor(id),"Selection rerolled");
            }
            Assert(patrons.Count==4 && origins.SetEquals(new[]{SorcerousOrigin.Draconic,SorcerousOrigin.Wild,SorcerousOrigin.Aberrant,SorcerousOrigin.Clockwork}),"Missing natural identity");
        });
        check("identities: legacy patrons migrate without changing custom patron IDs",()=>{
            var p=new CharacterProgression();p.Arcane.SetPatron("ama.patron.ember");Assert(p.Arcane.PatronId=="ama.patron.fiend","Ember not migrated");
            p.Arcane.SetPatron("ama.patron.veil");Assert(p.Arcane.PatronId=="ama.patron.great_old_one","Veil not migrated");
            p.Arcane.SetPatron("custom.patron");Assert(p.Arcane.PatronId=="custom.patron" && ArcaneIdentities.Patron(p.Arcane.PatronId)==null,"Custom patron fabricated");
        });
        check("identities: reserve bonuses never stack on refresh or refill current mana",()=>{
            var p=new CharacterProgression();p.Arcane.SetPatron("ama.patron.fiend");p.Arcane.SetOrigin(SorcerousOrigin.Draconic);p.CastSpell(20);
            for(int i=0;i<100;i++)p.GrowMana(1);
            Assert(p.MaxMana==155 && p.Mana==80,"Stacking or free mana");
            Assert(ArcaneIdentities.Patron("ama.patron.fiend").Attributes.Damage>ArcaneIdentities.Patron("ama.patron.celestial").Attributes.Damage,"Patron identity lost");
        });
        check("persistence: full round trip preserves resources, XP, pact, lineage, spells and techniques",()=>{
            var source=Trained();CharacterProgression p;ProgressionSnapshot decoded;Assert(Load(Snapshot(),out p,out decoded)==SnapshotStatus.Restored,"Round trip failed");
            Assert(p.Mana==source.Mana && p.MaxMana==source.MaxMana && p.Aura==43 && p.HasAwakenedAura,"Resources reset");
            Assert(p.GetWeaponExperience(Weapon.Sword)==100 && p.GetWeaponExperience(Weapon.Bow)==35 && p.GetMagicExperience(MagicSchool.Necromancy)==77,"XP lost");
            Assert(p.Arcane.GetExperience(ArcaneTradition.Wizard)==300 && p.Arcane.GetExperience(ArcaneTradition.Warlock)==100,"Traditions mixed");
            Assert(p.Arcane.PatronId=="ama.patron.fiend" && p.Arcane.Origin==SorcerousOrigin.Draconic && p.Arcane.TraditionFor(Drain)==ArcaneTradition.Warlock,"Identity lost");
            Assert(p.Formation.Ready && p.Formation.Tradition==ArcaneTradition.Warlock && p.Techniques.Get(TechniqueCatalog.ForWeapon(Weapon.Sword)[0].Id).Rank=="II","Formation or rank lost");
            Assert(p.GetOrigin(Drain).Source==KnowledgeSource.Pact && p.GetOrigin(Drain).ActorId==42 && decoded.PreferredSpell==Drain,"Knowledge source lost");
        });
        check("persistence: spell and technique cooldowns resume at the loaded simulation clock",()=>{
            CharacterProgression p;ProgressionSnapshot s;Load(Snapshot(),out p,out s);var session=new SpellCastingSession(p);
            session.RestoreCooldown(SpellCatalog.Find(Drain),s.Spells[0].Remaining,1000);
            Assert(session.CooldownRemaining(SpellCatalog.Find(Drain),1000)==1.5 && session.CooldownRemaining(SpellCatalog.Find(Drain),1001.5)==0,"Spell cooldown reset");
            var technique=TechniqueCatalog.ForWeapon(Weapon.Sword)[0];Assert(!TechniqueRules.CanUse(p,technique,Weapon.Sword,1000) && TechniqueRules.CanUse(p,technique,Weapon.Sword,1002),"Technique cooldown reset");
        });
        check("persistence: partial formation survives but cannot bank offline time",()=>{
            var p=new CharacterProgression();var c=new FormationContext{ActorId=42,Eligible=true,Available=true,ExistingTradition=ArcaneTradition.Warlock};
            for(int i=0;i<=15;i++){c.WorldTime=i;NaturalMagicRules.Observe(p,c);}
            var snapshot=ProgressionCodec.Capture(p,42,15);CharacterProgression restored;ProgressionSnapshot decoded;Load(snapshot,out restored,out decoded);
            c.WorldTime=1000;NaturalMagicRules.Observe(restored,c);Assert(restored.Formation.Work==15 && !restored.Formation.Ready,"Offline formation credited");
        });
        check("persistence: partial study survives without retroactive study or practice XP",()=>{
            var p=new CharacterProgression();var c=new LearningContext{ActorId=42,Eligible=true,Available=true,MagicEligible=true,Weapon=Weapon.Sword};
            for(int i=0;i<=17;i++){c.WorldTime=i;LearningRules.Tick(p,c);}
            var snapshot=ProgressionCodec.Capture(p,42,17);CharacterProgression restored;ProgressionSnapshot decoded;Load(snapshot,out restored,out decoded);
            c.WorldTime=1000;LearningRules.Tick(restored,c);var after=ProgressionCodec.Capture(restored,42,1000);
            Assert(restored.Learning.StudyProgress==p.Learning.StudyProgress && restored.GetWeaponExperience(Weapon.Sword)==p.GetWeaponExperience(Weapon.Sword),"Offline XP credited");
            Assert(after.PracticeClock==snapshot.PracticeClock && after.StudyClock==snapshot.StudyClock,"Partial work discarded");
        });
        check("persistence: foreign Actor data is never inherited by a clone",()=>{
            CharacterProgression p;ProgressionSnapshot decoded;string error;
            Assert(ProgressionCodec.Load(ProgressionCodec.Encode(Snapshot()),999,1000,out p,out decoded,out error)==SnapshotStatus.ForeignActor && p==null,"Shared clone powers");
        });
        check("persistence: future and unknown schema versions are preserved as unsupported",()=>{
            foreach(int version in new[]{0,2,999}){var s=Snapshot();s.Version=version;CharacterProgression p;ProgressionSnapshot decoded;
                Assert(Load(s,out p,out decoded)==SnapshotStatus.Unsupported && p==null,"Unsupported version accepted");}
        });
        check("persistence: missing data means untrained and malformed JSON is rejected",()=>{
            CharacterProgression p;ProgressionSnapshot decoded;string error;
            Assert(ProgressionCodec.Load(null,42,1000,out p,out decoded,out error)==SnapshotStatus.Missing && p==null,"Missing data grants powers");
            Assert(ProgressionCodec.Load("{broken",42,1000,out p,out decoded,out error)==SnapshotStatus.Invalid && p==null,"Malformed JSON accepted");
        });
        check("persistence: oversized and deeply nested payloads are bounded",()=>{
            CharacterProgression p;ProgressionSnapshot decoded;string error;
            Assert(ProgressionCodec.Load(new string('x',ProgressionCodec.MaxLength+1),42,1000,out p,out decoded,out error)==SnapshotStatus.Invalid,"Size limit missing");
            string deep="{\"Version\":1,\"ActorId\":42,\"x\":"+new string('[',30)+"0"+new string(']',30)+"}";
            Assert(ProgressionCodec.Load(deep,42,1000,out p,out decoded,out error)==SnapshotStatus.Invalid,"Depth limit missing");
        });
        check("persistence: invalid XP rejects the whole snapshot atomically",()=>{
            var s=Snapshot();s.WeaponXp[0]=-1;Reject(s);s=Snapshot();s.TraditionXp[1]=1000001;Reject(s);
            s=Snapshot();s.SchoolXp=new int[7];Reject(s);
        });
        check("persistence: unknown and duplicate spells cannot enter a repertoire",()=>{
            var s=Snapshot();s.Spells[0].Id="unknown.spell";Reject(s);
            s=Snapshot();s.Spells.Add(new SavedSpell{Id=Drain,Tradition="warlock"});Reject(s);
        });
        check("persistence: spell prerequisites cannot borrow XP from another tradition",()=>{
            var s=Snapshot();s.TraditionXp[1]=0;Reject(s);
            s=Snapshot();s.PatronId=null;Reject(s);
        });
        check("persistence: invalid lineage and incomplete power sources are rejected",()=>{
            var s=Snapshot();s.OriginId="unknown.origin";Reject(s);
            s=Snapshot();s.FormationTradition="sorcerer";s.OriginId=null;Reject(s);
        });
        check("persistence: legacy patron IDs migrate inside otherwise valid snapshots",()=>{
            var s=Snapshot();s.PatronId="ama.patron.ember";CharacterProgression p;ProgressionSnapshot decoded;
            Assert(Load(s,out p,out decoded)==SnapshotStatus.Restored && p.Arcane.PatronId=="ama.patron.fiend","Snapshot migration missing");
        });
        check("persistence: technique rank cannot bypass equipment XP or confirmed uses",()=>{
            var s=Snapshot();s.WeaponXp[0]=0;Reject(s);s=Snapshot();s.Techniques[0].Uses=21;Reject(s);
            s=Snapshot();s.Techniques[0].Id=TechniqueCatalog.ForWeapon(Weapon.Sword)[1].Id;Reject(s);
        });
        check("persistence: cooldown bounds cannot create negative or endless cooldowns",()=>{
            var s=Snapshot();s.Spells[0].Remaining=-1;Reject(s);s=Snapshot();s.Spells[0].Remaining=999;Reject(s);
            s=Snapshot();s.Techniques[0].Remaining=999;Reject(s);
        });
        check("persistence: only owned temporary buff types and finite lifetimes can restore",()=>{
            var s=Snapshot();s.Buffs[0].Id="immortal";Reject(s);s=Snapshot();s.Buffs[0].Remaining=0;Reject(s);
            s=Snapshot();s.Buffs[0].Remaining=100000;Reject(s);s=Snapshot();s.Buffs.Add(new SavedBuff{Id="ama_spell_shield",Remaining=1});Reject(s);
        });
        check("persistence: knowledge origins remain attached to their original actor",()=>{
            var s=Snapshot();s.Origins[0].ActorId=999;Reject(s);s=Snapshot();s.Origins[0].Source="invented";Reject(s);
        });
        check("persistence: prepared unknown spells and invalid study state are rejected",()=>{
            var s=Snapshot();s.PreferredSpell="unknown.spell";Reject(s);s=Snapshot();s.StudyingSpell=Drain;Reject(s);
            s=Snapshot();s.LastWeapon=4;Reject(s);s=Snapshot();s.FormationTradition="unknown";Reject(s);
        });
        check("persistence: resource capacities are recomputed and old excess is clamped",()=>{
            var s=Snapshot();s.Mana=999;s.Aura=999;CharacterProgression p;ProgressionSnapshot decoded;
            Assert(Load(s,out p,out decoded)==SnapshotStatus.Restored && p.Mana==p.MaxMana && p.Aura==p.MaxAura,"Unbounded capacity");
        });
        check("persistence: loading the same snapshot twice produces independent characters",()=>{
            CharacterProgression a,b;ProgressionSnapshot decoded;Load(Snapshot(),out a,out decoded);Load(Snapshot(),out b,out decoded);
            a.TrainWeapon(Weapon.Sword,100);a.CastSpell(5);Assert(b.GetWeaponExperience(Weapon.Sword)==100 && a.Mana==b.Mana-5,"Shared restored state");
        });
    }
}
