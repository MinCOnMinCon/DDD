using UnityEngine;

/// <summary>
/// 스테이지를 구성하는 타일의 종류를 정의합니다.
/// </summary>
public enum TileType
{
    Enemy,
    Event,
    Treasure
}
public enum EventType
{
    Exchange,
    Upgrade
}

/// <summary>
/// 모든 타일 데이터가 상속받는 기본 클래스입니다.
/// ScriptableObject를 상속받아 에셋으로 관리할 수 있습니다.
/// </summary>
public class TileData : ScriptableObject
{
    [Header("Base Tile Data")]
    public TileType tileType;

}

public class EnemyTile : TileData
{
    public int enemyLevel;
    
}

public class EventTile : TileData
{
    public EventType eventType;
}

public class TreasureTile : TileData
{
    public int treasureLevel;
}
