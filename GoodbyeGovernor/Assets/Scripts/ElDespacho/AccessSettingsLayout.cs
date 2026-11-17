using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 public sealed partial class AccessController {
  string revealSetting=""; bool generalAudio;
  void RenderSettings(List<Entry> entries){
   string context="settings|"+page+"|"+generalAudio;var oldNav=root.GetComponentsInChildren<ScrollRect>().FirstOrDefault(x=>x!=scroll);float previousNav=oldNav!=null&&layoutContext==context?oldNav.content.anchoredPosition.y:0;float previous=scroll!=null&&layoutContext==context?scroll.content.anchoredPosition.y:0;layoutContext=context;
   foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}rows.Clear();controls.Clear();
   bool dark=DeskPreferences.Current.highContrast;
   Color ink=Color.white,paper=dark?Color.black:new Color(.075f,.118f,.106f,.86f),gold=Kit.Hex("#C6A24F");
   float scale=P.textScale;
   bool categoryCards=page=="Visual"||page=="Motor"||page=="Cognitiva"||page=="Audio";
   var bg=Kit.Panel(root,Pal.Wood,Vector2.zero,Vector2.one);bg.raycastTarget=true;
   var art=Resources.Load<Texture2D>("Menus/GreenLamp");
   if(art!=null){var artwork=Kit.Rect(bg.transform,"SettingsArtwork",Vector2.zero,Vector2.one);var image=artwork.gameObject.AddComponent<RawImage>();image.texture=art;image.raycastTarget=false;var fit=artwork.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=art.width/(float)art.height;bg.gameObject.AddComponent<RectMask2D>();}
   Kit.Panel(bg.transform,new Color(.035f,.04f,.03f,dark?.65f:.14f),Vector2.zero,Vector2.one);
   Kit.Panel(bg.transform,Kit.WithAlpha(Kit.Hex("#131e1b"),dark?.98f:.80f),new Vector2(.045f,.07f),new Vector2(.95f,.9f));
   Kit.Panel(bg.transform,gold,new Vector2(.045f,.07f),new Vector2(.047f,.9f));
   Kit.Panel(bg.transform,gold,new Vector2(.035f,.93f),new Vector2(.965f,.933f));
   Kit.Label(bg.transform,"REPÚBLICA DE AURELIA / PRESIDENCIA",19,Pal.Cream,TextAnchor.MiddleLeft,new Vector2(.04f,.94f),new Vector2(.8f,.98f));
   Kit.Label(bg.transform,"AJUSTES DEL DESPACHO",Mathf.RoundToInt(38*P.textScale),Pal.Gold,TextAnchor.MiddleLeft,new Vector2(.07f,.8f),new Vector2(.92f,.89f));
   Kit.Label(bg.transform,generalAudio?"AUDIO Y NARRACIÓN":"ACCESIBILIDAD · "+(page=="Inicio"?"PERFILES":CategoryTitle()),Mathf.RoundToInt(22*P.textScale),Pal.Cream,TextAnchor.MiddleLeft,new Vector2(.35f,.715f),new Vector2(.92f,.79f));
   notice=Kit.Label(bg.transform,applied+"  TAB: elegir · INTRO: activar · ←/→: volumen · AvPág: desplazar · ESC: volver",Mathf.RoundToInt(12*scale),Color.white,TextAnchor.MiddleLeft,new Vector2(.07f,.005f),new Vector2(.94f,.065f));
   var viewport=Kit.Panel(bg.transform,Color.clear,new Vector2(.35f,.18f),new Vector2(.915f,.71f));viewport.raycastTarget=true;viewport.gameObject.AddComponent<RectMask2D>();
   content=Kit.Rect(viewport.transform,"Flow",new Vector2(0,1),new Vector2(1,1));content.pivot=new Vector2(.5f,1);
   scroll=viewport.gameObject.AddComponent<AccessScrollRect>();scroll.viewport=viewport.rectTransform;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=48;
   var rail=Kit.Panel(bg.transform,dark?Kit.Hex("#333333"):Kit.Hex("#7A4E2E"),new Vector2(.92f,.18f),new Vector2(.93f,.71f));rail.raycastTarget=true;
   var thumb=Kit.Panel(rail.transform,gold,Vector2.zero,Vector2.one);thumb.raycastTarget=true;
   var bar=rail.gameObject.AddComponent<Scrollbar>();bar.handleRect=thumb.rectTransform;bar.targetGraphic=thumb;bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;
   // Navigation is rendered separately; the setting model and callbacks remain unchanged.
   entries=entries.Where(e=>!e.text.StartsWith("Volver a ")&&e.text!="Ajustes por categoría"&&!(page=="Inicio"&&new[]{"VISIÓN","MOVIMIENTO Y CONTROLES","COMPRENSIÓN","AUDIO","GENERAL"}.Contains(e.text))).ToList();
   Canvas.ForceUpdateCanvases();float available=viewport.rectTransform.rect.width-48;float y=24;
   // Cards become a single column at enlarged scales; text is never shrunk to fit.
   bool twoColumns=scale<=1.25f&&available*canvas.scaleFactor>=840;
   float cardWidth=categoryCards&&twoColumns?(available-20)/2:available;
   float uniformTitle=0,uniformDescription=0,uniformControl=0;
   if(categoryCards){
    // Measure the whole page once: every equivalent card shares the same slots.
    // Long/localized text enlarges all cards together instead of overflowing or shrinking.
    float pad=16*P.interfaceScale,icon=28*P.interfaceScale;
    var measure=Kit.Label(content,"",18,ink,TextAnchor.UpperLeft,Vector2.zero,Vector2.zero);
    measure.supportRichText=false;measure.resizeTextForBestFit=false;
    foreach(var item in entries.Where(e=>e.action!=null||e.sliderChange!=null)){
     uniformTitle=Mathf.Max(uniformTitle,icon,Height(item.text,cardWidth-2*pad-icon-16,24,FontStyle.Bold)+4);
     uniformDescription=Mathf.Max(uniformDescription,Height(item.description,cardWidth-2*pad,18)+4);
     float control=item.sliderChange!=null?Height(item.enabled?Mathf.RoundToInt(item.sliderValue*100)+" %":"No disponible",cardWidth-2*pad,20)+4+12+48*P.interfaceScale:
      Mathf.Max(68*P.interfaceScale,Height(!item.enabled?"No disponible":string.IsNullOrEmpty(item.value)?"Abrir":item.value,cardWidth-2*pad-64,20,FontStyle.Bold)+28);
     uniformControl=Mathf.Max(uniformControl,control);
    }
    measure.gameObject.SetActive(false);Destroy(measure.gameObject);
    float Height(string value,float width,int font,FontStyle style=FontStyle.Normal){measure.text=value??"";measure.fontSize=Mathf.RoundToInt(font*scale);measure.fontStyle=style;measure.rectTransform.sizeDelta=new Vector2(width,100);return measure.preferredHeight;}
   }
   string section="";
   for(int i=0;i<entries.Count;i++){
    var e=entries[i];
    if(categoryCards){string next=SettingsSection(e.text);if(next!=section){section=next;y+=BuildSection(section,24,y,available)+14;}if(e.action==null&&e.sliderChange==null){y+=BuildNote(e,24,y,available)+18;continue;}}
    if(page=="Inicio"&&!e.card){y+=BuildProfileFooter(e,24,y,available)+16;continue;}
    bool pair=(e.card||categoryCards)&&scale<=1.25f&&available*canvas.scaleFactor>=840&&i+1<entries.Count&&
     (e.card?entries[i+1].card:(entries[i+1].action!=null||entries[i+1].sliderChange!=null)&&SettingsSection(entries[i+1].text)==section);
    if(pair){float w=(available-20)/2;float a=BuildSetting(e,24,y,w),b=BuildSetting(entries[++i],44+w,y,w);y+=Mathf.Max(a,b)+20;}
    else y+=BuildSetting(e,24,y,categoryCards?cardWidth:available)+(categoryCards?20:16);
   }
   content.sizeDelta=new Vector2(0,y+20);Canvas.ForceUpdateCanvases();content.anchoredPosition=new Vector2(0,Mathf.Clamp(previous,0,Mathf.Max(0,content.rect.height-viewport.rectTransform.rect.height)));
   BuildNavigation();if(focused>=controls.Count)focused=-1;
   if(revealSetting!=""){int index=controls.FindIndex(e=>e.text.StartsWith(revealSetting+"."));if(index>=0){focused=index;RevealFocused();}revealSetting="";}

   void BuildNavigation(){
    Kit.Label(bg.transform,generalAudio?"AJUSTES":"ACCESIBILIDAD",Mathf.RoundToInt(21*P.textScale),Pal.Gold,TextAnchor.MiddleLeft,new Vector2(.07f,.715f),new Vector2(.30f,.79f));
    var nav=Kit.Panel(bg.transform,Color.clear,new Vector2(.065f,.18f),new Vector2(.31f,.71f));nav.raycastTarget=true;nav.gameObject.AddComponent<RectMask2D>();
    var navContent=Kit.Rect(nav.transform,"Categories",new Vector2(0,1),new Vector2(1,1));navContent.pivot=new Vector2(.5f,1);
    var navScroll=nav.gameObject.AddComponent<AccessScrollRect>();navScroll.viewport=nav.rectTransform;navScroll.content=navContent;navScroll.horizontal=false;navScroll.movementType=ScrollRect.MovementType.Clamped;navScroll.scrollSensitivity=48;
    string[] ids={"Visual","Motor","Cognitiva","Audio","Inicio"},labels={"VISIÓN Y LECTURA","CONTROLES","COMPRENSIÓN","AUDIO Y NARRACIÓN","PERFILES"};
    if(generalAudio){ids=new[]{"0","1","2","3","4"};labels=new[]{"AUDIO","PANTALLA","JUGABILIDAD","CONTROLES","ACCESIBILIDAD"};}
    Canvas.ForceUpdateCanvases();float navY=8;float width=nav.rectTransform.rect.width-12;
    // Visual placement starts with profiles. Registration keeps the established TAB path
    // (profile actions, Visión, remaining categories) for the default home screen.
    var navButtons=new List<Button>();var navRows=new List<RectTransform>();var navEntries=new List<Entry>();
    foreach(int i in generalAudio?new[]{0,1,2,3,4}:new[]{4,0,1,2,3}){string id=ids[i];UnityEngine.Events.UnityAction navigate=generalAudio?()=>menus.ShowSettings(int.Parse(id)):()=>OpenCategory(id);string label=((generalAudio?id=="0":page==id)?"● ":"")+labels[i];var b=Ornate(navContent,label,navigate,width,Mathf.RoundToInt(20*scale));var rt=(RectTransform)b.transform;rt.anchoredPosition=new Vector2(6,-navY);navY+=rt.sizeDelta.y+14;navButtons.Add(b);navRows.Add(rt);navEntries.Add(new Entry(label,()=>navigate(),rt));}
    navContent.sizeDelta=new Vector2(0,navY+8);Canvas.ForceUpdateCanvases();navContent.anchoredPosition=new Vector2(0,Mathf.Clamp(previousNav,0,Mathf.Max(0,navContent.rect.height-nav.rectTransform.rect.height)));
    foreach(int i in generalAudio?new[]{0,1,2,3,4}:new[]{1,2,3,4,0}){rows.Add(navRows[i]);controls.Add(navEntries[i]);}
    var back=Kit.MakeButton(bg.transform,"VOLVER",Mathf.RoundToInt(20*scale),Pal.Wood,Pal.Cream,()=>ReturnSettingsLevel(),new Vector2(.73f,.085f),new Vector2(.92f,.15f));DeskMenuSkin.Apply(back);back.GetComponentInChildren<Text>().resizeTextForBestFit=false;rows.Add((RectTransform)back.transform);controls.Add(new Entry("Volver",()=>back.onClick.Invoke(),back.transform));
   }
   Button Ornate(Transform parent,string label,UnityEngine.Events.UnityAction action,float width,int font){
    var b=Kit.MakeButton(parent,label,font,Pal.Wood,Pal.Cream,action,new Vector2(0,1),new Vector2(0,1));var rt=(RectTransform)b.transform;rt.pivot=new Vector2(0,1);rt.sizeDelta=new Vector2(width,70);DeskMenuSkin.Apply(b,true);
    var text=b.GetComponentInChildren<Text>();text.resizeTextForBestFit=false;text.rectTransform.offsetMin=new Vector2(32,12);text.rectTransform.offsetMax=new Vector2(-32,-12);Canvas.ForceUpdateCanvases();rt.sizeDelta=new Vector2(width,Mathf.Max(68*P.interfaceScale,text.preferredHeight+28));return b;
   }

   // Informational text is kept out of actionable cards; it never receives keyboard focus.
   float BuildSection(string title,float x,float top,float width){
    var t=Kit.Label(content,title.ToUpperInvariant(),Mathf.RoundToInt(18*scale),dark?Color.white:Pal.Gold,TextAnchor.UpperLeft,Vector2.zero,Vector2.zero);
    var rt=t.rectTransform;rt.name="Section: "+title;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-top);rt.sizeDelta=new Vector2(width,100);rt.sizeDelta=new Vector2(width,t.preferredHeight+12);
    var line=Kit.Panel(rt,Kit.WithAlpha(gold,.45f),Vector2.zero,new Vector2(1,0));line.rectTransform.sizeDelta=new Vector2(0,1);return rt.sizeDelta.y;
   }
   float BuildNote(Entry e,float x,float top,float width){
    var t=Kit.Label(content,e.text+"\n"+e.description,Mathf.RoundToInt(18*scale),ink,TextAnchor.UpperLeft,Vector2.zero,Vector2.zero);t.supportRichText=false;
    var rt=t.rectTransform;rt.name="Note: "+e.text;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-top);rt.sizeDelta=new Vector2(width,100);rt.sizeDelta=new Vector2(width,t.preferredHeight+8);return rt.sizeDelta.y;
   }

   float BuildProfileFooter(Entry e,float x,float top,float width){
    if(e.action!=null){var b=Ornate(content,e.value,()=>{e.action();Refresh();},Mathf.Min(width,420*scale),Mathf.RoundToInt(18*scale));var rt=(RectTransform)b.transform;rt.name="Setting: "+e.text;rt.anchoredPosition=new Vector2(x,-top);rows.Add(rt);controls.Add(new Entry(e.text+". "+e.value,e.action,rt));return rt.sizeDelta.y;}
    var label=Kit.Label(content,"Opciones activas · "+e.description,Mathf.RoundToInt(16*scale),Pal.Cream,TextAnchor.UpperLeft,Vector2.zero,Vector2.zero);var textRect=label.rectTransform;textRect.name="CompactProfileSummary";textRect.anchorMin=textRect.anchorMax=new Vector2(0,1);textRect.pivot=new Vector2(0,1);textRect.anchoredPosition=new Vector2(x,-top);textRect.sizeDelta=new Vector2(width,100);textRect.sizeDelta=new Vector2(width,label.preferredHeight+8);return textRect.sizeDelta.y;
   }

   float BuildSetting(Entry e,float x,float top,float width){
    var rt=Kit.Rect(content,"Setting: "+e.text,new Vector2(0,1),new Vector2(0,1));rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-top);rt.sizeDelta=new Vector2(width,100);
    var border=rt.gameObject.AddComponent<Image>();border.color=dark?Kit.Hex("#777777"):Kit.WithAlpha(gold,e.card&&e.value=="Desactivar perfil"?.95f:.38f);border.raycastTarget=e.action!=null;
    var inner=Kit.Panel(rt,paper,Vector2.zero,Vector2.one,new Vector2(2,2),new Vector2(-2,-2));
    bool compact=e.card||categoryCards;
    float pad=(compact?16:24)*P.interfaceScale,iconSize=(compact?28:32)*P.interfaceScale;
    var iconRect=Kit.Rect(rt,"Icon",new Vector2(0,1),new Vector2(0,1));iconRect.pivot=new Vector2(0,1);iconRect.anchoredPosition=new Vector2(pad,-pad);iconRect.sizeDelta=Vector2.one*iconSize;
    var icon=iconRect.gameObject.AddComponent<AccessSymbol>();icon.symbol=e.icon??"D";icon.color=ink;icon.raycastTarget=false;
    var title=TextAt((e.card&&e.value=="Desactivar perfil"?"✓ ACTIVO · ":"")+e.text,pad+iconSize+16,pad,width-pad*2-iconSize-16,Mathf.RoundToInt(24*scale),dark?Color.white:Pal.Gold,FontStyle.Bold);
    title.name="CardTitle";
    if(categoryCards)title.rectTransform.sizeDelta=new Vector2(title.rectTransform.sizeDelta.x,uniformTitle);
    float current=pad+Mathf.Max(iconSize,title.rectTransform.sizeDelta.y)+14;
    if(!string.IsNullOrEmpty(e.description)){var desc=TextAt(e.description,pad,current,width-2*pad,Mathf.RoundToInt((compact?18:20)*scale),ink);desc.name="CardDescription";if(categoryCards)desc.rectTransform.sizeDelta=new Vector2(desc.rectTransform.sizeDelta.x,uniformDescription);current+=desc.rectTransform.sizeDelta.y+18;}
    if(categoryCards&&string.IsNullOrEmpty(e.description))current+=uniformDescription+18;
    float controlTop=current;
    if(e.sliderChange!=null){
     if(categoryCards)current=controlTop+uniformControl-(48*P.interfaceScale+12+Mathf.RoundToInt(20*scale)+4);
     var value=TextAt(e.enabled?Mathf.RoundToInt(e.sliderValue*100)+" %":"No disponible",pad,current,width-pad*2,Mathf.RoundToInt(20*scale),ink);if(categoryCards){current=controlTop+uniformControl-(value.rectTransform.sizeDelta.y+12+48*P.interfaceScale);value.rectTransform.anchoredPosition=new Vector2(pad,-current);}current+=value.rectTransform.sizeDelta.y+12;
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
     if(e.enabled){rows.Add(rt);controls.Add(new Entry(e.text+". "+e.description,()=>{},area));}
    }else if(e.action!=null){
     string caption=!e.enabled?"No disponible":string.IsNullOrEmpty(e.value)?"Abrir":e.value;
     var b=Ornate(rt,caption,()=>{revealSetting=e.text;e.action();Refresh();},width-pad*2,Mathf.RoundToInt(20*scale));var br=(RectTransform)b.transform;if(categoryCards)current=controlTop+uniformControl-br.sizeDelta.y;br.anchoredPosition=new Vector2(pad,-current);b.interactable=e.enabled;current+=br.sizeDelta.y+pad;
     if(e.enabled){rows.Add(rt);controls.Add(new Entry(e.text+". "+e.description+". "+e.value,e.action,br));}
    }else current+=pad;
    rt.sizeDelta=new Vector2(width,Mathf.Max(current,92*P.interfaceScale));return rt.sizeDelta.y;
    Text TextAt(string text,float left,float down,float w,int font,Color color,FontStyle style=FontStyle.Normal){
     var t=Kit.Label(rt,text,font,color,TextAnchor.UpperLeft,Vector2.zero,Vector2.zero);t.supportRichText=false;t.fontStyle=style;t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(0,1);t.rectTransform.pivot=new Vector2(0,1);t.rectTransform.anchoredPosition=new Vector2(left,-down);t.rectTransform.sizeDelta=new Vector2(w,100);t.rectTransform.sizeDelta=new Vector2(w,t.preferredHeight+4);return t;
    }
   }
  }
  // Grouping is presentation metadata only: model order, callbacks and preference keys stay intact.
  string SettingsSection(string title){
   if(title.StartsWith("VISTA PREVIA")||title.StartsWith("[=]")||title.StartsWith("Documento de ejemplo")||title=="Zona de lectura"||title=="Ejemplo de aviso")return "Vista previa";
   if(page=="Visual")return title=="Alto contraste"||title=="Tamaño de texto"||title=="Escala de interfaz"?"Presentación":title=="Indicadores con etiquetas"||title=="Reducir destellos"?"Apoyos visuales":"Lectura opcional";
   if(page=="Motor")return title=="Seleccionar documentos con clics"||title=="Navegación por teclado"||title=="Botones y zonas más grandes"?"Interacción":"Movimiento y ayuda";
   if(page=="Cognitiva")return title=="Instrucciones sencillas"||title=="Ayudas de navegación"?"Instrucciones y orientación":title=="Símbolos explicativos"||title=="Glosario disponible"?"Símbolos y términos":"Consultas";
   if(title=="Volumen maestro"||title=="Música"||title=="Efectos de sonido"||title=="Interfaz"||title=="Ambiente")return "Volúmenes del juego";
   if(title=="Narración del sistema"||title=="Volumen de narración"||title=="Velocidad de voz")return "Voz";
   if(title=="Audio mono"||title=="Reducir sonidos agudos"||title=="Avisos visuales")return "Apoyos de audio";
   return "Prueba de voz";
  }
  string CategoryTitle()=>page=="Visual"?"VISIÓN Y LECTURA":page=="Motor"?"CONTROLES":page=="Cognitiva"?"COMPRENSIÓN":page=="Audio"?"AUDIO Y NARRACIÓN":page.ToUpperInvariant();
  public bool SettingsPointerDrag {get;set;}
 }
 public sealed class AccessSliderDrag:MonoBehaviour,UnityEngine.EventSystems.IPointerDownHandler,UnityEngine.EventSystems.IPointerUpHandler {
  public AccessController owner;
  public void OnPointerDown(UnityEngine.EventSystems.PointerEventData e){owner.SettingsPointerDrag=true;}
  public void OnPointerUp(UnityEngine.EventSystems.PointerEventData e){owner.SettingsPointerDrag=false;owner.Refresh();}
  void OnDisable(){if(owner!=null)owner.SettingsPointerDrag=false;}
 }
}


