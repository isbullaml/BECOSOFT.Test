using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace BECOSOFT.Utilities.Expressions {
    internal class ExpressionParser {
        private readonly string _text;
        private readonly int _textLength;
        private int _textPosition;
        private char _char;
        private Token _token;

        private readonly Dictionary<string, object> _symbols;
        private IDictionary<string, object> _externals;
        private readonly Dictionary<Expression, string> _literals;

        private ParameterExpression _it;
        private readonly bool _noExceptions;
        private bool _cancelParse;

        internal ExpressionParser(string expression, ParameterExpression[] parameters, object[] values, bool noExceptions = false) {
            _text = expression;
            _textLength = expression.Length;
            _noExceptions = noExceptions;
            _symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _literals = new Dictionary<Expression, string>();
            if (parameters != null) { ProcessParameters(parameters); }
            if (values != null) { ProcessValues(values); }

            SetTextPosition(0);
            NextToken();
        }

        public Expression Parse(Type resultType) {
            var expressionPosition = _token.Position;
            var expression = ParseExpression();
            if (resultType != null) {
                var tempExpression = PromoteExpression(expression, resultType, true);
                if (tempExpression == null) {
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(expressionPosition, Res.ExpressionTypeMismatch, resultType.GetTypeName());
                }
                expression = tempExpression;
            }
            ValidateToken(TokenType.End);
            return expression;
        }

        // ?: operator
        private Expression ParseExpression() {
            if (_cancelParse) { return null; }
            var errorPosition = _token.Position;
            var expression = ParseNullCoalescing();
            if (_token.Type == TokenType.Question) {
                NextToken();
                var expr1 = ParseExpression();
                ValidateToken(TokenType.Colon);
                NextToken();
                var expr2 = ParseExpression();
                expression = GenerateConditional(expression, expr1, expr2, errorPosition);
            }
            return expression;
        }

        private Expression ParseNullCoalescing() {
            if (_cancelParse) { return null; }
            var errorPosition = _token.Position;
            var leftExpression = ParseLogicalOr();
            if (_token.Type == TokenType.NullCoalesce) {
                NextToken();
                var rightExpression = ParseExpression();
                var test = Expression.Equal(leftExpression, Expression.Constant(null, leftExpression.Type));
                if (leftExpression.Type.IsNullableType()) {
                    leftExpression = Expression.Property(leftExpression, "Value");
                }
                leftExpression = GenerateConditional(test, rightExpression, leftExpression, errorPosition);
            }
            return leftExpression;
        }

        // ||, or operator
        private Expression ParseLogicalOr() {
            if (_cancelParse) { return null; }
            var left = ParseLogicalAnd();
            while (_token.Type == TokenType.DoubleBar || TokenIdentifierIs("or")) {
                var op = _token;
                NextToken();
                var right = ParseLogicalAnd();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Position);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        // &&, and operator
        private Expression ParseLogicalAnd() {
            if (_cancelParse) { return null; }
            var left = ParseComparison();
            while (_token.Type == TokenType.DoubleAmpersand || TokenIdentifierIs("and")) {
                var op = _token;
                NextToken();
                var right = ParseComparison();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Position);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        // =, ==, !=, <>, >, >=, <, <= operators
        private Expression ParseComparison() {
            if (_cancelParse) { return null; }
            var left = ParseAdditive();
            if (_cancelParse) { return null; }
            while (_token.Type == TokenType.Equal
                   || _token.Type == TokenType.DoubleEqual
                   || _token.Type == TokenType.ExclamationEqual || _token.Type == TokenType.LessGreater
                   || _token.Type == TokenType.GreaterThan || _token.Type == TokenType.GreaterThanEqual
                   || _token.Type == TokenType.LessThan || _token.Type == TokenType.LessThanEqual) {
                var op = _token;
                NextToken();
                var right = ParseAdditive();
                if (_cancelParse) { return null; }
                var isEquality = op.Type == TokenType.Equal || op.Type == TokenType.DoubleEqual || op.Type == TokenType.ExclamationEqual || op.Type == TokenType.LessGreater;
                if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType) {
                    if (left == NullLiteral) {
                        left = Expression.Default(right.Type);
                    }
                    if (right == NullLiteral) {
                        right = Expression.Default(left.Type);
                    }
                    if (left.Type != right.Type) {
                        if (left.Type.IsAssignableFrom(right.Type)) {
                            right = Expression.Convert(right, left.Type);
                        } else if (right.Type.IsAssignableFrom(left.Type)) {
                            left = Expression.Convert(left, right.Type);
                        } else {
                            if (_noExceptions) {
                                _cancelParse = true;
                                return null;
                            }
                            throw IncompatibleOperandsError(op.Text, left, right, op.Position);
                        }
                    }
                } else if (left.Type.IsEnumType() || right.Type.IsEnumType()) {
                    if (left.Type != right.Type) {
                        Expression e;
                        if ((e = PromoteExpression(right, left.Type, true)) != null) {
                            right = e;
                        } else if ((e = PromoteExpression(left, right.Type, true)) != null) {
                            left = e;
                        } else {
                            if (_noExceptions) {
                                _cancelParse = true;
                                return null;
                            }
                            throw IncompatibleOperandsError(op.Text, left, right, op.Position);
                        }
                    }
                } else {
                    var signatureType = isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures);
                    CheckAndPromoteOperands(signatureType, op.Text, ref left, ref right, op.Position);
                }
                switch (op.Type) {
                    case TokenType.Equal:
                    case TokenType.DoubleEqual:
                        left = GenerateEqual(left, right);
                        break;
                    case TokenType.ExclamationEqual:
                    case TokenType.LessGreater:
                        left = GenerateNotEqual(left, right);
                        break;
                    case TokenType.GreaterThan:
                        left = GenerateGreaterThan(left, right);
                        break;
                    case TokenType.GreaterThanEqual:
                        left = GenerateGreaterThanEqual(left, right);
                        break;
                    case TokenType.LessThan:
                        left = GenerateLessThan(left, right);
                        break;
                    case TokenType.LessThanEqual:
                        left = GenerateLessThanEqual(left, right);
                        break;
                }
            }
            return left;
        }

        private Expression ParseAdditive() {
            if (_cancelParse) { return null; }
            var left = ParseMultiplicative();
            while (_token.Type == TokenType.Plus || _token.Type == TokenType.Minus || _token.Type == TokenType.Ampersand) {
                var op = _token;
                NextToken();
                var right = ParseMultiplicative();
                switch (op.Type) {
                    case TokenType.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string)) {
                            goto case TokenType.Ampersand;
                        }
                        CheckAndPromoteOperands(typeof(IAddSignatures), op.Text, ref left, ref right, op.Position);
                        left = GenerateAdd(left, right);
                        break;
                    case TokenType.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), op.Text, ref left, ref right, op.Position);
                        left = GenerateSubtract(left, right);
                        break;
                    case TokenType.Ampersand:
                        left = GenerateStringConcat(left, right);
                        break;
                }
            }
            return left;
        }

        private Expression ParseMultiplicative() {
            if (_cancelParse) { return null; }
            var left = ParseUnary();
            while (_token.Type == TokenType.Asterisk || _token.Type == TokenType.Slash || _token.Type == TokenType.Percent || TokenIdentifierIs("mod")) {
                var op = _token;
                NextToken();
                var right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.Text, ref left, ref right, op.Position);
                switch (op.Type) {
                    case TokenType.Asterisk:
                        left = Expression.Multiply(left, right);
                        break;
                    case TokenType.Slash:
                        left = Expression.Divide(left, right);
                        break;
                    case TokenType.Percent:
                    case TokenType.Identifier:
                        left = Expression.Modulo(left, right);
                        break;
                }
            }
            return left;
        }

        private Expression ParseUnary() {
            if (_cancelParse) { return null; }
            if (_token.Type == TokenType.Minus || _token.Type == TokenType.Exclamation || TokenIdentifierIs("not")) {
                var op = _token;
                NextToken();
                if (op.Type == TokenType.Minus && (_token.Type == TokenType.IntegerLiteral || _token.Type == TokenType.RealLiteral)) {
                    _token.Text = "-" + _token.Text;
                    _token.Position = op.Position;
                    return ParsePrimary();
                }
                var expression = ParseUnary();
                if (op.Type == TokenType.Minus) {
                    CheckAndPromoteOperand(typeof(INegationSignatures), op.Text, ref expression, op.Position);
                    expression = Expression.Negate(expression);
                } else {
                    CheckAndPromoteOperand(typeof(INotSignatures), op.Text, ref expression, op.Position);
                    expression = Expression.Not(expression);
                }
                return expression;
            }
            return ParsePrimary();
        }

        private Expression ParsePrimary() {
            if (_cancelParse) { return null; }
            var expression = ParsePrimaryStart();
            while (true) {
                if (_token.Type == TokenType.Dot) {
                    NextToken();
                    expression = ParseMemberAccess(null, expression);
                } else if (_token.Type == TokenType.NullPropagation) {
                    NextToken();
                    var errorPosition = _token.Position;
                    var nullableAccessExpression = expression;
                    if (expression.Type.IsNullableType()) {
                        nullableAccessExpression = Expression.Property(expression, "Value");
                    }
                    var memberAccessExpression = ParseMemberAccess(null, nullableAccessExpression);
                    var memberAccessType = memberAccessExpression.Type;
                    if (memberAccessType.IsPrimitive && !memberAccessType.IsNullableType()) {
                        var nullableType = typeof(Nullable<>);
                        var concreteNullableType = nullableType.MakeGenericType(memberAccessType);
                        memberAccessExpression = Expression.Convert(memberAccessExpression, concreteNullableType);
                    }
                    var test = Expression.Equal(expression, Expression.Constant(null, expression.Type));
                    expression = GenerateConditional(test, Expression.Constant(null, memberAccessExpression.Type), memberAccessExpression, errorPosition);
                } else if (_token.Type == TokenType.OpenBracket) {
                    expression = ParseElementAccess(expression);
                } else {
                    break;
                }
            }
            return expression;
        }

        private Expression ParsePrimaryStart() {
            if (_cancelParse) { return null; }
            switch (_token.Type) {
                case TokenType.Identifier:
                    return ParseIdentifier();
                case TokenType.StringLiteral:
                    return ParseStringLiteral();
                case TokenType.IntegerLiteral:
                    return ParseIntegerLiteral();
                case TokenType.RealLiteral:
                    return ParseRealLiteral();
                case TokenType.OpenParenthesis:
                    return ParseParenthesisExpression();
                default:
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(Res.ExpressionExpected);
            }
        }

        private Expression ParseStringLiteral() {
            if (_cancelParse) { return null; }
            ValidateToken(TokenType.StringLiteral);
            var quote = _token.Text[0];
            var s = _token.Text.Substring(1, _token.Text.Length - 2);
            s = s.Replace("\\n", "\n");
            var start = 0;
            while (true) {
                var i = s.IndexOf(quote, start);
                if (i < 0) {
                    break;
                }
                s = s.Remove(i, 1);
                start = i + 1;
            }
            if (quote == '\'') {
                if (s.Length != 1) {
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(Res.InvalidCharacterLiteral);
                }
                NextToken();
                return CreateLiteral(s[0], s);
            }
            NextToken();
            return CreateLiteral(s, s);
        }

        private Expression ParseIntegerLiteral() {
            if (_cancelParse) { return null; }
            ValidateToken(TokenType.IntegerLiteral);
            var text = _token.Text;
            if (text[0] != '-') {
                var value = Converter.GetValue<ulong?>(text);
                if (value == null) {
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                }
                NextToken();
                if (value.Value <= (ulong) int.MaxValue) { return CreateLiteral((int) value.Value, text); }
                if (value.Value <= (ulong) uint.MaxValue) { return CreateLiteral((uint) value.Value, text); }
                if (value.Value <= (ulong) long.MaxValue) { return CreateLiteral((long) value.Value, text); }
                return CreateLiteral(value.Value, text);
            } else {
                var value = Converter.GetValue<long?>(text);
                if (value == null) {
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                }
                NextToken();
                if (value.Value.LiesBetween(int.MinValue, int.MaxValue)) {
                    return CreateLiteral((int) value.Value, text);
                }
                return CreateLiteral(value.Value, text);
            }
        }

        private Expression ParseRealLiteral() {
            if (_cancelParse) { return null; }
            ValidateToken(TokenType.RealLiteral);
            var text = _token.Text;
            object value = null;
            var last = text[text.Length - 1];
            if (last == 'F' || last == 'f') {
                var temp = Converter.GetValue<float?>(text.Substring(0, text.Length - 1));
                if (temp.HasValue) {
                    value = temp.Value;
                }
            } else {
                var temp = Converter.GetValue<double?>(text);
                if (temp.HasValue) {
                    value = temp.Value;
                }
            }
            if (value == null) {
                if (_noExceptions) {
                    _cancelParse = true;
                    return null;
                }
                throw ParseError(Res.InvalidRealLiteral, text);
            }
            NextToken();
            return CreateLiteral(value, text);
        }

        private Expression ParseParenthesisExpression() {
            if (_cancelParse) { return null; }
            ValidateToken(TokenType.OpenParenthesis);
            NextToken();
            var expression = ParseExpression();
            ValidateToken(TokenType.CloseParenthesis);
            NextToken();
            return expression;
        }

        private Expression ParseIdentifier() {
            if (_cancelParse) { return null; }
            ValidateToken(TokenType.Identifier);
            object value;
            if (Keywords.TryGetValue(_token.Text, out value)) {
                if (value is Type) { return ParseTypeAccess((Type) value); }
                if (value == (object) KeywordIt) { return ParseIt(); }
                if (value == (object) KeywordIif) { return ParseIif(); }
                if (value == (object) KeywordNew) { return ParseNew(); }
                NextToken();
                return (Expression) value;
            }
            if (_symbols.TryGetValue(_token.Text, out value) || (_externals != null && _externals.TryGetValue(_token.Text, out value))) {
                var expression = value as Expression;
                if (expression == null) {
                    expression = Expression.Constant(value);
                } else {
                    var lambda = expression as LambdaExpression;
                    if (lambda != null) {
                        return ParseLambdaInvocation(lambda);
                    }
                }
                NextToken();
                return expression;
            }
            if (_it != null) {
                return ParseMemberAccess(null, _it);
            }
            var type = Type.GetType(_token.Text);
            if (type != null) {
                PredefinedTypes.Add(type);
                return ParseTypeAccess(type);
            }
            type = AssemblyHelpers.FindLoadedType(_token.Text);
            if (type != null) {
                PredefinedTypes.Add(type);
                return ParseTypeAccess(type);
            }
            if (_noExceptions) {
                _cancelParse = true;
                return null;
            }
            throw ParseError(Res.UnknownIdentifier, _token.Text);
        }

        private Expression ParseIt() {
            if (_cancelParse) { return null; }
            if (_it == null) {
                throw ParseError(Res.NoItInScope);
            }
            NextToken();
            return _it;
        }

        private Expression ParseIif() {
            if (_cancelParse) { return null; }
            var errorPos = _token.Position;
            NextToken();
            var args = ParseArgumentList();
            if (args.Length != 3) {
                throw ParseError(errorPos, Res.IifRequiresThreeArgs);
            }
            return GenerateConditional(args[0], args[1], args[2], errorPos);
        }

        private Expression ParseNew() {
            if (_cancelParse) { return null; }
            NextToken();
            ValidateToken(TokenType.OpenParenthesis);
            NextToken();
            var properties = new List<DynamicProperty>();
            var expressions = new List<Expression>();
            while (true) {
                var expressionPosition = _token.Position;
                var expression = ParseExpression();
                string propertyName;
                if (TokenIdentifierIs("as")) {
                    NextToken();
                    propertyName = GetIdentifier();
                    NextToken();
                } else {
                    var memberExpression = expression as MemberExpression;
                    if (memberExpression == null) {
                        throw ParseError(expressionPosition, Res.MissingAsClause);
                    }
                    propertyName = memberExpression.Member.Name;
                }
                expressions.Add(expression);
                properties.Add(new DynamicProperty(propertyName, expression.Type));
                if (_token.Type != TokenType.Comma) { break; }
                NextToken();
            }
            ValidateToken(TokenType.CloseParenthesis);
            NextToken();
            var type = DynamicExpression.CreateClass(properties);
            var bindings = new MemberBinding[properties.Count];
            for (var i = 0; i < bindings.Length; i++) {
                bindings[i] = Expression.Bind(type.GetProperty(properties[i].Name), expressions[i]);
            }
            return Expression.MemberInit(Expression.New(type), bindings);
        }

        private Expression ParseLambdaInvocation(LambdaExpression lambda) {
            if (_cancelParse) { return null; }
            var errorPosition = _token.Position;
            NextToken();
            var args = ParseArgumentList();
            MethodBase method;
            if (FindMethod(lambda.Type, "Invoke", false, args, null, out method) != 1) {
                throw ParseError(errorPosition, Res.ArgsIncompatibleWithLambda);
            }
            return Expression.Invoke(lambda, args);
        }

        private Expression ParseTypeAccess(Type type) {
            if (_cancelParse) { return null; }
            var errorPosition = _token.Position;
            NextToken();
            if (_token.Type == TokenType.Question) {
                if (!type.IsValueType || type.IsNullableType()) {
                    throw ParseError(errorPosition, Res.TypeHasNoNullableForm, type.GetTypeName());
                }
                type = typeof(Nullable<>).MakeGenericType(type);
                NextToken();
            }
            if (_token.Type == TokenType.OpenParenthesis) {
                var args = ParseArgumentList();
                MethodBase method;
                switch (FindBestMethod(type.GetConstructors(), args, out method)) {
                    case 0:
                        if (args.Length == 1) {
                            return GenerateConversion(args[0], type, errorPosition);
                        }
                        throw ParseError(errorPosition, Res.NoMatchingConstructor, type.GetTypeName());
                    case 1:
                        return Expression.New((ConstructorInfo) method, args);
                    default:
                        throw ParseError(errorPosition, Res.AmbiguousConstructorInvocation, type.GetTypeName());
                }
            }
            ValidateToken(TokenType.Dot);
            NextToken();
            return ParseMemberAccess(type, null);
        }

        private Expression GenerateConversion(Expression expression, Type type, int errorPosition) {
            var exprType = expression.Type;
            if (exprType == type) { return expression; }
            if (exprType.IsValueType && type.IsValueType) {
                if ((exprType.IsNullableType() || type.IsNullableType()) && type.GetNonNullableType() == type.GetNonNullableType()) {
                    return Expression.Convert(expression, type);
                }
                if ((exprType.IsNumeric() || exprType.IsEnumType())
                    && (type.IsNullableType() || type.IsEnumType())) {
                    return Expression.ConvertChecked(expression, type);
                }
            }
            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) || exprType.IsInterface || type.IsInterface) {
                return Expression.Convert(expression, type);
            }
            throw ParseError(errorPosition, Res.CannotConvertValue, exprType.GetTypeName(), type.GetTypeName());
        }

        private Expression ParseMemberAccess(Type type, Expression instance) {
            if (_cancelParse) { return null; }
            if (instance != null && type == null) {
                type = instance.Type;
            }
            var errorPosition = _token.Position;
            var id = GetIdentifier();
            NextToken();
            if (_token.Type == TokenType.OpenParenthesis) {
                if (instance != null && type != typeof(string)) {
                    var enumerableType = FindGenericType(typeof(IEnumerable<>), type);
                    if (enumerableType != null) {
                        var elementType = enumerableType.GetGenericArguments()[0];
                        return ParseAggregate(instance, elementType, id, errorPosition);
                    }
                }
                var args = ParseArgumentList();
                MethodBase method;
                switch (FindMethod(type, id, instance == null, args, instance, out method)) {
                    case 0:
                        if (_noExceptions) {
                            _cancelParse = true;
                            return null;
                        }

                        throw ParseError(errorPosition, Res.NoApplicableMethod, id, type.GetTypeName());
                    case 1:
                        var methodInfo = (MethodInfo) method;
                        if (!IsPredefinedType(methodInfo.DeclaringType)) {
                            if (_noExceptions) {
                                _cancelParse = true;
                                return null;
                            }
                            throw ParseError(errorPosition, Res.MethodsAreInaccessible, method.DeclaringType.GetTypeName());
                        }
                        if (methodInfo.ReturnType == typeof(void)) {
                            if (_noExceptions) {
                                _cancelParse = true;
                                return null;
                            }
                            throw ParseError(errorPosition, Res.MethodIsVoid, id, method.DeclaringType.GetTypeName());
                        }
                        if (methodInfo.IsStatic && instance != null) {
                            var newArgs = args.ToList();
                            newArgs.Insert(0, instance);
                            args = newArgs.ToArray();
                            var methodParams = methodInfo.GetParameters();
                            var convertedArgs = args.Select((a, index) => a.Type == methodParams[index].ParameterType ? a : Expression.Convert(a, typeof(object))).ToArray();
                            return Expression.Call(null, methodInfo, convertedArgs);
                        } else {
                            var methodParams = methodInfo.GetParameters();
                            var convertedArgs = args.Select((a, index) => a.Type == methodParams[index].ParameterType 
                                                                          ||(methodParams[index].ParameterType.IsInterface 
                                                                             && methodParams[index].ParameterType.IsAssignableFrom(a.Type)) ? a : Expression.Convert(a, typeof(object))).ToArray();
                            return Expression.Call(instance, methodInfo, convertedArgs);
                        }
                    default:
                        if (_noExceptions) {
                            _cancelParse = true;
                            return null;
                        }
                        throw ParseError(errorPosition, Res.AmbiguousMethodInvocation, id, type.GetTypeName());
                }
            }
            var member = FindPropertyOrField(type, id, instance == null);
            if (member == null) {
                if (_noExceptions) {
                    _cancelParse = true;
                    return null;
                }
                throw ParseError(errorPosition, Res.UnknownPropertyOrField, id, type.GetTypeName());
            }
            var propInfo = member as PropertyInfo;
            if (propInfo == null) {
                return Expression.Field(instance, (FieldInfo) member);
            }
            return Expression.Property(instance, propInfo);
        }

        private Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPosition) {
            if (_cancelParse) { return null; }
            var outerIt = _it;
            var innerIt = Expression.Parameter(elementType, "");
            _it = innerIt;
            var args = ParseArgumentList();
            _it = outerIt;
            MethodBase signature;
            if (FindMethod(typeof(IEnumerableSignatures), methodName, false, args, null, out signature) != 1) {
                if (_noExceptions) {
                    _cancelParse = true;
                    return null;
                }
                throw ParseError(errorPosition, Res.NoApplicableAggregate, methodName);
            }
            Type[] typeArgs;
            if (signature.Name.Equals("Min") || signature.Name.Equals("Max")) {
                typeArgs = new[] { elementType, args[0].Type };
            } else {
                typeArgs = new[] { elementType };
            }
            if (args.Length == 0) {
                args = new[] { instance };
            } else {
                if (args[0] is ConstantExpression) {
                    args = new[] { instance, args[0] };
                } else {
                    args = new[] { instance, Expression.Lambda(args[0], innerIt) };
                }
            }
            return Expression.Call(typeof(Enumerable), signature.Name, typeArgs, args);
        }

        private Expression[] ParseArgumentList() {
            if (_cancelParse) { return null; }
            ValidateToken(TokenType.OpenParenthesis);
            NextToken();
            var args = _token.Type != TokenType.CloseParenthesis ? ParseArguments() : new Expression[0];
            ValidateToken(TokenType.CloseParenthesis);
            NextToken();
            return args;
        }

        private Expression[] ParseArguments() {
            if (_cancelParse) { return null; }
            var argList = new List<Expression>();
            while (true) {
                argList.Add(ParseExpression());
                if (_token.Type != TokenType.Comma) { break; }
                NextToken();
            }
            return argList.ToArray();
        }

        private Expression ParseElementAccess(Expression expression) {
            if (_cancelParse) { return null; }
            var errorPosition = _token.Position;
            ValidateToken(TokenType.OpenBracket);
            NextToken();
            var args = ParseArguments();
            ValidateToken(TokenType.CloseBracket);
            NextToken();
            if (expression.Type.IsArray) {
                if (expression.Type.GetArrayRank() != 1 || args.Length != 1) {
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(errorPosition, Res.CannotIndexMultiDimArray);
                }
                var index = PromoteExpression(args[0], typeof(int), true);
                if (index == null) {
                    throw ParseError(errorPosition, Res.InvalidIndex);
                }
                return Expression.ArrayIndex(expression, index);
            }
            MethodBase mb;
            switch (FindIndexer(expression.Type, args, out mb)) {
                case 0:
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(errorPosition, Res.NoApplicableIndexer, expression.Type.GetTypeName());
                case 1:
                    return Expression.Call(expression, (MethodInfo) mb, args);
                default:
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(errorPosition, Res.AmbiguousIndexerInvocation, expression.Type.GetTypeName());
            }
        }

        private static bool IsPredefinedType(Type type) {
            if (PredefinedTypes.Contains(type)) {
                return true;
            }
            return PredefinedTypes.Add(type);
        }

        private static Type FindGenericType(Type generic, Type type) {
            while (type != null && type != typeof(object)) {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic) {
                    return type;
                }
                if (generic.IsInterface) {
                    foreach (var interfaceType in type.GetInterfaces()) {
                        var found = FindGenericType(generic, interfaceType);
                        if (found != null) {
                            return found;
                        }
                    }
                    type = type.BaseType;
                }
            }
            return null;
        }

        private Expression CreateLiteral(object value, string text) {
            var expr = Expression.Constant(value);
            _literals.Add(expr, text);
            return expr;
        }

        private Expression GenerateConditional(Expression test, Expression ifTrue, Expression ifFalse, int errorPosition) {
            if (test.Type != typeof(bool)) {
                if (_noExceptions) {
                    _cancelParse = true;
                    return null;
                }
                throw ParseError(errorPosition, Res.FirstExprMustBeBool);
            }
            if (ifTrue.Type != ifFalse.Type) {
                var ifTrueAsIfFalse = ifFalse != NullLiteral ? PromoteExpression(ifTrue, ifFalse.Type, true) : null;
                var ifFalseAsIfTrue = ifTrue != NullLiteral ? PromoteExpression(ifFalse, ifTrue.Type, true) : null;
                if (ifTrueAsIfFalse != null && ifFalseAsIfTrue == null) {
                    ifTrue = ifTrueAsIfFalse;
                } else if (ifFalseAsIfTrue != null && ifTrueAsIfFalse == null) {
                    ifFalse = ifFalseAsIfTrue;
                } else {
                    var typeTrue = ifTrue != NullLiteral ? ifTrue.Type.Name : "null";
                    var typeFalse = ifFalse != NullLiteral ? ifFalse.Type.Name : "null";
                    if (ifTrueAsIfFalse != null && ifFalseAsIfTrue != null) {
                        if (_noExceptions) {
                            _cancelParse = true;
                            return null;
                        }
                        throw ParseError(errorPosition, Res.BothTypesConvertToOther, typeTrue, typeFalse);
                    }
                    if (_noExceptions) {
                        _cancelParse = true;
                        return null;
                    }
                    throw ParseError(errorPosition, Res.NeitherTypeConvertsToOther, typeTrue, typeFalse);
                }
            }
            return Expression.Condition(test, ifTrue, ifFalse);
        }

        private Expression GenerateEqual(Expression left, Expression right) {
            return Expression.Equal(left, right);
        }

        private Expression GenerateNotEqual(Expression left, Expression right) {
            return Expression.NotEqual(left, right);
        }

        private Expression GenerateGreaterThan(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.GreaterThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.GreaterThan(left, right);
        }

        private Expression GenerateGreaterThanEqual(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.GreaterThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.GreaterThanOrEqual(left, right);
        }

        private Expression GenerateLessThan(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.LessThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.LessThan(left, right);
        }

        private Expression GenerateLessThanEqual(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.LessThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.LessThanOrEqual(left, right);
        }

        private Expression GenerateAdd(Expression left, Expression right) {
            if (left.Type == typeof(string) && right.Type == typeof(string)) {
                return GenerateStaticMethodCall("Concat", left, right);
            }
            return Expression.Add(left, right);
        }

        private Expression GenerateSubtract(Expression left, Expression right) {
            return Expression.Subtract(left, right);
        }

        private Expression GenerateStringConcat(Expression left, Expression right) {
            if (left.Type != typeof(string)) {
                var toStringMethod = left.Type.GetMethod("ToString", Type.EmptyTypes);
                if (toStringMethod != null) {
                    left = Expression.Call(left, toStringMethod);
                }
            }
            if (right.Type != typeof(string)) {
                var toStringMethod = right.Type.GetMethod("ToString", Type.EmptyTypes);
                if (toStringMethod != null) {
                    right = Expression.Call(right, toStringMethod);
                }
            }
            return Expression.Call(
                null,
                typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                new[] { left, right });
        }

        private Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right) {
            return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
        }

        private MethodInfo GetStaticMethod(string methodName, Expression left, Expression right) {
            return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        }

        private Expression PromoteExpression(Expression expression, Type type, bool exact) {
            if (_cancelParse) { return null; }
            if (expression.Type == type) { return expression; }
            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null) {
                if (constantExpression == NullLiteral) {
                    if (!type.IsValueType || type.IsNullableType()) {
                        return Expression.Constant(null, type);
                    }
                } else {
                    string text;
                    if (_literals.TryGetValue(constantExpression, out text)) {
                        var target = type.GetNonNullableType();
                        if (!target.IsInterface) {
                            var converter = Converter.GetDelegate(target);
                            var value = converter(text);
                            if (value != null) {
                                return Expression.Constant(value, type);
                            }
                        }
                    }
                }
            }
            if (expression.Type.IsCompatibleWith(type)) {
                if (type.IsValueType || exact) {
                    return Expression.Convert(expression, type);
                }
                return expression;
            }
            return null;
        }

        private void NextToken() {
            while (char.IsWhiteSpace(_char)) {
                NextChar();
            }
            var type = TokenType.Unknown;
            var tokenPosition = _textPosition;
            switch (_char) {
                case '!':
                    NextChar();
                    if (_char == '=') {
                        NextChar();
                        type = TokenType.ExclamationEqual;
                    } else {
                        type = TokenType.Exclamation;
                    }
                    break;
                case '%':
                    NextChar();
                    type = TokenType.Percent;
                    break;
                case '&':
                    NextChar();
                    if (_char == '&') {
                        NextChar();
                        type = TokenType.DoubleAmpersand;
                    } else {
                        type = TokenType.Ampersand;
                    }
                    break;
                case '(':
                    NextChar();
                    type = TokenType.OpenParenthesis;
                    break;
                case ')':
                    NextChar();
                    type = TokenType.CloseParenthesis;
                    break;
                case '*':
                    NextChar();
                    type = TokenType.Asterisk;
                    break;
                case '+':
                    NextChar();
                    type = TokenType.Plus;
                    break;
                case '-':
                    NextChar();
                    type = TokenType.Minus;
                    break;
                case ',':
                    NextChar();
                    type = TokenType.Comma;
                    break;
                case '.':
                    NextChar();
                    type = TokenType.Dot;
                    break;
                case '/':
                    NextChar();
                    type = TokenType.Slash;
                    break;
                case ':':
                    NextChar();
                    type = TokenType.Colon;
                    break;
                case '<':
                    NextChar();
                    if (_char == '=') {
                        NextChar();
                        type = TokenType.LessThanEqual;
                    } else if (_char == '>') {
                        NextChar();
                        type = TokenType.LessGreater;
                    } else {
                        type = TokenType.LessThan;
                    }
                    break;
                case '=':
                    NextChar();
                    if (_char == '=') {
                        NextChar();
                        type = TokenType.DoubleEqual;
                    } else {
                        type = TokenType.Equal;
                    }
                    break;
                case '>':
                    NextChar();
                    if (_char == '=') {
                        NextChar();
                        type = TokenType.GreaterThanEqual;
                    } else {
                        type = TokenType.GreaterThan;
                    }
                    break;
                case '?':
                    NextChar();
                    if (_char == '.') {
                        NextChar();
                        type = TokenType.NullPropagation;
                    } else if (_char == '?') {
                        NextChar();
                        type = TokenType.NullCoalesce;
                    } else {
                        type = TokenType.Question;
                    }
                    break;
                case '[':
                    NextChar();
                    type = TokenType.OpenBracket;
                    break;
                case ']':
                    NextChar();
                    type = TokenType.CloseBracket;
                    break;
                case '|':
                    NextChar();
                    if (_char == '|') {
                        NextChar();
                        type = TokenType.DoubleBar;
                    } else {
                        type = TokenType.Bar;
                    }
                    break;
                case '"':
                case '\'':
                    var quote = _char;
                    do {
                        NextChar();
                        while (_textPosition < _textLength && _char != quote) {
                            NextChar();
                        }
                        if (_textPosition == _textLength) {
                            if (_noExceptions) {
                                _cancelParse = true;
                                return;
                            }
                            throw ParseError(_textPosition, Res.UnterminatedStringLiteral);
                        }
                        NextChar();
                    } while (_char == quote);
                    type = TokenType.StringLiteral;
                    break;
                default:
                    if (char.IsLetter(_char) || _char == '@' || _char == '_') {
                        do {
                            NextChar();
                        } while (char.IsLetterOrDigit(_char) || _char == '_');
                        type = TokenType.Identifier;
                        break;
                    }
                    if (char.IsDigit(_char)) {
                        type = TokenType.IntegerLiteral;
                        do {
                            NextChar();
                        } while (char.IsDigit(_char));
                        if (_char == '.') {
                            type = TokenType.RealLiteral;
                            NextChar();
                            ValidateDigit();
                            do {
                                NextChar();
                            } while (char.IsDigit(_char));
                        }
                        if (_char == 'E' || _char == 'e') {
                            type = TokenType.RealLiteral;
                            NextChar();
                            if (_char == '+' || _char == '-') {
                                NextChar();
                            }
                            ValidateDigit();
                            do {
                                NextChar();
                            } while (char.IsDigit(_char));
                        }
                        if (_char == 'F' || _char == 'f') {
                            NextChar();
                        }
                        break;
                    }
                    if (_textPosition == _textLength) {
                        type = TokenType.End;
                        break;
                    }
                    if (_noExceptions) {
                        _cancelParse = true;
                        return;
                    }
                    throw ParseError(_textPosition, Res.InvalidCharacter, _char);
            }
            _token.Type = type;
            _token.Text = _text.Substring(tokenPosition, _textPosition - tokenPosition);
            _token.Position = tokenPosition;
        }

        private void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos) {
            var args = new[] { expr };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, null, out method) != 1) {
                if (_noExceptions) {
                    _cancelParse = true;
                    return;
                }
                throw ParseError(errorPos, Res.IncompatibleOperand, opName, args[0].Type.GetTypeName());
            }
            expr = args[0];
        }

        private void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPosition) {
            var args = new[] { left, right };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, null, out method) != 1) {
                if (_noExceptions) {
                    _cancelParse = true;
                    return;
                }
                throw IncompatibleOperandsError(opName, left, right, errorPosition);
            }
            left = args[0];
            right = args[1];
        }

        private static MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess) {
            var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                        (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            var types = SelfAndBaseTypes(type);
            foreach (var t in types) {
                var members = t.FindMembers(MemberTypes.Property | MemberTypes.Field,
                                            flags, Type.FilterNameIgnoreCase, memberName);
                if (members.Length != 0) { return members[0]; }
            }
            return null;
        }

        private int FindIndexer(Type type, Expression[] args, out MethodBase method) {
            foreach (var t in SelfAndBaseTypes(type)) {
                var members = t.GetDefaultMembers();
                if (members.Length == 0) {
                    continue;
                }
                var methods = members.OfType<PropertyInfo>().Select(p => (MethodBase) p.GetGetMethod()).Where(m => m != null);
                var count = FindBestMethod(methods, args, out method);
                if (count != 0) { return count; }
            }
            method = null;
            return 0;
        }

        private int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, Expression callingExpression, out MethodBase method) {
            var flags = BindingFlags.Public | BindingFlags.DeclaredOnly | (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (var t in SelfAndBaseTypes(type)) {
                var members = t.GetMethods(flags).Where(m => m.Name.EqualsIgnoreCase(methodName)).ToArray();
                if (members.Length == 0 && t.IsEnum) {
                    members = typeof(Enum).GetMethods(flags).Where(m => m.Name.EqualsIgnoreCase(methodName)).ToArray();
                }
                for (var i = 0; i < members.Length; i++) {
                    var member = members[0];
                    if (member == null || !member.IsGenericMethod) { continue; }
                    var genericArgCount = member.GetGenericArguments().Length;
                    var tempArgs = (genericArgCount < args.Length ? args.Skip(args.Length - genericArgCount) : args).Select(a => a.Type).ToArray();
                    members[0] = member.MakeGenericMethod(tempArgs);
                }
                var count = FindBestMethod(members, args, out method);
                if (count != 0) {
                    return count;
                }
            }
            var extensionType = AssemblyHelpers.FindTypeByExtensionMethod(methodName, type, args.Select(a => a?.Type).ToArray());
            if (extensionType != null) {
                var newArgs = args.ToList();
                newArgs.Insert(0, callingExpression);
                args = newArgs.ToArray();
                var cnt = FindMethod(extensionType, methodName, true, args, callingExpression, out method);
                if (cnt == 1) {
                    PredefinedTypes.Add(extensionType);
                }
                return cnt;
            }
            method = null;
            return 0;
        }

        private static IEnumerable<Type> SelfAndBaseTypes(Type type) {
            if (type.IsInterface) {
                var types = new List<Type>();
                AddInterface(types, type);
                return types;
            }
            return SelfAndBaseClasses(type);
        }

        private static IEnumerable<Type> SelfAndBaseClasses(Type type) {
            while (type != null) {
                yield return type;
                type = type.BaseType;
            }
        }

        private class MethodData {
            public MethodBase MethodBase;
            public ParameterInfo[] Parameters;
            public Expression[] Args;
        }

        private int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method) {
            var applicable = methods.Select(m => new MethodData {
                MethodBase = m,
                Parameters = m.GetParameters()
            }).Where(m => IsApplicable(m, args)).ToArray();
            if (applicable.Length > 1) {
                applicable = applicable.Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).ToArray();
            }
            if (applicable.Length == 1) {
                var md = applicable[0];
                for (var i = 0; i < args.Length; i++) {
                    args[i] = md.Args[i];
                }
                method = md.MethodBase;
            } else {
                method = null;
            }
            return applicable.Length;
        }


        private static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2) {
            var better = false;
            for (var i = 0; i < args.Length; i++) {
                var c = CompareConversions(args[i].Type,
                                           m1.Parameters[i].ParameterType,
                                           m2.Parameters[i].ParameterType);
                if (c < 0) { return false; }
                if (c > 0) { better = true; }
            }
            return better;
        }

        // Return 1 if s -> t1 is a better conversion than s -> t2
        // Return -1 if s -> t2 is a better conversion than s -> t1
        // Return 0 if neither conversion is better
        private static int CompareConversions(Type s, Type t1, Type t2) {
            if (t1 == t2) { return 0; }
            if (s == t1) { return 1; }
            if (s == t2) { return -1; }
            var t1t2 = t1.IsCompatibleWith(t2);
            var t2t1 = t2.IsCompatibleWith(t1);
            if (t1t2 && !t2t1) { return 1; }
            if (t2t1 && !t1t2) { return -1; }
            if (t1.IsSignedInteger() && t2.IsUnsignedInteger()) { return 1; }
            if (t2.IsSignedInteger() && t1.IsUnsignedInteger()) { return -1; }
            return 0;
        }

        private bool IsApplicable(MethodData method, Expression[] args) {
            if (method.Parameters.Length != args.Length) {
                return false;
            }
            var promotedArgs = new Expression[args.Length];
            for (var i = 0; i < args.Length; i++) {
                var pi = method.Parameters[i];
                if (pi.IsOut) {
                    return false;
                }
                var promoted = PromoteExpression(args[i], pi.ParameterType, false);
                if (promoted == null) {
                    return false;
                }
                promotedArgs[i] = promoted;
            }
            method.Args = promotedArgs;
            return true;
        }

        private static void AddInterface(List<Type> types, Type type) {
            if (types.Contains(type)) {
                return;
            }
            types.Add(type);
            foreach (var t in type.GetInterfaces()) {
                AddInterface(types, t);
            }
        }

        private bool TokenIdentifierIs(string type) {
            return _token.Type == TokenType.Identifier && string.Equals(type, _token.Text, StringComparison.OrdinalIgnoreCase);
        }

        private string GetIdentifier() {
            ValidateToken(TokenType.Identifier, Res.IdentifierExpected);
            var id = _token.Text;
            if (id.Length > 1 && id[0] == '@') {
                id = id.Substring(1);
            }
            return id;
        }

        private void ValidateToken(TokenType type, string errorMessage) {
            if (_token.Type == type) { return; }
            if (_noExceptions) {
                _cancelParse = true;
                return;
            }
            throw ParseError(errorMessage);
        }

        private void ValidateToken(TokenType type) {
            if (_token.Type == type) { return; }
            if (_noExceptions) {
                _cancelParse = true;
                return;
            }
            throw ParseError(Res.SyntaxError);
        }

        private void ValidateDigit() {
            if (char.IsDigit(_char)) { return; }
            if (_noExceptions) {
                _cancelParse = true;
                return;
            }
            throw ParseError(_textPosition, Res.DigitExpected);
        }

        private Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos) {
            return ParseError(pos, Res.IncompatibleOperands, opName, left?.Type.GetTypeName() ?? "Unknown", right?.Type.GetTypeName() ?? "Unknown");
        }

        private Exception ParseError(string format, params object[] args) {
            return ParseError(_token.Position, format, args);
        }

        static Exception ParseError(int pos, string format, params object[] args) {
            return new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);
        }

        private void SetTextPosition(int position) {
            _textPosition = position;
            if (_textPosition < _textLength) {
                _char = _text[_textPosition];
            } else {
                _char = '\0';
            }
        }

        private void NextChar() {
            if (_textPosition < _textLength) {
                _textPosition++;
            }
            SetTextPosition(_textPosition);
        }

        private void ProcessParameters(ParameterExpression[] parameters) {
            foreach (var parameter in parameters) {
                if (!parameter.Name.IsNullOrEmpty()) {
                    AddSymbol(parameter.Name, parameter);
                }
            }
            if (parameters.Length == 1 && parameters[0].Name.IsNullOrEmpty()) {
                _it = parameters[0];
            }
        }

        private void ProcessValues(object[] values) {
            for (var i = 0; i < values.Length; i++) {
                var value = values[i];
                if (i == values.Length - 1 && value is IDictionary<string, object>) {
                    _externals = (IDictionary<string, object>) value;
                } else {
                    AddSymbol("@" + i.ToString(), value);
                }
            }
        }

        private void AddSymbol(string name, object value) {
            if (_symbols.ContainsKey(name)) {
                if (_noExceptions) {
                    _cancelParse = true;
                    return;
                }
                throw ParseError(Res.DuplicateIdentifier, name);
            }
            _symbols.Add(name, value);
        }

        private static readonly Expression TrueLiteral = Expression.Constant(true);
        private static readonly Expression FalseLiteral = Expression.Constant(false);
        private static readonly Expression NullLiteral = Expression.Constant(null);

        private static readonly string KeywordIt = "it";
        private static readonly string KeywordIif = "iif";
        private static readonly string KeywordNew = "new";

        private static readonly HashSet<Type> PredefinedTypes = new HashSet<Type>(
            new[] {typeof(object), typeof(bool), typeof(char), typeof(string), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
                typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(Math), typeof(Convert), typeof(Enum)});


        private static readonly Dictionary<string, object> Keywords = CreateKeywords();

        private static Dictionary<string, object> CreateKeywords() {
            var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                {"true", TrueLiteral},
                {"false", FalseLiteral},
                {"null", NullLiteral},
                {KeywordIt, KeywordIt},
                {KeywordIif, KeywordIif},
                {KeywordNew, KeywordNew}
            };
            foreach (var type in PredefinedTypes) {
                d.Add(type.Name, type);
            }
            return d;
        }

        public class ExpressionParserResult {
            public Expression Expression { get; set; }
            public List<ParameterExpression> Symbols { get; set; }
        }

        [DebuggerDisplay("{Position}: '{Text}' ({Type})")]
        internal struct Token : IEquatable<Token> {
            public TokenType Type;
            public string Text;
            public int Position;

            public bool Equals(Token other) {
                return Type == other.Type && Text == other.Text && Position == other.Position;
            }

            public override bool Equals(object obj) {
                return obj is Token other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = (int)Type;
                    hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ Position;
                    return hashCode;
                }
            }
        }

        internal enum TokenType {
            Unknown,
            End,
            Identifier,
            StringLiteral,
            IntegerLiteral,
            RealLiteral,
            Exclamation,
            Percent,
            Ampersand,
            OpenParenthesis,
            CloseParenthesis,
            Asterisk,
            Plus,
            Comma,
            Minus,
            Dot,
            Slash,
            Colon,
            LessThan,
            Equal,
            GreaterThan,
            Question,
            OpenBracket,
            CloseBracket,
            Bar,
            ExclamationEqual,
            DoubleAmpersand,
            LessThanEqual,
            LessGreater,
            DoubleEqual,
            GreaterThanEqual,
            DoubleBar,
            NullPropagation,
            NullCoalesce,
        }

        internal class ClassFactory {
            public static readonly ClassFactory Instance = new ClassFactory();

            static ClassFactory() {
            } // Trigger lazy initialization of static fields

            readonly ModuleBuilder _module;
            readonly Dictionary<Signature, Type> _classes;
            int _classCount;
            readonly ReaderWriterLock _rwLock;

            private ClassFactory() {
                var name = new AssemblyName("DynamicClasses");
                var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                try {
                    _module = assembly.DefineDynamicModule("Module");
                } finally {
                }
                _classes = new Dictionary<Signature, Type>();
                _rwLock = new ReaderWriterLock();
            }

            public Type GetDynamicClass(IEnumerable<DynamicProperty> properties) {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try {
                    var signature = new Signature(properties);
                    Type type;
                    if (!_classes.TryGetValue(signature, out type)) {
                        type = CreateDynamicClass(signature.properties);
                        _classes.Add(signature, type);
                    }
                    return type;
                } finally {
                    _rwLock.ReleaseReaderLock();
                }
            }

            Type CreateDynamicClass(DynamicProperty[] properties) {
                var cookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                try {
                    var typeName = "DynamicClass" + (_classCount + 1);
                    try {
                        var tb = this._module.DefineType(typeName, TypeAttributes.Class |
                                                                   TypeAttributes.Public, typeof(DynamicClass));
                        var fields = GenerateProperties(tb, properties);
                        GenerateEquals(tb, fields);
                        GenerateGetHashCode(tb, fields);
                        var result = tb.CreateType();
                        _classCount++;
                        return result;
                    } finally {
                    }
                } finally {
                    _rwLock.DowngradeFromWriterLock(ref cookie);
                }
            }

            FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties) {
                FieldInfo[] fields = new FieldBuilder[properties.Length];
                for (var i = 0; i < properties.Length; i++) {
                    var dp = properties[i];
                    var fb = tb.DefineField("_" + dp.Name, dp.Type, FieldAttributes.Private);
                    var pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);
                    var mbGet = tb.DefineMethod("get_" + dp.Name,
                                                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                                dp.Type, Type.EmptyTypes);
                    var genGet = mbGet.GetILGenerator();
                    genGet.Emit(OpCodes.Ldarg_0);
                    genGet.Emit(OpCodes.Ldfld, fb);
                    genGet.Emit(OpCodes.Ret);
                    var mbSet = tb.DefineMethod("set_" + dp.Name,
                                                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                                null, new[] { dp.Type });
                    var genSet = mbSet.GetILGenerator();
                    genSet.Emit(OpCodes.Ldarg_0);
                    genSet.Emit(OpCodes.Ldarg_1);
                    genSet.Emit(OpCodes.Stfld, fb);
                    genSet.Emit(OpCodes.Ret);
                    pb.SetGetMethod(mbGet);
                    pb.SetSetMethod(mbSet);
                    fields[i] = fb;
                }
                return fields;
            }

            void GenerateEquals(TypeBuilder tb, FieldInfo[] fields) {
                var mb = tb.DefineMethod("Equals",
                                         MethodAttributes.Public | MethodAttributes.ReuseSlot |
                                         MethodAttributes.Virtual | MethodAttributes.HideBySig,
                                         typeof(bool), new[] { typeof(object) });
                var gen = mb.GetILGenerator();
                var other = gen.DeclareLocal(tb);
                var next = gen.DefineLabel();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Isinst, tb);
                gen.Emit(OpCodes.Stloc, other);
                gen.Emit(OpCodes.Ldloc, other);
                gen.Emit(OpCodes.Brtrue_S, next);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(next);
                foreach (var field in fields) {
                    var ft = field.FieldType;
                    var ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                    next = gen.DefineLabel();
                    gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, field);
                    gen.Emit(OpCodes.Ldloc, other);
                    gen.Emit(OpCodes.Ldfld, field);
                    gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("Equals", new[] { ft, ft }), null);
                    gen.Emit(OpCodes.Brtrue_S, next);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Ret);
                    gen.MarkLabel(next);
                }
                gen.Emit(OpCodes.Ldc_I4_1);
                gen.Emit(OpCodes.Ret);
            }

            void GenerateGetHashCode(TypeBuilder tb, FieldInfo[] fields) {
                var mb = tb.DefineMethod("GetHashCode",
                                         MethodAttributes.Public | MethodAttributes.ReuseSlot |
                                         MethodAttributes.Virtual | MethodAttributes.HideBySig,
                                         typeof(int), Type.EmptyTypes);
                var gen = mb.GetILGenerator();
                gen.Emit(OpCodes.Ldc_I4_0);
                foreach (var field in fields) {
                    var ft = field.FieldType;
                    var ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                    gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, field);
                    gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("GetHashCode", new[] { ft }), null);
                    gen.Emit(OpCodes.Xor);
                }
                gen.Emit(OpCodes.Ret);
            }
        }

        internal class DynamicOrdering {
            public Expression Selector;
            public bool Ascending;
        }

        internal class Signature : IEquatable<Signature> {
            public DynamicProperty[] properties;
            public int hashCode;

            public Signature(IEnumerable<DynamicProperty> properties) {
                this.properties = properties.ToArray();
                hashCode = 0;
                foreach (var p in properties) {
                    hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
                }
            }

            public override int GetHashCode() {
                return hashCode;
            }

            public override bool Equals(object obj) {
                return obj is Signature ? Equals((Signature) obj) : false;
            }

            public bool Equals(Signature other) {
                if (properties.Length != other.properties.Length) {
                    return false;
                }

                for (var i = 0; i < properties.Length; i++) {
                    if (properties[i].Name != other.properties[i].Name ||
                        properties[i].Type != other.properties[i].Type) {
                        return false;
                    }
                }
                return true;
            }
        }


        internal interface ILogicalSignatures {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        internal interface IArithmeticSignatures {
            void F(int x, int y);
            void F(uint x, uint y);
            void F(long x, long y);
            void F(ulong x, ulong y);
            void F(float x, float y);
            void F(double x, double y);
            void F(decimal x, decimal y);
            void F(int? x, int? y);
            void F(uint? x, uint? y);
            void F(long? x, long? y);
            void F(ulong? x, ulong? y);
            void F(float? x, float? y);
            void F(double? x, double? y);
            void F(decimal? x, decimal? y);
        }

        internal interface IRelationalSignatures : IArithmeticSignatures {
            void F(string x, string y);
            void F(char x, char y);
            void F(DateTime x, DateTime y);
            void F(TimeSpan x, TimeSpan y);
            void F(char? x, char? y);
            void F(DateTime? x, DateTime? y);
            void F(TimeSpan? x, TimeSpan? y);
        }

        internal interface IEqualitySignatures : IRelationalSignatures {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        internal interface IAddSignatures : IArithmeticSignatures {
            void F(DateTime x, TimeSpan y);
            void F(TimeSpan x, TimeSpan y);
            void F(DateTime? x, TimeSpan? y);
            void F(TimeSpan? x, TimeSpan? y);
        }

        internal interface ISubtractSignatures : IAddSignatures {
            void F(DateTime x, DateTime y);
            void F(DateTime? x, DateTime? y);
        }

        internal interface INegationSignatures {
            void F(int x);
            void F(long x);
            void F(float x);
            void F(double x);
            void F(decimal x);
            void F(int? x);
            void F(long? x);
            void F(float? x);
            void F(double? x);
            void F(decimal? x);
        }

        internal interface INotSignatures {
            void F(bool x);
            void F(bool? x);
        }

        internal interface IEnumerableSignatures {
            void Where(bool predicate);
            void Any();
            void Any(bool predicate);
            void All(bool predicate);
            void Count();
            void Count(bool predicate);
            void Min(object selector);
            void Max(object selector);
            void Sum(int selector);
            void Sum(int? selector);
            void Sum(long selector);
            void Sum(long? selector);
            void Sum(float selector);
            void Sum(float? selector);
            void Sum(double selector);
            void Sum(double? selector);
            void Sum(decimal selector);
            void Sum(decimal? selector);
            void Average(int selector);
            void Average(int? selector);
            void Average(long selector);
            void Average(long? selector);
            void Average(float selector);
            void Average(float? selector);
            void Average(double selector);
            void Average(double? selector);
            void Average(decimal selector);
            void Average(decimal? selector);
            void Contains(int selector);
            void Contains(int? selector);
            void Contains(long selector);
            void Contains(long? selector);
            void Contains(float selector);
            void Contains(float? selector);
            void Contains(double selector);
            void Contains(double? selector);
            void Contains(decimal selector);
            void Contains(decimal? selector);
            void Contains(string selector);
            void First();
            void First(bool predicate);
            void FirstOrDefault();
            void FirstOrDefault(bool predicate);
            void Single();
            void Single(bool predicate);
            void SingleOrDefault();
            void SingleOrDefault(bool predicate);
            void Skip(int count);
            void Take(int count);
        }

        private static class Res {
            public const string DuplicateIdentifier = "The identifier '{0}' was defined more than once";
            public const string ExpressionTypeMismatch = "Expression of type '{0}' expected";
            public const string ExpressionExpected = "Expression expected";
            public const string InvalidCharacterLiteral = "Character literal must contain exactly one character";
            public const string InvalidIntegerLiteral = "Invalid integer literal '{0}'";
            public const string InvalidRealLiteral = "Invalid real literal '{0}'";
            public const string UnknownIdentifier = "Unknown identifier '{0}'";
            public const string NoItInScope = "No 'it' is in scope";
            public const string IifRequiresThreeArgs = "The 'iif' function requires three arguments";
            public const string FirstExprMustBeBool = "The first expression must be of type 'Boolean'";
            public const string BothTypesConvertToOther = "Both of the types '{0}' and '{1}' convert to the other";
            public const string NeitherTypeConvertsToOther = "Neither of the types '{0}' and '{1}' converts to the other";
            public const string MissingAsClause = "Expression is missing an 'as' clause";
            public const string ArgsIncompatibleWithLambda = "Argument list incompatible with lambda expression";
            public const string TypeHasNoNullableForm = "Type '{0}' has no nullable form";
            public const string NoMatchingConstructor = "No matching constructor in type '{0}'";
            public const string AmbiguousConstructorInvocation = "Ambiguous invocation of '{0}' constructor";
            public const string CannotConvertValue = "A value of type '{0}' cannot be converted to type '{1}'";
            public const string NoApplicableMethod = "No applicable method '{0}' exists in type '{1}'";
            public const string MethodsAreInaccessible = "Methods on type '{0}' are not accessible";
            public const string MethodIsVoid = "Method '{0}' in type '{1}' does not return a value";
            public const string AmbiguousMethodInvocation = "Ambiguous invocation of method '{0}' in type '{1}'";
            public const string UnknownPropertyOrField = "No property or field '{0}' exists in type '{1}'";
            public const string NoApplicableAggregate = "No applicable aggregate method '{0}' exists";
            public const string CannotIndexMultiDimArray = "Indexing of multi-dimensional arrays is not supported";
            public const string InvalidIndex = "Array index must be an integer expression";
            public const string NoApplicableIndexer = "No applicable indexer exists in type '{0}'";
            public const string AmbiguousIndexerInvocation = "Ambiguous invocation of indexer in type '{0}'";
            public const string IncompatibleOperand = "Operator '{0}' incompatible with operand type '{1}'";
            public const string IncompatibleOperands = "Operator '{0}' incompatible with operand types '{1}' and '{2}'";
            public const string UnterminatedStringLiteral = "Unterminated string literal";
            public const string InvalidCharacter = "Syntax error '{0}'";
            public const string DigitExpected = "Digit expected";
            public const string SyntaxError = "Syntax error";
            public const string TokenExpected = "{0} expected";
            public const string ColonExpected = "':' expected";
            public const string OpenParenExpected = "'(' expected";
            public const string CloseParenOrOperatorExpected = "')' or operator expected";
            public const string CloseParenOrCommaExpected = "')' or ',' expected";
            public const string DotOrOpenParenExpected = "'.' or '(' expected";
            public const string OpenBracketExpected = "'[' expected";
            public const string CloseBracketOrCommaExpected = "']' or ',' expected";
            public const string IdentifierExpected = "Identifier expected";
        }
    }
}