using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image borders;
    public Element Element { get; private set; } = null;
    private bool _selected;


    private int row;
    private int column;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_selected) return;
        
        borders.enabled = true;
        var color = borders.color;
        borders.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_selected) return;
        
        borders.enabled = false;
        var color = borders.color;
        borders.color = new Color(color.r, color.g, color.b, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_selected) return;
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                borders.enabled = true;
                GameManager.Instance.SelectTile(this);
                _selected = true;
                var color = borders.color;
                borders.color = new Color(color.r, color.g, color.b, 1f);
                break;
            case PointerEventData.InputButton.Right:
                if (GameManager.Instance.SelectedTile == null) return;
                SwapElements(GameManager.Instance.SelectedTile);
                GameManager.Instance.SelectTile(null);
                break;
            case PointerEventData.InputButton.Middle:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Deselect()
    {
        _selected = false;
        borders.enabled = false;
        var color = borders.color;
        borders.color = new Color(color.r, color.g, color.b, 1f);
    }

    public void AddElement(Element element, bool force = false)
    {
        if (!force && Element != null)
        {
            Debug.LogWarning("Trying to add element to already occupied cell. Reverting.");
            return;
        }

        Element = element;
        element.transform.position = transform.position;
        element.transform.SetParent(transform);
    }

    public void SwapElements(Tile otherTile)
    {
        var tempElement = Element;
        Element = null;
        AddElement(otherTile.Element);
        otherTile.AddElement(tempElement, true);
        
        FindObjectOfType<Grid>().CheckLines();
    }

    public void UpdatePosition(int column, int row)
    {
        this.row = column;
        this.column = row;
        Vector2 newPos = new Vector2();
        Vector2 worldOffset = new Vector2(GameConstants.WorldOffsetX, GameConstants.WorldOffsetY);
        Vector2 screenOffset = Camera.main.WorldToScreenPoint(worldOffset);
        
        newPos.x = this.row * GameConstants.TileSize + screenOffset.x;
        newPos.y = this.column * GameConstants.TileSize + screenOffset.y;
        transform.position = newPos;
    }

    public void CheckNeighbours()
    {
        
    }
}
