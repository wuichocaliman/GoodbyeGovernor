using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 // Additional pointer surface only: events bubble to the existing document/stamp handlers.
 public sealed class AccessHitPadding:MonoBehaviour {
  RectTransform hit;DeskDocument doc;StampTool stamp;
  void Awake(){doc=GetComponent<DeskDocument>();stamp=GetComponent<StampTool>();var image=Kit.Panel(transform,Color.clear,Vector2.zero,Vector2.one,default,default,"AccessHitPadding");hit=image.rectTransform;hit.SetAsFirstSibling();image.raycastTarget=true;Refresh();}
  void LateUpdate()=>Refresh();
  void Refresh(){if(hit==null)return;bool active=AccessPreferences.Current.largeTargets && (doc!=null?doc.AccessAvailable:stamp!=null&&stamp.AccessAvailable);hit.gameObject.SetActive(active);float scale=Mathf.Max(.1f,transform.localScale.x);hit.offsetMin=Vector2.one*(-24f/scale);hit.offsetMax=Vector2.one*(24f/scale);}
  // Same logical padding as document/stamp targets; do not consume a neighbouring button.
  public static bool ContainsZone(RectTransform zone,Vector2 p,Camera camera=null){
   if(zone==null)return false;
   if(RectTransformUtility.RectangleContainsScreenPoint(zone,p,camera))return true;
   if(!AccessPreferences.Current.largeTargets||!RectTransformUtility.ScreenPointToLocalPointInRectangle(zone,p,camera,out var local))return false;
   var r=zone.rect;r.xMin-=24;r.xMax+=24;r.yMin-=24;r.yMax+=24;
   if(!r.Contains(local))return false;
   var canvas=zone.GetComponentInParent<Canvas>();
   if(canvas!=null)foreach(var button in canvas.GetComponentsInChildren<Button>())
    if(button.IsInteractable()&&RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform,p,camera))return false;
   return true;
  }
  public bool Contains(Vector2 p)=>hit!=null&&hit.gameObject.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(hit,p);
 }
}
