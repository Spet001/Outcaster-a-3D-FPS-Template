using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Flow")]
    [SerializeField] private string defaultSceneName = "DefaultScene";

    [Header("UI References")]
    [SerializeField] private GameObject optionsPanel;

    // Called via UI Button: loads the default gameplay scene.
    public void OnNewGame()
    {
        if (string.IsNullOrWhiteSpace(defaultSceneName))
        {
            Debug.LogError("Default scene name not configured on MainMenuManager.");
            return;
        }

        SceneManager.LoadScene(defaultSceneName);
    }

    // Placeholder for future save/load system.
    public void OnLoadGame()
    {
        Debug.LogWarning("Load Game not implemented yet – coming soon.");
    }

    // Basic options toggle so designers can hook up a simple panel.
    public void ToggleOptions(bool show)
    {
        if (optionsPanel == null)
        {
            Debug.LogWarning("Options panel not assigned on MainMenuManager.");
            return;
        }

        optionsPanel.SetActive(show);
    }
}
