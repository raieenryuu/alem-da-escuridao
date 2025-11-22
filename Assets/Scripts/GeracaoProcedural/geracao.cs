using UnityEngine;

public class geracao : MonoBehaviour
{
    //[SerializeField]
    //private Transform[] posicoesIniciais;
    [SerializeField]
    private GameObject[] salas;
    [SerializeField]
    private GameObject salaInicio;
    [SerializeField]
    private GameObject salaFim;
    
    private Vector3[] node = new Vector3[65];

    // ALTERAÇÃO: A distância entre salas agora é 100 (tamanho da sala)
    private float proxSala = 100; 
    private int direcao = 0;
    private int cont = 0;
    private bool gerando = true;
    private int salaLayer = 6;

    //private float tempo = 1f;
    void Start()
    {
       transform.position = new Vector3(0,0,0);
       salaLayer = 1 << salaLayer; 
       int rand = Random.Range(0, 4);
       
       // Precisamos garantir que o node esteja inicializado antes de usar
       addNode(); 
       
       transform.position = node[rand];
       Instantiate(salaInicio, transform.position, Quaternion.identity);
       //Instantiate(jogador, transform.position, Quaternion.identity);
       direcao = Random.Range(1, 7);
    }

    private void addNode()
    {
       for (int i = 0; i < 65; i++)
        {
          // ALTERAÇÃO: Ajuste da matemática do grid para escala 100.
          // Antes: -50 (2 salas de 25 para esquerda). Agora: -200 (2 salas de 100 para esquerda).
          node[i] = new Vector3(-200 + (i%5)*100, 0, 0 + (i/5)*100);
        }
    }

    private void prox()
    {
      int sala = 0;
      Vector3 novaPos = new Vector3(0, 0, 0);
      
      // Movimento para a Direita
      if(direcao == 1 || direcao == 2 || direcao == 3)
      {
        // ALTERAÇÃO: Limite X aumentado de 50 para 200
        if(transform.position.x < 200) 
        {
          cont = 0;
          sala = Random.Range(0,4);
          novaPos = new Vector3(transform.position.x + proxSala, transform.position.y, transform.position.z);
          direcao = Random.Range(1, 8);
          if(direcao == 4 || direcao == 5 || direcao == 6)
          {
            direcao = 2;
          }
        }
        else
        {
          direcao = 7;
        }
      }
      
      // Movimento para a Esquerda
      if (direcao == 4 || direcao == 5 || direcao == 6)
      {
        // ALTERAÇÃO: Limite X diminuído de -50 para -200
        if(transform.position.x > -200)
        {
          cont = 0;
          sala = Random.Range(0,4);
          novaPos = new Vector3(transform.position.x - proxSala, transform.position.y, transform.position.z);
          direcao = Random.Range(4, 8);
        }
        else
        {
          direcao = 7;
        }
      }
      
      // Movimento para Frente
      if (direcao == 7)
      {
        // ALTERAÇÃO: Limite Z aumentado de 300 para 1200 (proporcional a 4x o tamanho)
        if(transform.position.z < 1200)
        {
          cont++;
          sala = Random.Range(2, 4);

          // Usei um raio um pouco maior (5) para garantir detecção na escala grande, 
          // mas 1 funciona se a posição for exata.
          Collider[] colliders = Physics.OverlapSphere(transform.position, 5, salaLayer);
          
          if(cont > 1)
          {
            if(colliders.Length > 0)
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
            if(colliders.Length > 0)
            {
                Collider detector = colliders[0];
                tipoSala tipoScript = detector.GetComponent<tipoSala>();
                
                if(tipoScript != null)
                {
                    int tipo = tipoScript.getId();
                    if(tipo != 1 && tipo != 3)
                    {
                        tipoScript.destruir();
                        int novaSala = Random.Range(1, 4);
                        if(novaSala == 2)
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
          gerando = false;
          Collider[] colliders = Physics.OverlapSphere(transform.position, 5, salaLayer);
          if(cont > 1)
          {
            if(colliders.Length > 0)
              {
                Collider detector = colliders[0];
                tipoSala tipoScript = detector.GetComponent<tipoSala>();
                 if (tipoScript != null)
                    {
                        tipoScript.destruir();
                        Instantiate(salaFim, transform.position, Quaternion.identity);
                    }
                }
            }
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
        foreach (var no in node)
        {
          // Aumentei raio de detecção para 5 por segurança na escala maior
          Collider[] colliders = Physics.OverlapSphere(no, 5, salaLayer);
          
          if(colliders.Length == 0)
          {
            int novaSala = Random.Range(0, 4);
            Instantiate(salas[novaSala], no, Quaternion.identity);
          }
        }
        Destroy(this.gameObject);
      }
    }
}