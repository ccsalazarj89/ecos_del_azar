using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcosDelAzar.Elevator
{
    public static class ElevatorSceneLoader
    {
        public static string LastHubScene { get; private set; }

        /// <summary>Table the player is currently seated at (set when entering a minigame).</summary>
        public static string CurrentTableId { get; private set; }
        public static int CurrentTableFloor { get; private set; }
        public static bool CurrentTableAwardsChip { get; private set; }

        static string returnedTableId;

        /// <summary>Table the player just left, read once by PlayerSeating to spawn on its chair.</summary>
        public static string ConsumeReturnedTableId()
        {
            string id = returnedTableId;
            returnedTableId = null;
            return id;
        }

        public static bool IsCurrentScene(ElevatorFloorData floor)
        {
            if (floor == null) return false;
            return SceneManager.GetActiveScene().name == floor.SceneName;
        }

        public static void LoadFloor(ElevatorFloorData floor)
        {
            if (floor == null || string.IsNullOrWhiteSpace(floor.SceneName)) return;
            if (IsCurrentScene(floor)) return;
            SceneManager.LoadScene(floor.SceneName);
        }

        public static void LoadMinigame(string sceneName, string tableId, int tableFloor, bool awardsChip = true)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            LastHubScene = SceneManager.GetActiveScene().name;
            CurrentTableId = tableId;
            CurrentTableFloor = tableFloor;
            CurrentTableAwardsChip = awardsChip;
            SceneManager.LoadScene(sceneName);
        }

        public static void ReturnToHub()
        {
            if (string.IsNullOrWhiteSpace(LastHubScene)) return;
            returnedTableId = CurrentTableId;
            SceneManager.LoadScene(LastHubScene);
        }
    }
}
