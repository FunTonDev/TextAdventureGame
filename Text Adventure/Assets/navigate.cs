using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//CLASS FOR CREATION OF GOAL OBJECT
public class goal
{
    Vector2 goalPos;
    bool foundStatus;
    public goal()
    {
        goalPos = new Vector2(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f));
        foundStatus = false;
    }
    public Vector2 getGoalPos()
    {
        return goalPos;
    }
    public bool getStatus()
    {
        return foundStatus;
    }
    public void goalFound()
    {
        foundStatus = true;
    }

}

//CLASS FOR CREATION OF TORCHREFILL OBJECT
public class torchRefill
{
    Vector2 refillPos;
    bool consumedStatus;
    public torchRefill()
    {
        refillPos = new Vector2(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f));
        consumedStatus = false;
    }
    public Vector2 getRefillPos()
    {
        return refillPos;
    }
    public bool getStatus()
    {
        return consumedStatus;
    }
    public void refillFound()
    {
        consumedStatus = true;
    }
}

public class navigate : MonoBehaviour
{
    private Vector2 playerPos = new Vector2(50.0f, 50.0f);
    private int totalMoves = 0;
    private float torchEnergy = 100.0f;
    private bool torchLit = true;
    public AudioSource bump;
    public AudioSource music;
    public AudioSource click;
    public AudioSource click2;
    public AudioSource chime;
    public AudioSource beep;
    goal mainGoal;
    torchRefill refill1;
    torchRefill refill2;
    UnityEngine.UI.Text gameText;

    // Start is called before the first frame update
    void Start()
    {
        mainGoal = new goal();
        refill1 = new torchRefill();
        refill2 = new torchRefill();
        gameText = GameObject.Find("updateText").GetComponent<Text>();
        beep = GameObject.Find("beepSoundClip").GetComponent<AudioSource>();
        chime = GameObject.Find("chimeSoundClip").GetComponent<AudioSource>();
        click2 = GameObject.Find("click2SoundClip").GetComponent<AudioSource>();
        click2.volume = 0.6f;
        click = GameObject.Find("clickSoundClip").GetComponent<AudioSource>();
        click.volume = 0.6f;
        bump = GameObject.Find("errorSoundClip").GetComponent<AudioSource>();
        bump.volume = 0.8f;
        music = GameObject.Find("musicSoundClip").GetComponent<AudioSource>();
        music.volume = 1 - (Mathf.Abs(Vector2.Distance(playerPos, mainGoal.getGoalPos())) / 100.0f);
        music.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //METHOD FOR CHANGING TORCH VALUES AND CHECKING IN-GAME AFFECT AFTER
    void torchChangeCheck()
    {
        if (torchLit && torchEnergy > 0)
        {
            torchEnergy -= 3.0f;
            music.volume = 1 - (Mathf.Abs(Vector2.Distance(playerPos, mainGoal.getGoalPos())) / 100.0f);
        }

        if (torchEnergy <= 0 || !torchLit)
        {
            music.volume = 0.0f;
            if (torchEnergy < 0)
            {
                torchEnergy = 0;
            }
            gameText.text = System.String.Format("ITS ALL DARK!\nTorch Energy: {0}", torchEnergy);
        }
        else
        {
            gameText.text = System.String.Format("Current Position: ({0},{1})\nTorch Energy: {2}\nCURRENT DISTANCE FROM GOAL: {3}",
                playerPos.x, playerPos.y, torchEnergy, Vector2.Distance(playerPos, mainGoal.getGoalPos()));
        }
    }

    //METHOD FOR CHECKING IF PLAYER'S POSITION IS CLOSE TO GOAL OR REFILLS
    void itemPosCheck()
    {
        if (Mathf.Abs(Vector2.Distance(playerPos, mainGoal.getGoalPos())) <= 5 && !mainGoal.getStatus())
        {
            chime.Play();
            mainGoal.goalFound();
            gameText.text = System.String.Format("GAME OVER!\nYou found the pixie nest at ({0},{1})\nafter {2} moves!", mainGoal.getGoalPos().x.ToString("0.0"), mainGoal.getGoalPos().y.ToString("0.0"), totalMoves);
        }

        if (Mathf.Abs(Vector2.Distance(playerPos, refill1.getRefillPos())) <= 30 && !refill1.getStatus() && torchEnergy < 100)
        {
            beep.Play();
            torchEnergy = 100.0f;
            refill1.refillFound();
            gameText.text = "You stumbled upon a torch refill...\nTorch energy was completely replenished!";
        }

        if (Mathf.Abs(Vector2.Distance(playerPos, refill2.getRefillPos())) <= 30 && !refill2.getStatus() && torchEnergy < 100)
        {
            beep.Play();
            torchEnergy = 100.0f;
            refill2.refillFound();
            gameText.text = "You stumbled upon a torch refill...\nTorch energy was completely replenished!";
        }
    }

    //METHOD FOR TOGGLING TORCH ENERGY USAGE
    public void toggleTorch(GameObject toggler)
    {
        click2.Play();
        if(torchLit)
        {
            torchLit = toggler.GetComponent<UnityEngine.UI.Toggle>().isOn;
            gameText.text = System.String.Format("ITS ALL DARK!\nTorch Energy: {0}", torchEnergy);
            GameObject.Find("torchText").GetComponent<Text>().text = "Torch: Off";
            Debug.Log(System.String.Format("TORCHLIT: {0}", torchLit));
            music.volume = 0.0f;
        }
        else
        {
            if (torchEnergy > 0)
            {
                torchLit = toggler.GetComponent<UnityEngine.UI.Toggle>().isOn;
                gameText.text = System.String.Format("Current Position: ({0},{1})\nTorch Energy: {2}\nCURRENT DISTANCE FROM GOAL: {3}",
                    playerPos.x, playerPos.y, torchEnergy, Vector2.Distance(playerPos, mainGoal.getGoalPos()));
                GameObject.Find("torchText").GetComponent<Text>().text = "Torch: On";
                Debug.Log(System.String.Format("TORCHLIT: {0}", torchLit));
                music.volume = 1 - (Mathf.Abs(Vector2.Distance(playerPos, mainGoal.getGoalPos())) / 100.0f);
            }
        }
    }

    //METHODS FOR ALL MOVEMENT
    public void moveUp()
    {
        click.Play();
        if (playerPos.y < 100 && !mainGoal.getStatus())
        {
            playerPos.y += 5.0f;
            totalMoves++;
            torchChangeCheck();
        }
        else if (playerPos.y >= 100 && !mainGoal.getStatus())
        {
            Debug.Log("YOU HIT A WALL");
            bump.Play();
        }

        itemPosCheck();
    }
    public void moveDown()
    {
        click.Play();
        if (playerPos.y >= 5 && !mainGoal.getStatus())
        {
            playerPos.y -= 5.0f;
            totalMoves++;
            torchChangeCheck();
        }
        else if(playerPos.y <= 5 && !mainGoal.getStatus())
        {
            Debug.Log("YOU HIT A WALL");
            bump.Play();
        }

        itemPosCheck();
    }
    public void moveLeft()
    {
        click.Play();
        if (playerPos.x >= 5 && !mainGoal.getStatus())
        {
            playerPos.x -= 5.0f;
            totalMoves++;
            torchChangeCheck();
        }
        else if(playerPos.x <= 5 && !mainGoal.getStatus())
        {
            Debug.Log("YOU HIT A WALL");
            bump.Play();
        }

        itemPosCheck();
    }
    public void moveRight()
    {
        click.Play();
        if (playerPos.x < 100 && !mainGoal.getStatus())
        {
            playerPos.x += 5.0f;
            totalMoves++;
            torchChangeCheck();
        }
        else if(playerPos.x <= 100 && !mainGoal.getStatus())
        {
            Debug.Log("YOU HIT A WALL");
            bump.Play();
        }

        itemPosCheck();
    }
}
