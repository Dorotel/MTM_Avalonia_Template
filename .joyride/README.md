# Joyride Workspace Scripts

Joyride enables scripting VS Code with ClojureScript. This directory contains workspace-specific scripts and configurations.

## Directory Structure

```
.joyride/
├── README.md           # This file
├── data/               # Data files (EDN, JSON)
│   ├── service-metadata.edn         # Collected service metadata
│   └── todo-replacements.edn        # TODO replacement mappings
├── scripts/            # User scripts (executed with Cmd/Ctrl+Alt+J S)
│   ├── example.cljs    # Example script
│   └── Services/       # Service documentation automation
│       ├── collect-service-metadata.cljs
│       ├── apply-todo-replacements.cljs
│       ├── generate-service-docs.cljs
│       └── TODO-WORKFLOW-README.md
├── src/                # Namespace sources (require in scripts)
│   └── mtm/helpers.cljs
└── user.cljs           # User activation script (auto-loaded)
```

## MTM Custom Commands

This workspace includes pre-configured commands for service documentation automation:

- **mtm.collectServiceMetadata** - Extract metadata from C# service files
- **mtm.applyTodoReplacements** - Apply TODO replacements to documentation
- **mtm.generateServiceDocs** - Generate new service documentation

**Quick Start**: See `.joyride/scripts/Services/TODO-WORKFLOW-README.md` for complete workflow.

## Getting Started

### 1. Activate Joyride

Press `Ctrl+Alt+J` then `Ctrl+Alt+J` (double activation) to start Joyride.

### 2. Run a Script

- `Ctrl+Alt+J S` - Run a script from `scripts/` folder
- `Ctrl+Alt+J R` - Run current file as Joyride script

### 3. REPL

- `Ctrl+Alt+J Ctrl+Alt+R` - Connect to Joyride REPL
- Evaluate expressions interactively

## Example Scripts

### Hello World

Create `.joyride/scripts/hello.cljs`:

```clojure
(ns hello
  (:require ["vscode" :as vscode]))

(defn activate []
  (vscode/window.showInformationMessage "Hello from Joyride!"))

(activate)
```

Run with `Ctrl+Alt+J S` → Select "hello"

### Open File

```clojure
(ns open-file
  (:require ["vscode" :as vscode]))

(defn open-file [path]
  (-> (vscode/workspace.openTextDocument path)
      (.then #(vscode/window.showTextDocument %))))

(open-file "README.md")
```

### Register Command

Create `.joyride/user.cljs`:

```clojure
(ns user
  (:require ["vscode" :as vscode]
            [joyride.core :as joyride]))

(defn my-command []
  (vscode/window.showInformationMessage "Custom command executed!"))

(joyride/register-command
  "myproject.myCommand"
  my-command)
```

Add to `.vscode/settings.json`:

```json
{
  "command.keybindings": [
    {
      "command": "myproject.myCommand",
      "key": "ctrl+shift+h"
    }
  ]
}
```

## Common Use Cases

### 1. Build Task Automation

```clojure
(ns build
  (:require ["vscode" :as vscode]))

(defn run-build []
  (-> (vscode/tasks.fetchTasks)
      (.then (fn [tasks]
               (let [build-task (first (filter #(= "Build Solution" (.-name %)) tasks))]
                 (when build-task
                   (vscode/tasks.executeTask build-task)))))))
```

### 2. File Template Generator

```clojure
(ns templates
  (:require ["vscode" :as vscode]
            ["fs" :as fs]))

(defn create-service [name]
  (let [template (str "public class " name "Service : I" name "Service\n{\n    // Implementation\n}\n")]
    (fs/writeFileSync (str "Services/" name "Service.cs") template)))
```

### 3. Workspace Navigation

```clojure
(ns navigation
  (:require ["vscode" :as vscode]))

(defn goto-related-file []
  (let [current-file (-> vscode/window.activeTextEditor
                         (.-document)
                         (.-fileName))
        related (if (.endsWith current-file ".cs")
                  (.replace current-file ".cs" "Tests.cs")
                  (.replace current-file "Tests.cs" ".cs"))]
    (-> (vscode/workspace.openTextDocument related)
        (.then #(vscode/window.showTextDocument %)))))
```

## Resources

- [Joyride Documentation](https://github.com/BetterThanTomorrow/joyride)
- [Joyride Examples](https://github.com/BetterThanTomorrow/joyride/tree/master/examples)
- [VS Code API Reference](https://code.visualstudio.com/api/references/vscode-api)

## Tips

- Use `(js/console.log ...)` for debugging
- Check Output panel (View → Output → Joyride) for logs
- Reload window (`Ctrl+R`) after modifying `user.cljs`
- Use `prn` and `println` for REPL output

---

**Last Updated**: 2025-10-05
