using System;
using System.Collections.Generic;
using UnityEngine;
using NeoModLoader.api;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Spells;
using ArcaneMartialAscension.GameIntegration;
using ArcaneMartialAscension.VisualEffects;
using ArcaneMartialAscension.UI;
using MartialWeaponType = ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension
{
    public class Main : BasicMod<Main>
    {
        public static CombatVisualController Visuals { get; private set; }
        private Actor selected;
        private int schoolIndex=(int)MagicSchool.Evocation;
        private int spellIndex=0;
        private int weaponIndex=0;
        private static readonly string[] SchoolNames={"Abjuracao","Conjuracao","Adivinhacao","Encantamento","Evocacao","Ilusao","Necromancia","Transmutacao"};
        private static readonly string[] WeaponNames={"Espada","Lanca","Arco","Machado"};
        private bool traitsReady;
        private bool panelVisible=false;
        private bool autoWeapons=true;
        private bool autoLearning=true;
        private string status="Iniciando...";

        protected override void OnModLoad()
        {
            try
            {
                ModTraits.Register();
                WeaponHitObserver.Register();
                ActorProgressionPersistence.Register();
                traitsReady=true;
                status="Magias integradas: "+SpellCatalog.All.Count+" | selecione F7";
                LogInfo("AMA v0.4.0: " + SpellCatalog.All.Count + " embedded spells, four weapon mastery paths, registered traits");
            }
            catch(Exception error)
            {
                traitsReady=false;
                status="Erro em tracos; confira Player.log";
                LogError("AMA v0.4.0 trait registration failure: "+error);
            }
        }
        private void Start()
        {
            gameObject.AddComponent<CharacterPanels>().Initialize(PrepareSpell);
            try
            {
                Visuals=gameObject.AddComponent<CombatVisualController>();
                LogInfo("AMA v0.4.0: VFX screen fallback ready; F5 test, F7 select, F8 cast, F9 weapon, F10 aura");
            }
            catch(Exception error)
            {
                status="Visualizacao SpriteRenderer falhou; interface GUI permanece disponivel";
                LogError("AMA v0.4.0 SpriteRenderer init failed: "+error);
            }
        }
        private void Update()
        {
            AutonomousLearningAgent.Update(autoLearning);
            if(Input.GetKeyDown(KeyCode.F5)) ScreenEffects.Test();
            if(Input.GetKeyDown(KeyCode.F6)) panelVisible=!panelVisible;
            if(Input.GetKeyDown(KeyCode.F7)) PickActor();
            if(Input.GetKeyDown(KeyCode.F8)) Cast();
            if(Input.GetKeyDown(KeyCode.F9)) Weapon();
            if(Input.GetKeyDown(KeyCode.F10)) Aura();
            if(Input.GetKeyDown(KeyCode.F11)) weaponIndex=(weaponIndex+1)%4;
            if(Input.GetKeyDown(KeyCode.F12)) TrainMagic();
            ActorAbilities.UpdateMagic();
            if(autoWeapons) ActorAbilities.AutoDiscoverWeapons();
            ActorAbilities.Cleanup();
        }
        private Actor NearestAtCursor(bool excludeSelected)
        {
            if(World.world==null)return null;
            // Native cursor coordinates use logical map positions. Render transforms
            // can be stale at map zoom or while an actor is inside a building.
            Vector2 cursor=World.world.getMousePos();
            if(float.IsNaN(cursor.x) || float.IsNaN(cursor.y) || float.IsInfinity(cursor.x) || float.IsInfinity(cursor.y))return null;
            Actor native=World.world.getActorNearCursor();
            if(native!=null && native.isAlive() && (!excludeSelected || native!=selected))return native;
            Actor best=null;
            float closest=8f*8f;
            int count=0;
            foreach(Actor actor in World.world.units)
            {
                if(actor==null || !actor.isAlive() || (excludeSelected && actor==selected))continue;
                float distance=(actor.current_position-cursor).sqrMagnitude;
                if(distance<closest){best=actor;closest=distance;}
                if(++count>=12000)break;
            }
            return best;
        }
        private void PickActor()
        {
            selected=NearestAtCursor(false);
            status=selected!=null?"Selecionado: "+GameCompat.DisplayName(selected):
                "Nenhum habitante perto do cursor. Aproxime zoom.";
            LogInfo("AMA: "+status);
        }
        private bool Selected()
        {
            Actor inspected=SelectedUnit.unit;
            if(inspected!=null && inspected.isAlive()) selected=inspected;
            if(selected!=null && selected.isAlive())return true;
            PickActor();
            return selected!=null && selected.isAlive();
        }
        private SpellEntry CurrentSpell()
        {
            List<SpellEntry> list=SpellCatalog.School((MagicSchool)schoolIndex);
            if(list.Count==0)return null;
            spellIndex=(spellIndex%list.Count+list.Count)%list.Count;
            return list[spellIndex];
        }
        private void School(int delta)
        {
            schoolIndex=(schoolIndex+delta+8)%8;
            spellIndex=0;
        }
        private void ChangeSpell(int delta)
        {spellIndex=(spellIndex+delta+20)%20;}
        private void PrepareSpell(Actor actor,SpellEntry spell)
        {
            if(actor==null || !actor.isAlive() || spell==null)return;
            selected=actor;
            schoolIndex=(int)spell.School;
            spellIndex=SpellCatalog.School(spell.School).IndexOf(spell);
            ActorAbilities.PrepareForCombat(actor,spell);
            status="Preparada: "+spell.Name+". Aponte para o alvo e pressione F8.";
            LogInfo("AMA: "+status);
        }
        private void Cast()
        {
            if(!Selected())return;
            SpellEntry spell=CurrentSpell();
            SpellRuntimeProfile profile=SpellRuntimeCatalog.Find(spell);
            Actor target=NearestAtCursor(profile==null || profile.TargetRule==SpellTargetRule.Hostile);
            status=ActorAbilities.Cast(selected,target,spell,true);
            if(status.StartsWith("OK"))LogInfo("AMA cast: "+status);
        }
        private void Learn()
        {
            if(!Selected())return;
            status=ActorAbilities.Learn(selected,CurrentSpell());
        }
        private void Weapon()
        {
            if(!Selected())return;
            MartialWeaponType type=(MartialWeaponType)weaponIndex;
            ActorAbilities.TriggerWeapon(selected,type);
            status="Treino: "+ActorAbilities.WeaponStatus(selected,type);
        }
        private void Aura()
        {
            if(!Selected())return;
            ActorAbilities.TriggerAura(selected);
            status="Aura de "+GameCompat.DisplayName(selected)+" manifestada";
        }
        private void TrainMagic()
        {
            if(!Selected())return;
            ActorAbilities.TrainMagic(selected);
            status="Estudo: "+ActorAbilities.Status(selected);
        }
        private void OnGUI()
        {
            // Always draw fallback effects, even when the options panel is hidden.
            ScreenEffects.Draw();
            if(Time.unscaledTime<ActorAbilities.MessageUntil && !string.IsNullOrEmpty(ActorAbilities.LastMessage))
            {
                float width=Mathf.Min(620,Screen.width-24);
                GUI.Box(new Rect((Screen.width-width)*.5f,12,width,48),ActorAbilities.LastMessage);
            }
            if(!panelVisible)return;
            int x=Screen.width-425;
            if(x<5)x=5;
            GUI.Box(new Rect(x,30,414,552),"Arcane & Martial Ascension 0.4 ALPHA");
            GUI.Label(new Rect(x+10,52,396,19),traitsReady?
                "Catalogo 160 magias | 4 armas | Tracos OK" : "Tracos falharam: Player.log");
            GUI.Label(new Rect(x+10,74,393,34),status);
            if(GUI.Button(new Rect(x+8,105,95,26),"F5 VFX"))ScreenEffects.Test();
            if(GUI.Button(new Rect(x+106,105,96,26),"F7 Unidade"))PickActor();
            if(GUI.Button(new Rect(x+205,105,95,26),"F8 Magia"))Cast();
            if(GUI.Button(new Rect(x+303,105,101,26),"F10 Aura"))Aura();
            if(GUI.Button(new Rect(x+8,135,36,28),"<"))School(-1);
            GUI.Label(new Rect(x+52,139,298,22),"Escola: "+SchoolNames[schoolIndex]);
            if(GUI.Button(new Rect(x+364,135,39,28),">"))School(1);
            SpellEntry spell=CurrentSpell();
            if(GUI.Button(new Rect(x+8,166,36,26),"<"))ChangeSpell(-1);
            if(spell!=null)
            {
                GUI.Label(new Rect(x+49,169,312,23),spell.Name+" | Circulo "+spell.Circle);
                GUI.Label(new Rect(x+50,192,352,22),"Mana: "+spell.Mana+" | "+
                    (SpellCatalog.HasRuntime(spell)?"PROTOTIPO ATIVO":"MECANICA PENDENTE"));
            }
            if(GUI.Button(new Rect(x+364,166,39,26),">"))ChangeSpell(1);
            GUI.Label(new Rect(x+10,216,395,25),"Aponte para outro personagem e aperte F8.");
            if(GUI.Button(new Rect(x+8,239,188,27),"Aprender magia selecionada"))Learn();
            GUI.Label(new Rect(x+203,245,199,24),
                selected!=null && spell!=null && ActorAbilities.Knows(selected,spell)?"APRENDIDA":"Nao aprendida");
            if(GUI.Button(new Rect(x+8,273,97,27),"F9 Treinar"))Weapon();
            if(GUI.Button(new Rect(x+108,273,36,27),"<"))weaponIndex=(weaponIndex+3)%4;
            GUI.Label(new Rect(x+154,278,195,24),"Arma: "+WeaponNames[weaponIndex]);
            if(GUI.Button(new Rect(x+366,273,37,27),">"))weaponIndex=(weaponIndex+1)%4;
            if(GUI.Button(new Rect(x+8,305,170,27),"F12 Estudar magia"))TrainMagic();
            GUI.Label(new Rect(x+183,312,221,22),"F11 troca arma | F6 oculta");
            autoWeapons=GUI.Toggle(new Rect(x+9,339,393,24),autoWeapons,
                "Detectar armas dos NPCs automaticamente (experimental)");
            autoLearning=GUI.Toggle(new Rect(x+9,367,393,24),autoLearning,
                "IA: aprender magias e treinar armas (experimental)");
            if(selected!=null && selected.isAlive())
            {
                GUI.Label(new Rect(x+10,397,393,23),ActorAbilities.Status(selected));
                GUI.Label(new Rect(x+10,423,393,24),
                    ActorAbilities.WeaponStatus(selected,(MartialWeaponType)weaponIndex));
                GUI.Label(new Rect(x+10,452,393,46),ActorAbilities.LearningStatusForDisplay(selected));
                GUI.Label(new Rect(x+10,500,393,42),ActorAbilities.DiagnosticForDisplay(selected));
                GUI.Label(new Rect(x+10,546,393,29),ActorAbilities.TechniqueStatusForDisplay(selected));
            }
        }
        private void OnDestroy()
        {
            WeaponHitObserver.Unregister();
            ActorProgressionPersistence.Unregister();
            ActorAbilities.ResetRuntimeForWorld();
            ScreenEffects.Clear();
            Visuals=null;
        }
    }
}
