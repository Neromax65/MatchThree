using System;

namespace Helpers
{
    public class SwapTransaction : IDisposable
    {
        private bool _commited;

        private readonly Element _element1;
        private readonly Element _element2;
        
        public SwapTransaction(Element element1, Element element2)
        {
            _element1 = element1;
            _element2 = element2;

            SwapCoords(_element1,_element2);
        }

        public void Commit()
        {
            _commited = true;
        }
        
        public void Dispose()
        {
            if (_commited) return;
            
            SwapCoords(_element1,_element2);
        }

        private static void SwapCoords(Element element1, Element element2)
        {
            int tempCol = element1.Column;
            int tempRow = element1.Row;
            
            element1.Column = element2.Column;
            element1.Row = element2.Row;
            
            element2.Column = tempCol;
            element2.Row = tempRow;
            
            Grid.Instance.UpdateElementIndices(element1);
            Grid.Instance.UpdateElementIndices(element2);
        }
    }
}