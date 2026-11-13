#if UNITY_EDITOR
using System;using System.IO;using System.Collections;using UnityEngine;using UnityEditor;using ElDespacho;
public class AccessibilityVisualProbe:MonoBehaviour {
 UnityEngine.InputSystem.InputSettings original;
 void OnDisable(){if(original!=null)UnityEngine.InputSystem.InputSystem.settings=original;}
 IEnumerator Start(){yield return null;yield return null;original=UnityEngine.InputSystem.InputSystem.settings;var inputSettings=Instantiate(original);inputSettings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;inputSettings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;UnityEngine.InputSystem.InputSystem.settings=inputSettings;var game=FindAnyObjectByType<DespachoGame>();DeskPreferences.Current.autosave=false;AccessPreferences.Current.textScale=1.75f;AccessPreferences.Current.interfaceScale=1.2f;AccessPreferences.Current.keyboard=true;AccessPreferences.Current.simpleDocuments=true;AccessPreferences.Current.symbols=true;AccessPreferences.Current.simpleLanguage=true;DeskPreferences.Current.highContrast=true;game.GetComponent<DeskMenus>().Resume();game.StartFromMenu();AccessController.Instance.Refresh();}
}
public static class AccessibilityVisualRunner {
 static bool ExplicitLaunch=>Environment.CommandLine.Contains("-executeMethod AccessibilityVisualRunner.Run");
 public static void Run(){SessionState.SetBool("AccessVisual09",true);SessionState.SetString("AccessVisualDir09",Path.Combine(Path.GetTempPath(),"Despacho09Visual-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!ExplicitLaunch)return;if(!SessionState.GetBool("AccessVisual09",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("AccessVisualDir09","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!ExplicitLaunch)return;if(!SessionState.GetBool("AccessVisual09",false))return;SessionState.SetBool("AccessVisual09",false);new GameObject("AccessVisual").AddComponent<AccessibilityVisualProbe>();}
}
#endif
