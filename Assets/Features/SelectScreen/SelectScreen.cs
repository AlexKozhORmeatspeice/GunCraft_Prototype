using UnityEngine;
using UnityEngine.UI;

public class SelectScreen : MonoBehaviour
{
    [SerializeField] private Button readyBtn;

    private void Awake()
    {
        readyBtn.onClick.AddListener(() =>
        {
            CharSelectManager.Instance.SetPlayerReady();
        });
    }
}
