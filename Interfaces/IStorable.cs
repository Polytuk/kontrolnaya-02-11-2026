namespace BodagovKR1_.Interfaces
{
    /// <summary>
    /// Контракт для сохранения состояния объекта во внешнее хранилище.
    /// </summary>
    public interface IStorable
    {
        string FilePath { get; set; }
        void Save();
    }
}

