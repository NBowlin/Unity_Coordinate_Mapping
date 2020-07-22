using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinMaxSlider : MonoBehaviour
{
    private float minX = 0f;
    private float maxX = 0f;

    private RectTransform rt;

    [SerializeField] private RectTransform leftHandle;
    [SerializeField] private RectTransform rightHandle;

    private void Start() {
        rt = (RectTransform)transform;
        minX = leftHandle.rect.width / 2;
        maxX = rt.rect.width - minX;
    }

    public void LeftHandleDragged(BaseEventData data) {
        var pointerEvent = (PointerEventData)data;
        var selectedRT = (RectTransform)pointerEvent.pointerDrag.transform;

        float toX = Mathf.Clamp(selectedRT.anchoredPosition.x + pointerEvent.delta.x, minX, maxX);
        selectedRT.anchoredPosition = new Vector2(toX, selectedRT.anchoredPosition.y);
    }
}
