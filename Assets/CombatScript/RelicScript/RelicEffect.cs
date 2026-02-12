using UnityEngine;

// 공통 베이스
public abstract class ValueRelicEffectBase : RelicEffect, IValueRelic
{
    public abstract ValueStage Stage { get; }

    protected ValueRelicEffectBase(int relicId) : base(relicId) { }

    public virtual bool CanAffect(DiceSlotRole slotRole) => true;

    public abstract void Activate(ValueContext ctx);
}

// ---------------- 수치계산 - 1 (ValueApply) ----------------

public class Relic_6 : ValueRelicEffectBase
{
    public Relic_6() : base(6) { }
    public override ValueStage Stage => ValueStage.ValueApply;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.dice.diceType == DiceType.penalty)
        {
            ctx.dice.diceValue = -ctx.dice.diceValue;
        }
    }
}

public class Relic_22 : ValueRelicEffectBase
{
    public Relic_22() : base(22) { }
    public override ValueStage Stage => ValueStage.ValueApply;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.dice.diceEye <= 3)
            ctx.dice.diceValue += 2;
    }
}

public class Relic_31 : ValueRelicEffectBase
{
    public Relic_31() : base(31) { }
    public override ValueStage Stage => ValueStage.ValueApply;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.dice.diceEye == 1)
            ctx.dice.diceValue += 10;
        else
            ctx.dice.diceValue -= 2;
    }
}

public class Relic_32 : ValueRelicEffectBase
{
    public Relic_32() : base(32) { }
    public override ValueStage Stage => ValueStage.ValueApply;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.dice.diceType == DiceType.loan)
            ctx.dice.diceValue += 2;
    }
}

// ---------------- 수치계산 - 2 (ValueConditionCheck) ----------------

public class Relic_4 : ValueRelicEffectBase
{
    public Relic_4() : base(4) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.combatContext.turnCount == 1 && ctx.slotRole == DiceSlotRole.Attack)
        {
            ctx.combatContext.snapshot.calcAttackValue += 8;
        }
    }
}

public class Relic_10 : ValueRelicEffectBase
{
    public Relic_10() : base(10) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.slotRole == DiceSlotRole.Defense && ctx.slotSnapshot.totalDiceCount == 0)
            ctx.combatContext.snapshot.calcAttackValue += 10;
    }
}

public class Relic_11 : ValueRelicEffectBase
{
    public Relic_11() : base(11) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.combatContext.snapshot.penaltyDiceCount >= 6)
        {
            int bonus = ctx.slotSnapshot.basicDiceCount + ctx.slotSnapshot.loanDiceCount;
            if (ctx.slotRole == DiceSlotRole.Attack)
                ctx.combatContext.snapshot.calcAttackValue += bonus;
            else
                ctx.combatContext.snapshot.calcDefenseValue += bonus;
        }
    }
}

public class Relic_20 : ValueRelicEffectBase
{
    public Relic_20() : base(20) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.slotSnapshot.eyeMap.Count == 6) //eyeMap은 주사위 눈의 종류 수 만큼 길이가 정해지기 때문에 6이면 1부터 6이 다 있음
        {
            if (ctx.slotRole == DiceSlotRole.Attack)
                ctx.combatContext.snapshot.calcAttackValue *= 2;
            else
                ctx.combatContext.snapshot.calcDefenseValue *= 2;
        }
    }
}

public class Relic_28 : ValueRelicEffectBase
{
    public Relic_28() : base(28) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (!ctx.slotSnapshot.eyeMap.TryGetValue(4, out var eye)) return;
        ctx.combatContext.snapshot.calcAttackValue += eye.totalCount * 4;
    }
}

// ---------------- 수치계산 - 3 (FinalValueEffect) ----------------

public class Relic_26 : ValueRelicEffectBase
{
    public Relic_26() : base(26) { }
    public override ValueStage Stage => ValueStage.FinalValueEffect;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.slotRole == DiceSlotRole.Attack)
        {
            ctx.combatContext.snapshot.calcAttackValue += ctx.slotSnapshot.totalDiceCount * 4;
        }
    }
}

public class Relic_29 : ValueRelicEffectBase
{
    public Relic_29() : base(29) { }
    public override ValueStage Stage => ValueStage.FinalValueEffect;

    public override void Activate(ValueContext ctx)
    {
        int bonus = ctx.combatContext.snapshot.calcDefenseValue / 3;
        ctx.combatContext.snapshot.calcAttackValue += bonus;
    }
}


// ======================
// Combo Relic Base Class
// ======================

public abstract class ComboRelicEffectBase : RelicEffect, IComboRelic
{
    public abstract ComboStage Stage { get; }

    protected ComboRelicEffectBase(int relicId) : base(relicId) { }

    public virtual bool CanAffect(DiceSlotRole slotRole) => true;

    public abstract void Activate(ComboContext ctx);
}


// ======================
// 콤보계산 - 1 (CreateSnapshot)
// ======================

// 14. 극과 극
public class Relic_14 : ComboRelicEffectBase
{
    public Relic_14() : base(14) { }

    public override ComboStage Stage => ComboStage.CreateSnapshot;

    public override void Activate(ComboContext ctx)
    {
        // 주사위 눈 1을 6으로 취급
        ctx.eyeCounts[5] += ctx.eyeCounts[0];
        ctx.eyeCounts[0] = 0;
    }
}


// ======================
// 콤보계산 - 2 (BuildCandidate)
// ======================

// 8. 짝수 콤보
public class Relic_8 : ComboRelicEffectBase
{
    public Relic_8() : base(8) { }

    public override ComboStage Stage => ComboStage.BuildCandidate;

    public override void Activate(ComboContext ctx)
    {
        int[] evenEyes = { 2, 4, 6 };
        int total = 0;

        foreach (int eye in evenEyes)
            total += ctx.eyeCounts[eye - 1];

        if (total > 0)
            ctx.tempCandidate = new ComboCandidate(FindMostFrequent(evenEyes, ctx), total);
    }

    private int FindMostFrequent(int[] eyes, ComboContext ctx)
    {
        int bestEye = eyes[0];
        int bestCount = ctx.eyeCounts[bestEye - 1];

        foreach (int eye in eyes)
        {
            if (ctx.eyeCounts[eye - 1] > bestCount)
            {
                bestCount = ctx.eyeCounts[eye - 1];
                bestEye = eye;
            }
        }
        return bestEye;
    }
}


// 9. 홀수 콤보
public class Relic_9 : ComboRelicEffectBase
{
    public Relic_9() : base(9) { }

    public override ComboStage Stage => ComboStage.BuildCandidate;

    public override void Activate(ComboContext ctx)
    {
        int[] oddEyes = { 1, 3, 5 };
        int total = 0;

        foreach (int eye in oddEyes)
            total += ctx.eyeCounts[eye - 1];

        if (total > 0)
            ctx.tempCandidate = new ComboCandidate(FindMostFrequent(oddEyes, ctx), total);
    }

    private int FindMostFrequent(int[] eyes, ComboContext ctx)
    {
        int bestEye = eyes[0];
        int bestCount = ctx.eyeCounts[bestEye - 1];

        foreach (int eye in eyes)
        {
            if (ctx.eyeCounts[eye - 1] > bestCount)
            {
                bestCount = ctx.eyeCounts[eye - 1];
                bestEye = eye;
            }
        }
        return bestEye;
    }
}


// 23. 연타
public class Relic_23 : ComboRelicEffectBase
{
    public Relic_23() : base(23) { }

    public override ComboStage Stage => ComboStage.BuildCandidate;

    public override void Activate(ComboContext ctx)
    {
        if (ctx.candidate.Count >= 4)
        {
            ctx.tempCandidate = new ComboCandidate(
                ctx.candidate.Eye,
                ctx.candidate.Count + 2
            );
        }
    }
}


// ======================
// 콤보계산 - 3 (EffectApply)
// ======================

// 18. 3+3+3=?
public class Relic_18 : ComboRelicEffectBase
{
    public Relic_18() : base(18) { }

    public override ComboStage Stage => ComboStage.EffectApply;

    public override void Activate(ComboContext ctx)
    {
        if (ctx.candidate.Eye == 3 && ctx.candidate.Count >= 3)
        {
            ctx.combatCtx.snapshot.basicDiceCount += 1;
        }
    }
}


// 24. only one
public class Relic_24 : ComboRelicEffectBase
{
    public Relic_24() : base(24) { }

    public override ComboStage Stage => ComboStage.EffectApply;

    public override void Activate(ComboContext ctx)
    {
        if (ctx.slotRole == DiceSlotRole.Attack &&
            ctx.candidate.Eye == 1 &&
            ctx.candidate.Count >= 5)
        {
            ctx.combatCtx.snapshot.calcAttackValue += 40;
        }
    }
}


// 25. 2의2승
public class Relic_25 : ComboRelicEffectBase
{
    public Relic_25() : base(25) { }

    public override ComboStage Stage => ComboStage.EffectApply;

    public override void Activate(ComboContext ctx)
    {
        if (ctx.candidate.Eye == 2 && ctx.candidate.Count > 0)
        {
            int value = (int)Mathf.Pow(2, ctx.candidate.Count - 1);

            if (ctx.slotRole == DiceSlotRole.Attack)
                ctx.combatCtx.snapshot.calcAttackValue += value;
            else if (ctx.slotRole == DiceSlotRole.Defense)
                ctx.combatCtx.snapshot.calcDefenseValue += value;
        }
    }
}


// 27. 콤보 회복
public class Relic_27 : ComboRelicEffectBase
{
    public Relic_27() : base(27) { }

    public override ComboStage Stage => ComboStage.EffectApply;

    public override void Activate(ComboContext ctx)
    {
        if (ctx.slotRole == DiceSlotRole.Defense)
        {
            ctx.combatCtx.snapshot.currentHp += ctx.candidate.Count;
        }
    }
}
// ======================
// Saving Relic Base Class
// ======================

public abstract class SavingRelicEffectBase : RelicEffect, ISavingRelic
{
    public abstract SavingStage Stage { get; }

    protected SavingRelicEffectBase(int relicId) : base(relicId) { }

    public virtual bool CanAffect(DiceSlotRole slotRole) => true;

    public abstract void Activate(SavingContext ctx);
}


// ======================
// 3. 콤보 저축 (저축-멀티)
// ======================
// 저축 슬롯이 꽉 차 있고 모두 같은 눈이면
// 해당 주사위들을 maxSavingValue까지 올린다.

public class Relic_3 : SavingRelicEffectBase
{
    public Relic_3() : base(3) { }

    public override SavingStage Stage => SavingStage.MultiDice;

    public override void Activate(SavingContext ctx)
    {
        if (ctx.savingSnapshot.IsFullyFilled(ctx.maxSavingCount) &&
            ctx.savingSnapshot.IsAllSameEye())
        {
            foreach (var dice in ctx.diceList)
            {
                dice.diceValue = ctx.maxSavingValue;
            }
        }
    }
}


// ======================
// 19. 오늘의 일은 내일로 (저축-멀티)
// ======================
// 저축 슬롯이 패널티 주사위로 꽉 차 있으면
// 대출 주사위 3개 생성 (유물 효과이므로 isLoan = false)

public class Relic_19 : SavingRelicEffectBase
{
    public Relic_19() : base(19) { }

    public override SavingStage Stage => SavingStage.MultiDice;

    public override void Activate(SavingContext ctx)
    {
        if (!ctx.savingSnapshot.IsFullyFilled(ctx.maxSavingCount))
            return;

        var (isSame, type) = ctx.savingSnapshot.IsAllSameType();

        if (isSame && type == DiceType.penalty)
        {
            ctx.combatContext.diceState.loanDiceNum += 3;
        }
    }
}


// ======================
// 30. V (저축-싱글)
// ======================
// 눈 5를 저축하면 즉시 maxSavingValue로 만든다.

public class Relic_30 : SavingRelicEffectBase
{
    public Relic_30() : base(30) { }

    public override SavingStage Stage => SavingStage.SingleDice;

    public override void Activate(SavingContext ctx)
    {
        if (ctx.dice.diceEye == 5)
        {
            ctx.dice.diceValue = ctx.maxSavingValue;
        }
    }
}
