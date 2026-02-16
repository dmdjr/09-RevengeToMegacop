
using UnityEngine;

public static class MousePositionGetter
{
    public static Vector3 GetMousePositionInWorld(Vector3 target)
    {
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, target.y, 0));

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            hitPoint.y = target.y;

            return hitPoint;
        }

        return Vector3.zero;
    }
}