using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class CreateInstantRelic
{
    private const string OUTPUT_PATH = "Assets/CombatScript/RelicScript/RelicData/InstantRelics";

    [MenuItem("Tools/Relic/Create Instant RelicData (1,5)")]
    public static void Create()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/CombatScript/RelicScript/RelicData", "InstantRelics");
        }

        var relics = new List<(int id, string name, string desc)>
        {
            (1, "영구 대출",
                "대출 주사위는 전투 동안 사라지지 않는다."),

            (5, "vip 저축",
                "저축 수치 한도가 30까지 증가")
        };

        foreach (var r in relics)
        {
            var asset = ScriptableObject.CreateInstance<RelicData>();
            asset.id = r.id;
            asset.relicName = r.name;
            asset.description = r.desc;

            string safeName = r.name.Replace(" ", "_");
            string path = $"{OUTPUT_PATH}/Relic_{r.id}_{safeName}.asset";

            AssetDatabase.CreateAsset(asset, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("즉발 RelicData (1,5) 생성 완료");
    }
}
