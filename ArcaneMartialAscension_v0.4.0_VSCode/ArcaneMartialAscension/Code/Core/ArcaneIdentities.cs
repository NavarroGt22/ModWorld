using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ArcaneMartialAscension.Core
{
    // D&D archetype names, with original WorldBox attributes and existing AMA spells.
    public sealed class ArcaneIdentity
    {
        public readonly string Id,Name,TraitId,Description,Icon;
        public readonly SorcerousOrigin Origin;
        public readonly TraitAttributes Attributes;
        public readonly float ManaBonus;
        public readonly ReadOnlyCollection<string> Curriculum;
        public bool IsPatron {get{return Origin==SorcerousOrigin.None;}}
        public ArcaneIdentity(string id,string name,string trait,string description,string icon,TraitAttributes attributes,
            float mana,string[] curriculum,SorcerousOrigin origin=SorcerousOrigin.None)
        {Id=id;Name=name;TraitId=trait;Description=description;Icon=icon;Attributes=attributes;ManaBonus=mana;Curriculum=Array.AsReadOnly(curriculum);Origin=origin;}
    }
    public static class ArcaneIdentities
    {
        private const string Spark="ama.spell.evocation.0.centelha_rubra",Ice="ama.spell.evocation.0.estilhaco_glacial",
            Lightning="ama.spell.evocation.1.lanca_eletrica",Ash="ama.spell.evocation.1.rajada_de_cinzas",
            Needle="ama.spell.conjuration.0.agulha_eterea",Blade="ama.spell.conjuration.1.lamina_invocada",
            Touch="ama.spell.necromancy.0.toque_frio",Drain="ama.spell.necromancy.1.dreno_de_vigor",
            Skin="ama.spell.transmutation.0.pele_de_argila",Hands="ama.spell.transmutation.0.dedos_habeis",Steel="ama.spell.transmutation.1.endurecer_aco";
        public static readonly ReadOnlyCollection<ArcaneIdentity> All=Array.AsReadOnly(new[]{
            new ArcaneIdentity("ama.patron.fiend","Corruptor","ama_patron_fiend","Pacto de poder destrutivo: dano e vigor. Adaptação para WorldBox; obrigações de patrono ainda em desenvolvimento.","evocation",new TraitAttributes(health:15,damage:4),10,new[]{Spark,Touch,Ash,Drain}),
            new ArcaneIdentity("ama.patron.archfey","Arquifada","ama_patron_archfey","Pacto feérico: agilidade e proteção. Encantamentos e teleporte próprios ainda em desenvolvimento.","illusion",new TraitAttributes(armor:2,attackSpeed:4),15,new[]{Needle,Hands,Blade,Steel}),
            new ArcaneIdentity("ama.patron.celestial","Celestial","ama_patron_celestial","Pacto de proteção: vitalidade e armadura. Cura celestial específica ainda em desenvolvimento.","abjuration",new TraitAttributes(health:30,armor:2),15,new[]{Needle,Skin,Blade,Steel}),
            new ArcaneIdentity("ama.patron.great_old_one","Grande Antigo","ama_patron_great_old_one","Pacto de conhecimento incomum: reserva de mana e cadência. Telepatia e medo próprios ainda em desenvolvimento.","necromancy",new TraitAttributes(damage:2,attackSpeed:2),30,new[]{Needle,Touch,Drain,Blade}),
            new ArcaneIdentity("ama.origin.draconic","Dracônica","ama_origin_draconic","Origem dracônica: vitalidade e armadura naturais. Herança entre gerações e sopro próprio ainda em desenvolvimento.","evocation",new TraitAttributes(health:25,armor:3),10,new[]{Spark,Skin,Lightning,Ash},SorcerousOrigin.Draconic),
            new ArcaneIdentity("ama.origin.wild","Selvagem","ama_origin_wild","Origem de magia selvagem: potência e reserva arcana. Surtos aleatórios próprios ainda em desenvolvimento.","sorcerer",new TraitAttributes(damage:4),25,new[]{Spark,Ice,Lightning,Ash},SorcerousOrigin.Wild),
            new ArcaneIdentity("ama.origin.aberrant","Aberrante","ama_origin_aberrant","Origem aberrante: grande reserva arcana e cadência. Poderes mentais próprios ainda em desenvolvimento.","necromancy",new TraitAttributes(health:10,attackSpeed:3),30,new[]{Needle,Touch,Drain,Blade},SorcerousOrigin.Aberrant),
            new ArcaneIdentity("ama.origin.clockwork","Mecânica","ama_origin_clockwork","Origem ligada à ordem: armadura e precisão. Equilíbrio temporal próprio ainda em desenvolvimento.","transmutation",new TraitAttributes(armor:4,attackSpeed:2),15,new[]{Needle,Skin,Steel,Blade},SorcerousOrigin.Clockwork),
            new ArcaneIdentity("ama.origin.elemental","Elemental","ama_origin_elemental","Origem elemental do protótipo, preservada para personagens existentes. Potência elemental e reserva arcana.","evocation",new TraitAttributes(damage:3),20,new[]{Spark,Ice,Lightning,Ash},SorcerousOrigin.Elemental),
            new ArcaneIdentity("ama.origin.fey","Feérica","ama_origin_fey","Origem feérica do protótipo, preservada para personagens existentes. Agilidade, proteção e manifestação.","illusion",new TraitAttributes(armor:2,attackSpeed:3),15,new[]{Needle,Hands,Blade,Steel},SorcerousOrigin.Fey)
        });
        public static string NormalizePatron(string id)
        {return id=="ama.patron.ember"?"ama.patron.fiend":id=="ama.patron.veil"?"ama.patron.great_old_one":id;}
        public static ArcaneIdentity Patron(string id)
        {id=NormalizePatron(id);foreach(ArcaneIdentity profile in All)if(profile.IsPatron && profile.Id==id)return profile;return null;}
        public static ArcaneIdentity Lineage(SorcerousOrigin origin)
        {if(origin!=SorcerousOrigin.None)foreach(ArcaneIdentity profile in All)if(profile.Origin==origin)return profile;return null;}
        public static ArcaneIdentity For(ArcaneProgression progress,ArcaneTradition tradition)
        {return progress==null?null:tradition==ArcaneTradition.Warlock?Patron(progress.PatronId):tradition==ArcaneTradition.Sorcerer?Lineage(progress.Origin):null;}
        public static float ManaBonus(ArcaneProgression progress)
        {ArcaneIdentity patron=Patron(progress.PatronId),lineage=Lineage(progress.Origin);return (patron!=null?patron.ManaBonus:0)+(lineage!=null?lineage.ManaBonus:0);}
    }
}
