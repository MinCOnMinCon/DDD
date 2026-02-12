

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SavingManager : MonoBehaviour, ICombatHook
{
    private Dictionary<CombatPhase, int> orders;
    private List<ISavingRelic> savingRelicList;
    [SerializeField]    
    private int maxSavingValue = 20;
    [SerializeField]
    private int maxSavingCount = 3;
    private void Awake()
    {
        orders = new Dictionary<CombatPhase, int>();
        savingRelicList = new List<ISavingRelic>();
        orders[CombatPhase.valueChange] = 1;
    }
    private void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        foreach (var effect in RelicManager.inst.GetRelicEffects<ISavingRelic>())
        {
            savingRelicList.Add(effect);
        }
      
  
        CombatManager.inst.HookRegister(this);
    }


    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.valueChange:
                SavingContext savingContext = new SavingContext(ctx, ctx.snapshot.saveDice, maxSavingValue, maxSavingCount);
                EvaluateSingleDice(savingContext);
                savingContext.savingSnapshot = new SavingSnapshot(savingContext.diceList);
                EvaluateMultiDice(savingContext);
                break;

        }
    }
    private void EvaluateSingleDice(SavingContext ctx)
    {


        foreach (var diceData in ctx.diceList)
        {
            int doubledValue = diceData.diceValue * 2;
            diceData.diceValue = Mathf.Min(diceData.diceValue * 2, maxSavingValue);
            ctx.dice = diceData;
            ActivateSingleRelics(ctx);
        }
    }

    private void ActivateSingleRelics(SavingContext ctx)
    {
        var stageRelics = savingRelicList.Where(r => r.Stage == SavingStage.SingleDice);
        foreach (var relic in stageRelics)
        {
            relic.Activate(ctx);
        }
    }
    private void EvaluateMultiDice(SavingContext ctx)
    {
        var stageRelics = savingRelicList.Where(r => r.Stage == SavingStage.MultiDice);
        foreach (var relic in stageRelics)
        {
            relic.Activate(ctx);
        }
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = phase == CombatPhase.turnEnd;
        return canExecute;
    }
    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    
}
/// <summary>
/// 턴 종료 시 '저축 효과 및 유물 효과를 적용' 시키기 용이하게 하기 위한 클래스
/// </summary>
public class SavingContext
{
    public CombatContext combatContext { get; }
    public SavingSnapshot savingSnapshot;
    public List<DiceData> diceList;
    public int maxSavingValue;
    public int maxSavingCount;

    // SingleDice용
    public DiceData dice;

    public SavingContext(CombatContext combatContext, List<DiceData> diceList, int value, int count)
    {
        this.combatContext = combatContext;
        this.diceList = diceList;
        this.maxSavingCount = count;
        this.maxSavingValue = value;
    }

}
public interface ISavingRelic : IRelic
{
    SavingStage Stage { get; }
    void Activate(SavingContext ctx);
}
/// <summary>
/// 저축 관련 유물의 '조건 체크'를 용이하게 하기 위해 만든 클래스
/// </summary>
public class SavingSnapshot 
{
    public readonly List<DiceData> DiceList;
    public int DiceCount => DiceList.Count;

    public SavingSnapshot(List<DiceData> source)
    {
        DiceList = new List<DiceData>(source);
    }

    public bool IsFullyFilled(int maxCount)
    {
        return DiceList.Count == maxCount;
    }

    public bool IsAllSameEye()
    {
        if (DiceList.Count == 0) return false;

        int eye = DiceList[0].diceEye;
        foreach (var dice in DiceList)
        {
            if (dice.diceEye != eye)
                return false;
        }
        return true;
    }
    public (bool isSame, DiceType type) IsAllSameType()
    {
        if (DiceList.Count == 0) return (false, DiceType.basic);

        DiceType type = DiceList[0].diceType;
        foreach (var dice in DiceList)
        {
            if (dice.diceType != type)
                return (false, DiceType.basic);
        }
        return (true, type);
    }
}

public enum SavingStage 
{
    SingleDice,
    MultiDice
}
