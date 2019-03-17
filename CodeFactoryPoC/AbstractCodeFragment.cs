using System;
using System.IO;

namespace CodeFactory {
    
    public abstract class AbstractCodeFragment {

        protected readonly Action<TextWriter> writeCode;

        protected AbstractCodeFragment(Action<TextWriter> writeCode) {
            if (writeCode == null) {
                writeCode = writer => { };
                IsNop = true;
            }
            this.writeCode = writeCode;
        }

        public virtual void Flush(TextWriter writer) {
            writeCode(writer);
        }

        public override string ToString() {
            using (var writer = new StringWriter()) {
                Flush(writer);
                return writer.ToString();
            }
        }

        public bool IsNop { get; private set; }
        
    }

    public static class AbstractCodeFragmentExtensions {
        public static bool IsNullOrNop(this AbstractCodeFragment codeFragment) {
            return (codeFragment == null) || codeFragment.IsNop;
        }
    }
}
