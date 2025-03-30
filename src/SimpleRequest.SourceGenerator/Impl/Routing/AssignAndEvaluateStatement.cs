using CSharpAuthor;

namespace SimpleRequest.SourceGenerator.Impl.Routing;

public static class AssignAndEvaluateStatementExtensions {
    public static IfElseLogicBlockDefinition IfNotNullAssign(
        this BaseBlockDefinition block, IOutputComponent leftSide, IOutputComponent rightSide) {
        return block.If(new AssignAndEvaluateStatement(leftSide, rightSide));
    }
}

public class AssignAndEvaluateStatement : BaseBlockDefinition {
    private IOutputComponent _leftSide;
    private IOutputComponent _rightSide;

    public AssignAndEvaluateStatement(IOutputComponent leftSide, IOutputComponent rightSide) {
        _leftSide = leftSide;
        _rightSide = rightSide;
    }

    protected override void WriteComponentOutput(IOutputContext outputContext) {
        outputContext.Write("(");
        _leftSide.WriteOutput(outputContext);
        outputContext.Write(" = ");
        _rightSide.WriteOutput(outputContext);
        outputContext.Write(") != null");
    }
}