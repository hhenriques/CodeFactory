using System;
using System.IO;

namespace CodeFactory {

    public class Expression : AbstractCodeFragment {
        
        public static readonly Expression NOP = new Expression(null);

        internal Expression(Action<TextWriter> writeCode) : base(writeCode) {}
        
        public Statement ToStatement() {
            return ToStatement(true);
        }

        public Statement ToStatement(bool appendTerminator) {
            return new Statement(writeCode, appendTerminator);
        }

        public override bool Equals(object obj) {
            return obj.ToString().Equals(ToString());
        }
        
        public override int GetHashCode() {
            return ToString().GetHashCode();
        }
    }
}
