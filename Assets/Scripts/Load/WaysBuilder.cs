//#define OFFLINE

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

#if OFFLINE
using UnityEditor;
#endif

using ClipperLib;
using Poly2Tri;

public class WaysBuilder
{
    private const float clipScale = 10000f;
    private float pScale;
    private Vector3 moveVec;
    private Map map;
    private float mapScale;
    
    public bool writeAsset = false;

    public void SetScale(float scale, Vector3 translate)
    {
        pScale = scale;
        moveVec = translate;

    }

    public void ConstructWays(GameObject parent, Map worldMap, DownloadedData dlData, ConstructedData conData,
                                     Action completedCallback)
    {

        map = worldMap;
        mapScale = (float)map.RoundedScaleMultiplier;
        conData.ways = new List<MapWayModel>();

        var doc = new o5mParser();
        if (!doc.Parse(dlData.ways))
            throw new Exception("failed to parse o5m!");

        Debug.Log("created o5mParser");

        var mapNodes = GetMapNodes(doc.nodes, conData, parent);

        foreach (var way in doc.ways)
        {
            var currentNodeIds = new List<int>();

            string name = "";
            MapWayModel.WayType type = MapWayModel.WayType.Other;

            // get each noderef for the way
            foreach (var nodeRef in way.refs)
                currentNodeIds.Add((int)nodeRef);

            // set way name, if valid
            if (way.tags.ContainsKey("name"))
                name = way.tags["name"];

            // check way type
            if (way.tags.ContainsKey("highway"))
                type = MapWayModel.WayType.Highway;

            else if (way.tags.ContainsKey("building"))
                type = MapWayModel.WayType.Building;
            else
                continue; // we only deal with highways and buildings

            // create map ways from gathered data
            var mapway = new MapWayModel();
            mapway.Name = name;
            mapway.Type = type;
            mapway.Nodes = new List<MapNodeModel>();

            foreach (var id in currentNodeIds)
                if (mapNodes.ContainsKey(id))
                    mapway.Nodes.Add(mapNodes[id]);

            conData.ways.Add(mapway);
        }

        Debug.Log("creating way outlines");
        CreateOutlines(parent, conData);

        //FIXME
        //navigation object
        var navObj = new GameObject("Navigation");
        navObj.transform.parent = parent.transform;
        navObj.transform.localScale *= 10f * mapScale;
        var nav = navObj.AddComponent<Navigation>();
        nav.Ways = conData.ways;



        //Turn on Range Vol
        //var rangeVol = Camera.mainCamera.GetComponentInChildren<RangeVol>();
        //rangeVol.enabled = true;

        completedCallback();
    }

    Dictionary<int, MapNodeModel> GetMapNodes(List<WayNode> wayNodes, ConstructedData conData, GameObject parent)
    {
        var mapNodes = new Dictionary<int, MapNodeModel>();
        var counter = 0;

        foreach (var node in wayNodes)
        {
            var data = new MapNodeModel();
            data.Id = (int)node.id; // this should never be higher than 32 bits!
            data.Latitude = node.GetFriendlyLat();
            data.Longitude = node.GetFriendlyLon();

            // 0f is likely bad data, skip
            if (data.Longitude == 0f || data.Latitude == 0)
                continue;

            //Convert to World Coords
            data.Position = Util.LonLat2Mercator2(new Vector3(data.Latitude, 0, data.Longitude), map);
            //data.Position += moveVec;

            if (data.Position != null)
                counter++;
            //Sample Terrain Height and reposition data points
            var pos = data.Position;
            pos.y = Util.GetTerrainHeightAt(pos);
            data.Position = pos;

            mapNodes[data.Id] = data;
        }

        Debug.Log("Created " + counter + "Way points");
        return mapNodes;
    }

    void CreateOutlines(GameObject parent, ConstructedData conData)
    {
        var roadPolys = new List<List<IntPoint>>();
        var buildingPolys = new List<List<IntPoint>>();

        for (int i = 0; i < conData.ways.Count; i++)
        {
            var way = conData.ways[i];

            if (way.Type == MapWayModel.WayType.Highway)
                roadPolys.Add(Util.CreateOffsetClipFromWay(way, clipScale));

            if (way.Type == MapWayModel.WayType.Building)
                if (way.Nodes.Count > 2)
                    buildingPolys.Add(Util.CreateClipFromWay(way, clipScale));

        }

        Debug.Log("finished creating clips");
        Debug.Log("Created " + roadPolys.Count + "Road polys");

        //Build Road meshes


        RoadsFromClips(parent, roadPolys, conData);
        BuildingsFromClips(parent, buildingPolys, conData);
    }

    void RoadsFromClips (GameObject parent, List<List<IntPoint>> allPolys, ConstructedData conData)
		{
				Debug.Log ("About to build roads");

				//Road Meshes
				var roadMeshes = new GameObject ("Road Meshes");
				roadMeshes.transform.parent = parent.transform;

				allPolys = Clipper.OffsetPolygons (allPolys, 5d * mapScale * clipScale, JoinType.jtRound, 1d * mapScale * clipScale);
				Debug.Log ("offset all polygons");

				Debug.Log (conData.terrain.collider.bounds.ToString ());
        
				var roadDiff = Util.RoadDifferenceSingle (allPolys, conData.terrain.collider.bounds, clipScale);
				var roadPoly = new PolygonSet ();

				foreach (var road in roadDiff) {
						var tempPoly = Util.Clip2Poly (road.outer, clipScale);

						foreach (var h in road.holes) {
								var hPoly = Util.Clip2Poly (h, clipScale);
								tempPoly.AddHole (hPoly);
						}

						roadPoly.Add (tempPoly);
				}

				Debug.Log ("Created " + roadDiff.Count + " offset polygons");

				var tmpMeshList = new List<Mesh> ();
				var vertCount = 0;

				foreach (var r in roadPoly.Polygons) {
						var mesh = new Mesh ();
						var ex1Mesh = new List<Mesh> ();

						try {
								mesh = Util.CreateMeshFromPoly (r);
								ex1Mesh = Util.ExtrudeMesh (mesh, 1.5f * mapScale, false);
								ex1Mesh.RemoveAt (0);
						} catch (Exception) {
								Debug.Log ("error creating road mesh");
						}

						foreach (var m in ex1Mesh) {
								if (vertCount + m.vertexCount <= 12000) {
										tmpMeshList.Add (m);
										vertCount += m.vertexCount;
								} else {

										var obj = new GameObject ();
										obj.transform.parent = roadMeshes.transform;
										var fullMesh = CombineMeshes (tmpMeshList, obj);
										Util.calculateMeshTangents (fullMesh);
										obj.name = "Road Mesh_" + fullMesh.vertexCount;

										//var tmp = Util.FlipMesh(fullMesh);
										//create asset
										
										#if OFFLINE
										if (writeAsset) {
												var tmpName = obj.name + ".asset";
												var path = "Assets/Resources/UserAssets/" + tmpName;
												AssetDatabase.CreateAsset (fullMesh, path);
										}
										#endif

										var mat = Resources.Load ("Materials/Road", typeof(Material)) as Material;
										obj = Util.AddMeshToGameObject (obj, fullMesh, mat, false);
										//var meshCollider = obj.AddComponent<MeshCollider>();
										//meshCollider.isTrigger = true;
										//meshCollider.sharedMesh = tmp;
										obj.layer = 10;

										//reset
										vertCount = 0;
										tmpMeshList = new List<Mesh> ();
										tmpMeshList.Add (m);
								}
						}
				}

				if (tmpMeshList.Count > 0) {
						var obj = new GameObject ();
						obj.transform.parent = roadMeshes.transform;
						var fullMesh = CombineMeshes (tmpMeshList, obj);
						Util.calculateMeshTangents (fullMesh);
						obj.name = "Road Mesh_" + fullMesh.vertexCount;

						//var tmp = Util.FlipMesh(fullMesh);
						//create asset
						#if OFFLINE
						if (writeAsset) {
								var tmpName = obj.name + ".asset";
								var path = "Assets/Resources/UserAssets/" + tmpName;
								AssetDatabase.CreateAsset (fullMesh, path);
						}
						#endif
	
			//var loadedMesh = (Mesh) Resources.Load("UserAssets/" + tmpName);

            var mat = Resources.Load("Materials/Road", typeof(Material)) as Material;
            obj = Util.AddMeshToGameObject(obj, fullMesh, mat, false);
            //var meshCollider = obj.AddComponent<MeshCollider>();
            //meshCollider.isTrigger = true;
            //meshCollider.sharedMesh = tmp;
            obj.layer = 10;

            //reset
            vertCount = 0;

            Debug.Log("built a road and vert count is:  " + vertCount);
        }


        Debug.Log("finished meshing roads");
    }


    Mesh CombineMeshes(List<Mesh> meshes, GameObject parentObj)
    {
        CombineInstance[] combine = new CombineInstance[meshes.Count];

        for(int i = 0; i < combine.Length; i++)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = parentObj.transform.localToWorldMatrix;
        }

        var combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine, true);
        combinedMesh.RecalculateNormals();

        
        return combinedMesh;
    }

    void BuildingsFromClips (GameObject parent, List<List<IntPoint>> allPolys, ConstructedData conData)
		{
				var buildingMeshes = new GameObject ("Building Meshes");
				buildingMeshes.transform.parent = parent.transform;

				//Road Meshes
				var bldDiff = new List<ExPolygon> ();

				for (int i = 0; i < allPolys.Count; i++) {
						allPolys [i] = Util.SelfUnion (allPolys [i]);
				}

				bldDiff = Util.RoadDifferenceSingle (allPolys, conData.terrain.collider.bounds, clipScale);

				var buildings = new List<List<IntPoint>> ();

				foreach (var bld in bldDiff)
						foreach (var h in bld.holes)
								buildings.Add (h);

				var bldPoly = new PolygonSet ();
				foreach (var bld in bldDiff) {
						foreach (var h in bld.holes) {
								var hPoly = Util.Clip2Poly (h, clipScale);
								bldPoly.Add (hPoly);
						}
				}

				var tmpMeshList = new List<Mesh> ();
				var vertCount = 0;

				foreach (var r in bldPoly.Polygons) {
						var mesh = new Mesh ();
						var exMesh = new List<Mesh> ();

						try {
								mesh = Util.CreateMeshFromPoly (r);
								//must scale by principal scale in order to make elevation work
								exMesh = Util.ExtrudeMesh (mesh, mapScale * UnityEngine.Random.Range (20f, 50f), true);
								exMesh.RemoveAt (0);
						} catch (Exception) {
						}

						foreach (var m in exMesh) {
								if (vertCount + m.vertexCount <= 5000) {
										tmpMeshList.Add (m);
										vertCount += m.vertexCount;
								} else {
                    
										var obj = new GameObject ();
										obj.transform.parent = buildingMeshes.transform;
										var fullMesh = CombineMeshes (tmpMeshList, obj);
										Util.calculateMeshTangents (fullMesh);
										obj.name = "Building Mesh_" + fullMesh.vertexCount;

										//var tmp = Util.FlipMesh(fullMesh);
										//create asset
										#if OFFLINE
										if (writeAsset) {
												var tmpName = obj.name + ".asset";
												var path = "Assets/Resources/UserAssets/" + tmpName;
												AssetDatabase.CreateAsset (fullMesh, path);
										}
										#endif
		
										//var loadedMesh = (Mesh) Resources.Load("UserAssets/" + tmpName);

										var mat = Resources.Load ("Materials/Road", typeof(Material)) as Material;
										obj = Util.AddMeshToGameObject (obj, fullMesh, mat, false);
										//var meshCollider = obj.AddComponent<MeshCollider> ();
										//meshCollider.isTrigger = true;
										//meshCollider.sharedMesh = fullMesh;
										obj.layer = 10;

										//reset
										vertCount = 0;

										Debug.Log ("built a road and vert count is:  " + vertCount);
										tmpMeshList = new List<Mesh> ();
										tmpMeshList.Add (m);
								}
						}
				}

				if (tmpMeshList.Count > 0) {
						var obj = new GameObject ();
						obj.transform.parent = buildingMeshes.transform;
						var fullMesh = CombineMeshes (tmpMeshList, obj);
						Util.calculateMeshTangents (fullMesh);
						obj.name = "Building Mesh_" + fullMesh.vertexCount;

						//var tmp = Util.FlipMesh(fullMesh);
						//create asset
						#if OFFLINE
						if (writeAsset) {
								var tmpName = obj.name + ".asset";
								var path = "Assets/Resources/UserAssets/" + tmpName;
								AssetDatabase.CreateAsset (fullMesh, path);
						}
						#endif
	
			//var loadedMesh = (Mesh) Resources.Load("UserAssets/" + tmpName);
            var mat = Resources.Load("Materials/Road", typeof(Material)) as Material;
            
            obj = Util.AddMeshToGameObject(obj, fullMesh, mat, false);
            var meshCollider = obj.AddComponent<MeshCollider>();
            //meshCollider.isTrigger = true;
            meshCollider.sharedMesh = fullMesh;
            obj.layer = 10;

            //reset
            vertCount = 0;

            Debug.Log("built a building and vert count is:  " + vertCount);
        }


    }

}
