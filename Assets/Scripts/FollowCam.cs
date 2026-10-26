using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;
    public float upperBound;
    public float lowerBound;
    public float leftBound;
    public float rightBound;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = new Vector3(Mathf.Clamp(target.position.x,leftBound, rightBound), 
            Mathf.Clamp(target.position.y, lowerBound, upperBound), transform.position.z);
    }
}
