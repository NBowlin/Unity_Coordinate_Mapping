using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinMaxSlider : MonoBehaviour, IDragHandler
{
    private float minX = 0f;
    private float maxX = 0f;

    RectTransform rt;

    private void Start() {
        rt = (RectTransform)transform;
        minX = rt.rect.width / 2;
        maxX = ((RectTransform)rt.parent).rect.width - minX;
        Debug.Log(gameObject.name + ": " + minX + "/" + maxX + " AnchorP: " + rt.anchoredPosition + " LocalP: " + rt.localPosition + " WorldP: " + rt.position);
    }

    public void OnDrag(PointerEventData eventData) {
        float toX = Mathf.Clamp(rt.anchoredPosition.x + eventData.delta.x, minX, maxX);
        Debug.Log("Local: " + rt.anchoredPosition.x + " Delta: " + eventData.delta.x + " ToX: " + toX);
        rt.anchoredPosition = new Vector2(toX, rt.anchoredPosition.y);
    }
}
