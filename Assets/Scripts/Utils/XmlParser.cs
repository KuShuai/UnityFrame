using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class XmlParser
{

    public static string GetAttribString(XElement element, string attribName)
    {
        try
        {
            XAttribute attrib = element.Attribute(attribName);
            if (attrib == null)
            {
                Debug.LogErrorFormat("��ȡxml attribʧ�� û�ж�Ӧ��attrib {0}", attribName);
                return "";
            }
            return (string)attrib;
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("��ȡxml attribʧ�� {0}", e.ToString());
        }

        return null;
    }
}
