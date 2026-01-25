using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public CombatContext combatContext { get; private set; }
    public static CombatManager inst { get; private set; }
    private List<ICombatHook> combatHooks = new List<ICombatHook>();
    private void Awake()
    {
        inst = new CombatManager();
        combatContext = new CombatContext();
    }
    
    public void ValueChanage()
    {
        ActivateHook(CombatPhase.valueChange);
    }
    public void HookRegister(ICombatHook hook)
    {
        combatHooks.Add(hook);
    }
    private void ActivateHook(CombatPhase phase)
    {
        foreach (ICombatHook hook in combatHooks)
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
    public List<(GameObject, int)> savingSlotDiceList { get;  set; }

}
public interface ICombatHook // 각 페이즈마다 컴뱃 매니저가 요청해서 함수를 실행시키는 애들이 상속하는 인터페이스
{
    int GetOrder();
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
