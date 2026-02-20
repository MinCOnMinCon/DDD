using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, ICombatHook, ICombatContextProvider
{
    [SerializeField]
    private EnemyData data; // 스크립터블로 직접 넣기
    
    private EnemyAction action;
    private Dictionary<CombatPhase, int> orders;
    

    private void Awake()
    {
        Initialize();
    }
    private void Start()
    {
        CombatManager.inst.HookRegister(this);
        ApplyTo(CombatManager.inst.combatContext);
    }

    private void Initialize()
    {
        
        action = EnemyActionFactory.Create(data.id);
        action.GetEnemyData(data);

    }

    // =====================
    // ICombatHook
    // =====================

    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = phase == CombatPhase.turnStart || phase == CombatPhase.valueConfirm || phase == CombatPhase.turnEnd;
        return canExecute;
    }

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        if (action == null) return;

        switch (phase) 
        { 
            case CombatPhase.turnStart:
                action.DecidePattern(ctx);
                action.ApplyEffect(ctx, phase);
                break;
            case CombatPhase.turnEnd:
                action.ApplyEffect(ctx, phase);
                break;
            case CombatPhase.valueConfirm:
                action.ApplyEffect(ctx, phase);
                TakeDamage(ctx);
                break;
        }

    }

    public void ApplyTo(CombatContext ctx)
    {
        ctx.enemyState = new EnemyState
        {
            maxHp = data.maxHp,
            currentHp = data.maxHp,
            currentAttackValue = 0,
            currentDefenseValue = 0,
            intent = EnemyIntent.None
        };
    }
    // =====================

    public void TakeDamage(CombatContext ctx)
    {
        int playerAtkVal = ctx.snapshot.calcAttackValue + ctx.baseAttackValue;
        ctx.enemyState.currentHp += ctx.enemyState.currentDefenseValue - playerAtkVal;

        if (ctx.enemyState.currentHp <= 0)
            Die();
    }

    private void Die()
    {
        Reward();
        Destroy(gameObject);
    }

    private void Reward()
    {
        int reward = data.rewardValue;
    }
}
public class EnemyState
{
    public int currentHp;
    public int maxHp;

    public int currentAttackValue;
    public int currentDefenseValue;

    public EnemyIntent intent;
}

public enum EnemyIntent
{
    None,
    Attack,
    Defend,
    Buff,
    Debuff,
    AtkDef,
    Special
}

