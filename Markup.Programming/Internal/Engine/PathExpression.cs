using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The PathExpression class parses paths into expression objects that
    /// can be evaluated to produces values corresponding to the paths.
    /// </summary>
    public class PathExpression
    {
        private abstract class PathNode
        {
            public bool Get { get; set; }
            public PathNode Context { get; set; }
            public void ThrowIfUnset(object value) { if (IsUnset(value)) ThrowHelper.Throw("unset"); }
            public object Evaluate(Engine engine, object value)
            {
                engine.Trace(TraceFlags.Path, "Path: evaluate {0}", this);
                var result = OnEvaluate(engine, value);
                engine.Trace(TraceFlags.Path, "Path: {0} = {1}", this, result);
                return result;
            }
            protected abstract object OnEvaluate(Engine engine, object value);
        }

        private class ValueNode : PathNode
        {
            public object Value { get; set; }
            protected override object OnEvaluate(Engine engine, object value) { return Value; }
        }

        private class OpNode : PathNode
        {
            public OpNode() { Operands = new List<PathNode>(); }
            public Operator Operator { get; set; }
            public IList<PathNode> Operands { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                return engine.Evaluate(Operator, Operands.Select(operand => operand.Evaluate(engine, value)).ToArray());
            }
        }

        private class ContextNode : PathNode
        {
            protected override object OnEvaluate(Engine engine, object value) { return engine.Context; }
        }

        private class ParameterNode : PathNode
        {
            public string ParameterName { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                if (Get) return engine.LookupParameter(ParameterName);
                ThrowIfUnset(value);
                engine.DefineParameterInParentScope(ParameterName, value);
                return value;
            }
        }

        private class PropertyNode : PathNode
        {
            public string PropertyName { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                if (Get) return PathHelper.GetProperty(Context.Evaluate(engine, value), PropertyName);
                ThrowIfUnset(value);
                PathHelper.SetProperty(Context.Evaluate(engine, value), PropertyName, value);
                return value;
            }
        }

        private class ItemNode : PathNode
        {
            public PathNode Index { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                if (Get) return PathHelper.GetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value));
                ThrowIfUnset(value);
                PathHelper.SetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value), value);
                return value;
            }
        }

        private class MethodNode : PathNode
        {
            public string MethodName { get; set; }
            public IList<PathNode> Arguments { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                var context = Context.Evaluate(engine, value);
                var args = Arguments.Select(argument => argument.Evaluate(engine, value)).ToArray();
                var methodInfo = context.GetType().GetMethod(MethodName);
                return MethodHelper.CallMethod(MethodName, methodInfo, context, args, engine);
            }
        }

        private class TokenQueue
        {
            private List<string> list = new List<string>();
            private int current = 0;
            public void Enqueue(string item) { list.Add(item); }
            public string Dequeue() { return list[current++]; }
            public string Peek() { return list[current]; }
            public int Count { get { return list.Count - current; } }
        }

        private enum Value { UnsetValue = 0 };
        private static bool IsUnset(object value) { return value.Equals(Value.UnsetValue); }

        private PathNode root;

        public PathExpression(bool get, string path)
        {
            Get = get;
            Path = path;
            var tokens = Tokenize(Path);
            root = Parse(tokens);
            if (tokens.Count != 0) ThrowHelper.Throw("unexpected token: " + tokens.Dequeue());
        }

        public bool Get { get; private set; }
        public string Path { get; private set; }

        public object Evaluate(Engine engine) { return Evaluate(engine, Value.UnsetValue); }
        public object Evaluate(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: Evaluating: {0}", Path);
            return root.Evaluate(engine, value);
        }

        private PathNode Parse(TokenQueue tokens)
        {
            if (tokens.Count == 0) return new ValueNode { Value = null };
            var node = new ContextNode() as PathNode;
            while (tokens.Count > 0)
            {
                var token = tokens.Peek();
                var c = token[0];
                if (c == '.')
                    tokens.Dequeue();
                else if (c == '$')
                {
                    tokens.Dequeue();
                    if (tokens.Count == 0) ThrowHelper.Throw("missing parameter");
                    var parameterName = tokens.Dequeue();
                    node = new ParameterNode { Get = Get || tokens.Count > 0, ParameterName = parameterName };
                }
                else if (IsInitialIdentifierChar(c))
                {
                    tokens.Dequeue();
                    if (tokens.Count > 0 && tokens.Peek() == "(")
                        node = new MethodNode { Context = node, MethodName = token, Arguments = ParseArguments(tokens) };
                    else
                        node = new PropertyNode { Get = Get || tokens.Count > 0, Context = node, PropertyName = token };
                }
                else if (c == '[')
                {
                    tokens.Dequeue();
                    var index = Parse(tokens);
                    if (tokens.Count == 0 || tokens.Dequeue() != "]") ThrowHelper.Throw("missing closing square bracket");
                    node = new ItemNode { Get = Get || tokens.Count > 0, Context = node, Index = index };
                }
                else if (char.IsDigit(c))
                    node = new ValueNode { Value = int.Parse(tokens.Dequeue()) };
                else
                    return node;
            }
            return node;
        }

        private IList<PathNode> ParseArguments(TokenQueue tokens)
        {
            var nodes = new List<PathNode>();
            var expectedTokens = new List<string> { ",", ")" };
            var token = tokens.Dequeue();
            if (token != "(") ThrowHelper.Throw("missing opening parenthesis");
            if (tokens.Count > 0 && tokens.Peek() == ")") { tokens.Dequeue(); return nodes; }
            while (tokens.Count > 0)
            {
                nodes.Add(Parse(tokens));
                if (tokens.Count == 0) break;
                var nextToken = tokens.Dequeue();
                if (!expectedTokens.Contains(nextToken)) ThrowHelper.Throw("unexpected token: " + nextToken);
                if (nextToken == ")") return nodes;
            }
            return ThrowHelper.Throw("missing closing parenthesis") as IList<PathNode>;
        }

        private static TokenQueue Tokenize(string path)
        {
            TokenQueue tokens = new TokenQueue();
            for (int i = 0; i < path.Length; )
            {
                char c = path[i];
                if (c == ' ') { ++i; continue; }
                if ("$.[](),".Contains(c)) { tokens.Enqueue(c.ToString()); ++i; continue; }
                if (!IsIdentifierChar(c)) ThrowHelper.Throw("invalid token: " + path.Substring(i));
                {
                    var start = i;
                    for (++i; i < path.Length && IsIdentifierChar(path[i]); ++i) continue;
                    tokens.Enqueue(path.Substring(start, i - start));
                }
            }
            return tokens;
        }

        private static bool IsInitialIdentifierChar(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        private static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }
    }
}
