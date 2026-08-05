using UnityEngine;

public class HealthManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private Transform currentBar;

    public void SetHealth(float currentHealthPoint, float maxHealthPoint)
    {
        float ratio = maxHealthPoint > 0f
            ? Mathf.Clamp01(currentHealthPoint / maxHealthPoint)
            : 0f;

        currentBar.localScale = new Vector3(ratio, 1f, 1f);
    }
}
