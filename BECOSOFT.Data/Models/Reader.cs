using System.Collections.Generic;
using System.Data;

namespace BECOSOFT.Data.Models {
    public class Reader {
        public IDataReader DataReader { get; set; }
        public Dictionary<string, int> ColumnIndices { get; set; }

        public Reader(IDataReader reader, Dictionary<string, int> columnIndices) {
            DataReader = reader;
            ColumnIndices = columnIndices;
        }
    }
}