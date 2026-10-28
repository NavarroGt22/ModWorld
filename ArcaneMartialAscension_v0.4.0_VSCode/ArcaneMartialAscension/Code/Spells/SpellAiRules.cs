using System;
using ArcaneMartialAscension.Core;

namespace ArcaneMartialAscension.Spells
{
    public static class SpellAiRules
    {
        public static SpellEntry Choose(CharacterProgression progress,SpellCastingSession session,
            Func<SpellEntry,SpellCastRequest> requestFor,string preferredId=null)
        {
            if(progress==null || session==null || requestFor==null || session.IsBusy)return null;
            SpellEntry best=null;
            foreach(string id in progress.Arcane.KnownSpellIds)
            {
                SpellEntry spell=SpellCatalog.Find(id);
                SpellRuntimeProfile profile=SpellRuntimeCatalog.Find(spell);
                if(profile==null || profile.TargetRule!=SpellTargetRule.Hostile || session.Validate(requestFor(spell))!=null)continue;
                if(spell.Id==preferredId)return spell;
                if(best==null || spell.Circle>best.Circle || (spell.Circle==best.Circle &&
                    (spell.Mana<best.Mana || (spell.Mana==best.Mana && string.CompareOrdinal(spell.Id,best.Id)<0))))best=spell;
            }
            return best;
        }
    }
}
