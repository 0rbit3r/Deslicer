using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Drawable Drawable;

    public float StrechCoefficient = 10f;

    Mesh mesh;
    List<Vector3> Vertices;
    List<int> Triangles;


    //System.Random r = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        GameObject o = GameObject.Find("DrawingSurface");
        Drawable = o.GetComponent<Drawable>();

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateShape()
    {
        Texture2D[] textures = Drawable.GetTextures();

        Layer[] layers = new Layer[textures.Length];


        for (int i = 0; i < textures.Length; i++)
        {
            layers[i] = new Layer(textures[i], Vertices, (double)textures[0].width / (double)textures.Length * (double)i);
        }

        Vertices = new List<Vector3>();
        Triangles = new List<int>();

        foreach (var layer in layers)
        {
            layer.GetVertices(Vertices);
        }


        

        UpdateMesh();


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

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();
    }
}
