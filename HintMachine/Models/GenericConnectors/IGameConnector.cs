using System.Collections.Generic;

namespace HintMachine.Models.GenericConnectors
{
    public abstract class IGameConnector
    {
        /// <summary>
        /// Display name for this game used in the various UI components
        /// </summary>
        public string Name { get; protected set; } = "Unnamed Game";

        /// <summary>
        /// Brief description for the game used in the various UI components
        /// </summary>
        public string Description { get; protected set; } = "";

        /// <summary>
        /// The hardware platform the game was originally available on (e.g. PC for a PC game, "Nintendo 64" for the N64, etc...)
        /// </summary>
        public string Platform { get; protected set; } = "";

        /// <summary>
        /// String detailing which versions of the game have been tested to work with this connector
        /// </summary>
        public List<string> SupportedVersions { get; protected set; } = new List<string>{};

        /// <summary>
        /// String detailing which emulators have been tested to work with this connector (if relevant)
        /// </summary>
        public List<string> SupportedEmulators { get; protected set; } = new List<string> { };

        /// <summary>
        /// The author of the game connector
        /// </summary>
        public string Author { get; protected set; } = "";

        /// <summary>
        /// List of quests available for this game
        /// </summary>
        public List<HintQuest> Quests { get; protected set; } = new List<HintQuest>();

        /// <summary>
        /// The filename for the cover art of the game, which is looked for in the "Assets/covers/" directory
        /// </summary>
        public string CoverFilename { get; protected set; } = "unknown.png";

        /// <summary>
        /// Performs the full connection process by triggering BeforeConnect, Connect & AfterConnect methods.
        /// </summary>
        /// <returns>true if connection went well, false otherwise</returns>
        public bool DoConnect()
        {
            if (!BeforeConnect()) return false;
            if (!Connect()) return false;
            if (!AfterConnect()) return false;

            return true;
        }

        /// <summary>
        /// Function being called right before the connection process, which can be overriden.
        /// </summary>
        /// <returns>false if something went wrong, true if connection can keep going</returns>
        protected virtual bool BeforeConnect() => true;

        /// <summary>
        /// Abstract function meant to handle the connection to the related game process / files
        /// required for the data fetching to work afterwards.
        /// </summary>
        protected abstract bool Connect();

        /// <summary>
        /// Function being called right after the connection process, which can be overriden.
        /// </summary>
        /// <returns>false if something went wrong, true if connection can keep going</returns>
        protected virtual bool AfterConnect() => true;

        /// <summary>
        /// Abstract function handling disconnection from the game process / files, releasing all
        /// handles in a clean way.
        /// </summary>     
        public abstract void Disconnect();

        /// <summary>
        /// Performs the full polling process by triggering BeforePoll, Poll & AfterPoll methods.
        /// </summary>
        /// <returns>true if polling went well, false otherwise</returns>
        public bool DoPoll()
        {
            if (!BeforePoll()) return false;
            if (!Poll()) return false;
            if (!AfterPoll()) return false;

            return true;
        }

        /// <summary>
        /// Function being called right before the polling process, which can be overriden.
        /// </summary>
        /// <returns>false if something went wrong, true if polling can keep going</returns>
        protected virtual bool BeforePoll() => true;

        /// <summary>
        /// Abstract function handling the actual data retrieval from the game process / files,
        /// storing them in connector attributes for further usage.
        /// </summary>
        protected abstract bool Poll();

        /// <summary>
        /// Function being called right after the polling process, which can be overriden.
        /// </summary>
        /// <returns>false if something went wrong, true if polling can keep going</returns>
        protected virtual bool AfterPoll() => true;
    }
}
