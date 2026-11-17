using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 // A local presentation of the existing Text; never reconstructs gameplay or changes its content.
 // Keeps the source RectTransform intact for tutorial/layout/interaction owners.
 public sealed class GameplayTextAccess:MonoBehaviour {
  Text source;int baseFont;Color baseColor;bool baseEnabled;RectTransform viewport,flow;Image plate;GameObject track;Text visual;ScrollRect scroll;bool nativeScroll;string lastNative,rawNative;
  public Text Display=>nativeScroll?source:visual!=null&&visual.gameObject.activeInHierarchy?visual:source;
  public ScrollRect Scroll=>nativeScroll?source.GetComponentInParent<ScrollRect>():scroll;
  public bool Adapted {get;private set;}
  void Awake(){source=GetComponent<Text>();baseFont=source.fontSize;baseColor=source.color;baseEnabled=source.enabled;nativeScroll=source.GetComponent<ContentSizeFitter>()!=null;rawNative=source.text;}
  void LateUpdate()=>Apply();
  public void Apply(){
   if(source==null)return;float scale=AccessPreferences.Current.textScale;bool contrast=DeskPreferences.Current.highContrast;Adapted=contrast||!Mathf.Approximately(scale,1)||(GetComponent<GameplayGuidanceNote>()!=null&&AccessPreferences.Current.navigationHelp);
   if(nativeScroll){if(source.text!=lastNative)rawNative=source.text;source.fontSize=Mathf.RoundToInt(baseFont*scale);source.color=contrast?Color.white:baseColor;source.text=Format(rawNative,scale,contrast);lastNative=source.text;
    var view=source.GetComponentInParent<ScrollRect>();if(view!=null){if(plate==null){plate=Kit.Panel(view.viewport,Color.clear,Vector2.zero,Vector2.one,default,default,"GameplayTextPlate");plate.rectTransform.SetAsFirstSibling();}plate.color=contrast?Color.black:Color.clear;}return;}
   source.enabled=Adapted?false:baseEnabled;
   if(!Adapted){if(viewport!=null)viewport.gameObject.SetActive(false);return;}
   if(viewport==null){
    viewport=Kit.Rect(transform,"GameplayTextViewport",Vector2.zero,Vector2.one);var mask=viewport.gameObject.AddComponent<Image>();mask.color=Color.white;mask.raycastTarget=false;viewport.gameObject.AddComponent<Mask>().showMaskGraphic=false;
    plate=Kit.Panel(viewport,Color.clear,Vector2.zero,Vector2.one,default,default,"GameplayTextPlate");
    flow=Kit.Rect(viewport,"GameplayTextContent",new Vector2(0,1),new Vector2(1,1));flow.pivot=new Vector2(.5f,1);
    visual=flow.gameObject.AddComponent<Text>();visual.gameObject.AddComponent<GameplayTextVisual>();visual.raycastTarget=false;
    scroll=viewport.gameObject.AddComponent<AccessScrollRect>();scroll.content=flow;scroll.viewport=viewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=48;
    var rail=Kit.Panel(viewport,new Color(.3f,.3f,.3f,.8f),new Vector2(1,0),Vector2.one,new Vector2(-8,0),Vector2.zero);rail.raycastTarget=true;track=rail.gameObject;var handle=Kit.Panel(rail.transform,Pal.Gold,Vector2.zero,Vector2.one);handle.raycastTarget=true;var bar=rail.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle.rectTransform;bar.targetGraphic=handle;bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;
   }
   viewport.gameObject.SetActive(true);visual.font=source.font;visual.fontSize=Mathf.RoundToInt(baseFont*scale);visual.fontStyle=source.fontStyle;visual.lineSpacing=source.lineSpacing;visual.supportRichText=source.supportRichText;visual.resizeTextForBestFit=false;visual.horizontalOverflow=HorizontalWrapMode.Wrap;visual.verticalOverflow=VerticalWrapMode.Overflow;visual.color=contrast?Color.white:source.color;visual.text=Format(source.text,scale,contrast);
   float h=viewport.rect.height;float textH=visual.preferredHeight+2;bool overflow=textH>h+1;scroll.enabled=overflow;viewport.GetComponent<Image>().raycastTarget=overflow&&!GetComponentsInParent<Transform>().Any(t=>t.name=="ClosedCover");
   track.SetActive(overflow);flow.sizeDelta=new Vector2(overflow?-18:0,Mathf.Max(h,textH));visual.alignment=overflow?TextAnchor.UpperLeft:source.alignment;
   if(!overflow)flow.anchoredPosition=Vector2.zero;else flow.anchoredPosition=new Vector2(0,Mathf.Clamp(flow.anchoredPosition.y,0,Mathf.Max(0,textH-h)));
   float ph=Mathf.Min(h,textH+4);int row=(int)source.alignment/3,col=(int)source.alignment%3;var anchor=new Vector2(col==0?0:col==1?.5f:1,row==0?1:row==1?.5f:0);plate.rectTransform.anchorMin=plate.rectTransform.anchorMax=plate.rectTransform.pivot=anchor;plate.rectTransform.sizeDelta=new Vector2(Mathf.Min(viewport.rect.width,visual.preferredWidth+8),ph);plate.rectTransform.anchoredPosition=Vector2.zero;plate.color=contrast?Color.black:Color.clear;
  }
  static string Format(string value,float scale,bool contrast){value=value??"";value=Regex.Replace(value,@"<size=(\d+)>",m=>"<size="+Mathf.RoundToInt(int.Parse(m.Groups[1].Value)*scale)+">");return contrast?Regex.Replace(value,@"</?color[^>]*>",""):value;}
  void OnDisable(){if(source!=null)source.enabled=baseEnabled;}
 }
 public sealed class GameplayTextVisual:MonoBehaviour {}
 // Enlarges small native buttons without changing their callbacks or stealing a neighbour's original area.
 public sealed class GameplayButtonAccess:MonoBehaviour,ICanvasRaycastFilter {
  RectTransform hit;Button button;
  void Awake(){button=GetComponent<Button>();hit=Kit.Panel(transform,Color.clear,Vector2.zero,Vector2.one,default,default,"GameplayButtonPadding").rectTransform;hit.SetAsFirstSibling();hit.GetComponent<Image>().raycastTarget=true;}
  void LateUpdate(){if(hit==null)return;hit.gameObject.SetActive(AccessPreferences.Current.largeTargets&&button.IsInteractable());var r=(RectTransform)transform;var pad=new Vector2(Mathf.Max(0,72-r.rect.width)/2,Mathf.Max(0,72-r.rect.height)/2);hit.offsetMin=-pad;hit.offsetMax=pad;}
  public bool IsRaycastLocationValid(Vector2 p,Camera c){if(RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform,p,c))return true;return !GetComponentInParent<Canvas>().GetComponentsInChildren<Button>().Any(b=>b!=button&&b.IsInteractable()&&RectTransformUtility.RectangleContainsScreenPoint((RectTransform)b.transform,p,c));}
 }
 public sealed partial class AccessController {
  string lastContextHelp="";
  public string GameplayContextHelp(){
   if(menus.IsVisible)return "";
   if(game.ScreenName=="Mapa")return "HERRAMIENTAS: Pulse un país disponible para visitarlo. ! y + indican sospechas, no certezas. Revise la leyenda y vuelva al despacho cuando termine.";
   if(game.ScreenName=="Visita")return "HERRAMIENTAS: Cada pregunta consume una de las disponibles. Consulte DOSSIER y compare INTELIGENCIA (fuente y fecha), COMERCIO y REGIÓN. Los informes pueden estar equivocados o desactualizados. Alianza crea vínculos; cortar lazos aplica la acción indicada. Usted decide. F1: controles y glosario en Accesibilidad.";
   if(game.ScreenName=="Despacho")return "HERRAMIENTAS: Atienda los expedientes y visite los países pendientes. Después podrá cerrar el año; el mandato dura cuatro años. AYUDA permite consultar al asesor.";
   return "HERRAMIENTAS: Revise el resultado y los cambios de indicadores. Pulse el botón de continuación para avanzar.";
  }
  void UpdateGameplayPresentation(){


   foreach(var text in game.GetComponentsInChildren<Text>(false)){
    if(text.GetComponent<GameplayTextVisual>()!=null||!IsSource(text.transform))continue;
    var c=text.GetComponentInParent<Canvas>();if(c.sortingOrder==400&&(menus.PageName=="Principal"||menus.PageName=="Ajustes"))continue;
    if((text.text=="Elige un país con el ratón."||text.text.StartsWith("Puedes hacer")||text.text.StartsWith("No te quedan preguntas"))&&text.GetComponent<GameplayGuidanceNote>()==null)text.gameObject.AddComponent<GameplayGuidanceNote>();
    text.GetComponent<GameplayGuidanceNote>()?.Apply();
    var adapter=text.GetComponent<GameplayTextAccess>();if(adapter==null&&text.enabled)adapter=text.gameObject.AddComponent<GameplayTextAccess>();adapter?.Apply();
   }
   foreach(var b in game.GetComponentsInChildren<Button>(false))if(IsSource(b.transform)&&b.GetComponentInParent<Canvas>().sortingOrder!=400&&b.GetComponent<GameplayButtonAccess>()==null)b.gameObject.AddComponent<GameplayButtonAccess>();
  }
  public string NarrationSnapshot(){var list=Snapshot();return string.Join(". ",list.Where(e=>e.action==null).Select(e=>e.text))+". Opciones disponibles: "+string.Join(". ",list.Where(e=>e.action!=null&&e.enabled).Select(e=>e.text));}
 }
}
