using UnityEngine;

public class EnemyHpViewer : ValueViewer
{
    protected override void Awake()
    {
        base.Awake();
        orders[CombatPhase.combatStart] = 3;
        orders[CombatPhase.valueConfirm] = 1;
    }
    public override void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        tmp.text = ctx.enemyState.hp.ToString();
       
    }
    public override bool CanExecute(CombatPhase phase)
    {
        bool canExecute = orders.ContainsKey(phase);
        return canExecute;
    }
}

