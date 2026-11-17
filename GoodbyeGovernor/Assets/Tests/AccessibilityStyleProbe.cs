#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.UI;using UnityEditor;using ElDespacho;
public sealed class AccessibilityStyleProbe:MonoBehaviour {
 AccessController access;DeskMenus menus;List<string> results=new List<string>();
 void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines("Logs/Accessibility-style-v094.txt",results);if(!ok)throw new Exception(name);}
 void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert){File.AppendAllText("Logs/Accessibility-style-v094.txt",m+s);EditorApplication.Exit(1);}}
 IEnumerator Tick(){access.Refresh();yield return null;yield return null;yield return null;}
 Transform Root=>access.transform.Find("Accessibility");
 Button Button(string label)=>Root.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<Text>().text.Replace("● ","")==label);
 IEnumerator Start(){Application.logMessageReceived+=Log;yield return null;yield return null;access=AccessController.Instance;menus=access.GetComponent<DeskMenus>();menus.ShowSettings(0);yield return Tick();
 var native=access.GetComponentsInChildren<Button>().First(b=>b.name=="ACCESIBILIDAD");native.onClick.Invoke();yield return Tick();Check(access.CustomPage=="Inicio","opens from original settings button");
 string prefs=JsonUtility.ToJson(AccessPreferences.Current);
 string[] labels={"VISIÓN Y LECTURA","CONTROLES","COMPRENSIÓN","AUDIO Y NARRACIÓN"},ids={"Visual","Motor","Cognitiva","Audio"};
 for(int i=0;i<labels.Length;i++){Button(labels[i]).onClick.Invoke();yield return Tick();Check(access.CustomPage==ids[i],"sidebar navigation "+labels[i]);Check(Root.GetComponentsInChildren<RawImage>().Any(x=>x.texture==Resources.Load<Texture2D>("Menus/GreenLamp")),"shared lamp artwork "+ids[i]);Check(Button(labels[i]).GetComponent<Image>().sprite!=null&&Button("VOLVER").GetComponent<Image>().sprite!=null,"ornate navigation and return "+ids[i]);Check(Root.GetComponentsInChildren<ScrollRect>().Length==2,"independent content and navigation scrolling "+ids[i]);}
 Check(JsonUtility.ToJson(AccessPreferences.Current)==prefs,"visual navigation does not mutate preferences");Button("VOLVER").onClick.Invoke();yield return Tick();Check(access.CustomPage=="Inicio","return from category");Button("VOLVER").onClick.Invoke();yield return Tick();Check(access.CustomPage==""&&menus.PageName=="Ajustes"&&!Root.GetComponent<Canvas>().enabled,"return restores normal settings");
 menus.ShowSettings(4);yield return Tick();AccessPreferences.Current.textScale=1.75f;AccessPreferences.Current.interfaceScale=1.2f;DeskPreferences.Current.highContrast=true;access.OpenCategory("General");yield return Tick();for(int i=0;i<20;i++){access.MoveFocus(1);yield return Tick();}Check(Root.GetComponentsInChildren<Text>().All(t=>t.preferredHeight<=t.rectTransform.rect.height+3),"large high-contrast layout fits after focus navigation");
 results.Add("COMPLETE "+results.Count(x=>x.StartsWith("PASS "))+" PASS - automated Unity Play Mode");File.WriteAllLines("Logs/Accessibility-style-v094.txt",results);EditorApplication.Exit(0);
 }
}
public static class AccessibilityStyleRunner {
 public static void Run(){SessionState.SetBool("Style094",true);SessionState.SetString("StyleDir094",Path.Combine(Path.GetTempPath(),"Despacho094-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("Style094",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("StyleDir094","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("Style094",false))return;SessionState.SetBool("Style094",false);new GameObject("AccessibilityStyleValidation").AddComponent<AccessibilityStyleProbe>();}
}
#endif
