using System.Runtime.CompilerServices;
using Hypercube.Core.Resources;

namespace Shared.Extensions;

public static class ResourcePathExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResourcePath WithExtension(this ResourcePath path, string extension)
    {
        if (path.IsEmpty || path.IsSelf)
            return path;
        
        var cleanExt = extension.StartsWith('.') ? extension[1..] : extension;
        
        var value = path.Value;
        var lastDotIndex = value.LastIndexOf('.');
        var lastSeparatorIndex = value.LastIndexOf(ResourcePath.Separator);
        
        if (lastDotIndex == -1 || lastDotIndex < lastSeparatorIndex)
        {
            return string.IsNullOrEmpty(cleanExt) 
                ? path 
                : new ResourcePath($"{value}.{cleanExt}");
        }
        
        var basePath = value[..lastDotIndex];
        return string.IsNullOrEmpty(cleanExt) 
            ? new ResourcePath(basePath) 
            : new ResourcePath($"{basePath}.{cleanExt}");
    }
}