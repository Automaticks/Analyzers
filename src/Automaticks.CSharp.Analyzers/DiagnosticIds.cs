namespace Automaticks.CSharp;

/// <summary>
///     Centralised registry of all diagnostic IDs emitted by the CSharp Roslyn analyzers.
///     Use these constants when suppressing a diagnostic or referencing it from documentation.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    ///     Diagnostic IDs for C# language rules.
    /// </summary>
    public static class CSharp
    {
        /// <summary>Diagnostic ID for <c>ATXCS017</c>: identifier contains an abbreviated segment.</summary>
        public const string AbbreviatedIdentifier = "ATXCS017";

        /// <summary>Diagnostic ID for <c>ATXCS044</c>: two or more consecutive blank lines are forbidden.</summary>
        public const string ConsecutiveBlankLines = "ATXCS044";

        /// <summary>Diagnostic ID for <c>ATXCS045</c>: auto-implemented properties must be declared on a single line.</summary>
        public const string AutoPropertySingleLine = "ATXCS045";

        /// <summary>Diagnostic ID for <c>ATXCS012</c>: anonymous tuple types are forbidden.</summary>
        public const string AnonymousTuple = "ATXCS012";

        /// <summary>Diagnostic ID for <c>ATXCS003</c>: task-returning methods must use the <c>Async</c> suffix.</summary>
        public const string AsyncMethodNaming = "ATXCS003";

        /// <summary>
        ///     Diagnostic ID for <c>ATXCS009</c>: methods with the <c>Async</c> suffix must return
        ///     <see cref="System.Threading.Tasks.Task" /> or <see cref="System.Threading.Tasks.ValueTask" />.
        /// </summary>
        public const string AsyncSuffixReturnType = "ATXCS009";

        /// <summary>Diagnostic ID for <c>ATXCS034</c>: class exceeds the maximum lines-of-code limit.</summary>
        public const string ClassLineLimit = "ATXCS034";

        /// <summary>Diagnostic ID for <c>ATXCS033</c>: method cognitive complexity exceeds the configured maximum.</summary>
        public const string CognitiveComplexity = "ATXCS033";

        /// <summary>Diagnostic ID for <c>ATXCS028</c>: method cyclomatic complexity exceeds the configured maximum.</summary>
        public const string CyclomaticComplexity = "ATXCS028";

        /// <summary>Diagnostic ID for <c>ATXCS029</c>: direct cast to a reference type is forbidden.</summary>
        public const string DirectCast = "ATXCS029";

        /// <summary>
        ///     Diagnostic ID for <c>ATXCS007</c>: <see cref="System.EventHandler" /> and
        ///     <see cref="System.EventHandler{TEventArgs}" /> declarations are not allowed.
        /// </summary>
        public const string EventHandlerDeclaration = "ATXCS007";

        /// <summary>Diagnostic ID for <c>ATXCS022</c>: callable construct defines more than 4 parameters.</summary>
        public const string ExcessiveParameterCount = "ATXCS022";

        /// <summary>Diagnostic ID for <c>ATXCS031</c>: type name does not match the file name.</summary>
        public const string FileNameMismatch = "ATXCS031";

        /// <summary>
        ///     Diagnostic ID for <c>ATXCS020</c>: built-in generic delegate types (<c>Action</c>, <c>Func</c>,
        ///     <c>Predicate</c>, <c>Comparison</c>, <c>Converter</c>) are forbidden.
        /// </summary>
        public const string GenericDelegate = "ATXCS020";

        /// <summary>Diagnostic ID for <c>ATXCS013</c>: the <c>internal</c> access modifier is forbidden.</summary>
        public const string InternalModifier = "ATXCS013";

        /// <summary>Diagnostic ID for <c>ATXCS021</c>: method exceeds the maximum line limit.</summary>
        public const string MethodLineLimit = "ATXCS021";

        /// <summary>Diagnostic ID for <c>ATXCS032</c>: method nesting depth exceeds the maximum.</summary>
        public const string NestingDepth = "ATXCS032";

        /// <summary>Diagnostic ID for <c>ATXCS023</c>: method defines more than one <c>out</c> parameter.</summary>
        public const string OutParameterCount = "ATXCS023";

        /// <summary>Diagnostic ID for <c>ATXCS024</c>: <c>out</c> parameter is not the last parameter.</summary>
        public const string OutParameterPosition = "ATXCS024";

        /// <summary>Diagnostic ID for <c>ATXCS041</c>: plain single-line and block comments are forbidden.</summary>
        public const string PlainComment = "ATXCS041";

        /// <summary>Diagnostic ID for <c>ATXCS004</c>: provider/factory/builder/client types must not expose properties.</summary>
        public const string ProviderFactoryProperty = "ATXCS004";

        /// <summary>Diagnostic ID for <c>ATXCS014</c>: redundant null check on a non-nullable reference parameter.</summary>
        public const string RedundantNullCheck = "ATXCS014";

        /// <summary>Diagnostic ID for <c>ATXCS027</c>: method defines more than one <c>ref</c> parameter.</summary>
        public const string RefParameterCount = "ATXCS027";

        /// <summary>
        ///     Diagnostic ID for <c>ATXCS025</c>: <c>ref</c> parameter is forbidden in methods not named
        ///     <c>SetProperty</c>.
        /// </summary>
        public const string RefParameterForbidden = "ATXCS025";

        /// <summary>Diagnostic ID for <c>ATXCS026</c>: <c>ref</c> parameter is not the first parameter.</summary>
        public const string RefParameterPosition = "ATXCS026";

        /// <summary>Diagnostic ID for <c>ATXCS011</c>: static methods must only exist in static classes.</summary>
        public const string StaticMethodInNonStaticClass = "ATXCS011";

        /// <summary>Diagnostic ID for <c>ATXCS050</c>: <c>&lt;summary&gt;</c> content must start on a new line and be indented with exactly 4 spaces.</summary>
        public const string SummaryXmlDocFormat = "ATXCS050";

        /// <summary>
        ///     Diagnostic ID for <c>ATXCS036</c>: fields and auto-properties must not be initialized
        ///     inline; initialization must move to the constructor.
        /// </summary>
        public const string InlineFieldInitializer = "ATXCS036";

        /// <summary>Diagnostic ID for <c>ATXCS037</c>: explicit constructors are required; primary constructors are forbidden.</summary>
        public const string ExplicitConstructor = "ATXCS037";

        /// <summary>Diagnostic ID for <c>ATXCS038</c>: <c>&lt;remarks&gt;</c> XML doc element is forbidden.</summary>
        public const string RemarksXmlDoc = "ATXCS038";

        /// <summary>Diagnostic ID for <c>ATXCS039</c>: empty lines between adjacent field or constant declarations are forbidden.</summary>
        public const string EmptyLineBetweenFields = "ATXCS039";

        /// <summary>Diagnostic ID for <c>ATXCS040</c>: exactly one blank line is required between a property or indexer and an adjacent member declaration.</summary>
        public const string SingleBlankLineBetweenProperties = "ATXCS040";

        /// <summary>Diagnostic ID for <c>ATXCS043</c>: exactly one blank line is required between the last <c>using</c> directive and the <c>namespace</c> declaration.</summary>
        public const string SingleBlankLineBetweenUsingsAndNamespace = "ATXCS043";

        /// <summary>Diagnostic ID for <c>ATXCS042</c>: type members must appear in canonical order.</summary>
        public const string TypeMemberOrder = "ATXCS042";

        /// <summary>Diagnostic ID for <c>ATXCS046</c>: duplicate <c>using</c> directive.</summary>
        public const string DuplicateUsingDirective = "ATXCS046";

        /// <summary>Diagnostic ID for <c>ATXCS047</c>: <c>using</c> directives must be sorted alphabetically.</summary>
        public const string UnsortedUsingDirectives = "ATXCS047";

        /// <summary>Diagnostic ID for <c>ATXCS048</c>: unused <c>using</c> directive.</summary>
        public const string UnusedUsingDirective = "ATXCS048";

        /// <summary>Diagnostic ID for <c>ATXCS051</c>: public or protected member is missing a <c>&lt;summary&gt;</c> XML documentation comment.</summary>
        public const string MissingSummaryXmlDoc = "ATXCS051";

        /// <summary>Diagnostic ID for <c>ATXCS052</c>: public or protected method or constructor is missing a <c>&lt;param&gt;</c> XML documentation element for one or more parameters.</summary>
        public const string MissingParamXmlDoc = "ATXCS052";

        /// <summary>Diagnostic ID for <c>ATXCS053</c>: public or protected non-<c>void</c> method is missing a <c>&lt;returns&gt;</c> XML documentation element.</summary>
        public const string MissingReturnsXmlDoc = "ATXCS053";

        /// <summary>Diagnostic ID for <c>ATXCS054</c>: a blank line is required before the XML doc comment of a non-first member declaration.</summary>
        public const string MissingBlankLineBeforeXmlDoc = "ATXCS054";

        /// <summary>Diagnostic ID for <c>ATXCS055</c>: the <c>params</c> modifier is forbidden.</summary>
        public const string ParamsParameter = "ATXCS055";

        /// <summary>Diagnostic ID for <c>ATXCS057</c>: parameter default values are forbidden in methods, constructors, local functions, lambdas, anonymous methods, and indexers.</summary>
        public const string ParameterDefaultValue = "ATXCS057";

        /// <summary>Diagnostic ID for <c>ATXCS058</c>: inline <c>new</c> expressions are forbidden; the instance must be assigned to a local variable first.</summary>
        public const string InlineNewExpression = "ATXCS058";

        /// <summary>Diagnostic ID for <c>ATXCS059</c>: object, collection, array, or <c>with</c> initializer does not follow the one-member-per-line format with braces on their own lines.</summary>
        public const string ObjectInitializerFormat = "ATXCS059";

        /// <summary>Diagnostic ID for <c>ATXCS060</c>: object, collection, array, or <c>with</c> initializer has empty braces.</summary>
        public const string ObjectInitializerEmptyBraces = "ATXCS060";

        /// <summary>Diagnostic ID for <c>ATXCS061</c>: interface member must not have a default implementation body or be a static member.</summary>
        public const string InterfaceDefaultImplementation = "ATXCS061";

        /// <summary>Diagnostic ID for <c>ATXCS062</c>: boolean fields and properties must use an allowed prefix (<c>is</c> or <c>allow</c>, case-insensitive).</summary>
        public const string BooleanMemberNaming = "ATXCS062";

        /// <summary>Diagnostic ID for <c>ATXCS063</c>: methods and local functions returning <c>bool</c> or <c>bool?</c> must use an allowed prefix (<c>can</c> or <c>has</c>, case-insensitive).</summary>
        public const string BooleanMethodNaming = "ATXCS063";

        /// <summary>Diagnostic ID for <c>ATXCS064</c>: type member violates within-group ordering (public/protected/private, static/instance, then alphabetical).</summary>
        public const string TypeMemberWithinGroupOrder = "ATXCS064";

        /// <summary>
        ///     Diagnostic ID for <c>ATXCS065</c>: a public init-only auto-property is set in the
        ///     containing type's single instance constructor from a constructor parameter.
        /// </summary>
        public const string RedundantInitSetter = "ATXCS065";

        /// <summary>Diagnostic ID for <c>ATXCS066</c>: a folder directly contains more than the maximum number of source files.</summary>
        public const string FolderFileCount = "ATXCS066";

        /// <summary>Diagnostic ID for <c>ATXCS067</c>: a namespace is declared by more than the maximum number of source files.</summary>
        public const string NamespaceFileCount = "ATXCS067";
    }
}
