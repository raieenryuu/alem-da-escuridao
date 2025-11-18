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

    private float proxSala = 25;
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
       transform.position = node[rand];
       Instantiate(salaInicio, transform.position, Quaternion.identity);
       //Instantiate(jogador, transform.position, Quaternion.identity);
       direcao = Random.Range(1, 7);

       addNode();

    }

    private void addNode()
    {
       for (int i = 0; i < 65; i++)
        {
          node[i] = new Vector3(-50 + (i%5)*25, 0, 0 + (i/5)*25);
          //node[i].transform.position = no;
          //Debug.Log("foi:" + i);
        }


    }
    private void prox()
    {
      int sala = 0;
      Vector3 novaPos = new Vector3(0, 0, 0);
      if(direcao == 1 || direcao == 2 || direcao == 3)
      {
        if(transform.position.x < 50)
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
      if (direcao == 4 || direcao == 5 || direcao == 6)
      {

        if(transform.position.x > -50)
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
      if (direcao == 7)
      {
        if(transform.position.z < 300)
        {
          cont++;
          sala = Random.Range(2, 4);
          /*
          if (sala == 2)
          {
            sala = 1;
          }*/
        // CORREÇÃO: Usar Physics.OverlapSphere para 3D em vez de Physics2D.OverlapCircle       
                Collider[] colliders = Physics.OverlapSphere(transform.position, 1, salaLayer);
                if(cont > 1)
                {
                if(colliders.Length > 0)
                {
                    Collider detector = colliders[0];
                    tipoSala tipoScript = detector.GetComponent<tipoSala>();
                   // Debug.Log("colidiu com algo. Cont = " + cont);
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
                    //Debug.Log("colidiu com algo");
                    Collider detector = colliders[0];
                    tipoSala tipoScript = detector.GetComponent<tipoSala>();
                    
                    if(tipoScript != null)
                    {
                        int tipo = tipoScript.getId();
                        
                        // CORREÇÃO: Mudar || para && na condição
                        if(tipo != 1 && tipo != 3)
                        {
                            tipoScript.destruir();
                            int novaSala = Random.Range(1, 4);
                            if(novaSala == 2)
                            {
                                novaSala = 1;
                            }
                            // CORREÇÃO: Usar novaSala em vez de sala
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
          Collider[] colliders = Physics.OverlapSphere(transform.position, 1, salaLayer);
          if(cont > 1)
          {
            if(colliders.Length > 0)
              {
                Collider detector = colliders[0];
                tipoSala tipoScript = detector.GetComponent<tipoSala>();
                   // Debug.Log("colidiu com algo. Cont = " + cont);
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
      
//   direcao = Random.Range(1, 8);
    }

    // Update is called once per frame
    void Update()
    {
      //if (tempo < 0f)
      //{
      if (gerando)
      {
        prox(); 
      }
      else
      {

        foreach (var no in node)
        {
          Collider[] colliders = Physics.OverlapSphere(no, 1, salaLayer);
          
          if(colliders.Length == 0)
          {
            int novaSala = Random.Range(0, 4);
            Instantiate(salas[novaSala], no, Quaternion.identity);
          }
        }
        Destroy(this.gameObject);
      }
      //}
      //else
      //{
      // tempo -= Time.deltaTime;   
      //}
    }
}
