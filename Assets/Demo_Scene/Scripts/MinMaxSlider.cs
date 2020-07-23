using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinMaxSlider : MonoBehaviour
{
    private float minX = 0f;
    private float maxX = 0f;

    [SerializeField] private RectTransform leftHandle;
    [SerializeField] private RectTransform rightHandle;

    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 100f;

    private void Start() {
        minX = leftHandle.rect.width / 2;
        maxX = ((RectTransform)transform).rect.width - minX;
    }

    //These functioned are called via an Event Trigger on the handles
    public void LeftHandleDragged(BaseEventData data) {
        var pointerEvent = (PointerEventData)data;

        float toX = Mathf.Clamp(leftHandle.anchoredPosition.x + pointerEvent.delta.x, minX, maxX);

        if (toX + leftHandle.rect.width / 2f >= rightHandle.anchoredPosition.x - rightHandle.rect.width / 2f) {
            if (rightHandle.anchoredPosition.x >= maxX) { return; }

            rightHandle.anchoredPosition = new Vector2((toX + leftHandle.rect.width / 2f) + (rightHandle.rect.width / 2f), rightHandle.anchoredPosition.y);
        }

        leftHandle.anchoredPosition = new Vector2(toX, leftHandle.anchoredPosition.y);
    }

    public void RightHandleDragged(BaseEventData data) {
        var pointerEvent = (PointerEventData)data;

        float toX = Mathf.Clamp(rightHandle.anchoredPosition.x + pointerEvent.delta.x, minX, maxX);

        if(toX - rightHandle.rect.width / 2f <= leftHandle.anchoredPosition.x + leftHandle.rect.width / 2f) {
            if(leftHandle.anchoredPosition.x <= minX) { return; }

            leftHandle.anchoredPosition = new Vector2((toX - rightHandle.rect.width / 2f) - (leftHandle.rect.width / 2f), leftHandle.anchoredPosition.y);
        }

        rightHandle.anchoredPosition = new Vector2(toX, rightHandle.anchoredPosition.y);
    }
}
