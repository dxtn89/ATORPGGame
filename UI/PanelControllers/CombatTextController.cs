﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatTextController : MonoBehaviour
{
    [SerializeField]
    private Text text;

    [SerializeField]
    private Image image;

    [SerializeField]
    private float xUIOffset = 50f;

    [SerializeField]
    private float yUnitOffset = 2.2f;

    // pixels per second up or down
    //[SerializeField]
    private float movementSpeed = 1.0f;

    [SerializeField]
    private float fadeTime;

    private string displayText;
    private GameObject mainTarget;
    private float alpha;
    private Vector2 targetPos;
    private float fadeOutTimer;
    private float fadeRate;
    private Color textColor;
    private CombatMagnitude combatMagnitude;
    private CombatType textType;

    private float randomXLimit = 100f;
    private float randomYLimit = 50f;
    private float randomX;
    private float randomY;

    // change direction to downward text for hits against player
    private int directionMultiplier = 1;


    public string MyDisplayText { get => displayText; set => displayText = value; }
    public GameObject MyMainTarget { get => mainTarget; set => mainTarget = value; }
    public CombatMagnitude MyCombatMagnitude { get => combatMagnitude; set => combatMagnitude = value; }
    public CombatType MyCombatType { get => textType; set => textType = value; }
    public Image MyImage { get => image; set => image = value; }

    void Start() {
        //Debug.Log("Combat Text spawning: " + textType);
        randomX = Random.Range(0, randomXLimit);
        randomY = Random.Range(0, randomYLimit);
        //Debug.Log("Combat Text spawning: " + textType + "; randomX: " + randomX + "; randomY: " + randomY);
        targetPos = Camera.main.WorldToScreenPoint(mainTarget.transform.position);
        alpha = text.color.a;
        fadeOutTimer = fadeTime;
        fadeRate = 1.0f / fadeTime;

        string preText = string.Empty;
        string postText = string.Empty;

        if (image.sprite == null) {
            image.color = new Color32(0, 0, 0, 0);
        }
        if (mainTarget == PlayerManager.MyInstance.MyPlayerUnitObject) {
            directionMultiplier = -1;
            switch (textType) {
                case CombatType.normal:
                    textColor = Color.red;
                    int parseResult;
                    if (int.TryParse(displayText, out parseResult)) {
                        preText += parseResult > 0 ? "-" : "";
                    }
                    break;
                case CombatType.gainXP:
                    textColor = Color.yellow;
                    preText += "+";
                    postText += " XP";
                    text.fontSize = text.fontSize * 2;
                    break;
                case CombatType.gainBuff:
                    textColor = Color.cyan;
                    preText += "+";
                    //text.fontSize = text.fontSize * 2;
                    break;
                case CombatType.loseBuff:
                    textColor = Color.cyan;
                    preText += "+";
                    //text.fontSize = text.fontSize * 2;
                    break;
                case CombatType.ability:
                    break;
                default:
                    break;
            }
        } else {
            switch (textType) {
                case CombatType.normal:
                    textColor = Color.white;
                    break;
                case CombatType.ability:
                    textColor = Color.yellow;
                    text.fontSize = text.fontSize * 2;
                    break;
                default:
                    break;
            }
        }
        // defaults
        switch (textType) {
            case CombatType.gainHealth:
                textColor = Color.green;
                preText += "+";
                text.fontSize = text.fontSize * 2;
                break;
            case CombatType.gainMana:
                textColor = Color.blue;
                preText += "+";
                text.fontSize = text.fontSize * 2;
                break;
            default:
                break;
        }

        text.color = textColor;
        string finalString = preText + displayText + postText;
        text.text = finalString;
        if (MyCombatMagnitude == CombatMagnitude.critical) {
            text.fontSize = text.fontSize * 2;
        }
    }

    void FixedUpdate() {
        //Debug.Log("CombatTextController.FixedUpdate()");
        if (mainTarget != null) {
            //Debug.Log("CombatTextController.FixedUpdate(): maintarget is not null");
            targetPos = Camera.main.WorldToScreenPoint(mainTarget.transform.position + new Vector3(0, yUnitOffset, 0));
            //Debug.Log("CombatTextController.FixedUpdate(): targetpos:" + targetPos);
            transform.position = targetPos + new Vector2(randomX + xUIOffset, randomY);
        } else {
            //Debug.Log("CombatTextController.FixedUpdate(): maintarget is null");
        }
        if (fadeOutTimer > 0) {
            fadeOutTimer -= Time.fixedDeltaTime;
            
            alpha -= fadeRate * Time.fixedDeltaTime;
            Color tmp = text.color;
            tmp.a = alpha;
            text.color = tmp;
            
            randomY += (movementSpeed * directionMultiplier);
        } else {
            Destroy(this.gameObject);
        }
    }
}
