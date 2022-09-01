using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Button btn1;
    public Button btn2;


    [Header("根据格子数动态生成棋盘")]
    public int grid = 3; //棋盘单行格数，可动态生成，效果不太好，白做了。
    int totalGrid = 0;

    //=============资源相关=============
    [Header("生成棋子的点位")]
    [SerializeField]
    private Vector3[] points;//棋子点位
    private Transform units;//棋子父结点    
    private GameObject chess;//棋子的prefab

    //=============棋盘状态相关=============
    private bool setEnd = false;//是否已经设置完初始棋手
    private bool gameEnd = false;//是否结束游戏
    private bool cpuFirst = false;//是否cpu先行
    
    private int playerScore = 1;//玩家分数
    private int aiScore = 10;//cpu分数
    private int scoreToWin ; //可以赢的分数
    [Header("当前棋盘状态，数组")]
    [SerializeField]
    private int[] chessState;
    [SerializeField]
    private int[] horizontalState;
    [SerializeField]
    private int[] verticalState;
    [SerializeField]
    private int[] crossState;
    private int step = 0;    //当前步数

    [SerializeField]
    private List<Chess> chesses = new List<Chess>();
    private List<Chess> chessesStatic = new List<Chess>();

    private enum currentPlayer
    {
        player,
        ai
    }    
    [SerializeField]
    private currentPlayer curr;//当前棋手。


    private void Awake()
    {
        instance = this;

        scoreToWin = grid;
        //units的作用是作为生成棋子的父结点，目前棋盘的棋子是自动生成的，本来想做成根据grids变量扩展格子的棋盘，不过没时间了。
        units = GameObject.FindGameObjectWithTag("Units").GetComponent<Transform>();
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x,1 );
        totalGrid = grid * grid;//总格子数

        //初始化各类数组
        chessState = new int[totalGrid];    
        points = new Vector3[totalGrid];

        //根据grid数生成棋盘，
        float unitLength = transform.localScale.x / grid;
        for (int i = 0; i < grid; i++)
        {
            for (int j = 0; j < grid; j++)
            {
                points[(i)* grid + j] = new Vector3(-transform.localScale.x / 2 + unitLength/2+j* unitLength, 0,transform.localScale.x/2 - unitLength/2 - i* unitLength);
            }
        }
        //读取棋子的prefab
        chess = (GameObject)Resources.Load("Prefabs/Chess");
        for(int id=0;id< totalGrid; id++)
        {
            GameObject go = Instantiate(chess, points[id], Quaternion.identity,  units);
            Chess cs = go.GetComponent<Chess>();
            chesses.Add(cs);
            chessesStatic.Add(cs);
            float chessLength = unitLength * 0.9f;
            go.transform.localScale = new Vector3(chessLength, go.transform.localScale.y, chessLength);
            cs.id = id;
        }
    }
    // Start is called before the first frame update
    void Start()
    {   
    }
    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateChess(Transform trans)
    {
        if (gameEnd) return;    //如果游戏结束
        if (!setEnd) return;
        
        Chess chess = trans.GetComponent<Chess>();
        chesses.Remove(chess);      //可用棋子中去掉当前

        if (curr == currentPlayer.player)
        {
            chessState[chess.id] = 1;
        }
        else
        {
            chessState[chess.id] = 10;
        }
        step++;
        chess.SetMe(step);
        if (curr == currentPlayer.player)
            curr = currentPlayer.ai;
        else
            curr = currentPlayer.player;
        CheckVictor();
        AiTurn();

    }
    public void SetFirst(string first)
    {
        if (setEnd) return;
        setEnd = true;
        switch (first)
        {
            case "player":
                curr = currentPlayer.player;
                break;
            case "ai":
                curr = currentPlayer.ai;
                cpuFirst = true;
                AiTurn();
                break;
            default:
                break;
        }
        btn1.gameObject.SetActive(false);
        btn2.gameObject.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    private void CheckVictor()
    {
        //检查各向数组
        int who = SetArr(chessState,false,step);
        switch (who)
        {
            case 1:
                Debug.Log("PlayerWin");
                gameEnd = true;
                return;
            case -1:
                Debug.Log("CpuWin");
                gameEnd = true;
                return;
            default:
                break;
        }
        if(step == totalGrid)
        {
            Debug.Log("Tie");
            gameEnd = true;
        }
    }

    private int SetArr(int[] arr,bool isAssessing,int tmStep)
    {
        horizontalState = new int[grid];
        verticalState = new int[grid];
        crossState = new int[2];
        for (int id=0;id<arr.Length; id++)
        {
            horizontalState[id % grid] += arr[id]; //水平方向数组
            verticalState[id / grid] += arr[id]; //竖直方向数组
            if (id % grid == id / grid)//交叉数组
            {
                crossState[0] += arr[id];
            }
            if (id % grid + id / grid == grid - 1)
            {
                crossState[1] += arr[id];
            }
        }
        if (isAssessing)
            return Assess(horizontalState, tmStep) + Assess(verticalState, tmStep) + Assess(crossState, tmStep);
        else
            return CheckArr(horizontalState) + CheckArr(verticalState) + CheckArr(crossState);
    }

    private int CheckArr(int[] arr)
    {
        foreach (int a in arr)
        {
            if (a == scoreToWin)
            {
                return 1;       //1代表玩家赢
            }
            if (a == scoreToWin * 10)
            {
                return -1;      //-1代表cpu赢
            }
        }
        //Debug.Log("Checking ");
        return 0;

    }

    //AI落子
    private void AiTurn()
    {
        if (gameEnd) return;
        //不是自己回合不落子
        if (cpuFirst && step % 2 == 1) return;
        if (!cpuFirst && step % 2 == 0) return;
        if(step >= totalGrid) return;

        //版本1 随机落子
        //int rnd = Random.Range(0, chesses.Count - 1);
        //chesses[rnd].UpdateMe();
        //版本2 最大最小值
        //Debug.Log(Dfs(chessState, 1, step));

        int minN = 10000;
        int id = -1;

        for (int i = 0; i < totalGrid; i++)
        {
            if (chessState[i] == 0)
            {
                int[] tempArr = (int[])chessState.Clone();
                tempArr[i] = 10;
                int tmp = Dfs(tempArr, 1, step+1);
                if (minN > tmp)
                {
                    minN = tmp;
                    id = i;
                }
                Debug.Log("tmp " + tmp.ToString() + " id " + i.ToString());
            }
        }
       // Debug.Log("min " + minN.ToString() + " id " + id.ToString());
        if(id!=-1)
            chessesStatic[id].UpdateMe();

    }


    private int Dfs(int[] chessArr,int depth,int fakeStep)
    {
        int fs = 0;

        if (fakeStep== 8) return SetArr(chessArr, true, fs);

        int minN = 10000;
        int findID = -1;
        int maxN = -10000;

        for(int i = 0; i < totalGrid; i++)
        {
            int[] tempArr = (int[])chessArr.Clone();
            fs = fakeStep;
            if (tempArr[i] == 0)
            {
                if ((cpuFirst && fs % 2 == 0) || (!cpuFirst && fs % 2 == 1))
                {
                    tempArr[i] = 10;
                    fs++;
                    //检查各向数组
                    int res = SetArr(tempArr, true, fs);
                    if (minN >= res)  
                    {
                        minN = res;
                        findID = i;
                    }
                }
                else 
                {
                    tempArr[i] = 1;
                    fs++;
                    //检查各向数组
                    int res = SetArr(tempArr, true, fs);
                    if (maxN <= res)
                    {
                        maxN = res;
                        findID = i;
                    }
                }
            }

        }

        if (maxN >= 900) return maxN;
        if (minN <= -900) return minN;

        if (findID == -1) return 0;

        int ret = 0;
        if ((cpuFirst && fakeStep % 2 == 0) || (!cpuFirst && fakeStep % 2 == 1))
        {
            int [] tempArr = (int[])chessArr.Clone();
            tempArr[findID] = 10;
            ret = Dfs(tempArr, depth + 1, fakeStep + 1);
        }
        else
        {
            int[] tempArr = (int[])chessArr.Clone();
            tempArr[findID] = 1;
            ret = Dfs(tempArr, depth + 1, fakeStep + 1);
        }

        return ret;
    }

    //评估落子分值
    private int Assess(int[] arr,int tmStep)
    {
        int total = 0;
        foreach(int i in arr)
        {
            if (i %10 == 0)
            {
                total = total -(int)Mathf.Pow(10, i / 10);
            }
            else if(i < 10)
            {
                total = total + (int)Mathf.Pow(12, i) ;
            }
        }
        //Debug.Log("total" + total.ToString());
        return total;
    }

}
