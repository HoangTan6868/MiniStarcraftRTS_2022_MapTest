
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LANSelectionManager : MonoBehaviour
{
    private readonly List<LANWarriorNetwork> selectedUnits =
        new List<LANWarriorNetwork>();

    private Camera mainCamera;
    private Vector2 dragStart;
    private bool isDragging;

    public float formationSpacing = 0.8f;
    public float dragThreshold = 12f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!NetworkReady())
        {
            selectedUnits.Clear();
            isDragging = false;
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        // Bat dau chon linh
        if (Input.GetMouseButtonDown(0))
        {
            dragStart = Input.mousePosition;
            isDragging = true;
        }

        // Hoan tat chon linh
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            Vector2 dragEnd = Input.mousePosition;

            bool shiftHeld =
                Input.GetKey(KeyCode.LeftShift) ||
                Input.GetKey(KeyCode.RightShift);

            if (Vector2.Distance(dragStart, dragEnd) >= dragThreshold)
            {
                SelectInRectangle(dragStart, dragEnd, shiftHeld);
            }
            else
            {
                SelectByClick(dragEnd, shiftHeld);
            }

            isDragging = false;

            Debug.Log("So linh da chon: " + selectedUnits.Count);
        }

        // Click phai de di chuyen
        if (Input.GetMouseButtonDown(1))
        {
            MoveSelectedUnits();
        }
    }

    bool NetworkReady()
    {
        NetworkManager manager = NetworkManager.Singleton;

        return manager != null &&
               manager.IsListening &&
               (manager.IsHost || manager.IsConnectedClient);
    }

    void SelectByClick(Vector2 screenPosition, bool shiftHeld)
    {
        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        Collider2D[] hits = Physics2D.OverlapPointAll(
            new Vector2(worldPosition.x, worldPosition.y)
        );

        LANWarriorNetwork clickedUnit = null;

        foreach (Collider2D hit in hits)
        {
            LANWarriorNetwork unit =
                hit.GetComponentInParent<LANWarriorNetwork>();

            if (unit != null && unit.CanLocalPlayerControl())
            {
                clickedUnit = unit;
                break;
            }
        }

        if (!shiftHeld)
        {
            // Click lai linh dang chon de bo chon
            if (clickedUnit != null &&
                selectedUnits.Count == 1 &&
                selectedUnits[0] == clickedUnit)
            {
                selectedUnits.Clear();
                return;
            }

            selectedUnits.Clear();
        }

        if (clickedUnit == null)
            return;

        if (selectedUnits.Contains(clickedUnit))
            selectedUnits.Remove(clickedUnit);
        else
            selectedUnits.Add(clickedUnit);
    }

    void SelectInRectangle(Vector2 start, Vector2 end, bool shiftHeld)
    {
        if (!shiftHeld)
            selectedUnits.Clear();

        Rect selectionRect = MakeRect(start, end);

        LANWarriorNetwork[] allUnits =
            FindObjectsOfType<LANWarriorNetwork>();

        foreach (LANWarriorNetwork unit in allUnits)
        {
            if (!unit.CanLocalPlayerControl())
                continue;

            Vector3 screenPosition =
                mainCamera.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.z > 0 &&
                selectionRect.Contains(screenPosition) &&
                !selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
            }
        }
    }

    void MoveSelectedUnits()
    {
        selectedUnits.RemoveAll(unit => unit == null);

        if (selectedUnits.Count == 0)
            return;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        int count = selectedUnits.Count;
        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / columns);

        for (int i = 0; i < count; i++)
        {
            LANWarriorNetwork unit = selectedUnits[i];

            if (!unit.CanLocalPlayerControl())
                continue;

            int column = i % columns;
            int row = i / columns;

            float offsetX =
                (column - (columns - 1) * 0.5f) * formationSpacing;

            float offsetY =
                ((rows - 1) * 0.5f - row) * formationSpacing;

            Vector3 target = new Vector3(
                worldPosition.x + offsetX,
                worldPosition.y + offsetY,
                unit.transform.position.z
            );

            unit.RequestMove(target);
        }
    }

    Rect MakeRect(Vector2 a, Vector2 b)
    {
        return Rect.MinMaxRect(
            Mathf.Min(a.x, b.x),
            Mathf.Min(a.y, b.y),
            Mathf.Max(a.x, b.x),
            Mathf.Max(a.y, b.y)
        );
    }

    void OnGUI()
    {
        if (!NetworkReady() || mainCamera == null)
            return;

        Color previousColor = GUI.color;

        // Vien vang quanh linh da chon
        GUI.color = Color.yellow;

        foreach (LANWarriorNetwork unit in selectedUnits)
        {
            if (unit == null || !unit.CanLocalPlayerControl())
                continue;

            SpriteRenderer sprite = unit.GetComponent<SpriteRenderer>();

            if (sprite == null)
                continue;

            Bounds bounds = sprite.bounds;

            Vector3 topLeft = mainCamera.WorldToScreenPoint(
                new Vector3(bounds.min.x, bounds.max.y, bounds.center.z)
            );

            Vector3 bottomRight = mainCamera.WorldToScreenPoint(
                new Vector3(bounds.max.x, bounds.min.y, bounds.center.z)
            );

            if (topLeft.z <= 0)
                continue;

            Rect rect = new Rect(
                Mathf.Min(topLeft.x, bottomRight.x) - 4f,
                Screen.height - Mathf.Max(topLeft.y, bottomRight.y) - 4f,
                Mathf.Abs(bottomRight.x - topLeft.x) + 8f,
                Mathf.Abs(topLeft.y - bottomRight.y) + 8f
            );

            DrawBorder(rect);
        }

        // Khung keo chon nhieu linh
        if (isDragging)
        {
            Vector2 current = Input.mousePosition;

            if (Vector2.Distance(dragStart, current) >= dragThreshold)
            {
                Rect screenRect = MakeRect(dragStart, current);

                Rect guiRect = new Rect(
                    screenRect.x,
                    Screen.height - screenRect.yMax,
                    screenRect.width,
                    screenRect.height
                );

                GUI.color = Color.cyan;
                DrawBorder(guiRect);
            }
        }

        GUI.color = previousColor;
    }

    void DrawBorder(Rect rect)
    {
        float thickness = 2f;
        Texture2D texture = Texture2D.whiteTexture;

        GUI.DrawTexture(
            new Rect(rect.x, rect.y, rect.width, thickness), texture
        );

        GUI.DrawTexture(
            new Rect(rect.x, rect.yMax - thickness, rect.width, thickness),
            texture
        );

        GUI.DrawTexture(
            new Rect(rect.x, rect.y, thickness, rect.height), texture
        );

        GUI.DrawTexture(
            new Rect(rect.xMax - thickness, rect.y, thickness, rect.height),
            texture
        );
    }
}
