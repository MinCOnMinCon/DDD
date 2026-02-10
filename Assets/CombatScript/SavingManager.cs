

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SavingManager : MonoBehaviour, ICombatHook
{
    private Dictionary<CombatPhase, int> orders;
    private List<ISavingRelic> savingRelicList;
    [SerializeField]
    private int maxSavingValue = 20;
    private void Awake()
    {
        orders = new Dictionary<CombatPhase, int>();
        savingRelicList = new List<ISavingRelic>();
        orders[CombatPhase.turnEnd] = 0;
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
            case CombatPhase.turnEnd:
                SavingContext savingContext = new SavingContext(ctx, ctx.snapshot.saveDice);
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

public class SavingContext
{
    public CombatContext combatContext { get; }
    public SavingSnapshot savingSnapshot;
    public List<DiceData> diceList;

    // SingleDice¿ë
    public DiceData dice;

    public SavingContext(CombatContext combatContext, List<DiceData> diceList)
    {
        this.combatContext = combatContext;
        this.diceList = diceList;
       
    }

}
public interface ISavingRelic : IRelic
{
    SavingStage Stage { get; }
    void Activate(SavingContext ctx);
}

public class SavingSnapshot
{
    public readonly List<DiceData> DiceList;
    public int DiceCount => DiceList.Count;

    public SavingSnapshot(List<DiceData> source)
    {
        DiceList = new List<DiceData>(source);
    }

    public bool IsFullyFilled(int requiredCount)
    {
        return DiceList.Count == requiredCount;
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
