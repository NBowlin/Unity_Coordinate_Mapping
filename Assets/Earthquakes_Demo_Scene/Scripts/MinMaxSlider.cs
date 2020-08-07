using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

[System.Serializable] public class SliderDragEvent : UnityEvent<float, float> { }
[System.Serializable] public class SliderStopEvent : UnityEvent<bool> { }

public class MinMaxSlider : MonoBehaviour
{
    private float minX = 0f;
    private float maxX = 0f;

    [SerializeField] private RectTransform fillBar;
    [SerializeField] private RectTransform leftHandle;
    [SerializeField] private RectTransform rightHandle;
    private float handleRange = 0f;

    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 100f;
    private float valueRange = 100f;

    private float _currMinValue;
    public float currMinValue {
        get { return _currMinValue; }
        private set {
            _currMinValue = value;
            minText.text = value.ToString("F1");
            if(valueChanged != null) { valueChanged.Invoke(currMinValue, currMaxValue); }
        }
    }

    private float _currMaxValue;
    public float currMaxValue {
        get { return _currMaxValue; }
        private set {
            _currMaxValue = value;
            maxText.text = value.ToString("F1");
            if (valueChanged != null) { valueChanged.Invoke(currMinValue, currMaxValue); }
        }
    }

    [SerializeField] private TextMeshProUGUI minText;
    private Vector2 minTextOffset;

    [SerializeField] private TextMeshProUGUI maxText;
    private Vector2 maxTextOffset;

    public SliderDragEvent valueChanged;
    public SliderStopEvent didEndDragging;

    private void Start() {
        minX = leftHandle.rect.width / 2;
        maxX = ((RectTransform)transform).rect.width - minX;

        valueRange = maxValue - minValue;
        handleRange = maxX - minX;

        minTextOffset = new Vector2(leftHandle.anchoredPosition.x - minText.rectTransform.anchoredPosition.x, leftHandle.anchoredPosition.y - minText.rectTransform.anchoredPosition.y);
        maxTextOffset = new Vector2(maxText.rectTransform.anchoredPosition.x - rightHandle.anchoredPosition.x, maxText.rectTransform.anchoredPosition.y - rightHandle.anchoredPosition.y);

        currMinValue = minValue;
        currMaxValue = maxValue;
    }

    //These functioned are called via an Event Trigger on the handles
    public void LeftHandleDragged(BaseEventData data) {
        var pointerEvent = (PointerEventData)data;

        float toX = Mathf.Clamp(leftHandle.anchoredPosition.x + pointerEvent.delta.x, minX, maxX);

        if (toX + leftHandle.rect.width / 2f >= rightHandle.anchoredPosition.x - rightHandle.rect.width / 2f) {
            if (rightHandle.anchoredPosition.x >= maxX) { return; }
            UpdateRightHandleValues((toX + leftHandle.rect.width / 2f) + (rightHandle.rect.width / 2f));
        }

        UpdateLeftHandleValues(toX);
    }

    public void RightHandleDragged(BaseEventData data) {
        var pointerEvent = (PointerEventData)data;

        float toX = Mathf.Clamp(rightHandle.anchoredPosition.x + pointerEvent.delta.x, minX, maxX);

        if(toX - rightHandle.rect.width / 2f <= leftHandle.anchoredPosition.x + leftHandle.rect.width / 2f) {
            if(leftHandle.anchoredPosition.x <= minX) { return; }
            UpdateLeftHandleValues((toX - rightHandle.rect.width / 2f) - (leftHandle.rect.width / 2f));
        }

        UpdateRightHandleValues(toX);
    }

    private void UpdateLeftHandleValues(float toX) {
        leftHandle.anchoredPosition = new Vector2(toX, leftHandle.anchoredPosition.y);
        fillBar.offsetMin = new Vector2(leftHandle.anchoredPosition.x, fillBar.offsetMin.y);
        minText.rectTransform.anchoredPosition = leftHandle.anchoredPosition - minTextOffset;
        currMinValue = calculateSliderValue(toX);
    }

    private void UpdateRightHandleValues(float toX) {
        rightHandle.anchoredPosition = new Vector2(toX, rightHandle.anchoredPosition.y);
        fillBar.offsetMax = new Vector2(rightHandle.anchoredPosition.x - maxX, fillBar.offsetMax.y);
        maxText.rectTransform.anchoredPosition = rightHandle.anchoredPosition + maxTextOffset;
        currMaxValue = calculateSliderValue(toX);
    }

    public void SliderDidEndDragging(BaseEventData data) {
        if (didEndDragging != null) { didEndDragging.Invoke(true); }
    }

    private float calculateSliderValue(float x) {
        return minValue + ((x - minX) / handleRange) * valueRange;
    }
}