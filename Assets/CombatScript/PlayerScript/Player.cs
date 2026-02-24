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
    private Dictionary<CombatPhase, int> orders;
    
    public PlayerState playerState { get; private set; }

    private void Awake()
    {
        playerState = new PlayerState(piv);
        orders = new Dictionary<CombatPhase, int>();
        orders[CombatPhase.valueConfirm] = 0;
        orders[CombatPhase.combatStart] = 1;
    }
    private void Start()
    {
        CombatManager.inst.HookRegister(this);
    }
    public void TakeDamage(CombatContext ctx)
    {
        int playerDefVal = ctx.snapshot.baseDefenseValue + ctx.snapshot.calcDefenseValue;
        ctx.playerState.hp += playerDefVal - ctx.enemyState.currentAttackValue;
        if (ctx.playerState.hp <= 0)
        {
            Die();
        }

    }
    
    private void Die()
    {

    }
    

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.valueConfirm:
                TakeDamage(ctx);
                break;
            case CombatPhase.combatStart:
                
                ApplyTo(CombatManager.inst.combatContext);
                break;
        }
    }
    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }

    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = orders.ContainsKey(phase);
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
    public PlayerState(PlayerState other)
    {
        maxHp = other.maxHp;
        hp = other.hp;
        money = other.money;
        savingMaximumDice = other.savingMaximumDice;
        savingMaximumValue = other.savingMaximumValue;

    }
}