using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drawable : MonoBehaviour
{
    
    public int rad = 2;

    Texture2D[] Textures { get; set; }
    RawImage rawImage;
    RectTransform rect;
    int width, height;
    int layers = 10;
    int currentLayer;
    GameObject[] previews;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        width = (int)rect.rect.width;
        height = (int)rect.rect.height;

        rawImage = GetComponent<RawImage>();
        Textures = new Texture2D[layers];
        for (int i = 0; i < layers; i++)
        {
            Textures[i] = new Texture2D(width, height);
            Textures[i].filterMode = FilterMode.Point;
            ResetToBlack(i);
        }
        currentLayer = 0;
        rawImage.texture = Textures[currentLayer];

        previews = new GameObject[layers];
        for (int i = 0; i < layers; i++)
        {
            previews[i] = GameObject.Find("Preview" + i);
            RawImage image = previews[i].GetComponent<RawImage>();
            image.texture = Textures[i];
            image.texture.filterMode = FilterMode.Point;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, Camera.main, out mousePos);
            mousePos.x = mousePos.x + (width / 2) + 0.5f;
            mousePos.y = mousePos.y + (height / 2) + 0.5f;

            if (mousePos.x < width && mousePos.x >= 0 && mousePos.y < height && mousePos.y >= 0)
            {
                //print(mousePos.x + " " + mousePos.y);

                paintCircle(mousePos, Color.red);
            }
        }
        else if (Input.GetMouseButton(1))
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, Camera.main, out mousePos);
            mousePos.x = mousePos.x + (width / 2) + 0.5f;
            mousePos.y = mousePos.y + (height / 2) + 0.5f;

            if (mousePos.x < width && mousePos.x >= 0 && mousePos.y < height && mousePos.y >= 0)
            {
                //print(mousePos.x + " " + mousePos.y);

                paintCircle(mousePos, Color.black);

            }
        }
    }

    void paintCircle(Vector2 center, Color color)
    {
        for (int y = -rad + 1; y < rad; y++)
        {
            for (int x = -rad + 1; x < rad; x++)
            {
                if (y * y + x * x < rad * rad && center.x + x < Textures[0].width - 1 && center.x + x > 1 && center.y + y < Textures[0].height - 1 && center.y + y > 1)
                    Textures[currentLayer].SetPixel((int)(center.x + x), (int)(center.y + y), color);
            }
        }
        Textures[currentLayer].Apply();
    }

    public void ResetCurrentToBlack()
    {
        ResetToBlack(currentLayer);
    }
    public void ResetToBlack(int layer)
    {
        var resetColor = new Color32(0, 0, 0, 255);
        var resetArray = new Color32[width * height];
        for (int i = 0; i < resetArray.Length; i++)
        {
            resetArray[i] = resetColor;
        }
        Textures[layer].SetPixels32(resetArray);
        Textures[layer].Apply();
    }

    public void ChangeLayer(System.Single layer)
    {
        currentLayer = (int)layer;
        rawImage.texture = Textures[currentLayer];
    }

    public Texture2D[] GetTextures()
    {
        return Textures;
    }

    public void ChangeThickness(Toggle t)
    {
        rad = t.isOn ? 1 : 2;
    }
}
