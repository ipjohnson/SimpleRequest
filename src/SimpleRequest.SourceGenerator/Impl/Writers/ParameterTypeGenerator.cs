using CSharpAuthor;
using SimpleRequest.SourceGenerator.Impl.Models;
using static  CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Writers;

public class ParameterTypeGenerator {

    public ITypeDefinition GenerateParameterType(
        RequestHandlerModel requestModel, ClassDefinition classDefinition) {
        
        var parameterClass = classDefinition.AddClass("InvokeParameters");
        parameterClass.AddBaseType(KnownRequestTypes.IInvokeParameters);
        
        var parameterFields = GetParameterFields(requestModel, parameterClass);

        ConstructParameterInfo(requestModel, parameterClass);
        
        ImplementGetMethod(requestModel, parameterClass, parameterFields);

        ImplementSetMethod(requestModel, parameterClass, parameterFields);

        ImplementCloneMethod(requestModel, parameterClass, parameterFields);
        
        ImplementCountParameter(requestModel, parameterClass);

        ImplementTryGetParameter(requestModel, parameterClass, parameterFields);
        
        ImplementTrySetParameter(requestModel, parameterClass, parameterFields);
        
        return TypeDefinition.Get("", "InvokeParameters");
    }

    public class CommaList : BaseOutputComponent {
        private readonly IEnumerable<IOutputComponent> _outputComponents;

        public CommaList(IEnumerable<IOutputComponent> outputComponents) {
            _outputComponents = outputComponents;
        }

        protected override void WriteComponentOutput(IOutputContext outputContext) {
            _outputComponents.OutputCommaSeparatedList(outputContext);
        }
    }
    
    private void ConstructParameterInfo(RequestHandlerModel requestModel, ClassDefinition parameterClass) {
        var readonlyType = new GenericTypeDefinition(typeof(IReadOnlyList<>), new []{KnownRequestTypes.IInvokeParameterInfo});
        var newList = New(TypeDefinition.List(KnownRequestTypes.IInvokeParameterInfo));

        var listOfNew = new List<IOutputComponent>();
        for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameter = requestModel.RequestParameterInformationList[i];
            var parameterInfo = new NewStatement(KnownRequestTypes.InvokeParameterInfo);

            parameterInfo.AddArgument(QuoteString(parameter.Name));
            parameterInfo.AddArgument(i);
            parameterInfo.AddArgument(TypeOf(parameter.ParameterType));
            parameterInfo.AddArgument(
                "ParameterBindType." + Enum.GetName(typeof(ParameterBindType), parameter.BindingType));
            listOfNew.Add(parameterInfo);
        }
        
        newList.AddInitValue(new CommaList(listOfNew));
        
        var parameterInfoField = 
            parameterClass.AddField(
                readonlyType,"ParameterInfoField");

        parameterInfoField.Modifiers |= ComponentModifier.Static | ComponentModifier.Public;

        parameterInfoField.InitializeValue = newList;

        var property = 
            parameterClass.AddProperty(readonlyType, "Parameters");
        property.Get.LambdaSyntax = true;
        property.Get.AddCode("ParameterInfoField;");
        property.Set = null;
    }

    private void ImplementTrySetParameter(RequestHandlerModel requestModel, ClassDefinition parameterClass, Dictionary<RequestParameterInformation,FieldDefinition> parameterFields) {
        var method = parameterClass.AddMethod("TrySetParameter");
        
        method.SetReturnType(typeof(bool));
        
        var indexParameter = method.AddParameter(typeof(string), "parameterName");
        
        var valueParameter = method.AddParameter(TypeDefinition.Get(typeof(object)), "value");
        
        var switchBlockDefinition = method.Switch(indexParameter);
        
        foreach (var parameter in requestModel.RequestParameterInformationList) {
            var field = parameterFields[parameter];

            var caseBlockDefinition = 
                switchBlockDefinition.AddCase(QuoteString(parameter.Name));
            
            caseBlockDefinition.Assign(
                new StaticCastComponent(field.TypeDefinition, valueParameter)).To(field.Instance);
            
            caseBlockDefinition.Return(CodeOutputComponent.Get("true"));
        }
        method.Return(CodeOutputComponent.Get("false"));
    }

    private void ImplementTryGetParameter(RequestHandlerModel requestModel, ClassDefinition parameterClass, Dictionary<RequestParameterInformation,FieldDefinition> parameterFields) {
        var method = parameterClass.AddMethod("TryGetParameter");
        
        method.SetReturnType(typeof(bool));
        
        var indexParameter = method.AddParameter(typeof(string), "parameterName");
        
        var outParameter = method.AddParameter(TypeDefinition.Get(typeof(object)), "value");
        outParameter.IsOut = true;
        
        var switchBlockDefinition = method.Switch(indexParameter);
        
        foreach (var parameter in requestModel.RequestParameterInformationList) {
            var field = parameterFields[parameter];

            var caseBlockDefinition = 
                switchBlockDefinition.AddCase(QuoteString(parameter.Name));
            
            caseBlockDefinition.Assign(field.Instance).To(outParameter);
            
            caseBlockDefinition.Return(CodeOutputComponent.Get("true"));
        }
        method.Assign(Null()).To(outParameter);
        method.Return(CodeOutputComponent.Get("false"));
    }

    private void ImplementCountParameter(RequestHandlerModel requestModel, ClassDefinition parameterClass) {
        var countProperty = parameterClass.AddProperty( TypeDefinition.Get(typeof(int)), "ParameterCount");

        countProperty.Get.LambdaSyntax = true;
        countProperty.Get.AddCode($"{requestModel.RequestParameterInformationList.Count};");
        countProperty.Set = null;
    }

    private void ImplementCloneMethod(RequestHandlerModel requestModel, ClassDefinition parameterClass, Dictionary<RequestParameterInformation,FieldDefinition> parameterFields) {
        var method = parameterClass.AddMethod("Clone");
        method.SetReturnType(KnownRequestTypes.IInvokeParameters);
        var clone =
            method.Assign(new NewStatement(TypeDefinition.Get("", "InvokeParameters"))).ToVar("clone");
        
        for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameter = requestModel.RequestParameterInformationList[i];
            var field = parameterFields[parameter];
            
            method.Assign(field.Instance).To(clone.Property(field.Name));
        }
        
        method.Return(clone);
    }

    private void ImplementSetMethod(RequestHandlerModel requestModel, ClassDefinition parameterClass, Dictionary<RequestParameterInformation,FieldDefinition> parameterFields) {
        var method = parameterClass.AddMethod("Set");
        
        var objectParameter = method.AddParameter(typeof(object), "value");
        var indexParameter = method.AddParameter(typeof(int), "index");
        
        var switchBlockDefinition = method.Switch(indexParameter);
        
        for(var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameter = requestModel.RequestParameterInformationList[i];
            var field = parameterFields[parameter];

            var caseBlockDefinition = 
                switchBlockDefinition.AddCase(i);
            
            caseBlockDefinition.Assign(new StaticCastComponent(field.TypeDefinition, objectParameter)).To(field.Instance);
            
            caseBlockDefinition.Break();
        }
        switchBlockDefinition.AddDefault().Throw(typeof(ArgumentOutOfRangeException));
    }

    private void ImplementGetMethod(
        RequestHandlerModel requestModel, ClassDefinition parameterClass, Dictionary<RequestParameterInformation,FieldDefinition> parameterFields) {
        var method = parameterClass.AddMethod("Get");
        var genericType = TypeDefinition.Get("", "T");
        
        method.SetReturnType(genericType);
        method.AddGenericParameter(genericType);
        var indexParameter = method.AddParameter(typeof(int), "index");
        var switchBlockDefinition = method.Switch(indexParameter);
        
        for(var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameter = requestModel.RequestParameterInformationList[i];
            var field = parameterFields[parameter];

            var caseBlockDefinition = 
                switchBlockDefinition.AddCase(i);
            var objectCast = new StaticCastComponent(TypeDefinition.Get(typeof(object)), field.Instance);
            var tCast = new StaticCastComponent(genericType, objectCast);
            caseBlockDefinition.Return(tCast);
        }
        switchBlockDefinition.AddDefault().Throw(typeof(ArgumentOutOfRangeException));
    }

    private Dictionary<RequestParameterInformation,FieldDefinition> GetParameterFields(
        RequestHandlerModel requestModel, ClassDefinition parameterClass) {
        var fields = new Dictionary<RequestParameterInformation, FieldDefinition>();

        foreach (var parameter in requestModel.RequestParameterInformationList) {
            var field = parameterClass.AddField(parameter.ParameterType, parameter.Name);
            
            fields.Add(parameter, field);
        }
        
        return fields;
    }
}