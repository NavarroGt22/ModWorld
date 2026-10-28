using System;
using System.Collections.Generic;
using UnityEngine;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Spells;
using ArcaneMartialAscension.VisualEffects;
using ArcaneMartialAscension.Learning;
using MartialWeaponType = ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.GameIntegration
{
    public static class ActorAbilities
    {
        private sealed class ActorState
        {
            public readonly CharacterProgression Progress = new CharacterProgression();
            public readonly Dictionary<string,float> Cooldowns = new Dictionary<string,float>(StringComparer.Ordinal);
            public readonly Dictionary<string,float> Temporary = new Dictionary<string,float>(StringComparer.Ordinal);
            public float LastSword, LastMagic, LastAura, LastUpdate;
            public float LastDiagnostic;
            public string PreferredSpellId, LastCast;
            public PendingCast Pending;
            public bool Initialized;
        }
        private sealed class PendingCast
        {
            public Actor Caster,Target;
            public ActorState State;
            public SpellEntry Spell;
            public Vector3 From;
            public float Age,Duration;
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
        private static ActorState State(Actor actor)
        {
            ActorState s;
            if(!states.TryGetValue(actor,out s))
            {s=new ActorState();states[actor]=s;}
            if(!s.Initialized)
            {
                s.Initialized=true;
                s.LastUpdate=Time.time;
                // Powers now come from recorded study, practice or explicit player actions.
            }
            float dt=Mathf.Clamp(Time.time-s.LastUpdate,0f,10f);
            s.Progress.GrowMana(s.Progress.Arcane.HighestCircle);
            s.Progress.Regenerate(dt);
            s.LastUpdate=Time.time;
            return s;
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
            if(actor==null || !actor.isAlive()) return;
            ActorState s=State(actor);
            s.Progress.TrainWeapon(type,xp);
            s.Progress.RecordOrigin(LearningRules.WeaponKnowledgeId(type),
                new KnowledgeOrigin(KnowledgeSource.Player,World.world!=null?World.world.getCurWorldTime():0,actor.getID()));
            s.Progress.Learning.LastAction="Treino solicitado pelo jogador: "+LearningRules.WeaponName(type);
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
            if(attacker==null || !attacker.isAlive() || attacker.a==null || defender==null || !defender.isAlive()) return false;
            Actor caster=attacker.a;
            MartialWeaponType equipped;
            if(!GameWeaponDetector.TryDetect(caster,out equipped))return true;
            type=equipped;
            ActorState s=State(caster);
            if(!LearningRules.AwardCombat(s.Progress,type,Time.frameCount,World.world.getCurWorldTime(),caster.getID()))return true;
            SyncWeaponTraits(caster,type);
            if(Time.time-s.LastSword>=1.2f)
            {
                s.LastSword=Time.time;
                int level=s.Progress.GetWeaponLevel(type);
                // Supplemental nonlethal chip damage makes mastery observable even if
                // this WorldBox build ignores bonus stats on dynamically added traits.
                if(defender!=null && defender.a!=null && level>0)
                    CombatBridge.TryDamage(defender.a,Mathf.Min(9f,level*1.3f),caster);
                if(Main.Visuals!=null) Main.Visuals.SpawnSwordArc(GameCompat.PositionOrCursor(attacker));
            }
            return true;
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
            Ensure(actor,ModTraits.Aura);
            ActorState s=State(actor);
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
            s.Progress.Arcane.Study(ArcaneTradition.Wizard,250);
            s.Progress.GrowMana(s.Progress.Arcane.HighestCircle);
        }
        public static string Learn(Actor actor,SpellEntry spell)
        {
            if(actor==null || !actor.isAlive() || spell==null) return "Sem personagem/magia";
            ActorState s=State(actor);
            if(s.Progress.Arcane.GetCircle(ArcaneTradition.Wizard)<spell.Circle) return "Circulo insuficiente; estudar magia F12";
            if(s.Progress.Arcane.Knows(spell.Id))return "Magia ja aprendida: "+spell.Name;
            if(!LearningRules.Learn(s.Progress,spell,KnowledgeSource.Player,World.world!=null?World.world.getCurWorldTime():0,actor.getID()))
                return "Requisitos de aprendizado não atendidos.";
            Ensure(actor,ModTraits.Wizard);
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
            return actor!=null && states.TryGetValue(actor,out state)?state.Progress:null;
        }
        public static KnowledgeOrigin OriginForDisplay(Actor actor,string knowledgeId)
        {
            CharacterProgression progress=ProgressForDisplay(actor);
            return progress!=null?progress.GetOrigin(knowledgeId):null;
        }

        public static string LearningStatusForDisplay(Actor actor)
        {
            if(actor==null || !actor.isAlive())return "Selecione um habitante vivo.";
            if(!actor.isSapient() || !actor.isAdult())return "Aprendizado autônomo disponível para habitantes adultos.";
            if(actor._has_status_sleeping)return "Descansando; estudo e prática aguardam.";
            if(actor.attack_target!=null && actor.attack_target.isAlive())return "Em combate; estudo em paz interrompido.";
            CharacterProgression progress=ProgressForDisplay(actor);
            return progress!=null?progress.Learning.LastAction:"Aguardando vocação arcana ou uma arma equipada.";
        }

        public static void ObserveLearning(Actor actor,double now)
        {
            if(actor==null || !actor.isAlive() || !actor.isSapient() || !actor.isAdult())return;
            long id=actor.getID();
            bool wizard=actor.hasTrait(ModTraits.Wizard);
            bool otherPath=actor.hasTrait(ModTraits.Warlock) || actor.hasTrait(ModTraits.Sorcerer);
            bool magic=wizard || (!otherPath && (actor.hasTrait(ModTraits.Evocation) || LearningRules.HasArcaneAptitude(id)));
            MartialWeaponType weapon;
            bool armed=GameWeaponDetector.TryDetect(actor,out weapon);
            ActorState state;
            if(!magic && !armed && !states.TryGetValue(actor,out state))
            {SyncWeaponTraits(actor,null);return;}
            state=State(actor);
            int before=armed?state.Progress.GetWeaponLevel(weapon):0;
            LearningOutcome outcome=LearningRules.Tick(state.Progress,new LearningContext{
                ActorId=id,WorldTime=now,Eligible=true,MagicEligible=magic,
                Available=!actor._has_status_sleeping && (actor.attack_target==null || !actor.attack_target.isAlive()),
                Weapon=armed?(MartialWeaponType?)weapon:null
            });
            if(outcome.LearnedSpell!=null)
            {
                Ensure(actor,ModTraits.Wizard);
                Ensure(actor,ModTraits.SchoolTrait(outcome.LearnedSpell.School));
                Debug.Log("[AMA Learning] learned actor="+id+" spell="+outcome.LearnedSpell.Id+" source=self_study worldTime="+now);
            }
            SyncWeaponTraits(actor,armed?(MartialWeaponType?)weapon:null);
            if(armed && state.Progress.GetWeaponLevel(weapon)>before)
                Debug.Log("[AMA Learning] mastery actor="+id+" weapon="+weapon+" level="+state.Progress.GetWeaponLevel(weapon)+" source=practice");
        }

        public static void SyncWeaponTraits(Actor actor,MartialWeaponType? equipped)
        {
            if(actor==null || !actor.isAlive())return;
            CharacterProgression progress=ProgressForDisplay(actor);
            for(int i=0;i<4;i++)
            {
                MartialWeaponType type=(MartialWeaponType)i;
                bool active=equipped.HasValue && equipped.Value==type;
                string trait=ModTraits.WeaponTrait(type);
                if(active)Ensure(actor,trait);
                else if(actor.hasTrait(trait))CombatBridge.RemoveTrait(actor,trait);
                int level=active && progress!=null?progress.GetWeaponLevel(type):0;
                int tier=level>=6?3:level>=4?2:level>=2?1:0;
                for(int rank=1;rank<=3;rank++)
                {
                    string rankId=ModTraits.RankId(type,rank);
                    if(active && rank==tier)Ensure(actor,rankId);
                    else if(actor.hasTrait(rankId))CombatBridge.RemoveTrait(actor,rankId);
                }
            }
        }

        public static void ResetLearningObservation()
        {foreach(ActorState state in states.Values)state.Progress.Learning.ResetObservation();}

        public static void ResetRuntimeForWorld()
        {
            foreach(PendingCast cast in pending)Finish(cast,"Mundo alterado; conjuração cancelada sem custo.");
            pending.Clear();states.Clear();combatActors.Clear();
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
        private static string BeginCast(Actor caster,Actor target,SpellEntry spell,bool manual)
        {
            if(caster==null || !caster.isAlive() || spell==null)return "Sem conjurador ou magia";
            if(World.world==null)return "Mundo indisponível; mana preservada.";
            if(World.world.isPaused())return "Retome o jogo para conjurar. Mana preservada.";
            if(!SpellCatalog.HasRuntime(spell))return "Mecanica pendente (sem custo): "+spell.Name;
            if(spell.Mana<=0 || float.IsNaN(spell.Mana) || float.IsInfinity(spell.Mana))return "Custo inválido; mana preservada.";
            ActorState s=State(caster);
            if(s.Pending!=null)return "Já está lançando "+s.Pending.Spell.Name;
            if(!manual && !caster.hasTrait(ModTraits.SchoolTrait(spell.School)))return "Precisa do traco da escola";
            if(!s.Progress.Arcane.Knows(spell.Id))return "Magia nao aprendida: clique Aprender";
            if(s.Progress.Arcane.HighestCircle<spell.Circle)return "Circulo bloqueado: treine magia";
            float last;
            if(s.Cooldowns.TryGetValue(spell.Id,out last) && Time.time-last<spell.Cooldown)
                return "Recarga: "+Mathf.CeilToInt(spell.Cooldown-(Time.time-last))+"s";
            if(s.Progress.Mana<spell.Mana)return "Mana insuficiente";
            Color color=ColorFor(spell.School);
            if(IsOffensive(spell))
            {
                if(target==null || target==caster || !target.isAlive())return "Aponte para um alvo vivo diferente do mago. Mana preservada.";
                float distance=Vector2.Distance(caster.current_position,target.current_position);
                if(distance>Range(spell))return "Alvo fora do alcance ("+Mathf.CeilToInt(distance)+" / "+Range(spell)+"). Mana preservada.";
                if(pending.Count>=128)return "Muitas conjurações em andamento; tente novamente.";
                if(manual){Ensure(caster,ModTraits.Wizard);Ensure(caster,ModTraits.SchoolTrait(spell.School));}
                Vector3 from,to;
                if(!GameCompat.GetPosition(caster,out from) || !GameCompat.GetPosition(target,out to))return "Posição indisponível; mana preservada.";
                PendingCast cast=new PendingCast{Caster=caster,Target=target,State=s,Spell=spell,
                    From=from+Vector3.up*.5f,Duration=Mathf.Clamp(distance/12f,.55f,1.1f),Manual=manual};
                cast.Log=manual || Time.time-s.LastDiagnostic>10f;
                if(cast.Log)s.LastDiagnostic=Time.time;
                s.Pending=cast;
                pending.Add(cast);
                try
                {
                    cast.Flight=ScreenEffects.Launch(cast.From,to+Vector3.up*.5f,color);
                    if(Main.Visuals!=null)
                    {
                        Main.Visuals.SpawnCircle(from,spell.Circle,color);
                        cast.Sprite=Main.Visuals.SpawnBolt(cast.From,color);
                    }
                    else ScreenEffects.Emit(from,0,color,"CÍRCULO "+spell.Circle,false);
                }
                catch(Exception error){Debug.LogWarning("[AMA VFX] Falha visual; o projétil mantém sua simulação: "+error);}
                if(cast.Log)Debug.Log("[AMA Cast] launched spell="+spell.Id+" caster="+caster.getID()+" target="+target.getID()+" mana="+s.Progress.Mana);
                return "Lançando "+spell.Name+" em "+GameCompat.DisplayName(target);
            }
            if(manual){Ensure(caster,ModTraits.Wizard);Ensure(caster,ModTraits.SchoolTrait(spell.School));}
            bool applied=false;
            if(spell.Category=="escudo" || spell.Category=="melhoria" ||
                spell.Category=="deteccao" || spell.Category=="ilusao" ||
                spell.Category=="controle" || spell.Category=="enfraquecimento")
            {
                string buff=ModTraits.SpellBoost;
                if(spell.Category=="escudo")buff=ModTraits.SpellShield;
                if(spell.Category=="deteccao")buff=ModTraits.SpellFocus;
                if(spell.Category=="ilusao")buff=ModTraits.SpellIllusion;
                if(spell.Category=="controle"||spell.Category=="enfraquecimento")buff=ModTraits.SpellWeaken;
                bool hostile=spell.Category=="controle"||spell.Category=="enfraquecimento";
                Actor recipient=hostile?target:caster;
                if(recipient==null || !recipient.isAlive() || (hostile && recipient==caster))
                    return "Aponte para um alvo valido";
                Ensure(recipient,buff);
                ActorState state=State(recipient);
                state.Temporary[buff]=Time.time+Mathf.Max(4f,3f+spell.Circle*2f);
                applied=recipient.hasTrait(buff);
            }
            if(!applied) return "Efeito do jogo indisponivel; sem consumo de mana";
            if(!s.Progress.CastSpell(spell.Mana))return "Mana insuficiente";
            s.Cooldowns[spell.Id]=Time.time;
            s.Progress.Arcane.Study(ArcaneTradition.Wizard,8+spell.Circle*2);
            s.Progress.StudyMagic(spell.School,8+spell.Circle*2);
            Vector3 casterPosition;
            if(Main.Visuals!=null && GameCompat.GetPosition(caster,out casterPosition))Main.Visuals.SpawnCircle(casterPosition,spell.Circle,color);
            if(target!=null) ScreenEffects.Emit(target,3,color,"IMPACTO",false);
            return "OK: "+spell.Name+" | mana -"+spell.Mana;
        }

        private static bool IsOffensive(SpellEntry spell)
        {return spell!=null && (spell.Category=="dano" || spell.Category=="dreno_vital" || spell.Category=="dano_em_area");}
        private static float Range(SpellEntry spell){return 16f+spell.Circle*6f;}

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
                cast.Age+=Time.deltaTime;
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
                Time.time-state.LastMagic<.75f)return;
            if(!caster.hasTrait(ModTraits.Wizard) && !caster.hasTrait(ModTraits.Warlock) && !caster.hasTrait(ModTraits.Sorcerer))return;
            state.LastMagic=Time.time;
            try {if(!caster.canAttackTarget(target,true,false))return;}
            catch(Exception error)
            {
                if(Time.time-state.LastDiagnostic>10f)
                {state.LastDiagnostic=Time.time;Debug.LogWarning("[AMA AI] Falha ao validar alvo de combate: "+error.Message);}
                return;
            }
            SpellEntry best=null;
            foreach(string id in state.Progress.Arcane.KnownSpellIds)
            {
                SpellEntry spell=SpellCatalog.Find(id);
                if(!IsOffensive(spell) || !SpellCatalog.HasRuntime(spell) || spell.Circle>state.Progress.Arcane.HighestCircle ||
                    spell.Mana>state.Progress.Mana || !caster.hasTrait(ModTraits.SchoolTrait(spell.School)) ||
                    Vector2.Distance(caster.current_position,target.current_position)>Range(spell))continue;
                float used;
                if(state.Cooldowns.TryGetValue(spell.Id,out used) && Time.time-used<spell.Cooldown)continue;
                if(spell.Id==state.PreferredSpellId){best=spell;break;}
                if(best==null || spell.Circle>best.Circle)best=spell;
            }
            if(best!=null)Cast(caster,target,best,false);
        }

        private static void ResolveImpact(PendingCast cast,Vector3 impact)
        {
            ActorState state=cast.State;
            SpellEntry spell=cast.Spell;
            if(state.Progress.Mana<spell.Mana){Finish(cast,"Mana insuficiente no impacto; sem cobrança.");return;}
            int before=cast.Target.getHealth();
            float potency=8f+6f*spell.Circle;
            bool applied=CombatBridge.TryDamage(cast.Target,potency,cast.Caster);
            int after=cast.Target.getHealth();
            if(!applied){Finish(cast,"O alvo não recebeu dano (proteção ou imunidade). Mana preservada.");return;}
            // No other cast can spend this actor's mana while its projectile is in flight.
            if(!state.Progress.CastSpell(spell.Mana))throw new InvalidOperationException("Dano aplicado sem mana reservada.");
            state.Cooldowns[spell.Id]=Time.time;
            state.Progress.Arcane.Study(ArcaneTradition.Wizard,8+spell.Circle*2);
            state.Progress.StudyMagic(spell.School,8+spell.Circle*2);
            if(cast.Log)Debug.Log("[AMA Cast] applied spell="+spell.Id+" caster="+cast.Caster.getID()+" target="+cast.Target.getID()+
                " hp="+before+"->"+after+" mana="+state.Progress.Mana+" cooldown="+spell.Cooldown);
            try
            {
                if(spell.Category=="dreno_vital")CombatBridge.TryHeal(cast.Caster,(before-after)*.5f);
                if(spell.Category=="dano_em_area")
                {
                    int hits=0;
                    foreach(Actor nearby in World.world.units)
                    {
                        if(nearby==null || nearby==cast.Target || nearby==cast.Caster || !nearby.isAlive())continue;
                        if(Vector2.Distance(cast.Target.current_position,nearby.current_position)>2.5f || !cast.Caster.canAttackTarget(nearby,true,false))continue;
                        CombatBridge.TryDamage(nearby,potency*.5f,cast.Caster);
                        if(++hits>=5)break;
                    }
                }
            }
            catch(Exception error){Debug.LogWarning("[AMA Cast] Impacto principal aplicado; efeito secundário falhou: "+error);}
            Finish(cast,spell.Name+": impacto! Vida "+before+" → "+Mathf.Max(0,after)+" · Mana -"+spell.Mana);
            try {ScreenEffects.Emit(impact,2,ColorFor(spell.School),"-"+(before-after)+" VIDA",false);}
            catch(Exception error){Debug.LogWarning("[AMA VFX] Impacto aplicado; falha visual: "+error.Message);}
        }

        private static void Finish(PendingCast cast,string result)
        {
            cast.State.Pending=null;
            cast.State.LastCast=result;
            if(cast.Flight!=null)ScreenEffects.Finish(cast.Flight);
            if(cast.Sprite!=null)UnityEngine.Object.Destroy(cast.Sprite.gameObject);
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
                foreach(KeyValuePair<string,float> buff in s.Temporary)
                    if(Time.time>=buff.Value)
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
