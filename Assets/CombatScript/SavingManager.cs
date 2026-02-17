

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SavingManager : MonoBehaviour, ICombatHook, ICombatContextProvider
{
    private Dictionary<CombatPhase, int> orders;
    private List<ISavingRelic> savingRelicList;
    private SavingContext savingContext;
    [SerializeField]    
    private int maxSavingValue = 20;
    [SerializeField]
    private int maxSavingCount = 3;
    
    private void Awake()
    {
        orders = new Dictionary<CombatPhase, int>();
        savingRelicList = new List<ISavingRelic>();
        
        orders[CombatPhase.valueChange] = 1;
        orders[CombatPhase.valueConfirm] = 3;
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

        ApplyTo(CombatManager.inst.combatContext);
        CombatManager.inst.HookRegister(this);
    }


    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        
        switch (phase)
        {
            case CombatPhase.valueChange:
                savingContext = new SavingContext(ctx, ctx.savingSlotDiceList, ctx.maxSavingValue, ctx.maxSavingCount);
                savingContext.savingSnapshot = new SavingSnapshot(DiceDataBuilder.BuildDiceDataList(savingContext.diceObjectList));
                
                break;
            case CombatPhase.valueConfirm:
                if (savingContext == null) break; // 어떤 슬롯에도 주사위가 있지 않는 경우 => 그냥 넘김
                EvaluateSingleDice(savingContext);
                EvaluateMultiDice(savingContext);
                break;
        }
    }

    private void EvaluateSingleDice(SavingContext ctx)
    {

        
        foreach (var diceObject in ctx.diceObjectList)
        {
            DiceData diceData = diceObject.GetComponent<Dice>().diceData;
            int doubledValue = diceData.diceValue * 2;
            diceData.SetValue(Mathf.Min(diceData.diceValue * 2, ctx.maxSavingValue));
            ctx.dice = diceData;

            var stageRelics = savingRelicList.Where(r => r.Stage == SavingStage.SingleDiceAfterConfirm);
            foreach (var relic in stageRelics)
            {
                relic.Activate(ctx);

            }
            Debug.Log("현재 주사위의 수치 " + ctx.dice.diceValue);
        }
    }

   
    private void EvaluateMultiDice(SavingContext ctx)
    {
        var stageRelics = savingRelicList.Where(r => r.Stage == SavingStage.MultiDiceAfterConfirm);
        foreach (var relic in stageRelics)
        {
            relic.Activate(ctx);

        }
      
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = (phase == CombatPhase.valueChange || phase == CombatPhase.valueConfirm);
        return canExecute;
    }
    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    public void ApplyTo(CombatContext ctx)
    {
        ctx.maxSavingValue = maxSavingValue;
        ctx.maxSavingCount = maxSavingCount;
    }
    
}
/// <summary>
/// 턴 종료 시 '저축 효과 및 유물 효과를 적용' 시키기 용이하게 하기 위한 클래스
/// value, combo 컨텍스트와 달리 saving 컨텍스트는 주사위 리스트가 복사가 아닌 실제 주사위를 가르키는 GameObject 리스트이다.
/// 얘네는 나중에 사라지면 안되기 때문이다.
/// </summary>
public class SavingContext
{
    public CombatContext combatContext { get; }
    public SavingSnapshot savingSnapshot;
    public List<GameObject> diceObjectList;
    public int maxSavingValue;
    public int maxSavingCount;

    // SingleDice용
    public DiceData dice;

    public SavingContext(CombatContext combatContext, List<GameObject> diceObjectList, int value, int count)
    {
        this.combatContext = combatContext;
        this.diceObjectList = diceObjectList;
        this.maxSavingCount = count;
        this.maxSavingValue = value;
    }

}
public interface ISavingRelic 
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
    BeforeConfirm,
    SingleDiceAfterConfirm,
    MultiDiceAfterConfirm,

}
