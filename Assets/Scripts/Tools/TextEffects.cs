using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TextEffects : MonoBehaviour
{
    private StringBuilder mStringBuilder;
    string str;
    Text tex;
    int mSpeed = 0;   //调整这个可以调整出现的速度
    int timeSpeed = 0;   
    int index = 0;
    bool ison = true;
    private void Start()
    {
        tex = GetComponent<Text>();
        mStringBuilder = new StringBuilder();
        SetText("strin_str涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨涨",15);
    }

    public void SetText(string _str,int _speed = 15)
    {
        str = _str;
        mStringBuilder.Clear();
        tex.text = mStringBuilder.ToString();
        mSpeed = _speed;
        timeSpeed = mSpeed;
        index = 0;
        ison = true;
    }
    private void Update()
    {
        if (ison)
        {
            timeSpeed -= 1;
            if (timeSpeed <= 0)
            {
                if (index >= str.Length)
                {
                    ison = false;
                    return;
                }
                mStringBuilder.Append(str[index].ToString());
                tex.text = mStringBuilder.ToString();
                index += 1;
                timeSpeed = mSpeed;
            }
        }

    }
}