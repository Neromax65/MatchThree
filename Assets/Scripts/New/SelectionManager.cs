namespace New
{
    public static class SelectionManager
    {
        public static Element SelectedElement { get; private set; }

        public static void SelectElement(Element element)
        {
            SelectedElement = element;
        }
    }
}