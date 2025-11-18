using UnityEngine;

public class tipoSala : MonoBehaviour
{
   [SerializeField]
   private int id;

   public int getId()
   {
     return id;
   }
   public void destruir()
   {
     Destroy(this.gameObject);
   }
}
