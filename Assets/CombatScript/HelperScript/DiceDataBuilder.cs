using System.Collections.Generic;
using UnityEngine;

public static class DiceDataBuilder
{
    /// <summary>
    /// 컴뱃 컨텍스트의 각 슬롯에 있는 주사위 오브젝트 리스트를 파라미터로 주면 각 주사위의 다이스 데이터로 리스트를 구성해 리턴
    /// </summary>
    /// <param name="diceList"></param>
    /// <returns></returns>
    public static List<DiceData> BuildDiceDataList(IReadOnlyList<GameObject> diceList)
    {
        List<DiceData> result = new();

        if (diceList == null)
            return result;

        foreach (var dice in diceList)
        {
            if (dice == null) continue;

            Dice diceComp = dice.GetComponent<Dice>();
            if (diceComp == null) continue;

            result.Add(diceComp.diceData);
        }

        return result;
    }
}