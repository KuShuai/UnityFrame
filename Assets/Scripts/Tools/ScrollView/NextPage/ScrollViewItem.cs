using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewItem : MonoBehaviour
{

    public string fileName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLoadFunc()
    {

    }

    public void Data(int page = -1, int index = -1 , int select = -1)
    {
        //text.text = page+"  "+index+" "+select;
        if (index == -1)
        {
            return;
        }
        //GetComponent<RectTransform>().anchoredPosition = new Vector2((page % 4) / 2 * 951.5f, -((page % 4) % 2) * 531.5f);
        FileStream fileStream = new FileStream(Application.streamingAssetsPath + "\\"+ fileName + "\\" + (index+1) +".png",FileMode.Open,FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        byte[] bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        
        Texture2D texture2D = new Texture2D(1905, 940);
        texture2D.LoadImage(bytes);
        GetComponent<RawImage>().texture = texture2D;

    }
}
