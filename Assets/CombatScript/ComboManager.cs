using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;
using System.Linq;

public class ComboManager : MonoBehaviour, ICombatHook
{
    [SerializeField]
    private Dictionary<CombatPhase, int> orders;
    private List<IComboRelic> comboRelicList;
    private void Awake()
    {
        comboRelicList = new List<IComboRelic>();
        orders = new Dictionary<CombatPhase, int>();

        orders[CombatPhase.valueChange] = 0;
    }

    private void Start()
    {

        Initialize();


    }

    private void Initialize()
    {
        foreach (var effect in RelicManager.inst.GetRelicEffects<IComboRelic>())
        {
            comboRelicList.Add(effect);
        }
        
        CombatManager.inst.HookRegister(this);
    }
    public int[] BuildComboSnapshot(ComboContext ctx)
    {
        int[] eyeCounts = new int[6];
        List<DiceData> diceObjects;
        switch (ctx.slotRole)
        {
            case DiceSlotRole.Attack:
                diceObjects = ctx.combatCtx.snapshot.attackDice;
                break;
            case DiceSlotRole.Defense:
                diceObjects = ctx.combatCtx.snapshot.defenseDice;
                break;
            default:
                diceObjects = null;
                break;
        }
        foreach (var dice in diceObjects)
        { 
            // 1. 기본 눈 반영
            eyeCounts[dice.diceEye - 1]++;
            var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.CandidateModify);
            // 2. 복사본 제작에 관여하는 유물 처리
            foreach (var relic in stageRelics)
            {
                if (!relic.CanAffect(ctx.slotRole))
                    continue;

                relic.Activate(ctx);
                    
            }
        }

        return eyeCounts;
    }

    
    public ComboCandidate BuildComboCandidate(ComboContext ctx)
    {
        // 1. 기본 콤보 후보 선정
        ComboCandidate bestCandidate = GetBaseCandidate(ctx.eyeCounts);

        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.CandidateModify);
        // 2. 콤보 관여 유물 적용
        foreach (var relic in stageRelics)
        {
            if (!relic.CanAffect(ctx.slotRole))
                continue;

            relic.Activate(ctx);

            // 3. 더 높은 개수만 채택
            if (ctx.candidate.Count > bestCandidate.Count)
            {
                bestCandidate = ctx.candidate;
            }
        }

        return bestCandidate;
    }
    private ComboCandidate GetBaseCandidate(int[] eyeCounts)
    {
        int bestEye = 1;
        int bestCount = eyeCounts[0];

        for (int i = 1; i < 6; i++)
        {
            if (eyeCounts[i] > bestCount)
            {
                bestCount = eyeCounts[i];
                bestEye = i + 1;
            }
        }

        return new ComboCandidate(bestEye, bestCount);
    }
    public void ApplyComboEffect(ComboContext ctx)
    {
        bool isReplaced = false;
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.CandidateModify);
        foreach (var relic in stageRelics)
        {
            if (!relic.CanAffect(ctx.slotRole))
                continue;

            
            relic.Activate(ctx);
            isReplaced = true;
            break; // 하나만 적용
            
        }

        if (!isReplaced)
        {
            ApplyBaseCombo(ctx);
        }
    }
    private void ApplyBaseCombo(ComboContext ctx)
    {
        int count = ctx.candidate.Count - 1 < 0 ? 0 : ctx.candidate.Count - 1;
        int value = ctx.candidate.Eye * count;
        if(ctx.slotRole == DiceSlotRole.Attack)
        {
            ctx.combatCtx.snapshot.calcAttackValue += value;
        }
        if(ctx.slotRole == DiceSlotRole.Defense)
        {
            ctx.combatCtx.snapshot.calcDefenseValue += value;
        }
    }

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.valueChange:
                ComboContext attackCtx = new ComboContext(DiceSlotRole.Attack, ctx);
                ComboContext defenseCtx = new ComboContext(DiceSlotRole.Defense, ctx);

                attackCtx.eyeCounts = BuildComboSnapshot(attackCtx);
                defenseCtx.eyeCounts = BuildComboSnapshot(defenseCtx);

                attackCtx.candidate = BuildComboCandidate(attackCtx);
                defenseCtx.candidate =  BuildComboCandidate(defenseCtx);

                ApplyComboEffect(attackCtx);
                ApplyComboEffect(defenseCtx);
                break;
        }

    }

    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = phase == CombatPhase.valueChange;
        return canExecute;
    }
}



public interface IComboRelic : IRelicEffect
{
    ComboStage Stage { get; }
    
    void Activate(ComboContext ctx);
    
}
public class ComboContext
{
    public DiceSlotRole slotRole;
    public CombatContext combatCtx;

    public int[] eyeCounts;

    public ComboCandidate candidate;
    public ComboContext(DiceSlotRole role , CombatContext combatCtx)
    {
        this.slotRole = role;
        this.combatCtx = combatCtx;
    }
}
public struct ComboCandidate
{
    public int Eye;// 1~6
    public int Count;

    public ComboCandidate(int eye, int count)
    {
        Eye = eye;
        Count = count;
    }
}
public enum ComboStage
{
    CreateSnapshot,
    BuildCandidate,
    CandidateModify,
    EffectApply
}


