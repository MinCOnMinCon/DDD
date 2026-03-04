using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지의 각 분기점을 나타내는 타일 한 쌍과 클리어 여부를 저장하는 래퍼 클래스입니다.
/// </summary>
[System.Serializable]
public class TileWrapper
{
    public TileData tileA;
    public TileData tileB;
    public bool isCleared;

    public TileWrapper(TileData tileA, TileData tileB)
    {
        this.tileA = tileA;
        this.tileB = tileB;
        this.isCleared = false;
    }
}

public class TileManager : MonoBehaviour
{
    public static TileManager inst;
    // 스테이지의 전체 타일 데이터를 관리하는 리스트
    public List<TileWrapper> stageTiles;
    
    public List<GameObject> tileButton;
    
    // 현재 플레이어의 위치 (노드 인덱스 등)
    private int curPlayerNode;
    private int[,] tilePos = new int[2,2];

    private void Awake()
    {
        stageTiles = new List<TileWrapper>();
        tileButton = new List<GameObject>();
        inst = this;
    }
    void Start()
    {
        // TODO: 스테이지 데이터 에셋을 불러와서 stageTiles 리스트를 초기화해야 합니다.
        // 예: stageTiles = LoadStageData("Stage1").tileNodes;

        GenerateStageTiles();
        CreateChoiceButtons();
    }

    /// <summary>
    /// 스테이지 시작 시 전체 타일 오브젝트를 생성하는 함수 (추후 구현)
    /// </summary>
    public void GenerateStageTiles()
    {
        // 전체 스테이지 맵을 시각적으로 생성하는 로직이 들어갑니다.
        // 예: 각 TileWrapper를 순회하며 비활성화된 타일 오브젝트를 미리 생성
        Debug.Log("Generating entire stage layout...");
    }

    /// <summary>
    /// 플레이어가 선택할 수 있는 2개의 타일 버튼을 생성하는 함수
    /// </summary>
    public void CreateChoiceButtons()
    {
        // currentPlayerPosition에 해당하는 TileWrapper를 가져옵니다.
        // TileWrapper의 isCleared가 false일 경우에만 버튼을 생성합니다.
        // 각 버튼에 TileWrapper의 tileA, tileB 데이터를 할당하고,
        // 버튼 클릭 시 ClassifyAndExecuteTile 함수를 호출하도록 설정합니다.
        Debug.Log("Creating choice buttons for the player...");
    }

    /// <summary>
    /// 타일 데이터를 받아 종류에 따라 적절한 함수를 호출하는 분류 함수
    /// </summary>
    /// <param name="tileData">선택된 타일의 데이터</param>
    public void ClassifyTile(TileData tileData)
    {
        switch (tileData.tileType)
        {
            case TileType.Enemy:
                HandleEnemyTile(tileData);
                break;
            case TileType.Event:
                HandleEventTile(tileData);
                break;
            case TileType.Treasure:
                HandleTreasureTile(tileData);
                break;
            default:
                Debug.LogError("Unknown tile type!");
                break;
        }

        // TODO: 타일 선택 후 처리
        // 예: 현재 위치의 TileWrapper의 isCleared를 true로 설정
        // currentPlayerPosition을 다음 위치로 업데이트
        // CreateChoiceButtons() 다시 호출
    }

    /// <summary>
    /// 적 타일을 처리하는 함수
    /// </summary>
    private void HandleEnemyTile(TileData tileData)
    {
        // tileData를 EnemyTileData로 캐스팅합니다.
        // 전투에 필요한 정보를 PersistObject 같은 곳에 저장합니다.
        // CombatScene으로 씬을 전환합니다.
        Debug.Log("Handling Enemy Tile. Transitioning to Combat Scene...");
    }

    /// <summary>
    /// 이벤트 타일을 처리하는 함수
    /// </summary>
    private void HandleEventTile(TileData tileData)
    {
        // tileData를 EventTileData로 캐스팅합니다.
        // 이벤트 타입에 맞는 UI 창을 띄웁니다.
        Debug.Log("Handling Event Tile. Opening event window...");
    }

    /// <summary>
    /// 보물 타일을 처리하는 함수
    /// </summary>
    private void HandleTreasureTile(TileData tileData)
    {
        // tileData를 TreasureTileData로 캐스팅합니다.
        // 보물 등급에 맞는 보상 UI 창을 띄웁니다.
        Debug.Log("Handling Treasure Tile. Opening treasure window...");
    }
}
