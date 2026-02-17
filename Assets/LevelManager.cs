using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

    [Header("Level Buttons")]
    public Button[] levelButtons; // Array to hold your level buttons

   void Start()
    {
        // 1. Force the correct panels to show/hide immediately on load
        PlayerPrefs.DeleteAll();
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);

        // 2. Retrieve the highest unlocked level from disk
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);

        // 3. Loop through the buttons and lock unreached levels
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (i + 1 > levelReached)
            {
                levelButtons[i].interactable = false; 
            }
        }
    }

    // Call this from your Main Menu "Play" button
    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    // Call this from your Level Buttons, passing in the build index of that level
    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
}