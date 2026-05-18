const fs = require("fs")
const path = require("path")
const yaml = require("yaml")

const yamlAPIPath = path.join(__dirname, "../", "yaml", "types")
const mdAPIPath = path.join(__dirname, "../", "docs/api", "types")
const iconDataPath = path.join(__dirname, "../", "docs/theme/.icons", "polytoria")
const yamlEnumPath = path.join(__dirname, "../", "yaml", "enums")
const mdEnumPath = path.join(__dirname, "../", "docs/api", "enums")

// Cleanup md (excluding index.md files)
if (fs.existsSync(mdAPIPath)) {
    const files = fs.readdirSync(mdAPIPath)
    for (const file of files) {
        if (file !== 'index.md') {
            const filePath = path.join(mdAPIPath, file)
            fs.rmSync(filePath, { recursive: true, force: true })
        }
    }
}

if (fs.existsSync(mdEnumPath)) {
    const files = fs.readdirSync(mdEnumPath)
    for (const file of files) {
        if (file !== 'index.md') {
            const filePath = path.join(mdEnumPath, file)
            fs.rmSync(filePath, { recursive: true, force: true })
        }
    }
}

// Create directories
if (!fs.existsSync(mdAPIPath)) {
    fs.mkdirSync(mdAPIPath, { recursive: true })
}

if (!fs.existsSync(yamlAPIPath)) {
    fs.mkdirSync(yamlAPIPath, { recursive: true })
}

if (!fs.existsSync(mdEnumPath)) {
    fs.mkdirSync(mdEnumPath, { recursive: true })
}

if (!fs.existsSync(yamlEnumPath)) {
    fs.mkdirSync(yamlEnumPath, { recursive: true })
}

// Process API Classes
const yamlFiles = fs.readdirSync(yamlAPIPath).filter(file => file.endsWith('.yaml'));

const inheritedBy = {}
const classDataMap = {}

// First pass, check for inherited by
for (const yamlFile of yamlFiles) {
    const yamlPath = path.join(yamlAPIPath, yamlFile)
    const yamlContent = fs.readFileSync(yamlPath, "utf-8")
    const c = yaml.parse(yamlContent)
    const className = path.basename(yamlFile, '.yaml')
    classDataMap[className] = c

    if (c.BaseType) {
        if (!inheritedBy[c.BaseType]) {
            inheritedBy[c.BaseType] = []
        }
        inheritedBy[c.BaseType].push(c.Name || className)
    }
}

// Second pass, generate md files
for (const yamlFile of yamlFiles) {
    const className = path.basename(yamlFile, '.yaml')
    const c = classDataMap[className]

    let mdPath
    if (c.Category) {
        const catDir = path.join(mdAPIPath, c.Category)
        if (!fs.existsSync(catDir)) {
            fs.mkdirSync(catDir, { recursive: true })
        }
        mdPath = path.join(catDir, className + ".md")
    } else {
        mdPath = path.join(mdAPIPath, className + ".md")
    }

    let mk = ""
    const iconPath = path.join(iconDataPath, c.Name + ".svg")
    const emojiExists = fs.existsSync(iconPath)

    function appendLine(str) {
        mk += str + "\n"
    }

    appendLine("---")
    appendLine("title: " + c.Name)
    appendLine("description:")
    if (emojiExists) {
        appendLine("icon: polytoria/" + c.Name)
    } else {
        appendLine("icon: polytoria/Unknown")
    }
    appendLine("---")
    appendLine("")
    if (emojiExists) {
        appendLine(`# :polytoria-${c.Name}: ` + c.Name)
    } else {
        appendLine("# " + c.Name)
    }

    if (c.BaseType) {
        appendLine("")
        appendLine(`{{ inherits("${c.BaseType}") }}`)
    }

    // Inherited by
    const children = inheritedBy[c.Name || className]
    if (children && children.length > 0) {
        appendLine("")
        appendLine(`{{ inherited_by([${children.map(n => `"${n}"`).join(", ")}]) }}`)
    }

    appendLine("")
    appendLine(c.Description)
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

    const properties = c.Properties ? (Array.isArray(c.Properties) ? c.Properties : [c.Properties]) : [];

    if (properties.length > 0) {
        appendLine("")
        appendLine("## Properties")
        appendLine("")
    }

    for (const prop of properties) {
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

    const methods = c.Methods ? (Array.isArray(c.Methods) ? c.Methods : [c.Methods]) : [];

    if (methods.length > 0) {
        appendLine("")
        appendLine("## Methods")
        appendLine("")
    }
    for (const m of methods) {
        if (m.IsObsolete) continue
        let params = []

        const parameters = m.Parameters ? (Array.isArray(m.Parameters) ? m.Parameters : [m.Parameters]) : [];
        for (const p of parameters) {
            params.push(`${p.Name};${p.Type}${p.IsOptional ? "?" : ""}`)
        }

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

    const events = c.Events ? (Array.isArray(c.Events) ? c.Events : [c.Events]) : [];

    if (events.length > 0) {
        appendLine("")
        appendLine("## Events")
        appendLine("")
    }

    for (const e of events) {
        let args = []

        const aargs = e.Arguments ? (Array.isArray(e.Arguments) ? e.Arguments : [e.Arguments]) : [];
        for (const arg of aargs) {
            args.push(`${arg.Name};${arg.Type}`)
        }

        appendLine(`### ${e.Name}(${args.join(",")}) { event }`)
        appendLine(``)
        appendLine(e.Description || "")
        appendLine(``)
    }

    fs.writeFileSync(mdPath, mk)
}

console.log(`Converted ${yamlFiles.length} YAML files to Markdown`)

// Process Enums
const yamlEnumFiles = fs.readdirSync(yamlEnumPath).filter(file => file.endsWith('.yaml'));

for (const yamlFile of yamlEnumFiles) {
    const yamlPath = path.join(yamlEnumPath, yamlFile);
    const yamlContent = fs.readFileSync(yamlPath, "utf-8");

    const e = yaml.parse(yamlContent);
    const enumName = path.basename(yamlFile, '.yaml');
    let mdPath = path.join(mdEnumPath, enumName + ".md")
    let mk = ""

    function appendLine(str) {
        mk += str + "\n"
    }

    appendLine("---")
    appendLine("title: " + e.Name)
    appendLine("description: " + (e.Description && e.Description !== "Missing Documentation" ? e.Description : ""))
    appendLine("icon: polytoria/Enum")
    appendLine("---")
    appendLine("")
    appendLine("# " + e.Name)
    appendLine("")

    if (e.Description && e.Description !== "Missing Documentation") {
        appendLine(e.Description)
        appendLine("")
    }

    appendLine("| Name | Description |")
    appendLine("| --- | --- |")

    const options = e.Options ? (Array.isArray(e.Options) ? e.Options : [e.Options]) : [];
    for (const option of options) {
        const optionName = typeof option === 'string' ? option : option.Name;
        const optionDesc = typeof option === 'string' ? "" : (option.Description || "");
        const displayDesc = optionDesc === "Missing Documentation" ? "" : optionDesc;
        appendLine(`| \`${e.Name}.${optionName}\` | ${displayDesc} |`)
    }

    fs.writeFileSync(mdPath, mk)
}

console.log(`Converted ${yamlEnumFiles.length} enum YAML files to Markdown`)