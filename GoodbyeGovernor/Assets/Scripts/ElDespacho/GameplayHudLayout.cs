using System.Collections.Generic;using UnityEngine;using UnityEngine.UI;
namespace ElDespacho {
 // Reserve actual height for enlarged HUD labels; gameplay rules remain untouched.
 public sealed class GameplayHudLayout:MonoBehaviour {
  RectTransform rt;Dictionary<RectTransform,Vector2[]> cards=new Dictionary<RectTransform,Vector2[]>();Dictionary<RectTransform,Vector2> tops=new Dictionary<RectTransform,Vector2>();
  void Awake(){rt=(RectTransform)transform;}
  void LateUpdate(){bool large=AccessPreferences.Current.textScale>=1.5f;rt.offsetMin=new Vector2(0,large?-210:AccessPreferences.Current.glossary?-134:-100);foreach(Transform child in transform){var text=child.GetComponent<Text>();var cr=child as RectTransform;if(text!=null&&(text.text.Contains("ESTABILIDAD")||text.text.Contains("SEGURIDAD")||text.text.Contains("PROSPERIDAD")))cr.anchorMin=new Vector2(cr.anchorMin.x,large?.42f:.52f);if(text!=null&&text.text.Contains("Países por atender")){cr.offsetMax=new Vector2(cr.offsetMax.x,large?0:-30);cr.anchorMin=new Vector2(cr.anchorMin.x,large?.5f:.3f);}if(child.name=="Bar"){cr.anchorMin=new Vector2(cr.anchorMin.x,large?.12f:.2f);cr.anchorMax=new Vector2(cr.anchorMax.x,large?.32f:.46f);}}
   foreach(Transform child in transform.parent){var r=child as RectTransform;if(r==null||r==rt)continue;if(child.name=="MapCard"&&!cards.ContainsKey(r))cards[r]=new[]{r.anchorMin,r.anchorMax};if(!tops.ContainsKey(r)&&r.anchorMax.y>.87f&&r.anchorMax.y<.90f)tops[r]=r.anchorMax;}foreach(var item in cards)if(item.Key!=null){item.Key.anchorMin=item.Value[0]+new Vector2(0,large?-.07f:0);item.Key.anchorMax=item.Value[1]+new Vector2(0,large?-.07f:0);}
   foreach(var item in tops)if(item.Key!=null)item.Key.anchorMax=large?new Vector2(item.Value.x,.79f):AccessPreferences.Current.glossary?new Vector2(item.Value.x,.865f):item.Value;}
 }
}
