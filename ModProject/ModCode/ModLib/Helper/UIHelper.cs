using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class UIHelper
{
    public static IDictionary<string, UIBase> GetCurUI()
    {
        var curUI = new Dictionary<string, UIBase>();
        foreach (var x in g.ui.allUI)
        {
            foreach (var y in x.Value)
            {
                curUI.Add(y.name, y);
            }
        }
        return curUI;
    }

    //public static GameObject CreateButton()
    //{
    //    var go = new GameObject();
    //    var btn = g.root.GetComponent<GameObject>().AddComponent<Button>();
    //    var text = go.AddComponent<Text>();

    //    go.SetActive(true);
    //    go.transform.SetParent(g.root.transform);
    //    go.transform.position = new Vector3(0, 0);
    //    go.transform.localScale = new Vector3(1, 1);
    //    go.layer = UILayer.FullEffect.ToString().Parse<int>();

    //    btn.transform.SetParent(go.transform);
    //    btn.transform.position = new Vector3(0, 0);
    //    btn.transform.localScale = new Vector3(1, 1);

    //    text.transform.SetParent(btn.transform);
    //    text.transform.position = new Vector3(0, 0);
    //    text.transform.localScale = new Vector3(1, 1);
    //    text.text = "Hello";

    //    return go;
    //}
}