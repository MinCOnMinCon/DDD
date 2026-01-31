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
                Debug.Log("AAA");
                tmp.text = ctx.attackValue.ToString();
                break;
        }
    }
}
