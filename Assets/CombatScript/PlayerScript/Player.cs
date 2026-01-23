using System;
using System.Data.SqlTypes;
using UnityEngine;
using static Player;

public class Player : MonoBehaviour
{

    [SerializeField]
    private PlayerInitValue piv;
    private DiceManager diceM;
    private RelicManager relicM;
    
    public PlayerState playerState { get; private set; }

    private void Awake()
    {
        playerState = new PlayerState(piv);
    }
    
    public void TakeDamage(int damage)
    {
        playerState.hp -= damage;
        if (playerState.hp <= 0)
        {
            OnPlayerDied?.Invoke();
        }
        OnPlayerDamaged?.Invoke();
    }

    public event Action OnPlayerDied;
    public event Action OnPlayerDamaged;

}

public class PlayerState
{
    public int maxHp { set; private get; }
    private int _hp;
    public int hp
    {
        get => _hp;
        set => _hp = Mathf.Max(0, value);
    }
    private int _money;
    public int money
    {
        get => _money;
        set => _money = Mathf.Max(0, value);
    }

    public int savingMaximumValue { get; set; }
    public int savingMaximumDice { get; set; }

    public PlayerState(PlayerInitValue piv)
    {
        maxHp = piv.maxHp;
        hp = maxHp;
        money = piv.money;
        savingMaximumDice = piv.savingMaximumDice;
        savingMaximumValue = piv.savingMaximumValue;

    }
}