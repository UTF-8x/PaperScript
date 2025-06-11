# PaperScript

**PaperScript** is a modern alternative to [Papyrus](https://ck.uesp.net/wiki/Category:Papyrus) that transpiles into valid Papyrus code.

> 💬 **Contributions welcome!**  
> Please open issues and submit PRs with feature requests or suggestions. My goal is to make the **V1** release as complete and robust as possible before moving on to a smarter **v2** rewrite.
>
> I'm not great at writing good documentation so that's one area where PRs are especially appreciated.

---

## [Documentation](https://utf8x.gitbook.io/paperscript-docs)

---

## [Quick Start Guide](https://utf8x.gitbook.io/paperscript-docs/quick-start-guide)

---

## Why PaperScript?

Because **Papyrus is annoying**. It's not the worst language out there, but it's missing a lot of quality-of-life features and tends to be overly verbose.

For example, Papyrus doesn't support `foreach` or even `for` loops—just `while`. So something as simple as iterating over an array is tedious:

```papyrus
; Papyrus "foreach"
Int[] numbers = new Int[10]
Int numberIndex = 0
While numberIndex < numbers.Length
    Int number = numbers[numberIndex]
    DoSomething(number)
    numberIndex += 1
EndWhile
```

With **PaperScript**, you can write:

```paperscript
numbers: Int[] = new Int[10]

range number in numbers {
    DoSomething(number)
}
```

PaperScript transpiles that into the `while` loop for you—clean, readable, and less error-prone.
More quality of life features, such as a `switch` are coming...

---

## What Makes It Easy to Use?

From day one, PaperScript was designed to **stay out of your way**.

The transpiler doesn't just convert PaperScript to Papyrus—it also **compiles** it for you. When running in *Project Mode*, you configure the paths to your game's script source directory and the Papyrus compiler just once. After that, PaperScript takes care of the rest.

Whenever you update a `.pps` file, PaperScript automatically:

1. Transpiles it to Papyrus (`.psc`)
2. Copies it to your game’s script source folder
3. Runs the Papyrus compiler to produce the final `.pex` file

This means you can keep your PaperScript sources in a clean, organized Git repo—anywhere you like. PaperScript handles deployment and compilation behind the scenes, so you can focus on writing code instead of managing build steps.

In fact, it’s **more convenient than writing Papyrus directly.**

---

## How It Works

The transpiler is built using **C#** and **ANTLR4**. This setup is great for rapid prototyping and early development, but the current version behaves more like a "Google Translate for code" than a full compiler.

This makes some features harder to implement and error reporting less helpful. For example, a simple typo like `autso` instead of `auto` might result in a confusing error:

```
syntax error at line 1, column 0: mismatched input 'property' expecting ':'
```

In **v2**, the plan is to implement a proper recursive descent parser that builds an **AST** (Abstract Syntax Tree) and generates Papyrus from that. It'll be more robust, easier to extend, and allow for better error messages.
