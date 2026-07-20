using MQNet;

// Load the sample Markdown file shipped alongside this project.
string markdown = File.ReadAllText("sample-docs.md");

// ─────────────────────────────────────────────────────────────────────────────
// 1. Fluent API
//    Mq.Query(...).On(...).Run() — the quick one-shot entry point.
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("=== 1. Fluent API ===");
Console.WriteLine();

// All top-level H1 headings
var h1 = Mq.Query(".h(1)").On(markdown).Run();
Console.WriteLine($"H1 headings ({h1.Count}):");
foreach (var heading in h1)
    Console.WriteLine($"  {heading}");
Console.WriteLine();

// All H2 headings that mention "Install"
var installSections = Mq.Query(".h(2) | select(contains(\"Install\"))").On(markdown).Run();
Console.WriteLine($"H2 headings containing \"Install\" ({installSections.Count}):");
foreach (var s in installSections)
    Console.WriteLine($"  {s}");
Console.WriteLine();

// All bash code blocks — returns the raw fenced block text
var bashBlocks = Mq.Query(".code(\"bash\")").On(markdown).Run();
Console.WriteLine($"Bash code blocks found: {bashBlocks.Count}");
Console.WriteLine("First bash block:");
Console.WriteLine(bashBlocks[0]);
Console.WriteLine();

// Plain text of every heading (strips the leading # markers)
var headingText = Mq.Query(".h | to_text").On(markdown).Run();
Console.WriteLine($"All heading texts ({headingText.Count}):");
Console.WriteLine(headingText.Text);
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// 2. Typed selectors (MarkdownTag)
//    Strongly-typed, IntelliSense-friendly selectors — no raw strings needed.
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("=== 2. Typed selectors (MarkdownTag) ===");
Console.WriteLine();

// MarkdownTag.H1 is equivalent to ".h(1)"
var title = Mq.Query(MarkdownTag.H1).On(markdown).Run();
Console.WriteLine($"Title (H1): {title[0]}");
Console.WriteLine();

// All headings at any level
var allHeadings = Mq.Query(MarkdownTag.AllHeadings).On(markdown).Run();
Console.WriteLine($"Total headings: {allHeadings.Count}");
Console.WriteLine();

// Every fenced code block regardless of language
var allCode = Mq.Query(MarkdownTag.Code).On(markdown).Run();
Console.WriteLine($"Total code blocks: {allCode.Count}");
Console.WriteLine();

// Only Go code blocks — MarkdownTag.CodeBlock("go") == .code("go")
var goBlocks = Mq.Query(MarkdownTag.CodeBlock("go")).On(markdown).Run();
Console.WriteLine($"Go code blocks: {goBlocks.Count}");
if (goBlocks.Count > 0)
    Console.WriteLine(goBlocks[0]);
Console.WriteLine();

// H1 through H3 — inclusive range shorthand
var topThree = Mq.Query(MarkdownTag.HeadingRange(1, 3)).On(markdown).Run();
Console.WriteLine($"H1–H3 headings ({topThree.Count}):");
foreach (var h in topThree)
    Console.WriteLine($"  {h}");
Console.WriteLine();

// Convenience shorthand: Mq.Heading(2) == Mq.Query(MarkdownTag.HeadingLevel(2))
var h2 = Mq.Heading(2).On(markdown).Run();
Console.WriteLine($"H2 headings via Mq.Heading(2) ({h2.Count}):");
foreach (var h in h2)
    Console.WriteLine($"  {h}");
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// 3. MqEngine — reuse a single engine for multiple queries
//    More efficient when you need to run several queries on the same content;
//    avoids re-initialising the native engine on every call.
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("=== 3. MqEngine (reuse across queries) ===");
Console.WriteLine();

using var engine = new MqEngine();

// All H3 headings — used here to show section sub-structure
var h3headings = engine.Eval(".h(3)", markdown);
Console.WriteLine($"H3 headings ({h3headings.Count}):");
foreach (var r in h3headings)
    Console.WriteLine($"  {r}");
Console.WriteLine();

// Table cells — mq returns one entry per cell; .Text joins them all
var tableCells = engine.Eval(".table", markdown);
Console.WriteLine($"Table cells found: {tableCells.Count}");
Console.WriteLine("Table cell values:");
Console.WriteLine(tableCells.Text);
Console.WriteLine();

// All links
var links = engine.Eval(".link", markdown);
Console.WriteLine($"Links found: {links.Count}");
foreach (var link in links)
    Console.WriteLine($"  {link}");
Console.WriteLine();

// Strip formatting — plain text of every list item
var listItems = engine.Eval(".list | to_text", markdown);
Console.WriteLine($"List items (plain text, {listItems.Count}):");
foreach (var item in listItems)
    Console.WriteLine($"  {item}");
