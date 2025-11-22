using UnityEngine;
using System.Collections.Generic; // Necessário para List<Vector3>

public class geracao : MonoBehaviour
{
    [SerializeField]
    private GameObject[] salas;
    [SerializeField]
    private GameObject salaInicio;
    [SerializeField]
    private GameObject salaFim;

    // MUDANÇA 1: Tamanho do array para 4x4 = 16 nodes.
    private Vector3[] node = new Vector3[16];

    private float proxSala = 100;  // Distância entre salas
    private int direcao = 0;
    private int cont = 0;
    private bool gerando = true;
    private int salaLayer = 6;

    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
        salaLayer = 1 << salaLayer;

        addNode();

        // Inicia a geração em uma das posições iniciais (mantido da versão 5x5 ajustada)
        int rand = Random.Range(0, 4);
        transform.position = node[rand];
        Instantiate(salaInicio, transform.position, Quaternion.identity);
        direcao = Random.Range(1, 7);
    }

    private void addNode()
    {
        // MUDANÇA 2: Loop limitado a 16 nodes.
        for (int i = 0; i < 16; i++)
        {
            // X: -150, -50, 50, 150 (4 colunas)
            // Z: 0, 100, 200, 300 (4 linhas)
            node[i] = new Vector3(-150 + (i % 4) * 100, 0, 0 + (i / 4) * 100);
        }
    }

    private void prox()
    {
        int sala = 0;
        Vector3 novaPos = new Vector3(0, 0, 0);

        // Movimento para a Direita (Movement Right)
        if (direcao == 1 || direcao == 2 || direcao == 3)
        {
            // MUDANÇA 3: Limite X máximo é 150 (4ª coluna)
            if (transform.position.x < 150)
            {
                cont = 0;
                sala = Random.Range(0, 4);
                novaPos = new Vector3(transform.position.x + proxSala, transform.position.y, transform.position.z);
                direcao = Random.Range(1, 8);
                if (direcao == 4 || direcao == 5 || direcao == 6)
                {
                    direcao = 2;
                }
            }
            else
            {
                direcao = 7;
            }
        }

        // Movimento para a Esquerda (Movement Left)
        if (direcao == 4 || direcao == 5 || direcao == 6)
        {
            // MUDANÇA 4: Limite X mínimo é -150 (1ª coluna)
            if (transform.position.x > -150)
            {
                cont = 0;
                sala = Random.Range(0, 4);
                novaPos = new Vector3(transform.position.x - proxSala, transform.position.y, transform.position.z);
                direcao = Random.Range(4, 8);
            }
            else
            {
                direcao = 7;
            }
        }

        // Movimento para Frente (Movement Forward/Up)
        if (direcao == 7)
        {
            // MUDANÇA 5: Limite Z máximo é 300 (4ª linha)
            if (transform.position.z < 300)
            {
                cont++;
                sala = Random.Range(2, 4);

                Collider[] colliders = Physics.OverlapSphere(transform.position, 5, salaLayer);

                // Lógica existente para tratar sobreposições e ajustes de sala
                if (cont > 1)
                {
                    if (colliders.Length > 0)
                    {
                        Collider detector = colliders[0];
                        tipoSala tipoScript = detector.GetComponent<tipoSala>();
                        if (tipoScript != null)
                        {
                            int tipo = tipoScript.getId();
                            if (tipo != 3)
                            {
                                tipoScript.destruir();
                                Instantiate(salas[3], transform.position, Quaternion.identity);
                            }
                        }
                    }
                }
                else
                {
                    if (colliders.Length > 0)
                    {
                        Collider detector = colliders[0];
                        tipoSala tipoScript = detector.GetComponent<tipoSala>();

                        if (tipoScript != null)
                        {
                            int tipo = tipoScript.getId();
                            if (tipo != 1 && tipo != 3)
                            {
                                tipoScript.destruir();
                                int novaSala = Random.Range(1, 4);
                                if (novaSala == 2)
                                {
                                    novaSala = 1;
                                }
                                Instantiate(salas[novaSala], transform.position, Quaternion.identity);
                            }
                        }
                    }
                }

                novaPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + proxSala);
                direcao = Random.Range(1, 7);
            }
            else
            {
                // MUDANÇA 6: Para a geração principal. A sala final é colocada em Update().
                gerando = false;
                return;
            }
        }

        transform.position = novaPos;
        Instantiate(salas[sala], transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (gerando)
        {
            prox();
        }
        else
        {
            // MUDANÇA 7: Lógica para randomizar salaFim e preencher o restante.

            // 1. Encontra todos os nodes vazios
            List<Vector3> emptyNodes = new List<Vector3>();
            foreach (var no in node)
            {
                Collider[] colliders = Physics.OverlapSphere(no, 5, salaLayer);

                if (colliders.Length == 0)
                {
                    emptyNodes.Add(no);
                }
            }

            // 2. Coloca o Gerador (salaFim) em um node vazio aleatório
            if (emptyNodes.Count > 0)
            {
                int randIndex = Random.Range(0, emptyNodes.Count);
                Vector3 finalRoomPos = emptyNodes[randIndex];

                Instantiate(salaFim, finalRoomPos, Quaternion.identity);

                // Remove a posição escolhida para não ser preenchida com sala regular
                emptyNodes.RemoveAt(randIndex);
            }

            // 3. Preenche os nodes vazios restantes com salas aleatórias
            foreach (var no in emptyNodes)
            {
                int novaSala = Random.Range(0, salas.Length);
                Instantiate(salas[novaSala], no, Quaternion.identity);
            }

            // Finaliza o gerador
            Destroy(this.gameObject);
        }
    }
}