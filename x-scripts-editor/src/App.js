import logo from './logo.svg';
import './App.css';
import { Editor } from '@monaco-editor/react';
import { CodeEditor } from './components/EditorComponent';
import { ConsoleWindow } from './components/ConsoleComponent';
import { DividerComponent } from './components/DividerComponent';
import { Component } from 'react';
import { global } from './global';

export class App extends Component {
    componentDidMount(){
        global.runStartProcesses()
    }

    render(){
        return (
            <div className="App">
            <div className="ui-container">
                <CodeEditor/>
                <DividerComponent/>
                <ConsoleWindow/>
            </div>
            </div>
        );
    }
}
export default App;
