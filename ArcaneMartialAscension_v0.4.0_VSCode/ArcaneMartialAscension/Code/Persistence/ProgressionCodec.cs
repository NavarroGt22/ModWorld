using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Learning;
using ArcaneMartialAscension.Martial;
using ArcaneMartialAscension.Spells;
using Weapon=ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.Persistence
{
    [DataContract] public sealed class SavedSpell
    {[DataMember]public string Id;[DataMember]public string Tradition;[DataMember]public double Remaining;}
    [DataContract] public sealed class SavedTechnique
    {[DataMember]public string Id;[DataMember]public int Uses;[DataMember]public double Remaining;}
    [DataContract] public sealed class SavedOrigin
    {[DataMember]public string Id,Source;[DataMember]public long ActorId;[DataMember]public double WorldTime;}
    [DataContract] public sealed class SavedBuff
    {[DataMember]public string Id;[DataMember]public double Remaining;}
    [DataContract] public sealed class ProgressionSnapshot
    {
        [DataMember]public int Version=1;
        [DataMember]public long ActorId;
        [DataMember]public double SavedAt;
        [DataMember]public float Mana,Aura;
        [DataMember]public bool AuraAwakened,FormationReady;
        [DataMember]public string PatronId,OriginId,FormationTradition,PreferredSpell,StudyingSpell,LastAction;
        [DataMember]public double FormationWork,StudyClock,SpellWork,PracticeClock;
        [DataMember]public int? LastWeapon;
        [DataMember]public string LastTradition;
        [DataMember]public int[] TraditionXp=new int[3],SchoolXp=new int[8],WeaponXp=new int[4];
        [DataMember]public List<SavedSpell> Spells=new List<SavedSpell>();
        [DataMember]public List<SavedTechnique> Techniques=new List<SavedTechnique>();
        [DataMember]public List<SavedOrigin> Origins=new List<SavedOrigin>();
        [DataMember]public List<SavedBuff> Buffs=new List<SavedBuff>();
    }
    public enum SnapshotStatus { Missing,Restored,ForeignActor,Unsupported,Invalid }
    // Only plain bounded data crosses the save boundary; no Actor, Unity object or CLR type names.
    public static class ProgressionCodec
    {
        public const int Version=1,MaxLength=65536;
        public static string TraditionId(ArcaneTradition tradition){return tradition.ToString().ToLowerInvariant();}
        private static bool TryTradition(string id,out ArcaneTradition tradition)
        {return Enum.TryParse(id,true,out tradition) && Enum.IsDefined(typeof(ArcaneTradition),tradition);}
        private static bool Finite(double value){return !double.IsNaN(value) && !double.IsInfinity(value);}
        private static void Require(bool value,string error){if(!value)throw new SerializationException(error);}
        private static void Counter(double value,string name)
        {Require(Finite(value) && value>=0 && value<=1000000,"Valor inválido: "+name);}
        private static void Experience(int[] values,int size)
        {Require(values!=null && values.Length==size,"Lista de experiência inválida.");foreach(int value in values)Require(value>=0 && value<=1000000,"Experiência fora dos limites.");}
        public static bool IsOwnedBuff(string id)
        {return id=="ama_spell_shield" || id=="ama_spell_boost" || id=="ama_spell_focus" || id=="ama_spell_illusion" || id=="ama_spell_weaken";}
        public static TechniqueEntry Technique(string id)
        {for(int i=0;i<4;i++)foreach(TechniqueEntry entry in TechniqueCatalog.ForWeapon((Weapon)i))if(entry.Id==id)return entry;return null;}
        public static ProgressionSnapshot Capture(CharacterProgression progress,long actorId,double now)
        {
            if(progress==null || actorId<=0 || !Finite(now) || now<0)throw new ArgumentException("Progresso, Actor e tempo válidos são necessários.");
            ArcaneIdentity lineage=ArcaneIdentities.Lineage(progress.Arcane.Origin);
            var snapshot=new ProgressionSnapshot{ActorId=actorId,SavedAt=now,Mana=progress.Mana,Aura=progress.Aura,AuraAwakened=progress.HasAwakenedAura,
                PatronId=progress.Arcane.PatronId,OriginId=lineage!=null?lineage.Id:null,
                FormationTradition=progress.Formation.Tradition.HasValue?TraditionId(progress.Formation.Tradition.Value):null,
                FormationReady=progress.Formation.Ready,FormationWork=progress.Formation.Work,
                StudyClock=progress.Learning.StudyClock,SpellWork=progress.Learning.SpellWork,PracticeClock=progress.Learning.PracticeClock,
                StudyingSpell=progress.Learning.StudyingSpellId,LastAction=progress.Learning.LastAction,
                LastWeapon=progress.Learning.LastWeapon.HasValue?(int?)progress.Learning.LastWeapon.Value:null,LastTradition=TraditionId(progress.Learning.LastTradition)};
            for(int i=0;i<3;i++)snapshot.TraditionXp[i]=progress.Arcane.GetExperience((ArcaneTradition)i);
            for(int i=0;i<8;i++)snapshot.SchoolXp[i]=progress.GetMagicExperience((MagicSchool)i);
            for(int i=0;i<4;i++)snapshot.WeaponXp[i]=progress.GetWeaponExperience((Weapon)i);
            foreach(string id in progress.Arcane.KnownSpellIds)snapshot.Spells.Add(new SavedSpell{Id=id,Tradition=TraditionId(progress.Arcane.TraditionFor(id))});
            foreach(var item in progress.Techniques.Known)
            {
                TechniqueEntry entry=Technique(item.Key);
                snapshot.Techniques.Add(new SavedTechnique{Id=item.Key,Uses=item.Value.SuccessfulUses,
                    Remaining=entry!=null?Math.Max(0,entry.Cooldown-(now-item.Value.LastUsed)):0});
            }
            foreach(var item in progress.RecordedOrigins)snapshot.Origins.Add(new SavedOrigin{Id=item.Key,Source=item.Value.Source.ToString(),ActorId=item.Value.ActorId,WorldTime=item.Value.WorldTime});
            return snapshot;
        }
        private static DataContractJsonSerializer Serializer()
        {return new DataContractJsonSerializer(typeof(ProgressionSnapshot),new DataContractJsonSerializerSettings{MaxItemsInObjectGraph=4096});}
        public static string Encode(ProgressionSnapshot snapshot)
        {
            using(var stream=new MemoryStream())
            {Serializer().WriteObject(stream,snapshot);Require(stream.Length<=MaxLength,"Progresso excede o limite do save.");return Encoding.UTF8.GetString(stream.ToArray());}
        }
        public static SnapshotStatus Load(string json,long actorId,double now,out CharacterProgression progress,out ProgressionSnapshot snapshot,out string message)
        {
            progress=null;snapshot=null;message=null;
            if(string.IsNullOrEmpty(json))return SnapshotStatus.Missing;
            try
            {
                Require(json.Length<=MaxLength,"Progresso excede o limite do save.");
                byte[] bytes=Encoding.UTF8.GetBytes(json);
                Require(bytes.Length<=MaxLength,"Progresso excede o limite de bytes do save.");
                using(var reader=JsonReaderWriterFactory.CreateJsonReader(bytes,new XmlDictionaryReaderQuotas{MaxDepth=16,MaxStringContentLength=MaxLength,MaxArrayLength=512,MaxBytesPerRead=MaxLength,MaxNameTableCharCount=8192}))
                {
                    snapshot=(ProgressionSnapshot)Serializer().ReadObject(reader);
                    while(reader.Read())Require(reader.NodeType==XmlNodeType.Whitespace || reader.NodeType==XmlNodeType.None,"Conteúdo extra após o progresso.");
                }
                Require(snapshot!=null,"Progresso ausente.");
                if(snapshot.Version!=Version){message="Versão de progresso não suportada: "+snapshot.Version;return SnapshotStatus.Unsupported;}
                if(snapshot.ActorId!=actorId || actorId<=0){message="Progresso pertence a outro Actor.";return SnapshotStatus.ForeignActor;}
                Require(Finite(now) && now>=0 && Finite(snapshot.SavedAt) && snapshot.SavedAt>=0,"Tempo de save inválido.");
                Experience(snapshot.TraditionXp,3);Experience(snapshot.SchoolXp,8);Experience(snapshot.WeaponXp,4);
                Counter(snapshot.Mana,"mana");Counter(snapshot.Aura,"aura");Counter(snapshot.FormationWork,"formação");
                Counter(snapshot.StudyClock,"estudo");Counter(snapshot.SpellWork,"aprendizado");Counter(snapshot.PracticeClock,"prática");
                Require(snapshot.LastAction==null || snapshot.LastAction.Length<=512,"Descrição de atividade inválida.");
                Require(snapshot.PatronId==null || snapshot.PatronId.Length<=128,"Patrono inválido.");
                Require(snapshot.Spells!=null && snapshot.Spells.Count<=160 && snapshot.Techniques!=null && snapshot.Techniques.Count<=16 &&
                    snapshot.Origins!=null && snapshot.Origins.Count<=256 && snapshot.Buffs!=null && snapshot.Buffs.Count<=5,"Listas de progresso inválidas.");
                var restored=new CharacterProgression();restored.Arcane.SetPatron(snapshot.PatronId);
                if(snapshot.OriginId!=null)
                {
                    ArcaneIdentity lineage=null;
                    foreach(ArcaneIdentity identity in ArcaneIdentities.All)if(!identity.IsPatron && identity.Id==snapshot.OriginId)lineage=identity;
                    Require(lineage!=null,"Linhagem desconhecida.");restored.Arcane.SetOrigin(lineage.Origin);
                }
                for(int i=0;i<3;i++)restored.Arcane.Study((ArcaneTradition)i,snapshot.TraditionXp[i]);
                for(int i=0;i<8;i++)restored.StudyMagic((MagicSchool)i,snapshot.SchoolXp[i]);
                for(int i=0;i<4;i++)restored.TrainWeapon((Weapon)i,snapshot.WeaponXp[i]);
                ArcaneTradition tradition=ArcaneTradition.Wizard;
                if(snapshot.FormationTradition!=null)
                {
                    Require(TryTradition(snapshot.FormationTradition,out tradition),"Tradição de formação inválida.");
                    restored.Formation.Tradition=tradition;
                    Require(!snapshot.FormationReady || (tradition!=ArcaneTradition.Warlock || restored.Arcane.PatronId!=null) &&
                        (tradition!=ArcaneTradition.Sorcerer || restored.Arcane.Origin!=SorcerousOrigin.None),"Formação sem origem de poder.");
                }
                else Require(!snapshot.FormationReady,"Formação sem tradição.");
                restored.Formation.Ready=snapshot.FormationReady;restored.Formation.Work=snapshot.FormationWork;
                var ids=new HashSet<string>(StringComparer.Ordinal);
                foreach(SavedSpell record in snapshot.Spells)
                {
                    Require(record!=null,"Magia ausente.");SpellEntry spell=SpellCatalog.Find(record.Id);
                    Require(spell!=null && ids.Add(record.Id) && TryTradition(record.Tradition,out tradition),"Magia ou tradição inválida.");
                    Require(restored.Arcane.Learn(new SpellDefinition(spell.Id,spell.School,spell.Circle,spell.Mana),tradition),"Magia sem requisitos de aprendizado.");
                    Require(Finite(record.Remaining) && record.Remaining>=0 && record.Remaining<=spell.Cooldown,"Recarga de magia inválida.");
                }
                Require(snapshot.PreferredSpell==null || restored.Arcane.Knows(snapshot.PreferredSpell),"Magia preparada desconhecida.");
                ids.Clear();
                foreach(SavedTechnique record in snapshot.Techniques)
                {
                    Require(record!=null,"Técnica ausente.");TechniqueEntry entry=Technique(record.Id);
                    Require(entry!=null && entry.HasRuntime && ids.Add(record.Id) && restored.GetWeaponLevel(entry.Weapon)>=entry.RequiredLevel,"Técnica sem requisitos.");
                    Require(record.Uses>=0 && record.Uses<=20 && Finite(record.Remaining) && record.Remaining>=0 && record.Remaining<=entry.Cooldown,"Progresso de técnica inválido.");
                    restored.Techniques.Learn(entry);TechniqueKnowledge knowledge=restored.Techniques.Get(entry.Id);
                    knowledge.SuccessfulUses=record.Uses;knowledge.LastUsed=record.Remaining>0?now-(entry.Cooldown-record.Remaining):double.NegativeInfinity;
                }
                ids.Clear();
                foreach(SavedOrigin record in snapshot.Origins)
                {
                    Require(record!=null && !string.IsNullOrEmpty(record.Id) && record.Id.Length<=160 && ids.Add(record.Id) && record.ActorId==actorId && Finite(record.WorldTime) && record.WorldTime>=0,"Origem de conhecimento inválida.");
                    KnowledgeSource source;Require(Enum.TryParse(record.Source,out source) && Enum.IsDefined(typeof(KnowledgeSource),source),"Método de aprendizado inválido.");
                    restored.RecordOrigin(record.Id,new KnowledgeOrigin(source,record.WorldTime,record.ActorId));
                }
                ids.Clear();
                foreach(SavedBuff record in snapshot.Buffs)
                    Require(record!=null && IsOwnedBuff(record.Id) && ids.Add(record.Id) && Finite(record.Remaining) && record.Remaining>0 && record.Remaining<=600,"Bônus temporário inválido.");
                Require(!snapshot.LastWeapon.HasValue || snapshot.LastWeapon.Value>=0 && snapshot.LastWeapon.Value<=3,"Arma de prática inválida.");
                Require(TryTradition(snapshot.LastTradition??"wizard",out tradition),"Última tradição inválida.");
                Require(snapshot.StudyingSpell==null || SpellCatalog.Find(snapshot.StudyingSpell)!=null && !restored.Arcane.Knows(snapshot.StudyingSpell),"Magia em estudo inválida.");
                restored.Learning.LastTradition=tradition;restored.Learning.LastWeapon=snapshot.LastWeapon.HasValue?(Weapon?)snapshot.LastWeapon.Value:null;
                restored.Learning.StudyClock=snapshot.StudyClock;restored.Learning.SpellWork=snapshot.SpellWork;restored.Learning.PracticeClock=snapshot.PracticeClock;
                restored.Learning.StudyingSpellId=snapshot.StudyingSpell;restored.Learning.LastAction=snapshot.LastAction??"Progresso restaurado do save.";
                restored.Learning.ResetObservation();restored.Formation.ResetObservation();
                // Keep partial weapon practice while requiring a fresh observation after load.
                restored.Learning.LastWeapon=snapshot.LastWeapon.HasValue?(Weapon?)snapshot.LastWeapon.Value:null;
                restored.RestoreResources(snapshot.Mana,snapshot.Aura,snapshot.AuraAwakened);
                progress=restored;return SnapshotStatus.Restored;
            }
            catch(Exception error){message=error.Message;progress=null;snapshot=null;return SnapshotStatus.Invalid;}
        }
    }
}
