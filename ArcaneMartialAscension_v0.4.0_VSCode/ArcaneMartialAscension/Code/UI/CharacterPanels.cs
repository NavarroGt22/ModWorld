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
        private bool failed;
        private Text martialActor,magicActor,martialSummary,magicSummary,feedback,combatStatus,martialLearning,magicLearning;
        private readonly Text[] weaponLabels=new Text[4];
        private readonly Image[] weaponBars=new Image[4];
        private readonly Text[] techniqueLabels=new Text[4];
        private readonly Image[] weaponHighlights=new Image[4];
        private readonly Image[] schoolHighlights=new Image[8];
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
            martialContent=Content(scroll.transform_content,"AMA_MasteriesContent",630);
            magicContent=Content(scroll.transform_content,"AMA_SpellsContent",820);
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
            // The live template already has the Canvas created by DragOrderElement.Start.
            // Re-running that initializer on a clone tries to add a second Canvas.
            foreach(DragOrderElement drag in clone.GetComponentsInChildren<DragOrderElement>(true))
            {drag.enabled=false;Destroy(drag);}
            WindowMetaTab tab=clone.GetComponent<WindowMetaTab>();
            tab.tab_elements=new List<Transform>();
            tab.tab_action=new WindowMetaTabEvent();
            tab._worldtip_text=title;
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
            Label(martialContent,0,0,220,22,"MAESTRIAS",13,Gold);
            martialActor=Label(martialContent,0,24,220,22,"Selecione um habitante",11,Color.white);
            martialSummary=Label(martialContent,0,48,220,28,"",9,Muted);
            for(int i=0;i<4;i++)
            {
                int index=i;
                float y=80+i*50;
                Button card=MakeButton(martialContent,0,y,220,46,"",()=>{weaponIndex=index;Refresh();});
                weaponHighlights[i]=card.GetComponent<Image>();
                ImageFor(card.transform,6,7,26,26,Icon(WeaponIcons[i]));
                weaponLabels[i]=Label(card.transform,38,3,177,24,"",9,Color.white);
                ImageFor(card.transform,38,30,172,6,null).color=new Color(.1f,.13f,.12f);
                weaponBars[i]=ImageFor(card.transform,38,30,172,6,null);
                weaponBars[i].color=Gold;
            }
            Label(martialContent,0,288,220,23,"TÉCNICAS DA ARMA",11,Gold);
            Label(martialContent,0,312,220,30,"Prévia de técnicas planejadas; ainda sem efeito em combate.",9,Muted);
            for(int i=0;i<4;i++)
                techniqueLabels[i]=Label(martialContent,0,350+i*47,220,44,"",9,Color.white);
            martialLearning=Label(martialContent,0,544,220,50,"",9,Muted);
            Label(martialContent,0,602,220,23,"Níveis 0–7 · progresso individual por arma",8,Muted);
        }

        private void BuildMagic()
        {
            Label(magicContent,0,0,220,22,"GRIMÓRIO",13,new Color(.63f,.76f,1f));
            magicActor=Label(magicContent,0,24,220,22,"Selecione um habitante",11,Color.white);
            magicSummary=Label(magicContent,0,48,220,32,"",9,Muted);
            for(int i=0;i<8;i++)
            {
                int index=i;
                Button school=MakeButton(magicContent,(i%4)*55,86+(i/4)*47,52,43,"",()=>{schoolIndex=index;Refresh();});
                schoolHighlights[i]=school.GetComponent<Image>();
                ImageFor(school.transform,17,3,18,18,Icon(((MagicSchool)i).ToString().ToLowerInvariant()));
                Label(school.transform,1,24,50,17,Schools[i],7,Color.white).alignment=TextAnchor.MiddleCenter;
            }
            Label(magicContent,0,184,220,18,"CÍRCULO",9,Gold);
            for(int i=0;i<10;i++)
            {
                int value=i;
                Button filter=MakeButton(magicContent,i*22,206,20,22,i.ToString(),()=>{circle=value;Refresh();});
                circleHighlights[i]=filter.GetComponent<Image>();
            }
            for(int i=0;i<2;i++)
            {
                int index=i;
                float y=242+i*191;
                Image panel=ImageFor(magicContent,0,y,220,185,null);
                panel.color=new Color(.14f,.18f,.17f,.9f);
                spellLabels[i]=Label(panel.transform,7,5,206,134,"",9,Color.white);
                learnButtons[i]=MakeButton(panel.transform,7,147,87,26,"Aprender",()=>Learn(index));
                prepareButtons[i]=MakeButton(panel.transform,101,147,112,26,"Preparar · F8",()=>Prepare(index));
            }
            feedback=Label(magicContent,0,636,220,36,"",9,Gold);
            combatStatus=Label(magicContent,0,676,220,40,"",9,Muted);
            magicLearning=Label(magicContent,0,722,220,50,"",9,Muted);
            Label(magicContent,0,784,220,26,"Magias avançadas e efeitos exclusivos ainda em desenvolvimento.",8,Muted);
        }

        private void Refresh()
        {
            if(martialActor==null || magicActor==null)return;
            inspected=SelectedUnit.unit;
            bool alive=inspected!=null && inspected.isAlive();
            CharacterProgression progress=alive?ActorAbilities.ProgressForDisplay(inspected):null;
            progress=progress??untrained;
            string name=alive?GameCompat.DisplayName(inspected):"Nenhum habitante selecionado";
            martialActor.text=magicActor.text=name;
            martialSummary.text=alive?"Escolha uma arma para consultar nível, XP e técnicas.":"Abra a ficha de um habitante vivo.";
            martialLearning.text=magicLearning.text=ActorAbilities.LearningStatusForDisplay(alive?inspected:null);
            for(int i=0;i<4;i++)
            {
                MartialWeaponType weapon=(MartialWeaponType)i;
                int level=progress.GetWeaponLevel(weapon),xp=progress.GetWeaponExperience(weapon);
                int floor=CharacterProgression.WeaponXpThresholds[level];
                int ceiling=level<7?CharacterProgression.WeaponXpThresholds[level+1]:floor;
                weaponLabels[i].text=Weapons[i]+" · Nível "+level+"/7\n"+progress.GetWeaponRank(weapon)+" · XP "+xp+(level<7?" / "+ceiling:" · MÁXIMO");
                weaponBars[i].rectTransform.sizeDelta=new Vector2(172*(level==7?1f:Mathf.Clamp01((float)(xp-floor)/(ceiling-floor))),6);
                weaponHighlights[i].color=i==weaponIndex?Accent:new Color(.19f,.23f,.21f,.96f);
            }
            TechniqueEntry[] techniques=TechniqueCatalog.ForWeapon((MartialWeaponType)weaponIndex);
            int mastery=progress.GetWeaponLevel((MartialWeaponType)weaponIndex);
            for(int i=0;i<4;i++)
            {
                TechniqueEntry entry=techniques[i];
                techniqueLabels[i].text="<b>"+entry.Name+"</b> · nível "+entry.RequiredLevel+
                    (mastery<entry.RequiredLevel?" · bloqueada":" · requisito atingido")+"\n<color=#B8BCAE>Planejada</color> · "+entry.Description;
            }
            magicSummary.text="Mana "+(int)progress.Mana+" / "+(int)progress.MaxMana+" · Círculo máximo "+progress.Arcane.HighestCircle+"\n"+Schools[schoolIndex]+" · círculo "+circle;
            combatStatus.text=ActorAbilities.CombatStatusForDisplay(alive?inspected:null);
            for(int i=0;i<8;i++)schoolHighlights[i].color=i==schoolIndex?Accent:new Color(.19f,.23f,.21f,.96f);
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
                    "\nMana "+spell.Mana+" · Recarga "+spell.Cooldown+" s\n<color="+(runtime?"#EDD28C>Protótipo":"#B8BCAE>Planejada")+"</color>\n"+spell.Description;
                if(origin!=null)spellLabels[slot].text+="\nOrigem: "+origin.Description;
                learnButtons[slot].interactable=alive && !known && progress.Arcane.HighestCircle>=spell.Circle;
                prepareButtons[slot].interactable=alive && known && runtime && progress.Arcane.HighestCircle>=spell.Circle;
                slot++;
            }
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
