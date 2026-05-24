using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcosDelAzar.Elevator
{
    public static class ElevatorSceneLoader
    {
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
    }
}
