using UnityEngine;

public class HandFollower : MonoBehaviour
{
    public Vector2 offset = Vector2.zero;
    private RectTransform bounds;

    private RectTransform draggableRect;
    private Transform hand;
    private bool isDragging = false;
    private DraggablePrefab draggable;

    private void Start()
    {
        draggable = GetComponent<DraggablePrefab>();
        draggableRect = GetComponent<RectTransform>();

        if (draggable != null)
        {
            draggable.OnBeginDragEvent += () => isDragging = true;
            draggable.OnEndDragEvent += () => isDragging = false;
        }

        GameObject handObj = GameObject.FindWithTag("HandSprite");
        if (handObj != null)
            hand = handObj.transform;
        else
            Debug.LogWarning("[HandFollower] HandSprite não encontrado.");

        GameObject boundsObj = GameObject.FindWithTag("DragBounds");
        if (boundsObj != null)
            bounds = boundsObj.GetComponent<RectTransform>();
        else
            Debug.LogWarning("[HandFollower] DragBounds não encontrado.");
    }

    private void Update()
    {
        if (!isDragging || hand == null || draggableRect == null) return;

        if (bounds != null)
        {
            Vector3[] corners = new Vector3[4];
            bounds.GetWorldCorners(corners);

            Vector3 clampedPos = draggableRect.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, corners[0].x, corners[2].x);
            clampedPos.y = Mathf.Clamp(clampedPos.y, corners[0].y, corners[2].y);
            draggableRect.position = clampedPos;
        }

        hand.position = (Vector2)draggableRect.position + offset;
    }
}
