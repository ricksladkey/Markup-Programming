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
#if DEBUG
    [DebuggerTypeProxy(typeof(CodeTreeDebugView))]
#endif
    public class CodeTree
    {
        private Engine engine;
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
        private IDictionary<string, int> precedenceMap = new Dictionary<string, int>()
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


            { ":", 10 },
            { "?", 9 },

            { "=", 0 },
            { "+=", 0 },
            { "-=", 0 },
            { "*=", 0 },
            { "%=", 0 },
            { "/=", 0 },
            { "&=", 0 },
            { "|=", 0 },
        };

        private static string IdChars { get { return "_"; } }
        private static IDictionary<string, object> ConstantMap { get { return constantMap; } }
        private static IDictionary<string, Op> OperatorMap { get { return operatorMap; } }
        private static IDictionary<string, AssignmentOp> AssignmentOperatorMap { get { return assignmentOperatorMap; } }
        private IDictionary<string, int> PrecedenceMap { get { return precedenceMap; } }
        private bool IsCurrentCall { get { return IsCall && Tokens != null && Tokens.Count == 0 || PeekToken("("); } }

        public TokenQueue Tokens { get; set; }
        public CodeType CodeType { get; private set; }
        public bool IsVariable { get { return CodeType == CodeType.Variable; } }
        public bool IsSet { get { return CodeType == CodeType.SetExpression; } }
        public bool IsCall { get { return CodeType == CodeType.Call; } }
        public bool IsScript { get { return CodeType == CodeType.Script; } }
        public string Code { get; private set; }

        public CodeTree()
        {
        }

        public string GetVariable(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Code: Variable {0}", Code);
            return (root as VariableNode).VariableName;
        }

        public object Evaluate(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Code: Get {0}", Code);
            return (root as ExpressionNode).Evaluate(engine);
        }
        public object Set(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Code: Set {0} = {1}", Code, value);
            return (root as ExpressionNode).Set(engine, value);
        }
        public object Call(Engine engine, IEnumerable<object> args)
        {
            engine.Trace(TraceFlags.Path, "Code: Call: {0}", Code);
            var call = root as CallNode;
            if (call == null) engine.Throw("not a call node");
            return call.Call(engine, args);
        }
        public void Execute(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Code: Execute {0}", Code);
            var block = root as ScriptNode;
            block.Execute(engine);
        }

        public CodeTree Compile(Engine engine, CodeType expressionType, string path)
        {
            if (expressionType == CodeType && object.ReferenceEquals(Code, path)) return this;
            this.engine = engine;
            CodeType = expressionType;
            Code = path;
            Tokenize();
            if (IsVariable) root = ParseVariableExpression();
            else if (IsScript) root = ParseStatements();
            else root = ParseExpression();
            if (Tokens.Count > 0) engine.Throw("unexpected token: " + Tokens.Dequeue());
            this.engine = null;
            Tokens = null;
            return this;
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

        private ExpressionNode ParseExpression() { return ParseBinary(false); }
        private ExpressionNode ParseExpressionNoComma() { return ParseBinary(true); }

        private ExpressionNode ParseBinary(bool noComma)
        {
            var operators = new Stack<string>();
            var operands = new Stack<ExpressionNode>();
            operands.Push(ParseUnary());
            while (true)
            {
                var token = Tokens.Peek();
                if (token == null) break;
                if (AssignmentOperatorMap.ContainsKey(token) || OperatorMap.ContainsKey(token))
                    Tokens.Dequeue();
                else if ((token == "," && !noComma) || token == "?" || token == ":")
                    Tokens.Dequeue();
                else
                    break;
                while (operators.Count > 0 && ComparePrececence(token, operators.Peek()) <= 0)
                    PerformOperation(operators, operands);
                operators.Push(token);
                operands.Push(ParseUnary());
            }
            while (operators.Count > 0) PerformOperation(operators, operands);
            return operands.Pop();
        }

        private int ComparePrececence(string o1, string o2)
        {
            return PrecedenceMap[o1] - PrecedenceMap[o2];
        }

        private void PerformOperation(Stack<string> operators, Stack<ExpressionNode> operands)
        {
            var token = operators.Pop();
            var operand2 = operands.Pop();
            var operand1 = operands.Pop();
            if (AssignmentOperatorMap.ContainsKey(token))
                operands.Push(new SetNode { LValue = operand1, Op = AssignmentOperatorMap[token], RValue = operand2 });
            else if (OperatorMap.ContainsKey(token))
                operands.Push(new OpNode { Op = OperatorMap[token], Operands = { operand1, operand2 } });
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

        private ExpressionNode ParseUnary()
        {
            var token = Tokens.Peek();
            if (token == "+")
            {
                Tokens.Dequeue();
                return ParseUnary();
            }
            if (token == "-")
            {
                ParseToken("-");
                return new OpNode { Op = Op.Negate, Operands = { ParseUnary() } };
            }
            if (OperatorMap.ContainsKey(token) && OperatorMap[token].GetArity() == 1)
            {
                Tokens.Dequeue();
                return new OpNode { Op = OperatorMap[token], Operands = { ParseUnary() } };
            }
            if (token == "++" || token == "--")
            {
                Tokens.Dequeue();
                var op = token == "++" ? AssignmentOp.Increment : AssignmentOp.Decrement;
                return new IncrementNode { Op = op, LValue = ParseUnary() };
            }
            return ParsePrimary();
        }

        private ExpressionNode ParsePrimary()
        {
            var node = ParseAtom();
            while (true)
            {
                var token = Tokens.Peek();
                if (token == ".")
                {
                    ParseToken(".");
                    node = ParseIdentifierExpression(node);
                }
                else if (token == "[")
                    node = ParseItem(node);
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

        private ExpressionNode ParseAtom()
        {
            var token = Tokens.Peek();
            if (token == null) engine.Throw("missing expression");
            char c = token[0];
            if (char.IsDigit(c))
            {
                Tokens.Dequeue();
                return new ValueNode { Value = token.Contains('.') ? ParseDouble(token) : ParseInt(token) };
            }
            if (c == '"')
                return new ValueNode { Value = Tokens.Dequeue().Substring(1) };
            if ("`$@".Contains(c))
                return ParseIdentifierExpression(new ContextNode());
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

        private ExpressionNode ParseConditional(ExpressionNode node)
        {
            Tokens.Dequeue();
            var ifTrue = ParseExpression();
            ParseToken(":");
            var ifFalse = ParseExpression();
            return new ConditionalNode { Conditional = node, IfTrue = ifTrue, IfFalse = ifFalse };
        }

        private ExpressionNode ParseComma(ExpressionNode node)
        {
            Tokens.Dequeue();
            var value = ParseExpression();
            return new CommaNode { Operand1 = node, Operand2 = value };
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
            var token = Tokens.Peek();
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
            Tokens.Dequeue();
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

        private ExpressionNode ParseItem(ExpressionNode node)
        {
            ParseToken("[");
            var index = ParseExpressionNoComma();
            ParseToken("]");
            return new ItemNode { Context = node, Index = index };
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
            // Phase I: collect all the properties.
            var properties = new List<InitializerProperty>();
            while (true)
            {
                var property = new InitializerProperty();
                if (PeekToken("{")) return ParseDictionaryInitializer(typeNode);
                var token = Tokens.Dequeue();
                var isCollection = Tokens.Peek() != "=";
                Tokens.Undequeue(token);
                if (isCollection) return ParseCollectionInitializer(typeNode);
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
            return Tokens.Peek() == token;
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
                else if (OperatorMap.ContainsKey(c2) || AssignmentOperatorMap.ContainsKey(c2))
                {
                    Tokens.Enqueue(c2);
                    i += 2;
                }
                else if (OperatorMap.ContainsKey(c.ToString()) || "=.[](){},?:;".Contains(c))
                {
                    Tokens.Enqueue(c.ToString());
                    ++i;
                }
                else if (IsQuote(c))
                {
                    var start = ++i;
                    for (++i; i < Code.Length && Code[i] != c; ++i) continue;
                    if (Code[i] == Code.Length) engine.Throw("missing closing quote: " + Code);
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
                    Tokens.Enqueue(prefix + Code.Substring(start, i - start));
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
