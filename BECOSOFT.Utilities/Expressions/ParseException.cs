using System;

namespace BECOSOFT.Utilities.Expressions {
    public sealed class ParseException : Exception {
        public int Position { get; }

        public ParseException(string message, int position)
            : base(message) {
            Position = position;
        }


        public override string ToString() {
            return $"{Message} (at index {Position})";
        }
    }
}