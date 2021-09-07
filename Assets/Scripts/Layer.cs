using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Assets.Scripts
{
    class Layer
    {
        double LayerPosition;

        public List<Vector3> Vertices;

        public List<ShapeComponent> Components { get; private set; }

        Dictionary<ShapeComponent, List<ShapeComponent>> Pairs;
        public Layer(Texture2D t, List<Vector3> vertices, double layerZPosition)
        {
            Vertices = vertices;
            LayerPosition = layerZPosition;
            Components = GenerateComponents(t);
            SortIntoSubcomponents();
        }

        public List<ShapeComponent> GenerateComponents(Texture2D t)
        {
            List<ShapeComponent> layerComponents = new List<ShapeComponent>();

            List<bool[,]> connectedBitmaps = GetConnectedComponentMaps(t);
            foreach (var bitmap in connectedBitmaps)
            {
                var component = new ShapeComponent(bitmap, this);
                if (component != null)
                {
                    layerComponents.Add(component);
                }

                //Drawable.DebugShowPoints(bitmap);
            }

            return layerComponents;
        }

        static List<bool[,]> GetConnectedComponentMaps(Texture2D t)
        {
            List<bool[,]> toReturn = new List<bool[,]>();


            bool[,] usedFlags = new bool[t.width, t.height];

            for (int y = 0; y < t.height; y++)
            {
                for (int x = 0; x < t.width; x++)
                {
                    if (!usedFlags[x, y] && t.GetPixel(x, y).r == 1f)//Possible choke because of GetPixel?
                    {
                        toReturn.Add(IsolateComponentMap(t, x, y, usedFlags));
                    }
                    usedFlags[x, y] = true;
                }
            }

            return toReturn;
        }

        static bool[,] IsolateComponentMap(Texture2D t, int x, int y, bool[,] usedFlags)
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

        void SortIntoSubcomponents()
        {
            var toRemove = new List<ShapeComponent>();
            foreach (var component1 in Components)
            {
                foreach (var component2 in Components)
                {
                    if (component1.Contains(component2))
                    {
                        toRemove.Add(component2);
                        component1.AddSubcomponent(component2);
                    }
                }
            }

            foreach (var c in toRemove)
            {
                Components.Remove(c);
            }

        }

        public void GetVertices(List<Vector3> vertices)
        {
            foreach (var component in Components)
            {
                component.AddVertices(vertices, LayerPosition);
            }
        }

        public void PairComponents(Layer secondLayer)
        {

        }
    }
}

