using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    private bool _isPaused = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_pauseMenu != null)
        {
            _pauseMenu.SetActive(false);
        }

        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        }
        
    }
    
    private void OnDestroy()
    {
        // Отписываемся при уничтожении объекта
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
        }
    }

    // Update is called once per frame
    private void GameInput_OnPauseAction(object sender, System.EventArgs e)
    {
        if (_isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        if (_pauseMenu != null) _pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        _isPaused = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Pause()
    {
        if (_pauseMenu != null) _pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        _isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
