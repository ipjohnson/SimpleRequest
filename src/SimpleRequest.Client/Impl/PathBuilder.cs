using System.Text;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Impl;


public interface IPathBuilder {
    
    string BuildPath(IOperationParameters parameters, StringBuilder builder, bool includeQueryString = false);
}

public class PathBuilder(IOperationInfo operationInfo) : IPathBuilder {


    public string BuildPath(IOperationParameters parameters, StringBuilder builder, bool includeQueryString = true) {
        if (operationInfo.Path.Parts.Count == 0) {
            return operationInfo.Path.Template;
        }

        bool queryAdded = false;
        for (var i = 0; i < operationInfo.Path.Parts.Count; i++) {
            var part = operationInfo.Path.Parts[i];

            switch (part.PartType) {
                case PathPartType.Constant:
                    builder.Append(part.Value);
                    break;
                case PathPartType.Path:
                    if (part.ParameterInfo != null) {
                        builder.Append(parameters.Get<object>(part.ParameterInfo.Index));
                    }
                    break;
                case PathPartType.QueryString:
                    if (includeQueryString && part.ParameterInfo != null) {
                        if (!queryAdded) {
                            builder.Append('?');
                            queryAdded = true;
                        }
                        else {
                            builder.Append('&');
                        }

                        builder.Append(part.ParameterInfo.BindingName);
                        builder.Append('=');
                        builder.Append(parameters.Get<object>(part.ParameterInfo.Index));
                    }
                    break;
            }
        }
        
        return builder.ToString();
    }
}