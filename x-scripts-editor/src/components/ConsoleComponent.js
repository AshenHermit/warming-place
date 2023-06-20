import React, { createRef } from "react";
import { Editor } from "@monaco-editor/react";
import { global } from "../global";

export class ConsoleWindow extends React.Component{
    constructor(){
        super();
        this.contentChangeEvent = this.contentChangeEvent.bind(this)
        this.handleEditorDidMount = this.handleEditorDidMount.bind(this)
        this.editor = null;
    }
    
    handleEditorDidMount(editor, monaco){
        editor.updateOptions({
            theme: "vs-dark",
            wordWrap: 'on',
            wrappingIndent: 'indent',
            readOnly: true,
            lineNumbers: 'off',
        });
        this.editor = editor
        
        global.api.eventsHandlers.on_console_content_change.subscribe(this.contentChangeEvent)
    }

    contentChangeEvent(newContent){
        if (this.editor.getModel() == null) return;
        this.editor.getModel().setValue(newContent);
    }

    render(){
        return <div className="console-container">
            <div className="container-header">
                <div>console</div>
            </div>
            <Editor 
            className='editor' 
            defaultLanguage="plaintext" 
            onMount={this.handleEditorDidMount}
            defaultValue=""/>
        </div>
    }
}