using UnityEngine;
using DuckHunt.VR;

/// <summary>
/// Helper component that wires up XR button events to the StartPedestalController at runtime.
/// Uses standardized VRInteractable base class.
/// </summary>
public class ButtonEventWirer : VRInteractable
{
    [SerializeField] private StartPedestalController pedestalController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem buttonParticles;

    protected override void Awake()
    {
        base.Awake();

        // Auto-find particle system if not assigned
        if (buttonParticles == null)
        {
            buttonParticles = GetComponentInChildren<ParticleSystem>();
        }
    }

    protected override void HandleActivation()
    {
        base.HandleActivation();
        TriggerButton();
    }

    private void TriggerButton()
    {
        if (pedestalController != null)
        {
            pedestalController.OnStartButtonPressed();
        }

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        if (buttonParticles != null)
        {
            buttonParticles.Play();
        }
    }
}
