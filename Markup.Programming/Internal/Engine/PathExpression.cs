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
        public static bool IsUnset(object value) { return value.Equals(Value.UnsetValue); }

        private TokenQueue tokens = new TokenQueue();

        public PathExpression(bool isGet, bool isProperty, string path)
        {
            IsGet = isGet;
            IsProperty = isProperty;
            Path = path;
            Tokenize();
            PathNode = Parse();
            if (tokens.Count != 0) ThrowHelper.Throw("unexpected token: " + tokens.Dequeue());
            tokens = null;
        }

        public bool IsGet { get; private set; }
        public bool IsSet { get { return !IsGet; } }
        public bool IsProperty { get; private set; }
        public bool IsCall { get { return !IsProperty; } }
        public string Path { get; private set; }
        public PathNode PathNode { get; private set; }

        public object Evaluate(Engine engine) { return Evaluate(engine, Value.UnsetValue); }
        public object Evaluate(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: Evaluating: {0}", Path);
            return PathNode.Evaluate(engine, value);
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
                else if (IsInitialIdentifierChar(c))
                {
                    tokens.Dequeue();
                    if (IsCurrentMethod)
                    {
                        var args = tokens.Peek() == "(" ? ParseArguments() : null;
                        node = new MethodNode { Context = node, Name = token, Arguments = args };
                    }
                    else
                        node = new PropertyNode { IsGet = IsCurrentGet, Context = node, Name = token };
                }
                else if (c == '$')
                {
                    tokens.Dequeue();
                    if (tokens.Count == 0) ThrowHelper.Throw("missing parameter");
                    var name = tokens.Dequeue();
                    if (IsCurrentMethod)
                    {
                        var args = tokens.Peek() == "(" ? ParseArguments() : null;
                        node = new FunctionNode { Context = node, Name = name, Arguments = args };
                    }
                    else
                        node = new ParameterNode { IsGet = IsCurrentGet, ParameterName = name };
                }
                else if (c == '[')
                {
                    tokens.Dequeue();
                    var typeName = "";
                    while (tokens.Count > 0 && tokens.Peek() != "]") typeName += tokens.Dequeue();
                    VerifyToken("]");
                    VerifyToken(".");
                    var type = Engine.LookupType(typeName);
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
                    node = new ItemNode { IsGet = IsCurrentGet, Context = node, Index = index };
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

        private bool IsCurrentGet { get { return IsGet || tokens.Count > 0; } }
        private bool IsCurrentMethod { get { return tokens.Peek() == "(" || tokens.Count == 0 && IsCall; } }

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

        private void Tokenize()
        {
            for (int i = 0; i < Path.Length; )
            {
                char c = Path[i];
                if (c == ' ') { ++i; continue; }
                if (c == '\'')
                {
                    var start = ++i;
                    for (++i; i < Path.Length && Path[i] != '\''; ++i) continue;
                    if (Path[i] == Path.Length) ThrowHelper.Throw("missing closing quote");
                    tokens.Enqueue('"' + Path.Substring(start, i++ - start));
                    continue;
                }
                if ("$.[](),+-*/".Contains(c)) { tokens.Enqueue(c.ToString()); ++i; continue; }
                if (!IsIdentifierChar(c)) ThrowHelper.Throw("invalid token: " + Path.Substring(i));
                {
                    var start = i;
                    for (++i; i < Path.Length && IsIdentifierChar(Path[i]); ++i) continue;
                    tokens.Enqueue(Path.Substring(start, i - start));
                }
            }
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
