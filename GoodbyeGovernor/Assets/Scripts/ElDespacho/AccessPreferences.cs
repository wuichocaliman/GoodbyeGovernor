using System;
using System.IO;
using UnityEngine;
namespace ElDespacho {
 [Serializable] public sealed class AccessPreferences {
  public float textScale=1, interfaceScale=1, music=1, ambient=1, narrationVolume=.8f;
  public bool simpleDocuments, keyboard, largeTargets, independentColor, simpleLanguage, navigationHelp, symbols, glossary, narration, mono, softAudio, visualSounds, reduceFlashes;
  public bool readingView;
  public int profileMask;public string profileSnapshot="";
  public int speechRate=0, repeatKey=6, stopKey=7;
  public static AccessPreferences Current {get;private set;}=new AccessPreferences();
  public static string FilePath=>Path.Combine(Path.GetDirectoryName(DeskPreferences.FilePath),"accessibility.json");
  public static string Error {get;private set;}="";
  public bool Reflow=>readingView || textScale!=1 || interfaceScale!=1 || DeskPreferences.Current.highContrast || keyboard || largeTargets || independentColor || navigationHelp || symbols || glossary || simpleLanguage || visualSounds;
  public static void Load(){Current=new AccessPreferences();Error="";try{if(File.Exists(FilePath))JsonUtility.FromJsonOverwrite(File.ReadAllText(FilePath),Current);Current.Clamp();}catch(Exception e){Error="No se pudieron leer las preferencias de accesibilidad: "+e.Message;}}
  public void Clamp(){textScale=Mathf.Clamp(textScale,.75f,1.75f);interfaceScale=Mathf.Clamp(interfaceScale,.8f,1.2f);music=Mathf.Clamp01(music);ambient=Mathf.Clamp01(ambient);narrationVolume=Mathf.Clamp01(narrationVolume);speechRate=Mathf.Clamp(speechRate,-5,5);repeatKey=Mathf.Clamp(repeatKey,6,8);stopKey=Mathf.Clamp(stopKey,6,8);if(stopKey==repeatKey)stopKey=repeatKey==8?6:repeatKey+1;}
  public bool Save(){try{Clamp();Directory.CreateDirectory(Path.GetDirectoryName(FilePath));string temp=FilePath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(this,true));if(File.Exists(FilePath))File.Replace(temp,FilePath,FilePath+".bak");else File.Move(temp,FilePath);Error="";return true;}catch(Exception e){Error="No se guardaron las preferencias: "+e.Message;return false;}}
  public static void Reset(){Current=new AccessPreferences();DeskPreferences.Current.highContrast=false;DeskPreferences.Current.reduceMotion=false;DeskPreferences.Current.menuTextScale=1;Current.Save();DeskPreferences.Current.Save();}
 }
}
