using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PuzzleAdjacencyManager : MonoBehaviour
{
    public static PuzzleAdjacencyManager Instance { get; private set; }

    public enum Direction { Left, Right, Top, Bottom }

    [System.Serializable]
    public class AdjacencyEntry
    {
        public int fromId;      // from 퍼즐 id
        public int toId;        // to 퍼즐 id
        public Direction dir;   // "to" 가 from의 어느 쪽에 붙어야 하는지
        // 예: dir == Right => to는 from의 오른쪽에 있어야 함
    }

    // Inspector에서 편집
    public List<AdjacencyEntry> rules = new();

    void Awake()
    {
        Instance = this;
    }

    // 검사 유틸: from->to 규칙이 있는지 찾기
    public bool TryGetRule(int fromId, int toId, out Direction dir)
    {
        foreach (var r in rules)
        {
            if (r.fromId == fromId && r.toId == toId)
            {
                dir = r.dir;
                return true;
            }
        }
        dir = default;
        return false;
    }

    // 전체 규칙 존재 여부 (디버그 용)
    public bool HasAnyRules() => rules != null && rules.Count > 0;
}