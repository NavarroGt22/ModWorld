using System;
using System.Collections.Generic;
using ArcaneMartialAscension.Core;

namespace ArcaneMartialAscension.Spells
{
    public enum SpellDevelopmentState { Catalogued, Prototype, Implemented, Tested }
    public enum SpellDelivery { Instant, Projectile }
    public enum SpellEffectKind { Damage, AreaDamage, Heal, Shield, Buff, Debuff, Summon, Teleport }
    public enum SpellTargetRule { Hostile, Friendly, Self }
    public enum SpellTargetRelation { None, Self, Ally, Enemy }

    public sealed class SpellEffect
    {
        public readonly SpellEffectKind Kind;
        public readonly float Amount,Duration,Radius,DrainFraction;
        public readonly int MaxTargets;
        public readonly string TraitId;
        public SpellEffect(SpellEffectKind kind,float amount=0,float duration=0,float radius=0,int maxTargets=1,string traitId=null,float drainFraction=0)
        {Kind=kind;Amount=amount;Duration=duration;Radius=radius;MaxTargets=maxTargets;TraitId=traitId;DrainFraction=drainFraction;}
        public bool IsSupported {get{return Kind==SpellEffectKind.Damage || Kind==SpellEffectKind.AreaDamage || Kind==SpellEffectKind.Heal || Kind==SpellEffectKind.Buff || Kind==SpellEffectKind.Debuff;}}
    }
    public sealed class SpellRuntimeProfile
    {
        public readonly SpellDelivery Delivery;
        public readonly SpellTargetRule TargetRule;
        public readonly SpellEffect Effect;
        public readonly float Range;
        public readonly SpellDevelopmentState State;
        public readonly string Note;
        public readonly bool RequiresWeapon;
        public SpellRuntimeProfile(SpellDelivery delivery,SpellTargetRule target,SpellEffect effect,float range,SpellDevelopmentState state=SpellDevelopmentState.Implemented,string note="",bool requiresWeapon=false)
        {Delivery=delivery;TargetRule=target;Effect=effect;Range=range;State=state;Note=note;RequiresWeapon=requiresWeapon;}
    }
    public static class SpellRuntimeCatalog
    {
        private static readonly Dictionary<string,SpellRuntimeProfile> profiles=Build();
        private static Dictionary<string,SpellRuntimeProfile> Build()
        {
            var result=new Dictionary<string,SpellRuntimeProfile>(StringComparer.Ordinal);
            AddDamage(result,"evocation.0.centelha_rubra",8,16,SpellDelivery.Projectile);
            AddDamage(result,"evocation.0.estilhaco_glacial",10,16,SpellDelivery.Projectile,SpellDevelopmentState.Prototype,"Projétil de gelo; lentidão ainda pendente.");
            AddDamage(result,"evocation.1.lanca_eletrica",14,22,SpellDelivery.Instant);
            result.Add("ama.spell.evocation.1.rajada_de_cinzas",new SpellRuntimeProfile(SpellDelivery.Projectile,SpellTargetRule.Hostile,
                new SpellEffect(SpellEffectKind.AreaDamage,14,radius:2.5f,maxTargets:5),22,SpellDevelopmentState.Prototype,"Área circular limitada; cone de chamas ainda pendente."));
            AddDamage(result,"conjuration.0.agulha_eterea",6,16,SpellDelivery.Projectile);
            AddDamage(result,"conjuration.1.lamina_invocada",14,6,SpellDelivery.Instant);
            AddDamage(result,"necromancy.0.toque_frio",8,4,SpellDelivery.Instant,SpellDevelopmentState.Prototype,"Dano próximo; lentidão ainda pendente.");
            result.Add("ama.spell.necromancy.1.dreno_de_vigor",new SpellRuntimeProfile(SpellDelivery.Projectile,SpellTargetRule.Hostile,
                new SpellEffect(SpellEffectKind.Damage,14,drainFraction:.5f),22));
            AddBuff(result,"transmutation.0.pele_de_argila","ama_spell_shield",4);
            AddBuff(result,"transmutation.0.dedos_habeis","ama_spell_focus",4);
            AddBuff(result,"transmutation.1.endurecer_aco","ama_spell_boost",6,true);
            return result;
        }
        private static void AddDamage(Dictionary<string,SpellRuntimeProfile> result,string id,float amount,float range,SpellDelivery delivery,SpellDevelopmentState state=SpellDevelopmentState.Implemented,string note="")
        {result.Add("ama.spell."+id,new SpellRuntimeProfile(delivery,SpellTargetRule.Hostile,new SpellEffect(SpellEffectKind.Damage,amount),range,state,note));}
        private static void AddBuff(Dictionary<string,SpellRuntimeProfile> result,string id,string trait,float duration,bool weapon=false)
        {result.Add("ama.spell."+id,new SpellRuntimeProfile(SpellDelivery.Instant,SpellTargetRule.Friendly,new SpellEffect(SpellEffectKind.Buff,duration:duration,traitId:trait),6,requiresWeapon:weapon));}
        public static SpellRuntimeProfile Find(SpellEntry spell)
        {SpellRuntimeProfile profile;return spell!=null && profiles.TryGetValue(spell.Id,out profile)?profile:null;}
        public static SpellDevelopmentState State(SpellEntry spell)
        {SpellRuntimeProfile profile=Find(spell);return profile==null?SpellDevelopmentState.Catalogued:profile.State;}
        public static string StateName(SpellEntry spell)
        {
            switch(State(spell))
            {
                case SpellDevelopmentState.Prototype:return "Protótipo parcial";
                case SpellDevelopmentState.Implemented:return "Implementada · validação no jogo pendente";
                case SpellDevelopmentState.Tested:return "Testada no jogo";
                default:return "Catalogada · efeito pendente";
            }
        }
    }
    public sealed class SpellCastRequest
    {
        public SpellEntry Spell;
        public long CasterId,TargetId;
        public bool CasterAlive,TargetAlive,Paused,HasSchool,Manual,TargetHasWeapon;
        public SpellTargetRelation Relation;
        public double WorldTime;
        public float Distance;
    }
    public sealed class SpellCastResult
    {
        public readonly bool Applied;
        public readonly string Code,Message;
        public readonly float ManaSpent;
        public readonly int HealthLost,HealthRestored,TargetsAffected;
        public SpellCastResult(bool applied,string code,string message,float mana=0,int lost=0,int healed=0,int targets=0)
        {Applied=applied;Code=code;Message=message;ManaSpent=mana;HealthLost=lost;HealthRestored=healed;TargetsAffected=targets;}
        public static SpellCastResult Reject(string code,string message)
        {return new SpellCastResult(false,code,message+" Mana preservada.");}
    }
    public sealed class SpellEffectOutcome
    {
        public bool Applied;
        public int HealthLost,HealthRestored,TargetsAffected;
    }
    public interface ISpellEffectWorld
    {
        SpellEffectOutcome Apply(SpellEffect effect);
    }
    public sealed class SpellCastTicket
    {
        internal SpellCastTicket(SpellCastRequest request,SpellRuntimeProfile profile)
        {Spell=request.Spell;CasterId=request.CasterId;TargetId=request.TargetId;Profile=profile;}
        public readonly SpellEntry Spell;
        public readonly long CasterId,TargetId;
        public readonly SpellRuntimeProfile Profile;
    }
    // No Unity, Actor or VFX dependency. A successful effect is required to commit cost.
    public sealed class SpellCastingSession
    {
        private readonly CharacterProgression progress;
        private readonly Dictionary<string,double> cooldowns=new Dictionary<string,double>(StringComparer.Ordinal);
        private SpellCastTicket active;
        private bool resolving;
        public bool IsBusy {get{return active!=null;}}
        public SpellCastingSession(CharacterProgression character){progress=character??throw new ArgumentNullException("character");}
        public double CooldownRemaining(SpellEntry spell,double now)
        {double last;return spell!=null && cooldowns.TryGetValue(spell.Id,out last)?Math.Max(0,spell.Cooldown-(now-last)):0;}
        public void RestoreCooldown(SpellEntry spell,double remaining,double now)
        {
            if(spell==null || !progress.Arcane.Knows(spell.Id) || double.IsNaN(remaining) || double.IsInfinity(remaining) ||
                double.IsNaN(now) || double.IsInfinity(now) || remaining<=0 || remaining>spell.Cooldown)return;
            cooldowns[spell.Id]=now-(spell.Cooldown-remaining);
        }
        public SpellCastResult Validate(SpellCastRequest request)
        {return Validate(request,false);}
        private SpellCastResult Validate(SpellCastRequest request,bool completing)
        {
            if(request==null || request.Spell==null || !request.CasterAlive || request.CasterId<=0)return SpellCastResult.Reject("caster","Conjurador indisponível.");
            if(double.IsNaN(request.WorldTime) || double.IsInfinity(request.WorldTime) || request.WorldTime<0 || float.IsNaN(request.Distance) || float.IsInfinity(request.Distance) || request.Distance<0)
                return SpellCastResult.Reject("time_position","Tempo ou posição inválidos.");
            if(request.Paused)return SpellCastResult.Reject("paused","Retome o jogo para conjurar.");
            SpellEntry spell=request.Spell;
            SpellRuntimeProfile profile=SpellRuntimeCatalog.Find(spell);
            if(profile==null || !profile.Effect.IsSupported || profile.State==SpellDevelopmentState.Catalogued)return SpellCastResult.Reject("pending","Efeito ainda não implementado.");
            if(!completing && active!=null)return SpellCastResult.Reject("busy","Já existe uma conjuração em andamento.");
            if(!request.HasSchool)return SpellCastResult.Reject("school","O personagem não possui a escola da magia.");
            if(!progress.Arcane.Knows(spell.Id))return SpellCastResult.Reject("unknown","Magia ainda não aprendida.");
            if(progress.Arcane.GetCircle(progress.Arcane.TraditionFor(spell.Id))<spell.Circle)return SpellCastResult.Reject("circle","Círculo insuficiente nesta tradição.");
            if(spell.Mana<=0 || float.IsNaN(spell.Mana) || float.IsInfinity(spell.Mana) || progress.Mana<spell.Mana)return SpellCastResult.Reject("mana","Mana insuficiente ou custo inválido.");
            if(spell.Cooldown<=0 || float.IsNaN(spell.Cooldown) || float.IsInfinity(spell.Cooldown))return SpellCastResult.Reject("cooldown_data","Recarga inválida.");
            double remaining=CooldownRemaining(spell,request.WorldTime);
            if(remaining>0)return SpellCastResult.Reject("cooldown","Recarga: "+Math.Ceiling(remaining)+" s.");
            if(!request.TargetAlive || request.TargetId<=0)return SpellCastResult.Reject("target","Alvo indisponível.");
            if(profile.TargetRule==SpellTargetRule.Hostile && (request.TargetId==request.CasterId || (!request.Manual && request.Relation!=SpellTargetRelation.Enemy)))return SpellCastResult.Reject("hostile","Escolha um alvo hostil válido.");
            if(profile.TargetRule==SpellTargetRule.Friendly && request.Relation!=SpellTargetRelation.Ally && request.Relation!=SpellTargetRelation.Self)return SpellCastResult.Reject("friendly","Esta magia exige um aliado ou o próprio conjurador.");
            if(profile.TargetRule==SpellTargetRule.Self && request.TargetId!=request.CasterId)return SpellCastResult.Reject("self","A magia só pode ser usada no conjurador.");
            if(request.Distance>profile.Range)return SpellCastResult.Reject("range","Alvo fora do alcance de "+profile.Range+" blocos.");
            if(profile.RequiresWeapon && !request.TargetHasWeapon)return SpellCastResult.Reject("weapon","O alvo precisa de uma arma equipada.");
            return null;
        }
        public SpellCastResult TryReserve(SpellCastRequest request,out SpellCastTicket ticket)
        {
            ticket=null;
            SpellCastResult error=Validate(request);
            if(error!=null)return error;
            active=ticket=new SpellCastTicket(request,SpellRuntimeCatalog.Find(request.Spell));
            return null;
        }
        public void Cancel(SpellCastTicket ticket){if(!resolving && ReferenceEquals(active,ticket))active=null;}
        public SpellCastResult Complete(SpellCastTicket ticket,SpellCastRequest current,ISpellEffectWorld world)
        {
            if(ticket==null || !ReferenceEquals(active,ticket) || resolving)return SpellCastResult.Reject("ticket","Conjuração já finalizada ou inválida.");
            resolving=true;
            try
            {
                if(current==null || current.CasterId!=ticket.CasterId || current.TargetId!=ticket.TargetId || current.Spell!=ticket.Spell || world==null)return SpellCastResult.Reject("identity","O alvo ou conjurador da conjuração mudou.");
                SpellCastResult error=Validate(current,true);
                if(error!=null)return error;
                SpellEffectOutcome outcome=world.Apply(ticket.Profile.Effect);
                if(outcome==null || !outcome.Applied)return SpellCastResult.Reject("no_effect","O efeito não alterou o alvo.");
                if(!progress.CastSpell(ticket.Spell.Mana))throw new InvalidOperationException("Efeito aplicado sem mana disponível.");
                cooldowns[ticket.Spell.Id]=current.WorldTime;
                int xp=8+ticket.Spell.Circle*2;
                progress.Arcane.Study(progress.Arcane.TraditionFor(ticket.Spell.Id),xp);progress.StudyMagic(ticket.Spell.School,xp);
                return new SpellCastResult(true,"applied",ticket.Spell.Name+": efeito aplicado.",ticket.Spell.Mana,outcome.HealthLost,outcome.HealthRestored,outcome.TargetsAffected);
            }
            finally {active=null;resolving=false;}
        }
    }
}
