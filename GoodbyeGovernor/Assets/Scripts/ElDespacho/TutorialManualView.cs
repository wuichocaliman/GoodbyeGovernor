using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.Events;using UnityEngine.UI;
namespace ElDespacho {
 /// <summary>Presentation of the existing manual only; never owns campaign or tutorial progress.</summary>
 public sealed class TutorialManualView:MonoBehaviour {
  struct Type {public Text text;public int size;public bool ink;}
  readonly List<Type> typography=new List<Type>();readonly List<Image> papers=new List<Image>();
  RectTransform grid;Button[] lessons;bool contrast;float applied=-1;
  float Scale=>Mathf.Max(DeskPreferences.Current.menuTextScale,AccessPreferences.Current.textScale);
  Text Text(Transform parent,string name,string text,int size,bool ink,TextAnchor align,Vector2 min,Vector2 max){var t=Kit.Label(parent,text,size,ink?Kit.Hex("#2B2B2B"):Kit.Hex("#F3E7CF"),align,min,max,default,default,FontStyle.Normal,true);t.name=name;t.resizeTextForBestFit=false;typography.Add(new Type{text=t,size=size,ink=ink});return t;}
  Image Paper(Transform parent,Vector2 min,Vector2 max,string name){var p=Kit.Panel(parent,Kit.Hex("#F3E7CF"),min,max,default,default,name);papers.Add(p);var o=p.gameObject.AddComponent<Outline>();o.effectColor=Kit.Hex("#C6A24F");o.effectDistance=new Vector2(1,-1);return p;}
  Button Button(Transform parent,string name,string label,UnityAction action,Vector2 min,Vector2 max,bool enabled=true){var b=Kit.MakeButton(parent,label,23,Pal.Wood,Pal.Ink,()=>{Sfx.Play(Sfx.Click);action();},min,max);DeskMenuSkin.Apply(b);b.name=name;b.interactable=enabled;var t=b.GetComponentInChildren<Text>();t.resizeTextForBestFit=false;t.color=Kit.Hex("#291B12");typography.Add(new Type{text=t,size=23,ink=true});return b;}
  public static TutorialManualView Create(Transform page){var panel=Kit.Panel(page,new Color(.10f,.075f,.055f,.94f),new Vector2(.045f,.055f),new Vector2(.955f,.92f),default,default,"TrainingDossier");var o=panel.gameObject.AddComponent<Outline>();o.effectColor=Kit.Hex("#C6A24F");o.effectDistance=new Vector2(2,-2);return panel.gameObject.AddComponent<TutorialManualView>();}
  public void Index(UnityAction<int> open,UnityAction back){
   Text(transform,"ManualTitle","TUTORIAL PRESIDENCIAL",42,false,TextAnchor.MiddleLeft,new Vector2(.04f,.86f),new Vector2(.96f,.97f));
   Text(transform,"ManualSubtitle","Aprende a gobernar Aurelia con Julián Paz, tu jefe de gabinete",22,false,TextAnchor.MiddleLeft,new Vector2(.04f,.75f),new Vector2(.96f,.86f));
   Kit.Panel(transform,Kit.Hex("#C6A24F"),new Vector2(.04f,.735f),new Vector2(.96f,.737f));
   var viewport=Kit.Rect(transform,"LessonIndexViewport",new Vector2(.035f,.15f),new Vector2(.965f,.72f));var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;viewport.gameObject.AddComponent<RectMask2D>();grid=Kit.Rect(viewport,"LessonGrid",new Vector2(0,1),new Vector2(1,1));grid.pivot=new Vector2(.5f,1);var sc=viewport.gameObject.AddComponent<AccessScrollRect>();sc.viewport=viewport;sc.content=grid;sc.horizontal=false;sc.movementType=ScrollRect.MovementType.Clamped;sc.scrollSensitivity=50;GameplayScrollTools.AddBar(sc);lessons=new Button[12];
   for(int i=0;i<12;i++){int lesson=i;lessons[i]=Button(grid,"Lesson_"+i,(i+1).ToString("00")+"  "+TutorialLessons.Titles[i],()=>open(lesson),Vector2.zero,Vector2.zero);}
   Button(transform,"VOLVER","VOLVER",back,new Vector2(.72f,.035f),new Vector2(.96f,.125f));Apply();
  }
  public void Lesson(int lesson,int note,int count,int position,string content,UnityAction previous,UnityAction back,UnityAction next,bool hasPrevious,bool hasNext){
   Text(transform,"ManualTitle",TutorialLessons.Titles[lesson].ToUpperInvariant(),34,false,TextAnchor.MiddleLeft,new Vector2(.04f,.86f),new Vector2(.96f,.97f));
   Text(transform,"ManualProgress",$"Lección {lesson+1} de 12 · Nota {note+1} de {count}",22,false,TextAnchor.MiddleLeft,new Vector2(.04f,.77f),new Vector2(.96f,.85f));
   var progress=Kit.Panel(transform,Kit.Hex("#4B2E1E"),new Vector2(.04f,.745f),new Vector2(.96f,.755f));Kit.Panel(progress.transform,Kit.Hex("#C6A24F"),Vector2.zero,new Vector2((position+1)/18f,1));
   var advisor=Paper(transform,new Vector2(.04f,.18f),new Vector2(.255f,.71f),"AdvisorFile");
   var portrait=Kit.Panel(advisor.transform,Kit.Hex("#7A1F2A"),new Vector2(.23f,.55f),new Vector2(.77f,.94f),default,default,"JulianPortraitReserved");
   Text(portrait.transform,"AdvisorMonogram","JP",42,false,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);
   Text(advisor.transform,"AdvisorName","JULIÁN PAZ\nJEFE DE GABINETE",23,true,TextAnchor.MiddleCenter,new Vector2(.05f,.12f),new Vector2(.95f,.51f));
   var paper=Paper(transform,new Vector2(.285f,.18f),new Vector2(.96f,.71f),"OfficialTrainingDocument");
   Text(paper.transform,"DocumentHeading","EXPEDIENTE DE FORMACIÓN",18,true,TextAnchor.MiddleLeft,new Vector2(.035f,.84f),new Vector2(.965f,.98f));
   Kit.Panel(paper.transform,Kit.Hex("#C6A24F"),new Vector2(.035f,.825f),new Vector2(.965f,.828f));
   var scroll=Kit.ScrollText(paper.transform,content,28,Pal.Ink,new Vector2(32,26),new Vector2(-26,-100),false,"TrainingTextScroll");
   var body=scroll.content.GetComponent<Text>();body.name="TutorialBody";body.resizeTextForBestFit=false;typography.Add(new Type{text=body,size=28,ink=true});
   Button(transform,"ATRÁS","ATRÁS",previous,new Vector2(.04f,.035f),new Vector2(.32f,.13f),hasPrevious);
   Button(transform,"VOLVER","VOLVER",back,new Vector2(.36f,.035f),new Vector2(.64f,.13f));
   Button(transform,"SIGUIENTE","SIGUIENTE",next,new Vector2(.68f,.035f),new Vector2(.96f,.13f),hasNext);Apply();
  }
  void LateUpdate(){Apply();if(grid!=null){float height=Mathf.Max(76,54*Scale+22);float width=(grid.parent as RectTransform).rect.width-24;for(int i=0;i<12;i++){var rt=(RectTransform)lessons[i].transform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.sizeDelta=new Vector2((width-20)/2,height);rt.anchoredPosition=new Vector2((i/6)*(width+20)/2,-(i%6)*(height+12));}grid.sizeDelta=new Vector2(-18,6*height+5*12);}}
  void Apply(){float scale=Scale;bool hc=DeskPreferences.Current.highContrast;if(applied==scale&&contrast==hc)return;applied=scale;contrast=hc;foreach(var style in typography){style.text.fontSize=Mathf.RoundToInt(style.size*scale);style.text.resizeTextForBestFit=false;bool button=style.text.GetComponentInParent<Button>()!=null;style.text.color=button?Kit.Hex("#291B12"):style.ink?(hc?Color.white:Kit.Hex("#2B2B2B")):Kit.Hex("#F3E7CF");}foreach(var paper in papers)paper.color=hc?Color.black:Kit.Hex("#F3E7CF");foreach(var scroll in GetComponentsInChildren<ScrollRect>())if(scroll.verticalScrollbar!=null)scroll.verticalScrollbar.targetGraphic.color=hc?Kit.Hex("#C6A24F"):Pal.InkSoft;}
 }
}
