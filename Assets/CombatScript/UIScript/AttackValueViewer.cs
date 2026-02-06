using UnityEngine;

public class AttackValueViewer : ValueViewer
{
    protected override void Awake()
    {
        base.Awake();
        orders[CombatPhase.valueChange] = 3;
    }
    public override void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.valueChange:
                
                tmp.text = (ctx.calcAttackValue + ctx.baseAttackValue).ToString();
                break;
        }
    }
}
