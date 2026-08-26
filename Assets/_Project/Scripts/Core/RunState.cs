using UnityEngine;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Lifecycle of a run (one attempt at the casino). A run starts from the main
    /// menu, is saved continuously through <see cref="RunPrefs"/>, and ends on
    /// death or on beating the boss. The main menu offers "Continuar" only while
    /// a run exists.
    /// </summary>
    public static class RunState
    {
        const string ActiveKey = "active";
        const string SceneKey = "scene";
        const string DefaultScene = "SCN_Floor_00_Lobby";

        public static bool Exists => RunPrefs.GetInt(ActiveKey, 0) == 1;

        /// <summary>Hub scene to load when continuing. Updated every time a floor is entered.</summary>
        public static string CurrentScene => RunPrefs.GetString(SceneKey, DefaultScene);

        /// <summary>Wipes any previous run and resets coins, oxygen and floors to their starting values.</summary>
        public static void StartNew()
        {
            RunPrefs.DeleteAll();
            RunPrefs.SetInt(ActiveKey, 1);
            RunPrefs.SetString(SceneKey, DefaultScene);
            RunPrefs.Save();

            GameManager.Instance?.ResetRunValues();
            Debug.Log("[RunState] New run started.");
        }

        /// <summary>Ends the run (death or victory). "Continuar" disappears from the menu.</summary>
        public static void Clear()
        {
            RunPrefs.DeleteAll();
            Debug.Log("[RunState] Run cleared.");
        }

        public static void MarkScene(string sceneName)
        {
            if (!Exists || string.IsNullOrEmpty(sceneName)) return;
            RunPrefs.SetString(SceneKey, sceneName);
            RunPrefs.Save();
        }
    }
}
