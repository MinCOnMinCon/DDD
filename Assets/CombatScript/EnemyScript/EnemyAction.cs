using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
public abstract class EnemyAction
{
    public int enemyId { get; }
    protected HashSet<CombatPhase> actPhase; // ApplyEffect가 발동하는 페이즈를 가진 셋, 자식 클래스에서 추가

    protected EnemyData enemyData;

    protected EnemyAction(int enemyId)
    {
        this.enemyId = enemyId;
        actPhase = new HashSet<CombatPhase>();
        
    }
    public void GetEnemyData(EnemyData data)
    {
        enemyData = data; 
    }
    // 턴 시작 시 패턴 결정 (수치만 설정)
    public abstract void DecidePattern(CombatContext ctx);

    
    public virtual void ApplyEffect(CombatContext ctx, CombatPhase phase) { }
}


public class EnemyAction_1 : EnemyAction
{
    bool isActive;
    public EnemyAction_1() : base(1) 
    {
        actPhase.Add(CombatPhase.turnStart);
        isActive = false;
    }

    public override void DecidePattern(CombatContext ctx)
    {
        var enemy = ctx.enemyState;

        switch (ctx.turnCount % 2) 
        {
            case 1:
                enemy.intent = EnemyIntent.Attack;
                enemy.currentAttackValue = enemyData.baseAttack * 1;
                enemy.currentDefenseValue = 0;
                break;
            case 0:
                enemy.intent = EnemyIntent.Defend;
                enemy.currentAttackValue = 0;
                enemy.currentDefenseValue = enemyData.baseDefense * 1;
                break;
            default:
                break;

        }

       
    }

    public override void ApplyEffect(CombatContext ctx, CombatPhase phase)
    {
        if (!actPhase.Contains(phase)) return;
        var enemy = ctx.enemyState;

        if (ctx.turnCount > 2 && !isActive)
        {
            enemyData.baseAttack *= 2;
            isActive = true;
        }
    }
}