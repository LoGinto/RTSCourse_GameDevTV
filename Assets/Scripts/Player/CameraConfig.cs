using UnityEngine;


namespace GameDevTV.RTS.Player
{
    [System.Serializable]
    public class CameraConfig
    {
        [field:SerializeField] public bool EnableEdgePan  { get; private set; } = true;
        [field:SerializeField]public float MousePanSpeed { get; private set; } = 5;
        [field:SerializeField]public float EdgePanSize { get; private set; } = 50;


        [field:SerializeField]public float KeyboardForwardPanSpeed { get; private set; } = 2.5f;
        [field:SerializeField]public float KeyboardRightPanSpeed { get; private set; } = 5f;

        [field:SerializeField]public float RotationSpeed { get; private set; } = 90f;

        [field:SerializeField]public float MaxZoomIn { get; private set; } = 7.5f;
        [field:SerializeField]public float MaxZoomOut { get; private set; } = 15f;
        [field:SerializeField]public float ZoomStep { get; private set; } = 1f;
        [field:SerializeField]public float ZoomSpeed { get; private set; } = 8f;
    }
}