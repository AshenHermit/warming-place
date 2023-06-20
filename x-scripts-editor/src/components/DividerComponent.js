import React from "react";
import { Editor } from "@monaco-editor/react";

export class DividerComponent extends React.Component{
    state = {dividerWidth: 50}
    constructor(){
        super();

        this.dividerRef = React.createRef()

        this.handleDragStart = this.handleDragStart.bind(this)
        this.handleDrag = this.handleDrag.bind(this)
        this.handleDragEnd = this.handleDragEnd.bind(this)

        this.elementsWidths = ["100%","100%"]
        this.mouseDown = false

        this.elWidth = 4
    }
    componentDidMount(){
        this.setState({dividerWidth: this.dividerRef.current.parentNode.offsetWidth*0.7})
        this.calculateElementsWidths()
        window.addEventListener("mousemove", this.handleDrag);
        window.addEventListener("mouseup", this.handleDragEnd);
    }
    componentDidUpdate(){
        this.calculateElementsWidths()
    }

    calculateElementsWidths(){
        this.elementsWidths = [
            `calc(${this.state.dividerWidth}px - ${this.elWidth}px - 2em)`,
            `calc(${this.dividerRef.current.parentNode.offsetWidth}px - ${this.state.dividerWidth}px - ${this.elWidth}px - 2em)`
        ]
        this.dividerRef.current.parentNode.children[0].style.width = this.elementsWidths[0]
        this.dividerRef.current.parentNode.children[2].style.width = this.elementsWidths[1]
    }

    handleDragStart(){
        this.mouseDown = true
    }

    handleDrag(e){
        if (!this.mouseDown) return;
        const containerWidth = this.dividerRef.current.parentNode.offsetWidth;
        console.log(e)
        const mouseX = e.pageX;
        const newDividerWidth = Math.max(0, Math.min(mouseX, containerWidth));
        this.setState({dividerWidth: newDividerWidth});
        this.calculateElementsWidths()
    }
    
    handleDragEnd(){
        this.mouseDown = false
    }

    render(){
        return <div 
            ref={this.dividerRef}
            className="divider"
            style={{ left: `calc(${this.state.dividerWidth}px - ${this.elWidth/2}px)` }}
            onMouseDown={this.handleDragStart}
            onTouchStart={this.handleDragStart}
      ></div>
    }
}