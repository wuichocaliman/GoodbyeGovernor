#if UNITY_EDITOR
using System;using System.IO;using System.Collections;using UnityEngine;using UnityEditor;using ElDespacho;
public sealed class AccessibilityUxVisual:MonoBehaviour {
 IEnumerator Start(){yield return null;yield return null;DeskPreferences.Current.autosave=false;AccessController.Instance.ShowSettings();}
}
public static class AccessibilityUxVisualRunner {
 public static void Run(){SessionState.SetBool("UxVisual091",true);SessionState.SetString("UxVisualDir091",Path.Combine(Path.GetTempPath(),"Despacho091Visual-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("UxVisual091",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("UxVisualDir091","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("UxVisual091",false))return;SessionState.SetBool("UxVisual091",false);new GameObject("AccessibilityUxVisual").AddComponent<AccessibilityUxVisual>();}
}
#endif
