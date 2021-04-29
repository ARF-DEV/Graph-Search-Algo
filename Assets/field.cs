using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class tile
{
    private Vector2 pos;
    public int state;
    public tile prev = null;
    public GameObject obj;
    public SpriteRenderer spriteRenderer;

    public tile(int x, int y, GameObject o)
    {
        pos.x = x;
        pos.y = y;
        state = 1;
        obj = o;
        spriteRenderer = obj.GetComponent<SpriteRenderer>();
    }

    public void setState(int b)
    {
        
        state = b;
    }

    public bool isWalkable()
    {
        return (state & 1) == 1;
    }


    public bool isGoals()
    {
        return (state & 2) == 2;
    }

    public bool isStart()
    {
        return (state & 4) == 4;
    }
    public Vector2 getPos()
    {
        return pos;
    }
}


public class field : MonoBehaviour
{
    
    public int fieldWidth;
    public int fieldHeight;
    public int tileWidth, tileHeight;

    public Text TypeText;
    public GameObject tileObj;
    private tile[] tileField;
    private tile start = null, end = null;

    private float trueWidth, trueHeight;
    
    int state = 1;
    bool SolveType = false; // False = BFS; True = DFS

    
    void Start()
    {
        
        TypeText.text = "Breath First Search";
        
        
        tileField = new tile[fieldWidth * fieldHeight];
        trueWidth = fieldWidth * tileWidth;
        trueHeight = fieldHeight * tileHeight; 

        for (int i = 0; i < fieldHeight; i++)
        {
            for (int j = 0; j < fieldWidth; j++)
            {
               tileField[i * fieldWidth + j] = new tile(j * tileWidth, i * tileHeight, Instantiate(tileObj, new Vector3(j * tileWidth, i * tileHeight, 0), Quaternion.identity));
            }
        }

        Debug.Log("done");
    }
    
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tile curTile = GetTile(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));
            if (curTile != null)
            {
                // Debug.Log(curTile.getPos());

                SpriteRenderer sprt = curTile.obj.GetComponent<SpriteRenderer>();
                
                
                curTile.setState(state);

                if (curTile.isGoals())
                {
                    end = curTile;
                    curTile.spriteRenderer.color = Color.green;
                }
                else if (curTile.isStart())
                {
                    start = curTile;
                    curTile.spriteRenderer.color = Color.red;
                }
                else if (curTile.isWalkable())
                {
                    curTile.spriteRenderer.color = Color.white;
                }
                else
                {
                    curTile.spriteRenderer.color = Color.black;
                }
                
                Debug.Log(state);
                
            }
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tile curTile = GetTile(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));
            if (curTile != null)
            {
                Debug.Log(curTile.state);
                Debug.Log(curTile.isGoals());
            }
            
        }

        if (SolveType)
            TypeText.text = "Depth First Search";
        else
            TypeText.text = "Breath First Search";
    }

    
    public void OnSolveClick()
    {
        if (SolveType)
        {
            solver s = new solver(this, start, end);
            StartCoroutine(s.DFSsolve());
        }
        else
        {
            solver s = new solver(this, start, end);
            StartCoroutine(s.BFSsolve());
        }
    }

    public void onSetNotWalkable()
    {
        state = 0;
    }

    public void onSetWalkable()
    {
        state = 1;
    }

    public void onSetGoal()
    {
        state = 2;
    }

    public void onSetStart()
    {
        state = 4;
    }

    public void OnClickChange()
    {
        SolveType = !SolveType;
    }

    public void OnReset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public tile GetTile(int x, int y)
    {
        
        return (0 <= x  && x <= trueWidth && 0 <= y && y <= trueHeight) ? tileField[y * fieldWidth + x] : null;
    }

    public void SetTileType(int x, int y, int state)
    {
       GetTile(x,y).setState(state);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (tile tile in tileField)
        {
            Gizmos.DrawWireCube((Vector2) tile.getPos(), new Vector2(tileWidth, tileHeight));
        }
    }

    public field GetInstance()
    {
        return this;
    }

    public List<tile> GetNeighbor(tile cur)
    {
        List<tile> p = new List<tile>();
        Vector2 pos = cur.getPos();
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);

        if (y + 1 <= fieldHeight)
        {
            p.Add(tileField[(y + 1) * fieldWidth + x]);
        }
        if (y - 1 >= 0)
        {
            p.Add(tileField[(y - 1) * fieldWidth + x]);
        }
        if (x + 1 <= fieldWidth)
        {
            p.Add(tileField[y * fieldWidth + (x + 1)]);
        }
        if (x - 1 >= 0)
        {
            p.Add(tileField[y * fieldWidth + (x - 1)]);
        }

        return p;
    
    }
    
}
public class solver
{   
    private field field;
    private tile start, end;
    private List<tile> visited;
    public solver(field field, tile start, tile end)
    {
        this.field = field;
        this.start = start;
        this.end = end;
        visited = new List<tile>();
    }
    public IEnumerator BFSsolve()
    {
        Queue<tile> waiting = new Queue<tile>();
        waiting.Enqueue(start);
        visited.Add(start);
        bool found = false;
        while (waiting.Count != 0)
        {
            yield return new WaitForSeconds(0.1f);
            tile cur = waiting.Dequeue();
            if (!cur.isStart() && !cur.isGoals())
            {
                cur.spriteRenderer.color = Color.cyan;
            }
            
            if (cur.isGoals())
            {
                found = true;
                break;
            }

            List<tile> neighbors = field.GetNeighbor(cur);

            foreach (tile t in neighbors)
            {
                if (!visited.Contains(t) && (t.isWalkable() || t.isGoals()))
                {
                    waiting.Enqueue(t);
                    t.prev = cur;
                }
                visited.Add(cur);
            }
        }

        if (found)
        {
            tile c = end;
            while (c.prev != null)
            {
                c = c.prev;
                if (!c.isGoals() && !c.isStart())
                    c.spriteRenderer.color = Color.green;
                
            }
            //c.spriteRenderer.color = Color.green;
        }
        else
        {
            Debug.Log("solution not found");
        }
    }

    public IEnumerator DFSsolve()
    {
        Stack<tile> waiting = new Stack<tile>();
        
        waiting.Push(start);
        visited.Add(start);
        bool found = false;
        while (waiting.Count != 0)
        {
            yield return new WaitForSeconds(0.1f);
            tile cur = waiting.Pop();
            if (!cur.isStart() && !cur.isGoals())
            {
                cur.spriteRenderer.color = Color.cyan;
            }
            
            if (cur.isGoals())
            {
                found = true;
                break;
            }

            List<tile> neighbors = field.GetNeighbor(cur);

            foreach (tile t in neighbors)
            {
                if (!visited.Contains(t) && (t.isWalkable() || t.isGoals()))
                {
                    waiting.Push(t);
                    t.prev = cur;
                }
                visited.Add(cur);
            }
        }

        if (found)
        {
            tile c = end;
            while (c.prev != null)
            {
                c = c.prev;
                if (!c.isGoals() && !c.isStart())
                    c.spriteRenderer.color = Color.green;
                
            }
            //c.spriteRenderer.color = Color.green;
        }
        else
        {
            Debug.Log("solution not found");
        }
    }
}
