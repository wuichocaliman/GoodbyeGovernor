using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 public sealed class DiplomaticAccessLayout:MonoBehaviour {
  void LateUpdate(){foreach(Transform child in transform){var t=child.GetComponent<Text>();if(t==null)continue;var rt=t.rectTransform;if(t.text=="CONFIDENCIAL"){bool enlarged=AccessPreferences.Current.textScale>1;rt.anchorMin=new Vector2(.6f,enlarged?.02f:.86f);rt.anchorMax=new Vector2(.98f,enlarged?.1f:.98f);rt.localEulerAngles=new Vector3(0,0,enlarged?0:12);continue;}if(rt.offsetMax.y<=-80){rt.offsetMax=new Vector2(rt.offsetMax.x,-80*Mathf.Max(1,AccessPreferences.Current.textScale));if(transform.Cast<Transform>().Any(c=>c.GetComponent<Text>()!=null&&c.GetComponent<Text>().text=="CONFIDENCIAL"))rt.offsetMin=new Vector2(rt.offsetMin.x,AccessPreferences.Current.textScale>1?90:24);}}}
 }
}
