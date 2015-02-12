﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roslyn.Compilers.CSharp.Emit
{
    internal sealed class NestedTypeReference : NamedTypeReference, Microsoft.Cci.INestedTypeReference
    {
        public NestedTypeReference(Module moduleBeingBuilt, NamedTypeSymbol underlyingNamedType)
            : base(moduleBeingBuilt, underlyingNamedType)
        {
        }

        public override void Dispatch(Microsoft.Cci.IMetadataVisitor visitor)
        {
            visitor.Visit((Microsoft.Cci.INestedTypeReference)this);
        }

        Microsoft.Cci.ITypeReference Microsoft.Cci.ITypeMemberReference.ContainingType
        {
            get 
            {
                return ModuleBeingBuilt.Translate(UnderlyingNamedType.ContainingType, true); 
            }
        }

        protected override Microsoft.Cci.IArrayTypeReference AsArrayTypeReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IDefinition AsDefinition
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IFunctionPointerTypeReference AsFunctionPointerTypeReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IGenericMethodParameter AsGenericMethodParameter
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IGenericMethodParameterReference AsGenericMethodParameterReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IGenericTypeInstanceReference AsGenericTypeInstanceReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IGenericTypeParameter AsGenericTypeParameter
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IGenericTypeParameterReference AsGenericTypeParameterReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IManagedPointerTypeReference AsManagedPointerTypeReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.IModifiedTypeReference AsModifiedTypeReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.INamespaceTypeDefinition AsNamespaceTypeDefinition
        {
            get { return null; }
        }

        protected override Microsoft.Cci.INamespaceTypeReference AsNamespaceTypeReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.INestedTypeDefinition AsNestedTypeDefinition
        {
            get { return null; }
        }

        protected override Microsoft.Cci.INestedTypeReference AsNestedTypeReference
        {
            get { return this; }
        }

        protected override Microsoft.Cci.IPointerTypeReference AsPointerTypeReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.ISpecializedNestedTypeReference AsSpecializedNestedTypeReference
        {
            get { return null; }
        }

        protected override Microsoft.Cci.ITypeDefinition AsTypeDefinition
        {
            get { return null; }
        }

    }
}
