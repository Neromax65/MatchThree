using System;

namespace New
{
    public class SwapTransaction : IDisposable
    {
        private bool _commited = false;

        private readonly Element _element1;
        private readonly Element _element2;
        
        public SwapTransaction(Element element1, Element element2)
        {
            _element1 = element1;
            _element2 = element2;

            int tempCol = _element1.Column;
            int tempRow = _element1.Row;
            
            _element1.Column = _element2.Column;
            _element1.Row = _element2.Row;
            
            _element2.Column = tempCol;
            _element2.Row = tempRow;
        }

        public void Commit()
        {
            _commited = true;
        }
        
        public void Dispose()
        {
            if (_commited) return;
            
            int tempCol = _element1.Column;
            int tempRow = _element1.Row;
            
            _element1.Column = _element2.Column;
            _element1.Row = _element2.Row;
            
            _element2.Column = tempCol;
            _element2.Row = tempRow;
        }
    }
}