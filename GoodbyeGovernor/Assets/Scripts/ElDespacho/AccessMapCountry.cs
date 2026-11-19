using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
namespace ElDespacho {
 // Public map presentation only. No hidden diplomatic attributes are read.
 public sealed class AccessMapCountry:MonoBehaviour {
  UnityAction visit; public string Label {get;private set;} public string FocusLabel {get;private set;}
  public void Setup(string name,UnityAction action,string badge){visit=action;FocusLabel=AccessController.Plain(name).Replace("\n"," · ")+" · "+(action!=null?"Disponible\nINTRO/Espacio: visitar":"Bloqueado\nSin visita")+" · TAB: otros controles";Label=AccessController.Plain(name).Replace("\n"," · ")+" — "+(action!=null?"Disponible. INTRO o Espacio: visitar.":badge=="i"?"Visitado. Visita bloqueada.":"Visita bloqueada.")+" TAB: otros controles.";}
  public void Activate(){if(visit!=null){var b=GetComponent<Button>();if(b!=null&&b.IsActive()&&b.IsInteractable()){visit=null;b.onClick.Invoke();}}}
 }
 public sealed partial class AccessController {
  Text mapFocusHint;string originalMapHint;
  void NavigateMapCountry(RectTransform origin,Vector2 direction){
   var from=ScreenBounds(origin).center;int best=-1;float score=float.PositiveInfinity;
   for(int i=0;i<controls.Count;i++){var e=controls[i];if(e.target==origin||!SpatialEligible(e)||e.target.GetComponent<AccessMapCountry>()==null)continue;
    var delta=ScreenBounds((RectTransform)e.target).center-from;float forward=Vector2.Dot(delta,direction);if(forward<=1)continue;
    float lateral=Mathf.Abs(delta.x*direction.y-delta.y*direction.x);float candidate=delta.magnitude+lateral*2;
    if(candidate<score){score=candidate;best=i;}
   }
   if(best>=0)SetSpatialFocus(best); // No country in that half-plane: retain selection. TAB exits the map.
  }
  void UpdateMapFocus(RectTransform target){
   var node=target!=null?target.GetComponent<AccessMapCountry>():null;
   var hint=game.GetComponentsInChildren<Text>().FirstOrDefault(t=>t.name=="MapKeyboardHint");
   if(hint==null)return;if(mapFocusHint!=hint){mapFocusHint=hint;originalMapHint=hint.text;}
   string extra=P.simpleLanguage?"\nPASOS: Elija un país disponible. Lea sus informes antes de decidir. ! y + son sospechas, no certezas.":P.navigationHelp?"\nElija un país disponible. Las marcas de amenaza y aliado son sospechas, no certezas.":"";
   mapFocusHint.text=(node==null||reader||menus.IsVisible?originalMapHint:node.FocusLabel)+extra;
  }
 }
}
