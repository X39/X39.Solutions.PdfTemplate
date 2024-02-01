<!-- TOC -->
* [X39.Solutions.PdfTemplate](#x39solutionspdftemplate)
  * [Semantic Versioning](#semantic-versioning)
  * [Getting Started](#getting-started)
  * [Integration](#integration)
    * [Functions](#functions)
    * [Variables](#variables)
    * [Controls](#controls)
      * [Creating your own control](#creating-your-own-control)
      * [`text`](#text)
      * [`border`](#border)
      * [`image`](#image)
      * [`line`](#line)
      * [`pageNumber`](#pagenumber)
      * [`table`](#table)
        * [`th`](#th)
        * [`tr`](#tr)
        * [`td`](#td)
    * [Transformers](#transformers)
      * [Creating your own transformer](#creating-your-own-transformer)
      * [`alternate`](#alternate)
      * [`if`](#if)
      * [`for`](#for)
      * [`foreach`](#foreach)
  * [Building and Testing](#building-and-testing)
  * [Proper documentation for End-Users](#proper-documentation-for-end-users)
  * [Contributing](#contributing)
    * [Additional controls](#additional-controls)
  * [License](#license)
<!-- TOC -->

# X39.Solutions.PdfTemplate

This library provides a way to generate PDF documents (and images) from XML templates.
It uses SkiaSharp for rendering and supports a variety of controls for creating complex layouts.
You can easily integrate .NET objects into your templates by using so-called "variables" (`@myVariable`)
or pull data from a database as needed, by providing a custom function (`@myFunction()`).
You may even create your own controls by deriving from the `Control` base class!

## Semantic Versioning

This library follows the principles of [Semantic Versioning](https://semver.org/). This means that version numbers and
the way they change convey meaning about the underlying changes in the library. For example, if a minor version number
changes (e.g., 1.1 to 1.2), this indicates that new features have been added in a backwards-compatible manner.

## Getting Started

To get started, install the [NuGet package](https://www.nuget.org/packages/X39.Solutions.PdfTemplate/) into your
project:

```shell
dotnet add package X39.Solutions.PdfTemplate
```

Next, create an XML template. Here is a simple example:

```xml

<template>
    <body>
        <text>Hello, world!</text>
    </body>
</template>
```

Now, you can use the `Generator` class to generate a PDF document from the template:

```csharp
// IServiceProvider serviceProvider
// Stream xmlTemplateStream
var paintCache             = serviceProvider.GetRequiredService<SkPaintCache>();
var controlExpressionCache = serviceProvider.GetRequiredService<ControlExpressionCache>();
await using var generator = new Generator(
    paintCache,
    controlExpressionCache,
    Enumerable.Empty<IFunction>()
);
generator.AddDefaults();
using var textReader   = new StringReader(xmlTemplateStream);
using var reader       = XmlReader.Create(textReader);
using var pdfStream = new MemoryStream();
await generator.GeneratePdfAsync(pdfStream, reader, CultureInfo.CurrentUICulture);
// pdfStream now contains the PDF
```

This will generate a PDF document with the text "Hello, world!".

## Integration

### Functions

The library supports custom functions for use in templates and comes with two built-in functions: `allFunctions()`
and `allVariables()`.
These functions are used to list all available functions and variables, respectively.

To create your own function, derive a class from the `IFunction` interface and implement the `Invoke` method. Here is an
example:

```csharp
public class MyFunction : IFunction
{
    public MyFunction(ISomeDependency someDependency) // You can inject dependencies via the constructor.
    {
        // ...
    }

    public string Name => "my"; // The name of your function.
    public int Arguments => 0; // The number of arguments your function takes.
    public bool IsVariadic => false; // Whether your function takes a variable number of arguments. If true, `Arguments` is the minimum number of arguments.

    public ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default)
    {
        // Execute your function here.
        return ValueTask.FromResult<object?>("Hello, world!");
    }
}
```

### Variables

You can use variables in your templates to access .NET objects. To do this, you just need to add the variable to
the `Generator` instance:

```csharp
generator.TemplateData.SetVariable("MyVariable", "Hello World!");
```

### Controls

The library supports a variety of controls for creating complex layouts. Each control is represented by a class in
the `X39.Solutions.PdfTemplate.Controls` namespace.

#### Creating your own control

To create your own, derive a class from the `Control` base class and override the `Render` method. Here is an example:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X39.Solutions.PdfTemplate.Controls;

namespace MyControls;
[Control("MyControls")] // The namespace of your control.
public class MyControl : Control
{
    public MyControl(ISomeDependency someDependency) // You can inject dependencies via the constructor.
    {
        // ...
    }
    
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        // The size your control wants to be, given the remaining space.
        return new Size(100, 100);
    }

    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        // The size your control actually is, given the remaining space.
        return new Size(100, 100);
    }


    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        // Render your control here.
        return Size.Zero;
    }
}
```

Later in your code, add the control to the `Generator` instance:

```csharp
generator.AddControl<MyControl>();
```

You can now use the control in your XML templates (note the namespace import at the top):

```xml 

<template xmlns:my="MyControls">
    <body>
        <my:MyControl/>
    </body>
</template>
```

**WARNING** The template has an implicit default namespace.
If you change the default namespace (`xmlns="MyControls"`) instead of defining your own prefix,
you will have to appropriately refer to default controls and the template layout itself via that namespace!

#### `text`

The `text` control allows to render text. It can be used as follows:

```xml

<template>
    <body>
        <text>Hello, world!</text>
    </body>
</template>
```

You may derive from the `TextBaseControl` class to create your own text-based controls.

The `text` control supports the following attributes:

| Attribute         | Description                                                       | Values                                                                            | Default                                                      |
|-------------------|-------------------------------------------------------------------|-----------------------------------------------------------------------------------|--------------------------------------------------------------|
| `Foreground`      | The foreground color of the text.                                 | #RGB, #RGBA, #RRGGBB, #RRGGBBAA, some color names (eg. `red`)                     | `black`                                                      |
| `FontSize`        | The font size of the text in points.                              | A positive number                                                                 | `12`                                                         |
| `LineHeight`      | The line height of the text in points, relative to the font size. | A positive number                                                                 | `1.0`                                                        |
| `Scale`           | The scale of the text.                                            | A positive number                                                                 | `1.0`                                                        |
| `Rotation`        | The rotation of the text in degrees.                              | A number                                                                          | `0`                                                          |
| `StrokeThickness` | The thickness of the text stroke in points.                       | A positive number                                                                 | `1`                                                          |
| `FontSpacing`     | The spacing between characters in points.                         | A number                                                                          | `0`                                                          |
| `FontWeight`      | The weight of the font.                                           | Any positive number or the common names without a `-` (`thin`, `extraLight`, ...) | `normal`                                                     |
| `FontStyle`       | The style of the font.                                            | `normal`, `italic`, `oblique`, `upright`                                          | `normal`                                                     |
| `FontFamily`      | The font family of the text.                                      | A font family name or a comma-separated list of font family names                 | Windows: `Arial`, Any other system: OS-specific default font |
| `Text`            | The text to render. Also accepted as XML Content.                 | Any text                                                                          | `""`                                                         |

#### `border`
<!-- ToDo: Add content -->

#### `image`
<!-- ToDo: Add content -->

#### `line`
<!-- ToDo: Add content -->

#### `pageNumber`
<!-- ToDo: Add content -->

#### `table`
<!-- ToDo: Add content -->

##### `th`
<!-- ToDo: Add content -->

##### `tr`
<!-- ToDo: Add content -->

##### `td`
<!-- ToDo: Add content -->


### Transformers

Transformers are used to transform the XML template before it is rendered.
This allows to expand a template and enrich it with data from csharp.

#### Creating your own transformer

A transformer, at its core, is a class that implements the `ITransformer` interface.
It allows manipulating every node in its `{...}` block and hence is a very powerful tool regarding template
manipulation.

To create your own transformer, derive a class from the `ITransformer` interface and implement the `Transform` method:

```csharp
public class MyTransformer : ITransformer
{
    public string Name => "MyTransformer"; // The name of your transformer.

    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Return the transformed nodes.
    }
}
```

afterwards, add the transformer to the `Generator` instance:

```csharp
generator.AddTransformer(new MyTransformer());
```

Note that the way transformers are added is subject to change in the future to allow for a better integration with
dependency injection.

#### `alternate`

The `alternate` transformer allows to alternate between values, making it possible to eg. create a table with
alternating row colors.
It can be used as follows:

```xml

<template>
    <body>
        <!-- Every time we call @alternate with just on and a list of values, the value list will be progressed by one and put into @value -->
        <!-- If the value list is exhausted, it will start over -->
        @alternate on value with ["one", "two"] {
        <!-- @value is "one" -->
        <text>@value</text>
        }
        @alternate repeat on value {
        <!-- @value is "one" -->
        <text>@value</text>
        }
        @alternate on value with ["one", "two"] {
        <!-- @value is "two" -->
        <text>@value</text>
        }
        <!-- When calling @alternate with a different list, the alternate will be reset -->
        @alternate on value with ["three"] {
        <!-- @value is "three" -->
        <text>@value</text>
        }
    </body>
</template>
```

#### `if`

The `if` transformer allows to conditionally include parts of the template.
There is no `else` clause, but you can use `@if` multiple times to achieve the same effect.
It can be used as follows:

```xml

<template>
    <body>
        @if 1 == 1 {
        <text>Numerous operators are supported, including &gt;, &lt;, &gt;=, &lt;=, ==, !=, ===, !== and "in".</text>
        }
        @if false {
        <text>Never included</text>
        }
        @if true {
        <text>Always included</text>
        }
    </body>
</template>
```

#### `for`

The `for` transformer allows to repeat parts of the template.

<!-- Regex: "\A\s*(?<variable>[a-zA-Z][a-zA-Z0-9_]*)\s+from\s+(?<from>.+?)\s+to\s+(?<to>.+?)(\s+step\s+(?<step>.+?))?\s*\z -->

```xml

<template>
    <body>
        @for i from 0 to 10 {
        <!-- @i is 0, 1, 2, ..., 9 -->
        <text>@i</text>
        }
        @for i from 0 to 10 step 2 {
        <!-- @i is 0, 2, 4, ..., 8 -->
        <text>@i</text>
        }
        @for i from 10 to 0 step -2 {
        <!-- @i is 10, 8, 6, ..., 2 -->
        <text>@i</text>
        }
    </body>
</template>
```

#### `foreach`

The `foreach` transformer allows to repeat parts of the template for each element in a list.

```csharp
generator.TemplateData.SetVariable("MyList", new[] { "one", "two", "three" });
```

```xml

<template>
    <body>
        @foreach item in @MyList {
        <!-- @item is "one", "two", "three" -->
        <text>@item</text>
        }
        @foreach item in @MyList with index {
        <!-- @item is "one", "two", "three" -->
        <!-- @index is 0, 1, 2 -->
        <text>@item</text>
        }
    </body>
</template>
```

## Building and Testing

This project uses GitHub Actions for continuous integration. The workflow is defined in `.github/workflows/main.yml`. It
includes steps for restoring dependencies, building the project, running tests, and publishing a NuGet package.

To run the tests locally, use the following command:

```shell
dotnet test --framework net7.0 --no-build --verbosity normal
```

## Proper documentation for End-Users

While the code is documented, an appropriate documentation for end-users is still missing.
This is planned tho given that this is a spare-time project, it might take a while and does not have a high priority (on
my list).
Feel free to contribute to this project by adding documentation for end-users (e.g. using JetBrains Writerside or
similar tools) and
submitting a pull request. I will gladly review it and provide the necessary web-hosting in this repository (including a
domain).

## Contributing

Contributions are welcome!
Please submit a pull request or create a discussion to discuss any changes you wish to make.

### Code of Conduct

Be excellent to each other.

### Contributors Agreement

First of all, thank you for your interest in contributing to this project!
Please add yourself to the list of contributors in the [CONTRIBUTORS](CONTRIBUTORS.md) file when submitting your
first pull request.
Also, please always add the following to your pull request:

```
By contributing to this project, you agree to the following terms:
- You grant me and any other person who receives a copy of this project the right to use your contribution under the
  terms of the GNU Lesser General Public License v3.0.
- You grant me and any other person who receives a copy of this project the right to relicense your contribution under
  any other license.
- You grant me and any other person who receives a copy of this project the right to change your contribution.
- You waive your right to your contribution and transfer all rights to me and every user of this project.
- You agree that your contribution is free of any third-party rights.
- You agree that your contribution is given without any compensation.
- You agree that I may remove your contribution at any time for any reason.
- You confirm that you have the right to grant the above rights and that you are not violating any third-party rights
  by granting these rights.
- You confirm that your contribution is not subject to any license agreement or other agreement or obligation, which
  conflicts with the above terms.
```

This is necessary to ensure that this project can be licensed under the GNU Lesser General Public License v3.0 and
that a license change is possible in the future if necessary (e.g., to a more permissive license).
It also ensures that I can remove your contribution if necessary (e.g., because it violates third-party rights) and
that I can change your contribution if necessary (e.g., to fix a typo, change implementation details, or improve
performance).
It also shields me and every user of this project from any liability regarding your contribution by deflecting any
potential liability caused by your contribution to you (e.g., if your contribution violates the rights of your employer).
Feel free to discuss this agreement in the discussions section of this repository, i am open to changes here (as long as
they do not open me or any other user of this project to any liability due to a **malicious contribution**).

### Additional controls

If you have created an additional control which is not depending on any other library, feel free to submit a pull
request.
If your control depends on another library, please create a separate repository and create a pull request to add it to a
list in this README.md.
This way, the core library can stay as small as possible and users can decide which controls they want to use.
Feel free to ask for help regarding publishing your control as a separate NuGet package in the discussions section of
this repository.

## License

This project is licensed under the GNU Lesser General Public License v3.0. See the [LICENSE](LICENSE) file for details.