﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using STROOP.Structs;
using System.ComponentModel;
using STROOP.Utilities;
using STROOP.Structs.Gui;

namespace STROOP.M64
{
    /// <summary>
    /// The header of a .m64 recording.
    /// </summary>
    public class M64Header
    {
        /// <summary>
        /// An enum to determine whether the recording starts from a snapshot or the start of the game
        /// </summary>
        public enum MovieStartTypeEnum { 
            /// <summary>
            /// Start the recording from the bootup of the game.
            /// </summary>
            FromStart, 
            /// <summary>
            /// Start the recording from a particular snapshot.
            /// </summary>
            FromSnapshot
        }

        // the file this belongs to
        private readonly M64File _m64File;
        private readonly M64Gui _gui;

        // 018 4-byte little-endian unsigned int: number of input samples for any controllers
        private int _numInputs;

        /// <summary>
        /// The number of inputs.
        /// </summary>
        /// <value>018 4-byte little-endian unsigned int: number of input samples for any controllers</value>
        [Category("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("\u200B\u200B\u200B\u200BNum Inputs")]
        public int NumInputs { get => _numInputs; set { _numInputs = value; NotifyChange(); } }

        // 00C 4-byte little-endian unsigned int: number of frames(vertical interrupts)
        private int _numVis;

        /// <summary>
        /// The total number of frames.
        /// </summary>
        /// <value>00C 4-byte little-endian unsigned int: number of frames (vertical interrupts)</value>
        [Category("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("\u200B\u200B\u200BNum VIs")]
        public int NumVis { get => _numVis; set { _numVis = value; NotifyChange(); } }

        // 010 4-byte little-endian unsigned int: rerecord count
        private int _numRerecords;
        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("\u200B\u200BNum Rerecords")]
        public int NumRerecords { get => _numRerecords; set { _numRerecords = value; NotifyChange(); } }

        // 01C 2-byte unsigned int: movie start type
        // value 1: movie begins from snapshot(the snapshot will be loaded from an externalfile
        //     with the movie filename and a .st extension)
        // value 2: movie begins from power-on
        // other values: invalid movie
        private MovieStartTypeEnum _movieStartType;

        /// <summary>
        /// Where the recording starts from.
        /// </summary>
        /// <value>
        /// value 1: movie begins from snapshot(the snapshot will be loaded from an externalfile
        ///     with the movie filename and a .st extension)
        /// value 2: movie begins from power-on
        /// other values: invalid movie
        /// </value>
        /// <seealso cref="MovieStartTypeEnum"/>
        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("\u200BMovie Start Type")]
        public MovieStartTypeEnum MovieStartType { get => _movieStartType; set { _movieStartType = value; NotifyChange(); } }

        // 014 1-byte unsigned int: frames(vertical interrupts) per second
        private byte _fps;

        /// <summary>
        /// The FPS of the recording
        /// </summary>
        /// <value>014 1-byte unsigned int: frames(vertical interrupts) per second</value>
        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("FPS")]
        public byte Fps { get => _fps; set { _fps = value; NotifyChange(); } }

        // 0C4 32-byte ASCII string: internal name of ROM used when recording, directly from ROM
        private string _romName;

        /// <summary>
        /// The name of the ROM used for recording.
        /// </summary>
        /// <value>0C4 32-byte ASCII string: internal name of ROM used when recording, directly from ROM.</value>
        [CategoryAttribute("\u200B\u200B\u200B\u200BRom"), DisplayName("\u200B\u200BRom Name")]
        public string RomName { get => _romName; set { _romName = StringUtilities.Cap(value, 32); NotifyChange(); } }

        // 0E8 2-byte unsigned int: country code of ROM used when recording, directly from ROM
        private ushort _countryCode;

        /// <summary>
        /// The code that matches to a particular country or region.
        /// </summary>
        /// <value>0E8 2-byte unsigned int: country code of ROM used when recording, directly from ROM</value>
        [CategoryAttribute("\u200B\u200B\u200B\u200BRom"), DisplayName("\u200BCountry Code")]
        public ushort CountryCode { get => _countryCode; set { _countryCode = value; NotifyChange(); } }

        // 0E4 4-byte unsigned int: CRC32 of ROM used when recording, directly from ROM
        private uint _crc32;

        /// <summary>
        /// The CRC-32 used by the ROM
        /// </summary>
        /// <value>0E4 4-byte unsigned int: CRC32 of ROM used when recording, directly from ROM.</value>
        [CategoryAttribute("\u200B\u200B\u200B\u200BRom"), DisplayName("CRC32")]
        public uint Crc32 { get => _crc32; set { _crc32 = value; NotifyChange(); } }

        // 222 222-byte UTF-8 string: author name info
        private string _author;

        /// <summary>
        /// The author of the recording
        /// </summary>
        /// <value>222 222-byte UTF-8 string: author name info</value>
        [CategoryAttribute("\u200B\u200B\u200BDescription"), DisplayName("\u200BAuthor")]
        public string Author { get => _author; set { _author = StringUtilities.Cap(value, 222); NotifyChange(); } }

        // 300 256-byte UTF-8 string: author movie description info
        private string _description;
        [CategoryAttribute("\u200B\u200B\u200BDescription"), DisplayName("Description")]
        public string Description { get => _description; set { _description = StringUtilities.Cap(value, 256); NotifyChange(); } }

        // 015 1-byte unsigned int: number of controllers
        private byte _numControllers;
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200B\u200BNum Controllers")]
        public byte NumControllers { get => _numControllers; set { _numControllers = value; NotifyChange(); } }

        // 020 4-byte unsigned int: controller flags
        // bit 0: controller 1 present
        // bit 4: controller 1 has mempak
        // bit 8: controller 1 has rumblepak
        // +1..3 for controllers 2..4.
        private bool _controller1Present;

        /// <summary>
        /// Whether Controller 1 is present.
        /// </summary>
        /// <value>Whether Controller 1 is present.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200BController 1 Present")]
        public bool Controller1Present { get => _controller1Present; set { _controller1Present = value; NotifyChange(); } }

        private bool _controller2Present;
     
        /// <summary>
        /// Whether Controller 2 is present.
        /// </summary>
        /// <value>Whether Controller 2 is present.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200BController 2 Present")]
        public bool Controller2Present { get => _controller2Present; set { _controller2Present = value; NotifyChange(); } }

        private bool _controller3Present;

        /// <summary>
        /// Whether Controller 3 is present.
        /// </summary>
        /// <value>Whether Controller 3 is present.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200BController 3 Present")]
        public bool Controller3Present { get => _controller3Present; set { _controller3Present = value; NotifyChange(); } }

        private bool _controller4Present;

        /// <summary>
        /// Whether Controller 4 is present.
        /// </summary>
        /// <value>Whether Controller 4 is present.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200BController 4 Present")]
        public bool Controller4Present { get => _controller4Present; set { _controller4Present = value; NotifyChange(); } }

        private bool _controller1MemPak;

        /// <summary>
        /// Whether Controller 1 has a Mem Pak.
        /// </summary>
        /// <value>Whether Controller 1 has a Mem Pak.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200BController 1 MemPak")]
        public bool Controller1MemPak { get => _controller1MemPak; set { _controller1MemPak = value; NotifyChange(); } }

        private bool _controller2MemPak;

        /// <summary>
        /// Whether Controller 2 has a Mem Pak.
        /// </summary>
        /// <value>Whether Controller 2 has a Mem Pak.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200BController 2 MemPak")]
        public bool Controller2MemPak { get => _controller2MemPak; set { _controller2MemPak = value; NotifyChange(); } }

        private bool _controller3MemPak;

        /// <summary>
        /// Whether Controller 3 has a Mem Pak.
        /// </summary>
        /// <value>Whether Controller 3 has a Mem Pak.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200BController 3 MemPak")]
        public bool Controller3MemPak { get => _controller3MemPak; set { _controller3MemPak = value; NotifyChange(); } }

        private bool _controller4MemPak;

        /// <summary>
        /// Whether Controller 4 has a Mem Pak.
        /// </summary>
        /// <value>Whether Controller 4 has a Mem Pak.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200BController 4 MemPak")]
        public bool Controller4MemPak { get => _controller4MemPak; set { _controller4MemPak = value; NotifyChange(); } }

        private bool _controller1RumblePak;

        /// <summary>
        /// Whether Controller 1 has a Rumble Pak.
        /// </summary>
        /// <value>Whether Controller 1 has a Rumble Pak.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("Controller 1 RumblePak")]
        public bool Controller1RumblePak { get => _controller1RumblePak; set { _controller1RumblePak = value; NotifyChange(); } }

        private bool _controller2RumblePak;

        /// <summary>
        /// Whether Controller 2 has a Rumble Pak.
        /// </summary>
        /// <value>Whether Controller 2 has a Rumble Pak.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("Controller 2 RumblePak")]
        public bool Controller2RumblePak { get => _controller2RumblePak; set { _controller2RumblePak = value; NotifyChange(); } }

        private bool _controller3RumblePak;

        /// <summary>
        /// Whether Controller 3 has a Rumble Pak.
        /// </summary>
        /// <value>Whether Controller 3 has a Rumble Pak.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("Controller 3 RumblePak")]
        public bool Controller3RumblePak { get => _controller3RumblePak; set { _controller3RumblePak = value; NotifyChange(); } }

        private bool _controller4RumblePak;

        /// <summary>
        /// Whether Controller 4 has a Rumble Pak.
        /// </summary>
        /// <value>Whether Controller 4 has a Rumble Pak.</value>
        [CategoryAttribute("\u200B\u200BController"), DisplayName("Controller 4 RumblePak")]
        public bool Controller4RumblePak { get => _controller4RumblePak; set { _controller4RumblePak = value; NotifyChange(); } }

        // 122 64-byte ASCII string: name of video plugin used when recording, directly from plugin
        private string _videoPlugin;

        /// <summary>
        /// The video plugin that the recording took place on.
        /// </summary>
        /// <value>122 64-byte ASCII string: name of video plugin used when recording, directly from plugin</value>
        [CategoryAttribute("\u200BPlugin"), DisplayName("\u200B\u200B\u200BVideo Plugin")]
        public string VideoPlugin { get => _videoPlugin; set { _videoPlugin = StringUtilities.Cap(value, 64); NotifyChange(); } }

        // 162 64-byte ASCII string: name of sound plugin used when recording, directly from plugin
        private string _soundPlugin;

        /// <summary>
        /// The sound plugin that the recording took place using.
        /// </summary>
        /// <value>162 64-byte ASCII string: name of sound plugin used when recording, directly from plugin</value>
        [CategoryAttribute("\u200BPlugin"), DisplayName("\u200B\u200BSound Plugin")]
        public string SoundPlugin { get => _soundPlugin; set { _soundPlugin = StringUtilities.Cap(value, 64); NotifyChange(); } }

        // 1A2 64-byte ASCII string: name of input plugin used when recording, directly from plugin
        private string _inputPlugin;

        /// <summary>
        /// The input plugin that the recording took place using.
        /// </summary>
        /// <value>1A2 64-byte ASCII string: name of input plugin used when recording, directly from plugin.</value>
        [CategoryAttribute("\u200BPlugin"), DisplayName("\u200BInput Plugin")]
        public string InputPlugin { get => _inputPlugin; set { _inputPlugin = StringUtilities.Cap(value, 64); NotifyChange(); } }

        // 1E2 64-byte ASCII string: name of rsp plugin used when recording, directly from plugin
        private string _rspPlugin;

        /// <summary>
        /// The RSP plugin that the recording took place using.
        /// </summary>
        /// <value>1E2 64-byte ASCII string: name of rsp plugin used when recording, directly from plugin</value>
        [CategoryAttribute("\u200BPlugin"), DisplayName("RSP Plugin")]
        public string RspPlugin { get => _rspPlugin; set { _rspPlugin = StringUtilities.Cap(value, 64); NotifyChange(); } }

        // 000 4-byte signature: 4D 36 34 1A "M64\x1A"
        private uint _signature;

        /// <summary>
        /// The four-byte .m64 signature, should consist of 0x4D36311A
        /// </summary>
        /// <value>000 4-byte signature: 4D 36 34 1A "M64\x1A"</value>
        [CategoryAttribute("Mupen"), DisplayName("\u200B\u200BSignature")]
        public uint Signature { get => _signature; set { _signature = value; NotifyChange(); } }

        // 004 4-byte little-endian unsigned int: version number, should be 3
        private uint _versionNumber;
        [CategoryAttribute("Mupen"), DisplayName("\u200BVersion Number")]
        public uint VersionNumber { get => _versionNumber; set { _versionNumber = value; NotifyChange(); } }

        // 008 4-byte little-endian integer: movie "uid" - identifies the movie-savestate relationship,
        // also used as the recording time in Unix epoch format
        private int _uid;

        /// <summary>
        /// The UID of the recording.
        /// </summary>
        /// <value>008 4-byte little-endian integer: movie "uid" - identifies the movie-savestate relationship.</value>
        /// <remarks>Also used as the recording time in Unix epoch format.</remarks>
        [CategoryAttribute("Mupen"), DisplayName("UID")]
        public int Uid { get => _uid; set { _uid = value; NotifyChange(); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:STROOP.M64.M64Header"/> class.
        /// </summary>
        /// <param name="m64File">The .m64 file this belongs to.</param>
        /// <param name="gui">The GUI that this header belongs to.</param>
        public M64Header(M64File m64File, M64Gui gui)
        {
            _m64File = m64File;
            _gui = gui;
        }

        // if there is a change, signal that things have been modified
        private void NotifyChange()
        {
            _m64File.IsModified = true;
            _gui.PropertyGridHeader.Refresh();
        }

        /// <summary>
        /// Load an array of bytes into the header.
        /// </summary>
        /// <param name="bytes">The bytes to base this header off of.</param>
        public void LoadBytes(byte[] bytes)
        {
            // if the number of bytes don't line up with the .m64 header, throw an exception
            if (bytes.Length != M64Config.HeaderSize) throw new ArgumentOutOfRangeException();

            Signature = BitConverter.ToUInt32(bytes, 0x000);
            VersionNumber = BitConverter.ToUInt32(bytes, 0x004);
            Uid = BitConverter.ToInt32(bytes, 0x008);
            NumVis = BitConverter.ToInt32(bytes, 0x00C);
            NumRerecords = BitConverter.ToInt32(bytes, 0x010);
            Fps = bytes[0x014];
            NumControllers = bytes[0x015];
            NumInputs = BitConverter.ToInt32(bytes, 0x018);

            // get the type of the movie from the short value
            short movieStartTypeShort = BitConverter.ToInt16(bytes, 0x01C);
            MovieStartType = ConvertShortToMovieStartTypeEnum(movieStartTypeShort);

            // convert the controller flags value into the booleans used in the controller variables
            uint controllerFlagsValue = BitConverter.ToUInt16(bytes, 0x020);
            Controller1Present = (controllerFlagsValue & (1 << 0)) != 0;
            Controller2Present = (controllerFlagsValue & (1 << 1)) != 0;
            Controller3Present = (controllerFlagsValue & (1 << 2)) != 0;
            Controller4Present = (controllerFlagsValue & (1 << 3)) != 0;
            Controller1MemPak = (controllerFlagsValue & (1 << 4)) != 0;
            Controller2MemPak = (controllerFlagsValue & (1 << 5)) != 0;
            Controller3MemPak = (controllerFlagsValue & (1 << 6)) != 0;
            Controller4MemPak = (controllerFlagsValue & (1 << 7)) != 0;
            Controller1RumblePak = (controllerFlagsValue & (1 << 8)) != 0;
            Controller2RumblePak = (controllerFlagsValue & (1 << 9)) != 0;
            Controller3RumblePak = (controllerFlagsValue & (1 << 10)) != 0;
            Controller4RumblePak = (controllerFlagsValue & (1 << 11)) != 0;

            RomName = Encoding.ASCII.GetString(bytes, 0x0C4, 32).Replace("\0", "");
            Crc32 = BitConverter.ToUInt32(bytes, 0x0E4);
            CountryCode = BitConverter.ToUInt16(bytes, 0x0E8);
            VideoPlugin = Encoding.ASCII.GetString(bytes, 0x122, 64).Replace("\0", "");
            SoundPlugin = Encoding.ASCII.GetString(bytes, 0x162, 64).Replace("\0", "");
            InputPlugin = Encoding.ASCII.GetString(bytes, 0x1A2, 64).Replace("\0", "");
            RspPlugin = Encoding.ASCII.GetString(bytes, 0x1E2, 64).Replace("\0", "");
            Author = Encoding.UTF8.GetString(bytes, 0x222, 222).Replace("\0", "");
            Description = Encoding.UTF8.GetString(bytes, 0x300, 256).Replace("\0", "");

            // Verify that serialization works correctly
            if (!Enumerable.SequenceEqual(bytes, ToBytes())) throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Convert this header to an array of bytes
        /// </summary>
        /// <returns>The bytes that represent this header.</returns>
        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(TypeUtilities.GetBytes(Signature));
            bytes.AddRange(TypeUtilities.GetBytes(VersionNumber));
            bytes.AddRange(TypeUtilities.GetBytes(Uid));
            bytes.AddRange(TypeUtilities.GetBytes(NumVis));
            bytes.AddRange(TypeUtilities.GetBytes(NumRerecords));
            bytes.AddRange(TypeUtilities.GetBytes(Fps));
            bytes.AddRange(TypeUtilities.GetBytes(NumControllers));
            bytes.AddRange(new byte[2]);
            bytes.AddRange(TypeUtilities.GetBytes(NumInputs));
            bytes.AddRange(TypeUtilities.GetBytes(ConvertMovieStartTypeEnumToShort(MovieStartType)));
            bytes.AddRange(new byte[2]);
            bytes.AddRange(TypeUtilities.GetBytes(GetControllerFlagsValue()));
            bytes.AddRange(new byte[160]);
            bytes.AddRange(TypeUtilities.GetBytes(RomName, 32, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(Crc32));
            bytes.AddRange(TypeUtilities.GetBytes(CountryCode));
            bytes.AddRange(new byte[56]);
            bytes.AddRange(TypeUtilities.GetBytes(VideoPlugin, 64, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(SoundPlugin, 64, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(InputPlugin, 64, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(RspPlugin, 64, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(Author, 222, Encoding.UTF8));
            bytes.AddRange(TypeUtilities.GetBytes(Description, 256, Encoding.UTF8));
            if (bytes.Count != M64Config.HeaderSize) throw new ArgumentOutOfRangeException();
            return bytes.ToArray();
        }

        // convert the controller booleans to a value
        private uint GetControllerFlagsValue()
        {
            uint flags = 0;
            uint currentBit = 1;
            foreach (bool boolValue in GetControllerBoolList())
            {
                if (boolValue) flags |= currentBit;
                currentBit <<= 1;
            }
            return flags;
        }

        // get the list of controller booleans
        private List<bool> GetControllerBoolList()
        {
            return new List<bool>()
            {
                Controller1Present,
                Controller2Present,
                Controller3Present,
                Controller4Present,
                Controller1MemPak,
                Controller2MemPak,
                Controller3MemPak,
                Controller4MemPak,
                Controller1RumblePak,
                Controller2RumblePak,
                Controller3RumblePak,
                Controller4RumblePak,
            };
        }
     
        // note: you could probably axe these functions if you made the type of the MovieStartTypeEnum build to short
     
        // gets the movie start type and converts it to number
        private short ConvertMovieStartTypeEnumToShort(MovieStartTypeEnum movieStartType)
        {
            switch (movieStartType)
            {
                case MovieStartTypeEnum.FromStart:
                    return 2;
                case MovieStartTypeEnum.FromSnapshot:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // gets a number value and converts it to a movie type
        private MovieStartTypeEnum ConvertShortToMovieStartTypeEnum(short shortValue)
        {
            switch (shortValue)
            {
                case 1:
                    return MovieStartTypeEnum.FromSnapshot;
                case 2:
                    return MovieStartTypeEnum.FromStart;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Clear out the header of this instance
        /// </summary>
        public void Clear()
        {
            NumInputs = 0;
            NumVis = 0;
            NumRerecords = 0;
            MovieStartType = MovieStartTypeEnum.FromStart;
            Fps = 0;
            RomName = null;
            CountryCode = 0;
            Crc32 = 0;
            Author = null;
            Description = null;
            NumControllers = 0;
            Controller1Present = false;
            Controller2Present = false;
            Controller3Present = false;
            Controller4Present = false;
            Controller1MemPak = false;
            Controller2MemPak = false;
            Controller3MemPak = false;
            Controller4MemPak = false;
            Controller1RumblePak = false;
            Controller2RumblePak = false;
            Controller3RumblePak = false;
            Controller4RumblePak = false;
            VideoPlugin = null;
            SoundPlugin = null;
            InputPlugin = null;
            RspPlugin = null;
            Signature = 0;
            VersionNumber = 0;
            Uid = 0;
        }
    }
}
