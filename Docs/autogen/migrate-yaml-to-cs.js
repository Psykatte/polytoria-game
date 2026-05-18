#!/usr/bin/env node
/**
 * One-shot migration: ports YAML descriptions → C# XML doc comments.
 * Safe to re-run: skips members that already have /// <summary>.
 *
 * Usage (run from Docs/):
 *   node autogen/migrate-yaml-to-cs.js [--dry-run]
 *
 * --dry-run  Print what would change without writing any files.
 */

"use strict"

const fs   = require("fs")
const path = require("path")
const yaml = require("yaml")

const DRY_RUN     = process.argv.includes("--dry-run")
const YAML_TYPES  = path.join(__dirname, "../yaml/types")
const YAML_ENUMS  = path.join(__dirname, "../yaml/enums")
const SCRIPTS_ROOT = path.resolve(__dirname, "../../Polytoria/scripts")

// ─── Index all C# files by every class/enum name they define ─────────────────

/** Map: C# type name → absolute file path (first file wins) */
const csIndex = {}

function indexDir(dir) {
    let entries
    try { entries = fs.readdirSync(dir, { withFileTypes: true }) } catch { return }
    for (const entry of entries) {
        const full = path.join(dir, entry.name)
        if (entry.isDirectory()) {
            indexDir(full)
        } else if (entry.name.endsWith(".cs")) {
            const content = fs.readFileSync(full, "utf-8")
            for (const m of content.matchAll(/\b(?:class|enum|struct)\s+(\w+)/g)) {
                const name = m[1]
                if (!csIndex[name]) csIndex[name] = full
                // Also index without PT prefix
                if (name.startsWith("PT") && !csIndex[name.slice(2)]) {
                    csIndex[name.slice(2)] = full
                }
            }
        }
    }
}

indexDir(SCRIPTS_ROOT)

// ─── XML helpers ──────────────────────────────────────────────────────────────

function escapeXml(s) {
    return s
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/`([^`\n]+)`/g, "<c>$1</c>")  // convert markdown backtick → <c>
}

/** Build the `/// <summary>…</summary>` lines for a description string. */
function buildSummaryLines(indent, description) {
    if (!description || description.trim() === "" || description === "Missing Documentation") {
        return null
    }
    const paragraphs = description
        .trim()
        .split(/\n{2,}/)
        .map(p => p.replace(/\s+/g, " ").trim())
        .filter(Boolean)

    const lines = [`${indent}/// <summary>`]
    if (paragraphs.length === 1) {
        lines.push(`${indent}/// ${escapeXml(paragraphs[0])}`)
    } else {
        for (const para of paragraphs) {
            lines.push(`${indent}/// <para>${escapeXml(para)}</para>`)
        }
    }
    lines.push(`${indent}/// </summary>`)
    return lines
}

// ─── C# source patching ───────────────────────────────────────────────────────

/**
 * Find the line index of a named member inside a C# source file.
 * kind: "class" | "enum" | "property" | "method" | "event" | "enumvalue"
 */
function findMemberLine(lines, memberName, kind) {
    const escaped = memberName.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")
    const nameRe = new RegExp(`\\b${escaped}\\b`)
    // Also match PT-prefixed variant for class/enum declarations
    const namePtRe = new RegExp(`\\bPT${escaped}\\b`)
    // Detects "memberName(" to distinguish method calls from property names
    const nameCallRe = new RegExp(`\\b${escaped}\\s*\\(`)

    for (let i = 0; i < lines.length; i++) {
        const t = lines[i]
        if (!nameRe.test(t) && !namePtRe.test(t)) continue
        switch (kind) {
            case "class":
            case "enum":
                if (/\b(?:class|enum|struct)\b/.test(t) && (nameRe.test(t) || namePtRe.test(t))) return i
                break
            case "property":
                // Exclude if the member name is directly followed by '(' (that's a method call or declaration)
                if (/\bpublic\b/.test(t) && !/\b(?:class|enum)\b/.test(t) && !nameCallRe.test(t)) return i
                break
            case "method":
                if (/\bpublic\b/.test(t) && nameCallRe.test(t) && !/\b(?:class|enum)\b/.test(t)) return i
                break
            case "event":
                if (/PTSignal/.test(t) && /\bpublic\b/.test(t)) return i
                break
            case "enumvalue":
                // Enum values: bare name optionally followed by = value, comma, or end of line
                if (/^\s*\w+\s*(?:=\s*\d+)?\s*,?\s*$/.test(t)) return i
                break
        }
    }
    return -1
}

/**
 * Walk backwards from memberIdx over any `[Attribute]` lines to find the first
 * attribute line (which is where we insert the doc comment above).
 * Returns -1 if the member is already documented (has /// above the attribute block).
 */
function findInsertionPoint(lines, memberIdx, kind) {
    if (kind === "enumvalue") {
        // Enum values have no attribute block — insert directly before the value line
        if (memberIdx > 0 && lines[memberIdx - 1].trim().startsWith("///")) return -1
        return memberIdx
    }

    let i = memberIdx - 1
    while (i >= 0 && lines[i].trim().startsWith("[")) {
        i--
    }
    const insertAt = i + 1
    // Already documented if the line right before the attribute block is a ///
    if (i >= 0 && lines[i].trim().startsWith("///")) return -1
    return insertAt
}

/** Return the leading whitespace of a line. */
function indentOf(line) {
    return (line.match(/^(\s*)/) ?? ["", ""])[1]
}

// ─── Per-file patching ────────────────────────────────────────────────────────

const stats = { files: 0, inserted: 0, skipped: 0, notFound: 0 }

/**
 * patches: Array<{ memberName: string, kind: string, description: string }>
 * Applies all patches to the C# file at filePath (bottom-up so indices stay valid).
 */
function patchCsFile(filePath, patches) {
    let lines = fs.readFileSync(filePath, "utf-8").split("\n")
    let modified = false

    // Resolve insertion points first
    const resolved = []
    for (const patch of patches) {
        const memberIdx = findMemberLine(lines, patch.memberName, patch.kind)
        if (memberIdx === -1) {
            console.warn(`  ⚠  not found: ${patch.kind} "${patch.memberName}"`)
            stats.notFound++
            continue
        }
        const insertAt = findInsertionPoint(lines, memberIdx, patch.kind)
        if (insertAt === -1) {
            stats.skipped++
            continue
        }
        const docLines = buildSummaryLines(indentOf(lines[memberIdx]), patch.description)
        if (!docLines) {
            stats.skipped++
            continue
        }
        resolved.push({ insertAt, docLines })
    }

    // Sort descending by insertion line so earlier inserts don't shift later indices
    resolved.sort((a, b) => b.insertAt - a.insertAt)

    for (const { insertAt, docLines } of resolved) {
        lines = [...lines.slice(0, insertAt), ...docLines, ...lines.slice(insertAt)]
        modified = true
        stats.inserted++
    }

    if (modified) {
        const rel = path.relative(process.cwd(), filePath)
        console.log(`  ${DRY_RUN ? "[dry] " : ""}wrote ${rel}`)
        if (!DRY_RUN) fs.writeFileSync(filePath, lines.join("\n"), "utf-8")
        stats.files++
    }
}

// ─── Migrate types ────────────────────────────────────────────────────────────

const yamlTypeFiles = fs.readdirSync(YAML_TYPES).filter(f => f.endsWith(".yaml"))
console.log(`\nMigrating ${yamlTypeFiles.length} type YAML files…\n`)

for (const yamlFile of yamlTypeFiles) {
    const c = yaml.parse(fs.readFileSync(path.join(YAML_TYPES, yamlFile), "utf-8"))
    const className = c.Name ?? path.basename(yamlFile, ".yaml")

    const csFile = csIndex[className]
    if (!csFile) {
        console.warn(`⚠  no C# file for class: ${className}`)
        stats.notFound++
        continue
    }

    const patches = []

    if (c.Description && c.Description !== "Missing Documentation") {
        patches.push({ memberName: className, kind: "class", description: c.Description })
    }
    for (const prop of (c.Properties ?? [])) {
        if (prop.IsObsolete || !prop.Description || prop.Description === "Missing Documentation") continue
        patches.push({ memberName: prop.Name, kind: "property", description: prop.Description })
    }
    for (const m of (c.Methods ?? [])) {
        if (m.IsObsolete || m.Name?.startsWith("__")) continue
        if (!m.Description || m.Description === "Missing Documentation") continue
        patches.push({ memberName: m.Name, kind: "method", description: m.Description })
    }
    for (const e of (c.Events ?? [])) {
        if (!e.Description || e.Description === "Missing Documentation") continue
        patches.push({ memberName: e.Name, kind: "event", description: e.Description })
    }

    if (patches.length > 0) {
        console.log(`${className} (${patches.length} patches)`)
        patchCsFile(csFile, patches)
    }
}

// ─── Migrate enums ────────────────────────────────────────────────────────────

const yamlEnumFiles = fs.readdirSync(YAML_ENUMS).filter(f => f.endsWith(".yaml"))
console.log(`\nMigrating ${yamlEnumFiles.length} enum YAML files…\n`)

for (const yamlFile of yamlEnumFiles) {
    const e = yaml.parse(fs.readFileSync(path.join(YAML_ENUMS, yamlFile), "utf-8"))
    const enumExternalName = e.Name ?? path.basename(yamlFile, ".yaml")

    // C# enum name is the internal name — try both ExternalNameEnum and the name itself
    const csFile = csIndex[enumExternalName + "Enum"]
        ?? csIndex[enumExternalName]
        ?? (e.InternalName ? csIndex[e.InternalName] : null)
    if (!csFile) {
        console.warn(`⚠  no C# file for enum: ${enumExternalName}`)
        stats.notFound++
        continue
    }

    const patches = []
    const csEnumName = e.InternalName ?? enumExternalName + "Enum"

    if (e.Description && e.Description !== "Missing Documentation") {
        patches.push({ memberName: csEnumName, kind: "enum", description: e.Description })
    }
    for (const option of (e.Options ?? [])) {
        const optName = typeof option === "string" ? option : option.Name
        const optDesc = typeof option === "string" ? "" : (option.Description ?? "")
        if (!optDesc || optDesc === "Missing Documentation") continue
        patches.push({ memberName: optName, kind: "enumvalue", description: optDesc })
    }

    if (patches.length > 0) {
        console.log(`${enumExternalName} (${patches.length} patches)`)
        patchCsFile(csFile, patches)
    }
}

// ─── Summary ─────────────────────────────────────────────────────────────────

console.log("\n──────────────────────────────────────")
console.log(`  Files written:        ${stats.files}`)
console.log(`  Comments inserted:    ${stats.inserted}`)
console.log(`  Already documented:   ${stats.skipped}`)
console.log(`  Members not located:  ${stats.notFound}`)
if (DRY_RUN) console.log("  (dry run — no files written)")