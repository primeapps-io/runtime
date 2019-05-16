"use strict";
/*
*  Copyright (C) 1998-2018 by Northwoods Software Corporation. All Rights Reserved.
*/

// This file holds all of the JavaScript code specific to the BPMN.html page.

// Setup all of the Diagrams and what they need.
// This is called after the page is loaded.
window.initFunc = function () {
    init();
};

function init() {
    function checkLocalStorage() {
        try {
            window.localStorage.setItem('item', 'item');
            window.localStorage.removeItem('item');
            return true;
        } catch (e) {
            return false;
        }
    }

    if (!checkLocalStorage()) {
        var currentFile = document.getElementById("currentFile");
        currentFile.textContent = "Sorry! No web storage support. If you're using Internet Explorer / Microsoft Edge, you must load the page from a server for local storage to work.";
    }

    // setup the menubar
    jQuery("#menuui").menu();
    jQuery(function () {
        jQuery("#menuui").menu({ position: { my: "left top", at: "left top+30" } });
    });
    jQuery("#menuui").menu({
        icons: { submenu: "ui-icon-triangle-1-s" }
    });

    // hides open HTML Element
    var openDocument = document.getElementById("openDocument");
    //openDocument.style.visibility = "hidden";
    // hides remove HTML Element
    var removeDocument = document.getElementById("removeDocument");
    //removeDocument.style.visibility = "hidden";


    var $ = go.GraphObject.make;  // for more concise visual tree definitions



    // constants for design choices
    var nodeTemplateIconWitdh = 18;
    var nodeTemplateIconHeight = 18;

    var activityNodeTemplateIconWitdh = 22;
    var activityNodeTemplateIconHeight = 22;

    var GradientYellow = "rgba(0, 0, 0, 0.05)"; //Gateway
    var GradientLightGreen = "#F2F2F2";//"rgba(0, 0, 0, 0.05)";//Start and Stop
    var GradientLightGray = "#F2F2F2";//"rgba(0, 0, 0, 0.05)";//Data nodes

    var ActivityNodeFill = "#F2F2F2";//"rgba(0, 0, 0, 0.05)";//normal tasklar
    var ActivityNodeStroke = "rgba(0, 0, 0, 0)";
    var ActivityMarkerStrokeWidth = 1.5;
    var ActivityNodeWidth = 120;
    var ActivityNodeHeight = 70;
    var ActivityNodeTemplateWidth = 60;
    var ActivityNodeTemplateHeight = 60;
    var ActivityNodeStrokeWidth = 1;
    var ActivityNodeStrokeWidthIsCall = 4;

    var SubprocessNodeFill = ActivityNodeFill;
    var SubprocessNodeStroke = ActivityNodeStroke;

    var EventNodeSize = 40;
    var EventNodeInnerSize = EventNodeSize - 6;
    var EventNodeSymbolSize = EventNodeInnerSize - 14;
    var EventEndOuterFillColor = "rgba(0, 0, 0, 0.05)";
    var EventBackgroundColor = GradientLightGreen;
    var EventSymbolLightFill = "rgba(0, 0, 0, 0.5)";
    var EventSymbolDarkFill = "rgba(0, 0, 0, 0.5)";
    var EventDimensionStrokeColor = "rgba(0, 0, 0, 0)";
    var EventDimensionStrokeEndColor = "rgba(0, 0, 0, 0)";
    var EventNodeStrokeWidthIsEnd = 4;

    var GatewayNodeSize = 80;
    var GatewayNodeSymbolSize = 45;
    var GatewayNodeFill = GradientYellow;
    var GatewayNodeStroke = "rgba(0, 0, 0, 0)";
    var GatewayNodeSymbolStroke = "rgba(0, 0, 0, 0.5)";
    var GatewayNodeSymbolFill = GradientYellow;
    var GatewayNodeSymbolStrokeWidth = 3;

    var DataFill = GradientLightGray;


    // custom figures for Shapes

    go.Shape.defineFigureGenerator("Empty", function (shape, w, h) {
        return new go.Geometry();
    });

    var annotationStr = "M 150,0L 0,0L 0,600L 150,600 M 800,0";
    var annotationGeo = go.Geometry.parse(annotationStr);
    annotationGeo.normalize();
    go.Shape.defineFigureGenerator("Annotation", function (shape, w, h) {
        var geo = annotationGeo.copy();
        // calculate how much to scale the Geometry so that it fits in w x h
        var bounds = geo.bounds;
        var scale = Math.min(w / bounds.width, h / bounds.height);
        geo.scale(scale, scale);
        return geo;
    });

    var gearStr = "F M 391,5L 419,14L 444.5,30.5L 451,120.5L 485.5,126L 522,141L 595,83L 618.5,92L 644,106.5" +
        "L 660.5,132L 670,158L 616,220L 640.5,265.5L 658.122,317.809L 753.122,322.809L 770.122,348.309L 774.622,374.309" +
        "L 769.5,402L 756.622,420.309L 659.122,428.809L 640.5,475L 616.5,519.5L 670,573.5L 663,600L 646,626.5" +
        "L 622,639L 595,645.5L 531.5,597.5L 493.192,613.462L 450,627.5L 444.5,718.5L 421.5,733L 393,740.5L 361.5,733.5" +
        "L 336.5,719L 330,627.5L 277.5,611.5L 227.5,584.167L 156.5,646L 124.5,641L 102,626.5L 82,602.5L 78.5,572.5" +
        "L 148.167,500.833L 133.5,466.833L 122,432.5L 26.5,421L 11,400.5L 5,373.5L 12,347.5L 26.5,324L 123.5,317.5" +
        "L 136.833,274.167L 154,241L 75.5,152.5L 85.5,128.5L 103,105.5L 128.5,88.5001L 154.872,82.4758L 237,155" +
        "L 280.5,132L 330,121L 336,30L 361,15L 391,5 Z M 398.201,232L 510.201,275L 556.201,385L 505.201,491L 399.201,537" +
        "L 284.201,489L 242.201,385L 282.201,273L 398.201,232 Z";
    var gearGeo = go.Geometry.parse(gearStr);
    gearGeo.normalize();

    go.Shape.defineFigureGenerator("BpmnTaskService", function (shape, w, h) {
        var geo = gearGeo.copy();
        // calculate how much to scale the Geometry so that it fits in w x h
        var bounds = geo.bounds;
        var scale = Math.min(w / bounds.width, h / bounds.height);
        geo.scale(scale, scale);
        // text should go in the hand
        geo.spot1 = new go.Spot(0, 0.6, 10, 0);
        geo.spot2 = new go.Spot(1, 1);
        return geo;
    });

    var handGeo = go.Geometry.parse("F1M18.13,10.06 C18.18,10.07 18.22,10.07 18.26,10.08 18.91," +
        "10.20 21.20,10.12 21.28,12.93 21.36,15.75 21.42,32.40 21.42,32.40 21.42," +
        "32.40 21.12,34.10 23.08,33.06 23.08,33.06 22.89,24.76 23.80,24.17 24.72," +
        "23.59 26.69,23.81 27.19,24.40 27.69,24.98 28.03,24.97 28.03,33.34 28.03," +
        "33.34 29.32,34.54 29.93,33.12 30.47,31.84 29.71,27.11 30.86,26.56 31.80," +
        "26.12 34.53,26.12 34.72,28.29 34.94,30.82 34.22,36.12 35.64,35.79 35.64," +
        "35.79 36.64,36.08 36.72,34.54 36.80,33.00 37.17,30.15 38.42,29.90 39.67," +
        "29.65 41.22,30.20 41.30,32.29 41.39,34.37 42.30,46.69 38.86,55.40 35.75," +
        "63.29 36.42,62.62 33.47,63.12 30.76,63.58 26.69,63.12 26.69,63.12 26.69," +
        "63.12 17.72,64.45 15.64,57.62 13.55,50.79 10.80,40.95 7.30,38.95 3.80," +
        "36.95 4.24,36.37 4.28,35.35 4.32,34.33 7.60,31.25 12.97,35.75 12.97," +
        "35.75 16.10,39.79 16.10,42.00 16.10,42.00 15.69,14.30 15.80,12.79 15.96," +
        "10.75 17.42,10.04 18.13,10.06z ");
    handGeo.rotate(90, 0, 0);
    handGeo.normalize();
    go.Shape.defineFigureGenerator("BpmnTaskManual", function (shape, w, h) {
        var geo = handGeo.copy();
        // calculate how much to scale the Geometry so that it fits in w x h
        var bounds = geo.bounds;
        var scale = Math.min(w / bounds.width, h / bounds.height);
        geo.scale(scale, scale);
        // guess where text should go (in the hand)
        geo.spot1 = new go.Spot(0, 0.6, 10, 0);
        geo.spot2 = new go.Spot(1, 1);
        return geo;
    });


    // define the appearance of tooltips, shared by various templates
    var tooltiptemplate =
        $(go.Adornment, go.Panel.Auto,
            $(go.Shape, "RoundedRectangle",
                { fill: "whitesmoke", stroke: "gray" }),
            $(go.TextBlock,
                { margin: 3, editable: true },
                new go.Binding("text", "", function (data) {
                    if (data.text !== undefined) return data.text;
                    return "(unnamed item)";
                }))
        );


    // conversion functions used by data Bindings

    function nodeActivityTaskTypeConverter(s) {
        var tasks = ["Empty",
            "BpmnTaskMessage",
            "BpmnTaskUser",
            "BpmnTaskManual",   // Custom hand symbol
            "BpmnTaskScript",
            "BpmnTaskMessage",  // should be black on white
            "BpmnTaskService",  // Custom gear symbol
            "InternalStorage"];
        if (s < tasks.length) return tasks[s];
        return "NotAllowed"; // error
    }

    // location of event on boundary of Activity is based on the index of the event in the boundaryEventArray
    function nodeActivityBESpotConverter(s) {
        var x = 10 + (EventNodeSize / 2);
        if (s === 0) return new go.Spot(0, 1, x, 0);    // bottom left
        if (s === 1) return new go.Spot(1, 1, -x, 0);   // bottom right
        if (s === 2) return new go.Spot(1, 0, -x, 0);   // top right
        return new go.Spot(1, 0, -x - (s - 2) * EventNodeSize, 0);    // top ... right-to-left-ish spread
    }

    function nodeActivityTaskTypeColorConverter(s) {
        return (s == 5) ? "dimgray" : "white";
    }

    function nodeEventTypeConverter(s) {  // order here from BPMN 2.0 poster
        var tasks = ["NotAllowed",
            "Empty",
            "BpmnTaskMessage",
            "BpmnEventTimer",
            "BpmnEventEscalation",
            "BpmnEventConditional",
            "Arrow",
            "BpmnEventError",
            "ThinX",
            "BpmnActivityCompensation",
            "Triangle",
            "Pentagon",
            "ThickCross",
            "Circle"];
        if (s < tasks.length) return tasks[s];
        return "NotAllowed"; // error
    }

    function nodeEventDimensionStrokeColorConverter(s) {
        if (s === 8) return EventDimensionStrokeEndColor;
        return EventDimensionStrokeColor;
    }

    function nodeEventDimensionSymbolFillConverter(s) {
        if (s <= 6) return EventSymbolLightFill;
        return EventSymbolDarkFill;
    }


    //------------------------------------------  Activity Node Boundary Events   ----------------------------------------------

    var boundaryEventMenu =  // context menu for each boundaryEvent on Activity node
        $(go.Adornment, "Vertical",
            $("ContextMenuButton",
                $(go.TextBlock, "Remove event"),
                // in the click event handler, the obj.part is the Adornment; its adornedObject is the port
                {
                    click: function (e, obj) {
                        removeActivityNodeBoundaryEvent(obj.part.adornedObject);
                    }
                })
        );

    // removing a boundary event doesn't not reposition other BE circles on the node
    // just reassigning alignmentIndex in remaining BE would do that.
    function removeActivityNodeBoundaryEvent(obj) {
        window.myDiagram.startTransaction("removeBoundaryEvent");
        var pid = obj.portId;
        var arr = obj.panel.itemArray;
        for (var i = 0; i < arr.length; i++) {
            if (arr[i].portId === pid) {
                window.myDiagram.model.removeArrayItem(arr, i);
                break;
            }
        }
        window.myDiagram.commitTransaction("removeBoundaryEvent");
    }

    var boundaryEventItemTemplate =
        $(go.Panel, "Spot",
            {
                contextMenu: boundaryEventMenu,
                alignmentFocus: go.Spot.Center,
                fromLinkable: true, toLinkable: false, cursor: "pointer", fromSpot: go.Spot.Bottom,
                fromMaxLinks: 1, toMaxLinks: 0
            },
            new go.Binding("portId", "portId"),
            new go.Binding("alignment", "alignmentIndex", nodeActivityBESpotConverter),
            $(go.Shape, "Circle",
                { desiredSize: new go.Size(EventNodeSize, EventNodeSize) },
                new go.Binding("strokeDashArray", "eventDimension", function (s) {
                    return (s === 6) ? [4, 2] : null;
                }),
                new go.Binding("fromSpot", "alignmentIndex",
                    function (s) {
                        //  nodeActivityBEFromSpotConverter, 0 & 1 go on bottom, all others on top of activity
                        if (s < 2) return go.Spot.Bottom;
                        return go.Spot.Top;
                    }),
                new go.Binding("fill", "color")),
            $(go.Shape, "Circle",
                {
                    alignment: go.Spot.Center,
                    desiredSize: new go.Size(EventNodeInnerSize, EventNodeInnerSize), fill: null
                },
                new go.Binding("strokeDashArray", "eventDimension", function (s) {
                    return (s === 6) ? [4, 2] : null;
                })
            ),
            $(go.Shape, "NotAllowed",
                {
                    alignment: go.Spot.Center,
                    desiredSize: new go.Size(EventNodeSymbolSize, EventNodeSymbolSize), fill: "white"
                },
                new go.Binding("figure", "eventType", nodeEventTypeConverter)
            )
        );

    //------------------------------------------  Activity Node contextMenu   ----------------------------------------------

    var activityNodeMenu =
        $(go.Adornment, "Vertical",
            $("ContextMenuButton",
                $(go.TextBlock, "Add Email Event", { margin: 3 }),
                {
                    click: function (e, obj) {
                        addActivityNodeBoundaryEvent(2, 5);
                    }
                }),
            $("ContextMenuButton",
                $(go.TextBlock, "Add Timer Event", { margin: 3 }),
                {
                    click: function (e, obj) {
                        addActivityNodeBoundaryEvent(3, 5);
                    }
                }),
            $("ContextMenuButton",
                $(go.TextBlock, "Add Escalation Event", { margin: 3 }),
                {
                    click: function (e, obj) {
                        addActivityNodeBoundaryEvent(4, 5);
                    }
                }),
            $("ContextMenuButton",
                $(go.TextBlock, "Add Error Event", { margin: 3 }),
                {
                    click: function (e, obj) {
                        addActivityNodeBoundaryEvent(7, 5);
                    }
                }),
            $("ContextMenuButton",
                $(go.TextBlock, "Add Signal Event", { margin: 3 }),
                {
                    click: function (e, obj) {
                        addActivityNodeBoundaryEvent(10, 5);
                    }
                }),
            $("ContextMenuButton",
                $(go.TextBlock, "Add N-I Escalation Event", { margin: 3 }),
                {
                    click: function (e, obj) {
                        addActivityNodeBoundaryEvent(4, 6);
                    }
                }),
            $("ContextMenuButton",
                $(go.TextBlock, "Rename", { margin: 3 }),
                {
                    click: function (e, obj) {
                        rename(obj);
                    }
                }));


    // sub-process,  loop, parallel, sequential, ad doc and compensation markers in horizontal array
    function makeSubButton(sub) {
        if (sub)
            return [$("SubGraphExpanderButton"),
            { margin: 2, visible: false },
            new go.Binding("visible", "isSubProcess")];
        return [];
    }

    // sub-process,  loop, parallel, sequential, ad doc and compensation markers in horizontal array
    function makeMarkerPanel(sub, scale) {
        return $(go.Panel, "Horizontal",
            { alignment: go.Spot.MiddleBottom, alignmentFocus: go.Spot.MiddleBottom },
            $(go.Shape, "BpmnActivityLoop",
                { width: 12 / scale, height: 12 / scale, margin: 2, visible: false, strokeWidth: ActivityMarkerStrokeWidth },
                new go.Binding("visible", "isLoop")),
            $(go.Shape, "BpmnActivityParallel",
                { width: 12 / scale, height: 12 / scale, margin: 2, visible: false, strokeWidth: ActivityMarkerStrokeWidth },
                new go.Binding("visible", "isParallel")),
            $(go.Shape, "BpmnActivitySequential",
                { width: 12 / scale, height: 12 / scale, margin: 2, visible: false, strokeWidth: ActivityMarkerStrokeWidth },
                new go.Binding("visible", "isSequential")),
            $(go.Shape, "BpmnActivityAdHoc",
                { width: 12 / scale, height: 12 / scale, margin: 2, visible: false, strokeWidth: ActivityMarkerStrokeWidth },
                new go.Binding("visible", "isAdHoc")),
            $(go.Shape, "BpmnActivityCompensation",
                { width: 12 / scale, height: 12 / scale, margin: 2, visible: false, strokeWidth: ActivityMarkerStrokeWidth, fill: null },
                new go.Binding("visible", "isCompensation")),
            makeSubButton(sub)
        ); // end activity markers horizontal panel
    }


    function geoFunc(geoname) { //For Activity
        var geo = icons[geoname];
        if (geo === undefined) geo = icons["start"];  //TODO use this for an unknown icon name
        if (typeof geo === "string") {
            geo = icons[geoname] = go.Geometry.parse(geo, true);  // fill each geometry
        }

        return geo;
    }


    var activityNodeTemplate =
        $(go.Node, "Vertical",
            {
                locationObjectName: "SHAPEMAIN",
                name: "SHAPEMAIN",
                locationObjectName: "MAINPANEL",
                locationSpot: go.Spot.TopLeft,
                resizable: true, resizeObjectName: "PANEL",
                //toolTip: tooltiptemplate,
                desiredSize: new go.Size(ActivityNodeTemplateWidth + 200, ActivityNodeTemplateHeight + 50),
                selectionAdorned: true,  // use a Binding on the Shape.stroke to show selection
                // contextMenu: activityNodeMenu,
                selectionObjectName: "MAINPANEL",
                //itemTemplate: boundaryEventItemTemplate,
                click: mouseEnter,
                //mouseLeave: mouseLeave, 
                isShadowed: true,
                shadowOffset: new go.Point(10, 10),
                shadowBlur: 10,
                shadowColor: "rgba(0, 0, 0, 0.10)",
                selectionAdornmentTemplate:
                    $(go.Adornment, "Spot",
                        $(go.Panel, "Auto",
                            // this Adornment has a rectangular blue Shape around the selected node
                            $(go.Shape, "RoundedRectangle", {
                                fill: null, stroke: "dodgerblue", strokeWidth: 1, parameter1: 5, // corner size
                                desiredSize: new go.Size(ActivityNodeTemplateWidth + 40, ActivityNodeTemplateHeight),
                            }),
                            $(go.Placeholder)
                        )),
            },
            { resizable: false, resizeObjectName: "SHAPEMAIN" },
            new go.Binding("itemArray", "boundaryEventArray"),
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            // move a selected part into the Foreground layer, so it isn"t obscured by any non-selected parts
            //new go.Binding("layerName", "isSelected", function (s) {
            //    return s ? "Foreground" : "";
            //}).ofObject(),
            new go.Binding("click", "isSelected", function (sel) {
                if (sel) { return mouseEnter; } else { return mouseLeave; }
            }).ofObject(""),
            $(go.Panel, "Auto",
                {
                    name: "PANEL",
                    // minSize: new go.Size(ActivityNodeWidth, ActivityNodeHeight), 

                },
                $(go.Panel, "Spot",
                    $(go.Shape, "RoundedRectangle",  // the outside rounded rectangle
                        {
                            name: "MAINPANEL",  
                            fill: ActivityNodeFill, stroke: ActivityNodeStroke, desiredSize: new go.Size(ActivityNodeTemplateWidth + 40, ActivityNodeTemplateHeight),
                            parameter1: 5, // corner size
                            cursor: "move", 
                        },
                    ),
                    $(go.Shape,
                        {
                            alignment: go.Spot.Center, alignmentFocus: go.Spot.Center, margin: 8,
                            fill: "rgba(0, 0, 0, 0.5)", strokeWidth: 0, width: activityNodeTemplateIconWitdh, height: activityNodeTemplateIconHeight, cursor: "move"

                        },
                        new go.Binding("geometry", "icon", geoFunc),
                        new go.Binding("fill", "color")),
                    $(go.Shape, "Ellipse", {
                        name: "LINKIN", portId: "IN", cursor: "url(images/primeapps/bpm_cursor_arrow.cur),auto", strokeWidth: 0, desiredSize: new go.Size(10, 10),
                        alignment: new go.Spot(0, 0.5), fill: "#707070", toSpot: go.Spot.LeftSide, toLinkable: true,
                    },
                        new go.Binding("toMaxLinks", "toMaxLink")),
                    $(go.Shape, "Ellipse", {
                        name: "LINKOUT", portId: "OUT", cursor: "url(images/primeapps/bpm_cursor_arrow.cur),auto", strokeWidth: 0, desiredSize: new go.Size(10, 10),
                        alignment: new go.Spot(1, 0.5), fill: "#707070", fromSpot: go.Spot.RightSide, fromLinkable: true,
                    },
                        new go.Binding("fromMaxLinks", "fromMaxLink")),  
                    //makeMarkerPanel(false, 1) // sub-process,  loop, parallel, sequential, ad doc and compensation markers
                )
            ),
            $(go.TextBlock,
                {
                    name: "myTextBlock", alignment: go.Spot.Bottom, textAlign: "center", margin: 15, font: "bold 12px Nunito", wrap: go.TextBlock.WrapFit, stroke: "rgba(0, 0, 0, 0.5)",
                    overflow: go.TextBlock.OverflowEllipsis, maxLines: 3, width: ActivityNodeTemplateWidth + 60, editable: true, cursor: "move"
                },
                new go.Binding("text").makeTwoWay())
        );  // end go.Node, which is a Spot Panel with bound itemArray

    // ------------------------------- template for Activity / Task node in Palette  -------------------------------

    var palscale = 1;
    var activityNodeTemplateForPalette =
        $(go.Node, "Vertical",
            {
                locationObjectName: "SHAPEMAIN",
                //locationSpot: go.Spot.Center,
                selectionAdorned: false,
                //minSize: new go.Size(ActivityNodeTemplateWidth, ActivityNodeTemplateHeight),
                desiredSize: new go.Size(ActivityNodeTemplateWidth + 25, ActivityNodeTemplateHeight + 20),
                cursor: "move"
            },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            { resizable: false, resizeObjectName: "SHAPEMAIN" },
            $(go.Panel, "Spot",
                {
                    name: "PANEL",
                    desiredSize: new go.Size(ActivityNodeTemplateWidth / palscale, ActivityNodeTemplateHeight / palscale)
                },
                $(go.Shape, "RoundedRectangle",  // the outside rounded rectangle
                    {
                        name: "SHAPE",
                        fill: ActivityNodeFill, stroke: ActivityNodeStroke,
                        parameter1: 5 / palscale  // corner size (default 10)
                    }),

                $(go.Shape,
                    {
                        margin: 10, fill: "rgba(0, 0, 0, 0.5)", strokeWidth: 0, alignment: go.Spot.Center, alignmentFocus: go.Spot.Center,
                        width: activityNodeTemplateIconWitdh, height: activityNodeTemplateIconHeight, cursor: "move"
                    },
                    new go.Binding("geometry", "icon", geoFunc),
                    new go.Binding("fill", "color")),

            ),
            $(go.TextBlock,  // the center text
                {
                    alignment: go.Spot.Bottom, textAlign: "center", verticalAlignment: go.Spot.Bottom, margin: new go.Margin(8, 0, 8, 0), font: "bold 11px Nunito", stroke: "rgba(0, 0, 0, 0.75)",
                    maxLines: 1, width: ActivityNodeTemplateWidth + 20
                },
                new go.Binding("text").makeTwoWay()),
            makeMarkerPanel(false, palscale) // sub-process,  loop, parallel, sequential, ad doc and compensation markers


        );  // End Node 
    //------------------------------------------  Event Node Template  ----------------------------------------------

    var eventNodeTemplateForPalette =
        $(go.Node, "Vertical",
            {
                locationObjectName: "SHAPEMAIN",
                selectionAdorned: false,
                //locationSpot: go.Spot.Center,
                //toolTip: tooltiptemplate,
                //minSize: new go.Size(ActivityNodeTemplateWidth-20, ActivityNodeTemplateHeight-20),
                desiredSize: new go.Size(ActivityNodeTemplateWidth + 20, ActivityNodeTemplateHeight + 20),
                cursor: "move"
            },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            // move a selected part into the Foreground layer, so it isn't obscured by any non-selected parts

            // can be resided according to the user's desires
            { resizable: false, resizeObjectName: "SHAPEMAIN" },
            $(go.Panel, "Spot",
                {
                    name: "PANEL",
                    desiredSize: new go.Size(ActivityNodeTemplateWidth / palscale, ActivityNodeTemplateHeight / palscale)
                },
                $(go.Shape, "Ellipse",  // Outer circle
                    {

                        name: "SHAPE",
                        fill: ActivityNodeFill, stroke: ActivityNodeStroke,
                        parameter1: 10 / palscale
                    }),
                $(go.Shape,
                    {
                        margin: 10, fill: "rgba(0, 0, 0, 0.5)", strokeWidth: 0, alignment: go.Spot.Center, alignmentFocus: go.Spot.Center,
                        width: nodeTemplateIconWitdh, height: nodeTemplateIconHeight, cursor: "move"
                    },
                    new go.Binding("geometry", "icon", geoFunc),
                    new go.Binding("fill", "color"))

            ),
            $(go.TextBlock,
                {
                    margin: 8, editable: true, font: "bold 11px Nunito", maxLines: 1, wrap: go.TextBlock.WrapDesiredSize, cursor: "move",
                    stroke: "rgba(0, 0, 0, 0.75)",
                },
                new go.Binding("text"))


        ); // end go.Node Vertical

    var eventNodeTemplate =
        $(go.Node, "Vertical",
            {
                locationObjectName: "SHAPEMAIN",
                name: "SHAPEMAIN",
                toolTip: tooltiptemplate,
                locationObjectName: "MAINPANEL",
                locationSpot: go.Spot.TopLeft,
                selectionAdorned: true,  // use a Binding on the Shape.stroke to show selection 
                selectionObjectName: "MAINPANEL",
                isShadowed: true,
                shadowOffset: new go.Point(10, 10),
                shadowBlur: 10,
                shadowColor: "rgba(0, 0, 0, 0.10)",
                selectionAdornmentTemplate:
                    $(go.Adornment, "Spot",
                        $(go.Panel, "Auto",
                            // this Adornment has a rectangular blue Shape around the selected node
                            $(go.Shape, "Ellipse", {
                                fill: null, stroke: "dodgerblue", strokeWidth: 1,
                                desiredSize: new go.Size(EventNodeSize + 20, EventNodeSize + 20),
                            }),
                            $(go.Placeholder)
                        )),
                //minSize: new go.Size(ActivityNodeWidth, ActivityNodeHeight),
                desiredSize: new go.Size(ActivityNodeTemplateWidth + 200, ActivityNodeTemplateHeight + 200),
                click: mouseEnterForEvent,
                //mouseEnter: mouseEnterForEvent,
                //mouseLeave: mouseLeave, 
            },

            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            new go.Binding("click", "isSelected", function (sel) {
                if (sel) return mouseEnterForEvent; else return mouseLeave;
            }).ofObject(""),
            // move a selected part into the Foreground layer, so it isn't obscured by any non-selected parts
            //new go.Binding("layerName", "isSelected", function (s) { return s ? "Foreground" : ""; }).ofObject(),
            // can be resided according to the user's desires
            { resizable: false, resizeObjectName: "SHAPEMAIN" },
            $(go.Panel, "Spot",
                {
                    name: "PANEL",
                },
                $(go.Shape, "Ellipse",  // Outer circle
                    {
                        name: "MAINPANEL",
                        strokeWidth: 1,
                        cursor: "move",
                        margin: new go.Margin(20, 10, 0, 0),
                        ////alignment: new go.Spot(0.5, 1),
                        alignment: go.Spot.Center,
                        // alignmentFocus: go.Spot.Top,
                        desiredSize: new go.Size(EventNodeSize + 20, EventNodeSize + 20),
                        fill: ActivityNodeFill, stroke: ActivityNodeStroke,
                        parameter1: 10,
                    },
                    //new go.Binding("stroke", "valid", function (valid) {
                    //    if (valid || valid == null) return ActivityNodeStroke; else return "red";
                    //}),

                ),
                $(go.Shape,
                    {
                        fill: "rgba(0, 0, 0, 0.5)", strokeWidth: 0, width: 18, height: 18, cursor: "move"
                    },
                    new go.Binding("geometry", "icon", geoFunc),
                    new go.Binding("fill", "color")),
                $(go.Shape, "Ellipse", {
                    portId: "OUT", cursor: "url(images/primeapps/bpm_cursor_arrow.cur),auto", strokeWidth: 0, desiredSize: new go.Size(10, 10),
                    alignment: go.Spot.Right, fill: "#707070", fromSpot: go.Spot.RightSide,
                },
                    new go.Binding("fromMaxLinks", "fromMaxLink"),
                    new go.Binding("fromLinkable", "fromLink"),
                    new go.Binding("visible", "fromLink")),
                $(go.Shape, "Ellipse", {
                    portId: "IN", cursor: "url(images/primeapps/bpm_cursor_arrow.cur),auto", strokeWidth: 0, desiredSize: new go.Size(10, 10),
                    alignment: go.Spot.Left, fill: "#707070", toSpot: go.Spot.LeftSide,
                },
                    new go.Binding("toMaxLinks", "toMaxLink"),
                    new go.Binding("toLinkable", "toLink"),
                    new go.Binding("visible", "toLink"))
            ),

            $(go.TextBlock,
                {
                    name: "myTextBlock", alignment: go.Spot.Bottom, textAlign: "center", textAlign: "center", margin: new go.Margin(5, 10, 5, 0),
                    stroke: "rgba(0, 0, 0, 0.5)", editable: true, font: "bold 12px Nunito", wrap: go.TextBlock.WrapFit, overflow: go.TextBlock.OverflowEllipsis,
                    maxLines: 3, width: ActivityNodeTemplateWidth + 40, cursor: "move"
                },
                new go.Binding("text").makeTwoWay(),
            )

        ); // end go.Node Vertical

    //------------------------------------------  Gateway Node Template   ----------------------------------------------

    function nodeGatewaySymbolTypeConverter(s) {
        var tasks = ["NotAllowed",
            "ThinCross",      // 1 - Parallel
            "Circle",         // 2 - Inclusive
            "AsteriskLine",   // 3 - Complex
            "ThinX",          // 4 - Exclusive  (exclusive can also be no symbol, just bind to visible=false for no symbol)
            "Pentagon",       // 5 - double cicle event based gateway
            "Pentagon",       // 6 - exclusive event gateway to start a process (single circle)
            "ThickCross"]     // 7 - parallel event gateway to start a process (single circle)
        if (s < tasks.length) return tasks[s];
        return "NotAllowed"; // error
    }

    // tweak the size of some of the gateway icons
    function nodeGatewaySymbolSizeConverter(s) {
        var size = new go.Size(GatewayNodeSymbolSize, GatewayNodeSymbolSize);
        if (s === 4) {
            size.width = size.width / 4 * 3;
            size.height = size.height / 4 * 3;
        }
        else if (s > 4) {
            size.width = size.width / 1.6;
            size.height = size.height / 1.6;
        }
        return size;
    }

    function nodePalGatewaySymbolSizeConverter(s) {
        var size = nodeGatewaySymbolSizeConverter(s);
        size.width = size.width / 2;
        size.height = size.height / 2;
        return size;
    }

    var gatewayNodeTemplate =
        $(go.Node, "Vertical",
            {
                locationObjectName: "SHAPE",
                locationSpot: go.Spot.Center,
                toolTip: tooltiptemplate
            },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            // move a selected part into the Foreground layer, so it isn't obscured by any non-selected parts
            new go.Binding("layerName", "isSelected", function (s) {
                return s ? "Foreground" : "";
            }).ofObject(),
            // can be resided according to the user's desires
            { resizable: false, resizeObjectName: "SHAPE" },
            $(go.Panel, "Spot",
                $(go.Shape, "Diamond",
                    {
                        strokeWidth: 1, fill: GatewayNodeFill, stroke: GatewayNodeStroke,
                        name: "SHAPE",
                        desiredSize: new go.Size(GatewayNodeSize, GatewayNodeSize),
                        portId: "", fromLinkable: true, toLinkable: true, cursor: "pointer",
                        fromSpot: go.Spot.NotLeftSide, toSpot: go.Spot.MiddleLeft
                    },
                    new go.Binding("desiredSize", "size", go.Size.parse).makeTwoWay(go.Size.stringify)),  // end main shape
                $(go.Shape, "NotAllowed",
                    { alignment: go.Spot.Center, stroke: GatewayNodeSymbolStroke, fill: GatewayNodeSymbolFill },
                    new go.Binding("figure", "gatewayType", nodeGatewaySymbolTypeConverter),
                    //new go.Binding("visible", "gatewayType", function(s) { return s !== 4; }),   // comment out if you want exclusive gateway to be X instead of blank.
                    new go.Binding("strokeWidth", "gatewayType", function (s) {
                        return (s <= 4) ? GatewayNodeSymbolStrokeWidth : 1;
                    }),
                    new go.Binding("desiredSize", "gatewayType", nodeGatewaySymbolSizeConverter)),
                // the next 2 circles only show up for event gateway
                $(go.Shape, "Circle",  // Outer circle
                    {
                        strokeWidth: 1, stroke: GatewayNodeSymbolStroke, fill: null, desiredSize: new go.Size(EventNodeSize, EventNodeSize)
                    },
                    new go.Binding("visible", "gatewayType", function (s) {
                        return s >= 5;
                    }) // only visible for > 5
                ),  // end main shape
                $(go.Shape, "Circle",  // Inner circle
                    {
                        alignment: go.Spot.Center, stroke: GatewayNodeSymbolStroke,
                        desiredSize: new go.Size(EventNodeInnerSize, EventNodeInnerSize),
                        fill: null
                    },
                    new go.Binding("visible", "gatewayType", function (s) {
                        return s === 5;
                    }) // inner  only visible for == 5
                )
            ),
            $(go.TextBlock,
                { alignment: go.Spot.Center, textAlign: "center", margin: 5, editable: true },
                new go.Binding("text").makeTwoWay())
        ); // end go.Node Vertical

    //--------------------------------------------------------------------------------------------------------------

    var gatewayNodeTemplateForPalette =
        $(go.Node, "Vertical",
            {
                toolTip: tooltiptemplate,
                resizable: false,
                locationObjectName: "SHAPE",
                locationSpot: go.Spot.Center,
                resizeObjectName: "SHAPE"
            },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            $(go.Panel, "Spot",
                $(go.Shape, "Diamond",
                    {
                        strokeWidth: 1, fill: GatewayNodeFill, stroke: GatewayNodeStroke, name: "SHAPE",
                        desiredSize: new go.Size(GatewayNodeSize / 2, GatewayNodeSize / 2)
                    }),
                $(go.Shape, "NotAllowed",
                    { alignment: go.Spot.Center, stroke: GatewayNodeSymbolStroke, strokeWidth: GatewayNodeSymbolStrokeWidth, fill: GatewayNodeSymbolFill },
                    new go.Binding("figure", "gatewayType", nodeGatewaySymbolTypeConverter),
                    //new go.Binding("visible", "gatewayType", function(s) { return s !== 4; }),   // comment out if you want exclusive gateway to be X instead of blank.
                    new go.Binding("strokeWidth", "gatewayType", function (s) {
                        return (s <= 4) ? GatewayNodeSymbolStrokeWidth : 1;
                    }),
                    new go.Binding("desiredSize", "gatewayType", nodePalGatewaySymbolSizeConverter)),
                // the next 2 circles only show up for event gateway
                $(go.Shape, "Circle",  // Outer circle
                    {
                        strokeWidth: 1, stroke: GatewayNodeSymbolStroke, fill: null, desiredSize: new go.Size(EventNodeSize / 2, EventNodeSize / 2)
                    },
                    //new go.Binding("desiredSize", "gatewayType", new go.Size(EventNodeSize/2, EventNodeSize/2)),
                    new go.Binding("visible", "gatewayType", function (s) {
                        return s >= 5;
                    }) // only visible for > 5
                ),  // end main shape
                $(go.Shape, "Circle",  // Inner circle
                    {
                        alignment: go.Spot.Center, stroke: GatewayNodeSymbolStroke,
                        desiredSize: new go.Size(EventNodeInnerSize / 2, EventNodeInnerSize / 2),
                        fill: null
                    },
                    new go.Binding("visible", "gatewayType", function (s) {
                        return s === 5;
                    }) // inner  only visible for == 5
                )),

            $(go.TextBlock,
                { alignment: go.Spot.Center, textAlign: "center", margin: 5, editable: false },
                new go.Binding("text"))
        );

    //--------------------------------------------------------------------------------------------------------------

    var annotationNodeTemplate =
        $(go.Node, "Auto",
            { background: GradientLightGray, locationSpot: go.Spot.Center },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            $(go.Shape, "Annotation", // A left bracket shape
                { portId: "", fromLinkable: true, cursor: "pointer", fromSpot: go.Spot.Left, strokeWidth: 2, stroke: "gray" }),
            $(go.TextBlock,
                { margin: 5, editable: true },
                new go.Binding("text").makeTwoWay())
        );

    var dataObjectNodeTemplate =
        $(go.Node, "Vertical",
            { locationObjectName: "SHAPE", locationSpot: go.Spot.Center },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            $(go.Shape, "File",
                {
                    name: "SHAPE", portId: "", fromLinkable: true, toLinkable: true, cursor: "pointer",
                    fill: DataFill, desiredSize: new go.Size(EventNodeSize * 0.8, EventNodeSize)
                }),
            $(go.TextBlock,
                {
                    margin: 5,
                    editable: true
                },
                new go.Binding("text").makeTwoWay())
        );

    var dataStoreNodeTemplate =
        $(go.Node, "Vertical",
            { locationObjectName: "SHAPE", locationSpot: go.Spot.Center },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            $(go.Shape, "Database",
                {
                    name: "SHAPE", portId: "", fromLinkable: true, toLinkable: true, cursor: "pointer",
                    fill: DataFill, desiredSize: new go.Size(EventNodeSize, EventNodeSize)
                }),
            $(go.TextBlock,
                { margin: 5, editable: true },
                new go.Binding("text").makeTwoWay())
        );

    //------------------------------------------  private process Node Template Map   ----------------------------------------------

    var privateProcessNodeTemplate =
        $(go.Node, "Auto",
            { layerName: "Background", resizable: true, resizeObjectName: "LANE" },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            $(go.Shape, "Rectangle",
                { fill: null }),
            $(go.Panel, "Table",     // table with 2 cells to hold header and lane
                {
                    desiredSize: new go.Size(ActivityNodeWidth * 6, ActivityNodeHeight),
                    background: DataFill, name: "LANE", minSize: new go.Size(ActivityNodeWidth, ActivityNodeHeight * 0.667)
                },
                new go.Binding("desiredSize", "size", go.Size.parse).makeTwoWay(go.Size.stringify),
                $(go.TextBlock,
                    {
                        row: 0, column: 0,
                        angle: 270, margin: 5,
                        editable: true, textAlign: "center"
                    },
                    new go.Binding("text").makeTwoWay()),
                $(go.RowColumnDefinition, { column: 1, separatorStrokeWidth: 1, separatorStroke: "black" }),
                $(go.Shape, "Rectangle",
                    {
                        row: 0, column: 1,
                        stroke: null, fill: "transparent",
                        portId: "", fromLinkable: true, toLinkable: true,
                        fromSpot: go.Spot.TopBottomSides, toSpot: go.Spot.TopBottomSides,
                        cursor: "pointer", stretch: go.GraphObject.Fill
                    })
            )
        );

    var privateProcessNodeTemplateForPalette =
        $(go.Node, "Vertical",
            { locationSpot: go.Spot.Center },
            $(go.Shape, "Process",
                { fill: DataFill, desiredSize: new go.Size(GatewayNodeSize / 2, GatewayNodeSize / 4) }),
            $(go.TextBlock,
                { margin: 5, editable: true },
                new go.Binding("text"))
        );

    var poolTemplateForPalette =
        $(go.Group, "Vertical",
            {
                locationSpot: go.Spot.Center,
                computesBoundsIncludingLinks: false,
                isSubGraphExpanded: false
            },
            $(go.Shape, "Process",
                { fill: "white", desiredSize: new go.Size(GatewayNodeSize / 2, GatewayNodeSize / 4) }),
            $(go.Shape, "Process",
                { fill: "white", desiredSize: new go.Size(GatewayNodeSize / 2, GatewayNodeSize / 4) }),
            $(go.TextBlock,
                { margin: 5, editable: true },
                new go.Binding("text"))
        );

    var swimLanesGroupTemplateForPalette =
        $(go.Group, "Vertical"); // empty in the palette

    var subProcessGroupTemplate =
        $(go.Group, "Spot",
            {
                locationSpot: go.Spot.Center,
                locationObjectName: "PH",
                //locationSpot: go.Spot.Center,
                isSubGraphExpanded: false,
                memberValidation: function (group, part) {
                    return !(part instanceof go.Group) ||
                        (part.category !== "Pool" && part.category !== "Lane");
                },
                mouseDrop: function (e, grp) {
                    var ok = grp.addMembers(grp.diagram.selection, true);
                    if (!ok) grp.diagram.currentTool.doCancel();
                },
                contextMenu: activityNodeMenu,
                itemTemplate: boundaryEventItemTemplate
            },
            new go.Binding("itemArray", "boundaryEventArray"),
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            // move a selected part into the Foreground layer, so it isn't obscured by any non-selected parts
            // new go.Binding("layerName", "isSelected", function (s) { return s ? "Foreground" : ""; }).ofObject(),
            $(go.Panel, "Auto",
                $(go.Shape, "RoundedRectangle",
                    {
                        name: "PH", fill: SubprocessNodeFill, stroke: SubprocessNodeStroke,
                        minSize: new go.Size(ActivityNodeWidth, ActivityNodeHeight),
                        portId: "", fromLinkable: true, toLinkable: true, cursor: "pointer",
                        fromSpot: go.Spot.RightSide, toSpot: go.Spot.LeftSide
                    },
                    new go.Binding("strokeWidth", "isCall", function (s) {
                        return s ? ActivityNodeStrokeWidthIsCall : ActivityNodeStrokeWidth;
                    })
                ),
                $(go.Panel, "Vertical",
                    { defaultAlignment: go.Spot.Left },
                    $(go.TextBlock,  // label
                        { margin: 3, editable: true },
                        new go.Binding("text", "text").makeTwoWay(),
                        new go.Binding("alignment", "isSubGraphExpanded", function (s) {
                            return s ? go.Spot.TopLeft : go.Spot.Center;
                        })),
                    // create a placeholder to represent the area where the contents of the group are
                    $(go.Placeholder,
                        { padding: new go.Margin(5, 5) }),
                    makeMarkerPanel(true, 1)  // sub-process,  loop, parallel, sequential, ad doc and compensation markers
                )  // end Vertical Panel
            )
        );  // end Group

    //** need this in the subprocess group template above.
    //        $(go.Shape, "RoundedRectangle",  // the inner "Transaction" rounded rectangle
    //          { margin: 3,
    //            stretch: go.GraphObject.Fill,
    //            stroke: ActivityNodeStroke,
    //            parameter1: 8, fill: null, visible: false
    //          },
    //          new go.Binding("visible", "isTransaction")
    //         ),


    function groupStyle() {  // common settings for both Lane and Pool Groups
        return [
            {
                layerName: "Background",  // all pools and lanes are always behind all nodes and links
                background: "transparent",  // can grab anywhere in bounds
                movable: true, // allows users to re-order by dragging
                copyable: false,  // can't copy lanes or pools
                avoidable: false  // don't impede AvoidsNodes routed Links
            },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify)
        ];
    }

    // hide links between lanes when either lane is collapsed
    function updateCrossLaneLinks(group) {
        group.findExternalLinksConnected().each(function (l) {
            l.visible = (l.fromNode.isVisible() && l.toNode.isVisible());
        });
    }

    var laneEventMenu =  // context menu for each lane
        $(go.Adornment, "Vertical",
            $("ContextMenuButton",
                $(go.TextBlock, "Add Lane"),
                // in the click event handler, the obj.part is the Adornment; its adornedObject is the port
                {
                    click: function (e, obj) {
                        addLaneEvent(obj.part.adornedObject);
                    }
                })
        );

    // Add a lane to pool (lane parameter is lane above new lane)
    function addLaneEvent(lane) {
        window.myDiagram.startTransaction("addLane");
        if (lane != null && lane.data.category === "Lane") {
            // create a new lane data object
            var shape = lane.findObject("SHAPE");
            var size = new go.Size(shape.width, MINBREADTH);
            //size.height = MINBREADTH;
            var newlanedata = {
                category: "Lane",
                text: "New Lane",
                color: "white",
                isGroup: true,
                loc: go.Point.stringify(new go.Point(lane.location.x, lane.location.y + 1)), // place below selection
                size: go.Size.stringify(size),
                group: lane.data.group
            };
            // and add it to the model
            window.myDiagram.model.addNodeData(newlanedata);
        }
        window.myDiagram.commitTransaction("addLane");
    }

    var swimLanesGroupTemplate =
        $(go.Group, "Spot", groupStyle(),
            {
                name: "Lane",
                contextMenu: laneEventMenu,
                minLocation: new go.Point(NaN, -Infinity),  // only allow vertical movement
                maxLocation: new go.Point(NaN, Infinity),
                selectionObjectName: "SHAPE",  // selecting a lane causes the body of the lane to be highlit, not the label
                resizable: true, resizeObjectName: "SHAPE",  // the custom resizeAdornmentTemplate only permits two kinds of resizing
                layout: $(go.LayeredDigraphLayout,  // automatically lay out the lane's subgraph
                    {
                        isInitial: false,  // don't even do initial layout
                        isOngoing: false,  // don't invalidate layout when nodes or links are added or removed
                        direction: 0,
                        columnSpacing: 10,
                        layeringOption: go.LayeredDigraphLayout.LayerLongestPathSource
                    }),
                computesBoundsAfterDrag: true,  // needed to prevent recomputing Group.placeholder bounds too soon
                computesBoundsIncludingLinks: false,  // to reduce occurrences of links going briefly outside the lane
                computesBoundsIncludingLocation: true,  // to support empty space at top-left corner of lane
                handlesDragDropForMembers: true,  // don't need to define handlers on member Nodes and Links
                mouseDrop: function (e, grp) {  // dropping a copy of some Nodes and Links onto this Group adds them to this Group
                    // don't allow drag-and-dropping a mix of regular Nodes and Groups
                    if (!e.diagram.selection.any(function (n) {
                        return (n instanceof go.Group && n.category !== "subprocess") || n.category === "privateProcess";
                    })) {
                        var ok = grp.addMembers(grp.diagram.selection, true);
                        if (ok) {
                            updateCrossLaneLinks(grp);
                            relayoutDiagram();
                        } else {
                            grp.diagram.currentTool.doCancel();
                        }
                    }
                },
                subGraphExpandedChanged: function (grp) {
                    var shp = grp.resizeObject;
                    if (grp.diagram.undoManager.isUndoingRedoing) return;
                    if (grp.isSubGraphExpanded) {
                        shp.height = grp._savedBreadth;
                    } else {
                        grp._savedBreadth = shp.height;
                        shp.height = NaN;
                    }
                    updateCrossLaneLinks(grp);
                }
            },
            //new go.Binding("isSubGraphExpanded", "expanded").makeTwoWay(),

            $(go.Shape, "Rectangle",  // this is the resized object
                { name: "SHAPE", fill: "white", stroke: null },  // need stroke null here or you gray out some of pool border.
                new go.Binding("fill", "color"),
                new go.Binding("desiredSize", "size", go.Size.parse).makeTwoWay(go.Size.stringify)),

            // the lane header consisting of a Shape and a TextBlock
            $(go.Panel, "Horizontal",
                {
                    name: "HEADER",
                    angle: 270,  // maybe rotate the header to read sideways going up
                    alignment: go.Spot.LeftCenter, alignmentFocus: go.Spot.LeftCenter
                },
                $(go.TextBlock,  // the lane label
                    { editable: true, margin: new go.Margin(2, 0, 0, 8) },
                    new go.Binding("visible", "isSubGraphExpanded").ofObject(),
                    new go.Binding("text", "text").makeTwoWay()),
                $("SubGraphExpanderButton", { margin: 4, angle: -270 })  // but this remains always visible!
            ),  // end Horizontal Panel
            $(go.Placeholder,
                { padding: 12, alignment: go.Spot.TopLeft, alignmentFocus: go.Spot.TopLeft }),
            $(go.Panel, "Horizontal", { alignment: go.Spot.TopLeft, alignmentFocus: go.Spot.TopLeft },
                $(go.TextBlock,  // this TextBlock is only seen when the swimlane is collapsed
                    {
                        name: "LABEL",
                        editable: true, visible: false,
                        angle: 0, margin: new go.Margin(6, 0, 0, 20)
                    },
                    new go.Binding("visible", "isSubGraphExpanded", function (e) {
                        return !e;
                    }).ofObject(),
                    new go.Binding("text", "text").makeTwoWay())
            )
        );  // end swimLanesGroupTemplate

    // define a custom resize adornment that has two resize handles if the group is expanded
    //myDiagram.groupTemplate.resizeAdornmentTemplate =
    swimLanesGroupTemplate.resizeAdornmentTemplate =
        $(go.Adornment, "Spot",
            $(go.Placeholder),
            $(go.Shape,  // for changing the length of a lane
                {
                    alignment: go.Spot.Right,
                    desiredSize: new go.Size(7, 50),
                    fill: "lightblue", stroke: "dodgerblue",
                    cursor: "col-resize"
                },
                new go.Binding("visible", "", function (ad) {
                    if (ad.adornedPart === null) return false;
                    return ad.adornedPart.isSubGraphExpanded;
                }).ofObject()),
            $(go.Shape,  // for changing the breadth of a lane
                {
                    alignment: go.Spot.Bottom,
                    desiredSize: new go.Size(50, 7),
                    fill: "lightblue", stroke: "dodgerblue",
                    cursor: "row-resize"
                },
                new go.Binding("visible", "", function (ad) {
                    if (ad.adornedPart === null) return false;
                    return ad.adornedPart.isSubGraphExpanded;
                }).ofObject())
        );

    var poolGroupTemplate =
        $(go.Group, "Auto", groupStyle(),
            {
                computesBoundsIncludingLinks: false,
                // use a simple layout that ignores links to stack the "lane" Groups on top of each other
                layout: $(PoolLayout, { spacing: new go.Size(0, 0) })  // no space between lanes
            },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            $(go.Shape,
                { fill: "white" },
                new go.Binding("fill", "color")),
            $(go.Panel, "Table",
                { defaultColumnSeparatorStroke: "black" },
                $(go.Panel, "Horizontal",
                    { column: 0, angle: 270 },
                    $(go.TextBlock,
                        { editable: true, margin: new go.Margin(5, 0, 5, 0) },  // margin matches private process (black box pool)
                        new go.Binding("text").makeTwoWay())
                ),
                $(go.Placeholder,
                    { background: "darkgray", column: 1 })
            )
        ); // end poolGroupTemplate

    //------------------------------------------  Template Maps  ----------------------------------------------

    // create the nodeTemplateMap, holding main view node templates:
    var nodeTemplateMap = new go.Map("string", go.Node);
    // for each of the node categories, specify which template to use
    nodeTemplateMap.add("activity", activityNodeTemplate);
    nodeTemplateMap.add("event", eventNodeTemplate);
    nodeTemplateMap.add("gateway", gatewayNodeTemplate);
    nodeTemplateMap.add("annotation", annotationNodeTemplate);
    nodeTemplateMap.add("dataobject", dataObjectNodeTemplate);
    nodeTemplateMap.add("datastore", dataStoreNodeTemplate);
    nodeTemplateMap.add("privateProcess", privateProcessNodeTemplate);
    // for the default category, "", use the same template that Diagrams use by default
    // this just shows the key value as a simple TextBlock

    var groupTemplateMap = new go.Map("string", go.Group);
    //groupTemplateMap.add("subprocess", subProcessGroupTemplate);
    //groupTemplateMap.add("Lane", swimLanesGroupTemplate);
    //groupTemplateMap.add("Pool", poolGroupTemplate);

    // create the nodeTemplateMap, holding special palette "mini" node templates:
    var palNodeTemplateMap = new go.Map("string", go.Node);
    palNodeTemplateMap.add("activity", activityNodeTemplateForPalette);
    palNodeTemplateMap.add("event", eventNodeTemplateForPalette);
    //palNodeTemplateMap.add("gateway", gatewayNodeTemplateForPalette);
    //palNodeTemplateMap.add("annotation", annotationNodeTemplate);
    //palNodeTemplateMap.add("dataobject", dataObjectNodeTemplate);
    //palNodeTemplateMap.add("datastore", dataStoreNodeTemplate);
    //palNodeTemplateMap.add("privateProcess", privateProcessNodeTemplateForPalette);

    var palGroupTemplateMap = new go.Map("string", go.Group);
    //palGroupTemplateMap.add("subprocess", subProcessGroupTemplateForPalette);
    //palGroupTemplateMap.add("Pool", poolTemplateForPalette);
    //palGroupTemplateMap.add("Lane", swimLanesGroupTemplateForPalette);


    //------------------------------------------  Link Templates   ----------------------------------------------

    var sequenceLinkTemplate =
        $(go.Link,
            {
                //contextMenu:
                //    $(go.Adornment, "Vertical",
                //        $("ContextMenuButton",
                //            $(go.TextBlock, "Default Flow"),
                //            // in the click event handler, the obj.part is the Adornment; its adornedObject is the port
                //            {
                //                click: function (e, obj) {
                //                    setSequenceLinkDefaultFlow(obj.part.adornedObject);
                //                }
                //            }),
                //        $("ContextMenuButton",
                //            $(go.TextBlock, "Conditional Flow"),
                //            // in the click event handler, the obj.part is the Adornment; its adornedObject is the port
                //            {
                //                click: function (e, obj) {
                //                    setSequenceLinkConditionalFlow(obj.part.adornedObject);
                //                }
                //            })
                //), 
                name: "PANEL",
                fromPortId: "OUT",
                toPortId: "IN",
                routing: go.Link.AvoidsNodes, curve: go.Link.JumpOver, corner: 10,
                //fromSpot: go.Spot.RightSide, toSpot: go.Spot.LeftSide,
                reshapable: false, relinkableFrom: false, relinkableTo: false, toEndSegmentLength: 20,
                adjusting: go.Link.Stretch,
                click: mouseEnter,
                actionCancel: mouseLeave,
            },
            new go.Binding("points").makeTwoWay(),
            $(go.Shape, { stroke: "#707070", strokeWidth: 1 }),
            $(go.Shape, { toArrow: "Triangle", scale: 1.2, fill: "#707070", stroke: null },
                //$(go.Shape, { fromArrow: "Circle", scale: 0.5, stroke: "null", fill: "#707070" },
                new go.Binding("fromArrow", "isDefault", function (s) {
                    if (s === null) return "";
                    return s ? "BackSlash" : "StretchedDiamond";
                }),
                new go.Binding("segmentOffset", "isDefault", function (s) {
                    return s ? new go.Point(5, 0) : new go.Point(0, 0);
                })),
            $(go.TextBlock, { // this is a Link label
                name: "Label", editable: true, text: "label", segmentOffset: new go.Point(-10, -10), visible: false
            },
                new go.Binding("text", "text").makeTwoWay(),
                new go.Binding("visible", "visible").makeTwoWay())
        );

    // set Default Sequence Flow (backslash From Arrow)
    function setSequenceLinkDefaultFlow(obj) {
        window.myDiagram.startTransaction("setSequenceLinkDefaultFlow");
        var model = window.myDiagram.model;
        model.setDataProperty(obj.data, "isDefault", true);
        // Set all other links from the fromNode to be isDefault=null
        obj.fromNode.findLinksOutOf().each(function (link) {
            if (link !== obj && link.data.isDefault) {
                model.setDataProperty(link.data, "isDefault", null);
            }
        });
        window.myDiagram.commitTransaction("setSequenceLinkDefaultFlow");
    }

    // set Conditional Sequence Flow (diamond From Arrow)
    function setSequenceLinkConditionalFlow(obj) {
        window.myDiagram.startTransaction("setSequenceLinkConditionalFlow");
        var model = window.myDiagram.model;
        model.setDataProperty(obj.data, "isDefault", false);
        window.myDiagram.commitTransaction("setSequenceLinkConditionalFlow");
    }

    var messageFlowLinkTemplate =
        $(PoolLink, // defined in BPMNClasses.js
            {
                routing: go.Link.Orthogonal, curve: go.Link.JumpGap, corner: 10,
                fromSpot: go.Spot.TopBottomSides, toSpot: go.Spot.TopBottomSides,
                reshapable: true, relinkableTo: true, toEndSegmentLength: 20
            },
            new go.Binding("points").makeTwoWay(),
            $(go.Shape, { stroke: "black", strokeWidth: 1, strokeDashArray: [6, 2] }),
            $(go.Shape, { toArrow: "Triangle", scale: 1, fill: "white", stroke: "black" }),
            $(go.Shape, { fromArrow: "Circle", scale: 1, visible: true, stroke: "black", fill: "white" }),
            $(go.TextBlock, {
                editable: true, text: "label"
            }, // Link label
                new go.Binding("text", "text").makeTwoWay())
        );

    var dataAssociationLinkTemplate =
        $(go.Link,
            {
                routing: go.Link.AvoidsNodes, curve: go.Link.JumpGap, corner: 10,
                fromSpot: go.Spot.AllSides, toSpot: go.Spot.AllSides,
                reshapable: true, relinkableFrom: true, relinkableTo: true
            },
            new go.Binding("points").makeTwoWay(),
            $(go.Shape, { stroke: "black", strokeWidth: 1, strokeDashArray: [1, 3] }),
            $(go.Shape, { toArrow: "OpenTriangle", scale: 1, fill: null, stroke: "blue" })
        );

    var annotationAssociationLinkTemplate =
        $(go.Link,
            {
                reshapable: true, relinkableFrom: true, relinkableTo: true,
                toSpot: go.Spot.AllSides,
                toEndSegmentLength: 20, fromEndSegmentLength: 40
            },
            new go.Binding("points").makeTwoWay(),
            $(go.Shape, { stroke: "black", strokeWidth: 1, strokeDashArray: [1, 3] }),
            $(go.Shape, { toArrow: "OpenTriangle", scale: 1, stroke: "black" })
        );

    var linkTemplateMap = new go.Map("string", go.Link);
    linkTemplateMap.add("msg", messageFlowLinkTemplate);
    linkTemplateMap.add("annotation", annotationAssociationLinkTemplate);
    linkTemplateMap.add("data", dataAssociationLinkTemplate);
    linkTemplateMap.add("", sequenceLinkTemplate);  // default


    //------------------------------------------the main Diagram----------------------------------------------

    window.myDiagram =
        $(go.Diagram, "myDiagramDiv",
            {
                nodeTemplateMap: nodeTemplateMap,
                linkTemplateMap: linkTemplateMap,
                groupTemplateMap: groupTemplateMap,

                allowDrop: true,  // accept drops from palette

                commandHandler: new DrawCommandHandler(),  // defined in DrawCommandHandler.js
                // default to having arrow keys move selected nodes
                "commandHandler.arrowKeyBehavior": "move",

                mouseDrop: function (e) {
                    // when the selection is dropped in the diagram's background,
                    // make sure the selected Parts no longer belong to any Group
                    var ok = window.myDiagram.commandHandler.addTopLevelParts(window.myDiagram.selection, true);
                    //window.myDiagram.clearSelection();
                    //var scope = angular.element(document.getElementById("WorkflowEditorController")).scope();
                    //scope.currentObj = e;
                    //scope.toogleSideMenu(true);

                    if (!ok) window.myDiagram.currentTool.doCancel();

                },
                linkingTool: new BPMNLinkingTool(), // defined in BPMNClasses.js
                "SelectionMoved": relayoutDiagram,  // defined below
                "SelectionCopied": relayoutDiagram
            }
        );
    window.myDiagram.grid = $(go.Panel, "Grid",
        {
            name: "GRID",
            visible: false,
            gridCellSize: new go.Size(20, 20),
            gridOrigin: new go.Point(0, 0)
        },
        $(go.Shape, "LineH", { stroke: "rgba(0, 0, 0, 0.10)", strokeWidth: 0.5, interval: 1 }),
        $(go.Shape, "LineH", { stroke: "rgba(0, 0, 0, 0.15)", strokeWidth: 0.5, interval: 5 }),
        $(go.Shape, "LineH", { stroke: "rgba(0, 0, 0, 0.15)", strokeWidth: 1.0, interval: 10 }),
        $(go.Shape, "LineV", { stroke: "rgba(0, 0, 0, 0.10)", strokeWidth: 0.5, interval: 1 }),
        $(go.Shape, "LineV", { stroke: "rgba(0, 0, 0, 0.15)", strokeWidth: 0.5, interval: 5 }),
        $(go.Shape, "LineV", { stroke: "rgba(0, 0, 0, 0.15)", strokeWidth: 1.0, interval: 10 })
    );


    //Object Double Mouse Click
    window.myDiagram.addDiagramListener("ObjectDoubleClicked", function (e, obj) {
        if (e.Pw.Sb === "Delete") {
            return;
        }

        var scope = angular.element(document.getElementById("WorkflowEditorController")).scope();
        scope.currentObj = e;
        scope.toogleSideMenu(true);
    });

    ////Object Single Mouse Click
    //window.myDiagram.addDiagramListener("ObjectSingleClicked", function (e) {
    //    if (e.Pw.Sb === "Delete") {

    //        return;
    //    }
    //});

    //Object drop on diagram from external
    window.myDiagram.addDiagramListener("ExternalObjectsDropped", function (e) {
        window.myDiagram.clearSelection(e);

        var node = e.diagram.Fi.Th.key;

        if (!node)
            return;

        var result = eventOnceArray.filter(q => q.item == node.item);
        if (result.length <= 0)
            return;

        if (!window.myDiagram.model.nodeDataArray)
            return;

        var isThere = window.myDiagram.model.nodeDataArray.filter(q => q.item == node.item && q.key != node.key);

        if (isThere.length > 0) {
            window.myDiagram.currentTool.doCancel();
            window.myDiagram.model.removeNodeData(node);
        }
    });


    window.myDiagram.addDiagramListener("BackgroundSingleClicked", function (e) {
        if (myOldNodeData.e && myOldNodeData.obj) {
            mouseLeave(myOldNodeData.e, myOldNodeData.obj);
            myOldNodeData = {};
        }
        var scope = angular.element(document.getElementById("WorkflowEditorController")).scope();
        if (scope.currentObj)
            scope.toogleSideMenu(false);

        scope.currentObj = null;
    });

    //window.myDiagram.addModelChangedListener(function (evt) {
    //    if (!evt.isTransactionFinished) return;
    //    var txn = evt.object;  // a Transaction
    //    if (txn === null) return;
    //    // iterate over all of the actual ChangedEvents of the Transaction
    //    txn.changes.each(function (e) {
    //        // ignore any kind of change other than adding/removing a node
    //        if (e.modelChange !== "nodeDataArray") return;
    //        // record node insertions and removals
    //        if (e.change === go.ChangedEvent.Insert) {
    //            console.log(evt.propertyName + " added node with key: " + e.newValue.key);
    //        } else if (e.change === go.ChangedEvent.Remove) {
    //            console.log(evt.propertyName + " removed node with key: " + e.oldValue.key);
    //        }
    //    }); 
    //});

    //window.myDiagram.addDiagramListener("ExternalObjectsDropped", function (e) {
    //    console.log("Eleman oluşturuldu.", e);
    //    var scope = angular.element(document.getElementById("WorkflowEditorController")).scope();
    //    scope.currentObj = e;
    //    scope.toogleSideMenu(true);
    //})

    window.myDiagram.addDiagramListener("SelectionDeleted", function (e) {
        var scope = angular.element(document.getElementById("WorkflowEditorController")).scope();

        if (scope.currentObj) {
            delete scope.workflowModel[scope.currentObj.subject.part.data.ngModelName];
            scope.toogleSideMenu(false);
        }
    });

    //window.myDiagram.mouseDragOver = function (e) {
    //    window.myDiagram.clearSelection();
    //    myOldNodeData = {};
    //}

    //window.myDiagram.toolManager.mouseDownTools.insertAt(0, new LaneResizingTool());

    window.myDiagram.addDiagramListener("LinkDrawn", function (e) {
        if (e.subject.fromNode.category === "annotation") {
            e.subject.category = "annotation"; // annotation association
        } else if (e.subject.fromNode.category === "dataobject" || e.subject.toNode.category === "dataobject") {
            e.subject.category = "data"; // data association
        } else if (e.subject.fromNode.category === "datastore" || e.subject.toNode.category === "datastore") {
            e.subject.category = "data"; // data association
        }
    });

    //  uncomment this if you want a subprocess to expand on drop.  We decided we didn't like this behavior
    //  myDiagram.addDiagramListener("ExternalObjectsDropped", function(e) {
    //    // e.subject is the collection that was just dropped
    //    e.subject.each(function(part) {
    //        if (part instanceof go.Node && part.data.item === "end") {
    //          part.move(new go.Point(part.location.x  + 350, part.location.y))
    //        }
    //      });
    //    myDiagram.commandHandler.expandSubGraph();
    //  });

    // change the title to indicate that the diagram has been modified
    window.myDiagram.addDiagramListener("Modified", function (e) {
        var currentFile = document.getElementById("currentFile");
        var idx = currentFile.textContent.indexOf("*");
        if (window.myDiagram.isModified) {
            if (idx < 0) currentFile.textContent = currentFile.textContent + "*";
        } else {
            if (idx >= 0) currentFile.textContent = currentFile.textContent.substr(0, idx);
        }
    });


    //------------------------------------------  Palette   ----------------------------------------------

    // Make sure the pipes are ordered by their key in the palette inventory
    function keyCompare(a, b) {
        var at = a.data.key;
        var bt = b.data.key;
        if (at < bt) return -1;
        if (at > bt) return 1;
        return 0;
    }

    // initialize the first Palette, BPMN Spec Level 1
    window.myPaletteLevel1 =
        $(go.Palette, "myPaletteLevel1",
            { // share the templates with the main Diagram
                nodeTemplateMap: palNodeTemplateMap,
                groupTemplateMap: palGroupTemplateMap,
                layout: $(go.GridLayout,
                    {
                        wrappingColumn: 4,
                        alignment: go.GridLayout.Position,
                        // cellSize: go.Size.parse("220 2"),
                        // spacing: new go.Size(5, 5),
                        comparer: keyCompare
                    })
            });

    // initialize the second Palette, BPMN Spec Level 2
    /*window.myPaletteLevel2 =
        $(go.Palette, "myPaletteLevel2",
            { // share the templates with the main Diagram
                nodeTemplateMap: palNodeTemplateMap,
                groupTemplateMap: palGroupTemplateMap,
                layout: $(go.GridLayout,
                    {
                        cellSize: new go.Size(1, 1),
                        spacing: new go.Size(5, 5),
                        comparer: keyCompare
                    })
            });*/

    window.myPaletteLevel1.model = $(go.GraphLinksModel,
        {
            copiesArrays: true,
            copiesArrayObjects: true,
            nodeDataArray: [
                // -------------------------- Event Nodes
                { key: 101, category: "event", name: "Basic Start", text: "Basic Start", eventType: 1, eventDimension: 1, icon: "start", color: "#25A65B", item: "Start", sidebar: true, ngModelName: "start", fromLink: true, toLink: false, toMaxLink: 0, fromMaxLink: 1, valid: null }, //"Start"
                //{ key: 102, category: "event", text: "Message", eventType: 2, eventDimension: 2, item: "Message", sidebar: false }, //"Message" // BpmnTaskMessage
                //{ key: 103, category: "event", name: "Timer Start", text: "Timer Start", eventType: 1, eventDimension: 2, icon: "timer", color: "#25A65B", item: "Timer", sidebar: true, ngModelName: "timer", fromLink: true, toLink: false, toMaxLink: 0, fromMaxLink: 1, valid: null }, //"Timer"
                { key: 104, category: "event", name: "Terminate", text: "Terminate", eventType: 13, eventDimension: 8, icon: "end", color: "#DC3023", item: "End", sidebar: false, fromLink: false, toLink: true, toMaxLink: 1 }, //"End"
                //{ key: 105, category: "event", name: "Wait Signal", text: "Wait Signal", eventType: 1, eventDimension: 4, icon: "wait", color: "#F06933", item: "Wait", sidebar: true, ngModelName: "wait", fromLink: true, toLink: true, toMaxLink: 1, fromMaxLink: 1, valid: null }, //"Wait"
                //{ key: 107, category: "event", text: "Message", eventType: 2, eventDimension: 8, item: "End Message", sidebar: false },//"End Message" // BpmnTaskMessage
                //{ key: 108, category: "event", text: "Terminate", eventType: 13, eventDimension: 8, item: "Terminate", sidebar: false }, //"Terminate"
                // -------------------------- Task/Activity Nodes
                { key: 131, category: "activity", name: "Notification", text: "Notification", item: "Notification Task", icon: "notification", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "send_notification", toMaxLink: 1, fromMaxLink: 1, valid: null }, //"Notification Task"
                //{ key: 132, category: "activity", name: "Create Task", text: "Create Task", item: "User Task", icon: "task", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "create_task", toMaxLink: 1, fromMaxLink: 1, valid: null }, //"User Task"
                { key: 133, category: "activity", name: "Webhook", text: "Webhook", item: "WebHook Task", icon: "hook", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "webHook", toMaxLink: 1, fromMaxLink: 1, valid: null }, //"WebHook Task"
                //{ key: 134, category: "activity", name: "Change Access", text: "Change Access", item: "Change Access", icon: "access", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "access", toMaxLink: 1, fromMaxLink: 1, valid: null },
                { key: 135, category: "activity", name: "Data Update", text: "Data Update", item: "Data Task", icon: "update", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "field_update", toMaxLink: 1, fromMaxLink: 1, valid: null },
                //{ key: 136, category: "activity", name: "Data Read", text: "Data Read", item: "Data Read Task", icon: "read", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "data_read", toMaxLink: 1, fromMaxLink: 1, valid: null },
                //{ key: 137, category: "activity", name: "Data Add", text: "Data Add", item: "Data Add", icon: "add", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "data_add", toMaxLink: 1, fromMaxLink: 1, valid: null },
                //{ key: 138, category: "activity", name: "Data Delete", text: "Data Delete", item: "Data Delete", icon: "delete", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "data_delete", toMaxLink: 1, fromMaxLink: 1, valid: null },
                //{ key: 139, category: "activity", name: "Function", text: "Function", item: "Function Task", icon: "function", taskType: 0, sidebar: true, color: "#0D6FAA", ngModelName: "function", toMaxLink: 1, fromMaxLink: 1, valid: null },


                // subprocess and start and end
                //{ key: 134, category: "subprocess", loc: "0 0", text: "Subprocess", isGroup: true, isSubProcess: true, taskType: 0 },
                //{ key: -802, category: "event", loc: "0 0", group: 134, text: "Start", eventType: 1, eventDimension: 1, item: "Start" },
                //{ key: -803, category: "event", loc: "350 0", group: 134, text: "End", eventType: 1, eventDimension: 8, item: "End", name: "end" },
                // -------------------------- Gateway Nodes, Data, Pool and Annotation
                //{ key: 201, category: "gateway", text: "Parallel", gatewayType: 1, item: "Parallel" },
                //{ key: 204, category: "gateway", text: "Exclusive", gatewayType: 4, item: "Exclusive" },
                //{ key: 301, category: "dataobject", text: "Data\nObject", item: "Data Object" },
                //{ key: 302, category: "datastore", text: "Data\nStorage", item: "Data Storage" },
                //{ key: 401, category: "privateProcess", text: "Black Box", item: "Black Box" },
                //{ key: 501, text: "Pool 1", isGroup: true, category: "Pool", item: "Pool" },
                //{ key: "Lane5", text: "Lane 1", isGroup: true, group: 501, color: "lightyellow", category: "Lane", item: "Lane" },
                //  { key: "Lane6", text: "Lane 2", isGroup: true, group: 501, color: "lightgreen", category: "Lane", item: "Lane" },
                //{ key: 701, category: "annotation", icon: "read", text: "note", item: "Note" }
            ]  // end nodeDataArray
        });  // end model

    //For use only once can be added.
    var eventOnceArray = window.myPaletteLevel1.model.nodeDataArray.filter(q => q.category == 'event' && q.item != "Wait");

    // an activity with a boundary event:
    //        {
    //          key: 1,
    //          category: "activity",
    //          text: "Message",
    //          taskType: 1,
    //          item: "Message Task",
    //          boundaryEventArray: [{ "portId": "be0", alignmentIndex: 0, eventType: 2, color: "white" }]   // portId # and alignmentIndex should match
    //        },

    /*window.myPaletteLevel2.model = $(go.GraphLinksModel,
        {
            copiesArrays: true,
            copiesArrayObjects: true,
            nodeDataArray: [
                { key: 1, category: "activity", taskType: 1, text: "Receive Task", item: "Receive Task" },
                { key: 2, category: "activity", taskType: 5, text: "Send Task", item: "Send Task" },
                { key: 3, category: "activity", taskType: 7, text: "Business\nRule Task", item: "Business Rule Task" },
                { key: 4, category: "activity", taskType: 2, text: "User Task", item: "User Task", isCall: true },

                { key: 101, text: "Adhoc\nSubprocess", isGroup: true, isSubProcess: true, category: "subprocess", isAdHoc: true, taskType: 0, loc: "0 0" },
                { key: -812, group: 101, category: "event", text: "Start", eventType: 1, eventDimension: 1, item: "start", loc: "0 0" },
                { key: -813, group: 101, category: "event", text: "End", eventType: 1, eventDimension: 8, item: "end", name: "end" },

                { key: 102, text: "Transactional\nSubprocess", isGroup: true, isSubProcess: true, category: "subprocess", isTransaction: true, taskType: 0, loc: "0 0" },
                { key: -822, group: 102, category: "event", text: "Start", eventType: 1, eventDimension: 1, item: "start", loc: "0 0" },
                { key: -823, group: 102, category: "event", text: "End", eventType: 1, eventDimension: 8, item: "end", name: "end", loc: "350 0" },

                { key: 103, text: "Looping\nActivity", isGroup: true, isLoop: true, isSubProcess: true, category: "subprocess", taskType: 0, loc: "0 0" },
                { key: -831, group: 103, category: "event", text: "Start", eventType: 1, eventDimension: 1, item: "start", loc: "0 0" },
                { key: -832, group: 103, category: "event", text: "End", eventType: 1, eventDimension: 8, item: "end", name: "end", loc: "350 0" },

                { key: 104, text: "Multi-Instance\nActivity", isGroup: true, isSubProcess: true, isParallel: true, category: "subprocess", taskType: 0, loc: "0 0" },
                { key: -841, group: 104, category: "event", text: "Start", eventType: 1, eventDimension: 1, item: "start", loc: "0 0" },
                { key: -842, group: 104, category: "event", text: "End", eventType: 1, eventDimension: 8, item: "end", name: "end", loc: "350 0" },

                { key: 105, text: "Call\nSubprocess", isGroup: true, isSubProcess: true, category: "subprocess", isCall: true, taskType: 0, loc: "0 0" },
                { key: -861, group: 105, category: "event", text: "Start", eventType: 1, eventDimension: 1, item: "start", loc: "0 0" },
                { key: -862, group: 105, category: "event", text: "End", eventType: 1, eventDimension: 8, item: "end", name: "end", loc: "350 0" },

                // gateway nodes
                { key: 301, category: "gateway", gatewayType: 2, text: "Inclusive" },
                { key: 302, category: "gateway", gatewayType: 5, text: "Event\nGateway" },

                // events
                { key: 401, category: "event", eventType: 5, eventDimension: 1, text: "Conditional\nStart", item: "BpmnEventConditional" },
                { key: 402, category: "event", eventType: 10, eventDimension: 1, text: "Signal\nStart", item: "BpmnEventSignal" },  // start signal
                { key: 403, category: "event", eventType: 10, eventDimension: 8, text: "Signal\nEnd", item: "end signal" },
                { key: 404, category: "event", eventType: 7, eventDimension: 8, text: "Error", item: "BpmnEventError" },
                { key: 405, category: "event", eventType: 4, eventDimension: 8, text: "Escalation", item: "BpmnEventEscalation" },
                // throwing / catching intermedicate events
                { key: 502, category: "event", eventType: 6, eventDimension: 4, text: "Catch\nLink", item: "BpmnEventOffPage" },
                { key: 503, category: "event", eventType: 6, eventDimension: 7, text: "Throw\nLink", item: "BpmnEventOffPage" },
                { key: 504, category: "event", eventType: 2, eventDimension: 4, text: "Catch\nMessage", item: "Message" },
                { key: 505, category: "event", eventType: 2, eventDimension: 7, text: "Throw\nMessage", item: "Message" },
                { key: 506, category: "event", eventType: 5, eventDimension: 4, text: "Catch\nConditional", item: "" },
                { key: 507, category: "event", eventType: 3, eventDimension: 4, text: "Catch\nTimer", item: "" },
                { key: 508, category: "event", eventType: 4, eventDimension: 7, text: "Throw\nEscalation", item: "Escalation" },
                { key: 509, category: "event", eventType: 10, eventDimension: 4, text: "Catch\nSignal", item: "" },
                { key: 510, category: "event", eventType: 10, eventDimension: 7, text: "Throw\nSignal", item: "" }
            ]  // end nodeDataArray
        });  // end model*/

    //------------------------------------------  Overview   ----------------------------------------------

    var myOverview =
        $(go.Overview, "myOverviewDiv",
            { observed: window.myDiagram, maxScale: 0.5, contentAlignment: go.Spot.Center });
    // change color of viewport border in Overview
    myOverview.box.elt(0).stroke = "dodgerblue";


} // end init

var myKey;
var myDeleteActive = false;
var myOldNodeData = {};



function DeleteNode(e, obj) {
    var cmdhnd = window.myDiagram.commandHandler;
    var node = window.myDiagram.findNodeForKey(myKey);

    if (!node) {
        cmdhnd.deleteSelection(e);
        return false;
    }
    window.myDiagram.remove(node);

    var scope = angular.element(document.getElementById("WorkflowEditorController")).scope();

    if (!scope.currentObj)
        return true;
    if (scope.currentObj.subject)
        return false;

    var ngModelName = scope.currentObj.subject.part.data.ngModelName;
    if (ngModelName === 'start')
        scope.workflowStartModel = {};
    else
        delete scope.workflowModel[ngModelName];
};

function mouseEnter(e, obj) {
    if (myOldNodeData.e && myOldNodeData.obj) {
        mouseLeave(myOldNodeData.e, myOldNodeData.obj);
        myOldNodeData = {};
    }

    myOldNodeData.e = e;
    myOldNodeData.obj = obj;

    myKey = obj.Zd.key;
    var deleteButton = obj.findObject("Delete");
    var shapeMain = obj.findObject("MAINPANEL");

    if (shapeMain)
        shapeMain.stroke = 'rgba(0, 0, 0, 0)';

    if (deleteButton && !deleteButton.visible) {
        deleteButton.visible = true;
        return;
    }

    var icon = new go.Shape("ICON");
    var shape = obj.findObject("PANEL");

    //Delete icon
    icon.geometry = go.Geometry.parse("M12,24c-3.2,0-6.2-1.3-8.5-3.5C1.3,18.2,0,15.2,0,12c0-3.2,1.3-6.2,3.5-8.5 C5.8,1.3,8.8,0,12,0c3.2,0,6.2,1.3,8.5,3.5C22.7,5.8,24,8.8,24,12c0,3.2-1.3,6.2-3.5,8.5C18.2,22.7,15.2,24,12,24z M7.2,8.2 c-0.1,0-0.2,0-0.2,0.1C6.9,8.4,6.9,8.5,6.9,8.6l0.5,8.9c0,0.3,0.2,0.7,0.4,0.9c0.2,0.2,0.6,0.4,0.9,0.4h6.7c0.3,0,0.7-0.1,0.9-0.4 c0.2-0.2,0.4-0.5,0.4-0.9l0.5-8.9c0-0.1,0-0.2-0.1-0.2c-0.1-0.1-0.1-0.1-0.2-0.1L7.2,8.2z M6.7,5.6c-0.2,0-0.3,0.1-0.5,0.2 C6.1,5.9,6,6.1,6,6.3V7c0,0.1,0,0.2,0.1,0.2c0.1,0.1,0.1,0.1,0.2,0.1h11.6c0.1,0,0.2,0,0.2-0.1c0.1-0.1,0.1-0.1,0.1-0.2V6.3 c0-0.2-0.1-0.3-0.2-0.5c-0.1-0.1-0.3-0.2-0.5-0.2h-3.1l-0.2-0.5C14.2,5,14.2,4.9,14,4.8c-0.1-0.1-0.2-0.1-0.4-0.1h-3.1 c-0.1,0-0.2,0-0.4,0.1C10.1,4.9,10,5,10,5.1L9.7,5.6L6.7,5.6z", true);
    //icon.geometry.normalize();
    icon.name = "Delete"
    icon.fill = "rgba(0, 0, 0, 0.5)";
    icon.stroke = "rgba(0, 0, 0, 0.5)";
    icon.strokeWidth = 0;
    icon.background = "#F2F2F2";//ActivityNodeFill;
    icon.width = 20;
    icon.height = 20;
    icon.margin = new go.Margin(5, 10, 0, 0);
    icon.opacity = 0.5;
    icon.alignment = go.Spot.TopRight;
    icon.alignmentFocus = go.Spot.TopRight;
    icon.cursor = "pointer";
    icon.click = DeleteNode;
    shape.add(icon);
};

function mouseEnterForEvent(e, obj) {
    if (myOldNodeData.e && myOldNodeData.obj) {
        mouseLeave(myOldNodeData.e, myOldNodeData.obj);
        myOldNodeData = {};
    }

    myOldNodeData.e = e;
    myOldNodeData.obj = obj;
    myKey = obj.Zd.key;
    var shapeMain = obj.findObject("MAINPANEL");
    shapeMain.stroke = 'rgba(0, 0, 0, 0)';

    var deleteButton = obj.findObject("Delete");

    if (deleteButton) {
        deleteButton.visible = true;
        return;
    }

    var icon = new go.Shape("ICON");
    var shape = obj.findObject("PANEL");

    //Delete icon
    icon.geometry = go.Geometry.parse("M12,24c-3.2,0-6.2-1.3-8.5-3.5C1.3,18.2,0,15.2,0,12c0-3.2,1.3-6.2,3.5-8.5 C5.8,1.3,8.8,0,12,0c3.2,0,6.2,1.3,8.5,3.5C22.7,5.8,24,8.8,24,12c0,3.2-1.3,6.2-3.5,8.5C18.2,22.7,15.2,24,12,24z M7.2,8.2 c-0.1,0-0.2,0-0.2,0.1C6.9,8.4,6.9,8.5,6.9,8.6l0.5,8.9c0,0.3,0.2,0.7,0.4,0.9c0.2,0.2,0.6,0.4,0.9,0.4h6.7c0.3,0,0.7-0.1,0.9-0.4 c0.2-0.2,0.4-0.5,0.4-0.9l0.5-8.9c0-0.1,0-0.2-0.1-0.2c-0.1-0.1-0.1-0.1-0.2-0.1L7.2,8.2z M6.7,5.6c-0.2,0-0.3,0.1-0.5,0.2 C6.1,5.9,6,6.1,6,6.3V7c0,0.1,0,0.2,0.1,0.2c0.1,0.1,0.1,0.1,0.2,0.1h11.6c0.1,0,0.2,0,0.2-0.1c0.1-0.1,0.1-0.1,0.1-0.2V6.3 c0-0.2-0.1-0.3-0.2-0.5c-0.1-0.1-0.3-0.2-0.5-0.2h-3.1l-0.2-0.5C14.2,5,14.2,4.9,14,4.8c-0.1-0.1-0.2-0.1-0.4-0.1h-3.1 c-0.1,0-0.2,0-0.4,0.1C10.1,4.9,10,5,10,5.1L9.7,5.6L6.7,5.6z", true);
    //icon.geometry.normalize();
    icon.name = "Delete"
    icon.fill = "rgba(0, 0, 0, 0.5)";
    icon.stroke = "rgba(0, 0, 0, 0.5)";
    icon.strokeWidth = 0;
    icon.background = "#F2F2F2";//ActivityNodeFill;
    icon.width = 20;
    icon.height = 20;
    //icon.margin = new go.Margin(0, 0,5, 25);
    icon.opacity = 0.5;
    icon.alignment = go.Spot.TopRight;
    //icon.alignmentFocus = go.Spot.BottomLeft;
    icon.cursor = "pointer";
    icon.click = DeleteNode;
    shape.add(icon);
};

function mouseLeave(e, obj) {
    var a = obj.findObject("Delete");
    var b = obj.findObject("ICON");
    var shapeMain = obj.findObject("MAINPANEL");

    if (a)
        a.visible = false;
    if (b)
        b.visible = false;

    var key = obj.Zd.key;
    var currentNode = window.myDiagram.findNodeForKey(key);

    if (currentNode)
        if (currentNode.data.valid == false)
            shapeMain.stroke = "red";

    // obj.remove(b);
};

//------------------------------------------  pools / lanes   ----------------------------------------------

// swimlanes
var MINLENGTH = 400;  // this controls the minimum length of any swimlane
var MINBREADTH = 20;  // this controls the minimum breadth of any non-collapsed swimlane

// some shared functions

// this is called after nodes have been moved or lanes resized, to layout all of the Pool Groups again
function relayoutDiagram(e) {
    var node = e.subject.Th.key.Zd;
    var obj = window.myDiagram.findNodeForKey(node.key);
    if (node.category == 'event')
        mouseEnterForEvent(e, obj);
    else if (node.category == 'activity')
        mouseEnter(e, obj);

    window.myDiagram.layout.invalidateLayout();
    window.myDiagram.findTopLevelGroups().each(function (g) {
        if (g.category === "Pool") g.layout.invalidateLayout();
    });
    window.myDiagram.layoutDiagram();
}

// compute the minimum size of a Pool Group needed to hold all of the Lane Groups
function computeMinPoolSize(pool) {
    // assert(pool instanceof go.Group && pool.category === "Pool");
    var len = MINLENGTH;
    pool.memberParts.each(function (lane) {
        // pools ought to only contain lanes, not plain Nodes
        if (!(lane instanceof go.Group)) return;
        var holder = lane.placeholder;
        if (holder !== null) {
            var sz = holder.actualBounds;
            len = Math.max(len, sz.width);
        }
    });
    return new go.Size(len, NaN);
}

// compute the minimum size for a particular Lane Group
function computeLaneSize(lane) {
    // assert(lane instanceof go.Group && lane.category !== "Pool");
    var sz = computeMinLaneSize(lane);
    if (lane.isSubGraphExpanded) {
        var holder = lane.placeholder;
        if (holder !== null) {
            var hsz = holder.actualBounds;
            sz.height = Math.max(sz.height, hsz.height);
        }
    }
    // minimum breadth needs to be big enough to hold the header
    var hdr = lane.findObject("HEADER");
    if (hdr !== null) sz.height = Math.max(sz.height, hdr.actualBounds.height);
    return sz;
}

// determine the minimum size of a Lane Group, even if collapsed
function computeMinLaneSize(lane) {
    if (!lane.isSubGraphExpanded) return new go.Size(MINLENGTH, 1);
    return new go.Size(MINLENGTH, MINBREADTH);
}


// define a custom ResizingTool to limit how far one can shrink a lane Group
function LaneResizingTool() {
    go.ResizingTool.call(this);
}

go.Diagram.inherit(LaneResizingTool, go.ResizingTool);

LaneResizingTool.prototype.isLengthening = function () {
    return (this.handle.alignment === go.Spot.Right);
};

/** @override */
LaneResizingTool.prototype.computeMinSize = function () {
    var lane = this.adornedObject.part;
    // assert(lane instanceof go.Group && lane.category !== "Pool");
    var msz = computeMinLaneSize(lane);  // get the absolute minimum size
    if (this.isLengthening()) {  // compute the minimum length of all lanes
        var sz = computeMinPoolSize(lane.containingGroup);
        msz.width = Math.max(msz.width, sz.width);
    } else {  // find the minimum size of this single lane
        var sz = computeLaneSize(lane);
        msz.width = Math.max(msz.width, sz.width);
        msz.height = Math.max(msz.height, sz.height);
    }
    return msz;
};

/** @override */
LaneResizingTool.prototype.canStart = function () {
    if (!go.ResizingTool.prototype.canStart.call(this)) return false;

    // if this is a resize handle for a "Lane", we can start.
    var diagram = this.diagram;
    if (diagram === null) return;
    var handl = this.findToolHandleAt(diagram.firstInput.documentPoint, this.name);
    if (handl === null || handl.part === null || handl.part.adornedObject === null || handl.part.adornedObject.part === null) return false;
    return (handl.part.adornedObject.part.category === "Lane");
}

/** @override */
LaneResizingTool.prototype.resize = function (newr) {
    var lane = this.adornedObject.part;
    if (this.isLengthening()) {  // changing the length of all of the lanes
        lane.containingGroup.memberParts.each(function (lane) {
            if (!(lane instanceof go.Group)) return;
            var shape = lane.resizeObject;
            if (shape !== null) {  // set its desiredSize length, but leave each breadth alone
                shape.width = newr.width;
            }
        });
    } else {  // changing the breadth of a single lane
        go.ResizingTool.prototype.resize.call(this, newr);
    }
    relayoutDiagram();  // now that the lane has changed size, layout the pool again
};
// end LaneResizingTool class


// define a custom grid layout that makes sure the length of each lane is the same
// and that each lane is broad enough to hold its subgraph
function PoolLayout() {
    go.GridLayout.call(this);
    this.cellSize = new go.Size(1, 1);
    this.wrappingColumn = 1;
    this.wrappingWidth = Infinity;
    this.isRealtime = false;  // don't continuously layout while dragging
    this.alignment = go.GridLayout.Position;
    // This sorts based on the location of each Group.
    // This is useful when Groups can be moved up and down in order to change their order.
    this.comparer = function (a, b) {
        var ay = a.location.y;
        var by = b.location.y;
        if (isNaN(ay) || isNaN(by)) return 0;
        if (ay < by) return -1;
        if (ay > by) return 1;
        return 0;
    };
}

go.Diagram.inherit(PoolLayout, go.GridLayout);

/** @override */
PoolLayout.prototype.doLayout = function (coll) {
    var diagram = this.diagram;
    if (diagram === null) return;
    diagram.startTransaction("PoolLayout");
    var pool = this.group;
    if (pool !== null && pool.category === "Pool") {
        // make sure all of the Group Shapes are big enough
        var minsize = computeMinPoolSize(pool);
        pool.memberParts.each(function (lane) {
            if (!(lane instanceof go.Group)) return;
            if (lane.category !== "Pool") {
                var shape = lane.resizeObject;
                if (shape !== null) {  // change the desiredSize to be big enough in both directions
                    var sz = computeLaneSize(lane);
                    shape.width = (isNaN(shape.width) ? minsize.width : Math.max(shape.width, minsize.width));
                    shape.height = (!isNaN(shape.height)) ? Math.max(shape.height, sz.height) : sz.height;
                    var cell = lane.resizeCellSize;
                    if (!isNaN(shape.width) && !isNaN(cell.width) && cell.width > 0) shape.width = Math.ceil(shape.width / cell.width) * cell.width;
                    if (!isNaN(shape.height) && !isNaN(cell.height) && cell.height > 0) shape.height = Math.ceil(shape.height / cell.height) * cell.height;
                }
            }
        });
    }
    // now do all of the usual stuff, according to whatever properties have been set on this GridLayout
    go.GridLayout.prototype.doLayout.call(this, coll);
    diagram.commitTransaction("PoolLayout");
};
// end PoolLayout class


//------------------------------------------  Commands for this application  ----------------------------------------------

// Add a port to the specified side of the selected nodes.   name is beN  (be0, be1)
// evDim is 5 for Interrupting, 6 for non-Interrupting
function addActivityNodeBoundaryEvent(evType, evDim) {
    window.myDiagram.startTransaction("addBoundaryEvent");
    window.myDiagram.selection.each(function (node) {
        // skip any selected Links
        if (!(node instanceof go.Node)) return;
        if (node.data && (node.data.category === "activity" || node.data.category === "subprocess")) {
            // compute the next available index number for the side
            var i = 0;
            var defaultPort = node.findPort("");
            while (node.findPort("be" + i.toString()) !== defaultPort) i++;           // now this new port name is unique within the whole Node because of the side prefix
            var name = "be" + i.toString();
            if (!node.data.boundaryEventArray) {
                window.myDiagram.model.setDataProperty(node.data, "boundaryEventArray", []);
            }       // initialize the Array of port data if necessary
            // create a new port data object
            var newportdata = {
                portId: name,
                eventType: evType,
                eventDimension: evDim,
                color: "white",
                alignmentIndex: i
                // if you add port data properties here, you should copy them in copyPortData above  ** BUG...  we don't do that.
            };
            // and add it to the Array of port data
            window.myDiagram.model.insertArrayItem(node.data.boundaryEventArray, -1, newportdata);
        }
    });
    window.myDiagram.commitTransaction("addBoundaryEvent");
}

// changes the item of the object
function rename(obj) {
    window.myDiagram.startTransaction("rename");
    var newName = prompt("Rename " + obj.part.data.item + " to:");
    window.myDiagram.model.setDataProperty(obj.part.data, "item", newName);
    window.myDiagram.commitTransaction("rename");
}

// shows/hides gridlines
// to be implemented onclick of a button
function updateGridOption(value) {
    //window.myDiagram.startTransaction("grid"); 
    //window.myDiagram.commitTransaction("grid"); 
    window.myDiagram.grid.visible = value;

}

// enables/disables snapping tools, to be implemented by buttons
function updateSnapOption(value) {
    // no transaction needed, because we are modifying tools for future use 
    if (value) {
        window.myDiagram.toolManager.draggingTool.isGridSnapEnabled = true;
        window.myDiagram.toolManager.resizingTool.isGridSnapEnabled = true;
        window.myDiagram.toolManager.draggingTool.gridSnapCellSize = new go.Size(20, 20);
    } else {
        window.myDiagram.toolManager.draggingTool.isGridSnapEnabled = false;
        window.myDiagram.toolManager.resizingTool.isGridSnapEnabled = false;
    }
}

// user specifies the amount of space between nodes when making rows and column
function askSpace() {
    var space = prompt("Desired space between nodes (in pixels):", "0");
    return space;
}

var UnsavedFileName = "(Unsaved File)";

function getCurrentFileName() {
    var currentFile = document.getElementById("currentFile");
    var name = currentFile.textContent;
    if (name[name.length - 1] === "*") return name.substr(0, name.length - 1);
    return name;
}

function setCurrentFileName(name) {
    var currentFile = document.getElementById("currentFile");
    if (window.myDiagram.isModified) {
        name += "*";
    }
    currentFile.textContent = name;
}

function newDocument() {
    // checks to see if all changes have been saved
    if (window.myDiagram.isModified) {
        var save = confirm("Would you like to save changes to " + getCurrentFileName() + "?");
        if (save) {
            saveDocument();
        }
    }
    setCurrentFileName(UnsavedFileName);
    // loads an empty diagram
    window.myDiagram.model = new go.GraphLinksModel();
    resetModel();
}

function resetModel() {
    window.myDiagram.model.undoManager.isEnabled = true;
    window.myDiagram.model.linkFromPortIdProperty = "fromPort";
    window.myDiagram.model.linkToPortIdProperty = "toPort";

    window.myDiagram.model.copiesArrays = true;
    window.myDiagram.model.copiesArrayObjects = true;
    window.myDiagram.isModified = false;
}

function checkLocalStorage() {
    return (typeof (Storage) !== "undefined") && (window.localStorage !== undefined);
}

// saves the current floor plan to local storage
function saveDocument() {
    if (checkLocalStorage()) {
        var saveName = getCurrentFileName();
        if (saveName === UnsavedFileName) {
            saveDocumentAs();
        } else {
            saveDiagramProperties()
            window.localStorage.setItem(saveName, window.myDiagram.model.toJson());
            window.myDiagram.isModified = false;
        }
    }
    myChange();
}

// saves floor plan to local storage with a new name
function saveDocumentAs() {
    if (checkLocalStorage()) {
        var saveName = prompt("Save file as...", getCurrentFileName());
        if (saveName && saveName !== UnsavedFileName) {
            setCurrentFileName(saveName);
            saveDiagramProperties()
            window.localStorage.setItem(saveName, window.myDiagram.model.toJson());
            window.myDiagram.isModified = false;
        }
    }
    myChange();
}


// checks to see if all changes have been saved -> shows the open HTML element
//function openDocument() {
//    if (checkLocalStorage()) {
//        if (window.myDiagram.isModified) {
//            var save = confirm("Would you like to save changes to " + getCurrentFileName() + "?");
//            if (save) {
//                saveDocument();
//            }
//        }
//        openElement("openDocument", "mySavedFiles");
//    }
//    myChange();
//}

// shows the remove HTML element
//function removeDocument() {
//    if (checkLocalStorage()) {
//        openElement("removeDocument", "mySavedFiles2");
//    }
//}

// these functions are called when panel buttons are clicked

function loadFile() {
    var listbox = document.getElementById("mySavedFiles");
    // get selected filename
    var fileName = undefined;
    for (var i = 0; i < listbox.options.length; i++) {
        if (listbox.options[i].selected) fileName = listbox.options[i].text; // selected file
    }
    if (fileName !== undefined) {
        // changes the text of "currentFile" to be the same as the floor plan now loaded
        setCurrentFileName(fileName);
        // actually load the model from the JSON format string
        var savedFile = window.localStorage.getItem(fileName);
        window.myDiagram.model = go.Model.fromJson(savedFile);
        loadDiagramProperties();
        window.myDiagram.model.undoManager.isEnabled = true;
        window.myDiagram.isModified = false;
        // eventually loadDiagramProperties will be called to finish
        // restoring shared saved model/diagram properties
    }
    closeElement("openDocument");
    myChange();
}

function loadJSON(file) {
    jQuery.getJSON(file, function (jsondata) {
        // set these kinds of Diagram properties after initialization, not now
        window.myDiagram.addDiagramListener("InitialLayoutCompleted", loadDiagramProperties);  // defined below
        // create the model from the data in the JavaScript object parsed from JSON text
        //myDiagram.model = new go.GraphLinksModel(jsondata["nodes"], jsondata["links"]);
        window.myDiagram.model = go.Model.fromJson(jsondata);
        loadDiagramProperties();
        window.myDiagram.model.undoManager.isEnabled = true;
        window.myDiagram.isModified = false;
    });
    myChange();
}

window.loadJSONData = function (data) {
    // set these kinds of Diagram properties after initialization, not now
    window.myDiagram.addDiagramListener("InitialLayoutCompleted", loadDiagramProperties);  // defined below
    // create the model from the data in the JavaScript object parsed from JSON text
    //myDiagram.model = new go.GraphLinksModel(jsondata["nodes"], jsondata["links"]);
    window.myDiagram.model = go.Model.fromJson(data);
    loadDiagramProperties();
    window.myDiagram.model.undoManager.isEnabled = true;
    window.myDiagram.isModified = false;
    myChange();
};

// Store shared model state in the Model.modelData property
// (will be loaded by loadDiagramProperties)
function saveDiagramProperties() {
    mwindow.yDiagram.model.modelData.position = go.Point.stringify(window.myDiagram.position);
}

// Called by loadFile and loadJSON.
function loadDiagramProperties(e) {
    // set Diagram.initialPosition, not Diagram.position, to handle initialization side-effects
    var pos = window.myDiagram.model.modelData.position;
    if (pos) window.myDiagram.initialPosition = go.Point.parse(pos);
}


// deletes the selected file from local storage
function removeFile() {
    var listbox = document.getElementById("mySavedFiles2");
    // get selected filename
    var fileName = undefined;
    for (var i = 0; i < listbox.options.length; i++) {
        if (listbox.options[i].selected) fileName = listbox.options[i].text; // selected file
    }
    if (fileName !== undefined) {
        // removes file from local storage
        window.localStorage.removeItem(fileName);
        // the current document remains open, even if its storage was deleted
    }
    //closeElement("removeDocument");
}

function updateFileList(id) {
    // displays cached floor plan files in the listboxes
    var listbox = document.getElementById(id);
    // remove any old listing of files
    var last;
    while (last = listbox.lastChild) listbox.removeChild(last);
    // now add all saved files to the listbox
    for (var key in window.localStorage) {
        var storedFile = window.localStorage.getItem(key);
        if (!storedFile) continue;
        var option = document.createElement("option");
        option.value = key;
        option.text = key;
        listbox.add(option, null);
    }
    myChange();
}

function openElement(id, listid) {
    var panel = document.getElementById(id);
    if (panel.style.visibility === "hidden") {
        updateFileList(listid);
        panel.style.visibility = "visible";
    }
    myChange();
}

// hides the open/remove elements when the "cancel" button is pressed
function closeElement(id) {
    var panel = document.getElementById(id);
    if (panel.style.visibility === "visible") {
        panel.style.visibility = "hidden";
    }
}

function myChange() {
    document.getElementById("jsonData").innerHTML = window.myDiagram.model.toJson();
}


