using UnityEngine;

public class PersistObject : MonoBehaviour
{
    public static PersistObject inst;

    private void Awake()
    {
        if(inst == null)
        {
            inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
