using UnityEngine;

public class EnemyDefViewer : ValueViewer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        orders[CombatPhase.turnStart] = 3;
    }
    public override void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
       
        tmp.text = ctx.enemyState.currentDefenseValue.ToString();
        
    }
    public override bool CanExecute(CombatPhase phase)
    {
        bool canExecute = orders.ContainsKey(phase);
        return canExecute;
    }
}
