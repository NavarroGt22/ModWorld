using System;
using System.Collections.Generic;
using UnityEngine;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Spells;
using ArcaneMartialAscension.VisualEffects;

namespace ArcaneMartialAscension.GameIntegration
{
    public static class ActorAbilities
    {
        private sealed class ActorState
        {
            public readonly CharacterProgression Progress = new CharacterProgression();
            public readonly HashSet<string> KnownSpells = new HashSet<string>(StringComparer.Ordinal);
            public readonly Dictionary<string,float> Cooldowns = new Dictionary<string,float>(StringComparer.Ordinal);
            public readonly Dictionary<string,float> Temporary = new Dictionary<string,float>(StringComparer.Ordinal);
            public float LastSword, LastMagic, LastAura, LastUpdate;
            public bool Initialized;
        }
        private static readonly Dictionary<Actor, ActorState> states = new Dictionary<Actor, ActorState>();
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
                // These are sandbox-test starting skills; real character learning will be separate.
                s.Progress.Arcane.Study(ArcaneTradition.Wizard,100);
                s.Progress.Arcane.SetPatron("test_patron");
                s.Progress.Arcane.SetOrigin(SorcerousOrigin.Elemental);
                s.KnownSpells.Add("ama.spell.evocation.0.centelha_rubra");
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
        public static void EnsureWeapon(Actor actor,WeaponType type)
        {Ensure(actor,ModTraits.WeaponTrait(type));}
        public static void GainWeaponXp(Actor actor,WeaponType type,int xp)
        {
            if(actor==null || !actor.isAlive()) return;
            ActorState s=State(actor);
            s.Progress.TrainWeapon(type,xp);
            int level=s.Progress.GetWeaponLevel(type);
            if(level>=2) Ensure(actor,ModTraits.RankId(type,1));
            if(level>=4) Ensure(actor,ModTraits.RankId(type,2));
            if(level>=6) Ensure(actor,ModTraits.RankId(type,3));
        }
        public static bool OnSwordAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnWeaponAttack(a,b,WeaponType.Sword);}
        public static bool OnSpearAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnWeaponAttack(a,b,WeaponType.Spear);}
        public static bool OnBowAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnWeaponAttack(a,b,WeaponType.Bow);}
        public static bool OnAxeAttack(BaseSimObject a,BaseSimObject b,WorldTile t)
        {return OnWeaponAttack(a,b,WeaponType.Axe);}
        private static bool OnWeaponAttack(BaseSimObject attacker,BaseSimObject defender,WeaponType type)
        {
            if(attacker==null || !attacker.isAlive() || attacker.a==null) return false;
            Actor caster=attacker.a;
            WeaponType equipped;
            if(GameWeaponDetector.TryDetect(caster,out equipped)) type=equipped;
            EnsureWeapon(caster,type);
            GainWeaponXp(caster,type,2);
            ActorState s=State(caster);
            if(Time.time-s.LastSword>=1.2f)
            {
                s.LastSword=Time.time;
                int level=s.Progress.GetWeaponLevel(type);
                // Supplemental nonlethal chip damage makes mastery observable even if
                // this WorldBox build ignores bonus stats on dynamically added traits.
                if(defender!=null && defender.a!=null && level>0)
                    CombatBridge.TryDamage(defender.a,Mathf.Min(9f,level*1.3f));
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
            ActorState s=State(caster);
            if(Time.time-s.LastMagic<3f) return true;
            s.LastMagic=Time.time;
            if(defender.a==null) return true;
            SpellEntry entry=SpellCatalog.Find("ama.spell.evocation.0.centelha_rubra");
            if(entry!=null)
            {
                // Combat AI uses the basic tested spell only, limited by mana and cooldown.
                string outcome=Cast(caster,defender.a,entry,false);
                if(outcome.StartsWith("OK")) s.Progress.Arcane.Study(tradition,3);
            }
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
        public static void TriggerWeapon(Actor actor,WeaponType type)
        {
            if(actor==null || !actor.isAlive())return;
            EnsureWeapon(actor,type);
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
            if(s.Progress.Arcane.HighestCircle<spell.Circle) return "Circulo insuficiente; estudar magia F12";
            Ensure(actor,ModTraits.Wizard);
            Ensure(actor,ModTraits.SchoolTrait(spell.School));
            if(!s.KnownSpells.Add(spell.Id)) return "Magia ja aprendida: "+spell.Name;
            return "Aprendeu: "+spell.Name;
        }
        public static bool Knows(Actor actor,SpellEntry spell)
        {
            return actor!=null && spell!=null && State(actor).KnownSpells.Contains(spell.Id);
        }
        public static string Cast(Actor caster,Actor target,SpellEntry spell,bool manual)
        {
            if(caster==null || !caster.isAlive() || spell==null)return "Sem conjurador ou magia";
            // The ring is emitted even for an invalid cast, so failures are visually diagnosable.
            Color color=ColorFor(spell.School);
            ScreenEffects.Emit(null,0,color,"CIRCULO "+spell.Circle,false);
            if(!SpellCatalog.HasRuntime(spell))return "Mecanica pendente (sem custo): "+spell.Name;
            ActorState s=State(caster);
            if(manual)
            {
                Ensure(caster,ModTraits.Wizard);
                Ensure(caster,ModTraits.SchoolTrait(spell.School));
            }
            if(!caster.hasTrait(ModTraits.SchoolTrait(spell.School)))return "Precisa do traco da escola";
            if(!s.KnownSpells.Contains(spell.Id))return "Magia nao aprendida: clique Aprender";
            if(s.Progress.Arcane.HighestCircle<spell.Circle)return "Circulo bloqueado: treine magia";
            float last;
            if(s.Cooldowns.TryGetValue(spell.Id,out last) && Time.time-last<spell.Cooldown)
                return "Recarga: "+Mathf.CeilToInt(spell.Cooldown-(Time.time-last))+"s";
            if(s.Progress.Mana<spell.Mana)return "Mana insuficiente";

            bool applied=false;
            float potency=5f+4f*spell.Circle;
            if(spell.Category=="dano" || spell.Category=="dreno_vital" || spell.Category=="dano_em_area")
            {
                if(target==null || target==caster || !target.isAlive())return "Aponte para um ALVO diferente do mago";
                applied=CombatBridge.TryDamage(target,potency);
                if(applied && spell.Category=="dreno_vital") CombatBridge.TryHeal(caster,potency*.5f);
                if(applied && spell.Category=="dano_em_area")
                {
                    // Controlled extra splash for the early prototype; prevent indiscriminate mass scans.
                    int hits=0;Vector3 impact=GameCompat.PositionOrCursor(target);
                    foreach(Actor nearby in World.world.units)
                    {
                        if(nearby==null || nearby==target || nearby==caster || !nearby.isAlive())continue;
                        Vector3 other;
                        if(GameCompat.GetPosition(nearby,out other) && Vector3.Distance(impact,other)<2.5f)
                        {CombatBridge.TryDamage(nearby,potency*.5f);if(++hits>=5)break;}
                    }
                }
            }
            else if(spell.Category=="escudo" || spell.Category=="melhoria" ||
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
            if(Main.Visuals!=null)Main.Visuals.SpawnCircle(GameCompat.PositionOrCursor(caster),spell.Circle,color);
            if(target!=null) ScreenEffects.Emit(target,3,color,"IMPACTO",false);
            return "OK: "+spell.Name+" | mana -"+spell.Mana;
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
        public static string WeaponStatus(Actor actor,WeaponType type)
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
                    WeaponType equipment;
                    if(GameWeaponDetector.TryDetect(actor,out equipment))EnsureWeapon(actor,equipment);
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
