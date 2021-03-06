﻿using Roslyn.Compilers.Common;

namespace Roslyn.Services.Shared.CodeGeneration
{
    internal class TypeReferenceExpressionDefinition : ExpressionDefinition
    {
        public INamedTypeSymbol Type { get; private set; }

        public TypeReferenceExpressionDefinition(INamedTypeSymbol type)
        {
            this.Type = type;
        }

        protected override CodeDefinition Clone()
        {
            return new TypeReferenceExpressionDefinition(this.Type);
        }

        public override void Accept(ICodeDefinitionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Accept<T>(ICodeDefinitionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<TArgument, TResult>(ICodeDefinitionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.Visit(this, argument);
        }
    }
}