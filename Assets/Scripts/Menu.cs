using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Play menu
    // ------------------------------------------------------------------------

    //
    // Play
    //
    public void onPlay()
    {
        Debug.Log("--- MenuPlay:onPlay() !");
    }

    //
    // Pause
    //
    public void onPause()
    {
        Debug.Log("--- MenuPlay:onPause() !");
    }

    //
    // Stop
    //
    public void onStop()
    {
        Debug.Log("--- MenuPlay:onStop() !");
    }

    //
    // History
    //
    public void onHistory()
    {
        Debug.Log("--- MenuPlay:onHistory() !");
    }

    // ------------------------------------------------------------------------
    // Settings menu
    // ------------------------------------------------------------------------

    //
    // onCameraIndexTextChanged
    // 
    public void onCameraIndexTextChanged(string value)
    {
        Debug.Log("--- MenuSettings:onCameraIndexTextChanged: " + value);
        transform.Find("/MainCamera").gameObject.GetComponent<Shot>().cameraIndex = int.Parse(value);
    }

    //
    // onCameraIndexSliderChanged
    // 
    public void onCameraIndexSliderChanged(Slider sl)
    {
        Debug.Log("--- MenuSettings:onCameraIndexSliderChanged: " + sl.value);

        transform.Find("/UICamera/UICanvas/MenuSettings/SliderField/tiCameraIndex").gameObject.GetComponent<InputField>().text = sl.value.ToString();
        onCameraIndexTextChanged(sl.value.ToString());
    }


    //
    // onModeChanged
    // 
    public void onModeChanged(int value)
    {
        Debug.Log("--- MenuSettings:onModeChanged()");
    }

    //
    // Test button pressed
    // 
    public void onTest()
    {
        Debug.Log("--- MenuSettings:TEST !");

        // GameObject go = (GameObject)transform.Find("/UICamera/UICanvas/MenuPlay/Console/ScrollArea/Text").gameObject;
        // Text console = (Text)transform.Find("/UICamera/UICanvas/MenuPlay/Console/ScrollArea/Text").gameObject.GetComponent<Text>();
        transform.Find("/UICamera/UICanvas/MenuPlay/Console/ScrollArea/Text").gameObject.GetComponent<Text>().text += "\nQQ";
    }
}
