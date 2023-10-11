using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class IfTransformerTests
{
    [Fact]
    public void IfTrue()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if true {
                                         <text>True</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(new TemplateData(), new[] {new IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = templateReader.Read(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("True", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public void IfFalse()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if false {
                                         <text>True</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(new TemplateData(), new[] {new IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = templateReader.Read(xmlReader);
        Assert.Empty(nodeInformation.Children);
    }

    [Theory]
    [InlineData(2, "&gt;", 1, true)]
    [InlineData(1, "&gt;", 2, false)]
    [InlineData(1, "&gt;=", 1, true)]
    [InlineData(1, "&gt;=", 2, false)]
    [InlineData(2, "&gt;=", 1, true)]
    [InlineData(1, "&lt;", 2, true)]
    [InlineData(2, "&lt;", 1, false)]
    [InlineData(1, "&lt;=", 1, true)]
    [InlineData(2, "&lt;=", 1, false)]
    [InlineData(1, "&lt;=", 2, true)]
    [InlineData(1, "==", 1, true)]
    [InlineData(1, "==", 2, false)]
    [InlineData(1, "!=", 1, false)]
    [InlineData(1, "!=", 2, true)]
    [InlineData("abc", "==", "abc", true)]
    [InlineData("ABC", "==", "abc", true)]
    [InlineData("abc", "==", "ABC", true)]
    [InlineData("abc", "===", "abc", true)]
    [InlineData("ABC", "===", "abc", false)]
    [InlineData("abc", "===", "ABC", false)]
    [InlineData("abc", "!=", "abc", false)]
    [InlineData("ABC", "!=", "abc", false)]
    [InlineData("abc", "!=", "ABC", false)]
    [InlineData("abc", "!==", "abc", false)]
    [InlineData("ABC", "!==", "abc", true)]
    [InlineData("abc", "!==", "ABC", true)]
    [InlineData("a", "in", "abc", true)]
    [InlineData("b", "in", "abc", true)]
    [InlineData("c", "in", "abc", true)]
    [InlineData("d", "in", "abc", false)]
    public void IfTheory(object left, string op, object right, bool exists)
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            @if variable {{op}} function(1, "") {
                                <text>True</text>
                            }
                         </styleMustBeEmptyTagTest>
                         """;
        var data = new TemplateData();
        data.SetVariable("variable", left);
        data.RegisterFunction(new DummyValueFunction("function", right, new[] {typeof(int), typeof(string)}));
        var templateReader = new XmlTemplateReader(data, new[] {new IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = templateReader.Read(xmlReader);

        if (exists)
        {
            Assert.Single(nodeInformation.Children);
            Assert.Equal("True", nodeInformation.Children.ElementAt(0).TextContent);
        }
        else
        {
            Assert.Empty(nodeInformation.Children);
        }
    }
}