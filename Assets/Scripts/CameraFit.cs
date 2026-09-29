using UnityEngine;
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    public float height=34,width=32;
    void LateUpdate() { var c=GetComponent<Camera>(); c.orthographicSize=Mathf.Max(height*.5f,width/(2*c.aspect)); }
}
