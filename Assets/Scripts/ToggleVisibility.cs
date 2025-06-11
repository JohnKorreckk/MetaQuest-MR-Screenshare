using UnityEngine;
using UnityEngine.XR;

public class ToggleVisibility : MonoBehaviour
{
    private Renderer objRenderer;
    private bool wasButtonPressedLastFrame = false;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer == null)
        {
            Debug.LogWarning("No Renderer found on the GameObject!");
        }
    }

    void Update()
    {
        // Use the left controller
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // Read the primary button state (X button)
        if (leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
        {
            if (isPressed && !wasButtonPressedLastFrame)
            {
                // Toggle visibility
                if (objRenderer != null)
                {
                    objRenderer.enabled = !objRenderer.enabled;
                }
            }

            // Save state for next frame
            wasButtonPressedLastFrame = isPressed;
        }
    }
}
