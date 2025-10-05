(ns example
  "Example Joyride script demonstrating VS Code automation"
  (:require ["vscode" :as vscode]))

(defn show-workspace-info
  "Display current workspace information"
  []
  (let [workspace-folders (-> vscode/workspace.workspaceFolders
                              (js->clj :keywordize-keys true))
        folder-count (count workspace-folders)
        folder-names (map #(:name %) workspace-folders)]
    (vscode/window.showInformationMessage
      (str "Workspace has " folder-count " folder(s): " (clojure.string/join ", " folder-names)))))

(defn list-open-editors
  "Show all currently open editors"
  []
  (let [editors (-> vscode/window.visibleTextEditors
                    (js->clj :keywordize-keys true))
        editor-count (count editors)
        file-names (map #(-> % :document :fileName) editors)]
    (vscode/window.showInformationMessage
      (str "Open editors: " editor-count "\n" (clojure.string/join "\n" file-names)))))

(defn get-current-file-info
  "Display information about the currently active file"
  []
  (if-let [editor vscode/window.activeTextEditor]
    (let [doc (.-document editor)
          file-name (.-fileName doc)
          line-count (.-lineCount doc)
          is-dirty (.-isDirty doc)
          language (.-languageId doc)]
      (vscode/window.showInformationMessage
        (str "File: " file-name "\n"
             "Language: " language "\n"
             "Lines: " line-count "\n"
             "Modified: " is-dirty)))
    (vscode/window.showWarningMessage "No active editor")))

;; Run one of the functions
(comment
  (show-workspace-info)
  (list-open-editors)
  (get-current-file-info))

;; Entry point when script is executed
(show-workspace-info)
