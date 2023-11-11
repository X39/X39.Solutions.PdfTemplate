using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Xml.Exceptions;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Class to read a template from a <see cref="XmlReader"/>.
/// </summary>
public sealed class XmlTemplateReader : IDisposable
{
    /// <inheritdoc />
    public void Dispose()
    {
        _disposable?.Dispose();
    }

    private readonly TemplateData                      _templateData;
    private readonly IReadOnlyCollection<ITransformer> _transformers;
    private readonly IDisposable?                      _disposable;

    /// <summary>
    /// Creates a new <see cref="XmlTemplateReader"/> with the given <paramref name="templateData"/> and <paramref name="transformers"/>.
    /// </summary>
    /// <param name="templateData">The template data to use.</param>
    /// <param name="transformers">The transformers to use.</param>
    public XmlTemplateReader(ITemplateData templateData, IReadOnlyCollection<ITransformer> transformers)
    {
        if (templateData is not TemplateData data)
        {
            _templateData = new TemplateData();
            _disposable   = _templateData.Scope("root", templateData);
        }
        else
        {
            _templateData = data;
        }

        _transformers = transformers;
    }

    private readonly Stack<XmlStyleInformation> _styles = new();

    private static (int line, int column) Location(XmlReader xmlReader)
    {
        if (xmlReader is IXmlLineInfo xmlLineInfo)
            return (xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
        return (-1, -1);
    }

    /// <summary>
    /// Reads the template from the given <paramref name="reader"/> and returns the root node.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>The root node of the template.</returns>
    public XmlNodeInformation Read(XmlReader reader)
    {
        var nodeTree = ReadXmlNode(reader);
        TransformNodeTree(nodeTree);
        return HandleNode(nodeTree);
    }

    private void TransformNodeTree(XmlNode nodeTree)
    {
        for (var nodeIndex = 0; nodeIndex < nodeTree.Children.Count; nodeIndex++)
        {
            var node = nodeTree[nodeIndex];
            TransformNode(nodeTree, node, ref nodeIndex);
        }
    }

    private void TransformNode(XmlNode nodeTree, XmlNode node, ref int nodeIndex)
    {
        if (node is {IsTextNode: true, Text: { } text} && (text.Contains('@') || text.Contains('{')))
        {
#if !DEBUG
            try
            {
#endif
            TransformNodeTreeExpressionCandidate(ref nodeIndex, nodeTree, node, text);
#if !DEBUG
            }
            catch (Exception ex)
            {
                throw new UnhandledXmlTemplateTransformationException(ex, node);
            }
#endif
        }
        else
        {
            TransformNodeTree(node);
        }
    }

    private void TransformNodeTreeExpressionCandidate(ref int nodeIndex, XmlNode nodeTree, XmlNode node, string text)
    {
        var builder = new StringBuilder(text.Length);
        var previousIndex = -1;
        do
        {
            var indexOfExpressionStart = text.IndexOf('@', previousIndex + 1);
            if (indexOfExpressionStart == -1)
                break;
            var previousText = text[(previousIndex is -1 ? 0 : previousIndex)..indexOfExpressionStart];
            previousIndex = indexOfExpressionStart;
            builder.Append(previousText);
            if (!previousText.LastOrDefault().IsWhiteSpace() && indexOfExpressionStart is not 0)
                continue; // No match as @ must be preceded by whitespace or be at the beginning of the string.
            var endOfName = indexOfExpressionStart + 1;

            // Scan for the end of the name.
            while (text.Length > endOfName &&
                   (text[endOfName].IsLetterOrDigit() || text[endOfName] == '-' || text[endOfName] == '_'))
                endOfName++;

            var name = text[(indexOfExpressionStart + 1)..endOfName];
            var lookAhead = text.IndexOf('(', endOfName);
            if (lookAhead != -1 && text[endOfName..lookAhead].IsNullOrWhiteSpace())
            {
                // We got a function here
                var bracketCount = 1;
                var endOfFunction = lookAhead + 1;
                while (bracketCount > 0 && endOfFunction < text.Length)
                {
                    if (text[endOfFunction] == '(')
                        bracketCount++;
                    else if (text[endOfFunction] == ')')
                        bracketCount--;
                    endOfFunction++;
                }

                if (bracketCount > 0)
                    throw new TransformationFunctionMissingClosingBracketException(
                        text,
                        node,
                        bracketCount);
                object? functionResult;
                try
                {
                    functionResult = _templateData.Evaluate(text[(indexOfExpressionStart + 1)..endOfFunction]);
                }
                catch (FunctionNotFoundDuringEvaluationException ex)
                {
                    throw new TransformationFunctionNotFoundException(text[(indexOfExpressionStart + 1)..endOfFunction], node, ex.FunctionName);
                }

                previousIndex = endOfFunction;
                builder.Append(previousText);
                AppendValueToStringBuilder(functionResult, builder);
            }
            else if (_transformers.FirstOrDefault(
                         (t) => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is
                     { } transformer)
            {
                // A transformer name was matched
                var bracketIndex = text.IndexOf('{', endOfName);
                if (bracketIndex == -1)
                    throw new TransformationMissingOpeningBracketException(text, node);

                var transformerBody = text[(endOfName + 1)..bracketIndex];
                var remainingText = text[(bracketIndex + 1)..];
                if (builder.Length > 0 && builder.ToString().All(char.IsWhiteSpace))
                    node.SetText(builder.ToString());
                else
                    nodeTree.RemoveChild(node);
                if (remainingText.IsNotNullOrWhiteSpace())
                {
                    nodeTree.InsertChild(nodeIndex, new XmlNode(node.Line, node.Column, remainingText.TrimStart()));
                }

                var nodesOfTransformer = new List<XmlNode>();
                var currentNodeIndex = nodeIndex;
                var curlyBracketCount = 1;
                XmlNode? endNode = null;
                for (; currentNodeIndex < nodeTree.Children.Count; currentNodeIndex++)
                {
                    var childNode = nodeTree[currentNodeIndex];
                    if (!childNode.IsTextNode)
                    {
                        nodesOfTransformer.Add(childNode);
                        continue;
                    }

                    var childText = childNode.Text
#pragma warning disable CA2201
                                    ?? throw new NullReferenceException(
                                        $"Failed to parse transformer expression '{text}' at L{childNode.Line}:C{childNode.Column}, text node text is null.");
#pragma warning restore CA2201

                    for (var i = 0; i < childText.Length; i++)
                    {
                        var c = childText[i];
                        switch (c)
                        {
                            case '{':
                                curlyBracketCount++;
                                break;
                            case '}':
                                curlyBracketCount--;
                                break;
                        }

                        if (curlyBracketCount is not 0)
                            continue;
                        var leadingText = childText[..i];

                        if (leadingText.IsNotNullOrWhiteSpace())
                        {
                            var tmpNode = new XmlNode(childNode.Line, childNode.Column, leadingText.TrimEnd());
                            nodeTree.InsertChild(currentNodeIndex, tmpNode);
                            nodesOfTransformer.Add(tmpNode);
                            currentNodeIndex++;
                        }
                        
                        var trailingText = childText[(i + 1)..];
                        if (trailingText.IsNotNullOrWhiteSpace())
                        {
                            var tmpNode = new XmlNode(childNode.Line, childNode.Column, trailingText.TrimStart());
                            nodeTree.InsertChild(currentNodeIndex + 1, tmpNode);
                        }
                        childNode.SetText("}");

                        break;
                    }

                    if (curlyBracketCount > 0)
                        nodesOfTransformer.Add(childNode);
                    else
                    {
                        endNode = childNode;
                        break;
                    }
                }

                if (curlyBracketCount > 0)
                    throw new TransformationMissingClosingBracketException(text, node);

                if (endNode is null)
                    throw new TransformationMissingEndNodeBracketException(text, node);

                nodeTree.RemoveChild(endNode);
                foreach (var xmlNode in nodesOfTransformer)
                {
                    nodeTree.RemoveChild(xmlNode);
                }

                var transformedNodes = transformer.Transform(
                    _templateData,
                    transformerBody,
                    nodesOfTransformer.AsReadOnly());
                currentNodeIndex = nodeIndex;
                var scopeList = new List<Dictionary<string, object?>>();
                foreach (var transformedNode in transformedNodes)
                {
                    scopeList.Add(_templateData.PeekScope().ToDictionary((q) => q.Key, (q) => q.Value));
                    nodeTree.InsertChild(currentNodeIndex++, transformedNode);
                }

                scopeList.Reverse();
                var distanceToEnd = nodeTree.Children.Count - currentNodeIndex;
                for (; nodeIndex < nodeTree.Children.Count - distanceToEnd; nodeIndex++)
                {
                    var currentScope = scopeList.Last();
                    scopeList.RemoveAt(scopeList.Count - 1);
                    using var adjustedScope = _templateData.Scope(currentScope);
                    var tmpNodeIndex = nodeIndex;
                    var tmpNodeCount = nodeTree.Children.Count;
                    TransformNode(nodeTree, nodeTree[nodeIndex], ref nodeIndex);
                    var nodeCountDelta = nodeTree.Children.Count - tmpNodeCount;
                    for (; nodeCountDelta < 0; nodeCountDelta++)
                        scopeList.RemoveAt(scopeList.Count - 1);
                    for (var nodeIndexDelta = nodeIndex - tmpNodeIndex - Math.Max(nodeCountDelta, 0); nodeIndexDelta > 0; nodeIndexDelta--)
                        scopeList.RemoveAt(scopeList.Count - 1);
                }

                nodeIndex--;


                break;
            }
            else if (_templateData.TryGetVariable(name, out var variableValue))
            {
                // We got a variable here
                AppendValueToStringBuilder(variableValue, builder);
                previousIndex = endOfName;
            }
            else
            {
                // No match
            }
        } while (previousIndex < text.Length);

        if (previousIndex < 0)
            return;
        if (previousIndex < text.Length)
            builder.Append(text[previousIndex..]);
        node.SetText(builder.ToString());
    }

    private static void AppendValueToStringBuilder(object? functionResult, StringBuilder builder)
    {
        if (functionResult is null)
            builder.Append(string.Empty);
        else
        {
            var type = functionResult.GetType();
            if (type.IsEquivalentTo(typeof(string)))
                builder.Append((string) functionResult);
            else
            {
                var converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertTo(typeof(string)))
                    builder.Append(converter.ConvertTo(functionResult, typeof(string)) as string ?? string.Empty);
                else
                    builder.Append(functionResult.ToString() ?? string.Empty);
            }
        }
    }

    private XmlNode ReadXmlNode(XmlReader reader)
    {
        reader.MoveToContent();
        if (reader.NodeType is not XmlNodeType.Element)
            throw new XmlTemplateNodeTypeMismatchException(
                Location(reader).line,
                Location(reader).column,
                reader.NodeType,
                XmlNodeType.Element,
                $"Expected element at L{Location(reader).line}:C{Location(reader).column}.");
        var nodeStack = new Stack<XmlNode>();
        var location = Location(reader);
        var nodeName = reader.Name;
        var nodeNamespace = reader.NamespaceURI;
        var isEmptyElement = reader.IsEmptyElement;
        var node = new XmlNode(
            location.line,
            location.column,
            nodeNamespace,
            nodeName);
        if (nodeStack.Count > 0)
            nodeStack.Peek().AddChild(node);
        if (reader.HasAttributes)
        {
            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                node.SetAttribute(reader.Name, reader.Value);
            }
        }

        reader.ReadStartElement();
        if (isEmptyElement)
            return node;
        reader.MoveToContent();
        while (reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.NodeType is XmlNodeType.Text)
            {
                var (line, column) = Location(reader);
                node.AddChild(
                    new XmlNode(
                        line,
                        column,
                        reader.Value.Trim()));
                reader.Read();
            }
            else
            {
                node.AddChild(ReadXmlNode(reader));
            }

            reader.MoveToContent();
        }

        reader.ReadEndElement();
        reader.MoveToContent();

        return node;
    }

    private XmlNodeInformation HandleNode(XmlNode xmlNode)
    {
        if (!Validators.ControlName.IsValid(xmlNode.Name))
            throw new XmlNodeNameException(xmlNode);
        PushStyle();
        var children = new List<XmlNodeInformation>();
        var styleName = $"{xmlNode.Name.ToLower(CultureInfo.InvariantCulture)}.style";
        var effectiveStyle = GetEffectiveStyle();
        foreach (var nodeChild in xmlNode.Children)
        {
            if (nodeChild.Name.Equals(styleName, StringComparison.OrdinalIgnoreCase))
            {
                ReadStyle(nodeChild);
            }
            else
            {
                var information = HandleNode(nodeChild);
                children.Add(information);
            }
        }

        PopStyle();
        if (xmlNode.Children.All((q) => q.IsTextNode))
        {
            var text = string.Concat(xmlNode.Children.Select((q) => q.Text));
            return new XmlNodeInformation(
                xmlNode.Line,
                xmlNode.Column,
                xmlNode.Name,
                xmlNode.Namespace,
                text,
                effectiveStyle.Of(xmlNode.Name, xmlNode.Namespace, xmlNode.Attributes),
                ArraySegment<XmlNodeInformation>.Empty);
        }
        else
        {
            return new XmlNodeInformation(
                xmlNode.Line,
                xmlNode.Column,
                xmlNode.Name,
                xmlNode.Namespace,
                xmlNode.Text ?? string.Empty,
                effectiveStyle.Of(xmlNode.Name, xmlNode.Namespace, xmlNode.Attributes),
                children.AsReadOnly());
        }
    }

    private XmlStyleInformation GetEffectiveStyle()
    {
        Dictionary<(string controlName, string controlNamespace), IReadOnlyDictionary<string, string>> effectiveStyles =
            new();
        foreach (var style in _styles)
        {
            foreach (var (key, value) in style.GetAll())
            {
                effectiveStyles[key] = value;
            }
        }

        return new XmlStyleInformation(effectiveStyles);
    }

    private void ReadStyle(XmlNode xmlNode)
    {
        var style = CurrentStyle;
        foreach (var nodeChild in xmlNode.Children)
        {
            if (nodeChild.Children.Any())
                throw new XmlStyleInformationCannotNestException(
                    nodeChild.Line,
                    nodeChild.Column,
                    $"A style node (L{nodeChild.Line}:C{nodeChild.Column}) cannot have children.");
            style.Set(
                nodeChild.Line,
                nodeChild.Column,
                nodeChild.Name,
                nodeChild.Namespace,
                nodeChild.Attributes);
        }
    }

    private void PushStyle() => _styles.Push(new XmlStyleInformation());
    private void PopStyle() => _styles.Pop();
    private XmlStyleInformation CurrentStyle => _styles.Peek();
}