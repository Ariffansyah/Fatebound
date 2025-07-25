using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI rollsLeftText;
    [SerializeField] private TextMeshProUGUI rollsCooldownText;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCombat playerCombat;

    void Update()
    {
        if (playerCombat != null)
        {
            healthText.text = $"Health: {playerCombat.CurrentHealth} / {playerCombat.MaxHealth}";
            staminaText.text = $"Stamina: {playerCombat.CurrentStamina} / {playerCombat.MaxStamina}";
        }

        if (playerController != null)
        {
            rollsLeftText.text = $"Rolls Left: {playerController.CurrentRolls} / {playerController.MaxRolls}";
            rollsCooldownText.text = $"Roll Cooldown: {playerController.RollTimer:F1}s / {playerController.RollCooldown:F1}s";
        }
    }
}
