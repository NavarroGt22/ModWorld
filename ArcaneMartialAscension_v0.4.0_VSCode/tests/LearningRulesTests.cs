using System;
using System.Linq;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Learning;
using ArcaneMartialAscension.Spells;
using Weapon=ArcaneMartialAscension.Core.WeaponType;

internal static class LearningRulesTests
{
    private static int passed,failed;
    private const string Spark="ama.spell.evocation.0.centelha_rubra";
    private static void Assert(bool condition,string message)
    {if(!condition)throw new Exception(message);}
    private static void Check(string name,Action test)
    {
        try{test();passed++;Console.WriteLine("PASS: "+name);}
        catch(Exception error){failed++;Console.WriteLine("FAIL: "+name+" - "+error.Message);}
    }
    private static LearningContext Context(long id,bool magic=false,Weapon? weapon=null)
    {return new LearningContext{ActorId=id,Eligible=true,Available=true,MagicEligible=magic,Weapon=weapon};}
    private static void Advance(CharacterProgression progress,LearningContext context,int from,int to,int step=1)
    {for(int time=from;time<=to;time+=step){context.WorldTime=time;LearningRules.Tick(progress,context);}}

    private static int Main()
    {
        Check("fresh character has no free spell, XP, pact or innate origin",()=>{
            var p=new CharacterProgression();
            Assert(!p.Arcane.KnownSpellIds.Any() && p.Arcane.HighestCircle==0,"Unexpected starting powers");
            Assert(p.Arcane.PatronId==null && p.Arcane.Origin==SorcerousOrigin.None,"Fake pact/origin");
        });
        Check("first spell requires 32 observed seconds of study",()=>{
            var p=new CharacterProgression();var c=Context(42,true);
            Advance(p,c,0,31);Assert(!p.Arcane.Knows(Spark),"Instant unlock");
            Advance(p,c,32,32);Assert(p.Arcane.Knows(Spark),"Study did not complete");
            Assert(p.Mana==100,"Studying consumed mana");
        });
        Check("self study records actor, method and world time",()=>{
            var p=new CharacterProgression();Advance(p,Context(42,true),0,32);
            var o=p.GetOrigin(Spark);
            Assert(o!=null && o.ActorId==42 && o.Source==KnowledgeSource.SelfStudy && o.WorldTime==32,"Missing origin");
        });
        Check("curriculum ends at four spells and circle 1",()=>{
            var p=new CharacterProgression();var c=Context(4,true);Advance(p,c,0,1000);
            Assert(p.Arcane.KnownSpellIds.Count()==4 && p.Arcane.HighestCircle==1,"Wrong curriculum/circle");
            Assert(p.Arcane.KnownSpellIds.All(id=>SpellCatalog.Find(id).Circle<=1 && SpellCatalog.HasRuntime(SpellCatalog.Find(id))),"Unsupported spell learned");
            Assert(p.Arcane.GetExperience(ArcaneTradition.Wizard)<250,"Advanced circle awarded");
        });
        Check("noneligible actors cannot study or practice",()=>{
            var p=new CharacterProgression();var c=Context(1,true,Weapon.Sword);c.Eligible=false;Advance(p,c,0,300);
            Assert(!p.Arcane.KnownSpellIds.Any() && p.GetWeaponExperience(Weapon.Sword)==0,"Ineligible progression");
        });
        Check("combat or rest does not grant peace-time work",()=>{
            var p=new CharacterProgression();var c=Context(1,true,Weapon.Sword);c.Available=false;Advance(p,c,0,300);
            Assert(p.Arcane.GetExperience(ArcaneTradition.Wizard)==0 && p.GetWeaponExperience(Weapon.Sword)==0,"Combat credited as study");
        });
        Check("unchanged world time cannot award XP",()=>{
            var p=new CharacterProgression();var c=Context(1,true,Weapon.Sword);
            for(int i=0;i<1000;i++)LearningRules.Tick(p,c);
            Assert(p.GetWeaponExperience(Weapon.Sword)==0 && !p.Arcane.KnownSpellIds.Any(),"Paused-time XP");
        });
        Check("practice needs real elapsed time with the equipped weapon",()=>{
            var p=new CharacterProgression();var c=Context(1,false,Weapon.Spear);Advance(p,c,0,9);
            Assert(p.GetWeaponExperience(Weapon.Spear)==0,"Practice awarded early");
            Advance(p,c,10,10);Assert(p.GetWeaponExperience(Weapon.Spear)==2 && p.GetWeaponExperience(Weapon.Sword)==0,"Wrong weapon XP");
        });
        Check("unequipped characters cannot gain weapon XP",()=>{
            var p=new CharacterProgression();Advance(p,Context(1),0,300);
            Assert(Enum.GetValues(typeof(Weapon)).Cast<Weapon>().All(w=>p.GetWeaponExperience(w)==0),"Unarmed XP");
        });
        Check("switching weapons does not transfer partial practice",()=>{
            var p=new CharacterProgression();var c=Context(1,false,Weapon.Sword);Advance(p,c,0,5);
            c.Weapon=Weapon.Bow;Advance(p,c,10,15);Assert(p.GetWeaponExperience(Weapon.Bow)==0,"Transferred practice");
            Advance(p,c,16,20);Assert(p.GetWeaponExperience(Weapon.Bow)==2 && p.GetWeaponExperience(Weapon.Sword)==0,"Wrong practice target");
        });
        Check("characters and weapons keep independent progression",()=>{
            var a=new CharacterProgression();var b=new CharacterProgression();
            Advance(a,Context(1,false,Weapon.Sword),0,20);Advance(b,Context(2,false,Weapon.Axe),0,20);
            Assert(a.GetWeaponExperience(Weapon.Sword)==4 && a.GetWeaponExperience(Weapon.Axe)==0,"Actor A corrupted");
            Assert(b.GetWeaponExperience(Weapon.Axe)==4 && b.GetWeaponExperience(Weapon.Sword)==0,"Actor B corrupted");
            Assert(a.GetOrigin(LearningRules.WeaponKnowledgeId(Weapon.Sword)).ActorId==1,"Wrong training origin");
        });
        Check("duplicate combat callbacks cannot duplicate XP",()=>{
            var p=new CharacterProgression();
            Assert(LearningRules.AwardCombat(p,Weapon.Sword,100,1,1),"First attack rejected");
            Assert(!LearningRules.AwardCombat(p,Weapon.Sword,100,1,1),"Duplicate sword callback");
            Assert(!LearningRules.AwardCombat(p,Weapon.Bow,100,1,1),"Duplicate callback with another trait");
            Assert(p.GetWeaponExperience(Weapon.Sword)==2 && p.GetWeaponExperience(Weapon.Bow)==0,"Duplicate XP");
            Assert(LearningRules.AwardCombat(p,Weapon.Bow,101,2,1) && p.GetWeaponExperience(Weapon.Bow)==2,"Next attack rejected");
        });
        Check("same combat token on a different actor remains independent",()=>{
            var a=new CharacterProgression();var b=new CharacterProgression();
            LearningRules.AwardCombat(a,Weapon.Sword,1,1,1);LearningRules.AwardCombat(b,Weapon.Bow,1,1,2);
            Assert(a.GetWeaponExperience(Weapon.Sword)==2 && b.GetWeaponExperience(Weapon.Bow)==2,"Shared dedup state");
        });
        Check("invalid time cannot award work or combat XP",()=>{
            var p=new CharacterProgression();var c=Context(1,true,Weapon.Sword);
            foreach(double time in new[]{double.NaN,double.PositiveInfinity,double.NegativeInfinity})
            {c.WorldTime=time;LearningRules.Tick(p,c);Assert(!LearningRules.AwardCombat(p,Weapon.Sword,1,time,1),"Invalid combat timestamp");}
            Assert(p.GetWeaponExperience(Weapon.Sword)==0 && !p.Arcane.KnownSpellIds.Any(),"Invalid-time progression");
        });
        Check("a large observation gap cannot instantly create a master",()=>{
            var p=new CharacterProgression();var c=Context(1,true,Weapon.Sword);LearningRules.Tick(p,c);
            c.WorldTime=100000;LearningRules.Tick(p,c);
            Assert(p.GetWeaponExperience(Weapon.Sword)<=2 && !p.Arcane.Knows(Spark),"Catch-up exploit");
        });
        Check("reset after disabling learning does not bank disabled time",()=>{
            var p=new CharacterProgression();var c=Context(1,false,Weapon.Sword);Advance(p,c,0,5);
            p.Learning.ResetObservation();Advance(p,c,1000,1005);
            Assert(p.GetWeaponExperience(Weapon.Sword)==0,"Disabled-time progression");
            Advance(p,c,1006,1010);Assert(p.GetWeaponExperience(Weapon.Sword)==2,"Practice failed to resume");
        });
        Check("simultaneous interests share the available time",()=>{
            var p=new CharacterProgression();Advance(p,Context(1,true,Weapon.Sword),0,20);
            Assert(p.GetWeaponExperience(Weapon.Sword)==2 && p.Arcane.GetExperience(ArcaneTradition.Wizard)==5,"Time spent twice");
        });
        Check("manual learning keeps its real origin and checks circle",()=>{
            var p=new CharacterProgression();
            Assert(LearningRules.Learn(p,SpellCatalog.Find(Spark),KnowledgeSource.Player,1,4),"Manual circle 0 rejected");
            Assert(!LearningRules.Learn(p,SpellCatalog.Find(Spark),KnowledgeSource.SelfStudy,2,4),"Duplicate learned spell");
            Assert(p.GetOrigin(Spark).Source==KnowledgeSource.Player,"Original source overwritten");
            Assert(!LearningRules.Learn(p,SpellCatalog.Find("ama.spell.evocation.1.lanca_eletrica"),KnowledgeSource.Player,3,4),"Circle requirement bypassed");
        });
        Check("autonomous practice and combat stop at mastery level 7",()=>{
            var p=new CharacterProgression();p.TrainWeapon(Weapon.Sword,2499);Advance(p,Context(1,false,Weapon.Sword),0,100);
            Assert(p.GetWeaponExperience(Weapon.Sword)==2500 && p.GetWeaponLevel(Weapon.Sword)==7,"Practice overflow");
            Assert(!LearningRules.AwardCombat(p,Weapon.Sword,1,101,1),"Combat beyond cap");
            var q=new CharacterProgression();q.TrainWeapon(Weapon.Axe,2499);LearningRules.AwardCombat(q,Weapon.Axe,1,1,2);
            Assert(q.GetWeaponExperience(Weapon.Axe)==2500,"Combat cap overflow");
        });
        Check("manual high circle does not expand the autonomous curriculum",()=>{
            var p=new CharacterProgression();p.Arcane.Study(ArcaneTradition.Wizard,10000);Advance(p,Context(1,true),0,1000);
            Assert(p.Arcane.KnownSpellIds.Count()==4 && p.Arcane.KnownSpellIds.All(id=>SpellCatalog.Find(id).Circle<=1),"Advanced auto unlock");
        });
        Check("weapon IDs recognize supported weapons without false bow matches",()=>{
            Weapon w;
            Assert(WeaponIdentity.TryClassify("weapon_sword_fire",out w) && w==Weapon.Sword,"Sword not detected");
            Assert(WeaponIdentity.TryClassify("SPEAR",out w) && w==Weapon.Spear,"Spear not detected");
            Assert(WeaponIdentity.TryClassify("crossbow",out w) && w==Weapon.Bow,"Bow not detected");
            Assert(WeaponIdentity.TryClassify("battleaxe",out w) && w==Weapon.Axe,"Axe not detected");
            foreach(string id in new[]{"rainbow_staff","hands","fire_staff","",null})Assert(!WeaponIdentity.TryClassify(id,out w),"False weapon: "+id);
        });
        Check("arcane aptitude is stable for an actor ID",()=>{
            int count=0;
            for(int id=1;id<=1000;id++)
            {bool talent=LearningRules.HasArcaneAptitude(id);Assert(talent==LearningRules.HasArcaneAptitude(id),"Unstable aptitude");if(talent)count++;}
            Assert(count>=80 && count<=160 && !LearningRules.HasArcaneAptitude(0),"Unexpected deterministic cohort: "+count);
        });
        Check("short sampling intervals produce the same study outcome",()=>{
            var a=new CharacterProgression();var b=new CharacterProgression();
            Advance(a,Context(1,true),0,32,1);Advance(b,Context(1,true),0,32,2);
            Assert(a.Arcane.Knows(Spark) && b.Arcane.Knows(Spark) && a.Arcane.GetExperience(ArcaneTradition.Wizard)==b.Arcane.GetExperience(ArcaneTradition.Wizard),"Sampling-dependent progression");
        });
        Check("combat interruption does not count as study time",()=>{
            var p=new CharacterProgression();var c=Context(1,true);Advance(p,c,0,16);
            c.Available=false;Advance(p,c,17,50);Assert(!p.Arcane.Knows(Spark),"Learned during combat");
            c.Available=true;Advance(p,c,51,66);Assert(!p.Arcane.Knows(Spark),"Banked combat time");
            Advance(p,c,67,67);Assert(p.Arcane.Knows(Spark),"Interrupted study could not resume");
        });
        Console.WriteLine("Learning rules: "+passed+" passed, "+failed+" failed. No WorldBox runtime was executed.");
        return failed==0?0:1;
    }
}
