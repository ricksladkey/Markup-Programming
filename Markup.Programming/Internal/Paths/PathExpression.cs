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
        private Engine engine;
        private TokenQueue tokens;
        private PathNode root;
        private Dictionary<string, Func<PathNode>> keywordMap;
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

        private static string IdChars { get { return "_"; } }
        private static IDictionary<string, Op> OperatorMap { get { return operatorMap; } }
        private IDictionary<string, Func<PathNode>> KeywordMap { get { return keywordMap; } }
        private bool IsCurrentSet { get { return IsSet && tokens != null && tokens.Count == 0; } }
        private bool IsCurrentCall { get { return IsCall && tokens != null && tokens.Count == 0 || tokens.Peek() == "("; } }

        public ExpressionType ExpressionType { get; private set; }
        public bool IsSet { get { return (ExpressionType & ExpressionType.Set) == ExpressionType.Set; } }
        public bool IsCall { get { return (ExpressionType & ExpressionType.Call) == ExpressionType.Call; } }
        public bool IsBlock { get { return (ExpressionType & ExpressionType.Block) == ExpressionType.Block; } }
        public string Path { get; private set; }

        public object Evaluate(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Path: Get {0}", Path);
            return root.Evaluate(engine);
        }
        public object Evaluate(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: Set {0} = {1}", Path, value);
            return root.Evaluate(engine, value);
        }
        public object Call(Engine engine, IEnumerable<object> args)
        {
            engine.Trace(TraceFlags.Path, "Path: Call: {0}", Path);
            var call = root as CallNode;
            if (call == null) engine.Throw("not a call node");
            return call.Call(engine, args);
        }
        public void Execute(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Path: Execute {0}", Path);
            var block = root as BlockNode;
            block.Execute(engine);
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
            keywordMap = new Dictionary<string, Func<PathNode>>
            {
                { "return", ParseReturn },
            }; 
            var nodes = new List<PathNode>();
            while (true)
            {
                var token = tokens.Peek();
                if (token != null && token[0] == '`' && keywordMap.ContainsKey(token.Substring(1)))
                    nodes.Add(keywordMap[token.Substring(1)]());
                else
                    nodes.Add(Parse());
                ParseToken(";");
                if (tokens.Count == 0) break;
            }
            return new BlockNode { Nodes = nodes };
        }

        private PathNode ParseReturn()
        {
            tokens.Dequeue();
            return new ReturnNode { Context = Parse() };
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
                    ParseToken(":");
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
                    var identifier = ParseIdentifier();
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
                    ParseToken("]");
                    if (tokens.Peek() == ".")
                    {
                        ParseToken(".");
                        var methodName = ParseIdentifier();
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
                    ParseToken(")");
                }
                else
                    return node;
                while (tokens.Peek() == "[")
                {
                    tokens.Dequeue();
                    var index = Parse(true);
                    ParseToken("]");
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
                var property = ParseIdentifier();
                ParseToken("=");
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
                ParseToken("{");
                var key = Parse(true);
                ParseToken(",");
                var value = Parse(true);
                ParseToken("}");
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
            var typeName = ParseIdentifier();
            while (tokens.Peek() == ".")
            {
                tokens.Dequeue();
                typeName += "." + ParseIdentifier();
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

        private string ParseToken(string token)
        {
            if (tokens.Count == 0 || tokens.Peek() != token) engine.Throw("missing token: " + token);
            return tokens.Dequeue();
        }

        private string ParseIdentifier()
        {
            if (tokens.Count == 0 || tokens.Peek()[0] != '`') engine.Throw("expected identifier");
            return tokens.Dequeue().Substring(1);
        }

        private IList<PathNode> ParseArguments()
        {
            ParseToken("(");
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

        private static bool IsInitialIdChar(char c) { return char.IsLetter(c) || IdChars.Contains(c); }

        private static bool IsIdChar(char c) { return char.IsLetterOrDigit(c) || IdChars.Contains(c); }

        public static bool IsValidVariable(string variable)
        {
            if (variable.Length < 2 || variable[0] != '$') return false;
            return IsInitialIdChar(variable[1]) && variable.Skip(2).All(c => IsIdChar(c));
        }

        private static bool IsQuote(char c) { return "'\"".Contains(c); }
    }
}
