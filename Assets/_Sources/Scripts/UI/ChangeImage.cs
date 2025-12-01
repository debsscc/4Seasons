using UnityEngine;
using UnityEngine.UI;

public class ChangeImage : MonoBehaviour
{
    public Image oldImage;
    public Sprite newImage;
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void ChangeTheImage()
    {
        oldImage.sprite = newImage;
    }
}
