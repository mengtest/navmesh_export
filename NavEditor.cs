using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NavEditor : UnityEditor.EditorWindow
{
    [UnityEditor.MenuItem("Tools/gen mesh json")]
    static void show()
    {
        UnityEditor.EditorWindow.GetWindow<NavEditor>().Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("gen navmesh JSON"))
        {
            string outstring = GenNavMesh(0);
            string outfile = Application.dataPath + "\\navinfo.json";
            System.IO.File.WriteAllText(outfile, outstring);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="style">0 json 1 obj</param>
    /// <returns></returns>
    string GenNavMesh(int style)
    {
        NavMeshTriangulation navtri = UnityEngine.NavMesh.CalculateTriangulation();

        Dictionary<int, int> indexmap = new Dictionary<int, int>();
        List<Vector3> repos = new List<Vector3>();
        for (int i = 0; i < navtri.vertices.Length; i++)
        {
            int ito = -1;
            for (int j = 0; j < repos.Count; j++)
            {
                if (Vector3.Distance(navtri.vertices[i], repos[j]) < 0.01)
                {
                    ito = j;
                    break;
                }
            }
            if (ito < 0)
            {
                indexmap[i] = repos.Count;
                repos.Add(navtri.vertices[i]);
            }
            else
            {
                indexmap[i] = ito;
            }
        }

        //关系是 index 公用的三角形表示他们共同组成多边形
        //多边形之间的连接用顶点位置识别
		List<int> polylast = new List<int>();
		List<int> index = new List<int> ();
		List<int[]> polys = new List<int[]>();
		for (int i = 0; i < navtri.indices.Length / 3; i++)
		{
			int i0 = navtri.indices[i * 3 + 0];
			int i1 = navtri.indices[i * 3 + 1];
			int i2 = navtri.indices[i * 3 + 2];

			
			if (polylast.Contains(i0) || polylast.Contains(i1) || polylast.Contains(i2))
			{
				index.Add(navtri.areas[i]);
				if (polylast.Contains(i0) == false)
					polylast.Add(i0);
				if (polylast.Contains(i1) == false)
					polylast.Add(i1);
				if (polylast.Contains(i2) == false)
					polylast.Add(i2);
			}
			else
			{
				if (polylast.Count > 0)
				{
					polylast.Insert(0,polylast.Count);
					foreach(int id in index)
						polylast.Add(id);
					polys.Add(polylast.ToArray());
					index.Clear();
				}
				polylast.Clear();
				polylast.Add(i0);
				polylast.Add(i1);
				polylast.Add(i2);
				index.Add(navtri.areas[i]);
			}
		}
		if (polylast.Count > 0)
		{
			polylast.Insert(0,polylast.Count);
			foreach(int id in index)
				polylast.Add(id);
			polys.Add(polylast.ToArray());
		}

        string outnav = "";

        outnav = "{\"v\":[\n";
        for (int i = 0; i < repos.Count; i++)
        {
            if (i > 0)
                outnav += ",\n";

            outnav += "[" + repos[i].x + "," + repos[i].y + "," + repos[i].z + "]";
        }
        outnav += "\n],\"p\":[\n";

        for (int i = 0; i < polys.Count; i++)
        {
            string outs = polys[i][0].ToString();
            for (int j = 1; j <= polys[i][0]; j++)
            {
                outs += "," + indexmap[polys[i][j]];
            }

            for (int j = polys[i][0] + 1; j < polys[i].Length; j++)
            {
                outs += "," + polys[i][j].ToString();
            }

            if (i > 0)
                outnav += ",\n";

            outnav += "[" + outs + "]";
        }

        outnav += "]}";

        return outnav;
    }
}
