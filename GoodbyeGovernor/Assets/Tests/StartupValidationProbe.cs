#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.UI;using UnityEditor;using ElDespacho;
public sealed class StartupValidationProbe:MonoBehaviour {
 List<string> lines=new List<string>();AccessController a;DespachoGame game;DeskMenus menus;bool restart;string output;
 void Check(bool ok,string label){lines.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(output,lines);if(!ok)throw new Exception(label);}
 void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert){File.AppendAllText(output,m+"\n"+s);EditorApplication.Exit(1);}}
 IEnumerator Tick(){a.Refresh();yield return null;yield return null;yield return null;}
 void Title(){Check(menus.IsVisible&&menus.PageName=="Principal"&&!game.HasSession,"normal main menu without campaign");Check(a.CustomPage==""&&!a.transform.Find("Accessibility").GetComponent<Canvas>().enabled,"no forced accessibility screen or reader");Check(game.GetComponentsInChildren<RawImage>().Any(x=>x.name=="OfficeArtwork"&&x.texture==Resources.Load<Texture2D>("Menus/PresidentialOffice")),"original presidential artwork assigned");Check(game.GetComponentsInChildren<Button>().Any(x=>x.name=="AJUSTES")&&game.GetComponentsInChildren<Text>().Any(x=>x.text=="EL\nDESPACHO"),"native title and menu buttons");Check(!TutorialManager.Instance.IsActive,"tutorial inactive on title");}
 IEnumerator Start(){restart=SessionState.GetBool("StartupRestart",false);output="Logs/Startup-v092"+(restart?"-restart":"")+".txt";Application.logMessageReceived+=Log;yield return null;yield return null;game=FindAnyObjectByType<DespachoGame>();menus=game.GetComponent<DeskMenus>();a=AccessController.Instance;yield return Tick();Title();
 if(restart){Check(AccessPreferences.Current.profileMask==5&&AccessPreferences.Current.simpleLanguage&&AccessPreferences.Current.textScale==1.5f,"chosen combined profiles preserved across Unity processes");Check(File.ReadAllText(AccessPreferences.FilePath)==File.ReadAllText("Logs/Startup-preferences-before-restart.txt"),"startup does not rewrite saved accessibility preferences");}
 else {Check(!AccessPreferences.Current.Reflow&&AccessPreferences.Current.profileMask==0,"fresh installation has neutral defaults");Check(!File.Exists(AccessPreferences.FilePath),"startup does not create selected preferences");menus.ShowSettings(4);yield return Tick();Check(a.CustomPage=="Inicio","settings opens accessibility only on request");a.ApplyProfile(0);a.ApplyProfile(2);yield return Tick();Check(AccessPreferences.Current.profileMask==5&&AccessPreferences.Current.textScale==1.5f,"explicit profile choices work");menus.ShowMain();yield return Tick();Title();}
 string preferences=JsonUtility.ToJson(AccessPreferences.Current);menus.Resume();game.StartFromMenu();yield return Tick();Check(JsonUtility.ToJson(AccessPreferences.Current)==preferences,"new campaign preserves accessibility choices");menus.SaveSlot(1);menus.LoadSlot(1);yield return Tick();Check(JsonUtility.ToJson(AccessPreferences.Current)==preferences,"existing campaign load preserves accessibility choices");game.ReturnToTitle();yield return Tick();Title();
 // Missing settings file must reset stale in-memory values, but never delete a real file.
 string actual=DeskPreferences.TestDirectory;DeskPreferences.TestDirectory=Path.Combine(actual,"empty");DeskPreferences.Current.highContrast=true;DeskPreferences.Load();Check(!DeskPreferences.Current.highContrast,"fresh preference load cannot inherit prior session contrast");DeskPreferences.TestDirectory=actual;DeskPreferences.Load();
 if(!restart){File.WriteAllText("Logs/Startup-test-directory.txt",actual);File.WriteAllText("Logs/Startup-preferences-before-restart.txt",File.ReadAllText(AccessPreferences.FilePath));}
 lines.Add("COMPLETE "+lines.Count(x=>x.StartsWith("PASS "))+" PASS - automated Unity Play Mode");File.WriteAllLines(output,lines);EditorApplication.Exit(0);
 }
}
public static class StartupValidationRunner {
 public static void Run(){SessionState.SetBool("StartupRestart",false);Begin(Path.Combine(Path.GetTempPath(),"Despacho092-"+Guid.NewGuid().ToString("N")));}
 public static void Restart(){SessionState.SetBool("StartupRestart",true);Begin(File.ReadAllText("Logs/Startup-test-directory.txt"));}
 static void Begin(string path){SessionState.SetBool("Startup092",true);SessionState.SetString("StartupDir092",path);EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("Startup092",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("StartupDir092","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("Startup092",false))return;SessionState.SetBool("Startup092",false);new GameObject("StartupValidation").AddComponent<StartupValidationProbe>();}
}
#endif
