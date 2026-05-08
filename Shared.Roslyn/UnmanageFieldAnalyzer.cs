using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Shared.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnmanagedFieldAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "PREDICT001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Field must be unmanaged",
        "Field '{0}' in component '{1}' must be unmanaged to be used in PredictField",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodCall, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeMethodCall(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol || methodSymbol.Name != "PredictField")
            return;
        
        var componentType = methodSymbol.TypeArguments.FirstOrDefault();
        if (componentType == null) return;
        
        if (invocation.ArgumentList.Arguments.Count < 2) return;
        var fieldNameArg = invocation.ArgumentList.Arguments[1].Expression;
        var constantValue = context.SemanticModel.GetConstantValue(fieldNameArg);

        if (!constantValue.HasValue || constantValue.Value is not string fieldName)
            return;
        
        var field = componentType.GetMembers(fieldName).OfType<IFieldSymbol>().FirstOrDefault();
        
        if (field != null && !field.Type.IsUnmanagedType)
        {
            var diagnostic = Diagnostic.Create(Rule, fieldNameArg.GetLocation(), fieldName, componentType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}