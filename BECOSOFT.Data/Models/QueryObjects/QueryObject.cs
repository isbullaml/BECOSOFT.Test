namespace BECOSOFT.Data.Models.QueryObjects {
    /// <summary>
    /// Object that contains the options for the query
    /// </summary>
    public abstract class QueryObject {
        /// <summary>
        /// The tablepart that will be used. Defaults to <see langword="null" />.
        /// </summary>
        public string TablePart { get; set; }

        /// <summary>
        /// Bool indicating whether the result should be distinct. Defaults to <see langword="false" />.
        /// </summary>
        public bool Distinct { get; set; }

        /// <summary>
        /// Query timeout (default: <see cref="DatabaseCommand.DefaultQueryTimeout"/>
        /// </summary>
        public int? Timeout { get; set; }

        protected QueryObject(string tablePart = null, bool distinct = false) {
            TablePart = tablePart;
            Distinct = distinct;
        }

        public override string ToString() {
            return TablePart + "+" + Distinct;
        }
    }
}