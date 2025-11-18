using UnityEngine;

public class spawner : MonoBehaviour
{
  [SerializeField]
  private GameObject obj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject objeto = (GameObject)Instantiate(obj, transform.position, Quaternion.identity);
        objeto.transform.parent = transform;
    }

    // Update is called once per frame
        
}
