using System;
using System.Collections.Generic;
using UnityEngine;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Spells;
using ArcaneMartialAscension.VisualEffects;
using ArcaneMartialAscension.Learning;
using ArcaneMartialAscension.Martial;
using ArcaneMartialAscension.Persistence;
using MartialWeaponType = ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.GameIntegration
{
    public static class ActorAbilities
    {
        private sealed class ActorState
        {
            public readonly CharacterProgression Progress;
            public readonly SpellCastingSession Casting;
            public readonly long ActorId;
            public readonly ActorData Data;
            public bool PreserveSnapshot;
            public ActorState(Actor actor,CharacterProgression progress=null)
            {ActorId=actor.getID();Data=DataFor(actor);Progress=progress??new CharacterProgression();Casting=new SpellCastingSession(Progress);}
            public readonly Dictionary<string,double> Temporary = new Dictionary<string,double>(StringComparer.Ordinal);
            public float LastAura;
            public double LastMagic;
            public double LastUpdate;
            public int LastWeaponHitFrame=-1;
            public float LastDiagnostic;
            public string PreferredSpellId, LastCast,LastTechnique;
            public PendingCast Pending;
            public bool Initialized;
        }
        private sealed class PendingCast
        {
            public Actor Caster,Target;
            public ActorState State;
            public SpellEntry Spell;
            public SpellCastTicket Ticket;
            public Vector3 From;
            public float Age,Duration;
            public double LastWorldTime;
            public bool Manual,Log;
            public ScreenEffects.Flight Flight;
            public Transform Sprite;
        }
        private static readonly Dictionary<Actor, ActorState> states = new Dictionary<Actor, ActorState>();
        private static readonly List<PendingCast> pending=new List<PendingCast>();
        private static readonly List<Actor> combatActors=new List<Actor>();
        private static float nextCombatUpdate;
        private static int combatOffset;
        public static string LastMessage {get;private set;}
        public static float MessageUntil {get;private set;}
        private static float lastCleanup;
        private static float lastWeaponScan;
        private static int weaponScanOffset;
        private static double WorldTime {get{return World.world!=null?World.world.getCurWorldTime():0;}}
        private static ActorData DataFor(Actor actor){return actor!=null?actor.getData() as ActorData:null;}
        private static ActorState State(Actor actor)
        {
            ActorState s;
            if(!states.TryGetValue(actor,out s) || s.ActorId!=actor.getID() || !ReferenceEquals(s.Data,DataFor(actor)))
            {s=RestoreActor(actor);states[actor]=s;}
            if(!s.Initialized)
            {
                s.Initialized=true;
                s.LastUpdate=WorldTime;
                // Powers now come from recorded study, practice or explicit player actions.
            }
            double now=WorldTime;
            float dt=World.world!=null && !World.world.isPaused()?(float)Math.Max(0,Math.Min(10,now-s.LastUpdate)):0;
            s.Progress.GrowMana(s.Progress.Arcane.HighestCircle);
            s.Progress.Regenerate(dt);
            s.LastUpdate=now;
            if(!s.PreserveSnapshot)SyncIdentityTraits(actor,s.Progress);
            return s;
        }
        private static ActorState RestoreActor(Actor actor)
        {
            string json=null;
            ActorData data=DataFor(actor);
            if(data!=null)data.get(ActorProgressionPersistence.Key,out json,null);
            CharacterProgression progress;ProgressionSnapshot snapshot;string error;
            SnapshotStatus status=ProgressionCodec.Load(json,actor.getID(),WorldTime,out progress,out snapshot,out error);
            var state=new ActorState(actor,progress){PreserveSnapshot=status==SnapshotStatus.Invalid || status==SnapshotStatus.Unsupported};
            if(status==SnapshotStatus.Restored)
            {
                state.PreferredSpellId=snapshot.PreferredSpell;
                foreach(SavedSpell spell in snapshot.Spells)state.Casting.RestoreCooldown(SpellCatalog.Find(spell.Id),spell.Remaining,WorldTime);
                foreach(SavedBuff buff in snapshot.Buffs)state.Temporary[buff.Id]=WorldTime+buff.Remaining;
                Debug.Log("[AMA Save] restored actor="+actor.getID()+" spells="+snapshot.Spells.Count+" swordXp="+progress.GetWeaponExperience(MartialWeaponType.Sword)+
                    " mana="+progress.Mana+" patron="+progress.Arcane.PatronId+" origin="+progress.Arcane.Origin);
            }
            else if(status!=SnapshotStatus.Missing)Debug.LogWarning("[AMA Save] actor="+actor.getID()+" status="+status+" · "+error+
                (status==SnapshotStatus.ForeignActor?" · progresso de outro Actor ignorado.":" · dados originais preservados."));
            return state;
        }
        public static void SaveForActor(Actor actor)
        {
            ActorState state;
            ActorData data=DataFor(actor);
            if(actor==null || !actor.isAlive() || data==null || !states.TryGetValue(actor,out state) || state.ActorId!=actor.getID() ||
                !ReferenceEquals(state.Data,data) || state.PreserveSnapshot)return;
            state=State(actor);
            double now=WorldTime;
            var expired=new List<string>();
            foreach(var buff in state.Temporary)if(buff.Value<=now)
            {if(!CombatBridge.RemoveTrait(actor,buff.Key))throw new InvalidOperationException("Bônus expirado não removido: "+buff.Key);expired.Add(buff.Key);}
            foreach(string id in expired)state.Temporary.Remove(id);
            ProgressionSnapshot snapshot=ProgressionCodec.Capture(state.Progress,actor.getID(),now);
            snapshot.PreferredSpell=state.PreferredSpellId;
            foreach(SavedSpell spell in snapshot.Spells)spell.Remaining=state.Casting.CooldownRemaining(SpellCatalog.Find(spell.Id),now);
            foreach(var buff in state.Temporary)snapshot.Buffs.Add(new SavedBuff{Id=buff.Key,Remaining=buff.Value-now});
            string json=ProgressionCodec.Encode(snapshot);
            data.set(ActorProgressionPersistence.Key,json);
            Debug.Log("[AMA Save] saved actor="+actor.getID()+" spells="+snapshot.Spells.Count+" swordXp="+state.Progress.GetWeaponExperience(MartialWeaponType.Sword)+
                " mana="+state.Progress.Mana+" patron="+state.Progress.Arcane.PatronId+" origin="+state.Progress.Arcane.Origin+" bytes="+json.Length);
        }
        private static void SyncIdentityTraits(Actor actor,CharacterProgression progress)
        {
            ArcaneIdentity patron=ArcaneIdentities.Patron(progress.Arcane.PatronId),lineage=ArcaneIdentities.Lineage(progress.Arcane.Origin);
            foreach(ArcaneIdentity identity in ArcaneIdentities.All)
            {
                if(identity==patron || identity==lineage)Ensure(actor,identity.TraitId);
                else if(actor.hasTrait(identity.TraitId))CombatBridge.RemoveTrait(actor,identity.TraitId);
            }
        }
        public static string IdentityForDisplay(Actor actor)
        {
            CharacterProgression progress=ProgressForDisplay(actor);
            if(progress==null)return "Nenhum";
            ArcaneIdentity patron=ArcaneIdentities.Patron(progress.Arcane.PatronId),lineage=ArcaneIdentities.Lineage(progress.Arcane.Origin);
            return (patron!=null?patron.Name:"")+(patron!=null && lineage!=null?" / ":"")+(lineage!=null?lineage.Name:"")+
                (patron==null && lineage==null?"Nenhum":"");
        }
        public static string IdentityDetailsForDisplay(Actor actor)
        {
            CharacterProgression progress=ProgressForDisplay(actor);
            if(progress==null)return "Pactos e origens surgem durante a formação; consultar a ficha não concede poderes.";
            string result="";
            foreach(ArcaneTradition tradition in new[]{ArcaneTradition.Warlock,ArcaneTradition.Sorcerer})
            {
                ArcaneIdentity identity=ArcaneIdentities.For(progress.Arcane,tradition);
                if(identity==null)continue;
                result+=(result.Length>0?"\n":"")+"<b>"+identity.Name+"</b> · "+identity.Attributes.Describe()+" · Mana +"+identity.ManaBonus;
            }
            return result.Length>0?result:NaturalMagicRules.Status(progress);
        }
        public static void Ensure(Actor actor,string trait)
        {
            if(actor!=null && actor.isAlive() && !actor.hasTrait(trait))
            {actor.addTrait(trait);actor.setStatsDirty();}
        }
        public static void EnsureWeapon(Actor actor,MartialWeaponType type)
        {Ensure(actor,ModTraits.WeaponTrait(type));}
        public static void GainWeaponXp(Actor actor,MartialWeaponType type,int xp)
        {
            if(actor==null || !actor.isAlive() || xp<=0) return;
            ActorState s=State(actor);
            if(s.PreserveSnapshot)return;
            s.Progress.TrainWeapon(type,xp);
            s.Progress.RecordOrigin(LearningRules.WeaponKnowledgeId(type),
                new KnowledgeOrigin(KnowledgeSource.Player,World.world!=null?World.world.getCurWorldTime():0,actor.getID()));
            s.Progress.Learning.LastAction="Treino solicitado pelo jogador: "+LearningRules.WeaponName(type);
            TechniqueRules.LearnFromTraining(s.Progress,type,KnowledgeSource.Player,WorldTime,actor.getID());
            MartialWeaponType equipped;
            SyncWeaponTraits(actor,GameWeaponDetector.TryDetect(actor,out equipped)?(MartialWeaponType?)equipped:null);
        }
        public static bool OnSwordAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnWeaponAttack(a,b,MartialWeaponType.Sword);}
        public static bool OnSpearAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnWeaponAttack(a,b,MartialWeaponType.Spear);}
        public static bool OnBowAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnWeaponAttack(a,b,MartialWeaponType.Bow);}
        public static bool OnAxeAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnWeaponAttack(a,b,MartialWeaponType.Axe);}
        private static bool OnWeaponAttack(BaseSimObject attacker,BaseSimObject defender,MartialWeaponType type)
        {
            // Trait callbacks describe attempts, not confirmed hits. The observer owns XP.
            return true;
        }
        public static void OnConfirmedWeaponHit(Actor caster,Actor target,MartialWeaponType used,int healthLost)
        {
            if(caster==null || !caster.isAlive() || !caster.isSapient() || !caster.isAdult() || healthLost<=0 || World.world==null || World.world.isPaused())return;
            ActorState s=State(caster);
            if(s.PreserveSnapshot)return;
            if(s.LastWeaponHitFrame==Time.frameCount)return;
            s.LastWeaponHitFrame=Time.frameCount;
            int before=s.Progress.GetWeaponLevel(used);
            bool gained=LearningRules.AwardCombat(s.Progress,used,Time.frameCount,WorldTime,caster.getID());
            MartialWeaponType current;
            MartialWeaponType? equipped=GameWeaponDetector.TryDetect(caster,out current)?(MartialWeaponType?)current:null;
            SyncWeaponTraits(caster,equipped);
            if(gained && s.Progress.GetWeaponLevel(used)>before)
                Debug.Log("[AMA Martial] mastery actor="+caster.getID()+" weapon="+used+" level="+s.Progress.GetWeaponLevel(used)+" source=confirmed_hit");
            if(target==null || !target.isAlive() || equipped!=used || !caster.canAttackTarget(target,true,false))return;
            foreach(TechniqueEntry technique in TechniqueCatalog.ForWeapon(used))
            {
                double now=WorldTime;
                if(!TechniqueRules.CanUse(s.Progress,technique,equipped,now))continue;
                int hp=target.getHealth();
                CombatBridge.TryDamage(target,TechniqueRules.Damage(s.Progress,technique),caster);
                int loss=Math.Max(0,hp-Math.Max(0,target.getHealth()));
                if(!TechniqueRules.ConfirmHit(s.Progress,technique,equipped,now,loss))continue;
                s.LastTechnique=technique.Name+" "+s.Progress.Techniques.Get(technique.Id).Rank+" · dano adicional "+loss;
                if(Time.time-s.LastDiagnostic>10)
                {s.LastDiagnostic=Time.time;Debug.Log("[AMA Technique] applied actor="+caster.getID()+" technique="+technique.Id+" hp="+hp+"->"+target.getHealth());}
                try
                {
                    if(Main.Visuals!=null && used!=MartialWeaponType.Bow)Main.Visuals.SpawnSwordArc(GameCompat.PositionOrCursor(caster),GameCompat.PositionOrCursor(target));
                    ScreenEffects.Emit(target,1,ColorFor(MagicSchool.Transmutation),technique.Name,false);
                }
                catch(Exception error){Debug.LogWarning("[AMA Technique] Dano aplicado; falha visual: "+error.Message);}
                break;
            }
        }
        public static bool OnWizardAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnMagicAttack(a,b,ArcaneTradition.Wizard);}
        public static bool OnWarlockAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnMagicAttack(a,b,ArcaneTradition.Warlock);}
        public static bool OnSorcererAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnMagicAttack(a,b,ArcaneTradition.Sorcerer);}
        private static bool OnMagicAttack(BaseSimObject attacker,BaseSimObject defender,ArcaneTradition tradition)
        {
            if(attacker==null || defender==null || !attacker.isAlive() || attacker.a==null) return false;
            Actor caster=attacker.a;
            if(defender.a==null) return true;
            TryAutomaticCast(caster,defender.a,State(caster));
            return true;
        }
        public static bool OnAuraTick(BaseSimObject a,WorldTile tile)
        {
            if(a==null || !a.isAlive() || a.a==null) return false;
            Actor actor=a.a;
            ActorState s=State(actor);
            if(s.PreserveSnapshot)return true;
            if(Time.time-s.LastAura<5f) return true;
            if(!s.Progress.HasAwakenedAura) s.Progress.AwakenAura();
            if(s.Progress.UseAura(6f))
            {
                s.LastAura=Time.time;
                if(Main.Visuals!=null) Main.Visuals.SpawnAuraPulse(GameCompat.PositionOrCursor(actor));
            }
            return true;
        }
        public static void TriggerAura(Actor actor)
        {
            if(actor==null || !actor.isAlive()) return;
            ActorState s=State(actor);
            if(s.PreserveSnapshot)return;
            Ensure(actor,ModTraits.Aura);
            if(!s.Progress.HasAwakenedAura)s.Progress.AwakenAura();
            if(s.Progress.UseAura(5f) && Main.Visuals!=null)
                Main.Visuals.SpawnAuraPulse(GameCompat.PositionOrCursor(actor));
        }
        public static void TriggerWeapon(Actor actor,MartialWeaponType type)
        {
            if(actor==null || !actor.isAlive())return;
            GainWeaponXp(actor,type,15); // explicitly manual training, attacks award only +2 XP
            if(Main.Visuals!=null) Main.Visuals.SpawnSwordArc(GameCompat.PositionOrCursor(actor));
        }
        public static void TrainMagic(Actor actor)
        {
            if(actor==null || !actor.isAlive()) return;
            ActorState s=State(actor);
            if(s.PreserveSnapshot)return;
            s.Progress.Arcane.Study(TraditionFor(actor),250);
            s.Progress.GrowMana(s.Progress.Arcane.HighestCircle);
        }
        public static string Learn(Actor actor,SpellEntry spell)
        {
            if(actor==null || !actor.isAlive() || spell==null) return "Sem personagem/magia";
            ActorState s=State(actor);
            if(s.PreserveSnapshot)return "Progresso incompatível ou inválido; dados originais preservados.";
            ArcaneTradition tradition=TraditionFor(actor);
            if(s.Progress.Arcane.GetCircle(tradition)<spell.Circle) return "Círculo insuficiente nesta tradição; estudar magia F12";
            if(s.Progress.Arcane.Knows(spell.Id))return "Magia ja aprendida: "+spell.Name;
            if(!LearningRules.Learn(s.Progress,spell,KnowledgeSource.Player,WorldTime,actor.getID(),tradition))
                return "Requisitos não atendidos: confira círculo, pacto ou origem inata.";
            Ensure(actor,TraditionTrait(tradition));
            Ensure(actor,ModTraits.SchoolTrait(spell.School));
            return "Aprendeu: "+spell.Name;
        }
        public static bool Knows(Actor actor,SpellEntry spell)
        {
            ActorState state;
            return actor!=null && spell!=null && states.TryGetValue(actor,out state) &&
                state.Progress.Arcane.Knows(spell.Id);
        }
        // Inspection must not grant sandbox skills, XP, traits or a patron.
        public static CharacterProgression ProgressForDisplay(Actor actor)
        {
            ActorState state;
            if(actor==null || !actor.isAlive())return null;
            ActorData data=DataFor(actor);
            if(states.TryGetValue(actor,out state) && state.ActorId==actor.getID() && ReferenceEquals(state.Data,data))return state.Progress;
            string json=null;if(data!=null)data.get(ActorProgressionPersistence.Key,out json,null);
            return string.IsNullOrEmpty(json)?null:State(actor).Progress;
        }
        public static KnowledgeOrigin OriginForDisplay(Actor actor,string knowledgeId)
        {
            CharacterProgression progress=ProgressForDisplay(actor);
            return progress!=null?progress.GetOrigin(knowledgeId):null;
        }
        private static string TraditionTrait(ArcaneTradition tradition)
        {return tradition==ArcaneTradition.Warlock?ModTraits.Warlock:tradition==ArcaneTradition.Sorcerer?ModTraits.Sorcerer:ModTraits.Wizard;}
        private static ArcaneTradition? ExistingTradition(Actor actor)
        {
            if(actor.hasTrait(ModTraits.Warlock))return ArcaneTradition.Warlock;
            if(actor.hasTrait(ModTraits.Sorcerer))return ArcaneTradition.Sorcerer;
            return actor.hasTrait(ModTraits.Wizard)?(ArcaneTradition?)ArcaneTradition.Wizard:null;
        }
        private static ArcaneTradition TraditionFor(Actor actor)
        {
            CharacterProgression progress=ProgressForDisplay(actor);
            return (progress!=null?progress.Formation.Tradition:null)??ExistingTradition(actor)??ArcaneTradition.Wizard;
        }
        public static bool CanLearnForDisplay(Actor actor,SpellEntry spell)
        {
            if(actor==null || !actor.isAlive() || spell==null)return false;
            CharacterProgression progress=ProgressForDisplay(actor);
            ActorState existingState;if(states.TryGetValue(actor,out existingState) && existingState.PreserveSnapshot)return false;
            if(progress==null)return spell.Circle==0 && TraditionFor(actor)==ArcaneTradition.Wizard;
            return !progress.Arcane.Knows(spell.Id) && progress.Arcane.CanLearn(new SpellDefinition(spell.Id,spell.School,spell.Circle,spell.Mana),TraditionFor(actor));
        }
        public static string FormationForDisplay(Actor actor)
        {return NaturalMagicRules.Status(ProgressForDisplay(actor));}

        public static string LearningStatusForDisplay(Actor actor)
        {
            if(actor==null || !actor.isAlive())return "Selecione um habitante vivo.";
            if(!actor.isSapient() || !actor.isAdult())return "Aprendizado autônomo disponível para habitantes adultos.";
            if(actor._has_status_sleeping)return "Descansando; estudo e prática aguardam.";
            if(actor.attack_target!=null && actor.attack_target.isAlive())return "Em combate; estudo em paz interrompido.";
            CharacterProgression progress=ProgressForDisplay(actor);
            ActorState existingState;if(states.TryGetValue(actor,out existingState) && existingState.PreserveSnapshot)return "Progresso incompatível ou inválido; dados originais preservados.";
            return progress!=null && progress.Formation.Tradition.HasValue && !progress.Formation.Ready?NaturalMagicRules.Status(progress):
                progress!=null?progress.Learning.LastAction:"Aguardando vocação arcana ou uma arma equipada.";
        }

        public static void ObserveLearning(Actor actor,double now)
        {
            if(actor==null || !actor.isAlive() || !actor.isSapient() || !actor.isAdult())return;
            long id=actor.getID();
            ArcaneTradition? existing=ExistingTradition(actor);
            if(!existing.HasValue && actor.hasTrait(ModTraits.Evocation))existing=ArcaneTradition.Wizard;
            bool candidate=existing.HasValue || LearningRules.HasArcaneAptitude(id);
            MartialWeaponType weapon;
            bool armed=GameWeaponDetector.TryDetect(actor,out weapon);
            ActorState state;
            if(!candidate && !armed && !states.TryGetValue(actor,out state))
            {SyncWeaponTraits(actor,null);return;}
            state=State(actor);
            if(state.PreserveSnapshot)return;
            bool available=!actor._has_status_sleeping && (actor.attack_target==null || !actor.attack_target.isAlive());
            bool formed=NaturalMagicRules.Observe(state.Progress,new FormationContext{
                ActorId=id,WorldTime=now,Eligible=true,Available=available,ExistingTradition=existing
            });
            ArcaneFormation formation=state.Progress.Formation;
            if(formed)
            {
                Ensure(actor,TraditionTrait(formation.Tradition.Value));
                SyncIdentityTraits(actor,state.Progress);
                Debug.Log("[AMA Formation] actor="+id+" tradition="+formation.Tradition+" patron="+state.Progress.Arcane.PatronId+" origin="+state.Progress.Arcane.Origin+" worldTime="+now);
            }
            int before=armed?state.Progress.GetWeaponLevel(weapon):0;
            LearningOutcome outcome=LearningRules.Tick(state.Progress,new LearningContext{
                ActorId=id,WorldTime=now,Eligible=true,MagicEligible=formation.Ready,
                Tradition=formation.Tradition??ArcaneTradition.Wizard,
                Available=available && (!formation.Tradition.HasValue || formation.Ready),
                Weapon=armed?(MartialWeaponType?)weapon:null
            });
            if(outcome.LearnedSpell!=null)
            {
                Ensure(actor,TraditionTrait(formation.Tradition??ArcaneTradition.Wizard));
                Ensure(actor,ModTraits.SchoolTrait(outcome.LearnedSpell.School));
                Debug.Log("[AMA Learning] learned actor="+id+" spell="+outcome.LearnedSpell.Id+" tradition="+formation.Tradition+" source="+NaturalMagicRules.Source(formation.Tradition??ArcaneTradition.Wizard)+" worldTime="+now);
            }
            SyncWeaponTraits(actor,armed?(MartialWeaponType?)weapon:null);
            if(armed && state.Progress.GetWeaponLevel(weapon)>before)
                Debug.Log("[AMA Learning] mastery actor="+id+" weapon="+weapon+" level="+state.Progress.GetWeaponLevel(weapon)+" source=practice");
        }

        public static void SyncWeaponTraits(Actor actor,MartialWeaponType? equipped)
        {
            if(actor==null || !actor.isAlive())return;
            CharacterProgression progress=ProgressForDisplay(actor);
            ActorState existingState;if(states.TryGetValue(actor,out existingState) && existingState.PreserveSnapshot)return;
            for(int i=0;i<4;i++)
            {
                MartialWeaponType type=(MartialWeaponType)i;
                bool active=equipped.HasValue && equipped.Value==type;
                string trait=ModTraits.WeaponTrait(type);
                if(active)Ensure(actor,trait);
                else if(actor.hasTrait(trait))CombatBridge.RemoveTrait(actor,trait);
                int level=active && progress!=null?progress.GetWeaponLevel(type):0;
                int tier=level;
                for(int rank=1;rank<=TraitAttributeCatalog.MaxWeaponLevel;rank++)
                {
                    string rankId=ModTraits.RankId(type,rank);
                    if(active && rank==tier)Ensure(actor,rankId);
                    else if(actor.hasTrait(rankId))CombatBridge.RemoveTrait(actor,rankId);
                }
            }
        }

        public static void ResetLearningObservation()
        {foreach(ActorState state in states.Values){state.Progress.Learning.ResetObservation();state.Progress.Formation.ResetObservation();}}

        public static void ResetRuntimeForWorld()
        {
            foreach(PendingCast cast in pending)Finish(cast,"Mundo alterado; conjuração cancelada sem custo.");
            pending.Clear();states.Clear();combatActors.Clear();
            ScreenEffects.Clear();
            if(Main.Visuals!=null)Main.Visuals.ClearEffects();
            WeaponHitObserver.ResetWorld();
            combatOffset=weaponScanOffset=0;nextCombatUpdate=lastWeaponScan=lastCleanup=0;
            LastMessage=null;MessageUntil=0;
        }
        public static string Cast(Actor caster,Actor target,SpellEntry spell,bool manual)
        {
            string outcome;
            try {outcome=BeginCast(caster,target,spell,manual);}
            catch(Exception error)
            {
                Debug.LogError("[AMA Cast] Falha ao iniciar conjuração: "+error);
                outcome="Falha ao conjurar; consulte o log. Mana preservada.";
            }
            ActorState existing;
            if(caster!=null && states.TryGetValue(caster,out existing))existing.LastCast=outcome;
            if(manual){LastMessage=outcome;MessageUntil=Time.unscaledTime+5f;Debug.Log("[AMA Cast] manual: "+outcome);}
            return outcome;
        }
        private static SpellCastRequest Request(Actor caster,Actor target,SpellEntry spell,bool manual)
        {
            bool alive=caster!=null && caster.isAlive(),targetAlive=target!=null && target.isAlive();
            SpellTargetRelation relation=SpellTargetRelation.None;
            if(alive && targetAlive)
                relation=caster==target?SpellTargetRelation.Self:caster.canAttackTarget(target,true,false)?SpellTargetRelation.Enemy:SpellTargetRelation.Ally;
            return new SpellCastRequest{Spell=spell,CasterId=alive?caster.getID():0,TargetId=targetAlive?target.getID():0,
                CasterAlive=alive,TargetAlive=targetAlive,Paused=World.world==null || World.world.isPaused(),WorldTime=WorldTime,
                HasSchool=alive && spell!=null && caster.hasTrait(ModTraits.SchoolTrait(spell.School)),Manual=manual,Relation=relation,
                TargetHasWeapon=targetAlive && target.getWeapon()!=null,
                Distance=alive && targetAlive?Vector2.Distance(caster.current_position,target.current_position):0};
        }
        private static string BeginCast(Actor caster,Actor target,SpellEntry spell,bool manual)
        {
            if(caster==null || !caster.isAlive() || spell==null)return "Sem conjurador ou magia";
            SpellRuntimeProfile profile=SpellRuntimeCatalog.Find(spell);
            if(profile!=null && (profile.TargetRule==SpellTargetRule.Self || (profile.TargetRule==SpellTargetRule.Friendly && target==null)))target=caster;
            ActorState state=State(caster);
            if(state.PreserveSnapshot)return "Progresso incompatível ou inválido; dados originais preservados. Mana preservada.";
            SpellCastRequest request=Request(caster,target,spell,manual);
            if(profile!=null && profile.Delivery==SpellDelivery.Projectile && pending.Count>=128)return "Limite de projéteis atingido; mana preservada.";
            SpellCastTicket ticket;
            SpellCastResult error=state.Casting.TryReserve(request,out ticket);
            if(error!=null)return error.Message;
            bool transferred=false;
            try
            {
                if(profile.Delivery==SpellDelivery.Instant)
                {
                    SpellCastResult result=state.Casting.Complete(ticket,request,new SpellEffectExecutor(caster,target,ApplyTemporaryTrait));
                    if(result.Applied)
                    {
                        Debug.Log("[AMA Cast] applied spell="+spell.Id+" caster="+caster.getID()+" target="+target.getID()+" damage="+result.HealthLost+" mana="+state.Progress.Mana+" manual="+manual);
                        try
                        {
                            ScreenEffects.Emit(target,2,ColorFor(spell.School),spell.Name,false);
                            if(Main.Visuals!=null)
                            {
                                Vector3 start=GameCompat.PositionOrCursor(caster),end=GameCompat.PositionOrCursor(target);
                                Main.Visuals.SpawnCircle(start,spell.Circle,ColorFor(spell.School));
                                if(spell.Id=="ama.spell.evocation.1.lanca_eletrica")Main.Visuals.SpawnLightning(start,end);
                                else Main.Visuals.SpawnImpact(end,ColorFor(spell.School));
                            }
                        }
                        catch(Exception visualError){Debug.LogWarning("[AMA VFX] Efeito aplicado; falha visual: "+visualError.Message);}
                    }
                    return result.Message+(result.Applied?" Mana -"+result.ManaSpent:"");
                }
                Vector3 from,to;
                if(!GameCompat.GetPosition(caster,out from) || !GameCompat.GetPosition(target,out to))return "Posição indisponível; mana preservada.";
                var cast=new PendingCast{Caster=caster,Target=target,State=state,Spell=spell,Ticket=ticket,
                    From=from+Vector3.up*.5f,Duration=Mathf.Clamp(request.Distance/12f,.55f,1.1f),Manual=manual,LastWorldTime=WorldTime};
                cast.Log=manual || Time.time-state.LastDiagnostic>10f;
                if(cast.Log)state.LastDiagnostic=Time.time;
                state.Pending=cast;pending.Add(cast);transferred=true;
                try
                {
                    Color color=ColorFor(spell.School);
                    cast.Flight=ScreenEffects.Launch(cast.From,to+Vector3.up*.5f,color);
                    if(Main.Visuals!=null)
                    {Main.Visuals.SpawnCircle(from,spell.Circle,color);cast.Sprite=Main.Visuals.SpawnBolt(cast.From,color);}
                    else ScreenEffects.Emit(from,0,color,"CÍRCULO "+spell.Circle,false);
                }
                catch(Exception visualError){Debug.LogWarning("[AMA VFX] Falha visual; a simulação continua: "+visualError);}
                if(cast.Log)Debug.Log("[AMA Cast] launched spell="+spell.Id+" caster="+caster.getID()+" target="+target.getID()+" mana="+state.Progress.Mana+" manual="+manual);
                return "Lançando "+spell.Name+" em "+GameCompat.DisplayName(target);
            }
            finally {if(!transferred)state.Casting.Cancel(ticket);}
        }
        private static bool ApplyTemporaryTrait(Actor recipient,string trait,float duration)
        {
            if(recipient==null || !recipient.isAlive() || string.IsNullOrEmpty(trait) || duration<=0 || AssetManager.traits.get(trait)==null)return false;
            ActorState state=State(recipient);
            double until=WorldTime+duration,previous;
            if(recipient.hasTrait(trait))
            {
                // A permanent/player-granted trait must never become removable by this cast.
                if(!state.Temporary.TryGetValue(trait,out previous) || until<=previous)return false;
                state.Temporary[trait]=until;return true;
            }
            Ensure(recipient,trait);
            if(!recipient.hasTrait(trait))return false;
            state.Temporary[trait]=until;return true;
        }
        private static bool IsOffensive(SpellEntry spell)
        {SpellRuntimeProfile profile=SpellRuntimeCatalog.Find(spell);return profile!=null && profile.TargetRule==SpellTargetRule.Hostile;}
        public static void PrepareForCombat(Actor actor,SpellEntry spell)
        {
            ActorState state;
            if(actor!=null && spell!=null && states.TryGetValue(actor,out state) && state.Progress.Arcane.Knows(spell.Id))
                state.PreferredSpellId=spell.Id;
        }

        // Runs independently of normal melee attacks, following the game's current enemy.
        public static void UpdateMagic()
        {
            if(World.world==null || World.world.isPaused())return;
            for(int i=pending.Count-1;i>=0;i--)
            {
                PendingCast cast=pending[i];
                if(cast.Caster==null || !cast.Caster.isAlive() || cast.Target==null || !cast.Target.isAlive())
                {Finish(cast,"Conjuração interrompida: alvo ou conjurador indisponível. Mana preservada.");pending.RemoveAt(i);continue;}
                double now=WorldTime;
                cast.Age+=(float)Math.Max(0,now-cast.LastWorldTime);cast.LastWorldTime=now;
                Vector3 targetPosition;
                if(!GameCompat.GetPosition(cast.Target,out targetPosition))
                {Finish(cast,"Alvo sem posição; mana preservada.");pending.RemoveAt(i);continue;}
                Vector3 end=targetPosition+Vector3.up*.5f;
                float progress=Mathf.Clamp01(cast.Age/cast.Duration);
                if(cast.Flight!=null){cast.Flight.To=end;cast.Flight.Progress=progress;}
                if(cast.Sprite!=null)cast.Sprite.position=Vector3.Lerp(cast.From,end,progress);
                if(progress<1f)continue;
                pending.RemoveAt(i);
                try {ResolveImpact(cast,targetPosition);}
                catch(Exception error)
                {Debug.LogError("[AMA Cast] Falha ao resolver impacto: "+error);Finish(cast,"Falha no impacto; consulte o log.");}
            }
            if(Time.time<nextCombatUpdate)return;
            nextCombatUpdate=Time.time+.25f;
            combatActors.Clear();combatActors.AddRange(states.Keys);
            if(combatActors.Count==0)return;
            int count=Mathf.Min(32,combatActors.Count);
            for(int i=0;i<count;i++)
            {
                if(combatOffset>=combatActors.Count)combatOffset=0;
                Actor actor=combatActors[combatOffset++];
                if(actor==null || !actor.isAlive())continue;
                ActorState state=State(actor);
                if(actor.attack_target!=null && actor.attack_target.a!=null)
                    TryAutomaticCast(actor,actor.attack_target.a,state);
            }
        }

        private static void TryAutomaticCast(Actor caster,Actor target,ActorState state)
        {
            if(World.world==null || World.world.isPaused() || state.Pending!=null || target==null || !target.isAlive() ||
                WorldTime-state.LastMagic<.75)return;
            if(!caster.hasTrait(ModTraits.Wizard) && !caster.hasTrait(ModTraits.Warlock) && !caster.hasTrait(ModTraits.Sorcerer))return;
            state.LastMagic=WorldTime;
            try {if(!caster.canAttackTarget(target,true,false))return;}
            catch(Exception error)
            {
                if(Time.time-state.LastDiagnostic>10f)
                {state.LastDiagnostic=Time.time;Debug.LogWarning("[AMA AI] Falha ao validar alvo de combate: "+error.Message);}
                return;
            }
            SpellEntry best=SpellAiRules.Choose(state.Progress,state.Casting,spell=>Request(caster,target,spell,false),state.PreferredSpellId);
            if(best!=null)
            {
                if(Time.time-state.LastDiagnostic>10)Debug.Log("[AMA AI] casting actor="+caster.getID()+" tradition="+state.Progress.Arcane.TraditionFor(best.Id)+" spell="+best.Id+" target="+target.getID());
                Cast(caster,target,best,false);
            }
        }

        private static void ResolveImpact(PendingCast cast,Vector3 impact)
        {
            int before=cast.Target.getHealth();
            SpellCastResult result=cast.State.Casting.Complete(cast.Ticket,Request(cast.Caster,cast.Target,cast.Spell,cast.Manual),
                new SpellEffectExecutor(cast.Caster,cast.Target,ApplyTemporaryTrait));
            if(result.Applied && cast.Log)Debug.Log("[AMA Cast] applied spell="+cast.Spell.Id+" caster="+cast.Caster.getID()+" target="+cast.Target.getID()+
                " hp="+before+"->"+cast.Target.getHealth()+" damage="+result.HealthLost+" mana="+cast.State.Progress.Mana+" cooldown="+cast.Spell.Cooldown);
            Finish(cast,result.Message+(result.Applied?" Vida -"+result.HealthLost+" · Mana -"+result.ManaSpent:""));
            if(result.Applied)
                try {ScreenEffects.Emit(impact,2,ColorFor(cast.Spell.School),"-"+result.HealthLost+" VIDA",false);if(Main.Visuals!=null)Main.Visuals.SpawnImpact(impact,ColorFor(cast.Spell.School));}
                catch(Exception error){Debug.LogWarning("[AMA VFX] Impacto aplicado; falha visual: "+error.Message);}
        }
        private static void Finish(PendingCast cast,string result)
        {
            cast.State.Casting.Cancel(cast.Ticket);
            cast.State.Pending=null;
            cast.State.LastCast=result;
            if(cast.Flight!=null)ScreenEffects.Finish(cast.Flight);
            if(cast.Sprite!=null && Main.Visuals!=null)Main.Visuals.Release(cast.Sprite);
            if(cast.Manual){LastMessage=result;MessageUntil=Time.unscaledTime+5f;}
            if(cast.Log)Debug.Log("[AMA Cast] result spell="+cast.Spell.Id+": "+result);
        }

        public static string CombatStatusForDisplay(Actor actor)
        {
            ActorState state;
            if(actor==null || !states.TryGetValue(actor,out state))return "Aprenda uma magia para usar em combate.";
            if(state.Pending!=null)return "Lançando "+state.Pending.Spell.Name;
            if(!string.IsNullOrEmpty(state.LastCast))return state.LastCast;
            return "Automático: aguardando um inimigo em combate.";
        }
        public static string TechniqueStatusForDisplay(Actor actor)
        {ActorState state;return actor!=null && states.TryGetValue(actor,out state) && !string.IsNullOrEmpty(state.LastTechnique)?state.LastTechnique:"Técnicas aprendidas são usadas automaticamente após acertos válidos.";}
        public static string DiagnosticForDisplay(Actor actor)
        {
            ActorState state;
            Actor target=null;
            if(actor!=null && states.TryGetValue(actor,out state) && state.Pending!=null)target=state.Pending.Target;
            else if(actor!=null && actor.attack_target!=null)target=actor.attack_target.a;
            return "Alvo: "+(target!=null?GameCompat.DisplayName(target):"nenhum")+"\n"+CombatStatusForDisplay(actor);
        }
        public static Color ColorFor(MagicSchool s)
        {
            switch(s)
            {
                case MagicSchool.Abjuration: return new Color(.3f,1f,1f,1f);
                case MagicSchool.Conjuration: return new Color(.4f,1f,.5f,1f);
                case MagicSchool.Divination: return new Color(1f,.9f,.3f,1f);
                case MagicSchool.Enchantment: return new Color(1f,.3f,.8f,1f);
                case MagicSchool.Evocation: return new Color(1f,.45f,.2f,1f);
                case MagicSchool.Illusion: return new Color(.65f,.5f,1f,1f);
                case MagicSchool.Necromancy: return new Color(.7f,.3f,.9f,1f);
                default: return new Color(.2f,.95f,.7f,1f);
            }
        }
        public static string Status(Actor actor)
        {
            if(actor==null)return "Sem habitante";
            ActorState s=State(actor);
            return "Mana "+(int)s.Progress.Mana+"/"+(int)s.Progress.MaxMana+
                " | Aura "+(int)s.Progress.Aura+"/"+(int)s.Progress.MaxAura+
                " | Circulo "+s.Progress.Arcane.HighestCircle;
        }
        public static string WeaponStatus(Actor actor,MartialWeaponType type)
        {
            if(actor==null)return "Sem habitante";
            CharacterProgression p=State(actor).Progress;
            return type+" nivel "+p.GetWeaponLevel(type)+" ("+p.GetWeaponRank(type)+") - XP "+p.GetWeaponExperience(type);
        }
        // Opt-in equipment discovery for existing NPCs; 150 actors per batch to limit cost.
        public static void AutoDiscoverWeapons()
        {
            if(World.world==null || Time.time-lastWeaponScan<4f)return;
            lastWeaponScan=Time.time;
            int i=0, processed=0;
            bool any=false;
            foreach(Actor actor in World.world.units)
            {
                if(i++<weaponScanOffset)continue;
                any=true;
                if(actor!=null && actor.isAlive())
                {
                    MartialWeaponType equipment;
                    SyncWeaponTraits(actor,GameWeaponDetector.TryDetect(actor,out equipment)?(MartialWeaponType?)equipment:null);
                }
                if(++processed>=150)break;
            }
            weaponScanOffset=any && processed==150?weaponScanOffset+150:0;
        }
        public static void Cleanup()
        {
            if(Time.time-lastCleanup<2f)return;
            lastCleanup=Time.time;
            List<Actor> dead=new List<Actor>();
            foreach(KeyValuePair<Actor,ActorState> entry in states)
            {
                Actor actor=entry.Key;
                if(actor==null || !actor.isAlive()){dead.Add(actor);continue;}
                ActorState s=entry.Value;
                List<string> expired=new List<string>();
                foreach(KeyValuePair<string,double> buff in s.Temporary)
                    if(WorldTime>=buff.Value)
                    {
                        if(CombatBridge.RemoveTrait(actor,buff.Key))expired.Add(buff.Key);
                        // If removeTrait API is unavailable, refrain from retry spam;
                        // Player.log can be used to investigate a lingering buff.
                    }
                foreach(string key in expired)s.Temporary.Remove(key);
            }
            foreach(Actor a in dead)states.Remove(a);
        }
    }
}
