using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ElDespacho {
 // Wheel motion is independent of focus; ScrollRect still owns dragging, clipping and bars.
 public sealed class AccessScrollRect : ScrollRect {
  float target,lastWritten; bool pending;
  float Maximum=>content==null||viewport==null?0:Mathf.Max(0,content.rect.height-viewport.rect.height);
  public void CancelWheel(){pending=false;StopMovement();}
  public override void OnScroll(PointerEventData e){
   if(!IsActive()||content==null)return;
   Canvas.ForceUpdateCanvases();StopMovement();inertia=false;
   float delta=-Mathf.Clamp(e.scrollDelta.y,-1,1)*scrollSensitivity;
   target=Mathf.Clamp((pending?target:content.anchoredPosition.y)+delta,0,Maximum);
   lastWritten=content.anchoredPosition.y;pending=true;
   if(DeskPreferences.Current.reduceMotion){SetY(target);pending=false;}
  }
  void SetY(float y){var pos=content.anchoredPosition;pos.y=Mathf.Clamp(y,0,Maximum);content.anchoredPosition=pos;lastWritten=pos.y;}
  protected override void LateUpdate(){
   inertia=false;base.LateUpdate();
   if(!pending||content==null)return;
   // A drag, scrollbar or explicit PageUp/PageDown cancels an old wheel target.
   if(Mathf.Abs(content.anchoredPosition.y-lastWritten)>.5f){CancelWheel();return;}
   target=Mathf.Clamp(target,0,Maximum);
   float next=DeskPreferences.Current.reduceMotion?target:Mathf.Lerp(lastWritten,target,1-Mathf.Exp(-35*Time.unscaledDeltaTime));
   if(Mathf.Abs(next-target)<.5f){next=target;pending=false;}SetY(next);
  }
  public override void OnBeginDrag(PointerEventData e){CancelWheel();base.OnBeginDrag(e);}
  protected override void OnDisable(){CancelWheel();base.OnDisable();}
 }
}
