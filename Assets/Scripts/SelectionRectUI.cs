using UnityEngine;
using UnityEngine.UI;

public class SelectionRectUI : MonoBehaviour
{
    public Image selectionImage;
    private bool isSelecting;
    private Camera mainCamera;
    private RectTransform selectionRectTransform;

    private Vector2 startMousePositionScreen;

    private void Start()
    {
        selectionRectTransform = selectionImage.GetComponent<RectTransform>();
        selectionImage.gameObject.SetActive(false);
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Error: Main camera doesn't exist");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && mainCamera != null)
        {
            startMousePositionScreen = GetMouseScreenPosition();
            isSelecting = true;
            selectionImage.gameObject.SetActive(true);

            selectionRectTransform.anchoredPosition = startMousePositionScreen;
            selectionRectTransform.sizeDelta = Vector2.zero;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
            selectionImage.gameObject.SetActive(false);

            // NOTE: In case we need it
            // HandleSelection();
        }

        if (isSelecting && Input.GetMouseButton(0))
        {
            var currentMousePositionScreen = GetMouseScreenPosition();
            UpdateSelectionRect(startMousePositionScreen, currentMousePositionScreen);
        }
    }

    private Vector2 GetMouseScreenPosition()
    {
        return Input.mousePosition;
    }

    private void UpdateSelectionRect(Vector2 startScreen, Vector2 endScreen)
    {
        // NOTE: Calculate the size of the selection rectangle
        var size = endScreen - startScreen;

        // NOTE: Adjust the position based on the direction of the mouse movement
        if (size.x < 0)
        {
            selectionRectTransform.anchoredPosition = new Vector2(endScreen.x, selectionRectTransform.anchoredPosition.y);
            size.x = -size.x;
        }
        else
        {
            selectionRectTransform.anchoredPosition = new Vector2(startScreen.x, selectionRectTransform.anchoredPosition.y);
        }

        if (size.y < 0)
        {
            selectionRectTransform.anchoredPosition = new Vector2(selectionRectTransform.anchoredPosition.x, endScreen.y);
            size.y = -size.y;
        }
        else
        {
            selectionRectTransform.anchoredPosition = new Vector2(selectionRectTransform.anchoredPosition.x, startScreen.y);
        }

        selectionRectTransform.sizeDelta = size;
    }

    private void HandleSelection()
    {
        // NOTE: Si jamais on veut faire quelque chose, le faire ici. Actuellement, tout est géré directement dans le système ECS qui fait bien l'affaire. Ici, le code sert seulement à afficher un beau rectangle de sélection :)
    }
}