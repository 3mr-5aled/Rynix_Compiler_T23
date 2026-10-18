using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    // Reserved Words (Keywords)
    Var, Val, Fun, Class, If, Else, While, For, In, Return,
    True, False, Null, New, Print, Input, Break, Continue, Init,

    // Data Types
    Int, Float, String, Bool, Char, Array, Void,

    // Delimiters & Punctuation
    Dot, Semicolon, Colon, Comma,
    LParanthesis, RParanthesis,
    LBrace, RBrace,
    LBracket, RBracket,

    // Operators
    EqualOp, NotEqualOp, LessThanOp, GreaterThanOp,
    LessEqualOp, GreaterEqualOp,
    PlusOp, MinusOp, MultiplyOp, DivideOp, ModOp,
    AssignOp, RangeOp,

    // Identifiers & Literals
    Identifier,
    Constant,
    StringLiteral,
    CharLiteral
}

namespace Rynix_Compiler
{
    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            // Reserved Words (Keywords)
            ReservedWords.Add("var", Token_Class.Var);
            ReservedWords.Add("val", Token_Class.Val);
            ReservedWords.Add("fun", Token_Class.Fun);
            ReservedWords.Add("class", Token_Class.Class);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("while", Token_Class.While);
            ReservedWords.Add("for", Token_Class.For);
            ReservedWords.Add("in", Token_Class.In);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("true", Token_Class.True);
            ReservedWords.Add("false", Token_Class.False);
            ReservedWords.Add("null", Token_Class.Null);
            ReservedWords.Add("new", Token_Class.New);
            ReservedWords.Add("print", Token_Class.Print);
            ReservedWords.Add("input", Token_Class.Input);
            ReservedWords.Add("break", Token_Class.Break);
            ReservedWords.Add("continue", Token_Class.Continue);
            ReservedWords.Add("init", Token_Class.Init);

            // Data Types
            ReservedWords.Add("Int", Token_Class.Int);
            ReservedWords.Add("Float", Token_Class.Float);
            ReservedWords.Add("String", Token_Class.String);
            ReservedWords.Add("Bool", Token_Class.Bool);
            ReservedWords.Add("Char", Token_Class.Char);
            ReservedWords.Add("Array", Token_Class.Array);
            ReservedWords.Add("Void", Token_Class.Void);

            // Delimiters
            Operators.Add(".", Token_Class.Dot);
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(":", Token_Class.Colon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LBrace);
            Operators.Add("}", Token_Class.RBrace);
            Operators.Add("[", Token_Class.LBracket);
            Operators.Add("]", Token_Class.RBracket);

            // Operators
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("%", Token_Class.ModOp);
            Operators.Add("=", Token_Class.AssignOp);
            Operators.Add("==", Token_Class.EqualOp);
            Operators.Add("!=", Token_Class.NotEqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<=", Token_Class.LessEqualOp);
            Operators.Add(">=", Token_Class.GreaterEqualOp);
            Operators.Add("..", Token_Class.RangeOp);
        }

        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();

            for (int i = 0; i < SourceCode.Length; i++)
            {
                char CurrentChar = SourceCode[i];

                // 1. Whitespace
                if (CurrentChar == ' ' || CurrentChar == '\t' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                // 2. Comments
                if (CurrentChar == '/')
                {
                    // Single-line comment: // ...
                    if (i + 1 < SourceCode.Length && SourceCode[i + 1] == '/')
                    {
                        int j = i + 2;
                        while (j < SourceCode.Length && SourceCode[j] != '\r' && SourceCode[j] != '\n')
                        {
                            j++;
                        }
                        i = j - 1;
                        continue;
                    }
                    // Multi-line comment: /* ... */
                    else if (i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                    {
                        int j = i + 2;
                        bool closed = false;
                        while (j + 1 < SourceCode.Length)
                        {
                            if (SourceCode[j] == '*' && SourceCode[j + 1] == '/')
                            {
                                closed = true;
                                j += 2;
                                break;
                            }
                            j++;
                        }
                        if (!closed)
                        {
                            string unclosedComment = SourceCode.Substring(i);
                            Errors.Error_List.Add(unclosedComment);
                            i = SourceCode.Length;
                            break;
                        }
                        i = j - 1;
                        continue;
                    }
                }

                // 3. String Literal: "..."
                if (CurrentChar == '\"')
                {
                    int j = i + 1;
                    bool closed = false;
                    while (j < SourceCode.Length)
                    {
                        if (SourceCode[j] == '\r' || SourceCode[j] == '\n')
                        {
                            break;
                        }
                        if (SourceCode[j] == '\"')
                        {
                            closed = true;
                            j++;
                            break;
                        }
                        j++;
                    }
                    string currentLexeme = SourceCode.Substring(i, j - i);
                    if (!closed)
                    {
                        Errors.Error_List.Add(currentLexeme);
                    }
                    else
                    {
                        FindTokenClass(currentLexeme);
                    }
                    i = j - 1;
                    continue;
                }

                // 4. Character Literal: '...'
                if (CurrentChar == '\'')
                {
                    int j = i + 1;
                    bool closed = false;
                    while (j < SourceCode.Length)
                    {
                        if (SourceCode[j] == '\r' || SourceCode[j] == '\n')
                        {
                            break;
                        }
                        if (SourceCode[j] == '\'')
                        {
                            closed = true;
                            j++;
                            break;
                        }
                        j++;
                    }
                    string currentLexeme = SourceCode.Substring(i, j - i);
                    if (!closed || (currentLexeme.Length != 3 && !(currentLexeme.Length == 4 && currentLexeme[1] == '\\')))
                    {
                        Errors.Error_List.Add(currentLexeme);
                    }
                    else
                    {
                        FindTokenClass(currentLexeme);
                    }
                    i = j - 1;
                    continue;
                }

                // 5. Identifiers or Keywords (begins with letter or underscore)
                if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                {
                    int j = i;
                    while (j < SourceCode.Length && (char.IsLetterOrDigit(SourceCode[j]) || SourceCode[j] == '_'))
                    {
                        j++;
                    }
                    string currentLexeme = SourceCode.Substring(i, j - i);
                    FindTokenClass(currentLexeme);
                    i = j - 1;
                    continue;
                }

                // 6. Numbers (Integer / Float / Invalid tokens starting with digit)
                if (char.IsDigit(CurrentChar))
                {
                    int j = i;
                    while (j < SourceCode.Length && char.IsDigit(SourceCode[j]))
                    {
                        j++;
                    }

                    // Check for decimal point vs range operator '..'
                    if (j < SourceCode.Length && SourceCode[j] == '.')
                    {
                        // If followed by another dot, it's the range operator '..' -> stop scanning number
                        if (j + 1 < SourceCode.Length && SourceCode[j + 1] == '.')
                        {
                            // Do not consume '.'
                        }
                        else if (j + 1 < SourceCode.Length && char.IsDigit(SourceCode[j + 1]))
                        {
                            // Float constant: consume '.' and fractional digits
                            j++;
                            while (j < SourceCode.Length && char.IsDigit(SourceCode[j]))
                            {
                                j++;
                            }
                        }
                    }

                    // Check if letters or underscores or extra dots are attached (e.g. 1user, 12a, 1.2.3)
                    if (j < SourceCode.Length && (char.IsLetter(SourceCode[j]) || SourceCode[j] == '_' || SourceCode[j] == '.'))
                    {
                        while (j < SourceCode.Length && (char.IsLetterOrDigit(SourceCode[j]) || SourceCode[j] == '_' || SourceCode[j] == '.'))
                        {
                            // Stop if range operator '..' is encountered
                            if (SourceCode[j] == '.' && j + 1 < SourceCode.Length && SourceCode[j + 1] == '.')
                                break;
                            j++;
                        }
                    }

                    string currentLexeme = SourceCode.Substring(i, j - i);
                    FindTokenClass(currentLexeme);
                    i = j - 1;
                    continue;
                }

                // 7. Operators and Delimiters
                // Check 2-character operators first (==, !=, <=, >=, ..)
                if (i + 1 < SourceCode.Length)
                {
                    string twoChar = SourceCode.Substring(i, 2);
                    if (Operators.ContainsKey(twoChar))
                    {
                        FindTokenClass(twoChar);
                        i++;
                        continue;
                    }
                }

                // Check 1-character operators and delimiters
                string oneChar = CurrentChar.ToString();
                if (Operators.ContainsKey(oneChar))
                {
                    FindTokenClass(oneChar);
                    continue;
                }
                // int = 0
                // 8. Undefined / Invalid character
                FindTokenClass(oneChar);
            }

            Rynix_Compiler.TokenStream = Tokens;
        }

        void FindTokenClass(string Lex)
        {
            Token Tok = new Token();
            Tok.lex = Lex;

            // Is it a reserved word?
            if (ReservedWords.ContainsKey(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
            }
            // Is it an operator?
            else if (Operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }
            // Is it an identifier?
            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Identifier;
                Tokens.Add(Tok);
            }
            // Is it a Constant?
            else if (isConstant(Lex))
            {
                Tok.token_type = Token_Class.Constant;
                Tokens.Add(Tok);
            }
            // Is it a String Literal?
            else if (isStringLiteral(Lex))
            {
                Tok.token_type = Token_Class.StringLiteral;
                Tokens.Add(Tok);
            }
            // Is it a Character Literal?
            else if (isCharLiteral(Lex))
            {
                Tok.token_type = Token_Class.CharLiteral;
                Tokens.Add(Tok);
            }
            // Is it an undefined?
            else
            {
                Errors.Error_List.Add(Lex);
            }
        }

        bool isIdentifier(string lex)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(lex)) return false;
            // Reserved words cannot be used as identifiers
            if (ReservedWords.ContainsKey(lex)) return false;

            // Must begin with a letter or underscore, followed by letters, digits, or underscores
            Regex rx = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
            isValid = rx.IsMatch(lex);
            return isValid;
        }

        bool isConstant(string lex)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(lex)) return false;

            // Integer or Floating-point number (optional negative sign)
            Regex rx = new Regex(@"^-?[0-9]+(\.[0-9]+)?$");
            isValid = rx.IsMatch(lex);
            return isValid;
        }

        bool isStringLiteral(string lex)
        {
            if (string.IsNullOrEmpty(lex)) return false;
            if (lex.Length >= 2 && lex.StartsWith("\"") && lex.EndsWith("\""))
                return true;
            return false;
        }

        bool isCharLiteral(string lex)
        {
            if (string.IsNullOrEmpty(lex)) return false;
            if (lex.Length >= 3 && lex.StartsWith("'") && lex.EndsWith("'"))
            {
                if (lex.Length == 3) return true;
                if (lex.Length == 4 && lex[1] == '\\') return true;
            }
            return false;
        }
    }
}

namespace JASON_Compiler
{
    public class Token : Rynix_Compiler.Token { }
    public class Scanner : Rynix_Compiler.Scanner { }
}
