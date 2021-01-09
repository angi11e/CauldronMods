﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.TheMistressOfFate
{
    public class DayOfSinnersCardController : DayCardController
    {
        public DayOfSinnersCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            soughtKeywords = new string[] { "one-shot" };
        }

        /*
         * "When this card flips face up, put any cards beneath it into play. If there are none, reveal cards from the top of the villain deck until a One-shot is revealed, put it into play, and shuffle the other revealed cards into the villain deck.",
         * "When that one-shot leaves play, put it beneath this card."
         */
        protected override IEnumerator DayFlipFaceUpEffect()
        {
            yield return GetAndPlayStoredCard(soughtKeywords);
            yield break;
        }
    }
}
