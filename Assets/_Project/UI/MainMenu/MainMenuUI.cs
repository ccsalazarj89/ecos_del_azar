using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using EcosDelAzar.Core;

namespace EcosDelAzar.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] string firstSceneName = "SCN_Floor_00_Lobby";

        Button playButton;
        Button quitButton;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            playButton = doc.rootVisualElement.Q<Button>("PlayButton");
            quitButton = doc.rootVisualElement.Q<Button>("QuitButton");

            if (playButton != null) playButton.clicked += Play;
            if (quitButton != null) quitButton.clicked += Quit;
        }

        void OnDisable()
        {
            if (playButton != null) playButton.clicked -= Play;
            if (quitButton != null) quitButton.clicked -= Quit;
        }

        void Play()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);

            SceneManager.LoadScene(firstSceneName);
        }

        void Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
