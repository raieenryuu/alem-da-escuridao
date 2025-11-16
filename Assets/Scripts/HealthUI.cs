using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    public GameObject heartPrefab;
    public Transform heartsContainer;
    public PlayerHealth playerHealth;

    private List<GameObject> heartIcons = new List<GameObject>();

    void Start()
    {
        Debug.Log("HealthUI Start() called");
        Debug.Log("heartPrefab = " + heartPrefab);
        Debug.Log("heartContainer = " + heartsContainer);

        UpdateHearts();
        playerHealth.onHealthChanged += UpdateHearts;
    }

    void UpdateHearts()
    {
        foreach (var h in heartIcons)
            Destroy(h);
        heartIcons.Clear();

        for (int i = 0; i < playerHealth.maxHealth; i++)
        {
            Debug.Log("Instantiating heart #" + i);
            var heart = Instantiate(heartPrefab, heartsContainer);
            heartIcons.Add(heart);
            Debug.Log("Instantiated heart rect: " + heart.GetComponent<RectTransform>().anchoredPosition);

            if (i >= playerHealth.currentHealth)
            {
                heart.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
            }
        }
    }
}
