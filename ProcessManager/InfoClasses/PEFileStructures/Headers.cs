using System;

namespace ProcessManager.InfoClasses.PEFileStructures
{
    /// <summary>
    /// Intestazione COFF standard.
    /// </summary>
    public struct FileHeader
    {
        /// <summary>
        /// Architettura.
        /// </summary>
        public MachineType Machine { get; }
        /// <summary>
        /// Numero di sezioni, il loader di Windows limita questo valore a 96.
        /// </summary>
        public ushort NumberOfSections { get; }
        /// <summary>
        /// 
        /// </summary>
        public uint TimeDateStamp { get; }
        /// <summary>
        /// Puntatore alla tabella dei simboli.
        /// </summary>
        public uint PointerToSymbolTable { get; }
        /// <summary>
        /// Numero di simboli.
        /// </summary>
        public uint NumberOfSymbols { get; }
        /// <summary>
        /// Dimensione dell'intestazione opzionale.
        /// </summary>
        public ushort SizeOfOptionalHeader { get; }
        /// <summary>
        /// Caratteristiche dell'immagine.
        /// </summary>
        public FileCharacteristics Characteristics { get; }

        /// <summary>
        /// Inizializza la struttura <see cref="FileHeader"/>.
        /// </summary>
        /// <param name="Machine">Archittetura.</param>
        /// <param name="SectionsCount">Numero di sezioni.</param>
        /// <param name="TimeDateStamp"></param>
        /// <param name="SymbolTablePointer">Puntatore alla tabella dei simboli.</param>
        /// <param name="SymbolsCount">Numero di simboli.</param>
        /// <param name="OptionalHeaderSize">Dimensione dell'intestazione opzionale.</param>
        /// <param name="Characteristics">Caratteristiche dell'immagine.</param>
        public FileHeader(MachineType Machine, ushort SectionsCount, uint TimeDateStamp, uint SymbolTablePointer, uint SymbolsCount, ushort OptionalHeaderSize, FileCharacteristics Characteristics)
        {
            this.Machine = Machine;
            NumberOfSections = SectionsCount;
            this.TimeDateStamp = TimeDateStamp;
            PointerToSymbolTable = SymbolTablePointer;
            NumberOfSymbols = SymbolsCount;
            SizeOfOptionalHeader = OptionalHeaderSize;
            this.Characteristics = Characteristics;
        }
    }

    #region File Header Enumerations
    /// <summary>
    /// Architettura.
    /// </summary>
    public enum MachineType : ushort
    {
        Unknown,
        AM33 = 0x1d3,
        AMD64 = 0x8664,
        ARM = 0x1c0,
        ARM64 = 0xaa64,
        ARMNT = 0x1c4,
        EBC = 0xebc,
        I386 = 0x14c,
        IA64 = 0x200,
        M32R = 0x9041,
        MIPS16 = 0x266,
        MIPSFPU = 0x366,
        MIPSFPU16 = 0x466,
        POWERPC = 0x1f0,
        POWERPCFP = 0x1f1,
        R4000 = 0x166,
        RISCV32 = 0x5032,
        RISCV64 = 0x5064,
        RISCV128 = 0x5128,
        SH3 = 0x1a2,
        SH3DSP = 0x1a3,
        SH4 = 0x1a6,
        SH5 = 0x1a8,
        THUMB = 0x1c2,
        WCEMIPSV2 = 0x169
    }

    /// <summary>
    /// Caratteristiche dell'immagine.
    /// </summary>
    [Flags]
    public enum FileCharacteristics : ushort
    {
        /// <summary>
        /// L'immagine non contiene informazioni di trasferimento.
        /// </summary>
        RelocationsStripped = 0x0001,
        /// <summary>
        /// L'immagine è eseguibile.
        /// </summary>
        ExecutableImage,
        /// <summary>
        /// Numeri di linea COFF rimossi (deprecato).
        /// </summary>
        LineNumbersStripped = 0x0004,
        /// <summary>
        /// Voci della tabella dei simboli per simboli locali rimosse (deprecato).
        /// </summary>
        LocalSymbolsStripped = 0x0008,
        /// <summary>
        /// Riduce in modo aggressivo il working set (obsoleto).
        /// </summary>
        AggessiveWsTrim = 0x0010,
        /// <summary>
        /// L'applicazione può gestire indirizzi oltre i 2 GB.
        /// </summary>
        LargeAddressAware = 0x0020,
        /// <summary>
        /// Riservato.
        /// </summary>
        Reserved = 0x0040,
        /// <summary>
        /// Little endian (deprecato).
        /// </summary>
        BytesReversedLo = 0x0080,
        /// <summary>
        /// Archittettura basata su parole a 32 bit.
        /// </summary>
        Word32BitMachine = 0x0100,
        /// <summary>
        /// Informazioni di debug rimosse.
        /// </summary>
        DebugInfoStripped = 0x0200,
        /// <summary>
        /// Se l'immagine si trova su un dispositivo rimovibile, deve essere caricata completamente ed eseguita dal file di paging.
        /// </summary>
        RemovableRunFromSwap = 0x0400,
        /// <summary>
        /// Se l'immagine si trova su un dispositivo di rete, deve essere caricata completamente ed eseguita dal file di paging.
        /// </summary>
        NetRunFromSwap = 0x0800,
        /// <summary>
        /// File di sistema.
        /// </summary>
        System = 0x1000,
        /// <summary>
        /// DLL.
        /// </summary>
        DLL = 0x2000,
        /// <summary>
        /// L'immagine dovrebbe essere eseguita solo su sistemi uniprocessore.
        /// </summary>
        UniProcessorSystemOnly = 0x4000,
        /// <summary>
        /// Big endian.
        /// </summary>
        BytesReversedHi = 0x8000
    }
    #endregion
    /// <summary>
    /// Intestazione opzionale standard (32 bit).
    /// </summary>
    public struct OptionalHeaderStandard32
    {
        /// <summary>
        /// Stato dell'immagine.
        /// </summary>
        public PEType Magic { get; }
        /// <summary>
        /// Numero di versione maggiore del linker.
        /// </summary>
        public byte MajorLinkerVersion { get; }
        /// <summary>
        /// Numero di versione minore del linker.
        /// </summary>
        public byte MinorLinkerVersion { get; }
        /// <summary>
        /// Dimensione della dimensione della sezione di codice (text).
        /// </summary>
        public uint SizeOfCode { get; }
        /// <summary>
        /// Dimensione della sezione dei dati inizializzati.
        /// </summary>
        public uint SizeOfInitializedData { get; }
        /// <summary>
        /// Dimensione della sezione dei dati non inizializzati.
        /// </summary>
        public uint SizeOfUninitializedData { get; }
        /// <summary>
        /// Indirizzo del punto di entrata, rispetto alla base dell'immagine quando è caricata in memoria.
        /// </summary>
        public uint AddressOfEntryPoint { get; }
        /// <summary>
        /// Indirizzo, relativo alla base dell'immagine, dell'inizio della sezionde di codice.
        /// </summary>
        public uint BaseOfCode { get; }
        /// <summary>
        /// Indirizzo, relativo alla base dell'immagine, dell'inizio della sezione dei dati.
        /// </summary>
        public uint BaseOfData { get; }

        /// <summary>
        /// Inizializza una nuova struttura <see cref="OptionalHeaderStandard32"/>.
        /// </summary>
        /// <param name="Magic">Stato dell'immagine.</param>
        /// <param name="MajorLinkerVersion">Numero di versione maggiore del linker.</param>
        /// <param name="MinorLinkerVersion">Numero di versione minore del linker.</param>
        /// <param name="SizeOfCode">Dimensione della dimensione della sezione di codice (text).</param>
        /// <param name="SizeOfInitializedData">Dimensione della sezione dei dati inizializzati.</param>
        /// <param name="SizeOfUninitializedData">Dimensione della sezione dei dati non inizializzati.</param>
        /// <param name="AddressOfEntryPoint">Indirizzo del punto di entrata, rispetto alla base dell'immagine quando è caricata in memoria.</param>
        /// <param name="BaseOfCode">Indirizzo, relativo alla base dell'immagine, dell'inizio della sezionde di codice.</param>
        /// <param name="BaseOfData">Indirizzo, relativo alla base dell'immagine, dell'inizio della sezionde dei dati.</param>
        public OptionalHeaderStandard32(PEType Magic, byte MajorLinkerVersion, byte MinorLinkerVersion, uint SizeOfCode, uint SizeOfInitializedData, uint SizeOfUninitializedData, uint AddressOfEntryPoint, uint BaseOfCode, uint BaseOfData)
        {
            this.Magic = Magic;
            this.MajorLinkerVersion = MajorLinkerVersion;
            this.MinorLinkerVersion = MinorLinkerVersion;
            this.SizeOfCode = SizeOfCode;
            this.SizeOfInitializedData = SizeOfInitializedData;
            this.SizeOfUninitializedData = SizeOfUninitializedData;
            this.AddressOfEntryPoint = AddressOfEntryPoint;
            this.BaseOfCode = BaseOfCode;
            this.BaseOfData = BaseOfData;
        }
    }

    /// <summary>
    /// Intestazione opzionale standard (64 bit).
    /// </summary>
    public struct OptionalHeaderStandard64
    {
        /// <summary>
        /// Stato dell'immagine.
        /// </summary>
        public PEType Magic { get; }
        /// <summary>
        /// Numero di versione maggiore del linker.
        /// </summary>
        public byte MajorLinkerVersion { get; }
        /// <summary>
        /// Numero di versione minore del linker.
        /// </summary>
        public byte MinorLinkerVersion { get; }
        /// <summary>
        /// Dimensione della dimensione della sezione di codice (text).
        /// </summary>
        public uint SizeOfCode { get; }
        /// <summary>
        /// Dimensione della sezione dei dati inizializzati.
        /// </summary>
        public uint SizeOfInitializedData { get; }
        /// <summary>
        /// Dimensione della sezione dei dati non inizializzati.
        /// </summary>
        public uint SizeOfUninitializedData { get; }
        /// <summary>
        /// Indirizzo del punto di entrata, rispetto alla base dell'immagine quando è caricata in memoria.
        /// </summary>
        public uint AddressOfEntryPoint { get; }
        /// <summary>
        /// Indirizzo, relativo alla base dell'immagine, dell'inizio della sezionde di codice.
        /// </summary>
        public uint BaseOfCode { get; }

        /// <summary>
        /// Inizializza una nuova struttura <see cref="OptionalHeaderStandard64"/>.
        /// </summary>
        /// <param name="Magic">Stato dell'immagine.</param>
        /// <param name="MajorLinkerVersion">Numero di versione maggiore del linker.</param>
        /// <param name="MinorLinkerVersion">Numero di versione minore del linker.</param>
        /// <param name="SizeOfCode">Dimensione della dimensione della sezione di codice (text).</param>
        /// <param name="SizeOfInitializedData">Dimensione della sezione dei dati inizializzati.</param>
        /// <param name="SizeOfUninitializedData">Dimensione della sezione dei dati non inizializzati.</param>
        /// <param name="AddressOfEntryPoint">Indirizzo del punto di entrata, rispetto alla base dell'immagine quando è caricata in memoria.</param>
        /// <param name="BaseOfCode">Indirizzo, relativo alla base dell'immagine, dell'inizio della sezionde di codice.</param>
        public OptionalHeaderStandard64(PEType Magic, byte MajorLinkerVersion, byte MinorLinkerVersion, uint SizeOfCode, uint SizeOfInitializedData, uint SizeOfUninitializedData, uint AddressOfEntryPoint, uint BaseOfCode)
        {
            this.Magic = Magic;
            this.MajorLinkerVersion = MajorLinkerVersion;
            this.MinorLinkerVersion = MinorLinkerVersion;
            this.SizeOfCode = SizeOfCode;
            this.SizeOfInitializedData = SizeOfInitializedData;
            this.SizeOfUninitializedData = SizeOfUninitializedData;
            this.AddressOfEntryPoint = AddressOfEntryPoint;
            this.BaseOfCode = BaseOfCode;
        }
    }

    /// <summary>
    /// Tipo di PE.
    /// </summary>
    public enum PEType : ushort
    {
        PE32 = 0x10b,
        PE32Plus = 0x20b
    }

    /// <summary>
    /// Campi specifici per Windows dell'intestazione opzionale (32 bit).
    /// </summary>
    public struct OptionalHeaderWindowsSpecific32
    {
        /// <summary>
        /// Indirizzo preferito del primo byte dell'immagine quando caricata in memoria.
        /// </summary>
        public uint ImageBase { get; }
        /// <summary>
        /// Allineamento, in bytes, delle sezioni quando sono caricate in memoria.
        /// </summary>
        public uint SectionAlignment { get; }
        /// <summary>
        /// Allineamento, in bytes, delle sezioni nel file.
        /// </summary>
        public uint FileAlignment { get; }
        /// <summary>
        /// Numero di versione maggiore del sistema operativo.
        /// </summary>
        public ushort MajorOperatingSystemVersion { get; }
        /// <summary>
        /// Numero di versione minore del sistema operativo.
        /// </summary>
        public ushort MinorOperatingSystemVersion { get; }
        /// <summary>
        /// Numero di versione maggiore dell'immagine.
        /// </summary>
        public ushort MajorImageVersion { get; }
        /// <summary>
        /// Numero di versione minore dell'immagine.
        /// </summary>
        public ushort MinorImageVersion { get; }
        /// <summary>
        /// Numero di versione maggiore del sottosistema.
        /// </summary>
        public ushort MajorSubsystemVersion { get; }
        /// <summary>
        /// Numero di versione minore del sottosistema.
        /// </summary>
        public ushort MinorSubsystemVersion { get; }
        /// <summary>
        /// Riservato.
        /// </summary>
        public uint Win32VersionValue { get; }
        /// <summary>
        /// Dimensione dell'immagine, in bytes, quando è caricata in memoria.
        /// </summary>
        public uint SizeOfImage { get; }
        /// <summary>
        /// Dimensione combinata dello stub MS-DOS, delle intestazioni PE e delle intestazioni delle sezioni arrotondato a un multiplo di <see cref="FileAlignment"/>.
        /// </summary>
        public uint SizeOfHeaders { get; }
        /// <summary>
        /// Checksum dell'immagine.
        /// </summary>
        public uint Checksum { get; }
        /// <summary>
        /// Sottosistema necessario per eseguire l'immagine.
        /// </summary>
        public Subsystem Subsystem { get; }
        /// <summary>
        /// Caratteristiche DLL.
        /// </summary>
        public DllCharacteristics DllCharacteristics { get; }
        /// <summary>
        /// Dimensione dello stack da riservare.
        /// </summary>
        public uint SizeOfStackReserve { get; }
        /// <summary>
        /// Dimensione dello stack da mappare.
        /// </summary>
        public uint SizeOfStackCommit { get; }
        /// <summary>
        /// Dimensione dell'heap da riservare.
        /// </summary>
        public uint SizeOfHeapReserve { get; }
        /// <summary>
        /// Dimensoine dell'heap da mappare.
        /// </summary>
        public uint SizeOfHeapCommit { get; }
        /// <summary>
        /// Riservato.
        /// </summary>
        public uint LoaderFlags { get; }
        /// <summary>
        /// Numero di data directories presente nel resto dell'intestazione opzionale.
        /// </summary>
        public uint NumberOfRvaAndSizes { get; }

        /// <summary>
        /// Inizializza una nuova struttura <see cref="OptionalHeaderWindowsSpecific32"/>.
        /// </summary>
        /// <param name="ImageBase">Indirizzo preferito della base dell'immagine.</param>
        /// <param name="SectionAlignment">Allineamento, in bytes, delle sezioni quando sono caricate in memoria.</param>
        /// <param name="FileAlignment">Allineamento, in bytes, delle sezioni nel file.</param>
        /// <param name="MajorOperatingSystemVersion">Numero di versione maggiore del sistema operativo.</param>
        /// <param name="MinorOperatingSystemVersion">Numero di versione minore del sistema operativo.</param>
        /// <param name="MajorImageVersion">Numero di versione maggiore dell'immagine.</param>
        /// <param name="MinorImageVersion">Numero di versione minore dell'immagine.</param>
        /// <param name="MajorSubsystemVersion">Numero di versione maggiore del sottosistema.</param>
        /// <param name="MinorSubsystemVersion">Numero di versione minore del sottosistema.</param>
        /// <param name="Win32VersionValue">Riservato.</param>
        /// <param name="SizeOfImage">Dimensione dell'immagine.</param>
        /// <param name="SizeOfHeaders">Dimensione di tutte le intestazioni.</param>
        /// <param name="Checksum">Checksum.</param>
        /// <param name="Subsystem">Sottosistema necessario all'esecuzione dell'immagine.</param>
        /// <param name="DllCharacteristics">Caratteristiche DLL.</param>
        /// <param name="SizeOfStackReserve">Dimensione dello stack da riservare.</param>
        /// <param name="SizeOfStackCommit">Dimensione dello stack da mappare.</param>
        /// <param name="SizeOfHeapReserve">Dimensione dell'heap da riservare.</param>
        /// <param name="SizeOfHeapCommit">Dimensione dell'heap stack da mappare.</param>
        /// <param name="LoaderFlags">Riservato.</param>
        /// <param name="NumberOfRvaAndSizes">Numero di data directories presente nel resto dell'intestazione opzionale.</param>
        public OptionalHeaderWindowsSpecific32(uint ImageBase, uint SectionAlignment, uint FileAlignment, ushort MajorOperatingSystemVersion, ushort MinorOperatingSystemVersion, ushort MajorImageVersion, ushort MinorImageVersion, ushort MajorSubsystemVersion, ushort MinorSubsystemVersion, uint Win32VersionValue, uint SizeOfImage, uint SizeOfHeaders, uint Checksum, Subsystem Subsystem, DllCharacteristics DllCharacteristics, uint SizeOfStackReserve, uint SizeOfStackCommit, uint SizeOfHeapReserve, uint SizeOfHeapCommit, uint LoaderFlags, uint NumberOfRvaAndSizes)
        {
            this.ImageBase = ImageBase;
            this.SectionAlignment = SectionAlignment;
            this.FileAlignment = FileAlignment;
            this.MajorOperatingSystemVersion = MajorOperatingSystemVersion;
            this.MinorOperatingSystemVersion = MinorOperatingSystemVersion;
            this.MajorImageVersion = MajorImageVersion;
            this.MinorImageVersion = MinorImageVersion;
            this.MajorSubsystemVersion = MajorSubsystemVersion;
            this.MinorSubsystemVersion = MinorSubsystemVersion;
            this.Win32VersionValue = Win32VersionValue;
            this.SizeOfImage = SizeOfImage;
            this.SizeOfHeaders = SizeOfHeaders;
            this.Checksum = Checksum;
            this.Subsystem = Subsystem;
            this.DllCharacteristics = DllCharacteristics;
            this.SizeOfStackReserve = SizeOfStackReserve;
            this.SizeOfStackCommit = SizeOfStackCommit;
            this.SizeOfHeapReserve = SizeOfHeapReserve;
            this.SizeOfHeapCommit = SizeOfHeapCommit;
            this.LoaderFlags = LoaderFlags;
            this.NumberOfRvaAndSizes = NumberOfRvaAndSizes;
        }
    }
    
    /// <summary>
    /// Campi specifici per Windows dell'intestazione opzionale (64 bit).
    /// </summary>
    public struct OptionalHeaderWindowsSpecific64
    {
        /// <summary>
        /// Indirizzo preferito del primo byte dell'immagine quando caricata in memoria.
        /// </summary>
        public ulong ImageBase { get; }
        /// <summary>
        /// Allineamento, in bytes, delle sezioni quando sono caricate in memoria.
        /// </summary>
        public uint SectionAlignment { get; }
        /// <summary>
        /// Allineamento, in bytes, delle sezioni nel file.
        /// </summary>
        public uint FileAlignment { get; }
        /// <summary>
        /// Numero di versione maggiore del sistema operativo.
        /// </summary>
        public ushort MajorOperatingSystemVersion { get; }
        /// <summary>
        /// Numero di versione minore del sistema operativo.
        /// </summary>
        public ushort MinorOperatingSystemVersion { get; }
        /// <summary>
        /// Numero di versione maggiore dell'immagine.
        /// </summary>
        public ushort MajorImageVersion { get; }
        /// <summary>
        /// Numero di versione minore dell'immagine.
        /// </summary>
        public ushort MinorImageVersion { get; }
        /// <summary>
        /// Numero di versione maggiore del sottosistema.
        /// </summary>
        public ushort MajorSubsystemVersion { get; }
        /// <summary>
        /// Numero di versione minore del sottosistema.
        /// </summary>
        public ushort MinorSubsystemVersion { get; }
        /// <summary>
        /// Riservato.
        /// </summary>
        public uint Win32VersionValue { get; }
        /// <summary>
        /// Dimensione dell'immagine, in bytes, quando è caricata in memoria.
        /// </summary>
        public uint SizeOfImage { get; }
        /// <summary>
        /// Dimensione combinata dello stub MS-DOS, delle intestazioni PE e delle intestazioni delle sezioni arrotondato a un multiplo di <see cref="FileAlignment"/>.
        /// </summary>
        public uint SizeOfHeaders { get; }
        /// <summary>
        /// Checksum dell'immagine.
        /// </summary>
        public uint Checksum { get; }
        /// <summary>
        /// Sottosistema necessario per eseguire l'immagine.
        /// </summary>
        public Subsystem Subsystem { get; }
        /// <summary>
        /// Caratteristiche DLL.
        /// </summary>
        public DllCharacteristics DllCharacteristics { get; }
        /// <summary>
        /// Dimensione dello stack da riservare.
        /// </summary>
        public ulong SizeOfStackReserve { get; }
        /// <summary>
        /// Dimensione dello stack da mappare.
        /// </summary>
        public ulong SizeOfStackCommit { get; }
        /// <summary>
        /// Dimensione dell'heap da riservare.
        /// </summary>
        public ulong SizeOfHeapReserve { get; }
        /// <summary>
        /// Dimensoine dell'heap da mappare.
        /// </summary>
        public ulong SizeOfHeapCommit { get; }
        /// <summary>
        /// Riservato.
        /// </summary>
        public uint LoaderFlags { get; }
        /// <summary>
        /// Numero di data directories presente nel resto dell'intestazione opzionale.
        /// </summary>
        public uint NumberOfRvaAndSizes { get; }

        /// <summary>
        /// Inizializza una nuova struttura <see cref="OptionalHeaderWindowsSpecific64"/>.
        /// </summary>
        /// <param name="ImageBase">Indirizzo preferito della base dell'immagine.</param>
        /// <param name="SectionAlignment">Allineamento, in bytes, delle sezioni quando sono caricate in memoria.</param>
        /// <param name="FileAlignment">Allineamento, in bytes, delle sezioni nel file.</param>
        /// <param name="MajorOperatingSystemVersion">Numero di versione maggiore del sistema operativo.</param>
        /// <param name="MinorOperatingSystemVersion">Numero di versione minore del sistema operativo.</param>
        /// <param name="MajorImageVersion">Numero di versione maggiore dell'immagine.</param>
        /// <param name="MinorImageVersion">Numero di versione minore dell'immagine.</param>
        /// <param name="MajorSubsystemVersion">Numero di versione maggiore del sottosistema.</param>
        /// <param name="MinorSubsystemVersion">Numero di versione minore del sottosistema.</param>
        /// <param name="Win32VersionValue">Riservato.</param>
        /// <param name="SizeOfImage">Dimensione dell'immagine.</param>
        /// <param name="SizeOfHeaders">Dimensione di tutte le intestazioni.</param>
        /// <param name="Checksum">Checksum.</param>
        /// <param name="Subsystem">Sottosistema necessario all'esecuzione dell'immagine.</param>
        /// <param name="DllCharacteristics">Caratteristiche DLL.</param>
        /// <param name="SizeOfStackReserve">Dimensione dello stack da riservare.</param>
        /// <param name="SizeOfStackCommit">Dimensione dello stack da mappare.</param>
        /// <param name="SizeOfHeapReserve">Dimensione dell'heap da riservare.</param>
        /// <param name="SizeOfHeapCommit">Dimensione dell'heap stack da mappare.</param>
        /// <param name="LoaderFlags">Riservato.</param>
        /// <param name="NumberOfRvaAndSizes">Numero di data directories presente nel resto dell'intestazione opzionale.</param>
        public OptionalHeaderWindowsSpecific64(ulong ImageBase, uint SectionAlignment, uint FileAlignment, ushort MajorOperatingSystemVersion, ushort MinorOperatingSystemVersion, ushort MajorImageVersion, ushort MinorImageVersion, ushort MajorSubsystemVersion, ushort MinorSubsystemVersion, uint Win32VersionValue, uint SizeOfImage, uint SizeOfHeaders, uint Checksum, Subsystem Subsystem, DllCharacteristics DllCharacteristics, ulong SizeOfStackReserve, ulong SizeOfStackCommit, ulong SizeOfHeapReserve, ulong SizeOfHeapCommit, uint LoaderFlags, uint NumberOfRvaAndSizes)
        {
            this.ImageBase = ImageBase;
            this.SectionAlignment = SectionAlignment;
            this.FileAlignment = FileAlignment;
            this.MajorOperatingSystemVersion = MajorOperatingSystemVersion;
            this.MinorOperatingSystemVersion = MinorOperatingSystemVersion;
            this.MajorImageVersion = MajorImageVersion;
            this.MinorImageVersion = MinorImageVersion;
            this.MajorSubsystemVersion = MajorSubsystemVersion;
            this.MinorSubsystemVersion = MinorSubsystemVersion;
            this.Win32VersionValue = Win32VersionValue;
            this.SizeOfImage = SizeOfImage;
            this.SizeOfHeaders = SizeOfHeaders;
            this.Checksum = Checksum;
            this.Subsystem = Subsystem;
            this.DllCharacteristics = DllCharacteristics;
            this.SizeOfStackReserve = SizeOfStackReserve;
            this.SizeOfStackCommit = SizeOfStackCommit;
            this.SizeOfHeapReserve = SizeOfHeapReserve;
            this.SizeOfHeapCommit = SizeOfHeapCommit;
            this.LoaderFlags = LoaderFlags;
            this.NumberOfRvaAndSizes = NumberOfRvaAndSizes;
        }
    }
    #region Optional Header Windows Specific Enumerations
    /// <summary>
    /// Sottosistema.
    /// </summary>
    public enum Subsystem
    {
        /// <summary>
        /// Sottosistema sconosciuto.
        /// </summary>
        Unknown,
        /// <summary>
        /// Driver di dispositivo e processi nativi di Windows.
        /// </summary>
        Native,
        /// <summary>
        /// Sottosistema interfaccia grafica Windows.
        /// </summary>
        WindowsGUI,
        /// <summary>
        /// Sottosistema interfaccia a carattere Windows.
        /// </summary>
        WindowsCUI,
        /// <summary>
        /// Sottosistema interfaccia a carattere OS/2.
        /// </summary>
        OS2CUI = 5,
        /// <summary>
        /// Sottosistema interfaccia a carattere Posix.
        /// </summary>
        POSIXCUI = 7,
        /// <summary>
        /// Driver nativo Win9x.
        /// </summary>
        NativeWindows,
        /// <summary>
        /// Windows CE.
        /// </summary>
        WindowsCEGUI,
        /// <summary>
        /// Applicazione EFI.
        /// </summary>
        EFIApplication,
        /// <summary>
        /// Driver EFI con servizi di avvio.
        /// </summary>
        EFIBootServiceDriver,
        /// <summary>
        /// Driver EFI con servizi di runtime.
        /// </summary>
        EFIRuntimeDriver,
        /// <summary>
        /// EFI ROM.
        /// </summary>
        EFIRom,
        /// <summary>
        /// XBOX.
        /// </summary>
        XBOX,
        /// <summary>
        /// Applicazione di avvio di Windows.
        /// </summary>
        WindowsBootApplication = 16
    }

    /// <summary>
    /// Caratteristiche della DLL.
    /// </summary>
    [Flags]
    public enum DllCharacteristics
    {
        Reserved1,
        Reserved2,
        Reserved3 = 0x0004,
        Reserved4 = 0x0008,
        /// <summary>
        /// 
        /// </summary>
        HighEntropyVA = 0x0020,
        /// <summary>
        /// La DLL può essere riposizionata al momento del caricamento.
        /// </summary>
        DynamicBase = 0x0040,
        /// <summary>
        /// I controlli di integrità del codice sono obbligatori.
        /// </summary>
        ForceIntegrity = 0x0080,
        /// <summary>
        /// Compatibile con NX.
        /// </summary>
        NXCompat = 0x0100,
        /// <summary>
        /// L'immagine non deve essere isolata.
        /// </summary>
        NoIsolation = 0x0200,
        /// <summary>
        /// L'immagine non utilizza la gestione strutturata delle eccezioni.
        /// </summary>
        NoSEH = 0x0400,
        /// <summary>
        /// L'immagine non deve essere fissata.
        /// </summary>
        NoBind = 0x0800,
        /// <summary>
        /// L'immagine deve essere eseguita in un Appcontainer.
        /// </summary>
        Appcontainer = 0x1000,
        /// <summary>
        /// Driver WDM.
        /// </summary>
        WDMDriver = 0x2000,
        /// <summary>
        /// L'immagine supporta CFG.
        /// </summary>
        GuardCF = 0x4000,
        /// <summary>
        /// A conoscenza di Terminal Server.
        /// </summary>
        TerminalServerAware = 0x8000
    }
    #endregion
    /// <summary>
    /// Intestazione di una sezione.
    /// </summary>
    public struct SectionHeader
    {
        /// <summary>
        /// Nome della sezione.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Dimensione della sezione quando caricata in memoria.
        /// </summary>
        public uint VirtualSize { get; }
        /// <summary>
        /// Indirizzo virtuale della sezione relativo alla base dell'immagine.
        /// </summary>
        public uint VirtualAddress { get; }
        /// <summary>
        /// Dimensione dei dati inizializzati.
        /// </summary>
        public uint SizeOfRawData { get; }
        /// <summary>
        /// Offset alla prima pagine della sezione nel file.
        /// </summary>
        public uint PointerToRawData { get; }
        /// <summary>
        /// Offset all'inizio delle voci di rilocazione per la sezione.
        /// </summary>
        public uint PointerToRelocations { get; }
        /// <summary>
        /// Offset all'inizio delle voci di numeri linea per la sezione.
        /// </summary>
        public uint PointerToLineNumbers { get; }
        /// <summary>
        /// Numero di voci di relocazione per la sezione.
        /// </summary>
        public ushort NumberOfRelocations { get; }
        /// <summary>
        /// Numero di numeri linea.
        /// </summary>
        public ushort NumberOfLineNumbers { get; }
        /// <summary>
        /// Caratteristiche della sezione.
        /// </summary>
        public SectionCharacteristics Characteristics { get; }

        /// <summary>
        /// Inizializza una nuova struttura <see cref="SectionHeader"/>.
        /// </summary>
        /// <param name="Name">Nome della sezione.</param>
        /// <param name="VirtualSize">Dimensione della sezione quando caricata in memoria.</param>
        /// <param name="VirtualAddress">Indirizzo virtuale relativo alla base dell'immagine.</param>
        /// <param name="SizeOfRawData">Dimensione dei dati inizializzati.</param>
        /// <param name="PointerToRawData">Offset ai dati della sezione.</param>
        /// <param name="PointerToRelocations">Offset alle voci di relocazione.</param>
        /// <param name="PointerToLineNumbers">Offset ai numeri di linea.</param>
        /// <param name="NumberOfRelocations">Numero di voci di relocazione.</param>
        /// <param name="NumberOfLineNumbers">Numero di numeri di linea.</param>
        /// <param name="Characteristics">Caratteristiche della sezione.</param>
        public SectionHeader(string Name, uint VirtualSize, uint VirtualAddress, uint SizeOfRawData, uint PointerToRawData, uint PointerToRelocations, uint PointerToLineNumbers, ushort NumberOfRelocations, ushort NumberOfLineNumbers, SectionCharacteristics Characteristics)
        {
            this.Name = Name;
            this.VirtualSize = VirtualSize;
            this.VirtualAddress = VirtualAddress;
            this.SizeOfRawData = SizeOfRawData;
            this.PointerToRawData = PointerToRawData;
            this.PointerToRelocations = PointerToRelocations;
            this.PointerToLineNumbers = PointerToLineNumbers;
            this.NumberOfRelocations = NumberOfRelocations;
            this.NumberOfLineNumbers = NumberOfLineNumbers;
            this.Characteristics = Characteristics;
        }
    }

    /// <summary>
    /// Caratteristiche di una sezione.
    /// </summary>
    [Flags]
    public enum SectionCharacteristics : uint
    {
        Reserved1,
        Reserved2,
        Reserved3,
        Reserved4 = 0x00000004,
        Reserved5 = 0x00000010,
        /// <summary>
        /// La sezione contiene codice eseguibile.
        /// </summary>
        Code = 0x00000020,
        /// <summary>
        /// La sezione contiene dati inizializzati.
        /// </summary>
        InitializedData = 0x00000040,
        /// <summary>
        /// La sezione contiene dati non inizializzati.
        /// </summary>
        UninitializedData = 0x00000080,
        /// <summary>
        /// Riservato.
        /// </summary>
        LnkOther = 0x00000100,
        Reserved6 = 0x00000400,
        /// <summary>
        /// La sezione contiene dati riferiti dal puntatore globale.
        /// </summary>
        GlobalPointerReference = 0x00008000,
        /// <summary>
        /// Riservato.
        /// </summary>
        MemoryPurgeable = 0x00020000,
        /// <summary>
        /// Riservato.
        /// </summary>
        Memory16Bit = MemoryPurgeable,
        /// <summary>
        /// Riservato.
        /// </summary>
        MemoryLocked = 0x00040000,
        /// <summary>
        /// Riservato.
        /// </summary>
        MemoryPreload = 0x00080000,
        /// <summary>
        /// La sezione contiene relocazioni estese,
        /// </summary>
        ExtendendRelocations = 0x01000000,
        /// <summary>
        /// La sezione può essere eliminata se necessario.
        /// </summary>
        MemoryDiscardable = 0x02000000,
        /// <summary>
        /// La sezione non viene memorizzata nella cache.
        /// </summary>
        MemoryNotCached = 0x04000000,
        /// <summary>
        /// La sezione non può essere memorizzata nel file di paging.
        /// </summary>
        MemoryNotPaged = 0x08000000,
        /// <summary>
        /// La sezione può essere condivisa in memoria.
        /// </summary>
        MemoryShared = 0x10000000,
        /// <summary>
        /// La sezione può essere eseguita come codice.
        /// </summary>
        MemoryExecute = 0x20000000,
        /// <summary>
        /// La sezione può essere letta.
        /// </summary>
        MemoryRead = 0x40000000,
        /// <summary>
        /// Si può scrivere nella sezione.
        /// </summary>
        MemoryWrite = 0x80000000
    }
}