
using SimpleRequest.SourceGenerator.Impl.Routing.Tree;

namespace SimpleRequest.SourceGenerator.Tests.Impl.Routing.Tree;

public class SimpleWildCardRoutingTests {
    [Fact]
    public void SingleWildCardRoute() {
        var routes = new List<RouteTreeGenerator<string>.Entry> { new("/api/person/{id}", "GET", "Person"), };

        var generator = new RouteTreeGenerator<string>();

        var routeTree = generator.GenerateTree(routes);

        routeTree.AssertPath("/api/person/");
        routeTree.AssertNoLeafNodes();
        routeTree.WildCardNodes.AssertCount(1);
        routeTree.AssertNoChildren();

        var wildCardNode = routeTree.WildCardNodes[0];
        wildCardNode.AssertNoChildren();
        wildCardNode.AssertNoWildCardNodes();
        wildCardNode.LeafNodes.AssertCount(1);
        Assert.Equal(1, wildCardNode.WildCardDepth);

        var leafNode = wildCardNode.LeafNodes[0];

        Assert.Equal("GET", leafNode.Method);
        Assert.Equal("Person", leafNode.Value);
    }

    [Fact]
    public void WildCardDashTest() {
        var routes = new List<RouteTreeGenerator<int>.Entry> {
            new("get-notes-model", "POST", 1),
            new("get-notes-{id}", "POST", 2),

        };

        var generator = new RouteTreeGenerator<int>();

        var routeTree = generator.GenerateTree(routes);
        
    }
    
    [Fact]
    public void DoubleWildCardRoute() {
        var routes =
            new List<RouteTreeGenerator<string>.Entry> { new("/api/company/{company}/person/{id}", "GET", "Person"), };

        var generator = new RouteTreeGenerator<string>();

        var routeTree = generator.GenerateTree(routes);

        routeTree.AssertPath("/api/company/");
        routeTree.ChildNodes.AssertCount(0);
        routeTree.AssertNoLeafNodes();
        routeTree.WildCardNodes.AssertCount(1);

        var wildCardNode = routeTree.WildCardNodes[0];
        wildCardNode.ChildNodes.AssertCount(1);
        wildCardNode.AssertNoWildCardNodes();
        wildCardNode.AssertNoLeafNodes();
        Assert.Equal(1, wildCardNode.WildCardDepth);

        var wildChild = wildCardNode.ChildNodes.First();
        wildChild.AssertPath("person/");
        wildChild.AssertNoLeafNodes();
        wildChild.AssertNoChildren();
        wildChild.WildCardNodes.AssertCount(1);
        Assert.Equal(1, wildChild.WildCardDepth);

        var secondWildNode = wildChild.WildCardNodes.First();
        secondWildNode.AssertNoChildren();
        secondWildNode.AssertNoWildCardNodes();
        secondWildNode.LeafNodes.AssertCount(1);

        var leafNode = secondWildNode.LeafNodes.First();

        Assert.Equal("GET", leafNode.Method);
        Assert.Equal("Person", leafNode.Value);
    }

    [Fact]
    public void ParseMultipleEntries() {
        var routes = new List<RouteTreeGenerator<string>.Entry> {
            new("/recipes/{userId}", "GET", "Recipe"), new("/recipe/{userId}/{recipeId}", "GET", "Recipe"),
        };

        var generator = new RouteTreeGenerator<string>();

        var routeTree = generator.GenerateTree(routes);
    }
}