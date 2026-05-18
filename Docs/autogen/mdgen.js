const fs = require("fs")
const path = require("path")

const defJsonPath  = path.join(__dirname, "../", "../", "Polytoria", "def.json")
const mdAPIPath    = path.join(__dirname, "../", "docs/api", "types")
const iconDataPath = path.join(__dirname, "../", "docs/theme/.icons", "polytoria")
const mdEnumPath   = path.join(__dirname, "../", "docs/api", "enums")

const data = JSON.parse(fs.readFileSync(defJsonPath, "utf-8"))

// Cleanup md (excluding index.md files)
function cleanDir(dir) {
    if (!fs.existsSync(dir)) return
    for (const file of fs.readdirSync(dir)) {
        if (file === "index.md") continue
        fs.rmSync(path.join(dir, file), { recursive: true, force: true })
    }
}
cleanDir(mdAPIPath)
cleanDir(mdEnumPath)
fs.mkdirSync(mdAPIPath, { recursive: true })
fs.mkdirSync(mdEnumPath, { recursive: true })

// ─── Process API Classes ──────────────────────────────────────────────────────

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
        appendLine(`{{ inherits("${c.BaseType}") }}`)
    }

    const children = inheritedBy[c.Name]
    if (children && children.length > 0) {
        appendLine("")
        appendLine(`{{ inherited_by([${children.map(n => `"${n}"`).join(", ")}]) }}`)
    }

    appendLine("")
    appendLine(c.Description || "Missing documentation!")
    appendLine("")

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
        appendLine(`### ${prop.Name}:${prop.Type} { property }`)
        appendLine(``)
        appendLine(prop.Description || "Missing documentation!")
        appendLine(``)
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
        const params = (m.Parameters ?? []).map(p =>
            `${p.Name};${p.Type}${p.IsOptional ? "?" : ""}`
        )
        appendLine(`### ${m.Name}(${params.join(",")}):${m.ReturnType || "void"} { method }`)
        appendLine(``)
        appendLine(m.Description || "Missing documentation!")
        appendLine(``)
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
        const args = (e.Parameters ?? []).map(p => `${p.Name};${p.Type}`)
        appendLine(`### ${e.Name}(${args.join(",")}) { event }`)
        appendLine(``)
        appendLine(e.Description || "")
        appendLine(``)
    }

    fs.writeFileSync(mdPath, mk)
}
console.log(`Converted ${data.Classes.length} classes to Markdown`)

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

    appendLine("| Name | Description |")
    appendLine("| --- | --- |")

    for (const option of (e.Options ?? [])) {
        const optName = typeof option === "string" ? option : option.Name
        const optDesc = typeof option === "string" ? "" : (option.Description || "")
        const display = optDesc === "Missing Documentation" ? "" : optDesc
        appendLine(`| \`${e.Name}.${optName}\` | ${display} |`)
    }

    fs.writeFileSync(mdPath, mk)
}
console.log(`Converted ${data.Enums.length} enums to Markdown`)
