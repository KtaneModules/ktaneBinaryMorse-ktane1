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

    private string[] morseAlphabet = { "101100", "1101010100", "11010110100", "11010100", "100", "1010110100", "110110100", "101010100", "10100", "101101101100", "110101100", "1011010100", "1101100", "110100", "1101101100", "10110110100", "110110101100", "10110100", "1010100", "1100", "10101100", "1010101100", "101101100", "11010101100", "110101101100", "11011010100" };
    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int[] selectedLettersIndex = new int[5];
    private int[] index = new int[5];
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
        while (initialString.Length % 8 != 0)
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
                int rnd = UnityEngine.Random.Range(0, 5);
                switch (rnd)
                {
                    case 0:
                        display.text = "NICE";
                        break;
                    case 1:
                        display.text = "COOL";
                        break;
                    case 2:
                        display.text = "YAY";
                        break;
                    case 3:
                        display.text = "EPIC";
                        break;
                    case 4:
                        display.text = "!!!";
                        break;
                }
                input.text = "";
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

    //Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} type 69420 to type 69420 into the module, !{0} submit to submit, !{0} clear to clear the display";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            submit.OnInteract();
            yield break;
        }
        else if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            clear.OnInteract();
            yield break;
        }
        string[] parameters = command.Split(' ');
        bool untypable = false;
        if (Regex.IsMatch(parameters[0], @"^\s*type\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            for (int i = 0; i < parameters[1].Length; i++)
            {
                if (parameters[1][i] - '0' < 0 || parameters[1][i] - '0' > 9)
                {
                    untypable = true;
                }     
            }
            if (parameters[1] == "")
            {
                yield return "sendtochaterror There's nothing to type.";
                yield break;
            }
            else if (untypable)
            {
                yield return "sendtochaterror The module can only type numbers.";
                yield break;
            }
            else
            {
                clear.OnInteract();
                yield return null;
                for (int i = 0; i < parameters[1].Length; i++)
                {
                    yield return new WaitForSeconds(0.1f);
                    string comparer = parameters[1].ElementAt(i) + "";
                    for (int j = 0; j < 10; j++)
                    {
                        if (comparer.Equals(j.ToString()))
                        {
                            buttons[j].OnInteract();
                        }
                    }

                }
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            for (int i = 0; i < finalAnswer.ToString().Length; i++)
            {
                buttons[finalAnswer.ToString()[i] - '0'].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            submit.OnInteract();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
