using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public CombatContext combatContext { get; private set; }
    public static CombatManager inst { get; private set; }
    private List<ICombatHook> combatHooks = new List<ICombatHook>();

    private List<DiceSlot> allDiceSlots;
    private void Awake()
    {
        inst = this;
        combatContext = new CombatContext();
        allDiceSlots = new List<DiceSlot>();
    }

    public void CommitSlotChanges()
    {
        bool changed = false;
        
        foreach (var slot in allDiceSlots)
        {
            if (slot.ConsumeFlag())
            {
                changed = true;
                (slot as ICombatContextProvider)?.ApplyTo(combatContext);
            }
        }

        if (changed)
        {
           
            combatContext.attackValue = 0;
            combatContext.defenseValue = 0; // 이 두줄 나중에 담당 클래스에다가 맡기고 OnCombatPhase로 값 초기화
            ActivateHook(CombatPhase.valueChange);
        }
    }


    public void TurnStart()
    {
        ActivateHook(CombatPhase.turnStart);
    }
    public void HookRegister(ICombatHook hook)
    {
        combatHooks.Add(hook);
    }
    public void SlotRegister(DiceSlot slot)
    {
        allDiceSlots.Add(slot);
    }
    private void ActivateHook(CombatPhase phase)
    {
        var executableHooks = combatHooks
            .Where(h => h.CanExecute(phase))
            .OrderBy(h => h.GetOrder(phase))
            .ToList();

        foreach (var hook in executableHooks)
        {
            hook.OnCombatPhase(phase, combatContext);
        }
    }
}
public class CombatContext 
{
    public DiceState diceState { get;  set; }
    public PlayerState playerState { get; set; }

    public List<GameObject> attackSlotDiceList { get;  set; }
    public List<GameObject> defenseSlotDiceList { get;  set; }
    public List<GameObject> savingSlotDiceList { get;  set; }
    
    public int attackValue;
    public int defenseValue;
    public int turnCount;

    public CombatContext()
    {
        attackValue = 0;
        defenseValue = 0;
        turnCount = 1;
    }
    
}
public interface ICombatHook // 각 페이즈마다 컴뱃 매니저가 요청해서 함수를 실행시키는 애들이 상속하는 인터페이스
{
    int GetOrder(CombatPhase phase);
    bool CanExecute(CombatPhase phase);
    void OnCombatPhase(CombatPhase phase, CombatContext ctx);

}
public interface ICombatContextProvider // 컴뱃 컨텍스트의 변수를 채워 넣어야 하는 애들이 상속하는 인터페이스
{
    void ApplyTo(CombatContext ctx);
}


public enum CombatPhase 
{ 
    combatStart,
    turnStart,
    loan,
    valueChange,
    valueSubmit,
    turnEnd,
    combatEnd
}
