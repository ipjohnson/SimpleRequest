﻿
using SimpleRequest.SourceGenerator.Impl.Routing.Tree;

namespace SimpleRequest.SourceGenerator.Tests.Impl.Routing.Tree;

public class SimpleRoutingTreeTests {
    
    [Fact]
    public void SimpleRoutesAllLeafs() {
        var routes = new List<RouteTreeGenerator<int>.Entry> {
            new("/1", "GET", 1),
            new("/2", "GET", 2),
            new("/3", "GET", 3),
            new("/4", "GET", 4),
            new("/5", "GET", 5)
        };

        var generator = new RouteTreeGenerator<int>();

        var routeTree = generator.GenerateTree(routes);

        Assert.Equal("/", routeTree.Path);
        Assert.Equal(5, routeTree.ChildNodes.Count);

        for (var i = 0; i < routeTree.ChildNodes.Count; i++) {
            var childNode = routeTree.ChildNodes[i];
            Assert.Equal((i + 1).ToString(), childNode.Path);

            Assert.Single(childNode.LeafNodes);
            var leafNode = childNode.LeafNodes.First();
            Assert.Equal(i + 1, leafNode.Value);
        }
    }

    [Fact]
    public void SimpleRouteSomeOverlap() {
        var routes = new List<RouteTreeGenerator<int>.Entry> {
            new("/1", "GET", 1),
            new("/12", "GET", 12),
            new("/2", "GET", 2),
            new("/22", "GET", 22),
            new("/3", "GET", 3),
            new("/32", "GET", 32),
            new("/4", "GET", 4),
            new("/42", "GET", 42),
            new("/5", "GET", 5),
            new("/52", "GET", 52)
        };

        var generator = new RouteTreeGenerator<int>();

        var routeTree = generator.GenerateTree(routes);

        Assert.Equal("/", routeTree.Path);
        Assert.Equal(5, routeTree.ChildNodes.Count);

        for (var i = 0; i < routeTree.ChildNodes.Count; i++) {
            var assertValue = i + 1;
            var childNode = routeTree.ChildNodes[i];
            Assert.Equal(assertValue.ToString(), childNode.Path);

            Assert.Single(childNode.LeafNodes);
            var leafNode = childNode.LeafNodes.First();
            Assert.Equal(assertValue, leafNode.Value);

            Assert.Single(childNode.ChildNodes);
            var nestedChild = childNode.ChildNodes.First();
            Assert.Equal(2.ToString(), nestedChild.Path);

            Assert.Single(nestedChild.LeafNodes);
            var nestedLeafNode = nestedChild.LeafNodes.First();
            Assert.Equal((assertValue * 10) + 2, nestedLeafNode.Value);
        }
    }

    [Fact]
    public void SimpleRouteDifferentOverlap() {
        var routes = new List<RouteTreeGenerator<int>.Entry> {
            new("/Home", "GET", 1),
            new("/Header", "GET", 2),
            new("/api/person", "GET", 3),
            new("/api/person/view", "GET", 4),
        };

        var generator = new RouteTreeGenerator<int>();

        var routeTree = generator.GenerateTree(routes);
    }


    [Fact]
    public void SimpleRouteTypedNotes() {
        var routes = new List<RouteTreeGenerator<int>.Entry> {
            new("/Typed/notes", "GET", 1),
            new("/Typed/notes/{id}", "GET", 2),
        };

        var generator = new RouteTreeGenerator<int>();

        var routeTree = generator.GenerateTree(routes);
    }

    [Fact]
    public void SimpleRouteMoreOverlap() {
        var routes = new List<RouteTreeGenerator<int>.Entry> {
            new("/1", "GET", 1),
            new("/1233", "GET", 1233),
            new("/1234", "GET", 1234),
            new("/1235", "GET", 1235),
            new("/1236", "GET", 1236),
        };

        var generator = new RouteTreeGenerator<int>();

        var routeTree = generator.GenerateTree(routes);

        Assert.Equal("/1", routeTree.Path);
        Assert.Single(routeTree.ChildNodes);

        var twoNode = routeTree.ChildNodes.First();
        Assert.Equal("2", twoNode.Path);

        var threeNode = twoNode.ChildNodes.First();
        Assert.Equal("3", threeNode.Path);

        Assert.Equal(4, threeNode.ChildNodes.Count);

        for (var i = 0; i < 4; i++) {
            var assertValue = i + 3;
            var childNode = threeNode.ChildNodes[i];
            Assert.Equal(assertValue.ToString(), childNode.Path);
            Assert.Single(childNode.LeafNodes);
            Assert.Equal(1230 + assertValue, childNode.LeafNodes.First().Value);
        }
    }
}