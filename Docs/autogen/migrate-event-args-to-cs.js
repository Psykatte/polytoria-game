#!/usr/bin/env node
/**
 * One-shot migration: ports YAML event Arguments (name/type pairs that
 * reflection can't recover) → `<param name="…" type="…">…</param>` XML doc
 * tags on PTSignal fields in C#.
 *
 * Safe to re-run: skips events whose C# already has matching <param> tags.
 *
 * Usage (run from Docs/):
 *   node autogen/migrate-event-args-to-cs.js [--dry-run]
 */

"use strict"

const fs   = require("fs")
const path = require("path")
const yaml = require("yaml")

const DRY_RUN     = process.argv.includes("--dry-run")
const YAML_TYPES  = path.join(__dirname, "../yaml/types")
const SCRIPTS_ROOT = path.resolve(__dirname, "../../Polytoria/scripts")

// Same hardened indexer as migrate-categories-to-cs.js
const DECL_RE = /^\s*(?:(?:public|internal|private|protected|sealed|partial|abstract|static|readonly|ref)\s+)+(?:class|struct)\s+(\w+)/
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

function escapeXmlAttr(s) {
    return s.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
}

/**
 * Find the line that declares `public PTSignal<…>? EventName`. Detects via the
 * member name appearing on a line that mentions PTSignal.
 */
function findEventLine(lines, eventName) {
    const escaped = eventName.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")
    const re = new RegExp(`\\bPTSignal\\b.*\\b${escaped}\\b|\\b${escaped}\\b.*\\bPTSignal\\b`)
    for (let i = 0; i < lines.length; i++) {
        if (re.test(lines[i])) return i
    }
    return -1
}

/**
 * Walk backwards from `eventIdx` over `[Attribute]` and `///` lines.
 * Returns { docStart, docEnd, attrStart } where:
 *   - docStart/docEnd are the inclusive range of /// lines above the attributes
 *     (-1/-1 if absent)
 *   - attrStart is the first attribute line, or eventIdx if no attributes.
 */
function findDocBlock(lines, eventIdx) {
    let i = eventIdx - 1
    while (i >= 0 && /^\s*\[/.test(lines[i])) i--
    const attrStart = i + 1
    let docEnd = i
    while (i >= 0 && /^\s*\/\/\//.test(lines[i])) i--
    const docStart = i + 1
    if (docStart > docEnd) return { docStart: -1, docEnd: -1, attrStart }
    return { docStart, docEnd, attrStart }
}

const stats = { written: 0, skipped: 0, notFound: 0, missingFile: 0 }
const yamlFiles = fs.readdirSync(YAML_TYPES).filter(f => f.endsWith(".yaml"))

for (const yamlFile of yamlFiles) {
    const c = yaml.parse(fs.readFileSync(path.join(YAML_TYPES, yamlFile), "utf-8"))
    const className = c.Name ?? path.basename(yamlFile, ".yaml")

    const events = (c.Events ?? []).filter(e => {
        if (!e.Arguments || e.Arguments === "") return false
        const args = Array.isArray(e.Arguments) ? e.Arguments : [e.Arguments]
        return args.length > 0 && args.every(a => a && a.Name)
    })
    if (events.length === 0) continue

    const csFile = csIndex[className]
    if (!csFile) {
        console.warn(`⚠  no C# file for class: ${className}`)
        stats.missingFile++
        continue
    }

    let lines = fs.readFileSync(csFile, "utf-8").split("\n")
    let modified = false

    // Apply patches bottom-up so line indices stay valid
    const patches = []
    for (const e of events) {
        const idx = findEventLine(lines, e.Name)
        if (idx === -1) {
            console.warn(`  ⚠  event not found in ${path.basename(csFile)}: ${className}.${e.Name}`)
            stats.notFound++
            continue
        }
        patches.push({ idx, event: e })
    }
    patches.sort((a, b) => b.idx - a.idx)

    for (const { idx, event } of patches) {
        const { docStart, docEnd, attrStart } = findDocBlock(lines, idx)
        const args = Array.isArray(event.Arguments) ? event.Arguments : [event.Arguments]

        // If an existing /// block already includes <param> tags, skip
        if (docStart !== -1) {
            const block = lines.slice(docStart, docEnd + 1).join("\n")
            if (/<param\s/.test(block)) { stats.skipped++; continue }
        }

        const indent = (lines[idx].match(/^(\s*)/) ?? ["", ""])[1]
        const paramLines = args.map(a => {
            const name = escapeXmlAttr(a.Name)
            const type = a.Type ? ` type="${escapeXmlAttr(a.Type)}"` : ""
            return `${indent}/// <param name="${name}"${type}></param>`
        })

        if (docStart !== -1) {
            // Splice <param> lines at the end of the existing doc block
            lines = [
                ...lines.slice(0, docEnd + 1),
                ...paramLines,
                ...lines.slice(docEnd + 1)
            ]
        } else {
            // No existing doc block: insert <summary/> + <param> lines above attributes
            const summaryDesc = event.Description && event.Description !== "Missing Documentation"
                ? event.Description.replace(/\s+/g, " ").trim()
                : ""
            const docLines = summaryDesc
                ? [`${indent}/// <summary>${escapeXmlAttr(summaryDesc)}</summary>`]
                : [`${indent}/// <summary></summary>`]
            lines = [
                ...lines.slice(0, attrStart),
                ...docLines,
                ...paramLines,
                ...lines.slice(attrStart)
            ]
        }
        modified = true
        stats.written++
    }

    if (modified) {
        const rel = path.relative(process.cwd(), csFile)
        console.log(`  ${DRY_RUN ? "[dry] " : ""}wrote ${rel}  (${className})`)
        if (!DRY_RUN) fs.writeFileSync(csFile, lines.join("\n"), "utf-8")
    }
}

console.log("\n──────────────────────────────────────")
console.log(`  <param> tags inserted:  ${stats.written}`)
console.log(`  Already documented:     ${stats.skipped}`)
console.log(`  Event not located:      ${stats.notFound}`)
console.log(`  Class file missing:     ${stats.missingFile}`)
if (DRY_RUN) console.log("  (dry run — no files written)")