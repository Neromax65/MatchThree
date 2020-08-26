using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace New
{
    [RequireComponent(typeof(Element))]
    public class InputHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
    {
        private static Element _selectedElement;

        
        private Element _element;

        private void Awake()
        {
            if (_element == null)
                _element = GetComponent<Element>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (GameManager.GameStatus != GameManager.GameState.WaitingForInput) return;
            
            if (_selectedElement == _element)
            {
                // Debug.Log($"Element deselected");
                _selectedElement = null;
            } 
            else if (_selectedElement == null)
            {
                // Debug.Log($"Selected element [X:{Column}, Y:{Row}]. Current matches: {Grid.Instance.CheckMatches(_element).Count}");
                // SelectionManager.SelectElement(_element);
                _selectedElement = _element;
            }
            else if (_selectedElement.IsAdjacentTo(_element))
            {
                // Debug.Log($"Swapping element [X:{_selectedElement.Column}, Y:{_selectedElement.Row}] with element [X:{Column}, Y:{Row}]");
                _selectedElement.Swap(_element);
                // SelectionManager.SelectElement(null);
                _selectedElement = null;
            }
            else
            {
                // SelectionManager.SelectElement(null);
                _selectedElement = null;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (GameManager.GameStatus != GameManager.GameState.WaitingForInput) return;

            throw new NotImplementedException();
        }

        public void OnEndDrag(PointerEventData eventData)
        {

            throw new NotImplementedException();
        }

        public void OnDrop(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}