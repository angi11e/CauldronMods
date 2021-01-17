﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using Cauldron.Gargoyle;

namespace CauldronTests
{
    [TestFixture()]
    public class GargoyleTests : BaseTest
    {
        #region Gargoyle Utilities
        private HeroTurnTakerController gargoyle => FindHero("Gargoyle");
        private Card mobileDefensePlatform => GetCardInPlay("MobileDefensePlatform");
        private string[] gameDecks => new string[] { "BaronBlade", "Cauldron.Gargoyle", "Unity", "Bunker", "TheScholar", "Megalopolis" };

        private void StartTestGame()
        {
            SetupGameController(gameDecks);
            StartGame();

            DestroyCard(mobileDefensePlatform);
        }

        private void SetupIncapTest()
        {
            StartTestGame();
            SetupIncap(baron);
            AssertIncapacitated(gargoyle);
            GoToUseIncapacitatedAbilityPhase(gargoyle);
        }

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(gargoyle.CharacterCard, 1);
            DealDamage(villain, gargoyle, 2, DamageType.Melee);
        }

        #endregion Gargoyle Utilities

        [Test()]
        public void TestLoadGargoyle()
        {
            SetupGameController(gameDecks);

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(gargoyle);
            Assert.IsInstanceOf(typeof(GargoyleCharacterCardController), gargoyle.CharacterCardController);

            Assert.AreEqual(27, gargoyle.CharacterCard.HitPoints);
        }

        /*
         * Power:
         * "Select a target. Reduce the next damage it deals by 1. Increase the next damage Gargoyle deals by 1."
         */
        [Test()]
        public void TestGargoyleInnatePower()
        {
            StartTestGame();

            GoToUsePowerPhase(gargoyle);

            // Select a target. 
            DecisionSelectTarget = baron.CharacterCard;
            UsePower(gargoyle.CharacterCard);
                        
            // Reduce the next damage it deals by 1.
            QuickHPStorage(gargoyle.CharacterCard);
            DealDamage(baron.CharacterCard, gargoyle.CharacterCard, 1, DamageType.Melee);
            QuickHPCheckZero();

            // Increase the next damage Gargoyle deals by 1.
            QuickHPStorage(baron.CharacterCard);
            DealDamage(gargoyle.CharacterCard, baron.CharacterCard, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        /* 
         * Incap 1
         * "Increase the next damage dealt by a hero target by 2."
        */
        [Test()]
        public void TestGargoyleIncap1HeroCharacter()
        {
            SetupIncapTest();
            UseIncapacitatedAbility(gargoyle, 0);

            // Make sure it only affects hero damage
            QuickHPStorage(baron);
            DealDamage(bunker.CharacterCard, baron, 1, DamageType.Melee);
            QuickHPCheck(-3);

            // Should only be the next damage,  this packet of damage should not be increased
            QuickHPStorage(baron);
            DealDamage(bunker.CharacterCard, baron, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }
        [Test()]
        public void TestGargoyleIncap1HeroTarget()
        {
            Card mrChomps;

            SetupIncapTest();
            UseIncapacitatedAbility(gargoyle, 0);

            mrChomps = PlayCard(unity, "RaptorBot");

            // Make sure it only affects hero damage
            QuickHPStorage(baron);
            DealDamage(mrChomps, baron, 1, DamageType.Melee);
            QuickHPCheck(-3);

            // Should only be the next damage,  this packet of damage should not be increased
            QuickHPStorage(baron);
            DealDamage(mrChomps, baron, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }
        [Test()]
        public void TestGargoyleIncap1Villain()
        {
            SetupIncapTest();
            UseIncapacitatedAbility(gargoyle, 0);

            // Make sure it isn't affecting the villians damage
            QuickHPStorage(bunker);
            DealDamage(baron.CharacterCard, bunker, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }
        [Test()]
        public void TestGargoyleIncap1Environment()
        {
            TurnTakerController megalopolis;
            Card plummetingMonorail;

            SetupIncapTest();
            UseIncapacitatedAbility(gargoyle, 0);

            megalopolis = FindEnvironment();
            Assert.IsNotNull(megalopolis);

            plummetingMonorail = PlayCard(megalopolis, "PlummetingMonorail");
            Assert.IsNotNull(plummetingMonorail);

            // Make sure it isn't affecting the evironment target's damage
            QuickHPStorage(bunker);
            DealDamage(plummetingMonorail, bunker, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        /* 
         * Incap 2
         * "Reduce the next damage dealt to a hero target by 2."
        */
        [Test()]
        public void TestGargoyleIncap2HeroCharacter()
        {
            SetupIncapTest();
            UseIncapacitatedAbility(gargoyle, 1);

            // Make sure it only affects heroes
            QuickHPStorage(bunker);
            DealDamage(baron.CharacterCard, bunker, 2, DamageType.Melee);
            QuickHPCheckZero();

            // Should only be the next damage,  this packet of damage should not be increased
            QuickHPStorage(bunker);
            DealDamage(baron.CharacterCard, bunker, 2, DamageType.Melee);
            QuickHPCheck(-2);

        }
        [Test()]
        public void TestGargoyleIncap2HeroTarget()
        {
            Card mrChomps;

            SetupIncapTest();
            UseIncapacitatedAbility(gargoyle, 1);

            mrChomps = PlayCard(unity, "RaptorBot");

            // Make sure it only affects heroes
            QuickHPStorage(mrChomps);
            DealDamage(baron.CharacterCard, mrChomps, 2, DamageType.Melee);
            QuickHPCheckZero();

            // Should only be the next damage,  this packet of damage should not be increased
            QuickHPStorage(mrChomps);
            DealDamage(baron.CharacterCard, mrChomps, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }
        [Test()]
        public void TestGargoyleIncap2Villain()
        {
            SetupIncapTest();
            UseIncapacitatedAbility(gargoyle, 1);

            // Make sure it isn't affecting damage to the villian
            QuickHPStorage(baron);
            DealDamage(bunker.CharacterCard, baron, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }
        [Test()]
        public void TestGargoyleIncap2Environment()
        {
            TurnTakerController megalopolis;
            Card plummetingMonorail;

            SetupIncapTest();
            UseIncapacitatedAbility(gargoyle, 1);

            megalopolis = FindEnvironment();
            Assert.IsNotNull(megalopolis);

            plummetingMonorail = PlayCard(megalopolis, "PlummetingMonorail");
            Assert.IsNotNull(plummetingMonorail);

            // Make sure it isn't affecting damage to the evironment target
            QuickHPStorage(plummetingMonorail);
            DealDamage(bunker.CharacterCard, plummetingMonorail, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        /* 
         * Incap 3
         * "One player may draw a card now."
        */
        [Test()]
        public void TestGargoyleIncap3()
        {
            SetupIncapTest();
            AssertIncapLetsHeroDrawCard(gargoyle, 2, unity, 1);
        }
    }
}
