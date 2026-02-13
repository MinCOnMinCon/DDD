using UnityEngine;

public class DefenseValueViewer : ValueViewer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
                tmp.text = (ctx.snapshot.calcDefenseValue + ctx.snapshot.baseDefenseValue).ToString();
                break;
        }
    }
}
