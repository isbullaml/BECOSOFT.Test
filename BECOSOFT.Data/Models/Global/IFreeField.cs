namespace BECOSOFT.Data.Models.Global {
    public interface IFreeField {
        void SetFreeFieldValue<T>(FreeFieldType<T> type, int fieldIndex, T value);
        T GetFreeFieldValue<T>(FreeFieldType<T> type, int fieldIndex);
    }
}
