using UnityEngine;
using System.Collections.Generic;

public class PuzzleDrag : MonoBehaviour
{
    [Header("Settings")]
    public bool dragWholeGroup = true;
    public bool lockZ = true;
    public float zValue = 0f;
    public float clickHoldThreshold = 0.15f; // 빠른 클릭 → 홀드모드

    private Camera cam;
    private Transform target;
    private Transform root;
    private Vector3 grabOffset;
    private bool dragging = false;
    private bool holdMode = false;
    private float clickTime = 0f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        // 🎯 퍼즐을 "홀드" 중이면 마우스를 계속 따라감
        if (holdMode && root != null)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            if (lockZ) worldPos.z = zValue;
            root.position = worldPos + grabOffset;
        }

        // 👇 클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            clickTime = Time.time;

            RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                var piece = hit.collider.GetComponent<PuzzlePiece>();
                piece.GetComponent<SpriteRenderer>().sortingOrder = 1;
                if (piece != null)
                {
                    target = piece.transform;
                    root = dragWholeGroup ? (FindGroupRoot(target) ?? target) : target;

                    Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                    if (lockZ) worldPos.z = zValue;
                    grabOffset = root.position - worldPos;

                    dragging = true;

                    // 🎯 드래그 시작 시 커서 숨김
                    Cursor.visible = false;
                }
            }
        }

        // 👇 클릭 유지 (일반 드래그 중)
        if (Input.GetMouseButton(0) && dragging && !holdMode)
        {
            if (root == null) return;
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            if (lockZ) worldPos.z = zValue;
            root.position = worldPos + grabOffset;
        }

        // 👇 마우스 버튼 뗐을 때
        if (Input.GetMouseButtonUp(0))
        {
            float clickDuration = Time.time - clickTime;

            // 🎯 짧은 클릭이면 → 홀드 모드 전환
            if (clickDuration <= clickHoldThreshold)
            {
                if (!holdMode)
                {
                    // 홀드 시작
                    if (target != null)
                    {
                        root = dragWholeGroup ? (FindGroupRoot(target) ?? target) : target;

                        Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                        if (lockZ) worldPos.z = zValue;
                        grabOffset = root.position - worldPos;

                        holdMode = true;
                        Cursor.visible = false; // 홀드 모드 시작 시 커서 숨김
                    }
                }
                else
                {
                    // 홀드 해제
                    holdMode = false;
                    Cursor.visible = true; // 놓을 때 커서 다시 표시

                    target.GetComponent<SpriteRenderer>().sortingOrder = 0;
                    if (root != null)
                        TrySnapAll(root);

                    root = null;
                    target = null;
                }
            }
            else
            {
                // 일반 드래그 종료
                if (dragging)
                {
                    dragging = false;
                    Cursor.visible = true; // 🎯 드래그 끝났을 때 커서 다시 표시
                    target.GetComponent<SpriteRenderer>().sortingOrder = 0;

                    if (root != null)
                        TrySnapAll(root);

                    root = null;
                    target = null;
                }
            }
        }
    }

    void TrySnapAll(Transform root)
    {
        var pieces = root.GetComponentsInChildren<PuzzlePiece>(false);
        foreach (var p in pieces)
        {
            if (p == null) continue;
            try { p.TrySnapToNearbyPieces(); }
            catch (MissingReferenceException) { continue; }
        }
    }

    Transform FindGroupRoot(Transform t)
    {
        Transform current = t;
        Transform group = null;
        while (current != null)
        {
            if (current.name.StartsWith("Group_"))
                group = current;
            current = current.parent;
        }
        return group;
    }
}
