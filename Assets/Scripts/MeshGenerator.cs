using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Texture2D[] Textures;

    public float StrechCoefficient = 10f;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    public int Accuracy = 100;


    //System.Random r = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        GameObject o = GameObject.Find("DrawingSurface");
        Drawable d = o.GetComponent<Drawable>();
        Textures = d.GetTextures();

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateShape()
    {
        //DrawingSurface.LockDrawing();


        for (int layer = 0; layer < Textures.Length; layer++)
        {
            GenerateVertices(Textures[layer], (int)(layer * StrechCoefficient));
        }

        //vertices = new Vector3[] {
        //new Vector3(0,0,0),
        //new Vector3(0,0,1),
        //new Vector3(1,0,0),
        //};

        //triangles = new int[]
        //{
        //    0,1,2
        //};

    }

    private void GenerateVertices(Texture2D t, int layer)
    {
         LayersOfComponents;
        var components = GetConnectedComponents(t);
        foreach (var component in components)
        {
            List<Vector3> LayerVertices = getVerticesFromShape(component, layer);
            List<List<Vector3>> components = null;
        }
        
        //Pairs of these components based on normalized on normalized ?omparisons based on ...
    }

    private List<bool[,]> GetConnectedComponents(Texture2D t)
    {
        List<bool[,]> toReturn = new List<bool[,]>();


        bool[,] usedFlags = new bool[t.width, t.height];

        for (int y = 0; y < t.height; y++)
        {
            for (int x = 0; x < t.width; x++)
            {
                if (!usedFlags[x, y] && t.GetPixel(x, y).r == 1f)//Possible choke because of GetPixel?
                {
                    toReturn.Add(IsolateComponent(t, x, y, usedFlags));
                }
                usedFlags[x, y] = true;
            }
        }

        return toReturn;
    }

    private bool[,] IsolateComponent(Texture2D t, int x, int y, bool[,] usedFlags)
    {
        usedFlags[x, y] = true;
        var toReturn = new bool[t.width, t.height];
        toReturn[x, y] = true;
        var queue = new Queue<(int, int)>();
        queue.Enqueue((x, y));

        //Color c = new Color((float)(0.4 + r.NextDouble() / 2), (float)(0.4 + r.NextDouble() / 2), (float)(0.4 + r.NextDouble() / 2));
        //t.SetPixel(x, y, c);

        while (queue.Count != 0)
        {
            (x, y) = queue.Dequeue();
            //t.SetPixel(x, y, c);
            for (int dY = -1; dY <= 1; dY++)
            {
                for (int dX = -1; dX <= 1; dX++)
                {
                    if (!(dY == 0 && dX == 0) && x + dX < t.width && x + dX > -1 && y + dY < t.height && y + dY > -1)
                    {
                        if (!usedFlags[x + dX, y + dY] && t.GetPixel(x + dX, y + dY).r == 1f)
                        {
                            toReturn[x + dX, y + dY] = true;
                            usedFlags[x + dX, y + dY] = true;
                            queue.Enqueue((x + dX, y + dY));
                        }
                    }
                }
            }
        }

        //t.Apply();
        return toReturn;
    }

    private List<Vector3> getVerticesFromShape(bool[,] component, int layer)
    {
        //GetBorder(component, layer);

        var points = GetOrderedPoints(component);

        if(points == null)
        {
            return null;
        }
        points = DeleteRedundant(points);

        DebugShowPoints(points);

        return null;
    }

    private List<(int, int)> GetOrderedPoints(bool[,] component)
    {
        List<(int, int)> pointList = new List<(int, int)>();

        bool[,] enlarged = EnlargeComponent(component);

        for (int sy = 0; sy < enlarged.GetLength(1); sy++)
        {
            for (int sx = 0; sx < enlarged.GetLength(0); sx++)
            {
                if (enlarged[sx, sy] && IsOnEdge(enlarged, sx, sy) && !pointList.Contains((sx / 3, sy / 3))) //!!! Later needed to test connectedness (holes are in same list)
                {
                    int x = sx, y = sy;

                    int startX = x;
                    int startY = y;

                    pointList.Add((x / 3, y / 3));

                    (x, y) = TakeStepClockwise(enlarged, x, y);

                    int loop = 0;
                    while ((x != startX || y != startY) && loop < 100000)
                    {
                        if (pointList.Last() != (x / 3, y / 3))
                        {
                            pointList.Add((x / 3, y / 3));
                        }
                        (x, y) = TakeStepClockwise(enlarged, x, y);

                        if (pointList.Count > component.Length)
                        {
                            return null;
                        }
                    }
                }
            }
        }

        return pointList;
    }

    (int, int) TakeStepClockwise(bool[,] component, int x, int y)
    {
        //*
        if (x + 1 < component.GetLength(0) && y + 1 < component.GetLength(1) && !component[x + 1, y] && component[x + 1, y + 1])
        {
            return (x + 1, y + 1);
        }
        else if (x + 1 < component.GetLength(0) && y + 1 < component.GetLength(1) && !component[x + 1, y + 1] && component[x, y + 1])
        {
            return (x, y + 1);
        }
        else if (x - 1 >= 0 && y + 1 < component.GetLength(1) && !component[x, y + 1] && component[x - 1, y + 1])
        {
            return (x - 1, y + 1);
        }
        else if (x - 1 >= 0 && y + 1 < component.GetLength(1) && !component[x - 1, y + 1] && component[x - 1, y])
        {
            return (x - 1, y);
        }
        else if (x - 1 >= 0 && y - 1 >= 0 && !component[x - 1, y] && component[x - 1, y - 1])
        {
            return (x - 1, y - 1);
        }
        else if (x - 1 >= 0 && y - 1 >= 0 && !component[x - 1, y - 1] && component[x, y - 1])
        {
            return (x, y - 1);
        }
        else if (x + 1 < component.GetLength(0) && y - 1 >= 0 && !component[x, y - 1] && component[x + 1, y - 1])
        {
            return (x + 1, y - 1);
        }
        else if (x + 1 < component.GetLength(0) && y - 1 >= 0 && !component[x + 1, y - 1] && component[x + 1, y])
        {
            return (x + 1, y);
        }
        //*/

        throw new Exception("Couldn't find next vertex.");
    }

    private List<(int, int)> DeleteRedundant(List<(int, int)> points)
    {
        var toReturn = new List<(int, int)>();
        var pointsArr = points.ToArray();

        int curr = 0;
        toReturn.Add(pointsArr[curr]);

        for (int i = 1; i < pointsArr.Length; i++)
        {
            if (AreNeighbors(pointsArr[i - 1], pointsArr[i]))
            {
                double accuracy = GetAccuracy(pointsArr, curr, i);
                if (accuracy > Accuracy)
                {
                    toReturn.Add(pointsArr[i - 1]);
                    curr = i - 1;
                }
            }
            else
            {
                curr = i;
            }
        }

        return toReturn;
    }

    private double GetAccuracy((int, int)[] pointsArr, int curr, int last)
    {
        if(curr + 1 == last)
        {
            return 1000d; 
        }
        double sum = 0;
        double x1 = pointsArr[curr].Item1;
        double x2 = pointsArr[last].Item1;
        double y1 = pointsArr[curr].Item2;
        double y2 = pointsArr[last].Item2;
        for (int i = curr + 1; i < last; i++)
        {
            sum += Math.Abs((x2 - x1) * ((double)pointsArr[i].Item2 - y2) + (y1 - y2) * ((double)pointsArr[i].Item1 - x2)); ;
        }

        return sum;
    }

    private (int, int) FindFirstTopPixel(bool[,] component)
    {
        for (int y = 0; y < component.GetLength(1); y++)
        {
            for (int x = 0; x < component.GetLength(0); x++)
            {
                if (component[x, y])
                {
                    return (x, y);
                }
            }
        }

        return (-1, -1);
    }

    private void GetBorder(bool[,] component)
    {
        bool[,] newComponent = new bool[component.GetLength(0), component.GetLength(1)];

        for (int y = 1; y < component.GetLength(1) - 1; y++)
        {
            for (int x = 1; x < component.GetLength(0) - 1; x++)
            {
                if (component[x, y] && IsOnEdge(component, x, y))
                {
                    newComponent[x, y] = true;
                    //Textures[0].SetPixel(x, y, Color.white);
                }
                else
                {
                    newComponent[x, y] = false;
                }
            }
        }

        //Textures[0].Apply();
    }

    bool IsOnEdge(bool[,] component, int x, int y)
    {
        int falses = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (x + dx >= 0 && y + dy >= 0 && x + dx < component.GetLength(0) && y + dy < component.GetLength(1) && !component[x + dx, y + dy])
                {
                    falses++;
                    if (falses >= 2)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    bool[,] EnlargeComponent(bool[,] component)
    {
        bool[,] toReturn = new bool[component.GetLength(0) * 3, component.GetLength(1) * 3];

        for (int y = 0; y < toReturn.GetLength(1); y++)
        {
            for (int x = 0; x < toReturn.GetLength(0); x++)
            {
                toReturn[x, y] = component[x / 3, y / 3];
            }
        }

        return toReturn;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    bool AreNeighbors((int, int) first, (int, int) second)
    {
        if (Math.Abs(first.Item1 - second.Item1) <= 1 && Math.Abs(first.Item2 - second.Item2) <= 1)
        {
            return true;
        }
        return false;
    }

    private void DebugShowPoints(List<(int, int)> points)
    {
        foreach (var point in points)
        {
            Textures[0].SetPixel(point.Item1, point.Item2, Color.white);
        }
        Textures[0].Apply();
    }
}
