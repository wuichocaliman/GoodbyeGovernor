#if UNITY_EDITOR
using System;using System.IO;using System.Collections;using UnityEditor;using UnityEngine;using ElDespacho;
// Launch existing document/gamepad regressions with isolated stores before game initialization.
public class ControlsExistingProbe:MonoBehaviour {
 IEnumerator Start(){yield return null;yield return null;bool pad=SessionState.GetBool("ControlsRegressionPad",false);bool had=PlayerPrefs.HasKey("eldespacho_vibracion");int value=PlayerPrefs.GetInt("eldespacho_vibracion",1);bool error=false;Application.LogCallback handler=(s,st,t)=>{if(t==LogType.Error||t==LogType.Exception){error=true;EditorApplication.isPaused=false;}};Application.logMessageReceived+=handler;
 var settings=Instantiate(UnityEngine.InputSystem.InputSystem.settings);settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;UnityEngine.InputSystem.InputSystem.settings=settings;
 DeskPreferences.Current.tutorial=false;DeskPreferences.Current.autosave=false;AccessPreferences.Reset();MonoBehaviour probe=pad?(MonoBehaviour)new GameObject("Gamepad regression").AddComponent<GamepadValidationProbe>():new GameObject("Document regression").AddComponent<DeskValidationProbe>();float until=Time.realtimeSinceStartup+120;while(probe!=null&&!error&&Time.realtimeSinceStartup<until)yield return null;bool complete=probe==null&&!error;if(probe!=null){Destroy(probe.gameObject);yield return null;}
 if(pad){if(had)PlayerPrefs.SetInt("eldespacho_vibracion",value);else PlayerPrefs.DeleteKey("eldespacho_vibracion");PlayerPrefs.Save();}Application.logMessageReceived-=handler;File.WriteAllText(pad?"Logs/Controls-gamepad-preservation.txt":"Logs/Controls-documents-run.txt",complete?"PASS existing regression completed; isolated stores; vibration key restored exactly":"FAIL existing regression");EditorApplication.Exit(complete?0:1);
 }
}
public static class ControlsExistingRunner {
 public static void Documents(){SessionState.SetBool("ControlsRegressionPad",false);Run();}
 public static void Gamepad(){SessionState.SetBool("ControlsRegressionPad",true);Run();}
 static void Run(){SessionState.SetBool("ControlsExisting",true);SessionState.SetString("ControlsExistingDir",Path.Combine(Path.GetTempPath(),"DespachoControlsRegression-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(SessionState.GetBool("ControlsExisting",false))DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory=SessionState.GetString("ControlsExistingDir","");}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("ControlsExisting",false))return;SessionState.SetBool("ControlsExisting",false);new GameObject("ControlsExistingProbe").AddComponent<ControlsExistingProbe>();}
}
#endif
