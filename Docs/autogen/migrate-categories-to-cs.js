#!/usr/bin/env node
/**
 * One-shot migration: ports YAML `Category:` field → [DocCategory("...")] on
 * the C# class declaration. Safe to re-run: skips classes already attributed.
 *
 * Usage (run from Docs/):
 *   node autogen/migrate-categories-to-cs.js [--dry-run]
 */

"use strict"

const fs   = require("fs")
const path = require("path")
const yaml = require("yaml")

const DRY_RUN     = process.argv.includes("--dry-run")
const YAML_TYPES  = path.join(__dirname, "../yaml/types")
const SCRIPTS_ROOT = path.resolve(__dirname, "../../Polytoria/scripts")

/** Map: C# type name → absolute file path */
const csIndex = {}

// Only counts a "class X" or "struct X" line when it has a real C# modifier on
// it (public/internal/private/protected/partial/sealed/abstract/static). This
// avoids picking up Luau-style "declare class X" strings embedded in C#.
const DECL_RE = /^\s*(?:(?:public|internal|private|protected|sealed|partial|abstract|static|readonly|ref)\s+)+(?:class|struct)\s+(\w+)/

function indexDir(dir) {
    let entries
    try { entries = fs.readdirSync(dir, { withFileTypes: true }) } catch { return }
    for (const entry of entries) {
        const full = path.join(dir, entry.name)
        if (entry.isDirectory()) {
            indexDir(full)
        } else if (entry.name.endsWith(".cs")) {
            const content = fs.readFileSync(full, "utf-8")
            for (const line of content.split("\n")) {
                const m = line.match(DECL_RE)
                if (!m) continue
                const name = m[1]
                if (!csIndex[name]) csIndex[name] = full
                if (name.startsWith("PT") && !csIndex[name.slice(2)]) {
                    csIndex[name.slice(2)] = full
                }
            }
        }
    }
}
indexDir(SCRIPTS_ROOT)

function findClassDeclLine(lines, className) {
    const escaped = className.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")
    const re = new RegExp(`\\b(?:class|struct)\\s+(?:PT)?${escaped}\\b`)
    for (let i = 0; i < lines.length; i++) {
        if (re.test(lines[i])) return i
    }
    return -1
}

/**
 * Walk backwards from the class line over attribute and /// doc lines.
 * Returns the index of the first attribute line, or the class line itself.
 */
function findAttributeBlockStart(lines, classIdx) {
    let i = classIdx - 1
    while (i >= 0 && /^\s*\[/.test(lines[i])) i--
    return i + 1
}

const stats = { written: 0, skipped: 0, notFound: 0 }
const yamlFiles = fs.readdirSync(YAML_TYPES).filter(f => f.endsWith(".yaml"))

for (const yamlFile of yamlFiles) {
    const c = yaml.parse(fs.readFileSync(path.join(YAML_TYPES, yamlFile), "utf-8"))
    const className = c.Name ?? path.basename(yamlFile, ".yaml")
    const category = c.Category

    if (!category || category.trim() === "") continue

    const csFile = csIndex[className]
    if (!csFile) {
        console.warn(`⚠  no C# file for class: ${className}`)
        stats.notFound++
        continue
    }

    let lines = fs.readFileSync(csFile, "utf-8").split("\n")
    const classIdx = findClassDeclLine(lines, className)
    if (classIdx === -1) {
        console.warn(`⚠  class decl not found in ${csFile}: ${className}`)
        stats.notFound++
        continue
    }

    // Skip if any line in the attribute block already has DocCategory
    const attrStart = findAttributeBlockStart(lines, classIdx)
    let alreadyAttributed = false
    for (let i = attrStart; i < classIdx; i++) {
        if (/\bDocCategory\b/.test(lines[i])) { alreadyAttributed = true; break }
    }
    if (alreadyAttributed) { stats.skipped++; continue }

    // Insert [DocCategory("…")] immediately above the class declaration,
    // using the same indentation as the class line.
    const indent = (lines[classIdx].match(/^(\s*)/) ?? ["", ""])[1]
    const attrLine = `${indent}[DocCategory("${category}")]`
    lines = [...lines.slice(0, classIdx), attrLine, ...lines.slice(classIdx)]

    // Ensure `using Polytoria.Attributes;` is present
    const hasUsing = lines.some(l => /^\s*using\s+Polytoria\.Attributes\s*;/.test(l))
    if (!hasUsing) {
        // Insert after the last existing `using` directive, or before the namespace
        let lastUsing = -1
        let nsLine = -1
        for (let i = 0; i < lines.length; i++) {
            if (/^\s*using\s+[\w.]+\s*;/.test(lines[i])) lastUsing = i
            if (nsLine === -1 && /^\s*namespace\s+/.test(lines[i])) nsLine = i
        }
        const insertAt = lastUsing !== -1 ? lastUsing + 1 : (nsLine !== -1 ? nsLine : 0)
        const usingLine = "using Polytoria.Attributes;"
        // Add a blank line after if inserting before namespace with nothing between
        const insertLines = lastUsing === -1 && nsLine !== -1 ? [usingLine, ""] : [usingLine]
        lines = [...lines.slice(0, insertAt), ...insertLines, ...lines.slice(insertAt)]
    }

    const rel = path.relative(process.cwd(), csFile)
    console.log(`  ${DRY_RUN ? "[dry] " : ""}${rel}  ← ${className} (${category})`)
    if (!DRY_RUN) fs.writeFileSync(csFile, lines.join("\n"), "utf-8")
    stats.written++
}

console.log("\n──────────────────────────────────────")
console.log(`  Categories applied:   ${stats.written}`)
console.log(`  Already attributed:   ${stats.skipped}`)
console.log(`  Class not located:    ${stats.notFound}`)
if (DRY_RUN) console.log("  (dry run — no files written)")