using UnityEngine;

public class MineArea : MonoBehaviour
{
    [SerializeField] private int maxCount;
    public int MaxCount => maxCount;

    public int currentCount;

    void Awake()
    {
        currentCount = maxCount;
    }

    void Update()
    {
        if(currentCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
