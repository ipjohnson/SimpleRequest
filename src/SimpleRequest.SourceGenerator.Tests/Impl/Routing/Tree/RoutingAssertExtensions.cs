

using SimpleRequest.SourceGenerator.Impl.Routing.Tree;

namespace SimpleRequest.SourceGenerator.Tests.Impl.Routing.Tree;

public static class RoutingAssertExtensions {
    public static void AssertNoChildren<T>(this RouteTreeNode<T> node) {
        Assert.Empty(node.ChildNodes);
    }

    public static void AssertNoLeafNodes<T>(this RouteTreeNode<T> node) {
        Assert.Empty(node.LeafNodes);
    }

    public static void AssertNoWildCardNodes<T>(this RouteTreeNode<T> node) {
        Assert.Empty(node.WildCardNodes);
    }

    public static void AssertCount<T>(this IReadOnlyList<T> list, int count) {
        Assert.Equal(count, list.Count);
    }

    public static void AssertPath<T>(this RouteTreeNode<T> node, string path) {
        Assert.Equal(path, node.Path);
    }
}