using Antlr4.Runtime;
using Lab1.Core.Parsing;

namespace Lab1.Core;

public static class ExpressionParser
{
    public static Node Parse(string text)
    {
        var lexer = new ExprLexer(new AntlrInputStream(text));
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new LexerErrors());

        var parser = new ExprParser(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ParserErrors());

        var tree = parser.compileUnit();
        return new AstBuilder().Visit(tree);
    }

    private sealed class LexerErrors : IAntlrErrorListener<int>
    {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol,
            int line, int charPositionInLine, string msg, RecognitionException e)
            => throw new FormulaException(
                "Недопустимий символ або невідомий ідентифікатор (очікувалось ім'я клітинки, наприклад A1)",
                charPositionInLine, 1, true);
    }

    private sealed class ParserErrors : BaseErrorListener
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            if (offendingSymbol.Type == TokenConstants.EOF)
                throw new FormulaException(
                    "Неочікуваний кінець виразу (не вистачає операнда або закриваючої дужки)",
                    charPositionInLine, 1, true);

            throw new FormulaException(
                $"Неочікуваний елемент '{offendingSymbol.Text}': перевірте операції, операнди та дужки",
                charPositionInLine, offendingSymbol.Text?.Length ?? 1, true);
        }
    }
}
