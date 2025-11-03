using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class PuzzlePiece : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public int id;                           // 1부터 시작 가정
    public float baseSnapThreshold = 0.15f;  // 스냅 허용 거리(월드 거리)
    public float searchRadius = 1.0f;        // 그룹 탐색 반경

    private int rowLength = 10;         // 한 행에 7개 (1~7, 8~14, ...)
    private List<Transform> snapIns = new();
    private List<Transform> snapOuts = new();

    void Start()
    {
        rowLength = GameManager.instance.totalPieceCount == 35 ? 7 : 10;
        if (int.TryParse(gameObject.name, out int parsed))
            id = parsed;
        else
            Debug.LogWarning($"[PuzzlePiece] 이름이 숫자가 아님: {gameObject.name}");

        GetComponent<SpriteRenderer>().sortingOrder = id;

        snapIns.Clear();
        snapOuts.Clear();

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            string lower = t.name.ToLowerInvariant();
            if (lower.StartsWith("snapin"))  snapIns.Add(t);
            else if (lower.StartsWith("snapout")) snapOuts.Add(t);
        }

        if (snapIns.Count == 0 && snapOuts.Count == 0)
            Debug.LogWarning($"[PuzzlePiece {name}] SnapIn/SnapOut 포인트가 없습니다!");
    }

    public void TrySnapToNearbyPieces()
    {
        Transform myRoot = FindGroupRoot(transform) ?? transform;

        var myPieces = myRoot.GetComponentsInChildren<PuzzlePiece>(includeInactive: false);
        if (myPieces == null || myPieces.Length == 0) return;

        float threshold = baseSnapThreshold;

        PuzzlePiece bestOtherPiece = null;
        Transform bestOtherRoot = null;
        Transform bestMyAnchor = null;
        Transform bestOtherTarget = null;
        float bestDist = float.MaxValue;

        foreach (var myPiece in myPieces)
        {
            if (myPiece == null) continue;

            Collider2D[] hits = Physics2D.OverlapCircleAll(myPiece.transform.position, searchRadius);
            foreach (var h in hits)
            {
                if (!h) continue;
                var otherPiece = h.GetComponent<PuzzlePiece>();
                if (otherPiece == null) continue;

                Transform otherRoot = FindGroupRoot(otherPiece.transform) ?? otherPiece.transform;
                if (otherRoot == myRoot) continue; // 같은 그룹은 제외

                if (!AreGridNeighborsStrict(myPiece.id, otherPiece.id)) continue;

                foreach (var myOut in myPiece.snapOuts)
                {
                    foreach (var otherIn in otherPiece.snapIns)
                    {
                        if (!StrictDirectionalMatch(myOut.name, otherIn.name, myPiece.id, otherPiece.id)) continue;

                        float d = Vector2.Distance(myOut.position, otherIn.position);
                        if (d < threshold && d < bestDist)
                        {
                            bestDist = d;
                            bestOtherPiece = otherPiece;
                            bestOtherRoot = otherRoot;
                            bestMyAnchor = myOut;
                            bestOtherTarget = otherIn;
                        }
                    }
                }

                foreach (var myIn in myPiece.snapIns)
                {
                    foreach (var otherOut in otherPiece.snapOuts)
                    {
                        if (!StrictDirectionalMatch(otherOut.name, myIn.name, otherPiece.id, myPiece.id)) continue;

                        float d = Vector2.Distance(otherOut.position, myIn.position);
                        if (d < threshold && d < bestDist)
                        {
                            bestDist = d;
                            bestOtherPiece = otherPiece;
                            bestOtherRoot = otherRoot;
                            bestMyAnchor = myIn;
                            bestOtherTarget = otherOut;
                        }
                    }
                }
            }
        }

        if (bestOtherPiece == null || bestOtherRoot == null || bestMyAnchor == null || bestOtherTarget == null)
            return;

        Vector3 delta = myRoot.position - bestMyAnchor.position;
        myRoot.position = bestOtherTarget.position + delta;

        // 병합 실행
        MergeGroupsPreserveWorld(myRoot, bestOtherRoot);

        // 사운드
        AudioManager.instance?.PlaySound("Merge");

        // 🎯 병합 이후 즉시 완성 체크 (마지막 퍼즐 낱개도 포함)
        Transform mergedRoot = FindGroupRoot(myRoot) ?? myRoot;
        int count = mergedRoot.GetComponentsInChildren<PuzzlePiece>(true).Length;
        GameManager.instance?.CheckGameCompleted(count);
    }

    bool AreGridNeighborsStrict(int aId, int bId)
    {
        int aIdx = aId - 1, bIdx = bId - 1;
        int aRow = aIdx / rowLength, aCol = aIdx % rowLength;
        int bRow = bIdx / rowLength, bCol = bIdx % rowLength;

        // 좌우 이웃
        if (aRow == bRow && Mathf.Abs(aCol - bCol) == 1) return true;
        // 상하 이웃
        if (aCol == bCol && Mathf.Abs(aRow - bRow) == 1) return true;

        return false;
    }

    bool StrictDirectionalMatch(string aPointName, string bPointName, int aId, int bId)
    {
        string a = aPointName.ToLowerInvariant();
        string b = bPointName.ToLowerInvariant();

        if (!a.StartsWith("snapout")) return false; 
        if (!b.StartsWith("snapin"))  return false; 

        int aIdx = aId - 1, bIdx = bId - 1;
        int aRow = aIdx / rowLength, aCol = aIdx % rowLength;
        int bRow = bIdx / rowLength, bCol = bIdx % rowLength;

        if (aRow == bRow && bCol == aCol + 1) return a.EndsWith("_right") && b.EndsWith("_left");
        if (aRow == bRow && bCol == aCol - 1) return a.EndsWith("_left") && b.EndsWith("_right");
        if (aCol == bCol && bRow == aRow + 1) return a.EndsWith("_bottom") && b.EndsWith("_top");
        if (aCol == bCol && bRow == aRow - 1) return a.EndsWith("_top") && b.EndsWith("_bottom");

        return false;
    }

    Transform FindGroupRoot(Transform t)
    {
        Transform cur = t;
        Transform lastGroup = null;
        while (cur != null)
        {
            if (cur.name.StartsWith("Group_")) lastGroup = cur;
            cur = cur.parent;
        }
        return lastGroup;
    }

    void MergeGroupsPreserveWorld(Transform aAny, Transform bAny)
    {
        Transform rootA = FindGroupRoot(aAny) ?? aAny;
        Transform rootB = FindGroupRoot(bAny) ?? bAny;
        if (rootA == rootB) return;

        bool aIsGroup = rootA.name.StartsWith("Group_");
        bool bIsGroup = rootB.name.StartsWith("Group_");

        if (!aIsGroup && !bIsGroup)
        {
            GameObject group = new GameObject($"Group_{aAny.name}_{bAny.name}");
            aAny.SetParent(group.transform, true);
            bAny.SetParent(group.transform, true);
            return;
        }

        if (aIsGroup && !bIsGroup)
        {
            bAny.SetParent(rootA, true);
            return;
        }

        if (!aIsGroup && bIsGroup)
        {
            aAny.SetParent(rootB, true);
            return;
        }

        if (aIsGroup && bIsGroup)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform c in rootB) children.Add(c);

            var world = new Dictionary<Transform, (Vector3 pos, Quaternion rot)>();
            foreach (var c in children) world[c] = (c.position, c.rotation);

            foreach (var c in children) c.SetParent(rootA, true);

            foreach (var kv in world)
            {
                if (kv.Key != null)
                {
                    kv.Key.position = kv.Value.pos;
                    kv.Key.rotation = kv.Value.rot;
                }
            }

#if UNITY_EDITOR
            Object.DestroyImmediate(rootB.gameObject);
#else
            Object.Destroy(rootB.gameObject);
#endif
        }
    }
}
