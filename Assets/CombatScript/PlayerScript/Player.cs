using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using Unity.Collections;
using UnityEngine;
using static Player;

public class Player : MonoBehaviour, ICombatHook, ICombatContextProvider
{

    [SerializeField]
    private PlayerInitValue piv;
    private DiceManager diceM;
    private RelicManager relicM;
    [SerializeField]
    private Dictionary<CombatPhase, int> orders;
    
    public PlayerState playerState { get; private set; }

    private void Awake()
    {
        playerState = new PlayerState(piv);
    }
    private void Start()
    {
        CombatManager.inst.HookRegister(this);
        ApplyTo(CombatManager.inst.combatContext);

    }

    public void TakeDamage(int damage, CombatContext ctx)
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

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.valueSubmit:
                //ctx에서 적 공격 수치 읽어서 takedamage 함수 호출
                break;
        }
    }
    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }

    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = true ? phase == CombatPhase.turnEnd : false;
        return canExecute;

    }
    
    public void ApplyTo(CombatContext ctx)
    {
        ctx.playerState = playerState;
    }
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