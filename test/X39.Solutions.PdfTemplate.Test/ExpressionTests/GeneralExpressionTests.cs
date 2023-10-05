using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class GeneralExpressionTests
{
    [Theory]
    [InlineData("@i", "foobar", "i", "foobar")]
    [InlineData("prefix @i suffix", "prefix foobar suffix", "i", "foobar")]
    [InlineData("@is", "@is", "i", "shouldNotAppear")]
    [InlineData("s@i", "s@i", "i", "shouldNotAppear")]
    [InlineData("some@email.com", "some@email.com", "email", "shouldNotAppear")]
    public void VariableReplacements(string textInTemplate, string textExpected, string variable, object value)
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            <text>{{textInTemplate}}</text>
                         </styleMustBeEmptyTagTest>
                         """;
        var data = new TemplateData();
        data.SetVariable(variable, value);
        var templateReader = new XmlTemplateReader(data, Array.Empty<ITransformer>());
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = templateReader.Read(xmlReader);
        Assert.Equal(1, nodeInformation.Children.Count);
        Assert.Equal(textExpected, nodeInformation.Children.ElementAt(0).TextContent);
    }
}