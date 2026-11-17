using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 public sealed class GameplayVisitLayout:MonoBehaviour {
  RectTransform dialog,docs,desk,paper,view,flow;Button[] questions,tabs;List<Vector2[]> saved=new List<Vector2[]>();bool active;
  void Start(){dialog=(RectTransform)transform;docs=(RectTransform)transform.parent.Cast<Transform>().Last(t=>t.name=="Docs"&&t.gameObject.activeSelf);desk=(RectTransform)transform.parent.Cast<Transform>().Last(t=>t.name=="Buttons"&&t.gameObject.activeSelf);paper=(RectTransform)docs.Find("Paper");questions=GetComponentsInChildren<Button>().Where(b=>b.transform.parent==transform).ToArray();tabs=docs.GetComponentsInChildren<Button>().Where(b=>b.transform.parent==docs).ToArray();foreach(var b in questions){var r=(RectTransform)b.transform;saved.Add(new[]{r.anchorMin,r.anchorMax,r.offsetMin,r.offsetMax,r.pivot});}}
  void LateUpdate(){if(docs==null||questions==null)return;bool on=AccessPreferences.Current.textScale>1f;float bottom=AccessPreferences.Current.textScale>1.5f?.31f:.22f;dialog.anchorMin=new Vector2(.015f,on?bottom+.005f:.155f);docs.anchorMin=new Vector2(.505f,on?bottom+.005f:.155f);desk.anchorMax=new Vector2(1,on?bottom:.14f);paper.anchorMax=new Vector2(.98f,on?.80f:.9f);
   for(int i=0;i<tabs.Length;i++){var r=(RectTransform)tabs[i].transform;r.anchorMin=on?new Vector2(.02f+(i%2)*.48f,.905f-(i/2)*.09f):new Vector2(.02f+i*.24f,.91f);r.anchorMax=on?new Vector2(.49f+(i%2)*.48f,.985f-(i/2)*.09f):new Vector2(.25f+i*.24f,.985f);}
   if(on&&!active){if(view==null){view=Kit.Rect(transform,"QuestionsViewport",new Vector2(.03f,.02f),new Vector2(.97f,.25f));view.gameObject.AddComponent<Image>().color=Color.clear;view.GetComponent<Image>().raycastTarget=true;view.gameObject.AddComponent<RectMask2D>();flow=Kit.Rect(view,"QuestionsFlow",new Vector2(0,1),new Vector2(1,1));flow.pivot=new Vector2(.5f,1);var sc=view.gameObject.AddComponent<AccessScrollRect>();sc.viewport=view;sc.content=flow;sc.horizontal=false;sc.movementType=ScrollRect.MovementType.Clamped;sc.scrollSensitivity=48;GameplayScrollTools.AddBar(sc);}view.gameObject.SetActive(true);foreach(var b in questions)b.transform.SetParent(flow,false);active=true;}
   else if(!on&&active){for(int i=0;i<questions.Length;i++){var r=(RectTransform)questions[i].transform;r.SetParent(transform,false);r.anchorMin=saved[i][0];r.anchorMax=saved[i][1];r.offsetMin=saved[i][2];r.offsetMax=saved[i][3];r.pivot=saved[i][4];}view.gameObject.SetActive(false);active=false;}
   if(active){float y=0;foreach(var b in questions){var r=(RectTransform)b.transform;r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(.5f,1);var source=b.GetComponentInChildren<Text>();var a=source.GetComponent<GameplayTextAccess>();var text=a!=null?a.Display:source;float h=Mathf.Max(88,text.preferredHeight+40);r.sizeDelta=new Vector2(0,h);r.anchoredPosition=new Vector2(0,-y);y+=h+12;}flow.sizeDelta=new Vector2(-18,y);}
  }
 }
}
