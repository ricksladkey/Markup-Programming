using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The PathExpression class compiles paths into expression objects that
    /// can be evaluated to produces values corresponding to the paths.
    /// </summary>
    public class PathExpression
    {
        private enum Value { UnsetValue = 0 };
        private abstract class PathNode
        {
            public bool IsSet { get; set; }
            public PathNode Context { get; set; }
            public string Name { get; set; }
            public object Evaluate(Engine engine, object value)
            {
                engine.Trace(TraceFlags.Path, "Path: {0} {1} {2}", this.GetType().Name, IsSet, Name);
                if (IsSet && value.Equals(Value.UnsetValue)) engine.Throw("value not supplied");
                var result = OnEvaluate(engine, value);
                engine.Trace(TraceFlags.Path, "Path: {0} = {1}", this.GetType().Name, result);
                return result;
            }
            protected abstract object OnEvaluate(Engine engine, object value);

            /// <summary>
            /// Convert a path node back into a stream of tokens.
            /// This infrastructure is used for unit testing.
            /// </summary>
            /// <param name="tokens">The queue of tokens to populate</param>
            [Conditional("DEBUG")]
            public void Tokenize(TokenQueue tokens) { this.tokens = tokens; AddTokens(); }
            private TokenQueue tokens;
            [Conditional("DEBUG")]
            protected virtual void AddTokens() { }
            [Conditional("DEBUG")]
            protected void Add(params object[] items) { foreach (var item in items) this.tokens.Enqueue(item.ToString()); }
            [Conditional("DEBUG")]
            protected void Add(params PathNode[] nodes)
            {
                if (nodes.Length == 0) return;
                nodes.First().Tokenize(tokens);
                foreach (var node in nodes.Skip(1)) { Add(","); node.Tokenize(tokens); }
            }
        }

        private class BlockNode : PathNode
        {
            public IList<PathNode> Nodes { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                foreach (var node in Nodes) node.Evaluate(engine, value);
                return null;
            }
        }

        private class ValueNode : PathNode
        {
            public object Value { get; set; }
            protected override object OnEvaluate(Engine engine, object value) { return Value; }
            protected override void AddTokens() { Add(Value); }
        }

        private class PairNode : PathNode
        {
            public PathNode Key { get; set; }
            public PathNode Value { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                return new DictionaryEntry(Key.Evaluate(engine, value), Value.Evaluate(engine, value));
            }
        }

        private class PropertyInitializerNode : PathNode
        {
            public PathNode Value { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                var context = Context.Evaluate(engine, value);
                PathHelper.SetProperty(engine, context, Name, Value.Evaluate(engine, value));
                return context;
            }
        }

        private class CollectionInitializerNode : PathNode
        {
            public PathNode Collection { get; set; }
            public IList<PathNode> Items { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                var collection = Collection.Evaluate(engine, value) as IList;
                foreach (var item in Items) collection.Add(item.Evaluate(engine, value));
                return Context == Collection ? collection : Context.Evaluate(engine, value);
            }
        }

        private class DictionaryInitializerNode : PathNode
        {
            public PathNode Dictionary { get; set; }
            public IList<PathNode> Items { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                var dictionary = Dictionary.Evaluate(engine, value) as IDictionary;
                foreach (var item in Items)
                {
                    var entry = (DictionaryEntry)item.Evaluate(engine, value);
                    dictionary.Add(entry.Key, entry.Value);
                }
                return Context == Dictionary ? dictionary : Context.Evaluate(engine, value);
            }
        }

        private class SetNode : PathNode
        {
            public PathNode LValue { get; set; }
            public PathNode RValue { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                return LValue.Evaluate(engine, RValue.Evaluate(engine, value));
            }
        }

        private class TypeNode : PathNode
        {
            public IList<TypeNode> TypeArguments { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                var type = engine.LookupType(Name);
                if (TypeArguments == null || TypeArguments.Count == 0) return type;
                var typeArgs = TypeArguments.Select(arg => arg.Evaluate(engine, value)).Cast<Type>().ToArray();
                return type.MakeGenericType(typeArgs);
            }
            protected override void AddTokens()
            {
                if (TypeArguments == null || TypeArguments.Count == 0) { Add("[", Name, "]"); return; }
                Add("[", Name, "<"); Add(TypeArguments.ToArray()); Add(">", "]");
            }
        }

        private class OpNode : PathNode
        {
            public OpNode() { Operands = new List<PathNode>(); }
            public Op Op { get; set; }
            public IList<PathNode> Operands { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                return engine.Evaluate(Op, Operands.Select(operand => operand.Evaluate(engine, value)).ToArray());
            }
            protected override void AddTokens() { Add(Op, "("); Add(Operands.ToArray()); Add(")"); }
        }

        private class ContextNode : PathNode
        {
            protected override object OnEvaluate(Engine engine, object value) { return engine.Context; }
        }

        private class VariableNode : PathNode
        {
            protected override object OnEvaluate(Engine engine, object value)
            {
                if (!IsSet) return engine.LookupVariable(Name);
                return engine.DefineVariableInParentScope(Name, value);
            }
        }

        private class PropertyNode : PathNode
        {
            protected override object OnEvaluate(Engine engine, object value)
            {
                if (!IsSet) return PathHelper.GetProperty(engine, Context.Evaluate(engine, value), Name);
                return PathHelper.SetProperty(engine, Context.Evaluate(engine, value), Name, value);
            }
        }

        private class ItemNode : PathNode
        {
            public PathNode Index { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                if (!IsSet) return PathHelper.GetItem(engine, Context.Evaluate(engine, value), Index.Evaluate(engine, value));
                return PathHelper.SetItem(engine, Context.Evaluate(engine, value), Index.Evaluate(engine, value), value);
            }
        }

        private abstract class CallNode : PathNode
        {
            public IList<PathNode> Arguments { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                return Call(engine, GetArguments(engine, null));
            }
            protected IEnumerable<object> GetArguments(Engine engine, IEnumerable<object> args)
            {
                if (Arguments != null) return Arguments.Select(argument => argument.Evaluate(engine, Value.UnsetValue)).ToArray();
                if (args == null) engine.Throw("missing arguments");
                return args;
            }
            public abstract object Call(Engine engine, IEnumerable<object> args);
        }

        private class MethodNode : CallNode
        {
            public override object Call(Engine engine, IEnumerable<object> args)
            {
                var context = Context.Evaluate(engine, Value.UnsetValue);
                return CallHelper.CallMethod(Name, false, context.GetType(), context, GetArguments(engine, args), null, engine);
            }
        }

        private class StaticMethodNode : CallNode
        {
            public TypeNode Type { get; set; }
            public override object Call(Engine engine, IEnumerable<object> args)
            {
                var type = Type.Evaluate(engine, null) as Type;
                return CallHelper.CallMethod(Name, true, type, null, GetArguments(engine, args), null, engine);
            }
        }

        private class FunctionNode : CallNode
        {
            public override object Call(Engine engine, IEnumerable<object> args)
            {
                return engine.CallFunction(Name, GetArguments(engine, args));
            }
        }

        private class TokenQueue : IEnumerable<string>
        {
            private List<string> list = new List<string>();
            private int current = 0;
            public void Enqueue(string item) { list.Add(item); }
            public string Dequeue() { return list[current++]; }
            public void Undequeue() { --current; }
            public string Peek() { return current < list.Count ? list[current] : null; }
            public int Count { get { return list.Count - current; } }
            public IEnumerator<string> GetEnumerator() { return list.Skip(current).GetEnumerator(); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        private Engine engine;
        private TokenQueue tokens;
        private PathNode root;

        public ExpressionType ExpressionType { get; private set; }
        public bool IsSet { get { return (ExpressionType & ExpressionType.Set) == ExpressionType.Set; } }
        public bool IsCall { get { return (ExpressionType & ExpressionType.Call) == ExpressionType.Call; } }
        public bool IsBlock { get { return (ExpressionType & ExpressionType.Block) == ExpressionType.Block; } }
        public string Path { get; private set; }

        public object Evaluate(Engine engine) { return Evaluate(engine, Value.UnsetValue); }
        public object Evaluate(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: {0}: {0}", Path, value.Equals(Value.UnsetValue) ? "Get" : "Set");
            return root.Evaluate(engine, value);
        }
        public object Call(Engine engine, IEnumerable<object> args)
        {
            engine.Trace(TraceFlags.Path, "Path: Call: {0}", Path);
            var call = root as CallNode;
            if (call == null) engine.Throw("not a call node");
            return call.Call(engine, args);
        }

        public PathExpression Compile(Engine engine, ExpressionType expressionType, string path)
        {
            if (expressionType == ExpressionType && object.ReferenceEquals(Path, path)) return this;
            this.engine = engine;
            ExpressionType = expressionType;
            Path = path;
            Tokenize();
            root = IsBlock ? ParseBlock() : Parse();
            if (tokens.Count > 0) engine.Throw("unexpected token: " + tokens.Dequeue());
            this.engine = null;
            tokens = null;
            return this;
        }

        private PathNode ParseBlock()
        {
            var nodes = new List<PathNode>();
            while (true)
            {
                nodes.Add(Parse());
                VerifyToken(";");
                if (tokens.Count == 0) break;
            }
            return new BlockNode { Nodes = nodes };
        }

        private PathNode Parse() { return Parse(false); }

        private PathNode Parse(bool noComma)
        {
            if (tokens.Count == 0) return new ValueNode { Value = null };
            var node = new ContextNode() as PathNode;
            var nodeNext = true;
            for (var token = tokens.Peek(); token != null; token = tokens.Peek())
            {
                if (OperatorMap.ContainsKey(token))
                {
                    var op = OperatorMap[token];
                    var unary = op.GetArity() == 1;
                    if (nodeNext != unary) engine.Throw("unexpected operator" + op);
                    nodeNext = true;
                    tokens.Dequeue();
                    if (unary)
                        node = new OpNode { Op = op, Operands = { Parse() } };
                    else
                        node = new OpNode { Op = op, Operands = { node, Parse() } };
                    continue;
                }
                if ("=.?,".Contains(token[0]) && nodeNext) engine.Throw("unexpected operator: " + token);
                if (token == "=")
                {
                    node.IsSet = true;
                    tokens.Dequeue();
                    var rvalue = Parse();
                    node = new SetNode { LValue = node, RValue = rvalue };
                    continue;
                }
                if (token == ".")
                {
                    nodeNext = true;
                    tokens.Dequeue();
                    continue;
                }
                if (token == "?")
                {
                    tokens.Dequeue();
                    var ifTrue = Parse();
                    VerifyToken(":");
                    var ifFalse = Parse();
                    node = new OpNode { Op = Op.Conditional, Operands = { node, ifTrue, ifFalse } };
                    continue;
                }
                if (token == "," && !noComma)
                {
                    tokens.Dequeue();
                    node = new OpNode { Op = Op.Comma, Operands = { node, Parse() } };
                    continue;
                }
                if (!nodeNext) return node;
                char c = token[0];
                if (char.IsDigit(c))
                {
                    tokens.Dequeue();
                    node = new ValueNode { Value = token.Contains('.') ? ParseDouble(token) : ParseInt(token) };
                }
                else if (c == '"')
                    node = new ValueNode { Value = tokens.Dequeue().Substring(1) };
                else if (c == '`')
                {
                    var identifier = VerifyIdentifier();
                    if (IsCurrentCall)
                    {
                        var args = tokens.Peek() == "(" ? ParseArguments() : null;
                        node = new MethodNode { Context = node, Name = identifier, Arguments = args };
                    }
                    else
                        node = new PropertyNode { IsSet = IsCurrentSet, Context = node, Name = identifier };
                }
                else if (c == '$' || c == '@')
                {
                    tokens.Dequeue();
                    if (IsCurrentCall)
                    {
                        var args = tokens.Peek() == "(" ? ParseArguments() : null;
                        node = new FunctionNode { Context = node, Name = token, Arguments = args };
                    }
                    else
                        node = new VariableNode { IsSet = IsCurrentSet, Name = token };
                }
                else if (c == '[')
                {
                    tokens.Dequeue();
                    var typeNode = ParseType();
                    VerifyToken("]");
                    if (tokens.Peek() == ".")
                    {
                        VerifyToken(".");
                        var methodName = VerifyIdentifier();
                        var args = tokens.Peek() == "(" ? ParseArguments() : null;
                        node = new StaticMethodNode { Type = typeNode, Name = methodName, Arguments = args };
                    }
                    else if (tokens.Peek() == "(")
                        node = new OpNode { Op = Op.New, Operands = new PathNode[] { typeNode }.Concat(ParseArguments()).ToList() };
                    else if (tokens.Peek() == "{")
                    {
                        tokens.Dequeue();
                        node = ParseInitializer(typeNode);
                    }
                    else
                        node = typeNode;
                }
                else if (c == '(')
                {
                    tokens.Dequeue();
                    node = Parse();
                    VerifyToken(")");
                }
                else
                    return node;
                while (tokens.Peek() == "[")
                {
                    tokens.Dequeue();
                    var index = Parse(true);
                    VerifyToken("]");
                    node = new ItemNode { IsSet = IsCurrentSet, Context = node, Index = index };
                }
                nodeNext = false;
            }
            return node;
        }

        private PathNode ParseInitializer(TypeNode typeNode)
        {
            var node = new OpNode { Op = Op.New, Operands = { typeNode } } as PathNode;
            while (true)
            {
                if (tokens.Peek() == "{") return ParseDictionaryInitializer(node, node);
                tokens.Dequeue();
                var isCollection = tokens.Peek() != "=";
                tokens.Undequeue();
                if (isCollection) return ParseCollectionInitializer(node, node);
                var property = VerifyIdentifier();
                VerifyToken("=");
                if (tokens.Peek() == "{")
                {
                    tokens.Dequeue();
                    var propertyNode = new PropertyNode { Context = node, Name = property };
                    if (tokens.Peek() == "{")
                        node = ParseDictionaryInitializer(node, propertyNode);
                    else
                        node = ParseCollectionInitializer(node, propertyNode);
                }
                else
                    node = new PropertyInitializerNode { Context = node, Name = property, Value = Parse(true) };
                var token = tokens.Dequeue();
                if (token == "}") break;
                if (token != ",") engine.Throw("unexpected token: " + token);
            }
            return node;
        }

        private PathNode ParseCollectionInitializer(PathNode context, PathNode collection)
        {
            return new CollectionInitializerNode { Context = context, Collection = collection, Items = ParseList("}") };
        }

        private PathNode ParseDictionaryInitializer(PathNode context, PathNode dictionary)
        {
            var entries = new List<PathNode>();
            while (true)
            {
                VerifyToken("{");
                var key = Parse(true);
                VerifyToken(",");
                var value = Parse(true);
                VerifyToken("}");
                entries.Add(new PairNode { Key = key, Value = value });
                var token = tokens.Dequeue();
                if (token == "}") break;
                if (token != ",") engine.Throw("unexpected token: " + token);
            }
            return new DictionaryInitializerNode { Context = context, Dictionary = dictionary, Items = entries };
        }

        private TypeNode ParseType()
        {
            if (tokens.Peek() == "," || tokens.Peek() == ">") return null;
            var typeName = VerifyIdentifier();
            while (tokens.Peek() == ".")
            {
                tokens.Dequeue();
                typeName += "." + VerifyIdentifier();
            }
            var typeArgs = null as List<TypeNode>;
            if (tokens.Peek() == "<")
            {
                tokens.Dequeue();
                typeArgs = new List<TypeNode>();
                while (true)
                {
                    typeArgs.Add(ParseType());
                    var token = tokens.Dequeue();
                    if (token == ">") break;
                    if (token != ",") engine.Throw("unexpected token: " + token);
                }
                typeName += "`" + typeArgs.Count;
                if (typeArgs.All(typeArg => typeArg == null)) typeArgs = null;
                else if (!typeArgs.All(typeArg => typeArg != null)) engine.Throw("generic type partially specified");
            }
            return new TypeNode { Name = typeName, TypeArguments = typeArgs };
        }

        private object ParseDouble(string token)
        {
            double d;
            if (!double.TryParse(token, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                engine.Throw("bad double: " + token);
            return d;
        }

        private object ParseInt(string token)
        {
            int i;
            if (!int.TryParse(token, out i)) engine.Throw("bad int: " + token);
            return i;
        }

        private bool IsCurrentSet { get { return IsSet && tokens != null && tokens.Count == 0; } }
        private bool IsCurrentCall { get { return IsCall && tokens != null && tokens.Count == 0 || tokens.Peek() == "("; } }

        private string VerifyToken(string token)
        {
            if (tokens.Count == 0 || tokens.Peek() != token) engine.Throw("missing token: " + token);
            return tokens.Dequeue();
        }

        private string VerifyIdentifier()
        {
            if (tokens.Count == 0 || tokens.Peek()[0] != '`') engine.Throw("expected identifier");
            return tokens.Dequeue().Substring(1);
        }

        private IList<PathNode> ParseArguments()
        {
            VerifyToken("(");
            return ParseList(")");
        }

        private IList<PathNode> ParseList(string expectedToken)
        {
            var nodes = new List<PathNode>();
            if (tokens.Count > 0 && tokens.Peek() == expectedToken)
            {
                tokens.Dequeue();
                return nodes;
            }
            while (tokens.Count > 0)
            {
                nodes.Add(Parse(true));
                if (tokens.Count == 0) engine.Throw("missing token: " + expectedToken);
                var token = tokens.Dequeue();
                if (token == expectedToken) return nodes;
                if (token != ",") engine.Throw("unexpected token: " + token);
            }
            return nodes;
        }

        private void Tokenize()
        {
            tokens = new TokenQueue();
            for (int i = 0; i < Path.Length; )
            {
                char c = Path[i];
                if (char.IsWhiteSpace(c)) ++i;
                else if (i < Path.Length - 1 && Path.Substring(i, 2) == "/*")
                    i = EatComment(i);
                else if (i < Path.Length - 1 && OperatorMap.ContainsKey(Path.Substring(i, 2)))
                {
                    tokens.Enqueue(Path.Substring(i, 2));
                    i += 2;
                }
                else if (OperatorMap.ContainsKey(c.ToString()) || "=.[](){},?:;".Contains(c))
                {
                    tokens.Enqueue(c.ToString());
                    ++i;
                }
                else if (IsQuote(c))
                {
                    var start = ++i;
                    for (++i; i < Path.Length && Path[i] != c; ++i) continue;
                    if (Path[i] == Path.Length) engine.Throw("missing closing quote: " + Path);
                    tokens.Enqueue('"' + Path.Substring(start, i++ - start));
                }
                else if (char.IsDigit(c))
                {
                    var start = i;
                    for (++i; i < Path.Length && (char.IsDigit(Path[i]) || Path[i] == '.'); i++) continue;
                    tokens.Enqueue(Path.Substring(start, i - start));
                }
                else if (c == '$' || c == '@' || IsInitialIdChar(c))
                {
                    var start = i;
                    var prefix = "";
                    if (c == '$' || c == '@')
                    {
                        ++i;
                        if (i == Path.Length || !IsInitialIdChar(Path[i]))
                        {
                            if (c == '$') engine.Throw("missing identifier");
                            if (c == '@') { tokens.Enqueue("@"); continue; }
                        }
                    }
                    else
                        prefix = "`";
                    for (++i; i < Path.Length && IsIdChar(Path[i]); ++i) continue;
                    tokens.Enqueue(prefix + Path.Substring(start, i - start));
                }
                else
                    engine.Throw("invalid token: " + Path.Substring(i));
            }

        }

        private int EatComment(int i)
        {
            for (i += 2; i < Path.Length - 1 && Path.Substring(i, 2) != "*/"; i++)
                if (Path.Substring(i, 2) == "/*") i = EatComment(i) - 1;
            return i + 2;
        }

        private static string IdChars { get { return "_"; } }
        private static bool IsInitialIdChar(char c) { return char.IsLetter(c) || IdChars.Contains(c); }
        private static bool IsIdChar(char c) { return char.IsLetterOrDigit(c) || IdChars.Contains(c); }
        public static bool IsValidVariable(string variable)
        {
            if (variable.Length < 2 || variable[0] != '$') return false;
            return IsInitialIdChar(variable[1]) && variable.Skip(2).All(c => IsIdChar(c));
        }
        private static bool IsQuote(char c) { return "'\"".Contains(c); }

        public static IDictionary<string, Op> OperatorMap { get { return operatorMap; } }
        private static Dictionary<string, Op> operatorMap = new Dictionary<string, Op>
        {
            { "+", Op.Plus },
            { "-", Op.Minus },
            { "*", Op.Times },
            { "/", Op.Divide },
            { "&&", Op.AndAnd },
            { "||", Op.OrOr },
            { "&", Op.And },
            { "|", Op.Or },
            { "!", Op.Not },
            { "==", Op.Equals },
            { "!=", Op.NotEquals },
            { "<", Op.LessThan },
            { "<=", Op.LessThanOrEqual },
            { ">", Op.GreaterThan },
            { ">=", Op.GreaterThanOrEqual },
        };

#if DEBUG
        public List<string> DebugCompile(Engine engine, ExpressionType expressionType, string path)
        {
            Compile(engine, expressionType, path);
            var newTokens = new TokenQueue();
            root.Tokenize(newTokens);
            return new List<string>(newTokens);
        }
#endif

    }
}
