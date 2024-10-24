using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Abstraction;
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
    public async Task VariableReplacements(string textInTemplate, string textExpected, string variable, object value)
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
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, data, Array.Empty<ITransformer>());
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal(textExpected, nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task VariableNotCutOff()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <styleMustBeEmptyTagTest xmlns="{ns}" someAttribute="asd">
                                    <text>@i: @j @k! @nono- @yes-yes</text>
                                 </styleMustBeEmptyTagTest>
                                 """;
        var data = new TemplateData();
        data.SetVariable("i", "foo");
        data.SetVariable("j", "bar");
        data.SetVariable("k", "baz");
        data.SetVariable("nono", "error");
        data.SetVariable("yes-yes", "no-error");
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, data, Array.Empty<ITransformer>());
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("foo: bar baz! @nono- no-error", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task ChainedFunctionCalls()
    {
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <text>@foo() - @bar() - @baz()</text>
                         """;
        var data = new TemplateData();
        data.RegisterFunction(new DummyValueFunction("foo", "foo", Type.EmptyTypes));
        data.RegisterFunction(new DummyValueFunction("bar", "bar", Type.EmptyTypes));
        data.RegisterFunction(new DummyValueFunction("baz", "baz", Type.EmptyTypes));
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, data, Array.Empty<ITransformer>());
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal("foo - bar - baz", nodeInformation.TextContent);
    }

    [Fact]
    public async Task NestedFunctionCalls()
    {
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <text>@foo(bar(baz()))</text>
                         """;
        var data = new TemplateData();
        data.RegisterFunction(
            new DummyValueFunction("foo", (args) => string.Concat(args.Prepend("foo")), new[] {typeof(string)}));
        data.RegisterFunction(
            new DummyValueFunction("bar", (args) => string.Concat(args.Prepend("bar")), new[] {typeof(string)}));
        data.RegisterFunction(new DummyValueFunction("baz", "baz", Type.EmptyTypes));
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, data, Array.Empty<ITransformer>());
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        Assert.Equal("foobarbaz", nodeInformation.TextContent);
    }

    [Fact]
    public async Task ChainedNestedFunctionCalls()
    {
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <text>@foo(bar(baz())) - @foo(bar(baz())) - @foo(bar(baz()))</text>
                         """;
        var data = new TemplateData();
        data.RegisterFunction(
            new DummyValueFunction("foo", (args) => string.Concat(args.Prepend("foo")), new[] {typeof(string)}));
        data.RegisterFunction(
            new DummyValueFunction("bar", (args) => string.Concat(args.Prepend("bar")), new[] {typeof(string)}));
        data.RegisterFunction(new DummyValueFunction("baz", "baz", Type.EmptyTypes));
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, data, Array.Empty<ITransformer>());
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        Assert.Equal("foobarbaz - foobarbaz - foobarbaz", nodeInformation.TextContent);
    }
}