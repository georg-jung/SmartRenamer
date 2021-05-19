using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Sprache;

namespace SmartRenamer.Core.Parser
{
    internal static class ProfileGrammar
    {
        private static readonly Parser<ConstToken> ConstToken =
            (from value in Parse.CharExcept(c => char.IsWhiteSpace(c) || Path.GetInvalidPathChars().Contains(c) || new[] { '<', '>' }.Contains(c), "Whitespace or invalid path characters").AtLeastOnce().Text()
             select new ConstToken(value)).Named("Constant part of filename");

        private static readonly Parser<ConstToken> ConstLegalSpaceToken =
            (from value in Parse.Char(' ').AtLeastOnce().Text()
             select new ConstToken(value)).Named("Space(s) that are part of the filename");

        private static readonly Parser<Identifier> Identifier =
            Parse.Identifier(Parse.Letter.XOr(Parse.Char('_')), Parse.LetterOrDigit.XOr(Parse.Char('_'))).Select(x => new Identifier(x));

        private static readonly Parser<IEnumerable<Expression>> ExpressionList =
            Parse.Ref(() => Expression).Token().DelimitedBy(Parse.Char(','));

        private static readonly Parser<Indexer> Indexer =
            from open in Parse.Char('[')
            from idcs in ExpressionList
            from close in Parse.Char(']')
            select new Indexer(idcs.ToList());

        private static readonly Parser<ISymbolExpressionPart> TrailingSymbolStep =
            Parse.Char('.').Then(_ => Identifier)
            .XOr<ISymbolExpressionPart>(Indexer);

        private static readonly Parser<SymbolExpression> Symbol =
            from id in Identifier
            from rest in TrailingSymbolStep.Many()
            select new SymbolExpression(id, rest.ToList());

        private static readonly Parser<CallExpression> Call =
            from id in Identifier
            from open in Parse.Char('(')
            from args in ExpressionList
            from close in Parse.Char(')')
            select new CallExpression(id.Value, args.ToList());

        #region "String literal parsing"
                // see https://thomaslevesque.com/2017/02/23/easy-text-parsing-in-c-with-sprache/
        private static readonly Parser<char> DoubleQuote = Parse.Char('"');
        private static readonly Parser<char> Backslash = Parse.Char('\\');

        private static readonly Parser<char> QdText =
            Parse.AnyChar.Except(DoubleQuote);

        private static readonly Parser<char> QuotedPair =
            from _ in Backslash
            from c in Parse.AnyChar
            select c;

        private static readonly Parser<string> QuotedString =
            from open in DoubleQuote
            from text in QuotedPair.Or(QdText).Many().Text()
            from close in DoubleQuote
            select text;
        #endregion

        private static readonly Parser<LiteralExpression<string>> StringLiteral =
            QuotedString.Select(x => new LiteralExpression<string>(x));

        private static readonly Parser<LiteralExpression<int>> IntLiteral =
            Parse.Number.Select(x => new LiteralExpression<int>(int.Parse(x)));

        private static readonly Parser<LiteralExpression<double>> DoubleLiteral =
            Parse.DecimalInvariant.Select(x => new LiteralExpression<double>(double.Parse(x, CultureInfo.InvariantCulture)));

        private static readonly Parser<NullLiteralExpression> NullLiteral =
            Parse.IgnoreCase("null").Select(x => NullLiteralExpression.Instance);

        private static readonly Parser<string> DotBetweenTwoNumbers =
            from n1 in Parse.Number
            from dot in Parse.Char('.')
            from n2 in Parse.Number
            select string.Concat(n1, dot, n2);

        private static readonly Parser<LiteralExpression> Literal =
            NullLiteral
                .Or<LiteralExpression>(StringLiteral)
                .Or(IntLiteral).Except(DotBetweenTwoNumbers)
                .Or(DoubleLiteral);

        private static readonly Parser<Formatter> Formatter =
            from frmStr in Parse.Contained(Parse.CharExcept('}').Many().Text(), Parse.Char('{'), Parse.Char('}'))
            from inv in Parse.Char('!').Optional()
            select new Formatter(frmStr, inv.IsDefined);

        private static readonly Parser<Expression> UnformattedExpression =
            Literal.Or<Expression>(Call).Or(Symbol);

        private static readonly Parser<Expression> Expression =
            from expr in UnformattedExpression
            from _ in Parse.WhiteSpace.Many()
            from form in Formatter.Optional()
            select form.IsDefined ? new FormattedExpression(expr, form.Get()) : expr;

        private static readonly Parser<Expression> ExpressionToken =
            Parse.Contained(Expression.Token(), Parse.Char('<'), Parse.Char('>'));

        private static readonly Parser<IToken> NonWsToken =
            ExpressionToken.Or<IToken>(ConstToken);

        private static readonly Parser<IEnumerable<IToken>> TrailingSyntaxTree =
            (from sp in ConstLegalSpaceToken
             from tok in NonWsToken
             select new[] { sp, tok })
            .Or
            (from _ in Parse.WhiteSpace.Many()
             from tok in NonWsToken
             select new[] { tok });

        internal static Parser<IEnumerable<IToken>> ParserRoot =
            from _ in Parse.WhiteSpace.Many()
            from first in NonWsToken
            from further in TrailingSyntaxTree.Many()
            from _2 in Parse.WhiteSpace.Many().End()
            select further.SelectMany(x => x).Prepend(first);

    }
}
