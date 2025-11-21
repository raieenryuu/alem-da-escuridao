using UnityEngine;

public class animacaoNave : MonoBehaviour
{
    [SerializeField]
    private GameObject nave;
    //private Vector3 rotacao = new Vector3(0f, 0f, 0f);
    private float eixoY = 0;
    [SerializeField]
    private float intensidade = 1;
    // Update is called once per frame
    void FixedUpdate()
    {
        //rotacao += new Vector3(time.DeltaTime, 0f, 0f);
        eixoY = Time.deltaTime * intensidade;  
        nave.transform.Rotate(0f, eixoY, 0f, Space.Self);
    }
}
