using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Rynix_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public  Node root;
        
        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }

        //Program → Declarations
        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(Declarations());
            if (InputPointer < TokenStream.Count )
            {
                Errors.Error_List.Add("Parsing Error : "+ TokenStream[InputPointer].lex);
            }
            else if(Errors.Error_List.Count == 0)
            {
                MessageBox.Show("Success");
            } 
            return program;
        }

        // Declarations → ClassDecl Declarations | Statement Declarations | fun DeclTail
        Node Declarations()
        {
            Node decls = new Node("Declarations");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class current = TokenStream[InputPointer].token_type;
                if (current == Token_Class.Class)
                {
                    decls.Children.Add(ClassDecl());
                    decls.Children.Add(Declarations());
                }
                else if (current == Token_Class.Fun)
                {
                    decls.Children.Add(match(Token_Class.Fun));
                    decls.Children.Add(DeclTail());
                }
                else if (IsStatementStart(current))
                {
                    decls.Children.Add(Statement());
                    decls.Children.Add(Declarations());
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected 'fun main()', class, or declaration, found " + current + "\r\n");
                    InputPointer++;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Missing 'fun main(): Void' entry point.\r\n");
            }
            return decls;
        }

        // DeclTail → main ( ) : Void Block | identifier ( Parameters ) : ReturnType Block Declarations
        Node DeclTail()
        {
            Node declTail = new Node("DeclTail");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Main)
            {
                declTail.Children.Add(match(Token_Class.Main));
                declTail.Children.Add(match(Token_Class.LParanthesis));
                declTail.Children.Add(match(Token_Class.RParanthesis));
                declTail.Children.Add(match(Token_Class.Colon));
                declTail.Children.Add(match(Token_Class.Void));
                declTail.Children.Add(Block());
            }
            else
            {
                declTail.Children.Add(match(Token_Class.Identifier));
                declTail.Children.Add(match(Token_Class.LParanthesis));
                declTail.Children.Add(Parameters());
                declTail.Children.Add(match(Token_Class.RParanthesis));
                declTail.Children.Add(match(Token_Class.Colon));
                declTail.Children.Add(ReturnType());
                declTail.Children.Add(Block());
                declTail.Children.Add(Declarations());
            }
            return declTail;
        }

        // VarDecl → var identifier VarDeclTail | val identifier : Datatype = Expression
        Node VarDecl()
        {
            Node varDecl = new Node("VarDecl");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Var)
            {
                varDecl.Children.Add(match(Token_Class.Var));
                varDecl.Children.Add(match(Token_Class.Identifier));
                varDecl.Children.Add(VarDeclTail());
            }
            else
            {
                varDecl.Children.Add(match(Token_Class.Val));
                varDecl.Children.Add(match(Token_Class.Identifier));
                varDecl.Children.Add(match(Token_Class.Colon));
                varDecl.Children.Add(Datatype());
                varDecl.Children.Add(match(Token_Class.AssignOp));
                varDecl.Children.Add(Expression());
            }
            return varDecl;
        }

        // VarDeclTail → : Datatype VarInit | = Expression
        Node VarDeclTail()
        {
            Node varDeclTail = new Node("VarDeclTail");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Colon)
            {
                varDeclTail.Children.Add(match(Token_Class.Colon));
                varDeclTail.Children.Add(Datatype());
                varDeclTail.Children.Add(VarInit());
            }
            else
            {
                varDeclTail.Children.Add(match(Token_Class.AssignOp));
                varDeclTail.Children.Add(Expression());
            }
            return varDeclTail;
        }

        // VarInit → = Expression | ε
        Node VarInit()
        {
            Node varInit = new Node("VarInit");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.AssignOp)
            {
                varInit.Children.Add(match(Token_Class.AssignOp));
                varInit.Children.Add(Expression());
            }
            return varInit;
        }

        // Datatype → Int | Float | String | Bool | Char | Array < Datatype > | identifier
        Node Datatype()
        {
            Node dataType = new Node("Datatype");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.Int || t == Token_Class.Float || t == Token_Class.String ||
                    t == Token_Class.Bool || t == Token_Class.Char || t == Token_Class.Identifier)
                {
                    dataType.Children.Add(match(t));
                }
                else if (t == Token_Class.Array)
                {
                    dataType.Children.Add(match(Token_Class.Array));
                    dataType.Children.Add(match(Token_Class.LessThanOp));
                    dataType.Children.Add(Datatype());
                    dataType.Children.Add(match(Token_Class.GreaterThanOp));
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected Datatype, found " + t + "\r\n");
                    InputPointer++;
                }
            }
            return dataType;
        }

        // ReturnType → Datatype | Void
        Node ReturnType()
        {
            Node returnType = new Node("ReturnType");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Void)
            {
                returnType.Children.Add(match(Token_Class.Void));
            }
            else
            {
                returnType.Children.Add(Datatype());
            }
            return returnType;
        }

        // Parameters → Parameter Parameters' | ε
        Node Parameters()
        {
            Node parameters = new Node("Parameters");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                parameters.Children.Add(Parameter());
                parameters.Children.Add(ParametersPrime());
            }
            return parameters;
        }

        //Parameters' → , Parameter Parameters' | ε
        Node ParametersPrime()
        {
            Node parametersPrime = new Node("Parameters'");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                parametersPrime.Children.Add(match(Token_Class.Comma));
                parametersPrime.Children.Add(Parameter());
                parametersPrime.Children.Add(ParametersPrime());
            }
            return parametersPrime;
        }

        //Parameter → identifier : Datatype
        Node Parameter()
        {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(match(Token_Class.Identifier));
            parameter.Children.Add(match(Token_Class.Colon));
            parameter.Children.Add(Datatype());
            return parameter;
        }

        // ClassDecl → class identifier { ClassMembers }
        Node ClassDecl()
        {
            Node classDecl = new Node("ClassDecl");
            classDecl.Children.Add(match(Token_Class.Class));
            classDecl.Children.Add(match(Token_Class.Identifier));
            classDecl.Children.Add(match(Token_Class.LBrace));
            classDecl.Children.Add(ClassMembers());
            classDecl.Children.Add(match(Token_Class.RBrace));
            return classDecl;
        }

        // ClassMembers → ClassMember ClassMembers'
        Node ClassMembers()
        {
            Node classMembers = new Node("ClassMembers");
            classMembers.Children.Add(ClassMember());
            classMembers.Children.Add(ClassMembersPrime());
            return classMembers;
        }
        //ClassMembers' → ClassMember ClassMembers' | ε
        Node ClassMembersPrime()
        {
            Node classMembersPrime = new Node("ClassMembers'");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.Var || t == Token_Class.Val || t == Token_Class.Fun || t == Token_Class.Init)
                {
                    classMembersPrime.Children.Add(ClassMember());
                    classMembersPrime.Children.Add(ClassMembersPrime());
                }
            }
            return classMembersPrime;
        }

        // ClassMember → VarDecl | FunctionDecl | Constructor
        Node ClassMember()
        {
            Node classMember = new Node("ClassMember");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.Var || t == Token_Class.Val)
                    classMember.Children.Add(VarDecl());
                else if (t == Token_Class.Fun)
                    classMember.Children.Add(FunctionDecl());
                else if (t == Token_Class.Init)
                    classMember.Children.Add(Constructor());
                else
                {
                    Errors.Error_List.Add("Parsing Error: Invalid class member starting with " + t + "\r\n");
                    InputPointer++;
                }
            }
            return classMember;
        }

        //FunctionDecl → fun identifier ( Parameters ) : ReturnType Block
        Node FunctionDecl()
        {
            Node functionDecl = new Node("FunctionDecl");
            functionDecl.Children.Add(match(Token_Class.Fun));
            functionDecl.Children.Add(match(Token_Class.Identifier));
            functionDecl.Children.Add(match(Token_Class.LParanthesis));
            functionDecl.Children.Add(Parameters());
            functionDecl.Children.Add(match(Token_Class.RParanthesis));
            functionDecl.Children.Add(match(Token_Class.Colon));
            functionDecl.Children.Add(ReturnType());
            functionDecl.Children.Add(Block());
            return functionDecl;
        }
        //Constructor → init ( Parameters ) Block
        Node Constructor()
        {
            Node constructor = new Node("Constructor");
            constructor.Children.Add(match(Token_Class.Init));
            constructor.Children.Add(match(Token_Class.LParanthesis));
            constructor.Children.Add(Parameters());
            constructor.Children.Add(match(Token_Class.RParanthesis));
            constructor.Children.Add(Block());
            return constructor;
        }

        // Block → { Statements }
        Node Block()
        {
            Node block = new Node("Block");
            block.Children.Add(match(Token_Class.LBrace));
            block.Children.Add(Statements());
            block.Children.Add(match(Token_Class.RBrace));
            return block;
        }

        // isStatementStart?
        bool IsStatementStart(Token_Class t)
        {
            return t == Token_Class.Var || t == Token_Class.Val || t == Token_Class.Identifier ||
                   t == Token_Class.If || t == Token_Class.While || t == Token_Class.For ||
                   t == Token_Class.Return || t == Token_Class.Print || t == Token_Class.Break ||
                   t == Token_Class.Continue || t == Token_Class.Input;
        }

        //Statements  → Statement Statements' | ε
        Node Statements()
        {
            Node statements = new Node("Statements");
            if (InputPointer < TokenStream.Count && IsStatementStart(TokenStream[InputPointer].token_type))
            {
                statements.Children.Add(Statement());
                statements.Children.Add(StatementsPrime());
            }
            return statements;
        }

        //Statements' → Statement Statements' | ε
        Node StatementsPrime()
        {
            Node statementsPrime = new Node("Statements'");
            if (InputPointer < TokenStream.Count && IsStatementStart(TokenStream[InputPointer].token_type))
            {
                statementsPrime.Children.Add(Statement());
                statementsPrime.Children.Add(StatementsPrime());
            }
            return statementsPrime;
        }

        //Statement   → VarDecl | identifier LValueChain | IfStatement | WhileStatement | ForStatement | ReturnStatement | PrintStatement | BreakStatement | ContinueStatement | InputStatement
        Node Statement()
        {
            Node statement = new Node("Statement");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.Var || t == Token_Class.Val)
                    statement.Children.Add(VarDecl());
                else if (t == Token_Class.Identifier)
                {
                    statement.Children.Add(match(Token_Class.Identifier));
                    statement.Children.Add(LValueChain());
                }
                else if (t == Token_Class.If)
                    statement.Children.Add(IfStatement());
                else if (t == Token_Class.While)
                    statement.Children.Add(WhileStatement());
                else if (t == Token_Class.For)
                    statement.Children.Add(ForStatement());
                else if (t == Token_Class.Return)
                    statement.Children.Add(ReturnStatement());
                else if (t == Token_Class.Print)
                    statement.Children.Add(PrintStatement());
                else if (t == Token_Class.Break)
                    statement.Children.Add(BreakStatement());
                else if (t == Token_Class.Continue)
                    statement.Children.Add(ContinueStatement());
                else if (t == Token_Class.Input)
                    statement.Children.Add(InputStatement());
            }
            return statement;
        }

        // LValueChain → . identifier LValueChain | [ Expression ] LValueChain | = Expression | ( ArgList ) CallChain
        Node LValueChain()
        {
            Node lValueChain = new Node("LValueChain");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.Dot)
                {
                    lValueChain.Children.Add(match(Token_Class.Dot));
                    lValueChain.Children.Add(match(Token_Class.Identifier));
                    lValueChain.Children.Add(LValueChain());
                }
                else if (t == Token_Class.LBracket)
                {
                    lValueChain.Children.Add(match(Token_Class.LBracket));
                    lValueChain.Children.Add(Expression());
                    lValueChain.Children.Add(match(Token_Class.RBracket));
                    lValueChain.Children.Add(LValueChain());
                }
                else if (t == Token_Class.AssignOp)
                {
                    lValueChain.Children.Add(match(Token_Class.AssignOp));
                    lValueChain.Children.Add(Expression());
                }
                else if (t == Token_Class.LParanthesis)
                {
                    lValueChain.Children.Add(match(Token_Class.LParanthesis));
                    lValueChain.Children.Add(ArgList());
                    lValueChain.Children.Add(match(Token_Class.RParanthesis));
                    lValueChain.Children.Add(CallChain());
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected '.', '[', '=', or '(' after identifier, found " + t + "\r\n");
                    InputPointer++;
                }
            }
            return lValueChain;
        }

        //CallChain   → . identifier LValueChain| [Expression] LValueChain| (ArgList ) CallChain| ε
        Node CallChain()
        {
            Node callChain = new Node("CallChain");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.Dot)
                {
                    callChain.Children.Add(match(Token_Class.Dot));
                    callChain.Children.Add(match(Token_Class.Identifier));
                    callChain.Children.Add(LValueChain());
                }
                else if (t == Token_Class.LBracket)
                {
                    callChain.Children.Add(match(Token_Class.LBracket));
                    callChain.Children.Add(Expression());
                    callChain.Children.Add(match(Token_Class.RBracket));
                    callChain.Children.Add(LValueChain());
                }
                else if (t == Token_Class.LParanthesis)
                {
                    callChain.Children.Add(match(Token_Class.LParanthesis));
                    callChain.Children.Add(ArgList());
                    callChain.Children.Add(match(Token_Class.RParanthesis));
                    callChain.Children.Add(CallChain());
                }
            }
            return callChain;
        }

        //BreakStatement    → break
        Node BreakStatement()
        {
            Node breakStatement = new Node("BreakStatement");
            Token_Class t = TokenStream[InputPointer].token_type;
            if (t == Token_Class.Break)
            {
                breakStatement.Children.Add(match(Token_Class.Break));
            }
            return breakStatement;
        }

        //ContinueStatement → continue
        Node ContinueStatement()
        {
            Node continueStatement = new Node("ContinueStatement");
            Token_Class t = TokenStream[InputPointer].token_type;
            if (t == Token_Class.Continue)
            {
                continueStatement.Children.Add(match(Token_Class.Continue));
            }
            return continueStatement;
        }

        //InputStatement → input ( )
        Node InputStatement()
        {
            Node inputStatement = new Node("InputStatement");
            Token_Class t = TokenStream[InputPointer].token_type;
            if (t == Token_Class.Input)
            {
                inputStatement.Children.Add(match(Token_Class.Input));
                inputStatement.Children.Add(match(Token_Class.LParanthesis));
                inputStatement.Children.Add(match(Token_Class.RParanthesis));
            }
            return inputStatement;
        }

        //IfStatement  → if Condition Block IfStatement'
        Node IfStatement()
        {
            Node ifStatement = new Node("IfStatement");
            ifStatement.Children.Add(match(Token_Class.If));
            ifStatement.Children.Add(Condition());
            ifStatement.Children.Add(Block());
            ifStatement.Children.Add(IfStatementPrime());
            return ifStatement;
        }

        //IfStatement' → else Block | ε
        Node IfStatementPrime()
        {
            Node ifStatementPrime = new Node("IfStatement'");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                ifStatementPrime.Children.Add(match(Token_Class.Else));
                ifStatementPrime.Children.Add(Block());
            }
            return ifStatementPrime;
        }

        //WhileStatement → while Condition Block
        Node WhileStatement()
        {
            Node whileStatement = new Node("WhileStatement");
            whileStatement.Children.Add(match(Token_Class.While));
            whileStatement.Children.Add(Condition());
            whileStatement.Children.Add(Block());
            return whileStatement;
        }

        //ForStatement→ for identifier in ForIterable Block
        Node ForStatement()
        {
            Node forStatement = new Node("ForStatement");
            forStatement.Children.Add(match(Token_Class.For));
            forStatement.Children.Add(match(Token_Class.Identifier));
            forStatement.Children.Add(match(Token_Class.In));
            forStatement.Children.Add(ForIterable());
            forStatement.Children.Add(Block());
            return forStatement;
        }

        //ForIterable → Expression RangeExpression
        Node ForIterable()
        {
            Node forIterable = new Node("ForIterable");
            forIterable.Children.Add(Expression());
            forIterable.Children.Add(RangeExpression());
            return forIterable;
        }

        //RangeExpression   → .. Expression | ε
        Node RangeExpression()
        {
            Node rangeExpression = new Node("RangeExpression");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.RangeOp)
            {
                rangeExpression.Children.Add(match(Token_Class.RangeOp));
                rangeExpression.Children.Add(Expression());
            }
            return rangeExpression;
        }

        //ReturnStatement → return Expression
        Node ReturnStatement()
        {
            Node returnStatement = new Node("ReturnStatement");
            returnStatement.Children.Add(match(Token_Class.Return));
            returnStatement.Children.Add(Expression());
            return returnStatement;
        }

        //PrintStatement → print ( Expression )
        Node PrintStatement()
        {
            Node printStatement = new Node("PrintStatement");
            printStatement.Children.Add(match(Token_Class.Print));
            printStatement.Children.Add(match(Token_Class.LParanthesis));
            printStatement.Children.Add(Expression());
            printStatement.Children.Add(match(Token_Class.RParanthesis));
            return printStatement;
        }

        //Condition → Expression RelClause
        Node Condition()
        {
            Node condition = new Node("Condition");
            condition.Children.Add(Expression());
            condition.Children.Add(RelClause());
            return condition;
        }

        //RelClause → RelOp Expression | ε
        Node RelClause()
        {
            Node relClause = new Node("RelClause");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.EqualOp || t == Token_Class.NotEqualOp ||
                    t == Token_Class.GreaterThanOp || t == Token_Class.LessThanOp ||
                    t == Token_Class.GreaterEqualOp || t == Token_Class.LessEqualOp)
                {
                    relClause.Children.Add(RelOp());
                    relClause.Children.Add(Expression());
                }
            }
            return relClause;
        }

        //RelOp → == | != | > | < | >= | <=
        Node RelOp()
        {
            Node relOp = new Node("RelOp");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.EqualOp || t == Token_Class.NotEqualOp ||
                    t == Token_Class.GreaterThanOp || t == Token_Class.LessThanOp ||
                    t == Token_Class.GreaterEqualOp || t == Token_Class.LessEqualOp)
                {
                    relOp.Children.Add(match(t));
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected Relational Operator, found " + t + "\r\n");
                    InputPointer++;
                }
            }
            return relOp;
        }

        //Expression → Term Exp
        Node Expression()
        {
            Node expression = new Node("Expression");
            expression.Children.Add(Term());
            expression.Children.Add(Exp());
            return expression;
        }

        //Exp → AddOp Term Exp | ε
        Node Exp()
        {
            Node exp = new Node("Exp");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.PlusOp || t == Token_Class.MinusOp)
                {
                    exp.Children.Add(AddOp());
                    exp.Children.Add(Term());
                    exp.Children.Add(Exp());
                }
            }
            return exp;
        }

        // AddOp → + | -
        Node AddOp()
        {
            Node addOp = new Node("AddOp");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.PlusOp || t == Token_Class.MinusOp)
                {
                    addOp.Children.Add(match(t));
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected '+' or '-', found " + t + "\r\n");
                    InputPointer++;
                }
            }
            return addOp;
        }

        //Term → Factor Ter
        Node Term()
        {
            Node term = new Node("Term");
            term.Children.Add(Factor());
            term.Children.Add(Ter());
            return term;
        }

        //Ter  → MultOp Factor Ter | ε
        Node Ter()
        {
            Node ter = new Node("Ter");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.MultiplyOp || t == Token_Class.DivideOp || t == Token_Class.ModOp)
                {
                    ter.Children.Add(MultOp());
                    ter.Children.Add(Factor());
                    ter.Children.Add(Ter());
                }
            }
            return ter;
        }

        //MultOp → * | / | %
        Node MultOp()
        {
            Node multOp = new Node("MultOp");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.MultiplyOp || t == Token_Class.DivideOp || t == Token_Class.ModOp)
                {
                    multOp.Children.Add(match(t));
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected '*', '/', or '%', found " + t + "\r\n");
                    InputPointer++;
                }
            }
            return multOp;
        }

        //Factor     → Literal| - Number| identifier FactorTail| new identifier(ArgList )| input()| [Elements]
        Node Factor()
        {
            Node factor = new Node("Factor");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.IntegerLiteral || t == Token_Class.FloatLiteral ||
                    t == Token_Class.StringLiteral || t == Token_Class.CharLiteral ||
                    t == Token_Class.True || t == Token_Class.False || t == Token_Class.Null)
                {
                    factor.Children.Add(Literal());
                }
                else if (t == Token_Class.MinusOp)
                {
                    factor.Children.Add(match(Token_Class.MinusOp));
                    factor.Children.Add(Number());
                }
                else if (t == Token_Class.Identifier)
                {
                    factor.Children.Add(match(Token_Class.Identifier));
                    factor.Children.Add(FactorTail());
                }
                else if (t == Token_Class.New)
                {
                    factor.Children.Add(match(Token_Class.New));
                    factor.Children.Add(match(Token_Class.Identifier));
                    factor.Children.Add(match(Token_Class.LParanthesis));
                    factor.Children.Add(ArgList());
                    factor.Children.Add(match(Token_Class.RParanthesis));
                }
                else if (t == Token_Class.Input)
                {
                    factor.Children.Add(match(Token_Class.Input));
                    factor.Children.Add(match(Token_Class.LParanthesis));
                    factor.Children.Add(match(Token_Class.RParanthesis));
                }
                else if (t == Token_Class.LBracket)
                {
                    factor.Children.Add(match(Token_Class.LBracket));
                    factor.Children.Add(Elements());
                    factor.Children.Add(match(Token_Class.RBracket));
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Unexpected token in Factor: " + t + "\r\n");
                    InputPointer++;
                }
            }
            return factor;
        }

        // Literal → integer_literal | float_literal | StringLiteral | CharLiteral | true | false | null
        Node Literal()
        {
            Node literal = new Node("Literal");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.IntegerLiteral || t == Token_Class.FloatLiteral ||
                    t == Token_Class.StringLiteral || t == Token_Class.CharLiteral ||
                    t == Token_Class.True || t == Token_Class.False || t == Token_Class.Null)
                {
                    literal.Children.Add(match(t));
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected Literal, found " + t + "\r\n");
                    InputPointer++;
                }
            }
            return literal;
        }

        // Number → integer_literal | float_literal
        Node Number()
        {
            Node number = new Node("Number");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.IntegerLiteral || t == Token_Class.FloatLiteral)
                {
                    number.Children.Add(match(t));
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected Integer or Float after '-', found " + t + "\r\n");
                    InputPointer++;
                }
            }
            return number;
        }

        //FactorTail → LValue' CallClause
        Node FactorTail()
        {
            Node factorTail = new Node("FactorTail");
            factorTail.Children.Add(LValuePrime());
            factorTail.Children.Add(CallClause());
            return factorTail;
        }

        //LValue  → identifier LValue'
        Node LValue()
        {
            Node lValue = new Node("LValue");
            lValue.Children.Add(match(Token_Class.Identifier));
            lValue.Children.Add(LValuePrime());
            return lValue;
        }

        //LValue' → . identifier LValue'| [Expression] LValue'| ε
        Node LValuePrime()
        {
            Node lvPrime = new Node("LValue'");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.Dot)
                {
                    lvPrime.Children.Add(match(Token_Class.Dot));
                    lvPrime.Children.Add(match(Token_Class.Identifier));
                    lvPrime.Children.Add(LValuePrime());
                }
                else if (t == Token_Class.LBracket)
                {
                    lvPrime.Children.Add(match(Token_Class.LBracket));
                    lvPrime.Children.Add(Expression());
                    lvPrime.Children.Add(match(Token_Class.RBracket));
                    lvPrime.Children.Add(LValuePrime());
                }
            }
            return lvPrime;
        }

        // CallClause → ( ArgList ) | ε
        Node CallClause()
        {
            Node callClause = new Node("CallClause");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                callClause.Children.Add(match(Token_Class.LParanthesis));
                callClause.Children.Add(ArgList());
                callClause.Children.Add(match(Token_Class.RParanthesis));
            }
            return callClause;
        }

        //Elements → Expression Elem | ε
        Node Elements()
        {
            Node elements = new Node("Elements");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.RBracket)
            {
                elements.Children.Add(Expression());
                elements.Children.Add(Elem());
            }
            return elements;
        }

        //Elem → , Expression Elem | ε
        Node Elem()
        {
            Node elem = new Node("Elem");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                elem.Children.Add(match(Token_Class.Comma));
                elem.Children.Add(Expression());
                elem.Children.Add(Elem());
            }
            return elem;
        }

        //ArgList → Arguments | ε
        Node ArgList()
        {
            Node argList = new Node("ArgList");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.RParanthesis)
            {
                argList.Children.Add(Arguments());
            }
            return argList;
        }

        //Arguments → Expression Arg
        Node Arguments()
        {
            Node arguments = new Node("Arguments");
            arguments.Children.Add(Expression());
            arguments.Children.Add(Arg());
            return arguments;
        }

        //Arg → , Expression Arg | ε
        Node Arg()
        {
            Node arg = new Node("Arg");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                arg.Children.Add(match(Token_Class.Comma));
                arg.Children.Add(Expression());
                arg.Children.Add(Arg());
            }
            return arg;
        }








        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString()  + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
