using UnityEngine;

public class CameraProceduralFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    // NOVO: Variável para armazenar a rotação fixa da câmera
    private Quaternion fixedRotation;

    [Header("Settings")]
    public float followDelayTime = 0.5f;

    [Header("Horizontal Offset")] // NOVO HEAD
    // Ângulo em relação ao eixo Z (0 = atrás, 90 = direita, 45 = canto)
    [Range(0f, 360f)]
    public float horizontalAngle = 45f;

    [Header("Positioning")]
    public float cameraDistance = 20f;

    // Inclinação vertical (90 = topo)
    [Range(10f, 90f)]
    public float viewAngle = 60f;

    void Start()
    {
        // Calcula e armazena a rotação inicial que a câmera deve manter (para LookAt estático)
        if (target != null)
        {
            // Calcula o vetor de onde a câmera está para o alvo
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            // Ignora o componente Y do alvo para evitar que a rotação da câmera incline-se para baixo
            directionToTarget.y = 0;

            // Se você quer que a câmera olhe para a frente, use Quaternion.LookRotation(directionToTarget);
            // Se você quer que a câmera mantenha sua rotação original, use transform.rotation.
            // Para este cenário, manteremos a rotação inicial que você configurar manualmente no Unity,
            // pois o LookAt é problemático. Vamos configurar a rotação para olhar o alvo apenas uma vez.

            // Para garantir que a câmera olhe para o alvo na inicialização:
            fixedRotation = Quaternion.LookRotation(target.position - transform.position);
        }
        else
        {
            fixedRotation = transform.rotation;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- 1. Calcular a Posição Desejada (Offset) ---

        // 1a. Cálculo Vertical (Inclinação)
        float viewAngleRad = viewAngle * Mathf.Deg2Rad;

        float height = Mathf.Sin(viewAngleRad) * cameraDistance;
        float distanceBack = Mathf.Cos(viewAngleRad) * cameraDistance; // Distância horizontal total

        // 1b. Cálculo Horizontal (Ângulo X-Z)
        float horizontalAngleRad = horizontalAngle * Mathf.Deg2Rad;

        // O seno e cosseno distribuem a distância horizontal (distanceBack) nos eixos X e Z
        float offsetX = Mathf.Sin(horizontalAngleRad) * distanceBack;
        // O sinal negativo é usado para garantir que o recuo ocorra na direção correta
        float offsetZ = -Mathf.Cos(horizontalAngleRad) * distanceBack;

        Vector3 targetPosition = target.position;

        // Definir a posição final desejada da câmera
        Vector3 desiredPosition = new Vector3(
            targetPosition.x + offsetX,     // Aplica o offset horizontal no X
            targetPosition.y + height,      // Altura vertical
            targetPosition.z + offsetZ      // Aplica o offset horizontal no Z
        );

        // --- 2. Suavizar Movimento (Frame-Rate Independente) ---

        float factor = 1f - Mathf.Pow(0.001f, Time.deltaTime / followDelayTime);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, factor);

        transform.position = smoothedPosition;

        // --- 3. FIX: Impedir a Rotação do Jogador (Usa a Rotação Fixa) ---
        // Se a câmera for um objeto separado, ela sempre deve olhar para o alvo (fixedRotation).
        transform.rotation = fixedRotation;
    }

    void OnValidate()
    {
        if (target != null)
        {
            LateUpdate();
        }
    }
}