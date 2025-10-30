using UnityEngine;
using System.Collections.Generic;

public class PuzzleScatterKeepGrid_ShuffleChildren : MonoBehaviour
{
    [Header("Grid Settings (기존 방식 유지)")]
    public int columns = 12;
    public int rows = 8;
    public float spacing = 1.5f;
    public float centerClearRadius = 3f;

    [Header("Options")]
    public bool includeInactiveChildren = false; // 비활성 자식 포함 여부
    public int? randomSeed = null;               // 재현성 필요하면 시드 지정

    private readonly List<Transform> _children = new List<Transform>();
    private readonly List<Vector2> _gridPositions = new List<Vector2>();

    void Start()
    {
        BuildChildList();
        BuildGridPositions_KeepOldLogic();     // ★ 기존 그리드/중앙 비움 로직 그대로
        ShuffleChildrenOnly();                 // ★ 자식만 섞는다!
        MapChildrenToGrid();                   // ★ 섞인 순서대로 그리드에 배치
    }

    void BuildChildList()
    {
        _children.Clear();
        foreach (Transform child in transform)
        {
            if (includeInactiveChildren || child.gameObject.activeSelf)
                _children.Add(child);
        }
    }

    void BuildGridPositions_KeepOldLogic()
    {
        _gridPositions.Clear();

        // 기존 그리드 계산 (중앙 기준 정렬/비움 로직은 그대로 유지)
        float startX = -((columns - 1) * spacing) / 2f;
        float startY = -((rows - 1) * spacing) / 2f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector2 pos = new Vector2(startX + x * spacing, startY + y * spacing);

                // 중앙 비움 (원형 기준)
                if (pos.magnitude < centerClearRadius)
                    continue;

                _gridPositions.Add(pos);
            }
        }

        // ★ 기존 “퍼지는 모습”을 유지하려면, 예전에 쓰던 정렬 그대로 두세요.
        // 예: 위/아래 먼저 퍼지는 느낌: |y| 큰 순 → |x| 큰 순
        _gridPositions.Sort((a, b) =>
        {
            int byY = Mathf.Abs(b.y).CompareTo(Mathf.Abs(a.y));
            if (byY != 0) return byY;
            return Mathf.Abs(b.x).CompareTo(Mathf.Abs(a.x));
        });
    }

    void ShuffleChildrenOnly()
    {
        if (randomSeed.HasValue) Random.InitState(randomSeed.Value);

        // Fisher–Yates
        for (int i = 0; i < _children.Count; i++)
        {
            int r = Random.Range(i, _children.Count);
            ( _children[i], _children[r] ) = ( _children[r], _children[i] );
        }
    }

    void MapChildrenToGrid()
    {
        int count = Mathf.Min(_children.Count, _gridPositions.Count);
        for (int i = 0; i < count; i++)
        {
            _children[i].localPosition = _gridPositions[i];
            // 회전/오프셋 건드리지 않음 → “그리드 형태” 그대로 유지
        }
        // 자식이 더 많으면 나머지는 그대로 두거나, 필요하면 밖으로 빼는 처리 추가 가능
    }
}
