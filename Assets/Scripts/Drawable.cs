using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drawable : MonoBehaviour
{

    Texture2D t2D;
    RawImage rawImage;
    RectTransform rect;
    int width, height;

    // Start is called before the first frame update
    void Start()
    {
        rawImage = GetComponent<RawImage>();
        t2D = rawImage.texture as Texture2D;
        rect = GetComponent<RectTransform>();
        width = t2D.width;
        height = t2D.height;
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

                paintCircle(mousePos, 5, Color.red);
                t2D.Apply();
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

                paintCircle(mousePos, 5, Color.black);
                t2D.Apply();
            }
        }
    }

    void paintCircle(Vector2 center, int rad, Color color)
    {
        for (int y = -rad + 1; y < rad; y++)
        {
            for (int x = -rad + 1; x < rad; x++)
            {
                t2D.SetPixel((int)(center.x + x), (int)(center.y + y), color);
            }
        }
    }

    public void ResetToBlack()
    {
        var resetColor = new Color32(0, 0, 0, 255);
        var resetArray = new Color32[width*height];
        for (int i = 0; i < resetArray.Length; i++)
        {
            resetArray[i] = resetColor;
        }
        t2D.SetPixels32(resetArray);
        t2D.Apply();
    }
}
