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


    [Header("���ݸ�������̬��������")]
    public int grid = 3; //���̵��и������ɶ�̬���ɣ�Ч����̫�ã������ˡ�
    int totalGrid = 0;

    //=============��Դ���=============
    [Header("�������ӵĵ�λ")]
    [SerializeField]
    private Vector3[] points;//���ӵ�λ
    private Transform units;//���Ӹ����    
    private GameObject chess;//���ӵ�prefab

    //=============����״̬���=============
    private bool setEnd = false;//�Ƿ��Ѿ��������ʼ����
    private bool gameEnd = false;//�Ƿ������Ϸ
    private bool cpuFirst = false;//�Ƿ�cpu����
    
    private int playerScore = 1;//��ҷ���
    private int aiScore = 10;//cpu����
    private int scoreToWin ; //����Ӯ�ķ���
    [Header("��ǰ����״̬������")]
    [SerializeField]
    private int[] chessState;
    [SerializeField]
    private int[] horizontalState;
    [SerializeField]
    private int[] verticalState;
    [SerializeField]
    private int[] crossState;
    private int step = 0;    //��ǰ����

    [SerializeField]
    private List<Chess> chesses = new List<Chess>();
    private List<Chess> chessesStatic = new List<Chess>();

    private enum currentPlayer
    {
        player,
        ai
    }    
    [SerializeField]
    private currentPlayer curr;//��ǰ���֡�


    private void Awake()
    {
        instance = this;

        scoreToWin = grid;
        //units����������Ϊ�������ӵĸ���㣬Ŀǰ���̵��������Զ����ɵģ����������ɸ���grids������չ���ӵ����̣�����ûʱ���ˡ�
        units = GameObject.FindGameObjectWithTag("Units").GetComponent<Transform>();
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x,1 );
        totalGrid = grid * grid;//�ܸ�����

        //��ʼ����������
        chessState = new int[totalGrid];    
        points = new Vector3[totalGrid];

        //����grid���������̣�
        float unitLength = transform.localScale.x / grid;
        for (int i = 0; i < grid; i++)
        {
            for (int j = 0; j < grid; j++)
            {
                points[(i)* grid + j] = new Vector3(-transform.localScale.x / 2 + unitLength/2+j* unitLength, 0,transform.localScale.x/2 - unitLength/2 - i* unitLength);
            }
        }
        //��ȡ���ӵ�prefab
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
        if (gameEnd) return;    //�����Ϸ����
        if (!setEnd) return;
        
        Chess chess = trans.GetComponent<Chess>();
        chesses.Remove(chess);      //����������ȥ����ǰ

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
        //����������
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
            horizontalState[id % grid] += arr[id]; //ˮƽ��������
            verticalState[id / grid] += arr[id]; //��ֱ��������
            if (id % grid == id / grid)//��������
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
                return 1;       //1�������Ӯ
            }
            if (a == scoreToWin * 10)
            {
                return -1;      //-1����cpuӮ
            }
        }
        //Debug.Log("Checking ");
        return 0;

    }

    //AI����
    private void AiTurn()
    {
        if (gameEnd) return;
        //�����Լ��غϲ�����
        if (cpuFirst && step % 2 == 1) return;
        if (!cpuFirst && step % 2 == 0) return;
        if(step >= totalGrid) return;

        //�汾1 �������
        //int rnd = Random.Range(0, chesses.Count - 1);
        //chesses[rnd].UpdateMe();
        //�汾2 �����Сֵ
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
                    //����������
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
                    //����������
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

    //�������ӷ�ֵ
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
