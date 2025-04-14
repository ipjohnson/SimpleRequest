using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Routing.Tree;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Routing;

public class RoutingTableGenerator {
    private static readonly string _handlerString = "handler";
    private readonly ParameterDefinition _pathSpan = new(TypeDefinition.Get(typeof(ReadOnlySpan<Char>)), "path");
    private readonly ParameterDefinition _index = new(TypeDefinition.Get(typeof(int)), "index");
    private readonly ParameterDefinition _context = new(KnownRequestTypes.IRequestContext, "context");
    private readonly FieldDefinition _factoryField;
    private readonly string _handlerType;

    public RoutingTableGenerator(FieldDefinition factory, string handlerType) {
        _factoryField = factory;
        _handlerType = handlerType;
    }

    private record RoutingTableContext(
        RouteTreeNode<RequestHandlerModel> Node,
        int Index,
        ClassDefinition ClassDefinition,
        ModuleEntryPointModel ModuleEntry,
        BaseBlockDefinition Block,
        CancellationToken CancellationToken);

    public void GenerateRoutingTableMethods(
        ClassDefinition classDefinition, ModuleEntryPointModel moduleEntry, IReadOnlyList<RequestHandlerModel> requestModels, CancellationToken cancellationToken) {

        GenerateGetRequestHandlerMethod(classDefinition, moduleEntry, requestModels, cancellationToken);

        GenerateGetHandlersMethod(classDefinition, moduleEntry, requestModels, cancellationToken);
    }

    private void GenerateGetHandlersMethod(ClassDefinition classDefinition, ModuleEntryPointModel moduleEntry, IReadOnlyList<RequestHandlerModel> requestModels, CancellationToken cancellationToken) {
        var handlerMethods =
            classDefinition.Methods.Where(m => m.Name.StartsWith("GetHandler")).ToArray();

        var method = classDefinition.AddMethod("GetHandlers");
        method.SetReturnType(new GenericTypeDefinition(typeof(IEnumerable<>), new[] {
            KnownRequestTypes.IRequestHandler
        }));

        if (handlerMethods.Length == 0) {
            method.AddIndentedStatement("yield break;");
            return;
        }

        foreach (var methodDefinition in handlerMethods) {
            method.AddIndentedStatement(YieldReturn(Invoke(methodDefinition.Name)));
        }
    }

    private void GenerateGetRequestHandlerMethod(ClassDefinition classDefinition, ModuleEntryPointModel moduleEntry, IReadOnlyList<RequestHandlerModel> requestModels, CancellationToken cancellationToken) {
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

        return ProcessSingularNode(routingTableContext, true);
    }

    private MethodDefinition ProcessSingularNode(RoutingTableContext routingTableContext, bool applyIf) {
        var method = CreatePathTestMethod(routingTableContext, GetNewMethodName(routingTableContext));
        BaseBlockDefinition block;

        var handler = method.Assign(Null()).ToLocal(KnownRequestTypes.IRequestHandler, _handlerString);
        if (applyIf) {
            var ifStatementLogic = CreateIfStatementLogic(routingTableContext);

            block = method.If(And(ifStatementLogic));
        }
        else {
            block = method;
        }

        block.Assign(Add(_index, routingTableContext.Node.Path.Length)).To(_index);

        var newContext = routingTableContext with {
            Index = routingTableContext.Index + routingTableContext.Node.Path.Length,
            Block = block
        };

        ProcessLeafNodes(newContext);

        ProcessChildNodes(newContext);

        ProcessWildCardNodes(newContext, false);

        method.Return(handler);

        return method;
    }

    private void ProcessChildNodes(RoutingTableContext context, bool assignToVar = false) {
        if (context.Node.ChildNodes.Count == 0) {
            return;
        }

        MethodDefinition childMethod;

        if (context.Node.ChildNodes.Count == 1) {
            childMethod = WriteCurrentNode(context with {
                Node = context.Node.ChildNodes.First()
            });
        }
        else {
            childMethod = WriteSwitchChildNode(context);
        }

        if (assignToVar) {
            context.Block.Assign(Invoke(childMethod.Name, _pathSpan, _index, _context)).ToVar(_handlerString);
        }
        else {
            context.Block.Assign(Invoke(childMethod.Name, _pathSpan, _index, _context)).To(_handlerString);
        }
        context.Block.If(NotEquals(_handlerString, Null())).Return(_handlerString);
    }

    private MethodDefinition WriteSwitchChildNode(RoutingTableContext context) {
        var switchMethodName = GetNewMethodName(context);
        var method = CreatePathTestMethod(context, switchMethodName);

        var ifStatement = method.If(
            GreaterThan(_pathSpan.Property("Length"), _index));

        var switchStatement = ifStatement.Switch(_pathSpan.Index(_index));

        foreach (var childNode in context.Node.ChildNodes) {
            context.CancellationToken.ThrowIfCancellationRequested();

            var lowerChar = char.ToLowerInvariant(childNode.Path.First());
            var upperChar = char.ToUpperInvariant(lowerChar);

            if (upperChar != lowerChar) {
                switchStatement.AddCase($"'{upperChar}'");
            }

            var caseStatement = switchStatement.AddCase($"'{lowerChar}'");

            var newMethodName = ProcessSingularNode(context with {
                Node = childNode
            }, false);

            var invoke = Invoke(newMethodName.Name, _pathSpan, _index, _context);

            caseStatement.Return(invoke);
        }

        method.Return(Null());

        return method;
    }

    private void ProcessWildCardNodes(
        RoutingTableContext newContext, bool useCurrentIndex) {
        if (newContext.Node.WildCardNodes.Count == 0) {
            return;
        }

        var wildCardMethod = WriteWildCardMethod(newContext);

        object index = useCurrentIndex ? "currentIndex + 1" : _index;

        var invoke = Invoke(wildCardMethod, _pathSpan, index, _context);


        newContext.Block.Assign(invoke).To(_handlerString);
    }

    private string WriteWildCardMethod(
        RoutingTableContext context) {
        var methodName = GetNewMethodName(context);
        var wildCardMethod = CreatePathTestMethod(context, methodName);

        var handler =
            wildCardMethod.Assign(Null()).ToLocal(KnownRequestTypes.IRequestHandler, _handlerString);

        var orderedList =
            context.Node.WildCardNodes.OrderByDescending(n => n.Path).ToList();

        for (var i = 0; i < orderedList.Count; i++) {
            context.CancellationToken.ThrowIfCancellationRequested();

            var wildCardNode = orderedList[i];
            BaseBlockDefinition currentBlock = wildCardMethod;

            if (i > 0) {
                currentBlock = wildCardMethod.If(
                    EqualsStatement(handler, Null()));
            }

            var matchWildCardMethod =
                WriteWildCardMatchMethod(context with {
                    Block = currentBlock,
                    Node = wildCardNode
                });

            currentBlock.Assign(Invoke(matchWildCardMethod, _pathSpan, _index, _context)).To(handler);
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

        wildCardMethod.Assign(StaticCast(
            KnownRequestTypes.IRequestHandler, Null())).ToVar(_handlerString);

        var currentIndex = wildCardMethod.Assign(_index).ToVar("currentIndex");

        var whileBlock =
            wildCardMethod.While(LessThan(currentIndex, _pathSpan.Property("Length")));

        var pathCheckList = CreateIfStatementLogic(context, currentIndex);

        var ifStatementDefinition = whileBlock.If(And(pathCheckList));

        if (wildCardNode.ChildNodes.Count > 0) {
            ProcessChildNodes(context with {
                Block = ifStatementDefinition
            });
        }

        if (wildCardNode.WildCardNodes.Count > 0) {
            ProcessWildCardNodes(context with {
                Block = ifStatementDefinition
            }, true);
        }

        var matchIfHandlerBlock =
            ifStatementDefinition.If(NotEquals(_handlerString, Null()));

        AssignPathToken(currentIndex, matchIfHandlerBlock, wildCardNode);

        matchIfHandlerBlock.Return(_handlerString);

        whileBlock.AddIndentedStatement(Increment(currentIndex));
    }

    private void AssignPathToken(
        InstanceDefinition? currentIndex,
        BaseBlockDefinition blockDefinition,
        RouteTreeNode<RequestHandlerModel> wildCardNode) {
        IOutputComponent sliceStatement;

        if (currentIndex != null) {
            sliceStatement = _pathSpan.Invoke(
                "Slice", _index, Subtract(currentIndex, _index));
        }
        else {
            sliceStatement = _pathSpan.Invoke("Slice", _index);
        }

        sliceStatement = sliceStatement.Invoke("ToString");

        blockDefinition.AddIndentedStatement(
            _context.Property("RequestData").Property("PathTokenCollection").Invoke("Set",
                QuoteString(wildCardNode.WildCardToken ?? "token"),
                sliceStatement)
        );
    }

    private void GenerateWildCardLeafNode(RoutingTableContext context) {
        var wildCardMethod = context.Block;
        var wildCardNode = context.Node;
        
        if (context.Node.LeafNodes.Count > 0) {
            var handler = 
                wildCardMethod.Assign(Null()).ToLocal(KnownRequestTypes.IRequestHandler, _handlerString);
            var switchBlock = context.Block.Switch(_context.Property("RequestData").Property("Method"));

            var caseEnumerable = GroupNodesByMethod(context);

            foreach (var grouping in caseEnumerable) {
                var caseStatement = switchBlock.AddCase(QuoteString(grouping.Key));

                foreach (var leafNode in grouping) {
                    var ifStatement =
                        caseStatement.IfNotNullAssign(
                            handler,
                            InvokeHandlerStatement(context with {
                                    Block = caseStatement
                                }, leafNode
                            )
                        );
                
                    AssignPathToken(null, ifStatement, wildCardNode);
                    ifStatement.Return(handler);
                }
                
                caseStatement.Return(Null());
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
            newContext.Block.If(EqualsStatement(_pathSpan.Property("Length"), _index));

        var switchBlock = matchBlock.Switch(_context.Property("RequestData").Property("Method"));

        var caseEnumerable = GroupNodesByMethod(newContext);

        foreach (var methodGrouping in caseEnumerable) {
            var caseBlock = switchBlock.AddCase(QuoteString(methodGrouping.Key));

            var methodsList = methodGrouping.ToList();

            IOutputComponent? returnStatement = null;

            if (methodsList.Count > 1) {
                var orderedEnumerable = OrderHandlersByExtraMatchAttribute(methodsList);

                foreach (var node in orderedEnumerable) {
                    var invokeStatement = InvokeHandlerStatement(newContext with {
                        Block = caseBlock
                    }, node);
                    if (returnStatement == null) {
                        returnStatement = invokeStatement;
                    }
                    else {
                        returnStatement = NullCoalesce(
                            invokeStatement,
                            returnStatement
                        );
                    }
                }
            }
            else {
                returnStatement = InvokeHandlerStatement(newContext with {
                    Block = caseBlock
                }, methodsList.First());
            }

            caseBlock.Return(returnStatement);
        }
    }

    private static IOrderedEnumerable<RouteTreeLeafNode<RequestHandlerModel>> OrderHandlersByExtraMatchAttribute(List<RouteTreeLeafNode<RequestHandlerModel>> methodsList) {
        return methodsList.OrderBy(
            leafNode => leafNode.Value.Filters.Count(a => a.ImplementedInterfaces.Any(
                interfaceType => interfaceType.Equals(KnownRequestTypes.IExtendedRouteMatch))));
    }

    private static IEnumerable<IGrouping<string, RouteTreeLeafNode<RequestHandlerModel>>> GroupNodesByMethod(RoutingTableContext newContext) {
        var caseEnumerable =
            newContext.Node.LeafNodes.GroupBy(l => l.Method);
        return caseEnumerable;
    }

    private IOutputComponent InvokeHandlerStatement(RoutingTableContext routingTableContext, RouteTreeLeafNode<RequestHandlerModel> leafNode) {
        var count = routingTableContext.ClassDefinition.Fields.Count;
        var field =
            routingTableContext.ClassDefinition.AddField(KnownRequestTypes.IRequestHandler, "_handler" + count);

        var createMethod = routingTableContext.ClassDefinition.AddMethod("GetHandler" + count);

        createMethod.Modifiers |= ComponentModifier.Private;
        createMethod.SetReturnType(KnownRequestTypes.IRequestHandler);

        IOutputComponent assignStatement = NullCoalesceEqual(
            field.Instance,
            _factoryField.Instance.Invoke("GetHandler",
                new StaticPropertyStatement(leafNode.Value.GenerateInvokeType, "HandlerInfo") {
                    Indented = false
                },
                QuoteString(_handlerType)
            )
        );

        var parameters = new List<object>();
        if (leafNode.Value.Filters.Any(
                a => a.ImplementedInterfaces.Any(
                    i => i.Equals(KnownRequestTypes.IExtendedRouteMatch)))) {
            assignStatement = new WrapStatement(assignStatement, "(", ")");
            
            var parameter = createMethod.AddParameter(KnownRequestTypes.IRequestContext, "context");
            
            parameter.DefaultValue = Null();
            
            assignStatement = 
                assignStatement.Invoke("ReturnMatch", parameter);
            
            parameters.Add(parameter);
        }
        
        createMethod.Return(assignStatement);

        return Invoke("GetHandler" + count, parameters.ToArray());
    }

    private IReadOnlyList<IOutputComponent> CreateIfStatementLogic(RoutingTableContext routingTableContext, InstanceDefinition? indexValue = null) {
        if (string.IsNullOrEmpty(routingTableContext.Node.Path)) {
            throw new ArgumentNullException(nameof(routingTableContext.Node.Path));
        }

        var routeNodePath = routingTableContext.Node.Path;
        var returnList = new List<IOutputComponent>();

        indexValue ??= _index;

        returnList.Add(GreaterThanOrEquals(
            _pathSpan.Property("Length"), Add(indexValue, routeNodePath.Length)));

        int characterIndex = 0;
        foreach (var pathChar in routeNodePath) {
            routingTableContext.CancellationToken.ThrowIfCancellationRequested();

            var upperChar = char.ToUpper(pathChar);

            var lowerEqualStatement =
                EqualsStatement(
                    _pathSpan.Index(
                        Add(indexValue, characterIndex)), "'" + pathChar + "'");

            if (upperChar != pathChar) {
                var upperEqualStatement = EqualsStatement(
                    _pathSpan.Index(
                        Add(indexValue, characterIndex)), "'" + upperChar + "'");

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
            return ProcessSingularNode(routingTableContext, true);
        }

        return WriteSwitchChildNode(routingTableContext);
    }

    private MethodDefinition CreatePathTestMethod(RoutingTableContext routingTableContext, string getNewMethodName) {
        var method = routingTableContext.ClassDefinition.AddMethod(getNewMethodName);

        method.Modifiers |= ComponentModifier.Private;
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
}