using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //ui控件
    [HideInInspector]
    public Button btn1;
    [HideInInspector]
    public Button btn2;
    [HideInInspector]
    public GameObject tmp;
    private TextMeshProUGUI txt;

    [Header("根据格子数动态生成棋盘")]
    public int grid = 3;            //棋盘单行格数，可动态生成，不过格子多了效果并不好，白做了。
    int totalGrid = 0;

    //=============资源相关=============
    [Header("生成棋子的点位")]
    [SerializeField]
    private Vector3[] points;       //棋子点位
    private Transform units;        //棋子父结点    
    private GameObject chess;       //未激活状态棋子的prefab

    //=============棋盘状态相关=============
    private bool setEnd = false;    //是否已经设置完初始棋手
    private bool gameEnd = false;   //是否结束游戏
    private bool cpuFirst = false;  //是否cpu先行
    
    private int playerScore = 1;    //玩家分数基数
    private int aiScore = 10;       //cpu分数基数
    private int scoreToWin ;        //scoreToWin *10是ai赢，scoreToWin是player赢
    [Header("当前棋子状态")]
    [SerializeField]
    private int[] chessState;       //1为player，10为ai，0为未激活
    private int[] horizontalState;
    private int[] verticalState;
    private int[] crossState;
    private int step = 0;           //当前步数
    private int dep = 7;            //最大计算深度，调到0 ai变白痴。

    [Header("AI相关")]
    [SerializeField]
    private bool autoChess = false;
    [SerializeField]
    private List<Chess> chesses = new List<Chess>();    //用来随机落子的
    private List<Chess> chessesStatic = new List<Chess>();

    private enum currentPlayer
    {
        player,
        ai
    }
    [Header("当前棋手")]
    [SerializeField]
    private currentPlayer curr;     //当前棋手。


    private void Awake()
    {
        instance = this;    //单例
        txt = tmp.GetComponent<TextMeshProUGUI>();  //结算文本
        scoreToWin = grid;


        //units的作用是作为生成棋子的父结点。
        units = GameObject.FindGameObjectWithTag("Units").GetComponent<Transform>();
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x,1 );  //动态调整棋子大小。
        totalGrid = grid * grid;    //总格子数

        //初始化各类数组
        chessState = new int[totalGrid];    
        points = new Vector3[totalGrid];

        //根据grid数生成棋子生成坐标
        float unitLength = transform.localScale.x / grid;
        for (int i = 0; i < grid; i++)
        {
            for (int j = 0; j < grid; j++)
            {
                points[(i)* grid + j] = new Vector3(-transform.localScale.x / 2 + unitLength/2+j* unitLength, 0,transform.localScale.x/2 - unitLength/2 - i* unitLength);
            }
        }
        //读取棋子的prefab，动态摆放
        chess = (GameObject)Resources.Load("Prefabs/Chess");
        for(int id=0;id< totalGrid; id++)
        {
            GameObject go = Instantiate(chess, points[id], Quaternion.identity,  units);
            Chess cs = go.GetComponent<Chess>();
            chesses.Add(cs);
            chessesStatic.Add(cs);
            float chessLength = unitLength * 0.9f;
            go.transform.localScale = new Vector3(chessLength, go.transform.localScale.y, chessLength); //调整棋子缩放
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


    //设置先手
    public void SetFirst(string first)
    {
        if (setEnd) return; //只能设置一次
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

    //更新棋子状态，使用ray击中棋子后，由棋子调用函数
    public void UpdateChess(Transform trans)
    {
        if (gameEnd) return;            //游戏结束
        if (!setEnd) return;            //必须要设置先手
        
        Chess chess = trans.GetComponent<Chess>();
        chesses.Remove(chess);          //已激活的棋子删掉，这个是用来随机落子的

        //判断当前棋手并落子，加分
        if (curr == currentPlayer.player)   chessState[chess.id] = 1;
        else   chessState[chess.id] = 10;

        step++;
        chess.SetMe(step);

        if (curr == currentPlayer.player)   curr = currentPlayer.ai;
        else curr = currentPlayer.player;

        CheckVictor();
        AiTurn();  //ai逻辑

    }



    //AI落子
    private void AiTurn()
    {
        if (gameEnd) return;
        //不是自己回合不落子
        if (cpuFirst && step % 2 == 1) return;
        if (!cpuFirst && step % 2 == 0) return;
        if (step >= totalGrid) return;

        //版本1 随机落子，极弱智
        if (autoChess)
        {
            int rnd = Random.Range(0, chesses.Count - 1);
            chesses[rnd].UpdateMe();
            return;
        }

        //版本2 极大极小值，赢不了
        int minN = 10000;
        int id = -1;
        //轮询棋盘空位，找到最合适的
        for (int i = 0; i < totalGrid; i++)
        {
            if (chessState[i] == 0)
            {
                int[] tempArr = (int[])chessState.Clone();
                tempArr[i] = 10;
                int tmp = Dfs(tempArr, 1, step + 1);
                if (minN > tmp)
                {
                    minN = tmp;
                    id = i;
                }
            }
        }
        if (id != -1)
            chessesStatic[id].UpdateMe();

    }


    //递归计算极大/极小值
    private int Dfs(int[] chessArr,int depth,int fakeStep)
    {

        if (depth > dep) return 0;

        if (fakeStep== totalGrid-1) return SetArr(chessArr, true, fakeStep);

        int minN = 10000;
        int findID = -1;
        int maxN = -10000;

        for(int i = 0; i < totalGrid; i++)
        {
            int[] tempArr = (int[])chessArr.Clone();
            int fs  = fakeStep;
            if (tempArr[i] == 0)
            {
                if ((cpuFirst && fs % 2 == 0) || (!cpuFirst && fs % 2 == 1)) //Ai turn
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
                else //Player turn
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

        //格子多了的话这么写不行，懒得改了
        if (maxN >= 900) return maxN;
        if (minN <= -900) return minN;

        if (findID == -1) return 0;

        int ret;
        if ((cpuFirst && fakeStep % 2 == 0) || (!cpuFirst && fakeStep % 2 == 1))
        {
            //ai的选择
            int [] tempArr = (int[])chessArr.Clone();
            tempArr[findID] = 10;
            ret = Dfs(tempArr, depth + 1, fakeStep + 1);
        }
        else
        {
            //player的选择
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
                total = total -(int)Mathf.Pow(10, i / 10);
            else if(i < 10)
                total = total + (int)Mathf.Pow(12, i) ;     //player的估分比ai高，优先阻截player落子
        }
        return total;
    }

    //检查胜利者
    private void CheckVictor()
    {
        //检查各向数组
        int who = SetArr(chessState, false, step);
        switch (who)
        {
            case 1:
                txt.text = "You Win";
                gameEnd = true;
                return;
            case -1:
                txt.text = "You Lose";
                gameEnd = true;
                return;
            default:
                break;
        }
        if (step == totalGrid)
        {
            txt.text = "Tie";
            gameEnd = true;
        }
    }

    //将当前棋子状态划分为横/竖/交叉三个数组并分别计算分值
    private int SetArr(int[] arr, bool isAssessing, int tmStep)
    {
        horizontalState = new int[grid];
        verticalState = new int[grid];
        crossState = new int[2];
        for (int id = 0; id < arr.Length; id++)
        {
            horizontalState[id % grid] += arr[id];      //水平方向数组
            verticalState[id / grid] += arr[id];        //竖直方向数组
            if (id % grid == id / grid)                 //正向交叉数组
                crossState[0] += arr[id];
            if (id % grid + id / grid == grid - 1)      //反向交叉数组
                crossState[1] += arr[id];
        }
        if (isAssessing)    //用于递归中估分
            return Assess(horizontalState, tmStep) + Assess(verticalState, tmStep) + Assess(crossState, tmStep);
        else                //判断是否胜利，三种情况满足一种即可
            return CheckArr(horizontalState) + CheckArr(verticalState) + CheckArr(crossState);
    }

    //判断是否胜利
    private int CheckArr(int[] arr)
    {
        foreach (int a in arr)
        {
            if (a == scoreToWin)
                return 1;       //1代表玩家赢
            if (a == scoreToWin * 10)
                return -1;      //-1代表cpu赢
        }
        return 0;

    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
