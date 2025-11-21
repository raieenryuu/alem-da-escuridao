using UnityEngine;
using UnityEngine.SceneManagement;

public class Gerador : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
	    Debug.Log("colidiu");
        if (other.gameObject.CompareTag("Player"))
	{
		SceneManager.LoadScene(2);
		Debug.Log("foi");
	}
    }
}
