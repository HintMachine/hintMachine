using System.Collections.Generic;

namespace HintMachine
{
    public abstract class IGameConnector
    {
        public List<HintQuest> quests = new List<HintQuest>();

        /// <summary>
        /// Abstract function meant to return the display name for this game used in the various UI components
        /// </summary>
        public abstract string GetDisplayName();

        /// <summary>
        /// Abstract function meant to return a brief description for the game used in the various UI components
        /// </summary>
        public virtual string GetDescription() { return ""; }

        /// <summary>
        /// Abstract function meant to handle the connection to the related game process / files
        /// required for the data fetching to work afterwards.
        /// </summary>
        public abstract bool Connect();

        /// <summary>
        /// Abstract function handling disconnection from the game process / files, releasing all
        /// handles in a clean way.
        /// </summary>     
        public abstract void Disconnect();

        /// <summary>
        /// Abstract function handling the actual data retrieval from the game process / files,
        /// storing them in connector attributes for further usage.
        /// </summary>
        public abstract bool Poll();
    }
}
