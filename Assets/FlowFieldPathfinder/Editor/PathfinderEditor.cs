using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace FlowPathfinding
{
    [CustomEditor(typeof(Pathfinder))]
    public class PathfinderEditor : Editor
    {
        private Transform brush = null;
        private List<Tile> tilesCostChanged = new List<Tile>();

        bool showSettings = true;
        bool showVisualDebuggingSettings = false;
        bool showCostDrawing = false;

        string[] brushScaleNames = new string[] { "1x1", "3x3", "5x5", "7x7" };
        int[] brushScaleValues = new int[] { 1, 3, 5, 7 };

        string[] sectorLevelNames = new string[] { "None", "0", "1" };
        int[] sectorLevelValues = new int[] { -1, 0, 1 };

        GUIContent[] maxLevelNames = null;
        int[] maxLevelValues = new int[] { 0, 1, 2 };

        string[] levelScalingNames = new string[] { "2", "3", "4" };
        int[] levelScalingValues = new int[] { 2, 3, 4 };

        GUIContent content = new GUIContent();



        private void Settings(Pathfinder myTarget)
        {
            if(maxLevelNames == null)
            {
                maxLevelNames = new GUIContent[3];
                maxLevelNames[0] = new GUIContent();
                maxLevelNames[1] = new GUIContent();
                maxLevelNames[2] = new GUIContent();

                maxLevelNames[0].text = "None";
                maxLevelNames[1].text = "1";
                maxLevelNames[2].text = "2";
            }


            if (showSettings)
                GUI.backgroundColor = Color.grey;
            else
                GUI.backgroundColor = Color.green;

            if (GUILayout.Button(" Settings "))
                showSettings = !showSettings;

            GUI.backgroundColor = Color.white;

            if (showSettings)
                ShowSettings(myTarget);

        }

        private void ShowSettings(Pathfinder myTarget)
        {
            EditorGUILayout.Space();
            content.text = "Multi Layered Structure";
            content.tooltip = "Enable when walkable areas go over eachother";
            myTarget.worldIsMultiLayered = EditorGUILayout.Toggle(content, myTarget.worldIsMultiLayered);
            EditorGUILayout.Space();
            myTarget.worldStart = EditorGUILayout.Vector3Field("World Top-Left ", myTarget.worldStart);
            myTarget.worldWidth = EditorGUILayout.FloatField("World Width ", myTarget.worldWidth);
            myTarget.worldLength = EditorGUILayout.FloatField("World Length ", myTarget.worldLength);
            myTarget.worldHeight = EditorGUILayout.FloatField("World Height ", myTarget.worldHeight);

            content.text = "Climb Height";
            content.tooltip = "Defines the allowable height diffrence between tiles";
            myTarget.generationClimbHeight = EditorGUILayout.FloatField(content, myTarget.generationClimbHeight);

            myTarget.tileSize = EditorGUILayout.FloatField("Tile Size ", myTarget.tileSize);

            content.text = "Sector Size";
            content.tooltip = "sector rectangle size in relation to tiles: 1x1, 2x2, etc, tiles big";
            myTarget.sectorSize = EditorGUILayout.IntField(content, myTarget.sectorSize);

            if (myTarget.worldIsMultiLayered)
            {
                content.text = "Character/Layer Height";
                content.tooltip = "Defines the minimal Y distance between geometry, try to make it match your character plus a little extra";
                myTarget.characterHeight = EditorGUILayout.FloatField(content, myTarget.characterHeight);
                myTarget.twoDimensionalMode = false;
            }
            else
            {
                content.text = "2D Mode";
                content.tooltip = "Base Seeker location on Y position instead of Z. Intended for 2D X/Y axis games only";
                myTarget.twoDimensionalMode = EditorGUILayout.Toggle(content, myTarget.twoDimensionalMode);
            }

            content.text = "Levels Of Abstration";
            content.tooltip = "Amount of abstraction layers, usually you will want to use 1";
            myTarget.maxLevelAmount = EditorGUILayout.IntPopup(content, myTarget.maxLevelAmount, maxLevelNames, maxLevelValues);
            myTarget.levelScaling = EditorGUILayout.IntPopup("Abstration Scaling", myTarget.levelScaling, levelScalingNames, levelScalingValues);


            EditorGUILayout.Space();
            content.text = "Maximun Angle of Ramps ";
            content.tooltip = "This value shows you the maximum angle geometry can have from the ground away. This is determined by the ClimbHeight and TileSize ";
            EditorGUILayout.FloatField("Maximun Angle of Ramps ", Mathf.Atan(myTarget.generationClimbHeight / myTarget.tileSize) * (180 / Mathf.PI));
            EditorGUILayout.Space();

            content.text = "Ground Layer ";
            content.tooltip = "Defines which layers are seen as walkable ";
            myTarget.groundLayer = EditorGUILayout.LayerField(content, myTarget.groundLayer);

            content.text = "Obstacle Layer ";
            content.tooltip = "Defines which layers are seen as blocked ";
            myTarget.obstacleLayer = EditorGUILayout.LayerField(content, myTarget.obstacleLayer);
        }




        private void VisualDebugging(Pathfinder myTarget)
        {
            if (showVisualDebuggingSettings)
                GUI.backgroundColor = Color.grey;
            else
                GUI.backgroundColor = Color.green;

            if (GUILayout.Button(" Visual Debugging "))
                showVisualDebuggingSettings = !showVisualDebuggingSettings;

            GUI.backgroundColor = Color.white;

            if (showVisualDebuggingSettings)
                ShowVisualDebugging(myTarget);

        }

        private void ShowVisualDebugging(Pathfinder myTarget)
        {
            EditorGUILayout.Space();
            myTarget.drawTiles = EditorGUILayout.Toggle("Draw Tiles ", myTarget.drawTiles);
            myTarget.drawSectors = EditorGUILayout.Toggle("Draw Sectors ", myTarget.drawSectors);
            myTarget.drawSectorNetwork = EditorGUILayout.Toggle("Draw Network Graph ", myTarget.drawSectorNetwork);
            myTarget.drawSectorLevel = EditorGUILayout.IntPopup("Draw Level", myTarget.drawSectorLevel, sectorLevelNames, sectorLevelValues);
            EditorGUILayout.Space();

            myTarget.drawTree = EditorGUILayout.Toggle("Draw Data Tree  ", myTarget.drawTree);

            EditorGUILayout.Space();
            myTarget.showIntergrationField = EditorGUILayout.Toggle("Draw Integration Fields  ", myTarget.showIntergrationField);
            myTarget.showFlowField = EditorGUILayout.Toggle("Draw Flow Fields ", myTarget.showFlowField);

        }




        private void CostDrawing(Pathfinder myTarget)
        {
            if (showCostDrawing)
                GUI.backgroundColor = Color.grey;
            else
                GUI.backgroundColor = Color.green;

            if (GUILayout.Button(" Edit Cost "))
            {
                if (myTarget.costManager == null)
                    myTarget.costManager = myTarget.GetComponent<CostFieldManager>();

                if (showCostDrawing) // showing now, will be closed
                {
                    myTarget.worldData.drawCost = false;
                    myTarget.costManager.RemoveCostField();
                }

                showCostDrawing = !showCostDrawing;
            }


            GUI.backgroundColor = Color.white;

            if (showCostDrawing)
                ShowCostDrawing(myTarget);

        }

        private void ShowCostDrawing(Pathfinder myTarget)
        {
            EditorGUILayout.Space();
            if (myTarget.worldData.drawCost)
            {
                if (GUILayout.Button(" Close cost field "))
                {
                    myTarget.worldData.drawCost = false;
                    myTarget.costManager.RemoveCostField();
                }
            }
            else
            {
                if (GUILayout.Button(" Open/Create cost field "))
                {
                    myTarget.worldData.drawCost = true;
                    myTarget.costManager.SetupCostField();
                }
            }


            if (myTarget.worldData.drawCost)
            {
                myTarget.maxCostValue = EditorGUILayout.IntSlider("Maximum Cost Value ", myTarget.maxCostValue, 1, 100);
                myTarget.brushStrength = EditorGUILayout.IntSlider("Brush Strength ", myTarget.brushStrength, 1, 100);


                myTarget.brushSize = EditorGUILayout.IntPopup("Brush Size", myTarget.brushSize, brushScaleNames, brushScaleValues);
                myTarget.brushFallOff = EditorGUILayout.IntSlider("Brush Strength FallOff", myTarget.brushFallOff, 0, 10);
            }





            EditorGUILayout.Space();
            if (GUILayout.Button(" Save Cost Field "))
            {
                Debug.Log("save button pressed");
                myTarget.GetComponent<SaveLoad>().SaveLevel();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button(" Load Cost Field "))
            {
                myTarget.GenerateWorld(false, true);
            }
        }



        public override void OnInspectorGUI()
        {
            Pathfinder myTarget = (Pathfinder)target;

            Undo.RecordObject(myTarget, "modify settings on Pathfinder");

            EditorGUILayout.Space();
            Settings(myTarget);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            VisualDebugging(myTarget);


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            CostDrawing(myTarget);



            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();





            GUI.backgroundColor = Color.yellow;
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            content.text = "Generate Map";
            content.tooltip = "Gives you a preview of what the map will look like, and allows you to draw cost in the editor.";
            if (GUILayout.Button(content))
            {
                myTarget.GenerateWorld(false, false);
                EditorUtility.SetDirty(target);
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            content.text = "Remove Existing CostField";
            content.tooltip = "Save an empty costfield over the existing one.";
            if (GUILayout.Button(content))
            {
                myTarget.worldData.costFields = new List<int[][]>();
                myTarget.GetComponent<SaveLoad>().SaveLevel();
            }



            //SECURITY!! only change if you understand what this changes, read documentation
            if (GUI.changed)
            {
                if (myTarget.generationClimbHeight > myTarget.characterHeight * 0.98f)
                    myTarget.generationClimbHeight = myTarget.characterHeight * 0.98f;

                //myTarget.characterClimbHeight = myTarget.generationClimbHeight;
                // if (myTarget.characterClimbHeight > myTarget.generationClimbHeight * 0.98f)
                //    myTarget.characterClimbHeight = myTarget.generationClimbHeight * 0.98f;



                //myTarget.tileSize = EditorGUILayout.FloatField("Tile Size ", myTarget.tileSize);
                // Undo.RegisterUndo(myTarget, "Tile Size");
                //Undo.RecordObject(myTarget, "Set Tile Size");
                EditorUtility.SetDirty(target);
            }

        }


        void OnSceneGUI()
        {
            Pathfinder myTarget = (Pathfinder)target;

            if (myTarget.worldData.worldAreas.Count != 0) // no values/ nothing generated
            {
                if (myTarget.worldData.drawCost)
                {
                    if (brush == null)
                    {
                        brush = myTarget.transform.GetChild(1).gameObject.transform;
                        brush.gameObject.SetActive(true);
                    }

                    brush.localScale = new Vector3(myTarget.brushSize * myTarget.tileSize, 1, myTarget.brushSize * myTarget.tileSize);

                    Event current = Event.current;
                    int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
                    int layer = (1 << myTarget.groundLayer);

                    switch (current.type)
                    {
                        case EventType.MouseMove:
                            {
                                Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
                                RaycastHit hit;

                                if (Physics.Raycast(ray, out hit, 100f, layer))
                                {
                                    WorldArea area = myTarget.worldData.tileManager.GetWorldAreaAtPosition(hit.point);
                                    Tile tileHit = myTarget.worldData.tileManager.GetTileInWorldArea(area, hit.point);

                                    if (tileHit != null)
                                        brush.position = myTarget.worldData.tileManager.GetTileWorldPosition(tileHit, area);
                                }

                                break;
                            }


                        case EventType.MouseDrag:
                            {
                                if (current.button == 0)
                                {
                                    //Ray ray = Camera.current.ScreenPointToRay(e.mousePosition);
                                    Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
                                    RaycastHit hit;

                                    if (Physics.Raycast(ray, out hit, 100f, layer))
                                    {
                                        WorldArea area = myTarget.worldData.tileManager.GetWorldAreaAtPosition(hit.point);
                                        Tile tileHit = myTarget.worldData.tileManager.GetTileInWorldArea(area, hit.point);

                                        if (tileHit != null)
                                        {
                                            tileHit.cost += myTarget.brushStrength;
                                            if (tileHit.cost > myTarget.maxCostValue)
                                                tileHit.cost = myTarget.maxCostValue;

                                            tilesCostChanged.Clear();
                                            tilesCostChanged.Add(tileHit);
                                            brush.position = myTarget.worldData.tileManager.GetTileWorldPosition(tileHit, area);



                                            if (myTarget.brushSize != 1)
                                            {
                                                int brushMaxDif = (int)(myTarget.brushSize * 0.5f);

                                                for (int x = -brushMaxDif; x <= brushMaxDif; x++)
                                                {
                                                    for (int y = -brushMaxDif; y <= brushMaxDif; y++)
                                                    {
                                                        if (x != 0 || y != 0)
                                                        {
                                                            if (Physics.Raycast(brush.position + new Vector3(x * myTarget.tileSize, 0.1f, y * myTarget.tileSize) + (Vector3.up * myTarget.generationClimbHeight), Vector3.down, out hit, myTarget.generationClimbHeight + 0.2f, layer))
                                                            {
                                                                area = myTarget.worldData.tileManager.GetWorldAreaAtPosition(hit.point);
                                                                tileHit = myTarget.worldData.tileManager.GetTileInWorldArea(area, hit.point);

                                                                if (tileHit != null)
                                                                {
                                                                    int fallOff = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) * myTarget.brushFallOff;
                                                                    if (fallOff < myTarget.brushStrength)
                                                                    {
                                                                        tileHit.cost += myTarget.brushStrength - fallOff;
                                                                        if (tileHit.cost > myTarget.maxCostValue)
                                                                            tileHit.cost = myTarget.maxCostValue;

                                                                        tilesCostChanged.Add(tileHit);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                            myTarget.costManager.EditCostFieldAlpha(tilesCostChanged);
                                        }
                                    }
                                }
                                current.Use();
                                break;
                            }


                        case EventType.Layout:
                            HandleUtility.AddDefaultControl(controlID);
                            break;



                    }

                    if (GUI.changed)
                        EditorUtility.SetDirty(target);
                }
                else
                {
                    if (brush != null)
                        brush.gameObject.SetActive(false);

                    brush = null;
                }
            }

        }


    }
}