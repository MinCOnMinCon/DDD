using TMPro;
using UnityEngine;

public class PlayerHpStageUI : MonoBehaviour
{
    private TextMeshProUGUI hpText;
    private Player player;

    private void Awake()
    {
        hpText = GetComponent<TextMeshProUGUI>();
        if (hpText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject.", this);
        }
    }

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.playerState.OnHpChanged += UpdateHpText;
            UpdateHpText(player.playerState.hp);
        }
        else
        {
            Debug.LogError("Player object not found in the scene for PlayerHpStageUI.", this);
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.playerState.OnHpChanged -= UpdateHpText;
        }
    }
    
    private void UpdateHpText(int newHp)
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {newHp}";
        }
    }
}
