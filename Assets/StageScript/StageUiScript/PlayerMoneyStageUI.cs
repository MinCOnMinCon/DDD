using TMPro;
using UnityEngine;

public class PlayerMoneyStageUI : MonoBehaviour
{
    private TextMeshProUGUI moneyText;
    private Player player;

    private void Awake()
    {
        moneyText = GetComponent<TextMeshProUGUI>();
        if (moneyText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject.", this);
        }
    }

    private void Start()
    {
        // 요청하신 대로 FindFirstObjectByType을 사용하여 Player를 찾습니다.
        player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.playerState.OnMoneyChanged += UpdateMoneyText;
            UpdateMoneyText(player.playerState.money);
        }
        else
        {
            Debug.LogError("PlayerMoneyStageUI가 씬에서 Player 오브젝트를 찾을 수 없습니다.", this);
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.playerState.OnMoneyChanged -= UpdateMoneyText;
        }
    }
    
    private void UpdateMoneyText(int newMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: {newMoney}";
        }
    }
}
