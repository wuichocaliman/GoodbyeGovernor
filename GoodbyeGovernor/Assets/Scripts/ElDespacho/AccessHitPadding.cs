using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 // Additional pointer surface only: events bubble to the existing document/stamp handlers.
 public sealed class AccessHitPadding:MonoBehaviour {
  RectTransform hit;DeskDocument doc;StampTool stamp;
  void Awake(){doc=GetComponent<DeskDocument>();stamp=GetComponent<StampTool>();var image=Kit.Panel(transform,Color.clear,Vector2.zero,Vector2.one,default,default,"AccessHitPadding");hit=image.rectTransform;hit.SetAsFirstSibling();image.raycastTarget=true;Refresh();}
  void LateUpdate()=>Refresh();
  void Refresh(){if(hit==null)return;bool active=AccessPreferences.Current.largeTargets && (doc!=null?doc.AccessAvailable:stamp!=null&&stamp.AccessAvailable);hit.gameObject.SetActive(active);float scale=Mathf.Max(.1f,transform.localScale.x);hit.offsetMin=Vector2.one*(-24f/scale);hit.offsetMax=Vector2.one*(24f/scale);}
  public bool Contains(Vector2 p)=>hit!=null&&hit.gameObject.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(hit,p);
 }
}
