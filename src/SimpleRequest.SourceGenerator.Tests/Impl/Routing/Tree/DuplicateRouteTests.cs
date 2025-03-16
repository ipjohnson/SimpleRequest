using SimpleRequest.SourceGenerator.Impl.Routing.Tree;

namespace SimpleRequest.SourceGenerator.Tests.Impl.Routing.Tree;

public class DuplicateRouteTests {
    [Fact]
    public void SimpleRoutesAllLeafs() {
        var routes = new List<RouteTreeGenerator<int>.Entry> {
            new("/1", "GET", 1),
            new("/1", "GET", 2),
            new("/1", "GET", 3),
            new("/1", "GET", 4),
            new("/1", "GET", 5)
        };

        var generator = new RouteTreeGenerator<int>();

        var routeTree = generator.GenerateTree(routes);

        Assert.Equal("/1", routeTree.Path);
        Assert.Equal(5, routeTree.LeafNodes.Count);

        for (var i = 0; i < routeTree.LeafNodes.Count; i++) {
            var leafNode = routeTree.LeafNodes[i];
            Assert.Equal(i + 1, leafNode.Value);
        }
    }
}