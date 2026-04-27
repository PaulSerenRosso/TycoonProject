using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    protected PlayerPawn playerPawn;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Possess(PlayerPawn inPlayerPawn)
    {
        playerPawn = inPlayerPawn;
        playerPawn.OnPossess();
    }
}
