using UnityEngine;

// ---------------------------------

// 수치계산 - 1 (IDiceValueChangeRelic)
// ---------------------------------

// 6. 리버스 패널티
public class Relic_6 : RelicEffect, IDiceValueChangeRelic
{
    public Relic_6() : base(6) { }

    public void Activate(DiceData dice)
    {
        if (dice.diceType == DiceType.penalty)
        {
            dice.diceValue = -dice.diceValue;
        }
    }
}

// 22. 언더 독
public class Relic_22 : RelicEffect, IDiceValueChangeRelic
{
    public Relic_22() : base(22) { }

    public void Activate(DiceData dice)
    {
        if (dice.diceEye <= 3)
        {
            dice.diceValue += 2;
        }
    }
}

// 31. allforone
public class Relic_31 : RelicEffect, IDiceValueChangeRelic
{
    public Relic_31() : base(31) { }

    public void Activate(DiceData dice)
    {
        if (dice.diceEye == 1)
            dice.diceValue += 10;
        else
            dice.diceValue -= 2;
    }
}

// 32. 인플레이션
public class Relic_32 : RelicEffect, IDiceValueChangeRelic
{
    public Relic_32() : base(32) { }

    public void Activate(DiceData dice)
    {
        if (dice.diceType == DiceType.loan)
        {
            dice.diceValue += 2;
        }
    }
}

// ---------------------------------
// 수치계산 - 2 (ISnapshotConditionRelic)
// ---------------------------------

// 4. 선빵필승
public class Relic_4 : RelicEffect, ISnapshotConditionRelic
{
    public Relic_4() : base(4) { }

    public int Activate(SlotSnapshot snapshot, CombatContext ctx)
    {
        if (ctx.turnCount == 1 && snapshot.slotRole == DiceSlotRole.Attack)
        {
            ctx.snapshot.calcAttackValue += 8;
        }
        return 0;
    }
}

// 10. 최선의 방어는
public class Relic_10 : RelicEffect, ISnapshotConditionRelic
{
    public Relic_10() : base(10) { }

    public int Activate(SlotSnapshot snapshot, CombatContext ctx)
    {
        if (snapshot.slotRole == DiceSlotRole.Defense &&
            snapshot.totalDiceCount == 0)
        {
            ctx.snapshot.calcAttackValue += 10;
        }
        return 0;
    }
}

// 11. 레버리지
public class Relic_11 : RelicEffect, ISnapshotConditionRelic
{
    public Relic_11() : base(11) { }

    public int Activate(SlotSnapshot snapshot, CombatContext ctx)
    {
        if (snapshot.penaltyDiceCount >= 6)
        {
            int bonus = snapshot.basicDiceCount + snapshot.loanDiceCount;

            if (snapshot.slotRole == DiceSlotRole.Attack)
                ctx.snapshot.calcAttackValue += bonus;
            else if (snapshot.slotRole == DiceSlotRole.Defense)
                ctx.snapshot.calcDefenseValue += bonus;
        }
        return 0;
    }
}

// 20. 레인보우
public class Relic_20 : RelicEffect, ISnapshotConditionRelic
{
    public Relic_20() : base(20) { }

    public int Activate(SlotSnapshot snapshot, CombatContext ctx)
    {
        for (int i = 1; i <= 6; i++)
        {
            if (!snapshot.eyeMap.ContainsKey(i))
                return 0;
        }

        if (snapshot.slotRole == DiceSlotRole.Attack)
            ctx.snapshot.calcAttackValue *= 2;
        else if (snapshot.slotRole == DiceSlotRole.Defense)
            ctx.snapshot.calcDefenseValue *= 2;

        return 0;
    }
}

// 28. 필 사
public class Relic_28 : RelicEffect, ISnapshotConditionRelic
{
    public Relic_28() : base(28) { }

    public int Activate(SlotSnapshot snapshot, CombatContext ctx)
    {
        if (!snapshot.eyeMap.TryGetValue(4, out var eye))
            return 0;

        int bonus = eye.totalCount * 4;

        if (snapshot.slotRole == DiceSlotRole.Attack)
            ctx.snapshot.calcAttackValue += bonus;
        else if (snapshot.slotRole == DiceSlotRole.Defense)
            ctx.snapshot.calcDefenseValue += bonus;

        return 0;
    }
}

// ---------------------------------
// 수치계산 - 3 (IFinalValueRelic)
// ---------------------------------

// 26. 올 인
public class Relic_26 : RelicEffect, IFinalValueRelic
{
    public Relic_26() : base(26) { }

    public void Activate(CombatContext ctx)
    {
        ctx.snapshot.calcAttackValue += ctx.attackSlotDiceList.Count * 4;
    }
}

// 29. 쉴드 치기
public class Relic_29 : RelicEffect, IFinalValueRelic
{
    public Relic_29() : base(29) { }

    public void Activate(CombatContext ctx)
    {
        ctx.snapshot.calcAttackValue += ctx.snapshot.calcDefenseValue / 3;
    }
}

