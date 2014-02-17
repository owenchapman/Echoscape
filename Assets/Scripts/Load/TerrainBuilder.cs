//#define OFFLINE

using System;
using System.Collections;
using System.Collections.Generic;
#if OFFLINE
using UnityEditor;
#endif
using Poly2Tri;

using UnityEngine;

class TerrainBuilder
{
    private const int mapDataSize = 25;
    private float pScale;
    private Vector3 moveVec;
    
    public bool writeAsset = false;

    public void SetScale(float scale, Vector3 translate)
    {
        pScale = scale;
        moveVec = translate;
    }

    private void ParseAscData(DownloadedData dlData, ConstructedData conData)
    {
        var heightData = dlData.terrain.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        conData.heightMap = new LatLonAlt[mapDataSize, mapDataSize];

        int lineNum = 0;
        for (int x = 0; x < mapDataSize; x++)
        {
            for (int z = 0; z < mapDataSize; z++)
            {
                lineNum = (x * mapDataSize) + z;
                //Debug.Log(lineNum);
                // [lon] \s* [lat] \s* [alt]
                var line = heightData[lineNum].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var lon = float.Parse(line[0]);
                var lat = float.Parse(line[1]);
                var alt = float.Parse(line[2]);

                if (alt < 0)
                    alt = 0;

                conData.heightMap[x, z] = new LatLonAlt { Lat = lat, Lon = lon, Alt = alt };
            }
        }
    }

    public void ConstructTerrain (GameObject parent, Map worldMap, DownloadedData dlData, ConstructedData conData, Action completedCallback)
		{
				ParseAscData (dlData, conData);

				var pts = new List<TriangulationPoint> ();
				var heights = new List<float> ();

				for (int z = 0; z < mapDataSize; z++) {
						for (int x = 0; x < mapDataSize; x++) {
								var coordData = conData.heightMap [x, z];
								var vec = Util.LonLat2Mercator2 (new Vector3 (coordData.Lat, coordData.Alt, coordData.Lon), worldMap);
								//vec += moveVec;

								var pt = new TriangulationPoint (vec.x, vec.z);

								heights.Add (vec.y);
								pts.Add (pt);
						}
				}

				var bndry = new PointSet (pts);
				var mesh2 = Util.CreateMeshFromPointSet (bndry, heights); // flattens the mesh (doesn't use y coord)
				mesh2 = Util.FlipMesh (mesh2);
				mesh2.RecalculateNormals ();
		
				//create asset
				#if OFFLINE
				if (writeAsset) {
						AssetDatabase.CreateAsset (mesh2, "Assets/Resources/UserAssets/terrain.asset");	
						var loadedMesh = (Mesh)Resources.Load ("UserAssets/terrain.asset");
				}
				#endif

        var vizTerrain = new GameObject("Rendered Terrain");
        vizTerrain.transform.parent = parent.transform;

        var mat = (Material)Resources.Load("Materials/Ground");
        vizTerrain = Util.AddMeshToGameObject(vizTerrain, mesh2, mat, false);
        var meshCollider = vizTerrain.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh2;
        vizTerrain.layer = 8;
        vizTerrain.renderer.enabled = false;
        conData.terrain = vizTerrain;


      
        completedCallback();
    }
}
