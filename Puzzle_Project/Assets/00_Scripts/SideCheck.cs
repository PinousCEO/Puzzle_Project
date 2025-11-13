using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SideCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("툴팁 UI 전체 (Text의 부모 오브젝트)")]
    public RectTransform tooltipRoot;   // 설명창 전체
    public TMP_Text tooltipText;         // 텍스트 컴포넌트
    public string message;               // 표시할 설명 문구

    [Header("위치 설정")]
    public float offsetY = -60f;         // 마우스 아래로 떨어질 거리
    public float followSpeed = 20f;      // 이동 속도

    private bool isHovering = false;
    private bool isInitialized = false;  // 최초 표시 시 바로 위치 지정용

    void Start()
    {
        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isHovering || tooltipRoot == null) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 targetPos = mousePos + new Vector3(0, offsetY, 0);

        if (!isInitialized)
        {
            tooltipRoot.position = targetPos;
            isInitialized = true;
        }
        else
        {
            tooltipRoot.position = Vector3.Lerp(
                tooltipRoot.position,
                targetPos,
                Time.deltaTime * followSpeed
            );
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        isInitialized = false;

        if (tooltipRoot != null)
        {
            tooltipRoot.gameObject.SetActive(true);
            tooltipText.text = message;

            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRoot);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }
}
