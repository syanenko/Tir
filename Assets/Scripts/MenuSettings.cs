using UnityEngine;
using UnityEngine.UI;

public class MenuSettings : MonoBehaviour
{
    void Update ()
    {
		
	}

    public void onModeChanged(int value)
    {
        Debug.Log("--- MenuSettings:onModeChanged()");
    }


    public void onTest()
    {
        Debug.Log("--- MenuSettings:TEST !");

        // GameObject go = (GameObject)transform.Find("/UICamera/UICanvas/MenuPlay/Console/ScrollArea/Text").gameObject;
        // Text console = (Text)transform.Find("/UICamera/UICanvas/MenuPlay/Console/ScrollArea/Text").gameObject.GetComponent<Text>();
        transform.Find("/UICamera/UICanvas/MenuPlay/Console/ScrollArea/Text").gameObject.GetComponent<Text>().text += "\nQQ";
    }
}
