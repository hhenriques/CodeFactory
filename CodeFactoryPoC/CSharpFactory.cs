using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using CodeGeneration;

namespace CodeFactory {

    public class CSharpFactory : AbstractCodeFactory {

        private static CSharpFactory instance;
        public static CSharpFactory Instance {
            get {
                if (instance == null) {
                    instance = new CSharpFactory();
                }
                return instance;
            }
        }

        public override Statement DeclareProperty(string type, string name, string backingField, IEnumerable<Statement> getter, IEnumerable<Statement> setter) {
            return new Statement(writer => {
                writer.WriteLine("public " + type + " " + name + " {");

                if (!getter.IsEmpty()) {
                    writer.WriteLine("get {");
                    FlushAllInternal(writer, getter);
                    writer.WriteLine("}");
                }

                if (!setter.IsEmpty()) {
                    writer.WriteLine("set {");
                    FlushAllInternal(writer, setter);
                    writer.WriteLine("}");
                }

                writer.WriteLine("}");
            }, false);
        }

        public override Statement UsingDirective(params Expression[] usings) {
            return new Statement(writer => {
                foreach (var use in usings) {
                    writer.Write("using ");
                    use.Flush(writer);
                    writer.WriteLine(";");
                }
            }, false);
        }

        public override Statement DeclareNamespace(Expression name, params Statement[] body) {
            return new Statement(writer => {
                writer.Write("namespace ");
                name.Flush(writer);
                writer.WriteLine(" {");
                FlushAllInternal(writer, body);
                writer.WriteLine("}");
            }, false);
        }

        public override Statement DeclareClass(string name, Expression extends, params Statement[] body) {
            return new Statement(writer => {
                writer.Write("public class " + name + " ");
                if (extends != null) {
                    writer.Write(": ");
                    extends.Flush(writer);
                    writer.Write(" ");
                }
                writer.WriteLine("{ ");
                FlushAllInternal(writer, body);
                writer.WriteLine("}");
            }, false);
        }

        public override Expression Decimal(decimal value) {
            return new Expression(writer => writer.Write(value.ToString(CultureInfo.InvariantCulture) + "M"));        
        }

        public override Expression Coalesce(Expression leftSide, Expression rightSide) {
            return ChainExpressions(" ?? ", leftSide, rightSide);
        }

        public override Statement DeclareTypedStaticMethod(string returnType, string methodName, IEnumerable<TypedArgument> args, params Statement[] body) {
            return new Statement(writer => {
                writer.WriteLine("public static " + returnType + " " + methodName + "(" + string.Join(", ", args.Select(a => a.Type + " " + a.Name)) + ") {");
                FlushAllInternal(writer, body);
                writer.Write("}");
            }, false);
        }

        public override Statement DeclareTypedMethod(Visibility visibility, string returnType, string methodName, IEnumerable<TypedArgument> args, params Statement[] body) {
            return new Statement(writer => {
                writer.WriteLine(visibility.ToString().ToLower() + " " + returnType + " " + methodName + "(" + string.Join(", ", args.Select(a => a.Type + " " + a.Name)) + ") {");
                FlushAllInternal(writer, body);
                writer.Write("}");
            }, false);
        }

        public override Statement DeclareTypedConstructor(string className, IEnumerable<TypedArgument> arguments, params Statement[] body) {

            var args = arguments.Select(a => a.Type + " " + a.Name);

            return new Statement(writer => {
                writer.WriteLine("public " + className + "(" + string.Join(", ", args) + ") {");
                FlushAllInternal(writer, body);
                writer.WriteLine("}");
            }, false);

        }

        public override Expression NewArray(string type, params Expression[] values) {

            return new Expression(writer => {
                writer.Write("new " + type + "[] {");

                using (IEnumerator<Expression> iter = values.ToList().GetEnumerator()) {

                    if (iter.MoveNext()) {
                        iter.Current.Flush(writer);
                    }

                    while (iter.MoveNext()) {
                        writer.Write(", ");
                        iter.Current.Flush(writer);
                    }
                }

                writer.Write("}");
            });

        }

        public override Expression DictionaryGet(Expression dict, Expression key) {
            return new Expression(writer => {
                dict.Flush(writer);
                writer.Write("[");
                key.Flush(writer);
                writer.Write("]");
            });
        }

        public override Expression CallFunctionIdentifier(Expression objectIdentifier, string functionName, params Expression[] typedArguments) {
            if (typedArguments.IsEmpty()) {
                return Identifier(objectIdentifier, Identifier(functionName));
            } else {
                return Identifier(
                    objectIdentifier,
                    new Expression(writer => {
                        writer.Write(functionName);
                        writer.Write("<");
                        typedArguments.First().Flush(writer);
                        foreach (var arg in typedArguments.Skip(1)) {
                            writer.Write(", ");
                            arg.Flush(writer);
                        }
                        writer.Write(">");
                    }));
            }
        }

        public override Expression Lambda(IEnumerable<string> arguments, IEnumerable<Statement> body) {
            return LambdaInternal(arguments, body);
        }

        public override Expression Lambda(IEnumerable<string> arguments, Expression body) {
            return LambdaExpression(arguments, body);
        }

        private Expression LambdaExpression(IEnumerable<string> arguments, Expression body) {
            return new Expression(writer => {
                writer.Write("(" + arguments.StrCat(", ") + ") => ");
                body.Flush(writer);
            });
        }

        private Expression LambdaInternal(IEnumerable<string> arguments, IEnumerable<Statement> body) {
            return new Expression(writer => {
                writer.WriteLine("(" + (arguments ?? (new string[] { })).StrCat(", ") + ") => {");
                body.Apply(x => x.Flush(writer));
                writer.WriteLine("}");
            });
        }

        public override Statement Using(Expression disposableObject, IEnumerable<Statement> body) {
            return UsingInternal(disposableObject, body);
        }

        public override Statement Using(Expression disposableObject, params Statement[] body) {
            return UsingInternal(disposableObject, body);
        }

        private Statement UsingInternal(Expression disposableObject, IEnumerable<Statement> body) {
            return new Statement(writer => {
                writer.Write("using (");
                disposableObject.Flush(writer);
                writer.WriteLine(") {");
                FlushAllInternal(writer, body);
                writer.WriteLine("}");
            }, false);
        }

        public override Expression OutParameter(Expression parameter) {
            return new Expression(writer => { writer.Write("out "); parameter.Flush(writer); });
        }

        public override Statement DeclareOutParameterVariable(string type, string varName) {
            return new Statement(writer => {
                writer.Write("{0} {1}", type, varName);
            });
        }

        public override Statement DictionaryPut(Expression dict, Expression key, Expression val) {
            return new Statement(writer => {
                dict.Flush(writer);
                writer.Write("[");
                key.Flush(writer);
                writer.Write("]=");
                val.Flush(writer);
            });
        }

        public override Statement Attribute(string name, params Expression[] attrs) {
            return new Statement(writer => {
                writer.Write("[" + name +  "(");
                if (attrs.Length > 0) {
                    attrs.First().Flush(writer);
                    foreach (var attr in attrs.Skip(1)) {
                        writer.Write(", ");
                        attr.Flush(writer);
                    }
                }
                writer.Write(")]");
            }, appendTerminator: false);
        }

        public override Expression Eq(Expression leftSide, Expression rightSide, ExpressionType type) {
            switch (type) {
                case ExpressionType.Object:
                    return Call(Identifier("Object", "Equals"), leftSide, rightSide);
                case ExpressionType.BinaryData:
                    return Call(Identifier("BuiltInFunction", "AreBinaryNulls"), leftSide, rightSide);
                default:
                    return base.Eq(leftSide, rightSide, type);
            }
        }

        public override Expression Neq(Expression leftSide, Expression rightSide, ExpressionType type) {
            if (type == ExpressionType.BinaryData) {
                return Not(Eq(leftSide, rightSide, type));
            }
            return base.Neq(leftSide, rightSide, type);
        }

        public override Expression ClassTypeIdentifier(Expression className) {
            return Call("typeof", className);
        }
    }
}
