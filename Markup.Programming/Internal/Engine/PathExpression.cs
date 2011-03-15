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
        private enum Value { UnsetValue = 0 };
        private abstract class PathNode
        {
            public bool IsSet { get; set; }
            public PathNode Context { get; set; }
            public string Name { get; set; }
            public object Evaluate(Engine engine, object value)
            {
                engine.Trace(TraceFlags.Path, "Path: {0} {1} {2}", this.GetType().Name, IsSet, Name);
                if (IsSet && value.Equals(Value.UnsetValue)) ThrowHelper.Throw("value not supplied");
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
                if (!IsSet) return engine.LookupParameter(ParameterName);
                return engine.DefineParameterInParentScope(ParameterName, value);
            }
        }

        private class PropertyNode : PathNode
        {
            protected override object OnEvaluate(Engine engine, object value)
            {
                if (!IsSet) return PathHelper.GetProperty(Context.Evaluate(engine, value), Name);
                return PathHelper.SetProperty(Context.Evaluate(engine, value), Name, value);
            }
        }

        private class ItemNode : PathNode
        {
            public PathNode Index { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                if (!IsSet) return PathHelper.GetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value));
                return PathHelper.SetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value), value);
            }
        }

        private abstract class CallNode : PathNode
        {
            public IList<PathNode> Arguments { get; set; }
            protected override object OnEvaluate(Engine engine, object value)
            {
                return Call(engine, GetArguments(engine, null));
            }
            protected object[] GetArguments(Engine engine, object[] args)
            {
                if (Arguments != null) return Arguments.Select(argument => argument.Evaluate(engine, Value.UnsetValue)).ToArray();
                if (args == null) ThrowHelper.Throw("missing arguments");
                return args;
            }
            public abstract object Call(Engine engine, object[] args);
        }

        private class MethodNode : CallNode
        {
            public override object Call(Engine engine, object[] args)
            {
                var context = Context.Evaluate(engine, Value.UnsetValue);
                return MethodHelper.CallMethod(Name, false, context.GetType(), context, GetArguments(engine, args), null, engine);
            }
        }

        private class StaticMethodNode : CallNode
        {
            public Type Type { get; set; }
            public override object Call(Engine engine, object[] args)
            {
                return MethodHelper.CallMethod(Name, true, Type, null, args ?? GetArguments(engine, args), null, engine);
            }
        }

        private class FunctionNode : CallNode
        {
            public override object Call(Engine engine, object[] args)
            {
                return engine.CallFunction(Name, GetArguments(engine, args));
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

        private Engine engine;
        private TokenQueue tokens = new TokenQueue();
        private PathNode root;

        public PathExpression(Engine engine, bool isSet, bool isCall, string path)
        {
            this.engine = engine;
            IsSet = isSet;
            IsCall = isCall;
            Path = path;
            Tokenize();
            root = Parse();
            if (tokens.Count > 0) ThrowHelper.Throw("unexpected token: " + tokens.Dequeue());
            engine = null;
            tokens = null;
        }

        public bool IsSet { get; private set; }
        public bool IsCall { get; private set; }
        public string Path { get; private set; }

        public object Evaluate(Engine engine) { return Evaluate(engine, Value.UnsetValue); }
        public object Evaluate(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: {0}: {0}", Path, value.Equals(Value.UnsetValue) ? "Get" : "Set");
            return root.Evaluate(engine, value);
        }
        public object Call(Engine engine, object[] args)
        {
            engine.Trace(TraceFlags.Path, "Path: Call: {0}", Path);
            var call = root as CallNode;
            if (call == null) ThrowHelper.Throw("not a call node");
            return call.Call(engine, args);
        }

        private PathNode Parse()
        {
            if (tokens.Count == 0) return new ValueNode { Value = null };
            var node = new ContextNode() as PathNode;
            var nodeNext = true;
            for (var token = tokens.Peek(); token != null; token = tokens.Peek())
            {
                var c = token[0];
                if (".+-*/".Contains(c))
                {
                    if (nodeNext) ThrowHelper.Throw("unexpected operator");
                    nodeNext = true;
                    tokens.Dequeue();
                    if (c != '.') node = new OpNode { Operator = GetOperator(token), Operands = { node, Parse() } };
                    continue;
                }
                if (!nodeNext) return node;
                if (token == "@")
                {
                    tokens.Dequeue();
                    node = new ContextNode();
                }
                else if (char.IsDigit(c))
                    node = new ValueNode { Value = int.Parse(tokens.Dequeue()) };
                else if (c == '"')
                    node = new ValueNode { Value = tokens.Dequeue().Substring(1) };
                else if (IsIdChar(c))
                {
                    tokens.Dequeue();
                    if (IsCurrentCall)
                    {
                        var args = tokens.Peek() == "(" ? ParseArguments() : null;
                        node = new MethodNode { Context = node, Name = token, Arguments = args };
                    }
                    else
                        node = new PropertyNode { IsSet = IsCurrentSet, Context = node, Name = token };
                }
                else if (c == '$')
                {
                    tokens.Dequeue();
                    if (tokens.Count == 0) ThrowHelper.Throw("missing parameter");
                    var name = tokens.Dequeue();
                    if (IsCurrentCall)
                    {
                        var args = tokens.Peek() == "(" ? ParseArguments() : null;
                        node = new FunctionNode { Context = node, Name = name, Arguments = args };
                    }
                    else
                        node = new ParameterNode { IsSet = IsCurrentSet, ParameterName = name };
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
                    node = new StaticMethodNode { Type = type, Name = methodName, Arguments = args };
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
                    node = new ItemNode { IsSet = IsCurrentSet, Context = node, Index = index };
                }
                nodeNext = false;
            }
            return node;
        }

        private Operator GetOperator(string token)
        {
            char c = token[0];
            switch (c)
            {
                case '.': return Operator.GetProperty;
                case '+': return Operator.Plus;
                case '-': return Operator.Minus;
                case '*': return Operator.Times;
                case '/': return Operator.Divide;
            }
            return (Operator)ThrowHelper.Throw("invalid operator");
        }

        private bool IsCurrentSet { get { return IsSet && tokens.Count == 0; } }
        private bool IsCurrentCall { get { return IsCall && tokens.Count == 0 || tokens.Peek() == "("; } }

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

        private static string IdChars { get { return "@_"; } }
        private static bool IsInitialIdChar(char c) { return char.IsLetter(c) || IdChars.Contains(c); }
        private static bool IsIdChar(char c) { return char.IsLetterOrDigit(c) || IdChars.Contains(c); }

        private void Tokenize()
        {
            for (int i = 0; i < Path.Length; )
            {
                char c = Path[i];
                if (char.IsWhiteSpace(c)) { ++i; continue; }
                if (c == '\'')
                {
                    var start = ++i;
                    for (++i; i < Path.Length && Path[i] != '\''; ++i) continue;
                    if (Path[i] == Path.Length) ThrowHelper.Throw("missing closing quote");
                    tokens.Enqueue('"' + Path.Substring(start, i++ - start));
                    continue;
                }
                if ("$.[](),+-*/".Contains(c)) { tokens.Enqueue(c.ToString()); ++i; continue; }
                if (!IsIdChar(c)) ThrowHelper.Throw("invalid token: " + Path.Substring(i));
                {
                    var start = i;
                    for (++i; i < Path.Length && IsIdChar(Path[i]); ++i) continue;
                    tokens.Enqueue(Path.Substring(start, i - start));
                }
            }
        }
    }
}
