using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Test.ExpressionTests;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.Xml;

public class FunctionCallTests
{
    [Fact]
    public async Task CanCallEmptyFunction()
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    <text>@myFunc()</text>
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(new DummyValueFunction("myFunc", "someValue", []));

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("someValue", xmlNodeInformation.TextContent);
    }

    [Fact]
    public async Task CanCallSingleArgumentFunction()
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    <text>@myFunc(1)</text>
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(new DummyValueFunction("myFunc", "someValue", [typeof(int)]));

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("someValue", xmlNodeInformation.TextContent);
    }

    [Fact]
    public async Task CanCallDoubleArgumentFunction()
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    <text>@myFunc(1,2)</text>
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(new DummyValueFunction("myFunc", "someValue", [typeof(int), typeof(int)]));

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("someValue", xmlNodeInformation.TextContent);
    }

    [Fact]
    public async Task CanCallTripleArgumentFunction()
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    <text>@myFunc(1,2,3)</text>
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(
            new DummyValueFunction("myFunc", "someValue", [typeof(int), typeof(int), typeof(int)])
        );

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("someValue", xmlNodeInformation.TextContent);
    }

    [Theory]
    [InlineData("myFunc( 1 , 2 , 3 )", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc(1 , 2 , 3 )", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc(1, 2 , 3 )", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc(1,2 , 3 )", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc(1,2, 3 )", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc(1,2,3 )", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc( 1 , 2 , 3)", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc( 1 , 2 ,3)", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc( 1 , 2,3)", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc( 1 ,2,3)", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc( 1,2,3)", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc(1,2,3)", new[] { typeof(int), typeof(int), typeof(int) })]
    [InlineData("myFunc(\"someString\")", new[] { typeof(string) })]
    [InlineData("myFunc(\"someString\", \"someString\")", new[] { typeof(string), typeof(string) })]
    [InlineData("myFunc(true)", new[] { typeof(bool) })]
    [InlineData("myFunc(false)", new[] { typeof(bool) })]
    [InlineData("myFunc(1.1)", new[] { typeof(double) })]
    [InlineData("myFunc(1, true, \"string\")", new[] { typeof(int), typeof(bool), typeof(string) })]
    public async Task CanCallNonNestedFunction(string call, Type[] args)
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        string template = $$"""
                            <?xml version="1.0" encoding="utf-8"?>
                            <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                              <text>@{{call}}</text>
                            </styleMustBeEmptyTagTest>
                            """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(new DummyValueFunction("myFunc", "someValue", args));

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("someValue", xmlNodeInformation.TextContent);
    }

    [Fact]
    public async Task CanCallNestedFunctionWithNoArgs()
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    <text>@myFunc(nestedFunc())</text>
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(new DummyValueFunction("myFunc", "myValue", [typeof(string)]));
        templateData.RegisterFunction(new DummyValueFunction("nestedFunc", "someValue", []));

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("myValue", xmlNodeInformation.TextContent);
    }

    [Fact]
    public async Task CanCallNestedVariable()
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    <text>@myFunc(fancyVar)</text>
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(new DummyValueFunction("myFunc", "myValue", [typeof(string)]));
        templateData.SetVariable("fancyVar", "string");

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("myValue", xmlNodeInformation.TextContent);
    }

    [Fact]
    public async Task FunctionWithThreeArgsCanCallNestedFunctionWithThreeArgs()
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    <text>@myFunc(nestedFunc("string", "string", "string"),nestedFunc("string","string","string"), nestedFunc( "string" , "string" , "string" ) )</text>
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(new DummyValueFunction("myFunc", "myValue", [typeof(string),typeof(string),typeof(string)]));
        templateData.RegisterFunction(new DummyValueFunction("nestedFunc", "someValue", [typeof(string),typeof(string),typeof(string)]));

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("myValue", xmlNodeInformation.TextContent);
    }

    [Fact]
    public async Task FunctionWithSingleNestedFunctionWithTwoArgs()
    {
        // Setup
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    <text>@myFunc(nestedFunc("string", "string"))</text>
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateData = new TemplateData();
        templateData.RegisterFunction(new DummyValueFunction("myFunc", "myValue", [typeof(string)]));
        templateData.RegisterFunction(new DummyValueFunction("nestedFunc", "someValue", [typeof(string),typeof(string)]));

        // Act
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, templateData, []);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        // Assert
        var xmlNodeInformation = Assert.Single(nodeInformation.Children);
        Assert.Equal("myValue", xmlNodeInformation.TextContent);
    }
}