(ns mtm.helpers
  "Helper functions for MTM Avalonia Template workspace automation"
  (:require ["vscode" :as vscode]
            ["path" :as path]))

(defn workspace-root
  "Get the workspace root path"
  []
  (-> vscode/workspace.workspaceFolders
      first
      (.-uri)
      (.-fsPath)))

(defn open-file-in-workspace
  "Open a file relative to workspace root"
  [relative-path]
  (let [full-path (path/join (workspace-root) relative-path)]
    (-> (vscode/workspace.openTextDocument full-path)
        (.then #(vscode/window.showTextDocument %)))))

(defn run-task-by-name
  "Execute a VS Code task by name"
  [task-name]
  (-> (vscode/tasks.fetchTasks)
      (.then (fn [tasks]
               (let [task (first (filter #(= task-name (.-name %)) tasks))]
                 (if task
                   (vscode/tasks.executeTask task)
                   (vscode/window.showErrorMessage (str "Task not found: " task-name))))))))

(defn get-active-file-path
  "Get the path of the currently active file"
  []
  (when-let [editor vscode/window.activeTextEditor]
    (-> editor (.-document) (.-fileName))))

(defn is-csharp-file?
  "Check if current file is a C# file"
  []
  (when-let [file-path (get-active-file-path)]
    (.endsWith file-path ".cs")))

(defn show-notification
  "Show a notification with optional type (:info, :warning, :error)"
  ([message] (show-notification message :info))
  ([message type]
   (case type
     :info (vscode/window.showInformationMessage message)
     :warning (vscode/window.showWarningMessage message)
     :error (vscode/window.showErrorMessage message)
     (vscode/window.showInformationMessage message))))

(defn create-terminal
  "Create and show a new terminal with given name"
  [terminal-name]
  (let [terminal (vscode/window.createTerminal terminal-name)]
    (.show terminal)
    terminal))

(defn run-in-terminal
  "Run a command in a new or existing terminal"
  [command & {:keys [terminal-name show?]
              :or {terminal-name "Joyride" show? true}}]
  (let [terminal (create-terminal terminal-name)]
    (.sendText terminal command)
    (when show? (.show terminal))))
