using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    // Connector is currently inavailable until we find a way to display all quests in the panel
    // [AvailableGameConnector]
    public class SkyrimSpecialEditionConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "SkyrimSE",
            Hash = "537527CEC58E458425255B320844F0F1DAC1A902A93D21891A50DF7C1F238B8D"
        };

        private readonly HintQuestCumulative _dungeonsClearedQuest = new HintQuestCumulative
        {
            Name = "Dungeons Cleared",
            GoalValue = 1,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 120,
        };

        private readonly HintQuestCumulative _locationsFoundQuest = new HintQuestCumulative
        {
            Name = "Locations Found",
            GoalValue = 10,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _goldFoundQuest = new HintQuestCumulative
        {
            Name = "Gold Found",
            GoalValue = 2500,
        };

        private readonly HintQuestCumulative _chestsLootedQuest = new HintQuestCumulative
        {
            Name = "Containers Looted",
            GoalValue = 50,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _skillBooksReadQuest = new HintQuestCumulative
        {
            Name = "Skill Books Read",
            GoalValue = 1,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 30,
        };

        private readonly HintQuestCumulative _questsCompletedQuest = new HintQuestCumulative
        {
            Name = "Quests Completed",
            GoalValue = 1,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 120,
        };

        private readonly HintQuestCumulative _entitiesKilledQuest = new HintQuestCumulative
        {
            Name = "Entities Killed",
            GoalValue = 25,
            MaxIncrease = 3,
        };

        private readonly HintQuestCumulative _spellsLearnedQuest = new HintQuestCumulative
        {
            Name = "Spells Learned",
            GoalValue = 3,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 30,
        };

        private readonly HintQuestCumulative _dragonSoulsCollectedQuest = new HintQuestCumulative
        {
            Name = "Dragon Souls Collected",
            GoalValue = 1,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 120,
        };

        private readonly HintQuestCumulative _wordsOfPowerLearnedQuest = new HintQuestCumulative
        {
            Name = "Words of Power Learned",
            GoalValue = 1,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 120,
        };

        private readonly HintQuestCumulative _soulsTrappedQuest = new HintQuestCumulative
        {
            Name = "Souls Trapped",
            GoalValue = 3,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _itemsCraftedQuest = new HintQuestCumulative
        {
            Name = "Items Crafted",
            GoalValue = 30,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _itemsEnchantedQuest = new HintQuestCumulative
        {
            Name = "Items Enchanted",
            GoalValue = 10,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _potionsMixedQuest = new HintQuestCumulative
        {
            Name = "Potions / Poisons Mixed",
            GoalValue = 50,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _ingredientsHarvestedQuest = new HintQuestCumulative
        {
            Name = "Ingredients Harvested",
            GoalValue = 100,
            MaxIncrease = 5,
        };

        private readonly HintQuestCumulative _nirnrootsFoundQuest = new HintQuestCumulative
        {
            Name = "Nirnroots Found",
            GoalValue = 3,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _bountyAccumulatedQuest = new HintQuestCumulative
        {
            Name = "Total Bounty Accumulated",
            GoalValue = 5000,
        };

        private readonly HintQuestCumulative _locksPickedQuest = new HintQuestCumulative
        {
            Name = "Locks Picked",
            GoalValue = 10,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _itemsPickpocketedQuest = new HintQuestCumulative
        {
            Name = "Items Pickpocketed",
            GoalValue = 10,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _jailsEscapedQuest = new HintQuestCumulative
        {
            Name = "Jails Escaped",
            GoalValue = 1,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 300,
        };

        private readonly HintQuestCumulative _itemsStolenQuest = new HintQuestCumulative
        {
            Name = "Items Stolen",
            GoalValue = 100,
            MaxIncrease = 1,
        };

        private ProcessRamWatcher _ram = null;

        public SkyrimSpecialEditionConnector()
        {
            Name = "Skyrim Special Edition";
            Description = "Winner of more than 200 Game of the Year Awards, Skyrim Special Edition brings the epic fantasy to life in stunning detail. The Special Edition includes the critically acclaimed game and add-ons with all-new features like remastered art and effects, volumetric god rays, dynamic depth of field, screen-space reflections, and more. Skyrim Special Edition also brings the full power of mods to the PC and consoles. New quests, environments, characters, dialogue, armor, weapons and more – with Mods, there are no limits to what you can experience.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "skyrim_special_edition.png";
            Author = "Serpent.AI";

            Quests.Add(_dungeonsClearedQuest);
            Quests.Add(_locationsFoundQuest);
            Quests.Add(_goldFoundQuest);
            Quests.Add(_chestsLootedQuest);
            Quests.Add(_skillBooksReadQuest);
            Quests.Add(_questsCompletedQuest);
            Quests.Add(_entitiesKilledQuest);
            Quests.Add(_spellsLearnedQuest);
            Quests.Add(_dragonSoulsCollectedQuest);
            Quests.Add(_wordsOfPowerLearnedQuest);
            Quests.Add(_soulsTrappedQuest);
            Quests.Add(_itemsCraftedQuest);
            Quests.Add(_itemsEnchantedQuest);
            Quests.Add(_potionsMixedQuest);
            Quests.Add(_ingredientsHarvestedQuest);
            Quests.Add(_nirnrootsFoundQuest);
            Quests.Add(_bountyAccumulatedQuest);
            Quests.Add(_locksPickedQuest);
            Quests.Add(_itemsPickpocketedQuest);
            Quests.Add(_jailsEscapedQuest);
            Quests.Add(_itemsStolenQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(GAME_VERSION_STEAM);
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long generalStatsStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x30F0018, new int[] { 0x240 });

            if (generalStatsStructAddress != 0)
            {
                try
                {
                    long locationsFoundValue = _ram.ReadUint32(generalStatsStructAddress + 0x10);
                    long dungeonsClearedValue = _ram.ReadUint32(generalStatsStructAddress + 0x30);
                    long goldFoundValue = _ram.ReadUint32(generalStatsStructAddress + 0xD0);
                    long chestsLootedValue = _ram.ReadUint32(generalStatsStructAddress + 0x110);
                    long skillBooksReadValue = _ram.ReadUint32(generalStatsStructAddress + 0x150);
                    long questsCompletedValue = _ram.ReadUint32(generalStatsStructAddress + 0x2D0);
                    long peopleKilledValue = _ram.ReadUint32(generalStatsStructAddress + 0x430);
                    long animalsKilledValue = _ram.ReadUint32(generalStatsStructAddress + 0x450);
                    long creaturesKilledValue = _ram.ReadUint32(generalStatsStructAddress + 0x470);
                    long undeadKilledValue = _ram.ReadUint32(generalStatsStructAddress + 0x490);
                    long daedraKilledValue = _ram.ReadUint32(generalStatsStructAddress + 0x4B0);
                    long automatonsKilledValue = _ram.ReadUint32(generalStatsStructAddress + 0x4D0);
                    long spellsLearnedValue = _ram.ReadUint32(generalStatsStructAddress + 0x5D0);
                    long dragonSoulsCollectedValue = _ram.ReadUint32(generalStatsStructAddress + 0x630);
                    long wordsOfPowerLearnedValue = _ram.ReadUint32(generalStatsStructAddress + 0x650);
                    long soulsTrappedValue = _ram.ReadUint32(generalStatsStructAddress + 0x750);
                    long weaponsCraftedValue = _ram.ReadUint32(generalStatsStructAddress + 0x7B0);
                    long armorCraftedValue = _ram.ReadUint32(generalStatsStructAddress + 0x7F0);
                    long itemsEnchantedValue = _ram.ReadUint32(generalStatsStructAddress + 0x770);
                    long potionsMixedValue = _ram.ReadUint32(generalStatsStructAddress + 0x810);
                    long poisonsMixedValue = _ram.ReadUint32(generalStatsStructAddress + 0x850);
                    long ingredientsHarvestedValue = _ram.ReadUint32(generalStatsStructAddress + 0x890);
                    long nirnrootsFoundValue = _ram.ReadUint32(generalStatsStructAddress + 0x8D0);
                    long bountyAccumulatedValue = _ram.ReadUint32(generalStatsStructAddress + 0x910);
                    long locksPickedValue = _ram.ReadUint32(generalStatsStructAddress + 0x950);
                    long itemsPickpocketedValue = _ram.ReadUint32(generalStatsStructAddress + 0x990);
                    long jailsEscapedValue = _ram.ReadUint32(generalStatsStructAddress + 0xA10);
                    long itemsStolenValue = _ram.ReadUint32(generalStatsStructAddress + 0xA30);

                    if (locationsFoundValue > 0)
                    {
                        _locationsFoundQuest.UpdateValue(locationsFoundValue);
                        _dungeonsClearedQuest.UpdateValue(dungeonsClearedValue);
                        _goldFoundQuest.UpdateValue(goldFoundValue);
                        _chestsLootedQuest.UpdateValue(chestsLootedValue);
                        _skillBooksReadQuest.UpdateValue(skillBooksReadValue);
                        _questsCompletedQuest.UpdateValue(questsCompletedValue);
                        _entitiesKilledQuest.UpdateValue(peopleKilledValue + animalsKilledValue + creaturesKilledValue + undeadKilledValue + daedraKilledValue + automatonsKilledValue);
                        _spellsLearnedQuest.UpdateValue(spellsLearnedValue);
                        _dragonSoulsCollectedQuest.UpdateValue(dragonSoulsCollectedValue);
                        _wordsOfPowerLearnedQuest.UpdateValue(wordsOfPowerLearnedValue);
                        _soulsTrappedQuest.UpdateValue(soulsTrappedValue);
                        _itemsCraftedQuest.UpdateValue(weaponsCraftedValue + armorCraftedValue);
                        _itemsEnchantedQuest.UpdateValue(itemsEnchantedValue);
                        _potionsMixedQuest.UpdateValue(potionsMixedValue + poisonsMixedValue);
                        _ingredientsHarvestedQuest.UpdateValue(ingredientsHarvestedValue);
                        _nirnrootsFoundQuest.UpdateValue(nirnrootsFoundValue);
                        _bountyAccumulatedQuest.UpdateValue(bountyAccumulatedValue);
                        _locksPickedQuest.UpdateValue(locksPickedValue);
                        _itemsPickpocketedQuest.UpdateValue(itemsPickpocketedValue);
                        _jailsEscapedQuest.UpdateValue(jailsEscapedValue);
                        _itemsStolenQuest.UpdateValue(itemsStolenValue);
                    }
                    else
                    {
                        _locationsFoundQuest.IgnoreNextValue();
                        _dungeonsClearedQuest.IgnoreNextValue();
                        _goldFoundQuest.IgnoreNextValue();
                        _chestsLootedQuest.IgnoreNextValue();
                        _skillBooksReadQuest.IgnoreNextValue();
                        _questsCompletedQuest.IgnoreNextValue();
                        _entitiesKilledQuest.IgnoreNextValue();
                        _spellsLearnedQuest.IgnoreNextValue();
                        _dragonSoulsCollectedQuest.IgnoreNextValue();
                        _wordsOfPowerLearnedQuest.IgnoreNextValue();
                        _soulsTrappedQuest.IgnoreNextValue();
                        _itemsCraftedQuest.IgnoreNextValue();
                        _itemsEnchantedQuest.IgnoreNextValue();
                        _potionsMixedQuest.IgnoreNextValue();
                        _ingredientsHarvestedQuest.IgnoreNextValue();
                        _nirnrootsFoundQuest.IgnoreNextValue();
                        _bountyAccumulatedQuest.IgnoreNextValue();
                        _locksPickedQuest.IgnoreNextValue();
                        _itemsPickpocketedQuest.IgnoreNextValue();
                        _jailsEscapedQuest.IgnoreNextValue();
                        _itemsStolenQuest.IgnoreNextValue();
                    }
                }
                catch { }
            }

            return true;
        }
    }
}