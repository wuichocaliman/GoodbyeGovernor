using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 public sealed class GameplayIndicatorLayout:MonoBehaviour {
  Text title,value,status;Vector2 titleMin,titleMax,titleOffMin,titleOffMax,valueMin,valueMax;bool ready;Vector2 cardSize;
  void Start(){cardSize=((RectTransform)transform).sizeDelta;var labels=GetComponentsInChildren<Text>().Where(t=>t.transform.parent==transform).ToArray();if(labels.Length<2)return;title=labels[0];value=labels[1];titleMin=title.rectTransform.anchorMin;titleMax=title.rectTransform.anchorMax;titleOffMin=title.rectTransform.offsetMin;titleOffMax=title.rectTransform.offsetMax;valueMin=value.rectTransform.anchorMin;valueMax=value.rectTransform.anchorMax;status=Kit.Label(transform,"",14,Pal.Ink,TextAnchor.MiddleCenter,new Vector2(.03f,.02f),new Vector2(.97f,.26f));status.name="IndicatorStatus";ready=true;}
  void LateUpdate(){if(!ready)return;var p=AccessPreferences.Current;bool on=p.textScale>1||DeskPreferences.Current.highContrast;((RectTransform)transform).sizeDelta=cardSize+new Vector2(0,p.textScale>=1.5f?40:0);if(on){title.rectTransform.anchorMin=new Vector2(.03f,.64f);title.rectTransform.anchorMax=new Vector2(.97f,.99f);title.rectTransform.offsetMin=title.rectTransform.offsetMax=Vector2.zero;value.rectTransform.anchorMin=new Vector2(.05f,.28f);value.rectTransform.anchorMax=new Vector2(.95f,.64f);}else{title.rectTransform.anchorMin=titleMin;title.rectTransform.anchorMax=titleMax;title.rectTransform.offsetMin=titleOffMin;title.rectTransform.offsetMax=titleOffMax;value.rectTransform.anchorMin=valueMin;value.rectTransform.anchorMax=valueMax;}
   status.gameObject.SetActive(p.independentColor||p.symbols);if(int.TryParse(value.text,out int n))status.text=AccessController.Status(n);
  }
 }
}
