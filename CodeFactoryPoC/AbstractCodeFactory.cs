using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeGeneration;

namespace CodeFactory {

    public abstract class AbstractCodeFactory {

        protected const string PropertySetterArgumentName = "value";
        protected const string CatchAllDefaultExceptionVar = "ex";
        protected const string PropertyBackingFieldPrefix = "_";
        protected const string NewLineCharacter = "\n";

        protected void FlushAllInternal(TextWriter writer, IEnumerable<Statement> statements) {
            statements.Apply(x => x.Flush(writer));
        }

        protected virtual string GetCatchAllClause(string exceptionVarName) {
            return "Exception " + exceptionVarName;
        }
        
        protected virtual Statement WhileTrueInternal(IEnumerable<Statement> body) {
            return WhileInternal(SafeTrueCondition(), body);
        }

        protected Statement SequenceInternal(IEnumerable<Statement> statements) {
            return new Statement(writer =>
                statements.Apply(st => st.Flush(writer)), false);
        }

        public Statement Sequence(params Statement[] statements) {
            return SequenceInternal(statements);
        }

        public Statement Sequence(IEnumerable<Statement> statements) {
            return SequenceInternal(statements);
        }

        protected static Expression ExpressionFromString(string codeFragment) {
            return new Expression(writer => writer.Write(codeFragment));
        }

        public virtual Expression String(string str) {
            return ExpressionFromString("\"" + str.EscapeStringLiteral() + "\"");
        }

        public virtual Expression StringOrNull(string str) {
            return str == null ? Null() : String(str);
        }

        public Expression Boolean(bool value) {
            return value ? True() : False();
        }

        public Expression Integer(int value) {
            return new Expression(writer => writer.Write(value));
        }

        public virtual Expression Long(long value) {
            return new Expression(writer => writer.Write(value + "L"));
        }

        public abstract Expression Decimal(decimal value);

        public virtual Expression DateTime(int year, int month, int day, int hours, int minutes, int seconds) {
            return ExpressionFromString(string.Format("new DateTime({0}, {1}, {2}, {3}, {4}, {5})",
                year, month, day, hours, minutes, seconds));
        }

        public virtual Expression Date(int year, int month, int day) {
            return ExpressionFromString(string.Format("new DateTime({0}, {1}, {2})", year, month, day));
        }

        public Expression Time(int hours, int minutes, int seconds) {
            return DateTime(1900, 1, 1, hours, minutes, seconds);
        }

        public virtual Expression BinaryData() {
            return new Expression(writer => writer.Write("new byte[] {}"));
        }

        public virtual Expression ParameterlessObject() {
            return New("object");
        }

        public Expression Cast(string typeName, Expression expr) {
            return Cast(Identifier(typeName), expr);
        }

        public virtual Expression Cast(Expression typeName, Expression expr) {
            return new Expression(writer => {
                writer.Write("((");
                typeName.Flush(writer);
                writer.Write(")");
                expr.Flush(writer);
                writer.Write(")");
            });
        }

        public Expression Wrap(Expression st) {
            return new Expression(writer => {
                writer.Write("(");
                st.Flush(writer);
                writer.Write(")");
            });
        }

        protected void InnerWriteMultiLineComment(TextWriter writer, string text) {
            writer.Write("/*" + text.Replace("*/", "") + "*/");
        }

        public Statement MultiLineComment(string text) {
            return new Statement(writer => InnerWriteMultiLineComment(writer, text));
        }

        public Statement SingleLineComment(string text) {
            return new Statement(writer =>
            writer.Write("// " + text.Replace(NewLineCharacter, NewLineCharacter + "//")), false); 
        }
        
        protected Expression ChainExpressions(string connector, params Expression[] terms) {
            return ChainExpressions(connector, true, terms);
        }

        protected Expression ChainExpressions(string connector, bool surroundWithParenthesis, IEnumerable<Expression> terms) {
            return new Expression(writer => {
                var validTerms = terms.FilterEmpty();

                if (surroundWithParenthesis) {
                    writer.Write("(");
                }

                validTerms.First().Flush(writer);
                foreach (var t in validTerms.Skip(1)) {
                    writer.Write(connector);
                    t.Flush(writer);
                }

                if (surroundWithParenthesis) {
                    writer.Write(")");
                }
            });
        }

        protected Expression KeyExpressions(IEnumerable<Expression> terms) {
            return new Expression(writer => {
                var validTerms = terms.FilterEmpty();

                validTerms.First().Flush(writer);
                writer.Write("[\"");


                foreach (var t in validTerms.Skip(1)) {
                    t.Flush(writer);
                }


                writer.Write("\"]");
            });
        }

        public Expression Identifier(IEnumerable<string> identifierParts) {
            return IdentifierInternal(identifierParts);
        }

        public Expression Identifier(params string[] identifierParts) {
            return IdentifierInternal(identifierParts);
        }

        public Expression Identifier(params Expression[] identifierParts) {
            return IdentifierInternal(identifierParts);
        }

        public abstract Expression ClassTypeIdentifier(Expression className);


        private Expression IdentifierInternal(IEnumerable<string> identifierParts) {
            return IdentifierInternal(identifierParts.Where(part => !part.IsEmpty()).Select(id => ExpressionFromString(id)).ToArray());
        }

        protected Expression IdentifierInternal(Expression[] identifierParts) {
            return ChainExpressions(".", false, identifierParts);
        }

        protected Expression CreateBinaryOperation(string op, Expression leftSide, Expression rightSide) {
            return ChainExpressions(op, leftSide, rightSide);
        }

        public abstract Expression Coalesce(Expression leftSide, Expression rightSide);

        public Expression Or(Expression leftSide, Expression rightSide) {
            return ChainExpressions(" || ", leftSide, rightSide);
        }

        public Expression And(Expression leftSide, Expression rightSide) {
            return ChainExpressions(" && ", leftSide, rightSide);
        }

        public virtual Expression IsNull(Expression leftSide) {
            return CreateBinaryOperation(" == ", leftSide, Null());
        }

        public virtual Expression IsNotNull(Expression leftSide) {
            return CreateBinaryOperation(" != ", leftSide, Null());
        }

        public virtual Expression Eq(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" == ", leftSide, rightSide);
        }

        public virtual Expression Neq(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" != ", leftSide, rightSide);
        }

        public virtual Expression Add(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" + ", leftSide, rightSide);
        }

        public virtual Expression Sub(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" - ", leftSide, rightSide);
        }

        public virtual Expression Mul(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" * ", leftSide, rightSide);
        }

        public virtual Expression Div(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" / ", leftSide, rightSide);
        }

        public virtual Expression Lt(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" < ", leftSide, rightSide);
        }

        public virtual Expression Lte(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" <= ", leftSide, rightSide);
        }

        public virtual Expression Gt(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" > ", leftSide, rightSide);
        }

        public virtual Expression Gte(Expression leftSide, Expression rightSide, ExpressionType type) {
            return CreateBinaryOperation(" >= ", leftSide, rightSide);
        }

        public virtual Expression ConcatStrings(params Expression[] parts) {
            return ChainExpressions(" + ", false, parts);
        }

        public virtual Expression ConcatStrings(IEnumerable<Expression> parts) {
            return ChainExpressions(" + ", false, parts);
        }

        public Expression Not(Expression expression) {
            return new Expression(writer => {
                writer.Write("!(");
                expression.Flush(writer);
                writer.Write(")");
            });
        }

        public virtual Expression Neg(Expression expression, ExpressionType type) {
            return new Expression(writer => {
                writer.Write("-");
                expression.Flush(writer);
            });
        }

        public virtual Expression CallLambda(Expression lambdaIdentifier, params Expression[] args) {
            return CallInternal(lambdaIdentifier, args);
        }

        public Expression Call(string functionName, IEnumerable<Expression> args) {
            return CallInternal(Identifier(functionName), args);
        }

        public Expression Call(string functionName, params Expression[] args) {
            return CallInternal(Identifier(functionName), args);
        }

        public Expression Call(Expression functionIdentifier, IEnumerable<Expression> args) {
            return CallInternal(functionIdentifier, args);
        }

        public virtual Expression Call(Expression functionIdentifier, params Expression[] args) {
            return CallInternal(functionIdentifier, args);
        }

        public Expression Call(Expression objectIdentifier, string functionName, params Expression[] args) {
            return CallInternal(CallFunctionIdentifier(objectIdentifier, functionName), args);
        }

        public Expression Call(Expression objectIdentifier, string functionName, IEnumerable<Expression> args) {
            return CallInternal(CallFunctionIdentifier(objectIdentifier, functionName), args);
        }

        public virtual Expression CallFunctionIdentifier(Expression objectIdentifier, string functionName, params Expression[] typedArguments) {
            return Identifier(objectIdentifier, Identifier(functionName));
        }

        protected Expression CallInternal(Expression functionIdentifier, IEnumerable<Expression> args) {
            return new Expression(writer => {
                functionIdentifier.Flush(writer);
                writer.Write("(");
                var validArguments = args.FilterEmpty();
                if (validArguments.Any()) {
                    validArguments.First().Flush(writer);
                    foreach (var arg in validArguments.Skip(1)) {
                        writer.Write(", ");
                        arg.Flush(writer);
                    }
                }
                writer.Write(")");
            });
        }

        public Statement If(Expression condition, IEnumerable<Statement> body) {
            return IfInternal(condition, body, Statement.NOP.ToEnumerable());
        }

        public Statement If(Expression condition, params Statement[] body) {
            return IfInternal(condition, body, Statement.NOP.ToEnumerable());
        }

        public Statement If(Expression condition, IEnumerable<Statement> thenBranch, IEnumerable<Statement> elseBranch) {
            return IfInternal(condition, thenBranch, elseBranch);
        }

        protected virtual Statement IfInternal(Expression condition, IEnumerable<Statement> thenBranch, IEnumerable<Statement> elseBranch) {
            return new Statement(writer => {
                writer.Write("if(");
                condition.Flush(writer);
                writer.WriteLine(") {");
                FlushAllInternal(writer, thenBranch);
                var validElseBrach = elseBranch.FilterEmpty();
                if (validElseBrach.Any()) {
                    writer.WriteLine("} else {");
                    FlushAllInternal(writer, validElseBrach);
                }
                writer.WriteLine("}");
            }, false);
        }

        public virtual Expression Ternary(Expression condition, Expression trueExp , Expression falseExp) {
            return new Expression(writer => {
                condition.Flush(writer);
                writer.Write(" ? ");
                trueExp.Flush(writer);
                writer.Write(" : ");
                falseExp.Flush(writer);
            });
        }

        public Statement DoWhile(Expression condition, IEnumerable<Statement> body) {
            return DoWhileInternal(condition, body);
        }

        public Statement DoWhile(Expression condition, params Statement[] body) {
            return DoWhileInternal(condition, body);
        }

        protected virtual Statement DoWhileInternal(Expression condition, IEnumerable<Statement> body) {
            return new Statement(writer => {
                writer.WriteLine("do {");
                FlushAllInternal(writer, body);
                writer.Write("} while(");
                condition.Flush(writer);
                writer.WriteLine(")");
            });
        }

        public Statement While(Expression condition, IEnumerable<Statement> body) {
            return WhileInternal(condition, body);
        }

        public Statement While(Expression condition, params Statement[] body) {
            return WhileInternal(condition, body);
        }

        protected virtual Statement WhileInternal(Expression condition, IEnumerable<Statement> body) {
            return new Statement(writer => {
                writer.Write("while (");
                condition.Flush(writer);
                writer.WriteLine(") {");
                FlushAllInternal(writer, body);
                writer.WriteLine("}");
            }, false);
        }

        public abstract Statement Using(Expression disposableObject, IEnumerable<Statement> body);

        public abstract Statement Using(Expression disposableObject, params Statement[] body);

        public virtual Statement Break() {
            return ExpressionFromString("break").ToStatement();
        }

        public virtual Statement Continue() {
            return ExpressionFromString("continue").ToStatement();
        }

        public Statement Return() {
            return Return(null);
        }

        public virtual Statement Return(Expression value) {
            return new Statement(writer => {
                writer.Write("return");
                if (value != null) {
                    writer.Write(" ");
                    value.Flush(writer);
                }
            });
        }

        public virtual Statement Throw(Expression argument) {
            return new Statement(writer => {
                writer.Write("throw");

                if (argument != null && argument != Expression.NOP) {
                    writer.Write(" ");
                    argument.Flush(writer);
                }
            });
        }

        public Expression New(string className, params Expression[] arguments) {
            return New(Identifier(className), arguments);
        }

        public Expression New(Expression classIdentifier, params Expression[] arguments) {
            return new Expression(writer => {
                writer.Write("new ");
                Call(classIdentifier, arguments).Flush(writer);
            });
        }

        public Expression New(Expression classIdentifier, IEnumerable<Expression> arguments) {
            return new Expression(writer => {
                writer.Write("new ");
                Call(classIdentifier, arguments).Flush(writer);
            });
        }
        
        public virtual Statement DeclareVar(string type, string varName) {
            return new Statement(writer => {
                writer.Write("{0} {1}", type, varName);
            });
        }

        public virtual Statement DeclareVar(string type, string varName, Expression value) {
            return new Statement(writer => {
                writer.Write("{0} {1} = ", type, varName);
                value.Flush(writer);
            });
        }

        public virtual Statement DeclareVar(string varName, Expression value) {
            return DeclareVar("var", varName, value);
        }

        public Statement Assign(Expression left, Expression value) {
            return new Statement(w => {
                left.Flush(w);
                w.Write(" = ");
                value.Flush(w);
            });
        }

        public Expression Null() {
            return ExpressionFromString("null");
        }

        public Expression True() {
            return ExpressionFromString("true");
        }

        public Expression False() {
            return ExpressionFromString("false");
        }

        public virtual Expression SafeTrueCondition() {
            return True();
        }

        public virtual Statement DeclareProperty(string type, string name) {
            var backingField = PropertyBackingFieldPrefix + name;
            return DeclareProperty(type, name, backingField,
                Return(Identifier(backingField)).ToEnumerable(),
                new Statement[] { Assign(Identifier(backingField), Identifier(PropertySetterArgumentName)) });
        }

        public virtual Statement DeclareProperty(string type, string name, Statement getter, Statement setter) {
            return DeclareProperty(type, name, null, getter.ToEnumerable(), (setter == null)? Enumerable.Empty<Statement>(): setter.ToEnumerable());
        }

        public virtual Statement DeclareProperty(string type, string name, Statement getter) {
            return DeclareProperty(type, name, getter, null);
        }

        public virtual Statement DeclareProperty(string type, string name, IEnumerable<Statement> getter, IEnumerable<Statement> setter) {
            return DeclareProperty(type, name, null, getter, setter);
        }

        public abstract Statement DeclareProperty(string type, string name, string backingField, IEnumerable<Statement> getter, IEnumerable<Statement> setter);

        public virtual Expression GetProperty(Expression objectIdentifier, string property, ExpressionType propertyType) {
            return new Expression(writer => {
                objectIdentifier.Flush(writer);
                writer.Write(".{0}", property);
            });
        }

        public Expression GetPropertySequence(Expression objectIdentifier, params Pair<string, ExpressionType>[] properties) {
            return properties.Aggregate(objectIdentifier, (current, childProp) => GetProperty(current, childProp.First, childProp.Second));
        }

        public virtual Statement SetProperty(Expression objectIdentifier, string property, Expression value, ExpressionType propertyType) {
            return Assign(
                new Expression(writer => {
                    objectIdentifier.Flush(writer);
                    writer.Write(".{0}", property);
                }),
                value
            );
        }

        public virtual Expression Index(Expression collection, Expression index) {
            return new Expression(writer => {
                collection.Flush(writer);
                writer.Write("[");
                index.Flush(writer);
                writer.Write("]");
            });
        }

        public virtual Expression Index(Expression index) {
            return Index(Expression.NOP, index);
        }

        public virtual Statement IndexSetter(Expression index, Expression value) {
            return Assign(Index(index), value);
        }

        public abstract Statement UsingDirective(params Expression[] usings);

        public virtual Statement DeclareClass(string name, params Statement[] body) {
            return DeclareClass(name, null, body);
        }

        public abstract Statement DeclareNamespace(Expression name, params Statement[] body);

        public abstract Statement DeclareClass(string name, Expression extends, params Statement[] body);

        public Statement DeclareInternalClass(string name, params Statement[] body) {
            return new Statement(writer => {
                writer.WriteLine("class " + name + "{");
                FlushAllInternal(writer, body);
                writer.WriteLine("}");
            }, false);
        }

        public abstract Statement DeclareTypedStaticMethod(string returnType, string methodName, IEnumerable<TypedArgument> arguments, params Statement[] body);

        public abstract Statement DeclareTypedMethod(Visibility visibility, string returnType, string methodName, IEnumerable<TypedArgument> arguments, params Statement[] body);

        public abstract Statement DeclareTypedConstructor(string className, IEnumerable<TypedArgument> arguments, params Statement[] body);

        public Expression Increment(Expression expression, bool post) {
            return new Expression(delegate(TextWriter writer) {
                if (!post) {
                    writer.Write("++");
                }

                expression.Flush(writer);

                if (post) {
                    writer.Write("++");
                }
            });
        }

        public Expression Decrement(Expression expression, bool post) {
            return new Expression(delegate(TextWriter writer) {
                if (!post) {
                    writer.Write("--");
                }

                expression.Flush(writer);

                if (post) {
                    writer.Write("--");
                }
            });
        }

        public Statement TryFinally(IEnumerable<Statement> tryBody, IEnumerable<Statement> finallyBody) {
            return TryCatchAllFinally(tryBody, null, null, finallyBody);
        }

        public Statement TryCatchAll(IEnumerable<Statement> tryBody, IEnumerable<Statement> catchAllExceptionsBody) {
            return TryCatchAllFinally(tryBody, CatchAllDefaultExceptionVar, catchAllExceptionsBody, null);
        }

        public Statement TryCatchAll(IEnumerable<Statement> tryBody, string catchAllExceptionVarName, IEnumerable<Statement> catchAllExceptionsBody) {
            return TryCatchAllFinally(tryBody, catchAllExceptionVarName, catchAllExceptionsBody, null);
        }

        public Statement TryCatchAllFinally(IEnumerable<Statement> tryBody, IEnumerable<Statement> catchAllExceptionsBody, 
                IEnumerable<Statement> finallyBody) {
            
            return TryCatchAllFinally(tryBody, CatchAllDefaultExceptionVar, catchAllExceptionsBody, finallyBody);
        }

        public Expression GetCatchAllDefaultExceptionVariable() {
            return Identifier(CatchAllDefaultExceptionVar);
        }

        public string GetCatchAllDefaultExceptionVariableName() {
            return CatchAllDefaultExceptionVar;
        }

        public virtual Statement TryCatchAllFinally(IEnumerable<Statement> tryBody, string catchAllExceptionVarName, 
                IEnumerable<Statement> catchAllExceptionsBody, IEnumerable<Statement> finallyBody) {
            return TryCatchSpecificExceptionFinally(tryBody, catchAllExceptionVarName, catchAllExceptionsBody, finallyBody, null);

        }

        public virtual Statement TryCatchSpecificExceptionFinally(IEnumerable<Statement> tryBody, string catchExceptionVarName,
               IEnumerable<Statement> catchAllExceptionsBody, IEnumerable<Statement> finallyBody, string ExceptionType) {

            return new Statement(delegate (TextWriter writer) {
                catchAllExceptionsBody = catchAllExceptionsBody.FilterEmpty();
                bool dumpCatch = !System.String.IsNullOrEmpty(catchExceptionVarName) && !catchAllExceptionsBody.IsEmpty();
                finallyBody = finallyBody.FilterEmpty();
                bool dumpFinally = !finallyBody.IsEmpty();
                bool dumpTry = dumpCatch || dumpFinally;

                if (dumpTry) {
                    writer.Write("try {");
                }

                FlushAllInternal(writer, tryBody);

                if (dumpCatch) {
                    if(ExceptionType.IsNullOrEmpty()) {
                        writer.WriteLine("}} catch ({0}) {{", GetCatchAllClause(catchExceptionVarName));
                    } else {
                        writer.WriteLine("}} catch ({0} {1}) {{", ExceptionType, catchExceptionVarName);
                    }

                    FlushAllInternal(writer, catchAllExceptionsBody);
                }

                if (dumpFinally) {
                    writer.WriteLine("} finally {");
                    FlushAllInternal(writer, finallyBody);
                }

                if (dumpTry) {
                    writer.WriteLine("}");
                }
            }, false);
        }

        public virtual Statement ReThrowExceptionInCatch(Expression exceptionVar = null) {
            return Throw(exceptionVar);
        }
        
        public Statement WhileTrue(params Statement[] body) {
            return WhileTrueInternal(body); 
        }

        public Statement WhileTrue(IEnumerable<Statement> body) {
            return WhileTrueInternal(body); 
        }

        public Expression This() {
            return Identifier("this");
        }

        protected bool IsDecimalDefaultValue(decimal value) {
            return value == 0.0M;
        }

        protected bool IsLongDefaultValue(long value) {
            return value == 0L;
        }

        protected bool IsDefaultDateTime(int year, int month, int day, int hours, int minutes, int seconds)
        {
            return year == 1900 && month == 1 && day == 1 &&
                   hours == 0 && minutes == 0 && seconds == 0;
        }

        public abstract Expression NewArray(string type, params Expression[] values);

        public abstract Expression DictionaryGet(Expression dict, Expression key);

        public abstract Statement DictionaryPut(Expression dict, Expression key, Expression val);

        public Expression Lambda(params Statement[] body) {
            return Lambda(/*arguments*/Enumerable.Empty<string>(), (IEnumerable<Statement>)body);
        }

        public Expression Lambda(IEnumerable<Statement> body) {
            return Lambda(/*arguments*/Enumerable.Empty<string>(), body);
        }
                
        public Expression Lambda(string argument, IEnumerable<Statement> body) {
            return Lambda(argument.ToEnumerable(), body);
        }

        public Expression Lambda(string argument, params Statement[] body) {
            return Lambda(argument.ToEnumerable(), (IEnumerable<Statement>)body);
        }

        public Expression Lambda(IEnumerable<string> arguments, params Statement[] body) {
            return Lambda(arguments, (IEnumerable<Statement>)body);
        }

        public abstract Expression Lambda(IEnumerable<string> arguments, IEnumerable<Statement> body);

        public Expression Lambda(string arguments, Expression body) {
            return Lambda(arguments.ToEnumerable(), body);
        }

        public abstract Expression Lambda(IEnumerable<string> arguments, Expression body);


        public virtual Expression OutParameter(Expression parameter) {
            return new Expression(writer => parameter.Flush(writer));
        }

        public abstract Statement DeclareOutParameterVariable(string type, string varName);

        public virtual Statement DeclareField(string type, Expression varName) {
            return DeclareField(type, varName, /*defaultvalue*/null);
        }

        public virtual Statement DeclareField(string type, Expression varName, Expression defaultvalue) {
            return new Statement(writer => {
                writer.Write("public ");
                writer.Write(type);
                writer.Write(" ");
                varName.Flush(writer);
                if (defaultvalue != null && !defaultvalue.IsNop) {
                    writer.Write(" = ");
                    defaultvalue.Flush(writer);
                }
            });
        }

        public abstract Statement Attribute(string name, params Expression[] attrs);
    }
}
