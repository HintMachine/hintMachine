using System.Collections.Generic;
using System;
using SNI;
using Grpc.Net.Client;
using System.Net.Http;
using Grpc.Net.Client.Web;
using System.Text;
using System.Linq;

namespace HintMachine.GenericConnectors
{
    public abstract class ISNIConnector: IGameConnector
    {
        /// <summary>
        /// Client exposing DeviceMemory commands for connectors, connected to SNI.
        /// </summary>
        private DeviceMemory.DeviceMemoryClient SNIClient { get; set; }
        /// <summary>
        /// The currently connected SNES device connected to SNI. Use the Uri to send commands.
        /// </summary>
        private DevicesResponse.Types.Device Device { get; set; }
        /// <summary>
        /// Mapping information for the currently connected game.
        /// </summary>
        private DetectMemoryMappingResponse Mapping { get; set; }
        /// <summary>
        /// The 21-byte name of the rom as present at 0x7FC0 in the rom. 
        /// This is used to validate the rom being played is correct. 
        /// Any trailing whitespace will be trimmed.
        /// </summary>
        public string RomName { get; protected set; }

        public ISNIConnector()
        {
            Platform = "SNES";
            SupportedEmulators.Add("Snes9x-rr 1.60");
            SupportedEmulators.Add("bsnes-plus-nwa");
            SupportedEmulators.Add("Bizhawk (bsnes core)");
            SupportedEmulators.Add("Retroarch (bsnes-mercury core, certain games only)");
            SupportedEmulators.Add("FX PAK Pro");

        }

        /// <summary>
        /// Attempts to connect to SNI, find the console we wish to communicate with, and then confirm whether
        /// they are playing the specified game. Do not override in subclasses without calling base.Connect() first.
        /// </summary>
        /// <returns>Bool indicating successful connection to SNI and emulator.</returns>
        public override bool Connect()
        {
            GrpcChannelOptions channelOptions = new GrpcChannelOptions() { 
                HttpHandler = new GrpcWebHandler(new HttpClientHandler()), 
            };
            var channel = GrpcChannel.ForAddress("http://localhost:8190", channelOptions);
            Devices.DevicesClient client = new Devices.DevicesClient(channel);
            DevicesResponse deviceList = client.ListDevices(new DevicesRequest());
            int size = deviceList.CalculateSize();
            if (size > 0) 
            {
                List<string> uris = new List<string>();
                foreach(DevicesResponse.Types.Device device in deviceList.Devices)
                {
                    uris.Add(device.Uri);
                }
                ComboBoxPrompt prompt = new ComboBoxPrompt("Please select the SNES to connect to:", uris);
                if (prompt.ShowDialog() == true)
                {
                    Device = deviceList.Devices[prompt.ComboMain.SelectedIndex];
                }
                else return false;
            }
            else return false;
            // now get the rom name and compare to see if we keep it
            SNIClient = new DeviceMemory.DeviceMemoryClient(channel);
            Mapping = SNIClient.MappingDetect(new DetectMemoryMappingRequest { Uri = Device.Uri });
            ArraySegment<byte> array = new ArraySegment<byte>(Mapping.RomHeader00FFB0.ToByteArray(), 16, 21);
            string rom_name = Encoding.UTF8.GetString(array.ToArray());
            if (rom_name.Trim() == RomName.Trim())
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Disconnects from SNI and cleans up connector internals. Make sure to call base if overriding.
        /// </summary>
        public override void Disconnect()
        {
            //I don't actually have to do anything here, but we null device, mapping, and client in case
            SNIClient = null;
            Device = null;
            Mapping = null;

        }

        /// <summary>
        /// Reads a number of bytes from SNES memory from the given address with the given address space applied.
        /// </summary>
        /// <param name="address">The address of the bytes to read, as applied by the address space.</param>
        /// <param name="addressSpace">The address space to interpret the address.</param>
        /// <param name="size">Number of bytes to read.</param>
        /// <returns></returns>
        public byte[] ReadBytes(uint address, AddressSpace addressSpace, uint size)
        {
            SingleReadMemoryResponse response = SNIClient.SingleRead(new SingleReadMemoryRequest { Uri = Device.Uri, Request = new ReadMemoryRequest { RequestAddress = address, RequestAddressSpace = addressSpace, RequestMemoryMapping = Mapping.MemoryMapping, Size = size } });
            return response.Response.Data.ToByteArray();
        }

        /// <summary>
        /// Reads a single byte from SNES memory from the given address with the given address space applied.
        /// </summary>
        /// <param name="address">The address of the byte to read, as applied by the address space.</param>
        /// <param name="addressSpace">The address space to interpret the address.</param>
        /// <returns></returns>
        public byte ReadByte(uint address, AddressSpace addressSpace)
        {
            return ReadBytes(address, addressSpace, 1)[0];
        }

        /// <summary>
        /// Reads a 16-bit integer from SNES memory from the given address with the given address space applied.
        /// </summary>
        /// <param name="address">The address of the byte to read, as applied by the address space.</param>
        /// <param name="addressSpace">The address space to interpret the address.</param>
        /// <returns></returns>
        public short ReadInt16(uint address, AddressSpace addressSpace)
        {
            return BitConverter.ToInt16(ReadBytes(address, addressSpace, 2), 0);
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from SNES memory from the given address with the given address space applied.
        /// </summary>
        /// <param name="address">The address of the byte to read, as applied by the address space.</param>
        /// <param name="addressSpace">The address space to interpret the address.</param>
        /// <returns></returns>
        public ushort ReadUInt16(uint address, AddressSpace addressSpace)
        {
            return BitConverter.ToUInt16(ReadBytes(address, addressSpace, 2), 0);
        }

        /// <summary>
        /// Used to confirm that the specified game is still currently being played. 
        /// </summary>
        /// <returns>Bool stating whether the current game is still the one wanted by our connector.</returns>
        public bool ConfirmRomName()
        {
            string rom_name = Encoding.UTF8.GetString(ReadBytes(0x7FC0, AddressSpace.FxPakPro, 21)).Trim();
            if (rom_name != RomName.Trim())
            {
                return false;
            }
            else return true;
        }

    }
}
