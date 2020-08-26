using System;
using UnityEngine;

namespace New
{
    /// <summary>
    /// Плитка, на которой размещаются цветные элементы. Генерируется один раз при инициализации поля
    /// и больше никуда не перемещается. Используется при операциях с элементами. 
    /// </summary>
    public class Tile : MonoBehaviour
    {
        /// <summary>
        /// Индекс колонки плитки, её координата X в рамках игрового поля
        /// </summary>
        public int Column { get; private set; }
        
        /// <summary>
        /// Индекс ряда плитки, её координата Y в рамках игрового поля
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// Цветной элемент, привязанный к данной плитке. Может быть null.
        /// </summary>
        private Element _element;

        private void Awake()
        {
            Column = (int) transform.position.x;
            Row = (int) transform.position.y;
        }

        /// <summary>
        /// Связывает элемент из параметра с плиткой. Связывание происходит только, если данная плитка пустая.
        /// </summary>
        /// <param name="element">Элемент, связываемый с этой плиткой</param>
        public void LinkElement(Element element)
        {
            if (this._element != null)
            {
                Debug.LogWarning("Попытка связать элемент с занятой клеткой.");
                return;
            }

            _element = element;
        }
    }
}
