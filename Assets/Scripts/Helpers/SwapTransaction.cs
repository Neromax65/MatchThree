using System;

namespace Helpers
{
    /// <summary>
    ///     Вспомогательный класс, временно меняющий местами координаты элементов. При применении транзакции изменения
    ///     сохраняются.
    /// </summary>
    public class SwapTransaction : IDisposable
    {
        /// <summary>
        ///     Первый элемент
        /// </summary>
        private readonly Element _element1;

        /// <summary>
        ///     Второй элемент
        /// </summary>
        private readonly Element _element2;

        /// <summary>
        ///     Была ли применена транзакция
        /// </summary>
        private bool _commited;

        public SwapTransaction(Element element1, Element element2)
        {
            _element1 = element1;
            _element2 = element2;

            SwapCoords();
        }

        public void Dispose()
        {
            if (_commited) return;

            SwapCoords();
        }

        /// <summary>
        ///     Применение транзакции
        /// </summary>
        public void Commit()
        {
            _commited = true;
        }

        /// <summary>
        ///     Элементы транзакции меняются местами в координатах игровой сетки
        /// </summary>
        private void SwapCoords()
        {
            var tempCol = _element1.column;
            var tempRow = _element1.row;

            _element1.column = _element2.column;
            _element1.row = _element2.row;

            _element2.column = tempCol;
            _element2.row = tempRow;

            Grid.Instance.UpdateElementIndices(_element1);
            Grid.Instance.UpdateElementIndices(_element2);
        }
    }
}