using UnityEngine;

public class CameraProceduralFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float smoothSpeed = 0.125f;
    
    [Header("Positioning")]
    // O quão longe a câmera está do player (hipotenusa)
    public float cameraDistance = 20f; 
    
    // 90 = Totalmente de cima (Top Down), 45 = Isométrico, 0 = Chão
    [Range(10f, 90f)] 
    public float viewAngle = 60f; 

    void LateUpdate()
    {
        if (target == null) return;

        // Matemática para converter o ângulo e distância em posição X, Y, Z
        // Converter ângulo para radianos
        float angleRad = viewAngle * Mathf.Deg2Rad;

        // Calcular altura (Y) e recuo (Z) baseado no ângulo
        float height = Mathf.Sin(angleRad) * cameraDistance;
        float distanceBack = Mathf.Cos(angleRad) * cameraDistance;

        // Definir posição desejada
        Vector3 desiredPosition = new Vector3(
            target.position.x,
            target.position.y + height,
            target.position.z - distanceBack
        );

        // Suavizar movimento
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Garantir que a câmera olhe para o jogador
        transform.LookAt(target);
    }

    // Permite testar os sliders no editor sem dar Play
    void OnValidate()
    {
        if (target != null)
        {
            LateUpdate();
        }
    }
}