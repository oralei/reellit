using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HealthHeartBar : MonoBehaviour
{
    public GameObject heartPrefab;
    public FishMovement mainFish;
    private int maxHealth;
    List<FishHealthManager> hearts = new List<FishHealthManager>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrawHearts();
    }

    public void DrawHearts()
    {
        clearHearts();

        // make x hearts based on max health (capture attempts dependant on fish)
        maxHealth = mainFish.fishData.captureAttempts;
        int heartsToMake = maxHealth;

        for (int i = 0; i < heartsToMake; i++)
        {
            CreateEmptyHeart();
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartStatus = (mainFish.currentHealth > i) ? 1 : 0;  // Full if health > current heart index, empty otherwise
            hearts[i].SetHeartImage((HeartStatus)heartStatus);
        }
    }

    public void CreateEmptyHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab);
        newHeart.transform.SetParent(transform);

        FishHealthManager heartComponent = newHeart.GetComponent<FishHealthManager>();
        heartComponent.SetHeartImage(HeartStatus.Empty);
        hearts.Add(heartComponent);
    }

    public void clearHearts()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        hearts = new List<FishHealthManager>();
    }
}
