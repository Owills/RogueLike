using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
    {

    public Slider slider;
    public Color Low; 
    public Color High; 
    public Vector3 Offset; 
    void Start()
    {
    }
    void Update()
    {
        slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
        // new Vector3(0f, 10f, 0f));
    }
    public void SetHealth(float health, float maxHealth) {
        slider.gameObject.SetActive(health < maxHealth);
        slider.value = health;
        slider.maxValue = maxHealth;

        slider.fillRect.GetComponentInChildren<Image>().color =
                Color.Lerp(Low, High, slider.normalizedValue);
    }
}
