using System;
using System.IO;

namespace CodeFactory {

    public class Statement : AbstractCodeFragment {

        public static readonly Statement NOP = new Statement(null);
        
        private readonly bool appendTerminator;
        
        internal Statement(Action<TextWriter> writeCode) : this(writeCode, true) { }

        internal Statement(Action<TextWriter> writeCode, bool appendTerminator) : base(writeCode) {
            this.appendTerminator = appendTerminator;
        }
        
        public override void Flush(TextWriter writer) {
            if (IsNop) {
                return;
            }
            base.Flush(writer);
            if (appendTerminator) {
                writer.Write(';');
            }
            writer.WriteLine();
        }

        public Expression ToExpression() {
            return new Expression(writeCode);
        }
    }
}
