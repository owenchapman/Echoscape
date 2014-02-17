using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using Poly2Tri;
using ClipperLib;

public static class Util
{
    public delegate void AsyncUrlHandler(string data);
    public delegate void AsyncUrlByteHandler(byte[] data);

    public static IEnumerator GetUrlBytes(string url, AsyncUrlByteHandler handler)
    {
        Debug.Log("downloading [" + url + "]");
        var www = new WWW(url);

        // wait until done (success or error)
        while (!www.isDone)
            yield return null;

        if (www.error != null)
        {
            Debug.Log(String.Format("[{0}] {1}", url, www.error));

            //hack to get out of loading screen
            GameObject.FindGameObjectWithTag("GUIManager").GetComponent<WindowManager>().GUIAlphaLerp(Color.clear);

        }
        else
            handler(www.bytes);
    }

    public static Rect[] SelectionGrid(Rect regionRect, int num, int hDiv, float vSize, float gutter)
    {
        var hSize = regionRect.width / hDiv;
        var rectArr = new Rect[num];

        for (int i = 0; i < num; i++)
        {
            var x = regionRect.x + ((i % hDiv) * hSize);
            var v = regionRect.y + (Mathf.FloorToInt(i / hDiv) * vSize);

            rectArr[i] = new Rect(x, v, hSize - gutter, vSize - gutter);

        }

        return rectArr;
    }

    public static Rect[] SelectionGrid(Rect regionRect, int num, string[] titles, int hDiv, float vSize, float gutter, bool drawButtons, Action<int> selectionMethod)
    {
        var hSize = regionRect.width / hDiv;
        var rectArr = new Rect[num];

        for (int i = 0; i < num; i++)
        {
            var x = regionRect.x + ((i % hDiv) * hSize);
            var v = regionRect.y + (Mathf.FloorToInt(i / hDiv) * vSize);

            rectArr[i] = new Rect(x, v, hSize - gutter, vSize - gutter);

            if (drawButtons)
            {
                if (GUI.Button(rectArr[i], titles[i]))
                {
                    selectionMethod(i);
                }
            }
        }

        return rectArr;
    }

    public static void BeginLoadingEchoscape(float lat, float lon, int tile, Map map)
    {

        // create new loader parented to EchoscapesManager
        var loader = new GameObject(lon + ", " + lat + " [" + tile + "]");
        loader.tag = "EchoscapeTile";

        // set up loading parameters
        var request = loader.AddComponent<EchoscapesLoader>();
        request.map = map;
        request.requestArea = new RequestArea { lat = lat, lon = lon, tile = tile };
    
    }

    public static IEnumerator GetUrlContents(string url, AsyncUrlHandler handler)
    {
        Debug.Log("downloading [" + url + "]");
        var www = new WWW(url);

        // wait until done (success or error)
        while (!www.isDone)
            yield return null;

        if (www.error != null)
            Debug.Log(String.Format("[{0}] {1}", url, www.error));
        else
            handler(www.text.Trim());
    }

    public static List<Vector3> FillLineVertices(MapWayModel way)
    {
        var nodeCount = way.Nodes.Count;
        var wayInterop = new List<Vector3>();
        var intLength = 5f;
        for (int j = 0; j < nodeCount - 1; j++)
        {
            var start = way.Nodes[j].Position;
            var end = way.Nodes[j + 1].Position;
            var vec = end - start;
            var vecD = vec.magnitude;

            if (j == 0)
                wayInterop.Add(start);

            if (vecD > intLength)
            {
                var intNum = Mathf.Floor(vecD / intLength);
                for (int i = 1; i < intNum; i++)
                {
                    var tmpPoint = start + i * intLength * vec.normalized;
                    tmpPoint.y = Util.GetTerrainHeightAt(tmpPoint) + 0.1f;
                    wayInterop.Add(tmpPoint);
                }
            }

            wayInterop.Add(end);
        }

        return wayInterop;
    }

    public static List<IntPoint> CreateOffsetClipFromWay(MapWayModel way, float scale)
    {
        var width = 0.001f;

        //Resample Way points at a common length
        var filledVerts = FillLineVertices(way);
        var nodeCount = filledVerts.Count;
        var wayPoly = new List<IntPoint>();

        //Initialize
        for (int i = 0; i < nodeCount * 2; i++)
            wayPoly.Add(new IntPoint());

        
        for (int i = 0; i < nodeCount; i++)
        {
            // vertices
            var segment = new Vector3();
            var cp = new Vector3();
            var lh = new Vector3();
            var rh = new Vector3();
            var lhIntPt = new IntPoint();
            var rhIntPt = new IntPoint();

            if (i == nodeCount - 1)
            {
                segment = filledVerts[i - 1] - filledVerts[i];
                cp = Vector3.Cross(Vector3.up, segment).normalized;
                lh = filledVerts[i] - width * cp;
                rh = filledVerts[i] + width * cp;
            }
            else
            {
                segment = filledVerts[i + 1] - filledVerts[i];
                cp = Vector3.Cross(Vector3.up, segment).normalized;
                lh = filledVerts[i] + width * cp;
                rh = filledVerts[i] - width * cp;
            }

            //Transform Vector3 into IntPoint by scaling for Clipper. 
            //This needs to be undone later!

            lhIntPt.X = (int)Mathf.Round(lh.x * scale);
            lhIntPt.Y = (int)Mathf.Round(lh.z * scale);
            rhIntPt.X = (int)Mathf.Round(rh.x * scale);
            rhIntPt.Y = (int)Mathf.Round(rh.z * scale);

            wayPoly[i] = lhIntPt;
            wayPoly[2 * nodeCount - i - 1] = rhIntPt;

            //Debug.Log(wayPoly[i].X.ToString());
            //Debug.Log(wayPoly[i].Y.ToString());
        }


        return wayPoly;
    }

    public static List<IntPoint> CreateClipFromWay(MapWayModel way, float scale)
    {
        //Resample Way points at a common length
        var filledVerts = FillLineVertices(way);
        var nodeCount = filledVerts.Count;
        var wayPoly = new List<IntPoint>();

        //Initialize
        for (int i = 0; i < nodeCount; i++)
            wayPoly.Add(new IntPoint());


        
        for (int i = 0; i < nodeCount; i++)
        {
            wayPoly[i].X = (int)(filledVerts[i].x * scale);
            wayPoly[i].Y = (int)(filledVerts[i].z * scale);
        }


        return wayPoly;
    }

    public static List<ExPolygon> RoadUnion(List<List<IntPoint>> roadPolys)
    {
        //First Pass
        var roadUnion = new List<ExPolygon>();
        var c = new Clipper();

        c.AddPolygons(roadPolys, PolyType.ptSubject);

        c.Execute(ClipType.ctUnion, roadUnion, PolyFillType.pftNonZero, PolyFillType.pftNegative);

        return roadUnion;
    }

    public static List<IntPoint> SelfUnion(List<IntPoint> roadPoly)
    {
        //First Pass
        var roadUnion = new List<ExPolygon>();
        var c = new Clipper();

        c.AddPolygon(roadPoly, PolyType.ptSubject);

        c.Execute(ClipType.ctUnion, roadUnion, PolyFillType.pftNonZero, PolyFillType.pftNegative);

        if (roadUnion.Count > 0)
            return roadUnion[0].outer;
        else
            return new List<IntPoint>();
    }

    public static List<ExPolygon> RoadDifference(List<List<IntPoint>> roadPolys, Bounds bounds, int div, int divX, int divY, float scale)
    {
        //First Pass
        var roadDiff = new List<ExPolygon>();
        var c = new Clipper();
        var bndry = new List<IntPoint>();
        div -= 1;

        var minX = (int)(bounds.min.x * scale);
        var maxX = (int)(bounds.max.x * scale);
        var minY = (int)(bounds.min.z * scale);
        var maxY = (int)(bounds.max.z * scale);

        bndry.Add(new IntPoint(minX, minY));
        bndry.Add(new IntPoint(minX, maxY));
        bndry.Add(new IntPoint(maxX, maxY));
        bndry.Add(new IntPoint(maxX, minY));

        c.AddPolygons(roadPolys, PolyType.ptClip);
        bndry.Reverse();
        c.AddPolygon(bndry, PolyType.ptSubject);

        c.Execute(ClipType.ctDifference, roadDiff, PolyFillType.pftNonZero, PolyFillType.pftPositive);

        return roadDiff;
    }

    public static List<ExPolygon> RoadDifferenceSingle(List<List<IntPoint>> roadPolys, Bounds bounds, float scale)
    {
        //First Pass
        var roadDiff = new List<ExPolygon>();
        var c = new Clipper();
        var bndry = new List<IntPoint>();


        var minX = (int)(bounds.min.x * scale);
        var maxX = (int)(bounds.max.x * scale);
        var minY = (int)(bounds.min.z * scale);
        var maxY = (int)(bounds.max.z * scale);

        bndry.Add(new IntPoint(minX, minY));
        bndry.Add(new IntPoint(minX, maxY));
        bndry.Add(new IntPoint(maxX, maxY));
        bndry.Add(new IntPoint(maxX, minY));


        c.AddPolygons(roadPolys, PolyType.ptClip);
        //bndry.Reverse();
        c.AddPolygon(bndry, PolyType.ptSubject);

        c.Execute(ClipType.ctDifference, roadDiff, PolyFillType.pftNonZero, PolyFillType.pftPositive);

        return roadDiff;
    }

    public static List<ExPolygon> BuildingIntersection(List<List<IntPoint>> roadPolys, int radius, float scale)
    {
        //First Pass
        var roadDiff = new List<ExPolygon>();
        var c = new Clipper();

        var bndry = new List<IntPoint>();
        bndry.Add(new IntPoint(-radius * (int)scale, -radius * (int)scale));
        bndry.Add(new IntPoint(-radius * (int)scale, radius * (int)scale));
        bndry.Add(new IntPoint(radius * (int)scale, radius * (int)scale));
        bndry.Add(new IntPoint(radius * (int)scale, -radius * (int)scale));

        c.AddPolygons(roadPolys, PolyType.ptSubject);
        bndry.Reverse();
        c.AddPolygon(bndry, PolyType.ptSubject);


        c.Execute(ClipType.ctIntersection, roadDiff, PolyFillType.pftNonZero, PolyFillType.pftPositive);

        return roadDiff;
    }

    public static Polygon Clip2Poly(List<IntPoint> clipgon, float scale)
    {
        var length = clipgon.Count;
        var polyPts = new PolygonPoint[length];

        for (int i = 0; i < length; i++)
        {
            polyPts[i] = new PolygonPoint((double)clipgon[i].X / scale, (double)clipgon[i].Y / scale);
        }

        var poly = new Polygon(polyPts);

        return poly;

    }

    public static Mesh CopyMesh(Mesh mesh)
    {
        var tmpMesh = new Mesh();
        tmpMesh.vertices = mesh.vertices;
        tmpMesh.triangles = mesh.triangles;
        tmpMesh.uv = mesh.uv;
        tmpMesh.normals = mesh.normals;

        return tmpMesh;
    }

    public static void DebugPoly(Polygon poly)
    {
        //debug polys
        if (poly == null)
            return;

        var length = poly.Points.Count;
        var polyPts = poly.Points;
        for (int i = 0; i < length - 1; i++)
        {
            var start = new Vector3();
            var end = new Vector3();

            start.x = (float)polyPts[i].X;
            start.z = (float)polyPts[i].Y;
            start.y = GetTerrainHeightAt(start) + 1f;

            end.x = (float)polyPts[i + 1].X;
            end.z = (float)polyPts[i + 1].Y;
            end.y = GetTerrainHeightAt(end) + 1f;


            Debug.DrawLine(start, end, Color.cyan);

        }
    }

    public static List<Mesh> ExtrudeMesh(Mesh mesh, float dist, bool flatCap)
    {
        var exMeshes = new List<Mesh>();
        var nodeCount = mesh.vertices.Count();
        var topCap = float.MinValue;

        //Bottom mesh - just a copy of the input
        exMeshes.Add(mesh);

        //Side meshes
        var sideMesh = new Mesh();
        var allLengths = new float[nodeCount - 1];

        for (int i = 0; i < mesh.vertices.Count() - 1; i++)
        {
            var start = mesh.vertices[i];
            var end = mesh.vertices[i + 1];
            var seg = (end - start).magnitude;
            allLengths[i] = seg;
        }

        //vertices
        var verts = new Vector3[nodeCount * 2];

        // find highest point, a flat cap will set entire roof to highest y + dist
        if (flatCap)
        {
            for (int i = 0; i < nodeCount; i++)
                if (mesh.vertices[i].y > topCap)
                    topCap = mesh.vertices[i].y;
            topCap += dist;
        }

        for (int i = 0; i < nodeCount; i++)
        {
            verts[i] = mesh.vertices[i];
            var roofNode = mesh.vertices[i];

            if (flatCap)
                roofNode.y = topCap;
            else
                roofNode.y += dist; // metres

            verts[i + nodeCount] = roofNode;
        }
        sideMesh.vertices = verts;

        //triangles
        var indCount = nodeCount * 6;
        var inds = new int[indCount];

        for (int i = 0; i < nodeCount; i++)
        {
            var offset = i * 6;

            if (i == nodeCount - 1)
            {
                inds[offset] = i;
                inds[offset + 1] = 0;
                inds[offset + 2] = nodeCount + i;

                inds[offset + 3] = 0;
                inds[offset + 4] = nodeCount + 0;
                inds[offset + 5] = nodeCount + i;
            }
            else
            {
                inds[offset] = i;
                inds[offset + 1] = i + 1;
                inds[offset + 2] = nodeCount + i;

                inds[offset + 3] = i + 1;
                inds[offset + 4] = nodeCount + i + 1;
                inds[offset + 5] = nodeCount + i;
            }
        }
        sideMesh.triangles = inds;

        // fancy uvs....
        var uvs = new Vector2[nodeCount * 2];
        var curLength = 0f;
        var tileSizeU = 20f;
        var tileSizeV = 20f;

        for (int i = 0; i < nodeCount; i++)
        {
            if (i > 0)
                curLength += allLengths[i - 1];
            var v = mesh.vertices[i].y / tileSizeV;

            uvs[i] = new Vector2(curLength / tileSizeU, 0);
            uvs[nodeCount + i] = new Vector2(curLength / tileSizeU, v);
        }
        sideMesh.uv = uvs;
        sideMesh.RecalculateNormals();

        exMeshes.Add(sideMesh);

        //Top Mesh - translated copy of the first
        var topMesh = new Mesh();
        var topVerts = new Vector3[nodeCount];
        Vector3 translate = new Vector3(0, dist, 0);                   

        for (int i = 0; i < nodeCount; i++)
        {
            topVerts[i] = mesh.vertices[i];

            if (flatCap)
                topVerts[i].y = topCap;
            else
                topVerts[i] += translate;
        }

        topMesh.vertices = topVerts;
        topMesh.triangles = mesh.triangles;
        topMesh.normals = mesh.normals;
        topMesh.uv = mesh.uv;


        exMeshes.Add(topMesh);

        return exMeshes;
    }

    public static Mesh FlipMesh(Mesh meshIn)
    {

        var meshOut = new Mesh();
        var nodeCount = meshIn.vertices.Count();

        //vertices
        var verts = new Vector3[2 * nodeCount];

        for (int i = 0; i < nodeCount; i++)
        {
            verts[i] = meshIn.vertices[i];
            verts[i + nodeCount] = meshIn.vertices[i];
        }

        meshOut.vertices = verts;


        //triangles
        var triCount = meshIn.triangles.Count();
        var triangles = new int[3 * triCount];


        for (int i = 0; i < triCount; i += 3)
        {
            triangles[i] = meshIn.triangles[i];
            triangles[i + triCount] = meshIn.triangles[i];

            triangles[i + 1] = meshIn.triangles[i + 1];
            triangles[i + triCount + 1] = meshIn.triangles[i + 2];

            triangles[i + 2] = meshIn.triangles[i + 2];
            triangles[i + triCount + 2] = meshIn.triangles[i + 1];
        }

        meshOut.triangles = triangles;

        //normals
        var normals = new Vector3[2 * meshIn.normals.Count()];

        for (int i = 0; i < meshIn.normals.Count(); i++)
        {
            normals[i] = meshIn.normals[i];
            normals[i + meshIn.normals.Count()] = -meshIn.normals[i];
        }

        meshOut.normals = normals;

        //UV
        var uvs = new Vector2[2 * nodeCount];

        for (int i = 0; i < nodeCount; i++)
        {
            uvs[i] = meshIn.uv[i];
            uvs[i + nodeCount] = meshIn.uv[i];
        }

        meshOut.uv = uvs;


        return meshOut;
    }

    public static Mesh ReverseMesh(Mesh mesh)
    {
        var tris = new int[mesh.triangles.Length];

        for (int i = 0; i < tris.Length/3; i++)
        {
            tris[3*i] = mesh.triangles[3*i];
            tris[3*i + 1] = mesh.triangles[3*i + 2];
            tris[3*i + 2] = mesh.triangles[3*i + 1];
        }

        mesh.triangles = tris;

        return mesh;
    }



    public static void calculateMeshTangents(Mesh mesh)
    {
        //speed up math by copying the mesh arrays
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        Vector3[] normals = mesh.normals;
     
        //variable definitions
        int triangleCount = triangles.Length;
        int vertexCount = vertices.Length;
     
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];
     
        Vector4[] tangents = new Vector4[vertexCount];
     
        for (long a = 0; a < triangleCount; a += 3)
        {
            long i1 = triangles[a + 0];
            long i2 = triangles[a + 1];
            long i3 = triangles[a + 2];
     
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];
     
            Vector2 w1 = uv[i1];
            Vector2 w2 = uv[i2];
            Vector2 w3 = uv[i3];
     
            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;
     
            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;
     
            float r = 1.0f / (s1 * t2 - s2 * t1);
     
            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
     
            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;
     
            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }
     
     
        for (long a = 0; a < vertexCount; ++a)
        {
            Vector3 n = normals[a];
            Vector3 t = tan1[a];
     
            //Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
            //tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
            Vector3.OrthoNormalize(ref n, ref t);
            tangents[a].x = t.x;
            tangents[a].y = t.y;
            tangents[a].z = t.z;
     
            tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
        }
     
        mesh.tangents = tangents;
    }

    public static Vector3 LonLat2Mercator(Vector3 toProject, float pScale)
    {
        float lon = toProject.x * Mathf.Deg2Rad;
        float lat = toProject.z * Mathf.Deg2Rad;


        //these gives values between lon: -Pi to Pi and lat: -Pi/2 to Pi/2
        var x = lon;
        var z = (float)Mathf.Log(Mathf.Tan(lat) + (1f / Mathf.Cos(lat)));

        //rescale
        var pt = pScale * new Vector3(x, 0f, z);
        pt.y = (pScale / 6371009f) * toProject.y;

        //pt.x = scale * (pt.x / Mathf.PI);
        //pt.y = scale * (pt.y / (0.5f * Mathf.PI));

        return pt;
    }

    public static Vector3 LonLat2Mercator2(Vector3 toProject, Map map)
    {
        var coord2xy = map.wgs84ToEPSG900913Transform;

        //var center = map.CenterWGS84;
        var centerXY = map.CenterEPSG900913;// coord2xy.Transform(center);
        var ptXY = coord2xy.Transform(new double[2] { toProject.z, toProject.x });

        var ptVec = new Vector3((float)(ptXY[0] - centerXY[0]), 0f, (float)(ptXY[1] - centerXY[1]));
        ptVec *=  map.RoundedScaleMultiplier;
        ptVec.y = toProject.y * map.RoundedScaleMultiplier;

        return ptVec;
    }

    public static Vector3 Mercator2LonLat2(Vector3 toProject, Map map)
    {
        double[] displacementMeters = new double[2] { toProject.x / map.RoundedScaleMultiplier, toProject.z / map.RoundedScaleMultiplier };
        double[] centerMeters = new double[2] { map.CenterEPSG900913[0], map.CenterEPSG900913[1] };
        centerMeters[0] += displacementMeters[0];
        centerMeters[1] += displacementMeters[1];

        var xy2coord = map.epsg900913ToWGS84Transform;
        double[] mouseLatLon = xy2coord.Transform(centerMeters);

        return new Vector3((float)mouseLatLon[0], toProject.y, (float)mouseLatLon[1]);
    }

    public static Vector3 Mercator2LonLat(Vector3 toProject, float pScale)
    {
        //assumes the equator is on the x axis, and 0 longitude is on y axis.
        float x = toProject.x / pScale;
        float y = toProject.z / pScale;

        var lon = x;
        var lat = 2f * Mathf.Atan(Mathf.Exp(y)) - 0.5f * Mathf.PI;

        //first two are lon and lat, z is altitude
        var pt = new Vector3(lat * Mathf.Rad2Deg, lon * Mathf.Rad2Deg, toProject.y);
        
        //not sure about this...
        pt.z *= pScale;

        return pt;
    }

    public static Mesh CreateMeshFromPointSet(PointSet poly, List<float> alt)
    {

        var mesh = new Mesh();

        Poly2Tri.P2T.Triangulate(poly);

        var vertCount = poly.Points.Count;

        //Verts
        var verts = new Vector3[vertCount];
        for (int i = 0; i < vertCount; i++)
            verts[i] = new Vector3((float)poly.Points[i].X, alt[i], (float)poly.Points[i].Y);

        mesh.vertices = verts;

        var inds = new int[poly.Triangles.Count * 3];
        for (int i = 0; i < poly.Triangles.Count; i++)
        {
            inds[3 * i + 2] = poly.Points.IndexOf(poly.Triangles[i].Points._0);
            inds[3 * i + 1] = poly.Points.IndexOf(poly.Triangles[i].Points._1);
            inds[3 * i + 0] = poly.Points.IndexOf(poly.Triangles[i].Points._2);
        }
        mesh.triangles = inds;

        //Normals
        var normals = new Vector3[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            normals[i] = Vector3.up;
        }
        mesh.normals = normals;

        // uv
        var uvs = new Vector2[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            uvs[i] = new Vector2(verts[i].x / 10f, verts[i].z / 10f);
        }
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh CreateMeshFromPoly(Polygon poly)
    {

        var mesh = new Mesh();

        Poly2Tri.P2T.Triangulate(poly);

        if (poly.Holes != null)
        {
            if (poly.Holes.Count > 0)
            {
                foreach (var h in poly.Holes)
                {
                    foreach (var p in h.Points)
                        poly.Points.Add(p);
                }
            }
        }

        var vertCount = poly.Points.Count;

        //Verts
        var verts = new Vector3[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            verts[i] = new Vector3((float)poly.Points[i].X, 0f, (float)poly.Points[i].Y);
            //verts[i].y = 1000f;
            verts[i].y = Util.GetTerrainHeightAt(verts[i]);
        }
        mesh.vertices = verts;

        var inds = new int[poly.Triangles.Count * 3];

        for (int i = 0; i < poly.Triangles.Count; i++)
        {
            inds[3 * i + 2] = poly.Points.IndexOf(poly.Triangles[i].Points._0);
            inds[3 * i + 1] = poly.Points.IndexOf(poly.Triangles[i].Points._1);
            inds[3 * i + 0] = poly.Points.IndexOf(poly.Triangles[i].Points._2);
        }
        mesh.triangles = inds;

        //Normals
        var normals = new Vector3[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            normals[i] = Vector3.up;
        }
        mesh.normals = normals;

        // uv
        var uvs = new Vector2[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            uvs[i] = new Vector2(verts[i].x / 100f, verts[i].z / 100f);
        }
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        

        return mesh;
    }

    public static Mesh CreatePlanarUV(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        Bounds bounds = mesh.bounds;
        int i = 0;
        while (i < uvs.Length)
        {
            uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.x);
            i++;
        }
        mesh.uv = uvs;

        return mesh;
    }

    public static float GetTerrainHeightAt(Vector3 position)
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(position, Vector3.down, out hitInfo, 2000, 1 << 8))
            return hitInfo.point.y;
        else if (Physics.Raycast(position, Vector3.up, out hitInfo, 2000, 1 << 8))
            return hitInfo.point.y;

        return 0f;
    }

    public static float GetRoadHeightAt(Vector3 position)
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(position, Vector3.down, out hitInfo, 2000, 1 << 9))
            return hitInfo.point.y;
        else if (Physics.Raycast(position, Vector3.up, out hitInfo, 2000, 1 << 9))
            return hitInfo.point.y;

        return 0f;
    }

    public static float GetBuildingHeightAt(Vector3 position)
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(position, Vector3.down, out hitInfo, 2000, 1 << 10))
            return hitInfo.point.y;
        else if (Physics.Raycast(position, Vector3.up, out hitInfo, 2000, 1 << 10))
            return hitInfo.point.y;

        return 0f;
    }

    public static bool IsPointInPoly(List<IntPoint> poly, Vector3 pt, float scale)
    {
        var interior = false;
        pt.y = 0f;
        var pCount = poly.Count;
        int iCounter = 0;
        var start = new Vector3();
        var end = new Vector3();
        //bool runningTest = false;
        //bool stop = false;
        //int i = 0;

        for (int i = 0; i < pCount; i++)
        {
            if (i == pCount - 1)
            {
                start = new Vector3((float)poly[i].X, 0f, (float)poly[i].Y);
                end = new Vector3((float)poly[0].X, 0f, (float)poly[0].Y);
            }
            else
            {
                start = new Vector3((float)poly[i].X, 0f, (float)poly[i].Y);
                end = new Vector3((float)poly[i + 1].X, 0f, (float)poly[i + 1].Y);
            }

            var test = Util.IntersectRayAndLine(pt, start, end, scale);

            if (test)
                iCounter++;

        }

        if (iCounter % 2 != 0)
            interior = true;


        return interior;
    }

    public static bool IntersectRayAndLine(Vector3 pt, Vector3 lStart, Vector3 lEnd, float scale)
    {
        bool intersection = false;

        //convert points to scaled integers for speed
        var iStartX = (float)(lStart.x);
        var iStartY = (float)(lStart.z);
        var iEndX = (float)(lEnd.x);
        var iEndY = (float)(lEnd.z);
        var iTestX = (float)(pt.x * scale);
        var iTestY = (float)(pt.z * scale);
        var iMaxX = Mathf.Max(iStartX, iEndX);
        var iMinX = Mathf.Min(iStartX, iEndX);
        var iMaxY = Mathf.Max(iStartY, iEndY);
        var iMinY = Mathf.Min(iStartY, iEndY);

        if (iTestY == iStartY || iTestY == iEndY)
            return false;

        //calculate line coefficients
        var run = iEndX - iStartX;
        var rise = iEndY - iStartY;

        //Substitue ray emanating from test point if rise and run are non zero
        if (rise != 0 && run != 0)
        {
            float slope = (float)(rise / run);
            float b = iStartY - (slope * iStartX);
            float intX = (iTestY - b) / slope;
            if (intX > iMinX && intX < iMaxX && intX > iTestX)
                return true;
        }

        if (run == 0)
            if (iMaxX >= iTestX && iMaxY >= iTestY && iMinY <= iTestY)
                return true;

        if (rise == 0)
            if (iMaxY == iTestY && iTestX >= iMinX && iTestX <= iMaxX)
                return true;


        return intersection;
    }

    public static GameObject AddMeshToGameObject(GameObject obj, Mesh mesh, Material mat, bool shadows)
    {
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        var meshFilter = obj.AddComponent<MeshFilter>();
        meshRenderer.material = mat;
        meshFilter.mesh = mesh;
        meshRenderer.castShadows = shadows;

        return obj;
    }
}
