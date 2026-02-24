using UnityEngine;

public class PlayerAtkViewer : ValueViewer
{
    protected override void Awake()
    {
        base.Awake();
        orders[CombatPhase.valueChange] = 4;
    }
    public override void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
           tmp.text = (ctx.snapshot.calcAttackValue + ctx.snapshot.baseAttackValue).ToString();
   
    }

    public override bool CanExecute(CombatPhase phase)
    {
        bool canExecute = orders.ContainsKey(phase);
        return canExecute;
    }
}
