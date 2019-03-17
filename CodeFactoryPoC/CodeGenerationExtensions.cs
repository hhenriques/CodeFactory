using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeFactory;

namespace CodeGeneration {
    public static class CodeGenerationExtensions {

        public static void WriteStatement(this TextWriter writer, Statement codeFragment) {
            codeFragment.Flush(writer);
        }

        public static IList<TAbstractCodeFragment> FilterEmpty<TAbstractCodeFragment>(
                this IEnumerable<TAbstractCodeFragment> codeFragments) where TAbstractCodeFragment: AbstractCodeFragment {

            return (codeFragments == null)? new List<TAbstractCodeFragment>(): codeFragments.Where(s => !s.IsNop).ToList();
        }

        public static string ToCamelCase(this string value) {
            if (value.IsEmpty()) {
                return value;
            }

            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }
    }
}
