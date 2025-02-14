using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Routing.Tree;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Routing;

public class RoutingTableGenerator {
    private readonly ParameterDefinition _pathSpan = new (TypeDefinition.Get(typeof(ReadOnlySpan<Char>)), "path");
    private readonly ParameterDefinition _index = new (TypeDefinition.Get(typeof(int)), "index");
    private readonly ParameterDefinition _context = new (KnownRequestTypes.IRequestContext, "context");
    private readonly FieldDefinition _factoryField;

    public RoutingTableGenerator(FieldDefinition factory) {
        _factoryField = factory;
    }

    private record RoutingTableContext(
        RouteTreeNode<RequestHandlerModel> Node,
        int Index,
        ClassDefinition ClassDefinition,
        ModuleEntryPointModel ModuleEntry,
        BaseBlockDefinition Block,
        CancellationToken CancellationToken);
    
    public void GenerateGetRequestHandlerMethod(
        ClassDefinition classDefinition, ModuleEntryPointModel moduleEntry, ImmutableArray<RequestHandlerModel> requestModels, CancellationToken cancellationToken) {
        var routingTree = GetRoutingTree(moduleEntry, requestModels, cancellationToken);
        var handlerMethod = classDefinition.AddMethod("GetRequestHandler");

        var parameter = handlerMethod.AddParameter(KnownRequestTypes.IRequestContext, "context");
        var pathSpan = 
            handlerMethod.Assign(parameter.Property("RequestData").Property("Path").Invoke("AsSpan")).ToVar("path");
        
        handlerMethod.SetReturnType(KnownRequestTypes.IRequestHandler);

        var routeMethod = WriteCurrentNode(new RoutingTableContext(
            routingTree,
            0,
            classDefinition,
            moduleEntry,
            handlerMethod,
            cancellationToken
            ));
        
        handlerMethod.Return(Invoke(routeMethod.Name,
            pathSpan,
            0,
            parameter
        ));
    }

    private MethodDefinition WriteCurrentNode(RoutingTableContext routingTableContext) {
        
        if (string.IsNullOrEmpty(routingTableContext.Node.Path)) {
            return ProcessEmptyPathNode(routingTableContext);
        }

        return ProcessSingularNode(routingTableContext);
    }

    private MethodDefinition ProcessSingularNode(RoutingTableContext routingTableContext) {
        var method = CreatePathTestMethod(routingTableContext, GetNewMethodName(routingTableContext));

        var ifStatementLogic = CreateIfStatementLogic(routingTableContext);
        
        
        
        var block = method.If(And(ifStatementLogic));
        
        block.Assign(Add(_index, routingTableContext.Node.Path.Length)).To(_index);
        
        var newContext = routingTableContext with {
            Index = routingTableContext.Index + routingTableContext.Node.Path.Length,
            Block = block
        };
        
        ProcessLeafNodes(newContext);
        
        ProcessChildNodes(newContext);
        
        ProcessWildCardNodes(newContext);
        
        method.Return(Null());
        
        return method;
    }

    private void ProcessChildNodes(RoutingTableContext context) {
        if (context.Node.ChildNodes.Count == 0) {
            return;
        }
   
        MethodDefinition childMethod;

        if (context.Node.ChildNodes.Count == 1) {
            childMethod = WriteCurrentNode(context with{ Node = context.Node.ChildNodes.First() });
        }
        else {
            childMethod = WriteSwitchChildNode(context);
        }

        var handler = 
            context.Block.Assign(Invoke(childMethod.Name, _pathSpan, _index, _context)).ToVar("handler");
        
        context.Block.If(NotEquals(handler, Null())).Return(handler);
    }

    private MethodDefinition WriteSwitchChildNode(RoutingTableContext context) {
        var switchMethodName = GetNewMethodName(context);
        var method = CreatePathTestMethod(context, switchMethodName);
        
        var ifStatement = method.If("charSpan.Length > index");

        var switchStatement = ifStatement.Switch("charSpan[index]");

        foreach (var childNode in context.Node.ChildNodes) {
            context.CancellationToken.ThrowIfCancellationRequested();

            var lowerChar = char.ToLowerInvariant(childNode.Path.First());
            var upperChar = char.ToUpperInvariant(lowerChar);

            if (upperChar != lowerChar) {
                switchStatement.AddCase($"'{upperChar}'");
            }

            var caseStatement = switchStatement.AddCase($"'{lowerChar}'");

            var newMethodName = WriteCurrentNode(context with{ Node = childNode});

            var invoke = Invoke(newMethodName.Name, _pathSpan,Add(_index , 1), _context);

            caseStatement.Return(invoke);
        }

        method.Return(Null());

        return method;
    }

    
    private void ProcessWildCardNodes(
        RoutingTableContext newContext) {
        if (newContext.Node.WildCardNodes.Count == 0) {
            return;
        }
        
        var wildCardMethod = WriteWildCardMethod(newContext);

        var invoke = Invoke(wildCardMethod, _pathSpan, _index, _context);

        newContext.Block.Return(invoke);
    }

    private string WriteWildCardMethod(
        RoutingTableContext context) {
        var methodName = GetNewMethodName(context);
        var wildCardMethod= CreatePathTestMethod(context, methodName);

        var handler =
            wildCardMethod.Assign(Null()).ToLocal(KnownRequestTypes.IRequestHandler, "handler");

        var orderedList =
            context.Node.WildCardNodes.OrderByDescending(n => n.Path).ToList();

        for (var i = 0; i < orderedList.Count; i++) {
            context.CancellationToken.ThrowIfCancellationRequested();

            var wildCardNode = orderedList[i];
            BaseBlockDefinition currentBlock = wildCardMethod;

            if (i > 0) {
                currentBlock = wildCardMethod.If("handlerInfo == null");
            }

            var matchWildCardMethod = 
                WriteWildCardMatchMethod(context with { Block = currentBlock, Node = wildCardNode});

            currentBlock.Assign(Invoke(matchWildCardMethod,_pathSpan, _index, _context)).To(handler);
        }

        wildCardMethod.Return(handler);

        return methodName;
    }

    private string WriteWildCardMatchMethod(RoutingTableContext context) {
        var methodName = GetNewMethodName(context);
        var wildCardMatchMethod = CreatePathTestMethod(context, methodName);
        var wildCardNode = context.Node;

        var newContext = context with {
            Block = wildCardMatchMethod
        };
        
        if (wildCardNode.ChildNodes.Count > 0) {
            GenerateWildCardChildMatch(newContext);
        }

        if (wildCardNode.WildCardNodes.Count > 0) {
            GenerateWildCardChildMatch(newContext);
        }

        GenerateWildCardLeafNode(newContext);

        return methodName;
    }

    private void GenerateWildCardChildMatch(RoutingTableContext context) {
        var wildCardMethod = context.Block;
        var wildCardNode = context.Node;
        
        var handlerInfo = wildCardMethod.Assign(StaticCast(
            KnownRequestTypes.IRequestHandler, Null())).ToVar("handler");

        var currentIndex = wildCardMethod.Assign(_index).ToVar("currentIndex");

        var whileBlock =
            wildCardMethod.While(LessThan(currentIndex, _pathSpan.Property("Length")));

        var pathCheckList = CreateIfStatementLogic(context);

        var ifStatementDefinition = whileBlock.If(And(pathCheckList));

        var currentPlusOne = Add(currentIndex, 1);

        if (wildCardNode.ChildNodes.Count > 0) {
            GenerateWildCardChildMatch(context);
        }

        if (wildCardNode.WildCardNodes.Count > 0) {
            ProcessWildCardNodes(context);
        }

        var matchIfHandlerBlock =
            ifStatementDefinition.If(NotEquals(handlerInfo, Null()));
        //
        // var newPathToken = New(KnownTypes.Requests.PathToken,
        //     QuoteString(wildCardNode.WildCardToken!),
        //     span.Invoke(
        //         "Slice",
        //         index,
        //         Subtract(currentIndex, index)).Invoke("ToString")
        // );
        //
        // matchIfHandlerBlock.AddIndentedStatement(
        //     handlerInfo.Property("PathTokens").Invoke(
        //         "Set",
        //         wildCardNode.WildCardDepth - 1,
        //         newPathToken
        //     ));

        matchIfHandlerBlock.Return(handlerInfo);

        whileBlock.AddIndentedStatement(Increment(currentIndex));
    }

    private void GenerateWildCardLeafNode(RoutingTableContext context) {
        var wildCardMethod = context.Block;
        var wildCardNode = context.Node;
        if (context.Node.LeafNodes.Count > 0) {
            var switchBlock = context.Block.Switch(_context.Property("RequestData").Property("Method"));

            foreach (var leafNode in wildCardNode.LeafNodes) {
                var caseStatement = switchBlock.AddCase(QuoteString(leafNode.Method));

                // todo
            }

            switchBlock.AddDefault().Return(Null());
        }
        else {
            wildCardMethod.Return(Null());
        }
    }

    
    private void ProcessLeafNodes(RoutingTableContext newContext) {
        if (newContext.Node.LeafNodes.Count == 0) {
            return;
        }
        
        var matchBlock = 
            newContext.Block.If(EqualsStatement(_pathSpan.Property("Length"),_index));

        var switchBlock = matchBlock.Switch(_context.Property("RequestData").Property("Method"));
        
        foreach (var leafNode in newContext.Node.LeafNodes) {
            var caseBlock = switchBlock.AddCase(QuoteString(leafNode.Method));

            ReturnHandlerStatement(newContext with{ Block = caseBlock }, leafNode);
        }
    }

    private void ReturnHandlerStatement(RoutingTableContext routingTableContext, RouteTreeLeafNode<RequestHandlerModel> leafNode) {
        var field = 
            routingTableContext.ClassDefinition.AddField(KnownRequestTypes.IRequestHandler, "_handler" + routingTableContext.ClassDefinition.Fields.Count);

        var assignStatement = NullCoalesceEqual(
            field.Instance,
            _factoryField.Instance.Invoke("GetHandler", 
                new StaticPropertyStatement(leafNode.Value.GenerateInvokeType, "HandlerInfo") { Indented = false }
                )
        );
        
        routingTableContext.Block.Return(assignStatement);
    }

    private IReadOnlyList<IOutputComponent> CreateIfStatementLogic(RoutingTableContext routingTableContext) {
        if (string.IsNullOrEmpty(routingTableContext.Node.Path)) {
            throw new ArgumentNullException(nameof(routingTableContext.Node.Path));
        }

        var routeNodePath = routingTableContext.Node.Path;
        var returnList = new List<IOutputComponent>();

        returnList.Add(GreaterThanOrEquals(
            _pathSpan.Property("Length"), Add(_index , routeNodePath.Length)));

        int characterIndex = 0;
        foreach (var pathChar in routeNodePath) {
            routingTableContext.CancellationToken.ThrowIfCancellationRequested();

            var upperChar = char.ToUpper(pathChar);

            var lowerEqualStatement = 
                EqualsStatement(
                    _pathSpan.Index(
                        Add(_index, characterIndex)), "'" + pathChar + "'");

            if (upperChar != pathChar) {
                var upperEqualStatement = EqualsStatement(
                    _pathSpan.Index(
                        Add(_index,characterIndex)), "'" + upperChar + "'");

                returnList.Add(Or(lowerEqualStatement, upperEqualStatement));
            }
            else {
                returnList.Add(lowerEqualStatement);
            }

            characterIndex++;
        }

        return returnList;
    }
    
    private MethodDefinition ProcessEmptyPathNode(RoutingTableContext routingTableContext) {
        if (routingTableContext.Node.ChildNodes.Count == 1) {
            return ProcessSingularNode(routingTableContext);
        }

        return WriteSwitchChildNode(routingTableContext);
    }
    
    private MethodDefinition CreatePathTestMethod(RoutingTableContext routingTableContext, string getNewMethodName) {
        var method = routingTableContext.ClassDefinition.AddMethod(getNewMethodName);

        method.AddParameter(_pathSpan);
        method.AddParameter(_index);
        method.AddParameter(_context);
        
        method.SetReturnType(KnownRequestTypes.IRequestHandler);
        
        return method;
    }

    private string GetNewMethodName(RoutingTableContext context) {
        var methodName = "Test_";

        if (string.IsNullOrWhiteSpace(context.Node.Path)) {
            methodName += "Switch";
        }
        else {
            methodName += SanitizePath(context.Node.Path);
        }

        var count = 0;
        var finalName = methodName;
        
        while (context.ClassDefinition.Methods.Any(m => m.Name == finalName)) {
            count++;
            finalName = $"{methodName}_{count}";
        }

        return finalName;
    }

    private string SanitizePath(string nodePath) {
        return
            nodePath.Replace("/", "Slash").Replace("-", "Dash").Replace(".", "Period").Replace("%", "Per");
    }

    private static RouteTreeNode<RequestHandlerModel> GetRoutingTree(ModuleEntryPointModel moduleEntry,
        IReadOnlyList<RequestHandlerModel> endPointModels, CancellationToken cancellationToken) {
        var generator = new RouteTreeGenerator<RequestHandlerModel>(cancellationToken);

        var basePath = GetBasePath(moduleEntry);
        
        return generator.GenerateTree(endPointModels.Select(
            m => new RouteTreeGenerator<RequestHandlerModel>.Entry(
                basePath + m.Name.Path,
                m.Name.Method,
                m
            )).ToList());
    }

    private static string GetBasePath(ModuleEntryPointModel appModel) {
        if (appModel.AttributeModels.Count > 0) {
            var basePathAttribute = appModel.AttributeModels.FirstOrDefault(
                model => model.TypeDefinition.Name.StartsWith("BasePath"));

            if (basePathAttribute != null) {
                return basePathAttribute.Arguments.FirstOrDefault()?.Value as string ?? string.Empty;
            }
        }

        return "";
    }
    
    /*


    private static string WriteRouteNode(ClassDefinition routingClass, RouteTreeNode<RequestHandlerModel> routeNode,
        int pathIndex, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var path = routeNode.Path;

        if (pathIndex > 0) {
            if (path.Length < 2) {
                path = "";
            }
            else {
                path = path.Substring(1);
            }
        }

        var routeMethodName = GetRouteMethodName(routingClass, path);

        var testMethod = routingClass.AddMethod(routeMethodName);
        testMethod.SetReturnType(KnownRequestTypes.IRequestHandlerInfo);

        var span = testMethod.AddParameter(typeof(ReadOnlySpan<char>), "charSpan");
        var index = testMethod.AddParameter(typeof(int), "index");
        var methodString = testMethod.AddParameter(typeof(string), "methodString");

        var handler =
            testMethod.Assign(Null()).ToLocal(KnownTypes.Web.RequestHandlerInfo.MakeNullable(), "handlerInfo");

        BaseBlockDefinition block = testMethod;

        if (!string.IsNullOrEmpty(path)) {
            var pathIfStatement = CreatePathIfStatement(span, routeNode.Path, cancellationToken);

            block = testMethod.If(And(pathIfStatement));

            block.AddIndentedStatement("index += " + path.Length);
        }

        if (routeNode.LeafNodes.Count > 0) {
            ProcessLeafNodes(routingClass, routeNode, block, span, index, methodString, cancellationToken);
        }

        if (routeNode.ChildNodes.Count > 0) {
            ProcessChildNodes(routingClass, routeNode, block, span, index, methodString, handler, cancellationToken);
        }

        if (routeNode.WildCardNodes.Count > 0) {
            ProcessWildCardNodes(routingClass, routeNode, block, span, index, methodString, handler, cancellationToken);
        }

        testMethod.Return(handler);

        return routeMethodName;
    }

    private static void ProcessChildNodes(ClassDefinition routingClass,
        RouteTreeNode<RequestHandlerModel> routeNode,
        BaseBlockDefinition block,
        ParameterDefinition span,
        IOutputComponent index,
        ParameterDefinition methodString,
        InstanceDefinition handler,
        CancellationToken cancellationToken) {
        var childMethod = "";

        if (routeNode.ChildNodes.Count == 1) {
            childMethod = WriteRouteNode(routingClass, routeNode.ChildNodes.First(), 0, cancellationToken);
        }
        else {
            childMethod = WriteSwitchChildNode(routingClass, routeNode, cancellationToken);
        }

        block.Assign(Invoke(childMethod, span, index, methodString)).To(handler);
    }

    private static string WriteSwitchChildNode(ClassDefinition routingClass,
        RouteTreeNode<RequestHandlerModel> routeNode, CancellationToken cancellationToken) {
        var switchMethodName = GetRouteMethodName(routingClass, routeNode.Path, "CaseStatement");

        var switchMethod = routingClass.AddMethod(switchMethodName);
        switchMethod.SetReturnType(KnownTypes.Web.RequestHandlerInfo.MakeNullable());
        var span = switchMethod.AddParameter(typeof(ReadOnlySpan<char>), "charSpan");
        var index = switchMethod.AddParameter(typeof(int), "index");
        var methodString = switchMethod.AddParameter(typeof(string), "methodString");

        var ifStatement = switchMethod.If("charSpan.Length > index");

        var switchStatement = ifStatement.Switch("charSpan[index]");

        foreach (var childNode in routeNode.ChildNodes) {
            cancellationToken.ThrowIfCancellationRequested();

            var lowerChar = char.ToLowerInvariant(childNode.Path.First());
            var upperChar = char.ToUpperInvariant(lowerChar);

            if (upperChar != lowerChar) {
                switchStatement.AddCase($"'{upperChar}'");
            }

            var caseStatement = switchStatement.AddCase($"'{lowerChar}'");

            var newMethodName = WriteRouteNode(routingClass, childNode, 1, cancellationToken);

            var invoke = Invoke(newMethodName, span, "index + 1", methodString);

            caseStatement.Return(invoke);
        }

        switchMethod.Return(Null());

        return switchMethodName;
    }

    private static void ProcessWildCardNodes(ClassDefinition routingClass,
        RouteTreeNode<RequestHandlerModel> routeNode,
        BaseBlockDefinition block,
        ParameterDefinition span,
        IOutputComponent index,
        ParameterDefinition methodString,
        InstanceDefinition handler,
        CancellationToken cancellationToken) {
        var ifBlock = block.If("handlerInfo == null");

        var wildCardMethod = WriteWildCardMethod(routingClass, routeNode, cancellationToken);

        var invoke = Invoke(wildCardMethod, span, index, methodString);

        ifBlock.Assign(invoke).To(handler);
    }

    private static string WriteWildCardMethod(
        ClassDefinition routingClass, RouteTreeNode<RequestHandlerModel> routeNode,
        CancellationToken cancellationToken) {
        var methodName = GetRouteMethodName(routingClass, routeNode.Path, "WildCard");

        var wildCardMethod = routingClass.AddMethod(methodName);

        wildCardMethod.SetReturnType(KnownTypes.Web.RequestHandlerInfo.MakeNullable());
        var span = wildCardMethod.AddParameter(typeof(ReadOnlySpan<char>), "charSpan");
        var index = wildCardMethod.AddParameter(typeof(int), "index");
        var methodString = wildCardMethod.AddParameter(typeof(string), "methodString");

        var handler =
            wildCardMethod.Assign(Null()).ToLocal(KnownTypes.Web.RequestHandlerInfo.MakeNullable(), "handlerInfo");

        var orderedList =
            routeNode.WildCardNodes.OrderByDescending(n => n.Path).ToList();

        for (var i = 0; i < orderedList.Count; i++) {
            cancellationToken.ThrowIfCancellationRequested();

            var wildCardNode = orderedList[i];
            BaseBlockDefinition currentBlock = wildCardMethod;

            if (i > 0) {
                currentBlock = wildCardMethod.If("handlerInfo == null");
            }

            var matchWildCardMethod = WriteWildCardMatchMethod(routingClass, wildCardNode, cancellationToken);

            currentBlock.Assign(Invoke(matchWildCardMethod, span, index, methodString)).To(handler);
        }

        wildCardMethod.Return(handler);

        return methodName;
    }

    private static string WriteWildCardMatchMethod(ClassDefinition routingClass,
        RouteTreeNode<RequestHandlerModel> wildCardNode, CancellationToken cancellationToken) {
        var methodName = GetRouteMethodName(routingClass, wildCardNode.Path, "WildCardMatch");

        var wildCardMethod = routingClass.AddMethod(methodName);

        wildCardMethod.SetReturnType(KnownTypes.Web.RequestHandlerInfo.MakeNullable());
        var span = wildCardMethod.AddParameter(typeof(ReadOnlySpan<char>), "charSpan");
        var index = wildCardMethod.AddParameter(typeof(int), "index");
        var methodString = wildCardMethod.AddParameter(typeof(string), "methodString");

        if (wildCardNode.ChildNodes.Count > 0) {
            GenerateWildCardChildMatch(
                routingClass, wildCardNode, wildCardMethod, methodString, span, index, cancellationToken);
        }

        if (wildCardNode.WildCardNodes.Count > 0) {
            GenerateWildCardChildMatch(
                routingClass, wildCardNode, wildCardMethod, methodString, span, index, cancellationToken);
        }

        GenerateWildCardLeafNode(routingClass, wildCardNode, wildCardMethod, methodString, span, index);

        return methodName;
    }

    private static void GenerateWildCardChildMatch(ClassDefinition routingClass,
        RouteTreeNode<RequestHandlerModel> wildCardNode,
        MethodDefinition wildCardMethod,
        ParameterDefinition methodString,
        ParameterDefinition span,
        ParameterDefinition index,
        CancellationToken cancellationToken) {
        var handlerInfo = wildCardMethod.Assign(StaticCast(
            KnownTypes.Web.RequestHandlerInfo.MakeNullable(), Null())).ToVar("handlerInfo");

        var currentIndex = wildCardMethod.Assign(index).ToVar("currentIndex");

        var whileBlock =
            wildCardMethod.While(LessThan(currentIndex, span.Property("Length")));

        var pathCheck = CreatePathIfStatement(
            span, wildCardNode.Path, cancellationToken, currentIndex.Name);

        var ifStatement = whileBlock.If(And(pathCheck));

        var currentPlusOne = Add(currentIndex, 1);

        if (wildCardNode.ChildNodes.Count > 0) {
            ProcessChildNodes(
                routingClass,
                wildCardNode,
                ifStatement,
                span,
                currentPlusOne,
                methodString,
                handlerInfo,
                cancellationToken);
        }

        if (wildCardNode.WildCardNodes.Count > 0) {
            ProcessWildCardNodes(
                routingClass,
                wildCardNode,
                ifStatement,
                span,
                currentPlusOne,
                methodString,
                handlerInfo,
                cancellationToken
            );
        }

        var matchIfHandlerBlock =
            ifStatement.If(NotEquals(handlerInfo, Null()));

        var newPathToken = New(KnownTypes.Requests.PathToken,
            QuoteString(wildCardNode.WildCardToken!),
            span.Invoke(
                "Slice",
                index,
                Subtract(currentIndex, index)).Invoke("ToString")
        );

        matchIfHandlerBlock.AddIndentedStatement(
            handlerInfo.Property("PathTokens").Invoke(
                "Set",
                wildCardNode.WildCardDepth - 1,
                newPathToken
            ));

        matchIfHandlerBlock.Return(handlerInfo);

        whileBlock.AddIndentedStatement(Increment(currentIndex));
    }

    private static void GenerateWildCardLeafNode(ClassDefinition routingClass,
        RouteTreeNode<RequestHandlerModel> wildCardNode,
        MethodDefinition wildCardMethod, ParameterDefinition methodString, ParameterDefinition span,
        ParameterDefinition index) {
        if (wildCardNode.LeafNodes.Count > 0) {
            var switchBlock = wildCardMethod.Switch(methodString);

            foreach (var leafNode in wildCardNode.LeafNodes) {
                var caseStatement = switchBlock.AddCase(QuoteString(leafNode.Method));

                var field =
                    routingClass.AddField(leafNode.Value.InvokeHandlerType.MakeNullable(),
                        "_field" + leafNode.Value.InvokeHandlerType.Name);

                var coalesceHandler = NullCoalesceEqual(field.Instance,
                    New(leafNode.Value.InvokeHandlerType, "_rootServiceProvider"));

                coalesceHandler.PrintParentheses = false;

                var pathToken = New(KnownTypes.Requests.PathToken,
                    QuoteString(wildCardNode.WildCardToken!),
                    span.Invoke("Slice", index).Invoke("ToString")
                );

                IOutputComponent pathTokensCollection =
                    New(KnownTypes.Requests.PathTokenCollection,
                        wildCardNode.WildCardDepth,
                        pathToken
                    );

                caseStatement.Return(
                    New(KnownTypes.Web.RequestHandlerInfo,
                        coalesceHandler,
                        pathTokensCollection));
            }

            switchBlock.AddDefault().Return(Null());
        }
        else {
            wildCardMethod.Return(Null());
        }
    }

    private static void ProcessLeafNodes(ClassDefinition routingClass,
        RouteTreeNode<RequestHandlerModel> routeNode,
        BaseBlockDefinition block,
        ParameterDefinition span,
        ParameterDefinition index,
        ParameterDefinition methodString, CancellationToken cancellationToken) {
        var ifLengthMatch = block.If("charSpan.Length == index");

        var switchStatement = ifLengthMatch.Switch(methodString);

        foreach (var leafNode in routeNode.LeafNodes) {
            cancellationToken.ThrowIfCancellationRequested();

            var caseStatement = switchStatement.AddCase(QuoteString(leafNode.Method));

            var field =
                routingClass.AddField(leafNode.Value.InvokeHandlerType.MakeNullable(),
                    "_field" + leafNode.Value.InvokeHandlerType.Name);

            var coalesceHandler = NullCoalesceEqual(field.Instance,
                New(leafNode.Value.InvokeHandlerType, "_rootServiceProvider"));

            coalesceHandler.PrintParentheses = false;

            IOutputComponent pathTokensCollection = EmptyTokens;

            if (routeNode.WildCardDepth > 0) {
                pathTokensCollection = New(KnownTypes.Requests.PathTokenCollection, routeNode.WildCardDepth);
            }

            caseStatement.Return(
                New(
                    KnownTypes.Web.RequestHandlerInfo,
                    coalesceHandler,
                    pathTokensCollection));
        }

        switchStatement.AddDefault().Return(Null());
    }

    private static IReadOnlyList<IOutputComponent> CreatePathIfStatement(
        ParameterDefinition span,
        string routeNodePath,
        CancellationToken cancellationToken,
        string indexName = "index") {
        var returnList = new List<IOutputComponent>();

        returnList.Add(GreaterThanOrEquals(span.Property("Length"), indexName + " + " + routeNodePath.Length));

        int index = 0;
        foreach (var pathChar in routeNodePath) {
            cancellationToken.ThrowIfCancellationRequested();

            var upperChar = char.ToUpper(pathChar);

            var lowerEqualStatement = EqualsStatement($"{span.Name}[{indexName} + {index}]", "'" + pathChar + "'");

            if (upperChar != pathChar) {
                var upperEqualStatement = EqualsStatement($"{span.Name}[{indexName} + {index}]", "'" + upperChar + "'");

                returnList.Add(Or(lowerEqualStatement, upperEqualStatement));
            }
            else {
                returnList.Add(lowerEqualStatement);
            }

            index++;
        }

        return returnList;
    }

    private static string GetRouteMethodName(ClassDefinition routingClass,
        string path, string? postfix = null) {
        if (string.IsNullOrEmpty(path)) {
            path = "NoPath";
        }

        var baseName = "TestPath_" +
                       path.Replace("/", "Slash").Replace("-", "Dash").Replace(".", "Period").Replace("%", "Per");

        var testMethodName = baseName + postfix;
        var count = 1;
        while (routingClass.Methods.Any(m => m.Name == testMethodName)) {
            testMethodName = baseName + (++count);
        }

        return testMethodName;
    }

    private static RouteTreeNode<RequestHandlerModel> GetRoutingNodes(ModuleEntryPointModel appModel, IReadOnlyList<RequestHandlerModel> endPointModels, CancellationToken cancellationToken) {
        var generator = new RouteTreeGenerator<RequestHandlerModel>(cancellationToken);

        var basePath = GetBasePath(appModel);
        
        return generator.GenerateTree(endPointModels.Select(
            m => new RouteTreeGenerator<RequestHandlerModel>.Entry(
                basePath + m.Name.Path,
                m.Name.Method,
                m
            )).ToList());
    }

    private static string GetBasePath(ModuleEntryPointModel appModel) {
        if (appModel.AttributeModels != null) {
            var basePathAttribute = appModel.AttributeModels.FirstOrDefault(model => model.TypeDefinition.Name.StartsWith("BasePath"));

            if (basePathAttribute != null) {
                var basePath = basePathAttribute.Arguments.Split(',').First();

                return basePath.Trim('"');
            }
        }

        return "";
    }
    */
}