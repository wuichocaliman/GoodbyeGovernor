using System;
using System.IO;
using UnityEngine;

namespace ElDespacho
{
    [Serializable] public sealed class DeskPreferences
    {
        public float master=0.8f, effects=1f, ui=0.7f, brightness=1f;
        public bool fullscreen=true, vsync=true, tutorial=true, autosave=true, reduceMotion=false, highContrast=false;
        public int width=1920, height=1080, fps=60, autosaveMinutes=3, dragThreshold=8;
        public float menuTextScale=1f;
        public static DeskPreferences Current { get; private set; } = new DeskPreferences();
        public static bool Paused { get; internal set; }
        public static float GameDelta => Paused ? 0 : Time.unscaledDeltaTime;
        public static float AnimationDuration(float duration) => Current.reduceMotion ? Mathf.Min(duration,0.03f) : duration;
        #if UNITY_EDITOR
        public static string TestDirectory;
#endif
        public static string FilePath {
            get {
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(TestDirectory)) return Path.Combine(TestDirectory, "settings.json");
#endif
                return Path.Combine(Application.persistentDataPath,"ElDespacho","settings.json");
            }
        }
        public static string Load()
        {
            string error=null;
            try { if(File.Exists(FilePath)) Current=JsonUtility.FromJson<DeskPreferences>(File.ReadAllText(FilePath)) ?? new DeskPreferences(); }
            catch(Exception e) when(e is IOException || e is ArgumentException || e is UnauthorizedAccessException) { Current=new DeskPreferences(); error=e.Message; }
            Current.Clamp(); Current.Apply(false); return error;
        }
        public static void Reset() { Current=new DeskPreferences(); Current.Clamp(); Current.Apply(false); }
        void Clamp()
        {
            master=Mathf.Clamp01(master); effects=Mathf.Clamp01(effects); ui=Mathf.Clamp01(ui);
            brightness=Mathf.Clamp(brightness,0.65f,1.15f); menuTextScale=Mathf.Clamp(menuTextScale,1f,1.2f);
            width=Mathf.Clamp(width,800,7680); height=Mathf.Clamp(height,600,4320);
            fps=Mathf.Clamp(fps,30,240); autosaveMinutes=Mathf.Clamp(autosaveMinutes,1,15); dragThreshold=Mathf.Clamp(dragThreshold,0,30);
        }
        public void Save()
        {
            Clamp(); Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            string temp=FilePath+".tmp"; File.WriteAllText(temp,JsonUtility.ToJson(this,true));
            if(File.Exists(FilePath)) File.Replace(temp,FilePath,FilePath+".bak"); else File.Move(temp,FilePath);
        }
        public void Apply(bool display)
        {
            Clamp(); AudioListener.volume=master;
            QualitySettings.vSyncCount=vsync?1:0; Application.targetFrameRate=fps;
            if(UnityEngine.EventSystems.EventSystem.current!=null) UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold=dragThreshold;
            if(display && !Application.isEditor) Screen.SetResolution(width,height,fullscreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
        }
    }
}
