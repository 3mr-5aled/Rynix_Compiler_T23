using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rynix_Compiler
{
    public class CompilerError
    {
        public string Lexeme { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public int Line { get; set; }
    }

    public static class Errors
    {
        public static List<string> Error_List = new List<string>();
        public static List<CompilerError> DetailedErrors = new List<CompilerError>();

        public static void AddError(string lexeme, int startIndex = -1, int length = -1, int line = 1)
        {
            Error_List.Add(lexeme);
            DetailedErrors.Add(new CompilerError
            {
                Lexeme = lexeme,
                StartIndex = startIndex,
                Length = length >= 0 ? length : (lexeme != null ? lexeme.Length : 0),
                Line = line
            });
        }

        public static void Clear()
        {
            Error_List.Clear();
            DetailedErrors.Clear();
        }
    }
}
