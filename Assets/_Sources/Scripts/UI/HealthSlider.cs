using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    public Image fillImage;
    public int maxHealth = 100;
    public int currentHealth;
    [SerializeField] private CharacterData _character;


    void Start()
    {
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateBar();
    }

    

    public void TakeDamage(int damage)
    {
        SetHealth(currentHealth - damage);
    }

    public void Heal(int amount)
    {
        SetHealth(currentHealth + amount);
    }

    void UpdateBar()
    {
        fillImage.fillAmount = (float)currentHealth / maxHealth;
    }
}


