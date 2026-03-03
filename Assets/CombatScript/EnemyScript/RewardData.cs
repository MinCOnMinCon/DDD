using UnityEngine;
using System.Collections.Generic;

// Unity 에디터 메뉴: Create -> DDD -> Reward Data 를 통해 생성할 수 있습니다.
[CreateAssetMenu(menuName = "Game/Reward Data")]
public class RewardData : ScriptableObject
{
    // 각 레벨별 보상 정보를 리스트로 관리합니다.
    public List<RewardEntry> rewards;

    // 레벨을 key로 하여 보상 정보를 찾아주는 메소드
    public RewardEntry GetReward(int level)
    {
        // 리스트에서 매칭되는 레벨의 보상 정보를 찾아서 반환합니다.
        return rewards.Find(entry => entry.level == level);
    }
}

// 각 레벨별 보상 정보를 담는 클래스
// [System.Serializable]을 붙여야 Unity Inspector에 노출됩니다.
[System.Serializable]
public class RewardEntry
{
    public int level;       // 보상 레벨
    public int money;        // 지급할 돈
    public int shopGauge;   // 채워줄 상점 게이지
}
