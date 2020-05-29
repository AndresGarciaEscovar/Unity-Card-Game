using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a card game.
/// </summary>
public class CardGame : MonoBehaviour
{
    #region Enumerations and Structures

    // Enumeration with the possible operations of opening/closing cards.
    public enum Opers
    {
        OpenAbove,
        OpenBelow
    };

    #endregion

    #region Attributes

    // Constants regarding the vertical and horizontal separation of the card sprites
    const float HorzSep = 0.2f;
    const float VertSep = 0.2f;

    // Reset the game.
    public Button resetGameBttn;

    // The show solution button.
    public Button showSolBttn;

    // The button that controls the sound.
    public Button soundOnOffBttn;

    // The submit answer button.
    public Button submitAnswBttn;

    // The dropdown menu for the game difficulty.
    public Dropdown diffMenu;

    // The game object that represents the game.
    public GameObject prefabCard;

    // The text field that displays the difficulty level.
    public List<Button> hintBttns;

    // The list of sprites for the hints
    public Sprite[] hintSprites;

    // The text field that displays the difficulty level.
    public Text diffLevlDspl;

    // The text field
    public Text hintTxt;

    // The text field that displays the target score.
    public Text targetScoreTxt;

    // The text field that displays the player's score.
    public Text winLoseTxt;

    // The text field that displays if the player has win or lost and displays the results.
    public Text winLoseTxtLbl;

    // The history of the levels to restore
    private bool ansSubmitted;

    // The if sound should be played or not
    private bool soundOn;

    // The current value of the hint
    private int hintVal;

    // The current value of the multiplicated cards
    private int multState;

    // The value of the target score
    private int targetScoreVl;

    // The history of the levels to restore
    private List<Tuple<int, int>> levelHistory;

    // The list of target scores
    private List<Tuple<int, string>> targetScores;

    // The list that will hold the cards in the game.
    private List<List<GameObject>> gameObjLst;

    // The current level of card selection within the game
    private short currentLevel;

    // The difficulty level
    private short diffLevel = 2;

    #endregion

    #region Start Method

    /// <summary>
    /// Sets up the initial parameters, runs before the first frame.
    /// </summary>
    void Start()
    {
        // Setup the sound on
        soundOn = true;
    }

    #endregion

    #region Generic Methods

    /// <summary>
    /// Calculates the target scores possible
    /// </summary>
    /// <param name="level">The level at which to calculate the score.</param>
    /// <param name="currentScore"> The current score with which the function enters.</param>
    /// <param name="cardPos"> The position of the card within the script.</param>
    private void CalculateTargetScores(int level = 0, int currentScore = 1, int cardPos = 0, string pathStr = "")
    {
        //Auxiliary variable to register the position of the string
        string auxStr = pathStr + cardPos;

        // Create a new array of target scores at the beginning
        if (level == 0)
        {
            targetScores = new List<Tuple<int, string>>();
            targetScores.Clear();
        }

        // If the card is at the last level, store the score
        if (level >= gameObjLst.Count())
        {
            targetScores.Add(new Tuple<int, string>(currentScore, pathStr));
        }
        else if (level == (gameObjLst.Count() - 1))
        {
            CalculateTargetScores(level + 1, currentScore * gameObjLst[level][cardPos].GetComponent<Card>().CardNum, pathStr: auxStr);
        }
        else
        {
            CalculateTargetScores(level + 1, currentScore * gameObjLst[level][cardPos].GetComponent<Card>().CardNum, cardPos, auxStr);
            CalculateTargetScores(level + 1, currentScore * gameObjLst[level][cardPos].GetComponent<Card>().CardNum, cardPos + 1, auxStr);
        }
    }

    /// <summary>
    /// /// <summary>
    /// Sets up the difficulty level with respect to the choices.
    /// </summary>
    /// <param name="optn">The chosen level of difficulty.</param>
    public void DropDownValueChanged(int optn)
    {
        // If the option is custom, set it to the number on the counter
        if (optn <= 2)
        {
            diffLevel = (short)(2 * (optn + 1));
        }

        // Either way, set the level to the current level.
        diffLevlDspl.GetComponent<Text>().text = diffLevel.ToString();

    }

    /// <summary>
    ///  Flips the cards after a valid card is selected.
    /// </summary>
    /// <param name="level">The level at which the selected card is.</param>
    /// <param name="posCard">The position within the level at which the card is.</param>
    /// <param name="oper">The operation to be performed, if to go back a level or go forward.</param>
    public void FlipCards(int level, int posCard, Opers oper)
    {
        // If the action is to open a card. 
        if (oper == Opers.OpenBelow)
        {
            // Delete the hint
            hintTxt.text = "";

            // Play the sound effect when clicking a card
            if (soundOn) GetComponent<AudioSource>().Play();

            // Update the multiplication
            UpdateMultiplication(gameObjLst[level][posCard].GetComponent<Card>().CardNum, oper);

            // Set cards at the same level gray
            for (int i = posCard + 1; ; i++)
            {
                if (i >= gameObjLst[level].Count()) break;
                gameObjLst[level][i].GetComponent<Card>().CardStt = Card.State.Gray;
            }

            for (int i = (posCard - 1); ; i--)
            {
                if (i < 0) break;
                gameObjLst[level][i].GetComponent<Card>().CardStt = Card.State.Gray;
            }

            // Flip the two cards on the level below
            if (level < (gameObjLst.Count() - 1))
            {
                gameObjLst[level + 1][posCard].GetComponent<Card>().CardStt = Card.State.Blue;
                gameObjLst[level + 1][posCard + 1].GetComponent<Card>().CardStt = Card.State.Blue;
            }

            // If the level is zero, clear everything
            if (currentLevel == 0)
            {
                levelHistory.Clear();
                levelHistory.Add(new Tuple<int, int>(0, 0));
            }

            // Add one to the level and the corresponding cards
            if ((currentLevel + 1) < (gameObjLst.Count()))
            {
                currentLevel++;
                levelHistory.Add(new Tuple<int, int>(posCard, posCard + 1));
            }

            // Update the submit button state
            UpdateSubmitButton();
        }
        else if (oper == Opers.OpenAbove && !ansSubmitted)
        {
            //Auxiliary variables
            int idx1;
            int idx2;

            // Checks condition for the previous to last
            bool checkCond = false;

            // Check that the previous to last level is not red before proceeding
            if (currentLevel == (gameObjLst.Count() - 1))
            {
                checkCond = checkCond || gameObjLst[currentLevel][posCard].GetComponent<Card>().CardStt == Card.State.Red;
                checkCond = checkCond || gameObjLst[currentLevel][posCard + 1].GetComponent<Card>().CardStt == Card.State.Red;
            }

            // Turn the cards over if needed
            if (currentLevel == (gameObjLst.Count() - 1) && level == currentLevel)
            {
                // Delete the hint
                hintTxt.text = "";

                // Update the multiplication
                UpdateMultiplication(gameObjLst[level][posCard].GetComponent<Card>().CardNum, oper);

                // Get the indexes of the cards that where initially gray and turn them blue
                idx1 = levelHistory[currentLevel].Item1;
                idx2 = levelHistory[currentLevel].Item2;

                // Turn the cards blue, no need to delete the history
                gameObjLst[currentLevel][idx1].GetComponent<Card>().CardStt = Card.State.Blue;
                gameObjLst[currentLevel][idx2].GetComponent<Card>().CardStt = Card.State.Blue;

                // Play the sound effect when clicking a card
                if (soundOn) GetComponent<AudioSource>().Play();
            }
            else if ((level + 1) == currentLevel && !checkCond)
            {
                // Delete the hint
                hintTxt.text = "";

                // Update the multiplication
                UpdateMultiplication(gameObjLst[level][posCard].GetComponent<Card>().CardNum, oper);

                // Get the indexes of the cards at the current level and set them to gray
                idx1 = levelHistory[currentLevel].Item1;
                idx2 = levelHistory[currentLevel].Item2;

                // Turn the cards blue, no need to delete the history
                gameObjLst[currentLevel][idx1].GetComponent<Card>().CardStt = Card.State.Gray;
                gameObjLst[currentLevel][idx2].GetComponent<Card>().CardStt = Card.State.Gray;

                // Delete the last entry from the array
                levelHistory.RemoveAt(currentLevel);

                // Go down one level and set the cards to blue
                if (currentLevel > 0) currentLevel--;

                // Get the indexes of the cards at the current level and set them to blue
                idx1 = levelHistory[currentLevel].Item1;
                idx2 = levelHistory[currentLevel].Item2;

                // Turn the cards blue, no need to delete the history
                gameObjLst[currentLevel][idx1].GetComponent<Card>().CardStt = Card.State.Blue;
                gameObjLst[currentLevel][idx2].GetComponent<Card>().CardStt = Card.State.Blue;

                // Play the sound effect when clicking a card
                if (soundOn) GetComponent<AudioSource>().Play();
            }

            // Update the submit button state
            UpdateSubmitButton();
        }

    }

    /// <summary>
    /// /// <summary>
    /// Sets up a car scenario with a specific number of levels.
    /// </summary>
    /// <param name="levels">The number of levels that the pyramid has, must be between 2 and 9.</param>
    /// <param name="numLst">The specific numbers of the cards to be set.</param>
    private void GenerateLevels(short levels)
    {
        // If the list has not been created, create it.
        if (gameObjLst == null)
        {
            gameObjLst = new List<List<GameObject>>();
        }

        // Destroy all the objects in the list.
        for (int i = (gameObjLst.Count() - 1); i >= 0; i--)
        {
            for (int j = (gameObjLst[i].Count() - 1); j >= 0; j--)
            {
                Destroy(gameObjLst[i][j]);
            }
        }

        // Set the current level to zero and multiplication to one.
        multState = 1;
        currentLevel = 0;

        // Create the new level history.
        levelHistory = new List<Tuple<int, int>>();
        levelHistory.Clear();

        // Clear the list and start from scratch.
        gameObjLst.Clear();

        // Setup a random game by creating the list according to the number of levels.
        for (short i = 0; i < levels; i++)
        {
            // Add a new vector array for each level.
            gameObjLst.Add(new List<GameObject>());
            for (short j = 0; j <= i; j++)
            {
                // The level number has n cards
                gameObjLst[i].Add(Instantiate(prefabCard, new Vector2(0, 0), Quaternion.identity));
                gameObjLst[i][j].GetComponent<Card>().CardNum = (short)UnityEngine.Random.Range(1, 10);

                // Set the level and position for the card
                gameObjLst[i][j].GetComponent<Card>().CardLev = i;
                gameObjLst[i][j].GetComponent<Card>().CardPos = j;

                // Set the state of the cards to gray initially, except the first one
                if (i > 0)
                {
                    gameObjLst[i][j].GetComponent<Card>().CardStt = Card.State.Gray;
                }
            }
        }

        // Set the position of the cards.
        SetCardPositions();

        // Calculate the possible target scores and randomly select a target score.
        CalculateTargetScores();
        SetTargetScore();

        // Update the submit button if needed.
        UpdateSubmitButton();

        // Reset the labels
        ResetGameElements(true);

        // Reset all the hint sprites and set hint counter to zero
        ResetHintSprites();

    }

    /// <summary>
    /// Gives the current score, unless it is not on the base level.
    /// </summary>
    public void GenerateHint()
    {
        bool lastLevl = false;

        // Check if there are any red cards in the last level
        for (int i = 0; i < gameObjLst[gameObjLst.Count() - 1].Count() && !lastLevl; i++)
        {
            lastLevl = lastLevl || (gameObjLst[gameObjLst.Count() - 1][i].GetComponent<Card>().CardStt == Card.State.Red);
        }

        // Only accept hints if before the last level and if the answer has not been submitted.
        if (!lastLevl && !ansSubmitted)
        {
            // Disable the selected button
            hintBttns[hintVal].GetComponentInChildren<Button>().image.sprite = hintSprites[1];
            hintBttns[hintVal].GetComponentInChildren<Button>().interactable = false;

            // Change the text in the hint button
            hintTxt.text = multState.ToString();

            //Add one to the counter
            hintVal++;

            // Enable the next hint provided the hints have not been depleted
            if (hintVal < 3) hintBttns[hintVal].GetComponentInChildren<Button>().interactable = true;
        }
    }

    /// <summary>
    /// Increases or decreases the counter and sets the value of the dropdown menu if needed.
    /// </summary>
    /// <param name="oper">The operation to be performed.</param>
    public void IncreaseDecreaseDifficulty(int oper)
    {
        // Only allowed levels are between 2 and 6.
        if (!((short)(diffLevel + oper) < 2 || (short)(diffLevel + oper) > 6))
        {
            // Increase/Decrease the level.
            diffLevel += (short)oper;

            // Set the dropdown menu to the custom operation.
            diffMenu.value = 3;

            // Modify the level.
            diffLevlDspl.GetComponent<Text>().text = diffLevel.ToString();
        }
    }

    /// <summary>
    /// /// Resets the current game.
    /// </summary>
    public void ResetGame()
    {
        // Resets all the elements of the game.
        ResetGameElements(true, true);
    }

    /// <summary>
    /// /// Resets the labels and/or state of a game.
    /// </summary>
    /// <param name="rLabels"> If the labels should be reset.</param>
    /// <param name="rCards"> If the cards should be reset.</param>
    private void ResetGameElements(bool rLabels = false, bool rCards = false)
    {
        // Reset labels if needed.
        if (rLabels)
        {
            winLoseTxt.text = " ";
            winLoseTxtLbl.text = " ";
        }

        // Reset the cards and the proper variables.
        if (rCards)
        {
            // Verify that there are elements in the game before resetting.
            if (gameObjLst != null && gameObjLst.Count() > 0)
            {

                // Set the first card to blue.
                gameObjLst[0][0].GetComponent<Card>().CardStt = Card.State.Blue;

                // Set the rest of the cards to gray.
                for (int i = 1; i < gameObjLst.Count(); i++)
                {
                    for (int j = 0; j < gameObjLst[i].Count(); j++)
                    {
                        gameObjLst[i][j].GetComponent<Card>().CardStt = Card.State.Gray;
                    }
                }
            }

            //Reset the game variables
            multState = 1;
            currentLevel = 0;
            if (levelHistory != null) levelHistory.Clear();

            // Update the submit button state
            UpdateSubmitButton();

            // Choose a new target score
            SetTargetScore();
        }

        // The answer submission must ALWAYS be reset
        ansSubmitted = false;

        // ALWAYS update the show solution button
        UpdateShowSolutionButton();

    }

    /// <summary>
    ///  Resets the sprites for the hints.
    /// </summary>
    public void ResetHintSprites()
    {
        // Enable the first button
        hintBttns[0].GetComponentInChildren<Button>().interactable = true;

        // Set the sprites and disable the other buttons
        for (int i = 0; i < hintBttns.Count(); i++)
        {
            hintBttns[i].GetComponentInChildren<Button>().image.sprite = hintSprites[0];
            if (i > 0) hintBttns[i].GetComponentInChildren<Button>().interactable = false;
        }

        // Reset the hint value
        hintVal = 0;
    }

    /// <summary>
    /// Shows the solution to the problem.
    /// </summary>
    public void ShowSolution()
    {
        // Only valid if there are scores to show and the answer has been submitted
        if (targetScores != null && targetScores.Count() > 0 && ansSubmitted)
        {
            //Search for the tupples with the target score (there can be more than one)
            foreach (Tuple<int, string> tp in targetScores)
            {
                // Add the string if needed
                if (tp.Item1 == targetScoreVl)
                {
                    string tmp = tp.Item2;
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        gameObjLst[i][int.Parse("" + tmp[i])].GetComponent<Card>().CardStt = Card.State.Green;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Toggles the sound on/off.
    /// </summary>
    public void ToggleSoundOnOff()
    {
        // Change the state of the sound.
        soundOn = (!soundOn);

        // Change the button label according to the state of the sound.
        if (soundOn)
        {
            soundOnOffBttn.GetComponentInChildren<Text>().text = "Sound On";
        }
        else
        {
            soundOnOffBttn.GetComponentInChildren<Text>().text = "Sound Off";
        }

    }

    /// <summary>
    /// Verifies the score and gives a win/lose message with the scores and enables the reveal solution(s) message.
    /// </summary>
    public void VerifySolution()
    {
        // Set the score in the textbox.
        winLoseTxt.text = multState.ToString();

        // Format with the proper color.
        if (multState == targetScoreVl)
        {
            winLoseTxt.color = Color.green;
            winLoseTxtLbl.color = Color.green;
            winLoseTxtLbl.text = "You Win!";
        }
        else
        {
            winLoseTxt.color = Color.red;
            winLoseTxtLbl.color = Color.red;
            winLoseTxtLbl.text = "You Lose!";
        }

        // Update that the answer has been submitted.
        ansSubmitted = true;

        // Update the show solution button.
        UpdateShowSolutionButton();
    }

    #endregion

    #region Set Methods

    /// <summary>
    /// Sets the cards in the initial pyramidal shape.
    /// </summary>
    private void SetCardPositions()
    {
        int levs = gameObjLst.Count();

        // Start from the lowest level
        for (int i = (levs - 1); i >= 0; i--)
        {
            // Start from left to right
            for (int j = 0; j <= i; j++)
            {
                //Auxiliary variables
                Vector2 position;
                Vector2 collSize = gameObjLst[i][j].GetComponent<Collider2D>().bounds.size;

                // Make a shift according to the collider size
                position.x = gameObjLst[i][j].transform.position.x + collSize.x / 2.0f;
                position.y = gameObjLst[i][j].transform.position.y + collSize.y / 2.0f;

                position.x += j * collSize.x + (j + 1) * HorzSep - (i * collSize.x + (i - 1) * HorzSep) / 2.0f;
                position.y += (levs - 1 - i) * (collSize.y + VertSep) - (levs * collSize.y + (levs - 1) * VertSep) / 2.0f;

                // Set the new position of the card
                gameObjLst[i][j].transform.position = position;
            }
        }
    }

    /// <summary>
    /// Creates a random scenario that can have anywhere between 2 and 9 rows.
    /// </summary>
    public void SetRandomScenario()
    {
        // Generate a random scenario based on a random number from 2 to 6.
        GenerateLevels(diffLevel);
    }

    /// <summary>
    /// Sets the target score.
    /// </summary>
    private void SetTargetScore()
    {
        // Only if the arrays 
        if (targetScores != null && targetScores.Count() > 0)
        {
            // Get a random value for the target score
            targetScoreVl = targetScores[UnityEngine.Random.Range(0, targetScores.Count())].Item1;

            // Display the score
            targetScoreTxt.text = targetScoreVl.ToString();
        }
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Updates the multiplication value.
    /// </summary>
    /// <param name="fact">The factor by which the multiplication state shoud be modified.</param>
    /// <param name="oper">The operation to be performed, depending from where the situation has ocurred.</param>
    private void UpdateMultiplication(int fact, Opers oper)
    {
        // If the operation comes from above multiply, else divide
        if (oper == Opers.OpenBelow)
        {
            multState *= fact;
        }
        else
        {
            multState /= fact;
        }
    }

    /// <summary>
    /// Updates the enabled/disabled show solution button.
    /// </summary>
    private void UpdateShowSolutionButton()
    {
        showSolBttn.interactable = ansSubmitted;
    }

    /// <summary>
    /// Updates the enabled/disabled state of the score submission button.
    /// </summary>
    private void UpdateSubmitButton()
    {
        // Auxiliary variables
        bool btnInt = false;

        // Only do this if there is a game
        if (gameObjLst != null && gameObjLst.Count() > 0)
        {
            // Only if all the cards are chosen
            if (currentLevel == (gameObjLst.Count() - 1))
            {
                // Check that all the cards in the lowest level are red

                int crdL = gameObjLst.Count() - 1;
                int cntr = gameObjLst[crdL].Count();

                // Loop through all the cards in the last level
                for (int i = 0; i < cntr && !btnInt; i++)
                {
                    btnInt = btnInt || (gameObjLst[crdL][i].GetComponent<Card>().CardStt == Card.State.Red);
                }
            }

            // Update the button state
            submitAnswBttn.interactable = btnInt;
        }
    }

    #endregion
}
