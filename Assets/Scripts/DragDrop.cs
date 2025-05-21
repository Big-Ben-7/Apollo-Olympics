using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IDragHandler
{
    RectTransform myT;
    [SerializeField] Canvas canvas;

    void Awake()
    {
        myT = GetComponent<RectTransform>();
        myT.anchoredPosition = Vector2.zero;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        myT.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    void Update()
    {
        Vector2 newPos = myT.anchoredPosition;
        newPos.x = Mathf.Clamp(newPos.x, -320, 320);
        newPos.y = Mathf.Clamp(newPos.y, -115, 120);
        myT.anchoredPosition = newPos;
    }
}
