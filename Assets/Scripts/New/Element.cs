using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace New
{
    /// <summary>
    /// Цветной элемент, располагаемый на игровом поле
    /// </summary>
    public class Element : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image reverseGravityIcon;
        
        /// <summary>
        /// Индекс колонки элемента, его координата X в рамках игрового поля
        /// </summary>
        public int Column;
        
        /// <summary>
        /// Индекс ряда элемента, его координата Y в рамках игрового поля
        /// </summary>
        public int Row;
        
        // Перечисление возможных цветов/типов элементов
        public enum ElementType
        {
            Red, Green, Blue, Yellow, Cyan, Magenta
        }

        public bool IsFalling = false;

        /// <summary>
        /// Тип/цвет элемента
        /// </summary>
        public ElementType Type { get; private set; }

        public void SetRandomType()
        {
            Type = (ElementType)Random.Range(0, Enum.GetNames(typeof(ElementType)).Length);
            ApplyColor(Type);
        }
        
        void ApplyColor(ElementType type)
        {
            switch (type)
            {
                case ElementType.Red:
                    icon.color = Color.red;
                    break;
                case ElementType.Green:
                    icon.color = Color.green;
                    break;
                case ElementType.Blue:
                    icon.color = Color.blue;
                    break;
                case ElementType.Yellow:
                    icon.color = Color.yellow;
                    break;
                case ElementType.Cyan:
                    icon.color = Color.cyan;
                    break;            
                case ElementType.Magenta:
                    icon.color = Color.magenta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        
        /// <summary>
        /// Засчитывание элемента как совпавшего и удаление его из игры
        /// </summary>
        public void Match()
        {
            Destroy(gameObject);
        }

        private void Update()
        {
            // TODO: Test
            GetComponentInChildren<Text>().text = $"[{Column},{Row}]";
            gameObject.name = $"Element [{Column},{Row}][{Type}]";
        }

        // public event Action<Element, Element> SwapEnded;

        /// <summary>
        /// Обновление позиции элемента в игровом мире
        /// </summary>
        public void UpdatePosition(bool instantly = false)
        {
            int targetX = Column * Grid.CellSize + (int)Grid.Offset.x;
            int targetY = Row * Grid.CellSize + (int)Grid.Offset.y;
            Vector2 targetPosition = new Vector2Int(targetX, targetY);
            
            if (instantly)
            {
                
                transform.position = targetPosition;
                
                return;
            }

            StartCoroutine(UpdatePositionAnimation(targetPosition));

        }

        IEnumerator UpdatePositionAnimation(Vector2 targetPosition)
        {
            for (float t = 0; t < 1; t += Time.deltaTime)
            {
                transform.position = Vector2.Lerp(transform.position, targetPosition, t);
                yield return null;
            }
        }

        /// <summary>
        /// Поменяться местами с другим элементом
        /// </summary>
        /// <param name="element"></param>
        public void Swap(Element element, bool revert = false)
        {
            int tempCol = Column;
            int tempRow = Row;
            
            Column = element.Column;
            Row = element.Row;
            
            element.Column = tempCol;
            element.Row = tempRow;

            Grid.Instance.Elements[Column, Row] = this;
            Grid.Instance.Elements[element.Column, element.Row] = element;
            
            // UpdatePosition(true);
            // element.UpdatePosition(true);
            StartCoroutine(Grid.Instance.SwapAnimation(this, element, revert));

            // UpdatePosition();
            // element.UpdatePosition();

            // element.SwapEnded += Grid.OnSwapEnded;
        }


        // IEnumerator SwapAnimation(Element element1, Element element2)
        // {
        //     for (float t = 0; t < 1; t += Time.deltaTime)
        //     {
        //         transform.position = Vector2.Lerp(transform.position, targetPosition, t);
        //         yield return null;
        //     }
        // }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (SelectionManager.SelectedElement == this)
            {
                Debug.Log($"Element deselected");
                SelectionManager.SelectElement(null);
            } 
            else if (SelectionManager.SelectedElement == null)
            {
                Debug.Log($"Selected element [X:{Column}, Y:{Row}]. Current matches: {Grid.Instance.CheckMatches(Grid.Instance.Elements, this).Count}");
                SelectionManager.SelectElement(this);
            }
            else
            {
                Debug.Log($"Swapping element [X:{SelectionManager.SelectedElement.Column}, Y:{SelectionManager.SelectedElement.Row}] with element [X:{Column}, Y:{Row}]");
                SelectionManager.SelectedElement.Swap(this);
                SelectionManager.SelectElement(null);
            }
        }
    }
}