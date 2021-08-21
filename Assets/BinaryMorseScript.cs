using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class BinaryMorseScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
	public KMBombModule module;
    public KMSelectable[] buttons;
    public KMSelectable submit, clear;
    public TextMesh display;
    public TextMesh input;

    private string[] morseAlphabet = { "10111000", "111010101000", "11101011101000", "1110101000", "1000", "101011101000", "111011101000", "1010101000", "101000", "1011101110111000", "111010111000", "101110101000", "1110111000", "11101000", "11101110111000", "10111011101000", "1110111010111000", "1011101000", "10101000", "111000", "1010111000", "101010111000", "101110111000", "11101010111000", "1110101110111000", "11101110101000" };
    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int[] selectedLettersIndex = new int[7];
    private int[] index = new int[7];
    private string currentInput;
    private int finalAnswer;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved, displayCheck;

    void Awake()
    {
    	moduleId = moduleIdCounter++;

        for (int i = 0; i < buttons.Length; i++)
        {
            int j = i;
            buttons[j].OnInteract += () => { buttonHandler(j); return false; };
        }
        submit.OnInteract += delegate () { submitInput(); return false; };
        clear.OnInteract += delegate () { clearInput(); return false; };
    }

    void Start()
    {
        string selectedLetters = "";
        for (int i = 0; i < selectedLettersIndex.Length; i++)
        {
            selectedLettersIndex[i] = UnityEngine.Random.Range(0, alphabet.Length);
            selectedLetters += alphabet[selectedLettersIndex[i]];
            index[i] = UnityEngine.Random.Range(0, morseAlphabet[selectedLettersIndex[i]].Length);
        }
        Debug.LogFormat("[Binary Morse #{0}] Module initiated! The selected letters are {1}.", moduleId, selectedLetters);
        StartCoroutine("displayNumbers");
        numberModification();
    }

    IEnumerator displayNumbers()
    {
        if (!moduleSolved)
        {
            string nextNumber = "";
            for (int i = 0; i < index.Length; i++)
            {
                string placeholder = morseAlphabet[selectedLettersIndex[i]];
                nextNumber += placeholder[index[i]];
                index[i]++;
                if (index[i] >= morseAlphabet[selectedLettersIndex[i]].Length)
                {
                    index[i] = 0;
                }
            }
            nextNumber = Convert.ToInt32(nextNumber, 2).ToString();
            display.text = nextNumber;
            yield return new WaitForSecondsRealtime(1);
            displayCheck = true;
            yield return null;
        }
    }

    void numberModification()
    {
        string initialString = "";
        for (int i = 0; i < selectedLettersIndex.Length; i++)
        {
            initialString += morseAlphabet[selectedLettersIndex[i]];
        }
        initialString = initialString.TrimEnd('0');
        while (!(initialString.Length % 8 == 0))
        {
            initialString = "0" + initialString;
        }
        Debug.LogFormat("[Binary Morse #{0}] The whole concatenated binary string is {1}.", moduleId, initialString);
        string[] stringBreaks = new string[initialString.Length / 8];
        for (int i = 0; i < initialString.Length; i++)
        {
            stringBreaks[i / 8] += initialString[i];
        }
        for (int i = 0; i < stringBreaks.Length; i++)
        {
            finalAnswer += Convert.ToInt32(stringBreaks[i], 2);
        }
        Debug.LogFormat("[Binary Morse #{0}] The final sum is {1}.", moduleId, finalAnswer);
    }

    void buttonHandler(int k)
    {
        if (!moduleSolved)
        {
            audio.PlaySoundAtTransform("button", transform);
            buttons[k].AddInteractionPunch(0.25f);
            currentInput += k.ToString();
            input.text = currentInput;
        }
    }

    void clearInput()
    {
        if (!moduleSolved)
        {
            audio.PlaySoundAtTransform("button", transform);
            clear.AddInteractionPunch(0.5f);
            currentInput = "";
            input.text = "";
        }
    }

    void submitInput()
    {
        if (!moduleSolved)
        {
            submit.AddInteractionPunch(0.5f);
            audio.PlaySoundAtTransform("button", transform);
            if (currentInput == finalAnswer.ToString())
            {
                module.HandlePass();
                moduleSolved = true;
                audio.PlaySoundAtTransform("correct", transform);
                display.text = "NICE";
                input.text = "YAY YOU DID IT";
                Debug.LogFormat("[Binary Morse #{0}] Correct answer submitted. Module solved.", moduleId);
            }
            else
            {
                module.HandleStrike();
                audio.PlaySoundAtTransform("wrong", transform);
                Debug.LogFormat("[Binary Morse #{0}] A wrong answer ({1}) is submitted, strike.", moduleId, currentInput);
                input.text = "";
                currentInput = "";
            }
        }
    }

    void Update() //Runs every frame.
    {
        if (displayCheck)
        {
            StartCoroutine("displayNumbers");
            displayCheck = false;
        }          
    }

    
}
