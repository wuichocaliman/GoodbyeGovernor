#if UNITY_EDITOR
using System;using System.IO;using System.Collections;using UnityEngine;using UnityEditor;using ElDespacho;
public sealed class ReadingViewVisualProbe:MonoBehaviour {
 IEnumerator Start(){yield return null;yield return null;DeskPreferences.Current.tutorial=false;DeskPreferences.Current.autosave=false;var p=AccessPreferences.Current;p.keyboard=true;p.textScale=1.25f;p.symbols=true;p.navigationHelp=true;p.simpleLanguage=true;p.simpleDocuments=true;p.readingView=true;AccessController.Instance.Refresh();}
}
public static class ReadingViewVisualRunner {
 public static void Run(){SessionState.SetBool("ReaderVisual093",true);SessionState.SetString("ReaderVisualDir093",Path.Combine(Path.GetTempPath(),"Despacho093Visual-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 static bool Explicit=>Environment.CommandLine.Contains("-executeMethod ReadingViewVisualRunner.Run");
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!Explicit||!SessionState.GetBool("ReaderVisual093",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("ReaderVisualDir093","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!Explicit||!SessionState.GetBool("ReaderVisual093",false))return;SessionState.SetBool("ReaderVisual093",false);new GameObject("ReadingViewVisual").AddComponent<ReadingViewVisualProbe>();}
}
#endif
