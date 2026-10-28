# Rynix Compiler ⚡

A high-performance compiler and interactive IDE for the **Rynix** programming language, written in C# on .NET Framework 4.8.

---

## 📌 Project Overview

**Rynix** is a statically-typed programming language combining the clean and expressive syntax style of Python with the strong typing, object orientation, and structured safety found in Kotlin. 

The Rynix Compiler project implements a modular, multi-phase compiler pipeline along with a companion desktop IDE interface designed for editing, scanning, compiling, and debugging Rynix source programs.

> **Project Status:** Currently completed **Phase One: Lexical Analysis (Scanner)**.

---

## 🗺️ Compiler Architecture & Roadmap

The compiler follows classic compiler design principles structured into sequential phases:

```
+-------------------------------------------------------------+
|                     Rynix Source Code                       |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|         Phase 1: Lexical Analysis (Scanner.cs)              |  <-- [COMPLETED]
|   - Token Stream Generation (Token_Class)                   |
|   - Comments Stripping (// and /* ... */)                   |
|   - Error Diagnostics & Position Tracking (Errors.cs)       |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|         Phase 2: Syntax Analysis (Parser)                   |  <-- [IN PROGRESS]
|   - Context-Free Grammar (CFG) validation                   |
|   - Parse Tree / Concrete Syntax Tree (CST) construction    |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|         Phase 3: Semantic Analysis                          |  <-- [PLANNED]
|   - Symbol Table management & scope resolution              |
|   - Static type checking & semantic validation              |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|         Phase 4: Intermediate / Target Code Generation      |  <-- [PLANNED]
|   - Intermediate Representation (IR) / Execution Engine     |
+-------------------------------------------------------------+
```

| Phase | Component | File | Status | Description |
| :--- | :--- | :--- | :---: | :--- |
| **Phase 1** | **Lexical Analyzer** | `Scanner.cs`, `Errors.cs` |  **Completed** | Tokenizes raw source into classified tokens and detects lexical errors. |
| **Phase 2** | **Syntax Analyzer** | `Parser.cs` |  *Upcoming* | Validates token grammar and builds the parse tree. |
| **Phase 3** | **Semantic Analyzer** | `SemanticAnalyzer.cs` |  *Planned* | Type checking, variable scope resolution, and symbol table. |
| **Phase 4** | **Code Generation / Runtime** | `CodeGen.cs` |  *Planned* | Emits executable target code or bytecode. |

---

## 🔬 Phase One: Lexical Analysis Deep Dive

The Lexical Analyzer (`Scanner.cs`) scans input source code character-by-character, discarding whitespace and comments while extracting recognized lexemes into structured `Token` instances.

### 1. Token Classification (`Token_Class`)

Tokens recognized by Rynix are categorized into the following classes:

#### Reserved Words / Keywords
| Category | Keywords |
| :--- | :--- |
| **Declarations** | `var` (mutable variable), `val` (immutable constant), `fun` (function), `class` (class definition) |
| **Control Flow** | `if`, `else`, `while`, `for`, `in`, `return`, `break`, `continue` |
| **OOP & Instantiation** | `init` (constructor), `new` (object creation) |
| **Built-in Functions** | `print` (standard output), `input` (standard input) |
| **Literals / Values** | `true`, `false`, `null` |

#### Data Types
Rynix is strongly typed and supports primitive and composite data types:
- `Int`: Integer numbers (e.g. `20`, `-5`)
- `Float`: Floating-point numbers (e.g. `15.75`, `-0.5`)
- `String`: Double-quoted text sequences (e.g. `"Hello Rynix"`)
- `Bool`: Boolean logic values (`true`, `false`)
- `Char`: Single-quoted characters (e.g. `'A'`, `'\n'`)
- `Array`: Ordered homogeneous collections (e.g. `Array<Int>`)
- `Void`: Absence of a return value for functions

#### Operators & Delimiters
- **Arithmetic Operators:** `+`, `-`, `*`, `/`, `%`
- **Relational Operators:** `==`, `!=`, `<`, `>`, `<=`, `>=`
- **Assignment Operator:** `=`
- **Range Operator:** `..` (used in for-loops, e.g. `1..10`)
- **Delimiters / Punctuation:**
  - Parentheses: `(`, `)`
  - Braces: `{`, `}`
  - Brackets: `[`, `]`
  - Punctuation: `.`, `:`, `,`

#### Literals & Identifiers (Regex Validation)
- **Identifiers:** `^[a-zA-Z_][a-zA-Z0-9_]*$` (cannot conflict with reserved words).
- **Numeric Constants:** `^-?[0-9]+(\.[0-9]+)?$` (supports positive and negative integers and floating-point values).
- **String Literals:** `^"([^"\\\r\n]|\\.)*"$` (supports escape characters such as `\"`, `\\`, `\n`).
- **Character Literals:** Single character enclosed in single quotes `'a'` or valid escape sequences `'\n'`.

---

### 2. Comments Handling

The scanner automatically skips comments and tracks line counts:
- **Single-line Comments:** `// ...` (ignored until end-of-line `\r` or `\n`).
- **Multi-line Comments:** `/* ... */` (ignored across multiple lines, preserving accurate line numbers).

---

### 3. Lexical Error Diagnostics (`Errors.cs`)

Any token that does not conform to the lexical grammar is captured in `Errors.DetailedErrors` and `Errors.Error_List`:
- **Unclosed Strings:** Strings opened with `"` but not terminated on the same line.
- **Unclosed Character Literals:** Missing closing `'` or multi-character sequences (e.g. `'abc'`).
- **Unclosed Block Comments:** `/*` without matching closing `*/`.
- **Malformed Identifiers & Numbers:** Identifiers starting with digits (e.g. `1user`) or numbers with misplaced dots/letters (e.g. `123abc`, `12.34.56`).
- **Illegal Characters / Symbols:** Unrecognized characters or unsupported symbols.

Each error captures:
- The offending **Lexeme**
- **Line Number**
- **Start Index** (character offset in source)
- **Length** (for direct editor selection)

---

## 🖥️ IDE & Developer UI (`Form1.cs`)

The project features a dedicated Windows Forms IDE for interacting with the compiler:

- **Source Code Editor:** Integrated text editor with real-time synchronized line numbers.
- **Token Stream View:** Real-time `DataGridView` displaying the token index, lexeme, and assigned `Token_Class`.
- **Interactive Error Console:** Detailed `ListBox` listing errors with format `[Line X] [Error Y] <Lexeme>`.
- **Click-to-Navigate Error Highlighting:** Clicking any error in the error list automatically focuses the editor and highlights the exact character range of the error.
- **Hotkeys:** Press `F5` or `Ctrl + Enter` to scan/compile immediately.
- **Branches:**
  - `master`: Standard IDE UI.
  - `Modern-UI`: Refreshed dark-themed UI featuring custom draggable title bar, window controls, and modern controls styling.

---

## 📖 Rynix Language Quick Reference

### 1. Variables and Constants
```rynix
// Mutable variable
var counter: Int = 1

// Immutable constant
val maxAttempts: Int = 5
```

### 2. Functions & Return Types
```rynix
fun add(a: Int, b: Int): Int {
    return a + b
}

fun greet(name: String): Void {
    print("Hello, " + name)
}
```

### 3. Conditionals & Loops
```rynix
if score >= 50 {
    print("Pass")
} else {
    print("Fail")
}

// Range-based for loop
for i in 1..5 {
    print(i)
}

// While loop
while counter <= 10 {
    counter = counter + 1
}
```

### 4. Classes & Object Orientation
```rynix
class Student {
    var name: String
    var grade: Int

    init(n: String, g: Int) {
        name = n
        grade = g
    }

    fun display(): Void {
        print(name)
        print(grade)
    }
}

fun main(): Void {
    var s = new Student("Ali", 90)
    s.display()
}
```

---

## 📁 Project Structure

```
Rynix-Compiler-Project/
│
├── Rynix_Compiler.sln              # Visual Studio Solution File
├── README.md                       # Comprehensive Project Documentation
│
└── Rynix_Compiler/
    ├── Scanner.cs                  # Phase 1: Lexical Analyzer (Tokens & Regex)
    ├── Errors.cs                   # Compiler error tracking & diagnostics
    ├── Rynix_Compiler.cs           # Main compiler pipeline controller
    ├── Form1.cs                    # Main IDE Form logic & event handlers
    ├── Form1.Designer.cs           # WinForms designer components
    ├── Form1.resx                  # UI resources
    ├── Form2.cs                    # Secondary form / parse tree preview
    ├── Program.cs                  # WinForms entry point
    ├── App.config                  # Application configuration
    ├── Rynix_Compiler.csproj       # C# Project specification (.NET Framework 4.8)
    └── Rynix Programming Language.pdf  # Full formal language specification
```

---

## 🚀 Getting Started

### Prerequisites
- **Operating System:** Windows 10/11
- **IDE:** Visual Studio 2019 or Visual Studio 2022
- **Workload:** *.NET desktop development*
- **Target Framework:** .NET Framework 4.8

### Build & Run
1. Clone the repository:
   ```bash
   git clone https://github.com/3mr5aled/Rynix-Compiler-Project.git
   cd Rynix-Compiler-Project
   ```
2. Open `Rynix_Compiler.sln` in **Visual Studio**.
3. Set the build configuration to **Debug** or **Release** (Any CPU).
4. Press **F5** or click **Start** to build and launch the compiler IDE.
5. Paste any Rynix code into the editor and click **Compile** (or press `F5` / `Ctrl+Enter`) to inspect generated tokens and lexical diagnostics.

---

## 👥 Contributors

Developed as part of the **Compiler Theory (5th Semester)** course:
- **Amr Morcy** ([@3mr5aled](https://github.com/3mr5aled))
- **Omar Abdelmoaty** ([@Leklaka-Abdelmoaty](https://github.com/Leklaka-Abdelmoaty))
