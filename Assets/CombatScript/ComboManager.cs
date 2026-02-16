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
    private ComboContext attackCtx;
    private ComboContext defenseCtx;
    private void Awake()
    {
        comboRelicList = new List<IComboRelic>();
        orders = new Dictionary<CombatPhase, int>();

        orders[CombatPhase.valueChange] = 0;
        orders[CombatPhase.valueConfirm] = 2;
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
    public void BuildComboSnapshot(ComboContext ctx)
    {
        ctx.eyeCounts = new int[6];
        
        foreach (var dice in ctx.diceList)
        { 
            // 1. 기본 눈 반영
            ctx.eyeCounts[dice.diceEye - 1]++;
           

        }
        // 2. eyeCounts 제작에 관여하는 유물 처리
        // 기본 눈 반영이 된 eyeCounts를 보고 그걸 수정한다.
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.CreateSnapshot);
        foreach (var relic in stageRelics)
        {
            if (!relic.CanAffect(ctx.slotRole))
                continue;

            relic.Activate(ctx);

        }

    }

    
    public void BuildComboCandidate(ComboContext ctx)
    {
        // 1. 기본 콤보 후보 선정
        ctx.candidate = GetBaseCandidate(ctx.eyeCounts);
     
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.BuildCandidate);
        // 2. 콤보 관여 유물 적용
        foreach (var relic in stageRelics)
        {
            if (!relic.CanAffect(ctx.slotRole))
                continue;

            ctx.tempCandidate = ctx.candidate;
            relic.Activate(ctx);

            // 3. 후보 중 눈의 개수가 더 많은 후보를 고른다. 같으면 눈이 더 높은 후보를 고른다.
            if (ctx.tempCandidate.Count > ctx.candidate.Count)
            {
                ctx.candidate = ctx.tempCandidate;
            }

            if (ctx.tempCandidate.Count == ctx.candidate.Count)
            {
                if(ctx.tempCandidate.Eye > ctx.candidate.Eye)
                {
                    ctx.candidate = ctx.tempCandidate;
                }
            }
        }

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
    public void ModifyCandidateAfterBuild(ComboContext ctx)
    {
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.CandidateModify);
        foreach (var relic in stageRelics)
        {
            if (!relic.CanAffect(ctx.slotRole))
                continue;

            relic.Activate(ctx);

        }
    }
    private void ApplyComboEffect(ComboContext ctx)
    {
        ctx.isBaseComboReplaced = false;
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.EffectApply);
        foreach (var relic in stageRelics)
        {
            if (!relic.CanAffect(ctx.slotRole))
                continue;

            if (!ctx.isBaseComboReplaced && relic is IComboEffectReplace)
            {
                relic.Activate(ctx);
                Debug.Log(ctx.isBaseComboReplaced);
            }
            else if(relic is not IComboEffectReplace)
            {
                
                relic.Activate(ctx);
                
            }
        }

        if (!ctx.isBaseComboReplaced)
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
    private void ApplyEffectAfterConfirm(ComboContext ctx)
    {
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.AfterConfirm);
        foreach (var relic in stageRelics)
        {
            if (!relic.CanAffect(ctx.slotRole))
                continue;

            relic.Activate(ctx);
        }

    }
    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        
        switch (phase)
        {
            case CombatPhase.valueChange:
                attackCtx = new ComboContext(ctx, DiceSlotRole.Attack, ctx.snapshot.attackDice);
                defenseCtx = new ComboContext(ctx, DiceSlotRole.Defense, ctx.snapshot.defenseDice);

                BuildComboSnapshot(attackCtx);
                BuildComboSnapshot(defenseCtx);

                BuildComboCandidate(attackCtx);
                BuildComboCandidate(defenseCtx);

                ModifyCandidateAfterBuild(attackCtx);
                ModifyCandidateAfterBuild(defenseCtx);

          
                ApplyComboEffect(attackCtx);
                //ApplyComboEffect(defenseCtx);
                break;
            case CombatPhase.valueConfirm:
                if (attackCtx == null && defenseCtx == null) break; // 어떤 슬롯에도 주사위가 없는 경우 => 그냥 넘김
                ApplyEffectAfterConfirm(attackCtx);
                ApplyEffectAfterConfirm(defenseCtx); 
                
                break;

        }

    }

    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = (phase == CombatPhase.valueChange || phase == CombatPhase.valueConfirm);
        return canExecute;
    }
}

/// <summary>
/// 기본 콤보 효과를 대체하는 유물이 상속하는 인터페이스
/// </summary>
public interface IComboEffectReplace { } 
public interface IComboRelic : IRelic
{
    ComboStage Stage { get; }
    
    void Activate(ComboContext ctx);
    
}
public class ComboContext
{
    public DiceSlotRole slotRole;
    public CombatContext combatCtx;
    public IReadOnlyList<DiceData> diceList;
    public int[] eyeCounts;

    public bool isBaseComboReplaced; // 콤보 효과 대체 유물이 발동해 기본 콤보 효과가 대체되었는지 체크하는 변수
    public ComboCandidate candidate; // 가장 최고의 candidate
    public ComboCandidate tempCandidate; // 유물 효과로 발생한 임시 candidate
    public ComboContext(CombatContext combatContext,
        DiceSlotRole slotRole,
        IReadOnlyList<DiceData> diceList)
    {
        this.slotRole = slotRole;
        this.combatCtx = combatContext;
        this.diceList = diceList;
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
    EffectApply,
    AfterConfirm
}


