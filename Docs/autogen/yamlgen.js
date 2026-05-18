const fs = require("fs")
const path = require("path")
const yaml = require("yaml")

const yamlAPIPath = path.join(__dirname, "../", "yaml", "types")
const yamlEnumPath = path.join(__dirname, "../", "yaml", "enums")

if (!fs.existsSync(yamlAPIPath)) {
    fs.mkdirSync(yamlAPIPath, { recursive: true })
}

if (!fs.existsSync(yamlEnumPath)) {
    fs.mkdirSync(yamlEnumPath, { recursive: true })
}

const data = JSON.parse(fs.readFileSync(path.join(__dirname, "../", "../", "Polytoria", "def.json"), "utf-8"))

// Track current classes and enums
const currentClasses = new Set(data.Classes.map(c => c.Name))
const currentEnums = new Set(data.Enums.map(e => e.Name))

// Clean up removed classes
if (fs.existsSync(yamlAPIPath)) {
    const existingFiles = fs.readdirSync(yamlAPIPath)
    for (const file of existingFiles) {
        if (file.endsWith('.yaml')) {
            const className = path.basename(file, '.yaml')
            if (!currentClasses.has(className)) {
                const filePath = path.join(yamlAPIPath, file)
                fs.unlinkSync(filePath)
                console.log(`Removed obsolete class: ${className}`)
            }
        }
    }
}

// Clean up removed enums
if (fs.existsSync(yamlEnumPath)) {
    const existingFiles = fs.readdirSync(yamlEnumPath)
    for (const file of existingFiles) {
        if (file.endsWith('.yaml')) {
            const enumName = path.basename(file, '.yaml')
            if (!currentEnums.has(enumName)) {
                const filePath = path.join(yamlEnumPath, file)
                fs.unlinkSync(filePath)
                console.log(`Removed obsolete enum: ${enumName}`)
            }
        }
    }
}

// Process Classes
for (const c of data.Classes) {
    let yamlPath = path.join(yamlAPIPath, c.Name + ".yaml")

    // Load existing data if file exists
    let existingDescriptions = { Properties: {}, Methods: {}, Events: {} };
    let existingRemarks = { Properties: {}, Methods: {} };
    let existingArguments = { Events: {} };
    let existingClassDescription = "";
    let existingClassRemarks = null;
    let existingClassCategory = "";

    if (fs.existsSync(yamlPath)) {
        const existingYaml = fs.readFileSync(yamlPath, "utf-8");
        const existingData = yaml.parse(existingYaml);

        existingClassDescription = existingData.Description || "";
        existingClassRemarks = existingData.Remarks || null;

        if (existingData.Category) {
            existingClassCategory = existingData.Category;
        }

        // Build lookup maps for existing descriptions and remarks
        if (existingData.Properties) {
            const props = Array.isArray(existingData.Properties)
                ? existingData.Properties
                : [existingData.Properties];
            props.forEach(p => {
                if (p.Name) {
                    existingDescriptions.Properties[p.Name] = p.Description || "";
                    existingRemarks.Properties[p.Name] = p.Remarks || null;
                }
            });
        }

        if (existingData.Methods) {
            const methods = Array.isArray(existingData.Methods)
                ? existingData.Methods
                : [existingData.Methods];
            methods.forEach(m => {
                if (m.Name) {
                    existingDescriptions.Methods[m.Name] = m.Description || "";
                    existingRemarks.Methods[m.Name] = m.Remarks || null;
                }
            });
        }

        if (existingData.Events) {
            const events = Array.isArray(existingData.Events)
                ? existingData.Events
                : [existingData.Events];
            events.forEach(e => {
                if (e.Name) {
                    existingDescriptions.Events[e.Name] = e.Description || "";
                    existingArguments.Events[e.Name] = e.Arguments || "";
                }
            });
        }
    }

    // YAML description wins when it's real content; JSON description used as fallback
    function mergeDesc(yamlDesc, jsonDesc) {
        if (!yamlDesc || yamlDesc === "Missing Documentation") return jsonDesc || "Missing Documentation";
        return yamlDesc;
    }

    let obj = {
        ...c,
        Name: c.Name,
        Description: mergeDesc(existingClassDescription, c.Description),
        Remarks: existingClassRemarks !== null ? existingClassRemarks : (c.Remarks || null),
        Examples: c.Examples || null,
        SeeAlso: c.SeeAlso || null,
        Category: existingClassCategory,
        BaseType: c.BaseType,
        Properties: [],
        Methods: [],
        Events: [],
    }

    // Add properties
    for (const prop of c.Properties) {
        if (prop.IsObsolete) continue
        obj.Properties.push({
            ...prop,
            Description: mergeDesc(existingDescriptions.Properties[prop.Name], prop.Description),
            Remarks: prop.Name in existingRemarks.Properties
                ? existingRemarks.Properties[prop.Name]
                : (prop.Remarks || null),
            SeeAlso: prop.SeeAlso || null,
        })
    }

    // Add methods
    for (const m of c.Methods) {
        if (m.IsObsolete) continue

        // Ignore metamethods
        if (m.Name.startsWith("__")) continue
        obj.Methods.push({
            ...m,
            Description: mergeDesc(existingDescriptions.Methods[m.Name], m.Description),
            Remarks: m.Name in existingRemarks.Methods
                ? existingRemarks.Methods[m.Name]
                : (m.Remarks || null),
            Examples: m.Examples || null,
            SeeAlso: m.SeeAlso || null,
            Returns: m.Returns || null,
        })
    }

    // Add events
    for (const e of c.Events) {
        obj.Events.push({
            ...e,
            Description: mergeDesc(existingDescriptions.Events[e.Name], e.Description),
            Arguments: existingArguments.Events[e.Name] || ""
        })
    }

    fs.writeFileSync(yamlPath, yaml.stringify(obj))
}

// Process Enums
for (const e of data.Enums) {
    let yamlPath = path.join(yamlEnumPath, e.Name + ".yaml")

    let obj = {
        ...e,
        Options: []
    }

    let existingDescriptions = {};
    let existingEnumDescription = "";
    if (fs.existsSync(yamlPath)) {
        const existingYaml = fs.readFileSync(yamlPath, "utf-8");
        const existingData = yaml.parse(existingYaml);

        existingEnumDescription = existingData.Description || "";

        if (existingData.Options) {
            const options = Array.isArray(existingData.Options)
                ? existingData.Options
                : [existingData.Options];
            options.forEach(o => {
                if (o.Name) existingDescriptions[o.Name] = o.Description || "";
            });
        }
    }

    // Prefer YAML description for enum if it's real content; fall back to JSON
    obj.Description = (!existingEnumDescription || existingEnumDescription === "Missing Documentation")
        ? (e.Description || "")
        : existingEnumDescription;

    // Add options — e.Options is now [{Name, Description?}] from JSON
    for (const option of e.Options) {
        const optName = typeof option === "string" ? option : option.Name;
        const jsonDesc = typeof option === "string" ? "" : (option.Description || "");
        obj.Options.push({
            Name: optName,
            Description: existingDescriptions[optName] || jsonDesc || ""
        })
    }

    fs.writeFileSync(yamlPath, yaml.stringify(obj))
}