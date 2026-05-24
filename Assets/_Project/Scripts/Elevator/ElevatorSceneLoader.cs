using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcosDelAzar.Elevator
{
    public static class ElevatorSceneLoader
    {
        public static string LastHubScene { get; private set; }

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

        public static void LoadMinigame(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            LastHubScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
        }

        public static void ReturnToHub()
        {
            if (!string.IsNullOrWhiteSpace(LastHubScene))
            {
                SceneManager.LoadScene(LastHubScene);
            }
        }
    }
}
