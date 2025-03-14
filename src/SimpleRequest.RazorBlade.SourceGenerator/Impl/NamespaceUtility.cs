using DependencyModules.SourceGenerator.Impl.Models;

namespace SimpleRequest.RazorBlade.SourceGenerator.Impl;

public class NamespaceUtility {
    public static string GetTemplateNamespace(ModuleEntryPointModel model, string filePath) {
        var (rootDirectory, rootNamespace) = GetRootPathAndRootNamespace(model, filePath);

        var relativeNamespace = GetRelativeNamespace(rootDirectory, filePath);

        if (string.IsNullOrEmpty(relativeNamespace)) {
            return rootNamespace;
        }
        
        return rootNamespace + "." + relativeNamespace;
    }

    private static string GetRelativeNamespace(string rootDirectory, string filePath) {
        var fileName = Path.GetFileName(filePath);
        var pathLength = filePath.Length - fileName.Length - 1;

        if (pathLength == 0) {
            return "";
        }
        
        var path = filePath.Substring(0, pathLength);

        if (rootDirectory.Length > path.Length) {
            return "";
        }
        
        var relativePath = path.Substring(rootDirectory.Length);
        var relativeNamespace = relativePath.Replace(Path.DirectorySeparatorChar, '.');
        
        return relativeNamespace;
    }

    private static (string rootDirectory, string rootNamespace) GetRootPathAndRootNamespace(ModuleEntryPointModel model, string filePath) {
        var rootDirectory = "";
        var rootNamespace = "";
        
        
        var entryPointSplit = model.FileLocation.Split(Path.DirectorySeparatorChar);
        var fileLocationSplit = filePath.Split(Path.DirectorySeparatorChar);
        
        var compare = entryPointSplit.Length < fileLocationSplit.Length ? entryPointSplit : fileLocationSplit;
        var matchCount = 0;
        var matchString = "";
        for (; matchCount < compare.Length; matchCount++) {
            if (entryPointSplit[matchCount] != fileLocationSplit[matchCount]) {
                break;
            }
            matchString += "/" + entryPointSplit[matchCount];
        }
        var matchLength = matchString.Length;
        
        rootDirectory = model.FileLocation.Substring(0, matchLength);
        rootNamespace = model.EntryPointType.Namespace;

        var entryPointRelativePath = model.FileLocation.Substring(matchLength);
        
        if (entryPointRelativePath.IndexOf(Path.DirectorySeparatorChar) > 0) {
            rootNamespace = TrimEntryPointNamespace(entryPointRelativePath, rootNamespace);
        }
        
        return (rootDirectory, rootNamespace);
    }

    private static string TrimEntryPointNamespace(string entryPointRootPath, string rootNamespace) {
        var rootPathSplit = entryPointRootPath.Split(Path.DirectorySeparatorChar);
        var splitNamespace = rootNamespace.Split('.');

        if (splitNamespace.Length > rootPathSplit.Length - 1) {
            var namespaceString = "";
            var count = splitNamespace.Length - (rootPathSplit.Length -1);
            for (var i = 0; i < count; i++) {
                if (namespaceString != "") {
                    namespaceString += ".";
                }
                namespaceString += splitNamespace[i];
            }
            return namespaceString;
        }
        
        return rootNamespace;
    }
}