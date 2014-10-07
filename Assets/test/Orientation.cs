using UnityEngine;
using System.Collections;

public class Orientation : MonoBehaviour
{
    [SerializeField]
    Transform
        phoneAvatar;
    [SerializeField]
    Transform
        orientationFixer;
    [SerializeField]
    Transform cameraAvatar;
    bool isRectified;
    public Transform target;
    // Use this for initialization
    void Start()
    {
        isRectified = false;
        string a="TiltingFixer/OrientationFixer";

        orientationFixer=GameObject.Find(a).transform;//
        phoneAvatar=GameObject.Find(a+"/phoneAvatar").transform;//
        cameraAvatar=GameObject.Find(a+"/phoneAvatar/CameraHolder/CameraAvater").transform;

    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Rectify();
        }
        if(isRectified&&target!=null)
            target.rotation=cameraAvatar.rotation;
    }
    
    public void Rectify()
    {
        Quaternion childRotation = phoneAvatar.localRotation;
        Quaternion inv = Quaternion.Inverse(childRotation);
        Quaternion fix = inv;
        Quaternion lr = fix;
        orientationFixer.localRotation = lr;
        isRectified = true;
    }
    
}









