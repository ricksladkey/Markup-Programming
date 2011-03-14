using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The ParseHelper class parses paths into PathExpression objects that
    /// can be evaluated later.
    /// </summary>
    public static class ParseHelper
    {
        public class PathExpression
        {
            private enum Value { UnsetValue = 0 };
            public string Path { get; private set; }
            public PathNode PathNode { get; private set; }
            public PathExpression(string path, PathNode root) { Path = path; PathNode = root; }
            public static bool IsUnset(object value) { return value.Equals(Value.UnsetValue); }
            public object Evaluate(Engine engine) { return Evaluate(engine, Value.UnsetValue); }
            public object Evaluate(Engine engine, object value)
            {
                engine.Trace(TraceFlags.Path, "Path: Evaluating: {0}", Path);
                return PathNode.Evaluate(engine, value);
            }
        }

        public abstract class PathNode
        {
            public bool Get { get; set; }
            public PathNode Context { get; set; }
            public void ThrowIfUnset(object value) { if (PathExpression.IsUnset(value)) ThrowHelper.Throw("unset"); }
            public object Evaluate(Engine engine, object value)
            {
                engine.Trace(TraceFlags.Path, "Path: evaluate {0}", this);
                var result = OnEvaluate(engine, value);
                engine.Trace(TraceFlags.Path, "Path: {0} = {1}", this, result);
                return result;
            }
            protected abstract object OnEvaluate(Engine engine, object value);
        }

        public class ValueNode : PathNode
        {
            public object Value { get; set; }
            protected override object OnEvaluate(Engine engine, object value) { return Value; }
        }

        public class OpNode : PathNode
        {
            public OpNode() { Operands = new List<PathNode>(); }
            public Operator Operator { get; set; }
            public IList<PathNode> Operands { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                return engine.Evaluate(Operator, Operands.Select(operand => operand.Evaluate(engine, value)).ToArray());
            }
        }

        public class ContextNode : PathNode
        {
            protected override object OnEvaluate(Engine engine, object value) { return engine.Context; }
        }

#if DEBUG
        [DebuggerDisplay("Property: Get = {Get}, ParameterName = {ParameterName}")]
#endif
        public class ParameterNode : PathNode
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

#if DEBUG
        [DebuggerDisplay("Property: Get = {Get}, PropertyName = {PropertyName}")]
#endif
        public class PropertyNode : PathNode
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

#if DEBUG
        [DebuggerDisplay("Item: Get = {Get}")]
#endif
        public class ItemNode : PathNode
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

#if DEBUG
        [DebuggerDisplay("Method: MethodName = {MethodName}, Arguments = {Arguments.Count}")]
#endif
        public class MethodNode : PathNode
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

        public class TokenQueue
        {
            private List<string> list = new List<string>();
            private int current = 0;
            public void Enqueue(string item) { list.Add(item); }
            public string Dequeue() { return list[current++]; }
            public string Peek() { return list[current]; }
            public int Count { get { return list.Count - current; } }
        }

        public static PathExpression ParseGet(string path) { return Parse(path, true); }
        public static PathExpression ParseSet(string path) { return Parse(path, false); }

        private static PathExpression Parse(string path, bool getPath)
        {
            var tokens = Tokenize(path);
            var node = Parse(getPath, tokens);
            if (tokens.Count != 0) ThrowHelper.Throw("unexpected token: " + tokens.Dequeue());
            return new PathExpression(path, node);
        }

        public static PathNode Parse(bool getPath, TokenQueue tokens)
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
                    node = new ParameterNode { Get = getPath || tokens.Count > 0, ParameterName = parameterName };
                }
                else if (IsInitialIdentifierChar(c))
                {
                    tokens.Dequeue();
                    if (tokens.Count > 0 && tokens.Peek() == "(")
                        node = new MethodNode { Context = node, MethodName = token, Arguments = ParseArguments(tokens) };
                    else
                        node = new PropertyNode { Get = getPath || tokens.Count > 0, Context = node, PropertyName = token };
                }
                else if (c == '[')
                {
                    tokens.Dequeue();
                    var index = Parse(false, tokens);
                    if (tokens.Count == 0 || tokens.Dequeue() != "]") ThrowHelper.Throw("missing closing square bracket");
                    node = new ItemNode { Get = getPath || tokens.Count > 0, Context = node, Index = index };
                }
                else if (char.IsDigit(c))
                    node = new ValueNode { Value = int.Parse(tokens.Dequeue()) };
                else
                    return node;
            }
            return node;
        }

        private static IList<PathNode> ParseArguments(TokenQueue tokens)
        {
            var nodes = new List<PathNode>();
            var expectedTokens = new List<string> { ",", ")" };
            var token = tokens.Dequeue();
            if (token != "(") ThrowHelper.Throw("missing open parenthesis");
            if (tokens.Count > 0 && tokens.Peek() == ")") { tokens.Dequeue(); return nodes; }
            while (tokens.Count > 0)
            {
                nodes.Add(Parse(false, tokens));
                if (tokens.Count == 0) break;
                var nextToken = tokens.Dequeue();
                if (!expectedTokens.Contains(nextToken)) ThrowHelper.Throw("unexpected token: " + nextToken);
                if (nextToken == ")") return nodes;
            }
            return ThrowHelper.Throw("missing close parenthesis") as IList<PathNode>;
        }

        private static TokenQueue Tokenize(string path)
        {
            TokenQueue tokens = new TokenQueue();
            int i = 0;
            int n = path.Length;
            while (i < n)
            {
                char c = path[i];
                if (c == ' ')
                {
                    ++i;
                    continue;
                }
                switch (c)
                {
                    case '$':
                    case '.':
                    case '[':
                    case ']':
                    case '(':
                    case ')':
                    case ',':
                        tokens.Enqueue(c.ToString());
                        ++i;
                        continue;
                }
                if (!IsIdentifierChar(c)) ThrowHelper.Throw("invalid token: " + path.Substring(i));
                {
                    var start = i;
                    ++i;
                    while (i < n && IsIdentifierChar(path[i])) ++i;
                    tokens.Enqueue(path.Substring(start, i - start));
                    continue;
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
