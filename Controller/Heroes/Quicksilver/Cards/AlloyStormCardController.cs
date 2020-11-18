﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cauldron.Quicksilver
{
    public class AlloyStormCardController : ComboCardController
    {
        public AlloyStormCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            //{Quicksilver} deals each non-hero target 1 projectile damage.
            IEnumerator coroutine = base.DealDamage(base.CharacterCard, (Card c) => !c.IsHero, 1, DamageType.Projectile);
            //You may play a Finisher, or {Quicksilver} may deal herself 2 melee damage and play a Combo.
            IEnumerator coroutine2 = base.ComboOrFinish();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
                yield return base.GameController.StartCoroutine(coroutine2);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
                base.GameController.ExhaustCoroutine(coroutine2);
            }
            yield break;
        }
    }
}