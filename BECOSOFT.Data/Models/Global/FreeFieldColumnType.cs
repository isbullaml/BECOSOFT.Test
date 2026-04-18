using System;
using System.ComponentModel;

namespace BECOSOFT.Data.Models.Global {
    public enum FreeFieldColumnType {
        Text = 0,
        Bit,
        InternetBit,
        Cmb,
        Date,
        MultiSelect,
        [Description("Bit{0}Date")]
        BitDate,
        Time,
        Number,
    }

    public class FreeFieldType<T> : IFreeFieldType {
        public FreeFieldColumnType ColumnType { get; }
        public Type DataType => typeof(T);
        public bool UseDescription { get; }

        public FreeFieldType(FreeFieldColumnType type, bool useDescription = false) {
            ColumnType = type;
            UseDescription = useDescription;
        }
    }

    public interface IFreeFieldType {
        FreeFieldColumnType ColumnType { get; }
        bool UseDescription { get; }
    }
}