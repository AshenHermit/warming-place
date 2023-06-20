import React from "react";
import { Editor } from "@monaco-editor/react";
import { global } from "../global";

export class CodeEditor extends React.Component{
    constructor(){
        super();
        this.editor = null
        this.handleEditorDidMount = this.handleEditorDidMount.bind(this)
        this.runCode = this.runCode.bind(this)
    }

    handleEditorDidMount(editor, monaco){
        editor.updateOptions({
            theme: "vs-dark",
        });
        this.editor = editor
    }

    runCode(){
        global.api.terminalEventsProvider.runCode(this.editor.getValue())
    }

    render(){
        return <div className="editor-container">
            <div className="container-header">
                <div>editor</div>
                <button 
                    className="container-header-button"
                    onClick={this.runCode}
                >run code</button>
            </div>
            <Editor 
            className='editor' 
            defaultLanguage="python" 
            onMount={this.handleEditorDidMount}/>
        </div>
    }
}