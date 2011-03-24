﻿using System;
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
        private static Dictionary<string, object> constantMap = new Dictionary<string, object>
        {
            { "true", true },
            { "false", false },
            { "null", null },
        };
        private static Dictionary<string, Op> operatorMap = new Dictionary<string, Op>
        {
            { "+", Op.Plus },
            { "-", Op.Minus },
            { "*", Op.Times },
            { "/", Op.Divide },
            { "%", Op.Mod },
            { "&&", Op.AndAnd },
            { "||", Op.OrOr },
            { "!", Op.Not },
            { "&", Op.BitwiseAnd },
            { "|", Op.BitwiseOr },
            { "^", Op.BitwiseXor },
            { "~", Op.BitwiseNot },
            { "<<", Op.LeftShift },
            { ">>", Op.RightShift },
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
            { "%=", AssignmentOp.ModEquals },
            { "/=", AssignmentOp.DivideEquals },
            { "&=", AssignmentOp.AndEquals },
            { "|=", AssignmentOp.OrEquals },
            { "++", AssignmentOp.Increment },
            { "--", AssignmentOp.Increment },
        };

        private static string IdChars { get { return "_"; } }
        private static IDictionary<string, object> ConstantMap { get { return constantMap; } }
        private static IDictionary<string, Op> OperatorMap { get { return operatorMap; } }
        private static IDictionary<string, AssignmentOp> AssignmentOperatorMap { get { return assignmentOperatorMap; } }
        private bool IsCurrentCall { get { return IsCall && tokens != null && tokens.Count == 0 || PeekToken("("); } }

        public CodeType CodeType { get; private set; }
        public bool IsVariable { get { return CodeType == CodeType.Variable; } }
        public bool IsSet { get { return CodeType == CodeType.SetExpression; } }
        public bool IsCall { get { return CodeType == CodeType.Call; } }
        public bool IsScript { get { return CodeType == CodeType.Script; } }
        public string Path { get; private set; }

        public CodeTree()
        {
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
            var block = root as ScriptNode;
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
            var token= PeekKeyword();
            if (token == "var") return ParseVar();
            if (token == "if") return ParseIf();
            if (token == "while") return ParseWhile();
            if (token == "continue") return ParseContinue();
            if (token == "break") return ParseBreak();
            if (token == "for") return ParseFor();
            if (token == "foreach") return ParseForEach();
            if (token == "return") return ParseReturn();
            if (token == "yield") return ParseYield();
            if (token == "{")
            {
                ParseToken("{");
                var block = ParseStatements();
                ParseToken("}");
                return block;
            }
            var node = ParseExpression();
            ParseSemicolon();
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
            return new ScriptNode { Nodes = nodes };
        }

        private StatementNode ParseVar()
        {
            ParseKeyword("var");
            var name = ParseVariable();
            if (PeekToken("("))
            {
                return ParseFunction(name);
            }
            ParseToken("=");
            var expression = ParseExpression();
            ParseSemicolon();
            return new VarNode { VariableName = name, Value = expression };
        }

        private StatementNode ParseFunction(string name)
        {
            ParseToken("(");
            var parameters = new ParameterCollection();
            parameters.Add(new Parameter { ParameterName = ParseVariable() });
            while (!PeekToken(")"))
            {
                ParseToken(",");
                parameters.Add(new Parameter { ParameterName = ParseVariable() });
            }
            ParseToken(")");
            var body = ParseStatement();
            return new FuncNode { FunctionName = name, Parameters = parameters, Body = body };
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
            var next = !PeekToken(")") ? ParseExpression() : null;
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
            var value = !PeekToken(";") ? ParseExpression() : null;
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
            int start = tokens.Count;
            var node = ParseExpressionInternal(noComma);
            if (tokens.Count == start) engine.Throw("empty expression");
            return node;
        }

        private ExpressionNode ParseExpressionInternal(bool noComma)
        {
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
            var expression = ParseExpression();
            if (op == Op.Minus && nodeNext) return new OpNode { Op = Op.Negate, Operands = { expression } };
            if (nodeNext != unary) engine.Throw("unexpected operator" + op);
            if (unary) return new OpNode { Op = op, Operands = { expression } };
            return new OpNode { Op = op, Operands = { node, expression } };
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

        private ExpressionNode ParseBlock()
        {
            ParseToken("@block");
            ParseToken(":");
            var body = ParseStatement();
            return new BlockNode { Body = body };
        }

        private ExpressionNode ParseIdentifierExpression(ExpressionNode node)
        {
            var token = tokens.Peek();
            if (token == "@iterator") return ParseIterator();
            if (token == "@block") return ParseBlock();
            if (token[0] == '`')
            {
                var identifier = ParseIdentifier();
                if (ConstantMap.ContainsKey(identifier)) return new ValueNode { Value = ConstantMap[identifier] };
                if (IsCurrentCall)
                {
                    var args = PeekToken("(") ? ParseArguments() : null;
                    return new MethodNode { Callee = node, MethodName = identifier, Arguments = args };
                }
                return new PropertyNode { Context = node, PropertyName = identifier };
            }
            tokens.Dequeue();
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
                var identifier = ParseIdentifier();
                if (IsCurrentCall)
                {
                    var args = PeekToken("(") ? ParseArguments() : null;
                    return new StaticMethodNode { Type = typeNode, MethodName = identifier, Arguments = args };
                }
                return new StaticPropertyNode { Type = typeNode, PropertyName = identifier };
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
            // Phase I: collect all the properties.
            var properties = new List<InitializerProperty>();
            while (true)
            {
                var property = new InitializerProperty();
                if (PeekToken("{")) return ParseDictionaryInitializer(typeNode);
                var token = tokens.Dequeue();
                var isCollection = tokens.Peek() != "=";
                tokens.Undequeue(token);
                if (isCollection) return ParseCollectionInitializer(typeNode);
                property.PropertyName = ParseIdentifier();
                ParseToken("=");
                if (PeekToken("{"))
                {
                    tokens.Dequeue();
                    if (PeekToken("{"))
                    {
                        property.IsDictionary = true;
                        property.Values = ParseDictionary();
                    }
                    else
                    {
                        property.IsCollection = true;
                        property.Values = ParseList("}");
                    }
                }
                else
                    property.Value = ParseExpression(true);
                properties.Add(property);
                token = tokens.Dequeue();
                if (token == "}") break;
                if (token != ",") engine.Throw("unexpected token: " + token);
            }

            // Phase II: apply all the properties.
            var context = new ObjectNode { Type = typeNode, Properties = properties } as ExpressionNode;
            foreach (var property in properties)
            {
                if (property.IsCollection || property.IsDictionary)
                {
                    var propertyNode = new PropertyNode { Context = context, PropertyName = property.PropertyName };
                    if (property.IsDictionary)
                        context = new DictionaryInitializerNode { Context = context, Dictionary = propertyNode, Items = property.Values };
                    else
                        context = new CollectionInitializerNode { Context = context, Collection = propertyNode, Items = property.Values };
                }
                else
                    context = new PropertyInitializerNode { Context = context, PropertyName = property.PropertyName, Value = property.Value };
                property.Value = null;
                property.Values = null;
            }
            return context;
        }

        private ExpressionNode ParseCollectionInitializer(TypeNode typeNode)
        {
            var context = new OpNode { Op = Op.New, Operands = { typeNode } } as ExpressionNode;
            return ParseCollectionInitializer(context, context);
        }

        private ExpressionNode ParseCollectionInitializer(ExpressionNode context, ExpressionNode collection)
        {
            return new CollectionInitializerNode { Context = context, Collection = collection, Items = ParseList("}") };
        }

        private ExpressionNode ParseDictionaryInitializer(TypeNode typeNode)
        {
            //var context = new OpNode { Op = Op.New, Operands = { typeNode } } as ExpressionNode;
            //return ParseDictionaryInitializer(context, context);
            //return new DictionaryInitializerNode { Context = context, Dictionary = dictionary, Items = entries };
            return null;
        }

        private List<ExpressionNode> ParseDictionary()
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
            return entries;
        }

        private TypeNode ParseType()
        {
            if (PeekToken("]") || PeekToken(",") || PeekTokenStartsWith(">")) return null;
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
                    if (token.StartsWith(">"))
                    {
                        if (token == ">>") tokens.Undequeue(">");
                        break;
                    }
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

        private bool PeekTokenStartsWith(string token)
        {
            return tokens.Peek() != null && tokens.Peek().StartsWith(token);
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

        private string PeekKeyword()
        {
            return tokens.Peek() != null && tokens.Peek()[0] == '`' ? tokens.Peek().Substring(1) : tokens.Peek();
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
                    i = EatMultiLineComment(i);
                else if (c2 == "//")
                    i = EatSingleLineComment(i);
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

        private int EatMultiLineComment(int i)
        {
            for (i += 2; i < Path.Length - 1 && Path.Substring(i, 2) != "*/"; i++)
                if (Path.Substring(i, 2) == "/*") i = EatMultiLineComment(i) - 1;
            return i + 2;
        }

        private int EatSingleLineComment(int i)
        {
            for (i += 2; i < Path.Length && Path[i] != ';'; i++) continue;
            return Math.Min(Path.Length, i + 1);
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
