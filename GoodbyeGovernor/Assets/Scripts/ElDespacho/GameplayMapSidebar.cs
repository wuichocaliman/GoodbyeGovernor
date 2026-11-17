using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 public sealed class GameplayMapSidebar:MonoBehaviour {
  struct Saved {public RectTransform rt;public Vector2 min,max,pivot,pos,size;public int index;}
  List<Saved> items;List<GameObject> decoration;RectTransform view,flow;ScrollRect scroll;bool active;
  void Start(){items=new List<Saved>();decoration=new List<GameObject>();foreach(Transform child in transform){var rt=(RectTransform)child;if(child.GetComponent<Text>()!=null||child.GetComponent<Button>()!=null)items.Add(new Saved{rt=rt,min=rt.anchorMin,max=rt.anchorMax,pivot=rt.pivot,pos=rt.anchoredPosition,size=rt.sizeDelta,index=rt.GetSiblingIndex()});else decoration.Add(child.gameObject);}}
  void LateUpdate(){if(items==null)return;bool on=AccessPreferences.Current.textScale>1||DeskPreferences.Current.highContrast||AccessPreferences.Current.navigationHelp; if(on&&!active){
   if(view==null){view=Kit.Rect(transform,"MapInformationViewport",Vector2.zero,Vector2.one,new Vector2(12,12),new Vector2(-12,-12));view.gameObject.AddComponent<Image>().color=Color.clear;view.GetComponent<Image>().raycastTarget=true;view.gameObject.AddComponent<RectMask2D>();flow=Kit.Rect(view,"MapInformationFlow",new Vector2(0,1),new Vector2(1,1));flow.pivot=new Vector2(.5f,1);scroll=view.gameObject.AddComponent<AccessScrollRect>();scroll.viewport=view;scroll.content=flow;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=48;GameplayScrollTools.AddBar(scroll);}
   view.gameObject.SetActive(true);foreach(var item in items)item.rt.SetParent(flow,false);foreach(var item in decoration)item.SetActive(false);active=true;
  }else if(!on&&active){foreach(var s in items){s.rt.SetParent(transform,false);s.rt.SetSiblingIndex(s.index);s.rt.anchorMin=s.min;s.rt.anchorMax=s.max;s.rt.pivot=s.pivot;s.rt.sizeDelta=s.size;s.rt.anchoredPosition=s.pos;}foreach(var item in decoration)item.SetActive(true);view.gameObject.SetActive(false);active=false;}
  if(!active)return;float y=8;foreach(var item in items){var rt=item.rt;rt.anchorMin=new Vector2(0,1);rt.anchorMax=new Vector2(1,1);rt.pivot=new Vector2(.5f,1);rt.sizeDelta=new Vector2(0,100);var text=rt.GetComponent<Text>();float height;if(text!=null){var adapter=text.GetComponent<GameplayTextAccess>();var display=adapter!=null?adapter.Display:text;height=Mathf.Max(36,display.preferredHeight+12);}else height=100*AccessPreferences.Current.textScale;rt.sizeDelta=new Vector2(0,height);rt.anchoredPosition=new Vector2(0,-y);y+=height+18;}flow.sizeDelta=new Vector2(-18,y);}
 }
}
