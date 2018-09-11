using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using reverseMorse;

public class reverseMorseScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable dotButton;
    public KMSelectable dashButton;
    public KMSelectable breakButton;
    public KMSelectable spaceButton;
    public KMSelectable resetButton;

    public TextMesh[] screens;
    public Color[] colorOptions;
    public String[] symbolOptions;
    public String[] letterOptions;
    public Renderer[] passLights;
    public Material[] lightColours;

    private String letterToTransmit1;
    private String letterToTransmit2;
    private int transmissionIndex1 = 0;
    private int transmissionIndex2 = 0;

    public List<String> selectedLetters1 = new List<String>();
    public List<String> selectedLetters2 = new List<String>();

    public String typingLetters;
    public String[] submittedLetters;
    private int letterIndex = 0;
    private int checkingIndex = 0;
    public String[] typedLetters;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    int stage = 1;
    private bool stage1;
    private bool moduleSolved;
    private bool strikeAlarm;
    private bool transmitting;
    private bool noPunch;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        dotButton.OnInteract += delegate () { OnDotButton(); return false; };
        dashButton.OnInteract += delegate () { OnDashButton(); return false; };
        breakButton.OnInteract += delegate () { OnBreakButton(); return false; };
        spaceButton.OnInteract += delegate () { OnSpaceButton(); return false; };
        resetButton.OnInteract += delegate () { OnResetButton(); return false; };
    }

    void Start()
    {
        foreach(Renderer passLight in passLights)
        {
            passLight.material = lightColours[0];
        }
        screens[0].text = "";
        screens[1].text = "";
        typingLetters = "";
        screens[2].text = typingLetters;
        SelectTransmission();
        StartCoroutine(screen1Transmission());
        StartCoroutine(screen2Transmission());
    }

    void SelectTransmission()
    {
        while(selectedLetters1.Count() < 6)
        {
            int index = UnityEngine.Random.Range(0,36);
            selectedLetters1.Add(letterOptions[index]);
        }
        while(selectedLetters2.Count() < 6)
        {
            int index = UnityEngine.Random.Range(0,36);
            selectedLetters2.Add(letterOptions[index]);
        }
        Debug.LogFormat("[Reverse Morse #{0}] Your first message is {1}{2}{3}{4}{5}{6}.", moduleId, selectedLetters1[0], selectedLetters1[1], selectedLetters1[2], selectedLetters1[3], selectedLetters1[4], selectedLetters1[5]);
        Debug.LogFormat("[Reverse Morse #{0}] Your second message is {1}{2}{3}{4}{5}{6}.", moduleId, selectedLetters2[0], selectedLetters2[1], selectedLetters2[2], selectedLetters2[3], selectedLetters2[4], selectedLetters2[5]);;
    }

    IEnumerator screen1Transmission()
    {
        while(!stage1)
        {
            yield return new WaitForSeconds(0.1f);
            letterToTransmit1 = selectedLetters1[transmissionIndex1];
            TransmissionFormat1();
            yield return new WaitForSeconds(1.3f);
            screens[0].text = "";
            yield return new WaitForSeconds(0.4f);
            transmissionIndex1++;
            if(transmissionIndex1 == 6)
            {
                yield return new WaitForSeconds(1f);
                transmissionIndex1 = 0;
            }
        }
    }

    IEnumerator screen2Transmission()
    {
        while(!moduleSolved)
        {
            yield return new WaitForSeconds(0.1f);
            letterToTransmit2 = selectedLetters2[transmissionIndex2];
            TransmissionFormat2();
            yield return new WaitForSeconds(0.9f);
            screens[1].text = "";
            yield return new WaitForSeconds(0.25f);
            transmissionIndex2++;
            if(transmissionIndex2 == 6)
            {
                yield return new WaitForSeconds(1f);
                transmissionIndex2 = 0;
            }
        }
    }

    public void OnDotButton()
    {
        GetComponent<KMSelectable>().AddInteractionPunch(0.5f);
        if(typingLetters.Count() < 5 && !moduleSolved && !transmitting)
        {
            Audio.PlaySoundAtTransform("dot", transform);
            typingLetters += ".";
            screens[2].text = typingLetters;
        }
    }

    public void OnDashButton()
    {
        GetComponent<KMSelectable>().AddInteractionPunch(0.5f);
        if(typingLetters.Count() < 5 && !moduleSolved && !transmitting)
        {
            Audio.PlaySoundAtTransform("dash", transform);
            typingLetters += "-";
            screens[2].text = typingLetters;
        }
    }

    public void OnBreakButton()
    {
        GetComponent<KMSelectable>().AddInteractionPunch();
        if(moduleSolved || transmitting)
        {
            return;
        }
        if(letterIndex > 5)
        {
            typingLetters = "";
            return;
        }
        Audio.PlaySoundAtTransform("click", transform);
        submittedLetters[letterIndex] = typingLetters;
        Debug.LogFormat("[Reverse Morse #{0}] You submitted {1}", moduleId, submittedLetters[letterIndex]);
        passLights[letterIndex].material = lightColours[1];
        letterIndex++;
        typingLetters = "";
        screens[2].text = typingLetters;
    }

    public void OnSpaceButton()
    {
        GetComponent<KMSelectable>().AddInteractionPunch();
        if(moduleSolved || transmitting)
        {
            return;
        }
        checkingIndex = 0;
        Audio.PlaySoundAtTransform("click", transform);

        if(stage == 1)
            {
            foreach(string letters in submittedLetters)
            {
                MorseConversion();
                checkingIndex++;
            }
            for(int i = 0; i<=5; i++)
            {
                if(typedLetters[i] == selectedLetters1[i])
                {

                }
                else
                {
                    strikeAlarm = true;
                }
            }
            if(strikeAlarm)
            {
                transmitting = true;
                StartCoroutine(Strike1());
            }
            else
            {
                transmitting = true;
                StartCoroutine(Pass1());
            }
        }
        else
        {
            foreach(string letters in submittedLetters)
            {
                MorseConversion();
                checkingIndex++;
            }
            for(int i = 0; i<=5; i++)
            {
                if(typedLetters[i] == selectedLetters2[i])
                {

                }
                else
                {
                    strikeAlarm = true;
                }
            }
            if(strikeAlarm)
            {
                transmitting = true;
                StartCoroutine(Strike2());
            }
            else
            {
                transmitting = true;
                StartCoroutine(Pass2());
            }
        }
    }

    public void OnResetButton()
    {
        if(noPunch)
        {
            noPunch = false;
        }
        else
        {
            GetComponent<KMSelectable>().AddInteractionPunch();
        }
        if(moduleSolved || transmitting)
        {
            return;
        }
        Audio.PlaySoundAtTransform("click", transform);
        typingLetters = "";
        screens[2].text = typingLetters;
        letterIndex = 0;
        for(int i = 0; i<= 5; i++)
        {
            submittedLetters[i] = "";
        }
        foreach(Renderer passLight in passLights)
        {
            passLight.material = lightColours[0];
        }
        Debug.LogFormat("[Reverse Morse #{0}] Input reset.", moduleId);
    }

    IEnumerator Pass1()
    {
        yield return new WaitForSeconds(0.5f);
        Audio.PlaySoundAtTransform("transmit", transform);
        yield return new WaitForSeconds (5f);
        Debug.LogFormat("[Reverse Morse #{0}] Your first transmission was {1}{2}{3}{4}{5}{6}. That is correct.", moduleId, typedLetters[0], typedLetters[1], typedLetters[2], typedLetters[3], typedLetters[4], typedLetters[5]);
        stage++;
        stage1 = true;
        transmitting = false;
        noPunch = true;
        OnResetButton();
    }

    IEnumerator Strike1()
    {
        yield return new WaitForSeconds(0.5f);
        Audio.PlaySoundAtTransform("transmit", transform);
        yield return new WaitForSeconds (5f);
        Debug.LogFormat("[Reverse Morse #{0}] Strike! Your first transmission was {1}{2}{3}{4}{5}{6}. That is incorrect.", moduleId, typedLetters[0], typedLetters[1], typedLetters[2], typedLetters[3], typedLetters[4], typedLetters[5]);
        GetComponent<KMBombModule>().HandleStrike();
        strikeAlarm = false;
        transmitting = false;
        noPunch = true;
        OnResetButton();
    }

    IEnumerator Pass2()
    {
        yield return new WaitForSeconds(0.5f);
        Audio.PlaySoundAtTransform("transmit", transform);
        yield return new WaitForSeconds (5f);
        Debug.LogFormat("[Reverse Morse #{0}] Your second transmission was {1}{2}{3}{4}{5}{6}. That is correct. Module disarmed.", moduleId, typedLetters[0], typedLetters[1], typedLetters[2], typedLetters[3], typedLetters[4], typedLetters[5]);
        moduleSolved = true;
        transmitting = false;
        GetComponent<KMBombModule>().HandlePass();
        noPunch = true;
        OnResetButton();
    }

    IEnumerator Strike2()
    {
        yield return new WaitForSeconds(0.5f);
        Audio.PlaySoundAtTransform("transmit", transform);
        yield return new WaitForSeconds (5f);
        Debug.LogFormat("[Reverse Morse #{0}] Strike! Your second transmission was {1}{2}{3}{4}{5}{6}. That is incorrect.", moduleId, typedLetters[0], typedLetters[1], typedLetters[2], typedLetters[3], typedLetters[4], typedLetters[5]);
        GetComponent<KMBombModule>().HandleStrike();
        strikeAlarm = false;
        transmitting = false;
        noPunch = true;
        OnResetButton();
    }

    //Databases
    void MorseConversion()
    {
        if(submittedLetters[checkingIndex] == ".-")
        {
            typedLetters[checkingIndex] = "A";
        }
        else if(submittedLetters[checkingIndex] == "-...")
        {
            typedLetters[checkingIndex] = "B";
        }
        else if(submittedLetters[checkingIndex] == "-.-.")
        {
            typedLetters[checkingIndex] = "C";
        }
        else if(submittedLetters[checkingIndex] == "-..")
        {
            typedLetters[checkingIndex] = "D";
        }
        else if(submittedLetters[checkingIndex] == ".")
        {
            typedLetters[checkingIndex] = "E";
        }
        else if(submittedLetters[checkingIndex] == "..-.")
        {
            typedLetters[checkingIndex] = "F";
        }
        else if(submittedLetters[checkingIndex] == "--.")
        {
            typedLetters[checkingIndex] = "G";
        }
        else if(submittedLetters[checkingIndex] == "....")
        {
            typedLetters[checkingIndex] = "H";
        }
        else if(submittedLetters[checkingIndex] == "..")
        {
            typedLetters[checkingIndex] = "I";
        }
        else if(submittedLetters[checkingIndex] == ".---")
        {
            typedLetters[checkingIndex] = "J";
        }
        else if(submittedLetters[checkingIndex] == "-.-")
        {
            typedLetters[checkingIndex] = "K";
        }
        else if(submittedLetters[checkingIndex] == ".-..")
        {
            typedLetters[checkingIndex] = "L";
        }
        else if(submittedLetters[checkingIndex] == "--")
        {
            typedLetters[checkingIndex] = "M";
        }
        else if(submittedLetters[checkingIndex] == "-.")
        {
            typedLetters[checkingIndex] = "N";
        }
        else if(submittedLetters[checkingIndex] == "---")
        {
            typedLetters[checkingIndex] = "O";
        }
        else if(submittedLetters[checkingIndex] == ".--.")
        {
            typedLetters[checkingIndex] = "P";
        }
        else if(submittedLetters[checkingIndex] == "--.-")
        {
            typedLetters[checkingIndex] = "Q";
        }
        else if(submittedLetters[checkingIndex] == ".-.")
        {
            typedLetters[checkingIndex] = "R";
        }
        else if(submittedLetters[checkingIndex] == "...")
        {
            typedLetters[checkingIndex] = "S";
        }
        else if(submittedLetters[checkingIndex] == "-")
        {
            typedLetters[checkingIndex] = "T";
        }
        else if(submittedLetters[checkingIndex] == "..-")
        {
            typedLetters[checkingIndex] = "U";
        }
        else if(submittedLetters[checkingIndex] == "...-")
        {
            typedLetters[checkingIndex] = "V";
        }
        else if(submittedLetters[checkingIndex] == ".--")
        {
            typedLetters[checkingIndex] = "W";
        }
        else if(submittedLetters[checkingIndex] == "-..-")
        {
            typedLetters[checkingIndex] = "X";
        }
        else if(submittedLetters[checkingIndex] == "-.--")
        {
            typedLetters[checkingIndex] = "Y";
        }
        else if(submittedLetters[checkingIndex] == "--..")
        {
            typedLetters[checkingIndex] = "Z";
        }
        else if(submittedLetters[checkingIndex] == ".----")
        {
            typedLetters[checkingIndex] = "1";
        }
        else if(submittedLetters[checkingIndex] == "..---")
        {
            typedLetters[checkingIndex] = "2";
        }
        else if(submittedLetters[checkingIndex] == "...--")
        {
            typedLetters[checkingIndex] = "3";
        }
        else if(submittedLetters[checkingIndex] == "....-")
        {
            typedLetters[checkingIndex] = "4";
        }
        else if(submittedLetters[checkingIndex] == ".....")
        {
            typedLetters[checkingIndex] = "5";
        }
        else if(submittedLetters[checkingIndex] == "-....")
        {
            typedLetters[checkingIndex] = "6";
        }
        else if(submittedLetters[checkingIndex] == "--...")
        {
            typedLetters[checkingIndex] = "7";
        }
        else if(submittedLetters[checkingIndex] == "---..")
        {
            typedLetters[checkingIndex] = "8";
        }
        else if(submittedLetters[checkingIndex] == "----.")
        {
            typedLetters[checkingIndex] = "9";
        }
        else if(submittedLetters[checkingIndex] == "-----")
        {
            typedLetters[checkingIndex] = "0";
        }
        else
        {
            typedLetters[checkingIndex] = "!";
        }
    }

    void TransmissionFormat1()
    {
        if(letterToTransmit1 == "A")
        {
            screens[0].color = colorOptions[2];
            screens[0].text = symbolOptions[3];
        }
        else if (letterToTransmit1 == "B")
        {
            screens[0].color = colorOptions[3];
            screens[0].text = symbolOptions[1];
        }
        else if (letterToTransmit1 == "C")
        {
            screens[0].color = colorOptions[1];
            screens[0].text = symbolOptions[5];
        }
        else if (letterToTransmit1 == "D")
        {
            screens[0].color = colorOptions[1];
            screens[0].text = symbolOptions[4];
        }
        else if (letterToTransmit1 == "E")
        {
            screens[0].color = colorOptions[5];
            screens[0].text = symbolOptions[0];
        }
        else if (letterToTransmit1 == "F")
        {
            screens[0].color = colorOptions[2];
            screens[0].text = symbolOptions[2];
        }
        else if (letterToTransmit1 == "G")
        {
            screens[0].color = colorOptions[5];
            screens[0].text = symbolOptions[3];
        }
        else if (letterToTransmit1 == "H")
        {
            screens[0].color = colorOptions[2];
            screens[0].text = symbolOptions[4];
        }
        else if (letterToTransmit1 == "I")
        {
            screens[0].color = colorOptions[0];
            screens[0].text = symbolOptions[2];
        }
        else if (letterToTransmit1 == "J")
        {
            screens[0].color = colorOptions[5];
            screens[0].text = symbolOptions[1];
        }
        else if (letterToTransmit1 == "K")
        {
            screens[0].color = colorOptions[1];
            screens[0].text = symbolOptions[0];
        }
        else if (letterToTransmit1 == "L")
        {
            screens[0].color = colorOptions[4];
            screens[0].text = symbolOptions[5];
        }
        else if (letterToTransmit1 == "M")
        {
            screens[0].color = colorOptions[3];
            screens[0].text = symbolOptions[4];
        }
        else if (letterToTransmit1 == "N")
        {
            screens[0].color = colorOptions[3];
            screens[0].text = symbolOptions[2];
        }
        else if (letterToTransmit1 == "O")
        {
            screens[0].color = colorOptions[2];
            screens[0].text = symbolOptions[0];
        }
        else if (letterToTransmit1 == "P")
        {
            screens[0].color = colorOptions[1];
            screens[0].text = symbolOptions[1];
        }
        else if (letterToTransmit1 == "Q")
        {
            screens[0].color = colorOptions[0];
            screens[0].text = symbolOptions[3];
        }
        else if (letterToTransmit1 == "R")
        {
            screens[0].color = colorOptions[5];
            screens[0].text = symbolOptions[5];
        }
        else if (letterToTransmit1 == "S")
        {
            screens[0].color = colorOptions[0];
            screens[0].text = symbolOptions[5];
        }
        else if (letterToTransmit1 == "T")
        {
            screens[0].color = colorOptions[4];
            screens[0].text = symbolOptions[4];
        }
        else if (letterToTransmit1 == "U")
        {
            screens[0].color = colorOptions[1];
            screens[0].text = symbolOptions[3];
        }
        else if (letterToTransmit1 == "V")
        {
            screens[0].color = colorOptions[4];
            screens[0].text = symbolOptions[2];
        }
        else if (letterToTransmit1 == "W")
        {
            screens[0].color = colorOptions[4];
            screens[0].text = symbolOptions[1];
        }
        else if (letterToTransmit1 == "X")
        {
            screens[0].color = colorOptions[0];
            screens[0].text = symbolOptions[0];
        }
        else if (letterToTransmit1 == "Y")
        {
            screens[0].color = colorOptions[3];
            screens[0].text = symbolOptions[0];
        }
        else if (letterToTransmit1 == "Z")
        {
            screens[0].color = colorOptions[5];
            screens[0].text = symbolOptions[2];
        }
        else if (letterToTransmit1 == "0")
        {
            screens[0].color = colorOptions[4];
            screens[0].text = symbolOptions[3];
        }
        else if (letterToTransmit1 == "1")
        {
            screens[0].color = colorOptions[2];
            screens[0].text = symbolOptions[1];
        }
        else if (letterToTransmit1 == "2")
        {
            screens[0].color = colorOptions[3];
            screens[0].text = symbolOptions[5];
        }
        else if (letterToTransmit1 == "3")
        {
            screens[0].color = colorOptions[5];
            screens[0].text = symbolOptions[4];
        }
        else if (letterToTransmit1 == "4")
        {
            screens[0].color = colorOptions[0];
            screens[0].text = symbolOptions[1];
        }
        else if (letterToTransmit1 == "5")
        {
            screens[0].color = colorOptions[3];
            screens[0].text = symbolOptions[3];
        }
        else if (letterToTransmit1 == "6")
        {
            screens[0].color = colorOptions[2];
            screens[0].text = symbolOptions[5];
        }
        else if (letterToTransmit1 == "7")
        {
            screens[0].color = colorOptions[0];
            screens[0].text = symbolOptions[4];
        }
        else if (letterToTransmit1 == "8")
        {
            screens[0].color = colorOptions[1];
            screens[0].text = symbolOptions[2];
        }
        else if (letterToTransmit1 == "9")
        {
            screens[0].color = colorOptions[4];
            screens[0].text = symbolOptions[0];
        }
    }

    void TransmissionFormat2()
    {
        if(letterToTransmit2 == "A")
        {
            screens[1].color = colorOptions[2];
            screens[1].text = symbolOptions[3];
        }
        else if (letterToTransmit2 == "B")
        {
            screens[1].color = colorOptions[3];
            screens[1].text = symbolOptions[1];
        }
        else if (letterToTransmit2 == "C")
        {
            screens[1].color = colorOptions[1];
            screens[1].text = symbolOptions[5];
        }
        else if (letterToTransmit2 == "D")
        {
            screens[1].color = colorOptions[1];
            screens[1].text = symbolOptions[4];
        }
        else if (letterToTransmit2 == "E")
        {
            screens[1].color = colorOptions[5];
            screens[1].text = symbolOptions[0];
        }
        else if (letterToTransmit2 == "F")
        {
            screens[1].color = colorOptions[2];
            screens[1].text = symbolOptions[2];
        }
        else if (letterToTransmit2 == "G")
        {
            screens[1].color = colorOptions[5];
            screens[1].text = symbolOptions[3];
        }
        else if (letterToTransmit2 == "H")
        {
            screens[1].color = colorOptions[2];
            screens[1].text = symbolOptions[4];
        }
        else if (letterToTransmit2 == "I")
        {
            screens[1].color = colorOptions[0];
            screens[1].text = symbolOptions[2];
        }
        else if (letterToTransmit2 == "J")
        {
            screens[1].color = colorOptions[5];
            screens[1].text = symbolOptions[1];
        }
        else if (letterToTransmit2 == "K")
        {
            screens[1].color = colorOptions[1];
            screens[1].text = symbolOptions[0];
        }
        else if (letterToTransmit2 == "L")
        {
            screens[1].color = colorOptions[4];
            screens[1].text = symbolOptions[5];
        }
        else if (letterToTransmit2 == "M")
        {
            screens[1].color = colorOptions[3];
            screens[1].text = symbolOptions[4];
        }
        else if (letterToTransmit2 == "N")
        {
            screens[1].color = colorOptions[3];
            screens[1].text = symbolOptions[2];
        }
        else if (letterToTransmit2 == "O")
        {
            screens[1].color = colorOptions[2];
            screens[1].text = symbolOptions[0];
        }
        else if (letterToTransmit2 == "P")
        {
            screens[1].color = colorOptions[1];
            screens[1].text = symbolOptions[1];
        }
        else if (letterToTransmit2 == "Q")
        {
            screens[1].color = colorOptions[0];
            screens[1].text = symbolOptions[3];
        }
        else if (letterToTransmit2 == "R")
        {
            screens[1].color = colorOptions[5];
            screens[1].text = symbolOptions[5];
        }
        else if (letterToTransmit2 == "S")
        {
            screens[1].color = colorOptions[0];
            screens[1].text = symbolOptions[5];
        }
        else if (letterToTransmit2 == "T")
        {
            screens[1].color = colorOptions[4];
            screens[1].text = symbolOptions[4];
        }
        else if (letterToTransmit2 == "U")
        {
            screens[1].color = colorOptions[1];
            screens[1].text = symbolOptions[3];
        }
        else if (letterToTransmit2 == "V")
        {
            screens[1].color = colorOptions[4];
            screens[1].text = symbolOptions[2];
        }
        else if (letterToTransmit2 == "W")
        {
            screens[1].color = colorOptions[4];
            screens[1].text = symbolOptions[1];
        }
        else if (letterToTransmit2 == "X")
        {
            screens[1].color = colorOptions[0];
            screens[1].text = symbolOptions[0];
        }
        else if (letterToTransmit2 == "Y")
        {
            screens[1].color = colorOptions[3];
            screens[1].text = symbolOptions[0];
        }
        else if (letterToTransmit2 == "Z")
        {
            screens[1].color = colorOptions[5];
            screens[1].text = symbolOptions[2];
        }
        else if (letterToTransmit2 == "0")
        {
            screens[1].color = colorOptions[4];
            screens[1].text = symbolOptions[3];
        }
        else if (letterToTransmit2 == "1")
        {
            screens[1].color = colorOptions[2];
            screens[1].text = symbolOptions[1];
        }
        else if (letterToTransmit2 == "2")
        {
            screens[1].color = colorOptions[3];
            screens[1].text = symbolOptions[5];
        }
        else if (letterToTransmit2 == "3")
        {
            screens[1].color = colorOptions[5];
            screens[1].text = symbolOptions[4];
        }
        else if (letterToTransmit2 == "4")
        {
            screens[1].color = colorOptions[0];
            screens[1].text = symbolOptions[1];
        }
        else if (letterToTransmit2 == "5")
        {
            screens[1].color = colorOptions[3];
            screens[1].text = symbolOptions[3];
        }
        else if (letterToTransmit2 == "6")
        {
            screens[1].color = colorOptions[2];
            screens[1].text = symbolOptions[5];
        }
        else if (letterToTransmit2 == "7")
        {
            screens[1].color = colorOptions[0];
            screens[1].text = symbolOptions[4];
        }
        else if (letterToTransmit2 == "8")
        {
            screens[1].color = colorOptions[1];
            screens[1].text = symbolOptions[2];
        }
        else if (letterToTransmit2 == "9")
        {
            screens[1].color = colorOptions[4];
            screens[1].text = symbolOptions[0];
        }
    }
}
