using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Navigate : MonoBehaviour {
    //TODO: Decouple main game logic from navigation and ensure grid generation and other move related features follow const parameters
    const float moveMagnitude = 5.0f;
    const float torchConsumeMagnitude = 3.0f;
    const float topRightBound = 100.0f;
    const string refillID = "refill";

    //TODO: Rebalance torch for more limited usage
    private int totalMoves = 0;
    private float torchEnergy = 100.0f;
    private bool usingTorch = true;
    private Vector2 playerPosition = new Vector2(50.0f, 50.0f);
    private Waypoint goal;
    private Waypoint refill1;
    private Waypoint refill2;
    private List<Waypoint> waypoints;
    private TextMeshProUGUI text_prompt;
    private Toggle toggle_torch;
    private AudioSource ambientAudioSource;
    private AudioSource oneshotAudioSource;

    [Header("Audio Fields")]
    public AudioClip ambientClip;
    public AudioClip winClip;
    public AudioClip refillClip;
    public List<AudioClip> moveClips;
    public List<AudioClip> collisionClips;


    void Start() {
        UnityAction goalAction = () => {
            triggerOneShotAudio(new List<AudioClip> { winClip });
            Vector2 goalPos = goal.getPos();
            text_prompt.text = string.Format("GAME OVER!\nYou found the pixie nest at ({0},{1})\nafter {2} moves!", goalPos.x.ToString("0.0"), goalPos.y.ToString("0.0"), totalMoves);
        };
        UnityAction refillAction = () => {
            triggerOneShotAudio(new List<AudioClip> { refillClip });
            text_prompt.text = "You stumbled upon a torch refill...\nTorch energy was completely replenished!";
            torchEnergy = 100.0f;
        };
        goal = new Waypoint("goal", topRightBound, goalAction);
        refill1 = new Waypoint(refillID, topRightBound, refillAction, 30.0f);
        refill2 = new Waypoint(refillID, topRightBound, refillAction, 30.0f);
        waypoints = new List<Waypoint>();
        waypoints.AddRange(new List<Waypoint> { goal, refill1, refill2 });
        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        oneshotAudioSource = gameObject.AddComponent<AudioSource>();
        Transform canvas = GameObject.Find("Canvas").transform;

        Transform controlUI =  canvas.Find("Controls");
        controlUI.Find("button_up").gameObject.GetComponent<Button>().onClick.AddListener(() => {
            updatePosition(new Vector2(0, 1));
        });
        controlUI.Find("button_down").gameObject.GetComponent<Button>().onClick.AddListener(() => {
            updatePosition(new Vector2(0, -1));
        });
        controlUI.Find("button_left").gameObject.GetComponent<Button>().onClick.AddListener(() => {
            updatePosition(new Vector2(-1, 0));
        });
        controlUI.Find("button_right").gameObject.GetComponent<Button>().onClick.AddListener(() => {
            updatePosition(new Vector2(1, 0));
        });

        toggle_torch = canvas.Find("toggle_torch").GetComponent<Toggle>();
        toggle_torch.onValueChanged.AddListener((bool b) => {
            toggleTorch(b);
        });

        text_prompt = canvas.Find("text_prompt").GetComponent<TextMeshProUGUI>();
        
        updateAmbianceSpatialization();
        ambientAudioSource.clip = ambientClip;
        ambientAudioSource.Play();
    }

    /**
     * <summary>
     * Applies strictly horizontal/vertical movement within vertical grid range previously specified
     * </summary>
     * <param name="direction">The 2D move direction vector</param>
     * <returns>Nothing</returns>
     */
    public void updatePosition(Vector2 direction) {
        /* Attempt move in cardinal direction if possible by updating player position, move count, AND torch status, OR notifying of boundary collision, 
         * THEN attempt all Waypoints' 'reached events'*/
        if (!goal.wasReached()) {
            Vector2 moveDiff = direction * moveMagnitude;
            bool movingVertical = (moveDiff.y > 0 && playerPosition.y <= 95) || (moveDiff.y < 0 && playerPosition.y >= 5);
            bool movingHorizontal = (moveDiff.x > 0 && playerPosition.x <= 95) || (moveDiff.x < 0 && playerPosition.x >= 5);

            if (movingVertical || movingHorizontal) {
                triggerOneShotAudio(moveClips);
                playerPosition += moveDiff;
                totalMoves++;
                updateTorch();
            }
            else {
                triggerOneShotAudio(collisionClips);
                Debug.Log("YOU HIT A WALL");
            }
        }

        foreach (Waypoint wp in waypoints) {
            if (wp.getId() == refillID && torchEnergy == 100) {
                continue;
            }
            wp.attemptReachedEvent(playerPosition);
        }
    }

    /**
     * <summary>
     * One-shots an indexed AudioClip from AudioSource if specified, otherwise uses random clip
     * </summary>
     * <param name="clips">AudioClip List to query from</param>
     * <param name="index">'clips' List index to use</param>
     * 
     * <returns>Nothing</returns>
     */
    private void triggerOneShotAudio(List<AudioClip> clips, int index = -1) {
        if (index == -1) {
            index = UnityEngine.Random.Range(0, clips.Count);
        }
        oneshotAudioSource.PlayOneShot(clips[index]);
    }

    /**
     * <summary>
     * Updates torch status and UI according to 'usage' flag and torch energy left
     * </summary>
     * <returns>Nothing</returns>
     */
    private void updateTorch() {
        /* Attempt consumption of torch energy, 
         * THEN attempt correction of torch energy deduction, 
         * THEN attempt toggling off torch if deactivated or drained */
        if (usingTorch && torchEnergy > 0) {
            torchEnergy -= torchConsumeMagnitude;
            updateAmbianceSpatialization();
            text_prompt.text = string.Format("Current Position: ({0},{1})\nTorch Energy: {2}\nCURRENT DISTANCE FROM GOAL: {3}",
                playerPosition.x, playerPosition.y, torchEnergy, Vector2.Distance(playerPosition, goal.getPos()));
        }
        if (torchEnergy < 0) {
            torchEnergy = 0;
        }
        if (torchEnergy == 0 || !usingTorch) {
            toggleTorch(false);
        }
    }

    /**
     * <summary>
     * Toggles torch energy usage
     * </summary>
     * <param name="toggle"></param>
     * <returns>Nothing</returns>
     */
    private void toggleTorch(bool newStatus) {
        usingTorch = newStatus;
        //triggerOneShotAudio(responseAudioSource, collisionClips);
        string statusStr = (!usingTorch || torchEnergy <= 0) ? "Off" : "On";
        toggle_torch.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = "Torchlight Status: " + statusStr;
        updatePrompt();
    }

    private void updatePrompt() {
        if (usingTorch) {
            updateAmbianceSpatialization();
            text_prompt.text = string.Format("Current Position: ({0},{1})\nTorch Energy: {2}\nCURRENT DISTANCE FROM GOAL: {3}",
                playerPosition.x, playerPosition.y, torchEnergy, Vector2.Distance(playerPosition, goal.getPos()));
            
        } else {
            ambientAudioSource.volume = 0.0f;
            text_prompt.text = string.Format("ITS ALL DARK!\nTorch Energy: {0}", torchEnergy);
        }
    }

    private void updateAmbianceSpatialization() {
        ambientAudioSource.volume = 0.5f - (Mathf.Abs(Vector2.Distance(playerPosition, goal.getPos())) / 100.0f);
    }
}