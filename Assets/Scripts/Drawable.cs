using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drawable : MonoBehaviour
{

    Texture2D[] textures;
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
        textures = new Texture2D[layers];
        for (int i = 0; i < layers; i++)
        {
            textures[i] = new Texture2D(width, height);
            textures[i].filterMode = FilterMode.Point;
            ResetToBlack(i);
        }
        currentLayer = 0;
        rawImage.texture = textures[currentLayer];

        previews = new GameObject[layers];
        for (int i = 0; i < layers; i++)
        {
            previews[i] = GameObject.Find("Preview" + i);
            RawImage image = previews[i].GetComponent<RawImage>();
            image.texture = textures[i];
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
            mousePos.x = mousePos.x + (width / 2);
            mousePos.y = mousePos.y + (height / 2);

            if (mousePos.x < width && mousePos.x >= 0 && mousePos.y < height && mousePos.y >= 0)
            {
                //print(mousePos.x + " " + mousePos.y);

                paintCircle(mousePos, 2, Color.red);
            }
        }
        else if (Input.GetMouseButton(1))
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, Camera.main, out mousePos);
            mousePos.x = mousePos.x + (width / 2);
            mousePos.y = mousePos.y + (height / 2);

            if (mousePos.x < width && mousePos.x >= 0 && mousePos.y < height && mousePos.y >= 0)
            {
                //print(mousePos.x + " " + mousePos.y);

                paintCircle(mousePos, 2, Color.black);
                
            }
        }
    }

    void paintCircle(Vector2 center, int rad, Color color)
    {
        for (int y = -rad + 1; y < rad; y++)
        {
            for (int x = -rad + 1; x < rad; x++)
            {
                if(y*y + x*x < rad*rad)
                textures[currentLayer].SetPixel((int)(center.x + x), (int)(center.y + y), color);
            }
        }
        textures[currentLayer].Apply();
    }

    public void ResetCurrentToBlack()
    {
        ResetToBlack(currentLayer);
    }
    public void ResetToBlack(int layer)
    {
        var resetColor = new Color32(0, 0, 0, 255);
        var resetArray = new Color32[width*height];
        for (int i = 0; i < resetArray.Length; i++)
        {
            resetArray[i] = resetColor;
        }
        textures[layer].SetPixels32(resetArray);
        textures[layer].Apply();
    }

    public void ChangeLayer(System.Single layer)
    {
        currentLayer = (int)layer;
        rawImage.texture = textures[currentLayer];
    }
}
