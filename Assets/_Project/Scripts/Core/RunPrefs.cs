using System.Collections.Generic;
using UnityEngine;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// PlayerPrefs scoped to the current run. Every key is stored under the
    /// "run." prefix and tracked in an index, so <see cref="DeleteAll"/> can wipe
    /// the whole run without touching user preferences (UI positions, volume...).
    /// Use this for anything that must reset on death or on "new game".
    /// </summary>
    public static class RunPrefs
    {
        const string Prefix = "run.";
        const string IndexKey = "run.__keys";

        public static bool HasKey(string key) => PlayerPrefs.HasKey(Prefix + key);

        public static int GetInt(string key, int fallback = 0) => PlayerPrefs.GetInt(Prefix + key, fallback);
        public static float GetFloat(string key, float fallback = 0f) => PlayerPrefs.GetFloat(Prefix + key, fallback);
        public static string GetString(string key, string fallback = "") => PlayerPrefs.GetString(Prefix + key, fallback);

        public static void SetInt(string key, int value) { PlayerPrefs.SetInt(Prefix + key, value); Track(key); }
        public static void SetFloat(string key, float value) { PlayerPrefs.SetFloat(Prefix + key, value); Track(key); }
        public static void SetString(string key, string value) { PlayerPrefs.SetString(Prefix + key, value); Track(key); }

        public static void Save() => PlayerPrefs.Save();

        /// <summary>Removes every run-scoped key. Preferences outside the "run." prefix are untouched.</summary>
        public static void DeleteAll()
        {
            foreach (var key in ReadIndex())
                PlayerPrefs.DeleteKey(Prefix + key);

            PlayerPrefs.DeleteKey(IndexKey);
            PlayerPrefs.Save();
        }

        static void Track(string key)
        {
            var keys = ReadIndex();
            if (keys.Contains(key)) return;
            keys.Add(key);
            PlayerPrefs.SetString(IndexKey, string.Join("|", keys));
        }

        static List<string> ReadIndex()
        {
            string raw = PlayerPrefs.GetString(IndexKey, "");
            return string.IsNullOrEmpty(raw) ? new List<string>() : new List<string>(raw.Split('|'));
        }
    }
}
