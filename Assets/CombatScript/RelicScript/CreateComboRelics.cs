using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class CreateComboRelics
{
    private const string OUTPUT_PATH = "Assets/CombatScript/RelicScript/RelicData/ComboRelics";

    [MenuItem("Tools/Relic/Create Combo RelicData")]
    public static void Create()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/Relics", "ComboRelics");
        }

        var relics = new List<(int id, string name, string desc)>
        {
            (8,  "짝수 콤보",
                "짝수 눈끼리 콤보를 발동할 수 있다. 콤보를 대표하는 눈은 가장 수가 많은 눈이 된다. 획득 시 패널티 주사위 +3"),

            (9,  "홀수 콤보",
                "홀수 눈끼리 콤보를 발동할 수 있다. 콤보를 대표하는 눈은 가장 수가 많은 눈이 된다. 획득 시 패널티 주사위 +3"),

            (14, "극과 극",
                "콤보를 만들 때 주사위 눈 1을 6으로 취급한다."),

            (18, "3+3+3",
                "공격이나 방어 슬롯에서 주사위 눈 3으로 콤보 수 3 이상이면 다음 턴에 전투 동안 지속되는 기본 주사위 1개 추가"),

            (23, "연타",
                "콤보 수가 4 이상이면 콤보 수가 2만큼 증가한다."),

            (24, "only one",
                "공격 슬롯에서 1로 콤보 수 5 이상을 만들었다면 그 턴에 공격 수치 40을 추가한다."),

            (25, "2의2승",
                "2로 발생하는 콤보는 콤보 수치 계산이 2^(콤보 수 - 1)로 바뀐다."),

            (27, "콤보 회복",
                "방어에서 얻는 콤보 수치만큼 체력을 회복한다."),
        };

        foreach (var r in relics)
        {
            var asset = ScriptableObject.CreateInstance<RelicData>();
            asset.id = r.id;
            asset.relicName = r.name;
            asset.description = r.desc;

            string path = $"{OUTPUT_PATH}/Relic_{r.id}_{r.name}.asset";
            AssetDatabase.CreateAsset(asset, path);
        }

    }
}
