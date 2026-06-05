using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Arkham.Core
{
    /// <summary>
    /// Small reusable navigation helper. Drop it on a GameObject in any scene,
    /// assign whichever buttons that scene has, and it subscribes them in Awake.
    /// Scene names are configurable so we don't hard-code strings across the project.
    /// </summary>
    public class SceneNav : MonoBehaviour
    {
        [Tooltip("Name of the main-menu scene file (no path, no .unity extension).")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Tooltip("Name of the gameplay scene file (no path, no .unity extension).")]
        [SerializeField] private string gameSceneName = "SampleScene";

        [Header("Buttons (assign the ones this scene has)")]
        [Tooltip("Main-menu Play button -> loads the game scene.")]
        [SerializeField] private Button playButton;

        [Tooltip("Main-menu Exit button -> quits the game.")]
        [SerializeField] private Button exitButton;

        [Tooltip("End-game 'Main Menu' buttons -> load the main menu. Can be several (one per panel).")]
        [SerializeField] private Button[] mainMenuButtons;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(LoadGameScene);
            if (exitButton != null) exitButton.onClick.AddListener(QuitGame);
            if (mainMenuButtons != null)
            {
                foreach (var b in mainMenuButtons)
                    if (b != null) b.onClick.AddListener(LoadMainMenu);
            }
        }

        /// <summary>Load the gameplay scene (the Play button).</summary>
        public void LoadGameScene()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>Load the main-menu scene (the end-game buttons).</summary>
        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>Quit the game (stops Play mode in the Editor).</summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            // Application.Quit() does nothing in the Editor, so stop Play mode instead.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
