#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using UnityEditor;using ElDespacho;
public sealed class NavigationScrollProbe:MonoBehaviour {
 AccessController a;DeskMenus m;List<string> lines=new List<string>();const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 Transform UI=>a.transform.Find("Accessibility");
 void Check(bool ok,string name){lines.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines("Logs/Navigation-scroll-v098.txt",lines);if(!ok)throw new Exception(name);}
 IEnumerator Tick(){a.Refresh();yield return null;yield return null;yield return null;Canvas.ForceUpdateCanvases();}
 Button Native(string name)=>m.GetComponentsInChildren<Button>().First(b=>b.GetComponentInParent<Canvas>().sortingOrder==400&&b.name==name);
 Button Back()=>UI.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<Text>().text=="VOLVER");
 void Err(string x,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert){File.AppendAllText("Logs/Navigation-scroll-v098.txt",x+stack);EditorApplication.Exit(1);}}
 IEnumerator Start(){Application.logMessageReceived+=Err;yield return null;yield return null;a=AccessController.Instance;m=a.GetComponent<DeskMenus>();
 for(int n=0;n<3;n++){
  m.ShowMain();yield return Tick();Native("AJUSTES").onClick.Invoke();yield return Tick();Check(m.PageName=="Ajustes"&&a.CustomPage==""&&!UI.GetComponent<Canvas>().enabled,"main -> general settings "+n);
  foreach(string title in new[]{"AUDIO","PANTALLA","JUGABILIDAD","CONTROLES","ACCESIBILIDAD"})Check(Native(title)!=null,"general tab "+title);
  Native("ACCESIBILIDAD").onClick.Invoke();yield return Tick();Check(a.CustomPage=="Inicio","explicit accessibility entry");a.OpenCategory("Visual");yield return Tick();Back().onClick.Invoke();yield return Tick();Check(a.CustomPage=="Inicio","category back -> profiles");Back().onClick.Invoke();yield return Tick();Check(a.CustomPage==""&&m.PageName=="Ajustes","accessibility back -> general");Native("VOLVER").onClick.Invoke();yield return Tick();Check(m.PageName=="Principal","general back -> main");
 }
 m.ShowSettings(0);yield return Tick();Check(UI.GetComponentsInChildren<Text>().Any(t=>t.text=="AUDIO Y NARRACIÓN")&&!UI.GetComponentsInChildren<Text>().Any(t=>t.text=="PERFILES"),"shared Audio uses general navigation, not accessibility center");Back().onClick.Invoke();yield return Tick();Check(a.CustomPage==""&&m.PageName=="Ajustes","general audio back -> settings");
 m.ShowSettings(4);yield return Tick();a.Escape();yield return Tick();Check(a.CustomPage==""&&m.PageName=="Ajustes","ESC matches accessibility back");
 a.SetReadingView(true);a.OpenCategory("Glosario");yield return Tick();a.Escape();yield return Tick();a.Escape();yield return Tick();Check(a.IsReadingView,"reader consultation returns to reader rather than opening general settings");a.SetReadingView(false);
 foreach(float size in new[]{1f,1.75f})foreach(bool reduced in new[]{false,true}){
  AccessPreferences.Current.textScale=size;DeskPreferences.Current.reduceMotion=reduced;a.OpenCategory("Audio");yield return Tick();var s=UI.GetComponentsInChildren<AccessScrollRect>().First();s.CancelWheel();s.verticalNormalizedPosition=1;Canvas.ForceUpdateCanvases();float max=s.content.rect.height-s.viewport.rect.height;Check(max>300,"scrollable content "+size);
  var evt=new PointerEventData(EventSystem.current){scrollDelta=new Vector2(0,-120)};s.OnScroll(evt);float immediate=s.content.anchoredPosition.y;
  Check(reduced?Mathf.Abs(immediate-48)<1:Mathf.Abs(immediate)<1,"wheel starts bounded / reduced motion immediate");
  yield return new WaitForSecondsRealtime(.25f);float y=s.content.anchoredPosition.y;Check(Mathf.Abs(y-48)<1,"one wheel event settles at 48 units, not a page");
  yield return new WaitForSecondsRealtime(.25f);Check(Mathf.Abs(y-s.content.anchoredPosition.y)<1,"wheel stops without long inertia");
  var focusField=typeof(AccessController).GetField("focused",F);a.MoveFocus(1);int focus=(int)focusField.GetValue(a);s.OnScroll(evt);yield return new WaitForSecondsRealtime(.25f);float manual=s.content.anchoredPosition.y;yield return Tick();s=UI.GetComponentsInChildren<AccessScrollRect>().First();Check(Mathf.Abs(s.content.anchoredPosition.y-manual)<1&&(int)focusField.GetValue(a)==focus,"refresh preserves manual position and focus");
  s.OnScroll(new PointerEventData(EventSystem.current){scrollDelta=new Vector2(0,-.25f)});yield return new WaitForSecondsRealtime(.25f);Check(Mathf.Abs(s.content.anchoredPosition.y-manual-12)<1,"fractional wheel supports intermediate position");
  for(int i=0;i<800;i++)s.OnScroll(evt);yield return new WaitForSecondsRealtime(.5f);Check(Mathf.Abs(s.content.anchoredPosition.y-max)<1,"bottom clamped");
  evt.scrollDelta=new Vector2(0,120);for(int i=0;i<800;i++)s.OnScroll(evt);yield return new WaitForSecondsRealtime(.5f);Check(Mathf.Abs(s.content.anchoredPosition.y)<1,"top clamped without bounce");
  var controls=(List<AccessController.Entry>)typeof(AccessController).GetField("controls",F).GetValue(a);
  for(int i=0;i<controls.Count;i++){a.MoveFocus(1);Canvas.ForceUpdateCanvases();int idx=(int)focusField.GetValue(a);var rt=(RectTransform)controls[idx].target;var sc=rt.GetComponentInParent<ScrollRect>();if(sc==null)continue;var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(sc.viewport,rt);Check(bounds.min.y>=sc.viewport.rect.yMin-1&&bounds.max.y<=sc.viewport.rect.yMax+1,"keyboard focus visible "+i);}
 }
 lines.Add("COMPLETE "+lines.Count(x=>x.StartsWith("PASS "))+" PASS; simulated wheel and focus, real Unity Play Mode; no physical device claim");File.WriteAllLines("Logs/Navigation-scroll-v098.txt",lines);EditorApplication.Exit(0);
 }
}
public static class NavigationScrollRunner {
 public static void Run(){SessionState.SetBool("Nav098",true);SessionState.SetString("Nav098Dir",Path.Combine(Path.GetTempPath(),"Despacho098-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("Nav098",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("Nav098Dir","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("Nav098",false))return;SessionState.SetBool("Nav098",false);new GameObject("Nav098").AddComponent<NavigationScrollProbe>();}
}
#endif
