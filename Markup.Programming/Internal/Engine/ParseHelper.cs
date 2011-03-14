using System.Collections.Generic;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    public static class ParseHelper
    {
        public class PathExpression
        {
            private enum Value { UnsetValue = 0 };
            private PathNode root;
            public PathExpression(PathNode root) { this.root = root; }
            public static bool IsUnset(object value) { return value.Equals(Value.UnsetValue); }
            public object Evaluate(Engine engine) { return root.Evaluate(engine, Value.UnsetValue); }
            public object Evaluate(Engine engine, object value) { return root.Evaluate(engine, value); }
        }

        public abstract class PathNode
        {
            public bool Get { get; set; }
            public PathNode Context { get; set; }
            public abstract object Evaluate(Engine engine, object value);
            public void ThrowIfUnset(object value) { if (PathExpression.IsUnset(value)) ThrowHelper.Throw("unset"); }
        }

        public class ValueNode : PathNode
        {
            public object Value { get; set; }
            public override object Evaluate(Engine engine, object value) { return Value; }
        }

        public class ContextNode : PathNode
        {
            public override object Evaluate(Engine engine, object value) { return engine.Context; }
        }

#if DEBUG
        [DebuggerDisplay("Property: Get = {Get}, ParameterName = {ParameterName}")]
#endif
        public class ParameterNode : PathNode
        {
            public string ParameterName { get; set; }
            public override object Evaluate(Engine engine, object value)
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
            public override object Evaluate(Engine engine, object value)
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
            public override object Evaluate(Engine engine, object value)
            {
#if true
                if (Get)
                {
                    var context = Context.Evaluate(engine, value);
                    var index = Index.Evaluate(engine, value);
                    return PathHelper.GetItem(context, index);
                }
#else
                if (Get) return PathHelper.GetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value));
#endif
                ThrowIfUnset(value);
                PathHelper.SetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value), value);
                return value;
            }
        }

        public static PathExpression ParseGet(string path) { return Parse(path, true); }
        public static PathExpression ParseSet(string path) { return Parse(path, false); }

        private static PathExpression Parse(string path, bool getPath)
        {
            var tokens = Tokenize(path);
            var pathNode = Parse(tokens, null, getPath);
            if (tokens.Count != 0) ThrowHelper.Throw("unexpected token: " + tokens.Dequeue());
            return new PathExpression(pathNode);
        }

        public static PathNode Parse(Queue<string> tokens, string expectedToken, bool getPath)
        {
            if (tokens.Count == 0) return new ValueNode { Value = null };
            var node = new ContextNode() as PathNode;
            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();
                var c = token[0];
                if (c == '.') continue;
                if (c == '$')
                {
                    if (tokens.Count == 0) ThrowHelper.Throw("missing parameter");
                    var parameterName = tokens.Dequeue();
                    var get = getPath || tokens.Count > 0;
                    node = new ParameterNode { Get = get, ParameterName = parameterName };
                    continue;
                }
                if (IsInitialIdentifierChar(c))
                {
                    var get = getPath || tokens.Count > 0;
                    node = new PropertyNode { Get = get, Context = node, PropertyName = token };
                    continue;
                }
                if (c == '[')
                {
                    var index = Parse(tokens, "]", true);
                    var get = getPath || tokens.Count > 0;
                    node = new ItemNode { Get = get, Context = node, Index = index };
                    continue;
                }
                if (char.IsDigit(c))
                {
                    node = new ValueNode { Value = int.Parse(token) };
                    continue;
                }
                if (token != expectedToken) ThrowHelper.Throw("unexpected token: " + token);
                break;
            }
            return node;
        }

        private static Queue<string> Tokenize(string path)
        {
            Queue<string> tokens = new Queue<string>();
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
                    case '$': case '[': case ']': case '!': case '.':
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
