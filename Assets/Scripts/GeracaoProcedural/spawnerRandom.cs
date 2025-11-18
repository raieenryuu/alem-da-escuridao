using UnityEngine;

public class spawnerRandom : MonoBehaviour
{
  [SerializeField]
  private GameObject[] obj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int rand = Random.Range(0, obj.Length);
        GameObject objeto = (GameObject)Instantiate(obj[rand], transform.position, Quaternion.identity);
        objeto.transform.parent = transform;
    }
}
