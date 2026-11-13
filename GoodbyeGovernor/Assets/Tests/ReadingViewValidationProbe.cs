#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using UnityEditor;using ElDespacho;
public sealed class ReadingViewValidationProbe:MonoBehaviour {
 AccessController a;DespachoGame game;DeskMenus menus;List<string> lines=new List<string>();const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 void Check(bool ok,string name){lines.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines("Logs/ReadingView-v093.txt",lines);if(!ok)throw new Exception(name);}
 void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert){File.AppendAllText("Logs/ReadingView-v093.txt",m+s);EditorApplication.Exit(1);}}
 IEnumerator Tick(){a.Refresh();yield return null;yield return null;yield return null;}
 void Click(string label){var b=game.GetComponentsInChildren<Button>().First(x=>x.IsInteractable()&&string.Join(" ",x.GetComponentsInChildren<Text>().Select(t=>t.text)).Contains(label));b.onClick.Invoke();}
 void Visual(string label){Check(!a.IsReadingView&&!a.transform.Find("Accessibility").GetComponent<Canvas>().enabled,label+": no reader overlay");Check(game.transform.Find("Canvas").GetComponent<Canvas>().enabled,label+": original game canvas enabled");}
 string State(){var s=game.CaptureSave();s.utcTicks=1;return JsonUtility.ToJson(s);}
 IEnumerator Start(){Application.logMessageReceived+=Log;yield return null;yield return null;game=FindAnyObjectByType<DespachoGame>();menus=game.GetComponent<DeskMenus>();a=AccessController.Instance;yield return Tick();Check(menus.PageName=="Principal"&&!game.HasSession,"normal boot");Visual("boot");
 // Seed a temporary prior campaign so NUEVA PARTIDA exercises confirmation.
 menus.Resume();game.StartFromMenu();menus.SaveSlot(1);game.ReturnToTitle();yield return Tick();
 for(int config=0;config<4;config++){
  AccessPreferences.Reset();DeskPreferences.Current.tutorial=false;DeskPreferences.Current.autosave=false;var p=AccessPreferences.Current;
  if(config==1){p.keyboard=true;p.textScale=1.25f;p.symbols=true;p.simpleLanguage=true;p.navigationHelp=true;p.glossary=true;}
  if(config==2){a.ApplyProfile(0);a.ApplyProfile(1);a.ApplyProfile(2);p.visualSounds=true;}
  if(config==3){p.readingView=true;p.textScale=1.75f;p.keyboard=true;p.independentColor=true;}
  p.Save();AccessPreferences.Load();p=AccessPreferences.Current;string prefs=JsonUtility.ToJson(p);game.ReturnToTitle();yield return Tick();Visual("title configuration "+config);
  Check(game.GetComponentsInChildren<RawImage>().Any(i=>i.name=="OfficeArtwork"&&i.texture!=null),"main artwork "+config);
  Click("NUEVA PARTIDA");yield return Tick();Check(menus.PageName=="Confirmación","new mandate confirmation "+config);Click("CONFIRMAR");yield return Tick();Visual("introduction "+config);Click("ASUMIR EL CARGO");yield return Tick();Check(game.ScreenName=="Despacho","office entered "+config);Visual("office "+config);
  var d=game.GetComponentInChildren<DeskDocument>();Check(d!=null&&!d.CanDecide,"physical closed document "+config);
  if(p.simpleDocuments){d.AccessSelect();d.AccessPlace();}else{
   var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,d.transform.position),pointerId=-1,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(d.gameObject,e,ExecuteEvents.pointerDownHandler);e.position=RectTransformUtility.WorldToScreenPoint(null,d.transform.parent.Find("ReadingZone").position);ExecuteEvents.Execute(d.gameObject,e,ExecuteEvents.dragHandler);ExecuteEvents.Execute(d.gameObject,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(d.gameObject,e,ExecuteEvents.endDragHandler);
  }
  yield return new WaitForSecondsRealtime(.4f);Check(d.CanDecide,"document opens via real interaction handlers "+config);Visual("opened document "+config);
  Click("MAPA DEL MUNDO");yield return Tick();Check(game.ScreenName=="Mapa","world map "+config);Visual("map "+config);
  var country=game.GetComponentsInChildren<AccessName>().First(x=>x.label.Contains("Sin visitar")&&x.GetComponent<Button>().IsInteractable());country.GetComponent<Button>().onClick.Invoke();yield return Tick();Check(game.ScreenName=="Visita","country visit "+config);Check(game.GetComponentsInChildren<Transform>().Any(t=>t.name=="Portrait"),"portrait panel preserved "+config);Visual("diplomacy "+config);
  Click("HACER ALIANZA");yield return Tick();Click("VOLVER AL DESPACHO");yield return Tick();Check(game.ScreenName=="Despacho","return to office "+config);Visual("office return "+config);
  string state=State();a.OpenReadingView();yield return Tick();Check(a.IsReadingView&&a.transform.Find("Accessibility").GetComponent<Canvas>().enabled,"explicit reading view "+config);Check(a.Snapshot().Any(e=>e.text.Contains("Volver al modo visual")),"visible exit action "+config);a.Snapshot().First(e=>e.text.Contains("Volver al modo visual")).action();yield return Tick();Visual("reader exited "+config);Check(state==State(),"reader round trip leaves campaign unchanged "+config);
  a.SetReadingView(true);yield return Tick();Check(a.Escape(),"ESC exits reading view "+config);yield return Tick();Visual("ESC exit "+config);
  a.SetReadingView(true);menus.SaveSlot(1);menus.LoadSlot(1);yield return Tick();Visual("load from reader "+config);Check(JsonUtility.ToJson(p)==prefs,"preferences unchanged through new/load/reader "+config);
 }
 var original=UnityEngine.InputSystem.InputSystem.settings;var settings=Instantiate(original);settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;UnityEngine.InputSystem.InputSystem.settings=settings;var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
 UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.F2));yield return Tick();Check(a.IsReadingView,"F2 input event explicitly opens reader");UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return Tick();UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.F2));yield return Tick();Check(!a.IsReadingView,"second F2 input event exits reader");UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);// The isolated batch process exits below; do not restore a transient settings object Unity has destroyed.
 lines.Add("COMPLETE "+lines.Count(x=>x.StartsWith("PASS "))+" PASS; full transitions in Unity Play Mode using UI callbacks and pointer event handlers, no physical input claim");File.WriteAllLines("Logs/ReadingView-v093.txt",lines);EditorApplication.Exit(0);
 }
}
public static class ReadingViewValidationRunner {
 public static void Run(){SessionState.SetBool("Reader093",true);SessionState.SetString("ReaderDir093",Path.Combine(Path.GetTempPath(),"Despacho093-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("Reader093",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("ReaderDir093","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("Reader093",false))return;SessionState.SetBool("Reader093",false);new GameObject("ReadingViewValidation").AddComponent<ReadingViewValidationProbe>();}
}
#endif
