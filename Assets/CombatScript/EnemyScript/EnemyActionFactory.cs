using System;
using System.Collections.Generic;

public static class EnemyActionFactory
{
    private static readonly Dictionary<int, Func<EnemyAction>> factory
        = new()
        {
            { 1, () => new EnemyAction_1() }
        };

    public static EnemyAction Create(int enemyId)
    {
        if (factory.TryGetValue(enemyId, out var creator))
            return creator();

        return null;
    }
}