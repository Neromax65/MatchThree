using Enums;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     Класс, принимающий ввод игрока
/// </summary>
[RequireComponent(typeof(Element))]
public class InputHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler,
    IDropHandler
{
    /// <summary>
    ///     Текущий выбранный элемент
    /// </summary>
    private static Element _selectedElement;

    /// <summary>
    ///     Элемент к которому применился ввод
    /// </summary>
    [SerializeField] private Element element;

    /// <summary>
    ///     Был ли произведен обмен элементами
    /// </summary>
    private static bool _hasSwapped;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.GameStatus != GameStatus.WaitingForInput) return;

        element.ToggleRaycasts(false);
        Select(element);
        element.transform.SetSiblingIndex(transform.parent.childCount - 1);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManager.GameStatus != GameStatus.WaitingForInput) return;

        var raycastPosition = eventData.pointerCurrentRaycast.screenPosition;

        if (raycastPosition.x > 0 && raycastPosition.x < Screen.width && raycastPosition.y > 0 &&
            raycastPosition.y < Screen.height)
            transform.position = eventData.pointerCurrentRaycast.screenPosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedElement = eventData.pointerDrag.GetComponent<Element>();
        if (droppedElement == null || droppedElement != _selectedElement || !droppedElement.IsAdjacentTo(element))
            return;

        _hasSwapped = true;
        StartCoroutine(droppedElement.Swap(element));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_hasSwapped)
            StartCoroutine(element.UpdateWorldPosition(0.15f));
        
        Deselect();
        element.ToggleRaycasts(true);

        _hasSwapped = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.GameStatus != GameStatus.WaitingForInput || eventData.dragging) return;

        if (_selectedElement == element)
        {
            Deselect();
        }
        else if (_selectedElement == null)
        {
            Select(element);
        }
        else if (_selectedElement.IsAdjacentTo(element))
        {
            StartCoroutine(_selectedElement.Swap(element));
            Deselect();
        }
        else
        {
            Deselect();
        }
    }

    /// <summary>
    ///     Выбрать элемент и подсветить его рамкой
    /// </summary>
    /// <param name="elementToSelect">Выбранный элемент</param>
    private void Select(Element elementToSelect)
    {
        Deselect();
        elementToSelect.ToggleSelectionBorder(true);
        _selectedElement = elementToSelect;
    }

    /// <summary>
    ///     Отменить выделение текущего элемента, если он существует
    /// </summary>
    private static void Deselect()
    {
        if (_selectedElement == null) return;

        _selectedElement.ToggleSelectionBorder(false);
        _selectedElement = null;
    }
}