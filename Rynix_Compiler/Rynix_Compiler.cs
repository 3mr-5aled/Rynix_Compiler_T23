using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rynix_Compiler
{
    public static class Rynix_Compiler
    {
        public static Scanner Rynix_Scanner = new Scanner();
        public static Parser Rynix_Parser = new Parser();
        public static List<Token> TokenStream = new List<Token>();
        public static Node treeroot;

        public static void Start_Compiling(string SourceCode) //character by character
        {
            //Scanner
            Rynix_Scanner.StartScanning(SourceCode);
            //Parser
            Rynix_Parser.StartParsing(TokenStream);
            treeroot = Rynix_Parser.root;
        } 
    }
}
