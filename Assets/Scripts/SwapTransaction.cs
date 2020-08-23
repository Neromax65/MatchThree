using System;

namespace DefaultNamespace
{
    public class SwapTransaction : IDisposable
    {
        private readonly Tile _tile1;
        private readonly Tile _tile2;
        public SwapTransaction(Tile tile1, Tile tile2)
        {
            _tile1 = tile1;
            _tile2 = tile2;
            
            var tempElement = tile1.Element;
            tile1.SetElement(tile2.Element);
            tile2.SetElement(tempElement);
        }
        
        public void Dispose()
        {
            var tempElement = _tile1.Element;
            _tile1.SetElement(_tile2.Element);
            _tile2.SetElement(tempElement);
        }
    }
}