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
    private float handleRange = 0f;

    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 100f;
    private float valueRange = 100f;

    public float currMinValue { get; private set; }
    public float currMaxValue { get; private set; }

    private void Start() {
        minX = leftHandle.rect.width / 2;
        maxX = ((RectTransform)transform).rect.width - minX;

        valueRange = maxValue - minValue;
        handleRange = maxX - minX;
    }

    //These functioned are called via an Event Trigger on the handles
    public void LeftHandleDragged(BaseEventData data) {
        var pointerEvent = (PointerEventData)data;

        float toX = Mathf.Clamp(leftHandle.anchoredPosition.x + pointerEvent.delta.x, minX, maxX);

        if (toX + leftHandle.rect.width / 2f >= rightHandle.anchoredPosition.x - rightHandle.rect.width / 2f) {
            if (rightHandle.anchoredPosition.x >= maxX) { return; }

            rightHandle.anchoredPosition = new Vector2((toX + leftHandle.rect.width / 2f) + (rightHandle.rect.width / 2f), rightHandle.anchoredPosition.y);
            currMaxValue = calculateSliderValue(rightHandle.anchoredPosition.x);
        }

        leftHandle.anchoredPosition = new Vector2(toX, leftHandle.anchoredPosition.y);
        currMinValue = calculateSliderValue(toX);
    }

    public void RightHandleDragged(BaseEventData data) {
        var pointerEvent = (PointerEventData)data;

        float toX = Mathf.Clamp(rightHandle.anchoredPosition.x + pointerEvent.delta.x, minX, maxX);

        if(toX - rightHandle.rect.width / 2f <= leftHandle.anchoredPosition.x + leftHandle.rect.width / 2f) {
            if(leftHandle.anchoredPosition.x <= minX) { return; }

            leftHandle.anchoredPosition = new Vector2((toX - rightHandle.rect.width / 2f) - (leftHandle.rect.width / 2f), leftHandle.anchoredPosition.y);
            currMinValue = calculateSliderValue(leftHandle.anchoredPosition.x);
        }

        rightHandle.anchoredPosition = new Vector2(toX, rightHandle.anchoredPosition.y);
        currMaxValue = calculateSliderValue(toX);
    }

    private float calculateSliderValue(float x) {
        return minValue + ((x - minX) / handleRange) * valueRange;
    }
}
