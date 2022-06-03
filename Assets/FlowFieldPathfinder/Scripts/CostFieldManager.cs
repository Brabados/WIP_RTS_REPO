using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class CostFieldManager : MonoBehaviour
    {
        public Pathfinder pathfinder;

        private List<GameObject> costFieldsGameObjects = new List<GameObject>();
        private Material myMaterial;
        private Texture2D texture;

        public void SetupCostField()
        {
            if (pathfinder.worldData.worldGenerated)
            {
                costFieldsGameObjects.Clear();
                GameObject costField = null;
                int count = pathfinder.transform.GetChild(0).childCount;

                bool createNewCostFields = false;
                if (pathfinder.worldData.costFields.Count == 0)
                    createNewCostFields = true;

                for (int i = 0; i < pathfinder.worldData.worldAreas.Count; i++)
                {
                    if (createNewCostFields)
                    {
                        int[][] costFieldValues = new int[pathfinder.worldData.worldAreas[i].gridWidth][];
                        for (int j = 0; j < pathfinder.worldData.worldAreas[i].gridWidth; j++)
                            costFieldValues[j] = new int[pathfinder.worldData.worldAreas[i].gridLength];

                        pathfinder.worldData.costFields.Add(costFieldValues);
                    }

                    // visuals
                    if (i < count) // re-use earlier made cost field
                    {
                        costField = pathfinder.transform.GetChild(0).GetChild(i).gameObject;
                        SetupCostFieldAlpha(costField, pathfinder.worldData.worldAreas[i]);
                        costField.transform.localScale = new Vector3(pathfinder.worldData.worldAreas[i].gridWidth * pathfinder.worldData.pathfinder.tileSize, 1, pathfinder.worldData.worldAreas[i].gridLength * pathfinder.worldData.pathfinder.tileSize);

                        if (pathfinder.worldData.worldAreas[i].angleDirectionX)
                            costField.transform.rotation = Quaternion.Euler(0, 0, pathfinder.worldData.worldAreas[i].angle * pathfinder.worldData.worldAreas[i].anglePositive);
                        else
                            costField.transform.rotation = Quaternion.Euler(pathfinder.worldData.worldAreas[i].angle * pathfinder.worldData.worldAreas[i].anglePositive, 0, 0);

                        costField.transform.position = pathfinder.worldData.worldAreas[i].origin + new Vector3((pathfinder.worldData.worldAreas[i].gridWidth - 1) * pathfinder.worldData.pathfinder.tileSize * 0.5f, 0, -(pathfinder.worldData.worldAreas[i].gridLength - 1) * pathfinder.worldData.pathfinder.tileSize * 0.5f);
                        costField.transform.position += Vector3.up * 0.15f; //costField.transform.up * 0.25f;


                        costField.SetActive(true);
                        costFieldsGameObjects.Add(costField);
                    }
                    else // create new cost field
                    {
                        costField = Instantiate(Resources.Load("Prefab/CostFieldVisualParent"), pathfinder.worldData.worldAreas[i].origin + new Vector3(pathfinder.worldData.worldAreas[i].gridWidth * pathfinder.worldData.pathfinder.tileSize * 0.5f, 0, -pathfinder.worldData.worldAreas[i].gridLength * pathfinder.worldData.pathfinder.tileSize * 0.5f), Quaternion.identity) as GameObject;
                        SetupCostFieldAlpha(costField, pathfinder.worldData.worldAreas[i]);
                        costField.transform.localScale = new Vector3(pathfinder.worldData.worldAreas[i].gridWidth * pathfinder.worldData.pathfinder.tileSize, 1, pathfinder.worldData.worldAreas[i].gridLength * pathfinder.worldData.pathfinder.tileSize);
                        costField.transform.parent = transform.GetChild(0);


                        if (pathfinder.worldData.worldAreas[i].angleDirectionX)
                            costField.transform.rotation = Quaternion.Euler(0, 0, pathfinder.worldData.worldAreas[i].angle * pathfinder.worldData.worldAreas[i].anglePositive);
                        else
                            costField.transform.rotation = Quaternion.Euler(pathfinder.worldData.worldAreas[i].angle * pathfinder.worldData.worldAreas[i].anglePositive, 0, 0);

                        costField.transform.position += Vector3.up * 0.15f; // costField.transform.up * 0.25f;
                        costFieldsGameObjects.Add(costField);
                    }
                }
            }
        }

        public void RemoveCostField()
        {
            if (pathfinder.worldData.worldGenerated)
            {
                foreach (GameObject field in costFieldsGameObjects)
                    if (field != null)
                        field.SetActive(false);
            }
        }

        public void SetupCostFieldAlpha(GameObject costField, WorldArea area)
        {
            myMaterial = new Material(Shader.Find("Custom/CostShader"));
            myMaterial.SetColor("_Color", Color.white);
            myMaterial.SetTexture("_MainTex", Resources.Load("Textures/Black") as Texture);
            myMaterial.SetTexture("_PathTex", Resources.Load("Textures/Green") as Texture);

            texture = new Texture2D(area.gridWidth, area.gridLength, TextureFormat.ARGB32, true);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            int y = 0;
            while (y < texture.height)
            {
                int x = 0;
                while (x < texture.width)
                {
                    Color color;
                    if (area.tileGrid[x][y] != null && pathfinder.worldData.costFields[area.index][x][y] != 1)
                        color = new Color(0, 0, 0, pathfinder.worldData.costFields[area.index][x][y] * 0.01f);
                    else
                        color = new Color(0, 0, 0, -1);

                    texture.SetPixel(x, texture.height - 1 - y, color);
                    x++;
                }
                y++;
            }

            texture.Apply();

            myMaterial.SetTexture("_PathMask", texture);
            costField.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = myMaterial;

            Resources.UnloadUnusedAssets();

        }

        public void EditCostFieldAlpha(List<Tile> tilesChanged)
        {
            if (pathfinder.worldData.costFields.Count > 0)
            {
                WorldArea area;
                List<int> areaIndexes = new List<int>();
                foreach (Tile tileHit in tilesChanged)
                {
                    area = pathfinder.worldData.worldAreas[tileHit.worldAreaIndex];

                    if (tileHit.cost == 1)
                        pathfinder.worldData.costFields[area.index][tileHit.gridPos.x][tileHit.gridPos.y] = 0;
                    else
                        pathfinder.worldData.costFields[area.index][tileHit.gridPos.x][tileHit.gridPos.y] = tileHit.cost;

                    if (!areaIndexes.Contains(tileHit.worldAreaIndex))
                        areaIndexes.Add(tileHit.worldAreaIndex);
                }

                GameObject costField;
                foreach (int index in areaIndexes)
                {
                    costField = costFieldsGameObjects[index];
                    area = pathfinder.worldData.worldAreas[index];

                    myMaterial = new Material(Shader.Find("Custom/CostShader"));
                    myMaterial.SetColor("_Color", Color.white);
                    myMaterial.SetTexture("_MainTex", Resources.Load("Textures/Black") as Texture);
                    myMaterial.SetTexture("_PathTex", Resources.Load("Textures/Green") as Texture);

                    texture = new Texture2D(area.gridWidth, area.gridLength, TextureFormat.ARGB32, true);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Point;

                    int y = 0;
                    while (y < texture.height)
                    {
                        int x = 0;
                        while (x < texture.width)
                        {
                            Color color;
                            if (area.tileGrid[x][y] != null && pathfinder.worldData.costFields[area.index][x][y] != 0)
                                color = new Color(0, 0, 0, pathfinder.worldData.costFields[area.index][x][y] * 0.01f);
                            else
                                color = new Color(0, 0, 0, -1);

                            texture.SetPixel(x, texture.height - 1 - y, color);
                            x++;
                        }
                        y++;
                    }

                    texture.Apply();

                    myMaterial.SetTexture("_PathMask", texture);
                    //Debug.Log("DONE");
                    costField.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = myMaterial;

                    Resources.UnloadUnusedAssets();
                }
            }
        }
    }
}
