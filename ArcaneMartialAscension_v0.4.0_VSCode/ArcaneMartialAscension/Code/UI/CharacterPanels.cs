using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.GameIntegration;
using ArcaneMartialAscension.Martial;
using ArcaneMartialAscension.Spells;
using ArcaneMartialAscension.Learning;
using MartialWeaponType = ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.UI
{
    // Native tabs in the inspected actor's window. No progression changes on inspection.
    public sealed class CharacterPanels : MonoBehaviour
    {
        private static readonly string[] Weapons={"Espada","Lança","Arco","Machado"};
        private static readonly string[] WeaponIcons={"sword","spear","bow","axe"};
        private static readonly string[] Schools={"Abjuração","Conjuração","Adivinhação","Encantamento","Evocação","Ilusão","Necromancia","Transmutação"};
        private static readonly Color Gold=new Color(1f,.77f,.32f);
        private static readonly Color Muted=new Color(.73f,.76f,.72f);
        private static readonly Color Accent=new Color(.28f,.43f,.48f,.96f);
        private readonly CharacterProgression untrained=new CharacterProgression();
        private UnitWindow unitWindow;
        private WindowMetaTab martialTab,magicTab;
        private RectTransform martialContent,magicContent;
        private Action<Actor,SpellEntry> prepareSpell;
        private Font font;
        private Actor inspected;
        private float nextRefresh,nextSearch;
        private int weaponIndex,schoolIndex=(int)MagicSchool.Evocation,circle;
        private string selectedTechniqueId;
        private bool failed;
        private Text martialActor,magicActor,martialSummary,feedback,combatStatus,martialLearning,magicLearning;
        private Text martialGrade,martialNext,martialXp,martialAttributes,martialActive;
        private Text martialClass,martialCount,emptyTechniques,techniqueDetail;
        private Text magicSchools,magicTraditions,magicCircle,magicMana,magicKnown,magicAttributes,schoolAttributes,catalogSchool,magicIdentity,identityDetails;
        private readonly Text[] weaponLabels=new Text[4];
        private readonly Text[] techniqueLabels=new Text[4];
        private readonly Text[] techniqueRanks=new Text[4],plannedTechniqueLabels=new Text[4];
        private readonly Button[] techniqueButtons=new Button[4];
        private readonly Image[] techniqueIcons=new Image[4];
        private readonly TechniqueEntry[] shownTechniques=new TechniqueEntry[4];
        private readonly Image[] weaponHighlights=new Image[4];
        private readonly Image[] schoolHighlights=new Image[8];
        private readonly Text[] schoolLabels=new Text[8];
        private readonly Image[] circleHighlights=new Image[10];
        private readonly Text[] spellLabels=new Text[2];
        private readonly Button[] learnButtons=new Button[2],prepareButtons=new Button[2];
        private readonly SpellEntry[] shownSpells=new SpellEntry[2];
        private readonly List<GameObject> owned=new List<GameObject>();

        public void Initialize(Action<Actor,SpellEntry> onPrepare)
        {prepareSpell=onPrepare;}

        private void Update()
        {
            if(failed)return;
            if(unitWindow==null)
            {
                if(Time.unscaledTime<nextSearch)return;
                nextSearch=Time.unscaledTime+1f;
                foreach(UnitWindow candidate in Resources.FindObjectsOfTypeAll<UnitWindow>())
                {
                    if(!candidate.gameObject.scene.IsValid() || candidate._button_equipment_editor==null ||
                        candidate._button_equipment_editor.container==null)continue;
                    try {Attach(candidate);}
                    catch(Exception error)
                    {
                        failed=true;
                        Cleanup();
                        Debug.LogError("[AMA UI] Falha ao criar abas de personagem: "+error);
                    }
                    break;
                }
            }
            if(unitWindow==null || !unitWindow.gameObject.activeInHierarchy)return;
            if(!martialContent.gameObject.activeInHierarchy && !magicContent.gameObject.activeInHierarchy)return;
            Actor current=SelectedUnit.unit;
            if(current!=inspected)
            {
                SelectDefaultWeapon(current);
                inspected=current;
                feedback.text="";
                nextRefresh=0;
            }
            if(Time.unscaledTime<nextRefresh)return;
            nextRefresh=Time.unscaledTime+.25f;
            Refresh();
        }

        private void Attach(UnitWindow window)
        {
            unitWindow=window;
            ScrollWindow scroll=window.GetComponentInParent<ScrollWindow>();
            if(scroll==null || scroll.transform_content==null)throw new InvalidOperationException("UnitWindow sem conteúdo rolável.");
            font=scroll.titleText.font;
            WindowMetaTab template=window._button_equipment_editor;
            martialContent=Content(scroll.transform_content,"AMA_MasteriesContent",900);
            magicContent=Content(scroll.transform_content,"AMA_SpellsContent",1462);
            martialTab=CreateTab(template,"ama_masteries","Maestrias e técnicas","sword",martialContent,0);
            magicTab=CreateTab(template,"ama_grimoire","Magias, escolas e círculos","wizard",magicContent,1);
            BuildMartial();
            BuildMagic();
            martialContent.gameObject.SetActive(false);
            magicContent.gameObject.SetActive(false);
            template.container.refillTabsWithContent();
            Debug.Log("[AMA UI] Abas Maestrias e Grimório registradas em UnitWindow; seleção nativa de Actor.");
        }

        private WindowMetaTab CreateTab(WindowMetaTab template,string id,string title,string icon,RectTransform content,int order)
        {
            GameObject clone=Instantiate(template.gameObject,template.transform.parent);
            owned.Add(clone);
            clone.name=id;
            // Start would add a duplicate Canvas. Remove the cloned drag component
            // immediately: the native container enumerates even disabled components
            // before deferred Destroy runs, calling getRect with a null _container.
            foreach(DragOrderElement drag in clone.GetComponentsInChildren<DragOrderElement>(true))
            {drag.enabled=false;DestroyImmediate(drag);}
            WindowMetaTab tab=clone.GetComponent<WindowMetaTab>();
            tab.tab_elements=new List<Transform>();
            tab.tab_action=new WindowMetaTabEvent();
            tab._worldtip_text=title;
            TipButton tip=clone.GetComponent<TipButton>();
            if(tip!=null)
            {
                tip.textOnClick=id;
                tip.textOnClickDescription=id+"_info";
                tip.text_description_2="";
            }
            tab.container=template.container;
            tab.tab_action.AddListener(chosen=>{
                chosen.container.showTab(chosen);
                nextRefresh=0;
                Refresh();
            });
            Button button=clone.GetComponent<Button>();
            button.onClick=new Button.ButtonClickedEvent();
            button.onClick.AddListener(tab.doAction);
            Sprite sprite=Icon(icon);
            foreach(Image image in clone.GetComponentsInChildren<Image>(true))
                if(image.gameObject!=clone && sprite!=null){image.sprite=sprite;image.overrideSprite=sprite;break;}
            tab.container._tabs.Add(tab);
            tab.container.addTabContent(tab,content);
            tab.toggleActive(true);
            RectTransform rect=clone.GetComponent<RectTransform>();
            if(template.transform.parent.GetComponent<LayoutGroup>()!=null)
                clone.transform.SetSiblingIndex(template.transform.GetSiblingIndex()+1+order);
            else
            {
                float bottom=0;
                foreach(Transform sibling in template.transform.parent)
                {
                    if(sibling==clone.transform || sibling.name.StartsWith("ama_"))continue;
                    RectTransform other=sibling as RectTransform;
                    if(other!=null)bottom=Mathf.Min(bottom,other.anchoredPosition.y-other.rect.height*.5f);
                }
                rect.anchoredPosition=new Vector2(((RectTransform)template.transform).anchoredPosition.x,
                    bottom-(rect.rect.height+2f)*(order+.5f));
            }
            clone.SetActive(true);
            return tab;
        }

        private RectTransform Content(Transform parent,string name,float height)
        {
            GameObject obj=new GameObject(name,typeof(RectTransform));
            owned.Add(obj);
            RectTransform rect=(RectTransform)obj.transform;
            rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,1f);
            rect.pivot=new Vector2(.5f,1f);
            rect.sizeDelta=new Vector2(220,height);
            rect.anchoredPosition=new Vector2(0,-4);
            LayoutElement layout=obj.AddComponent<LayoutElement>();
            layout.preferredHeight=layout.minHeight=height;
            layout.preferredWidth=220;
            layout.flexibleHeight=0;
            return rect;
        }

        private void BuildMartial()
        {
            Section(martialContent,0,"CLASSE MARCIAL","sword",Gold);
            martialActor=Label(martialContent,0,28,220,22,"Selecione um habitante",11,Color.white);
            martialClass=StatRow(martialContent,56,"Classe",0);
            for(int i=0;i<4;i++)
            {
                int index=i;
                Button card=MakeButton(martialContent,i*55,88,52,34,"",()=>{weaponIndex=index;selectedTechniqueId=null;Refresh();});
                weaponHighlights[i]=card.GetComponent<Image>();
                ImageFor(card.transform,18,2,16,16,Icon(WeaponIcons[i]));
                weaponLabels[i]=Label(card.transform,1,18,50,14,Weapons[i],8,Color.white);
                weaponLabels[i].alignment=TextAnchor.MiddleCenter;
            }
            martialSummary=StatRow(martialContent,128,"Arma equipada",1);
            martialGrade=StatRow(martialContent,156,"Grau",0);
            martialNext=StatRow(martialContent,184,"Próximo grau",1);
            martialXp=StatRow(martialContent,212,"XP da arma",0);
            martialCount=StatRow(martialContent,240,"Técnicas conhecidas",1);
            Section(martialContent,274,"TÉCNICAS","sword",Gold);
            emptyTechniques=Label(martialContent,6,308,208,28,"Ainda não conhece técnicas desta arma.",9,Muted);
            for(int i=0;i<4;i++)
            {
                int index=i;
                Button row=MakeButton(martialContent,0,306+i*26,220,24,"",()=>{
                    if(shownTechniques[index]!=null)selectedTechniqueId=shownTechniques[index].Id;
                    Refresh();
                });
                techniqueButtons[i]=row;
                techniqueIcons[i]=ImageFor(row.transform,3,4,16,16,null);
                techniqueLabels[i]=Label(row.transform,24,3,172,19,"",9,Color.white);
                techniqueRanks[i]=Label(row.transform,198,3,18,19,"",9,Gold);
                techniqueRanks[i].alignment=TextAnchor.MiddleRight;
            }
            techniqueDetail=Label(martialContent,6,418,208,78,"",9,Muted);
            martialActive=Label(martialContent,6,504,208,28,"",9,Gold);
            martialAttributes=Label(martialContent,6,538,208,76,"",9,Color.white);
            martialLearning=Label(martialContent,0,620,220,42,"",9,Muted);
            Section(martialContent,672,"PRÓXIMOS TREINOS","aura",Gold);
            for(int i=0;i<4;i++)plannedTechniqueLabels[i]=Label(martialContent,6,706+i*26,208,24,"",8,Muted);
            Label(martialContent,0,820,220,30,"A técnica tem treino próprio (I–III). Grau da arma e aura são progressões separadas.",8,Muted);
        }
        private void BuildMagic()
        {
            Color blue=new Color(.63f,.76f,1f);
            Section(magicContent,0,"FORMAÇÃO ARCANA","wizard",blue);
            magicActor=Label(magicContent,0,28,220,22,"Selecione um habitante",11,Color.white);
            magicSchools=StatRow(magicContent,58,"Escolas",0);
            magicCircle=StatRow(magicContent,86,"Círculo mágico",1);
            magicTraditions=StatRow(magicContent,114,"Tradição",0);
            magicMana=StatRow(magicContent,142,"Mana",1);
            magicKnown=StatRow(magicContent,170,"Magias aprendidas",0);
            magicIdentity=StatRow(magicContent,198,"Pacto / linhagem",1);
            magicAttributes=Label(magicContent,6,234,208,58,"",9,Muted);
            identityDetails=Label(magicContent,6,304,208,72,"",8,Gold);
            Section(magicContent,388,"ESCOLAS E CÍRCULOS","evocation",blue);
            Label(magicContent,0,418,220,28,"Cada escola mostra o maior círculo aprendido e o total de magias conhecidas.",8,Muted);
            for(int i=0;i<8;i++)
            {
                int index=i;
                Button school=MakeButton(magicContent,0,456+i*30,220,28,"",()=>{schoolIndex=index;Refresh();});
                schoolHighlights[i]=school.GetComponent<Image>();
                ImageFor(school.transform,4,5,18,18,Icon(((MagicSchool)i).ToString().ToLowerInvariant()));
                schoolLabels[i]=Label(school.transform,28,3,188,23,"",8,Color.white);
            }
            schoolAttributes=Label(magicContent,6,706,208,64,"",9,Muted);
            Section(magicContent,782,"CATÁLOGO DE MAGIAS","wizard",blue);
            catalogSchool=Label(magicContent,0,812,220,22,"",10,Color.white);
            Label(magicContent,0,838,220,18,"Filtrar círculo do catálogo",9,Gold);
            for(int i=0;i<10;i++)
            {
                int value=i;
                Button filter=MakeButton(magicContent,i*22,862,20,22,i.ToString(),()=>{circle=value;Refresh();});
                circleHighlights[i]=filter.GetComponent<Image>();
            }
            for(int i=0;i<2;i++)
            {
                int index=i;
                float y=900+i*191;
                Image panel=ImageFor(magicContent,0,y,220,185,null);
                panel.color=new Color(.14f,.18f,.17f,.9f);
                spellLabels[i]=Label(panel.transform,7,5,206,134,"",9,Color.white);
                learnButtons[i]=MakeButton(panel.transform,7,147,87,26,"Aprender",()=>Learn(index));
                prepareButtons[i]=MakeButton(panel.transform,101,147,112,26,"Preparar · F8",()=>Prepare(index));
            }
            feedback=Label(magicContent,0,1294,220,36,"",9,Gold);
            combatStatus=Label(magicContent,0,1334,220,40,"",9,Muted);
            magicLearning=Label(magicContent,0,1380,220,44,"",9,Muted);
            Label(magicContent,0,1434,220,26,"Magias avançadas e efeitos exclusivos ainda em desenvolvimento.",8,Muted);
        }

        private void Refresh()
        {
            if(martialActor==null || magicActor==null)return;
            if(inspected!=SelectedUnit.unit)SelectDefaultWeapon(SelectedUnit.unit);
            inspected=SelectedUnit.unit;
            bool alive=inspected!=null && inspected.isAlive();
            CharacterProgression progress=alive?ActorAbilities.ProgressForDisplay(inspected):null;
            progress=progress??untrained;
            string name=alive?GameCompat.DisplayName(inspected):"Nenhum habitante selecionado";
            martialActor.text=magicActor.text=name;
            MartialWeaponType equipped=MartialWeaponType.Sword;
            bool armed=alive && GameWeaponDetector.TryDetect(inspected,out equipped);
            martialSummary.text=armed?Weapons[(int)equipped]:"Nenhuma das 4 armas";
            martialLearning.text=magicLearning.text=ActorAbilities.LearningStatusForDisplay(alive?inspected:null);
            for(int i=0;i<4;i++)
            {
                weaponHighlights[i].color=i==weaponIndex?Accent:new Color(.19f,.23f,.21f,.96f);
            }
            TechniqueEntry[] techniques=TechniqueCatalog.ForWeapon((MartialWeaponType)weaponIndex);
            int mastery=progress.GetWeaponLevel((MartialWeaponType)weaponIndex);
            MartialWeaponType selectedWeapon=(MartialWeaponType)weaponIndex;
            string[] classes={"Espadachim","Lanceiro","Arqueiro","Machadeiro"};
            martialClass.text=classes[weaponIndex];
            int weaponXp=progress.GetWeaponExperience(selectedWeapon);
            martialGrade.text=progress.GetWeaponRank(selectedWeapon)+" · "+mastery+"/7";
            martialNext.text=mastery<7?CharacterProgression.WeaponRankName(mastery+1):"Grau máximo";
            martialXp.text=weaponXp+(mastery<7?" / "+CharacterProgression.WeaponXpThresholds[mastery+1]:" · MÁXIMO");
            bool weaponActive=armed && equipped==selectedWeapon && inspected.hasTrait(ModTraits.WeaponTrait(selectedWeapon)) &&
                (mastery==0 || inspected.hasTrait(ModTraits.RankId(selectedWeapon,mastery)));
            martialActive.text=weaponActive?"Bônus ativos: "+Weapons[weaponIndex]:armed && equipped==selectedWeapon?"Bônus aguardando atualização":"Bônus inativos: equipe "+Weapons[weaponIndex].ToLowerInvariant();
            TraitAttributes discipline=TraitAttributeCatalog.Weapon(selectedWeapon);
            TraitAttributes rankAttributes=TraitAttributeCatalog.Rank(selectedWeapon,mastery);
            martialAttributes.text="<b>Disciplina</b> · "+discipline.Describe()+"\n<b>Grau "+mastery+"</b> · "+rankAttributes.Describe();
            RefreshTechniques(progress,techniques,alive);
            RefreshArcane(progress,alive);
            combatStatus.text=ActorAbilities.CombatStatusForDisplay(alive?inspected:null);
            for(int i=0;i<10;i++)circleHighlights[i].color=i==circle?Accent:new Color(.19f,.23f,.21f,.96f);
            int slot=0;
            foreach(SpellEntry spell in SpellCatalog.School((MagicSchool)schoolIndex))
            {
                if(spell.Circle!=circle || slot>=2)continue;
                shownSpells[slot]=spell;
                bool known=alive && ActorAbilities.Knows(inspected,spell);
                bool runtime=SpellCatalog.HasRuntime(spell);
                KnowledgeOrigin origin=ActorAbilities.OriginForDisplay(alive?inspected:null,spell.Id);
                spellLabels[slot].text="<b>"+spell.Name+"</b>\nCírculo "+spell.Circle+" · "+(known?"Aprendida":"Não aprendida")+
                    "\nMana "+spell.Mana+" · Recarga "+spell.Cooldown+" s\n<color="+(runtime?"#EDD28C>":"#B8BCAE>")+SpellRuntimeCatalog.StateName(spell)+"</color>\n"+spell.Description;
                SpellRuntimeProfile profile=SpellRuntimeCatalog.Find(spell);
                if(profile!=null && profile.Note.Length>0)spellLabels[slot].text+="\n"+profile.Note;
                if(origin!=null)spellLabels[slot].text+="\nOrigem: "+origin.Description;
                learnButtons[slot].interactable=ActorAbilities.CanLearnForDisplay(alive?inspected:null,spell);
                prepareButtons[slot].interactable=alive && known && runtime && progress.Arcane.GetCircle(progress.Arcane.TraditionFor(spell.Id))>=spell.Circle;
                slot++;
            }
        }

        private void SelectDefaultWeapon(Actor actor)
        {
            selectedTechniqueId=null;
            MartialWeaponType weapon;
            if(actor!=null && GameWeaponDetector.TryDetect(actor,out weapon)){weaponIndex=(int)weapon;return;}
            CharacterProgression progress=ActorAbilities.ProgressForDisplay(actor);
            weaponIndex=0;
            if(progress!=null)for(int i=1;i<4;i++)
                if(progress.GetWeaponExperience((MartialWeaponType)i)>progress.GetWeaponExperience((MartialWeaponType)weaponIndex))weaponIndex=i;
        }

        private void RefreshTechniques(CharacterProgression progress,TechniqueEntry[] techniques,bool alive)
        {
            int learned=0,planned=0;
            TechniqueEntry selected=null;
            foreach(TechniqueEntry entry in techniques)
            {
                TechniqueKnowledge knowledge=progress.Techniques.Get(entry.Id);
                if(knowledge!=null)
                {
                    if(selectedTechniqueId==null)selectedTechniqueId=entry.Id;
                    shownTechniques[learned]=entry;
                    techniqueButtons[learned].gameObject.SetActive(true);
                    techniqueLabels[learned].text=entry.Name;
                    techniqueRanks[learned].text=knowledge.Rank;
                    techniqueIcons[learned].sprite=Icon(WeaponIcons[weaponIndex]);
                    techniqueIcons[learned].preserveAspect=true;
                    bool chosen=entry.Id==selectedTechniqueId;
                    techniqueButtons[learned].GetComponent<Image>().color=chosen?Accent:new Color(.17f,.21f,.18f,.96f);
                    if(chosen || selected==null)selected=entry;
                    learned++;
                }
                else
                {
                    plannedTechniqueLabels[planned].text=entry.Name+" · arma nv. "+entry.RequiredLevel+(entry.HasRuntime?" · por treino":" · planejada");
                    plannedTechniqueLabels[planned++].gameObject.SetActive(true);
                }
            }
            for(int i=learned;i<4;i++){techniqueButtons[i].gameObject.SetActive(false);shownTechniques[i]=null;}
            for(int i=planned;i<4;i++)plannedTechniqueLabels[i].gameObject.SetActive(false);
            emptyTechniques.gameObject.SetActive(learned==0);
            martialCount.text=learned.ToString();
            martialLearning.text=ActorAbilities.TechniqueStatusForDisplay(alive?inspected:null);
            if(selected==null){techniqueDetail.text="Treine esta arma até o nível 1 para aprender a primeira técnica. Consultar a ficha não concede conhecimento.";return;}
            TechniqueKnowledge data=progress.Techniques.Get(selected.Id);
            KnowledgeOrigin origin=progress.GetOrigin(selected.Id);
            techniqueDetail.text="<b>"+selected.Name+" "+data.Rank+"</b>\n"+selected.Description+
                "\nPotência +"+TechniqueRules.Damage(progress,selected)+" · Recarga "+selected.Cooldown+" s"+
                "\nAcertos da técnica: "+data.SuccessfulUses+" / "+(data.Level<2?5:20)+(origin!=null?"\nOrigem: "+origin.Description:"");
        }

        private void RefreshArcane(CharacterProgression progress,bool alive)
        {
            ArcaneOverview overview=new ArcaneOverview(progress);
            string traditions="";
            TraitAttributes active=new TraitAttributes();
            string[] ids={ModTraits.Wizard,ModTraits.Warlock,ModTraits.Sorcerer};
            string[] names={"Mago","Bruxo","Feiticeiro"};
            for(int i=0;i<3;i++)
            {
                if(!alive || !inspected.hasTrait(ids[i]))continue;
                traditions+=(traditions.Length>0?" / ":"")+names[i];
                active=active.Plus(TraitAttributeCatalog.Tradition((ArcaneTradition)i));
            }
            magicTraditions.text=traditions.Length>0?traditions:"Sem tradição";
            magicCircle.text=traditions.Length>0 || overview.HasTraining?progress.Arcane.HighestCircle+" / 9":"Sem formação";
            magicMana.text=alive?(int)progress.Mana+" / "+(int)progress.MaxMana:"—";
            magicKnown.text=overview.KnownSpells.ToString();
            magicIdentity.text=ActorAbilities.IdentityForDisplay(alive?inspected:null);
            identityDetails.text=ActorAbilities.IdentityDetailsForDisplay(alive?inspected:null);
            magicAttributes.text="<b>Bônus das tradições ativas</b>\n"+active.Describe()+"\n"+ActorAbilities.FormationForDisplay(alive?inspected:null);
            int knownSchools=0;
            string firstSchool="";
            for(int i=0;i<8;i++)
            {
                SchoolOverview school=overview.Schools[i];
                if(school.KnownSpells>0){if(knownSchools==0)firstSchool=Schools[i];knownSchools++;}
                bool affinity=alive && inspected.hasTrait(ModTraits.SchoolTrait((MagicSchool)i));
                string state=school.KnownSpells>0?"C"+school.HighestLearnedCircle+" · "+school.KnownSpells+" magia(s)":affinity?"Afinidade · 0 magias":"Não estudada";
                schoolLabels[i].text=Schools[i]+"\n"+state;
                schoolLabels[i].color=school.KnownSpells>0?Color.white:Muted;
                schoolHighlights[i].color=i==schoolIndex?Accent:new Color(.19f,.23f,.21f,.96f);
            }
            magicSchools.text=knownSchools>0?firstSchool+(knownSchools>1?" +"+(knownSchools-1):""):"Nenhuma aprendida";
            MagicSchool selectedSchool=(MagicSchool)schoolIndex;
            bool schoolActive=alive && inspected.hasTrait(ModTraits.SchoolTrait(selectedSchool));
            schoolAttributes.text="<b>"+Schools[schoolIndex]+" · "+(schoolActive?"bônus ativos":"bônus do traço")+"</b>\n"+
                TraitAttributeCatalog.School(selectedSchool).Describe()+(!schoolActive?"\nRequer o traço desta escola.":"");
            catalogSchool.text=Schools[schoolIndex]+" · círculo "+circle;
        }

        private void Learn(int index)
        {
            Actor actor=SelectedUnit.unit;
            if(actor==null || !actor.isAlive() || shownSpells[index]==null)return;
            feedback.text=ActorAbilities.Learn(actor,shownSpells[index]);
            Refresh();
        }

        private void Prepare(int index)
        {
            Actor actor=SelectedUnit.unit;
            SpellEntry spell=shownSpells[index];
            if(actor==null || !actor.isAlive() || spell==null || !ActorAbilities.Knows(actor,spell) || !SpellCatalog.HasRuntime(spell))return;
            if(prepareSpell!=null)prepareSpell(actor,spell);
            feedback.text=spell.Name+" preparada. Feche a ficha, aponte para o alvo e use F8.";
        }

        private Sprite Icon(string name)
        {return SpriteTextureLoader.getSprite("ui/icons/ama/"+name);}

        private void Section(Transform parent,float y,string title,string icon,Color tint)
        {
            Image band=ImageFor(parent,0,y,220,25,null);
            band.color=new Color(.12f,.15f,.13f,.95f);
            ImageFor(band.transform,4,3,19,19,Icon(icon));
            Text heading=Label(band.transform,27,3,187,20,title,10,tint);
            heading.alignment=TextAnchor.MiddleCenter;
        }

        private Text StatRow(Transform parent,float y,string title,int stripe)
        {
            Image band=ImageFor(parent,0,y,220,26,null);
            band.color=stripe==0?new Color(.16f,.19f,.16f,.9f):new Color(.20f,.23f,.19f,.9f);
            Text caption=Label(band.transform,5,3,94,21,title,9,Muted);
            caption.alignment=TextAnchor.MiddleLeft;
            Text value=Label(band.transform,101,3,114,21,"",9,Color.white);
            value.alignment=TextAnchor.MiddleRight;
            return value;
        }

        private RectTransform Rect(Transform parent,string name,float x,float y,float width,float height)
        {
            GameObject obj=new GameObject(name,typeof(RectTransform));
            RectTransform rect=(RectTransform)obj.transform;
            rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=new Vector2(0,1);
            rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(x,-y);
            rect.sizeDelta=new Vector2(width,height);
            return rect;
        }

        private Text Label(Transform parent,float x,float y,float width,float height,string value,int size,Color tint)
        {
            Text text=Rect(parent,"AMA_Label",x,y,width,height).gameObject.AddComponent<Text>();
            text.font=font;text.fontSize=size;text.text=value;text.color=tint;
            text.supportRichText=true;text.raycastTarget=false;
            text.alignment=TextAnchor.UpperLeft;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;
            text.verticalOverflow=VerticalWrapMode.Truncate;
            return text;
        }

        private Image ImageFor(Transform parent,float x,float y,float width,float height,Sprite sprite)
        {
            Image image=Rect(parent,"AMA_Image",x,y,width,height).gameObject.AddComponent<Image>();
            image.sprite=sprite;image.raycastTarget=false;
            image.preserveAspect=sprite!=null;
            return image;
        }

        private Button MakeButton(Transform parent,float x,float y,float width,float height,string title,UnityEngine.Events.UnityAction action)
        {
            RectTransform rect=Rect(parent,"AMA_Button",x,y,width,height);
            Image background=rect.gameObject.AddComponent<Image>();
            background.color=new Color(.19f,.23f,.21f,.96f);
            Button button=rect.gameObject.AddComponent<Button>();
            button.targetGraphic=background;
            button.onClick.AddListener(action);
            Text label=Label(rect,2,2,width-4,height-4,title,9,Color.white);
            label.alignment=TextAnchor.MiddleCenter;
            return button;
        }

        private void Cleanup()
        {
            foreach(WindowMetaTab tab in new[]{martialTab,magicTab})
                if(tab!=null && tab.container!=null)tab.container.removeTab(tab);
            foreach(GameObject obj in owned)if(obj!=null)Destroy(obj);
            owned.Clear();
            unitWindow=null;
        }

        private void OnDestroy(){Cleanup();}
    }
}
