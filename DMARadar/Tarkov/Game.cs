using DMARadar.Misc;
using DMARadar.Tarkov;
using System;
using System.Collections.ObjectModel;

namespace DMARadar
{
	public class Game
	{
		private GameObjectManager _gom;
        private RegisteredPlayers _rgtPlayers;
		private ulong _localGameWorld;
        private readonly ulong _unityBase;
        private bool _inHideout = false;
		private volatile bool _inGame = false;
		private volatile string _mapName = string.Empty;
		private volatile bool _isScav = false;

		public enum GameStatus
		{
			NotFound,
			Found,
			Menu,
			Hideout,
			LoadingLoot,
			Matching,
			InGame,
			Error
		}

		public bool InGame
		{
			get => _inGame;
		}
		// in InHideout means local game world not false and registered players is 1
		public bool InHideout
		{
			get => _inHideout;
		}
		public bool IsScav
		{
			get => _isScav;
		}
		public string MapName
		{
			get => _mapName;
		}
		public int PlayerSide
		{
			get => 0;
		}
        public ReadOnlyDictionary<string, Player> Players
        {
            get => _rgtPlayers?.Players;
        }
        public Game(ulong unityBase)
        {
            _unityBase = unityBase;
        }

        private void GetMapName()
        {
            if (this._inHideout)
            {
                this._mapName = string.Empty;
                return;
            }

            try
            {
                var mapNamePrt = Memory.ReadPtrChain(this._localGameWorld, new uint[] { Offsets.LocalGameWorld.MainPlayer, Offsets.Player.Location });
                this._mapName = Memory.ReadUnityString(mapNamePrt);
            }
            catch
            {

                try
                {
                    var mapNamePrt = Memory.ReadPtr(this._localGameWorld + Offsets.LocalGameWorld.MapName);
                    if (mapNamePrt != 0)
                    {
                        this._mapName = Memory.ReadUnityString(mapNamePrt);
                    }
                }
                catch
                {
                    Program.Log("Couldn't find map name!!!");
                    this._mapName = "bigmap";
                }

            }
        }

        /// <summary>
        /// Helper method to locate Game World object.
        /// </summary>
        private ulong GetObjectFromList(ulong activeObjectsPtr, ulong lastObjectPtr, string objectName)
        {
            var activeObject = Memory.ReadValue<BaseObject>(Memory.ReadPtr(activeObjectsPtr));
            var lastObject = Memory.ReadValue<BaseObject>(Memory.ReadPtr(lastObjectPtr));
            if (activeObject.obj != 0x0 && lastObject.obj == 0x0)
            {
                // Add wait for lastObject to be populated
                Program.Log("Waiting for lastObject to be populated...");
                while (lastObject.obj == 0x0)
                {
                    lastObject = Memory.ReadValue<BaseObject>(Memory.ReadPtr(lastObjectPtr));
                    Thread.Sleep(1000);
                }
            }

            if (activeObject.obj != 0x0)
            {
                while (activeObject.obj != 0x0 && activeObject.obj != lastObject.obj)
                {
                    var objectNamePtr = Memory.ReadPtr(activeObject.obj + Offsets.GameObject.ObjectName);
                    var objectNameStr = Memory.ReadString(objectNamePtr, 64);
                    if (objectNameStr.Contains(objectName, StringComparison.OrdinalIgnoreCase))
                    {
                        Program.Log($"Found object {objectNameStr}");
                        return activeObject.obj;
                    }

                    activeObject = Memory.ReadValue<BaseObject>(activeObject.nextObjectLink); // Read next object
                }
            }
            if (lastObject.obj != 0x0)
            {
                var objectNamePtr = Memory.ReadPtr(lastObject.obj + Offsets.GameObject.ObjectName);
                var objectNameStr = Memory.ReadString(objectNamePtr, 64);
                if (objectNameStr.Contains(objectName, StringComparison.OrdinalIgnoreCase))
                {
                    Program.Log($"Found object {objectNameStr}");
                    return lastObject.obj;
                }
            }
            Program.Log($"Couldn't find object {objectName}");
            return 0;
        }

        /// <summary>
        /// Gets Game Object Manager structure.
        /// </summary>
        private bool GetGOM()
        {
            try
            {
                var addr = Memory.ReadPtr(_unityBase + Offsets.ModuleBase.GameObjectManager);
                _gom = Memory.ReadValue<GameObjectManager>(addr);
                Program.Log($"Found Game Object Manager at 0x{addr.ToString("X")}");
                return true;
            }
            catch (DMAShutdown) { throw; }
            catch (Exception ex)
            {
                throw new GameNotRunningException($"ERROR getting Game Object Manager, game may not be running: {ex}");
            }
        }

        /// <summary>
        /// Gets Local Game World address.
        /// </summary>
        public bool GetLGW()
        {
            var found = false;
            try
            {
                ulong gameWorld;
                ulong activeNodes;
                ulong lastActiveNode;
                try
                {
                    activeNodes = Memory.ReadPtr(_gom.ActiveNodes);
                    lastActiveNode = Memory.ReadPtr(_gom.LastActiveNode);
                    gameWorld = GetObjectFromList(activeNodes, lastActiveNode, "GameWorld");
                }
                catch
                {
                    GetGOM();
                    return found;
                }
                if (gameWorld == 0)
                {
                    Program.Log("Unable to find GameWorld Object, likely not in raid.");
                }
                else
                {
                    try
                    {
                        _localGameWorld = Memory.ReadPtrChain(gameWorld, Offsets.GameWorld.To_LocalGameWorld);
                        Program.Log($"Found LocalGameWorld at 0x{_localGameWorld.ToString("X")}");
                    }
                    catch
                    {
                        Program.Log("Couldnt find LocalGameWorld pointer");
                        Memory.GameStatus = GameStatus.Menu;
                    }

                    if (_localGameWorld == 0)
                    {
                        Program.Log("LocalGameWorld found but is 0");
                    }
                    else
                    {
                        Memory.GameStatus = GameStatus.Matching;

                        if (!Memory.ReadValue<bool>(_localGameWorld + 0x220))
                        {
                            Program.Log("Raid hasn't started!");
                        }
                        else
                        {
                            RegisteredPlayers registeredPlayers = new RegisteredPlayers(Memory.ReadPtr(_localGameWorld + Offsets.LocalGameWorld.RegisteredPlayers));
                            _rgtPlayers = registeredPlayers;
                        }
                    }
                }
            }
            catch (DMAShutdown)
            {
                throw; // Propagate the DMAShutdown exception upwards
            }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting Local Game World: {ex}. Retrying...");
            }

            return found;
        }

        public void GameLoop()
        {
            try
            {
                if (_localGameWorld == 0)
                {
                    GetLGW();
                }else
                {
                    GetMapName();
                    _rgtPlayers.UpdateList();
                    _rgtPlayers.UpdateAllPlayers();
                }
            }
            catch (DMAShutdown)
            {
                HandleDMAShutdown();
            }
            catch (RaidEnded e)
            {
                HandleRaidEnded(e);
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
        }

        /// <summary>
        /// Handles the scenario when DMA shutdown occurs.
        /// </summary>
        private void HandleDMAShutdown()
        {
            _inGame = false;
        }

        /// <summary>
        /// Handles the scenario when the raid ends.
        /// </summary>
        /// <param name="e">The RaidEnded exception instance containing details about the raid end.</param>
        private void HandleRaidEnded(RaidEnded e)
        {
            Program.Log("Raid has ended!");

            _inGame = false;
            Memory.GameStatus = GameStatus.Menu;
        }

        /// <summary>
        /// Handles unexpected exceptions that occur during the game loop.
        /// </summary>
        /// <param name="ex">The exception instance that was thrown.</param>
        private void HandleUnexpectedException(Exception ex)
        {
            Program.Log($"CRITICAL ERROR - Raid ended due to unhandled exception: {ex}");
            _inGame = false;
        }



    }

    #region Exceptions
    public class GameNotRunningException : Exception
	{
		public GameNotRunningException()
		{
		}

		public GameNotRunningException(string message)
			: base(message)
		{
		}

		public GameNotRunningException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	public class RaidEnded : Exception
	{
		public RaidEnded()
		{

		}

		public RaidEnded(string message)
			: base(message)
		{
		}

		public RaidEnded(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
	#endregion
}
