using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ElDespacho
{
    // Versioned envelope; never deserialize over live game objects.
    public static class DeskSaveStore
    {
        [Serializable] class Envelope { public int format = 1; public string payload, sha256; }
        #if UNITY_EDITOR
        public static string TestDirectory;
#endif
        public static string DirectoryPath {
            get {
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(TestDirectory)) return Path.Combine(TestDirectory, "Saves");
#endif
                return Path.Combine(Application.persistentDataPath, "ElDespacho", "Saves");
            }
        }
        static string PathFor(int slot)
        {
            if (slot < 0 || slot > 3) throw new ArgumentOutOfRangeException(nameof(slot));
            return Path.Combine(DirectoryPath, "slot-" + slot + ".json");
        }
        static string Hash(string text)
        {
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
        public static void Write(int slot, DespachoGame.SaveData data)
        {
            string path = PathFor(slot);
            Directory.CreateDirectory(DirectoryPath);
            string payload = JsonUtility.ToJson(data);
            string json = JsonUtility.ToJson(new Envelope { payload = payload, sha256 = Hash(payload) }, true);
            string temp = path + ".tmp";
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
            else File.Move(temp, path);
        }
        public static bool TryRead(int slot, out DespachoGame.SaveData data, out string error)
        {
            data = null; error = null;
            string path = PathFor(slot);
            if (!File.Exists(path)) { error = "Vacío"; return false; }
            try
            {
                var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(path));
                if (envelope == null || envelope.format != 1 || string.IsNullOrEmpty(envelope.payload) || Hash(envelope.payload) != envelope.sha256)
                    throw new InvalidDataException("Formato o integridad incorrectos");
                data = JsonUtility.FromJson<DespachoGame.SaveData>(envelope.payload);
                DespachoGame.ValidateSave(data);
                return true;
            }
            catch (Exception e) when (e is IOException || e is ArgumentException || e is InvalidOperationException || e is UnauthorizedAccessException)
            { error = "No disponible: " + e.Message; data = null; return false; }
        }
        public static int LatestSlot()
        {
            int latest = -1; long ticks = 0;
            for (int i = 0; i <= 3; i++)
                if (TryRead(i, out var s, out _) && s.utcTicks > ticks) { latest = i; ticks = s.utcTicks; }
            return latest;
        }
    }
}
