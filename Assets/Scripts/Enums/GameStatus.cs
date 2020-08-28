namespace Enums
{
    /// <summary>
    ///     Перечисление состояний игры
    /// </summary>
    public enum GameStatus
    {
        /// <summary>
        ///     Генерация игрового поля
        /// </summary>
        Initializing,

        /// <summary>
        ///     Проигрывание анимаций
        /// </summary>
        PlayingAnimation,

        /// <summary>
        ///     Ожидания действий игрока
        /// </summary>
        WaitingForInput
    }
}