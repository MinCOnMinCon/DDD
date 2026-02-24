using UnityEngine;

public class PlayerHpViewer : ValueViewer
{
    protected override void Awake()
    {
        base.Awake();
        orders[CombatPhase.combatStart] = 4;
        orders[CombatPhase.valueConfirm] = 3;
    }
    public override void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        
         tmp.text = ctx.playerState.hp.ToString();
        
    }
    public override bool CanExecute(CombatPhase phase)
    {
        bool canExecute = orders.ContainsKey(phase);
        return canExecute;
    }
}

