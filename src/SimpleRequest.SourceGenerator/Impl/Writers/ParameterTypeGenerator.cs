using CSharpAuthor;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Utils;
using static  CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Writers;

public class ParameterTypeGenerator {

    public ITypeDefinition GenerateParameterType(
        RequestHandlerModel requestModel, ClassDefinition classDefinition) {
        
        var parameterClass = classDefinition.AddClass("InvokeParameters");
        parameterClass.AddBaseType(KnownRequestTypes.IInvokeParameters);
        parameterClass.AddLeadingTrait(CodeOutputComponent.Get("#nullable enable", true));
        parameterClass.AddTrailingTrait(CodeOutputComponent.Get("#nullable disable", true));
        
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

        var staticParams = parameterClass.AddClass("StaticParams");
        staticParams.Modifiers |= ComponentModifier.Static | ComponentModifier.Public;
        
        var listOfNew = new List<IOutputComponent>();
        for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameter = requestModel.RequestParameterInformationList[i];
            var parameterInfo = new NewStatement(KnownRequestTypes.InvokeParameterInfo);

            parameterInfo.AddArgument(QuoteString(parameter.Name));
            parameterInfo.AddArgument(i);
            parameterInfo.AddArgument(TypeOf(parameter.ParameterType));
            parameterInfo.AddArgument(
                "ParameterBindType." + Enum.GetName(typeof(ParameterBindType), parameter.BindingType));

            if (parameter.DefaultValue != null) {
                parameterInfo.AddArgument(QuoteString(parameter.DefaultValue));
            }
            else {
                parameterInfo.AddArgument(Null());
            }
            parameterInfo.AddArgument(parameter.Required.ToString().ToLower());
            parameterInfo.AddArgument(QuoteString(parameter.BindingName));
            parameterInfo.AddArgument(AttributeArrayHelper.CreateAttributeArray(parameter.CustomAttributes));
            
            var field = staticParams.AddField(KnownRequestTypes.InvokeParameterInfo, "static_" + parameter.Name);
            
            field.Modifiers |= ComponentModifier.Static | ComponentModifier.Public | ComponentModifier.Readonly;
            field.InitializeValue = parameterInfo;
            
            listOfNew.Add(CodeOutputComponent.Get("StaticParams.static_" + parameter.Name));
        }
        
        newList.AddInitValue(new CommaList(listOfNew));
        
        var parameterInfoField = 
            parameterClass.AddField(
                readonlyType,"ParameterInfoField");

        parameterInfoField.Modifiers |=
            ComponentModifier.Static | ComponentModifier.Public | ComponentModifier.Readonly;

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

        if (requestModel.RequestParameterInformationList.Count > 0) {
            var switchBlockDefinition = method.Switch(indexParameter);

            foreach (var parameter in requestModel.RequestParameterInformationList) {
                var field = parameterFields[parameter];

                var caseBlockDefinition =
                    switchBlockDefinition.AddCase(QuoteString(parameter.Name));

                caseBlockDefinition.Assign(
                    new StaticCastComponent(field.TypeDefinition, valueParameter)).
                    To("this." + field.Name);

                caseBlockDefinition.Return(CodeOutputComponent.Get("true"));
            }
        }
        
        method.Return(CodeOutputComponent.Get("false"));
    }

    private void ImplementTryGetParameter(RequestHandlerModel requestModel, ClassDefinition parameterClass, Dictionary<RequestParameterInformation,FieldDefinition> parameterFields) {
        var method = parameterClass.AddMethod("TryGetParameter");
        
        method.SetReturnType(typeof(bool));
        
        var indexParameter = method.AddParameter(typeof(string), "parameterName");
        
        var outParameter = method.AddParameter(TypeDefinition.Get(typeof(object)).MakeNullable(), "value");
        outParameter.IsOut = true;

        if (requestModel.RequestParameterInformationList.Count > 0) {
            var switchBlockDefinition = method.Switch(indexParameter);

            foreach (var parameter in requestModel.RequestParameterInformationList) {
                var field = parameterFields[parameter];

                var caseBlockDefinition =
                    switchBlockDefinition.AddCase(QuoteString(parameter.Name));

                caseBlockDefinition.Assign("this." + field.Name).To(outParameter);

                caseBlockDefinition.Break();
            }

            var defaultBlock = switchBlockDefinition.AddDefault();
            
            defaultBlock.Assign(Null()).To(outParameter);
            defaultBlock.Break();
            
        }
        else {
            method.Assign(Null()).To(outParameter);
        }
        
        method.Return(NotEquals(outParameter, Null()));
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
        
        var objectParameter = method.AddParameter(TypeDefinition.Get( typeof(object)).MakeNullable(), "value");
        var indexParameter = method.AddParameter(typeof(int), "index");

        if (requestModel.RequestParameterInformationList.Count > 0) {
            var switchBlockDefinition = method.Switch(indexParameter);

            for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
                var parameter = requestModel.RequestParameterInformationList[i];
                var field = parameterFields[parameter];

                var caseBlockDefinition =
                    switchBlockDefinition.AddCase(i);

                caseBlockDefinition.Assign(new StaticCastComponent(field.TypeDefinition, objectParameter)).To("this." + field.Name);

                caseBlockDefinition.Break();
            }
            switchBlockDefinition.AddDefault().Throw(typeof(ArgumentOutOfRangeException));
        }
    }

    private void ImplementGetMethod(
        RequestHandlerModel requestModel, ClassDefinition parameterClass, Dictionary<RequestParameterInformation,FieldDefinition> parameterFields) {
        var method = parameterClass.AddMethod("Get");
        var genericType = TypeDefinition.Get("", "T");
        var genericTypeNullable = genericType.MakeNullable();
        
        method.SetReturnType(genericTypeNullable);
        method.AddGenericParameter(genericType);
        
        var indexParameter = method.AddParameter(typeof(int), "index");
        var defaultValue = method.AddParameter(genericTypeNullable, "defaultValue");
        
        if (requestModel.RequestParameterInformationList.Count > 0) {
            var returnValue =
                method.Assign(Null()).ToLocal(TypeDefinition.Get(typeof(object)).MakeNullable(), "returnValue");
            
            var switchBlockDefinition = method.Switch(indexParameter);

            for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
                var parameter = requestModel.RequestParameterInformationList[i];
                var field = parameterFields[parameter];

                var caseBlockDefinition =
                    switchBlockDefinition.AddCase(i);
                caseBlockDefinition.Assign(field.Instance).To(returnValue);
                
                caseBlockDefinition.Break();
            }
            
            switchBlockDefinition.AddDefault().Throw(typeof(ArgumentOutOfRangeException));
            
            method.Return(
                new StaticCastComponent(
                    genericTypeNullable,
                    NullCoalesce( returnValue, defaultValue)));
        }
        else {
            method.Throw(typeof(ArgumentOutOfRangeException));
        }
    }

    private Dictionary<RequestParameterInformation,FieldDefinition> GetParameterFields(
        RequestHandlerModel requestModel, ClassDefinition parameterClass) {
        var fields = new Dictionary<RequestParameterInformation, FieldDefinition>();

        foreach (var parameter in requestModel.RequestParameterInformationList) {
            var field = parameterClass.AddField(parameter.ParameterType.MakeNullable(), parameter.Name);
            
            fields.Add(parameter, field);
        }
        
        return fields;
    }
}