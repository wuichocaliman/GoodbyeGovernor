using System;
using UnityEngine;
namespace ElDespacho {
 public sealed partial class AccessPreferences {
  // Only fields owned by profiles participate. Campaign/audio levels and other options are excluded.
  public int profileSchema, profileOverrides;
  public float[] profileBase, profileApplied;
  public bool profileNarration, profilesMigrated;
  const int ProfileFieldCount=11;
  float[] ProfileValues()=>new[]{textScale,DeskPreferences.Current.highContrast?1f:0f,independentColor?1f:0f,largeTargets?1f:0f,simpleDocuments?1f:0f,simpleLanguage?1f:0f,navigationHelp?1f:0f,symbols?1f:0f,glossary?1f:0f,visualSounds?1f:0f,narration?1f:0f};
  void WriteProfileValues(float[] v){textScale=v[0];DeskPreferences.Current.highContrast=v[1]!=0;independentColor=v[2]!=0;largeTargets=v[3]!=0;simpleDocuments=v[4]!=0;simpleLanguage=v[5]!=0;navigationHelp=v[6]!=0;symbols=v[7]!=0;glossary=v[8]!=0;visualSounds=v[9]!=0;narration=v[10]!=0;}
  public void InitializeProfiles(){
   if(profileSchema==1&&profileBase!=null&&profileApplied!=null&&profileBase.Length==ProfileFieldCount&&profileApplied.Length==ProfileFieldCount){profileMask&=15;return;}
   // Legacy snapshots described the final configuration, never the pre-profile baseline.
   // Do not guess or reset intentional preferences. Adopt current effective values as custom base.
   profilesMigrated=profileMask!=0;profileSchema=1;profileMask=0;profileOverrides=0;profileSnapshot="";
   profileBase=ProfileValues();profileApplied=ProfileValues();
  }
  public bool HasProfileOverrides {get{if(profileOverrides!=0)return true;var v=ProfileValues();return HasExternalChanges(v);}}
  bool HasExternalChanges(float[] v){if(profileApplied==null||profileApplied.Length!=v.Length)return false;for(int i=0;i<v.Length;i++)if(!Mathf.Approximately(v[i],profileApplied[i]))return true;return false;}
  public void CaptureManualProfileChanges(){
   InitializeProfiles();var current=ProfileValues();
   for(int i=0;i<current.Length;i++)if(!Mathf.Approximately(current[i],profileApplied[i])){profileBase[i]=current[i];if(profileMask!=0)profileOverrides|=1<<i;}
   profileApplied=current;
  }
  public void ToggleProfile(int profile,bool voiceAvailable){
   if(profile<0||profile>3)return;
   CaptureManualProfileChanges();
   if(profileMask==0){profileBase=ProfileValues();profileOverrides=0;}
   bool activating=(profileMask&(1<<profile))==0;
   profileMask^=1<<profile;
   if(profile==3&&activating)profileNarration=voiceAvailable;
   float[] v=(float[])profileBase.Clone();
   // Union of boolean requirements; text never shrinks a larger custom base. Order-independent.
   if((profileMask&1)!=0){v[0]=Mathf.Max(v[0],1.5f);v[1]=v[2]=v[3]=1;}
   if((profileMask&2)!=0)v[3]=v[4]=1;
   if((profileMask&4)!=0)v[5]=v[6]=v[7]=v[8]=1;
   if((profileMask&8)!=0){v[9]=1;if(profileNarration)v[10]=1;}
   for(int i=0;i<v.Length;i++)if((profileOverrides&(1<<i))!=0)v[i]=profileBase[i];
   WriteProfileValues(v);profileApplied=ProfileValues();profilesMigrated=false;
   if(profileMask==0){profileBase=ProfileValues();profileOverrides=0;}
  }
 }
}

