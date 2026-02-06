using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class VariableTransformerTests
{
    [Theory]
    [InlineData(" testb = context-b, testa = context-a  ")]
    [InlineData(" testb= context-b, testa = context-a  ")]
    [InlineData(" testb =context-b, testa = context-a  ")]
    [InlineData(" testb=context-b, testa = context-a  ")]
    [InlineData(" testb = context-b , testa = context-a  ")]
    [InlineData(" testb= context-b , testa = context-a  ")]
    [InlineData(" testb =context-b , testa = context-a  ")]
    [InlineData(" testb=context-b , testa = context-a  ")]
    [InlineData(" testb = context-b,testa = context-a  ")]
    [InlineData(" testb= context-b,testa = context-a  ")]
    [InlineData(" testb =context-b,testa = context-a  ")]
    [InlineData(" testb=context-b,testa = context-a  ")]
    [InlineData(" testb = context-b ,testa = context-a  ")]
    [InlineData(" testb= context-b ,testa = context-a  ")]
    [InlineData(" testb =context-b ,testa = context-a  ")]
    [InlineData(" testb=context-b ,testa = context-a  ")]
    [InlineData(" testb = context-b,testa= context-a  ")]
    [InlineData(" testb= context-b,testa= context-a  ")]
    [InlineData(" testb =context-b,testa= context-a  ")]
    [InlineData(" testb=context-b,testa= context-a  ")]
    [InlineData(" testb = context-b ,testa= context-a  ")]
    [InlineData(" testb= context-b ,testa= context-a  ")]
    [InlineData(" testb =context-b ,testa= context-a  ")]
    [InlineData(" testb=context-b ,testa= context-a  ")]
    [InlineData(" testb=context-b,testa=context-a")]
    [InlineData(" testa=context-a,testb=context-b")]
    [InlineData(
        "                 testa                  =                 context-a                 ,                 testb                =                 context-b                  "
    )]
    [InlineData(
        "                 testb                  =                 context-b                 ,                 testa                =                 context-a                  "
    )]
    public async Task MultipleVariablesWork(string expressionString)
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <SingleVariableWorks xmlns="{{ns}}" someAttribute="asd">
                            @var{{expressionString}}{
                                <text>@testa</text>
                                <text>@testb</text>
                            }
                         </SingleVariableWorks>
                         """;
        var data = new TemplateData();
        data.SetVariable("context-a", "foobar");
        data.SetVariable("context-b", "barfoo");
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            data,
            new[] { new VariableTransformer() }
        );
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(2, nodeInformation.Children.Count);
        Assert.Equal("foobar", nodeInformation.Children.ElementAt(0).TextContent);
        Assert.Equal("barfoo", nodeInformation.Children.ElementAt(1).TextContent);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public async Task NVariablesWork(int i)
    {
        var data = new TemplateData();
        var expression = new StringBuilder();
        var contents = new StringBuilder();
        for (var j = 0; j < i; j++)
        {
            var variable = string.Concat("test-", j);
            var value = string.Concat("test-value-", j);
            data.SetVariable(variable, value);
            if (j > 0)
                expression.Append(',');
            expression.Append(variable);
            expression.Append('=');
            expression.Append('"');
            expression.Append(value);
            expression.Append('"');
            contents.Append("<text>");
            contents.Append('@');
            contents.Append(variable);
            contents.Append("</text>");
        }

        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <SingleVariableWorks xmlns="{{ns}}" someAttribute="asd">
                            @var {{expression}} {
                                {{contents}}
                            }
                         </SingleVariableWorks>
                         """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            data,
            new[] { new VariableTransformer() }
        );
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(i, nodeInformation.Children.Count);
        Assert.Collection(
            nodeInformation.Children,
            Enumerable.Range(0, i)
                      .Select(
                          (i) => new Action<PdfTemplate.Xml.XmlNodeInformation>(
                              value => Assert.Equal(string.Concat("test-value-", i), value.TextContent)
                          )
                      )
                      .ToArray()
        );
    }

    [Theory]
    [InlineData(" test = context-a ")]
    [InlineData(" test= context-a ")]
    [InlineData(" test =context-a ")]
    [InlineData(" test=context-a ")]
    [InlineData(" test = context-a")]
    [InlineData(" test= context-a")]
    [InlineData(" test =context-a")]
    [InlineData(" test=context-a")]
    [InlineData("            test            =            context-a           ")]
    public async Task SingleVariableWorks(string expressionStyle)
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <SingleVariableWorks xmlns="{{ns}}" someAttribute="asd">
                            @var{{expressionStyle}}{
                                <text>@test</text>
                            }
                         </SingleVariableWorks>
                         """;
        var data = new TemplateData();
        data.SetVariable("context-a", "foobar");
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            data,
            new[] { new VariableTransformer() }
        );
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("foobar", nodeInformation.Children.ElementAt(0).TextContent);
    }
}