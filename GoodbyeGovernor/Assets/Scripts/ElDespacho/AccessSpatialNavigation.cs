using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 public sealed partial class AccessController {
  // Screen-space geometry keeps navigation correct across canvases/scales and reflowed grids.
  // TAB retains its sequential order. Arrows never wrap to the opposite edge.
  public void NavigateDirection(Vector2 direction){
   if(!P.keyboard||controls.Count==0)return;
   if(focused<0||focused>=controls.Count||!SpatialEligible(controls[focused])){
    int first=controls.FindIndex(SpatialEligible);if(first>=0)SetSpatialFocus(first);return;
   }
   var origin=controls[focused].target as RectTransform;
   if(Mathf.Abs(direction.x)>.5f){
    var slider=origin.GetComponentInChildren<Slider>();
    if(slider!=null&&slider.IsActive()&&slider.IsInteractable()){
     float step=(slider.maxValue-slider.minValue)*.05f;if(slider.wholeNumbers)step=Mathf.Max(1,Mathf.Round(step));
     slider.value=Mathf.Clamp(slider.value+Mathf.Sign(direction.x)*step,slider.minValue,slider.maxValue);
     return; // A value change must never also move focus, including at either limit.
    }
   }
   Canvas.ForceUpdateCanvases();var from=ScreenBounds(origin);var group=origin.GetComponentInParent<ScrollRect>();
   var country=origin.GetComponent<AccessMapCountry>();
   if(country!=null){NavigateMapCountry(origin,direction);return;}
   int best=FindDirectional(from,direction,origin,Mathf.Abs(direction.y)>.5f?group:null);
   if(best<0&&group!=null)best=FindDirectional(from,direction,origin,null);
   if(best>=0)SetSpatialFocus(best);
  }
  int FindDirectional(Rect from,Vector2 dir,RectTransform origin,ScrollRect restrict){
   int best=-1;float score=float.PositiveInfinity;bool horizontal=Mathf.Abs(dir.x)>.5f;
   for(int i=0;i<controls.Count;i++){
    var e=controls[i];if(i==focused||!SpatialEligible(e)||e.target==origin)continue;
    var rt=(RectTransform)e.target;if(restrict!=null&&rt.GetComponentInParent<ScrollRect>()!=restrict)continue;
    var to=ScreenBounds(rt);var delta=to.center-from.center;float advance=Vector2.Dot(delta,dir);
    if(advance<=1f)continue;
    float gap=horizontal?Mathf.Max(0,Mathf.Max(from.yMin-to.yMax,to.yMin-from.yMax)):Mathf.Max(0,Mathf.Max(from.xMin-to.xMax,to.xMin-from.xMax));
    float cross=horizontal?Mathf.Abs(delta.y):Mathf.Abs(delta.x);
    float candidate=(gap>2?1000000:0)+advance+gap*4+cross*.2f;
    if(candidate<score){score=candidate;best=i;}
   }
   return best;
  }
  bool SpatialEligible(Entry e){
   if(!e.enabled||e.action==null||!(e.target is RectTransform)||!e.target.gameObject.activeInHierarchy)return false;
   if(e.target.GetComponentsInParent<CanvasGroup>().Any(g=>g.alpha<=.01f))return false;
   var button=e.target.GetComponent<Button>();if(button!=null&&!button.IsInteractable())return false;
   return true;
  }
  static Rect ScreenBounds(RectTransform rt){
   var canvas=rt.GetComponentInParent<Canvas>();Camera camera=canvas!=null&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
   var corners=new Vector3[4];rt.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);var b=a;
   foreach(var corner in corners){var p=RectTransformUtility.WorldToScreenPoint(camera,corner);a=Vector2.Min(a,p);b=Vector2.Max(b,p);}
   return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
  }
  void SetSpatialFocus(int index){focused=index;Speech.Say(controls[index].text);RevealFocused();DrawFocus();}
 }
}
