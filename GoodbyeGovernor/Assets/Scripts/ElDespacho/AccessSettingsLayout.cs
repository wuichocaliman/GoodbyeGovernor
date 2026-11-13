using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 public sealed partial class AccessController {
  void RenderSettings(List<Entry> entries){
   string context="settings|"+page;float previous=scroll!=null&&layoutContext==context?scroll.verticalNormalizedPosition:1;layoutContext=context;
   foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}rows.Clear();controls.Clear();
   bool dark=DeskPreferences.Current.highContrast;
   Color ink=dark?Color.white:Kit.Hex("#2B2B2B"),paper=dark?Color.black:Kit.Hex("#F3E7CF"),gold=Kit.Hex("#C6A24F");
   float scale=P.textScale*P.interfaceScale;
   var bg=Kit.Panel(root,dark?Color.black:Kit.Hex("#4B2E1E"),Vector2.zero,Vector2.one);bg.raycastTarget=true;
   Kit.Panel(bg.transform,gold,new Vector2(.025f,.925f),new Vector2(.975f,.928f));
   Kit.Label(bg.transform,"ACCESIBILIDAD"+(page=="Inicio"?"":" / "+CategoryTitle()),Mathf.RoundToInt(24*P.textScale),dark?Color.white:Kit.Hex("#F3E7CF"),TextAnchor.MiddleLeft,new Vector2(.035f,.935f),new Vector2(.96f,.995f));
   notice=Kit.Label(bg.transform,applied+"  TAB: elegir · INTRO: activar · ←/→: volumen · AvPág: desplazar · ESC: volver",Mathf.RoundToInt(14*P.textScale),Color.white,TextAnchor.MiddleLeft,new Vector2(.035f,.005f),new Vector2(.965f,.082f));
   var viewport=Kit.Panel(bg.transform,dark?Color.black:Kit.Hex("#D9C8A3"),new Vector2(.035f,.09f),new Vector2(.965f,.915f));viewport.raycastTarget=true;viewport.gameObject.AddComponent<RectMask2D>();
   content=Kit.Rect(viewport.transform,"Flow",new Vector2(0,1),new Vector2(1,1));content.pivot=new Vector2(.5f,1);
   scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport.rectTransform;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=90;
   var rail=Kit.Panel(bg.transform,dark?Kit.Hex("#333333"):Kit.Hex("#7A4E2E"),new Vector2(.969f,.09f),new Vector2(.985f,.915f));rail.raycastTarget=true;
   var thumb=Kit.Panel(rail.transform,gold,Vector2.zero,Vector2.one);thumb.raycastTarget=true;
   var bar=rail.gameObject.AddComponent<Scrollbar>();bar.handleRect=thumb.rectTransform;bar.targetGraphic=thumb;bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;
   Canvas.ForceUpdateCanvases();float available=viewport.rectTransform.rect.width-48;float y=24;
   // Cards become a single column at enlarged scales; text is never shrunk to fit.
   for(int i=0;i<entries.Count;i++){
    var e=entries[i];bool pair=e.card&&scale<=1.25f&&i+1<entries.Count&&entries[i+1].card;
    if(pair){float w=(available-20)/2;float a=BuildSetting(e,24,y,w),b=BuildSetting(entries[++i],44+w,y,w);y+=Mathf.Max(a,b)+20;}
    else y+=BuildSetting(e,24,y,available)+16;
   }
   content.sizeDelta=new Vector2(0,y+20);scroll.verticalNormalizedPosition=previous;if(focused>=controls.Count)focused=-1;

   float BuildSetting(Entry e,float x,float top,float width){
    var rt=Kit.Rect(content,"Setting: "+e.text,new Vector2(0,1),new Vector2(0,1));rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-top);rt.sizeDelta=new Vector2(width,100);
    var border=rt.gameObject.AddComponent<Image>();border.color=dark?Kit.Hex("#777777"):gold;border.raycastTarget=e.action!=null;
    var inner=Kit.Panel(rt,paper,Vector2.zero,Vector2.one,new Vector2(2,2),new Vector2(-2,-2));
    float pad=24,iconSize=32*P.interfaceScale;
    var iconRect=Kit.Rect(rt,"Icon",new Vector2(0,1),new Vector2(0,1));iconRect.pivot=new Vector2(0,1);iconRect.anchoredPosition=new Vector2(pad,-pad);iconRect.sizeDelta=Vector2.one*iconSize;
    var icon=iconRect.gameObject.AddComponent<AccessSymbol>();icon.symbol=e.icon??"D";icon.color=ink;icon.raycastTarget=false;
    var title=TextAt(e.text,pad+iconSize+16,pad,width-pad*2-iconSize-16,Mathf.RoundToInt((e.card?26:24)*scale),dark?Color.white:Kit.Hex("#7A1F2A"),FontStyle.Bold);
    float current=pad+Mathf.Max(iconSize,title.rectTransform.sizeDelta.y)+14;
    if(!string.IsNullOrEmpty(e.description)){var desc=TextAt(e.description,pad,current,width-2*pad,Mathf.RoundToInt(20*scale),ink);current+=desc.rectTransform.sizeDelta.y+18;}
    if(e.sliderChange!=null){
     var value=TextAt(e.enabled?Mathf.RoundToInt(e.sliderValue*100)+" %":"No disponible",pad,current,width-pad*2,Mathf.RoundToInt(20*scale),ink);current+=value.rectTransform.sizeDelta.y+12;
     var area=Kit.Rect(rt,"Volume",new Vector2(0,1),new Vector2(0,1));area.pivot=new Vector2(0,1);area.anchoredPosition=new Vector2(pad,-current);area.sizeDelta=new Vector2(width-2*pad,48*P.interfaceScale);
     var hit=area.gameObject.AddComponent<Image>();hit.color=new Color(0,0,0,0);hit.raycastTarget=true;
     var track=Kit.Panel(area,dark?Color.gray:Kit.Hex("#7A4E2E"),new Vector2(0,.4f),new Vector2(1,.6f));
     var fillArea=Kit.Rect(area,"Fill area",new Vector2(0,.4f),new Vector2(1,.6f));
     var fill=Kit.Panel(fillArea,dark?Color.white:Kit.Hex("#2F4B3A"),Vector2.zero,Vector2.one);
     var handleArea=Kit.Rect(area,"Handle area",Vector2.zero,Vector2.one,new Vector2(16,0),new Vector2(-16,0));
     var handle=Kit.Panel(handleArea,gold,Vector2.zero,Vector2.one);handle.rectTransform.sizeDelta=new Vector2(32,0);handle.raycastTarget=true;
     var slider=area.gameObject.AddComponent<Slider>();slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.direction=Slider.Direction.LeftToRight;slider.minValue=0;slider.maxValue=1;slider.SetValueWithoutNotify(e.sliderValue);slider.interactable=e.enabled;
     slider.onValueChanged.AddListener(v=>{value.text=Mathf.RoundToInt(v*100)+" %";e.sliderChange(v);});
     // Persist on release to keep the same Slider alive throughout a pointer drag.
     var drag=area.gameObject.AddComponent<AccessSliderDrag>();drag.owner=this;
     current+=area.sizeDelta.y+pad;
     if(e.enabled){rows.Add(rt);controls.Add(new Entry(e.text+". "+e.description,()=>{},rt));}
    }else if(e.action!=null){
     var value=TextAt(!e.enabled?"No disponible":string.IsNullOrEmpty(e.value)?"Abrir":e.value,pad,current,width-pad*2,Mathf.RoundToInt(20*scale),ink,FontStyle.Bold);current+=value.rectTransform.sizeDelta.y+pad;
     var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=inner;b.interactable=e.enabled;
     var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=dark?new Color(.65f,.65f,.65f):new Color(1,.92f,.72f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color(.75f,.72f,.65f);colors.disabledColor=new Color(.65f,.65f,.65f);b.colors=colors;
     b.onClick.AddListener(()=>{e.action();Refresh();});
     if(e.enabled){rows.Add(rt);controls.Add(new Entry(e.text+". "+e.description+". "+e.value,e.action,rt));}
    }else current+=pad;
    rt.sizeDelta=new Vector2(width,Mathf.Max(current,92*P.interfaceScale));return rt.sizeDelta.y;
    Text TextAt(string text,float left,float down,float w,int font,Color color,FontStyle style=FontStyle.Normal){
     var t=Kit.Label(rt,text,font,color,TextAnchor.UpperLeft,Vector2.zero,Vector2.zero);t.supportRichText=false;t.fontStyle=style;t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(0,1);t.rectTransform.pivot=new Vector2(0,1);t.rectTransform.anchoredPosition=new Vector2(left,-down);t.rectTransform.sizeDelta=new Vector2(w,100);t.rectTransform.sizeDelta=new Vector2(w,t.preferredHeight+4);return t;
    }
   }
  }
  string CategoryTitle()=>page=="Visual"?"VISIÓN":page=="Motor"?"MOVIMIENTO Y CONTROLES":page=="Cognitiva"?"COMPRENSIÓN":page.ToUpperInvariant();
  public bool SettingsPointerDrag {get;set;}
 }
 public sealed class AccessSliderDrag:MonoBehaviour,UnityEngine.EventSystems.IPointerDownHandler,UnityEngine.EventSystems.IPointerUpHandler {
  public AccessController owner;
  public void OnPointerDown(UnityEngine.EventSystems.PointerEventData e){owner.SettingsPointerDrag=true;}
  public void OnPointerUp(UnityEngine.EventSystems.PointerEventData e){owner.SettingsPointerDrag=false;owner.Refresh();}
  void OnDisable(){if(owner!=null)owner.SettingsPointerDrag=false;}
 }
}
