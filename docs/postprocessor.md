# The Postprocessor

The postprocessor works kind of like the C preprocessor but most
of it happens after the output script is generated (because that's
easier to implement).

It's very rudimentary and only supports a few features at the moment,
but it can be very useful in some cases.

## Defines

You can define a postprocessor variable. Defines can only be at the
top of a file.

Defines can either be value-less, in which case the value is internally
substituted to "true", or they can be valued. Define values must be
enclosed in quotes.

```c
#define DEBUG
#define VERSION "1.0.0"
```

## Conditionals

You can place simple conditions into your code that completely remove
a block of code from the output if a condition is not met. This is
very useful for, for example, debug builds having more logging.

```c
// ...

#if DEBUG
Debug.Notification("something happened")
#endif

// ...
```

## Substitution

Be careful with how you name your defines because every mention of
it anywhere in the code will get substituted with the defines value.

```c
#define VERSION "1.0.0"

Debug.Notification("loading version VERSION")

// this will turn into

Debug.Notification("loading version 1.0.0")
```

## Special Defines

There are a few reserved define names that allow you to talk to the
transpiler from your code.

| Define Name        | Description                                               | Default Value |
|--------------------|-----------------------------------------------------------|---------------|
| `OUTPUT_FILE_NAME` | Optionally overrides the name of the final papyrus script | `null`        |
| `DEBUG`            | May be set from `project.yaml`                            | `null`        |

## Includes

You can include the contents of a different file.

**The include functionality is work in progress. The content is included verbatim and does
not get transpiled so if you try to include PaperScript code, the resulting script won't work.**

The include path is relative to the work directory.

```c
#include "script.pps"
```