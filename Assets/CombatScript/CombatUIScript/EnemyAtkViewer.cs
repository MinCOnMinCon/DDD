using UnityEngine;

public class EnemyAtkViewer : ValueViewer
{
    protected override void Awake()
    {
        base.Awake();
        orders[CombatPhase.turnStart] = 2;
    }
    public override void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
           tmp.text = ctx.enemyState.currentAttackValue.ToString();
   
    }

    public override bool CanExecute(CombatPhase phase)
    {
        bool canExecute = orders.ContainsKey(phase);
        return canExecute;
    }
}
