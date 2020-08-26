using System;
using System.Collections;
using DefaultNamespace;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image borders;
    public Element Element { get; private set; } = null;
    private bool _selected;

    public event Action MovingAnimationEnded;
    
    public int Row { get;  set; }
    public int Column { get;  set; }

    public bool IsSwapping;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.SelectedTile == this) return;
        HighlightBorder(0.5f);
        
        // if (_selected) return;
        //
        // borders.enabled = true;
        // var color = borders.color;
        // borders.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.Instance.SelectedTile == this) return;
        HighlightBorder(0f);
        // if (_selected || Grid.ValidSwapTiles.Contains(this)) return;
        //
        // borders.enabled = false;
        // var color = borders.color;
        // borders.color = new Color(color.r, color.g, color.b, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || Element == null) return;
        // GameManager.Instance.SelectTile(this);
    }

    public void HighlightBorder(float opacity = 1f)
    {
        if (opacity == 0)
        {
            borders.enabled = false;
            return;
        }
        // Utils.SetColorAlpha(ref borders.color, opacity);
        borders.enabled = true;
        var borderColor = borders.color;
        borders.color = new Color(borderColor.r, borderColor.g, borderColor.b, opacity);
    }

    public void Deselect()
    {
        _selected = false;
        borders.enabled = false;
        var color = borders.color;
        borders.color = new Color(color.r, color.g, color.b, 1f);
    }

    public void AddElement(Element element, bool animate = false)
    {
        // animate = false;
        if (Element != null)
        {
            Debug.LogWarning("Trying to add element to already occupied cell. Reverting.");
            return;
        }
        
        if (element == null) return;

        // element.transform.position = transform.position;
        if (animate)
        {
            IsSwapping = true;
            StartCoroutine(AnimateElementMove(element, element.transform.position, transform.position));
        }
        else
        {
            Element = element;
            element.transform.SetParent(transform);
            element.transform.position = transform.position;
        }
    }

    IEnumerator AnimateElementMove(Element element, Vector2 startPosition, Vector2 targetPosition)
    {
        element.IsMoving = true;
        element.transform.SetParent(FindObjectOfType<Canvas>().transform);
        for (float t = 0; t < 1; t += Time.deltaTime * 3)
        {
            element.transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        element.transform.SetParent(transform);
        element.transform.position = targetPosition;
        element.IsMoving = false;
        Element = element;
        IsSwapping = false;
        MovingAnimationEnded?.Invoke();
        MovingAnimationEnded = null;
    }

    public void UpdatePosition()
    {
        Vector2 newPos = new Vector2();
        Vector2 worldOffset = new Vector2(GameConstants.WorldOffsetX, GameConstants.WorldOffsetY);
        Vector2 screenOffset = Camera.main.WorldToScreenPoint(worldOffset);
        
        newPos.x = Column * GameConstants.TileSize + screenOffset.x;
        newPos.y = Row * GameConstants.TileSize + screenOffset.y;
        transform.position = newPos;
    }

    public void SetElement(Element element)
    {
        Element = element;
    }

    public bool IsValidToSwapWith(Tile tile)
    {
        return Mathf.Abs(tile.Column - Column) + Mathf.Abs(tile.Row - Row) == 1;
        
        // return new Vector2(tile.Column, tile.Row) - new Vector2(Column, Row) == Vector2.one;
        // return tile.Column == Column + 1 || tile.Column == Column - 1 || tile.Row == Row + 1 || tile.Row == Row - 1;
    }

    public void RemoveElement(bool destroy)
    {
        if (destroy) Element.Match();
        Element = null;
    }
}
