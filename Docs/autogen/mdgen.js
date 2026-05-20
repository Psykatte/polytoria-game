// Print help and exit.
if (process.argv.includes("--help") || process.argv.includes("-h")) {
    console.log(`\
Usage: node mdgen.js [options]
Generate Markdown formatted documentation from C# documentation comments.

Options:
  --strict, -s   fail if any scriptable members are undocumented
  --help, -h     display this help and exit`)
    process.exit(0)
}

const fs = require("fs")
const path = require("path")

// When set fail if any scriptable members are undocumented.
const STRICT = process.argv.includes("--strict") || process.argv.includes("-s")

const defJsonPath  = path.join(__dirname, "../", "../", "Polytoria", "def.json")
const mdAPIPath    = path.join(__dirname, "../", "docs/api", "types")
const mdEnumPath   = path.join(__dirname, "../", "docs/api", "enums")
const iconDataPath = path.join(__dirname, "../", "docs/theme/.icons", "polytoria")

const data = JSON.parse(fs.readFileSync(defJsonPath, "utf-8"))

// Track scriptable members that lack a /// <summary> in C#.
const undocumented = []
function isMissing(desc) {
    return !desc || desc.trim() === "" || desc === "Missing Documentation"
}
function flagMissing(kind, name) {
    if (kind && name) undocumented.push(`${kind}  ${name}`)
}

// Cleanup md (excluding index.md files)
function cleanDir(dir) {
    if (!fs.existsSync(dir)) return
    for (const file of fs.readdirSync(dir)) {
        if (file === "index.md") continue
        fs.rmSync(path.join(dir, file), { recursive: true, force: true })
    }
}
console.log(`Cleaning old markdown directories...`)
cleanDir(mdAPIPath)
cleanDir(mdEnumPath)
fs.mkdirSync(mdAPIPath, { recursive: true })
fs.mkdirSync(mdEnumPath, { recursive: true })

// ─── Link & heading rendering ────────────────────────────────────────────────

// Friendly-name tables mirror the legacy main.py macros so output stays identical.
const TYPE_FRIENDLY = { "bool": "boolean", "array": "[]" }
const PARAM_TYPE_FRIENDLY = { "bool": "boolean" }

// classIndex maps a type name (and enum InternalName) to its rendered URL path under /api/.
const classIndex = {}
for (const c of data.Classes) {
    classIndex[c.Name] = c.Category ? `types/${c.Category}/${c.Name}` : `types/${c.Name}`
}
for (const e of data.Enums) {
    classIndex[e.Name] = `enums/${e.Name}`
    if (e.InternalName && e.InternalName !== e.Name) {
        classIndex[e.InternalName] = `enums/${e.Name}`
    }
}

// Returns a markdown link to the type's doc page, or null if the type is unknown.
function getClassLink(typeName) {
    const url = classIndex[typeName]
    if (!url) return null
    // Display name strips the "Enum" suffix that some C# types carry internally.
    const display = typeName.endsWith("Enum") && classIndex[typeName.slice(0, -4)]
        ? typeName.slice(0, -4)
        : typeName
    return `[${display}](/api/${url}/)`
}

// Render a type as a link if known, otherwise as inline code. Applies the friendly-name table first.
function renderType(typeName, friendlyTable) {
    const friendly = friendlyTable[typeName] !== undefined ? friendlyTable[typeName] : typeName
    return getClassLink(friendly) || `\`${friendly}\``
}

// Renders the parameters block that follows a method/event heading.
function renderParameters(params) {
    if (!params || params.length === 0) return ""
    const items = params.map(p => {
        const typeText = renderType(p.Type, PARAM_TYPE_FRIENDLY)
        const optMsg = p.IsOptional ? " - this parameter is optional" : ""
        return p.Name ? `${p.Name} [ ${typeText} ]${optMsg}` : typeText
    })
    if (items.length > 1) {
        return "\n??? quote \"Parameters\"\n" + items.map(s => "    " + s).join("\n\n")
    }
    return `\n!!! quote "**Parameters:** <span style="font-weight: normal;">${items[0]}</span>"`
}

function renderPropertyHeading(prop) {
    const typeText = renderType(prop.Type, TYPE_FRIENDLY)
    return `### :polytoria-Property: ${prop.Name} : ${typeText} { #${prop.Name} data-toc-label="${prop.Name}" }`
}

function renderMethodHeading(m) {
    const ret = m.ReturnType || "void"
    const returnText = "→ " + renderType(ret, TYPE_FRIENDLY)
    return `### :polytoria-Method: ${m.Name} ${returnText} { #${m.Name} data-toc-label="${m.Name}" }` + renderParameters(m.Parameters)
}

function renderEventHeading(e) {
    return `### <a href="/objects/types/Event/">:polytoria-Event:</a> ${e.Name} { #${e.Name} data-toc-label="${e.Name}" }` + renderParameters(e.Parameters)
}

// ─── Process API Classes ─────────────────────────────────────────────────────

// First pass: build inherited-by index
const inheritedBy = {}
for (const c of data.Classes) {
    if (!c.BaseType) continue
    if (!inheritedBy[c.BaseType]) inheritedBy[c.BaseType] = []
    inheritedBy[c.BaseType].push(c.Name)
}

for (const c of data.Classes) {
    const className = c.Name

    let mdPath
    if (c.Category) {
        const catDir = path.join(mdAPIPath, c.Category)
        fs.mkdirSync(catDir, { recursive: true })
        mdPath = path.join(catDir, className + ".md")
    } else {
        mdPath = path.join(mdAPIPath, className + ".md")
    }

    let mk = ""
    const iconPath = path.join(iconDataPath, c.Name + ".svg")
    const emojiExists = fs.existsSync(iconPath)

    function appendLine(str) { mk += str + "\n" }

    appendLine("---")
    appendLine("title: " + c.Name)
    appendLine("description:")
    appendLine(emojiExists ? "icon: polytoria/" + c.Name : "icon: polytoria/Unknown")
    appendLine("---")
    appendLine("")
    appendLine(emojiExists ? `# :polytoria-${c.Name}: ` + c.Name : "# " + c.Name)

    if (c.BaseType) {
        appendLine("")
        appendLine(`Inherits ${getClassLink(c.BaseType) || c.BaseType}`)
        appendLine("{ data-search-exclude }")
    }

    const children = inheritedBy[c.Name]
    if (children && children.length > 0) {
        appendLine("")
        appendLine("Inherited by " + children.map(n => getClassLink(n) || n).join(", "))
        appendLine("{ data-search-exclude }")
    }

    appendLine("")
    appendLine(c.Description || "Missing documentation!")
    appendLine("")
    if (isMissing(c.Description)) flagMissing("class   ", c.Name)

    if (c.Remarks) {
        appendLine('!!! note "Remarks"')
        appendLine("    " + c.Remarks.replace(/\n/g, "\n    "))
        appendLine("")
    }

    if (c.Examples && c.Examples.length > 0) {
        for (const ex of c.Examples) {
            appendLine('!!! example "Example"')
            appendLine("    " + ex.replace(/\n/g, "\n    "))
            appendLine("")
        }
    }

    if (c.SeeAlso && c.SeeAlso.length > 0) {
        appendLine("**See also:** " + c.SeeAlso.map(s => `[${s}](/api/types/${s}/)`).join(", "))
        appendLine("")
    }

    if (c.IsStatic) {
        appendLine("")
        appendLine(`{{ staticclass(${c.StaticAlias ? `"${c.StaticAlias}"` : ""}) }}`)
        appendLine("")
    }

    if (c.IsAbstract) {
        appendLine("{{ abstract() }}")
        appendLine("")
    }

    if (!c.IsInstantiable) {
        appendLine("{{ notnewable() }}")
        appendLine("")
    }

    const properties = c.Properties ?? []
    if (properties.length > 0) {
        appendLine("")
        appendLine("## Properties")
        appendLine("")
    }
    for (const prop of properties) {
        if (prop.IsObsolete) continue
        appendLine(renderPropertyHeading(prop))
        appendLine(``)
        appendLine(prop.Description || "Missing documentation!")
        appendLine(``)
        if (isMissing(prop.Description)) flagMissing("property", `${c.Name}.${prop.Name}`)
        if (prop.Remarks) {
            appendLine('!!! note "Remarks"')
            appendLine("    " + prop.Remarks.replace(/\n/g, "\n    "))
            appendLine(``)
        }
        if (prop.SeeAlso && prop.SeeAlso.length > 0) {
            appendLine("**See also:** " + prop.SeeAlso.map(s => `[${s}](/api/types/${s}/)`).join(", "))
            appendLine(``)
        }
    }

    const methods = (c.Methods ?? []).filter(m => !m.IsObsolete && !m.Name.startsWith("__"))
    if (methods.length > 0) {
        appendLine("")
        appendLine("## Methods")
        appendLine("")
    }
    for (const m of methods) {
        appendLine(renderMethodHeading(m))
        appendLine(``)
        appendLine(m.Description || "Missing documentation!")
        appendLine(``)
        if (isMissing(m.Description)) flagMissing("method  ", `${c.Name}.${m.Name}`)
        if (m.Returns) {
            appendLine(`!!! quote "**Returns:** <span style="font-weight: normal;">${m.Returns}</span>"`)
            appendLine(``)
        }
        if (m.Remarks) {
            appendLine('!!! note "Remarks"')
            appendLine("    " + m.Remarks.replace(/\n/g, "\n    "))
            appendLine(``)
        }
        if (m.Examples && m.Examples.length > 0) {
            for (const ex of m.Examples) {
                appendLine('!!! example "Example"')
                appendLine("    " + ex.replace(/\n/g, "\n    "))
                appendLine(``)
            }
        }
        if (m.SeeAlso && m.SeeAlso.length > 0) {
            appendLine("**See also:** " + m.SeeAlso.map(s => `[${s}](/api/types/${s}/)`).join(", "))
            appendLine(``)
        }
    }

    const events = c.Events ?? []
    if (events.length > 0) {
        appendLine("")
        appendLine("## Events")
        appendLine("")
    }
    for (const e of events) {
        appendLine(renderEventHeading(e))
        appendLine(``)
        appendLine(e.Description || "")
        appendLine(``)
        if (isMissing(e.Description)) flagMissing("event   ", `${c.Name}.${e.Name}`)
    }

    fs.writeFileSync(mdPath, mk)
}
console.log(`Converted ${data.Classes.length} classes to Markdown.`)

// ─── Process Enums ────────────────────────────────────────────────────────────

for (const e of data.Enums) {
    const mdPath = path.join(mdEnumPath, e.Name + ".md")
    let mk = ""
    function appendLine(str) { mk += str + "\n" }

    const desc = e.Description && e.Description !== "Missing Documentation" ? e.Description : ""

    appendLine("---")
    appendLine("title: " + e.Name)
    appendLine("description: " + desc)
    appendLine("icon: polytoria/Enum")
    appendLine("---")
    appendLine("")
    appendLine("# " + e.Name)
    appendLine("")

    if (desc) {
        appendLine(desc)
        appendLine("")
    }

    if (isMissing(e.Description)) flagMissing("enum    ", e.Name)

    appendLine("| Name | Description |")
    appendLine("| --- | --- |")

    for (const option of (e.Options ?? [])) {
        const optName = typeof option === "string" ? option : option.Name
        const optDesc = typeof option === "string" ? "" : (option.Description || "")
        const display = optDesc === "Missing Documentation" ? "" : optDesc
        appendLine(`| \`${e.Name}.${optName}\` | ${display} |`)
        if (isMissing(optDesc)) flagMissing("enumval ", `${e.Name}.${optName}`)
    }

    fs.writeFileSync(mdPath, mk)
}
console.log(`Converted ${data.Enums.length} enums to Markdown.`)

// ─── Undocumented-member report ───────────────────────────────────────────────

if (undocumented.length > 0) {
    const prefix = STRICT ? "ERROR" : "WARNING"
    console.warn(`\n${prefix}: ${undocumented.length} scriptable member(s) lack an XML doc <summary>:`)
    for (const entry of undocumented) console.warn(`  ${entry}`)
    if (STRICT) {
        console.error(`\nFailing because --strict was set.`)
        process.exit(1)
    }
}
