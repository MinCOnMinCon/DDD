using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class CreateValueRelic
{
    private const string OUTPUT_PATH = "Assets/CombatScript/RelicScript/RelicData/ValueRelics";

    [MenuItem("Tools/Relic/Create Value RelicData")]
    public static void Create()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/CombatScript/RelicScript/RelicData", "ValueRelics");
        }

        var relics = new List<(int id, string name, string desc)>
        {
            (4,  "선빵필승", "전투의 첫 번째 턴에 공격 수치 +8"),
            (6,  "리버스 패널티",
                "패널티 주사위가 더 이상 눈만큼 수치를 감소시키지 않고 눈만큼 수치를 더한다.\n대신 전투 중 패널티 주사위가 7개가 되는 순간 즉사한다."),
            (10, "최선의 방어는", "방어에 주사위가 없을 때 공격 수치 +10"),
            (11, "레버리지", "패널티 주사위가 6개 이상이면 각 슬롯의 기본, 대출 주사위 수만큼 각 슬롯의 수치에 더한다."),
            (20, "레인보우", "공격이나 방어 슬롯에 주사위 눈이 1부터 6까지 최소 하나씩 있다면 그 슬롯의 수치를 두 배로 한다."),
            (22, "언더 독", "주사위 눈 1, 2, 3의 수치가 2만큼 증가한다."),
            (26, "올 인", "공격 슬롯에 있는 주사위 수 * 4만큼 공격 수치에 더한다."),
            (28, "필 사", "공격이나 방어 슬롯에 있는 주사위 눈 4의 개수만큼 공격 수치 4를 늘린다."),
            (29, "쉴드 치기", "방어 총 수치의 1/3만큼 공격 수치에 더한다."),
            (31, "allforone", "1을 제외한 눈의 수치를 2만큼 감소시킨다. 눈 1의 수치를 10만큼 증가시킨다."),
            (32, "인플레이션", "대출 주사위의 수치를 2만큼 늘린다."),
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

        Debug.Log("수치계산 RelicData 생성 완료");
    }
}
