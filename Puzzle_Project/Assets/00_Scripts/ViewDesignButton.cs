using UnityEngine;
using UnityEngine.EventSystems;

public class ViewDesignButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject DimDesign;
    public void OnPointerEnter(PointerEventData eventData)
    {
        DimDesign.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DimDesign.SetActive(false);
    }
}
