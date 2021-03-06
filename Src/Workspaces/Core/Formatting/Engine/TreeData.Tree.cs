using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Services.Shared.Extensions;
using Roslyn.Utilities;

namespace Roslyn.Services.Formatting
{
    internal abstract partial class TreeData
    {
        private class Tree : TreeData
        {
            private readonly CommonSyntaxTree syntaxTree;

            public Tree(CommonSyntaxTree syntaxTree)
            {
                Contract.ThrowIfNull(syntaxTree);
                this.syntaxTree = syntaxTree;
            }

            public override int GetColumnOfToken(CommonSyntaxToken token, int tabSize)
            {
                Contract.ThrowIfTrue(token.Kind == 0);

                var line = this.syntaxTree.Text.GetLineFromPosition(token.Span.Start);
                var lineText = line.GetText();

                return lineText.ConvertStringTextPositionToColumn(tabSize, token.Span.Start - line.Start);
            }

            public override string GetTextBetween(CommonSyntaxToken token1, CommonSyntaxToken token2)
            {
                if (token1.Kind == 0)
                {
                    // get leading trivia text
                    return this.syntaxTree.Text.GetText(TextSpan.FromBounds(token2.FullSpan.Start, token2.Span.Start));
                }

                if (token2.Kind == 0)
                {
                    // get trailing trivia text
                    return this.syntaxTree.Text.GetText(TextSpan.FromBounds(token1.Span.End, token1.FullSpan.End));
                }

                return this.syntaxTree.Text.GetText(TextSpan.FromBounds(token1.Span.End, token2.Span.Start));
            }

            public override CommonSyntaxNode Root
            {
                get
                {
                    return this.syntaxTree.Root;
                }
            }
        }
    }
}
