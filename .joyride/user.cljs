(ns user
  "Joyride user activation script - automatically loaded when Joyride starts"
  (:require ["vscode" :as vscode]
            [joyride.core :as joyride]))

;; This script runs when Joyride activates
;; Use it to register custom commands, keybindings, or startup tasks

(defn welcome-message
  "Show welcome message when Joyride activates"
  []
  (vscode/window.showInformationMessage
    "🎉 Joyride activated for MTM_Avalonia_Template!"))

;; Uncomment to show welcome message on activation
;; (welcome-message)

;; Example: Register a custom command
(comment
  (defn quick-build []
    (-> (vscode/tasks.fetchTasks)
        (.then (fn [tasks]
                 (let [build-task (first (filter #(= "Build Solution" (.-name %)) tasks))]
                   (when build-task
                     (vscode/tasks.executeTask build-task)))))))

  (joyride/register-command
    "mtm.quickBuild"
    quick-build))

;; Example: File watcher
(comment
  (defn on-file-change [uri]
    (js/console.log "File changed:" (.-fsPath uri)))

  (def watcher (vscode/workspace.createFileSystemWatcher "**/*.cs"))
  (.onDidChange watcher on-file-change))

;; Example: Custom status bar item
(comment
  (def status-item (vscode/window.createStatusBarItem vscode/StatusBarAlignment.Right 100))
  (set! (.-text status-item) "$(rocket) MTM")
  (set! (.-tooltip status-item) "MTM Avalonia Template")
  (.show status-item))

(println "Joyride user.cljs loaded for MTM_Avalonia_Template")
