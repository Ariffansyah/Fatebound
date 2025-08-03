using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;

    public void Move(float deltaX, float deltaY)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= deltaX * parallaxFactor;
        newPos.y -= deltaY * parallaxFactor;
        transform.localPosition = newPos;
    }

}
