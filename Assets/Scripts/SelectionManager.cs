
using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    private List<UnitSelection> selectedUnits =
        new List<UnitSelection>();

    private Vector3 dragStart;
    private Vector3 dragEnd;
    private bool isDragging = false;

    public float dragThreshold = 10f;
    public float unitSpacing = 1f;

    void Update()
    {
        // Bat dau nhan chuot trai
        if (Input.GetMouseButtonDown(0))
        {
            dragStart = Input.mousePosition;
            dragEnd = dragStart;
            isDragging = false;
        }

        // Giu va keo chuot trai
        if (Input.GetMouseButton(0))
        {
            dragEnd = Input.mousePosition;

            if (Vector3.Distance(dragStart, dragEnd)
                > dragThreshold)
            {
                isDragging = true;
            }
        }

        // Tha chuot trai
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                SelectUnitsInBox(dragStart, Input.mousePosition);
            }
            else
            {
                SelectSingleUnit(Input.mousePosition);
            }

            isDragging = false;
        }

        // Chuot phai: ra lenh di chuyen
        if (Input.GetMouseButtonDown(1))
        {
            MoveSelectedUnits();
        }
    }

    // Chon mot linh bang chuot trai
    void SelectSingleUnit(Vector3 mousePosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos =
            cam.ScreenToWorldPoint(mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(
            new Vector2(worldPos.x, worldPos.y)
        );

        UnitSelection clickedUnit = null;

        if (hit != null)
        {
            clickedUnit = hit.GetComponent<UnitSelection>();
        }

        bool wasOnlySelected = clickedUnit != null
            && selectedUnits.Count == 1
            && selectedUnits.Contains(clickedUnit);

        ClearSelection();

        if (clickedUnit != null && !wasOnlySelected)
        {
            AddUnit(clickedUnit);
        }
    }

    // Chon nhieu linh trong khung keo chuot
    void SelectUnitsInBox(Vector3 start, Vector3 end)
    {
        ClearSelection();

        Rect selectionRect = GetScreenRect(start, end);

        Camera cam = Camera.main;
        if (cam == null) return;

        UnitSelection[] allUnits =
            FindObjectsOfType<UnitSelection>();

        foreach (UnitSelection unit in allUnits)
        {
            Vector3 screenPos =
                cam.WorldToScreenPoint(unit.transform.position);

            if (screenPos.z > 0 &&
                selectionRect.Contains(
                    new Vector2(screenPos.x, screenPos.y)))
            {
                AddUnit(unit);
            }
        }

        Debug.Log("So linh da chon: " + selectedUnits.Count);
    }

    void AddUnit(UnitSelection unit)
    {
        if (unit == null || selectedUnits.Contains(unit))
            return;

        selectedUnits.Add(unit);
        unit.SetSelected(true);
    }

    void ClearSelection()
    {
        foreach (UnitSelection unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.SetSelected(false);
            }
        }

        selectedUnits.Clear();
    }

    // Dieu khien ca nhom di chuyen
    void MoveSelectedUnits()
    {
        if (selectedUnits.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(Input.mousePosition);

        // Dan hang ngang de khong dung chong len nhau
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            UnitSelection unit = selectedUnits[i];

            if (unit == null) continue;

            float offsetX =
                (i - (selectedUnits.Count - 1) / 2f)
                * unitSpacing;

            Vector3 destination = new Vector3(
                mouseWorld.x + offsetX,
                mouseWorld.y,
                unit.transform.position.z
            );

            unit.MoveTo(destination);
        }
    }

    Rect GetScreenRect(Vector3 start, Vector3 end)
    {
        return Rect.MinMaxRect(
            Mathf.Min(start.x, end.x),
            Mathf.Min(start.y, end.y),
            Mathf.Max(start.x, end.x),
            Mathf.Max(start.y, end.y)
        );
    }

    // Ve khung chon mau xanh trong Game
    void OnGUI()
    {
        if (!isDragging) return;

        Vector3 current = Input.mousePosition;

        float x = Mathf.Min(dragStart.x, current.x);
        float y = Screen.height -
            Mathf.Max(dragStart.y, current.y);

        float width = Mathf.Abs(current.x - dragStart.x);
        float height = Mathf.Abs(current.y - dragStart.y);

        Rect rect = new Rect(x, y, width, height);

        Color oldColor = GUI.color;

        GUI.color = new Color(0.2f, 1f, 0.5f, 0.15f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = new Color(0.2f, 1f, 0.5f, 0.95f);

        GUI.DrawTexture(
            new Rect(x, y, width, 2),
            Texture2D.whiteTexture
        );

        GUI.DrawTexture(
            new Rect(x, y + height - 2, width, 2),
            Texture2D.whiteTexture
        );

        GUI.DrawTexture(
            new Rect(x, y, 2, height),
            Texture2D.whiteTexture
        );

        GUI.DrawTexture(
            new Rect(x + width - 2, y, 2, height),
            Texture2D.whiteTexture
        );

        GUI.color = oldColor;
    }
}
