using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class ForTransformerTests
{
    [Fact]
    public async Task NestedImmediately()
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            @for i from 0 to 10 {
                               @for i from 0 to 10 {
                                   <text>@i</text>
                               }
                            }
                         </styleMustBeEmptyTagTest>
                         """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] {new ForTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(100, nodeInformation.Children.Count);
    }

    [Fact]
    public async Task NestedMixed()
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            @for i from 0 to 10 {
                               <text>@i</text>
                               @for i from 0 to 3 {
                                   <text>@i</text>
                               }
                               <text>@i</text>
                            }
                         </styleMustBeEmptyTagTest>
                         """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] {new ForTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(50, nodeInformation.Children.Count);
    }

    [Fact]
    public async Task MixedContent()
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            <text>Before</text>
                            @for i from 0 to 10 {
                                <text>@i</text>
                            }
                            <text>After</text>
                         </styleMustBeEmptyTagTest>
                         """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] {new ForTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(12, nodeInformation.Children.Count);
    }

    [Theory]
    [InlineData(0, 10, null, 10)]
    [InlineData(5, 10, null, 5)]
    [InlineData(0, 10, 2, 5)]
    [InlineData(10, 0, -1, 10)]
    public async Task ForLoopWithNumbers(int start, int end, int? step, int expected)
    {
        const string ns = Constants.ControlsNamespace;
        var stepString = step is null ? string.Empty : $"step {step}";
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            @for i from {{start}} to {{end}} {{stepString}} {
                                <text>@i</text>
                            }
                         </styleMustBeEmptyTagTest>
                         """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] {new ForTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(expected, nodeInformation.Children.Count);
    }

    [Theory]
    [InlineData(0, 10, null, 10)]
    [InlineData(5, 10, null, 5)]
    [InlineData(0, 10, 2, 5)]
    [InlineData(10, 0, -1, 10)]
    public async Task ForLoopWithFunctions(int start, int end, int? step, int expected)
    {
        const string ns = Constants.ControlsNamespace;
        var stepString = step is null ? string.Empty : @"step step(1, """")";
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            @for i from start(1, "") to end(1, "") {{stepString}} {
                                <text>@i</text>
                            }
                         </styleMustBeEmptyTagTest>
                         """;
        var data = new TemplateData();
        data.RegisterFunction(new DummyValueFunction("start", start, new[] {typeof(int), typeof(string)}));
        data.RegisterFunction(new DummyValueFunction("end", end, new[] {typeof(int), typeof(string)}));
        data.RegisterFunction(new DummyValueFunction("step", step ?? 1, new[] {typeof(int), typeof(string)}));
        var templateReader = new XmlTemplateReader(default, CultureInfo.InvariantCulture, data, new[] {new ForTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(expected, nodeInformation.Children.Count);
    }

    [Theory]
    [InlineData(0, 10, null, 10)]
    [InlineData(5, 10, null, 5)]
    [InlineData(0, 10, 2, 5)]
    [InlineData(10, 0, -1, 10)]
    public async Task ForLoopWithVariables(int start, int end, int? step, int expected)
    {
        const string ns = Constants.ControlsNamespace;
        var stepString = step is null ? string.Empty : @"step step";
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            @for i from start to end {{stepString}} {
                                <text>@i</text>
                            }
                         </styleMustBeEmptyTagTest>
                         """;
        var data = new TemplateData();
        data.SetVariable("start", start);
        data.SetVariable("end", end);
        data.SetVariable("step", step ?? 1);
        var templateReader = new XmlTemplateReader(default, CultureInfo.InvariantCulture, data, new[] {new ForTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(expected, nodeInformation.Children.Count);
    }
}