
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CombatStartManager : MonoBehaviour, ICombatHook
{
    private List<ICombatStartRelic> combatStartRelicList;
    private Dictionary<CombatPhase, int> orders;
    private void Awake()
    {
        combatStartRelicList = new List<ICombatStartRelic>();
        orders = new Dictionary<CombatPhase, int>();

        orders[CombatPhase.combatStart] = 0;
    }
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (var effect in RelicManager.inst.GetRelicEffects<ICombatStartRelic>())
        {
            combatStartRelicList.Add(effect);
        }

        CombatManager.inst.HookRegister(this);
    }

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {

        switch (phase)
        {
            case CombatPhase.combatStart:
                ActivateCombatStartRelic(ctx);
                break;
                

        }

    }

    private void ActivateCombatStartRelic(CombatContext ctx)
    {
        if (combatStartRelicList == null) return;

        foreach(var relic in  combatStartRelicList)
        {
            relic.Activate(ctx);
        }
    }

    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = orders.ContainsKey(phase);
        return canExecute;
    }

}

public interface ICombatStartRelic 
{
    public void Activate(CombatContext ctx);
}
