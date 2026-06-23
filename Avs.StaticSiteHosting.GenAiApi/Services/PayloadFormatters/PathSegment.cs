using System.Text;
using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

/// <summary>
/// Kinds of segments in a compiled JSON path.
/// </summary>
internal enum PathSegmentKind : byte
{
    /// <summary>Anchor: the source document root ($).</summary>
    Root,
    /// <summary>Anchor: the current item (@) when iterating an array.</summary>
    Current,
    /// <summary>Object property access (.field). FieldUtf8 is populated.</summary>
    Field,
    /// <summary>Array index access ([N]). Index is populated.</summary>
    Index,
    /// <summary>Wildcard ([*]). Only allowed as the LAST segment of an array/find path.</summary>
    Wildcard
}

/// <summary>
/// One step of a compiled JSON path. Designed to be a small value type
/// so the compiled path is just a contiguous <c>PathSegment[]</c>.
/// </summary>
internal readonly struct PathSegment
{
    public PathSegmentKind Kind { get; }

    /// <summary>UTF-8 bytes of the field name (for <see cref="PathSegmentKind.Field"/>).</summary>
    public byte[] FieldUtf8 { get; }

    /// <summary>Array index (for <see cref="PathSegmentKind.Index"/>).</summary>
    public int Index { get; }

    private PathSegment(PathSegmentKind kind, byte[]? fieldUtf8, int index)
    {
        Kind = kind;
        FieldUtf8 = fieldUtf8 ?? Array.Empty<byte>();
        Index = index;
    }

    public static PathSegment Root() => new(PathSegmentKind.Root, null, 0);
    public static PathSegment Current() => new(PathSegmentKind.Current, null, 0);
    public static PathSegment Field(string name) => new(PathSegmentKind.Field, Encoding.UTF8.GetBytes(name), 0);
    public static PathSegment IndexSeg(int index) => new(PathSegmentKind.Index, null, index);
    public static PathSegment Wildcard() => new(PathSegmentKind.Wildcard, null, 0);

    public override string ToString() => Kind switch
    {
        PathSegmentKind.Root => "$",
        PathSegmentKind.Current => "@",
        PathSegmentKind.Field => "." + Encoding.UTF8.GetString(FieldUtf8),
        PathSegmentKind.Index => "[" + Index + "]",
        PathSegmentKind.Wildcard => "[*]",
        _ => "?"
    };
}

/// <summary>
/// Parser + resolver for the path mini-language.
/// Grammar (compact JSONPath subset):
///   path        := anchor (segment)*
///   anchor      := '$' | '@'
///   segment     := '.' name | '[' (uint | '*') ']'
///   name        := [a-zA-Z_][a-zA-Z0-9_-]*    (we accept anything until . or [ for resilience)
/// Wildcards are only legal as the trailing segment and only consumed by callers that iterate.
/// </summary>
internal static class JsonPath
{
    public static PathSegment[] Parse(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path must not be empty.", nameof(path));
        }

        var segments = new List<PathSegment>(8);
        int i = 0;

        switch (path[0])
        {
            case '$':
                segments.Add(PathSegment.Root());
                break;
            case '@':
                segments.Add(PathSegment.Current());
                break;
            default:
                throw new ArgumentException($"Path must start with '$' or '@': '{path}'.", nameof(path));
        }
        
        i++;

        while (i < path.Length)
        {
            char c = path[i];
            switch (c)
            {
                case '.':
                {
                    i++;
                    int start = i;
                    while (i < path.Length && path[i] != '.' && path[i] != '[')
                    {
                        i++;
                    }

                    if (i == start)
                    {
                        throw new ArgumentException($"Empty field name in path '{path}'.", nameof(path));
                    }

                    segments.Add(PathSegment.Field(path.Substring(start, i - start)));
                    break;
                }
                case '[':
                {
                    i++;
                    if (i >= path.Length)
                    {
                        throw new ArgumentException($"Unterminated '[' in path '{path}'.", nameof(path));
                    }

                    if (path[i] == '*')
                    {
                        i++;
                        if (i >= path.Length || path[i] != ']')
                        {
                            throw new ArgumentException($"Expected ']' after '*' in path '{path}'.", nameof(path));
                        }

                        i++;
                        segments.Add(PathSegment.Wildcard());
                    }
                    else
                    {
                        int start = i;
                        while (i < path.Length && path[i] != ']')
                        {
                            i++;
                        }

                        if (i >= path.Length)
                        {
                            throw new ArgumentException($"Unterminated '[' in path '{path}'.", nameof(path));
                        }

                        if (!int.TryParse(path.AsSpan(start, i - start), out int index) || index < 0)
                        {
                            throw new ArgumentException($"Invalid index in path '{path}'.", nameof(path));
                        }

                        i++;
                        segments.Add(PathSegment.IndexSeg(index));
                    }

                    break;
                }
                default:
                    throw new ArgumentException($"Unexpected character '{c}' at position {i} in path '{path}'.", nameof(path));
            }
        }

        return segments.ToArray();
    }

    /// <summary>
    /// Resolves a precompiled path against the source. Returns true if the path resolves to a value.
    /// Wildcard segments are NOT consumed here (callers handle iteration separately).
    /// </summary>
    public static bool TryResolve(
        JsonElement root,
        JsonElement current,
        ReadOnlySpan<PathSegment> segments,
        out JsonElement result)
    {
        if (segments.Length == 0)
        {
            result = root;
            return true;
        }

        JsonElement el;
        var first = segments[0];
        switch (first.Kind)
        {
            case PathSegmentKind.Root: 
                el = root; 
                break;
            case PathSegmentKind.Current: 
                el = current; 
                break;
            default:
                result = default;
                return false;
        }

        for (int i = 1; i < segments.Length; i++)
        {
            var seg = segments[i];
            switch (seg.Kind)
            {
                case PathSegmentKind.Field:
                    if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(seg.FieldUtf8, out el))
                    {
                        result = default;
                        return false;
                    }
                    break;
                case PathSegmentKind.Index:
                    if (el.ValueKind != JsonValueKind.Array || seg.Index >= el.GetArrayLength())
                    {
                        result = default;
                        return false;
                    }
                    el = el[seg.Index];
                    break;
                case PathSegmentKind.Wildcard:
                    // Wildcards are not navigated; callers strip them before calling Resolve.
                    result = default;
                    return false;
                default:
                    result = default;
                    return false;
            }
        }

        result = el;
        return true;
    }
}
