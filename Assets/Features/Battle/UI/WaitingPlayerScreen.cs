using UnityEngine;

public class WaitingPlayerScreen : MonoBehaviour
{
    void Start()
    {
        GameStateManager.Instance.onStartLoading += OnStartLoad;
        GameStateManager.Instance.onStartGamePlaying += OnStartGame;

        Hide();
    }

    void OnDestroy()
    {
        GameStateManager.Instance.onStartLoading -= OnStartLoad;
        GameStateManager.Instance.onStartGamePlaying -= OnStartGame;
    }

    private void OnStartLoad()
    {
        Show();

    }

    private void OnStartGame()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
