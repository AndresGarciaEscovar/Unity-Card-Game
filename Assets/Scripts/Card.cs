using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Card that represents an integer number.
/// </summary>
public class Card : MonoBehaviour
{
    #region Enumerations and Structures

    // Enumeration with the possible states of the cards.
    public enum State
    {
        Blue,
        Gray,
        Red,
        Green
    };

    #endregion

    #region Attributes

    // A copy of the CardGame script.
    private CardGame tmp;

    // Number of card sprites per number.
    private const int NCards = 4;

    // Card number, can only take numbers from 1 to 9.
    private short cardNum;

    // The position of the card within the level.
    private short cardPos;

    // The level of the card (from one to six).
    private short cardLevel;

    // List of sprites.
    public Sprite[] SpriteList = new Sprite[27];

    // The state of the card can only take one of the three states.
    State cardState;

    void Start()
    {
        tmp = Camera.main.GetComponent<CardGame>();
    }

    #endregion

    #region Action Listener Methods

    /// <summary>
    /// When clicking on card changes it's state from red to blue.
    /// </summary>
    /// <remarks>
    /// Gets the action if the mouse is pressed.
    ///</remarks>
    void OnMouseDown()
    {
        // React to the click on the card.
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the value of the card is blue and turn it red.
            if (cardState == State.Blue)
            {
                // Set the card state to red and update the sprite
                cardState = State.Red;
                UpdateCardSprite();

                // Flip the adjacent card and card below if needed
                tmp.FlipCards(cardLevel, cardPos, CardGame.Opers.OpenBelow);
            }
            else if (cardState == State.Red)
            {
                // Flip the previous cards
                tmp.FlipCards(cardLevel, cardPos, CardGame.Opers.OpenAbove);
            }
        }
    }

    #endregion

    #region Get/Set Methods

    /// <summary>
    /// Returns or sets the card number.
    /// </summary>
    public short CardNum
    {
        get
        {
            return cardNum;
        }
        set
        {
            cardNum = value;
            UpdateCardSprite();
        }
    }

    /// <summary>
    /// Gets and sets the state of the card.
    /// </summary>
    public State CardStt
    {
        get
        {
            return cardState;
        }
        set
        {
            cardState = value;
            UpdateCardSprite();
        }
    }

    /// <summary>
    /// Gets and sets the level at which the card is.
    /// </summary>
    public short CardLev
    {
        get
        {
            return cardLevel;
        }
        set
        {
            cardLevel = value;
        }
    }

    /// <summary>
    /// Gets and sets the position within the level at which the card is.
    /// </summary>
    public short CardPos
    {
        get
        {
            return cardPos;
        }
        set
        {
            cardPos = value;
        }
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Updates the card sprite according to the state of the card and its number.
    /// </summary>
    private void UpdateCardSprite()
    {

        if (cardState == State.Blue)
        {
            // Set the color of the card to blue.
            GetComponent<SpriteRenderer>().sprite = SpriteList[(cardNum - 1) * NCards];
        }
        else if (cardState == State.Gray)
        {
            // Set the color of the card to gray.
            GetComponent<SpriteRenderer>().sprite = SpriteList[(cardNum - 1) * NCards + 1];
        }
        else if (cardState == State.Red)
        {
            // Set the color of the card to red.
            GetComponent<SpriteRenderer>().sprite = SpriteList[(cardNum - 1) * NCards + 2];
        }
        else if (cardState == State.Green)
        {
            // Set the color of the card to red.
            GetComponent<SpriteRenderer>().sprite = SpriteList[(cardNum - 1) * NCards + 3];
        }
    }

    #endregion

}
