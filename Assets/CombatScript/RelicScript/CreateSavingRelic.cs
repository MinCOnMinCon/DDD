using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class CreateSavingRelic
{
    private const string OUTPUT_PATH = "Assets/CombatScript/RelicScript/RelicData/SavingRelics";

    [MenuItem("Tools/Relic/Create Saving RelicData")]
    public static void Create()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/CombatScript/RelicScript/RelicData", "SavingRelics");
        }

        var relics = new List<(int id, string name, string desc)>
        {
            (3,  "콤보 저축",
                "저축 슬롯에 저장된 주사위의 눈이 모두 같고 빈칸 없이 저축 슬롯을 채웠다면, 다음 턴에 상한치까지 주사위의 수치가 오른다."),

            (19, "오늘의 일은 내일로",
                "저축 슬롯에 패널티 주사위로 꽉 차 있으면 다음 턴에 한 턴동안 지속되는 대출 주사위 3개 추가"),

            (30, "V",
                "5를 저축하면 다음 턴에 수치가 저축 최대치가 된다.")
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

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("저축 RelicData 생성 완료");
    }
}
