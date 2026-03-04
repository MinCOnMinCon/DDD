using TMPro;
using UnityEngine;

public class PlayerShopGaugeStageUI : MonoBehaviour
{
    private TextMeshProUGUI shopGaugeText;
    private Player player;

    private void Awake()
    {
        shopGaugeText = GetComponent<TextMeshProUGUI>();
        if (shopGaugeText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject.", this);
        }
    }

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.playerState.OnShopGaugeChanged += UpdateShopGaugeText;
            UpdateShopGaugeText(player.playerState.shopGauge);
        }
        else
        {
            Debug.LogError("Player object not found in the scene for PlayerShopGaugeStageUI.", this);
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.playerState.OnShopGaugeChanged -= UpdateShopGaugeText;
        }
    }
    
    private void UpdateShopGaugeText(int newShopGauge)
    {
        if (shopGaugeText != null)
        {
            shopGaugeText.text = $"Shop Gauge: {newShopGauge}";
        }
    }
}
