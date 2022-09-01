using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess : MonoBehaviour
{
    public int id = -1;
    private int status = 0;
    [SerializeField]
    private GameObject chessX;
    [SerializeField]
    private GameObject chessO;
    private GameManager gameManager;
    private bool imDone = false;
    private void Awake()
    {
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        chessX = (GameObject)Resources.Load("Prefabs/ChessX");
        chessO = (GameObject)Resources.Load("Prefabs/ChessO");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateMe()
    {
        if(! imDone)
            gameManager.UpdateChess(transform);
    }

    public void SetMe(int step)
    {
        imDone = true;
        GameObject go;
        if (step % 2 == 0)
        {
            go = Instantiate(chessX, transform.position, transform.rotation, transform);
        }
        else
        {
            go = Instantiate(chessO, transform.position, transform.rotation, transform);
        }
    }

}
