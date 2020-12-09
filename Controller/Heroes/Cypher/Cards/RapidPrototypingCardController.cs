﻿using System;
using System.Collections;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;


namespace Cauldron.Cypher
{
    public class RapidPrototypingCardController : CypherBaseCardController
    {
        //==============================================================
        // Draw 2 cards.
        // Play any number of Augments from your hand.
        //==============================================================

        public static string Identifier = "RapidPrototyping";

        private const int CardsToDraw = 2;

        public RapidPrototypingCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            // Draw 2 cards.
            IEnumerator routine = base.DrawCards(base.HeroTurnTakerController, CardsToDraw);

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }

            // Play any number of Augments from your hand
            routine = base.GameController.PlayCards(this.DecisionMaker, card => card.IsInHand && IsAugment(card), true, true, 
                null, cardSource: GetCardSource());

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }
        }
    }
}