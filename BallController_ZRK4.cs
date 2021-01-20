using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
//using System.Diagnostics;

public class BallController : MonoBehaviour
{
    // 图表组件
    public SpeedChart chart;
    
    private int nball;
    private int ndyn;
    private float dt;
    private float k;
    private float damping;
    private float ks;
    private float m;
    private float g;
    private float L0;
    private float L1;
    private float L2;
    private float dL;
    private float t;
    private float driver_f;
    private float driver_r;
    private float driver_a;
    private float driver_p;
    private float L;
    private float Fsr;
    private float radius;
    private float cradius;
    private float diameter;
    private float cdiameter;
    private float amp;
    private float force_radius;
    private Vector3[] X;
    private Vector3[] V;
    private Vector3[] a;
    //龙格库塔法所需变量--------------------
    private Vector3[] X0;
    private Vector3[] V0;

    private Vector3[] X1;
    private Vector3[] V1;
    private Vector3[] a1;

    private Vector3[] X2;
    private Vector3[] V2;
    private Vector3[] a2;

    private Vector3[] X3;
    private Vector3[] V3;
    private Vector3[] a3;

    private Vector3[] X4;
    private Vector3[] V4;
    private Vector3[] a4;

    private Vector3[] dXX;
    private Vector3[] dV;
    //--自适应龙格库塔法所需额外变量----------
    private float upsilon;
    private Vector3[] ah1;
    private Vector3[] ah2;
    //----------------------------------------
    private Vector3 Fs2;
    private Vector3 Fs1;
    private Vector3 Ft2;
    private Vector3 Ft1;
    private Vector3[,] F_pair;
    private Vector3[] F;
    private Vector3 e0;
    private Vector3 e1;
    private Vector3 e;
    private Vector3 dX;
    private Vector3 origin;
    private Vector3 CX0;
    private Vector3 CX1;
    private float[,] Fr;
    private float[,] D;
    private List<GameObject> balls;
    private List<MoveController> ballscripts;
    private List<GameObject> strings;
    private List<MoveString> stringscripts;
    public GameObject Ballpref;
    public GameObject Stringpref;

    // 在第一帧更新之前调用Start 
    void Start()
    {
        Initiate_params();

        for (int i = 0; i < nball; i++)
        {
            GameObject newBall = Instantiate(Ballpref);
            newBall.transform.localScale = new Vector3(diameter, diameter, diameter);
            MoveController ballscript = newBall.GetComponent<MoveController>();
            balls.Add(newBall.gameObject);
            ballscripts.Add(ballscript);
        }

        for (int i = 0; i < 2; i++)
        {
            GameObject newString = Instantiate(Stringpref);
            newString.transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
            MoveString stringscript = newString.GetComponent<MoveString>();
            strings.Add(newString.gameObject);
            stringscripts.Add(stringscript);
        }

        Initiate_xv();

        UpdatePos();
    }

    public void Run()
    {
        Get_xv();
        Distance();
        //L0 = L;
        Get_bond();

        
        InvokeRepeating("UpdatePos", 0f, 0.01f);
    }

    void UpdatePos()
    {

        Get_xv();
        Update_xv(nball, X, V, dt, ndyn, 1);
        //Debug.Log("L0 = " + L0);
        //Debug.Log("L  = " + L );
        Set_xv();
        Update_position();

        // 更新图表
        chart.AddData("",V[2].x);

        if (Input.GetKey(KeyCode.Escape))
        {
            Screen.fullScreen = false;  //退出全屏         
        }
        //按A全屏
        if (Input.GetKey(KeyCode.A))
        {
            Screen.SetResolution(1920, 1080, true);
            Screen.fullScreen = true;  //设置成全屏
        }

    }
    //若改变参量
    public void ChangeSomething(string key, string value)
    {
        origin = new Vector3(0.0f, 0.0f, 0.0f);
        //Parse将输入的被改变参量数据转换为float
        if (key == "dp")
        {
            damping = float.Parse(value);
        }
        else if (key == "l0")
        {
            L0 = float.Parse(value);
        }
        else if (key == "g")
        {
            g = float.Parse(value);
        }
        else if (key == "X0x")
        {
            ballscripts[0].x.x = float.Parse(value) + origin.x;
        }
        else if (key == "X0y")
        {
            ballscripts[0].x.y = float.Parse(value) + origin.y;
        }
        else if (key == "X0z")
        {
            ballscripts[0].x.z = float.Parse(value) + origin.z;
        }
        else if (key == "X1x")
        {
            ballscripts[1].x.x = float.Parse(value) + origin.x;
        }
        else if (key == "X1y")
        {
            ballscripts[1].x.y = float.Parse(value) + origin.y;
        }
        else if (key == "X1z")
        {
            ballscripts[1].x.z = float.Parse(value) + origin.z;
        }
        else if (key == "X2x")
        {
            ballscripts[2].x.x = float.Parse(value) + origin.x;
        }
        else if (key == "X2y")
        {
            ballscripts[2].x.y = float.Parse(value) + origin.y;
        }
        else if (key == "X2z")
        {
            ballscripts[2].x.z = float.Parse(value) + origin.z;
        }
        else if (key == "V0x")
        {
            ballscripts[0].v.x = float.Parse(value);
        }
        else if (key == "V0y")
        {
            ballscripts[0].v.y = float.Parse(value);
        }
        else if (key == "V0z")
        {
            ballscripts[0].v.z = float.Parse(value);
        }
        else if (key == "V1x")
        {
            ballscripts[1].v.x = float.Parse(value);
        }
        else if (key == "V1y")
        {
            ballscripts[1].v.y = float.Parse(value);
        }
        else if (key == "V1z")
        {
            ballscripts[1].v.z = float.Parse(value);
        }
        else if (key == "V2x")
        {
            ballscripts[2].v.x = float.Parse(value);
        }
        else if (key == "V2y")
        {
            ballscripts[2].v.y = float.Parse(value);
        }
        else if (key == "V2z")
        {
            ballscripts[2].v.z = float.Parse(value);
        }
        Get_xv();
        Distance();
        Get_bond();
        Update_position();
    }
    //若改变顶球的坐标
    public void ChangeY(float value)
    {
        ballscripts[0].x.y = value * 0.1f;
    }
    public void ChangeXZ(float xratio, float yratio)
    {
        float x0 = -0.2f;
        float z0 = -0.2f;
        float wid = 0.4f;
        float hei = 0.4f;
        ballscripts[0].x.x = x0 + wid * xratio;
        ballscripts[0].x.z = z0 + hei * yratio;

    }
    

    public void Jump(Vector3 pos)
    {
        //X[0].y += 5;
        ballscripts[0].x.y = pos.y * 0.1f;

    }

    //常量初始化
    void Initiate_params()
    {
        nball = 3;
        ndyn = 20;
        dt = 0.0001f;
        radius = 0.01f;
        cradius = radius / 5.0f;
        diameter = radius * 2.0f;
        cdiameter = cradius * 2.0f;
        damping = 0.005f;
        driver_f = 1.0f;
        driver_p = 0.0f;
        t = 0.0f;
        k = 28000.0f;
        ks = 28000.0f;
        m = 31.88f;
        m = m / 1000.0f;
        g = 9.8f;
        force_radius = 2.0f * radius;
        L0 = 0.61f;

        balls = new List<GameObject>();
        ballscripts = new List<MoveController>();
        strings = new List<GameObject>();
        stringscripts = new List<MoveString>();

        e = new Vector3();
        origin = new Vector3();
        dX = new Vector3();
        e0 = new Vector3();
        e1 = new Vector3();
        CX0 = new Vector3();
        CX1 = new Vector3();
        Fs2 = new Vector3();
        Fs1 = new Vector3();
        Ft2 = new Vector3();
        Ft1 = new Vector3();
        X = new Vector3[nball];//三个三维的向量，分别对应三个球的位移
        V = new Vector3[nball];//三个三维的向量，分别对应三个球的速度
        a = new Vector3[nball];//三个三维的向量，分别对应三个球的加速度

        //龙格库塔法所需变量--------------------
        X0 = new Vector3[nball];
        X1 = new Vector3[nball];
        X2 = new Vector3[nball];
        X3 = new Vector3[nball];
        X4 = new Vector3[nball];

        V0 = new Vector3[nball];
        V1 = new Vector3[nball];
        V2 = new Vector3[nball];
        V3 = new Vector3[nball];
        V4 = new Vector3[nball];

        a1 = new Vector3[nball];
        a2 = new Vector3[nball];
        a3 = new Vector3[nball];
        a4 = new Vector3[nball];

        dXX = new Vector3[nball];
        dV = new Vector3[nball];
        //--自适应龙格库塔法所需额外变量----------
        upsilon = 0.0001f;
        ah1 = new Vector3[nball];
        ah2 = new Vector3[nball];
        //----------------------------------------
        F = new Vector3[nball];
        F_pair = new Vector3[nball, nball];
        D = new float[nball, nball];
        Fr = new float[nball, nball];
    }
    void Initiate_xv()//初始化三个球分别的位移、速度
    {
        //----------------------------------------------------------------------
        origin = new Vector3(0.0f, 0.0f, 0.0f);
        //-----顶球-------------------------------------------------------------
        ballscripts[0].x = new Vector3(0.0f + driver_r, 0.0f, 0.0f);
        ballscripts[0].v = new Vector3(0.0f, 0.0f, 0.0f);
        //-----中球-------------------------------------------------------------
        ballscripts[1].x = new Vector3(0.0f, -0.3f, 0.0f);
        ballscripts[1].v = new Vector3(-1.0f, -1.0f, 0.0f);
        //-----下球------------------------------------------------------------
        ballscripts[2].x = new Vector3(0.0f, -0.61f, 0.0f);
        ballscripts[2].v = new Vector3(3.0f, 0.0f, 0.0f);
        ////----初始时三球在同一条直线上-------------------------------------------------
    }
    void Get_xv()
    {
        for (int i = 0; i < nball; i++)
        {
            X[i] = ballscripts[i].x;
            V[i] = ballscripts[i].v;
        }
    }
    void Set_xv()
    {
        for (int i = 0; i < nball; i++)
        {
            ballscripts[i].x = X[i];
            ballscripts[i].v = V[i];
        }
    }
    void Update_position()//更新位置
    {
        for (int i = 0; i < nball; i++)
        {
            balls[i].transform.position = ballscripts[i].x;
        }
    }
    void Distance()//三球间距
    {
        for (int i = 0; i < nball; i++)
        {
            for (int j = i+1; j < nball; j++)
            {
                if (i == j)
                {
                    D[i, j] = 0.0f;
                }//球对自身的距离为0
                else
                {
                    D[i, j] = (X[i] - X[j]).magnitude;
                    //.magnitude返回向量的长度
                    D[j, i] = D[i, j];
                }
            }
        }
        L1 = D[0, 1];//顶、中球距离
        L2 = D[1, 2];//中、下球距离
        L = L1 + L2;//总绳长是固定的（二球不系）
    }
    void Get_bond()//定义绳子
    {
        //----两个绳的自由度：位置、旋转----------------------------------------------------
        strings[0].transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        strings[1].transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        strings[0].transform.localScale = new Vector3(cdiameter, L1/2.0f, cdiameter);
        strings[1].transform.localScale = new Vector3(cdiameter, L2/2.0f, cdiameter);
        //-----以两球位置中点做绳的位置---------------------------------------------
        CX0 = (X[0]+X[1])/2.0f;
        CX1 = (X[1]+X[2])/2.0f;
        //-----得到绳的位置---------------------------------------------------
        strings[0].transform.position = CX0;
        strings[1].transform.position = CX1;
        //-----得到单位方向向量e0,e1---------------------------------------------
        e = new Vector3(0.0f, 1.0f, 0.0f);
        e0 = (X[1] - X[0]) / (X[1] - X[0]).magnitude;
        e1 = (X[2] - X[1]) / (X[2] - X[1]).magnitude;
        strings[0].transform.rotation = Quaternion.FromToRotation(e, e0);
        strings[1].transform.rotation = Quaternion.FromToRotation(e, e1);
        //----------------------------------------------------------------------
    }
    void Cal_force_pair()//根据球之间的位置计算两绳之间的力
    {
        for (int i = 0; i < nball - 1; i++)
        {
            for (int j = i + 1; j < nball; j++)
            {
                if (D[i, j] < force_radius)
                {
                    float dr = force_radius - D[i, j];
                    Fr[i, j] = k * dr;
                }
                else
                {
                    Fr[i, j] = 0.0f;
                }
                e = (X[i] - X[j]) / D[i, j];
                // The force act on i
                F_pair[i, j] = e * Fr[i, j];
                // The force act on j
                F_pair[j, i] = -F_pair[i, j];
                //牛顿第三定律作用力与反作用力
            }
        }
        //----------------------------------------------------------------------
        dL = L - L0;
        if (dL>0.0f)
        {
            Fsr = ks * dL;
        }
        else
        {
            Fsr = 0.0f;
        }
        //---胡克定律算绳子上的拉力大小------------------------------------------
        e = (X[1] - X[2]) / L2;
        Fs2 = e * Fsr;
        Ft2 = -Fs2;
        //----------------------------------------------------------------------
        e = (X[0] - X[1]) / L1; 
        Ft1 = e * Fsr;
        Fs1 = Ft2 + Ft1;
        //-----赋予方向成为矢量------------------------------------------------------
        Fs1 = Fs1 - V[1] * damping;
        Fs2 = Fs2 - V[2] * damping;
        //----------------------------------------------------------------------
    }
    void Cal_force()
    {
        Distance();
        Cal_force_pair();
        //----------------------------------------------------------------------
        for (int i = 0; i < nball; i++)
        {
            F[i] = new Vector3(0.0f, 0.0f, 0.0f);
            for (int j = 0; j < nball; j++)
            {
                if (j != i && D[i, j] < force_radius)
                {
                    F[i] = F[i] + F_pair[i, j];
                }
            }
            //------------------------------------------------------------------
            F[i].y = F[i].y - m * g;
            //------------------------------------------------------------------
        }
        //----------------------------------------------------------------------
        F[1] = F[1] + Fs1;//中球所受力
        F[2] = F[2] + Fs2;//底球所受力
        //----------------------------------------------------------------------

        for (int i = 0; i < nball; i++)
        {
            a[i] = F[i] / m;//根据F=m*a计算加速度
        }
        a[0] = new Vector3(0.0f, 0.0f, 0.0f);//顶球加速度始终为0
    }

    //开始自适应四阶龙格库塔法
    void Update_xv(int N, Vector3[] X, Vector3[] V, float dt, int ndyn, int itype)//nball, X, V, dt, ndyn, 1
    {

        //先“自适应”出步长---------------------------------------------------------------------

        Cal_force();
        for (int i = 0; i < N; i++)
        {
            ah1[i].x = a[i].x;
            ah1[i].y = a[i].y;
            ah1[i].z = a[i].z;
        }//得到以dt为步长的加速度

        dt = dt / 2.0f;//步长减半
        Cal_force();
        for (int i = 0; i < N; i++)
        {
            ah2[i].x = a[i].x;
            ah2[i].y = a[i].y;
            ah2[i].z = a[i].z;
        }//得到以dt/2为步长的加速度

        
       if ((ah1[1] - ah2[1]).magnitude > upsilon && (ah1[2] - ah2[2]).magnitude > upsilon)//当Δ>upsilon
        {
            while ((ah1[1] - ah2[1]).magnitude > upsilon && (ah1[2] - ah2[2]).magnitude > upsilon)
            {
                for (int i = 0; i < N; i++)
                {
                    ah1[i].x = a2[i].x;
                    ah1[i].y = a2[i].y;
                    ah1[i].z = a2[i].z;
                }//得到以dt/2为步长的加速度

                dt = dt / 2.0f;//步长减半
                Cal_force();
                for (int i = 0; i < N; i++)
                {
                    ah2[i].x = a[i].x;
                    ah2[i].y = a[i].y;
                    ah2[i].z = a[i].z;
                }//得到以dt/4为步长的加速度
                 //如此往复
            }
        }else if ((ah1[1] - ah2[1]).magnitude < upsilon && (ah1[2] - ah2[2]).magnitude < upsilon)//当Δ<upsilon
        {
            while ((ah1[1] - ah2[1]).magnitude < upsilon && (ah1[2] - ah2[2]).magnitude < upsilon)
            {
                for (int i = 0; i < N; i++)
                {
                    ah1[i].x = a2[i].x;
                    ah1[i].y = a2[i].y;
                    ah1[i].z = a2[i].z;
                }//得到以dt/2为步长的加速度

                dt = dt * 2.0f;//步长加倍
                Cal_force();
                for (int i = 0; i < N; i++)
                {
                    ah2[i].x = a[i].x;
                    ah2[i].y = a[i].y;
                    ah2[i].z = a[i].z;
                }//得到以dt为步长的加速度
                 //如此往复
            }

        }   
        //----------------------------------------------------------------------------------------------
        for (int idyn=1; idyn<ndyn; idyn++)
        {
            for (int i = 0; i < N; i++)
            {
                V0[i].x = V[i].x;
                V0[i].y = V[i].y;
                V0[i].z = V[i].z;
                X0[i].x = X[i].x;
                X0[i].y = X[i].y;
                X0[i].z = X[i].z;
            }

            //第一阶----------------------------------
            for (int i = 0; i < N; i++)
            {
                V1[i].x = V0[i].x; 
                V1[i].y = V0[i].y;
                V1[i].z = V0[i].z;
                X1[i].x = X0[i].x;
                X1[i].y = X0[i].y;
                X1[i].z = X0[i].z;
            }
            
            Cal_force();//由X得到a
            
            for (int i = 0; i < N; i++)
            {
                a1[i].x = dt * a[i].x;
                a1[i].y = dt * a[i].y;
                a1[i].z = dt * a[i].z;
            }
            for (int i = 0; i < N; i++)
            {
                V1[i].x = dt * V1[i].x;
                V1[i].y = dt * V1[i].y;
                V1[i].z = dt * V1[i].z;
            }

            //第二阶--------------------------------------
            for (int i = 0; i < N; i++)
            {
                X2[i].x = X0[i].x + (V1[i].x * 0.5f);
                X2[i].y = X0[i].y + (V1[i].y * 0.5f);
                X2[i].z = X0[i].z + (V1[i].z * 0.5f);
            }

            for (int i = 0; i < N; i++)
            {
                V2[i].x = V0[i].x + (a1[i].x * 0.5f);
                V2[i].y = V0[i].y + (a1[i].y * 0.5f);
                V2[i].z = V0[i].z + (a1[i].z * 0.5f);
            }

            for (int i = 0; i < N; i++)
            {
                V[i].x = V2[i].x;
                V[i].y = V2[i].y;
                V[i].z = V2[i].z;
                X[i].x = X2[i].x;
                X[i].y = X2[i].y;
                X[i].z = X2[i].z;
            }

            Cal_force();//由X得到a

            for (int i = 0; i < N; i++)
            {
                a2[i].x = dt * a[i].x;
                a2[i].y = dt * a[i].y;
                a2[i].z = dt * a[i].z;
            }

            for (int i = 0; i < N; i++)
            {
                V2[i].x = dt * V2[i].x;
                V2[i].y = dt * V2[i].y;
                V2[i].z = dt * V2[i].z;
            }

            //第三阶--------------------------------------
            for (int i = 0; i < N; i++)
            {
                X3[i].x = X0[i].x + (V2[i].x * 0.5f);
                X3[i].y = X0[i].y + (V2[i].y * 0.5f);
                X3[i].z = X0[i].z + (V2[i].z * 0.5f);
            }

            for (int i = 0; i < N; i++)
            {
                V3[i].x = V0[i].x + (a2[i].x * 0.5f);
                V3[i].y = V0[i].y + (a2[i].y * 0.5f);
                V3[i].z = V0[i].z + (a2[i].z * 0.5f);
            }

            for (int i = 0; i < N; i++)
            {
                V[i].x = V3[i].x;
                V[i].y = V3[i].y;
                V[i].z = V3[i].z;
                X[i].x = X3[i].x;
                X[i].y = X3[i].y;
                X[i].z = X3[i].z;
            }

            Cal_force();//由X得到a

            for (int i = 0; i < N; i++)
            {
                a3[i].x = dt * a[i].x;
                a3[i].y = dt * a[i].y;
                a3[i].z = dt * a[i].z;
            }

            for (int i = 0; i < N; i++)
            {
                V3[i].x = dt * V3[i].x;
                V3[i].y = dt * V3[i].y;
                V3[i].z = dt * V3[i].z;
            }

            //第四阶----------------------------------
            for (int i = 0; i < N; i++)
            {
                V4[i].x = V0[i].x + a3[i].x;
                V4[i].y = V0[i].y + a3[i].y;
                V4[i].z = V0[i].z + a3[i].z;
                X4[i].x = X0[i].x + V3[i].x;
                X4[i].y = X0[i].y + V3[i].y;
                X4[i].z = X0[i].z + V3[i].z;
            }

            for (int i = 0; i < N; i++)
            {
                V[i].x = V4[i].x;
                V[i].y = V4[i].y;
                V[i].z = V4[i].z;
                X[i].x = X4[i].x;
                X[i].y = X4[i].y;
                X[i].z = X4[i].z;
            }

            Cal_force();//由X得到a

            for (int i = 0; i < N; i++)
            {
                a4[i].x = dt * a[i].x;
                a4[i].y = dt * a[i].y;
                a4[i].z = dt * a[i].z;
            }

            for (int i = 0; i < N; i++)
            {
                V4[i].x = dt * V4[i].x;
                V4[i].y = dt * V4[i].y;
                V4[i].z = dt * V4[i].z;
            }

            //加权平均----------------------------------
            for (int i = 0; i < N; i++)
            {
                dV[i].x = a1[i].x + 2.0f * (a2[i].x + a3[i].x) + a4[i].x;
                dV[i].y = a1[i].y + 2.0f * (a2[i].y + a3[i].y) + a4[i].y;
                dV[i].z = a1[i].z + 2.0f * (a2[i].z + a3[i].z) + a4[i].z;
            }
            for (int i = 0; i < N; i++)
            {
                V[i].x = V0[i].x + dV[i].x/6.0f;
                V[i].y = V0[i].y + dV[i].y/6.0f;
                V[i].z = V0[i].z + dV[i].z/6.0f;
            }

            for (int i = 0; i < N; i++)
            {
                dXX[i].x = V1[i].x + 2.0f * (V2[i].x + V3[i].x) + V4[i].x;
                dXX[i].y = V1[i].y + 2.0f * (V2[i].y + V3[i].y) + V4[i].y;
                dXX[i].z = V1[i].z + 2.0f * (V2[i].z + V3[i].z) + V4[i].z;
            }
            for (int i = 0; i < N; i++)
            {
                X[i].x = X0[i].x + dXX[i].x / 6.0f;
                X[i].y = X0[i].y + dXX[i].y / 6.0f;
                X[i].z = X0[i].z + dXX[i].z / 6.0f;
            }
            Get_bond();
        }
    }


    // 每帧更新一次
    void Update()
    {
        

    }

   
}