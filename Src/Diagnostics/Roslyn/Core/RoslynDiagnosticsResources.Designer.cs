﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Roslyn.Diagnostics.Analyzers {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class RoslynDiagnosticsResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RoslynDiagnosticsResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Roslyn.Diagnostics.Analyzers.RoslynDiagnosticsResources", typeof(RoslynDiagnosticsResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CancellationToken parameters must come last.
        /// </summary>
        internal static string CancellationTokenMustBeLastDescription {
            get {
                return ResourceManager.GetString("CancellationTokenMustBeLastDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Method &apos;{0}&apos; should take CancellationToken as the last parameter.
        /// </summary>
        internal static string CancellationTokenMustBeLastMessage {
            get {
                return ResourceManager.GetString("CancellationTokenMustBeLastMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not directly await a Task.
        /// </summary>
        internal static string DirectlyAwaitingTaskDescription {
            get {
                return ResourceManager.GetString("DirectlyAwaitingTaskDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not directly await a Task without calling ConfigureAwait.
        /// </summary>
        internal static string DirectlyAwaitingTaskMessage {
            get {
                return ResourceManager.GetString("DirectlyAwaitingTaskMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use generic CodeAction.Create to create CodeAction.
        /// </summary>
        internal static string DontUseCodeActionCreateDescription {
            get {
                return ResourceManager.GetString("DontUseCodeActionCreateDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider creating unique code action type per different fix. it will help us to see how each code action is used. otherwise, we will only see bunch of generic code actions being used..
        /// </summary>
        internal static string DontUseCodeActionCreateMessage {
            get {
                return ResourceManager.GetString("DontUseCodeActionCreateMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Implement IEquatable&lt;T&gt; when overriding Object.Equals.
        /// </summary>
        internal static string ImplementIEquatableDescription {
            get {
                return ResourceManager.GetString("ImplementIEquatableDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type {0} should implement IEquatable&lt;T&gt; because it overrides Equals.
        /// </summary>
        internal static string ImplementIEquatableMessage {
            get {
                return ResourceManager.GetString("ImplementIEquatableMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parts exported with MEFv2 must be marked as Shared..
        /// </summary>
        internal static string MissingSharedAttributeDescription {
            get {
                return ResourceManager.GetString("MissingSharedAttributeDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Part exported with MEFv2 must be marked with the Shared attribute..
        /// </summary>
        internal static string MissingSharedAttributeMessage {
            get {
                return ResourceManager.GetString("MissingSharedAttributeMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not mix attributes from different versions of MEF.
        /// </summary>
        internal static string MixedVersionsOfMefAttributesDescription {
            get {
                return ResourceManager.GetString("MixedVersionsOfMefAttributesDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attribute &apos;{0}&apos; comes from a different version of MEF than the export attribute on &apos;{1}&apos;.
        /// </summary>
        internal static string MixedVersionsOfMefAttributesMessage {
            get {
                return ResourceManager.GetString("MixedVersionsOfMefAttributesMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Override Object.Equals(object) when implementing IEquatable&lt;T&gt; .
        /// </summary>
        internal static string OverrideObjectEqualsDescription {
            get {
                return ResourceManager.GetString("OverrideObjectEqualsDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type {0} should override Equals because it implements IEquatable&lt;T&gt;.
        /// </summary>
        internal static string OverrideObjectEqualsMessage {
            get {
                return ResourceManager.GetString("OverrideObjectEqualsMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Avoid zero-length array allocations..
        /// </summary>
        internal static string UseArrayEmptyDescription {
            get {
                return ResourceManager.GetString("UseArrayEmptyDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Avoid unnecessary zero-length array allocations.  Use Array.Empty&lt;T&gt;() instead..
        /// </summary>
        internal static string UseArrayEmptyMessage {
            get {
                return ResourceManager.GetString("UseArrayEmptyMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use SpecializedCollections.EmptyEnumerable&lt;T&gt;().
        /// </summary>
        internal static string UseEmptyEnumerableDescription {
            get {
                return ResourceManager.GetString("UseEmptyEnumerableDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use SpecializedCollections.EmptyEnumerable&lt;T&gt;().
        /// </summary>
        internal static string UseEmptyEnumerableMessage {
            get {
                return ResourceManager.GetString("UseEmptyEnumerableMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use cref tags without a type prefix..
        /// </summary>
        internal static string UseProperCrefTagsDescription {
            get {
                return ResourceManager.GetString("UseProperCrefTagsDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cref tag has prefix &apos;{0}&apos;, which should be removed unless the type or member cannot be accessed..
        /// </summary>
        internal static string UseProperCrefTagsMessage {
            get {
                return ResourceManager.GetString("UseProperCrefTagsMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use SpecializedCollections.SingletonEnumerable&lt;T&gt;().
        /// </summary>
        internal static string UseSingletonEnumerableDescription {
            get {
                return ResourceManager.GetString("UseSingletonEnumerableDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use SpecializedCollections.SingletonEnumerable&lt;T&gt;().
        /// </summary>
        internal static string UseSingletonEnumerableMessage {
            get {
                return ResourceManager.GetString("UseSingletonEnumerableMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invoke the correct property to ensure correct use site diagnostics..
        /// </summary>
        internal static string UseSiteDiagnosticsCheckerDescription {
            get {
                return ResourceManager.GetString("UseSiteDiagnosticsCheckerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not directly invoke the property &apos;{0}&apos;, instead use &apos;{0}NoUseSiteDiagnostics&apos;..
        /// </summary>
        internal static string UseSiteDiagnosticsCheckerMessage {
            get {
                return ResourceManager.GetString("UseSiteDiagnosticsCheckerMessage", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Do not call ToImmutableArray on ImmutableArray
        /// </summary>
        internal static string DoNotCallToImmutableArrayMessage {
            get {
                return ResourceManager.GetString("DoNotCallToImmutableArrayMessage", resourceCulture);
            }
        }
    }
}