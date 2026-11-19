using System;
using System.IO;
using UnityEngine;
namespace ElDespacho {
 [Serializable] public sealed partial class AccessPreferences {
  public float textScale=1, interfaceScale=1, music=1, ambient=1, narrationVolume=.8f;
  public bool simpleDocuments, keyboard, largeTargets, independentColor, simpleLanguage, navigationHelp, symbols, glossary, narration, mono, softAudio, visualSounds, reduceFlashes;
  public bool readingView;
  public int navigationSchema;
  public int keyboardChoiceSchema; // 0 legacy/absent; 1 explicitly chosen; 2 legacy value retained.
  public void SetKeyboard(bool enabled){keyboard=enabled;keyboardChoiceSchema=1;}
  public int profileMask;public string profileSnapshot="";
  public int speechRate=0, repeatKey=6, stopKey=7;
  public static AccessPreferences Current {get;private set;}=new AccessPreferences();
  public static string FilePath=>Path.Combine(Path.GetDirectoryName(DeskPreferences.FilePath),"accessibility.json");
  public static string Error {get;private set;}="";
  public bool Reflow=>readingView; // Saved reading preference, never an automatic display trigger.
  public static void Load(){Current=new AccessPreferences();Error="";try{if(File.Exists(FilePath))JsonUtility.FromJsonOverwrite(File.ReadAllText(FilePath),Current);Current.Clamp();Current.MigrateNavigation();Current.AdoptIndividualPreferences();if(Current.keyboardChoiceSchema==0&&File.Exists(FilePath)&&System.Text.RegularExpressions.Regex.IsMatch(File.ReadAllText(FilePath),"\"keyboard\"\\s*:"))Current.keyboardChoiceSchema=2;}catch(Exception e){Error="No se pudieron leer las preferencias de accesibilidad: "+e.Message;}}
  // Preserve the glossary access supplied by symbols in older versions. Never open the reader.
  void MigrateNavigation(){
   if(navigationSchema>=1)return;
   glossary=glossary||symbols;
   navigationSchema=1;
  }
  public void Clamp(){textScale=Mathf.Clamp(textScale,.75f,1.75f);interfaceScale=Mathf.Clamp(interfaceScale,.8f,1.2f);music=Mathf.Clamp01(music);ambient=Mathf.Clamp01(ambient);narrationVolume=Mathf.Clamp01(narrationVolume);speechRate=Mathf.Clamp(speechRate,-5,5);repeatKey=Mathf.Clamp(repeatKey,6,8);stopKey=Mathf.Clamp(stopKey,6,8);if(stopKey==repeatKey)stopKey=repeatKey==8?6:repeatKey+1;}
  public bool Save(){try{navigationSchema=1;Clamp();AdoptIndividualPreferences();Directory.CreateDirectory(Path.GetDirectoryName(FilePath));string temp=FilePath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(this,true));if(File.Exists(FilePath))File.Replace(temp,FilePath,FilePath+".bak");else File.Move(temp,FilePath);Error="";return true;}catch(Exception e){Error="No se guardaron las preferencias: "+e.Message;return false;}}
  public static void Reset(){Current=new AccessPreferences();DeskPreferences.Current.highContrast=false;DeskPreferences.Current.reduceMotion=false;DeskPreferences.Current.menuTextScale=1;Current.Save();DeskPreferences.Current.Save();}
 }
}
