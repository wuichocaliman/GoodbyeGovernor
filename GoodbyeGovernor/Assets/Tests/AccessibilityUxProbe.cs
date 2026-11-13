#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using UnityEditor;using ElDespacho;
public sealed class AccessibilityUxProbe:MonoBehaviour {
 AccessController a;DeskMenus menus;List<string> results=new List<string>();int errors;
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines("Logs/Accessibility-v091.txt",results);if(!ok)throw new Exception(name);}
 void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert){errors++;File.AppendAllText("Logs/Accessibility-v091.txt",m+"\n"+s);EditorApplication.Exit(1);}}
 IEnumerator Tick(){a.Refresh();yield return null;yield return null;yield return null;Canvas.ForceUpdateCanvases();}
 List<AccessController.Entry> Entries()=>(List<AccessController.Entry>)typeof(AccessController).GetMethod("SettingsEntries",F).Invoke(a,null);
 Transform UI=>a.transform.Find("Accessibility");
 IEnumerator Start(){Application.logMessageReceived+=Log;yield return null;yield return null;a=AccessController.Instance;menus=a.GetComponent<DeskMenus>();a.ShowSettings();yield return Tick();
 Check(a.CustomPage=="Inicio"&&Entries().Count(e=>e.card)==4,"home has four profile cards");
 for(int i=0;i<4;i++){a.ApplyProfile(i);yield return Tick();Check((AccessPreferences.Current.profileMask&(1<<i))!=0,"profile applied "+i);}
 Check(AccessPreferences.Current.simpleDocuments&&AccessPreferences.Current.simpleLanguage&&AccessPreferences.Current.textScale==1.5f,"profiles combine without clearing earlier options");
 Check(a.ConfigurationStatus.StartsWith("Configuración recomendada"),"combined profile state");
 AccessPreferences.Current.textScale=1.75f;Check(a.ConfigurationStatus=="Configuración personalizada","individual setting marks custom state");
 foreach(var cat in new[]{"Visual","Motor","Cognitiva","Audio","General"}){a.OpenCategory(cat);yield return Tick();Check(Entries().Any(e=>e.text.StartsWith("VISTA PREVIA")),"live preview exists "+cat);foreach(float size in new[]{.75f,1f,1.75f})foreach(float scale in new[]{.8f,1.2f}){AccessPreferences.Current.textScale=size;AccessPreferences.Current.interfaceScale=scale;DeskPreferences.Current.highContrast=size>1;yield return Tick();Check(UI.GetComponentsInChildren<Text>().All(t=>t.preferredHeight<=t.rectTransform.rect.height+3),"all text fits "+cat+" "+size+"/"+scale);}}
 a.OpenCategory("Motor");yield return Tick();var game=a.GetComponent<DespachoGame>();menus.Resume();game.StartFromMenu();menus.ShowPause();a.OpenCategory("Motor");yield return Tick();var before=game.CaptureSave();before.utcTicks=1;string state=JsonUtility.ToJson(before);a.DemoPlace();Check(!a.DemoOpened,"demo requires selection");a.DemoDocument();a.DemoPlace();yield return Tick();Check(a.DemoOpened,"document demo select then place");var after=game.CaptureSave();after.utcTicks=1;Check(state==JsonUtility.ToJson(after),"demo leaves campaign untouched");
 a.OpenCategory("Audio");yield return new WaitForSecondsRealtime(3);yield return Tick();Check(a.Speech.Ready,"voice availability probe");results.Add("VOICE "+a.Speech.Voice);
 var sliders=UI.GetComponentsInChildren<Slider>();Check(sliders.Length==6,"six real volume sliders");Check(sliders.Count(s=>!s.interactable)==2,"music and ambience disabled without sources");
 var master=sliders.First();var drag=master.GetComponent<AccessSliderDrag>();drag.OnPointerDown(new PointerEventData(EventSystem.current));master.value=.35f;yield return Tick();Check(master!=null&&Mathf.Abs(DeskPreferences.Current.master-.35f)<.01f,"slider survives pointer drag and applies value");drag.OnPointerUp(new PointerEventData(EventSystem.current));yield return Tick();
 a.MoveFocus(1);a.AdjustFocused(1);yield return Tick();Check(Mathf.Abs(DeskPreferences.Current.master-.4f)<.01f,"keyboard adjusts focused slider");
 AccessPreferences.Current.Save();AccessPreferences.Load();Check(AccessPreferences.Current.simpleDocuments&&AccessPreferences.Current.profileMask==15,"new preferences persist");
 a.OpenCategory("General");yield return Tick();Entries().First(e=>e.text=="Restaurar valores").action();yield return Tick();Check(a.CustomPage=="Restaurar"&&AccessPreferences.Current.simpleDocuments,"reset needs confirmation");a.Escape();yield return Tick();Check(AccessPreferences.Current.simpleDocuments,"cancel reset preserves preferences");
 a.OpenCategory("Restaurar");yield return Tick();Entries().First(e=>e.text=="Confirmar restauración").action();yield return Tick();Check(!AccessPreferences.Current.simpleDocuments&&AccessPreferences.Current.profileMask==0,"confirmed reset restores defaults");
 File.WriteAllText(AccessPreferences.FilePath,"{\"textScale\":1.25,\"simpleDocuments\":true}");AccessPreferences.Load();Check(AccessPreferences.Current.textScale==1.25f&&AccessPreferences.Current.simpleDocuments&&!AccessPreferences.Current.readingView,"old preference JSON gains safe defaults");
 Check(errors==0,"no runtime errors");results.Add("COMPLETE "+results.Count(x=>x.StartsWith("PASS "))+" PASS; automated Play Mode, no physical usability or hearing test");File.WriteAllLines("Logs/Accessibility-v091.txt",results);EditorApplication.Exit(0);
 }
}
public static class AccessibilityUxRunner {
 public static void Run(){SessionState.SetBool("Ux091",true);SessionState.SetString("UxDir091",Path.Combine(Path.GetTempPath(),"Despacho091-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("Ux091",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("UxDir091","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("Ux091",false))return;SessionState.SetBool("Ux091",false);new GameObject("AccessibilityUxValidation").AddComponent<AccessibilityUxProbe>();}
}
#endif
