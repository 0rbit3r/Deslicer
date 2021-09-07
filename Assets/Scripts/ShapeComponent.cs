using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    class ShapeComponent
    {

        Layer Layer;

        List<Vector3> Vertices;

        public int startIndex;

        public int[] HolesIndexes;

        int VerticesNum { get; set; }

        public (int, int, int, int) BoundingBox { get; private set; }

        public (int, int) Center { get; private set; }

        List<ShapeComponent> Subcomponents;

        List<(int, int)> Border;

        List<List<(int, int)>> Holes;

        Drawable Drawable;

        public readonly double MaxLineError = 5d;

        bool Debug = true;
        int DebugId;

        static System.Random r = new System.Random();

        public ShapeComponent(bool[,] bitMap, Layer layer)
        {
            Layer = layer;
            Vertices = layer.Vertices;

            Border = new List<(int, int)>();
            Holes = new List<List<(int, int)>>();
            Subcomponents = new List<ShapeComponent>();


            GameObject o = GameObject.Find("DrawingSurface");
            Drawable = o.GetComponent<Drawable>();

            DebugId = r.Next(0, 100);

            GenerateComponent(bitMap);
        }

        private void GenerateComponent(bool[,] componentMap)
        {
            //GetBorder(component, layer);

            var edges = GetOrderedEdges(componentMap);


            if (edges == null)
            {
                throw new Exception("Ayy lmao");
            }
            GetBorderAndHoles(edges);

            GenerateComponentProperties();

            VerticesNum = Border.Count;
            foreach (var hole in Holes)
            {
                VerticesNum += hole.Count;
            }

            if (Debug)
                DebugShowPoints();
        }

        private List<(int, int)> GetOrderedEdges(bool[,] component)
        {
            List<(int, int)> pointList = new List<(int, int)>();

            bool[,] enlargedBitmap = EnlargeComponent(component);

            for (int sy = 0; sy < enlargedBitmap.GetLength(1); sy++)
            {
                for (int sx = 0; sx < enlargedBitmap.GetLength(0); sx++)
                {
                    if (enlargedBitmap[sx, sy] && IsOnEdge(enlargedBitmap, sx, sy) && !pointList.Contains((sx / 3, sy / 3))) //!!! Later needed to test connectedness (holes are in same list) | On it. | Still on it.
                    {
                        int x = sx, y = sy;

                        int startX = x;
                        int startY = y;

                        pointList.Add((x / 3, y / 3));

                        (x, y) = TakeStepClockwise(enlargedBitmap, x, y);

                        int loop = 0;
                        while ((x != startX || y != startY) && loop++ < 100000)
                        {
                            if (pointList.Last() != (x / 3, y / 3))
                            {
                                pointList.Add((x / 3, y / 3));
                            }
                            (x, y) = TakeStepClockwise(enlargedBitmap, x, y);

                            if (pointList.Count > component.Length)
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            pointList.RemoveAt(pointList.Count - 1);
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

        private void GetBorderAndHoles(List<(int, int)> edges)
        {
            var currentEdge = Border;
            var pointsArr = edges.ToArray();

            int curr = 0;
            currentEdge.Add(pointsArr[curr]);

            for (int i = 2; i < pointsArr.Length; i++)
            {
                if (AreNeighbors(pointsArr[i - 1], pointsArr[i]))
                {
                    double lineError = GetAccuracy(pointsArr, curr, i);

                    if (lineError > MaxLineError)
                    {
                        currentEdge.Add(edges[i - 1]);
                        curr = i - 1;
                    }
                }
                else
                {
                    curr = i;

                    if (currentEdge.Count < 3)
                    {
                        Holes.RemoveAt(Holes.Count - 1);
                    }

                    var hole = new List<(int, int)>();
                    Holes.Add(hole);

                    currentEdge = hole;
                }
            }
            if (currentEdge.Count < 3)
            {
                if (Holes.Count > 0)
                {
                    Holes.RemoveAt(Holes.Count - 1);
                }
            }
        }

        private double GetAccuracy((int, int)[] pointsArr, int curr, int last)
        {
            if (curr + 1 == last)
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


        bool AreNeighbors((int, int) first, (int, int) second)
        {
            if (Math.Abs(first.Item1 - second.Item1) <= 1 && Math.Abs(first.Item2 - second.Item2) <= 1)
            {
                return true;
            }
            return false;
        }

        private void GenerateComponentProperties()
        {
            int lowestX = int.MaxValue, highestX = 0, lowestY = int.MaxValue, highestY = 0;
            foreach (var point in Border)
            {
                lowestX = point.Item1 < lowestX ? point.Item1 : lowestX;
                highestX = point.Item1 > highestX ? point.Item1 : highestX;

                lowestY = point.Item2 < lowestY ? point.Item2 : lowestY;
                highestY = point.Item2 > highestY ? point.Item2 : highestY;
            }
            BoundingBox = (lowestX, highestX, lowestY, highestY);
            Center = (lowestX + (highestX - lowestX) / 2, lowestY + (highestY - lowestY) / 2);
        }

        public void DebugShowPoints()
        {
            Texture2D[] textures = Drawable.GetTextures();
            foreach (var point in Border)
            {
                textures[0].SetPixel(point.Item1, point.Item2, Color.white);
            }
            foreach (var hole in Holes)
            {
                foreach (var point in hole)
                {
                    textures[0].SetPixel(point.Item1, point.Item2, new Color(1, 1, 0));
                }
            }
            textures[0].Apply();
        }

        public void AddSubcomponent(ShapeComponent NewComponent)
        {
            foreach (var component in Subcomponents)
            {
                if (NewComponent == component)
                {
                    return;
                }
            }

            foreach (var subcomponent in Subcomponents)
            {
                if (subcomponent.Contains(NewComponent))
                {
                    subcomponent.AddSubcomponent(NewComponent);
                    VerticesNum += NewComponent.VerticesNum;
                    return;
                }
                if (NewComponent.Contains(subcomponent))
                {
                    Subcomponents.Remove(subcomponent);
                    NewComponent.AddSubcomponent(subcomponent);
                }
            }
            VerticesNum += NewComponent.VerticesNum;
            Subcomponents.Add(NewComponent);
        }

        public bool Contains(ShapeComponent c2)
        {
            if (c2 == this)
            {
                return false;
            }

            (int c1x1, int c1x2, int c1y1, int c1y2) = BoundingBox;
            (int c2x1, int c2x2, int c2y1, int c2y2) = c2.BoundingBox;

            if (c1x1 < c2x1 && c2x2 < c1x2 && c1y1 < c2y1 && c2y2 < c1y2)
            {
                return true;
            }
            return false;
        }

        public void AddVertices(List<Vector3> vertices, double zValue)
        {
            HolesIndexes = new int[Holes.Count];
            startIndex = vertices.Count;

            foreach (var pixel in Border)
            {
                vertices.Add(new Vector3(pixel.Item1, pixel.Item2, (float)zValue));
            }
            if (Holes.Count > 0)
            {
                HolesIndexes[0] = startIndex + Border.Count;

                int currHoleIndex = 1;

                foreach (var hole in Holes)
                {
                    foreach (var pixel in hole)
                    {
                        vertices.Add(new Vector3(pixel.Item1, pixel.Item2, (float)zValue));
                    }
                    if (currHoleIndex < Holes.Count)
                    {
                        HolesIndexes[currHoleIndex] = startIndex + Border.Count + Holes[currHoleIndex - 1].Count;
                        currHoleIndex++;
                    }
                }
            }
            foreach (var component in Subcomponents)
            {
                component.AddVertices(vertices, zValue);
            }
        }

    }
}
