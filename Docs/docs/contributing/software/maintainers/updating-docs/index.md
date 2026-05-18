---
title: Updating Documentation
weight: 2
---

# Updating Documentation

The API reference is generated from XML doc comments on C# source code. Each
scriptable type, property, method, and event is documented inline next to its
declaration, then surfaced through a two-stage pipeline:

```
C# /// <summary> …      (in Polytoria/scripts/**)
  ↓ csc -doc
Polytoria.xml           (emitted alongside the assembly)
  ↓ APIReferenceGenerator.cs (Polytoria/scripts/docsgen)
Polytoria/def.json
  ↓ Docs/autogen/mdgen.js
Docs/docs/api/**/*.md   (rendered by MkDocs)
```

## Editing the source

All documentation for the scripting API lives in `/// <summary>` blocks
co-located with the C# member it describes. Common tags:

| Tag | Purpose |
| --- | --- |
| `<summary>`            | Short, sentence-style description shown as the headline. |
| `<remarks>`            | Longer caveats / context. Rendered as a `!!! note "Remarks"` admonition. |
| `<param name="x">`     | Describes parameter `x` (methods and PTSignal fields). |
| `<returns>`            | Describes the return value. Rendered as a `!!! quote "Returns"` admonition. |
| `<example>`            | A usage example. Wrap code in `<code>…</code>` to get a fenced `lua` block. May appear multiple times. |
| `<seealso cref="…">`   | Cross-link to another type. The last segment of `cref` is rendered as a link. |
| `<c>foo</c>`           | Inline code (becomes `` `foo` `` in markdown). |
| `<para>…</para>`       | Paragraph break inside a longer block. |

### Two Polytoria-specific conventions

**Sidebar category.** Group a type into a documentation section with the
`[DocCategory(...)]` attribute. The category name is used as the markdown
sub-folder (e.g. `services`, `physics`, `ui`):

```csharp
[Instantiable]
[DocCategory("game")]
public sealed partial class Camera : Dynamic { … }
```

**Non-generic `PTSignal` arguments.** Reflection can't recover argument types
from a bare `PTSignal` (as opposed to `PTSignal<T1, T2>`), so for those events
specify a `type=` attribute on each `<param>` tag:

```csharp
/// <summary>Fires when a player sends a chat message.</summary>
/// <param name="sender" type="Player">The player who sent the message.</param>
/// <param name="message" type="string">The message contents.</param>
[ScriptProperty]
public PTSignal NewChatMessage { get; private set; } = new();
```

For generic `PTSignal<…>`, parameter *types* come from reflection and `<param>`
tags supply the parameter *names* and *descriptions* by position.

## Regenerating the docs

1. Ensure the C# project builds — `Polytoria.xml` is emitted next to the
   assembly when `GenerateDocumentationFile` is on (already configured for
   Debug/ExportDebug).

2. Regenerate `Polytoria/def.json` by running the Creator in API-export mode:

    ```bash
    godot-mono --path Polytoria --creator --genapi
    ```

    The `--genapi` flag causes `APIReferenceGenerator.GenerateRefFile()` to
    read `Polytoria.xml`, walk the `[ScriptProperty]` / `[ScriptMethod]` /
    `PTSignal` surface, and write `res://def.json` (which resolves to
    `Polytoria/def.json`).

3. Regenerate the markdown tree from `def.json`:

    ```bash
    cd Docs
    npm run gen
    ```

    This runs [`autogen/mdgen.js`](https://github.com/Polytoria/polytoria-game/blob/main/Docs/autogen/mdgen.js),
    which reads `Polytoria/def.json` and rewrites `Docs/docs/api/types/**` and
    `Docs/docs/api/enums/**`.

4. Preview the result locally with `npm run dev`, then commit both the C#
   source edits and the regenerated markdown.

## What gets regenerated vs. hand-edited

| File / path | Authoring |
| --- | --- |
| `Polytoria/scripts/**/*.cs` (XML doc comments) | **Hand-edited — source of truth** |
| `Polytoria/Polytoria.xml` | Auto (compiler output) |
| `Polytoria/def.json` | Auto (`--genapi`) |
| `Docs/docs/api/types/**/*.md` | Auto (`npm run gen`) |
| `Docs/docs/api/enums/**/*.md` | Auto (`npm run gen`) |
| Everything else under `Docs/docs/**` | Hand-edited |

Never hand-edit anything under `Docs/docs/api/types/` or
`Docs/docs/api/enums/` — those files are deleted and regenerated on every
`npm run gen` and your edits will be lost. Fix the XML doc comment in the C#
source instead.