using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

    public bool playerLoggedIn;
    public bool hasSeed;
    public bool hasWordList;
    public string wordlist;
    public int playerID;
    int leaderboardID = 1430;

    public Text playerText;
    public Text scoreText;

    public bool checkingForPlayability;
    public bool allowedToPlay = false;

    //The name of the player
    public string playerName;

    // The placeholder text in the input field
    public Text inputFieldPlaceHolderText;

    // The  text in the input field
    public Text inputFieldText;

    // The input field for the player to change their name
    public InputField playerNameInputField;

    // The Input field where the player will paste their saved UUID
    public InputField changePlayerInputText;

    // The text field where we will show information to the player
    public Text changePlayerInputTextPlaceholder;

    // The Inputfield that holds the current UUID
    public InputField currentPlayerUUIDinputfield;

    // The placeholder text that holds the current UUID
    public Text currentPlayerUUIDTextPlaceHolder;

    // Current Unique User Identifier
    public string UUID;

    public UInt32 seed;

    // A string where we store all our friends
    public Text friendNames;
    // A string where we store all our friends scores
    public Text friendScores;
    // Where the players public ID can be copied from
    public InputField playerPublicID;
    // Where the friend public ID will be pasted
    public InputField addFriendInputID;
    // How many friends does the player currently have
    public int currentAmountOfFriends = 0;

    // Start is called before the first frame update
    void Start()
    {

    }


    public void StartSession()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.Log("Error starting LootLocker session");

                return;
            }

            Debug.Log("Successfully started LootLocker session");
            playerID = response.player_id;
            playerPublicID.text = playerID.ToString();
            UUID = response.player_identifier;
            currentPlayerUUIDinputfield.text = UUID;
            currentPlayerUUIDTextPlaceHolder.text = UUID;
            playerLoggedIn = true;
        });
    }

    public UInt32 GetInternetTimeSeed()
    {
        return 10;
        // Make a request to a website
        UnityWebRequest myHttpWebRequest = (UnityWebRequest)UnityWebRequest.Get("https://www.microsoft.com");
        // Save the response
        //WebResponse response = myHttpWebRequest.result
        // Save todays date to a string
        //myHttpWebRequest.
        string originalDate = myHttpWebRequest.GetResponseHeader("date");
        Debug.LogWarning(originalDate);
        //string originalDate = "Wed, 26 Jan 2022 08:57:56 GMT";
        string dateSeed = "";

        // We now have a string that has the following format:
        // Day, XX Month 2022 Hour:Minute:Seconds Timezone
        // For example: Wed, 26 Jan 2022 08:57:56 GMT
        Debug.Log(originalDate);

        // We want to have a new word each day, so we do not need the clock or the timezone information
        // so let's first remove that from the string
        dateSeed = originalDate.Substring(0, originalDate.Length - 13);

        // Now we have the following:
        // Day, XX Month Year

        // What we want to do next is to remove the weekday since that is not needed, we do this by removing 5 characters;
        // the weekday in 3 letters, the ',' and the space
        dateSeed = dateSeed.Substring(5);

        // Now we have the following:
        // XX Month Year
        // This will give us a new value for each day,
        // and if you come back the same day, you will get the same result

        // Now we'll swap out the name of the month to a number from our list of months
        for (int i = 0; i < months.Length; i++)
        {
            if (dateSeed.Contains(months[i]))
            {
                // Replace the current month with the number of the month instead
                dateSeed = dateSeed.Replace(months[i], i.ToString());
                break;
            }
        }
        // Now we have:
        // 26 Jan 2022 becomes -> 26 0 2022


        // Now we will use regex to remove the whitespaces from this string
        dateSeed = Regex.Replace(dateSeed, @"\s", "");

        // Then parse this to an int
        UInt32 seed = UInt32.Parse(dateSeed);

        // What we now have is only numbers, so for example:
        // Wed, 26 Jan 2022 becomes 2602022
        // This is what we'll use for our seed,
        // this will give us a different number for every day for all days to come!

        // Outpout to the console:
        Debug.Log("Todays seed:" + seed.ToString());
        hasSeed = true;
        return 10;
    }

    public IEnumerator SetInterTimeSeedRoutine()
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Access-Control-Allow-Origin", "*");

        // test this:
        //https://thingproxy.freeboard.io/fetch/https://www.microsoft.com
        UnityWebRequest www = new UnityWebRequest("https://www.api.lootlocker.io/node/health/");
        yield return www.SendWebRequest();
        Debug.Log("<color=red>" + www.GetResponseHeader("date") + "</color>");

        //Debug.Log("<color=red>" + System.DateTime.Now + "</color>");

        string originalDate = "";
        string uri = "https://www.api.lootlocker.io/node/health";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    Debug.LogWarning(webRequest.downloadHandler.text);
                    originalDate = webRequest.GetResponseHeader("date");
                    break;
            }
        }

        Debug.LogWarning(originalDate);
        //string originalDate = "Wed, 26 Jan 2022 08:57:56 GMT";
        string dateSeed = "";

        // We now have a string that has the following format:
        // Day, XX Month 2022 Hour:Minute:Seconds Timezone
        // For example: Wed, 26 Jan 2022 08:57:56 GMT
        Debug.Log(originalDate);

        // We want to have a new word each day, so we do not need the clock or the timezone information
        // so let's first remove that from the string
        dateSeed = originalDate.Substring(0, originalDate.Length - 13);

        // Now we have the following:
        // Day, XX Month Year

        // What we want to do next is to remove the weekday since that is not needed, we do this by removing 5 characters;
        // the weekday in 3 letters, the ',' and the space
        dateSeed = dateSeed.Substring(5);

        // Now we have the following:
        // XX Month Year
        // This will give us a new value for each day,
        // and if you come back the same day, you will get the same result

        // Now we'll swap out the name of the month to a number from our list of months
        for (int i = 0; i < months.Length; i++)
        {
            if (dateSeed.Contains(months[i]))
            {
                // Replace the current month with the number of the month instead
                dateSeed = dateSeed.Replace(months[i], i.ToString());
                break;
            }
        }
        // Now we have:
        // 26 Jan 2022 becomes -> 26 0 2022


        // Now we will use regex to remove the whitespaces from this string
        dateSeed = Regex.Replace(dateSeed, @"\s", "");

        // Then parse this to an int
        UInt32 newSeed = UInt32.Parse(dateSeed);

        // What we now have is only numbers, so for example:
        // Wed, 26 Jan 2022 becomes 2602022
        // This is what we'll use for our seed,
        // this will give us a different number for every day for all days to come!

        // Outpout to the console:
        Debug.Log("Todays seed:" + newSeed.ToString());
        seed = newSeed;
        hasSeed = true;
    }

    public void GetCorrectWordsFromCloud(int fileIndex)
    {
        string[] list = new string[] { "89852" };

        LootLockerSDKManager.GetAssetsById(list, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully retrieved " + response.assets.Length + " assets");
                Debug.Log("First Asset ID: " + response.assets[0].id);

                // Does the file exist?
                if (response.assets[0].files[fileIndex] != null)
                {
                    // Get the file requested by the index
                    StartCoroutine(GetFileRoutine(response.assets[0].files[fileIndex].url));
                }
            }
            else
            {
                Debug.Log("Error retrieving assets");
            }
        });
    }

    public IEnumerator GetFileRoutine(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                wordlist = www.downloadHandler.text;
                hasWordList = true;
            }
        }
    }

    public void SubmitScore(int score)
    {
        LootLockerSDKManager.GetMemberRank(leaderboardID.ToString(), playerID, (response) =>
        {
            if (response.statusCode == 200)
            {
                Debug.Log("Successful");
                // Add the previous score to our current score
                // Unless we've already played with this seed before, then we just return
                score += response.score;

                string scoreSeed = GetInternetTimeSeed().ToString();
                string metadataSeed = "";
                if (response.metadata != null)
                {
                    metadataSeed = response.metadata.ToString();
                }
                Debug.Log(scoreSeed);
                if (scoreSeed != metadataSeed)
                {
                    // Submit the new score, and use the seed of the day as metadata
                    LootLockerSDKManager.SubmitScore(playerID.ToString(), score, leaderboardID.ToString(), scoreSeed, (response) =>
                    {
                        if (response.statusCode == 200)
                        {
                            Debug.Log("Successful");
                        }
                        else
                        {
                            Debug.Log("failed: " + response.Error);
                        }
                    });
                }
                else
                {
                    Debug.Log("Already played today, come back tomorrow!");
                    return;
                }
            }
            else
            {
                Debug.Log("failed: " + response.Error);
            }
        });
    }

    public void UpdateLeaderBoardGlobal()
    {
        // How many should we fetch?
        int count = 50;
        int after = 0;
        playerText.text = "";
        scoreText.text = "";
        LootLockerSDKManager.GetScoreListMain(leaderboardID, count, after, (response) =>
        {
            if (response.statusCode == 200)
            {
                Debug.Log("Successful");
                // Get all players
                LootLockerLeaderboardMember[] players = response.items;
                for (int i = 0; i < players.Length; i++)
                {
                    string playerName = players[i].player.name;
                    // If the player has no name, use the player ID instead
                    if (playerName == "")
                    {
                        playerName = players[i].player.id.ToString();
                    }
                    // Update our text components to show them
                    playerText.text += players[i].rank.ToString() + ". " + playerName + "\n";
                    scoreText.text += players[i].score.ToString() + "\n"; ;
                }
            }
            else
            {
                Debug.Log("failed: " + response.Error);
            }
        });
    }

    public void AllowedToPlayCheck()
    {
        checkingForPlayability = true;
        LootLockerSDKManager.GetMemberRank(leaderboardID.ToString(), playerID, (response) =>
        {
            if (response.statusCode == 200)
            {
                Debug.Log("Successful");

                string todaysSeed = GetInternetTimeSeed().ToString();
                string metadataSeed = "";
                if (response.metadata != null)
                {
                    metadataSeed = response.metadata.ToString();
                }
                // Is the seed different?
                if (todaysSeed != metadataSeed)
                {
                    allowedToPlay = true;
                }
                checkingForPlayability = false;
            }
            else
            {
                Debug.Log("failed: " + response.Error);
            }
        });
    }

    public void TrySetPlayerNameToInputField()
    {

        LootLockerSDKManager.GetPlayerName((response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully retrieved player name: " + response.name);
                playerName = response.name;

                // Player has a name
                if (playerName != "")
                {
                    inputFieldPlaceHolderText.text = playerName;
                }
                else
                {
                    // Player has no name
                }
            }
            else
            {
                Debug.Log("Error getting player name");
            }
        });
    }

    public IEnumerator SetPlayerName()
    {
        bool done = false;
        LootLockerSDKManager.SetPlayerName(inputFieldText.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set player name");
                playerName = inputFieldText.text;
                done = true;
            }
            else
            {
                Debug.Log("Error setting player name");
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    public void ChangePlayerIdentity()
    {
        // Get the new UUID from the inputfield
        string newUUID = changePlayerInputText.text;

        // Check if the UUID is valid
        if (Guid.TryParse(newUUID, out Guid newGuid))
        {
            // It was valid, set our new UUID
            PlayerPrefs.SetString("LootLockerGuestPlayerID", newUUID);

            // We have set the new user, reload the scene so that we start a new session with the new player
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // The id format was incorrect, give feedback to the player that it was wrong
            changePlayerInputTextPlaceholder.text = "<color=red>UUID format is wrong, it looks similar to this: \n 00000000-0000-0000-0000-000000000000</color>";
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddFriend();
        }
    }

    public IEnumerator SetupFriends()
    {
        // Retrieve the whole storage from the player (this will be just the friends in this case)
        bool storageRetrieved = false;
        string tempFriends = "";
        LootLockerSDKManager.GetEntirePersistentStorage((response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully retrieved player storage: " + response.payload.Length);
                currentAmountOfFriends = response.payload.Length;

                // Add the friend ID's to the friend names lsit
                for (int i = 0; i < response.payload.Length; i++)
                {
                    tempFriends += response.payload[i].value + ",";
                }
                storageRetrieved = true;
            }
            else
            {
                Debug.Log("Error getting player storage");
                storageRetrieved = true;
            }
        });
        yield return new WaitWhile(() => storageRetrieved == false);

        // Separate the names for each ',' character
        char[] separator = { ',' };
        string[] memberIDs = tempFriends.Split(separator);

        // Create a list where we will keep the values
        List<string> tempFriendNames = new List<string>();
        List<string> tempFriendScores = new List<string>();

        bool friendLeaderBoardDone = false;
        // Get all the players friends scores by using the ID's we got from earlier
        LootLockerSDKManager.GetByListOfMembers(memberIDs, leaderboardID, (response) =>
        {
            if (response.statusCode == 200)
            {
                // Add the scores to the friendScores column
                for (int i = 0; i < response.members.Length; i++)
                {
                    // Does the friend exist on the leaderboard?
                    if (response.members[i].rank > 0)
                    {
                        string newFriendName = "";
                        // If the friend doesn't have a name set up, show the ID instead
                        Debug.Log(response.members[i].rank);
                        if (response.members[i].player != null && response.members[i].player.name != null)
                        {
                            newFriendName = response.members[i].player.name;
                        }
                        else
                        {
                            newFriendName = response.members[i].member_id;
                        }
                        // Update the names with the ranking position and name
                        tempFriendNames.Add(response.members[i].rank + ". " + newFriendName);

                        // Update the score
                        tempFriendScores.Add(response.members[i].score.ToString());
                    }
                }

                Debug.Log("Successful");
                friendLeaderBoardDone = true;
            }
            else
            {
                Debug.Log("failed: " + response.Error);
                friendLeaderBoardDone = true;
            }
        });
        yield return new WaitWhile(() => friendLeaderBoardDone == false);

        // Now we sort the lists based on the rank in the leaderboard.
        // This is by far not the most efficient way to sort a list.
        // This could be replaced with list.Sort() if it weren't for the fact that
        // we need to scores the have the same ordering as the names.
        Debug.Log(tempFriendNames.Count);
        for (int i = 0; i < tempFriendNames.Count - 1; i++)
        {
            // Parse the the first letter (the ranking number)
            int firstNumber = int.Parse(tempFriendNames[i].ToCharArray()[0].ToString());
            int secondNumber = int.Parse(tempFriendNames[i + 1].ToCharArray()[0].ToString());

            // Comparing the numbers
            if (firstNumber > secondNumber)
            {
                // Swapping of the characters
                string tempName = tempFriendNames[i];
                tempFriendNames[i] = tempFriendNames[i + 1];
                tempFriendNames[i + 1] = tempName;

                // The scores will sort in the same manner
                string tempScore = tempFriendScores[i];
                tempFriendScores[i] = tempFriendScores[i + 1];
                tempFriendScores[i + 1] = tempScore;
                i = -1;
            }
        }

        // Now we set the correct values from our two lists to our 2 strings where we store the values
        friendNames.text = "";
        friendScores.text = "";
        for (int i = 0; i < tempFriendNames.Count; i++)
        {
            // Change the ranking from the global leaderboard to be ascending in this new order
            string newRankingNumber = (i + 1).ToString() + tempFriendNames[i].Substring(1);

            friendNames.text += newRankingNumber + "\n";
            // Since both lists are sorted the same we can use the same index for the scores
            friendScores.text += tempFriendScores[i] + "\n";
        }

    }

    public void AddFriend()
    {
        StartCoroutine(AddFriendRoutine());
    }

    IEnumerator AddFriendRoutine()
    {
        bool friendAdded = false;
        // Add 1 to our friends
        currentAmountOfFriends++;
        LootLockerSDKManager.UpdateOrCreateKeyValue("Friend" + currentAmountOfFriends.ToString(), addFriendInputID.text, (getPersistentStorageResponse) =>
          {
              if (getPersistentStorageResponse.success)
              {
                  Debug.Log("Successfully Added friend");
                  friendAdded = true;
              }
              else
              {
                  Debug.Log("Error updating player storage");
                  friendAdded = true;
              }
          });
        yield return new WaitWhile(() => friendAdded == false);

        // Our friend was added, let's update the friends leaderboard again
        yield return SetupFriends();
    }

}
