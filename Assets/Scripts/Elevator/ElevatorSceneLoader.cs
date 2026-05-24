using UnityEngine;
using UnityEngine.SceneManagement;

public static class ElevatorSceneLoader
{
    public static bool IsCurrentScene(ElevatorFloorData floor)
    {
        if (floor == null)
        {
            return false;
        }

        Scene activeScene = SceneManager.GetActiveScene();

        return activeScene.name == floor.SceneName;
    }

    public static void LoadFloor(ElevatorFloorData floor)
    {
        if (floor == null)
        {
            Debug.LogWarning("Cannot load floor because floor data is null.");
            return;
        }

        if (string.IsNullOrWhiteSpace(floor.SceneName))
        {
            Debug.LogWarning($"Cannot load floor '{floor.DisplayName}' because scene name is empty.");
            return;
        }

        if (IsCurrentScene(floor))
        {
            Debug.Log($"Already in scene: {floor.SceneName}");
            return;
        }

        SceneManager.LoadScene(floor.SceneName);
    }
}