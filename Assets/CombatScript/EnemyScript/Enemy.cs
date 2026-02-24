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
    }

    private void Initialize()
    {
        
        action = EnemyActionFactory.Create(data.id);
        action.GetEnemyData(data);

        orders = new Dictionary<CombatPhase, int>();
        orders.Add(CombatPhase.turnStart, 1);
        orders.Add(CombatPhase.valueConfirm, 0);
        orders.Add(CombatPhase.turnEnd, 1);
        orders.Add(CombatPhase.combatStart, 2);

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
        bool canExecute = orders.ContainsKey(phase);
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
            case CombatPhase.combatStart:
                
                ApplyTo(CombatManager.inst.combatContext);
                break;
        }

    }

    public void ApplyTo(CombatContext ctx)
    {
        ctx.enemyState = new EnemyState
        {
            maxHp = data.maxHp,
            hp = data.maxHp,
            currentAttackValue = 0,
            currentDefenseValue = 0,
            intent = EnemyIntent.None
        };
    }
    // =====================

    public void TakeDamage(CombatContext ctx)
    {
        int playerAtkVal = ctx.snapshot.calcAttackValue + ctx.snapshot.baseAttackValue;
        ctx.enemyState.hp += ctx.enemyState.currentDefenseValue - playerAtkVal;

        if (ctx.enemyState.hp <= 0)
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
    public int hp;
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

