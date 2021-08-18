using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
#endif

public class PianoTracker : MonoBehaviour
{
    public PianoTextOutput pianoTextOutput;
    public GameObject nextTryButton;

    private TestRun testRun;
    private PianoTestResults testResults = new PianoTestResults();
    private double timeWhenLastKeyPressRegistered = 0;
    private TestResultsWriter writer;

    private int currentSessionNumber = 1;
    private static readonly int NUMBER_OF_SESSIONS = 3;

    // Use this for initialization
    void Start()
    {
        testRun = new TestRun(currentSessionNumber);
        writer = new TestResultsWriter(testResults.id + ".json");
        writer.init();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RegisterKeyPress(string nameOfKey)
    {


        lock (this)
        {
            double timeOfPress = Time.unscaledTimeAsDouble;
            PianoKeyPress newKeyPress = new PianoKeyPress();

            newKeyPress.nameOfKey = nameOfKey;
            if (testRun.pressSequence.Count == 0)
            {
                newKeyPress.secondsFromPreviousKeyPress = 0;
            }
            else
            {
                newKeyPress.secondsFromPreviousKeyPress = timeOfPress - timeWhenLastKeyPressRegistered;
            }
            testRun.add(newKeyPress);
            timeWhenLastKeyPressRegistered = timeOfPress;
        }


    }

    public void NextSession()
    {
        if (currentSessionNumber <= NUMBER_OF_SESSIONS)
        {
            if(currentSessionNumber == NUMBER_OF_SESSIONS && nextTryButton != null)
            {
                nextTryButton.SetActive(false);
            }
            string jsonOfTestResults = "";
            lock (this)
            {
                testResults.addTry(testRun);

                timeWhenLastKeyPressRegistered = 0;
                currentSessionNumber++;
                testRun = new TestRun(currentSessionNumber);

                jsonOfTestResults = testResults.Stringify();
            }
            writer.write(jsonOfTestResults);
            if (pianoTextOutput != null)
            {
                pianoTextOutput.SetText($"{currentSessionNumber - 1} of {NUMBER_OF_SESSIONS} tries");
            }
        }
    }
}

public class TestResultsWriter
{
#if WINDOWS_UWP
       Windows.Storage.ApplicationDataContainer localSettings = 
    Windows.Storage.ApplicationData.Current.LocalSettings;
Windows.Storage.StorageFolder localFolder = 
    Windows.Storage.ApplicationData.Current.LocalFolder;
        private Windows.Storage.StorageFile file;
        
#endif
    private string fileName;

    public TestResultsWriter(string fileName)
    {
        this.fileName = fileName;

    }

    public async void init()
    {
#if WINDOWS_UWP

        file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
#endif

    }

    public async void write(string toWrite)
    {
#if WINDOWS_UWP
        await Windows.Storage.FileIO.WriteTextAsync(file, toWrite);
#endif

    }


}

[Serializable]
public class TestRun
{
    public List<PianoKeyPress> pressSequence = new List<PianoKeyPress>();
    public int testRunNumber;

    public TestRun(int testRunNumber)
    {
        this.testRunNumber = testRunNumber;
    }

    public void add(PianoKeyPress pianoKeyPress)
    {
        pressSequence.Add(pianoKeyPress);
    }

}
[Serializable]
public class PianoTestResults
{
    public List<TestRun> testTries;

    public string id = Guid.NewGuid().ToString();
    public string timestamp = System.DateTime.Now.ToString().Replace("/", "-").Replace(":", "-").Replace(" ", "-");

    public PianoTestResults()
    {
        testTries = new List<TestRun>();
    }

    public void addTry(TestRun trySequence)
    {
        testTries.Add(trySequence);

    }

    public string Stringify()
    {
        return JsonUtility.ToJson(this);
    }

}

[Serializable]
public class PianoKeyPress
{
    public string nameOfKey;
    public double secondsFromPreviousKeyPress;
}
