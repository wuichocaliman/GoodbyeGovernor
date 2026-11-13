using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
namespace ElDespacho {
 // Offline Windows bridge; no shell interpolation of game text. One worker owns one synthesizer.
 public sealed class AccessSpeech : IDisposable {
  Process worker; public volatile bool Ready; public volatile int Accepted; public string Voice {get;private set;}=""; public string Last {get;private set;}=""; public string Error {get;private set;}="";
  public bool Running=>worker!=null&&!worker.HasExited;
  [Serializable] class Request {public string text;public int volume,rate;public bool stop;}
  const string Script=@"$ProgressPreference='SilentlyContinue'; $ErrorActionPreference='Stop'; [Console]::InputEncoding=New-Object System.Text.UTF8Encoding; [Console]::OutputEncoding=New-Object System.Text.UTF8Encoding; Add-Type -AssemblyName System.Speech; $s=New-Object System.Speech.Synthesis.SpeechSynthesizer; try { $v=$s.GetInstalledVoices() | Where-Object {$_.Enabled -and $_.VoiceInfo.Culture.Name -like 'es-*'} | Select-Object -First 1; if($v){$s.SelectVoice($v.VoiceInfo.Name)}; [Console]::WriteLine('VOICE|'+$s.Voice.Name+'|'+$s.Voice.Culture.Name); [Console]::WriteLine('READY'); while($null -ne ($line=[Console]::ReadLine())) { try { $r=$line|ConvertFrom-Json; $s.SpeakAsyncCancelAll(); if(-not $r.stop){ $s.Volume=[int]$r.volume; $s.Rate=[int]$r.rate; [void]$s.SpeakAsync([string]$r.text) }; [Console]::WriteLine('ACCEPTED') } catch {[Console]::Error.WriteLine($_.Exception.Message)} } } finally {$s.SpeakAsyncCancelAll();$s.Dispose()}";
  public void Probe(){Start();}
  bool Start(){
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
   try{if(Running)return true;var path=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),@"WindowsPowerShell\v1.0\powershell.exe");worker=new Process {StartInfo=new ProcessStartInfo(path,"-NoLogo -NoProfile -NonInteractive -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(Script))){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardError=true,RedirectStandardOutput=true}};worker.ErrorDataReceived+=(s,e)=>{if(!string.IsNullOrEmpty(e.Data))Error="Narración no disponible: "+e.Data;};worker.OutputDataReceived+=(s,e)=>{if(e.Data!=null && e.Data.StartsWith("VOICE|"))Voice=e.Data.Substring(6);if(e.Data=="READY")Ready=true;if(e.Data=="ACCEPTED")Accepted++;};worker.Start();worker.BeginErrorReadLine();worker.BeginOutputReadLine();return true;}catch(Exception e){Error="Narración no disponible: "+e.Message;return false;}
#else
   Error="La narración local requiere Windows y una voz instalada.";return false;
#endif
  }
  public void Say(string text,bool repeat=false){if(!AccessPreferences.Current.narration || string.IsNullOrWhiteSpace(text))return; if(!repeat && text==Last)return;Last=text;if(!Start())return;Send(new Request{text=text,volume=Mathf.RoundToInt(AccessPreferences.Current.narrationVolume*DeskPreferences.Current.master*100),rate=AccessPreferences.Current.speechRate});}
  void Send(Request r){try{if(Running){worker.StandardInput.WriteLine(JsonUtility.ToJson(r));worker.StandardInput.Flush();}}catch(Exception e){Error="Se interrumpió la narración: "+e.Message;}}
  public void Repeat()=>Say(Last,true);
  public void Stop(){Send(new Request{stop=true});}
  public void Dispose(){try{if(Running){Stop();worker.StandardInput.Close();if(!worker.WaitForExit(400))worker.Kill();}worker?.Dispose();}catch{}worker=null;Ready=false;}
 }
 // Attach per source. No Unity API calls on the audio thread.
 public sealed class AccessAudioFilter : MonoBehaviour {
  volatile bool mono,soft; float[] previous=new float[8];
  void Update(){mono=AccessPreferences.Current.mono;soft=AccessPreferences.Current.softAudio;}
  void OnAudioFilterRead(float[] data,int channels){Process(data,channels,mono,soft,previous);}
  public static void Process(float[] data,int channels,bool mono,bool soft,float[] previous){if(channels<1)return;for(int i=0;i+channels<=data.Length;i+=channels){float sum=0;for(int c=0;c<channels;c++)sum+=data[i+c];for(int c=0;c<channels;c++){float v=mono?sum/channels:data[i+c];if(soft&&c<previous.Length){previous[c]+=.25f*(v-previous[c]);v=previous[c];}data[i+c]=v;}}}
 }
}
