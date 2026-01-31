using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class ValueViewer : MonoBehaviour, ICombatHook
{
    protected Dictionary<CombatPhase, int> orders;
    protected TextMeshProUGUI tmp;
    protected virtual void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        orders = new Dictionary<CombatPhase, int>();
    }
    protected void Start()
    {
        CombatManager.inst.HookRegister(this);
    }

    public abstract void OnCombatPhase(CombatPhase phase, CombatContext ctx);
    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }

    public bool CanExecute(CombatPhase phase)
    {
        return phase == CombatPhase.valueChange;
    }
}

