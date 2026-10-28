using System;
using ArcaneMartialAscension.Spells;
using UnityEngine;

namespace ArcaneMartialAscension.GameIntegration
{
    // Native effects only. Visual failures cannot turn a landed effect into a free cast.
    public sealed class SpellEffectExecutor : ISpellEffectWorld
    {
        private readonly Actor caster,target;
        private readonly Func<Actor,string,float,bool> applyTimedTrait;
        public SpellEffectExecutor(Actor source,Actor destination,Func<Actor,string,float,bool> timedTrait)
        {caster=source;target=destination;applyTimedTrait=timedTrait;}
        public SpellEffectOutcome Apply(SpellEffect effect)
        {
            var result=new SpellEffectOutcome();
            if(caster==null || !caster.isAlive() || target==null || !target.isAlive())return result;
            switch(effect.Kind)
            {
                case SpellEffectKind.Damage:
                case SpellEffectKind.AreaDamage:
                    int before=target.getHealth();
                    CombatBridge.TryDamage(target,effect.Amount,caster);
                    result.HealthLost=Math.Max(0,before-Math.Max(0,target.getHealth()));
                    result.Applied=result.HealthLost>0;
                    if(!result.Applied)return result;
                    result.TargetsAffected=1;
                    // The primary landed effect remains valid if a secondary effect fails.
                    try
                    {
                        if(effect.DrainFraction>0)
                        {
                            int hp=caster.getHealth();
                            CombatBridge.TryHeal(caster,result.HealthLost*effect.DrainFraction);
                            result.HealthRestored=Math.Max(0,caster.getHealth()-hp);
                        }
                        if(effect.Kind==SpellEffectKind.AreaDamage && World.world!=null)
                        {
                            int considered=0;
                            foreach(Actor nearby in World.world.units)
                            {
                                if(++considered>12000 || result.TargetsAffected>=effect.MaxTargets)break;
                                if(nearby==null || nearby==caster || nearby==target || !nearby.isAlive() ||
                                    Vector2.Distance(target.current_position,nearby.current_position)>effect.Radius || !caster.canAttackTarget(nearby,true,false))continue;
                                int hp=nearby.getHealth();
                                CombatBridge.TryDamage(nearby,effect.Amount*.5f,caster);
                                int loss=Math.Max(0,hp-Math.Max(0,nearby.getHealth()));
                                if(loss<=0)continue;
                                result.HealthLost+=loss;result.TargetsAffected++;
                            }
                        }
                    }
                    catch(Exception error){Debug.LogWarning("[AMA Effect] Efeito principal aplicado; falha secundária: "+error);}
                    return result;
                case SpellEffectKind.Heal:
                    int previous=target.getHealth();
                    CombatBridge.TryHeal(target,effect.Amount);
                    result.HealthRestored=Math.Max(0,target.getHealth()-previous);
                    result.Applied=result.HealthRestored>0;
                    break;
                case SpellEffectKind.Buff:
                case SpellEffectKind.Debuff:
                    result.Applied=applyTimedTrait!=null && applyTimedTrait(target,effect.TraitId,effect.Duration);
                    break;
                // Shield absorption, summoning and teleport require dedicated adapters.
                default:return result;
            }
            result.TargetsAffected=result.Applied?1:0;
            return result;
        }
    }
}
