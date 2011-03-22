using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The CodeTree class compiles code into a tree structure that can be
    /// executed to produce side-effects or results.
    /// </summary>
    public class CodeTree
    {
        private Engine engine;
        private TokenQueue tokens;
        private Node root;
        private Dictionary<string, Func<StatementNode>> keywordMap;
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
        private static Dictionary<string, AssignmentOp> assignmentOperatorMap = new Dictionary<string, AssignmentOp>
        {
            { "=", AssignmentOp.Assign },
            { "+=", AssignmentOp.PlusEquals },
            { "-=", AssignmentOp.MinusEquals },
            { "*=", AssignmentOp.TimesEquals },
            { "/=", AssignmentOp.DivideEquals },
            { "&=", AssignmentOp.AndEquals },
            { "|=", AssignmentOp.OrEquals },
            { "++", AssignmentOp.Increment },
            { "--", AssignmentOp.Increment },
        };

        private static string IdChars { get { return "_"; } }
        private static IDictionary<string, Op> OperatorMap { get { return operatorMap; } }
        private static IDictionary<string, AssignmentOp> AssignmentOperatorMap { get { return assignmentOperatorMap; } }
        private IDictionary<string, Func<StatementNode>> KeywordMap { get { return keywordMap; } }
        private bool IsCurrentCall { get { return IsCall && tokens != null && tokens.Count == 0 || PeekToken("("); } }

        public CodeType CodeType { get; private set; }
        public bool IsVariable { get { return CodeType == CodeType.Variable; } }
        public bool IsSet { get { return CodeType == CodeType.SetExpression; } }
        public bool IsCall { get { return CodeType == CodeType.Call; } }
        public bool IsScript { get { return CodeType == CodeType.Script; } }
        public string Path { get; private set; }

        public CodeTree()
        {
            keywordMap = new Dictionary<string, Func<StatementNode>>
            {
                { "var", ParseVar },
                { "if", ParseIf },
                { "while", ParseWhile },
                { "continue", ParseContinue },
                { "break", ParseBreak },
                { "for", ParseFor },
                { "foreach", ParseForEach },
                { "return", ParseReturn },
                { "yield", ParseYield },
            };
        }

        public string GetVariable(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Path: Variable {0}", Path);
            return (root as VariableNode).VariableName;
        }

        public object Evaluate(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Path: Get {0}", Path);
            return (root as ExpressionNode).Evaluate(engine);
        }
        public object Set(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: Set {0} = {1}", Path, value);
            return (root as ExpressionNode).Set(engine, value);
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

        public CodeTree Compile(Engine engine, CodeType expressionType, string path)
        {
            if (expressionType == CodeType && object.ReferenceEquals(Path, path)) return this;
            this.engine = engine;
            CodeType = expressionType;
            Path = path;
            Tokenize();
            if (IsVariable) root = ParseVariableExpression();
            else if (IsScript) root = ParseStatements();
            else root = ParseExpression();
            if (tokens.Count > 0) engine.Throw("unexpected token: " + tokens.Dequeue());
            this.engine = null;
            tokens = null;
            return this;
        }

        private StatementNode ParseStatement()
        {
            if (tokens.Count == 0 || PeekToken("}")) return null;
            if (PeekToken(";"))
            {
                ParseToken(";");
                return new EmptyNode();
            }
            var node = null as StatementNode;
            var token = tokens.Peek();
            if (token != null && token[0] == '`' && keywordMap.ContainsKey(token.Substring(1)))
                node = keywordMap[token.Substring(1)]();
            else if (token == "{")
                node = ParseBlock();
            else
            {
                node = ParseExpression();
                ParseSemicolon();
            }
            return node;
        }

        private void ParseSemicolon()
        {
            if (tokens.Count > 0) ParseToken(";");
        }

        private StatementNode ParseStatements()
        {
            var nodes = new List<StatementNode>();
            while (true)
            {
                var node = ParseStatement();
                if (node == null) break;
                nodes.Add(node);
            }
            return new BlockNode { Nodes = nodes };
        }

        private StatementNode ParseBlock()
        {
            ParseToken("{");
            var block = ParseStatements();
            ParseToken("}");
            return block;
        }

        private StatementNode ParseVar()
        {
            ParseKeyword("var");
            var variable = ParseVariable();
            ParseToken("=");
            var expression = ParseExpression();
            ParseSemicolon();
            return new VarNode { VariableName = variable, Value = expression };
        }

        private StatementNode ParseIf()
        {
            var pairs = new List<IfNode.Pair>();
            pairs.Add(ParseIfPair());
            var elseNode = null as StatementNode;
            while (PeekKeyword("else"))
            {
                ParseKeyword("else");
                if (!PeekKeyword("if"))
                {
                    elseNode = ParseStatement();
                    break;
                }
                pairs.Add(ParseIfPair());
            }
            return new IfNode { Pairs = pairs, Else = elseNode };
        }

        private IfNode.Pair ParseIfPair()
        {
            ParseKeyword("if");
            ParseToken("(");
            var expression = ParseExpression();
            ParseToken(")");
            var statement = ParseStatement();
            return new IfNode.Pair { Expression = expression, Statement = statement };
        }

        private StatementNode ParseWhile()
        {
            ParseKeyword("while");
            ParseToken("(");
            var expression = ParseExpression();
            ParseToken(")");
            var statement = ParseStatement();
            return new WhileNode { Condition = expression, Body = statement };
        }

        private StatementNode ParseContinue()
        {
            ParseKeyword("continue");
            ParseSemicolon();
            return new ContinueNode();
        }

        private StatementNode ParseBreak()
        {
            ParseKeyword("break");
            ParseSemicolon();
            return new BreakNode();
        }
         
        private StatementNode ParseFor()
        {
            ParseKeyword("for");
            ParseToken("(");
            var initial = ParseStatement();
            var condition = !PeekToken(";") ? ParseExpression() : null;
            ParseToken(";");
            var next = !PeekToken(";") ? ParseExpression() : null;
            ParseToken(")");
            var body = ParseStatement();
            return new ForNode { Initial = initial, Condition = condition, Next = next, Body = body };
        }

        private StatementNode ParseForEach()
        {
            ParseKeyword("foreach");
            ParseToken("(");
            ParseKeyword("var");
            var name = ParseVariable();
            ParseKeyword("in");
            var expression = ParseExpression();
            ParseToken(")");
            var statement = ParseStatement();
            return new ForEachNode { Collection = expression, VariableName = name, Body = statement };
        }

        private StatementNode ParseReturn()
        {
            ParseKeyword("return");
            var value = ParseExpression();
            ParseSemicolon();
            return new ReturnNode { Value = value };
        }

        private StatementNode ParseYield()
        {
            ParseKeyword("yield");
            var value = ParseExpression();
            ParseSemicolon();
            return new YieldNode { Value = value };
        }

        private ExpressionNode ParseExpression() { return ParseExpression(false); }

        private ExpressionNode ParseExpression(bool noComma)
        {
            if (tokens.Count == 0) return new ValueNode { Value = null };
            var node = new ContextNode() as ExpressionNode;
            var nodeNext = true;
            for (var token = tokens.Peek(); token != null; token = tokens.Peek())
            {
                if (OperatorMap.ContainsKey(token))
                {
                    node = ParseOperator(node, nodeNext);
                    nodeNext = true;
                    continue;
                }
                if (AssignmentOperatorMap.ContainsKey(token))
                {
                    node = ParseAssignmentOperator(node, nodeNext);
                    nodeNext = true;
                    continue;
                }
                if ("=.?,".Contains(token[0]) && nodeNext) engine.Throw("unexpected operator: " + token);
                if (token == ".")
                {
                    nodeNext = true;
                    tokens.Dequeue();
                    continue;
                }
                if (token == "?")
                    node = ParseConditional(node);
                if (token == "," && !noComma)
                    node = ParseComma(node);
                if (!nodeNext) return node;
                char c = token[0];
                if (char.IsDigit(c))
                {
                    tokens.Dequeue();
                    node = new ValueNode { Value = token.Contains('.') ? ParseDouble(token) : ParseInt(token) };
                }
                else if (c == '"')
                    node = new ValueNode { Value = tokens.Dequeue().Substring(1) };
                else if (token == "@iterator")
                    node = ParseIterator();
                else if ("`$@".Contains(c))
                    node = ParseIdentifierExpression(node);
                else if (c == '[')
                    node = ParseTypeExpression();
                else if (c == '(')
                {
                    tokens.Dequeue();
                    node = ParseExpression();
                    ParseToken(")");
                }
                else
                    return node;
                node = ParseItems(node);
                nodeNext = false;
            }
            return node;
        }

        private ExpressionNode ParseOperator(ExpressionNode node, bool nodeNext)
        {
            var token = tokens.Dequeue();
            var op = OperatorMap[token];
            var unary = op.GetArity() == 1;
            if (nodeNext != unary) engine.Throw("unexpected operator" + op);
            if (unary) return new OpNode { Op = op, Operands = { ParseExpression() } };
            return new OpNode { Op = op, Operands = { node, ParseExpression() } };
        }

        private ExpressionNode ParseAssignmentOperator(ExpressionNode node, bool nodeNext)
        {
            var token = tokens.Dequeue();
            var op = AssignmentOperatorMap[token];
            if (op == AssignmentOp.Increment || op == AssignmentOp.Decrement)
            {
                var increment = op == AssignmentOp.Increment ? 1 : 0;
                return new IncrementNode { LValue = node, PostFix = nodeNext, Increment = increment };
            }
            if (nodeNext) engine.Throw("unexpected operator" + op);
            var expression = ParseExpression();
            if (op == AssignmentOp.Assign) return new SetNode { LValue = node, RValue = expression };
            return new SetNode { LValue = node, RValue = new OpNode { Op = (Op)op, Operands = { node, expression } } };
        }

        private ExpressionNode ParseConditional(ExpressionNode node)
        {
            tokens.Dequeue();
            var ifTrue = ParseExpression();
            ParseToken(":");
            var ifFalse = ParseExpression();
            return new ConditionalNode { Conditional = node, IfTrue = ifTrue, IfFalse = ifFalse };
        }

        private ExpressionNode ParseComma(ExpressionNode node)
        {
            tokens.Dequeue();
            var value = ParseExpression();
            return new CommaNode { Operand1 = node, Operand2= value };
        }

        private ExpressionNode ParseIterator()
        {
            ParseToken("@iterator");
            var type = !PeekToken(":") ? ParseTypeExpression() : null;
            ParseToken(":");
            var body = ParseStatement();
            return new IteratorNode { Type = type, Body = body };
        }

        private ExpressionNode ParseIdentifierExpression(ExpressionNode node)
        {
            if (tokens.Peek()[0] == '`')
            {
                var identifier = ParseIdentifier();
                if (IsCurrentCall)
                {
                    var args = PeekToken("(") ? ParseArguments() : null;
                    return new MethodNode { Callee = node, MethodName = identifier, Arguments = args };
                }
                return new PropertyNode { Context = node, PropertyName = identifier };
            }
            var token = tokens.Dequeue();
            if (IsCurrentCall)
            {
                var args = PeekToken("(") ? ParseArguments() : null;
                return new FunctionNode { FunctionName = token, Arguments = args };
            }
            return new VariableNode { VariableName = token };
        }

        private ExpressionNode ParseVariableExpression()
        {
            return new VariableNode { VariableName = ParseVariable() };
        }

        private ExpressionNode ParseItems(ExpressionNode node)
        {
            while (PeekToken("["))
            {
                tokens.Dequeue();
                var index = ParseExpression(true);
                ParseToken("]");
                node = new ItemNode { Context = node, Index = index };
            }
            return node;
        }

        private ExpressionNode ParseTypeExpression()
        {
            tokens.Dequeue();
            var typeNode = ParseType();
            ParseToken("]");
            if (PeekToken("."))
            {
                ParseToken(".");
                var methodName = ParseIdentifier();
                var args = PeekToken("(") ? ParseArguments() : null;
                return new StaticMethodNode { Type = typeNode, MethodName = methodName, Arguments = args };
            }
            if (PeekToken("("))
                return new OpNode { Op = Op.New, Operands = new ExpressionNode[] { typeNode }.Concat(ParseArguments()).ToList() };
            if (PeekToken("{"))
            {
                tokens.Dequeue();
                return ParseInitializer(typeNode);
            }
            return typeNode;
        }

        private ExpressionNode ParseInitializer(TypeNode typeNode)
        {
            var node = new OpNode { Op = Op.New, Operands = { typeNode } } as ExpressionNode;
            while (true)
            {
                if (PeekToken("{")) return ParseDictionaryInitializer(node, node);
                tokens.Dequeue();
                var isCollection = tokens.Peek() != "=";
                tokens.Undequeue();
                if (isCollection) return ParseCollectionInitializer(node, node);
                var property = ParseIdentifier();
                ParseToken("=");
                if (PeekToken("{"))
                {
                    tokens.Dequeue();
                    var propertyNode = new PropertyNode { Context = node, PropertyName = property };
                    if (PeekToken("{"))
                        node = ParseDictionaryInitializer(node, propertyNode);
                    else
                        node = ParseCollectionInitializer(node, propertyNode);
                }
                else
                    node = new PropertyInitializerNode { Context = node, PropertyName = property, Value = ParseExpression(true) };
                var token = tokens.Dequeue();
                if (token == "}") break;
                if (token != ",") engine.Throw("unexpected token: " + token);
            }
            return node;
        }

        private ExpressionNode ParseCollectionInitializer(ExpressionNode context, ExpressionNode collection)
        {
            return new CollectionInitializerNode { Context = context, Collection = collection, Items = ParseList("}") };
        }

        private ExpressionNode ParseDictionaryInitializer(ExpressionNode context, ExpressionNode dictionary)
        {
            var entries = new List<ExpressionNode>();
            while (true)
            {
                ParseToken("{");
                var key = ParseExpression(true);
                ParseToken(",");
                var value = ParseExpression(true);
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
            if (PeekToken(",") || PeekToken(">")) return null;
            var typeName = ParseIdentifier();
            while (PeekToken("."))
            {
                tokens.Dequeue();
                typeName += "." + ParseIdentifier();
            }
            var typeArgs = null as List<TypeNode>;
            if (PeekToken("<"))
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
            return new TypeNode { TypeName = typeName, TypeArguments = typeArgs };
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

        private bool PeekToken(string token)
        {
            return tokens.Peek() == token;
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

        private string ParseVariable()
        {
            if (tokens.Count == 0 || tokens.Peek()[0] != '$') engine.Throw("expected variable");
            return tokens.Dequeue();
        }

        private bool PeekKeyword(string keyword)
        {
            return PeekToken("`" + keyword);
        }

        private void ParseKeyword(string keyword)
        {
            if (ParseIdentifier() != keyword) engine.Throw("expected keyword: " + keyword);
        }

        private IList<ExpressionNode> ParseArguments()
        {
            ParseToken("(");
            return ParseList(")");
        }

        private IList<ExpressionNode> ParseList(string expectedToken)
        {
            var nodes = new List<ExpressionNode>();
            if (tokens.Count > 0 && tokens.Peek() == expectedToken)
            {
                tokens.Dequeue();
                return nodes;
            }
            while (tokens.Count > 0)
            {
                nodes.Add(ParseExpression(true));
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
                string c2 = Path.Substring(i, Math.Min(2, Path.Length - i));
                if (char.IsWhiteSpace(c)) ++i;
                else if (c2 == "/*")
                    i = EatComment(i);
                else if (OperatorMap.ContainsKey(c2) || AssignmentOperatorMap.ContainsKey(c2))
                {
                    tokens.Enqueue(c2);
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
