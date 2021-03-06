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
#if DEBUG
    [DebuggerTypeProxy(typeof(CodeTreeDebugView))]
#endif
    public class CodeTree
    {
        private Engine engine;

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
            { "??", Op.FirstNonNull },
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
        private static Dictionary<string, int> precedenceMap = new Dictionary<string, int>()
        {
            { "*", 20 },
            { "%", 20},
            { "/", 20 },

            { "+", 19 },
            { "-", 19 },

            { "<<", 18 },
            { ">>", 18 },

            { "<", 17 },
            { "<=", 17 },
            { ">", 17 },
            { ">=", 17 },

            { "==", 16 },
            { "!=", 16 },

            { "&", 15 },
            { "^", 14 },
            { "|", 13 },

            { "&&", 12 },
            { "||", 11 },

            { "??", 10 },

            { ":", 9 },
            { "?", 8 },

            { "=", 1 },
            { "+=", 1 },
            { "-=", 1 },
            { "*=", 1 },
            { "%=", 1 },
            { "/=", 1 },
            { "&=", 1 },
            { "|=", 1 },

            { ",", 0 },

        };
        private static IDictionary<string, string> operatorAliasMap = new Dictionary<string, string>()
        {
            { "@gt", ">" },
            { "@gteq", ">=" },
            { "@lt", "<" },
            { "@lteq", "<=" },
            { "@eq", "==" },
            { "@neq", "!=" },
            { "@and", "&" },
            { "@or", "|" },
            { "@andand", "&&" },
            { "@oror", "||" },
        };

        private static string IdChars { get { return "_"; } }
        private bool IsCurrentEvent { get { return IsEvent && Tokens.Count == 0; } }
        private bool IsCurrentCall { get { return IsCall && Tokens.Count == 0 || PeekToken("("); } }

        public Node Root { get; private set; }
        public TokenQueue Tokens { get; private set; }
        public CodeType CodeType { get; private set; }
        public string Code { get; private set; }

        public bool IsStatement { get { return (CodeType & CodeType.Statement) == CodeType.Statement; } }
        public bool IsVariable { get { return (CodeType & CodeType.Variable) == CodeType.Variable; } }
        public bool IsEvent { get { return (CodeType & CodeType.Event) == CodeType.Event; } }
        public bool IsSet { get { return (CodeType & CodeType.Set) == CodeType.Set; } }
        public bool IsCall { get { return (CodeType & CodeType.Call) == CodeType.Call; } }
        public bool IsScript { get { return (CodeType & CodeType.Script) == CodeType.Script; } }
        public bool HasHandler { get { return Root is EventNode && (Root as EventNode).Handler != null; } }
        public bool IsSetOrIncrement { get { return Root is PathNode &&
            ((Root as PathNode).Path is SetNode || (Root as PathNode).Path is IncrementNode); } }

        public CodeTree Compile(Engine engine, CodeType expressionType, string path)
        {
            if (expressionType == CodeType && object.ReferenceEquals(Code, path)) return this;
            this.engine = engine;
            CodeType = expressionType;
            Code = path;
            Tokenize();
            if (IsVariable) Root = ParseVariableExpression();
            else if (IsScript) Root = ParseStatements();
            else if (IsEvent && IsStatement) Root = ParseEventStatement();
            else if (IsEvent) Root = ParseEventExpression();
            else Root = ParsePath();
            if (Tokens.Count > 0) engine.Throw("unexpected token: " + Tokens.Dequeue());
            this.engine = null;
            Tokens = null;
            return this;
        }

        public string GetVariable(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Code: GetVariable {0}", Code);
            if (!(Root is VariableNode)) engine.Throw("not a variable expression");
            return (Root as VariableNode).VariableName;
        }

        public string GetEvent(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Code: GetEvent {0}", Code);
            if (!(Root is EventNode)) engine.Throw("not an event expression");
            return (Root as EventNode).EventName;
        }

        public object GetContext(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Code: GetContext {0}", Code);
            if (!(Root is EventNode)) engine.Throw("not an event expression");
            return (Root as EventNode).Context.Get(engine);
        }

        public object Get(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Code: Get {0}", Code);
            if (!(Root is ExpressionNode)) engine.Throw("not an expression");
            return (Root as ExpressionNode).Get(engine);
        }

        public object Set(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Code: Set {0} = {1}", Code, value);
            if (!(Root is ExpressionNode)) engine.Throw("not an expression");
            return (Root as ExpressionNode).Set(engine, value);
        }

        public object Call(Engine engine, IEnumerable<object> args)
        {
            engine.Trace(TraceFlags.Path, "Code: Call: {0}", Code);
            if (!(Root is CallNode)) engine.Throw("not a call expression");
            return (Root as CallNode).Call(engine, args);
        }

        public void Execute(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Code: Execute {0}", Code);
            if (!(Root is ScriptNode)) engine.Throw("not a script");
            (Root as ScriptNode).Execute(engine);
        }

        private PathNode ParsePath()
        {
            return new PathNode { Path = ParseExpression(0) };
        }

        private EventNode ParseEventExpression()
        {
            var eventNode = ParseExpression(0);
            if (!(eventNode is EventNode)) engine.Throw("not an event expression");
            return eventNode as EventNode;
        }

        private EventNode ParseEventStatement()
        {
            var eventNode = ParseStatement();
            if (!(eventNode is EventNode)) engine.Throw("not an event statement");
            return eventNode as EventNode;
        }

        private StatementNode ParseStatement()
        {
            if (Tokens.Count == 0 || PeekToken("}")) return null;
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
            if (token == "@context") return ParseContext();
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
            if (Tokens.Count > 0) ParseToken(";");
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
            var name = ParseIdentifierOrVariable();
            if (name[0] == '`') name = name.Substring(1);
            if (PeekToken("("))
            {
                return ParseFunction(name);
            }
            var expression = null as ExpressionNode;
            if (!PeekToken(";"))
            {
                ParseToken("=");
                expression = ParseExpression();
            }
            ParseSemicolon();
            return new VarNode { VariableName = name, Value = expression };
        }

        private StatementNode ParseFunction(string name)
        {
            ParseToken("(");
            var parameters = new ParameterCollection();
            if (!PeekToken(")")) parameters.Add(new Parameter { ParameterName = ParseVariable() });
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
            var condition = ParseExpression();
            ParseToken(")");
            var body = ParseStatement();
            return new WhileNode { Condition = condition, Body = body };
        }

        private StatementNode ParseContext()
        {
            ParseToken("@context");
            ParseToken("(");
            var context = ParseExpression();
            ParseToken(")");
            var body = ParseStatement();
            return new ContexteNode { Context= context, Body = body };
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

        private ExpressionNode ParseExpression() { return ParseBinary(1, false); }
        private ExpressionNode ParseExpressionNoComma() { return ParseBinary(1, true); }

        private ExpressionNode ParseExpression(int level) { return ParseBinary(level, false); }

        private ExpressionNode ParseBinary(int level, bool noComma)
        {
            var operators = new Stack<string>();
            var operands = new Stack<ExpressionNode>();
            operands.Push(ParseUnary(level));
            while (true)
            {
                var token = Tokens.Peek();
                if (token == null) break;
                if (assignmentOperatorMap.ContainsKey(token) || operatorMap.ContainsKey(token))
                    Tokens.Dequeue();
                else if ((token == "," && !noComma) || token == "?" || token == ":")
                    Tokens.Dequeue();
                else
                    break;
                while (operators.Count > 0 && ShouldPerformOperation(token, operators.Peek()))
                    PerformOperation(operators, operands);
                operators.Push(token);
                operands.Push(ParseUnary(level + 1));
            }
            while (operators.Count > 0) PerformOperation(operators, operands);
            return operands.Pop();
        }

        private bool ShouldPerformOperation(string o1, string o2)
        {
            var delta = precedenceMap[o1] - precedenceMap[o2];
            bool rightAssociative = assignmentOperatorMap.ContainsKey(o1) || o1 == ":";
            return rightAssociative ? delta < 0 : delta <= 0;
        }

        private void PerformOperation(Stack<string> operators, Stack<ExpressionNode> operands)
        {
            var token = operators.Pop();
            var operand2 = operands.Pop();
            var operand1 = operands.Pop();
            if (assignmentOperatorMap.ContainsKey(token))
                operands.Push(new SetNode { LValue = operand1, Op = assignmentOperatorMap[token], RValue = operand2 });
            else if (operatorMap.ContainsKey(token))
                operands.Push(new OpNode { Op = operatorMap[token], Operands = { operand1, operand2 } });
            else if (token == ",")
                operands.Push(new CommaNode { Operand1 = operand1, Operand2 = operand2 });
            else if (token == ":")
            {
                if (operators.Peek() != "?") engine.Throw("incomplete conditional operator");
                operators.Pop();
                var operand0 = operands.Pop();
                operands.Push(new ConditionalNode { Conditional = operand0, IfTrue = operand1, IfFalse = operand2 });
            }
        }

        private ExpressionNode ParseUnary(int level)
        {
            var token = Tokens.Peek();
            if (token == "+")
            {
                Tokens.Dequeue();
                return ParseUnary(level);
            }
            if (token == "-")
            {
                ParseToken("-");
                return new OpNode { Op = Op.Negate, Operands = { ParseUnary(level) } };
            }
            if (operatorMap.ContainsKey(token) && operatorMap[token].GetArity() == 1)
            {
                Tokens.Dequeue();
                return new OpNode { Op = operatorMap[token], Operands = { ParseUnary(level) } };
            }
            if (token == "++" || token == "--")
            {
                Tokens.Dequeue();
                var op = token == "++" ? AssignmentOp.Increment : AssignmentOp.Decrement;
                return new IncrementNode { Op = op, LValue = ParseUnary(level) };
            }
            return ParsePrimary(level);
        }

        private ExpressionNode ParsePrimary(int level)
        {
            var node = ParseAtom(level);
            while (true)
            {
                var token = Tokens.Peek();
                if (token == ".")
                {
                    ParseToken(".");
                    node = ParseIdentifierExpression(level, node);
                }
                else if (token == "[")
                {
                    ParseToken("[");
                    node = new ItemNode { Context = node, Arguments = ParseList("]") };
                }
                else if (token == "++" || token == "--")
                {
                    Tokens.Dequeue();
                    var op = token == "++" ? AssignmentOp.PostIncrement : AssignmentOp.PostDecrement;
                    return new IncrementNode { Op = op, LValue = node };
                }
                else
                    break;
            }
            return node;
        }

        private ExpressionNode ParseAtom(int level)
        {
            var token = Tokens.Peek();
            if (token == null) engine.Throw("missing expression");
            char c = token[0];
            if (c == '`' && constantMap.ContainsKey(token.Substring(1)))
            {
                Tokens.Dequeue();
                return new ValueNode { Value = constantMap[token.Substring(1)] };
            }
            if (char.IsDigit(c))
            {
                Tokens.Dequeue();
                return new ValueNode { Value = token.Contains('.') ? ParseDouble(token) : ParseInt(token) };
            }
            if (c == '"')
                return new ValueNode { Value = Tokens.Dequeue().Substring(1) };
            if ("`$@".Contains(c))
                return ParseIdentifierExpression(level, new VariableNode { VariableName = Engine.ContextKey });
            if (c == '[')
                return ParseTypeExpression();
            if (c == '(')
            {
                Tokens.Dequeue();
                var node = ParseExpression();
                ParseToken(")");
                return node;
            }

            return engine.Throw("unexpected token") as ExpressionNode;
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

        private ExpressionNode ParseIdentifierExpression(int level, ExpressionNode node)
        {
            var token = Tokens.Peek();
            if (token == "@iterator") return ParseIterator();
            if (token == "@block") return ParseBlock();
            if (token[0] == '`' || token == Handler.AttachedKey)
            {
                var identifier = token == Handler.AttachedKey ? ParseToken(Handler.AttachedKey) : ParseIdentifier();
                if ((IsCurrentEvent && level == 0) || PeekToken("=>")) return ParseEventExpression(level, node, identifier);
                if (IsCurrentCall)
                {
                    var args = PeekToken("(") ? ParseArguments() : null;
                    return new MethodNode { Callee = node, MethodName = identifier, Arguments = args };
                }
                return new PropertyNode { Context = node, PropertyName = identifier };
            }
            Tokens.Dequeue();
            if (IsCurrentCall)
            {
                var args = PeekToken("(") ? ParseArguments() : null;
                return new FunctionNode { FunctionName = token, Arguments = args };
            }
            return new VariableNode { VariableName = token };
        }

        private ExpressionNode ParseEventExpression(int level, ExpressionNode node, string identifier)
        {
            var context = node;
            if (node is VariableNode && (node as VariableNode).VariableName == Engine.ContextKey)
                context = new VariableNode { VariableName = Engine.AssociatedObjectKey };
            if (!PeekToken("=>")) return new EventNode { Context = context, EventName = identifier };
            ParseToken("=>");
            var handler = PeekToken("{") ? new BlockNode { Body = ParseStatement() } : ParseExpression();
            return new EventNode { Context = context, EventName = identifier, Handler = handler };
        }

        private ExpressionNode ParseVariableExpression()
        {
            return new VariableNode { VariableName = ParseVariable() };
        }

        private ExpressionNode ParseTypeExpression()
        {
            Tokens.Dequeue();
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
                Tokens.Dequeue();
                return ParseInitializer(typeNode);
            }
            return typeNode;
        }

        private ExpressionNode ParseInitializer(TypeNode typeNode)
        {
            var properties = new List<InitializerProperty>();
            while (true)
            {
                var property = new InitializerProperty();
                if (PeekToken("{")) return new DictionaryNode { Type = typeNode, Items = ParseDictionary() };
                var token = Tokens.Dequeue();
                var isCollection = Tokens.Peek() != "=";
                Tokens.Undequeue(token);
                if (isCollection) return new CollectionNode { Type = typeNode, Items = ParseList("}") };
                property.PropertyName = ParseIdentifier();
                ParseToken("=");
                if (PeekToken("{"))
                {
                    Tokens.Dequeue();
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
                    property.Value = ParseExpressionNoComma();
                properties.Add(property);
                token = Tokens.Dequeue();
                if (token == "}") break;
                if (token != ",") engine.Throw("unexpected token: " + token);
                if (PeekToken("}"))
                {
                    ParseToken("}");
                    break;
                }
            }
            return new ObjectNode { Type = typeNode, Properties = properties } as ExpressionNode;
        }

        private List<ExpressionNode> ParseDictionary()
        {
            var entries = new List<ExpressionNode>();
            while (true)
            {
                ParseToken("{");
                var key = ParseExpressionNoComma();
                ParseToken(",");
                var value = ParseExpressionNoComma();
                ParseToken("}");
                entries.Add(new PairNode { Key = key, Value = value });
                var token = Tokens.Dequeue();
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
                Tokens.Dequeue();
                typeName += "." + ParseIdentifier();
            }
            var typeArgs = null as List<TypeNode>;
            if (PeekToken("<"))
            {
                Tokens.Dequeue();
                typeArgs = new List<TypeNode>();
                while (true)
                {
                    typeArgs.Add(ParseType());
                    var token = Tokens.Dequeue();
                    if (token.StartsWith(">"))
                    {
                        if (token == ">>") Tokens.Undequeue(">");
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
            return Tokens != null && Tokens.Peek() == token;
        }

        private bool PeekTokenStartsWith(string token)
        {
            return Tokens.Peek() != null && Tokens.Peek().StartsWith(token);
        }

        private string ParseToken(string token)
        {
            if (Tokens.Count == 0 || Tokens.Peek() != token) engine.Throw("missing token: " + token);
            return Tokens.Dequeue();
        }

        private string ParseIdentifier()
        {
            if (Tokens.Count == 0 || Tokens.Peek()[0] != '`') engine.Throw("expected identifier");
            return Tokens.Dequeue().Substring(1);
        }

        private string ParseVariable()
        {
            if (Tokens.Count == 0 || Tokens.Peek()[0] != '$') engine.Throw("expected variable");
            return Tokens.Dequeue();
        }

        private string ParseIdentifierOrVariable()
        {
            if (Tokens.Count == 0 || !"`$".Contains(Tokens.Peek()[0])) engine.Throw("expected variable");
            return Tokens.Dequeue();
        }

        private string PeekKeyword()
        {
            return Tokens.Peek() != null && Tokens.Peek()[0] == '`' ? Tokens.Peek().Substring(1) : Tokens.Peek();
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
            if (Tokens.Count > 0 && Tokens.Peek() == expectedToken)
            {
                Tokens.Dequeue();
                return nodes;
            }
            while (Tokens.Count > 0)
            {
                nodes.Add(ParseExpressionNoComma());
                if (Tokens.Count == 0) engine.Throw("missing token: " + expectedToken);
                var token = Tokens.Dequeue();
                if (token == expectedToken) return nodes;
                if (token != ",") engine.Throw("unexpected token: " + token);
            }
            return nodes;
        }

        private void Tokenize()
        {
            Tokens = new TokenQueue();
            for (int i = 0; i < Code.Length; )
            {
                char c = Code[i];
                string c2 = Code.Substring(i, Math.Min(2, Code.Length - i));
                if (char.IsWhiteSpace(c)) ++i;
                else if (c2 == "/*")
                    i = EatMultiLineComment(i);
                else if (c2 == "//")
                    i = EatSingleLineComment(i);
                else if (operatorMap.ContainsKey(c2) || assignmentOperatorMap.ContainsKey(c2) | c2 == "=>")
                {
                    Tokens.Enqueue(c2);
                    i += 2;
                }
                else if (operatorMap.ContainsKey(c.ToString()) || "=.[](){},?:;".Contains(c))
                {
                    Tokens.Enqueue(c.ToString());
                    ++i;
                }
                else if (IsQuote(c))
                {
                    var start = ++i;
                    for (++i; i < Code.Length && Code[i] != c; ++i) continue;
                    if (i == Code.Length) engine.Throw("missing closing quote: " + Code);
                    Tokens.Enqueue('"' + Code.Substring(start, i++ - start));
                }
                else if (char.IsDigit(c))
                {
                    var start = i;
                    for (++i; i < Code.Length && (char.IsDigit(Code[i]) || Code[i] == '.'); i++) continue;
                    Tokens.Enqueue(Code.Substring(start, i - start));
                }
                else if (c == '$' || c == '@' || IsInitialIdChar(c))
                {
                    var start = i;
                    var prefix = "";
                    if (c == '$' || c == '@')
                    {
                        ++i;
                        if (i == Code.Length || !IsInitialIdChar(Code[i]))
                        {
                            if (c == '$') engine.Throw("missing identifier");
                            if (c == '@') { Tokens.Enqueue("@"); continue; }
                        }
                    }
                    else
                        prefix = "`";
                    for (++i; i < Code.Length && IsIdChar(Code[i]); ++i) continue;
                    var identifier = prefix + Code.Substring(start, i - start);
                    if (operatorAliasMap.ContainsKey(identifier)) Tokens.Enqueue(operatorAliasMap[identifier]);
                    else Tokens.Enqueue(identifier);
                }
                else
                    engine.Throw("invalid token: " + Code.Substring(i));
            }

        }

        private int EatMultiLineComment(int i)
        {
            for (i += 2; i < Code.Length - 1 && Code.Substring(i, 2) != "*/"; i++)
                if (Code.Substring(i, 2) == "/*") i = EatMultiLineComment(i) - 1;
            return i + 2;
        }

        private int EatSingleLineComment(int i)
        {
            for (i += 2; i < Code.Length && Code[i] != ';'; i++) continue;
            return Math.Min(Code.Length, i + 1);
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
