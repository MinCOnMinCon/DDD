using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public CombatContext combatContext { get; private set; }
    public static CombatManager inst { get; private set; }
    private List<ICombatHook> combatHooks;
    private bool isTurnStart;
    private bool isValueConfirm;


    private List<DiceSlot> allDiceSlots;
    private void Awake()
    {
        inst = this;
        isValueConfirm = false;
        isTurnStart = false;
        combatHooks = new List<ICombatHook>();
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
           
            combatContext.snapshot = CombatContextSnapshotFactory.Create(combatContext);
            ActivateHook(CombatPhase.valueChange);
            combatContext.snapshot = null;
        }
    }

    public void ConfirmValue()
    {
        if (isValueConfirm) return;

        isValueConfirm = true;
        ActivateHook(CombatPhase.valueConfirm);
        TurnEnd();
    }

    public void TurnStart()
    {
        if(isTurnStart) return; 
        
        isTurnStart = true;
        ActivateHook(CombatPhase.turnStart);
    }
    public void TurnEnd()
    {
        ActivateHook(CombatPhase.turnEnd);

        isTurnStart = false;
        isValueConfirm = false;
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
            Debug.Log("현재 페이즈 : " + phase.ToString() + " 발동 훅 : " + hook.GetOrder(phase));
            hook.OnCombatPhase(phase, combatContext);
        }
    }
}
/// <summary>
/// 전투에서 사용하는 컨텍스트
/// 
/// </summary>
public class CombatContext 
{
    public DiceState diceState { get;  set; }
    public PlayerState playerState { get; set; }
    //public EnemyState enemyState { get; set; }

    public List<GameObject> attackSlotDiceList { get;  set; }
    public List<GameObject> defenseSlotDiceList { get;  set; }
    public List<GameObject> savingSlotDiceList { get;  set; }

    public int baseAttackValue;
    public int baseDefenseValue;
    public int turnCount;

    public int maxSavingValue;
    public int maxSavingCount;

    public CombatContextSnapshot snapshot { get; set; }
    public IDiceService diceFactory;

    public CombatContext()
    {
        baseAttackValue = 0;
        baseDefenseValue = 0;
        baseDefenseValue = 0;
      
        turnCount = 1;
    }
    
}

/// <summary>
/// CombatPhase가 valueChange일 때 만드는 컴뱃 컨텍스트의 스냅샷. 
/// valueChange가 확정나면 이 컴뱃 컨텍스트 스냅샷의 내용을 적용시킨다.
/// 확정나지 않았다면 스냅샷을 버린다.
/// </summary>
public class CombatContextSnapshot 
{
    // ===== HP =====
    public int currentHp;
    public int maxHp;

    // ===== Base Values (전투 시작 기준) =====
    public int baseAttackValue;
    public int baseDefenseValue;

    // ===== Calculated Values (valueChange 전용) =====
    public int calcAttackValue;
    public int calcDefenseValue;

    // ===== Slot Dice (복사본 DiceData) =====
    public List<DiceData> attackDice;
    public List<DiceData> defenseDice;
    public List<DiceData> saveDice;

    // ===== Dice Counts =====
    public int basicDiceCount;
    public int penaltyDiceCount;
    public int loanDiceCount;

    public int turnCount;

    
}
public static class CombatContextSnapshotFactory
{
    public static CombatContextSnapshot Create(CombatContext ctx)
    {
        return new CombatContextSnapshot
        {
            currentHp = ctx.playerState.hp,

            baseAttackValue = ctx.baseAttackValue,
            baseDefenseValue = ctx.baseDefenseValue,

            calcAttackValue = 0,
            calcDefenseValue = 0,

            attackDice = DiceDataBuilder.BuildDiceDataList(ctx.attackSlotDiceList),
            defenseDice = DiceDataBuilder.BuildDiceDataList(ctx.defenseSlotDiceList),
            saveDice = DiceDataBuilder.BuildDiceDataList(ctx.savingSlotDiceList),

            basicDiceCount = ctx.diceState.basicDiceNum,
            penaltyDiceCount = ctx.diceState.penaltyDiceNum,
            loanDiceCount = ctx.diceState.loanDiceNum,

            turnCount = ctx.turnCount
        };
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
    valueChange,
    valueConfirm,
    turnEnd,
    combatEnd
}
