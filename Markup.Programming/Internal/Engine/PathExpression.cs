using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            public bool IsGet { get; set; }
            public PathNode Context { get; set; }
            public void ThrowIfUnset(object value) { if (IsUnset(value)) ThrowHelper.Throw("unset"); }
            public object Evaluate(Engine engine, object value)
            {
                engine.Trace(TraceFlags.Path, "Path: evaluate {0}", this.GetType().Name);
                var result = OnEvaluate(engine, value);
                engine.Trace(TraceFlags.Path, "Path: {0} = {1}", this.GetType().Name, result);
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
                if (IsGet) return engine.LookupParameter(ParameterName);
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
                if (IsGet) return PathHelper.GetProperty(Context.Evaluate(engine, value), PropertyName);
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
                if (IsGet) return PathHelper.GetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value));
                ThrowIfUnset(value);
                PathHelper.SetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value), value);
                return value;
            }
        }

        private class MethodNode : PathNode
        {
            public string MethodName { get; set; }
            public IList<PathNode> Arguments { get; set; }
            protected object[] EvaluateArguments(Engine engine, object value)
            {
                return Arguments.Select(argument => argument.Evaluate(engine, value)).ToArray();
            }
            protected override object OnEvaluate(Engine engine, object value)
            {
                var context = Context.Evaluate(engine, value);
                var args = EvaluateArguments(engine, value);
                var methodInfo = context.GetType().GetMethod(MethodName);
                return MethodHelper.CallMethod(MethodName, methodInfo, context, args, engine);
            }
        }

        private class StaticMethodNode : MethodNode
        {
            public Type Type { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                var args = EvaluateArguments(engine, value);
                var methodInfo = Type.GetMethod(MethodName);
                return MethodHelper.CallMethod(MethodName, methodInfo, null, args, engine);
            }
        }

        private class TokenQueue
        {
            private List<string> list = new List<string>();
            private int current = 0;
            public void Enqueue(string item) { list.Add(item); }
            public string Dequeue() { return list[current++]; }
            public string Peek() { return current < list.Count ? list[current] : null; }
            public int Count { get { return list.Count - current; } }
        }

        private enum Value { UnsetValue = 0 };
        private static bool IsUnset(object value) { return value.Equals(Value.UnsetValue); }

        private Engine engine;
        private PathNode root;
        private TokenQueue tokens;

        public PathExpression(bool isGet, bool isProperty, string path, Engine engine)
        {
            IsGet = isGet;
            IsProperty = isProperty;
            Path = path;
            this.engine = engine;
            tokens = Tokenize(Path);
            root = Parse();
            if (tokens.Count != 0) ThrowHelper.Throw("unexpected token: " + tokens.Dequeue());
            tokens = null;
        }

        public bool IsGet { get; private set; }
        public bool IsSet { get { return !IsGet; } }
        public bool IsProperty { get; private set; }
        public bool IsMethod { get { return !IsProperty; } }
        public string Path { get; private set; }

        public object Evaluate(Engine engine) { return Evaluate(engine, Value.UnsetValue); }
        public object Evaluate(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: Evaluating: {0}", Path);
            return root.Evaluate(engine, value);
        }

        private PathNode Parse()
        {
            var node = null as PathNode;
            if (tokens.Count == 0) return new ValueNode { Value = null };
            node = new ContextNode();
            var nodeNext = true;
            for (var token = tokens.Peek(); token != null; token = tokens.Peek())
            {
                var c = token[0];
                if (c == '.')
                {
                    if (nodeNext) ThrowHelper.Throw("unexpected dot operator");
                    nodeNext = true;
                    tokens.Dequeue();
                    continue;
                }
                if (!nodeNext) return node;
                if (token == "@")
                    node = new ContextNode();
                else if (char.IsDigit(c))
                    node = new ValueNode { Value = int.Parse(tokens.Dequeue()) };
                else if (IsInitialIdentifierChar(c))
                {
                    tokens.Dequeue();
                    if (tokens.Peek() == "(")
                        node = new MethodNode { Context = node, MethodName = token, Arguments = ParseArguments() };
                    else
                        node = new PropertyNode { IsGet = IsCurrentGet, Context = node, PropertyName = token };
                }
                else if (c == '$')
                {
                    tokens.Dequeue();
                    if (tokens.Count == 0) ThrowHelper.Throw("missing parameter");
                    var parameterName = tokens.Dequeue();
                    node = new ParameterNode { IsGet = IsCurrentGet, ParameterName = parameterName };
                }
                else if (c == '[')
                {
                    tokens.Dequeue();
                    var typeName = "";
                    while (tokens.Count > 0 && tokens.Peek() != "]") typeName += tokens.Dequeue();
                    VerifyToken("]");
                    VerifyToken(".");
                    var type = engine.LookupType(typeName);
                    var methodName = tokens.Dequeue();
                    var args = tokens.Peek() == "(" ? ParseArguments() : null;
                    node = new StaticMethodNode { Type = type, MethodName = methodName, Arguments = args };
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
                    var index = Parse();
                    VerifyToken("]");
                    node = new ItemNode { IsGet = IsCurrentGet, Context = node, Index = index };
                }
                nodeNext = false;
            }
            return node;
        }

        private bool IsCurrentGet { get { return IsGet || tokens.Count > 0; } }

        private void VerifyToken(string token)
        {
            if (tokens.Count == 0 || tokens.Dequeue() != token) ThrowHelper.Throw("missing token: " + token);
        }

        private IList<PathNode> ParseArguments()
        {
            var nodes = new List<PathNode>();
            var expectedTokens = new List<string> { ",", ")" };
            var token = tokens.Dequeue();
            if (token != "(") ThrowHelper.Throw("missing opening parenthesis");
            if (tokens.Count > 0 && tokens.Peek() == ")") { tokens.Dequeue(); return nodes; }
            while (tokens.Count > 0)
            {
                nodes.Add(Parse());
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
                if ("$.[](),+-*/".Contains(c)) { tokens.Enqueue(c.ToString()); ++i; continue; }
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
            return char.IsLetter(c) || c == '_' || c == '@';
        }

        private static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '@';
        }
    }
}
