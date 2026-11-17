#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEditor;using UnityEngine;using UnityEngine.UI;using ElDespacho;
public class CategoriesLayoutProbe:MonoBehaviour {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;AccessController a;string dir;List<string> log=new List<string>();bool Updated=>DeskMenus.Version=="v0.9.12-prototipo";Transform UI=>a.transform.Find("Accessibility");
 List<AccessController.Entry> Controls=>(List<AccessController.Entry>)typeof(AccessController).GetField("controls",F).GetValue(a);
 RectTransform Focus=>(RectTransform)typeof(AccessController).GetField("focusRect",F).GetValue(a);
 void Check(bool b,string s){log.Add((b?"PASS ":"FAIL ")+s);File.WriteAllLines(dir+"/results.txt",log);if(!b)throw new Exception(s);}
 IEnumerator Tick(){a.Refresh();yield return null;yield return null;yield return null;Canvas.ForceUpdateCanvases();}
 IEnumerator Shot(string n){yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(dir+"/"+n+".png",t.EncodeToPNG());Destroy(t);}
 void Fit(string p){Uniform(p);Check(UI.GetComponentsInChildren<Text>().All(t=>t.preferredHeight<=t.rectTransform.rect.height+3),p+" text fits without shrinking");}
 void Uniform(string page){
  if(!Updated)return;
  var cards=UI.GetComponentsInChildren<RectTransform>().Where(t=>t.name.StartsWith("Setting: ")).ToArray();
  Check(cards.Length>1&&cards.All(t=>Vector2.Distance(t.sizeDelta,cards[0].sizeDelta)<.1f),page+" ALL cards have equal width and height, including orphan rows");
  var titles=cards.Select(t=>(RectTransform)t.Find("CardTitle")).ToArray();var desc=cards.Select(t=>(RectTransform)t.Find("CardDescription")).ToArray();
  Check(titles.All(t=>Mathf.Abs(t.sizeDelta.y-titles[0].sizeDelta.y)<.1f&&Mathf.Abs(t.anchoredPosition.y-titles[0].anchoredPosition.y)<.1f),page+" equal header slots");
  Check(desc.All(t=>Mathf.Abs(t.sizeDelta.y-desc[0].sizeDelta.y)<.1f&&Mathf.Abs(t.anchoredPosition.y-desc[0].anchoredPosition.y)<.1f),page+" equal description slots");
  var targets=cards.Select(t=>{var slider=t.GetComponentInChildren<Slider>();return slider!=null?(RectTransform)slider.transform:(RectTransform)t.GetComponentInChildren<Button>().transform;}).ToArray();
  Check(targets.All(t=>Mathf.Abs((-t.anchoredPosition.y+t.rect.height)-(-targets[0].anchoredPosition.y+targets[0].rect.height))<.1f),page+" control bottoms aligned");
  Check(targets.All(t=>Mathf.Abs(t.anchoredPosition.x-targets[0].anchoredPosition.x)<.1f),page+" same inner padding");
 }
 string[] pages={"Visual","Motor","Cognitiva","Audio"};
 IEnumerator Start(){dir="Logs/CategoriesLayout-"+DeskMenus.Version;Directory.CreateDirectory(dir);Application.logMessageReceived+=(s,st,t)=>{if(t==LogType.Exception||t==LogType.Error){File.AppendAllText(dir+"/results.txt",s+st);EditorApplication.Exit(1);}};yield return null;yield return null;a=AccessController.Instance;a.ShowSettings();yield return new WaitForSecondsRealtime(2);
 foreach(string p in pages){a.OpenCategory(p);yield return Tick();yield return Shot(p+"-normal");Fit(p);Check(UI.GetComponentsInChildren<RawImage>().Any(x=>x.texture==Resources.Load<Texture2D>("Menus/GreenLamp")),p+" original background");if(!Updated)continue;
 Check(UI.GetComponentsInChildren<RectTransform>().Any(t=>t.name.StartsWith("Section: ")),p+" grouped headings");var first=UI.GetComponentsInChildren<RectTransform>().Where(t=>t.name.StartsWith("Setting: ")).Take(2).ToArray();Check(first.Length==2&&Mathf.Abs(first[0].anchoredPosition.y-first[1].anchoredPosition.y)<1&&first[1].anchoredPosition.x>first[0].anchoredPosition.x,p+" two-column cards at full HD");
 a.MoveFocus(1);yield return Tick();Check(Focus.parent==Controls[0].target&&Focus.GetComponentInParent<RectMask2D>()!=null,p+" focus attached and masked");var sc=Controls[0].target.GetComponentInParent<ScrollRect>();sc.verticalNormalizedPosition=0;float y=sc.content.anchoredPosition.y;yield return Tick();sc=Controls[0].target.GetComponentInParent<ScrollRect>();Check(Mathf.Abs(y-sc.content.anchoredPosition.y)<1,p+" refresh preserves manual scroll");yield return Shot(p+"-scroll-end");Check(Focus.GetComponentsInChildren<Graphic>().All(g=>g.canvasRenderer.cull),p+" outside focus culled");a.MoveFocus(1);yield return Tick();yield return Shot(p+"-keyboard-return");Check(Focus.GetComponentsInChildren<Graphic>().All(g=>!g.canvasRenderer.cull),p+" keyboard reveals focused control");
 }
 if(Updated){
 var asm=typeof(Editor).Assembly;var sizesType=asm.GetType("UnityEditor.GameViewSizes");var groupType=asm.GetType("UnityEditor.GameViewSizeGroupType");var sizeType=asm.GetType("UnityEditor.GameViewSize");var kind=asm.GetType("UnityEditor.GameViewSizeType");var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;var sizes=sizesType.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{Enum.Parse(groupType,"Standalone")});var gt=group.GetType();int index=(int)gt.GetMethod("GetTotalCount").Invoke(group,null);var size=Activator.CreateInstance(sizeType,flags,null,new object[]{Enum.Parse(kind,"FixedResolution"),1280,720,"Categories audit temporary"},null);gt.GetMethod("AddCustomSize").Invoke(group,new[]{size});var window=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));var selected=window.GetType().GetProperty("selectedSizeIndex",flags);int old=(int)selected.GetValue(window);
 try {selected.SetValue(window,index);yield return new WaitForSecondsRealtime(.5f);Check(Screen.width==1280&&Screen.height==720,"actual 1280x720 Game View");
 foreach(float scale in new[]{1f,1.75f})foreach(string p in pages){AccessPreferences.Current.textScale=scale;AccessPreferences.Current.interfaceScale=scale>1?1.2f:1;DeskPreferences.Current.highContrast=scale>1;AccessPreferences.Current.symbols=scale>1;AccessPreferences.Current.independentColor=scale>1;a.OpenCategory(p);yield return Tick();Fit(p+" 720p "+scale);var cards=UI.GetComponentsInChildren<RectTransform>().Where(t=>t.name.StartsWith("Setting: ")).ToArray();Check(cards.All(t=>Mathf.Abs(t.anchoredPosition.x-cards[0].anchoredPosition.x)<1),p+" one column at 720p "+scale);yield return Shot(p+"-720p-"+(scale>1?"175":"100"));a.MoveFocus(1);yield return Tick();a.MoveFocus(1);yield return Tick();yield return Shot(p+"-720p-focus-"+(scale>1?"175":"100"));Check(Focus.parent==Controls[1].target,p+" keyboard focus at 720p "+scale);}
 }finally{selected.SetValue(window,old);gt.GetMethod("RemoveCustomSize").Invoke(group,new object[]{index});}
 }
 log.Add("COMPLETE "+log.Count(x=>x.StartsWith("PASS "))+" PASS; Unity Play Mode, simulated focus/scroll; no physical device");File.WriteAllLines(dir+"/results.txt",log);EditorApplication.Exit(0);
 }
}
public static class CategoriesLayoutRunner {
 public static void Run(){SessionState.SetBool("CategoriesLayout",true);SessionState.SetString("CategoriesLayoutDir",Path.Combine(Path.GetTempPath(),"DespachoCategories-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("CategoriesLayout",false))return;DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory=SessionState.GetString("CategoriesLayoutDir","");}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("CategoriesLayout",false))return;SessionState.SetBool("CategoriesLayout",false);new GameObject("CategoriesLayoutProbe").AddComponent<CategoriesLayoutProbe>();}
}
#endif
