using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public event Action<PlayerState> OnPlayerStateAvailable;
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
        }
    }

    public void ConfirmValue()
    {
        if (isValueConfirm) return;

        isValueConfirm = true;
        ActivateHook(CombatPhase.valueConfirm);
        TurnEnd();
    }
    public void CombatStart()
    {
        ActivateHook(CombatPhase.combatStart);
    }
    public void TurnStart()
    {
        if(isTurnStart) return;
        CombatStart(); // �׽�Ʈ�� ���� �ӽ� �ڵ�
        isTurnStart = true;
        ActivateHook(CombatPhase.turnStart);
    }
    public void TurnEnd()
    {
        ActivateHook(CombatPhase.turnEnd);

        combatContext.turnCount++;
        isTurnStart = false;
        isValueConfirm = false;
    }

    public void CombatEnd()
    {
        ActivateHook(CombatPhase.combatEnd);
        SceneManager.LoadScene("StageScene");
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
            Debug.Log("���� ������ : " + phase.ToString() + " �ߵ� �� : " + hook.GetOrder(phase));
            hook.OnCombatPhase(phase, combatContext);
        }
    }
}
/// <summary>
/// �������� ����ϴ� ���ؽ�Ʈ
/// 
/// </summary>
public class CombatContext 
{
    public DiceState diceState { get;  set; }
    public PlayerState playerState { get; set; }
    public EnemyState enemyState { get; set; }

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
        
        snapshot = new CombatContextSnapshot();
        turnCount = 1;
    }
    
}

/// <summary>
/// CombatPhase�� valueChange�� �� ����� �Ĺ� ���ؽ�Ʈ�� ������. 
/// valueChange�� Ȯ������ �� �Ĺ� ���ؽ�Ʈ �������� ������ �����Ų��.
/// Ȯ������ �ʾҴٸ� �������� ������.
/// </summary>
public class CombatContextSnapshot 
{
    // ===== HP =====
    public int currentHp;
    public int maxHp;

    // ===== Base Values (���� ���� ����) =====
    public int baseAttackValue;
    public int baseDefenseValue;

    // ===== Calculated Values (valueChange ����) =====
    public int calcAttackValue;
    public int calcDefenseValue;

    // ===== Slot Dice (���纻 DiceData) =====
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

public interface ICombatHook // �� ������� �Ĺ� �Ŵ����� ��û�ؼ� �Լ��� �����Ű�� �ֵ��� ����ϴ� �������̽�
{
    int GetOrder(CombatPhase phase);
    bool CanExecute(CombatPhase phase);
    void OnCombatPhase(CombatPhase phase, CombatContext ctx);

}
public interface ICombatContextProvider // �Ĺ� ���ؽ�Ʈ�� ������ ä�� �־�� �ϴ� �ֵ��� ����ϴ� �������̽�
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
