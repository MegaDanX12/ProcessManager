using ProcessManager.InfoClasses.PEFileStructures;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessManager
{
    /// <summary>
    /// Contiene dichiarazione di funzioni, enumerazioni e costanti delle API Win32 e NT.
    /// </summary>
    public static class NativeMethods
    {
        #region Fields
        /// <summary>
        /// Delegato per l'elaborazione delle informazioni di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="Parameter">Parametro fornito dall'applicazione.</param>
        /// <returns>true per continuare l'enumerazione, false per fermarla.</returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool EnumWindowsCallback(IntPtr WindowHandle, IntPtr Parameter);

        /// <summary>
        /// Delegato per l'elaborazione delle informazioni sulle proprietà di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra le cui proprietà sono in corso di enumerazione.</param>
        /// <param name="PropertyString">Componente stringa della proprietà.</param>
        /// <param name="DataHandle">Handle nativo ai dati della proprietà.</param>
        /// <param name="Parameter">Parametro fornito dall'applicazione.</param>
        /// <returns>true per continuare l'enumerazione, false per fermarla.</returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool EnumWindowPropsCallback(IntPtr WindowHandle, IntPtr PropertyString, IntPtr DataHandle, IntPtr Parameter);

        /// <summary>
        /// Delegato per l'elaborazione di un evento relativo a un servizio.
        /// </summary>
        /// <param name="ServiceNotifyStructure">Struttura <see cref="Win32Structures.SERVICE_NOTIFY_2"/> che contiene le informazioni sulla notifica ricevuta.</param>
        public delegate void ServiceStatusChangeCallback(IntPtr ServiceNotifyStructure);

        /// <summary>
        /// Delegato per l'elaborazione di eventi generati da un oggetto.
        /// </summary>
        /// <param name="WindowEventHookHandle">Handle nativo alla funzione di hook dell'evento.</param>
        /// <param name="Event">L'evento che si è verificato.</param>
        /// <param name="WindowHandle">Handle alla finestra che ha generato l'evento, <see cref="IntPtr.Zero"/> se nessuna finestra è associata all'evento.</param>
        /// <param name="idObject">Identificatore dell'oggetto associato all'evento.</param>
        /// <param name="idChild">Identifica se l'evento è stato generato da un oggetto o da un elemento figlio di un oggetto.</param>
        /// <param name="idEventThread"></param>
        /// <param name="EventTimeMilliseconds">Tempo, in millisecondi, da quando l'evento è stato generato.</param>
        /// <remarks>Se il valore del parametro idChild ha valore <see cref="Win32Enumerations.ObjectIDs.CHILDID_SELF"/>, l'evento è stato generato da un oggetto, altrimenti questo valore e l'ID dell'elemento che ha generato l'evento.</remarks>
        public delegate void WindowEventCallback(IntPtr WindowEventHookHandle, Win32Enumerations.WinEvents Event, IntPtr WindowHandle, Win32Enumerations.ObjectIDs idObject, Win32Enumerations.ObjectIDs idChild, uint idEventThread, uint EventTimeMilliseconds);

        /// <summary>
        /// Delegato per l'elaborazione di eventi generati da un oggetto.
        /// </summary>
        /// <param name="WindowEventHookHandle">Handle nativo alla funzione di hook dell'evento.</param>
        /// <param name="Event">L'evento che si è verificato.</param>
        /// <param name="WindowHandle">Handle alla finestra che ha generato l'evento, <see cref="IntPtr.Zero"/> se nessuna finestra è associata all'evento.</param>
        /// <param name="idObject">Identificatore dell'oggetto associato all'evento.</param>
        /// <param name="idChild">Identifica se l'evento è stato generato da un oggetto o da un elemento figlio di un oggetto.</param>
        /// <param name="idEventThread"></param>
        /// <param name="EventTimeMilliseconds">Tempo, in millisecondi, da quando l'evento è stato generato.</param>
        /// <remarks>Se il valore del parametro idChild ha valore <see cref="Win32Enumerations.ObjectIDs.CHILDID_SELF"/>, l'evento è stato generato da un oggetto, altrimenti questo valore e l'ID dell'elemento che ha generato l'evento.</remarks>
        public delegate void WindowEventCallbackForeground(IntPtr WindowEventHookHandle, Win32Enumerations.WinEvents Event, IntPtr WindowHandle, Win32Enumerations.ObjectIDs idObject, Win32Enumerations.ObjectIDs idChild, uint idEventThread, uint EventTimeMilliseconds);
        #endregion
        /// <summary>
        /// Costanti Win32.
        /// </summary>
        public static class Win32Constants
        {
            #region System Error Codes
            /// <summary>
            /// Operazione riuscita.
            /// </summary>
            public const uint NO_ERROR = 0;

            /// <summary>
            /// Operazione riuscita.
            /// </summary>
            public const uint ERROR_SUCCESS = 0;

            /// <summary>
            /// Accesso negato.
            /// </summary>
            public const uint ERROR_ACCESS_DENIED = 5;

            /// <summary>
            /// Una richiesta di lettura o scrittura della memoria di un processo è stata completata solo parzialmente.
            /// </summary>
            public const uint ERROR_PARTIAL_COPY = 299;

            /// <summary>
            /// Lunghezza del comando errata.
            /// </summary>
            public const uint ERROR_BAD_LENGTH = 24;

            /// <summary>
            /// Non esistono più file.
            /// </summary>
            public const uint ERROR_NO_MORE_FILES = 18;

            /// <summary>
            /// Parametro non corretto.
            /// </summary>
            public const uint ERROR_INVALID_PARAMETER = 87;

            /// <summary>
            /// Non tutti i permessi di cui la modidica è stata richiesta sono assegnati al chiamante.
            /// </summary>
            public const uint ERROR_NOT_ALL_ASSIGNED = 1300;

            /// <summary>
            /// Memoria insufficiente per completare l'operazione.
            /// </summary>
            public const uint ERROR_NOT_ENOUGH_MEMORY = 8;

            /// <summary>
            /// Il buffer è troppo piccolo, non tutte le voci sono state recuperate.
            /// </summary>
            public const uint ERROR_MORE_DATA = 234;

            /// <summary>
            /// SID non valido.
            /// </summary>
            public const uint ERROR_INVALID_SID = 1337;

            /// <summary>
            /// Il livello di chiamata a una system call non è corretto.
            /// </summary>
            public const uint ERROR_INVALID_LEVEL = 124;

            /// <summary>
            /// Server RPC non disponibile.
            /// </summary>
            public const uint RPC_S_SERVER_UNAVAILABLE = 1722;

            /// <summary>
            /// L'area dati passata a una system call è troppo piccola.
            /// </summary>
            public const uint ERROR_INSUFFICIENT_BUFFER = 122;

            /// <summary>
            /// Richiesta non supportata.
            /// </summary>
            public const uint ERROR_NOT_SUPPORTED = 50;

            /// <summary>
            /// Buffer utente fornito non valido per l'operazione richiesta.
            /// </summary>
            public const uint ERROR_INVALID_USER_BUFFER = 1784;

            /// <summary>
            /// Elemento non trovato.
            /// </summary>
            public const uint ERROR_NOT_FOUND = 1168;

            /// <summary>
            /// Il processo non può accedere al file perchè è in uso da un altro processo.
            /// </summary>
            public const uint ERROR_SHARING_VIOLATION = 32;

            /// <summary>
            /// Impossibile creare un file se esiste già.
            /// </summary>
            public const uint ERROR_ALREADY_EXISTS = 183;

            /// <summary>
            /// Il file esiste.
            /// </summary>
            public const uint ERROR_FILE_EXIST = 80;

            /// <summary>
            /// File specificato non trovato.
            /// </summary>
            public const uint ERROR_FILE_NOT_FOUND = 2;

            /// <summary>
            /// Operazione I/O asincrona in corso.
            /// </summary>
            public const uint ERROR_IO_PENDING = 997;

            /// <summary>
            /// Il volume di un file è stato alterato esternamente oppure il file aperto non è più valido.
            /// </summary>
            public const uint ERROR_FILE_INVALID = 1006;

            /// <summary>
            /// Handle non valido.
            /// </summary>
            public const uint ERROR_INVALID_HANDLE = 6;

            /// <summary>
            /// Percorso non trovato.
            /// </summary>
            public const uint ERROR_PATH_NOT_FOUND = 3;

            /// <summary>
            /// Si è verificato un errore nell'invio di un comando a un'applicazione.
            /// </summary>
            public const uint ERROR_DDE_FAIL = 1156;

            /// <summary>
            /// Una delle librerie necessarie per avviare l'applicazione non è stata trovata.
            /// </summary>
            public const uint ERROR_DLL_NOT_FOUND = 1157;

            /// <summary>
            /// Nessuna applicazione associata all'estensione.
            /// </summary>
            public const uint ERROR_NO_ASSOCIATION = 1155;

            /// <summary>
            /// Sono state richieste altre informazioni all'utente ma quest'ultimo ha annullato la richiesta.
            /// </summary>
            public const uint ERROR_CANCELLED = 1223;

            /// <summary>
            /// Il processo non ha un'identità di pacchetto.
            /// </summary>
            public const uint APPMODEL_ERROR_NO_PACKAGE = 15700;

            /// <summary>
            /// Il database specificato non esiste.
            /// </summary>
            public const uint ERROR_DATABASE_DOES_NOT_EXIST = 1065;

            /// <summary>
            /// Il servizio sta per essere eliminato.
            /// </summary>
            public const uint ERROR_SERVICE_MARKED_FOR_DELETE = 1072;

            /// <summary>
            /// Messaggio di errore non trovato.
            /// </summary>
            public const uint ERROR_MR_MID_NOT_FOUND = 317;

            /// <summary>
            /// Il client di notifica dei servizi non riesce a tenere il passo con lo stato corrente dei servizi nel sistema.
            /// </summary>
            public const uint ERROR_SERVICE_NOTIFY_CLIENT_LAGGING = 1294;

            /// <summary>
            /// Percorso di rete non trovato.
            /// </summary>
            public const uint ERROR_BAD_NETPATH = 53;

            /// <summary>
            /// Il formato del nome del computer specificato non è valido.
            /// </summary>
            public const uint ERROR_INVALID_COMPUTERNAME = 1210;

            /// <summary>
            /// Funzione non corretta.
            /// </summary>
            public const uint ERROR_INVALID_FUNCTION = 1;

            /// <summary>
            /// Lo spegnimento del sistema è in corso.
            /// </summary>
            public const uint ERROR_SHUTDOWN_IN_PROGRESS = 1115;

            /// <summary>
            /// Lo spegnimento del sistema è già stato programmato.
            /// </summary>
            public const uint ERROR_SHUTDOWN_IS_SCHEDULED = 1190;

            /// <summary>
            /// Lo spegnimento del sistema non può essere eseguito perchè altri utenti sono connessi al sistema.
            /// </summary>
            public const uint ERROR_SHUTDOWN_USERS_LOGGED_ON = 1191;

            /// <summary>
            /// Risorse insufficienti per completare l'operazione.
            /// </summary>
            public const uint ERROR_NO_SYSTEM_RESOURCES = 1450;

            /// <summary>
            /// Il database dell'autorità di sicurezza locale (LSA) è inconsistente.
            /// </summary>
            public const uint ERROR_INTERNAL_DB_ERROR = 1383;

            /// <summary>
            /// Il server del gestore della sicurezza degli account (SAM) o dell'autorità di sicurezza locale (LSA) non sono nello stato corretto per eseguire l'operazione di sicurezza.
            /// </summary>
            public const uint ERROR_INVALID_SERVER_STATE = 1352;

            /// <summary>
            /// Privilegio inesistente.
            /// </summary>
            public const uint ERROR_NO_SUCH_PRIVILEGE = 1313;

            /// <summary>
            /// Un dispositivo collegato al sistema non funziona correttamente.
            /// </summary>
            public const uint ERROR_GEN_FAILURE = 31;
            #endregion
            #region NTSTATUS Codes
            /// <summary>
            /// Operazione non riuscita.
            /// </summary>
            public const uint STATUS_UNSUCCESSFUL = 0xC0000001;

            /// <summary>
            /// Il buffer fornito è troppo piccolo, nessun dato è stato scritto.
            /// </summary>
            public const uint STATUS_BUFFER_TOO_SMALL = 0xC0000023;

            /// <summary>
            /// Il buffer fornito è troppo piccolo, solo parte dei dati è stata scritta.
            /// </summary>
            public const uint STATUS_BUFFER_OVERFLOW = 0x80000005;

            /// <summary>
            /// Tipo dell'oggetto errato.
            /// </summary>
            public const uint STATUS_OBJECT_TYPE_MISMATCH = 0xC0000024;

            /// <summary>
            /// L'operazione non è ancora completata.
            /// </summary>
            public const uint STATUS_PENDING = 0x00000103;

            /// <summary>
            /// Il timeout è scaduto.
            /// </summary>
            public const uint STATUS_TIMEOUT = 0x00000102;

            /// <summary>
            /// Operazione completata con successo.
            /// </summary>
            public const uint STATUS_SUCCESS = 0x00000000;

            /// <summary>
            /// Fornito parametro non valido.
            /// </summary>
            public const uint STATUS_INVALID_PARAMETER = 0xC000000D;

            /// <summary>
            /// Dimensione del buffer fornito troppo piccola.
            /// </summary>
            public const uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;

            /// <summary>
            /// ID del client non valido.
            /// </summary>
            public const uint STATUS_INVALID_CID = 0xC000000B;

            /// <summary>
            /// Accesso negato.
            /// </summary>
            public const uint STATUS_ACCESS_DENIED = 0xC0000022;

            /// <summary>
            /// Risorse di sistema insufficienti per l'esecuzione di una API di sistema.
            /// </summary>
            public const uint STATUS_INSUFFICIENT_RESOURCES = 0xC000009A;

            /// <summary>
            /// Il database dell'autorità di sicurezza locale (LSA) è inconsistente.
            /// </summary>
            public const uint STATUS_INTERNAL_DB_ERROR = 0xC0000158;

            /// <summary>
            /// Handle non valido.
            /// </summary>
            public const uint STATUS_INVALID_HANDLE = 0xC0000008;

            /// <summary>
            /// Il server SAM era in uno stato in cui non poteva eseguire l'operazione.
            /// </summary>
            public const uint STATUS_INVALID_SERVER_STATE = 0xC00000DC;

            /// <summary>
            /// Privilegio inesistente.
            /// </summary>
            public const uint STATUS_NO_SUCH_PRIVILEGE = 0xC0000060;

            /// <summary>
            /// Oggetto non trovato.
            /// </summary>
            public const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xC0000034;
            #endregion
            #region HRESULT Codes
            /// <summary>
            /// Successo.
            /// </summary>
            public const uint S_OK = 0x0;

            /// <summary>
            /// Successo.
            /// </summary>
            public const uint S_FALSE = 0x1;

            /// <summary>
            /// Parametro non valido.
            /// </summary>
            public const uint E_INVALIDARG = 0x80070057;

            /// <summary>
            /// Memoria insufficiente.
            /// </summary>
            public const uint E_OUTOFMEMORY = 0x8007000E;

            /// <summary>
            /// Condizione inaspettata.
            /// </summary>
            public const uint E_UNEXPECTED = 0x8000FFFF;

            /// <summary>
            /// Non è possibile cmabiare la modalità del thread una volta impostata.
            /// </summary>
            public const uint RPC_E_CHANGED_MODE = 0x80010106;
            #endregion
            #region Shell Execute Error Codes
            /// <summary>
            /// File non trovato.
            /// </summary>
            public const uint SE_ERR_FNF = 2;

            /// <summary>
            /// Percorso non trovato.
            /// </summary>
            public const uint SE_ERR_PNF = 3;

            /// <summary>
            /// Accesso negato.
            /// </summary>
            public const uint SE_ERR_ACCESSDENIED = 5;

            /// <summary>
            /// Memoria insufficiente.
            /// </summary>
            public const uint SE_ERR_OOM = 8;

            /// <summary>
            /// Libreria DLL non trovata.
            /// </summary>
            public const uint SE_ERR_DLLNOTFOUND = 32;

            /// <summary>
            /// Non è possibile condividere un file aperto.
            /// </summary>
            public const uint SE_ERR_SHARE = 26;

            /// <summary>
            /// Informazioni di associazione file non complete.
            /// </summary>
            public const uint SE_ERR_ASSOCINCOMPLETE = 27;

            /// <summary>
            /// Timeout operazione DDE.
            /// </summary>
            public const uint SE_ERR_DDETIMEOUT = 28;

            /// <summary>
            /// Oprerazione DDE fallita.
            /// </summary>
            public const uint SE_ERR_DDEFAIL = 29;

            /// <summary>
            /// Operazione DDE in corso.
            /// </summary>
            public const uint SE_ERR_DDEBUSY = 30;

            /// <summary>
            /// Associazione file non disponibile.
            /// </summary>
            public const uint SE_ERR_NOASSOC = 31;
            #endregion
            #region Other Constants
            /// <summary>
            /// Valore costante per indicare al sistema di allocare il buffer.
            /// </summary>
            public const int MAX_PREFERRED_LENGTH = -1;

            /// <summary>
            /// Indica che un processo è ancora in esecuzione.
            /// </summary>
            public const int STILL_ACTIVE = 259;

            /// <summary>
            /// Valore restituito dalla funzione <see cref="Win32ProcessFunctions.GetThreadPriority"/> in caso di errore.
            /// </summary>
            public const int THREAD_PRIORITY_ERROR_RETURN = int.MaxValue;

            /// <summary>
            /// Dimensione, in byte, della fonte di un token.
            /// </summary>
            public const int TOKEN_SOURCE_LENGTH = 8;

            /// <summary>
            /// Numero di hash in una SID_HASH_ENTRY.
            /// </summary>
            public const int SID_HASH_SIZE = 32;

            /// <summary>
            /// Versione di un attributo di sicurezza.
            /// </summary>
            public const ushort CLAIM_SECURITY_ATTRIBUTES_INFORMATION_VERSION_V1 = 1;

            /// <summary>
            /// Revisione attuale delle ACL.
            /// </summary>
            public const uint ACL_REVISION = 2;

            /// <summary>
            /// Lunghezza della stringa per un indirizzo IPv4.
            /// </summary>
            public const uint INET_ADDRSTRLEN = 22;

            /// <summary>
            /// Lunghezza della stringa per un indirizzo IPv6.
            /// </summary>
            public const uint INET6_ADDRSTRLEN = 65;

            /// <summary>
            /// Handle non valido.
            /// </summary>
            public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

            /// <summary>
            /// Puntatore file non valido.
            /// </summary>
            public const uint INVALID_SET_FILE_POINTER = uint.MaxValue;

            /// <summary>
            /// Dimensione file non valida.
            /// </summary>
            public const uint INVALID_FILE_SIZE = uint.MaxValue;

            /// <summary>
            /// Accesso massimo consentito a un handle.
            /// </summary>
            public const uint MAXIMUM_ALLOWED = 0x02000000;

            /// <summary>
            /// Attesa senza limite di tempo.
            /// </summary>
            public const uint INFINITE = uint.MaxValue;

            /// <summary>
            /// Dimensione massima di un percorso, in caratteri.
            /// </summary>
            public const uint MAX_PATH = 260;

            /// <summary>
            /// Carattere che permette di distinguere il nome di un gruppo di caricamento dal nome di un servizio.
            /// </summary>
            public const char SC_GROUP_INDENTIFIER = '+';

            /// <summary>
            /// Numero massimo di oggetti che la funzione <see cref="Win32OtherFunctions.WaitForMultipleObjects"/> può accettare.
            /// </summary>
            public const uint MAXIMUM_WAIT_OBJECTS = 64;

            /// <summary>
            /// Lunghezza, in caratteri, di un GUID di un profilo hardware.
            /// </summary>
            public const uint HW_PROFILE_GUIDLEN = 39;

            /// <summary>
            /// Lunghezza massima del nome di un profilo hardware.
            /// </summary>
            public const uint MAX_PROFILE_LEN = 80;

            /// <summary>
            /// Lunghezza massima del nome di un font.
            /// </summary>
            public const uint LF_FACESIZE = 32;

            /// <summary>
            /// Lunghezza massima del nome di una lingua.
            /// </summary>
            public const uint LOCALE_NAME_MAX_LENGTH = 85;

            /// <summary>
            /// Indica di restituire nomi neutrali per una lingua.
            /// </summary>
            public const uint LOCALE_ALLOW_NEUTRAL_NAMES = 0x08000000;
            #endregion
            #region Network Management Functions Costants
            /// <summary>
            /// Dati di utenti il cui account primario si trova in un altro dominio.
            /// </summary>
            /// <remarks>Questo tipo di account permette di accedere al dominio ma non ai domini per i quali questo dominio è fidato.<br>Il gestore utenti si riferisce a questo account come a un account locale.</br></remarks>
            public const uint FILTER_TEMP_DUPLICATE_ACCOUNT = 0x0001;

            /// <summary>
            /// Dati di utenti normali.
            /// </summary>
            /// <remarks>Questo tipo di account è associato con un utente tipico.</remarks>
            public const uint FILTER_NORMAL_ACCOUNT = 0x0002;

            /// <summary>
            /// Dati di utenti fidati interdominio.
            /// </summary>
            /// <remarks>Questo tipo di account è associato a un account fidato per i domini fidati di un altro dominio.</remarks>
            public const uint FILTER_INTERDOMAIN_TRUST_ACCOUNT = 0x0008;

            /// <summary>
            /// Dati di utenti fidati di server e workstation.
            /// </summary>
            /// <remarks>Questo tipo di account è associato con un account di un computer membro di un dominio.</remarks>
            public const uint FILTER_WORKSTATION_TRUST_ACCOUNT = 0x0010;

            /// <summary>
            /// Dati di utenti di un server.
            /// </summary>
            /// <remarks>Questo tipo di account è associato a un account di un controller di dominio di backup membro del dominio.</remarks>
            public const uint FILTER_SERVER_TRUST_ACCOUNT = 0x0020;


            public const uint LG_INCLUDE_INDIRECT = 0x0001;
            #endregion
            #region Network Management Functions Error Codes
            /// <summary>
            /// Codice di errore base per le funzioni di gestione rete.
            /// </summary>
            public const uint NERR_BASE = 2100;

            /// <summary>
            /// Il buffer è troppo piccolo.
            /// </summary>
            public const uint NERR_BufTooSmall = NERR_BASE + 23;

            /// <summary>
            /// Nome del computer non valido.
            /// </summary>
            public const uint NERR_InvalidComputer = NERR_BASE + 251;

            /// <summary>
            /// Impossibile trovare il controller di dominio.
            /// </summary>
            public const uint NERR_DCNotFound = NERR_BASE + 353;

            /// <summary>
            /// Nome utente non trovato.
            /// </summary>
            public const uint NERR_UserNotFound = NERR_BASE + 121;
            #endregion
            #region Privilege Constants
            /// <summary>
            /// Richiesto per eseguire il debug e alterare la memoria di un processo che non appartiene all'utente corrente.
            /// </summary>
            public const string SE_DEBUG_NAME = "SeDebugPrivilege";

            /// <summary>
            /// Richiesto per aumentare la quota assegnata a un processo.
            /// </summary>
            public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";

            /// <summary>
            /// Richiesto per iniziare lo spegnimento del sistema locale.
            /// </summary>
            public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

            /// <summary>
            /// Richiesto per aumentare la dimensione del working set di un processo.
            /// </summary>
            public const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";

            /// <summary>
            /// Richiesto per eseguire la profilatura di un singolo processo.
            /// </summary>
            public const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
            #endregion
            #region Service Trigger Type GUIDs
            /// <summary>
            /// Apertura di una named pipe.
            /// </summary>
            public static readonly Guid NAMED_PIPE_EVENT_GUID = new("1F81D131-3FAC-4537-9E0C-7E7B0C2F4B55");

            /// <summary>
            /// Ricevuta richiesta di risoluzione di un endpoint.
            /// </summary>
            public static readonly Guid RPC_INTERFACE_EVENT_GUID = new("BC90D167-9470-4139-A9BA-BE0BBBF5B74D");

            /// <summary>
            /// Unione a un dominio.
            /// </summary>
            public static readonly Guid DOMAIN_JOIN_GUID = new("1ce20aba-9851-4421-9430-1ddeb766e809");

            /// <summary>
            /// Rimozione da un dominio.
            /// </summary>
            public static readonly Guid DOMAIN_LEAVE_GUID = new("ddaf516e-58c2-4866-9574-c3b615d42ea1");

            /// <summary>
            /// Apertura di una porta.
            /// </summary>
            public static readonly Guid FIREWALL_PORT_OPEN_GUID = new("b7569e07-8421-4ee0-ad10-86915afdad09");

            /// <summary>
            /// Chiusura di una porta.
            /// </summary>
            public static readonly Guid FIREWALL_PORT_CLOSE_GUID = new("a144ed38-8e12-4de4-9d96-e64740b1a524");

            /// <summary>
            /// I criteri di sistema sono cambiati.
            /// </summary>
            public static readonly Guid MACHINE_POLICY_PRESENT_GUID = new("659FCAE6-5BDB-4DA9-B1FF-CA2A178D46E0");

            /// <summary>
            /// Il primo indirizzo IP sullo stack TCP/IP diventa disponibile.
            /// </summary>
            public static readonly Guid NETWORK_MANAGER_FIRST_IP_ADDRESS_ARRIVAL_GUID = new("4f27f2de-14e2-430b-a549-7cd48cbc8245");

            /// <summary>
            /// L'ultimo indirizzo IP sullo stack TCP/IP diventa non disponibile.
            /// </summary>
            public static readonly Guid NETWORK_MANAGER_LAST_IP_ADDRESS_REMOVAL_GUID = new("cc4ba62a-162e-4648-847a-b6bdf993e335");

            /// <summary>
            /// I criteri utente sono cambiati.
            /// </summary>
            public static readonly Guid USER_POLICY_PRESENT_GUID = new("54FB46C8-F089-464C-B1FD-59D1B62C3B50");
            #endregion
        }

        /// <summary>
        /// Enumerazioni Win32.
        /// </summary>
        public static class Win32Enumerations
        {
            #region Other Enumerations
            /// <summary>
            /// Porzione del sistema di cui eseguire lo snapshot.
            /// </summary>
            [Flags]
            public enum SnapshotSystemPortions : uint
            {
                /// <summary>
                /// Indica che l'handle dello snapshot deve essere ereditabile.
                /// </summary>
                TH32CS_INHERIT = 0x80000000,
                /// <summary>
                /// Include tutti i processi e i thread del sistema, inoltre include lo heap e i moduli di un processo.
                /// </summary>
                TH32CS_SNAPALL = TH32CS_INHERIT | TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32 | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD,
                /// <summary>
                /// Include lo heap di un processo.
                /// </summary>
                TH32CS_SNAPHEAPLIST = 0x00000001,
                /// <summary>
                /// Include i moduli di un processo.
                /// </summary>
                TH32CS_SNAPMODULE = 0x00000008,
                /// <summary>
                /// Include i moduli a 32 bit di un processo.
                /// </summary>
                TH32CS_SNAPMODULE32 = 0x00000010,
                /// <summary>
                /// Include tutti i processi del sistema.
                /// </summary>
                TH32CS_SNAPPROCESS = 0x00000002,
                /// <summary>
                /// Include tutti i thread del sistema.
                /// </summary>
                TH32CS_SNAPTHREAD = 0x00000004
            }

            /// <summary>
            /// Informazioni sul sistema.
            /// </summary>
            public enum SystemInformationClass
            {
                SystemBasicInformation,
                SystemPerformanceInformation = 2,
                SystemTimeOfDayInformation,
                SystemProcessInformation = 5,
                SystemProcessorPerformanceInformation = 8,
                SystemModuleInformation = 11,
                SystemHandleInformation = 16,
                SystemPagefileInformation = 18,
                SystemInterruptInformation = 23,
                SystemExceptionInformation = 33,
                SystemRegistryQuotaInformation = 37,
                SystemLookasideInformation = 45,
                SystemExtendedHandleInformation = 64,
                SystemMemoryListInformation = 80,
                SystemProcessIdInformation = 88,
                SystemCodeIntegrityInformation = 103,
                SystemQueryPerformanceCounterInformation = 124,
                SystemPolicyInformation = 134,
                SystemPagefileInformationEx = 144,
                SystemKernelVaShadowInformation = 196,
                SystemSpeculationControlInformation = 201
            }

            /// <summary>
            /// Informazioni su un oggetto.
            /// </summary>
            public enum ObjectInformationClass
            {
                ObjectBasicInformation,
                ObjectNameInformation,
                ObjectTypeInformation,
                ObjectTypesInformation,
                ObjectHandleFlagInformation,
                ObjectSessionInformation,
                ObjectSessionObjectInformation,
                MaxObjectInfoClass
            }
            #region SID Enumerations
            /// <summary>
            /// Tipo di SID.
            /// </summary>
            public enum SidNameUse : uint
            {
                /// <summary>
                /// SID di un utente.
                /// </summary>
                SideTypeUser,
                /// <summary>
                /// SID di un gruppo.
                /// </summary>
                SidTypeGroup,
                /// <summary>
                /// SID di un dominio.
                /// </summary>
                SidTypeDomain,
                /// <summary>
                /// SID di un alias.
                /// </summary>
                SidTypeAlias,
                /// <summary>
                /// SID di un gruppo noto.
                /// </summary>
                SidTypeWellKnownGroup,
                /// <summary>
                /// SID di un account eliminato.
                /// </summary>
                SidTypeDeletedAccount,
                /// <summary>
                /// SID non valido.
                /// </summary>
                SidTypeInvalid,
                /// <summary>
                /// SID di tipo sconosciuto.
                /// </summary>
                SidTypeUnknown,
                /// <summary>
                /// SID di un computer.
                /// </summary>
                SidTypeComputer,
                /// <summary>
                /// SID di un'etichetta di integrità obbligatoria.
                /// </summary>
                SidTypeLabel,

                SidTypeLogonSession
            }

            /// <summary>
            /// Attributi per SID di un gruppo.
            /// </summary>
            [Flags]
            public enum GroupsSIDAttributes : uint
            {
                /// <summary>
                /// Il SID è abilitato per controllo di accesso.
                /// </summary>
                /// <remarks>Un SID senza questo attributo viene ignorato durante un controllo di accesso a meno che non sia attivo l'attributo <see cref="SE_GROUP_USE_FOR_DENY_ONLY"/>.</remarks>
                SE_GROUP_ENABLED = 0x00000004,
                /// <summary>
                /// Il SID è abilitato di default.
                /// </summary>
                SE_GROUP_ENABLED_BY_DEFAULT = 0x00000002,
                /// <summary>
                /// Il SID è un SID di integrità obbligatoria.
                /// </summary>
                SE_GROUP_INTEGRITY = 0x00000020,
                /// <summary>
                /// Il SID di integrità obbligatoria è valutato durante i controlli di accesso.
                /// </summary>
                SE_GROUP_INTEGRITY_ENABLED = 0x00000040,
                /// <summary>
                /// Il SID è un SID di accesso che identifica la sessione di accesso associata con un token di accesso.
                /// </summary>
                SE_GROUP_LOGON_ID = 0xC0000000,
                /// <summary>
                /// Il SID non può avere l'attributo <see cref="SE_GROUP_ENABLED"/> disattivato da una chiamata alla funzione AdjustTokenGroups.
                /// </summary>
                SE_GROUP_MANDATORY = 0x00000001,
                /// <summary>
                /// Il SID identifica un account di gruppo per cui l'utente del token è il proprietario del gruppo, oppure il SID può essere assegnato come proprietario del token o degli oggetti.
                /// </summary>
                SE_GROUP_OWNER = 0x00000008,
                /// <summary>
                /// Il SID identifica un gruppo di dominio locale.
                /// </summary>
                SE_GROUP_RESOURCE = 0x20000000,
                /// <summary>
                /// Il SID è un SID di accesso negato, il sistema controlla soltanto per ACE per accesso negato applicabili.
                /// </summary>
                /// <remarks>Quando questo attributo è impostato <see cref="SE_GROUP_ENABLED"/> non è attivo e il SID non può essere riabilitato.</remarks>
                SE_GROUP_USE_FOR_DENY_ONLY = 0x00000010
            }

            /// <summary>
            /// Livello di integrità processo.
            /// </summary>
            public enum ProcessIntegrityLevel
            {
                /// <summary>
                /// Non fidato.
                /// </summary>
                SECURITY_MANDATORY_UNTRUSTED_RID = 0x00000000,
                /// <summary>
                /// Livello basso.
                /// </summary>
                SECURITY_MANDATORY_LOW_RID = 0x00001000,
                /// <summary>
                /// Livello medio.
                /// </summary>
                SECURITY_MANDATORY_MEDIUM_RID = 0x00002000,
                /// <summary>
                /// Livello medio-alto.
                /// </summary>
                SECURITY_MANDATORY_MEDIUM_PLUS = SECURITY_MANDATORY_MEDIUM_RID + 0x100,
                /// <summary>
                /// Livello alto.
                /// </summary>
                SECURITY_MANDATORY_HIGH_RID = 0x00003000,
                /// <summary>
                /// Livello di sistema.
                /// </summary>
                SECURITY_MANDATORY_SYSTEM_RID = 0x00004000,
                /// <summary>
                /// Processo protetto.
                /// </summary>
                SECURITY_MANDATORY_PROTECTED_PROCESS_RID = 0x00005000
            }

            /// <summary>
            /// SID noti.
            /// </summary>
            /// <remarks>Questa enumerazione contiene solo i valori usati in questa applicazione.<br/>
            /// Questa enumerazione è utilizzata dalla funzione <see cref="Win32OtherFunctions.CreateWellKnownSid(WellKnownSid, IntPtr, IntPtr, ref uint)"/>.</remarks>
            public enum WellKnownSid
            {
                /// <summary>
                /// Etichetta non fidata.
                /// </summary>
                WinUntrustedLabelSid = 65,
                /// <summary>
                /// Etichetta con affidabilità bassa.
                /// </summary>
                WinLowLabelSid,
                /// <summary>
                /// Etichetta con affidabilità media.
                /// </summary>
                WinMediumLabelSid,
                /// <summary>
                /// Etichetta con affidabilità alta.
                /// </summary>
                WinHighLabelSid,
                /// <summary>
                /// Etichetta di sistema.
                /// </summary>
                WinSystemLabelSid,
                /// <summary>
                /// Etichetta con affidabilità media +.
                /// </summary>
                WinMediumPlusLabelSid = 79
            }
            #endregion
            /// <summary>
            /// Attributi di un LUID relativo a un privilegio.
            /// </summary>
            [Flags]
            public enum PrivilegeLUIDAttributes : uint
            {
                SE_PRIVILEGE_DISABLED,
                SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001,
                SE_PRIVILEGE_ENABLED = 0x00000002,
                SE_PRIVILEGE_REMOVED = 0x00000004,
                SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000
            }

            /// <summary>
            /// Opzioni per la duplicazione di un handle.
            /// </summary>
            [Flags]
            public enum DuplicateHandleOptions
            {
                /// <summary>
                /// Chiude l'handle di origine.
                /// </summary>
                /// <remarks>Questo comportamento avviene in ogni caso.</remarks>
                DUPLICATE_CLOSE_SOURCE = 0x00000001,
                /// <summary>
                /// L'handle duplicato ha lo stesso tipo di accesso dell'handle di origine.
                /// </summary>
                DUPLICATE_SAME_ACCESS = 0x00000002
            }

            /// <summary>
            /// Informazioni su un oggetto utente.
            /// </summary>
            public enum UserObjectInfo
            {
                /// <summary>
                /// Caratteristiche di un handle.
                /// </summary>
                UOI_FLAGS = 1,
                /// <summary>
                /// Dimensione dell'heap del desktop, in KB.
                /// </summary>
                UOI_HEAPSIZE = 5,
                /// <summary>
                /// Indica se il desktop sta ricevendo input dall'utente.
                /// </summary>
                UOI_IO,
                /// <summary>
                /// Nome dell'oggetto.
                /// </summary>
                UOI_NAME = 2,
                /// <summary>
                /// Tipo dell'oggetto.
                /// </summary>
                UOI_TYPE,
                /// <summary>
                /// SID dell'utente associato all'oggetto.
                /// </summary>
                UOI_USER_SID
            }

            /// <summary>
            /// Caratteristiche di un oggetto utente.
            /// </summary>
            public enum UserObjectFlag : uint
            {
                /// <summary>
                /// La window station ha superfici visibili.
                /// </summary>
                WSF_VISIBLE = 0x0001,
                /// <summary>
                /// I processi in esecuzione in un altro account possono impostare hook nel processo corrente.
                /// </summary>
                DF_ALLOWOTHERACCOUNTHOOK = WSF_VISIBLE
            }

            /// <summary>
            /// Opzioni di creazione di un evento.
            /// </summary>
            public enum EventFlags
            {
                /// <summary>
                /// L'evento deve essere a reset manuale
                /// </summary>
                /// <remarks>Se questa opzione non viene specificata il sistema resetta automaticamente l'evento dopo che è stato permesso a un singolo thread di continuare.</remarks>
                CREATE_EVENT_MANUAL_RESET = 0x00000001,
                /// <summary>
                /// L'evento deve essere creato come segnalato.
                /// </summary>
                /// <remarks>Se questa opzione non viene specificata l'evento viene creato come non segnalato.</remarks>
                CREATE_EVENT_INITIAL_SET = 0x00000002
            }

            /// <summary>
            /// Attributi di carattere.
            /// </summary>
            [Flags]
            public enum CharacterAttributes : ushort
            {
                /// <summary>
                /// Blu.
                /// </summary>
                FOREGROUND_BLUE = 1,
                /// <summary>
                /// Verde.
                /// </summary>
                FOREGROUND_GREEN,
                /// <summary>
                /// Rosso.
                /// </summary>
                FOREGROUND_RED = 4,
                /// <summary>
                /// Il colore è intensificato.
                /// </summary>
                FOREGROUND_INTENSITY = 8,
                /// <summary>
                /// Blu (sottofondo).
                /// </summary>
                BACKGROUND_BLUE = 0x0010,
                /// <summary>
                /// Verde (sottofondo).
                /// </summary>
                BACKGROUND_GREEN = 0x0020,
                /// <summary>
                /// Rosso (sottofondo).
                /// </summary>
                BACKGROUND_RED = 0x0040,
                /// <summary>
                /// Il colore di sottofondo è intensificato.
                /// </summary>
                BACKGROUND_INTENSITY = 0x0080,
                /// <summary>
                /// Byte iniziale.
                /// </summary>
                COMMON_LVB_LEADING_BYTE = 0x0100,
                /// <summary>
                /// Byte finale.
                /// </summary>
                COMMON_LVB_TRAILING_BYTE = 0x0200,
                /// <summary>
                /// Orizzontale superiore.
                /// </summary>
                COMMON_LVB_GRID_HORIZONTAL = 0x0400,
                /// <summary>
                /// Verticale sinistro.
                /// </summary>
                COMMON_LVB_GRID_LVERTICAL = 0x0800,
                /// <summary>
                /// Verticale destro.
                /// </summary>
                COMMON_LVB_GRID_RVERTICAL = 0x1000,
                /// <summary>
                /// Gli attributi foreground background sono invertiti.
                /// </summary>
                COMMON_LVB_REVERSE_VIDEO = 0x4000,
                /// <summary>
                /// Trattino basso.
                /// </summary>
                COMMON_LVB_UNDERSCORE = 0x8000
            }

            /// <summary>
            /// Opzioni di accesso.
            /// </summary>
            [Flags]
            public enum LogonOptions : uint
            {
                /// <summary>
                /// Esegue l'accesso e carica il profilo utente nella chiave di registro HKEY_USERS.
                /// </summary>
                LOGON_WITH_PROFILE = 0x00000001,
                /// <summary>
                /// Esegue l'accesso ma usa le credenziali solo sulla rete.
                /// </summary>
                LOGON_NETCREDENTIALS_ONLY = 0x00000002
            }
            #region WinEvents Enumerations
            /// <summary>
            /// Eventi generati dal sistema operativo.
            /// </summary>
            public enum WinEvents : uint
            {
                #region Objects
                /// <summary>
                /// La proprietà KeyboardShortcut è cambiata.
                /// </summary>
                EVENT_OBJECT_ACCELERATORCHANGE = 0x8012,
                /// <summary>
                /// Una finestra è stata nascosta.
                /// </summary>
                EVENT_OBJECT_CLOAKED = 0x8017,
                /// <summary>
                /// Lo scorrimento di una finestra è terminato.
                /// </summary>
                /// <remarks>Il parametro hwnd della funzione di callback si riferisce alla finestra, il parametro idObject ha valore <see cref="ObjectIDs.OBJID_CLIENT"/>, il parametro idChild ha valore <see cref="ObjectIDs.CHILDID_SELF"/>.</remarks>
                EVENT_OBJECT_CONTENTSCROLLED = 0x8015,
                /// <summary>
                /// Un oggetto è stato creato.
                /// </summary>
                EVENT_OBJECT_CREATE = 0x8000,
                /// <summary>
                /// La proprietà DefaultAction è stata cambiata.
                /// </summary>
                EVENT_OBJECT_DEFACTIONCHANGE = 0x8011,
                /// <summary>
                /// La proprietà DefaultAction è stata cambiata.
                /// </summary>
                EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D,
                /// <summary>
                /// Un oggetto è stato eliminato.
                /// </summary>
                EVENT_OBJECT_DESTROY = 0x8001,
                /// <summary>
                /// L'utente ha iniziato il trascinamento di un elemento.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'oggetto trascinato.</remarks>
                EVENT_OBJECT_DRAGSTART = 0x8021,
                /// <summary>
                /// L'utente ha finito un operazione di trascinamento prima di aver lasciato l'elemento su un obbiettivo valido.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'oggetto trascinato.</remarks>
                EVENT_OBJECT_DRAGCANCEL = 0x8022,
                /// <summary>
                /// L'utente ha finito un operazione di trascinamento.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'oggetto trascinato.</remarks>
                EVENT_OBJECT_DRAGCOMPLETE = 0x8023,
                /// <summary>
                /// L'utente ha trascinato un elemento all'interno del bordo di un obbiettivo valido.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'obbiettivo.</remarks>
                EVENT_OBJECT_DRAGENTER = 0x8024,
                /// <summary>
                /// L'utente ha trascinato un elemento all'esterno del bordo di un obbiettivo valido.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'obbiettivo.</remarks>
                EVENT_OBJECT_DRAGLEAVE = 0x8025,
                /// <summary>
                /// L'utente ha rilasciato un elemento su un obbiettivo valido.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'obbiettivo.</remarks>
                EVENT_OBJECT_DRAGDROPPED = 0x8026,
                /// <summary>
                /// Un oggetto è diventato la destinazione dell'input da tastiera.
                /// </summary>
                /// <remarks>I parametri del callback identificano la finestra.</remarks>
                EVENT_OBJECT_FOCUS = 0x8005,
                /// <summary>
                /// La proprietà Help di un oggetto è cambiata.
                /// </summary>
                EVENT_OBJECT_HELPCHANGE = 0x8010,
                /// <summary>
                /// Un oggetto è stato nascosto.
                /// </summary>
                EVENT_OBJECT_HIDE = 0x8003,
                /// <summary>
                /// Una finestra che ospita oggetti accessibili ha cambiato gli oggetti ospitati.
                /// </summary>
                EVENT_OBJECT_HOSTEDOBJECTSINVALIDATED = 0x8020,
                /// <summary>
                /// Una finestra IME è stata nascosta.
                /// </summary>
                EVENT_OBJECT_IME_HIDE = 0x8028,
                /// <summary>
                /// Una finestra IME è diventata visibile.
                /// </summary>
                EVENT_OBJECT_IME_SHOW = 0x8027,
                /// <summary>
                /// Una finestra IME ha cambiato posizione o dimensione.
                /// </summary>
                EVENT_OBJECT_IME_CHANGE = 0x8029,
                /// <summary>
                /// Un oggetto è stato invocato.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'oggetto l'invocato.</remarks>
                EVENT_OBJECT_INVOKED = 0x8013,
                /// <summary>
                /// Un oggetto parte di una regione attiva è cambiato.
                /// </summary>
                EVENT_OBJECT_LIVEREGIONCHANGED = 0x8019,
                /// <summary>
                /// Un oggetto ha cambiato posizione, forma o dimensione.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'oggetto figlio modificato.</remarks>
                EVENT_OBJECT_LOCATIONCHANGE = 0x800B,
                /// <summary>
                /// La proprietà Name di un oggetto è cambiata.
                /// </summary>
                EVENT_OBJECT_NAMECHANGE = 0x800C,
                /// <summary>
                /// L'oggetto padre di un oggetto è cambiato.
                /// </summary>
                EVENT_OBJECT_PARENTCHANGE = 0x800F,
                /// <summary>
                /// Gli oggetti figli di un oggetto contenitore sono stati modificati (aggiunta, rimozione e riordinamento).
                /// </summary>
                EVENT_OBJECT_REORDER = 0x8004,
                /// <summary>
                /// La selezione all'interno di un oggetto contenitore è cambiata.
                /// </summary>
                /// <remarks>I parametri hwnd e idObject identificano il contenitore, il parametro idChild identifica l'oggetto selezionato, se l'oggetto è una finestra il parametro ha valore <see cref="ObjectIDs.OBJID_WINDOW"/>.</remarks>
                EVENT_OBJECT_SELECTION = 0x8006,
                /// <summary>
                /// Un oggetto figlio all'interno di un contenitore è stato aggiunto a una selezione esistente.
                /// </summary>
                /// <remarks>I parametri hwnd e idObject identificano il contenitore, il parametro idChild identifica l'oggetto aggiunto alla selezione.</remarks>
                EVENT_OBJECT_SELECTIONADD = 0x8007,
                /// <summary>
                /// Un oggetto figlio all'interno di un contenitore è stato rimosso dalla selezione.
                /// </summary>
                /// <remarks>I parametri hwnd e idObject identificano il contenitore, il parametro idChild identifica l'oggetto rimosso dalla selezione.</remarks>
                EVENT_OBJECT_SELECTIONREMOVE = 0x8008,
                /// <summary>
                /// Diversi cambiamenti della selezione sono avvenuti all'interno di un oggetto contenitore.
                /// </summary>
                /// <remarks>I parametri del callback identificano il contenitore in cui i cambiamenti sono avvenuti.</remarks>
                EVENT_OBJECT_SELECTIONWITHIN = 0x8009,
                /// <summary>
                /// Un oggetto nascosto è stato reso visibile.
                /// </summary>
                EVENT_OBJECT_SHOW = 0x8002,
                /// <summary>
                /// Lo stato di un oggetto è cambiato.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'oggetto il cui stato è cambiato.</remarks>
                EVENT_OBJECT_STATECHANGE = 0x800A,
                /// <summary>
                /// L'obbiettivo di una conversione all'interno di una composizione IME è cambiato.
                /// </summary>
                EVENT_OBJECT_TEXTEDIT_CONVERSIONTARGETCHANGED = 0x8030,
                /// <summary>
                /// La selezione del testo in un oggetto è cambiata.
                /// </summary>
                /// <remarks>I parametri del callback identificano l'oggetto che è contenuto nella selezione del testo aggiornata.</remarks>
                EVENT_OBJECT_TEXTSELECTIONCHANGED = 0x8014,
                /// <summary>
                /// Una finestra è stata nascosta.
                /// </summary>
                EVENT_OBJECT_UNCLOAKED = 0x8018,
                /// <summary>
                /// La proprietà Value di un oggetto è cambiato.
                /// </summary>
                EVENT_OBJECT_VALUECHANGE = 0x800E,
                #endregion
                #region System
                /// <summary>
                /// Un avviso è stato generato.
                /// </summary>
                EVENT_SYSTEM_ALERT = 0x0002,
                /// <summary>
                /// Un rettangolo di anteprima è stato visualizzato.
                /// </summary>
                EVENT_SYSTEM_ARRANGMENTPREVIEW = 0x8016,
                /// <summary>
                /// Una finestra ha rilasciato il mouse.
                /// </summary>
                EVENT_SYSTEM_CAPTUREEND = 0x0009,
                /// <summary>
                /// Una finestra ha catturato il mouse.
                /// </summary>
                EVENT_SYSTEM_CAPTURESTART = 0x0008,
                /// <summary>
                /// Una finestra è uscita dalla modalità di aiuto relativa al contesto.
                /// </summary>
                EVENT_SYSTEM_CONTEXTHELPEND = 0x000D,
                /// <summary>
                /// Una finestra è entrata nella modalità di aiuto relativa al contesto.
                /// </summary>
                EVENT_SYSTEM_CONTEXTHELPSTART = 0x000C,
                /// <summary>
                /// Il desktop attivo è cambiato.
                /// </summary>
                EVENT_SYSTEM_DESKTOPSWITCH = 0x0020,
                /// <summary>
                /// Una finestra di dialogo è stata chiusa.
                /// </summary>
                EVENT_SYSTEM_DIALOGEND = 0x0011,
                /// <summary>
                /// Una finestra di dialogo è stata visualizzata.
                /// </summary>
                EVENT_SYSTEM_DIALOGSTART = 0x0010,
                /// <summary>
                /// Un'applicazione sta per uscire dalla modalità trascina e rilascia.
                /// </summary>
                EVENT_SYSTEM_DRAGDROPEND = 0x000F,
                /// <summary>
                /// Un'applicazione sta per entrare nella modalità trascina e rilascia.
                /// </summary>
                EVENT_SYSTEM_DRAGDROPSTART = 0x000E,
                /// <summary>
                /// La finestra in primo piano è cambiata.
                /// </summary>
                /// <remarks>Il parametro hwnd rappresenta l'handle alla finestra in primo piano, il parametro idObject ha valore <see cref="ObjectIDs.OBJID_WINDOW"/> e il parametro idChild ha valore <see cref="ObjectIDs.CHILDID_SELF"/>.</remarks>
                EVENT_SYSTEM_FOREGROUND = 0x0003,
                /// <summary>
                /// Un menu popup è stato chiuso.
                /// </summary>
                EVENT_SYSTEM_MENUPOPUPEND = 0x0007,
                /// <summary>
                /// Un menu popup è stato visualizzato.
                /// </summary>
                EVENT_SYSTEM_MENUPOPUPSTART = 0x0006,
                /// <summary>
                /// Un menu della barra dei menu è stato chiuso.
                /// </summary>
                /// <remarks>I parametri del callback si riferisce al controllo che contiene la barra dei menu o al controllo che attiva il menù di contesto, il parametro hwnd rappresenta l'handle alla finestra collegata all'evento, il parametro idObject ha valore <see cref="ObjectIDs.OBJID_MENU"/> oppure <see cref="ObjectIDs.OBJID_SYSMENU"/> per un menu, ha valore <see cref="ObjectIDs.OBJID_WINDOW"/> per un menu popup, il parametro idChild ha valore <see cref="ObjectIDs.CHILDID_SELF"/>.</remarks>
                EVENT_SYSTEM_MENUEND = 0x0005,
                /// <summary>
                /// Una opzione di menu della barra dei menu è stata selezionata.
                /// </summary>
                /// <remarks>I parametri del callback si riferisce al controllo che contiene la barra dei menu o al controllo che attiva il menù di contesto, il parametro hwnd rappresenta l'handle alla finestra collegata all'evento, il parametro idObject ha valore <see cref="ObjectIDs.OBJID_MENU"/> oppure <see cref="ObjectIDs.OBJID_SYSMENU"/> per un menu, ha valore <see cref="ObjectIDs.OBJID_WINDOW"/> per un menu popup, il parametro idChild ha valore <see cref="ObjectIDs.CHILDID_SELF"/>.</remarks>
                EVENT_SYSTEM_MENUSTART = 0x0004,
                /// <summary>
                /// Una finestra sta per essere ripristinata.
                /// </summary>
                EVENT_SYSTEM_MINIMIZEEND = 0x0017,
                /// <summary>
                /// Una finestra sta per essere minimizzata.
                /// </summary>
                EVENT_SYSTEM_MINIMIZESTART = 0x0016,
                /// <summary>
                /// Il movimento o il ridimensionamento di una finestra è terminato.
                /// </summary>
                EVENT_SYSTEM_MOVESIZEEND = 0x000B,
                /// <summary>
                /// Il movimento o il ridimensionamento di una finestra è in corso.
                /// </summary>
                EVENT_SYSTEM_MOVESIZESTART = 0x000A,
                /// <summary>
                /// Lo scorrimento di una barra di scorrimento è terminato.
                /// </summary>
                /// <remarks>Il parametro idObject del callback ha valore <see cref="ObjectIDs.OBJID_HSCROLL"/> per barre di scorrimento orizzontali, ha valore <see cref="ObjectIDs.OBJID_VSCROLL"/> per barre di scorrimento verticali.</remarks>
                EVENT_SYSTEM_SCROLLINGEND = 0x0013,
                /// <summary>
                /// Lo scorrimento di una barra di scorrimento è iniziato.
                /// </summary>
                /// <remarks>Il parametro idObject del callback ha valore <see cref="ObjectIDs.OBJID_HSCROLL"/> per barre di scorrimento orizzontali, ha valore <see cref="ObjectIDs.OBJID_VSCROLL"/> per barre di scorrimento verticali.</remarks>
                EVENT_SYSTEM_SCROLLINGSTART = 0x0012,
                /// <summary>
                /// Un suono è stato emesso.
                /// </summary>
                /// <remarks>Il parametro idObject del callback ha valore <see cref="ObjectIDs.OBJID_SOUND"/>.</remarks>
                EVENT_SYSTEM_SOUND = 0x0001,
                /// <summary>
                /// L'utente ha rilasciato la combinazione di tasti ALT+TAB.
                /// </summary>
                /// <remarks>Il parametro hwnd rappresenta l'handle alla finestra che l'utente ha attivato, se è in esecuzione una sola applicazione al momento della pressione della combinazione di tasti, il sistema invia questo evento senza un corrispondente <see cref="EVENT_SYSTEM_SWITCHSTART"/>.</remarks>
                EVENT_SYSTEM_SWITCHEND = 0x0015,
                /// <summary>
                /// L'utente ha premuto la combinazione di tasti ALT+TAB.
                /// </summary>
                /// <remarks>Il parametro hwnd rappresenta l'handle alla finestra che l'utente sta attivando, se è in esecuzione una sola applicazione al momento della pressione della combinazione di tasti, il sistema invia solamente l'evento <see cref="EVENT_SYSTEM_SWITCHEND"/> senza un corrispondente <see cref="EVENT_SYSTEM_SWITCHSTART"/>.</remarks>
                EVENT_SYSTEM_SWITCHSTART = 0x0014
                #endregion
            }

            /// <summary>
            /// Identificativi degli oggetti.
            /// </summary>
            public enum ObjectIDs : uint
            {
                CHILDID_SELF,

                INDEXID_OBJECT = CHILDID_SELF,

                INDEXID_CONTAINER = CHILDID_SELF,
                /// <summary>
                /// Una finestra.
                /// </summary>
                OBJID_WINDOW = CHILDID_SELF,
                /// <summary>
                /// Il menù di sistema della finestra.
                /// </summary>
                OBJID_SYSMENU = 0xFFFFFFFF,
                /// <summary>
                /// La barra del titolo della finestra.
                /// </summary>
                OBJID_TITLEBAR = 0xFFFFFFFE,
                /// <summary>
                /// La barra dei menù della finestra.
                /// </summary>
                OBJID_MENU = 0xFFFFFFFD,
                /// <summary>
                /// L'area client di una finestra.
                /// </summary>
                OBJID_CLIENT = 0xFFFFFFFC,
                /// <summary>
                /// La barra di scorrimento verticale di una finestra.
                /// </summary>
                OBJID_VSCROLL = 0xFFFFFFFB,
                /// <summary>
                /// La barra di scorrimento orizzontale di una finestra.
                /// </summary>
                OBJID_HSCROLL = 0xFFFFFFFA,
                /// <summary>
                /// 
                /// </summary>
                OBJID_SIZEGRIP = 0xFFFFFFF9,
                /// <summary>
                /// La barra di inserimento del testo in una finestra.
                /// </summary>
                OBJID_CARET = 0xFFFFFFF8,
                /// <summary>
                /// Il puntatore del mouse.
                /// </summary>
                OBJID_CURSOR = 0xFFFFFFF7,
                /// <summary>
                /// Un avviso associato a una finestra o a un'applicazione.
                /// </summary>
                /// <remarks>Solamente le finestre di dialogo del sistema inviano eventi con questo identificativo.</remarks>
                OBJID_ALERT = 0xFFFFFFF6,
                /// <summary>
                /// Un oggetto suono.
                /// </summary>
                /// <remarks>Questi tipi di oggetti sono figli dell'applicazione che ha emesso il suono, questi oggetti non hanno associate posizioni sullo schermo o figli.</remarks>
                OBJID_SOUND = 0xFFFFFFF5,
                /// <summary>
                /// Identificativo usato internamente da Oleacc.dll.
                /// </summary>
                OBJID_QUERYCLASSNAMEIDX = 0xFFFFFFF4,
                /// <summary>
                /// Interfaccia COM.
                /// </summary>
                /// <remarks>Le applicazioni di terze parti possono restituire una qualunque interfaccia COM in risposta a questo identificativo.</remarks>
                OBJID_NATIVEOM = 0xFFFFFFF0
            }

            /// <summary>
            /// Opzioni relative all'hooking di un evento.
            /// </summary>
            [Flags]
            public enum EventHookingFlags : uint
            {
                /// <summary>
                /// La funzione di callback non è mappata nello spazio di indirizzamento del processo che genera l'evento.
                /// </summary>
                WINEVENT_OUTOFCONTEXT = 0x0000,
                /// <summary>
                /// Ignora gli eventi generati dal thread che ha registrato l'hook.
                /// </summary>
                WINEVENT_SKIPOWNTHREAD = 0x0001,
                /// <summary>
                /// Ignora gli eventi generati dal processo.
                /// </summary>
                WINEVENT_SKIPOWNPROCESS = 0x0002,
                /// <summary>
                /// La DLL che contiene la funzione di callback è mappata nello spazio di indirizzamento del processo che genera l'evento.
                /// </summary>
                WINEVENT_INCONTEXT = 0x0004
            }
            #endregion
            /// <summary>
            /// Filtri per le notifiche di cambiamenti al registro di sistema.
            /// </summary>
            [Flags]
            public enum RegistryNotificationFilters : uint
            {
                /// <summary>
                /// Una sottochiave è stata aggiunta o eliminata.
                /// </summary>
                REG_NOTIFY_CHANGE_NAME = 0x00000001,
                /// <summary>
                /// Gli attributi di una chiave sono cambiati.
                /// </summary>
                REG_NOTIFY_CHANGE_ATTRIBUTES = 0x00000002,
                /// <summary>
                /// Il valore di una chiave è cambiato.
                /// </summary>
                REG_NOTIFY_CHANGE_LAST_SET = 0x00000004,
                /// <summary>
                /// Il descrittore di sicurezza è cambiato.
                /// </summary>
                REG_NOTIFY_CHANGE_SECURITY = 0x00000008,
                /// <summary>
                /// Il tempo della sottoscrizione alle notifiche non deve essere legato al thread che ha chiamato la funzione.
                /// </summary>
                REG_NOTIFY_THREAD_AGNOSTIC = 0x10000000
            }

            /// <summary>
            /// Famiglia di indirizzi.
            /// </summary>
            public enum AddressFamily : ushort
            {
                /// <summary>
                /// Non specificato.
                /// </summary>
                AF_UNSPEC,
                /// <summary>
                /// Locale all'host.
                /// </summary>
                AF_UNIX,
                /// <summary>
                /// Internetwork.
                /// </summary>
                AF_INET,
                /// <summary>
                /// Indirizzi Arpanet.
                /// </summary>
                AF_IMPLINK,
                /// <summary>
                /// Protocolli PUP.
                /// </summary>
                AF_PUP,
                /// <summary>
                /// Protocollo CHAOS del MIT.
                /// </summary>
                AF_CHAOS,
                /// <summary>
                /// IPX e SPX.
                /// </summary>
                AF_IPX,
                /// <summary>
                /// Protocolli XEROX NS.
                /// </summary>
                AF_NS = AF_IPX,
                /// <summary>
                /// Protocolli ISO.
                /// </summary>
                AF_ISO,
                /// <summary>
                /// Protocolli ISO.
                /// </summary>
                AF_OSI = AF_ISO,
                /// <summary>
                /// European Computer Manufacturers.
                /// </summary>
                AF_ECMA,
                /// <summary>
                /// Protocolli datakit.
                /// </summary>
                AF_DATAKIT,
                /// <summary>
                /// Protocolli CCITT.
                /// </summary>
                AF_CCITT,
                /// <summary>
                /// IBM SNA.
                /// </summary>
                AF_SNA,
                /// <summary>
                /// DECnet.
                /// </summary>
                AF_DECnet,
                /// <summary>
                /// Interfaccia di collegamento dati diretta.
                /// </summary>
                AF_DLI,
                /// <summary>
                /// LAT.
                /// </summary>
                AF_LAT,
                /// <summary>
                /// NSC Hyperchannel.
                /// </summary>
                AF_HYLINK,
                /// <summary>
                /// AppleTalk.
                /// </summary>
                AF_APPLETALK,
                /// <summary>
                /// Indirizzi in stile NetBios.
                /// </summary>
                AF_NETBIOS,
                /// <summary>
                /// VoiceView.
                /// </summary>
                AF_VOICEVIEW,
                /// <summary>
                /// FireFox.
                /// </summary>
                AF_FIREFOX,
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                AF_UNKNOWN1,
                /// <summary>
                /// Banyan.
                /// </summary>
                AF_BAN,
                /// <summary>
                /// Servizi ATM nativi.
                /// </summary>
                AF_ATM,
                /// <summary>
                /// IPv6.
                /// </summary>
                AF_INET6,
                /// <summary>
                /// Microsoft Wolfpack.
                /// </summary>
                AF_CLUSTER,
                /// <summary>
                /// IEEE 1284.4 WG AF.
                /// </summary>
                AF_12844,
                /// <summary>
                /// IrDA.
                /// </summary>
                AF_IRDA,
                /// <summary>
                /// Progettisti di rete & gateway.
                /// </summary>
                AF_NETDES = 28,

                AF_TCNPROCESS,

                AF_TCNMESSAGE,

                AF_ICLFXBM,
                /// <summary>
                /// Protocolli Bluetooth RFCOMM/L2CAP.
                /// </summary>
                AF_BTH,

                AF_LINK,

                AF_HYPERV,
                /// <summary>
                /// Valore massimo dell'enumerazione.
                /// </summary>
                AF_MAX
            }
            #region Memory Management Enumerations
            /// <summary>
            /// Comandi per la pulizia della memoria.
            /// </summary>
            public enum MemoryListCommand : uint
            {
                MemoryCaptureAccessedBits,
                MemoryCaptureAndResetAccessedBits,
                MemoryEmptyWorkingSets,
                MemoryFlushModifiedList,
                MemoryPurgeStandbyList,
                MemoryPurgeLowPriorityStandbyList
            }

            /// <summary>
            /// Tipo di notifica dello stato della memoria.
            /// </summary>
            public enum MemoryResourceNotificationType : uint
            {
                /// <summary>
                /// La memoria fisica disponibile è bassa.
                /// </summary>
                LowMemoryResourceNotification,
                /// <summary>
                /// La memoria fisica disponibile è alta.
                /// </summary>
                HighMemoryResourceNotification
            }
            #endregion
            #endregion
            #region Process Enumerations
            /// <summary>
            /// Valori di priorità per un processo.
            /// </summary>
            public enum ProcessPriorityValue : uint
            {
                /// <summary>
                /// Sopra il normale.
                /// </summary>
                ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000,
                /// <summary>
                /// Sotto il normale.
                /// </summary>
                BELOW_NORMAL_PRIORITY_CLASS = 0x00004000,
                /// <summary>
                /// Alta priorità.
                /// </summary>
                HIGH_PRIORITY_CLASS = 0x00000080,
                /// <summary>
                /// Priorità inattivo.
                /// </summary>
                IDLE_PRIORITY_CLASS = 0x00000040,
                /// <summary>
                /// Priorità normale.
                /// </summary>
                NORMAL_PRIORITY_CLASS = 0x00000020,
                /// <summary>
                /// Priorità tempo reale.
                /// </summary>
                REALTIME_PRIORITY_CLASS = 0x00000100
            }

            /// <summary>
            /// Politica di mitigazione di un processo.
            /// </summary>
            public enum ProcessMitigationPolicy : uint
            {
                /// <summary>
                /// Politica Data Execution Prevention (DEP).
                /// </summary>
                ProcessDEPPolicy,
                /// <summary>
                /// Politica Address Space Layout Randomization (ASLR).
                /// </summary>
                ProcessASLRPolicy,
                /// <summary>
                /// Politica codice dinamico, se attiva il processo non può generare codice dinamico o modificare codice eseguibile esistente.
                /// </summary>
                ProcessDynamicCodePolicy,
                /// <summary>
                /// La manipolazione di un handle non valido causerà un errore fatale.
                /// </summary>
                ProcessStrictHandleCheckPolicy,
                /// <summary>
                /// Non permette al processo di usare funzioni GDI/NTUser al livello più basso.
                /// </summary>
                ProcessSystemCallDisablePolicy,
                /// <summary>
                /// Maschera di bit validi per tutte le opzioni di mitigazione del sistema.
                /// </summary>
                ProcessMitigationOptionsMask,
                /// <summary>
                /// Impedisce l'attivazione di certe estensioni di terze parti.
                /// </summary>
                ProcessExtensionPointDisablePolicy,
                /// <summary>
                /// Politica Control FLow Guard (CFG).
                /// </summary>
                ProcessControlFlowGuardPolicy,
                /// <summary>
                /// Impedisce il caricamento di immagini che non sono firmate da Microsoft, dal Windows Store e da Windows Hardware Quality Labs (WHQL).
                /// </summary>
                ProcessSignaturePolicy,
                /// <summary>
                /// Politica di caricamento dei font, se attiva solo i font di sistema possono essere caricati.
                /// </summary>
                ProcessFontDisablePolicy,
                /// <summary>
                /// Politica di caricamento immagine, se attiva immagini proveniente da certi percorsi non possono essere caricati.
                /// </summary>
                ProcessImageLoadPolicy,
                /// <summary>
                /// 
                /// </summary>
                ProcessSystemCallFilterPolicy,
                /// <summary>
                /// 
                /// </summary>
                ProcessPayloadRestrictionPolicy,
                /// <summary>
                /// 
                /// </summary>
                ProcessChildProcessPolicy,
                /// <summary>
                /// Politica relativa all'isolamento dei side channels (da Windows 10, versione 1809).
                /// </summary>
                ProcessSideChannelIsolationPolicy,
                /// <summary>
                /// Politica relativa alla protezione dello stack gestita dall'hardware in modalità utente (da Windows 10, versione 2004).
                /// </summary>
                ProcessUserShadowStackPolicy,
                /// <summary>
                /// Valore massimo.
                /// </summary>
                MaxProcessMitigationPolicy
            }

            /// <summary>
            /// Protezione di una pagina del working set di un processo.
            /// </summary>
            public enum ProcessWorkingSetPageProtection : uint
            {
                NotAccessed1,
                ReadOnly,
                Executable,
                ReadOnlyExecutable,
                ReadWrite,
                CopyOnWrite,
                ExecutableReadWrite,
                ExecutableCopyOnWrite,
                NotAccessed2,
                NonCacheableReadOnly,
                NonCacheableExecutable,
                NonCacheableExecutableReadOnly,
                NonCacheableReadWrite,
                NonCacheableCopyOnWrite,
                NonCacheableExecutableReadWrite,
                NonCacheableExecutableCopyOnWrite,
                NotAccessed3,
                GuardReadOnly,
                GuardExecutable,
                GuardExecutableReadOnly,
                GuardReadWrite,
                GuardCopyOnWrite,
                GuardExecutableReadWrite,
                GuardExecutableCopyOnWrite,
                NotAccessed4,
                NonCacheableGuardReadOnly,
                NonCacheableGuardExecutable,
                NonCacheableGuardExecutableReadOnly,
                NonCacheableGuardReadWrite,
                NonCacheableGuardCopyOnWrite,
                NonCacheableGuardExecutableReadWrite,
                NonCacheableGuardExecutableCopyOnWrite
            }

            /// <summary>
            /// Tipi di oggetti GUI.
            /// </summary>
            public enum ProcessGUIObjectType : uint
            {
                /// <summary>
                /// Conteggio di oggetti GDI.
                /// </summary>
                GR_GDIOBJECTS,
                /// <summary>
                /// Conteggio massimo di oggetti GDI.
                /// </summary>
                GR_GDIOBJECTS_PEAK = 2,
                /// <summary>
                /// Conteggio di oggetti USER.
                /// </summary>
                GR_USEROBJECTS = 1,
                /// <summary>
                /// Conteggio massimo di oggetti USER.
                /// </summary>
                GR_USEROBJECTS_PEAK = 4
            }

            /// <summary>
            /// Membri della struttura <see cref="Win32Structures.STARTUPINFO"/> usati.
            /// </summary>
            [Flags]
            public enum StartupInfoStructureAvailableData : uint
            {
                /// <summary>
                /// Il cursore è in modalità feedback per due secondi dopo la creazione del processo.
                /// </summary>
                /// <remarks>Se durante i due secondi il processo esegue la prima chiamata GUI, il sistema dà altri 5 secondi al processo, se durante questi 5 secondi il processo visualizza una finestra, il sistema dà altri 5 secondi al processo per finire il disegno della finestra.<br/><br/>
                /// La modalità feedback del cursore viene disattivata dopo la prima chiamata a GetMessage a prescindere dal fatto che il processo stia disegnando o meno.</remarks>
                STARTF_FORCEONFEEDBACK = 0x00000040,
                /// <summary>
                /// La modalità feedback del cursore è forzatamente disattivata durante l'avvio del processo.
                /// </summary>
                STARTF_FORCEOFFFEEDBACK = 0x00000080,
                /// <summary>
                /// Le finestre create dal processo non possono essere fissate sulla barra delle applicazioni.
                /// </summary>
                /// <remarks>Questa opzione deve essere combinata con <see cref="STARTF_TITLEISAPPID"/>.</remarks>
                STARTF_PREVENTPINNING = 0x00002000,
                /// <summary>
                /// Il processo deve essere eseguito a schermo intero.
                /// </summary>
                /// <remarks>Questa opzioni è valida solo per applicazioni console in un computer x86.</remarks>
                STARTF_RUNFULLSCREEN = 0x00000020,
                /// <summary>
                /// Il membro <see cref="Win32Structures.STARTUPINFO.Title"/> contiene un AppUserModelID.
                /// </summary>
                /// <remarks>Se l'opzione <see cref="STARTF_PREVENTPINNING"/> le finestre dell'applicazione non posso essere fissate sulla barra delle applicazioni, l'utilizzo delle proprietà della finestra relative all'AppUserModelID sovrascrive tale opzioni solo per quella finestra.<br/><br/>
                /// Questa opzione non può essere utilizzata con <see cref="STARTF_TITLEISLINKNAME"/>.</remarks>
                STARTF_TITLEISAPPID = 0x00001000,
                /// <summary>
                /// Il membro <see cref="Win32Structures.STARTUPINFO.Title"/> contiene il percorso di un collegamento che l'utente ha invocato per avviare il processo.
                /// </summary>
                /// <remarks>Questa opzione non può essere usata con <see cref="STARTF_TITLEISAPPID"/>.</remarks>
                STARTF_TITLEISLINKNAME = 0x00000800,
                /// <summary>
                /// La linea di comando ha un'origine non affidabile.
                /// </summary>
                STARTF_UNTRUSTEDSOURCE = 0x00008000,
                /// <summary>
                /// I membri <see cref="Win32Structures.STARTUPINFO.XCountChars"/> e <see cref="Win32Structures.STARTUPINFO.YCountChars"/> contengono ulteriori informazioni.
                /// </summary>
                STARTF_USECOUNTCHARS = 0x00000008,
                /// <summary>
                /// Il membro <see cref="Win32Structures.STARTUPINFO.FillAttribute"/> contiene ulteriori informazioni.
                /// </summary>
                STARTF_USEFILLATTRIBUTE = 0x00000010,
                /// <summary>
                /// Il membro <see cref="Win32Structures.STARTUPINFO.StdInput"/> contiene ulteriori informazioni.
                /// </summary>
                /// <remarks>Questa opzione non può essere utilizzata con <see cref="STARTF_USESTDHANDLES"/>.</remarks>
                STARTF_USEHOTKEY = 0x00000200,
                /// <summary>
                /// I membri <see cref="Win32Structures.STARTUPINFO.X"/> e <see cref="Win32Structures.STARTUPINFO.Y"/> contengono ulteriori informazioni.
                /// </summary>
                STARTF_USEPOSITION = 0x00000004,
                /// <summary>
                /// Il membro <see cref="Win32Structures.STARTUPINFO.ShowWindow"/> contiene ulteriori informazioni.
                /// </summary>
                STARTF_USESHOWWINDOW = 0x00000001,
                /// <summary>
                /// I membri <see cref="Win32Structures.STARTUPINFO.XSize"/> e <see cref="Win32Structures.STARTUPINFO.YSize"/> contengono ulteriori informazioni.
                /// </summary>
                STARTF_USESIZE = 0x00000002,
                /// <summary>
                /// I membri <see cref="Win32Structures.STARTUPINFO.StdInput"/>, <see cref="Win32Structures.STARTUPINFO.StdOutput"/> e <see cref="Win32Structures.STARTUPINFO.StdError"/> contengono ulteriori informazioni.
                /// </summary>
                /// <remarks>Se questa opzione viene specifica con una delle funzioni di creazione di processi, gli handle devono essere ereditabili e il parametro InheritHandles deve essere true.<br/><br/>
                /// Se questa opzione viene specificata durante la chiamata alla funzione GetStartupInfo, i membri hanno i valori dati durante la creazione del processo oppure <see cref="Win32Constants.INVALID_HANDLE_VALUE"/>.<br/>
                /// Gli handle devono essere chiusi usando la funzione <see cref="Win32OtherFunctions.CloseHandle(IntPtr)"/> quando non sono più necessari.<br/><br/>
                /// Questa opzione non può essere usata con <see cref="STARTF_USEHOTKEY"/></remarks>
                STARTF_USESTDHANDLES = 0x00000100
            }

            /// <summary>
            /// Opzioni per la creazione di un processo.
            /// </summary>
            [Flags]
            public enum ProcessCreationOptions : uint
            {
                NONE = 0,
                /// <summary>
                /// I processi figli associati a un processo associato a un job non sono associati a un job.
                /// </summary>
                /// <remarks>Se il processo chiamante non è associato a un job, questa opzione non ha effetto, in caso contrario il job deve avere il limite JOB_OBJECT_LIMIT_BREAKAWAY_OK applicato.</remarks>
                CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
                /// <summary>
                /// Il nuovo processo non eredita la modalità di errore del processo chiamante, ad esso viene assegnata la modalità di errore di default.
                /// </summary>
                CREATE_DEFAULT_ERROR_MODE = 0x04000000,
                /// <summary>
                /// Il nuovo processo ha una nuova console al posto di ereditare quello del padre.
                /// </summary>
                /// <remarks>Questa opzione non può essere utilizzata con <see cref="DETACHED_PROCESS"/>.</remarks>
                CREATE_NEW_CONSOLE = 0x00000010,
                /// <summary>
                /// Il nuovo processo è il processo root di un nuovo gruppo di processi.
                /// </summary>
                /// <remarks>Questa opzione viene ignorata se utilizzata con <see cref="CREATE_NEW_CONSOLE"/>.</remarks>
                CREATE_NEW_PROCESS_GROUP = 0x00000200,
                /// <summary>
                /// Il processo è un'applicazione console eseguita senza una finestra console.
                /// </summary>
                /// <remarks>Questa opzione viene ignorata se l'applicazione non è un'applicazione console o se usata insieme a <see cref="CREATE_NEW_CONSOLE"/> oppure a <see cref="DETACHED_PROCESS"/>.</remarks>
                CREATE_NO_WINDOW = 0x08000000,
                /// <summary>
                /// Il processo viene eseguito come processo protetto.
                /// </summary>
                CREATE_PROTECTED_PROCESS = 0x00040000,
                /// <summary>
                /// Permette al chiamante di eseguire processi figli che ignorano le restrizioni che sarebbero normalmente applicate.
                /// </summary>
                CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
                /// <summary>
                /// Permette la creazione di un processo sicuro, cioè eseguito in un ambiente di sicurezza basata sulla virtualizzazione.
                /// </summary>
                CREATE_SECURE_PROCESS = 0x00400000,
                /// <summary>
                /// Il processo viene eseguito in una Virtual DOS Machine (VDM) privata.
                /// </summary>
                /// <remarks>Questa opzione è valida solo quando viene avviata un'applicazione a 16-bit basata su Windows.</remarks>
                CREATE_SEPARATE_WOW_VDM = 0x00000800,
                /// <summary>
                /// Il processo viene eseguito in una Virtual DOS Machine (VDM) condivisa con altri processi.
                /// </summary>
                /// <remarks>Questa opzione è valida solo quando viene avviata un'applicazione a 16-bit basata su Windows.</remarks>
                CREATE_SHARED_WOW_VDM = 0x00001000,
                /// <summary>
                /// Il thread primario del nuovo processo viene creato in uno stato sospeso.
                /// </summary>
                CREATE_SUSPENDED = 0x00000004,
                /// <summary>
                /// Questa opzione indica che il blocco d'ambiente usa caratteri Unicode, in caso contrario il blocco usa caratteri ANSI.
                /// </summary>
                CREATE_UNICODE_ENVIRONMENT = 0x00000400,
                /// <summary>
                /// Il thread chiamante avvia ed esegue il debug del nuovo processo.
                /// </summary>
                DEBUG_ONLY_THIS_PROCESS = 0x00000002,
                /// <summary>
                /// Il thread chiamante avvia ed esegue il debug del nuovo processo e di tutti i processi figli avviati da quest'ultimo.
                /// </summary>
                /// <remarks>Se questa opzione è combinata a <see cref="DEBUG_PROCESS"/>, il chiamante esegue il debug solo del nuovo processo.</remarks>
                DEBUG_PROCESS = 0x00000001,
                /// <summary>
                /// Il processo non eredità la console del padre.
                /// </summary>
                /// <remarks>Non è possibile usare questa opzione insieme a <see cref="CREATE_NEW_CONSOLE"/>, questa opzioni è valida solo per processi console.</remarks>
                DETACHED_PROCESS = 0x00000008,
                /// <summary>
                /// Il processo è stato creato con informazioni di avvio estese.
                /// </summary>
                EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
                /// <summary>
                /// Il processo eredità l'affinità del padre.
                /// </summary>
                /// <remarks>Se il processo padre ha thread in più gruppi di processori, il nuovo processo eredita l'affinità relativa di un gruppo arbitrario in uso dal padre.</remarks>
                INHERIT_PARENT_AFFINITY = 0x00010000
            }
            #endregion
            #region NT API Enumerations
            #region Process Enumerations
            /// <summary>
            /// Tipo di informazioni da recuperare su un processo.
            /// </summary>
            public enum ProcessInformationClass
            {
                /// <summary>
                /// Puntatore alla struttura PEB e il PID.
                /// </summary>
                ProcessBasicInformation,
                /// <summary>
                /// Informazioni sulla memoria di un processo.
                /// </summary>
                ProcessVmCounters = 3,
                /// <summary>
                /// Numero di porta del debugger per il processo.
                /// </summary>
                ProcessDebugPort = 7,
                /// <summary>
                /// Determina se il processo è in esecuzione in ambiente WOW64.
                /// </summary>
                ProcessWow64Information = 26,
                /// <summary>
                /// Percorso completo dell'eseguibile di un processo.
                /// </summary>
                ProcessImageFileName = 27,
                /// <summary>
                /// Determina se il processo è di sistema.
                /// </summary>
                ProcessBreakOnTermination = 29,
                /// <summary>
                /// Percorso completo dell'eseguibile di un processo (Win32).
                /// </summary>
                ProcessImageFileNameWin32 = 43,
                /// <summary>
                /// Informazioni sugli handle di un processo.
                /// </summary>
                ProcessHandleInformation = 51,
                /// <summary>
                /// Linea di comando di un processo.
                /// </summary>
                ProcessCommandLineInformation = 60,
                /// <summary>
                /// Informazioni sulla protezione di un processo.
                /// </summary>
                ProcessProtectionInformation = 61,
                /// <summary>
                /// Tipo di sottosistema del processo.
                /// </summary>
                ProcessSubsystemInformation = 75
            }

            /// <summary>
            /// Livello di protezione di un processo.
            /// </summary>
            public enum ProcessProtectionLevel
            {
                None,
                Light,
                Full
            }

            /// <summary>
            /// Tipo di protezione di un processo.
            /// </summary>
            public enum ProcessProtectionType
            {
                None,
                Authenticode,
                CodeGen,
                Antimalware,
                Lsa,
                Windows,
                WinTcb,
                WinSystem,
                App,
                Max
            }
            #endregion
            #region Thread Enumerations
            /// <summary>
            /// Informazioni su un thread.
            /// </summary>
            public enum ThreadInformationClass
            {
                ThreadBasicInformation,
                ThreadTimes,
                ThreadPriority,
                ThreadBasePriority,
                ThreadAffinityMask,
                ThreadImpersonationToken,
                ThreadDescriptorTableEntry,
                ThreadEnableAlignmentFaultFixup,
                ThreadEventPair_Reusable,
                ThreadQuerySetWin32StartAddress,
                ThreadZeroTlsCell,
                ThreadPerformanceCount,
                ThreadAmlLastThread,
                ThreadIdealProcessor,
                ThreadPriorityBoost,
                ThreadSetTlsArrayAddress,
                ThreadIsIoPending,
                ThreadHideFromDebugger,
                ThreadBreakOnTermination,
                ThreadSwitchLegacyState,
                ThreadIsTerminated,
                ThreadLastSystemCall,
                ThreadIoPriority,
                ThreadCycleTime,
                ThreadPagePriority,
                ThreadActualBasePriority,
                ThreadTebInformation,
                ThreadCSwitchMon,
                ThreadCSwitchPmu,
                ThreadWow64Context,
                ThreadGroupInformation,
                ThreadUmsInformation,
                ThreadCountProfiling,
                ThreadIdealProcessorEx,
                ThreadCpuAccountingInformation,
                ThreadSuspendCount,
                ThreadHeterogeneousCpuPolicy,
                ThreadContainerId,
                ThreadNameInformation,
                ThreadSelectedCpuSets,
                ThreadSystemThreadInformation,
                ThreadActualGroupAffinity,
                MaxThreadInfoClass
            }

            /// <summary>
            /// Stato di un thread.
            /// </summary>
            public enum ThreadState : uint
            {
                /// <summary>
                /// Riconosciuto dal microkernel.
                /// </summary>
                Initialized,
                /// <summary>
                /// Pronto all'esecuzione.
                /// </summary>
                Ready,
                /// <summary>
                /// In esecuzione.
                /// </summary>
                Running,
                /// <summary>
                /// Sta per essere eseguito.
                /// </summary>
                Standby,
                /// <summary>
                /// Esecuzione terminata.
                /// </summary>
                Terminated,
                /// <summary>
                /// Non pronto per il processore.
                /// </summary>
                Waiting,
                /// <summary>
                /// In attesa di una risorsa diversa dal processore.
                /// </summary>
                Transition,
                /// <summary>
                /// Stato sconosciuto.
                /// </summary>
                Unknown
            }

            /// <summary>
            /// Motivazione per cui un thread è in attesa.
            /// </summary>
            public enum ThreadWaitReason : uint
            {
                Executive1,
                FreePage1,
                PageIn1,
                PoolAllocation1,
                ExecutionDelay1,
                FreePage2,
                PageIn2,
                Executive2,
                FreePage3,
                PageIn3,
                PoolAllocation2,
                ExecutionDelay2,
                FreePage4,
                PageIn4,
                EventPairHigh,
                EventPairLow,
                LPCReceive,
                LPCReply,
                VirtualMemory,
                PageOut,
                Unknown
            }
            #endregion
            #region Objects Info Enumerations
            /// <summary>
            /// Informazioni su un timer.
            /// </summary>
            public enum TimerInformationClass
            {
                /// <summary>
                /// Informazioni di base su un timer.
                /// </summary>
                TimerBasicInformation
            }

            /// <summary>
            /// Informazioni su un semaforo.
            /// </summary>
            public enum SemaphoreInformationClass
            {
                /// <summary>
                /// Informazioni di base su un semaforo.
                /// </summary>
                SemaphoreBasicInformation
            }

            /// <summary>
            /// Informazioni su una sezione.
            /// </summary>
            public enum SectionInformationClass
            {
                /// <summary>
                /// Informazioni di base su una sezione.
                /// </summary>
                SectionBasicInformation,
                /// <summary>
                /// Informazioni sull'immagine associata alla sezione
                /// </summary>
                SectionImageInformation
            }

            /// <summary>
            /// Attributi di una sezione.
            /// </summary>
            [Flags]
            public enum SectionAttributes : uint
            {
                SEC_RESERVE = FileMappingAttributes.SEC_RESERVE,
                SEC_IMAGE = FileMappingAttributes.SEC_IMAGE,
                SEC_FILE = 0x00800000
            }

            /// <summary>
            /// Informazioni su un mutante.
            /// </summary>
            public enum MutantInformationClass
            {
                /// <summary>
                /// Informazioni di base su un mutante.
                /// </summary>
                MutantBasicInformation
            }

            /// <summary>
            /// Informazioni su un evento.
            /// </summary>
            public enum EventInformationClass
            {
                /// <summary>
                /// Informazioni di base su un evento.
                /// </summary>
                EventBasicInformation
            }

            /// <summary>
            /// Tipo di evento.
            /// </summary>
            public enum EventType
            {
                /// <summary>
                /// Evento con reset manuale.
                /// </summary>
                NotificationEvent,
                /// <summary>
                /// Evento con reset automatico.
                /// </summary>
                SynchronizationEvent
            }

            /// <summary>
            /// Informazioni su una chiave di registro.
            /// </summary>
            public enum KeyinformationClass
            {
                KeyBasicInformation,
                KeyNodeInformation,
                KeyFullInformation,
                KeyNameInformation,
                KeyCachedInformation,
                KeyFlagsInformation,
                KeyVirtualizationInformation,
                KeyHandleTagsInformation,
                KeyTrustInformation,
                KeyLayerInformation,
                MaxKeyInfoClass
            }
            #endregion
            #endregion
            #region Thread Enumerations
            /// <summary>
            /// Priorità di un thread.
            /// </summary>
            public enum ThreadPriority
            {
                /// <summary>
                /// Attiva la modalità di elaborazione in background.
                /// </summary>
                /// <remarks>Questa valore può essere utilizzato solo con il thread attuale.</remarks>
                THREAD_MODE_BACKGROUND_BEGIN = 0x00010000,
                /// <summary>
                /// Disattiva la modalità di elaborazione in background.
                /// </summary>
                /// <remarks>Questa valore può essere utilizzato solo con il thread attuale.</remarks>
                THREAD_MODE_BACKGROUND_END = 0x00020000,
                /// <summary>
                /// Sopra il normale.
                /// </summary>
                THREAD_PRIORITY_ABOVE_NORMAL = 1,
                /// <summary>
                /// Sotto il normale.
                /// </summary>
                THREAD_PRIORITY_BELOW_NORMAL = -1,
                /// <summary>
                /// Alta priorità.
                /// </summary>
                THREAD_PRIORITY_HIGHEST = 2,
                /// <summary>
                /// Inattivo.
                /// </summary>
                THREAD_PRIORITY_IDLE = -15,
                /// <summary>
                /// Bassa priorità.
                /// </summary>
                THREAD_PRIORITY_LOWEST = -2,
                /// <summary>
                /// Normale.
                /// </summary>
                THREAD_PRIORITY_NORMAL = 0,
                /// <summary>
                /// Tempo reale.
                /// </summary>
                THREAD_PRIORITY_TIME_CRITICAL = 15
            }

            /// <summary>
            /// Stato di un GUI thread.
            /// </summary>
            [Flags]
            public enum GUIThreadStates : uint
            {
                /// <summary>
                /// Stato del rettangolo, impostato se il rettangolo è visibile.
                /// </summary>
                GUI_CARETBLINKING = 0x00000001,
                /// <summary>
                /// Stato del menù, impostato se il thread è in modalità menù.
                /// </summary>
                GUI_INMENUMODE = 0x00000004,
                /// <summary>
                /// Stato del movimento, impostato se il thread è in un ciclo di movimento o ridimensionamento.
                /// </summary>
                GUI_INMOVESIZE = 0x00000002,
                /// <summary>
                /// Stato del menù popup, impostato se il thread ha un menù popup attivo.
                /// </summary>
                GUI_POPUPMENUMODE = 0x00000010,
                /// <summary>
                /// Stato del menù di sistema, impostato se il thread è in una modalità del menù di sistema.
                /// </summary>
                GUI_SYSTEMMENUMODE = 0x00000008
            }

            /// <summary>
            /// Opzioni per la creazione di un thread remoto.
            /// </summary>
            [Flags]
            public enum RemoteThreadOptions : uint
            {
                /// <summary>
                /// Il thread viene eseguito immediatamente.
                /// </summary>
                RunImmediately,
                /// <summary>
                /// Il thread viene creato sospeso.
                /// </summary>
                CreateSuspended = 0x00000004,
                /// <summary>
                /// La dimensione dello stack indicata specifica la dimensione iniziale riservata.
                /// </summary>
                StackSizeParamIsAReservation = 0x00010000
            }
            #endregion
            #region Window Enumerations
            #region Window Normal And Extended Styles
            /// <summary>
            /// Stili di una finestra.
            /// </summary>
            [Flags]
            public enum WindowStyles : uint
            {
                /// <summary>
                /// La finestra ha un bordo fine.
                /// </summary>
                WS_BORDER = 0x00800000,
                /// <summary>
                /// La finestra ha una barra del titolo.
                /// </summary>
                WS_CAPTION = 0x00C00000,
                /// <summary>
                /// La finestra è una finestra figlia, questo stile non può essere usato con <see cref="WS_POPUP"/>.
                /// </summary>
                WS_CHILD = 0x40000000,
                /// <summary>
                /// La finestra è una finestra figlia, questo stile non può essere usato con <see cref="WS_POPUP"/>.
                /// </summary>
                WS_CHILDWINDOW = WS_CHILD,
                /// <summary>
                /// Esclude l'area occupata dalle finestre figlio al momento del disegno all'interno della finestra padre, questo stile viene usato per creare la finestra padre.
                /// </summary>
                WS_CLIPCHILDREN = 0x02000000,
                /// <summary>
                /// 
                /// </summary>
                WS_CLIPSIBLINGS = 0x04000000,
                /// <summary>
                /// La finestra è inizialmente disabilitata, una finestra disabilitata non può ricevere input dall'utente.
                /// </summary>
                WS_DISABLED = 0x08000000,
                /// <summary>
                /// La finestra ha un bordo il cui stile è tipico delle finestre di dialogo, una finestra con questo stile non può avere una barra del titolo.
                /// </summary>
                WS_DLGFRAME = 0x00400000,
                /// <summary>
                /// La finestra è il primo controllo di un gruppo di controlli.
                /// </summary>
                WS_GROUP = 0x00020000,
                /// <summary>
                /// La finestra ha un barra di scorrimento orizzontale.
                /// </summary>
                WS_HSCROLL = 0x00100000,
                /// <summary>
                /// La finestra è inizialmente minimizzata.
                /// </summary>
                WS_ICONIC = 0x20000000,
                /// <summary>
                /// La finestra è inizialmente massimizzata.
                /// </summary>
                WS_MAXIMIZE = 0x01000000,
                /// <summary>
                /// La finestra ha un pulsante "Ingrandisci", non puo essere combinato con <see cref="ExtendedWindowStyles.WS_EX_CONTEXTHELP"/>, deve essere specificato anche lo stile <see cref="WS_SYSMENU"/>.
                /// </summary>
                WS_MAXIMIZEBOX = 0x00010000,
                /// <summary>
                /// La finestra è inizialmente minimizzata.
                /// </summary>
                WS_MINIMIZE = WS_ICONIC,
                /// <summary>
                /// La finestra ha un pulsante "Ripristina giù", non puo essere combinato con <see cref="ExtendedWindowStyles.WS_EX_CONTEXTHELP"/>, deve essere specificato anche lo stile <see cref="WS_SYSMENU"/>.
                /// </summary>
                WS_MINIMIZEBOX = WS_GROUP,
                /// <summary>
                /// 
                /// </summary>
                WS_OVERLAPPED = 0x00000000,
                /// <summary>
                /// 
                /// </summary>
                WS_OVERLAPPEDWINDOW =
                    WS_OVERLAPPED |
                    WS_CAPTION |
                    WS_SYSMENU |
                    WS_THICKFRAME |
                    WS_MINIMIZEBOX |
                    WS_MAXIMIZEBOX,
                /// <summary>
                /// Finestra popup, non può essere usato con <see cref="WS_CHILD"/>.
                /// </summary>
                WS_POPUP = 0x80000000,
                /// <summary>
                /// Finestra popup, perché il menù sia visibile questo stile deve essere combinato con <see cref="WS_CAPTION"/>.
                /// </summary>
                WS_POPUPWINDOW =
                    WS_POPUP |
                    WS_BORDER |
                    WS_SYSMENU,
                /// <summary>
                /// La finestra ha un bordo spesso.
                /// </summary>
                WS_SIZEBOX = 0x00040000,
                /// <summary>
                /// La finestra presenta i pulsanti del menù di sistema sulla barra del titolo, deve essere anche specificato lo stile <see cref="WS_CAPTION"/>.
                /// </summary>
                WS_SYSMENU = 0x00080000,
                /// <summary>
                /// La finestra è un controllo che può ricevere il controllo della tastiera quando l'utente preme TAB.
                /// </summary>
                WS_TABSTOP = WS_MAXIMIZEBOX,
                /// <summary>
                /// La finestra ha un bordo spesso.
                /// </summary>
                WS_THICKFRAME = WS_SIZEBOX,
                /// <summary>
                /// 
                /// </summary>
                WS_TILED = WS_OVERLAPPED,
                /// <summary>
                /// 
                /// </summary>
                WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,
                /// <summary>
                /// La finestra è inizialmente visibile.
                /// </summary>
                WS_VISIBLE = 0x10000000,
                /// <summary>
                /// La finestra ha un barra di scorrimento verticale.
                /// </summary>
                WS_VSCROLL = 0x00200000
            }

            /// <summary>
            /// Stili estesi di una finestra.
            /// </summary>
            [Flags]
            public enum ExtendedWindowStyles : uint
            {
                /// <summary>
                /// La finestra accetta drag-drop di files.
                /// </summary>
                WS_EX_ACCEPTFILES = 0x00000010,
                /// <summary>
                /// Forza una finestra top-level sulla barra delle applicazioni quando è visibile.
                /// </summary>
                WS_EX_APPWINDOW = 0x00040000,
                /// <summary>
                /// La finestra ha un bordo incavato.
                /// </summary>
                WS_EX_CLIENTEDGE = 0x00000200,
                /// <summary>
                /// Disegna tutti i discendenti di una finestra dal basso verso l'altro usando il double buffering, questo stile non può essere usato insieme agli stili di classe <see cref="WindowClassStyles.CS_OWNDC"/> e <see cref="WindowClassStyles.CS_CLASSDC"/>.
                /// </summary>
                WS_EX_COMPOSITED = 0x02000000,
                /// <summary>
                /// La barra del titolo della finestra include un punto di domanda, questo stile non può essere usato insieme agli stili <see cref="WindowStyles.WS_MAXIMIZEBOX"/> e <see cref="WindowStyles.WS_MINIMIZEBOX"/>.
                /// </summary>
                WS_EX_CONTEXTHELP = 0x00000400,
                /// <summary>
                /// La finestra contiene finestre figlie che dovrebbero prendere parte alla navigazione di un box di dialogo.
                /// </summary>
                WS_EX_CONTROLPARENT = 0x00010000,
                /// <summary>
                /// La finestra ha un bordo doppio, può avere, in modo opzionale, una barra del titolo se lo stile <see cref="WindowStyles.WS_CAPTION"/> è specificato.
                /// </summary>
                WS_EX_DLGMODALFRAME = 0x00000001,
                /// <summary>
                /// La finestra è una finestra a strati, questo stile non può essere usato con gli stili di classe <see cref="WindowClassStyles.CS_OWNDC"/> e <see cref="WindowClassStyles.CS_CLASSDC"/>.
                /// </summary>
                WS_EX_LAYERED = 0x00080000,
                /// <summary>
                /// Per i linguaggi che supportano l'ordine di lettura, l'origine orizzontale della finestra si trova sul bordo destro.
                /// </summary>
                WS_EX_LAYOUTRTL = 0x00400000,
                /// <summary>
                /// I contenuti della finestra sono allineati a sinistra.
                /// </summary>
                WS_EX_LEFT = 0x00000000,
                /// <summary>
                /// Per i linguaggi che supportano l'ordine di lettura, la barra di scorrimento verticale (se presente) si trova a sinistra dell'area client, per gli altri linguaggi questo stile viene ignorato.
                /// </summary>
                WS_EX_LEFTSCROOLBAR = 0x00004000,
                /// <summary>
                /// Il testo della finestra è reso visibile usando proprietà di lettura da sinistra a destra.
                /// </summary>
                WS_EX_LTRREADING = WS_EX_LEFT,
                /// <summary>
                /// La finestra è una finestra figlia MDI.
                /// </summary>
                WS_EX_MDICHILD = 0x00000040,
                /// <summary>
                /// Una finestra top-level creata con questo stile non diventa la finestra in primo piano quando l'utente ci clicca sopra, il sistema non la porta in primo piano quando l'utente la minimizza o chiude la finestra in primo piano.<br/>
                /// La finestra non è visibile sulla barra delle applicazioni per impostazione predefinita, usare lo stile <see cref="WS_EX_APPWINDOW"/> per renderla visibile sulla barra delle applicazioni.
                /// </summary>
                WS_EX_NOACTIVATE = 0x08000000,
                /// <summary>
                /// La finestra non passa il suo layout alle finestre figlie.
                /// </summary>
                WS_EX_NOINHERITLAYOUT = 0x00100000,
                /// <summary>
                /// Una finestra figlia creata con questo stile non notifica, quando viene creata o distrutta, la sua finestra padre.
                /// </summary>
                WS_EX_NOPARENTNOTIFY = 0x00000004,
                /// <summary>
                /// La finestra non esegue il rendering di una superficie, questo stile è utile per le finestre che non hanno contenuto visibile o che usano meccanismi propri per la visualizzazione.
                /// </summary>
                WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
                /// <summary>
                /// 
                /// </summary>
                WS_EX_OVERLAPPEDWINDOW =
                    WS_EX_WINDOWEDGE |
                    WS_EX_CLIENTEDGE,
                /// <summary>
                /// 
                /// </summary>
                WS_EX_PALETTEWINDOW =
                    WS_EX_WINDOWEDGE |
                    WS_EX_TOOLWINDOW |
                    WS_EX_TOPMOST,
                /// <summary>
                /// I contenuti della finestra sono allineati a destra, dipende dalla classe della finestra, è utile soltanto per lingue che supportano l'ordine di lettura.
                /// </summary>
                WS_EX_RIGHT = 0x00001000,
                /// <summary>
                /// La barra di scorrimento verticale (se presente) è visibile alla destra dell'area client.
                /// </summary>
                WS_EX_RIGHTSCROLLBAR = WS_EX_LEFT,
                /// <summary>
                /// Il testo della finestra viene reso visibile usando proprietà di lettura da destra verso sinistra, valido soltanto per lingue che supportano l'ordine di lettura.
                /// </summary>
                WS_EX_RTLREADING = 0x00002000,
                /// <summary>
                /// La finestra ha un bordo tridimensionale inteso che oggetti che non accettano input dall'utente.
                /// </summary>
                WS_EX_STATICEDGE = 0x00020000,
                /// <summary>
                /// La finestra è intesa per l'utilizzo come una barra degli strumenti fluttuante, queste finestre hanno una barra del titolo più corta del normale e viene usato un font più piccolo per disegnare il titolo.<br/>
                /// Queste finestre non sono presenti sulla barra delle applicazioni e non sono visibili tra la selezione di finestre visibile quando si preme ALT+TAB.<br/>
                /// Se la finestra ha un menù di sistema, le sue icone non sono visibili sulla barra del titolo, ma possono essere rese visibili facendo clic destro o premendo ALT+SPACE.
                /// </summary>
                WS_EX_TOOLWINDOW = 0x00000080,
                /// <summary>
                /// La finestra dovrebbe essere posizionata sopra tutte le altre e restarci anche quando viene disattivata.
                /// </summary>
                WS_EX_TOPMOST = 0x00000008,
                /// <summary>
                /// La finestra non dovrebbe essere disegnata fino a quando le altre finestre, create dallo stesso thread, al di sotto di essa siano state disegnate.
                /// </summary>
                WS_EX_TRANSPARENT = 0x00000020,
                /// <summary>
                /// La finestra ha un bordo rialzato.
                /// </summary>
                WS_EX_WINDOWEDGE = 0x00000100
            }
            #endregion
            #region Window And Window Class Info
            /// <summary>
            /// Offset dove recuperare alcune informazioni su una finestra.
            /// </summary>
            /// <remarks>Alcuni membri di questa enumerazione non sono definiti perché i loro valori non sono costanti.</remarks>
            [Flags]
            public enum WindowsInfoOffsets
            {
                /// <summary>
                /// Stili estesi della finestra.
                /// </summary>
                GWL_EXSTYLE = -20,
                /// <summary>
                /// Handle all'istanza dell'applicazione.
                /// </summary>
                GWLP_HINSTANCE = -6,
                /// <summary>
                /// Handle alla finestra padre, se esiste.
                /// </summary>
                GWLP_HWNDPARENT = -8,
                /// <summary>
                /// ID della finestra.
                /// </summary>
                GWLP_ID = -12,
                /// <summary>
                /// Stili della finestra.
                /// </summary>
                GWL_STYLE = -16,
                /// <summary>
                /// Dati utente associati alla finestra.
                /// </summary>
                GWLP_USERDATA = -21,
                /// <summary>
                /// Puntatore alla funzione principale della finestra.
                /// </summary>
                GWLP_WNDPROC = -4,
                /// <summary>
                /// Risultato di un messaggio elaborato.
                /// </summary>
                /// <remarks>Questo membro può essere usato soltanto con le finestre di dialogo.</remarks>
                DWLP_MSGRESULT = 0
            }

            /// <summary>
            /// Stili di classe.
            /// </summary>
            [Flags]
            public enum WindowClassStyles
            {
                /// <summary>
                /// Allinea l'area client di una finestra rispetto a un bordo byte (nella direzione x), questo stile ha effetto sulla larghezza della finestra e sul suo posizionamento orizzontale sullo schermo.
                /// </summary>
                CS_BYTEALIGNCLIENT = 0x1000,
                /// <summary>
                /// Allinea la finestra rispetto a un bordo byte (nella direzione x), questo stile ha effetto sulla larghezza della finestra e sul suo posizionamento orizzontale sullo schermo.
                /// </summary>
                CS_BYTEALIGNWINDOW = 0x2000,
                /// <summary>
                /// Alloca un contesto di dispositivo da condividere con tutte le finestre della classe.
                /// </summary>
                CS_CLASSDC = 0x0040,
                /// <summary>
                /// Invia un messaggio di doppio clic alla funzione principale della finestra quando l'utente fa doppio clic mentre il cursore del mouse si trova all'interno di una finestra appartenente alla classe.
                /// </summary>
                CS_DBLCLKS = 0x0008,
                /// <summary>
                /// Abilità l'ombra di una finestra.
                /// </summary>
                CS_DROPSHADOW = 0x00020000,
                /// <summary>
                /// Indica che la classe è globale per l'applicazione.
                /// </summary>
                CS_GLOBALCLASS = 0x4000,
                /// <summary>
                /// Ridisegna interamente la finestra se avviene un movimento o un ridimensionamento dello spessore dell'area client.
                /// </summary>
                CS_HREDRAW = 0x0002,
                /// <summary>
                /// Disabilità il tasto chiude del menù di sistema della finestra.
                /// </summary>
                CS_NOCLOSE = 0x0200,
                /// <summary>
                /// Alloca un contesto di dispositivo unico per ogni finestra della classe.
                /// </summary>
                CS_OWNDC = 0x0020,
                /// <summary>
                /// Permette a una finestra figlia di disegnare sopra la finestra padre.
                /// </summary>
                CS_PARENTDC = 0x0080,
                /// <summary>
                /// Salva, come bitmap, la porzione di schermo oscurata da una finestra della classe.
                /// </summary>
                CS_SAVEBITS = 0x0800,
                /// <summary>
                /// Ridisegna interamente la finestra se avviene un movimento o un ridimensionamento dell'altezza dell'area client.
                /// </summary>
                CS_VREDRAW = 0x0001
            }

            /// <summary>
            /// Informazioni sulla classe di una finestra.
            /// </summary>
            public enum WindowClassInfo
            {
                /// <summary>
                /// Atom che identifica la classe.
                /// </summary>
                GCW_ATOM = -32,
                /// <summary>
                /// Dimensione, in bytes, della memoria extra associata con la classe.
                /// </summary>
                GCL_CBCLSEXTRA = -20,
                /// <summary>
                /// Dimensione, in bytes, della memoria extra associata con ogni finestra della classe.
                /// </summary>
                GCL_CBWNDEXTRA = -18,
                /// <summary>
                /// Handle nativo al pennello in background associato alla classe.
                /// </summary>
                GCLP_HBRBACKGROUND = -10,
                /// <summary>
                /// Handle nativo al cursore associato alla classe.
                /// </summary>
                GCLP_HCURSOR = -12,
                /// <summary>
                /// Handle nativo all'icona associata alla classe.
                /// </summary>
                GCLP_HICON = -14,
                /// <summary>
                /// Handle nativo all'icona piccola associata alla classe.
                /// </summary>
                GCLP_HICONSM = -34,
                /// <summary>
                /// Handle nativo al modulo che ha registrato la classe.
                /// </summary>
                GCLP_HMODULE = -16,
                /// <summary>
                /// Nome del menù associato alla classe.
                /// </summary>
                GCLP_MENUNAME = -8,
                /// <summary>
                /// Stili della classe.
                /// </summary>
                GCL_STYLE = -26,
                /// <summary>
                /// Puntatore alla procedura principale della finestra.
                /// </summary>
                GCLP_WNDPROC = -24
            }
            #endregion
            #region Other Values
            /// <summary>
            /// Valori speciali per il parametro PrecedingWindowHandle della funzione <see cref="Win32ProcessFunctions.SetWindowPos(IntPtr, IntPtr, int, int, int, int, uint)"/>.
            /// </summary>
            public enum PrecedingWindowHandleSpecialValue
            {
                /// <summary>
                /// Posiziona la finestra alla base dell'ordine Z.
                /// </summary>
                /// <remarks>Se utilizzato con una finestra in primo piano permanente, essa perde questo stato e viene posizionata all'ultimo posto dell'ordine Z.</remarks>
                HWND_BOTTOM = 1,
                /// <summary>
                /// Posiziona la finestra sotto tutte le finestre in primo piano non permanente.
                /// </summary>
                /// <remarks>Questo valore non ha effetto se la finestra è già in tale posizione.</remarks>
                HWND_NOTOPMOST = -2,
                /// <summary>
                /// Posiziona la finestra alla primo posto nell'ordine Z.
                /// </summary>
                HWND_TOP = 0,
                /// <summary>
                /// Posiziona la finestra sopra tutte le finestre in primo piano non permanente.
                /// </summary>
                /// <remarks>La finestra mantiene la sua posizione anche se disattivata.</remarks>
                HWND_TOPMOST = -1
            }

            /// <summary>
            /// Opzioni per il parametro Flags della funzione <see cref="Win32ProcessFunctions.SetWindowPos(IntPtr, IntPtr, int, int, int, int, uint)"/>.
            /// </summary>
            [Flags]
            public enum WindowSizingAndPositioningOptions : uint
            {
                /// <summary>
                /// Se il thread chiamante e il thread proprietario della finestra sono associati a due input queue diverse, il sistema invia la richiesta al thread proprietario della finestra.<br/>
                /// Questo evita che il thread chiamante resti bloccato in attesa che la richiesta venga elaborata.
                /// </summary>
                SWP_ASYNCWINDOWPOS = 0x4000,
                /// <summary>
                /// Impedisce la generazione del messaggio WM_SYNCPAINT.
                /// </summary>
                SWP_DEFERERASE = 0x2000,
                /// <summary>
                /// Disegna un frame (definitivo nella descrizione della classe) attorno alla finestra.
                /// </summary>
                SWP_DRAWFRAME = 0x0020,
                /// <summary>
                /// Applica i nuovi stili appena definiti al frame, invia il messaggio WM_NCCALCSIZE anche se la dimensione della finestra non è cambiata.
                /// </summary>
                SWP_FRAMECHANGED = SWP_DRAWFRAME,
                /// <summary>
                /// Nasconde la finestra.
                /// </summary>
                SWP_HIDEWINDOW = 0x0080,
                /// <summary>
                /// Non attiva la finestra.
                /// </summary>
                /// <remarks>Se questa opzione non è specificata, la finestra viene attivata e posizionata dove richiesto.</remarks>
                SWP_NOACTIVATE = 0x0010,
                /// <summary>
                /// I contenuti dell'area client della finestra vengono ignorati.
                /// </summary>
                /// <remarks>Se questa opzione non è specificata, il contenuto valido dell'area client viene salvato e copiato di nuovo nell'area client dopo il ridimensionamento o il riposizionamento.</remarks>
                SWP_NOCOPYBITS = 0x0100,
                /// <summary>
                /// Mantiene la posizione attuale della finestra.
                /// </summary>
                SWP_NOMOVE = 0x0002,
                /// <summary>
                /// Non cambia la posizione della finestra proprietaria nell'ordine Z.
                /// </summary>
                SWP_NOOWNERZORDER = 0x0200,
                /// <summary>
                /// Non ridisegna i cambiamenti alla finestra, questa opzione ha effetto sull'area client, sull'area non client e su ogni parte della finestra scoperta dopo il riposizionamento.
                /// </summary>
                /// <remarks>Quando questa opzione è impostata, l'applicazione deve esplicitamente invalidare o ridisegnare le parti della finestra e della finestra padre che necessitano di essere ridisegnate.</remarks>
                SWP_NOREDRAW = 0x0008,
                /// <summary>
                /// Non cambia la posizione della finestra proprietaria nell'ordine Z.
                /// </summary>
                SWP_NOREPOSITION = SWP_NOOWNERZORDER,
                /// <summary>
                /// Impedisce alla finestra di ricevere il messaggio WM_WINDOWPOSCHANGING.
                /// </summary>
                SWP_NOSENDCHANGING = 0x0400,
                /// <summary>
                /// Mantiene la dimensione corrente della finestra.
                /// </summary>
                SWP_NOSIZE = 0x0001,
                /// <summary>
                /// Mantiene l'ordine Z corrente della finestra.
                /// </summary>
                SWP_NOZORDER = 0x0004,
                /// <summary>
                /// Mostra la finestra.
                /// </summary>
                SWP_SHOWWINDOW = 0x0040
            }

            /// <summary>
            /// Stato visivo di una finestra.
            /// </summary>
            public enum WindowShowState
            {
                /// <summary>
                /// Riduce a icona una finestra, anche se il thread proprietario non risponde.
                /// </summary>
                /// <remarks>Questa opzione dovrebbe essere utilizzata solo per ridurre a icona finestre che non appartengono al thread chiamante.</remarks>
                SW_FORCEMINIMIZE = 11,
                /// <summary>
                /// Nasconde la finestra e ne attiva un'altra.
                /// </summary>
                SW_HIDE = 0,
                /// <summary>
                /// Ingrandisce la finestra.
                /// </summary>
                SW_MAXIMIZE = 3,
                /// <summary>
                /// Riduce a icona la finestra e attiva la prossima finestra top-level nell'ordine Z.
                /// </summary>
                SW_MINIMIZE = 6,
                /// <summary>
                /// Attiva e visualizza la finestra, se la finestra è ridotta a icona o ingrandita, il sistema la ripristina alla sua posizione e dimensione originale.
                /// </summary>
                /// <remarks>Questa opzione dovrebbe essere utilizza per ripristinare una finestra ridotta a icona.</remarks>
                SW_RESTORE = 9,
                /// <summary>
                /// Attiva e visualizza la finestra nella sua posizione e dimensione corrente.
                /// </summary>
                SW_SHOW = 5,
                /// <summary>
                /// Visualizza la finestra utilizzando le impostazioni specificate dall'applicazione proprietaria.
                /// </summary>
                SW_SHOWDEFAULT = 10,
                /// <summary>
                /// Attiva e visualizza la finestra come finestra ingradita.
                /// </summary>
                SW_SHOWMAXIMIZED = SW_MAXIMIZE,
                /// <summary>
                /// Attiva e visualizza la finestra come finestra ridotta a icona.
                /// </summary>
                SW_SHOWMINIMIZED = 2,
                /// <summary>
                /// Visualizza la finestra come finestra ridotta a icona.
                /// </summary>
                SW_SHOWMINNOACTIVE = 7,
                /// <summary>
                /// Visualizza la finestra nella sua posizione e dimensione corrente senza attivarla.
                /// </summary>
                SW_SHOWNA = 8,
                /// <summary>
                /// Visualizza la finestra nella sua più recente posizione e dimensione senza attivarla.
                /// </summary>
                SW_SHOWNOACTIVATE = 4,
                /// <summary>
                /// Attiva e visualizza la finestra, se la finestra è ridotta a icona o ingrandita, il sistema la ripristina alla sua posizione e dimensione originale.
                /// </summary>
                SW_SHOWNORMAL = 1
            }

            /// <summary>
            /// Opzioni per le impostazioni degli attributi di una finestra a strati, usate dalla funzione <see cref="Win32ProcessFunctions.SetLayeredWindowAttributes(IntPtr, uint, byte, LayeredWindowAttributesFlags)"/>.
            /// </summary>
            [Flags]
            public enum LayeredWindowAttributesFlags
            {
                /// <summary>
                /// Usare il valore alpha per determinare l'opacità della finestra.
                /// </summary>
                LWA_ALPHA = 0x00000002,
                /// <summary>
                /// Usare il colore di trasparenza.
                /// </summary>
                LWA_COLORKEY = 0x00000001
            }
            #endregion
            #endregion
            #region Token Enumerations
            /// <summary>
            /// Informazioni su un token di accesso.
            /// </summary>
            public enum TokenInformationClass
            {
                /// <summary>
                /// Informazioni sull'account utente.
                /// </summary>
                TokenUser = 1,
                /// <summary>
                /// Informazioni sui gruppi di account associati al token.
                /// </summary>
                TokenGroups,
                /// <summary>
                /// Privilegi del token.
                /// </summary>
                TokenPrivileges,
                /// <summary>
                /// Informazioni sull'identificatore di sicurezza (SID) del proprietario di default per nuovi oggetti.
                /// </summary>
                TokenOwner,
                /// <summary>
                /// Informazioni sul SID del gruppo primario di default per nuovi oggetti.
                /// </summary>
                TokenPrimaryGroup,
                /// <summary>
                /// Dacl di default per nuovi oggetti.
                /// </summary>
                TokenDefaultDacl,
                /// <summary>
                /// Fonte del token, il permesso <see cref="TokenAccessRights.TOKEN_QUERY_SOURCE"/> è necessario per recuperare questa informazione.
                /// </summary>
                TokenSource,
                /// <summary>
                /// Indica se il token è un token primario o di impersonazione.
                /// </summary>
                TokenType,
                /// <summary>
                /// Indica il livello di impersonazione di un token, se il token non è di impersonazione al funzione che richiede questa informazioni non riuscirà.
                /// </summary>
                TokenImpersonationLevel,
                /// <summary>
                /// Informazioni sulle statistiche del token.
                /// </summary>
                TokenStatistics,
                /// <summary>
                /// SID a cui il token non può accedere.
                /// </summary>
                TokenRestrictedSids,
                /// <summary>
                /// Identificatore della sessione Terminal Services associata al token, in un ambiente non-Terminal Services l'identificatore ha valore 0. (Valore DWORD)
                /// </summary>
                TokenSessionId,
                /// <summary>
                /// SID dell'utente, i gruppi di account, i SID a cui il token non può accedere e l'ID di autenticazione associati con il token.
                /// </summary>
                TokenGroupsAndPrivileges,
                /// <summary>
                /// Riservato.
                /// </summary>
                TokenSessionReference,
                /// <summary>
                /// Se il token include la flag SANDBOX_INERT il valore DWORD recuperato sarà diverso da 0. (Valore DWORD)
                /// </summary>
                TokenSandboxInert,
                /// <summary>
                /// Riservato.
                /// </summary>
                TokenAuditPolicy,
                /// <summary>
                /// Informazioni sull'origine del token, se esso è stato creato usando credenziali esplicite il valore corrisponderà all'ID della sessione di accesso, se è stato creato usando autenticazione di rete allora il valore sarà 0.
                /// </summary>
                TokenOrigin,
                /// <summary>
                /// Livello di elevazione del token.
                /// </summary>
                TokenElevationType,
                /// <summary>
                /// Struttura che contiene un handle a un altro token collegato a questo.
                /// </summary>
                TokenLinkedToken,
                /// <summary>
                /// Struttura che specifica se il token è elevato.
                /// </summary>
                TokenElevation,
                /// <summary>
                /// Valore DWORD se il token è mai stato filtrato.
                /// </summary>
                TokenHasRestrictions,
                /// <summary>
                /// Struttura che specifica le informazioni di sicurezza contenute in un token.
                /// </summary>
                TokenAccessInformation,
                /// <summary>
                /// Valore DWORD che indica se la virtualizzazione è permessa per il token.
                /// </summary>
                TokenVirtualizationAllowed,
                /// <summary>
                /// Valore DWORD che indica se la virtualizzazione è abilitata per il token.
                /// </summary>
                TokenVirtualizationEnabled,
                /// <summary>
                /// Struttura che specifica il livello di integrità del token.
                /// </summary>
                TokenIntegrityLevel,
                /// <summary>
                /// Valore DWORD diverso da 0 se il token ha il flag UIAccess impostato.
                /// </summary>
                TokenUIAccess,
                /// <summary>
                /// Struttura che specifica la politica di integrità obbligatoria.
                /// </summary>
                TokenMandatoryPolicy,
                /// <summary>
                /// Struttura che specifica il SID di accesso del token.
                /// </summary>
                TokenLogonSid,
                /// <summary>
                /// Valore DWORD diverso da 0 se il token è di tipo app container.
                /// </summary>
                TokenIsAppContainer,
                /// <summary>
                /// Struttura che contiene le capacità associate al token.
                /// </summary>
                TokenCapabilities,
                /// <summary>
                /// Struttura che contiene il l'AppContainerSID associato al token, il membro TokenAppContainer della struttura è nullo se il token non è associato con un app container.
                /// </summary>
                TokenAppContainerSid,
                /// <summary>
                /// Valore DWORD del numero dell'app container del token.
                /// </summary>
                TokenAppContainerNumber,
                /// <summary>
                /// Struttura che contiene i user claims associati al token.
                /// </summary>
                TokenUserClaimAttributes,
                /// <summary>
                /// Struttura che contiene i device claims associati al token.
                /// </summary>
                TokenDeviceClaimAttributes,
                /// <summary>
                /// Riservato.
                /// </summary>
                TokenRestrictedUserClaimAttributes,
                /// <summary>
                /// Riservato.
                /// </summary>
                TokenRestrictedDeviceClaimAttributes,
                /// <summary>
                /// Struttura che contiene i gruppi di dispositivi associati con il token.
                /// </summary>
                TokenDeviceGroups,
                /// <summary>
                /// Struttura che contiene i gruppi limitati di dispositivi associati con il token.
                /// </summary>
                TokenRestrictedDeviceGroups,
                /// <summary>
                /// Riservato.
                /// </summary>
                TokenSecurityAttributes,
                /// <summary>
                /// Riservato.
                /// </summary>
                TokenIsRestricted,
                /// <summary>
                /// 
                /// </summary>
                TokenProcessTrustLevel,
                /// <summary>
                /// 
                /// </summary>
                TokenPrivateNameSpace,
                /// <summary>
                /// 
                /// </summary>
                TokenSingletonAttributes,
                /// <summary>
                /// 
                /// </summary>
                TokenBnoIsolation,
                /// <summary>
                /// 
                /// </summary>
                TokenChildProcessFlags,
                /// <summary>
                /// 
                /// </summary>
                TokenIsLessPrivilegedAppContainer,
                /// <summary>
                /// 
                /// </summary>
                TokenIsSandboxed,
                /// <summary>
                /// 
                /// </summary>
                TokenOriginatingProcessTrustLevel,
                /// <summary>
                /// Valore massimo per l'enumerazione.
                /// </summary>
                MaxTokenInfoClass
            }

            /// <summary>
            /// Tipo di token di accesso.
            /// </summary>
            public enum TokenType
            {
                /// <summary>
                /// Token primario.
                /// </summary>
                TokenPrimary = 1,
                /// <summary>
                /// Token di impersonazione.
                /// </summary>
                TokenImpersonation
            }

            /// <summary>
            /// Specifica la politica di integrità obbligatoria per un token.
            /// </summary>
            public enum TokenMandatoryPolicy
            {
                /// <summary>
                /// Nessuna politica di integrità obbligatoria.
                /// </summary>
                Off,
                /// <summary>
                /// Un processo associato con il token non può scrivere oggetti con un livello di integrità obbligatoria superiore.
                /// </summary>
                NoWriteUp,
                /// <summary>
                /// Un processo creato con il token ha un livello di integrità minore del processo padre e del file eseguibile.
                /// </summary>
                NewProcessMin,
                /// <summary>
                /// Una combinazione di <see cref="NoWriteUp"/> e <see cref="NewProcessMin"/>.
                /// </summary>
                ValidMask = NoWriteUp | NewProcessMin
            }
            #endregion
            #region User Account Functions Enumerations
            /// <summary>
            /// Tipi di dati recuperabili dagli account utente.
            /// </summary>
            public enum UserInfoDataType : uint
            {
                /// <summary>
                /// I dati presenti nella struttura <see cref="USER_INFO_0"/>.
                /// </summary>
                UserAccountNames,
                /// <summary>
                /// I dati presenti nella struttura <see cref="USER_INFO_1"/>.
                /// </summary>
                UserAccountDetailedInfo,
                /// <summary>
                /// I dati presenti nella struttura <see cref="USER_INFO_2"/>.
                /// </summary>
                UserAccountDetailedInfo2,
                /// <summary>
                /// I dati presenti nella struttura <see cref="USER_INFO_3"/>.
                /// </summary>
                UserAccountDetailedInfo3,
                /// <summary>
                /// I dati presenti nella struttura <see cref="USER_INFO_10"/>.
                /// </summary>
                UserAccountNamesAndComments = 10,
                /// <summary>
                /// I dati presenti nella struttura <see cref="USER_INFO_11"/>.
                /// </summary>
                UserAccountDetailedInfo4,
                /// <summary>
                /// I dati presenti nella struttura <see cref="USER_INFO_20"/>.
                /// </summary>
                UserAccountNameAndAttributes = 20
            }
            #endregion
            #region Security Enumerations
            /// <summary>
            /// Livello di sicurezza per l'impersonazione.
            /// </summary>
            public enum SecurityImpersonationLevel
            {
                /// <summary>
                /// Il processo server non può ottenere informazioni di identificazione sul client e non lo può impersonare.
                /// </summary>
                SecurityAnonymous,
                /// <summary>
                /// Il processo server può ottenere informazioni sul client, come SID e privilegi, ma non lo può impersonare.
                /// </summary>
                /// <remarks>Questo livello è utile per server che esportano i propri oggetti.</remarks>
                SecurityIdentification,
                /// <summary>
                /// Il processo server può impersonare il contesto di sicurezza del client nel sistema locale, il server non può impersonare il client in un sistema remoto.
                /// </summary>
                SecurityImpersonation,
                /// <summary>
                /// Il processo server può impersonare il contesto di sicurezza del client in un sistema remoto.
                /// </summary>
                SecurityDelegation
            }

            /// <summary>
            /// Tipo di valore di un attributo di sicurezza di una richiesta.
            /// </summary>
            public enum SecurityAttributesValuesType : ushort
            {
                /// <summary>
                /// Il valore è un intero a 64 bit.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_TYPE_INT64 = 0x0001,
                /// <summary>
                /// Il valore è un intero senza segno a 64 bit.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_TYPE_UINT64,
                /// <summary>
                /// Il valore è una stringa Unicode.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_TYPE_STRING,
                /// <summary>
                /// Il valore è una struttura <see cref="Win32Structures.CLAIM_SECURITY_ATTRIBUTE_FQBN_VALUE"/>.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_TYPE_FQBN,
                /// <summary>
                /// Il valore è una struttura <see cref="Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE"/> che rappresenta un SID.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_TYPE_SID,
                /// <summary>
                /// Il valore è un intero a 64 bit che indica un valore logico TRUE (1) o FALSE (0).
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_TYPE_BOOLEAN,
                /// <summary>
                /// Il valore è una struttura <see cref="Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE"/>.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_TYPE_OCTET_STRING = 0x0010
            }

            /// <summary>
            /// Possibili valori dei bit dal 0 al 15 del campo <see cref="Win32Structures.CLAIM_SECURITY_ATTRIBUTE_V1.Flags"/>.
            /// </summary>
            [Flags]
            public enum SecurityAttributesFlags
            {
                /// <summary>
                /// L'attributo viene ignorato dal sistema operativo e non viene ereditato dai processi figli.
                /// </summary>
                CLAIM_SECURITY_ATTIBUTE_NON_INHERITABLE = 0x0001,
                /// <summary>
                /// Il valore dell'attributo è case sensitive.
                /// </summary>
                /// <remarks>Questo valore è valido solo per tipi stringa.</remarks>
                CLAIM_SECURITY_ATTRIBUTE_VALUE_CASE_SENSITIVE,
                /// <summary>
                /// L'attributo è utilizzato soltanto per ACE di accesso negato.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_USE_FOR_DENY_ONLY = 0x0004,
                /// <summary>
                /// L'attributo è disabilitato di default.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_DISABLED_BY_DEFAULT = 0x0008,
                /// <summary>
                /// L'attributo è disabilitato.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_DISABLED = 0x0010,
                /// <summary>
                /// L'attributo è obbligatorio.
                /// </summary>
                CLAIM_SECURITY_ATTRIBUTE_MANDATORY = 0x0020
            }

            /// <summary>
            /// Modo in cui i diritti di accesso di una ACE vengono applicati.
            /// </summary>
            public enum AccessMode
            {
                /// <summary>
                /// Non usato.
                /// </summary>
                NOT_USED_ACCESS,
                /// <summary>
                /// Accesso consentito.
                /// </summary>
                GRANT_ACCESS,
                /// <summary>
                /// Accesso consentito, usato per l'impostazione di diritti di accesso.
                /// </summary>
                /// <remarks>Dato in input causa la sostituzione delle informazioni di accesso attualmente presenti.</remarks>
                SET_ACCESS,
                /// <summary>
                /// Accesso negato.
                /// </summary>
                /// <remarks>Dato in input causa l'unione delle nuove informazioni con quelle presenti.</remarks>
                DENY_ACCESS,
                /// <summary>
                /// Tutti i diritti di accesso concessi vengono rimossi.
                /// </summary>
                REVOKE_ACCESS,
                /// <summary>
                /// Notifica quando l'utilizzo di un diritto di accesso è riuscito.
                /// </summary>
                /// <remarks>Dato in input causa l'unione delle nuove informazioni con quelle presenti.</remarks>
                SET_AUDIT_SUCCESS,
                /// <summary>
                /// Notifica quando l'utilizzo di un diritto di accesso è fallito.
                /// </summary>
                /// <remarks>Dato in input causa l'unione delle nuove informazioni con quelle presenti.</remarks>
                SET_AUDIT_FAILURE
            }

            /// <summary>
            /// Indica se una struttura <see cref="Win32Structures.TRUSTEE"/> è di impersonazione o meno.
            /// </summary>
            public enum MultipleTrusteeOperation
            {
                /// <summary>
                /// Non è un trustee di impersonazione.
                /// </summary>
                NoMultipleTrustee,
                /// <summary>
                /// E' un trustee di impersonazione.
                /// </summary>
                TrusteeIsImpersonate
            }

            /// <summary>
            /// Tipo di dato del membro <see cref="Win32Structures.TRUSTEE.Name"/>.
            /// </summary>
            public enum TrusteeForm
            {
                /// <summary>
                /// Puntatore a un SID.
                /// </summary>
                SID,
                /// <summary>
                /// Stringa.
                /// </summary>
                Name,
                /// <summary>
                /// Non valido.
                /// </summary>
                Bad,
                /// <summary>
                /// Puntatore a una struttura <see cref="Win32Structures.OBJECTS_AND_SID"/>.
                /// </summary>
                ObjectsAndSID,
                /// <summary>
                /// Puntatore a un struttura <see cref="Win32Structures.OBJECTS_AND_NAME"/>.
                /// </summary>
                ObjectsAndName
            }

            /// <summary>
            /// Tipo di trustee.
            /// </summary>
            public enum TrusteeType
            {
                /// <summary>
                /// Sconosciuto, ma può essere valido.
                /// </summary>
                Unknown,
                /// <summary>
                /// Utente.
                /// </summary>
                User,
                /// <summary>
                /// Gruppo.
                /// </summary>
                Group,
                /// <summary>
                /// Dominio.
                /// </summary>
                Domain,
                /// <summary>
                /// Alias.
                /// </summary>
                Alias,
                /// <summary>
                /// Gruppo noto.
                /// </summary>
                WellKnownGroup,
                /// <summary>
                /// Account eliminato.
                /// </summary>
                Deleted,
                /// <summary>
                /// Non valido.
                /// </summary>
                Invalid,
                /// <summary>
                /// Computer.
                /// </summary>
                Computer
            }

            /// <summary>
            /// Indica se i campi di tipo <see cref="Guid"/> delle strutture <see cref="Win32Structures.OBJECTS_AND_SID"/> e <see cref="Win32Structures.OBJECTS_AND_NAME"/> hanno un valore.
            /// </summary>
            [Flags]
            public enum ObjectsPresent
            {
                /// <summary>
                /// Il campo <see cref="Win32Structures.OBJECTS_AND_SID.ObjectTypeGUID"/> ha valore.
                /// </summary>
                ACE_OBJECT_TYPE_PRESENT = 0x1,
                /// <summary>
                /// Il campo <see cref="Win32Structures.OBJECTS_AND_SID.InheritedObjectTypeGUID"/> ha valore.
                /// </summary>
                ACE_INHERITED_OBJECT_TYPE_PRESENT
            }

            /// <summary>
            /// Tipi di oggetti che supportano impostazioni di sicurezza.
            /// </summary>
            public enum ObjectType
            {
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                Unknown,
                /// <summary>
                /// File o directory.
                /// </summary>
                File,
                /// <summary>
                /// Servizio.
                /// </summary>
                Service,
                /// <summary>
                /// Stampante.
                /// </summary>
                Printer,
                /// <summary>
                /// Chiave di registro.
                /// </summary>
                RegistryKey,
                /// <summary>
                /// Unità di rete.
                /// </summary>
                LmShare,
                /// <summary>
                /// Oggetto kernel locale.
                /// </summary>
                KernelObject,
                /// <summary>
                /// Window station o un oggetto desktop locale.
                /// </summary>
                WindowObject,
                /// <summary>
                /// Oggetto directory service, set di proprietà oppure una singola proprietà di un oggetto directory service.
                /// </summary>
                DSObject,
                /// <summary>
                /// Oggetto directory service e tutti i suoi set di proprietà e singole proprietà.
                /// </summary>
                DSObjectAll,
                /// <summary>
                /// Oggetto definito dal provider.
                /// </summary>
                ProviderDefined,
                /// <summary>
                /// Oggetto WMI.
                /// </summary>
                WMIGUID,
                /// <summary>
                /// Oggetto rappresentante una voce di registro nell'ambiente WOW64.
                /// </summary>
                RegistryWOW64_32Key,
                /// <summary>
                /// Oggetto rappresentante una voce di registro.
                /// </summary>
                RegistryWOW64_64Key
            }

            /// <summary>
            /// Opzioni di ereditarietà di un ACE.
            /// </summary>
            [Flags]
            public enum ACEInheritance
            {
                /// <summary>
                /// Gli altri contenitori contenuti dall'oggetto primario ereditano l'ACE.
                /// </summary>
                CONTAINER_INHERIT_ACE = 0x2,
                /// <summary>
                /// Eredita ma non si propaga.
                /// </summary>
                INHERIT_NO_PROPAGATE = 0x4,
                /// <summary>
                /// Eredita soltanto.
                /// </summary>
                INHERIT_ONLY = 0x8,
                /// <summary>
                /// L'ACE non si applica all'oggetto primario a cui l'ACL è associata ma gli oggetti contenuti dall'oggetto primario ereditano l'ACE.
                /// </summary>
                INHERIT_ONLY_ACE = INHERIT_ONLY,
                /// <summary>
                /// Nessuna ereditarietà.
                /// </summary>
                NO_INHERITANCE = 0x0,
                /// <summary>
                /// Le impostazioni <see cref="OBJECT_INHERIT_ACE"/> e <see cref="CONTAINER_INHERIT_ACE"/> non sono propagate a un ACE ereditato.
                /// </summary>
                NO_PROPAGATE_INHERIT_ACE = INHERIT_NO_PROPAGATE,
                /// <summary>
                /// Oggetti non container contenuti dall'oggetto primario ereditano l'ACE.
                /// </summary>
                OBJECT_INHERIT_ACE = 0x1,
                /// <summary>
                /// Sia gli oggetti contenitori che quelli non contenitori che sono contenuti dall'oggetto primario eredita l'ACE
                /// </summary>
                /// <remarks>Questa opzione è una combinazione di <see cref="CONTAINER_INHERIT_ACE"/> e <see cref="OBJECT_INHERIT_ACE"/>.</remarks>
                SUB_CONTAINERS_AND_OBJECTS_INHERIT = CONTAINER_INHERIT_ACE | OBJECT_INHERIT_ACE,
                /// <summary>
                /// Altri contenitori contenuti dall'oggetto primario ereditano l'ACE.
                /// </summary>
                SUB_CONTAINERS_ONLY_INHERIT = CONTAINER_INHERIT_ACE,
                /// <summary>
                /// Gli oggetti non contenitori contenuti dall'oggetto primario ereditano l'ACE.
                /// </summary>
                SUB_OBJECTS_ONLY_INHERIT = OBJECT_INHERIT_ACE
            }

            /// <summary>
            /// Rappresenta le informazioni di sicurezza di un oggetto in corso di impostazione o da cui si stanno richiedendo informazioni.
            /// </summary>
            [Flags]
            public enum SecurityInformations : uint
            {

                AttributeSecurityInformation = 0x00000020,

                BackupSecurityInformation = 0x00010000,

                DACLSecurityInformation = 0x00000004,

                GroupSecurityInformation = 0x00000002,

                LabelSecurityInformation = 0x00000010,

                OwnerSecurityInformation = 0x00000001,

                ProtectedDACLSecurityInformation = 0x80000000,

                ProtectedSACLSecurityInformation = 0x40000000,

                SACLSecurityInformation = 0x00000008,

                ScopeSecurityInformation = 0x00000040,

                UnprotectedDACLSecurityInformation = 0x20000000,

                UnprotectedSACLSecurityInformation = 0x10000000
            }

            /// <summary>
            /// Informazioni recuperabili di una ACL.
            /// </summary>
            public enum ACLInformationClass : uint
            {
                /// <summary>
                /// Revisione dell'ACL.
                /// </summary>
                ACLRevisionInformation = 1,
                /// <summary>
                /// Dimensione dell'ACL.
                /// </summary>
                ACLSizeInformation
            }

            /// <summary>
            /// Valori che possono essere inclusi in una maschera di bit che definisce i permessi per un token di accesso.
            /// </summary>
            [Flags]
            public enum AccessMaskToken : uint
            {
                DELETE = GenericObjectAccessRights.DELETE,
                READ_CONTROL = GenericObjectAccessRights.READ_CONTROL,
                WRITE_DAC = GenericObjectAccessRights.WRITE_DAC,
                WRITE_OWNER = GenericObjectAccessRights.WRITE_OWNER,
                SYNCHRONIZE = GenericObjectAccessRights.SYNCHRONIZE,
                STANDARD_RIGHTS_REQUIRED = StandardAccessRights.STANDARD_RIGHTS_REQUIRED,
                STANDARD_RIGHTS_READ = READ_CONTROL,
                STANDARD_RIGHTS_WRITE = READ_CONTROL,
                STANDARD_RIGHTS_EXECUTE = READ_CONTROL,
                STANDARD_RIGHTS_ALL = StandardAccessRights.STANDARD_RIGHTS_ALL,
                SPECIFIC_RIGHTS_ALL = StandardAccessRights.SPECIFIC_RIGHTS_ALL,
                ACCESS_SYSTEM_SECURITY = GenericObjectAccessRights.ACCESS_SYSTEM_SECURITY,
                MAXIMUM_ALLOWED = 0x02000000,
                GENERIC_READ = 0x80000000,
                GENERIC_WRITE = 0x40000000,
                GENERIC_EXECUTE = 0x20000000,
                GENERIC_ALL = 0x10000000,
                TOKEN_ADJUST_DEFAULT = TokenAccessRights.TOKEN_ADJUST_DEFAULT,
                TOKEN_ADJUST_GROUPS = TokenAccessRights.TOKEN_ADJUST_GROUPS,
                TOKEN_ADJUST_PRIVILEGES = TokenAccessRights.TOKEN_ADJUST_PRIVILEGES,
                TOKEN_ADJUST_SESSIONID = TokenAccessRights.TOKEN_ADJUST_SESSIONID,
                TOKEN_ASSIGN_PRIMARY = TokenAccessRights.TOKEN_ASSIGN_PRIMARY,
                TOKEN_DUPLICATE = TokenAccessRights.TOKEN_DUPLICATE,
                TOKEN_EXECUTE = TokenAccessRights.TOKEN_EXECUTE,
                TOKEN_IMPERSONATE = TokenAccessRights.TOKEN_IMPERSONATE,
                TOKEN_QUERY = TokenAccessRights.TOKEN_QUERY,
                TOKEN_QUERY_SOURCE = TokenAccessRights.TOKEN_QUERY_SOURCE,
                TOKEN_READ = TokenAccessRights.TOKEN_READ,
                TOKEN_WRITE = TokenAccessRights.TOKEN_WRITE,
                TOKEN_ALL_ACCESS = TokenAccessRights.TOKEN_ALL_ACCESS
            }
            #endregion
            #region Memory Enumerations
            /// <summary>
            /// Stato delle pagine di memoria in una regione.
            /// </summary>
            public enum MemoryPageState : uint
            {
                /// <summary>
                /// Indica pagine mappate su un dispositivo fisico su cui sono allocate (memoria o file di paging).
                /// </summary>
                MEM_COMMIT = 0x1000,
                /// <summary>
                /// Indica pagine libere non accessibili al processo chiamante e disponibili per l'allocazione.<br/>
                /// Per questo tipo di pagine i campi <see cref="Win32Structures.MEMORY_BASIC_INFORMATION.AllocationBase"/>, <see cref="Win32Structures.MEMORY_BASIC_INFORMATION.AllocationProtect"/>, <see cref="Win32Structures.MEMORY_BASIC_INFORMATION.Protect"/> e <see cref="Win32Structures.MEMORY_BASIC_INFORMATION.Type"/> non sono definiti.
                /// </summary>
                MEM_FREE = 0x10000,
                /// <summary>
                /// Indica pagine riservate per le quali non è stato allocato spazio su un dispositivo fisico.<br/>
                /// Per questo tipo di pagine il campo <see cref="Win32Structures.MEMORY_BASIC_INFORMATION.Protect"/> non è definito.
                /// </summary>
                MEM_RESERVE = 0x2000
            }

            /// <summary>
            /// Tipo di pagina di memoria.
            /// </summary>
            public enum MemoryPageType : uint
            {
                /// <summary>
                /// Indica se le pagine nella regione sono mappate all'interno della vista della sezione di un immagine.
                /// </summary>
                MEM_IMAGE = 0x1000000,
                /// <summary>
                /// Indica se le pagine nella regione sono mappate all'interno della vista di una sezione.
                /// </summary>
                MEM_MAPPED = 0x40000,
                /// <summary>
                /// Indica se le pagine nella regione sono private (cioè non condivise con altri processi).
                /// </summary>
                MEM_PRIVATE = 0x20000
            }

            /// <summary>
            /// Protezione di una pagina di memoria.
            /// </summary>
            [Flags]
            public enum MemoryProtections : uint
            {
                /// <summary>
                /// Il chiamante non ha accesso all'informazione.
                /// </summary>
                NO_ACCESS = 0,
                /// <summary>
                /// Abilita l'accesso in esecuzione a una regione di pagine mappata, un tentativo di scrittura causerà una violazione di accesso, la funzione CreateFileMapping non supporta questa costante.
                /// </summary>
                PAGE_EXECUTE = 0x10,
                /// <summary>
                /// Abilita l'accesso in esecuzione o in sola lettura a una regione di pagine mappata, un tentativo di scrittura causerà una violazione di accesso, la funzione CreateFileMapping non supporta questa costante.
                /// </summary>
                PAGE_EXECUTE_READ = 0x20,
                /// <summary>
                /// Abilita l'accesso in esecuzione, in sola lettura o in lettura e scrittura a una regione di pagine mappata.
                /// </summary>
                PAGE_EXECUTE_READWRITE = 0x40,
                /// <summary>
                /// Abilita l'accesso in esecuzione, in sola lettura o in copy-on-write a una vista di un oggetto di file mapping, un tentativo di scrittura su una pagina copy-on-write causa la creazione di un copia privata al processo.<br/>
                /// La pagina privata viene indicata come <see cref="PAGE_EXECUTE_READWRITE"/> e i cambiamenti sono scritti sulla nuova pagina.
                /// </summary>
                PAGE_EXECUTE_WRITECOPY = 0x80,
                /// <summary>
                /// Disabilita tutti i tipi di accesso a una regione mappata di pagine, un tentativo di lettura, scrittura o esecuzione causa una violazione di accesso, la funzione CreateFileMapping non supporta questa costante.
                /// </summary>
                PAGE_NOACCESS = 0x01,
                /// <summary>
                /// Abilita accesso in lettura a una regione mappata di pagine, un tentativo di scrittura causa una violazione di accesso, se DEP è attiva un tentativo di eseguire codice causerà una violazione di accesso.
                /// </summary>
                PAGE_READONLY = 0x02,
                /// <summary>
                /// Abilita l'accesso in lettura/scrittura a una regione mappata di pagine, se DEP è attiva un tentativo di esecuzione di codice causerà una violazione di accesso.
                /// </summary>
                PAGE_READWRITE = 0x04,
                /// <summary>
                /// Abilita l'accesso in sola lettura o in copy-on-write a una vista mappata di un oggetto di file mapping, un tentativo di scrittura crea una copia di una pagine privata al processo.<br/>
                /// La pagina viene indicata come <see cref="PAGE_READWRITE"/> e i cambiamenti sono scritti sulla nuova pagina.<br/>
                /// Se la DEP è attiva un tentativo di eseguire codice nella regione causerà una violazione di accesso.<br/>
                /// Le funzioni VirtualAlloc e VirtualAllocEx non supportano questa costante.
                /// </summary>
                PAGE_WRITECOPY = 0x08,
                /// <summary>
                /// Imposta tutte le posizioni nelle pagine con non valide per CFG.<br/>
                /// Da usare insieme ad altre protezioni come <see cref="PAGE_EXECUTE"/>, <see cref="PAGE_EXECUTE_READ"/>, <see cref="PAGE_EXECUTE_READWRITE"/> e <see cref="PAGE_EXECUTE_WRITECOPY"/>.<br/>
                /// Qualunque chiamata indiretta a posizioni nelle pagine fallirà i controlli CFG e il processo verrà terminato, il comportamento di default per pagine eseguibili allocate come valide per CFG.<br/>
                /// Le funzioni VirtualProtect e CreateFileMapping non supportano questa costante.
                /// </summary>
                PAGE_TARGETS_INVALID = 0x40000000,
                /// <summary>
                /// Le informazioni CFG delle pagine della regione non verranno aggiornate durante il cambiamento della protezione effettuato dalla funzione VirtualProtect.<br/>
                /// Questa costante è valida solo quando la protezione di una pagina eseguibile assume il valore <see cref="PAGE_EXECUTE"/>, <see cref="PAGE_EXECUTE_READ"/>, <see cref="PAGE_EXECUTE_READWRITE"/>, <see cref="PAGE_EXECUTE_WRITECOPY"/>.<br/>
                /// Il comportamento di default per la protezione assegnata alle pagine eseguibili dalla funzione VirtualProtect è di impostare tutte le posizioni come valide per CFG.
                /// </summary>
                PAGE_TARGETS_NO_UPDATE = PAGE_TARGETS_INVALID,
                /// <summary>
                /// Le pagine nella regione diventano guard pages, un tentativo di accesso a questo tipo di pagine causa un eccezione di tipo STATUS_GUARD_PAGE_VIOLATION e la disabilitazione dello stato di guard page.<br/>
                /// Quando un tentativo di accesso disattiva lo stato di page guard la protezione impostata su di essa viene attivata.<br/>
                /// Se l'eccezione avviene in un servizio di sistema esso tipicamente restituisce uno stato di errore.<br/>
                /// Questa costante non può essere utilizzata con <see cref="PAGE_NOACCESS"/> e non è supportata dalla funzione CreateFileMapping.
                /// </summary>
                PAGE_GUARD = 0x100,
                /// <summary>
                /// Imposta tutte le pagine come non-cachable, le applicazioni non dovrebbero usare questa costante a meno che non sia espliciamente richiesto per un dispositivo.<br/>
                /// Questa constante non può essere utilizzata insieme a <see cref="PAGE_GUARD"/>, <see cref="PAGE_NOACCESS"/> o <see cref="PAGE_WRITECOMBINE"/>.<br/>
                /// Questa costante può essere usata solo durante l'allocazione di memoria tramite le funzioni VirtualAlloc, VirtualAllocEx e VirtualAllocExNuma.<br/>
                /// Per abilitare accesso non-cached per la memoria condivisa specificare SEC_NOCACHE durante la chiamata alla funzione CreateFileMapping.
                /// </summary>
                PAGE_NOCACHE = 0x200,
                /// <summary>
                /// Imposta tutte le pagine come write-combined, le applicazioni non dovrebbero usare questa costante a meno che non sia espliciamente richiesto per un dispositivo.<br/>
                /// Questa costante non può essere utilizzata insieme a <see cref="PAGE_NOACCESS"/>, <see cref="PAGE_GUARD"/> e <see cref="PAGE_NOCACHE"/>.<br/>
                /// Questa costante può essere usata solo durante l'allocazione di memoria tramite le funzioni VirtualAlloc, VirtualAllocEx e VirtualAllocExNuma.<br/>
                /// Per abilitare accesso write-combined per la memoria condivisa specificare SEC_WRITECOMBINE durante la chiamata alla funzione CreateFileMapping.
                /// </summary>
                PAGE_WRITECOMBINE = 0x400,
                /// <summary>
                /// La pagine contiene un struttura di controllo thread (TCS).
                /// </summary>
                PAGE_ENCLAVE_THREAD_CONTROL = 0x80000000,
                /// <summary>
                /// I contenuti della pagina forniti sono esclusi dalla misurazione dell'istruzione EEXTEND del modello di programmazione Intel SGX.
                /// </summary>
                PAGE_ENCLAVE_UNVALIDATED = 0x20000000
            }

            /// <summary>
            /// Tipo di operazione eseguita da VirtualFreeEx.
            /// </summary>
            public enum FreeOperationType : uint
            {
                /// <summary>
                /// Annulla la mappatura di una regione di pagine, la regione passa allo stato riservato.
                /// </summary>
                MEM_DECOMMIT = 0x00004000,
                /// <summary>
                /// Rilascia la regione di pagine, la regione passa allo stato libero.
                /// </summary>
                MEM_RELEASE = 0x00008000,
                /// <summary>
                /// 
                /// </summary>
                MEM_COALESCE_PLACEHOLDERS = 0x00000001,
                /// <summary>
                /// 
                /// </summary>
                MEM_PRESERVE_PLACEHOLDER = 0x00000002
            }

            /// <summary>
            /// Opzioni per il controllo del working set di un processo.
            /// </summary>
            [Flags]
            public enum ProcessWorkingSetQuotaLimitsOptions : uint
            {
                /// <summary>
                /// Il working set del processo può avere un valore più basso di quello indicato.
                /// </summary>
                /// <remarks>Questa opzione non può essere usata insieme a <see cref="QUOTA_LIMITS_HARDWS_MIN_ENABLE"/>.</remarks>
                QUOTA_LIMITS_HARDWS_MIN_DISABLE = 0x00000002,
                /// <summary>
                /// Il working set del processo non può avere un valore più basso di quello indicato.
                /// </summary>
                /// <remarks>Questa opzione non può essere usata insieme a <see cref="QUOTA_LIMITS_HARDWS_MIN_DISABLE"/>.</remarks>
                QUOTA_LIMITS_HARDWS_MIN_ENABLE = 0x00000001,
                /// <summary>
                /// Il working set del processo può avere un valore più alto di quello indicato.
                /// </summary>
                /// <remarks>Questa opzione non può essere usata insieme a <see cref="QUOTA_LIMITS_HARDWS_MAX_ENABLE"/>.</remarks>
                QUOTA_LIMITS_HARDWS_MAX_DISABLE = 0x00000008,
                /// <summary>
                /// Il working set del processo non può avere un valore più alto di quello indicato.
                /// </summary>
                /// <remarks>Questa opzione non può essere usata insieme a <see cref="QUOTA_LIMITS_HARDWS_MAX_DISABLE"/>.</remarks>
                QUOTA_LIMITS_HARDWS_MAX_ENABLE = 0x00000004
            }
            #endregion
            #region File And Directory Enumerations
            /// <summary>
            /// Opzioni per la condivisione di un file.
            /// </summary>
            [Flags]
            public enum FileShareOptions : uint
            {
                /// <summary>
                /// Nessuna condivisione.
                /// </summary>
                FILE_SHARE_NONE,
                /// <summary>
                /// Permette l'eliminazione e la ridenominazione.
                /// </summary>
                FILE_SHARE_DELETE = 0x00000004,
                /// <summary>
                /// Permette l'apertura di un file per la lettura.
                /// </summary>
                FILE_SHARE_READ = 0x00000001,
                /// <summary>
                /// Permette l'apertura di un file per la scrittura.
                /// </summary>
                FILE_SHARE_WRITE = 0x00000002
            }

            /// <summary>
            /// Azione da intraprendere su un file.
            /// </summary>
            public enum FileCreationDisposition
            {
                /// <summary>
                /// Crea sempre un nuovo file.
                /// </summary>
                /// <remarks>Se il file esiste ed è scrivibile il file viene sovrascritto, se il file non esiste e il percorso è valido, viene creato un nuovo file.</remarks>
                CREATE_ALWAYS = 2,
                /// <summary>
                /// Crea un nuovo file se già non esiste.
                /// </summary>
                /// <remarks>Se il file esiste questa opzione non può essere usata, altrimenti, se il percorso è valido, viene creato un nuovo file.</remarks>
                CREATE_NEW = 1,
                /// <summary>
                /// Apre sempre un file.
                /// </summary>
                /// <remarks>Se il file non esiste e il percorso è valido viene creato un nuovo file.</remarks>
                OPEN_ALWAYS = 4,
                /// <summary>
                /// Apre un file o un dispositivo.
                /// </summary>
                OPEN_EXISTING = 3,
                /// <summary>
                /// Apre e tronca un file così che la sua dimensione sia azzerata.
                /// </summary>
                /// <remarks>Il processo deve avere un handle con il permesso <see cref="GenericAccessRights.GENERIC_WRITE"/>.</remarks>
                TRUNCATE_EXISTING = 5
            }

            /// <summary>
            /// Attributi e altre opzioni applicabili a un handle di file.
            /// </summary>
            [Flags]
            public enum FileAttributesSQOSInfoAndFlags : uint
            {
                #region File Attributes
                /// <summary>
                /// Il file dovrebbe essere archiviato.
                /// </summary>
                FILE_ATTRIBUTE_ARCHIVE = 0x20,
                /// <summary>
                /// Il file o la directory è cifrato.
                /// </summary>
                /// <remarks>Questo attibuto non ha effetto se <see cref="FILE_ATTRIBUTE_SYSTEM"/> è specificato.</remarks>
                FILE_ATTRIBUTE_ENCRYPTED = 0x4000,
                /// <summary>
                /// Il file è nascosto.
                /// </summary>
                FILE_ATTRIBUTE_HIDDEN = 0x2,
                /// <summary>
                /// Il file non ha altri attributi impostati.
                /// </summary>
                /// <remarks>Questo attributo è valido solo se usato da solo.</remarks>
                FILE_ATTRIBUTE_NORMAL = 0x80,
                /// <summary>
                /// I dati del file non sono immediatamente disponibili.
                /// </summary>
                /// <remarks>Questo attributo indica che i dati del file si trova in uno storage offline.<br/>
                /// Questo attributo è usato da Remote Storage, le applicazione non dovrebbero cambiarlo.</remarks>
                FILE_ATTRIBUTE_OFFLINE = 0x1000,
                /// <summary>
                /// Il file è di sola lettura.
                /// </summary>
                FILE_ATTRIBUTE_READONLY = 0x1,
                /// <summary>
                /// Il file è parte o è usato esclusivamente dal sistema operativo.
                /// </summary>
                FILE_ATTRIBUTE_SYSTEM = 0x4,
                /// <summary>
                /// Il file viene usato come storage temporaneo.
                /// </summary>
                FILE_ATTRIBUTE_TEMPORARY = 0x100,
                #endregion
                #region File Flags
                /// <summary>
                /// Il file è in corso di apertura per un'operazione di backup o di ripristino.
                /// </summary>
                /// <remarks>Il processo che tenta l'operazione ignora i controlli di sicurezza se i privilegi SE_BACKUP_NAME e SE_RESTORE_NAME sono attivi.<br/>
                /// Per ottenere un handle a una directory questa opzione è necessaria.</remarks>
                FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
                /// <summary>
                /// Il file deve essere cancellato dopo che tutti gli handle ad esso sono stati chiusi.
                /// </summary>
                /// <remarks>Se ci sono già handle aperti allora essi devono avere l'opzione <see cref="FileShareOptions.FILE_SHARE_DELETE"/> attiva perchè questa opzione possa essere utilizzata.</remarks>
                FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
                /// <summary>
                /// Il sistema non salva nella cache alcuna informazioni per operazioni di lettura e scrittura relative al file.
                /// </summary>
                /// <remarks>Questa opzione non ha effetto sull'uso della cache del disco o file mappati in memoria.</remarks>
                FILE_FLAG_NO_BUFFERING = 0x20000000,
                /// <summary>
                /// I dati del file sono richiesti ma non deve essere spostati dallo storage remoto.
                /// </summary>
                FILE_FLAG_OPEN_NO_RECALL = 0x00100000,
                /// <summary>
                /// L'elaborazione dei collegamenti simbolici non verrà eseguita.
                /// </summary>
                /// <remarks>Questa opzione non può essere usata con <see cref="FileCreationDisposition.CREATE_ALWAYS"/>, se il file non è un collegamento simbolico, questa opzione viene ignorata.</remarks>
                FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
                /// <summary>
                /// Il file o dispositivo è in corso di apertura per un'operazione asincrona.
                /// </summary>
                /// <remarks>Se questa opzione è specificata allora il file è utilizzata per letture e scrittura simultanee, altrimenti le operazione sono serializzate.</remarks>
                FILE_FLAG_OVERLAPPED = 0x40000000,
                /// <summary>
                /// L'accesso al file rispetterà le regole POSIX.
                /// </summary>
                FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
                /// <summary>
                /// L'accesso al file è casuale.
                /// </summary>
                /// <remarks>Il sistema può utilizzare questa opzione per ottimizzare il file caching.<br/>
                /// Questa opzione non ha alcun effetto se il sistema non supporta l'uso della cache per operazioni I/O e se l'opzione <see cref="FILE_FLAG_NO_BUFFERING"/> è attiva.</remarks>
                FILE_FLAG_RANDOM_ACCESS = 0x10000000,
                /// <summary>
                /// Il file o il dispositivo è in corso di apertura ed è a conoscenza della sessione.
                /// </summary>
                /// <remarks>Questa opzione è necessaria se un processo nella sessione 0 deve accedere al file.<br/>
                /// Questa opzione ha effetto solo nelle versioni server di Windows e solo per processi in esecuzione nella sessione 0.</remarks>
                FILE_FLAG_SESSION_AWARE = 0x00800000,
                /// <summary>
                /// L'accesso al file è sequenziale dall'inizio alla fine.
                /// </summary>
                /// <remarks>Il sistema può utilizzare questa opzione per ottimizzare il file caching.<br/>
                /// Questa opzione non ha alcun effetto se il sistema non supporta l'uso della cache per operazioni I/O e se l'opzione <see cref="FILE_FLAG_NO_BUFFERING"/> è attiva.<br/>
                /// Questa opzione non dovrebbe essere usata per letture in direzione opposta.</remarks>
                FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
                /// <summary>
                /// Le operazioni di scrittura non passeranno attraverso nessuna cache intermedia, verranno direttamente eseguite sul disco.
                /// </summary>
                FILE_FLAG_WRITE_THROUGH = 0x80000000,
                #endregion
                #region Security Quality Of Service Info
                /// <summary>
                /// 
                /// </summary>
                SECURITY_ANONYMOUS = SecurityImpersonationLevel.SecurityAnonymous,
                /// <summary>
                /// 
                /// </summary>
                SECURITY_DELEGATION = SecurityImpersonationLevel.SecurityDelegation,
                /// <summary>
                /// 
                /// </summary>
#pragma warning disable CA1069 // I valori di enumerazione non devono essere duplicati
                SECURITY_IDENTIFICATION = SecurityImpersonationLevel.SecurityIdentification,
                /// <summary>
                /// 
                /// </summary>
                SECURITY_IMPERSONATION = SecurityImpersonationLevel.SecurityImpersonation,
                /// <summary>
                /// Indica che sono presenti informazioni SQOS (Security Quality Of Service).
                /// </summary>
                SECURITY_SQOS_PRESENT = 0x00100000
#pragma warning restore CA1069 // I valori di enumerazione non devono essere duplicati
                #endregion
            }

            /// <summary>
            /// Posizione iniziale del puntatore di un file.
            /// </summary>
            public enum FilePointerStartingPosition
            {
                /// <summary>
                /// Inizio del file.
                /// </summary>
                FILE_BEGIN,
                /// <summary>
                /// Posizione attuale.
                /// </summary>
                FILE_CURRENT,
                /// <summary>
                /// Fine del file.
                /// </summary>
                FILE_END
            }

            /// <summary>
            /// Attributi applicabili a un oggetti file mapping.
            /// </summary>
            [Flags]
            public enum FileMappingAttributes : uint
            {
                /// <summary>
                /// Se l'oggetto file mapping si basa sul file di paging del sistema operativo, questo attributo specifica che quando la vista di un file viene mappata nello spazio di indirizzamento di un processo, tutte le pagine devono essere mappate.<br/>
                /// Il sistema deve avere abbastanza pagine mappabili per completare l'operazione.<br/>
                /// Questo attributo non ha effetto per gli oggetti file mapping che si basano su immagini eseguibili e file di dati.<br/>
                /// Questo attributo non può essere combinato con <see cref="SEC_RESERVE"/>.
                /// </summary>
                SEC_COMMIT = 0x8000000,
                /// <summary>
                /// L'handle specificato si riferisce a un'immagine eseguibile.<br/>
                /// Questo attributo deve essere combinato con <see cref="MemoryProtections.PAGE_READONLY"/>, nessun altro attributo può essere combinato.
                /// </summary>
                SEC_IMAGE = 0x1000000,
                /// <summary>
                /// L'handle specificato si riferisce a un'immagine eseguibile che non verrà eseguita e che non saranno eseguiti controlli di integrità, inoltre non verrano invocati i driver callback registrati.<br/>
                /// Questo attributo deve essere combinato con <see cref="MemoryProtections.PAGE_READONLY"/>, non può essere combinato con altri attributi.
                /// </summary>
                SEC_IMAGE_NO_EXECUTE = 0x11000000,
                /// <summary>
                /// Permette l'utilizzo di pagine grandi per oggetti file mapping basati sul file di paging del sistema operativo, la dimensione massima dell'oggetto deve essere un multiplo della dimensione minima di una pagina grande, inoltre gli oggetti creati con questo attributo devono avere come indirizzo di vase e dimensione un multiplo della dimensione minima di una pagine grande.<br/>
                /// Il privilegio SeLockMemoryPrivilege deve essere attivo per utilizzare questo attributo.<br/>
                /// <see cref="SEC_COMMIT"/> deve essere combinato con questo attributo.
                /// </summary>
                SEC_LARGE_PAGES = 0x80000000,
                /// <summary>
                /// Tutte le pagine non saranno inserite nella cache, questo attributo dovrebbe essere utilizzto solo se richiesto dal dispositivo.<br/>
                /// <see cref="SEC_RESERVE"/> oppure <see cref="SEC_COMMIT"/> devono essere combinati con questo.
                /// </summary>
                SEC_NOCACHE = 0x10000000,
                /// <summary>
                /// Se l'oggetto file mapping si basa sul file di paging del sistema operativo, questo attributo specifica che quando la vista di un file viene mappata nello spazio di indirizzamento di un processo, tutte le pagine devono essere riservate.<br/>
                /// Questo attributo non ha effetto per gli oggetti file mapping che si basano su immagini eseguibili e file di dati.<br/>
                /// Questo attributo non può essere combinato con <see cref="SEC_COMMIT"/>.
                /// </summary>
                SEC_RESERVE = 0x4000000,
                /// <summary>
                /// Tutte le pagine sono write-combined, questo attributo dovrebbe essere utlizzato solo se richiesto dal dispositivo.<br/>
                /// <see cref="SEC_RESERVE"/> oppure <see cref="SEC_COMMIT"/> devono essere combinati con questo.
                /// </summary>
                SEC_WRITECOMBINE = 0x40000000
            }

            /// <summary>
            /// Tipo di risultato.
            /// </summary>
            public enum FileResultType
            {
                /// <summary>
                /// Nome del disco normalizzato.
                /// </summary>
                FILE_NAME_NORMALIZED,
                /// <summary>
                /// Nome del file aperto (non normalizzato).
                /// </summary>
                FILE_NAME_OPENED = 0x8,
                /// <summary>
                /// Percorso con la lettera del disco.
                /// </summary>
                VOLUME_NAME_DOS = FILE_NAME_NORMALIZED,
                /// <summary>
                /// Percorso con il GUID del volume al posto della lettera del disco.
                /// </summary>
                VOLUME_NAME_GUID = 0x1,
                /// <summary>
                /// Percorso senza informazioni sul disco.
                /// </summary>
                VOLUME_NAME_NONE = 0x4,
                /// <summary>
                /// Percorso del dispositivo incluso.
                /// </summary>
                VOLUME_NAME_NT = 0x2
            }

            /// <summary>
            /// Informazioni recuperabili da un handle di file o directory, utilizzabili con la funzione GetFileInformationByHandleEx.
            /// </summary>
            public enum FileHandleQueryClass
            {
                FileBasicInfo,
                FileStandardInfo,
                FileNameInfo,
                FileStreamInfo = 7,
                FileCompressionInfo,
                FileAttributeTagInfo,
                FileIdBothDirectoryInfo = 10,
                FileIdBothDirectoryRestartInfo = 11,
                FileRemoteProtocolInfo = 13,
                FileFullDirectoryInfo = 14,
                FileFullDirectoryRestartInfo = 15,
                FileStorageInfo = 16,
                FileAlignmentInfo = 17,
                FileIdInfo = 18,
                FileIdExtdDirectoryInfo = 19,
                FileIdExtdDirectoryRestartInfo = 20
            }

            /// <summary>
            /// Tipo di file, utilizzato dalla funzione <see cref="Win32FileFunctions.GetFileType(IntPtr)"/>.
            /// </summary>
            public enum FileType2 : uint
            {
                /// <summary>
                /// File di caratteri.
                /// </summary>
                Character = 0x0002,
                /// <summary>
                /// File di disco.
                /// </summary>
                Disk = 0x0001,
                /// <summary>
                /// Socket, named pipe, pipe anonimo.
                /// </summary>
                Pipe = 0x0003,
                /// <summary>
                /// Non usato.
                /// </summary>
                Remote = 0x8000,
                /// <summary>
                /// Sconosciuto, indica anche una condizione di errore.
                /// </summary>
                Unknown = 0x0000
            }

            /// <summary>
            /// Livello di informazioni sugli attributi di un file.
            /// </summary>
            public enum FileAttributesInfoLevel
            {
                /// <summary>
                /// Informazioni standard sugli attributi.
                /// </summary>
                GetFileExInfoStandard,
                /// <summary>
                /// Valore massimo.
                /// </summary>
                GetFileExMaxInfoLevel
            }

            /// <summary>
            /// Attributi di un file o una directory.
            /// </summary>
            [Flags]
            public enum FileAttributes : uint
            {
                /// <summary>
                /// Archiviato.
                /// </summary>
                Archive = 32,
                /// <summary>
                /// Compresso.
                /// </summary>
                Compressed = 2048,
                /// <summary>
                /// Riservato.
                /// </summary>
                Device = 64,
                /// <summary>
                /// Directory.
                /// </summary>
                Directory = 16,
                /// <summary>
                /// Cifrato.
                /// </summary>
                Encrypted = 16384,
                /// <summary>
                /// Nascosto.
                /// </summary>
                Hidden = 2,
                /// <summary>
                /// Flusso configurato con integrità.
                /// </summary>
                IntegrityStream = 32768,
                /// <summary>
                /// Normale.
                /// </summary>
                Normal = 128,
                /// <summary>
                /// Escluso dall'indicizzazione.
                /// </summary>
                NotContentIndexed = 8192,
                /// <summary>
                /// Flusso non leggibile da scrubbers.
                /// </summary>
                NoScrubData = 131072,
                /// <summary>
                /// Non immediatamente disponibile.
                /// </summary>
                Offline = 4096,
                /// <summary>
                /// Sola lettura.
                /// </summary>
                /// <remarks>Le directory ignorano questo attributo.</remarks>
                Readonly = 1,
                /// <summary>
                /// Non completamente presente in locale.
                /// </summary>
                /// <remarks>Questo attributo può essere impostato solo in modalità kernel.</remarks>
                RecallOnDataAccess = 4194304,
                /// <summary>
                /// Nessuna rappresentazione fisica sul sistema locale.
                /// </summary>
                RecallOnOpen = 262144,
                /// <summary>
                /// Collegamento simbolico.
                /// </summary>
                ReparsePoint = 1024,
                /// <summary>
                /// File sparso.
                /// </summary>
                SparseFile = 512,
                /// <summary>
                /// File di sistema.
                /// </summary>
                System = 4,
                /// <summary>
                /// File temporaneo.
                /// </summary>
                Temporary = 256,
                /// <summary>
                /// Riservato.
                /// </summary>
                Virtual = 65536
            }
            #endregion
            #region File Version Enumerations
            /// <summary>
            /// Attributi del file.
            /// </summary>
            [Flags]
            public enum FileFlags : uint
            {
                /// <summary>
                /// Il file contiene informazioni di debug o è stato compilato con le funzionalità di debug attiva.
                /// </summary>
                VS_FF_DEBUG = 0x00000001,
                /// <summary>
                /// La struttura con le informazioni di versione è stata creata dinamicamente, alcuni membri potrebbero essere vuoti o errati.
                /// </summary>
                VS_FF_INFOINFERRED = 0x00000010,
                /// <summary>
                /// Il file è stato modificato e non è identico all'originale con lo stesso numero di versione.
                /// </summary>
                VS_FF_PATCHED = 0x00000004,
                /// <summary>
                /// Il file è una versione di sviluppo.
                /// </summary>
                VS_FF_PRERELEASE = 0x00000002,
                /// <summary>
                /// Il file non è stato creato usando la procedura di rilascio standard.
                /// </summary>
                VS_FF_PRIVATEBUILD = 0x00000008,
                /// <summary>
                /// Il file è stato creato dall'azienda usando la procedura di rilascio standard ma rappresenta una variazione del file normale con lo stesso numero di versione.
                /// </summary>
                ES_FF_SPECIALBUILD = 0x00000020
            }

            /// <summary>
            /// Sistema operativo per cui un file è stato progettato.
            /// </summary>
            [Flags]
            public enum FileOS : uint
            {
                /// <summary>
                /// MS-DOS.
                /// </summary>
                VOS_DOS = 0x00010000,
                /// <summary>
                /// Windows NT.
                /// </summary>
                VOS_NT = 0x00040000,
                /// <summary>
                /// Windows 16 bit.
                /// </summary>
                VOS_WINDOWS16 = 0x00000001,
                /// <summary>
                /// Windows 32 bit.
                /// </summary>
                VOS_WINDOWS32 = 0x00000004,
                /// <summary>
                /// OS/2 16 bit.
                /// </summary>
                VOS_OS216 = 0x00020000,
                /// <summary>
                /// OS/2 32 bit.
                /// </summary>
                VOS_OS232 = 0x00030000,
                /// <summary>
                /// Presentation Manager 16 bit.
                /// </summary>
                VOS_PM16 = 0x00000002,
                /// <summary>
                /// Presentation Manager 32 bit.
                /// </summary>
                VOS_PM32 = 0x00000003,
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                VOS_UNKNOWN = 0x00000000
            }

            /// <summary>
            /// Tipo di file.
            /// </summary>
            public enum FileType : uint
            {
                /// <summary>
                /// Applicazione.
                /// </summary>
                VFT_APP = 0x00000001,
                /// <summary>
                /// DLL.
                /// </summary>
                VFT_DLL = 0x00000002,
                /// <summary>
                /// Driver di dispositivo.
                /// </summary>
                VFT_DRV = 0x00000003,
                /// <summary>
                /// Font.
                /// </summary>
                VFT_FONT = 0x00000004,
                /// <summary>
                /// Libreria di collegamento statico.
                /// </summary>
                VFT_STATIC_LIB = 0x00000007,
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                VFT_UNKNOWN = 0x00000000,
                /// <summary>
                /// Dispositivo virtuale.
                /// </summary>
                VFT_VXD = 0x00000005
            }

            /// <summary>
            /// Funzione del file.
            /// </summary>
            public enum FileSubType : uint
            {
                #region Device Driver Type
                /// <summary>
                /// Driver per dispositivo di comunicazione.
                /// </summary>
                VFT2_DRV_COMM = 0x0000000A,
                /// <summary>
                /// Driver per display.
                /// </summary>
                VFT2_DRV_DISPLAY = 0x00000004,
                /// <summary>
                /// Driver installabile.
                /// </summary>
                VFT2_DRV_INSTALLABLE = 0x00000008,
                /// <summary>
                /// Driver per tastiera.
                /// </summary>
                VFT2_DRV_KEYBOARD = 0x00000002,
                /// <summary>
                /// Driver di lingua.
                /// </summary>
                VFT2_DRV_LANGUAGE = 0x00000003,
                /// <summary>
                /// Driver per mouse.
                /// </summary>
                VFT2_DRV_MOUSE = 0x00000005,
                /// <summary>
                /// Driver di rete.
                /// </summary>
                VFT2_DRV_NETWORK = 0x00000006,
                /// <summary>
                /// Driver per stampante.
                /// </summary>
                VFT2_DRV_PRINTER = 0x00000001,
                /// <summary>
                /// Driver per scheda audio.
                /// </summary>
                VFT2_DRV_SOUND = 0x00000009,
                /// <summary>
                /// Driver di sistema.
                /// </summary>
                VFT2_DRV_SYSTEM = 0x00000007,
                /// <summary>
                /// Driver per stampante con versione.
                /// </summary>
                VFT2_DRV_VERSIONED_PRINTER = 0x0000000C,
                #endregion
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                VFT2_UNKNOWN = 0x00000000,
                #region Font Type
                /// <summary>
                /// Font raster.
                /// </summary>
                VFT2_FONT_RASTER = VFT2_DRV_PRINTER,
                /// <summary>
                /// Font TrueType.
                /// </summary>
                VFT2_FONT_TRUETYPE = VFT2_DRV_LANGUAGE,
                /// <summary>
                /// Font vector.
                /// </summary>
                VFT2_FONT_VECTOR = VFT2_DRV_KEYBOARD,
                #endregion
            }
            #endregion
            #region Shell Enumerations
            /// <summary>
            /// Tipo di oggetto contenuto nel parametro ObjectName della funzione SHObjectProperties.
            /// </summary>
            public enum PropertiesObjectType : uint
            {
                /// <summary>
                /// Nome di una stampante.
                /// </summary>
                SHOP_PRINTERNAME = 0x00000001,
                /// <summary>
                /// Percorso completo del file.
                /// </summary>
                SHOP_FILEPATH = 0x00000002,
                /// <summary>
                /// GUID di un volume oppure o una lettera di unità.
                /// </summary>
                SHOP_VOLUMEGUID = 0x00000004
            }
            #endregion
            #region Wait Enumerations
            /// <summary>
            /// Valori di ritorno di una funzione di attesa.
            /// </summary>
            public enum WaitResult : uint
            {
                /// <summary>
                /// L'oggetto mutex non è stato rilasciato dal thread proprietario prima del suo termine.
                /// </summary>
                /// <remarks>Il thread corrente diventa il proprietario dell'oggetto e il suo stato viene impostato come non segnalato.</remarks>
                WAIT_ABANDONED = 0x00000080,
                /// <summary>
                /// L'oggetto è segnalato.
                /// </summary>
                WAIT_OBJECT_0 = 0x00000000,
                /// <summary>
                /// Il tempo è scaduto e l'oggetto non è segnalato.
                /// </summary>
                WAIT_TIMEOUT = 0x00000102,
                /// <summary>
                /// Operazione fallita.
                /// </summary>
                WAIT_FAILED = 0xFFFFFFFF,
                /// <summary>
                /// APC messa in coda al thread.
                /// </summary>
                WAIT_IO_COMPLETION = 0x000000C0
            }
            #endregion
            #region Access Rights Enumerations
            #region Standard And General
            /// <summary>
            /// Diritti di accesso generici.
            /// </summary>
            [Flags]
            public enum GenericObjectAccessRights : uint
            {
                /// <summary>
                /// Eliminazione.
                /// </summary>
                DELETE = 0x00010000,
                /// <summary>
                /// Lettura informazioni del descrittore di sicurezza (escluso SACL).
                /// </summary>
                READ_CONTROL = 0x00020000,
                /// <summary>
                /// Utilizzo per la sincronizzazione.
                /// </summary>
                SYNCHRONIZE = 0x00100000,
                /// <summary>
                /// Modifica del DACL nel descrittore di sicurezza.
                /// </summary>
                WRITE_DAC = 0x00040000,
                /// <summary>
                /// Modifica del proprietario nel descrittore di sicurezza.
                /// </summary>
                WRITE_OWNER = 0x00080000,
                /// <summary>
                /// Accesso al descrittore di sicurezza di sistema.
                /// </summary>
                ACCESS_SYSTEM_SECURITY = 0x01000000
            }

            /// <summary>
            /// Diritti di accesso standard.
            /// </summary>
            [Flags]
            public enum StandardAccessRights : uint
            {
                STANDARD_RIGHTS_REQUIRED =
                    GenericObjectAccessRights.DELETE |
                    GenericObjectAccessRights.READ_CONTROL |
                    GenericObjectAccessRights.WRITE_DAC |
                    GenericObjectAccessRights.WRITE_OWNER,
                STANDARD_RIGHTS_READ = GenericObjectAccessRights.READ_CONTROL,
                STANDARD_RIGHTS_WRITE = STANDARD_RIGHTS_READ,
                STANDARD_RIGHTS_EXECUTE = STANDARD_RIGHTS_READ,
                STANDARD_RIGHTS_ALL = STANDARD_RIGHTS_REQUIRED | GenericObjectAccessRights.SYNCHRONIZE,
                SPECIFIC_RIGHTS_ALL = 0x0000FFFF
            }

            /// <summary>
            /// Diritti di accesso generici.
            /// </summary>
            [Flags]
            public enum GenericAccessRights : uint
            {
                GENERIC_READ = 0x80000000,
                GENERIC_WRITE = 0x40000000,
                GENERIC_EXECUTE = 0x20000000,
                GENERIC_ALL = 0x10000000
            }
            #endregion
            #region Process And Threads
            /// <summary>
            /// Diritti di accesso a un processo.
            /// </summary>
            [Flags]
            public enum ProcessAccessRights : uint
            {
                /// <summary>
                /// Creazione di un processo.
                /// </summary>
                PROCESS_CREATE_PROCESS = 0x0080,
                /// <summary>
                /// Creazione di un thread.
                /// </summary>
                PROCESS_CREATE_THREAD = 0x0002,
                /// <summary>
                /// Duplicazione di un handle.
                /// </summary>
                PROCESS_DUP_HANDLE = 0x0040,
                /// <summary>
                /// Recuperare alcune informazioni sul processo (token, codice d'uscita e priorità).
                /// </summary>
                PROCESS_QUERY_INFORMATION = 0x0400,
                /// <summary>
                /// Recuperare alcune informazioni sul processo (codice di uscita, priorità, se sta eseguendo un job, nome completo dell'eseguibile).
                /// </summary>
                PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
                /// <summary>
                /// Impostare alcune informazioni di un processo (priorità).
                /// </summary>
                PROCESS_SET_INFORMATION = 0x0200,
                /// <summary>
                /// Impostare limiti di memoria.
                /// </summary>
                PROCESS_SET_QUOTA = 0x0100,
                /// <summary>
                /// Sospendere o riprendere un processo.
                /// </summary>
                PROCESS_SUSPEND_RESUME = 0x0800,
                /// <summary>
                /// Terminare un processo.
                /// </summary>
                PROCESS_TERMINATE = 0x0001,
                /// <summary>
                /// Eseguire operazioni nello spazio di indirizzamento di un processo.
                /// </summary>
                PROCESS_VM_OPERATION = 0x0008,
                /// <summary>
                /// Leggere la memoria di un processo.
                /// </summary>
                PROCESS_VM_READ = 0x0010,
                /// <summary>
                /// Scrivere la memoria di un processo.
                /// </summary>
                PROCESS_VM_WRITE = 0x0020,
                /// <summary>
                /// 
                /// </summary>
                PROCESS_SET_LIMITED_INFORMATION = 0x2000,

                PROCESS_SET_SESSIONID = 0x0004,
                /// <summary>
                /// Tutti i possibili diritti di accesso.
                /// </summary>
                PROCESS_ALL_ACCESS =
                    StandardAccessRights.STANDARD_RIGHTS_ALL |
                    StandardAccessRights.SPECIFIC_RIGHTS_ALL
            }

            /// <summary>
            /// Diritti di accesso a un thread.
            /// </summary>
            [Flags]
            public enum ThreadAccessRights : uint
            {
                /// <summary>
                /// Richiesto a un thread server per impersonare un client.
                /// </summary>
                THREAD_DIRECT_IMPERSONATION = 0x0200,
                /// <summary>
                /// Richiesto per leggere il contesto di un thread.
                /// </summary>
                THREAD_GET_CONTEXT = 0x0008,
                /// <summary>
                /// Richiesto per utilizzare le informazioni di sicurezza di un thread senza comunicare con esso attraverso un meccanismo che fornisce servizi di impersonazione.
                /// </summary>
                THREAD_IMPERSONATE = 0x0100,
                /// <summary>
                /// Richiesto per recuperare alcune informazioni da un thread.
                /// </summary>
                THREAD_QUERY_INFORMATION = 0x0040,
                /// <summary>
                /// Richiesto per recuperare alcune informazioni da un thread.
                /// </summary>
                THREAD_QUERY_LIMITED_INFORMATION = 0x0800,
                /// <summary>
                /// Richiesto per scrivere il contesto di un thread.
                /// </summary>
                THREAD_SET_CONTEXT = 0x0010,
                /// <summary>
                /// Richiesto per impostare alcune informazioni di un thread.
                /// </summary>
                THREAD_SET_INFORMATION = 0x0020,
                /// <summary>
                /// Richiesto per impostare alcune informazioni di un thread.
                /// </summary>
                THREAD_SET_LIMITED_INFORMATION = 0x0400,
                /// <summary>
                /// Richiesto per impostare il token di impersonazione per un thread.
                /// </summary>
                THREAD_SET_THREAD_TOKEN = 0x0080,
                /// <summary>
                /// Richiesto per sospendere o riavviare un thread.
                /// </summary>
                THREAD_SUSPEND_RESUME = 0x0002,
                /// <summary>
                /// Richiesto per terminare un thread.
                /// </summary>
                THREAD_TERMINATE = 0x0001,

                THREAD_RESUME = 0x1000,
                /// <summary>
                /// Tutti i possibili diritti di accesso.
                /// </summary>
                THREAD_ALL_ACCESS = 
                    StandardAccessRights.STANDARD_RIGHTS_ALL |
                    StandardAccessRights.SPECIFIC_RIGHTS_ALL
            }
            #endregion
            #region Token
            /// <summary>
            /// Diritti di accesso a un token.
            /// </summary>
            [Flags]
            public enum TokenAccessRights : uint
            {
                /// <summary>
                /// Modifica del proprietario di default, del gruppo principale, della DACL.
                /// </summary>
                TOKEN_ADJUST_DEFAULT = 0x0080,
                /// <summary>
                /// Modifica degli attributi dei gruppi.
                /// </summary>
                TOKEN_ADJUST_GROUPS = 0x0040,
                /// <summary>
                /// Abilitazione o disabilitazione dei privilegi.
                /// </summary>
                TOKEN_ADJUST_PRIVILEGES = 0x0020,
                /// <summary>
                /// Modifica dell'ID sessione (richiede il privilegio SE_TCB_NAME).
                /// </summary>
                TOKEN_ADJUST_SESSIONID = 0x0100,
                /// <summary>
                /// Assegnare un token primario a un processo (richiede il privilegio SE_ASSIGNPRIMARYTOKEN_NAME).
                /// </summary>
                TOKEN_ASSIGN_PRIMARY = 0x0001,
                /// <summary>
                /// Duplicare il token.
                /// </summary>
                TOKEN_DUPLICATE = 0x0002,
                /// <summary>
                /// Assegnare un token di impersonazione a un processo.
                /// </summary>
                TOKEN_IMPERSONATE = 0x0004,
                /// <summary>
                /// Lettura del descrittore di sicurezza e assegnare un token di impersonazione a un processo.
                /// </summary>
                TOKEN_EXECUTE = StandardAccessRights.STANDARD_RIGHTS_EXECUTE | TOKEN_IMPERSONATE,
                /// <summary>
                /// Richiedere informazioni al token.
                /// </summary>
                TOKEN_QUERY = 0x0008,
                /// <summary>
                /// Richiedere informazioni alla fonte del token.
                /// </summary>
                TOKEN_QUERY_SOURCE = 0x0010,
                /// <summary>
                /// Lettura del descrittore di sicurezza e richiedere informazioni al token.
                /// </summary>
                TOKEN_READ = StandardAccessRights.STANDARD_RIGHTS_READ | TOKEN_QUERY,
                /// <summary>
                /// Lettura del descrittore di sicurezza, modifica dei privilegi, modifica degli attributi dei gruppi, modifica del proprietario di default, del gruppo principale e della DACL.
                /// </summary>
                TOKEN_WRITE = StandardAccessRights.STANDARD_RIGHTS_WRITE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT,
                /// <summary>
                /// Tutti i possibili permessi per un token.
                /// </summary>
                TOKEN_ALL_ACCESS =
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED |
                    TOKEN_ASSIGN_PRIMARY |
                    TOKEN_DUPLICATE |
                    TOKEN_IMPERSONATE |
                    TOKEN_QUERY |
                    TOKEN_QUERY_SOURCE |
                    TOKEN_ADJUST_PRIVILEGES |
                    TOKEN_ADJUST_GROUPS |
                    TOKEN_ADJUST_DEFAULT |
                    TOKEN_ADJUST_SESSIONID
            }
            #endregion
            #region File And Directory
            /// <summary>
            /// Diritti di accesso a un file o a una directory.
            /// </summary>
            [Flags]
            public enum FileDirectoryAccessRights : uint
            {
                /// <summary>
                /// Permette di aggiungere file a una directory.
                /// </summary>
                FILE_ADD_FILE = 0x2,
                /// <summary>
                /// Permette di creare una sottodirectory.
                /// </summary>
                FILE_ADD_SUBDIRECTORY = 0x4,
                /// <summary>
                /// Permette di aggiungere dati a un file o di creare una sottodirectory.
                /// </summary>
                FILE_APPEND_DATA = FILE_ADD_SUBDIRECTORY,
                /// <summary>
                /// Permette di creare una pipe.
                /// </summary>
                FILE_CREATE_PIPE_INSTANCE = FILE_ADD_SUBDIRECTORY,
                /// <summary>
                /// Per una directory, permette di eliminare una directory è tutti i file in essa, inclusi i file di sola lettura.
                /// </summary>
                FILE_DELETE_CHILD = 0x40,
                /// <summary>
                /// Permette di eseguire un file (solo file di codice nativo).
                /// </summary>
                FILE_EXECUTE = 0x20,
                /// <summary>
                /// Per una directory, permette di elencare i contenuti di una cartella.
                /// </summary>
                FILE_LIST_DIRECTORY = 0x1,
                /// <summary>
                /// Permette di leggere gli attributi di un file.
                /// </summary>
                FILE_READ_ATTRIBUTES = 0x80,
                /// <summary>
                /// Permette di leggere i dati da un file a da una directory.
                /// </summary>
                FILE_READ_DATA = FILE_LIST_DIRECTORY,
                /// <summary>
                /// Permette di leggere gli attributi estesi di un file.
                /// </summary>
                FILE_READ_EA = 0x8,
                /// <summary>
                /// Per una directory, il permesso di attraversare la directory.
                /// </summary>
                FILE_TRAVERSE = FILE_EXECUTE,
                /// <summary>
                /// Permette di scrivere gli attributi di un file.
                /// </summary>
                FILE_WRITE_ATTRIBUTES = 0x100,
                /// <summary>
                /// Per un file, permette scrivere dati in esso, per una directory permette di creare un file al suo interno.
                /// </summary>
                FILE_WRITE_DATA = FILE_ADD_FILE,
                /// <summary>
                /// Permette di scrivere gli attributi estesi di un file.
                /// </summary>
                FILE_WRITE_EA = 0x10,
                /// <summary>
                /// Permette di leggere il descrittore di sicurezza di un file o di una directory (esclusa SACL).
                /// </summary>
                STANDARD_RIGHTS_READ = StandardAccessRights.STANDARD_RIGHTS_READ,
                /// <summary>
                /// Equivalente a <see cref="STANDARD_RIGHTS_READ"/>.
                /// </summary>
                STANDARD_RIGHTS_WRITE = STANDARD_RIGHTS_READ,
                /// <summary>
                /// Tutti i possibili diritti di accesso.
                /// </summary>
                FILE_ALL_ACCESS =
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED |
                    GenericObjectAccessRights.SYNCHRONIZE |
                    FILE_LIST_DIRECTORY |
                    FILE_ADD_FILE |
                    FILE_ADD_SUBDIRECTORY |
                    FILE_READ_EA |
                    FILE_WRITE_EA |
                    FILE_EXECUTE |
                    FILE_DELETE_CHILD |
                    FILE_READ_ATTRIBUTES |
                    FILE_WRITE_ATTRIBUTES
            }

            /// <summary>
            /// Diritti di accesso generici per file e directory.
            /// </summary>
            [Flags]
            public enum FileGenericAccessRights : uint
            {
                FILE_GENERIC_EXECUTE =
                    FileDirectoryAccessRights.FILE_EXECUTE |
                    FileDirectoryAccessRights.FILE_READ_ATTRIBUTES |
                    StandardAccessRights.STANDARD_RIGHTS_EXECUTE |
                    GenericObjectAccessRights.SYNCHRONIZE,
                FILE_GENERIC_READ =
                    FileDirectoryAccessRights.FILE_READ_ATTRIBUTES |
                    FileDirectoryAccessRights.FILE_READ_DATA |
                    FileDirectoryAccessRights.FILE_READ_EA |
                    FileDirectoryAccessRights.STANDARD_RIGHTS_READ |
                    GenericObjectAccessRights.SYNCHRONIZE,
                FILE_GENERIC_WRITE =
                    FileDirectoryAccessRights.FILE_APPEND_DATA |
                    FileDirectoryAccessRights.FILE_WRITE_ATTRIBUTES |
                    FileDirectoryAccessRights.FILE_WRITE_DATA |
                    FileDirectoryAccessRights.FILE_WRITE_EA |
                    FileDirectoryAccessRights.STANDARD_RIGHTS_WRITE |
                    GenericObjectAccessRights.SYNCHRONIZE
            }
            #endregion
            #region File Mapping
            /// <summary>
            /// Diritti di accesso a un oggetto file mapping e altre opzioni.
            /// </summary>
            [Flags]
            public enum FileMappingAccessRightsAndFlags : uint
            {
                #region Access Rights
                /// <summary>
                /// Permette il mapping di viste di oggetti file mapping di sola lettura, copy-on-write e di lettura/scrittura.
                /// </summary>
                /// <remarks>L'oggetto deve essere stato creato con la protezione delle pagine che garantisce accesso in scrittura (<see cref="MemoryProtections.PAGE_READWRITE"/>, <see cref="MemoryProtections.PAGE_EXECUTE_READWRITE"/>).</remarks>
                FILE_MAP_WRITE = 0x0002,
                /// <summary>
                /// Permette il mapping di viste di oggetti file mapping di sola lettura e copy-on-write.
                /// </summary>
                FILE_MAP_READ = 0x0004,
                /// <summary>
                /// Permette il mapping di viste eseguibili dell'oggetto file mapping.
                /// </summary>
                /// <remarks>L'oggetto deve essere stato creato con la protezione delle pagine che garantisce accesso in esecuzione (<see cref="MemoryProtections.PAGE_EXECUTE_READ"/>, <see cref="MemoryProtections.PAGE_EXECUTE_READWRITE"/>, <see cref="MemoryProtections.PAGE_EXECUTE_WRITECOPY"/>).</remarks>
                FILE_MAP_EXECUTE = 0x0020,
                /// <summary>
                /// Tutti i possibili diritti di accesso eccetto <see cref="FILE_MAP_EXECUTE"/>.
                /// </summary>
                FILE_MAP_ALL_ACCESS =
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED |
                    0x0001 |
                    FILE_MAP_WRITE |
                    FILE_MAP_READ |
                    0x0008 |
                    0x0010,
                #endregion
                #region Flags
                /// <summary>
                /// Una vista copy-on-write viene mappata.
                /// </summary>
                /// <remarks>L'oggetto file mapping deve essere stato creato con una delle seguenti protezioni:<br/><br/>
                /// <see cref="MemoryProtections.PAGE_READONLY"/><br/>
                /// <see cref="MemoryProtections.PAGE_EXECUTE_READ"/><br/>
                /// <see cref="MemoryProtections.PAGE_WRITECOPY"/><br/>
                /// <see cref="MemoryProtections.PAGE_EXECUTE_WRITECOPY"/><br/>
                /// <see cref="MemoryProtections.PAGE_READWRITE"/><br/>
                /// <see cref="MemoryProtections.PAGE_EXECUTE_READWRITE"/></remarks>
                FILE_MAP_COPY = 0x00000001,
                /// <summary>
                /// La vista dovrebbe essere mappata usando il supporto per pagine grandi.
                /// </summary>
                /// <remarks>L'oggetto file mapping deve essere stato creato con l'opzione <see cref="FileMappingAttributes.SEC_LARGE_PAGES"/>.</remarks>
                FILE_MAP_LARGE_PAGES = 0x20000000,
                /// <summary>
                /// Imposta tutte le posizioni nel file mappato come non valide per CFG.
                /// </summary>
                /// <remarks>Questa opzione deve essere usata insieme a <see cref="FILE_MAP_EXECUTE"/>.</remarks>
                FILE_MAP_TARGETS_INVALID = 0x40000000,

                FILE_MAP_RESERVE = 0x80000000
                #endregion
            }
            #endregion
            #region Desktop
            /// <summary>
            /// Diritti di accesso al desktop.
            /// </summary>
            [Flags]
            public enum DesktopAccessRights : uint
            {
                /// <summary>
                /// Creazione di un menù.
                /// </summary>
                DESKTOP_CREATEMENU = 0x0004,
                /// <summary>
                /// Creazione di una finestra.
                /// </summary>
                DESKTOP_CREATEWINDOW = 0x0002,
                /// <summary>
                /// Enumerazione del desktop.
                /// </summary>
                DESKTOP_ENUMERATE = 0x0040,
                /// <summary>
                /// Permesso di impostare hook alle finestre nel desktop.
                /// </summary>
                DESKTOP_HOOKCONTROL = 0x0008,
                /// <summary>
                /// Riproduzione del journal.
                /// </summary>
                DESKTOP_JOURNALPLAYBACK = 0x0020,
                /// <summary>
                /// Registrazione del journal.
                /// </summary>
                DESKTOP_JOURNALRECORD = 0x0010,
                /// <summary>
                /// Lettura di oggetti sul desktop.
                /// </summary>
                DESKTOP_READOBJECTS = 0x0001,
                /// <summary>
                /// Attivare il desktop.
                /// </summary>
                DESKTOP_SWITCHDESKTOP = 0x0100,
                /// <summary>
                /// Scrittura di oggetti sul desktop.
                /// </summary>
                DESKTOP_WRITEOBJECTS = 0x0080
            }
            #endregion
            #region Synchronization Objects
            /// <summary>
            /// Diritti di accesso a un oggetto di sincronizzazione (Timer, Semaforo, Evento).
            /// </summary>
            [Flags]
            public enum SyncObjectAccessRights : uint
            {
                /// <summary>
                /// Accesso totale.
                /// </summary>
                SYNC_OBJECT_ALL_ACCESS = 
                    GenericObjectAccessRights.SYNCHRONIZE |
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED |
                    0x3,
                /// <summary>
                /// Modifica stato.
                /// </summary>
                SYNC_OBJECT_MODIFY_STATE = 0x0002
            }

            /// <summary>
            /// Diritti di accesso a un mutex.
            /// </summary>
            [Flags]
            public enum MutexAccessRights : uint
            {
                /// <summary>
                /// Modifica stato.
                /// </summary>
                MUTANT_QUERY_STATE = 0x0001,
                /// <summary>
                /// Accesso totale.
                /// </summary>
                MUTEX_ALL_ACCESS = 
                    GenericObjectAccessRights.SYNCHRONIZE |
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED |
                    MUTANT_QUERY_STATE
            }
            #endregion
            #region Jobs
            /// <summary>
            /// Diritti di accesso per un job.
            /// </summary>
            [Flags]
            public enum JobObjectAccessRights : uint
            {
                /// <summary>
                /// Assegnamento di un processo a un job.
                /// </summary>
                JOB_OBJECT_ASSIGN_PROCESS = 0x0001,
                /// <summary>
                /// Recupero di alcune informazioni.
                /// </summary>
                JOB_OBJECT_QUERY = 0x0004,
                /// <summary>
                /// Impostare gli attributi.
                /// </summary>
                JOB_OBJECT_SET_ATTRIBUTES = 0x0002,
                /// <summary>
                /// Terminare tutti i processi associati.
                /// </summary>
                JOB_OBJECT_TERMINATE = 0x0008,
                /// <summary>
                /// Non supportato.
                /// </summary>
                JOB_OBJECT_SET_SECURITY_ATTRIBUTES = 0x0010,

                JOB_OBJECT_IMPERSONATE = 0x0020,
                /// <summary>
                /// Tutti i possibili diritti di accesso.
                /// </summary>
                JOB_OBJECT_ALL_ACCESS = 
                    JOB_OBJECT_ASSIGN_PROCESS |
                    JOB_OBJECT_QUERY |
                    JOB_OBJECT_SET_ATTRIBUTES |
                    JOB_OBJECT_TERMINATE |
                    JOB_OBJECT_SET_SECURITY_ATTRIBUTES |
                    StandardAccessRights.STANDARD_RIGHTS_ALL
            }
            #endregion
            #region Registry Key
            /// <summary>
            /// Diritti di accesso per chiavi di registro.
            /// </summary>
            [Flags]
            public enum RegistryKeyAccessRights : uint
            {
                /// <summary>
                /// Riservato.
                /// </summary>
                KEY_CREATE_LINK = 0x0020,
                /// <summary>
                /// Creazione di una sottochiave.
                /// </summary>
                KEY_CREATE_SUB_KEY = 0x0004,
                /// <summary>
                /// Enumerazione delle sottochiavi.
                /// </summary>
                KEY_ENUMERATE_SUB_KEYS = 0x0008,
                /// <summary>
                /// Richiesta notifiche di cambiamento.
                /// </summary>
                KEY_NOTIFY = 0x0010,
                /// <summary>
                /// Lettura valore.
                /// </summary>
                KEY_QUERY_VALUE = 0x0001,
                /// <summary>
                /// Impostazione valore.
                /// </summary>
                KEY_SET_VALUE = 0x0002,
                /// <summary>
                /// Lettura informazioni descrittore di sicurezza (esclusa SACL), lettura valore, enumerazione delle sottochiavi, richiesta notifiche di cambiamento.
                /// </summary>
                KEY_READ = 
                    StandardAccessRights.STANDARD_RIGHTS_READ |
                    KEY_QUERY_VALUE |
                    KEY_ENUMERATE_SUB_KEYS |
                    KEY_NOTIFY,
                /// <summary>
                /// Equivalente a <see cref="KEY_READ"/>.
                /// </summary>
                KEY_EXECUTE = KEY_READ,
                /// <summary>
                /// Lettura informazioni descrittore di sicurezza (esclusa SACL), impostazione valore, creazione sottochiavi.
                /// </summary>
                KEY_WRITE = 
                    StandardAccessRights.STANDARD_RIGHTS_WRITE |
                    KEY_SET_VALUE |
                    KEY_CREATE_SUB_KEY,
                /// <summary>
                /// Tutti i possibili diritti di accesso.
                /// </summary>
                KEY_ALL_ACCESS = 
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED |
                    KEY_QUERY_VALUE |
                    KEY_SET_VALUE |
                    KEY_CREATE_SUB_KEY |
                    KEY_ENUMERATE_SUB_KEYS |
                    KEY_NOTIFY |
                    KEY_CREATE_LINK
            }
            #endregion
            #region Window Station
            /// <summary>
            /// Diritti di accesso a una window station.
            /// </summary>
            [Flags]
            public enum WindowStationAccessRights : uint
            {
                /// <summary>
                /// Uso della clipboard.
                /// </summary>
                WINSTA_ACCESSCLIPBOARD = 0x0004,
                /// <summary>
                /// Manipolazione degli atoms globali.
                /// </summary>
                WINSTA_ACCESSGLOBALATOMS = 0x0020,
                /// <summary>
                /// Creazione di un desktop sulla window station.
                /// </summary>
                WINSTA_CREATEDESKTOP = 0x0008,
                /// <summary>
                /// Enumerazione dei desktop.
                /// </summary>
                WINSTA_ENUMDESKTOPS = 0x0001,
                /// <summary>
                /// Enumerazione della window station.
                /// </summary>
                WINSTA_ENUMERATE = 0x0100,
                /// <summary>
                /// Chiusura delle finestre sulla window station.
                /// </summary>
                /// <remarks>Necessario per chiamare la funzione ExitWindowsEx.</remarks>
                WINSTA_EXITWINDOWS = 0x0040,
                /// <summary>
                /// Lettura degli attributi.
                /// </summary>
                WINSTA_READATTRIBUTES = 0x0002,
                /// <summary>
                /// Accesso ai contenuti dello schermo.
                /// </summary>
                WINSTA_READSCREEN = 0x0200,
                /// <summary>
                /// Modifica degli attributi.
                /// </summary>
                WINSTA_WRITEATTRIBUTES = 0x0010,
                /// <summary>
                /// Tutti i possibili diritti di accesso.
                /// </summary>
                WINSTA_ALL_ACCESS =
                    WINSTA_ACCESSCLIPBOARD |
                    WINSTA_ACCESSGLOBALATOMS |
                    WINSTA_CREATEDESKTOP |
                    WINSTA_ENUMDESKTOPS |
                    WINSTA_ENUMERATE |
                    WINSTA_EXITWINDOWS |
                    WINSTA_READATTRIBUTES |
                    WINSTA_READSCREEN |
                    WINSTA_WRITEATTRIBUTES
            }
            #endregion
            #region Section
            /// <summary>
            /// Diritti di accesso a una sezione.
            /// </summary>
            [Flags]
            public enum SectionAccessRights : uint
            {

                SECTION_QUERY = 0x0001,

                SECTION_MAP_WRITE = 0x0002,

                SECTION_MAP_READ = 0x0004,

                SECTION_MAP_EXECUTE = 0x0008,

                SECTION_EXTEND_SIZE = 0x0010,

                SECTION_MAP_EXECUTE_EXPLICIT = 0x0020,

                SECTION_ALL_ACCESS =
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED |
                    SECTION_QUERY |
                    SECTION_MAP_WRITE |
                    SECTION_MAP_READ |
                    SECTION_MAP_EXECUTE |
                    SECTION_EXTEND_SIZE
            }
            #endregion
            #endregion
            #region Service Enumerations
            #region Access Rights Enumerations
            /// <summary>
            /// Diritti di accesso a Gestore controllo servizi.
            /// </summary>
            [Flags]
            public enum ServiceControlManagerAccessRights : uint
            {
                /// <summary>
                /// Creazione di un servizio.
                /// </summary>
                SC_MANAGER_CREATE_SERVICE = 0x0002,
                /// <summary>
                /// Connessione a Gestore controllo servizi.
                /// </summary>
                SC_MANAGER_CONNECT = 0x0001,
                /// <summary>
                /// Enumerazione dei servizi.
                /// </summary>
                SC_MANAGER_ENUMERATE_SERVICE = 0x0004,
                /// <summary>
                /// Blocco del database.
                /// </summary>
                SC_MANAGER_LOCK = 0x0008,
                /// <summary>
                /// Necessario per chiamare la funzione NotifyBootConfigStatus.
                /// </summary>
                SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020,
                /// <summary>
                /// Recuperare informazioni sul blocco del database.
                /// </summary>
                SC_MANAGER_QUERY_LOCK_STATUS = 0x0010,
                /// <summary>
                /// Tutti i diritti di accesso.
                /// </summary>
                SC_MANAGER_ALL_ACCESS = 
                    SC_MANAGER_CREATE_SERVICE |
                    SC_MANAGER_CONNECT |
                    SC_MANAGER_ENUMERATE_SERVICE |
                    SC_MANAGER_LOCK |
                    SC_MANAGER_MODIFY_BOOT_CONFIG |
                    SC_MANAGER_QUERY_LOCK_STATUS |
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED
            }

            /// <summary>
            /// Diritti di accesso a un servizio.
            /// </summary>
            [Flags]
            public enum ServiceAccessRights : uint
            {
                /// <summary>
                /// Cambiare la configurazione.
                /// </summary>
                SERVICE_CHANGE_CONFIG = 0x0002,
                /// <summary>
                /// Enumerare i servizi dipendenti.
                /// </summary>
                SERVICE_ENUMERATE_DEPENDENTS = 0x0008,
                /// <summary>
                /// Richiesta dello stato.
                /// </summary>
                SERVICE_INTERROGATE = 0x0080,
                /// <summary>
                /// Messa in pausa e continuazione.
                /// </summary>
                SERVICE_PAUSE_CONTINUE = 0x0040,
                /// <summary>
                /// Richiesta informazioni sulla configurazione.
                /// </summary>
                SERVICE_QUERY_CONFIG = 0x0001,
                /// <summary>
                /// Richiesta informazioni sullo stato e ricezione notifiche di cambio stato.
                /// </summary>
                SERVICE_QUERY_STATUS = 0x0004,
                /// <summary>
                /// Avvio del servizio.
                /// </summary>
                SERVICE_START = 0x0010,
                /// <summary>
                /// Arresto del servizio.
                /// </summary>
                SERVICE_STOP = 0x0020,
                /// <summary>
                /// Invio di un codice di controllo definito dall'utente.
                /// </summary>
                SERVICE_USER_DEFINED_CONTROL = 0x0100,
                /// <summary>
                /// Tutti i diritti di accesso.
                /// </summary>
                SERVICE_ALL_ACCESS =
                    SERVICE_CHANGE_CONFIG |
                    SERVICE_ENUMERATE_DEPENDENTS |
                    SERVICE_INTERROGATE |
                    SERVICE_PAUSE_CONTINUE |
                    SERVICE_QUERY_CONFIG |
                    SERVICE_QUERY_STATUS |
                    SERVICE_START |
                    SERVICE_STOP |
                    SERVICE_USER_DEFINED_CONTROL |
                    StandardAccessRights.STANDARD_RIGHTS_REQUIRED
            }
            #endregion
            #region Info Enumerations
            /// <summary>
            /// Livello di informazioni richiesto a un servizio.
            /// </summary>
            public enum ServiceEnumerationInfoLevel : uint
            {
                SC_ENUM_PROCESS_INFO,

                SC_STATUS_PROCESS_INFO = SC_ENUM_PROCESS_INFO
            }

            /// <summary>
            /// Tipo di servizio.
            /// </summary>
            public enum ServiceType : uint
            {
                /// <summary>
                /// Servizi di tipo <see cref="SERVICE_FILE_SYSTEM_DRIVER"/> e <see cref="SERVICE_KERNEL_DRIVER"/>.
                /// </summary>
                SERVICE_DRIVER = 0x0000000B,
                /// <summary>
                /// Servizio driver di file system.
                /// </summary>
                SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,
                /// <summary>
                /// Servizio driver.
                /// </summary>
                SERVICE_KERNEL_DRIVER = 0x00000001,
                /// <summary>
                /// Servizio in esecuzione in un proprio processo.
                /// </summary>
                SERVICE_WIN32_OWN_PROCESS = 0x00000010,
                /// <summary>
                /// Servizio in esecuzione in un processo condiviso.
                /// </summary>
                SERVICE_WIN32_SHARE_PROCESS = 0x00000020,
                /// <summary>
                /// Servizi del tipo <see cref="SERVICE_WIN32_OWN_PROCESS"/> e <see cref="SERVICE_WIN32_SHARE_PROCESS"/>.
                /// </summary>
                SERVICE_WIN32 =
                    SERVICE_WIN32_OWN_PROCESS |
                    SERVICE_WIN32_SHARE_PROCESS,
                /// <summary>
                /// Il servizio può interagire con il desktop.
                /// </summary>
                SERVICE_INTERACTIVE_PROCESS = 0x00000100
            }

            /// <summary>
            /// Stati comprensivi di un servizio.
            /// </summary>
            [Flags]
            public enum ServiceState2 : uint
            {
                /// <summary>
                /// Servizi attivi.
                /// </summary>
                SERVICE_ACTIVE = 0x00000001,
                /// <summary>
                /// Servizi inattivi.
                /// </summary>
                SERVICE_INACTIVE = 0x00000002,
                /// <summary>
                /// Tutti i servizi.
                /// </summary>
                SERVICE_STATE_ALL =
                    SERVICE_ACTIVE |
                    SERVICE_INACTIVE
            }

            /// <summary>
            /// Stato di un servizio.
            /// </summary>
            public enum ServiceState : uint
            {
                /// <summary>
                /// Il servizio si sta preparando a continuare.
                /// </summary>
                SERVICE_CONTINUE_PENDING = 0x00000005,
                /// <summary>
                /// Il servizio si sta mettendo in pausa.
                /// </summary>
                SERVICE_PAUSE_PENDING = 0x00000006,
                /// <summary>
                /// Servizio in pausa.
                /// </summary>
                SERVICE_PAUSED = 0x00000007,
                /// <summary>
                /// Servizio in esecuzione.
                /// </summary>
                SERVICE_RUNNING = 0x00000004,
                /// <summary>
                /// Servizio in avvio.
                /// </summary>
                SERVICE_START_PENDING = 0x00000002,
                /// <summary>
                /// Servizio in arresto.
                /// </summary>
                SERVICE_STOP_PENDING = 0x00000003,
                /// <summary>
                /// Servizio arrestato.
                /// </summary>
                SERVICE_STOPPED = 0x00000001
            }

            /// <summary>
            /// Codici di controllo accettati da un servizio.
            /// </summary>
            [Flags]
            public enum ServiceAcceptedControlCodes : uint
            {
                /// <summary>
                /// Il servizio è un componente di rete che può accettare cambiamenti nei suoi binding senza essere riavviato.
                /// </summary>
                SERVICE_ACCEPT_NETBINDCHANGE = 0x00000010,
                /// <summary>
                /// Il servizio può leggere nuovamente i suoi parametri di avvio senza riavviarsi.
                /// </summary>
                SERVICE_ACCEPT_PARAMCHANGE = 0x00000008,
                /// <summary>
                /// Il servizio può essere messo in pause e poi riavviato.
                /// </summary>
                SERVICE_ACCEPT_PAUSE_CONTINUE = 0x00000002,
                /// <summary>
                /// Il servizio può eseguire operazioni pre-spegnimento.
                /// </summary>
                SERVICE_ACCEPT_PRESHUTDOWN = 0x00000100,
                /// <summary>
                /// Il servizio può eseguire operazioni durante lo spegnimento.
                /// </summary>
                SERVICE_ACCEPT_SHUTDOWN = 0x00000004,
                /// <summary>
                /// Il servizio può essere arrestato.
                /// </summary>
                SERVICE_ACCEPT_STOP = 0x00000001,
                /// <summary>
                /// Il servizio è notificato quando il profilo hardware del computer è cambiato.
                /// </summary>
                SERVICE_ACCEPT_HARDWAREPROFILECHANGE = 0x00000020,
                /// <summary>
                /// Il servizio è notificato quando lo stato energetico del computer è cambiato.
                /// </summary>
                SERVICE_ACCEPT_POWEREVENT = 0x00000040,
                /// <summary>
                /// Il servizio è notificato quando lo stato della sessione è cambiato.
                /// </summary>
                SERVICE_ACCEPT_SESSIONCHANGE = 0x00000080,
                /// <summary>
                /// Modifica dell'ora del sistema.
                /// </summary>
                SERVICE_ACCEPT_TIMECHANGE = 0x00000200,

                SERVICE_ACCEPT_TRIGGEREVENT = 0x00000400,
                /// <summary>
                /// Disconnessione dell'utente.
                /// </summary>
                SERVICE_ACCEPT_USER_LOGOFF = 0x00000800,
                /// <summary>
                /// Poche risorse disponibili.
                /// </summary>
                SERVICE_ACCEPT_LOWRESOURCES = 0x00002000,
                /// <summary>
                /// Basse risorse di sistema.
                /// </summary>
                SERVICE_ACCEPT_SYSTEM_LOWRESOURCES = 0x00004000,
                /// <summary>
                /// Il servizio non accetta nessun comando.
                /// </summary>
                NONE = 0x00000000
            }

            /// <summary>
            /// Caratteristiche di un servizio.
            /// </summary>
            public enum ServiceFlags : uint
            {
                /// <summary>
                /// Il servizio è in esecuzione in un processo non di sistema o non è in esecuzione.
                /// </summary>
                SERVICE_RUNS_IN_NORMAL_PROCESS,
                /// <summary>
                /// Il servizio è in esecuzione in un processo di sistema e deve essere sempre in esecuzione.
                /// </summary>
                SERVICE_RUNS_IN_SYSTEM_PROCESS
            }

            /// <summary>
            /// Modalità di avvio di un servizio.
            /// </summary>
            public enum ServiceStartType : uint
            {
                /// <summary>
                /// Avvio automatico.
                /// </summary>
                SERVICE_AUTO_START = 0x00000002,
                /// <summary>
                /// Avvio al boot, servizio avviato dal caricatore del sistema.
                /// </summary>
                SERVICE_BOOT_START = 0x00000000,
                /// <summary>
                /// Avvio su richiesta.
                /// </summary>
                SERVICE_DEMAND_START = 0x00000003,
                /// <summary>
                /// Servizio disattivato.
                /// </summary>
                SERVICE_DISABLED = 0x00000004,
                /// <summary>
                /// Servizio avviato all'avvio del sistema, caricato dalla funzione IoInitSystem.
                /// </summary>
                SERVICE_SYSTEM_START = 0x00000001
            }

            /// <summary>
            /// Modalità di controllo errori di un servizio.
            /// </summary>
            public enum ServiceErrorControlMode : uint
            {
                /// <summary>
                /// Il programma di avvio mette a log l'errore, il sistema viene riavviato nell'ultima configurazione valida, se l'avvio in questa modalità è già in corso l'operazione fallisce.
                /// </summary>
                SERVICE_ERROR_CRITICAL = 0x00000003,
                /// <summary>
                /// Il programma di avvio ignora l'errore e continua l'avvio.
                /// </summary>
                SERVICE_ERROR_IGNORE = 0x00000000,
                /// <summary>
                /// Il programma di avvio mette a log l'errore e continua l'avvio.
                /// </summary>
                SERVICE_ERROR_NORMAL = 0x00000001,
                /// <summary>
                /// Il programma di avvio mette a log l'errore, il sistema viene riavviato nell'ultima configurazione valida, se l'avvio in questa modalità è già in corso l'operazione continua.
                /// </summary>
                SERVICE_ERROR_SEVERE = 0x00000002
            }

            /// <summary>
            /// Tipo di SID di un servizio.
            /// </summary>
            public enum ServiceSIDType : uint
            {

                SERVICE_SID_TYPE_NONE,

                SERVICE_SID_TYPE_RESTRICTED = 3,

                SERVICE_SID_TYPE_UNRESTRICTED = 1
            }

            /// <summary>
            /// Dati specifici di un trigger per un servizio.
            /// </summary>
            public enum ServiceTriggerDataType : uint
            {
                /// <summary>
                /// Binario.
                /// </summary>
                SERVICE_TRIGGER_DATA_TYPE_BINARY = 1,
                /// <summary>
                /// Stringa.
                /// </summary>
                SERVICE_TRIGGER_DATA_TYPE_STRING,
                /// <summary>
                /// Byte.
                /// </summary>
                SERVICE_TRIGGER_DATA_TYPE_LEVEL,
                /// <summary>
                /// Valore intero a 64 bit senza segno.
                /// </summary>
                SERVICE_TRIGGER_DATA_TYPE_KEYWORD_ANY,
                /// <summary>
                /// Valore intero a 64 bit senza segno.
                /// </summary>
                SERVICE_TRIGGER_DATA_TYPE_KEYWORD_ALL
            }

            /// <summary>
            /// Tipi di trigger per un servizio.
            /// </summary>
            public enum ServiceTriggerType : uint
            {
                /// <summary>
                /// Evento personalizzato generato da un provider ETW.
                /// </summary>
                SERVICE_TRIGGER_TYPE_CUSTOM = 20,
                /// <summary>
                /// Evento che si verifica quando un dispositivo è presente all'avvio del sistema.
                /// </summary>
                SERVICE_TRIGGER_TYPE_DEVICE_INTERFACE_ARRIVAL = 1,
                /// <summary>
                /// Evento che si verifica quando il computer si unisce o lascia un dominio.
                /// </summary>
                SERVICE_TRIGGER_TYPE_DOMAIN_JOIN = 3,
                /// <summary>
                /// Evento che si verifica quando una porta del firewall si apre o circa 60 secondi dopo che la porta del firewall è stata chiusa.
                /// </summary>
                SERVICE_TRIGGER_TYPE_FIREWALL_PORT_EVENT = 4,
                /// <summary>
                /// Evento che si verifica quando una politica utente o di sistema cambia.
                /// </summary>
                SERVICE_TRIGGER_TYPE_GROUP_POLICY,
                /// <summary>
                /// Evento che si verifica quando il primo indirizzo sullo stack di rete TCP/IP diventa disponibili oppure quando l'ultimo indirizzo IP diventa non disponibile.
                /// </summary>
                SERVICE_TRIGGER_TYPE_IP_ADDRESS_AVAILABILITY = 2,
                /// <summary>
                /// Evento che si verifica quando un pacchetto o una richiesta viene ricevuta su un particolare protocollo.
                /// </summary>
                SERVICE_TRIGGER_TYPE_NETWORK_ENDPOINT = 6
            }

            /// <summary>
            /// Azioni da eseguibili quando si verifica un evento trigger per un servizio.
            /// </summary>
            public enum ServiceTriggerAction : uint
            {
                /// <summary>
                /// Avvia il servizio quando si verifica l'evento.
                /// </summary>
                SERVICE_TRIGGER_ACTION_SERVICE_START = 1,
                /// <summary>
                /// Arresta il servizio quando si verifica l'evento.
                /// </summary>
                SERVICE_TRIGGER_ACTION_SERVICE_STOP
            }

            /// <summary>
            /// Tipo di protezione di un servizio.
            /// </summary>
            public enum ServiceLaunchProtectionType
            {
                SERVICE_LAUNCH_PROTECTED_NONE,

                SERVICE_LAUNCH_PROTECTED_WINDOWS,

                SERVICE_LAUNCH_PROTECTED_WINDOWS_LIGHT,

                SERVICE_LAUNCH_PROTECTED_ANTIMALWARE_LIGHT
            }
            #endregion
            #region Other Enumerations
            /// <summary>
            /// Cambi di stato di un servizio.
            /// </summary>
            [Flags]
            public enum ServiceNotificationReasons : uint
            {
                /// <summary>
                /// Creazione di un servizio.
                /// </summary>
                /// <remarks>Utilizzabile solo con un handle a Gestore Controllo Servizi.</remarks>
                SERVICE_NOTIFY_CREATED = 0x00000080,
                /// <summary>
                /// Il servizio è in ripresa.
                /// </summary>
                SERVICE_NOTIFY_CONTINUE_PENDING = 0x00000010,
                /// <summary>
                /// Il servizio sta per essere eliminato.
                /// </summary>
                SERVICE_NOTIFY_DELETE_PENDING = 0x00000200,
                /// <summary>
                /// Eliminazione di un servizio.
                /// </summary>
                /// <remarks>Utilizzabile solo con un handle a Gestore Controllo Servizi.</remarks>
                SERVICE_NOTIFY_DELETED = 0x00000100,
                /// <summary>
                /// Il servizio si sta mettendo in pausa.
                /// </summary>
                SERVICE_NOTIFY_PAUSE_PENDING = 0x00000020,
                /// <summary>
                /// Il servizio è in pausa.
                /// </summary>
                SERVICE_NOTIFY_PAUSED = 0x00000040,
                /// <summary>
                /// Il servizio è in esecuzione.
                /// </summary>
                SERVICE_NOTIFY_RUNNING = 0x00000008,
                /// <summary>
                /// Il servizio si sta avviando.
                /// </summary>
                SERVICE_NOTIFY_START_PENDING = 0x00000002,
                /// <summary>
                /// Il servizio è in arresto.
                /// </summary>
                SERVICE_NOTIFY_STOP_PENDING = 0x00000004,
                /// <summary>
                /// Il servizio è arrestato.
                /// </summary>
                SERVICE_NOTIFY_STOPPED = 0x00000001
            }

            /// <summary>
            /// Informazione da richiedere sulla configurazione opzionale di un servizio.
            /// </summary>
            public enum ServiceOptionalConfigurationInfoLevel : uint
            {
                /// <summary>
                /// Informazioni sullo stato di avvio ritardato, valore BOOL a 4 byte.
                /// </summary>
                SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 3,
                /// <summary>
                /// Descrizione del servizio, stringa.
                /// </summary>
                SERVICE_CONFIG_DESCRIPTION = 1,
                /// <summary>
                /// Azioni da eseguire ad ogni crash del servizio, struttura <see cref="Win32Structures.SERVICE_FAILURE_ACTIONS"/>.
                /// </summary>
                SERVICE_CONFIG_FAILURE_ACTIONS,
                /// <summary>
                /// Indica se eseguire le azioni relative ai crash anche in caso non si verifichi un errore che determina un crash, valore BOOL a 4 byte.
                /// </summary>
                SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 4,
                /// <summary>
                /// Nodo preferito, struttura <see cref="Win32Structures.SERVICE_PREFERRED_NODE_INFO"/>.
                /// </summary>
                SERVICE_CONFIG_PREFERRED_NODE = 9,
                /// <summary>
                /// Tempo di timeout per eseguire azioni prespegnimento, valore DWORD.
                /// </summary>
                SERVICE_CONFIG_PRESHUTDOWN_INFO = 7,
                /// <summary>
                /// Privilegi necessari al servizio, multi-stringa.
                /// </summary>
                SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO = 6,
                /// <summary>
                /// Tipo di SID del servizio, valore <see cref="ServiceSIDType"/>.
                /// </summary>
                SERVICE_CONFIG_SERVICE_SID_INFO = 5,
                /// <summary>
                /// Trigger del servizio, struttura <see cref="Win32Structures.SERVICE_TRIGGER_INFO"/>.
                /// </summary>
                SERVICE_CONFIG_TRIGGER_INFO = 8,
                /// <summary>
                /// Tipo di protezione di un servizio, valore DWORD.
                /// </summary>
                SERVICE_CONFIG_LAUNCH_PROTECTED = 12
            }

            /// <summary>
            /// Azioni che Gestione Controllo Servizi può eseguire.
            /// </summary>
            public enum ServiceControlManagerAction : uint
            {
                /// <summary>
                /// Nessuna azione.
                /// </summary>
                SC_ACTION_NONE,
                /// <summary>
                /// Riavvia il servizio.
                /// </summary>
                SC_ACTION_RESTART,
                /// <summary>
                /// Riavvia il computer.
                /// </summary>
                SC_ACTION_REBOOT,
                /// <summary>
                /// Esecuzione di un comando.
                /// </summary>
                SC_ACTION_RUN_COMMAND
            }
            #endregion
            #endregion
            #region System Shutdown Enumerations
            /// <summary>
            /// Ragione dello spegnimento del computer.
            /// </summary>
            [Flags]
            public enum ShutdownReasons : uint
            {
                #region Major
                /// <summary>
                /// Problema con un'applicazione.
                /// </summary>
                SHTDN_REASON_MAJOR_APPLICATION = 0x00040000,
                /// <summary>
                /// Problema con un dispositivo hardware.
                /// </summary>
                SHTDN_REASON_MAJOR_HARDWARE = 0x00010000,
                /// <summary>
                /// La funzione InitiateSystemShutdown è stata utilizzata al posto della funzione InitiateSystemShutdownEx.
                /// </summary>
                SHTDN_REASON_MAJOR_LEGACY_API = 0x00070000,
                /// <summary>
                /// Problema con il sistema operativo.
                /// </summary>
                SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000,
                /// <summary>
                /// Problema di altro tipo.
                /// </summary>
                SHTDN_REASON_MAJOR_OTHER = 0x00000000,
                /// <summary>
                /// Perdita di potenza.
                /// </summary>
                SHTDN_REASON_MAJOR_POWER = 0x00060000,
                /// <summary>
                /// Problema software.
                /// </summary>
                SHTDN_REASON_MAJOR_SOFTWARE = 0x00030000,
                /// <summary>
                /// Problema di sistema.
                /// </summary>
                SHTDN_REASON_MAJOR_SYSTEM = 0x00050000,
                #endregion
                #region Minor
                /// <summary>
                /// Schermata blu.
                /// </summary>
                SHTDN_REASON_MINOR_BLUESCREEN = 0x0000000F,
                /// <summary>
                /// Spina staccata.
                /// </summary>
                SHTDN_REASON_MINOR_CORDUNPLUGGED = 0x0000000b,
                /// <summary>
                /// Disco.
                /// </summary>
                SHTDN_REASON_MINOR_DISK = 0x00000007,
                /// <summary>
                /// Ambiente.
                /// </summary>
                SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c,
                /// <summary>
                /// Driver.
                /// </summary>
                SHTDN_REASON_MINOR_HARDWARE_DRIVER = 0x0000000d,
                /// <summary>
                /// Hotfix.
                /// </summary>
                SHTDN_REASON_MINOR_HOTFIX = 0x00000011,
                /// <summary>
                /// Disinstallazione hotfix.
                /// </summary>
                SHTDN_REASON_MINOR_HOTFIX_UNINSTALL = 0x00000017,
                /// <summary>
                /// Nessuna risposta.
                /// </summary>
                SHTDN_REASON_MINOR_HUNG = 0x00000005,
                /// <summary>
                /// Installazione.
                /// </summary>
                SHTDN_REASON_MINOR_INSTALLATION = 0x00000002,
                /// <summary>
                /// Manutenzione.
                /// </summary>
                SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001,
                /// <summary>
                /// Problema con MMC.
                /// </summary>
                SHTDN_REASON_MINOR_MMC = 0x00000019,
                /// <summary>
                /// Connettività di rete.
                /// </summary>
                SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014,
                /// <summary>
                /// Scheda di rete.
                /// </summary>
                SHTDN_REASON_MINOR_NETWORKCARD = 0x00000009,
                /// <summary>
                /// Altro problema.
                /// </summary>
                SHTDN_REASON_MINOR_OTHER = SHTDN_REASON_MAJOR_OTHER,
                /// <summary>
                /// Altro evento driver.
                /// </summary>
                SHTDN_REASON_MINOR_OTHERDRIVER = 0x0000000e,
                /// <summary>
                /// Potenza.
                /// </summary>
                SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a,
                /// <summary>
                /// Processore.
                /// </summary>
                SHTDN_REASON_MINOR_PROCESSOR = 0x00000008,
                /// <summary>
                /// Riconfigurazione.
                /// </summary>
                SHTDN_REASON_MINOR_RECONFIG = 0x00000004,
                /// <summary>
                /// Sicurezza.
                /// </summary>
                SHTDN_REASON_MINOR_SECURITY = 0x00000013,
                /// <summary>
                /// Patch di sicurezza.
                /// </summary>
                SHTDN_REASON_MINOR_SECURITYFIX = 0x00000012,
                /// <summary>
                /// Disinstallazione patch di sicurezza.
                /// </summary>
                SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL = 0x00000018,
                /// <summary>
                /// Service pack.
                /// </summary>
                SHTDN_REASON_MINOR_SERVICEPACK = 0x00000010,
                /// <summary>
                /// Disinstallazione service pack.
                /// </summary>
                SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL = 0x00000016,
                /// <summary>
                /// Terminal Services.
                /// </summary>
                SHTDN_REASON_MINOR_TERMSRV = 0x00000020,
                /// <summary>
                /// Instabilità.
                /// </summary>
                SHTDN_REASON_MINOR_UNSTABLE = 0x00000006,
                /// <summary>
                /// Upgrade.
                /// </summary>
                SHTDN_REASON_MINOR_UPGRADE = 0x00000003,
                /// <summary>
                /// Problema con WMI.
                /// </summary>
                SHTDN_REASON_MINOR_WMI = 0x00000015,
                #endregion
                /// <summary>
                /// Ragione definita dall'utente.
                /// </summary>
                SHTDN_REASON_FLAG_USER_DEFINED = 0x40000000,
                /// <summary>
                /// Lo spegnimento è programmato.
                /// </summary>
                SHTDN_REASON_FLAG_PLANNED = 0x80000000
            }

            /// <summary>
            /// Opzioni di spegnimento.
            /// </summary>
            [Flags]
            public enum ShutdownFlags : uint
            {
                /// <summary>
                /// Tutte le sessioni sono forzatamente disconnesse, se questa opzione non è impostata la funzione imposta il codice di errore <see cref="Win32Constants.ERROR_SHUTDOWN_USERS_LOGGED_ON"/>.
                /// </summary>
                SHUTDOWN_FORCE_OTHERS = 0x1,
                /// <summary>
                /// La sessione di origine deve essere forzatamente disconnessa, se questa opzione non è impostata la disconnessione avviene in modo interattivo.
                /// </summary>
                /// <remarks>Se la disconnessione avviene in modo interattivo, lo spegnimento non è garantito.</remarks>
                SHUTDOWN_FORCE_SELF = 0x2,
                /// <summary>
                /// Ignora il timer di spegnimento e spegne il computer immediatamente.
                /// </summary>
                SHUTDOWN_GRACE_OVERRIDE = 0x20,
                /// <summary>
                /// Abilità lo spegnimento ibrido.
                /// </summary>
                SHUTDOWN_HYBRID = 0x200,
                /// <summary>
                /// Installa gli aggiornamento prima dello spegnimento.
                /// </summary>
                SHUTDOWN_INSTALL_UPDATES = 0x40,
                /// <summary>
                /// Il sistema operativo viene arrestato ma il computer non viene spento o riavviato.
                /// </summary>
                SHUTDOWN_NOREBOOT = 0x10,
                /// <summary>
                /// Il computer viene spento.
                /// </summary>
                SHUTDOWN_POWEROFF = 0x8,
                /// <summary>
                /// Il computer viene riavviato.
                /// </summary>
                SHUTDOWN_RESTART = 0x4,
                /// <summary>
                /// Il computer viene riavviato insieme a tutte le applicazione registrate.
                /// </summary>
                SHUTDOWN_RESTARTAPPS = 0x80,
                /// <summary>
                /// Il computer viene riavviato e vengono visualizzate le opzioni di boot.
                /// </summary>
                SHUTDOWN_RESTART_BOOT_OPTIONS = 0x400
            }

            /// <summary>
            /// Opzioni per la disconnessione dell'utente.
            /// </summary>
            public enum LogOffFlags : uint
            {
                /// <summary>
                /// Disconnette l'utente.
                /// </summary>
                /// <remarks>Questa opzione può essere utilizzata solo da i processi nella sessione interattiva.</remarks>
                EWX_LOGOFF,
                /// <summary>
                /// Il sistema non invia il messaggio WM_QUERYENDSESSION.
                /// </summary>
                /// <remarks>Se questa opzione è specificata le applicazioni potrebbero perdere dati.<br/><br/>
                /// Questa opzione non ha nessun effetto se Terminal Services è abilitato.</remarks>
                EWX_FORCE = 0x00000004,
                /// <summary>
                /// Forza la chiusura dei processi se non rispondono ai messaggi WM_QUERYENDSESSION e WM_ENDSESSION nel tempo limite.
                /// </summary>
                EWX_FORCEIFHUNG = 0x00000010
            }
            #endregion
            #region System Info Enumerations
            #region Generic
            /// <summary>
            /// Formato del nome del computer.
            /// </summary>
            public enum ComputerNameFormat
            {
                /// <summary>
                /// Nome NetBIOS del computer locale.
                /// </summary>
                NetBIOS,
                /// <summary>
                /// Nome DNS del computer locale.
                /// </summary>
                DnsHostname,
                /// <summary>
                /// Nome DNS del dominio assegnato al computer locale.
                /// </summary>
                DnsDomain,
                /// <summary>
                /// Nome DNS completo che identifica il computer in modo univoco.
                /// </summary>
                DnsFullyQualified,
                /// <summary>
                /// Nome NetBIOS del computer locale.
                /// </summary>
                PhysicalNetBIOS,
                /// <summary>
                /// Nome dell'host DNS del computer locale.
                /// </summary>
                PhysicalDnsHostname,
                /// <summary>
                /// Nome del dominio DNS assegnato al computer locale.
                /// </summary>
                PhysicalDnsDomain,
                /// <summary>
                /// Nome DNS completo che identifica univocamente il computer.
                /// </summary>
                PhysicalDnsFullyQualified,
                /// <summary>
                /// Non usato.
                /// </summary>
                Max
            }

            /// <summary>
            /// Stato di collegamento alla base del computer.
            /// </summary>
            public enum DockingState : uint
            {
                /// <summary>
                /// Connesso alla base.
                /// </summary>
                DOCKINFO_DOCKED = 0x2,
                /// <summary>
                /// Disconnesso dalla base.
                /// </summary>
                DOCKINFO_UNDOCKED = 0x1,
                /// <summary>
                /// L'informazioni è fornita dall'utente.
                /// </summary>
                DOCKINFO_USER_SUPPLIED = 0x4,
                /// <summary>
                /// Connesso alla base (utente).
                /// </summary>
                DOCKINFO_USER_DOCKED = DOCKINFO_USER_SUPPLIED | DOCKINFO_DOCKED,
                /// <summary>
                /// Disconnesso dalla base (utente).
                /// </summary>
                DOCKINFO_USER_UNDOCKED = DOCKINFO_USER_SUPPLIED | DOCKINFO_UNDOCKED
            }

            /// <summary>
            /// Tipo di firmware.
            /// </summary>
            public enum FirmwareType
            {
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                Unknown,
                /// <summary>
                /// BIOS.
                /// </summary>
                Bios,
                /// <summary>
                /// UEFI.
                /// </summary>
                Uefi,
                /// <summary>
                /// Non implementato.
                /// </summary>
                Max
            }

            /// <summary>
            /// Tipo di prodotto Windows.
            /// </summary>
            public enum ProductName : uint
            {
                [Description("Business")]
                Business = 0x00000006,

                [Description("BusinessN")]
                BusinessN = 0x00000010,

                [Description("HPC Edition")]
                HPCEdition = 0x00000012,

                [Description("Windows 10 Home")]
                Windows10Home = 0x00000065,

                [Description("Windows 10 Home China")]
                Windows10HomeChina = 0x00000063,

                [Description("Windows 10 Home N")]
                Windows10HomeN = 0x00000062,

                [Description("Windows 10 Home Single Language")]
                Windows10HomeSingleLanguage = 0x00000064,

                [Description("Windows 10 Education")]
                Windows10Education = 0x00000079,

                [Description("Windows 10 Education N")]
                Windows10EducationN = 0x0000007A,

                [Description("Windows 10 Enterprise")]
                Windows10Enterprise = 0x00000004,

                [Description("Windows 10 Enterprise E")]
                Windows10EnterpriseE = 0x00000046,

                [Description("Windows 10 Enterprise Evaluation")]
                Windows10EnterpriseEvaluation = 0x00000048,

                [Description("Windows 10 Enterprise N")]
                Windows10EnterpriseN = 0x0000001B,

                [Description("Windows 10 Enterprise N Evalution")]
                Windows10EnterpriseNEvaluation = 0x00000054,

                [Description("Windows 10 Enterprise 2015 LTSB")]
                Windows10Enterprise2015LTSB = 0x0000007D,

                [Description("Windows 10 Enteprise 2015 LTSB Evaluation")]
                Windows10Enterprise2015LTSBEvaluation = 0x00000081,

                [Description("Windows 10 Enterprise 2015 LTSB N")]
                Windows10Enterprise2015LTSBN = 0x0000007E,

                [Description("Windows 10 Enterprise 2015 LTSB N Evaluation")]
                Windows10Enterprise2015LTSBNEvaluation = 0x00000082,

                [Description("Home Basic")]
                HomeBasic = 0x00000002,

                [Description("Home Basic N")]
                HomeBasicN = 0x00000005,

                [Description("Home Premium")]
                HomePremium = 0x00000003,

                [Description("Home Premium N")]
                HomePremiumN = 0x0000001A,

                [Description("Windows 10 IoT Core")]
                Windows10IoTCore = 0x0000007B,

                [Description("Windows 10 IoT Core Commercial")]
                Windows10IoTCoreCommercial = 0x00000083,

                [Description("Windows 10 Mobile")]
                Windows10Mobile = 0x00000068,

                [Description("Windows 10 Mobile Enterprise")]
                Windows10MobileEnterprise = 0x00000085,

                [Description("Windows 10 Pro For Workstations")]
                Windows10ProForWorkstations = 0x000000A1,

                [Description("Windows 10 Pro For Workstations N")]
                Windows10ProForWorkstationsN = 0x000000A2,

                [Description("Windows 10 Pro")]
                Windows10Pro = 0x00000030,

                [Description("Windows 10 Pro N")]
                Windows10ProN = 0x00000031,

                [Description("Professial with Media Center")]
                ProfessionalWithMediaCenter = 0x00000067,

                [Description("Starter")]
                Starter = 0x0000000B,

                [Description("Starter N")]
                StarterN = 0x0000002F,

                [Description("Ultimate")]
                Ultimate = 0x00000001,

                [Description("Ultimate N")]
                UltimateN = 0x0000001C,

                [Description("Unknown")]
                Undefined = 0x00000000
            }
            #endregion
            #region Processor
            /// <summary>
            /// Architettura processore.
            /// </summary>
            public enum ProcessorArchitecture : ushort
            {
                /// <summary>
                /// x64.
                /// </summary>
                AMD64 = 9,
                /// <summary>
                /// ARM.
                /// </summary>
                ARM = 5,
                /// <summary>
                /// ARM64.
                /// </summary>
                ARM64 = 12,
                /// <summary>
                /// Intel Itanium.
                /// </summary>
                IA64 = 6,
                /// <summary>
                /// x86.
                /// </summary>
                Intel = 0,
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                Unknown = 0xffff
            }

            /// <summary>
            /// Funzionalità processore.
            /// </summary>
            public enum ProcessorFeature : uint
            {
                /// <summary>
                /// Le istruzioni load/store a 64 bit atomiche sono disponibili.
                /// </summary>
                [Description("Load/Store 64-bit atomic")]
                LoadStore64Bit = 25,
                /// <summary>
                /// L'istruzione divide è disponibile.
                /// </summary>
                [Description("ARM divide")]
                ARMDivide = 24,
                /// <summary>
                /// La cache esterna è disponibile.
                /// </summary>
                [Description("ARM External Cache")]
                ARMExternalCache = 26,
                /// <summary>
                /// Le istruzioni multiply-accumulate per i numeri floating point sono disponibili.
                /// </summary>
                [Description("Floating-point multiply/accumulate instructions")]
                ARMFmacInstructions,
                /// <summary>
                /// I registri VFP/Neon 32 x 64 bit sono presenti.
                /// </summary>
                [Description("ARM VFP/Neon 32 x 64 bit")]
                ARMVfp32Registers = 18,
                /// <summary>
                /// Il set di istruzioni 3D-Now è disponibile.
                /// </summary>
                [Description("3D Now instructions")]
                Instructions3DNow = 7,
                /// <summary>
                /// I canali del processore sono abilitati.
                /// </summary>
                [Description("Channels enabled")]
                ChannelsEnabled = 16,
                /// <summary>
                /// L'operazione di confronto e scambio atomica è disponibile.
                /// </summary>
                [Description("Compare Exchange Double")]
                CompareExchangeDouble = 2,
                /// <summary>
                /// L'operazione di confronto e scambio a 128 bit atomica è disponibile.
                /// </summary>
                [Description("Compare Exchange 128-bit")]
                CompareExchange128 = 14,
                /// <summary>
                /// L'operazione di confronto a 64 bit e scambio a 128 bit atomica è disponibile.
                /// </summary>
                [Description("Compare 64-bit Exchange 128-bit")]
                Compare64Exchange128 = 15,
                /// <summary>
                /// _fastfail() è disponibile.
                /// </summary>
                [Description("Fastfail available")]
                Fastfail = 23,
                /// <summary>
                /// Le operazioni floating point sono emulate tramite software.
                /// </summary>
                [Description("Floating point ops emulated")]
                FloatingPointEmulated = 1,
                /// <summary>
                /// Su processori Pentium, un errore di precisione può raramente accadere.
                /// </summary>
                [Description("Floating point precision error")]
                FloatingPointPrecisionError = 0,
                /// <summary>
                /// Il set di istruzioni MMX è disponibile.
                /// </summary>
                [Description("MMX Instructions")]
                MMXInstructions = 3,
                /// <summary>
                /// Data Execution Prevention è abilitata.
                /// </summary>
                [Description("DEP enabled")]
                NXEnabled = 12,
                /// <summary>
                /// Il processore supporta PAE (Physical Address Extension).
                /// </summary>
                /// <remarks>Tutti i processori a 64 bit supportano questa caratteristica.</remarks>
                [Description("Physical Address Extension enabled")]
                PhysicalAddressExtensionEnabled = 9,
                /// <summary>
                /// L'istruzione RDTSC è disponibile.
                /// </summary>
                [Description("RDTSC instruction available")]
                RDTSCInstruction = 8,
                /// <summary>
                /// Le istruzioni RDFSBASE, RDGSBASE, WRFSBASE e WRGSBASE sono disponibile.
                /// </summary>
                [Description("RDFSBASE, RDGSBASE, WRFSBASE and WRGSBASE instructions available")]
                RDWRFSGSBASEInstructions = 22,
                /// <summary>
                /// La traduzione degli indirizzi di secondo livello sono supportate dall'hardware.
                /// </summary>
                [Description("Second Level Address Translation supported")]
                SecondLevelAddressTranslation = 20,
                /// <summary>
                /// Il set di istruzioni SSE3 è disponibile.
                /// </summary>
                [Description("SSE3 instruction set available")]
                SSE3Instructions = 13,
                /// <summary>
                /// La virtualizzazione è abilitata nel firmware ed è resa disponibile dal sistema operativo.
                /// </summary>
                [Description("Virtualization available")]
                VirtualizationEnabled = 21,
                /// <summary>
                /// Il set di istruzioni SSE è disponibile.
                /// </summary>
                [Description("SSE instruction set available")]
                SSEInstructions = 6,
                /// <summary>
                /// Il set di istruzioni SSE2 è disponibile.
                /// </summary>
                [Description("SSE2 instruction set available")]
                SSE2Instructions = 10,
                /// <summary>
                /// Il processore implementa le istruzioni XSAVE e XRSTOR.
                /// </summary>
                [Description("XSAVE and XRSTOR instructions implemented")]
                XSave = 17,
                /// <summary>
                /// Il processore implementa il set di istruzioni ARM v8.
                /// </summary>
                [Description("ARM v8 instruction set available")]
                ARMv8Instructions = 29,
                /// <summary>
                /// Il processore implementa il set di istruzioni criptografiche di sicurezza extra ARM v8.
                /// </summary>
                [Description("ARM v8 Crypto instruction set available")]
                ARMv8CryptoInstructions,
                /// <summary>
                /// Il processore implementa le istruzioni ARM v8 CRC32 extra.
                /// </summary>
                [Description("ARM v8 CRC32 instructions implemented")]
                ARMv8CRC32Instructions,
                /// <summary>
                /// Il processore implementa le istruzioni atomiche ARM v8.1.
                /// </summary>
                [Description("ARM v8.1 atomic istructions implemented")]
                ARMv81AtomicInstructions = 34
            }

            /// <summary>
            /// Tipo di relazione tra i processori logici.
            /// </summary>
            public enum ProcessorRelationshipType
            {
                /// <summary>
                /// Informazioni sui processori logici che condividono una cache,
                /// </summary>
                RelationCache = 2,
                /// <summary>
                /// Informazioni sui processori logici che sono parte dello stesso nodo NUMA.
                /// </summary>
                RelationNumaNode = 1,
                /// <summary>
                /// Informazioni sui processori logici che condividono un core.
                /// </summary>
                RelationProcessorCore = 0,
                /// <summary>
                /// Informazioni sui processori logici che condividono un package fisico.
                /// </summary>
                RelationProcessorePackage = 3,
                /// <summary>
                /// Informazioni sui processori logici che fanno parte dello stesso gruppo.
                /// </summary>
                RelationGroup = 4,
                /// <summary>
                /// Informazioni sui processori logici per tutti i tipi di informazioni.
                /// </summary>
                RelationAll = 0xffff
            }
            #endregion
            #region System Parameters
            /// <summary>
            /// Parametri di sistema relativi alle funzionalità di accessibilità.
            /// </summary>
            public enum SystemAccessibilityParameters : uint
            {
                /// <summary>
                /// Tempo di timeout associato con le funzionalità di accessibilità.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.ACCESSTIMEOUT"/>, il membro <see cref="Win32Structures.ACCESSTIMEOUT.Size"/> della struttura deve essere impostato alla dimensione di quest'ultima.</remarks>
                SPI_GETACCESSTIMEOUT = 0x003C,
                /// <summary>
                /// Determina se le descrizioni audio sono abilitate o disabilitate.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.AUDIODESCRIPTION"/>, il membro <see cref="Win32Structures.AUDIODESCRIPTION.Size"/> della struttura deve essere impostato alla dimensione di quest'ultima.</remarks>
                SPI_GETAUDIODESCRIPTION = 0x0074,
                /// <summary>
                /// Determina se le animazioni sono abilitate.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETCLIENTAREAANIMATION = 0x1042,
                /// <summary>
                /// Determina se il contenuto sovrapposto è abilitato.
                /// </summary>
                /// <remarks>Il parametro pvParam deve puntare a un valore BOOL.</remarks>
                SPI_GETDISABLEOVERLAPPEDCONTENT = 0x1040,
                /// <summary>
                /// Determina se Filtro Tasti è attivo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.FILTERKEYS"/>, il membro <see cref="Win32Structures.FILTERKEYS.Size"/> della struttura deve essere impostato alla dimensione di quest'ultima.</remarks>
                SPI_GETFILTERKEYS = 0x0032,
                /// <summary>
                /// Altezza, in pixel, dei bordi superiori e inferiore del rettangolo di focus disegnato con la funzione DrawFocusRect.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore UINT.</remarks>
                SPI_GETFOCUSBORDERHEIGHT = 0x2010,
                /// <summary>
                /// Larghezza, in pixel, dei bordi superiori e inferiore del rettangolo di focus disegnato con la funzione DrawFocusRect.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore UINT.</remarks>
                SPI_GETFOCUSBORDERWIDTH = 0x200E,
                /// <summary>
                /// Informazioni sulla funzionalità di accessibilità Alto Contrasto.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.HIGHCONTRAST"/>, il membro <see cref="Win32Structures.HIGHCONTRAST.Size"/> della struttura deve essere impostato alla dimensione di quest'ultima.</remarks>
                SPI_GETHIGHCONTRAST = 0x0042,
                /// <summary>
                /// Tempo, in secondi, durante il quale i popup di notifica sono visualizzati.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore ULONG.</remarks>
                SPI_GETMESSAGEDURATION = 0x2016,
                /// <summary>
                /// Stato della funzionalità di blocco del tasto primario del mouse.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETMOUSECLICKLOCK = 0x101E,
                /// <summary>
                /// Tempo, in millisecondi, prima che il pulsante del mouse venga bloccato quando la funzionalità relativa è attiva.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore DWORD.</remarks>
                SPI_GETMOUSECLICKLOCKTIME = 0x2008,
                /// <summary>
                /// Informazioni sulla funzionalità MouseKeys. 
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.MOUSEKEYS"/>, il membro <see cref="Win32Structures.MOUSEKEYS.Size"/> della struttura deve essere impostato alla dimensione di quest'ultima.</remarks>
                SPI_GETMOUSEKEYS = 0x0036,
                /// <summary>
                /// Stato della funzionalità Mouse Sonar.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETMOUSESONAR = 0x101C,
                /// <summary>
                /// Stato della funzionalità Mouse Vanish.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETMOUSEVANISH = 0x1020,
                /// <summary>
                /// Determina se un'utilità di lettura schermo è in esecuzione.
                /// </summary>
                /// <remarks>Narrator, l'utilità di lettura schermo di Windows, non imposta questa informazione.<br/><br/>
                /// Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETSCREENREADER = 0x0046,
                /// <summary>
                /// Determina se la funzionalità Mostra Suoni è attiva.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETSHOWSOUNDS = 0x0038,
                /// <summary>
                /// Informazioni sulla funzionalità SoundSentry.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.SOUNDSENTRY"/>, il membro <see cref="Win32Structures.SOUNDSENTRY.Size"/> della struttura deve essere impostato alla dimensione di quest'ultima.</remarks>
                SPI_GETSOUNDSENTRY = 0x0040,
                /// <summary>
                /// Informazioni sulla funzionalità Tasti permanenti.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.STICKYKEYS"/>, il membro <see cref="Win32Structures.STICKYKEYS.Size"/> della struttura deve essere impostato alla dimensione di quest'ultima.</remarks>
                SPI_GETSTICKYKEYS = 0x003A,
                /// <summary>
                /// Informazioni sulla funzionalità ToggleKeys.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.TOGGLEKEYS"/>, il membro <see cref="Win32Structures.TOGGLEKEYS.Size"/> della struttura deve essere impostato alla dimensione di quest'ultima.</remarks>
                SPI_GETTOGGLEKEYS = 0x0034
            }

            /// <summary>
            /// Parametri di sistema relativi al desktop.
            /// </summary>
            public enum SystemDesktopParameters : uint
            {
                /// <summary>
                /// Determina se ClearType è abilitato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETCLEARTYPE = 0x1048,
                /// <summary>
                /// Percorso completo del bitmap usato per lo sfondo del desktop.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un buffer che riceverà una stringa a terminazione nulla.<br/>
                /// Il parametro uiParam deve essere impostato con la dimensione, in caratteri, di Param.</remarks>
                SPI_GETDESKWALLPAPER = 0x0073,
                /// <summary>
                /// Determina se l'effetto dell'ombra sul punto di rilascia è abilitato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETDROPSHADOW = 0x1024,
                /// <summary>
                /// Determina se i menù nativi hanno l'apparenza di base.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETFLATMENU = 0x1022,
                /// <summary>
                /// Determina se lo smoothing dei font è attivo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETFONTSMOOTHING = 0x004A,
                /// <summary>
                /// Valore di contrasto usato da ClearType.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo UINT.<br/><br/>
                /// I valori validi per il contrasto vanno da 1000 a 2200, il valore di default è 1400.</remarks>
                SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,
                /// <summary>
                /// Orientamento dello smoothing dei font.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore UINT.<br/><br/>
                /// I valori validi sono inclusi nell'enumerazione <see cref="FontSmoothingOrientation"/>.</remarks>
                SPI_GETFONTSMOOTHINGORIENTATION = 0x2012,
                /// <summary>
                /// Tipo di smoothing dei font.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore UINT.<br/><br/>
                /// I valori validi sono inclusi nell'enumerazione <see cref="FontSmoothingType"/>.</remarks>
                SPI_GETFONTSMOOTHINGTYPE = 0x200A,
                /// <summary>
                /// Dimensione della zona di lavoro nel monitor primario.
                /// </summary>
                /// <remarks>La zona di lavoro corrisponde alla porzione dello schermo non coperta dalla barra delle applicazioni o da toolbar delle applicazioni desktop.<br/><br/>
                /// Il parametro Param deve puntare a una struttura <see cref="Win32Structures.RECT"/> che riceve le coordinate della zona di lavoro, espressa in pixel fisici, la modalità di virtualizzazione DPI del chiamante non ha effetto sull'output.</remarks>
                SPI_GETWORKAREA = 0x0030
            }

            /// <summary>
            /// Parametri di sistema relativi alle icone.
            /// </summary>
            public enum SystemIconParameters : uint
            {
                /// <summary>
                /// Metriche associate con le icone.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.ICONMETRICS"/>, il membro <see cref="Win32Structures.ICONMETRICS.Size"/> deve essere impostato alla dimensione, in byte, di quest'ultima.</remarks>
                SPI_GETICONMETRICS = 0x002D,
                /// <summary>
                /// Informazioni sul font logico per il font corrente dei titoli delle icone.
                /// </summary>
                /// <remarks>Il parametro uiParam deve specificare la dimensione di una struttura <see cref="Win32Structures.LOGFONT"/>, il parametro Param deve puntare a tale struttura.</remarks>
                SPI_GETICONTITLELOGFONT = 0x001F,
                /// <summary>
                /// Determina se i titoli delle icone vanno a capo automaticamente.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETICONTITLEWRAP = 0x0019,
                /// <summary>
                /// Larghezza, in pixel, della cella di un icona.
                /// </summary>
                /// <remarks>Per recuperare il valore il parametro Param deve puntare a un intero (32 bit).</remarks>
                SPI_ICONHORIZONTALSPACING = 0x000D,
                /// <summary>
                /// Altezza, in pixel, della cella di un icona.
                /// </summary>
                /// <remarks>Per recuperare il valore il parametro Param deve puntare a un intero (32 bit).</remarks>
                SPI_ICONVERTICALSPACING = 0x0018
            }

            /// <summary>
            /// Parametri di sistema relativi all'input.
            /// </summary>
            public enum SystemInputParameters : uint
            {
                /// <summary>
                /// Determina se l'allarme è attivo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile BOOL.</remarks>
                SPI_GETBEEP = 0x0001,
                /// <summary>
                /// Determina se un'applicazione può resettare il time dello screensaver tramite la funzione SendInput.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile BOOL.</remarks>
                SPI_GETBLOCKSENDINPUTRESETS = 0x1026,
                /// <summary>
                /// L'impostazione attuale del feedback UI per i contatti.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore ULONG, i valori validi sono definiti nell'enumerazione <see cref="ContactVisualizationSetting"/>.</remarks>
                SPI_GETCONTACTVISUALIZATION = 0x2018,
                /// <summary>
                /// Identificatore della localizzazione dell'input per la lingua di input di default.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo HKL (<see cref="IntPtr"/>).</remarks>
                SPI_GETDEFAULTINPUTLANG = 0x0059,
                /// <summary>
                /// L'impostazione attuale del feedback UI per i gesti.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore ULONG, i valori validi sono definiti nell'enumerazione <see cref="GestureVisualizationSetting"/>.</remarks>
                SPI_GETGESTUREVISUALIZATION = 0x201A,
                /// <summary>
                /// Determina se le lettere per accedere ai menù sono sempre sottolineate.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile BOOL.</remarks>
                SPI_GETKEYBOARDCUES = 0x100A,
                /// <summary>
                /// Tempo di ripetizione tasti della tastiera.
                /// </summary>
                /// <remarks>Questo valore può andare da 0 (250 ms) a 3 (1 s), l'effettivo valore del ritardo dipende dall'hardware.<br/><br/>
                /// Il parametro Param deve puntare a una variabile intera (32 bit).</remarks>
                SPI_GETKEYBOARDDELAY = 0x0016,
                /// <summary>
                /// Determina se l'utente utilizza principalmente la tastiera al posto del mouse e le applicazione devono visualizzare interfacce per la tastiera altrimenti nascoste.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un variabile di tipo BOOL.</remarks>
                SPI_GETKEYBOARDPREF = 0x0044,
                /// <summary>
                /// Numero di ripetizione dei tasti al secondo.
                /// </summary>
                /// <remarks>Questo valore può andare da 0 (2,5 ripetizioni) a 31 (30 ripetizioni), l'effettivo valore delle ripetizioni dipende dall'hardware.<br/><br/>
                /// Il parametro Param deve puntare a una variabile di tipo DWORD.</remarks>
                SPI_GETKEYBOARDSPEED = 0x000A,
                /// <summary>
                /// Valori delle soglie del mouse e della sua accelerazione.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un array di tre interi (32 bit).</remarks>
                SPI_GETMOUSE = 0x0003,
                /// <summary>
                /// Altezza, in pixel, del rettangolo nel quale il puntatore deve trovarsi per generare un evento WM_MOUSEHOVER.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo UINT.</remarks>
                SPI_GETMOUSEHOVERHEIGHT = 0x0064,
                /// <summary>
                /// Tempo, in millisecondi, durante il quale il puntatore del mouse deve restare nel rettangolo per generare un evento WM_MOUSEHOVER.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo UINT.</remarks>
                SPI_GETMOUSEHOVERTIME = 0x0066,
                /// <summary>
                /// Larghezza, in pixel, del rettangolo nel quale il puntatore deve trovarsi per generare un evento WM_MOUSEHOVER.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo UINT.</remarks>
                SPI_GETMOUSEHOVERWIDTH = 0x0062,
                /// <summary>
                /// Velocità attuale del mouse.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera (32 bit).<br/><br/>
                /// Il valore recuperato va da 1 a 20.</remarks>
                SPI_GETMOUSESPEED = 0x0070,
                /// <summary>
                /// Determina se la traccia del puntatore del mouse è abilitata.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera (32 bit).<br/><br/>
                /// Se il valore recuperato è 1 oppure 0, la funzionalità è disattivata, se il valore è maggiore di 1, la funzionalità è attivata e il valore rappresenta il numero di cursori disegnati sulla traccia.</remarks>
                SPI_GETMOUSETRAILS = 0x005E,
                /// <summary>
                /// Funzionamento in caso di pressione della rotella del mouse.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo DWORD.<br/><br/>
                /// I valori validi sono quelli presenti nell'enumerazione <see cref="MouseWheelRoutingSetting"/>.</remarks>
                SPI_GETMOUSEWHEELROUTING = 0x201C,
                /// <summary>
                /// L'impostazione attuale del feedback UI per i gesti con la penna.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore ULONG, i valori validi sono definiti nell'enumerazione <see cref="PenVisualizationSetting"/>.</remarks>
                SPI_GETPENVISUALIZATION = 0x201E,
                /// <summary>
                /// Determina se lo spostamento automatico del cursore del mouse sul pulsante di default è attivo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETSNAPTODEFBUTTON = 0x005F,
                /// <summary>
                /// Determina se la barra della lingua è attiva.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETSYSTEMLANGUAGEBAR = 0x1050,
                /// <summary>
                /// Determina se le impostazioni correnti di input hanno ambito locale (per thread, TRUE) o globale (sessione, FALSE).
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETTHREADLOCALINPUTSETTINGS = 0x104E,
                /// <summary>
                /// Numero di caratteri da scorrere quando la rotella orizzontale del mouse viene mossa.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore UINT.</remarks>
                SPI_GETWHEELSCROLLCHARS = 0x006C,
                /// <summary>
                /// Numero di linee da scorrere quando la rotella verticale del mouse viene mossa.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore UINT.</remarks>
                SPI_GETWHEELSCROLLLINES = 0x0068
            }

            /// <summary>
            /// Parametri di sistema relativi ai menù.
            /// </summary>
            public enum SystemMenuParameters : uint
            {
                /// <summary>
                /// Determina se i menù a comparsa sono allineati a sinistra o a destra, relativamente all'oggetto di menù.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.<br/><br/>
                /// TRUE in caso di allineamento a sinistra, FALSE in caso di allineamento a destra.</remarks>
                SPI_GETMENUDROPALIGNMENT = 0x001B,
                /// <summary>
                /// Determina se l'animazione di scomparsa dei menù è abilitata.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETMENUFADE = 0x1012,
                /// <summary>
                /// Tempo, in millisecondi, di attesa prima che il sistema mostra un menù quando il cursore del mouse si trova sopra un sottomenù.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore DWORD.</remarks>
                SPI_GETMENUSHOWDELAY = 0x006A
            }

            /// <summary>
            /// Parametri di sistema relativi al salvaschermo.
            /// </summary>
            public enum SystemScreenSaverParameters : uint
            {
                /// <summary>
                /// Determina se il salvaschermo è attivo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETSCREENSAVEACTIVE = 0x0010,
                /// <summary>
                /// Determina se il salvaschermo è attualmente in esecuzione sulla window station del processo chiamante.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETSCREENSAVERUNNING = 0x0072,
                /// <summary>
                /// Determina se il salvaschermo richiede una password per mostrare il desktop.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETSCREENSAVESECURE = 0x0076,
                /// <summary>
                /// Timeout del salvaschermo, in secondi.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera (32 bit).</remarks>
                SPI_GETSCREENSAVETIMEOUT = 0x000E
            }

            /// <summary>
            /// Parametri di sistema relativi ai timeout.
            /// </summary>
            public enum SystemTimeoutParameters : uint
            {
                /// <summary>
                /// Numero di millisecondi che un thread può non rispondere ai messaggi prima che il sistema lo considera bloccato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera (32 bit).</remarks>
                SPI_GETHUNGAPPTIMEOUT = 0x0078,
                /// <summary>
                /// Numero di millisecondi di attesa dopo i quali il sistema termina un'applicazione che non risponde a una richiesta di chiusura.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera (32 bit).</remarks>
                SPI_GETWAITTOKILLTIMEOUT = 0x007A,
                /// <summary>
                /// Numero di millisecondi di attesa dopo i quali Gestione Controllo Servizi termina un servizio che non ha risposto a una richiesta di chiusura.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera (32 bit).</remarks>
                SPI_GETWAITTOKILLSERVICETIMEOUT = 0x007C
            }

            /// <summary>
            /// Parametri di sistema relativi all'interfaccia utente.
            /// </summary>
            public enum SystemUIParameters : uint
            {
                /// <summary>
                /// Determina se l'effetto di scorrimento delle caselle combinate è abilitato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETCOMBOBOXANIMATION = 0x1004,
                /// <summary>
                /// Determina se il cursore del mouse ha un'ombra attorno.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETCURSORSHADOW = 0x101A,
                /// <summary>
                /// Determina se l'effetto gradiente per le barre del titolo è abilitato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETGRADIENTCAPTIONS = 0x1008,
                /// <summary>
                /// Determina se il tracciamento degli elementi dell'interfaccia utente è abilitato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETHOTTRACKING = 0x100E,
                /// <summary>
                /// Determina se l'effetto di scorrimento delle liste è abilitato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006,
                /// <summary>
                /// Determina se le animazioni dei menù sono attive.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETMENUANIMATION = 0x1002,
                /// <summary>
                /// Determina se l'effetto di scomparsa della selezione di un menù è abilitato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETSELECTIONFADE = 0x1014,
                /// <summary>
                /// Determina se l'animazione dei tooltip è abilitata.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETTOOLTIPANIMATION = 0x1016,
                /// <summary>
                /// Determina se l'animazione di scomparsa dei tooltip è abilitata.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETTOOLTIPFADE = 0x1018,
                /// <summary>
                /// Determia se gli effetti dell'interfaccia utente sono attivi.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore BOOL.</remarks>
                SPI_GETUIEFFECTS = 0x103E
            }

            /// <summary>
            /// Parametri di sistema relativi alle finestre.
            /// </summary>
            public enum SystemWindowParameters : uint
            {
                /// <summary>
                /// Determina se il tracciamento della finestra attiva è attivo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETACTIVEWINDOWTRACKING = 0x1000,
                /// <summary>
                /// Determina se le finestra attivare tramite il tracciamento devono essere portate in primo piano.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETACTIVEWNDTRKZORDER = 0x100C,
                /// <summary>
                /// Millisecondi che rappresentano il ritardo del tracciamento fineste attive.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore DWORD.</remarks>
                SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002,
                /// <summary>
                /// Effetti di animazione associati alle azioni dell'utente.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.ANIMATIONINFO"/>, il campo <see cref="Win32Structures.ANIMATIONINFO.Size"/> della struttura deve essere impostato con la dimensione, in byte, di quest'ultima.</remarks>
                SPI_GETANIMATION = 0x0048,
                /// <summary>
                /// Fattore moltiplicativo che determina la larghezza del bordo di ridimensionamento di una finestra.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera (32 bit).</remarks>
                SPI_GETBORDER = 0x0005,
                /// <summary>
                /// 
                /// </summary>
                SPI_GETCARETWIDTH = 0x2006,
                /// <summary>
                /// Determina se il blocco delle finestre quando vengono spostate ai lati del monitor o dei monitori.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETDOCKMOVING = 0x0090,
                /// <summary>
                /// Determina se una finestra ingrandita viene ripristina quando la sua barra del titolo viene trascinata.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETDRAGFROMMAXIMIZE = 0x008C,
                /// <summary>
                /// Determina se il trascinamento di finestre è attivo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo BOOL.</remarks>
                SPI_GETDRAGFULLWINDOWS = 0x0026,
                /// <summary>
                /// Numero di volte che la funzione SetForegoundWindow farà lampeggiare il pulsante sulla barra delle applicazioni quando una richiesta di cambio è stata rifiutata.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo DWORD.</remarks>
                SPI_GETFOREGROUNDFLASHCOUNT = 0x2004,
                /// <summary>
                /// Tempo, in millisecondi, dopo input dell'utente, durante il quale il sistema non permette alle applicazioni di mettersi in primo piano.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile di tipo DWORD.</remarks>
                SPI_GETFOREGOUNDLOCKTIMEOUT = 0x2000,
                /// <summary>
                /// Metriche associate alle finestre minimizzate.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.MINIMIZEDMETRICS"/>, il campo <see cref="Win32Structures.MINIMIZEDMETRICS.Size"/> deve essere impostato alla dimensione, in byte, di quest'ultima.</remarks>
                SPI_GETMINIMIZEDMETRICS = 0x002B,
                /// <summary>
                /// Soglia, in pixel, rispetto al bordo del monitor o dei monitor, dove il blocco viene causato dal movimento del mouse verso quel bordo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore DWORD.</remarks>
                SPI_GETMOUSEDOCKTHRESHOLD = 0x007E,
                /// <summary>
                /// Soglia, in pixel, rispetto al bordo del monitor o dei monitor, dove lo sblocco viene causato dal movimento del mouse lontano da quel bordo.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore DWORD.</remarks>
                SPI_GETMOUSEDRAGOUTTHRESHOLD = 0x0084,
                /// <summary>
                /// Soglia, in pixel, rispetto alla parte superiore del monitor o dei monitor, dove una finestra verticalmente ingrandita viene ripristinata quando viene trascinata con il mouse.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a un valore DWORD.</remarks>
                SPI_GETMOUSESIDEMOVETHRESHOLD = 0x0088,
                /// <summary>
                /// Metriche associato con l'area non client di finestre non minimizzate.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una struttura <see cref="Win32Structures.NONCLIENTMETRICS"/>, il membro <see cref="Win32Structures.NONCLIENTMETRICS.Size"/> deve essere impostato con la dimensione, in byte, di quest'ultima.</remarks>
                SPI_GETNONCLIENTMETRICS = 0x0029,
                /// <summary>
                /// Soglia, in pixel, che causa il blocco di una finestra al bordo dello schermo o degli schermi tramite trascinamento con una penna.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera.</remarks>
                SPI_GETPENDOCKTHRESHOLD = 0x0080,
                /// <summary>
                /// Soglia, in pixel, che causa lo sblocco di una finestra dal bordo dello schermo o degli schermi tramite trascinamento verso il centro con una penna.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera.</remarks>
                SPI_GETPENDRAGOUTTHRESHOLD = 0x0086,
                /// <summary>
                /// Soglia, in pixel, che causa il ripristino di una finestra verticalmente massimizzata quando trascinata con una penna verso la parte alta del monitor.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile intera.</remarks>
                SPI_GETPENSIDEMOVETHRESHOLD = 0x008A,
                /// <summary>
                /// Determina se le finestre di stato IME sono visibile (per utente).
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile BOOL.</remarks>
                SPI_GETSHOWIMEUI = 0x006E,
                /// <summary>
                /// Determina se una finestra viene verticalmente massimizzata quando posizionata nella parte superiore o inferiore del monitor o dei monitor.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile BOOL.</remarks>
                SPI_GETSNAPSIZING = 0x008E,
                /// <summary>
                /// Determina se l'ordinamento delle finestre è abilitato.
                /// </summary>
                /// <remarks>Il parametro Param deve puntare a una variabile BOOL.</remarks>
                SPI_GETWINARRANGING = 0x0082
            }

            /// <summary>
            /// Opzioni di aggiornamento del profilo utente.
            /// </summary>
            [Flags]
            public enum SystemParametersUserProfileUpdateOptions : uint
            {
                /// <summary>
                /// La nuova impostazione viene scritta nel profilo utente.
                /// </summary>
                SPIF_UPDATEINIFILE = 0x0001,
                /// <summary>
                /// Invia il messaggio WM_SETTINGCHANGE dopo l'aggiornamento del profilo utente.
                /// </summary>
                SPIF_SENDWININICHANGE = 0x0002,
                /// <summary>
                /// Uguale a <see cref="SPIF_SENDWININICHANGE"/>.
                /// </summary>
                SPIF_SENDCHANGE = SPIF_SENDWININICHANGE
            }
            #endregion
            #endregion
            #region Accessibility Enumerations
            /// <summary>
            /// Impostazioni del timeout delle funzionalità di accessibilità.
            /// </summary>
            [Flags]
            public enum AccessibilityTimeoutSettings : uint
            {
                /// <summary>
                /// Se impostato, il sistema operativo riproduce il suono discendente di un allarme quando il timeout scade e le funzionalità di accessiblità sono disattivate.
                /// </summary>
                ATF_ONOFFFEEDBACK = 0x00000002,
                /// <summary>
                /// Se impostato, il timeout è attivato.
                /// </summary>
                ATF_TIMEOUTON = 0x00000001
            }

            /// <summary>
            /// Proprietà della funzionalità Filtro Tasti.
            /// </summary>
            [Flags]
            public enum FilterKeysSettings : uint
            {
                /// <summary>
                /// Indica se la funzionalità è disponibile.
                /// </summary>
                FKF_AVAILABLE = 0x00000002,
                /// <summary>
                /// Il computer riproduce un suono quando un tasto viene premuto.
                /// </summary>
                /// <remarks>Se la funzionalità Tasti lenti è attiva viene generato un suono quando il tasto viene premuto e quando la pressione del tasto è stato accettata.</remarks>
                FKF_CLICKON = 0x00000040,
                /// <summary>
                /// Indica se la funzionalità è attiva.
                /// </summary>
                FKF_FILTERKEYSON = 0x00000001,
                /// <summary>
                /// Indica se l'utente può attivare o disattivare la funzionalità premendo lo SHIFT sinistro per 8 secondi.
                /// </summary>
                FKF_HOTKEYACTIVE = 0x00000004,
                /// <summary>
                /// Se impostato, il computer riproduce un suono di allarme quando l'utente attiva o disattiva la funzionalità usando l'hotkey.
                /// </summary>
                FKF_HOTKEYSOUND = 0x00000010
            }

            /// <summary>
            /// Impostazioni della funzionalità Alto Contrasto.
            /// </summary>
            [Flags]
            public enum HighContrastSettings : uint
            {
                /// <summary>
                /// Indica se la funzionalità è attiva.
                /// </summary>
                HCF_HIGHCONTRASTON = 0x00000001,
                /// <summary>
                /// Indica se la funzionalità è disponibile.
                /// </summary>
                HCF_AVAILABLE = 0x00000002,
                /// <summary>
                /// Indica se la funzionalità può essere attivata e disattivata con la combinazione di tasti ALT sinistro + SHIFT sinitro + PRINT SCREEN.
                /// </summary>
                HCF_HOTKEYACTIVE = 0x00000004,
                /// <summary>
                /// Indica se una finestra di dialogo viene mostrata quando la funzionalità viene attivata tramite la combinazione di tasti.
                /// </summary>
                HCF_CONFIRMHOTKEY = 0x00000008,
                /// <summary>
                /// Indice se il suono di un allarme viene riprodotto quando l'utente attiva la funzionalità tramite la combinazione di tasti.
                /// </summary>
                HCF_HOTKEYSOUND = 0x00000010,
                /// <summary>
                /// Indica se la combinazione di tasti per attivare la funzionalità è disponibile.
                /// </summary>
                HCF_HOTKEYAVAILABLE = 0x00000040,
                /// <summary>
                /// Indice di non cambiare il tema.
                /// </summary>
                HCF_OPTION_NOTHEMECHANGE = 0x00001000,


            }

            /// <summary>
            /// Impostazioni della funzionalità MouseKeys.
            /// </summary>
            [Flags]
            public enum MouseKeysSettings : uint
            {
                /// <summary>
                /// Indica se la funzionalità è disponibile.
                /// </summary>
                MKF_AVAILABLE = 0x00000002,
                /// <summary>
                /// Indica se la fuzionalità può essere attivata e disattivata con la combinazione di tasti ALT sinistro + SHIFT sinistro + NUM LOCK.
                /// </summary>
                MKF_HOTKEYACTIVE = 0x00000004,
                /// <summary>
                /// Indice se il suono di un allarme viene riprodotto quando l'utente attiva la funzionalità tramite la combinazione di tasti.
                /// </summary>
                MKF_HOTKEYSOUND = 0x00000010,
                /// <summary>
                /// Indica se la funzionalità è attiva.
                /// </summary>
                MKF_MOUSEKEYSON = 0x00000001
            }

            /// <summary>
            /// Impostazioni della funzionalità SoundSentry.
            /// </summary>
            [Flags]
            public enum SoundSentrySettings : uint
            {
                /// <summary>
                /// Indica se la funzionalità è disponibile.
                /// </summary>
                SSF_AVAILABLE = 0x00000002,
                /// <summary>
                /// Indica se la funzionalità è attiva.
                /// </summary>
                SSF_SOUNDSENTRYON = 0x00000001
            }

            /// <summary>
            /// Effetti visuali della funzionalità SoundSentry.
            /// </summary>
            public enum SoundSentryWindowsEffect : uint
            {
                /// <summary>
                /// Segnale visuale personalizzato.
                /// </summary>
                SSWF_CUSTOM = 4,
                /// <summary>
                /// Il display lampeggia.
                /// </summary>
                SSWF_DISPLAY = 3,
                /// <summary>
                /// Nessun segnale visuale.
                /// </summary>
                SSWF_NONE =  0,
                /// <summary>
                /// La barra del titolo della finestra attiva lampeggia.
                /// </summary>
                SSWF_TITLE = 1,
                /// <summary>
                /// La finestra attiva lampeggia.
                /// </summary>
                SSWF_WINDOW = 2
            }

            /// <summary>
            /// Impostazioni di Tasti Permanenti.
            /// </summary>
            [Flags]
            public enum StickyKeysSettings : uint
            {
                /// <summary>
                /// Indica se il sistema riproduce un suono quando l'utente aggancia, blocca o rilascia un tasto di controllo usando la funzionalitò.
                /// </summary>
                SKF_AUDIBLEFEEDBACK = 0x00000040,
                /// <summary>
                /// Indica se la funzionalità è disponibile.
                /// </summary>
                SKF_AVAILABLE = 0x00000002,
                /// <summary>
                /// Indica se è possibile attivare o disattivare la funzionalità premendo il tasto SHIFT per 5 volte.
                /// </summary>
                SKF_HOTKEYACTIVE = 0x00000004,
                /// <summary>
                /// Indica che il sistema riproduce un suono di allarme quando l'utente attiva o disattiva la funzionalità tramite l'hotkey.
                /// </summary>
                SKF_HOTKEYSOUND = 0x00000010,
                /// <summary>
                /// Indica se la funzionalità è abilitata.
                /// </summary>
                SKF_STICKYKEYSON = 0x00000001,
                /// <summary>
                /// Indica se la pressione di un tasto di controllo due volte di fila lo blocca fino a una terza pressione da parte dell'utente.
                /// </summary>
                SKF_TRISTATE = 0x00000080,
                /// <summary>
                /// Indica se rilasciare un tasto di controllo premuto in combinazione con qualunque altra chiave disattiva la funzionalità.
                /// </summary>
                SKF_TWOKEYSOFF = 0x00000100
            }

            /// <summary>
            /// Impostazioni della funzionalità ToggleKeys.
            /// </summary>
            [Flags]
            public enum ToggleKeysSettings : uint
            {
                /// <summary>
                /// Indice se la funzionalità è disponibile.
                /// </summary>
                TKF_AVAILABLE = 0x00000002,
                /// <summary>
                /// Indica se è possibile attivare o disattivare la funzionalità tenendo premuto NUM LOCK per 8 secondi.
                /// </summary>
                TKF_HOTKEYACTIVE = 0x00000004,
                /// <summary>
                /// Indica se il sistema riproduce il suono di un'allarme quando l'utente attiva o disattiva la funzionalità con l'hotkey.
                /// </summary>
                TKF_HOTKEYSOUND = 0x00000010,
                /// <summary>
                /// Indica se la funzionalità è attiva.
                /// </summary>
                TKF_TOGGLEKEYSON = 0x00000001
            }
            #endregion
            #region ClearType Enumerations
            /// <summary>
            /// Orientamento smoothing ClearType.
            /// </summary>
            public enum FontSmoothingOrientation : uint
            {
                /// <summary>
                /// Blu-verde-rosso.
                /// </summary>
                FE_FONTSMOOTHINGORIENTATIONBGR = 0x0000,
                /// <summary>
                /// Rosso-verde-blu.
                /// </summary>
                FE_FONTSMOOTHINGORIENTATIONRGB = 0x0001
            }

            /// <summary>
            /// Tipo di smoothing font.
            /// </summary>
            public enum FontSmoothingType : uint
            {
                /// <summary>
                /// Standard.
                /// </summary>
                FE_FONTSMOOTHINGSTANDARD = 0x0001,
                /// <summary>
                /// ClearType.
                /// </summary>
                FE_FONTSMOOTHINGCLEARTYPE = 0x0002
            }
            #endregion
            #region Font Enumerations
            /// <summary>
            /// Peso del font.
            /// </summary>
            public enum FontWeight
            {
                FW_DONTCARE,
                FW_THIN = 100,
                FW_EXTRALIGHT = 200,
                FW_ULTRALIGHT = FW_EXTRALIGHT,
                FW_LIGHT = 300,
                FW_NORMAL = 400,
                FW_REGULAR = FW_NORMAL,
                FW_MEDIUM = 500,
                FW_SEMIBOLD = 600,
                FW_DEMIBOLD = FW_SEMIBOLD,
                FW_BOLD = 700,
                FW_EXTRABOLD = 800,
                FW_ULTRABOLD = FW_EXTRABOLD,
                FW_HEAVY = 900,
                FW_BLACK = FW_HEAVY
            }

            /// <summary>
            /// Set di caratteri.
            /// </summary>
            public enum Charset : byte
            {
                ANSI_CHARSET,
                DEFAULT_CHARSET,
                SYMBOL_CHARSET,
                SHIFTJIS_CHARSET = 128,
                HANGEUL_CHARSET = 129,
                HANGUL_CHARSET = HANGEUL_CHARSET,
                GB2312_CHARSET = 134,
                CHINESEBIG5_CHARSET = 136,
                OEM_CHARSET = 255,
                JOHAB_CHARSET = 130,
                HEBREW_CHARSET = 177,
                ARABIC_CHARSET = 178,
                GREEK_CHARSET = 161,
                TURKISH_CHARSET = 162,
                VIETNAMESE_CHARSET = 163,
                THAI_CHARSET = 222,
                EASTEUROPE_CHARSET = 238,
                RUSSIAN_CHARSET = 204,
                MAC_CHARSET = 77,
                BALTIC_CHARSET = 186
            }

            /// <summary>
            /// Precisione dell'output.
            /// </summary>
            public enum OutputPrecision : byte
            {
                /// <summary>
                /// Comportamento di default.
                /// </summary>
                OUT_DEFAULT_PRECIS,
                /// <summary>
                /// Non usato.
                /// </summary>
                OUT_STRING_PRECIS,
                /// <summary>
                /// Non usato.
                /// </summary>
                OUT_STROKE_PRECIS = 3,
                /// <summary>
                /// Selezionare un font TrueType.
                /// </summary>
                OUT_TT_PRECIS,
                /// <summary>
                /// Selezionare un font dispositivo quando il sistema contiene font multipli con lo stesso nome.
                /// </summary>
                OUT_DEVICE_PRECIS,
                /// <summary>
                /// Selezionare un font raster se il sistema contiene font multipli con lo stesso nome.
                /// </summary>
                OUT_RASTER_PRECIS,
                /// <summary>
                /// Selezionare solo font TrueType, se esistenti.
                /// </summary>
                OUT_TT_ONLY_PRECIS,
                /// <summary>
                /// Selezionare tra font TrueType e altri font basa sui bordi.
                /// </summary>
                OUT_OUTLINE_PRECIS,
                /// <summary>
                /// Selezionare sono font PostScript.
                /// </summary>
                OUT_PS_ONLY_PRECIS = 10
            }

            /// <summary>
            /// Precisione di taglio.
            /// </summary>
            [Flags]
            public enum ClipPrecision : byte
            {
                /// <summary>
                /// Comportamento di default.
                /// </summary>
                CLIP_DEFAULT_PRECIS,
                /// <summary>
                /// Non usato.
                /// </summary>
                CLIP_STROKE_PRECIS = 2,
                /// <summary>
                /// In base all'orientamento del sistema di coordinate.
                /// </summary>
                CLIP_LH_ANGLES = 1 << 4,
                /// <summary>
                /// Disattiva l'associazione font.
                /// </summary>
                CLIP_DFA_DISABLE = 4 << 4,
                /// <summary>
                /// Da specificare per utilizzare un font di sola lettura integrato.
                /// </summary>
                CLIP_EMBEDDED = 8 << 4
            }

            /// <summary>
            /// Qualità del font.
            /// </summary>
            public enum FontQuality : byte
            {
                /// <summary>
                /// La qualità del font non importa.
                /// </summary>
                DEFAULT_QUALITY,
                /// <summary>
                /// La qualità del font è meno importante di quando <see cref="PROOF_QUALITY"/> è usato.
                /// </summary>
                /// <remarks>Per i font raster GDI, lo scaling è abilitato, il che significa che sono disponibili più dimensioni, ma la qualità può essere più bassa.<br/>
                /// I font grassetto, corsivo, sottolineato e sbarrato vengono sintetizzati se necessario.</remarks>
                DRAFT_QUALITY,
                /// <summary>
                /// La qualità del font è più importante della sua corrispondenza con gli attributi logici del font.
                /// </summary>
                /// <remarks>Per i font raster GDI, lo scaling è disabilitato e il font con dimensione più vicine a quelle richieste viene scelto.<br/>
                /// Con questa opzione la qualità del font è alta e non ci sono distorsioni.<br/>
                /// I font grassetto, corsivo, sottolineato e sbarrato sono sintetizzati se necessario.</remarks>
                PROOF_QUALITY,
                /// <summary>
                /// Il font non usa l'antialiasing.
                /// </summary>
                NONANTIALIASED_QUALITY,
                /// <summary>
                /// Il font usa l'antialiasing.
                /// </summary>
                ANTIALIASED_QUALITY,
                /// <summary>
                /// Il testo viene renderizzato (quando possibile) usa l'antialiasing ClearType.
                /// </summary>
                CLEARTYPE_QUALITY,

                CLEARTYPE_NATURAL_QUALITY
            }

            /// <summary>
            /// Inclinazione del font.
            /// </summary>
            public enum FontPitch : byte
            {
                DEFAULT_PITCH,
                FIXED_PITCH,
                VARIABLE_PITCH
            }

            /// <summary>
            /// Famiglia del font.
            /// </summary>
            public enum FontFamily : byte
            {
                /// <summary>
                /// La famiglia del font non ha importanza.
                /// </summary>
                FF_DONTCARE = 0 << 4,
                /// <summary>
                /// Font proporzionali con serif.
                /// </summary>
                FF_ROMAN = 1 << 4,
                /// <summary>
                /// Font proporzionali senza serif.
                /// </summary>
                FF_SWISS = 2 << 4,
                /// <summary>
                /// Font monospazio con o senza serif.
                /// </summary>
                FF_MODERN = 3 << 4,
                /// <summary>
                /// Font creati per somigliare alla scrittura a mano.
                /// </summary>
                FF_SCRIPT = 4 << 4,
                /// <summary>
                /// 
                /// </summary>
                FF_DECORATIVE = 5 << 4
            }
            #endregion
            #region Job Object Enumerations
            /// <summary>
            /// Informazioni recuperabili su un job.
            /// </summary>
            public enum JobInformationClass : uint
            {
                JobObjectBasicAccountingInformation = 1,
                JobObjectBasicAndIoAccountingInformation = 8,
                JobObjectBasicProcessIdList = 3,
                JobObjectLimitViolationInformation = 13,
                JobObjectAssociateCompletionPortInformation = 7,
                JobObjectBasicLimitInformation = 2,
                JobObjectBasicUIRestrictions = 4,
                JobObjectCpuRateControlInformation = 15,
                JobObjectEndOfJobTimeInformation = 6,
                JobObjectExtendedLimitInformation = 9,
                JobObjectGroupInformation = 11,
                JobObjectGroupInformationEx = 14,
                JobObjectLimitViolationInformation2 = 34,
                JobObjectNetRateControlInformation = 32,
                JobObjectNotificationLimitInformation = 12,
                JobObjectNotificationLimitInformation2 = 33
            }

            /// <summary>
            /// Modalità di controllo del tasso di utilizzo della CPU per un job.
            /// </summary>
            [Flags]
            public enum JobObjectCPURateControl : uint
            {
                /// <summary>
                /// Abilita il controllo del tasso di utilizzo della CPU.
                /// </summary>
                /// <remarks>Questo valore deve essere impostato se vengono impostati i valori <see cref="CpuRateControlWeightBased"/>, <see cref="CpuRateControlHardCap"/> oppure <see cref="CpuRateControlMinMaxRate"/>.</remarks>
                CpuRateControlEnabled = 0x1,
                /// <summary>
                /// Il tasso di utilizzo della CPU è calcolato in base al suo peso relativo rispetto al peso degli altri job.
                /// </summary>
                /// <remarks>Se questo valore viene impostato, non può essere impostato il valore <see cref="CpuRateControlMinMaxRate"/>.</remarks>
                CpuRateControlWeightBased = 0x2,
                /// <summary>
                /// Il tasso di utilizzo della CPU è un limite imposto.
                /// </summary>
                /// <remarks>Una volta che il job raggiunge il limite per l'attuale intervallo di scheduling, nessun thread associato al job sarà eseguito fino al prossimo intervallo.<br/><br/>
                /// Se questo valore viene impostato, non può essere impostato il valore <see cref="CpuRateControlMinMaxRate"/>.</remarks>
                CpuRateControlHardCap = 0x4,
                /// <summary>
                /// Invia messaggi quando il tasso di utilizzo della CPU per il job supera i limiti durante l'intervallo di tolleranza.
                /// </summary>
                CpuRateControlNotify = 0x8,
                /// <summary>
                /// Il tasso di utilizzo della CPU è limitato dal tasso minimo e dal tasso massimo specificati.
                /// </summary>
                /// <remarks>Se questo valore viene impostato, non possono essere impostati i valori <see cref="CpuRateControlWeightBased"/> e <see cref="CpuRateControlHardCap"/>.</remarks>
                CpuRateControlMinMaxRate = 0x10
            }
            #endregion
            #region Power Enumerations
            /// <summary>
            /// Requisito di esecuzione di un thread.
            /// </summary>
            [Flags]
            public enum ExecutionState : uint
            {
                /// <summary>
                /// Attiva la modalità utente assente.
                /// </summary>
                /// <remarks>Questo valore deve essere usato con <see cref="ES_CONTINUOUS"/>.<br/><br/>
                /// La modalità utente assente dovrebbe essere usata solo da applicazioni di registrazione e distribuzione di media che deve eseguire elaborazioni critiche in background su computer desktop mentre esso sembra in sospensione.</remarks>
                ES_AWAYMODE_REQUIRED = 0x00000040,
                /// <summary>
                /// Informa il sistema che lo stato impostato deve essere mantenuto fino alla prossima chiamata con questa opzione e uno degli altri stati è stato rimosso.
                /// </summary>
                ES_CONTINUOUS = 0x80000000,
                /// <summary>
                /// Forza il display a rimanere acceso.
                /// </summary>
                ES_DISPLAY_REQUIRED = 0x00000002,
                /// <summary>
                /// Forza il sistema a rimanere attivo.
                /// </summary>
                ES_SYSTEM_REQUIRED = 0x00000001,
                /// <summary>
                /// Non supportato.
                /// </summary>
                ES_USER_PRESENT = 0x00000004
            }
            #endregion
            #region Input Enumerations
            /// <summary>
            /// Impostazioni del feedback UI.
            /// </summary>
            public enum ContactVisualizationSetting : uint
            {
                /// <summary>
                /// Feedback UI disattivato per tutti i contatti.
                /// </summary>
                CONTACTVISUALIZATION_OFF = 0x0000,
                /// <summary>
                /// Feedback UI attivato per tutti i contatti.
                /// </summary>
                CONTACTVISUALIZATION_ON = 0x0001,
                /// <summary>
                /// Feedback UI attivo per tutti i contatti con visuali della modalità presentazione.
                /// </summary>
                CONTACTVISUALIZATION_PRESENTATIONMODE = 0x0002
            }

            /// <summary>
            /// Impostazioni feedback UI per i gesti.
            /// </summary>
            public enum GestureVisualizationSetting : uint
            {
                /// <summary>
                /// Feedback UI disattivato per tutti i gesti.
                /// </summary>
                GESTUREVISUALIZATION_OFF = 0x0000,
                /// <summary>
                /// Feedback UI attivato per tutti i gesti.
                /// </summary>
                GESTUREVISUALIZATION_ON = 0x001F,
                /// <summary>
                /// Feedback UI attivato per il tocco.
                /// </summary>
                GESTUREVISUALIZATION_TAP = 0x0001,
                /// <summary>
                /// Feedback UI attivato per il doppio tocco.
                /// </summary>
                GESTUREVISUALIZATION_DOUBLETAP = 0x0002,
                /// <summary>
                /// Feedback UI attivato per tocco e pressione.
                /// </summary>
                GESTUREVISUALIZATION_PRESSANDTAP = 0x0004,
                /// <summary>
                /// Feedback UI attivato per pressione continua.
                /// </summary>
                GESTUREVISUALIZATION_PRESSANDHOLD = 0x0008,
                /// <summary>
                /// Feedback UI attivato per tocco (tasto destro).
                /// </summary>
                GESTUREVISUALIZATION_RIGHTTAP = 0x0010
            }

            /// <summary>
            /// Impostazioni di routing dell'input del mouse dopo la pressione della rotellina.
            /// </summary>
            public enum MouseWheelRoutingSetting : uint
            {
                /// <summary>
                /// Input inviato all'applicazione attiva.
                /// </summary>
                MOUSEWHEEL_ROUTING_FOCUS,
                /// <summary>
                /// Input inviato all'applicazione attiva (desktop) oppure all'applicazione sotto il cursore.
                /// </summary>
                MOUSEWHEEL_ROUTING_HYBRID,
                /// <summary>
                /// Input inviato all'applicazione sotto il cursore del mouse.
                /// </summary>
                MOUSEWHEEL_ROUTING_MOUSE_POS
            }

            /// <summary>
            /// Impostazioni feedback UI per i gesti con la penna.
            /// </summary>
            public enum PenVisualizationSetting : uint
            {
                /// <summary>
                /// Feedback UI per i gesti con la penna disattivato.
                /// </summary>
                PENVISUALIZATION_OFF = 0x0000,
                /// <summary>
                /// Feedback UI per i gesti con la penna attivato.
                /// </summary>
                PENVISUALIZATION_ON = 0x0023,
                /// <summary>
                /// Feedback UI attivato per il tocco con la penna.
                /// </summary>
                PENVISUALIZATION_TAP = 0x0001,
                /// <summary>
                /// Feedback UI attivato per il doppio tocco con la penna.
                /// </summary>
                PENVISUALIZATION_DOUBLETAP = 0x0002,
                /// <summary>
                /// Feedback UI attivato per il cursore penna.
                /// </summary>
                PENVISUALIZATION_CURSOR = 0x0020
            }
            #endregion
            #region Window Parameters Enumerations
            /// <summary>
            /// Posizione iniziale e direzione delle finestre minimizzate ordinate.
            /// </summary>
            [Flags]
            public enum MinimizedWindowsStartingPositionsAndDirection
            {
                /// <summary>
                /// La posizione iniziale è l'angolo inferiore sinistro dell'area di lavoro.
                /// </summary>
                ARW_BOTTOMLEFT = 0x0000,
                /// <summary>
                /// La posizione iniziale è l'angolo inferiore destro dell'area di lavoro.
                /// </summary>
                ARW_BOTTOMRIGHT = 0x0001,
                /// <summary>
                /// La posizione iniziale è l'angolo superiore sinistro dell'area di lavoro.
                /// </summary>
                ARW_TOPLEFT = 0x0002,
                /// <summary>
                /// La posizione iniziale è l'angolo superiore destro dell'area di lavoro.
                /// </summary>
                ARW_TOPRIGHT = 0x0003,
                /// <summary>
                /// Ordinamento a sinistra.
                /// </summary>
                ARW_LEFT = ARW_BOTTOMLEFT,
                /// <summary>
                /// Ordinamento a destra.
                /// </summary>
                ARW_RIGHT = ARW_BOTTOMLEFT,
                /// <summary>
                /// Ordinamento verso l'alto.
                /// </summary>
                ARW_UP = 0x0004,
                /// <summary>
                /// Ordinamento verso il basso.
                /// </summary>
                ARW_DOWN = ARW_UP,
                /// <summary>
                /// Nasconde le finestre.
                /// </summary>
                ARW_HIDE = 0x0008
            }
            #endregion
        }

        /// <summary>
        /// Strutture Win32.
        /// </summary>
        public static class Win32Structures
        {
            #region Process And Thread Structures
            #region Process Structures
            /// <summary>
            /// Utilizzo della memoria di un processo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MEMORY_COUNTERS
            {
                /// <summary>
                /// Dimensioni della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Numero di page fault.
                /// </summary>
                public uint PageFaultCount;
                /// <summary>
                /// Dimensione massima del working set, in bytes.
                /// </summary>
                public IntPtr PeakWorkingSetSize;
                /// <summary>
                /// Dimensione del working set, in bytes.
                /// </summary>
                public IntPtr WorkingSetSize;
                /// <summary>
                /// 
                /// </summary>
                public IntPtr QuotaPeakPagedPoolUsage;
                /// <summary>
                /// 
                /// </summary>
                public IntPtr QuotaPagedPoolUsage;
                /// <summary>
                /// 
                /// </summary>
                public IntPtr QuotaPeakNonPagedPoolUsage;
                /// <summary>
                /// 
                /// </summary>
                public IntPtr QuotaNonPagedPoolUsage;
                /// <summary>
                /// Memoria privata, in bytes.
                /// </summary>
                public IntPtr PageFileUsage;
                /// <summary>
                /// Memoria privata massima, in bytes.
                /// </summary>
                public IntPtr PeakPageFileUsage;
            }

            /// <summary>
            /// Informazioni su un processo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESSENTRY32
            {
                /// <summary>
                /// Dimensioni della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Non usato.
                /// </summary>
                public uint Usage;
                /// <summary>
                /// PID del processo.
                /// </summary>
                public uint PID;
                /// <summary>
                /// Non usato.
                /// </summary>
                public UIntPtr DefaultHeapID;
                /// <summary>
                /// Non usato.
                /// </summary>
                public uint ModuleID;
                /// <summary>
                /// Numero di thread del processo.
                /// </summary>
                public uint Threads;
                /// <summary>
                /// PID del padre.
                /// </summary>
                public uint ParentPID;
                /// <summary>
                /// Priorità base del processo.
                /// </summary>
                public int BasePriority;
                /// <summary>
                /// Non usato.
                /// </summary>
                public uint Flags;
                /// <summary>
                /// Nome dell'eseguibile.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string ExecutableName;
            }

            /// <summary>
            /// Informazioni sul working set di un processo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PSAPI_WORKING_SET_INFORMATION
            {
                /// <summary>
                /// Numero di elementi nel membro <see cref="WorkingSetInfo"/> di questa struttura.
                /// </summary>
                public UIntPtr NumberOfEntries;
                /// <summary>
                /// Informazioni su ogni pagina nel working set.
                /// </summary>
                /// <remarks>Ognuno di questi valori contiene le seguenti informazioni:<br/>
                /// 1) protezione (5 bit)<br/>
                /// 2) conteggio condivisione (numero di processi che condividono la pagina) (3 bit)<br/>
                /// 3) condivisa (indica se la pagina è condivisa) (1 bit)<br/>
                /// 4) riservato (3 bit)<br/>
                /// 5) indirizzo della pagina in memoria (52 bit su Windows a 64 bit, altrimenti 20 bit)</remarks>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x989680, ArraySubType = UnmanagedType.SysUInt)]
                public UIntPtr[] WorkingSetInfo;
            }

            /// <summary>
            /// Contiene informazioni sull'attività I/O di un processo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct IO_COUNTERS
            {
                /// <summary>
                /// Il numero di operazioni di lettura effettuate.
                /// </summary>
                public ulong ReadOperationsCount;
                /// <summary>
                /// Il numero di operazioni di scrittura effettuate.
                /// </summary>
                public ulong WriteOperationCount;
                /// <summary>
                /// Il numero di operazioni diverse da lettura e scrittura effettuate.
                /// </summary>
                public ulong OtherOperationCount;
                /// <summary>
                /// Il numero di byte letti.
                /// </summary>
                public ulong ReadTransferCount;
                /// <summary>
                /// Il numero di byte scritti.
                /// </summary>
                public ulong WriteTransferCount;
                /// <summary>
                /// Il numero di byte trasferiti durante operazioni diverse da lettura e scrittura.
                /// </summary>
                public ulong OtherTransferCount;
            }

            /// <summary>
            /// Informazioni di avvio per un processo.
            /// </summary>
            public struct STARTUPINFO
            {
                /// <summary>
                /// Dimensione della struttura, in bytes.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Riservato, deve essere nullo.
                /// </summary>
                public string Reserved;
                /// <summary>
                /// Nome del desktop (ed eventualmente della window station) da associare al processo.
                /// </summary>
                /// <remarks>La presenza di un backslash nella stringa indica che essa include sia il desktop che la window station.</remarks>
                public string Desktop;
                /// <summary>
                /// Titolo della finestra.
                /// </summary>
                /// <remarks>Questo membro è valido solo se il processo crea una console, deve essere nullo se il processo è un processo GUI o se non crea una console.</remarks>
                public string Title;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USEPOSITION"/>, questo campo rappresenta l'offset x dell'angolo superiore sinistro della finestra, in pixel.<br/><br/>
                /// L'offset parte dall'angolo superiore sinistro dello schermo.
                /// </summary>
                public uint X;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USEPOSITION"/>, questo campo rappresenta l'offset y dell'angolo superiore sinistro della finestra, in pixel.<br/><br/>
                /// L'offset parte dall'angolo superiore sinistro dello schermo.
                /// </summary>
                public uint Y;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESIZE"/>, questo campo rappresenta la larghezza della finestra, in pixels.
                /// </summary>
                public uint XSize;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESIZE"/>, questo campo rappresenta l'altezza della finestra, in pixels.
                /// </summary>
                public uint YSize;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USECOUNTCHARS"/>, questo campo specifica la larghezza del buffer dello schermo, in colonne di caratteri.
                /// </summary>
                public uint XCountChars;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USECOUNTCHARS"/>, questo campo specifica l'altezza del buffer dello schermo, in righe di caratteri.
                /// </summary>
                public uint YCountChars;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USEFILLATTRIBUTE"/>, questo campo indica i colori iniziali di foreground e background del testo.
                /// </summary>
                /// <remarks>Il valore di questo campo può essere una qualsiasi combinazione dei seguenti valori:<br/><br/>
                /// <see cref="Win32Enumerations.CharacterAttributes.FOREGROUND_BLUE"/><br/>
                /// <see cref="Win32Enumerations.CharacterAttributes.FOREGROUND_GREEN"/><br/>
                /// <see cref="Win32Enumerations.CharacterAttributes.FOREGROUND_RED"/><br/>
                /// <see cref="Win32Enumerations.CharacterAttributes.FOREGROUND_INTENSITY"/><br/>
                /// <see cref="Win32Enumerations.CharacterAttributes.BACKGROUND_BLUE"/><br/>
                /// <see cref="Win32Enumerations.CharacterAttributes.BACKGROUND_GREEN"/><br/>
                /// <see cref="Win32Enumerations.CharacterAttributes.BACKGROUND_RED"/><br/>
                /// <see cref="Win32Enumerations.CharacterAttributes.BACKGROUND_INTENSITY"/></remarks>
                public uint FillAttribute;
                /// <summary>
                /// Un campo di bit che determina quali membri della struttura sono usati quando il processo crea una finestra.
                /// </summary>
                public Win32Enumerations.StartupInfoStructureAvailableData Flags;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESHOWWINDOW"/>, questo campo può essere uno qualunque dei valori presenti in <see cref="Win32Enumerations.WindowShowState"/>.
                /// </summary>
                public ushort ShowWindow;
                /// <summary>
                /// Riservato per uso dal C Runtime, deve essere 0.
                /// </summary>
                public ushort Reserved2;
                /// <summary>
                /// Riservato per uso dal C Runtime, deve essere <see cref="IntPtr.Zero"/>.
                /// </summary>
                public IntPtr Reserved3;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESTDHANDLES"/>, questo campo specifica l'handle dell'input standard per il processo.
                /// </summary>
                /// <remarks>Se l'opzione <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESTDHANDLES"/> non è specificata, l'input standard di default è il buffer della tastiera.<br/><br/>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USEHOTKEY"/>, questo campo specifica un valore hotkey inviato come il parametro wParam del messaggio WM_SETHOTKEY inviato alla prima finestra top-level valida creata dall'applicazione associata al processo.</remarks>
                public IntPtr StdInput;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESTDHANDLES"/>, questo campo specifica l'handle dell'output standard per il processo.
                /// </summary>
                /// <remarks>Se l'opzione <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESTDHANDLES"/> non è specificata, l'output standard di default è il buffer della finestra della console.<br/><br/>
                /// Se il processo viene avviato dalla barra delle applicazioni o dalla jump list, il sistema imposta questo campo all'handle del monitor che contiene la barra delle applicazioni o la jump list usata per avviare il processo.</remarks>
                public IntPtr StdOutput;
                /// <summary>
                /// Se <see cref="Flags"/> specifica <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESTDHANDLES"/>, questo campo specifica l'handle dell'errore standard per il processo.
                /// </summary>
                /// <remarks>Se l'opzione <see cref="Win32Enumerations.StartupInfoStructureAvailableData.STARTF_USESTDHANDLES"/> non è specificata, l'errore standard di default è il buffer della finestra della console.</remarks>
                public IntPtr StdError;
            }

            /// <summary>
            /// Informazioni su un processo appena creato e sul suo thread primario.
            /// </summary>
            public struct PROCESS_INFORMATION
            {
                /// <summary>
                /// Handle nativo al processo.
                /// </summary>
                public IntPtr ProcessHandle;
                /// <summary>
                /// Handle nativo al thread primario.
                /// </summary>
                public IntPtr ThreadHandle;
                /// <summary>
                /// ID del processo.
                /// </summary>
                public uint ProcessID;
                /// <summary>
                /// ID del thread primario.
                /// </summary>
                public uint ThreadID;
            }
            #endregion
            #region Thread Structures
            /// <summary>
            /// Informazioni su un thread.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct THREADENTRY32
            {
                /// <summary>
                /// Dimensioni della struttura.
                /// </summary>
                public uint StructureSize;
                /// <summary>
                /// Non usato.
                /// </summary>
                public uint Usage;
                /// <summary>
                /// ID del thread.
                /// </summary>
                public uint ThreadID;
                /// <summary>
                /// PID del processo che ha creato il thread.
                /// </summary>
                public uint OwnerPID;
                /// <summary>
                /// Priorità del thread.
                /// </summary>
                public int BasePriority;
                /// <summary>
                /// Non usato.
                /// </summary>
                public int DeltaPriority;
                /// <summary>
                /// Non usato.
                /// </summary>
                public uint Flags;
            }

            /// <summary>
            /// Informazioni su un thread GUI.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct GUITHREADINFO
            {
                /// <summary>
                /// Dimensione, in bytes, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Stato del thread.
                /// </summary>
                public Win32Enumerations.GUIThreadStates Flags;
                /// <summary>
                /// Handle nativo alla finestra attiva.
                /// </summary>
                public IntPtr ActiveWindowHandle;
                /// <summary>
                /// Handle nativo alla finestra che ha il controllo della tastiera.
                /// </summary>
                public IntPtr KeyboardFocusWindowHandle;
                /// <summary>
                /// Handle nativo alla finestra che ha catturato il mouse.
                /// </summary>
                public IntPtr MouseCaptureWindowHandle;
                /// <summary>
                /// Handle nativo alla finestra che è proprietaria di un menù attivo.
                /// </summary>
                public IntPtr ActiveMenuOwnerWindowHandle;
                /// <summary>
                /// Handle nativo alla finestra in corso di movimento o ridimensionamento.
                /// </summary>
                public IntPtr MoveSizeLoopWindowHandle;
                /// <summary>
                /// Handle nativo alla finestra il cui rettangolo è visibile.
                /// </summary>
                public IntPtr CaretWindowHandle;
                /// <summary>
                /// Coordinate del rettagolo relative alla finestra specificata dal campo <see cref="CaretWindowHandle"/>.
                /// </summary>
                public RECT Caret;
            }
            #endregion
            #region Module Structures
            /// <summary>
            /// Informazioni su un modulo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MODULEENTRY32
            {
                /// <summary>
                /// Dimensione della struttura, in bytes.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Non usato, ha sempre valore 1.
                /// </summary>
                public uint ModuleID;
                /// <summary>
                /// ID del processo i cui moduli devono essere esaminati.
                /// </summary>
                public uint ProcessID;
                /// <summary>
                /// Valore non significativo, sempre impostato a 0xFFFF.
                /// </summary>
                public uint ModuleUsage1;
                /// <summary>
                /// Valore non significativo, sempre impostato a 0xFFFF.
                /// </summary>
                public uint ModuleUsage2;
                /// <summary>
                /// Indirizzo di base del modulo nel contesto che processo a cui il modulo appartiene.
                /// </summary>
                public IntPtr ModuleBaseAddress;
                /// <summary>
                /// Dimensione del modulo, in bytes.
                /// </summary>
                public uint ModuleBaseSize;
                /// <summary>
                /// Handle al modulo, valido nel contesto del processo a cui il modulo appartiene.
                /// </summary>
                public IntPtr ModuleHandle;
                /// <summary>
                /// Nome del modulo.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string ModuleName;
                /// <summary>
                /// Percorso del modulo.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string ModuleExePath;
            }

            /// <summary>
            /// Informazioni su un modulo caricato.
            /// </summary>
            public struct RTL_PROCESS_MODULE_INFORMATION
            {
                public IntPtr Section;

                public IntPtr MappedBase;

                public IntPtr ImageBase;

                public uint ImageSize;

                public uint Flags;

                public ushort LoadOrderIndex;

                public ushort InitOrderIndex;

                public ushort LoadCount;

                public ushort OffsetToFileName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
                public string FullPathName;
            }
            #endregion
            #region Process And Thread NT API Structures
            #region NtQueryInformationProcess Structures
            /// <summary>
            /// Informazioni di base su un processo.
            /// </summary>
            /// <remarks>Questa struttura è il risultato di una chiamata alla funzione NtQueryInformationProcess il cui parametro ProcessInformationClass ha valore <see cref="Win32Enumerations.ProcessInformationClass.ProcessBasicInformation"/>.</remarks>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_BASIC_INFORMATION_DOCUMENTED
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                public IntPtr Reserved1;
                /// <summary>
                /// Indirizzo del PEB (Process Environment Block).
                /// </summary>
                public IntPtr PEBAddress;
                /// <summary>
                /// Membro riservato.
                /// </summary>
                public IntPtr Reserved2_0;
                /// <summary>
                /// Membro riservato.
                /// </summary>
                public IntPtr Reserved2_1;
                /// <summary>
                /// PID del processo.
                /// </summary>
                public IntPtr PID;
                /// <summary>
                /// Membro riservato.
                /// </summary>
                public IntPtr Reserved3;
            }

            /// <summary>
            /// Informazioni sulla protezione di un processo.
            /// </summary>
            /// <remarks>Questa struttura è il risultato di una chiamata alla funzione NtQueryInformationProcess il cui parametro ProcessInformationClass ha valore <see cref="Win32Enumerations.ProcessInformationClass.ProcessProtectionInformation"/>.</remarks>
            [StructLayout(LayoutKind.Sequential)]
            public struct PS_PROTECTION
            {
                public byte Level;
            }

            /// <summary>
            /// Utilizzo della memoria da parte di un processo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct VM_COUNTERS
            {
                /// <summary>
                /// Memoria virtuale massima.
                /// </summary>
                public IntPtr PeakVirtualSize;
                /// <summary>
                /// Memoria virtuale corrente.
                /// </summary>
                public IntPtr VirtualSize;
                /// <summary>
                /// Numero di page fault.
                /// </summary>
                public uint PageFaultCount;
                /// <summary>
                /// Working set massimo.
                /// </summary>
                public IntPtr PeakWorkingSetSize;
                /// <summary>
                /// Working set corrente.
                /// </summary>
                public IntPtr WorkingSetSize;
                /// <summary>
                /// 
                /// </summary>
                public IntPtr QuotaPeakPagedPoolUsage;
                /// <summary>
                /// 
                /// </summary>
                public IntPtr QuotaPagedPoolUsage;
                /// <summary>
                /// 
                /// </summary>
                public IntPtr QuotaPeakNonPagedPoolUsage;
                /// <summary>
                /// 
                /// </summary>
                public IntPtr QuotaNonPagedPoolUsage;
                /// <summary>
                /// Memoria privata di un processo.
                /// </summary>
                public IntPtr PageFileUsage;
                /// <summary>
                /// Memoria privata massima di un processo.
                /// </summary>
                public IntPtr PeakPageFileUsage;
            }

            /// <summary>
            /// Parametri processo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct RTL_USER_PROCESS_PARAMETERS
            {
                public uint MaximumLength;
                public uint Length;
                public uint Flags;
                public uint DebugFlags;
                public IntPtr ConsoleHandle;
                public uint ConsoleFlags;
                public IntPtr StdInputHandle;
                public IntPtr StdOutputHandle;
                public IntPtr StdErrorHandle;
                public UNICODE_STRING2 CurrentDirectoryPath;
                public IntPtr CurrentDirectoryHandle;
                public UNICODE_STRING2 DLLPath;
                public UNICODE_STRING2 ImagePathName;
                public UNICODE_STRING2 CommandLine;
                public IntPtr Environment;
                public uint StartingPositionLeft;
                public uint StartingPositionTop;
                public uint Width;
                public uint Height;
                public uint CharWidth;
                public uint CharHeight;
                public uint ConsoleTextAttributes;
                public uint WindowFlags;
                public uint ShowWindowFlags;
                public UNICODE_STRING2 WindowTitle;
                public UNICODE_STRING2 DesktopName;
                public UNICODE_STRING2 ShellInfo;
                public UNICODE_STRING2 RuntimeData;
                public IntPtr DLCurrentDirectory;
            }

            /// <summary>
            /// Parametri di un processo.
            /// </summary>
            public struct RTL_USER_PROCESS_PARAMETERS_DOCUMENTED
            {
                /// <summary>
                /// Riservato.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] Reserved1;
                /// <summary>
                /// Riservato.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public IntPtr[] Reserved2;
                /// <summary>
                /// Percorso del file immagine.
                /// </summary>
                public UNICODE_STRING2 ImagePathName;
                /// <summary>
                /// Linea di comando.
                /// </summary>
                public UNICODE_STRING2 CommandLine;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RTL_DRIVE_LETTER_CURDIR
            {
                public ushort Flags;
                public ushort Length;
                public uint TimeStamp;
                public UNICODE_STRING2 DosPath;
            }

            /// <summary>
            /// Process Environment Block.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PEB
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] Reserved1;
                /// <summary>
                /// Indica se il processo è in corso di debug.
                /// </summary>
                public byte BeingDebugged;
                public byte Reserved2;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public IntPtr[] Reserved3;
                /// <summary>
                /// Puntatore a una struttura PEB_LDR_DATA che contiene informazioni sui moduli caricati per il processo.
                /// </summary>
                public IntPtr Ldr;
                /// <summary>
                /// Puntatore a struttura <see cref="RTL_USER_PROCESS_PARAMETERS"/> che contiene i parametri di un processo tra cui la linea di comando.
                /// </summary>
                public IntPtr ProcessParameters;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public IntPtr[] Reserved4;
                public IntPtr AtlThunkSListPtr;
                public IntPtr Reserved5;
                public uint Reserved6;
                public IntPtr Reserved7;
                public uint Reserved8;
                public uint AtlThunkSListPtr32;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 45)]
                public IntPtr[] Reserved9;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
                public byte[] Reserved10;
                public IntPtr PostProcessInitRoutine;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
                public byte[] Reserved11;
                public IntPtr Reserved12;
                public uint SessionID;
            }

            /// <summary>
            /// Process Environment Block su Windows a 64 bit.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PEB_64
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] Reserved1;
                public byte BeingDebugged;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
                public byte[] Reserved2;
                public IntPtr LoaderData;
                public IntPtr ProcessParameters;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 520)]
                public byte[] Reserved3;
                public IntPtr PostProcessInitRoutine;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 136)]
                public byte[] Reserved4;
                public uint SessionID;
            }
            #endregion
            #region NtQueryInformationThread Structures
            /// <summary>
            /// Informazioni di base su un thread.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct THREAD_BASIC_INFORMATION
            {

                public uint ExitStatus;

                public IntPtr TEBBaseAddress;

                public CLIENT_ID ClientID;

                public IntPtr AffinityMask;

                public int Priority;

                public int BasePriority;
            }
            #endregion
            #endregion
            #region Mitigation Policies Structures
            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica Data Execution Prevention (DEP).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_DEP_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>Il primo bit indica se la politica è attiva o meno.</remarks>
                public uint Flags;
                /// <summary>
                /// Indica se la politica è attiva permanentemente.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)] public bool Permanent;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica Address Space Randomization Layout (ASLR).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_ASLR_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>I primi quattro bit contengono le impostazioni:<br/>
                /// 1) EnableBottomUpRandomization<br/>
                /// 2) EnableForceRelocateImages<br/>
                /// 3) EnableHighEntropy<br/>
                /// 4) DisallowStrippedImages</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa alla generazione e modifica di codice dinamico.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_DYNAMIC_CODE_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>I primi 4 bit contengono le impostazioni:<br/>
                /// 1) ProhibitDynamicCode - se impostato impedisce al processo di generare codice dinamico o di modificare codice eseguibile esistente.<br/>
                /// 2) AllowThreadOptOut - se impostato permette ai thread di ignorare la politica.<br/>
                /// 3) AllowRemoteDowngrade - se impostato permette ai processi che non sono AppContainer di modificare le impostazioni relative al codice dinamico del processo.<br/>
                /// 4) AuditProhibitDynamicCode</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa alla gestione di handle non validi.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_STRICT_HANDLE_CHECK_POLICY
            {
                /// <summary>
                /// Memmbro riservato.
                /// </summary>
                /// <remarks>I primi due bit contengono le impostazioni:<br/>
                /// 1) RaiseExceptionOnInvalidHandleReference<br/>
                /// 2) HandleExceptionsPermanentlyEnabled</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa all'esecuzione di system call.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_SYSTEM_CALL_DISABLE_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>I primi due bit contengono le impostazioni:<br/>
                /// 1) DisallowWin32kSystemCalls<br/>
                /// 2) AuditDisallowWin32kSystemCalls</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa agli extension point.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_EXTENSION_POINT_DISABLE_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>Il primo bit indica se gli extension point sono abilitati.</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica Control Flow Guard (CFG).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_CONTROL_FLOW_GUARD_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>I primi tre bit contengono le impostazioni:<br/>
                /// 1) EnableControlFlowGuard - se impostato abilita la CFG.<br/>
                /// 2) EnableExportSuppression - se impostato le funzioni esportate saranno trattate come bersagli non validi di chiamate indirette di default, le funzioni esportate diventano bersagli validi solo se il loro indirizzo viene risolto dinamicamente.<br/>
                /// 3) StrictMode - se impostato tutte le DLL devono abilitare CFG, in caso contrario il caricamento dell'immagine fallisce.</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa al caricamento di immagini firmate.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>I primi 5 bit contengono le impostazioni:<br/>
                /// 1) MicrosoftSignedOnly - se impostato impedisce al processo di caricare immagini non firmate da Microsoft.<br/>
                /// 2) StoreSignedOnly - se impostato impedisce al processo di caricare immagini non firmate dal Windows Store.<br/>
                /// 3) MitigationOptIn - se impostato impedisce la processo di caricare immagini non firmate da Microsoft, dal Windows Store e dai Windows Hardware Quality Labs (WHQL).<br/>
                /// 4) AuditMicrosoftSignedOnly<br/>
                /// 5) AuditStoreSignedOnly</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa al caricamento di font non di sistema.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_FONT_DISABLE_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>I primi 2 bit contengono le impostazioni:<br/>
                /// 1) DisableNonSystemFonts - se impostato impedisce al processo di caricare font non di sistema.<br/>
                /// 2) AuditNonSystemFontLoading - se impostato indica che un evento ETW (Event Tracing for Windows) dovrebbe essere loggato se il processo tenta di caricare un font non di sistema.</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa al caricamento di immagini da un dispositivo remoto.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_IMAGE_LOAD_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>I primi 5 bit contengono le impostazioni:<br/>
                /// 1) No RemoteImages - se impostato impedisce al processo di caricare immagini da un dispositivo remoto.<br/>
                /// 2) NoLowMandatoryLabelImages - se impostato impedisce al processo di caricare immagini che hanno una Low mandatory label, scritta dal low IL.<br/>
                /// 3) PreferSystem32Images - se impostato il processo cerca immagini da caricare prima nella sottocartella System32 della cartella di installazione di Windows e poi nella cartella dell'applicazione seguendo l'ordine di ricerca standard delle DLL.<br/>
                /// 4) AuditNoRemoteImages<br/>
                /// 5) AuditNoLowMandatoryLabelImages</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa alla mitigazione dei side channels.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_SIDE_CHANNEL_ISOLATION_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>I primi 4 bit contengono le impostazioni:<br/>
                /// 1) SmtBranchTargetIsolation<br/>
                /// 2) IsolateSecurityDomain<br/>
                /// 3) DisablePageCombine<br/>
                /// 4) SpeculativeStoreBypassDisable</remarks>
                public uint Flags;
            }

            /// <summary>
            /// Impostazioni di mitigazione di un processo per la politica relativa alla protezione dello stack eseguita dall'hardware in modalità utente.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_MITIGATION_USER_SHADOW_STACK_POLICY
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                /// <remarks>Il primo bit indica se la protezione dello stack eseguita dall'hardware in modalità utente è abilitata per il processo in modalità di compatibilità.</remarks>
                public uint Flags;
            }
            #endregion
            #endregion
            #region Other Structures
            /// <summary>
            /// Contiene un valore a 64 bit rappresentante il numero di intervalli di 100 nanosecondi a partire dal 1° gennaio 1601 (UTC).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct FILETIME
            {
                /// <summary>
                /// 
                /// </summary>
                public uint LowDateTime;
                /// <summary>
                /// 
                /// </summary>
                public uint HighDateTime;
            }

            /// <summary>
            /// Rappresenta un rettangolo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                /// <summary>
                /// Coordinata x dell'angolo superiore sinistro del rettangolo.
                /// </summary>
                public int Left;
                /// <summary>
                /// Coordinata y dell'angolo superiore sinistro del rettangolo.
                /// </summary>
                public int Top;
                /// <summary>
                /// Coordinata x dell'angolo inferiore destro del rettangolo.
                /// </summary>
                public int Right;
                /// <summary>
                /// Coordinata y dell'angolo inferiore destro del rettangolo.
                /// </summary>
                public int Bottom;
            }

            /// <summary>
            /// Rappresenta un processore logico in un gruppo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESSOR_NUMBER
            {
                /// <summary>
                /// Gruppo del processore logico.
                /// </summary>
                public ushort Group;
                /// <summary>
                /// Numero del processore logico relativo al suo gruppo.
                /// </summary>
                public byte Number;
                /// <summary>
                /// Membro riservato.
                /// </summary>
                public byte Reserved;
            }

            /// <summary>
            /// Rappresenta un SID e i suoi attirbuti.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SID_AND_ATTRIBUTES
            {
                /// <summary>
                /// Puntatore a una struttura SID.
                /// </summary>
                public IntPtr Sid;
                /// <summary>
                /// Attributi del SID.
                /// </summary>
                public uint Attributes;
            }

            /// <summary>
            /// Specifica valori hash per un array di SID.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SID_AND_ATTRIBUTES_HASH
            {
                /// <summary>
                /// Numero di SID nell'array puntato dal campo <see cref="SidAttributes"/>.
                /// </summary>
                public uint SidCount;
                /// <summary>
                /// Puntatore a un array di strutture <see cref="SID_AND_ATTRIBUTES"/> che rappresentano i SID e i loro attributi.
                /// </summary>
                public IntPtr SidAttributes;
                /// <summary>
                /// Array di puntatori di valori hash.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Win32Constants.SID_HASH_SIZE)]
                public IntPtr[] Hash;
            }

            /// <summary>
            /// Rappresenta un identificativo locale e i suoi attributi.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct LUID_AND_ATTRIBUTES
            {
                /// <summary>
                /// Identificativo locale.
                /// </summary>
                public LUID Luid;
                /// <summary>
                /// Attributi dell'identificativo.
                /// </summary>
                public uint Attributes;
            }

            /// <summary>
            /// Identificativo locale.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct LUID
            {
                /// <summary>
                /// Numeri senza segno dell'ID.
                /// </summary>
                public int LowPart;
                /// <summary>
                /// Numeri con segno dell'ID.
                /// </summary>
                public int HighPart;
            }

            /// <summary>
            /// Caratteristiche di un oggetto utente.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct USEROBJECTFLAGS
            {
                /// <summary>
                /// Indica se l'handle può essere ereditato da nuovi processi.
                /// </summary>
                [MarshalAs(UnmanagedType.Bool)]
                public bool Inherit;
                /// <summary>
                /// Riservato, deve essere false.
                /// </summary>
                [MarshalAs(UnmanagedType.Bool)]
                public bool Reserved;
                /// <summary>
                /// Caratteristiche dell'oggetto.
                /// </summary>
                /// <remarks>Per una window station questo campo può avere valore <see cref="Win32Enumerations.UserObjectFlag.WSF_VISIBLE"/>, per un desktop può avere valore <see cref="Win32Enumerations.UserObjectFlag.DF_ALLOWOTHERACCOUNTHOOK"/>.</remarks>
                public Win32Enumerations.UserObjectFlag Flags;
            }

            /// <summary>
            /// Rappresenta le coordinate di un punto.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                /// <summary>
                /// Coordinata x del punto.
                /// </summary>
                public int x;
                /// <summary>
                /// Coordinata y del punto.
                /// </summary>
                public int y;
            }
            #endregion
            #region Window Structures
            /// <summary>
            /// Informazioni su una finestra.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct WINDOWINFO
            {
                /// <summary>
                /// Dimensione della struttura, in bytes.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Coordinate della finestra.
                /// </summary>
                public RECT Window;
                /// <summary>
                /// Coordinate dell'area client.
                /// </summary>
                public RECT ClientArea;
                /// <summary>
                /// Stili della finestra.
                /// </summary>
                public Win32Enumerations.WindowStyles Styles;
                /// <summary>
                /// Stili estesi della finestra.
                /// </summary>
                public Win32Enumerations.ExtendedWindowStyles ExtendedStyles;
                /// <summary>
                /// Stato (attivo, inattivo).
                /// </summary>
                public uint Status;
                /// <summary>
                /// Spessore del bordo della finestra, in pixel.
                /// </summary>
                public uint BorderWidth;
                /// <summary>
                /// Altezza del bordo della finestra, in pixel.
                /// </summary>
                public uint BorderHeight;
                /// <summary>
                /// Valore Atom della classe della finestra.
                /// </summary>
                public ushort ClassAtom;
                /// <summary>
                /// Versione dell'applicazione che ha creato la finestra.
                /// </summary>
                public ushort CreatorVersion;
            }
            #endregion
            #region Token Structures
            /// <summary>
            /// Privilegi assegnati a un token.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_PRIVILEGES
            {
                /// <summary>
                /// Numero di strutture <see cref="LUID_AND_ATTRIBUTES"/> nel campo Privileges.
                /// </summary>
                public uint PrivilegeCount;
                /// <summary>
                /// Array di strutture <see cref="LUID_AND_ATTRIBUTES"/>.
                /// </summary>
                public IntPtr Privileges;
            }

            /// <summary>
            /// Privilegi assegnati a un token.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_PRIVILEGES2
            {
                /// <summary>
                /// Numero di privilegi.
                /// </summary>
                public uint PrivilegeCount;
                /// <summary>
                /// LUID del privilegio.
                /// </summary>
                public LUID LUID;
                /// <summary>
                /// Attributi del privilegio.
                /// </summary>
                public Win32Enumerations.PrivilegeLUIDAttributes Attributes;
            }

            /// <summary>
            /// Identifica l'utente associato a un token di accesso.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_USER
            {
                /// <summary>
                /// Struttura <see cref="SID_AND_ATTRIBUTES"/> che rappresenta l'utente associato al token di accesso.
                /// </summary>
                public SID_AND_ATTRIBUTES User;
            }

            /// <summary>
            /// Indica se un token di accesso possiede privilegi elevati.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_ELEVATION
            {
                /// <summary>
                /// Indica se il token possiede privilegi elevati.
                /// </summary>
                public uint IsElevated;
            }

            /// <summary>
            /// Contiene il SID dell'app container.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_APPCONTAINER_INFORMATION
            {
                /// <summary>
                /// Puntatore al SID dell'app container.
                /// </summary>
                public IntPtr TokenAppContainer;
            }

            /// <summary>
            /// Informazioni relative ai SID di gruppo e i privilegi di un token di accesso.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_GROUPS_AND_PRIVILEGES
            {
                /// <summary>
                /// Numero di SID nel token.
                /// </summary>
                public uint SidCount;
                /// <summary>
                /// Quantità di bytes necessari per contenere tutti i SID degli utenti e i SID degli account di gruppo.
                /// </summary>
                public uint SidLength;
                /// <summary>
                /// Puntatore a un array di strutture <see cref="SID_AND_ATTRIBUTES"/> che contengono un set di SID e corrispondenti attributi.
                /// </summary>
                public IntPtr Sids;
                /// <summary>
                /// Numero di SID limitati.
                /// </summary>
                public uint RestrictedSidCount;
                /// <summary>
                /// Quantità di bytes necessaria richiesta per contenere tutti i SID limitati.
                /// </summary>
                public uint RestrictedSidLength;
                /// <summary>
                /// Puntatore a un array di strutture <see cref="SID_AND_ATTRIBUTES"/> che contiene un set di SID limitati e corrispondenti attributi.
                /// </summary>
                public IntPtr RestrictedSids;
                /// <summary>
                /// Numero di privilegi.
                /// </summary>
                public uint PrivilegeCount;
                /// <summary>
                /// Qunatità di bytes necessari per contenere l'array dei privilegi.
                /// </summary>
                public uint PrivilegeLength;
                /// <summary>
                /// Array di strutture <see cref="LUID_AND_ATTRIBUTES"/> che contengono informazioni sui privilegi.
                /// </summary>
                public IntPtr Privileges;
                /// <summary>
                /// <see cref="LUID"/> dell'autenticatore del token.
                /// </summary>
                public LUID AuthenticationID;
            }

            /// <summary>
            /// Contiene informazioni sul livello di integrità obbligatoria del token.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_MANDATORY_LABEL
            {
                /// <summary>
                /// Struttura <see cref="SID_AND_ATTRIBUTES"/> che specifica il livello di integrità obbligatoria del token.
                /// </summary>
                public SID_AND_ATTRIBUTES Label;
            }

            /// <summary>
            /// Contiene informazioni sul proprietario di default del token.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_OWNER
            {
                /// <summary>
                /// Puntatore a una struttura SID che rappresenta l'utente che diventerà proprietario di tutti gli oggetti creati dal processo con questo token di accesso.
                /// </summary>
                /// <remarks>Il SID deve appartenere agli utenti o gruppi già presenti nel token.</remarks>
                public IntPtr OwnerSID;
            }

            /// <summary>
            /// Contiene informazioni sul gruppo primario del token.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_PRIMARY_GROUP
            {
                /// <summary>
                /// Puntatore a una struttura SID che rappresenta il gruppo che diventerà il gruppo primario di ogni oggetto creato dal processo usando questo token di accesso.
                /// </summary>
                /// <remarks>Il SID deve appartenere agli utenti o gruppi già presenti nel token.</remarks>
                public IntPtr PrimaryGroupSID;
            }

            /// <summary>
            /// Identifica la fonte del token.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_SOURCE
            {
                /// <summary>
                /// Nome della fonte.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Win32Constants.TOKEN_SOURCE_LENGTH)]
                public char[] SourceName;
                /// <summary>
                /// Struttura <see cref="LUID"/> che identifica la fonte.
                /// </summary>
                public LUID SourceID;
            }

            /// <summary>
            /// Specifica la politica di integrità obbligatoria per un token.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_MANDATORY_POLICY
            {
                /// <summary>
                /// Politica di integrità obbligatoria del token di accesso associato.
                /// </summary>
                public uint Policy;
            }

            /// <summary>
            /// Informazioni necessarie per eseguire un controllo di accesso.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_ACCESS_INFORMATION
            {
                /// <summary>
                /// Hash del SID del token.
                /// </summary>
                public SID_AND_ATTRIBUTES_HASH SidHash;
                /// <summary>
                /// Hash del SID del token limitato.
                /// </summary>
                public SID_AND_ATTRIBUTES_HASH RestrictedSidHash;
                /// <summary>
                /// Privilegi del token.
                /// </summary>
                public TOKEN_PRIVILEGES Privileges;
                /// <summary>
                /// Identità del token.
                /// </summary>
                public LUID AuthenticationID;
                /// <summary>
                /// Tipo del token.
                /// </summary>
                public Win32Enumerations.TokenType Type;
                /// <summary>
                /// Livello di impersonazione del token.
                /// </summary>
                public Win32Enumerations.SecurityImpersonationLevel ImpersonationLevel;
                /// <summary>
                /// Politica di integrità obbligatoria del token.
                /// </summary>
                public TOKEN_MANDATORY_POLICY MandatoryPolicy;
                /// <summary>
                /// Riservato, impostato su 0.
                /// </summary>
                public uint Flags;
                /// <summary>
                /// Il numero dell'app container per il token, 0 se il token non è un token di un app container.
                /// </summary>
                public uint AppContainerNumber;
                /// <summary>
                /// SID dell'app container, <see cref="IntPtr.Zero"/> se il token non è un token di un app container.
                /// </summary>
                public IntPtr PackageSid;
                /// <summary>
                /// Hash del SID delle capacità del token.
                /// </summary>
                public SID_AND_ATTRIBUTES_HASH CapabilitiesHash;
                /// <summary>
                /// Il livello di sicurezza del processo protetto associato al token.
                /// </summary>
                public IntPtr TrustLevelSid;
                /// <summary>
                /// Riservato, deve essere <see cref="IntPtr.Zero"/>.
                /// </summary>
                public IntPtr SecurityAttributes;
            }

            /// <summary>
            /// Contiene un handle nativo a un token, quest'ultimo è collegato al token utilizzato a una chiamata alla funzione <see cref="Win32TokenFunctions.GetTokenInformation(IntPtr, Win32Enumerations.TokenInformationClass, IntPtr, uint, out uint)"/> o alla funzione <see cref="Win32TokenFunctions.SetTokenInformation(IntPtr, Win32Enumerations.TokenInformationClass, IntPtr, uint)"/>.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_LINKED_TOKEN
            {
                /// <summary>
                /// Handle nativo al token collegato.
                /// </summary>
                public IntPtr LinkedToken;
            }

            /// <summary>
            /// Informazioni sui SID di gruppo in un token di accesso.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_GROUPS
            {
                /// <summary>
                /// Numero di gruppi nel token di accesso.
                /// </summary>
                public uint GroupCount;
                /// <summary>
                /// Puntatore a un array di strutture <see cref="SID_AND_ATTRIBUTES"/> che contiene un set di SID e relativi attributi.
                /// </summary>
                public IntPtr Groups;
            }

            /// <summary>
            /// Contiene statistiche di un token di accesso.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_STATISTICS
            {
                /// <summary>
                /// <see cref="LUID"/> che indentifica l'istanza del token.
                /// </summary>
                public LUID TokenID;
                /// <summary>
                /// <see cref="LUID"/> assegnato alla sessione che il token rappresenta.
                /// </summary>
                public LUID AuthenticationID;
                /// <summary>
                /// Scadenza del token, attualmente non supportato.
                /// </summary>
                public long ExpirationTime;
                /// <summary>
                /// Tipo di token.
                /// </summary>
                public Win32Enumerations.TokenType TokenType;
                /// <summary>
                /// Livello di impersonazione.
                /// </summary>
                /// <remarks>Questo membro è valido solo se <see cref="TokenType"/> ha valore <see cref="Win32Enumerations.TokenType.TokenImpersonation"/>.</remarks>
                public Win32Enumerations.SecurityImpersonationLevel ImpersonationLevel;
                /// <summary>
                /// Quantità di memoria, in bytes, allocata per memorizzare le informazioni di protezione di default e l'indentificatore del gruppo primario.
                /// </summary>
                public uint DynamicCharged;
                /// <summary>
                /// Quantità di memoria non usata, in bytes, allocata per memorizzare le informazioni di protezione di default e l'indentificatore del gruppo primario.
                /// </summary>
                /// <remarks>Questo valore viene indicato come un conteggio di bytes liberi.</remarks>
                public uint DynamicAvailable;
                /// <summary>
                /// Numero di SID di gruppo supplementari inclusi nel token.
                /// </summary>
                public uint GroupCount;
                /// <summary>
                /// Numero di privilegi inclusi nel token.
                /// </summary>
                public uint PrivilegeCount;
                /// <summary>
                /// <see cref="LUID"/> che cambia ogni volta che il token viene modificato.
                /// </summary>
                public LUID ModifiedID;
            }
            #endregion
            #region User Account Structures
            /// <summary>
            /// Struttura che contiene il nome di un account utente.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct USER_INFO_0
            {
                /// <summary>
                /// Nome utente.
                /// </summary>
                [MarshalAs(UnmanagedType.LPWStr)]
                public string Name;
            }

            /// <summary>
            /// Struttura che contiene il nome di un gruppo locale.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct LOCALGROUP_USERS_INFO_0
            {
                /// <summary>
                /// Nome gruppo.
                /// </summary>
                [MarshalAs(UnmanagedType.LPWStr)]
                public string Name;
            }
            #endregion
            #region Security Structures
            /// <summary>
            /// Definisce gli attributi di sicurezza della richiesta.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct CLAIM_SECURITY_ATTRIBUTES_INFORMATION
            {
                /// <summary>
                /// Versione dell'attributo di sicurezza.
                /// </summary>
                public ushort Version;
                /// <summary>
                /// Riservato, deve essere 0 quando impostato e deve essere ignorato quando recuperato.
                /// </summary>
                public ushort Reserved;
                /// <summary>
                /// Numero di valori.
                /// </summary>
                public uint AttributeCount;
                /// <summary>
                /// Puntatore a un array di strutture <see cref="CLAIM_SECURITY_ATTRIBUTE_V1"/>.
                /// </summary>
                public IntPtr Attributes;
            }

            /// <summary>
            /// Definisce un attributo di sicurezza che può essere associato a un token o a un contesto di autorizzazione.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct CLAIM_SECURITY_ATTRIBUTE_V1
            {
                /// <summary>
                /// Nome dell'attributo di sicurezza.
                /// </summary>
                /// <remarks>Dimensione minima: 4 byte.</remarks>
                public string Name;
                /// <summary>
                /// Tipo dei valori presenti nell'array puntato dal campo <see cref="Values"/>.
                /// </summary>
                public Win32Enumerations.SecurityAttributesValuesType ValueType;
                /// <summary>
                /// Riservato, deve essere 0 quando impostato e deve essere ignorato quando recuperato.
                /// </summary>
                public ushort Reserved;
                /// <summary>
                /// Valore a 32 bit che contiene la caratteristiche dell'attributo.
                /// </summary>
                /// <remarks>I bit dal 16 al 31 possono essere impostati a qualunque valore, i bit da 0 a 15 devono avere valore 0 oppure essere uno o una combinazione dei valori presenti in <see cref="Win32Enumerations.SecurityAttributesFlags"/>.</remarks>
                public uint Flags;
                /// <summary>
                /// Il numero di valori nell'array puntato dal campo <see cref="Values"/>.
                /// </summary>
                public uint ValueCount;
                /// <summary>
                /// Puntatore a un array di valori del tipo specificato nel campo <see cref="ValueType"/>.
                /// </summary>
                public IntPtr Values;
            }

            /// <summary>
            /// Specifica il nome binario completo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct CLAIM_SECURITY_ATTRIBUTE_FQBN_VALUE
            {
                /// <summary>
                /// Versione del valore del nome binario completo.
                /// </summary>
                public ulong Version;
                /// <summary>
                /// Nome binario completo.
                /// </summary>
                public string Name;
            }

            /// <summary>
            /// Specifica la stringa (OCTET_STRING) dell'attributo di sicurezza della richiesta.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE
            {
                /// <summary>
                /// Buffer che contiene il valore della stringa, esso è una serie di byte della lunghezza indicata nel campo <see cref="ValueLength"/>.
                /// </summary>
                public IntPtr Value;
                /// <summary>
                /// Dimensione, in bytes, della stringa.
                /// </summary>
                public uint ValueLength;
            }

            /// <summary>
            /// Contiene un SID che indentifica un trustee e GUID che indentificano i tipi di oggetto di una ACE specifica di un oggetto.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct OBJECTS_AND_SID
            {
                /// <summary>
                /// Indica se i campi <see cref="ObjectTypeGUID"/> e <see cref="InheritedObjectTypeGUID"/> hanno valore.
                /// </summary>
                public Win32Enumerations.ObjectsPresent ObjectsPresent;
                /// <summary>
                /// <see cref="Guid"/> che identifica il tipo di oggetto, set di proprietà o singola proprietà protetta dalla ACE.
                /// </summary>
                /// <remarks>Se l'impostazione <see cref="Win32Enumerations.ObjectsPresent.ACE_OBJECT_TYPE_PRESENT"/> non è attiva, questo campo viene ignorato
                /// e l'ACE protegge l'oggetto a cui è assegnata.</remarks>
                public Guid ObjectTypeGUID;
                /// <summary>
                /// <see cref="Guid"/> che identifica il tipo di oggetto che può ereditare l'ACE.
                /// </summary>
                /// <remarks>Se l'impostazione <see cref="Win32Enumerations.ObjectsPresent.ACE_INHERITED_OBJECT_TYPE_PRESENT"/> non è attiva, questo campo viene ignorato
                /// e tutti gli oggetti figlio possono ereditare l'ACE.<br/>
                /// In ogni caso l'ereditarietà è controllata anche dai flag impostati nella struttura ACE_HEADER e da ogni protezione contro l'ereditarietà impostata sugli oggetti figlio.</remarks>
                public Guid InheritedObjectTypeGUID;
                /// <summary>
                /// SID del trustee a cui l'ACE si applica.
                /// </summary>
                public IntPtr SID;
            }

            /// <summary>
            /// Contiene una stringa che identifica il trustee tramite il nome e 
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct OBJECTS_AND_NAME
            {
                /// <summary>
                /// Indica se i campi <see cref="ObjectTypeName"/> e <see cref="InheritedObjectTypeName"/> hanno un valore.
                /// </summary>
                public Win32Enumerations.ObjectsPresent ObjectsPresent;
                /// <summary>
                /// Tipo di oggetto.
                /// </summary>
                public Win32Enumerations.ObjectType ObjectType;
                /// <summary>
                /// Nome del tipo di oggetto a cui si applica la ACE.
                /// </summary>
                public string ObjectTypeName;
                /// <summary>
                /// Nome del tipo di oggetto che può ereditare la ACE.
                /// </summary>
                public string InheritedObjectTypeName;
                /// <summary>
                /// Nome del trustee.
                /// </summary>
                public string Name;
            }

            /// <summary>
            /// Identifica un account utente, un account di gruppo o una sessione di accesso a cui una ACE si applica.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TRUSTEE
            {
                /// <summary>
                /// Puntatore a una struttura <see cref="TRUSTEE"/> che identifica un account server che può impersonare l'utente identificato dal campo <see cref="Name"/>.
                /// </summary>
                /// <remarks>Questo campo non è attualmente supportato e ha valore <see cref="IntPtr.Zero"/>.</remarks>
                public IntPtr MultipleTrustee;
                /// <summary>
                /// Un valore di <see cref="Win32Enumerations.MultipleTrusteeOperation"/>.
                /// </summary>
                /// <remarks>Attualmente questo campo può solo avere valore <see cref="Win32Enumerations.MultipleTrusteeOperation.NoMultipleTrustee"/>.</remarks>
                public Win32Enumerations.MultipleTrusteeOperation MultipleTrusteeOperation;
                /// <summary>
                /// Tipo di dati del campo <see cref="Name"/>.
                /// </summary>
                public Win32Enumerations.TrusteeForm TrusteeForm;
                /// <summary>
                /// Indica se questa struttura si riferisce a un account utente, un account di gruppo o un tipo di account sconosciuto.
                /// </summary>
                public Win32Enumerations.TrusteeType TrusteeType;
                /// <summary>
                /// Buffer che identifica il trustee e contiene eventuali informazioni sulle ACE specifiche dell'oggetto.
                /// </summary>
                public IntPtr Name;
            }

            /// <summary>
            /// Definisce informazioni di accesso per uno specifico trustee.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct EXPLICIT_ACCESS
            {
                /// <summary>
                /// Maschera di bit che specifica i diritti di accesso che una ACE permette, nega o controlla per il trustee.
                /// </summary>
                /// <remarks>Le funzioni che utilizzano questa struttura non convertono, interpretano o convalidano i bit di questa maschera.</remarks>
                public uint AccessPermissions;
                /// <summary>
                /// Valore che indica come i diritti di accesso indicato vengono applicati al trustee.
                /// </summary>
                /// <remarks>Per una DACL questo valore indica se i diritti di accesso sono concessi o negati, per una SACL questo valore indica se l'ACL genera messaggi di controllo in caso di tentativo di utilizzo riuscito, fallito o entrambi.</remarks>
                public Win32Enumerations.AccessMode AccessMode;
                /// <summary>
                /// Valore che determina se altri contenitori o oggetti possono ereditare l'ACE dall'oggetto primario a cui l'ACL è associata.
                /// </summary>
                /// <remarks>Questo valore può essere <see cref="Win32Enumerations.ACEInheritance.NO_INHERITANCE"/> oppure una combinazione dei valori di <see cref="Win32Enumerations.ACEInheritance"/>.</remarks>
                public Win32Enumerations.ACEInheritance Inheritance;
                /// <summary>
                /// Struttura <see cref="TRUSTEE"/> che indentifica l'utente, il gruppo o il programma a cui l'ACE si applica.
                /// </summary>
                public TRUSTEE Trustee;
            }

            /// <summary>
            /// Contiene informazioni sulla revisione di un'ACL.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ACL_REVISION_INFORMATION
            {
                /// <summary>
                /// Numero di revisione, quello attuale è rappresentato da <see cref="Win32Constants.ACL_REVISION"/>.
                /// </summary>
                public uint ACLRevision;
            }

            /// <summary>
            /// Contiene informazioni di dimensione di un'ACL.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ACL_SIZE_INFORMATION
            {
                /// <summary>
                /// Numero di ACE nell'ACL.
                /// </summary>
                public uint ACECount;
                /// <summary>
                /// Byte usati per ospitare le ACE e l'ACL, questo valore potrebbe essere minore del numero totale di byte.
                /// </summary>
                public uint ACLBytesInUse;
                /// <summary>
                /// Byte non utilizzati nell'ACL.
                /// </summary>
                public uint ACLBytesFree;
            }
            #endregion
            #region Memory Information Structures
            /// <summary>
            /// Contiene informazioni su un insieme di pagine nello spazio di indirizzamento virtuale di un processo (32 bit).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MEMORY_BASIC_INFORMATION
            {
                /// <summary>
                /// Puntatore all'indirizzo di base di una regione di pagine.
                /// </summary>
                public IntPtr BaseAddress;
                /// <summary>
                /// Puntatore all'indirizzo di base delle pagine allocate dalla funzione VirtualAlloc, le pagine puntate da <see cref="BaseAddress"/> sono contenute in questo insieme di indirizzi.
                /// </summary>
                public IntPtr AllocationBase;
                /// <summary>
                /// Protezione della memoria applicata quando la regione è stata inizialmente allocata, questo campo può avere valore corrispondente a una delle costanti di protezione oppure 0 se il chiamante non ha accesso.
                /// </summary>
                public Win32Enumerations.MemoryProtections AllocationProtect;
                /// <summary>
                /// Dimensione della regione che inizia dall'indirizzo di base in cui tutte le pagine hanno gli stessi attributi, in bytes.
                /// </summary>
                public UIntPtr RegionSize;
                /// <summary>
                /// Lo stato delle pagine nella regione.
                /// </summary>
                public Win32Enumerations.MemoryPageState State;
                /// <summary>
                /// Protezione applicata alle pagine nella regione, questo campo ha uno dei possibili valori del campo <see cref="AllocationProtect"/>.
                /// </summary>
                public Win32Enumerations.MemoryProtections Protect;
                /// <summary>
                /// Tipo di pagine nella regione.
                /// </summary>
                public Win32Enumerations.MemoryPageType Type;
            }

            /// <summary>
            /// Contiene informazioni su un insieme di pagine nello spazio di indirizzamento virtuale di un processo (64 bit).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MEMORY_BASIC_INFORMATION64
            {
                /// <summary>
                /// Puntatore all'indirizzo di base di una regione di pagine.
                /// </summary>
                public ulong BaseAddress;
                /// <summary>
                /// Puntatore all'indirizzo di base allocate dalla funzione VirtualAlloc, le pagine puntate da <see cref="BaseAddress"/> sono contenute in questo insieme di indirizzi.
                /// </summary>
                public ulong AllocationBase;
                /// <summary>
                /// Protezione della memoria applicata quando la regione è stata inizialmente allocata, questo campo può avere valore corrispondente a una delle costanti di protezione oppure 0 se il chiamante non ha accesso.
                /// </summary>
                public Win32Enumerations.MemoryProtections AllocationProtect;

                public uint __alignment1;
                /// <summary>
                /// Dimensione della regione che inizia dall'indirizzo di base in cui tutte le pagine hanno gli stessi attributi, in bytes.
                /// </summary>
                public ulong RegionSize;
                /// <summary>
                /// Lo stato delle pagine nella regione.
                /// </summary>
                public Win32Enumerations.MemoryPageState State;
                /// <summary>
                /// Protezione applicata alle pagine nella regione, questo campo ha uno dei possibili valori del campo <see cref="AllocationProtect"/>.
                /// </summary>
                public Win32Enumerations.MemoryProtections Protect;
                /// <summary>
                /// Tipo di pagine nella regione.
                /// </summary>
                public Win32Enumerations.MemoryPageType Type;

                public uint __alignment2;
            }
            #endregion
            #region File Structures
            #region File Query Structures
            /// <summary>
            /// Informazioni di base su un file.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct FILE_BASIC_INFO
            {
                /// <summary>
                /// Data di creazione, formato FILETIME.
                /// </summary>
                public long CreationTime;
                /// <summary>
                /// Data ultimo accesso, formato FILETIME.
                /// </summary>
                public long LastAccessTime;
                /// <summary>
                /// Data ultima scrittura, formato FILETIME.
                /// </summary>
                public long LastWriteTime;
                /// <summary>
                /// Data ultima modifica, formato FILETIME.
                /// </summary>
                public long ChangeTime;
                /// <summary>
                /// Attributi.
                /// </summary>
                public Win32Enumerations.FileAttributes FileAttributes;
            }

            /// <summary>
            /// Informazioni standard su un file.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct FILE_STANDARD_INFO
            {
                /// <summary>
                /// Spazio allocato per il file.
                /// </summary>
                public long AllocationSize;
                /// <summary>
                /// Fine del file.
                /// </summary>
                public long EndOfFile;
                /// <summary>
                /// Numero di collegamenti al file.
                /// </summary>
                public uint NumberOfLinks;
                /// <summary>
                /// Indica se il file deve essere eliminato.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool DeletePending;
                /// <summary>
                /// Indica se l'handle si riferisce a una directory.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool Directory;
            }

            /// <summary>
            /// Informazioni di identificazione per un file.
            /// </summary>
            /// <remarks>Questa struttura contiene dati per identificare un file in modo univoco su un singolo computer.</remarks>
            [StructLayout(LayoutKind.Sequential)]
            public struct FILE_ID_INFO
            {
                /// <summary>
                /// Numero seriale del volume che contiene il file.
                /// </summary>
                public ulong VolumeSerialNumber;
                /// <summary>
                /// Identificatore a 128 bit del file.
                /// </summary>
                public FILE_ID_128 FileID;
            }

            /// <summary>
            /// Identificatore a 128 bit di un file.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct FILE_ID_128
            {
                /// <summary>
                /// Indentificatore.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] Identifier;
            }

            /// <summary>
            /// Informazioni sugli attributi di un file.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct WIN32_FILE_ATTRIBUTE_DATA
            {
                /// <summary>
                /// Attributi del file.
                /// </summary>
                public Win32Enumerations.FileAttributes FileAttributes;
                /// <summary>
                /// Data di creazione.
                /// </summary>
                public FILETIME CreationTime;
                /// <summary>
                /// Data ultimo accesso.
                /// </summary>
                public FILETIME LastAccessTime;
                /// <summary>
                /// Data ultima modifica.
                /// </summary>
                public FILETIME LastWriteTime;
                /// <summary>
                /// 32 bit più significativi della dimensione del file.
                /// </summary>
                public uint FileSizeHigh;
                /// <summary>
                /// 32 bit meno significativi della dimensione del file.
                /// </summary>
                public uint FileSizeLow;
            }
            #endregion
            #region File Version Structures
            /// <summary>
            /// Informazioni fisse di versione su un file.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct VS_FIXEDFILEINFO
            {
                /// <summary>
                /// Contiene il valore 0xFEEF04BD.
                /// </summary>
                public uint Signature;
                /// <summary>
                /// Numero di versione binario dells struttura, i 16 bit più significativi contengono il numero di versione maggiore, i 16 bit meno significativi contengono il numero di versione minore.
                /// </summary>
                public uint StructVersion;
                /// <summary>
                /// I 32 bit più significativi del numero di versione binario.
                /// </summary>
                public uint FileVersionMS;
                /// <summary>
                /// I 32 bit meno significativi del numero di versione binario.
                /// </summary>
                public uint FileVersionLS;
                /// <summary>
                /// I 32 bit più significativi del numero di versione binario del prodotto con cui il file è stato distribuito.
                /// </summary>
                public uint ProductVersionMS;
                /// <summary>
                /// I 32 bit meno significativi del numero di versione binario del prodotto con cui il file è stato distribuito.
                /// </summary>
                public uint ProductVersionLS;
                /// <summary>
                /// Maschera di bit che specifica i bit validi in <see cref="FileFlags"/>.
                /// </summary>
                /// <remarks>Un bit è valido solo se è stato definito quando il file è stato creato.</remarks>
                public uint FileFlagsMask;
                /// <summary>
                /// Attributi del file.
                /// </summary>
                public Win32Enumerations.FileFlags FileFlags;
                /// <summary>
                /// Sistema operativo per cui il file è stato creato.
                /// </summary>
                public Win32Enumerations.FileOS FileOS;
                /// <summary>
                /// Tipo di file.
                /// </summary>
                public Win32Enumerations.FileType FileType;
                /// <summary>
                /// Funzione del file.
                /// </summary>
                public Win32Enumerations.FileSubType FileSubtype;
                /// <summary>
                /// I 32 bit più significativi della data di creazione del file.
                /// </summary>
                public uint FileDateMS;
                /// <summary>
                /// I 32 bit meno significativi della data di creazione del file.
                /// </summary>
                public uint FileDateLS;
            }
            #endregion
            #endregion
            #region NT API Structures
            #region NtQueryObject Structures
            /// <summary>
            /// Informazioni di base su un oggetto (struttura pubblica).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PUBLIC_OBJECT_BASIC_INFORMATION
            {
                public uint Attributes;
                public uint GrantedAccess;
                public uint HandleCount;
                public uint PointerCount;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public uint[] Reserved;
            }
            /// <summary>
            /// Informazioni sul tipo di un oggetto.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct OBJECT_TYPE_INFORMATION
            {
                public UNICODE_STRING TypeName;

                public uint TotalNumberOfObjects;

                public uint TotalNumberOfHandles;

                public uint TotalPagedPoolUsage;

                public uint TotalNonPagedPoolUsage;

                public uint TotalNamePoolUsage;

                public uint TotalHandleTableUsage;

                public uint HighWaterNumberOfObjects;

                public uint HighWaterNumberOfHandles;

                public uint HighWaterPagedPoolUsage;

                public uint HighWaterNonPagedPoolUsage;

                public uint HighWaterNamePoolUsage;

                public uint HighWaterHandleTableUsage;

                public uint InvalidAttributes;

                public GENERIC_MAPPING GenericMapping;

                public uint ValidAccessMask;

                [MarshalAs(UnmanagedType.U1)]
                public bool SecurityRequired;

                [MarshalAs(UnmanagedType.U1)]
                public bool MaintainHandleCount;

                public byte TypeIndex;

                public uint PoolType;

                public ulong DefaultPagedPoolCharge;

                public ulong DefaultNonPagedPoolCharge;
            }

            /// <summary>
            /// Mapping dei permessi.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct GENERIC_MAPPING
            {
                public uint GenericRead;

                public uint GenericWrite;

                public uint GenericExecute;

                public uint GenericAll;
            }

            /// <summary>
            /// Rappresenta una stringa Unicode.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct UNICODE_STRING
            {
                /// <summary>
                /// Dimensione, in bytes, della stringa nel campo <see cref="Buffer"/>.
                /// </summary>
                public ushort Length;
                /// <summary>
                /// Dimensione, in bytes, di <see cref="Buffer"/>.
                /// </summary>
                public ushort MaximumLength;
                /// <summary>
                /// Stringa.
                /// </summary>
                [MarshalAs(UnmanagedType.LPWStr)]
                public string Buffer;
            }

            /// <summary>
            /// Rappresenta una stringa Unicode.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct UNICODE_STRING2
            {
                /// <summary>
                /// Dimensione, in bytes, della stringa nel campo <see cref="Buffer"/>.
                /// </summary>
                public ushort Length;
                /// <summary>
                /// Dimensione, in bytes, di <see cref="Buffer"/>.
                /// </summary>
                public ushort MaximumLength;
                /// <summary>
                /// Stringa.
                /// </summary>
                public IntPtr Buffer;
            }
            #endregion
            #region NtQuerySystemInformation Structures
            /// <summary>
            /// Informazioni sugli handle esistenti nel sistema.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SYSTEM_HANDLE_INFORMATION_EX
            {
                public UIntPtr NumberOfHandles;

                public UIntPtr Reserved;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
                public SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] Handles;
            }

            /// <summary>
            /// Informazioni su un handle.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
            {
#pragma warning disable CA1720 // L'identificatore contiene il nome di tipo
                public IntPtr Object;
#pragma warning restore CA1720 // L'identificatore contiene il nome di tipo

                public IntPtr ProcessID;

                public IntPtr HandleValue;

                public uint GrantedAccess;

                public ushort CreatorBackTraceIndex;

                public byte ObjectTypeIndex;

                public uint HandleAttributes;

                public uint Reserved;
            }

            /// <summary>
            /// Informazioni su un thread.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SYSTEM_THREAD_INFORMATION
            {
                /// <summary>
                /// Membro riservato.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public long[] Reserved1;
                /// <summary>
                /// Membro riservato.
                /// </summary>
                public uint Reserved2;
                /// <summary>
                /// Indirizzo di memoria del thread.
                /// </summary>
                public IntPtr StartAddress;
                /// <summary>
                /// Struttura <see cref="CLIENT_ID"/> che contiene l'ID del thread e del processo ad esso collegato.
                /// </summary>
                public CLIENT_ID ClientId;
                /// <summary>
                /// Priorità dinamica del thread.
                /// </summary>
                public int Priority;
                /// <summary>
                /// Priorità base del thread.
                /// </summary>
                public int BasePriority;
                /// <summary>
                /// Numero di context switches.
                /// </summary>
                public uint ContextSwitches;
                /// <summary>
                /// Stato del thread.
                /// </summary>
                public Win32Enumerations.ThreadState ThreadState;
                /// <summary>
                /// Motivo per cui il thread è in attesa.
                /// </summary>
                public Win32Enumerations.ThreadWaitReason WaitReason;
            }

            /// <summary>
            /// 
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct CLIENT_ID
            {
                public IntPtr UniqueProcess;
                public IntPtr UniqueThread;
            }

            /// <summary>
            /// Informazioni su un file di paging.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SYSTEM_PAGEFILE_INFORMATION
            {
                public uint NextEntryOffset;

                public uint TotalSize;

                public uint TotalInUse;

                public uint PeakUsage;

                public UNICODE_STRING PageFileName;
            }
            #endregion
            #region NtQueryTimer Structures
            /// <summary>
            /// Informazioni di base su un timer.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TIMER_BASIC_INFORMATION
            {
                /// <summary>
                /// Tempo fino al prossimo segnale oppure tempo passato dall'ultimo segnale.
                /// </summary>
                public long RemainingTime;
                /// <summary>
                /// Indica se il timer è stato segnalato.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool TimerState;
            }
            #endregion
            #region NtQuerySemaphore Structures
            /// <summary>
            /// Informazioni di base su un semaforo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SEMAPHORE_BASIC_INFORMATION
            {
                /// <summary>
                /// Stato attuale del contatore.
                /// </summary>
                public uint CurrentCount;
                /// <summary>
                /// Contatore massimo.
                /// </summary>
                public uint MaximumCount;
            }
            #endregion
            #region NtQuerySection Structures
            /// <summary>
            /// Informazioni di base su una sezione.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SECTION_BASIC_INFORMATION
            {
                /// <summary>
                /// Sconosciuto, sempre impostato a 0.
                /// </summary>
                public uint Unknown;
                /// <summary>
                /// Attributi della sezione.
                /// </summary>
                public Win32Enumerations.SectionAttributes SectionAttributes;
                /// <summary>
                /// Dimensione della sezione, in bytes.
                /// </summary>
                public ulong SectionSize;
            }

            /// <summary>
            /// Informazioni sull'immagine associata a una sezione.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SECTION_IMAGE_INFORMATION
            {
                /// <summary>
                /// Punto di entrata.
                /// </summary>
                public IntPtr EntryPoint;
                /// <summary>
                /// Numero di bit dalla parte sinistra dell'indirizzo dello stack, esso deve essere 0.
                /// </summary>
                public uint StackZeroBits;
                /// <summary>
                /// Dimensione totale dello stack, in bytes.
                /// </summary>
                public uint StackReserved;
                /// <summary>
                /// Dimensione iniziale del blocco dello stack.
                /// </summary>
                public uint StackCommit;
                /// <summary>
                /// Sottosistema dell'immagine.
                /// </summary>
                public Subsystem ImageSubsystem;
                /// <summary>
                /// 16 bit meno significativi della versione del sottosistema.
                /// </summary>
                public short SubsystemVersionLow;
                /// <summary>
                /// 16 bit più significativi della versione del sottosistema.
                /// </summary>
                public short SubsystemVersionHigh;
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                public uint Unknown1;
                /// <summary>
                /// Caratteristiche DLL.
                /// </summary>
                public DllCharacteristics ImageCharacteristics;
                /// <summary>
                /// Tipo di macchina.
                /// </summary>
                public MachineType ImageMachineType;
                /// <summary>
                /// Sconosciuto.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public uint[] Unknown2;
            }
            #endregion
            #region NtQueryMutant Structures

            [StructLayout(LayoutKind.Sequential)]
            public struct MUTANT_BASIC_INFORMATION
            {
                /// <summary>
                /// Se questo valore è minore di 0, il mutante è segnalato.
                /// </summary>
                public int CurrentCount;
                /// <summary>
                /// Indica se il mutante è stato segnalato dal thread chiamante.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool OwnedByCaller;
                /// <summary>
                /// Indica se il mutante non è stato rilasciato prima della chiusura del thread.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool AbandonedState;
            }
            #endregion
            #region NtQueryEvent Structures
            /// <summary>
            /// Informazioni di base su un evento.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct EVENT_BASIC_INFORMATION
            {
                /// <summary>
                /// Tipo di evento.
                /// </summary>
                public Win32Enumerations.EventType EventType;
                /// <summary>
                /// Stato attuale.
                /// </summary>
                public int EventState;
            }
            #endregion
            #endregion
            #region Service Structures
            /// <summary>
            /// Dati su una notifica.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_NOTIFY_2
            {
                /// <summary>
                /// Versione della struttura.
                /// </summary>
                public uint Version;
                /// <summary>
                /// Callback.
                /// </summary>
                public ServiceStatusChangeCallback NotifyCallback;
                /// <summary>
                /// Parametro da passare al callback.
                /// </summary>
                public IntPtr Context;
                /// <summary>
                /// Stato della notifica.
                /// </summary>
                /// <remarks>Se questo membro ha valore <see cref="Win32Constants.ERROR_SUCCESS"/> la notifica ha avuto successo e <see cref="ServiceStatus"/> contiene informazioni valide.<br/>
                /// Se questo membro ha valore <see cref="Win32Constants.ERROR_SERVICE_MARKED_FOR_DELETE"/>, il servizio sta per essere eliminato e l'handle utilizzato per <see cref="Win32ServiceFunctions.NotifyServiceStatusChange"/> deve essere chiuso.</remarks>
                public uint NotificationStatus;
                /// <summary>
                /// Stato del servizio.
                /// </summary>
                public SERVICE_STATUS_PROCESS ServiceStatus;
                /// <summary>
                /// Eventi che hanno causato la notifica.
                /// </summary>
                public Win32Enumerations.ServiceNotificationReasons NotificationTriggered;
                /// <summary>
                /// Nomi dei servizi coinvolti.
                /// </summary>
                /// <remarks>Questo membro è valido quando <see cref="NotificationStatus"/> ha valore <see cref="Win32Constants.ERROR_SUCCESS"/> e l'evento che ha causato la notifica è <see cref="Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_CREATED"/> oppure <see cref="Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETED"/>.<br/>
                /// I nomi dei servizi creati iniziano con "/" per distringuerli da quelli eliminati.</remarks>
                public IntPtr ServiceNames;
            }
            #region Info Structures
            /// <summary>
            /// Informazioni sullo stato di un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_STATUS_PROCESS
            {
                /// <summary>
                /// Tipo di servizio.
                /// </summary>
                public Win32Enumerations.ServiceType ServiceType;
                /// <summary>
                /// Stato del servizio.
                /// </summary>
                public Win32Enumerations.ServiceState CurrentState;
                /// <summary>
                /// Codici di controllo accettati.
                /// </summary>
                public Win32Enumerations.ServiceAcceptedControlCodes ControlsAccepted;
                /// <summary>
                /// Codice di uscita Win32.
                /// </summary>
                public uint Win32ExitCode;
                /// <summary>
                /// Codice di uscita specifico del servizio.
                /// </summary>
                public uint ServiceSpecificExitCode;
                /// <summary>
                /// Valore che il servizio incrementa periodicamente per indicare il progresso durante un avvio, arresto, messa in pausa e ripresa di lunga durata.
                /// </summary>
                public uint CheckPoint;
                /// <summary>
                /// Il tempo stimato per il completamento di un'operazione di avvio, arresto, messa in pausa e ripresa, in millisecondi.
                /// </summary>
                public uint WaitHint;
                /// <summary>
                /// ID del processo del servizio.
                /// </summary>
                public uint ProcessID;
                /// <summary>
                /// Caratteristiche del servizio.
                /// </summary>
                public Win32Enumerations.ServiceFlags ServiceFlags;
            }

            /// <summary>
            /// Informazioni su un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct ENUM_SERVICE_STATUS_PROCESS
            {
                /// <summary>
                /// Nome del servizio nel database.
                /// </summary>
                public string ServiceName;
                /// <summary>
                /// Nome usato dalle applicazione per identificare il servizio.
                /// </summary>
                public string DisplayName;
                /// <summary>
                /// Stato del servizio.
                /// </summary>
                public SERVICE_STATUS_PROCESS ServiceStatusProcess;
            }

            /// <summary>
            /// Informazioni sulla configurazione di un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct QUERY_SERVICE_CONFIG
            {
                /// <summary>
                /// Tipo di servizio.
                /// </summary>
                public Win32Enumerations.ServiceType ServiceType;
                /// <summary>
                /// Modalità di avvio del servizio.
                /// </summary>
                public Win32Enumerations.ServiceStartType StartType;
                /// <summary>
                /// Modalità di gestione errori del servizio.
                /// </summary>
                public Win32Enumerations.ServiceErrorControlMode ErrorControlMode;
                /// <summary>
                /// Percorso completo del file che ospita il servizio.
                /// </summary>
                public string BinaryPathName;
                /// <summary>
                /// Nome del gruppo di caricamento a cui questo servizio appartiene.
                /// </summary>
                /// <remarks>Se questo membro è nullo o una stringa vuota, il servizio non appartiene a un gruppo.</remarks>
                public string LoadOrderGroup;
                /// <summary>
                /// Valore univoco che identifica il servizio nel gruppo di caricamento.
                /// </summary>
                /// <remarks>Se questo membro ha valore 0 al servizio non è stato assegnato nessun valore univoco.</remarks>
                public uint TagID;
                /// <summary>
                /// Puntatore a un array di nomi di servizi o di gruppi di caricamento che devono essere caricati prima di questo servizio.
                /// </summary>
                /// <remarks>Se il puntatore è nullo o se punta a una stringa vuota il servizio non ha dipendenze.<br/>
                /// Se un nome di un gruppo è specificato deve essere prefisso dal carattere <see cref="Win32Constants.SC_GROUP_INDENTIFIER"/>.</remarks>
                public IntPtr Dependencies;
                /// <summary>
                /// Nome dell'account sotto cui il servizio deve essere eseguito.
                /// </summary>
                public string ServiceStartName;
                /// <summary>
                /// Nome descrittivo del servizio.
                /// </summary>
                public string DisplayName;
            }
            #region Optional Info Structures
            /// <summary>
            /// Rappresenta un'azione che Gestione Controllo Servizi può eseguire.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SC_ACTION
            {
                /// <summary>
                /// Azione da eseguire.
                /// </summary>
                public Win32Enumerations.ServiceControlManagerAction Type;
                /// <summary>
                /// Tempo di attesa prima di eseguire l'azione, in millisecondi.
                /// </summary>
                public uint Delay;
            }

            /// <summary>
            /// Rappresenta le azioni che Gestione Controllo Servizi dovrebbe eseguire per ogni crash del servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SERVICE_FAILURE_ACTIONS
            {
                /// <summary>
                /// Tempo, in secondi, dopo il quale resettare il contatore dei crash del servizio se non ce ne sono nel frattempo.
                /// </summary>
                /// <remarks>Specificare <see cref="Win32Constants.INFINITE"/> per indicare che il contatore non deve essere resettato.</remarks>
                public uint ResetPeriod;
                /// <summary>
                /// Messaggio che deve essere inviato agli utenti del server prima di riavviare in risposta all'azione <see cref="Win32Enumerations.ServiceControlManagerAction.SC_ACTION_REBOOT"/> di Gestore Controllo Servizi.
                /// </summary>
                /// <remarks>Se questo membro è nullo, il messaggio non cambia, se è una stringa vuota il messaggio viene cancellato e non viene inviato nessun messaggio.</remarks>
                public string RebootMessage;
                /// <summary>
                /// Linea di comando del processo da eseguire in risposta all'azione <see cref="Win32Enumerations.ServiceControlManagerAction.SC_ACTION_RUN_COMMAND"/> di Gestione Controllo Servizi.
                /// </summary>
                /// <remarks>Se questo campo è nullo, il comando non cambia, se il valore è una stringa vuota la linea di comando viene cancellata e non viene eseguito alcun programma al crash del servizio.</remarks>
                public string Command;
                /// <summary>
                /// Numero di elementi nell'array puntato da <see cref="Actions"/>.
                /// </summary>
                /// <remarks>Se questo campo ha valore 0 ma <see cref="Actions"/> non ha valore <see cref="IntPtr.Zero"/> il periodo di reset e le azioni in caso di crash vengono eliminate.</remarks>
                public uint ActionsCount;
                /// <summary>
                /// Puntatore a un array di struttura <see cref="Win32Structures.SC_ACTION"/>.
                /// </summary>
                /// <remarks>Se questo campo ha valore <see cref="IntPtr.Zero"/> I membri <see cref="ActionsCount"/> e <see cref="ResetPeriod"/> sono ignorati.</remarks>
                public IntPtr Actions;
            }

            /// <summary>
            /// Rappresenta il nodo preferito su cui eseguire un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_PREFERRED_NODE_INFO
            {
                /// <summary>
                /// Numero del nodo preferito.
                /// </summary>
                public ushort PreferredNode;
                /// <summary>
                /// true se l'impostazione deve essere eliminata, false altrimenti.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool Delete;
            }

            /// <summary>
            /// Descrizione di un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SERVICE_DESCRIPTION
            {
                /// <summary>
                /// Descrizione del servizio.
                /// </summary>
                public string Description;
            }

            /// <summary>
            /// Impostazioni dell'avvio ritardato del servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_DELAYED_AUTO_START_INFO
            {
                /// <summary>
                /// Indica se l'avvio automatico del servizio è ritardato.
                /// </summary>
                [MarshalAs(UnmanagedType.Bool)]
                public bool DelayedAutoStart;
            }

            /// <summary>
            /// Impostazioni aggiuntive relative alle azioni in caso di crash di un servizio.
            /// </summary>
            /// <remarks>Questa impostazione determina quando eseguire le azioni.</remarks>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_FAILURE_ACTIONS_FLAG
            {
                /// <summary>
                /// Indica se eseguire le azioni in caso di crash anche in caso di termine previsto del servizio.
                /// </summary>
                [MarshalAs(UnmanagedType.Bool)]
                public bool FailureActionsOnNonCrashFailures;
            }

            /// <summary>
            /// Contiene impostazioni di presegnimento.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_PRESHUTDOWN_INFO
            {
                /// <summary>
                /// Timeout, in millisecondi.
                /// </summary>
                public uint PreshutdownTimeout;
            }

            /// <summary>
            /// Rappresenta i privilegi richiesti per un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_REQUIRED_PRIVILEGES_INFO
            {
                /// <summary>
                /// Lista privilegi richiesti (multi-stringa).
                /// </summary>
                public IntPtr RequiredPrivileges;
            }

            /// <summary>
            /// Contiene informazioni sul SID di un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_SID_INFO
            {
                /// <summary>
                /// Tipo SID.
                /// </summary>
                public Win32Enumerations.ServiceSIDType ServiceSidType;
            }

            /// <summary>
            /// Indica il tipo di protezione di un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_LAUNCH_PROTECTED_INFO
            {
                /// <summary>
                /// Tipo di protezione del servizio.
                /// </summary>
                public Win32Enumerations.ServiceLaunchProtectionType LaunchProtected;
            }

            /// <summary>
            /// Contiene informazioni sui trigger per un servizio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_TRIGGER_INFO
            {
                /// <summary>
                /// Numero di elementi nell'array di strutture <see cref="SERVICE_TRIGGER"/> puntato da <see cref="Triggers"/>.
                /// </summary>
                public uint TriggersCount;
                /// <summary>
                /// Puntatore a un array di struttura <see cref="SERVICE_TRIGGER"/> che specificano gli eventi trigger per il servizio.
                /// </summary>
                /// <remarks>Se <see cref="TriggersCount"/> ha valore 0, questo campo non è usato.</remarks>
                public IntPtr Triggers;
                /// <summary>
                /// Riservato, deve essere <see cref="IntPtr.Zero"/>.
                /// </summary>
                public IntPtr Reserved;
            }

            /// <summary>
            /// Contiene i dati specifici per un evento trigger per un servizio.
            /// </summary>
            /// <remarks>Questa struttura è utilizzata dalla struttura <see cref="Win32Structures.SERVICE_TRIGGER"/> per i seguenti eventi:<br/><br/>
            /// <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_CUSTOM"/><br/>
            /// <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_DEVICE_INTERFACE_ARRIVAL"/><br/>
            /// <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_FIREWALL_PORT_EVENT"/><br/>
            /// <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_NETWORK_ENDPOINT"/></remarks>
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_TRIGGER_SPECIFIC_DATA_ITEM_STRING
            {
                /// <summary>
                /// Tipo di dati.
                /// </summary>
                public Win32Enumerations.ServiceTriggerDataType DataType;
                /// <summary>
                /// Dimensione, in bytes, di <see cref="Data"/>.
                /// </summary>
                public uint DataSize;
                /// <summary>
                /// Puntatore ai dati specifici del trigger.
                /// </summary>
                /// <remarks>Se <see cref="DataType"/> ha valore <see cref="Win32Enumerations.ServiceTriggerDataType.SERVICE_TRIGGER_DATA_TYPE_BINARY"/> questo campo è un array di bytes.<br/>
                /// Se <see cref="DataType"/> ha valore <see cref="Win32Enumerations.ServiceTriggerDataType.SERVICE_TRIGGER_DATA_TYPE_STRING"/> questo campo è una stringa oppure una multi-stringa, le stringhe usano la codifica UNICODE.<br/><br/>
                /// I seguenti sono i dati specifici del trigger per tipo di dati:<br/><br/>
                /// <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_CUSTOM"/>: definito dal provider ETW che definisce l'evento personalizzato<br/>
                /// <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_DEVICE_INTERFACE_ARRIVAL"/>: stringa che specifica l'ID hardware o la stringa ID compatibile per la classe dell'interfaccia del dispositivo<br/>
                /// <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_FIREWALL_PORT_EVENT"/>: multi-stringa che specifica la porta, il protocollo e, in modo facoltatiivo, il percorso dell'eseguibile e il nome del servizio in ascolto sull'evento<br/>
                /// <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_NETWORK_ENDPOINT"/>: stringa che specifica la porta, la named pipe o l'interfaccia RPC per l'endpoint di rete</remarks>
                [MarshalAs(UnmanagedType.LPWStr)]
                public char[] Data;
            }

            /// <summary>
            /// Rappresenta un trigger per un evento, questa struttura è usata da <see cref="SERVICE_TRIGGER_INFO"/>.
            /// </summary>
            public struct SERVICE_TRIGGER
            {
                /// <summary>
                /// Tipo di evento.
                /// </summary>
                /// <remarks>Se questo campo ha valore <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_CUSTOM"/>, <see cref="TriggerSubType"/> è il GUID del provider dell'evento, <see cref="DataItems"/> contiene i dati specifici definiti dal provider.<br/><br/>
                /// Se questo campo ha valore <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_DEVICE_INTERFACE_ARRIVAL"/>, <see cref="TriggerSubType"/> specifica il GUID della classe dell'interfaccia del dispositivo, <see cref="DataItems"/> specifica uno o più ID hardware e stringhe ID compatibili per la classe dell'interfaccia del dispositivo.<br/><br/>
                /// Se questo campo ha valore <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_DOMAIN_JOIN"/>, <see cref="TriggerSubType"/> ha valore <see cref="Win32Constants.DOMAIN_JOIN_GUID"/> oppure <see cref="Win32Constants.DOMAIN_LEAVE_GUID"/>, <see cref="DataItems"/> non è usato.<br/><br/>
                /// Se questo campo ha valore <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_FIREWALL_PORT_EVENT"/>, <see cref="TriggerSubType"/> ha valore <see cref="Win32Constants.FIREWALL_PORT_OPEN_GUID"/> oppure <see cref="Win32Constants.FIREWALL_PORT_CLOSE_GUID"/>, <see cref="DataItems"/> specifica la porta, il protocollo, e in modo facoltativo, il percorso dell'eseguibile e le informazioni sull'utente (SID o nome) del servizio in ascolto sull'evento.<br/>
                /// Il token "RPC" può essere usato al posto della porta per specificare un socket di ascolto usato da RPC, il token "system" può essere usato al posto del percorso dell'eseguibile per specificare porte create e sulle quali il kernel è in ascolto.<br/><br/>
                /// Se questo campo ha valore <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_GROUP_POLICY"/>, <see cref="TriggerSubType"/> ha valore <see cref="Win32Constants.MACHINE_POLICY_PRESENT_GUID"/> oppure <see cref="Win32Constants.USER_POLICY_PRESENT_GUID"/>, <see cref="DataItems"/> non è usato.<br/><br/>
                /// Se questo campo ha valore <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_IP_ADDRESS_AVAILABILITY"/>, <see cref="TriggerSubType"/> ha valore <see cref="Win32Constants.NETWORK_MANAGER_FIRST_IP_ADDRESS_ARRIVAL_GUID"/> oppure <see cref="Win32Constants.NETWORK_MANAGER_LAST_IP_ADDRESS_REMOVAL_GUID"/>, <see cref="DataItems"/> non è usato.<br/><br></br>
                /// Se questo campo ha valore <see cref="Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_NETWORK_ENDPOINT"/>, <see cref="TriggerSubType"/> ha valore <see cref="Win32Constants.RPC_INTERFACE_EVENT_GUID"/> oppure <see cref="Win32Constants.NAMED_PIPE_EVENT_GUID"/>, <see cref="DataItems"/> specifica il GUID di un endpoint o di una interfaccia, <see cref="Action"/> deve essere <see cref="Win32Enumerations.ServiceTriggerAction.SERVICE_TRIGGER_ACTION_SERVICE_START"/>.</remarks>
                public Win32Enumerations.ServiceTriggerType TriggerType;
                /// <summary>
                /// Azione da eseguire.
                /// </summary>
                public Win32Enumerations.ServiceTriggerAction Action;
                /// <summary>
                /// GUID che identifica il sottotipo dell'evento.
                /// </summary>
                /// <remarks>Per altre informazioni vedere <see cref="TriggerType"/>.</remarks>
                public Guid TriggerSubType;
                /// <summary>
                /// Numero di strutture <see cref="SERVICE_TRIGGER_SPECIFIC_DATA_ITEM"/> nell'array puntato da <see cref="DataItems"/>.
                /// </summary>
                /// <remarks>Questo membro è valido solo in alcuni casi, vedere <see cref="TriggerType"/>.</remarks>
                public uint DataItemsCount;
                /// <summary>
                /// Puntatore a un array di strutture <see cref="SERVICE_TRIGGER_SPECIFIC_DATA_ITEM"/> che contengono le informazioni specifiche sul trigger.
                /// </summary>
                public IntPtr DataItems;
            }
            #endregion
            #endregion
            #endregion
            #region Computer Info Structures
            /// <summary>
            /// Contiene informazioni sul sistema.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SYSTEM_INFO
            {
                /// <summary>
                /// Architettura del processore.
                /// </summary>
                public Win32Enumerations.ProcessorArchitecture ProcessorArchitecture;
                /// <summary>
                /// Membro riservato.
                /// </summary>
                public ushort Reserved;
                /// <summary>
                /// Dimensione delle pagine in memoria.
                /// </summary>
                public uint PageSize;
                /// <summary>
                /// Indirizzo di memoria minimo accessibile alle applicazioni e alle DLL.
                /// </summary>
                public IntPtr MinimumApplicationAddress;
                /// <summary>
                /// Indirizzo di memoria massimo accessibile alle applicazione e alle DLL.
                /// </summary>
                public IntPtr MaximumApplicationAddress;
                /// <summary>
                /// Maschera che rappresenta i processori attivi.
                /// </summary>
                public UIntPtr ActiveProcessorMask;
                /// <summary>
                /// Numero di processori logici.
                /// </summary>
                public uint NumberOfProcessors;
                /// <summary>
                /// Tipo di processore (membro obsoleto).
                /// </summary>
                public uint ProcessorType;
                /// <summary>
                /// Granularità per l'indirizzo minimo dove la memoria virtuale può essere allocata.
                /// </summary>
                public uint AllocationGranularity;
                /// <summary>
                /// Livello del processore, dipende dall'architettura.
                /// </summary>
                public ushort ProcessorLevel;
                /// <summary>
                /// Revisione del processore, dipende dall'architettura.
                /// </summary>
                public ushort ProcessorRevision;
            }

            /// <summary>
            /// Informazioni su un profilo hardware.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct HW_PROFILE_INFO
            {
                /// <summary>
                /// Stato di collegamento alla base del computer.
                /// </summary>
                public Win32Enumerations.DockingState DockInfo;
                /// <summary>
                /// GUID del profilo hardware corrente.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Win32Constants.HW_PROFILE_GUIDLEN)]
                public string HwProfileGuid;
                /// <summary>
                /// Mone descrittivo del profilo hardware corrente.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Win32Constants.MAX_PROFILE_LEN)]
                public string HwProfileName;
            }
            #region Logical Processors Info Structures
            /// <summary>
            /// Affinità di un processore specifica del gruppo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct GROUP_AFFINITY
            {
                /// <summary>
                /// Affinità di 0 o più processori all'interno del gruppo.
                /// </summary>
                public IntPtr Mask;
                /// <summary>
                /// Numero del gruppo.
                /// </summary>
                public ushort Group;
                /// <summary>
                /// Riservato.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public ushort[] Reserved;
            }

            /// <summary>
            /// Informazioni sull'affinità in un gruppo di processori.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESSOR_RELATIONSHIP
            {
                /// <summary>
                /// 1 se il campo <see cref="SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX.Relationship"/> ha valore <see cref="Win32Enumerations.ProcessorRelationshipType.RelationProcessorCore"/>, se il campo ha valore <see cref="Win32Enumerations.ProcessorRelationshipType.RelationProcessorPackage"/> questo campo ha valore 0.
                /// </summary>
                public byte Flags;
                /// <summary>
                /// Esprime la performance di un processore rispetto agli altri, questo campo è diverso da 0 solo su sistemi con un set eterogeneo di core.
                /// </summary>
                public byte EfficiencyClass;
                /// <summary>
                /// Riservato.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] Reserved;
                /// <summary>
                /// Numero di elementi nel campo <see cref="GroupMask"/>.
                /// </summary>
                public ushort GroupCount;
                /// <summary>
                /// Array di strutture <see cref="GROUP_AFFINITY"/> che specificano un numero di gruppo e l'affinità del processore nel gruppo.
                /// </summary>
                public IntPtr GroupMask;
            }

            /// <summary>
            /// Informazioni sui gruppi di processori.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct GROUP_RELATIONSHIP
            {
                /// <summary>
                /// Numero massimo di gruppi di processori nel sistema.
                /// </summary>
                public ushort MaximumGroupCount;
                /// <summary>
                /// Numero di gruppi attivi nel sistema, questo campo indica quante strutture <see cref="PROCESSOR_GROUP_INFO"/> sono contenute nel campo <see cref="GroupInfo"/>.
                /// </summary>
                public ushort ActiveGroupCount;
                /// <summary>
                /// Riservato.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] Reserved;
                /// <summary>
                /// Array di strutture <see cref="PROCESSOR_GROUP_INFO"/>, ogni struttura rappresenta il numero e l'affinità dei processori in un gruppo attivo nel sistema.
                /// </summary>
                public IntPtr GroupInfo;
            }

            /// <summary>
            /// Rappresenta il numero e l'affinità dei processori in un gruppo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESSOR_GROUP_INFO
            {
                /// <summary>
                /// Numero massimo di processori nel gruppo.
                /// </summary>
                public byte MaximumProcessorCount;
                /// <summary>
                /// Numero di processori attivi nel gruppo.
                /// </summary>
                public byte ActiveProcessorCount;
                /// <summary>
                /// Riservato.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 38)]
                public byte[] Reserved;
                /// <summary>
                /// Affinità di 0 o più processori attivi nel gruppo.
                /// </summary>
                public int ActiveProcessorMask;
            }

            /// <summary>
            /// Informazioni sui processori logici e hardware correlato.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX
            {
                /// <summary>
                /// Tipo di relazione tra i processori logici.
                /// </summary>
                public Win32Enumerations.ProcessorRelationshipType Relationship;
                /// <summary>
                /// Dimensione della struttura, in bytes.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Puntatore a una struttura che contiene le informazioni richieste.
                /// </summary>
                public IntPtr RequestedInfo;
            }
            #endregion
            /// <summary>
            /// Informazioni sullo stato attuale della memoria fisica e virtuale, inclusa la memoria estesa.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MEMORYSTATUSEX
            {
                /// <summary>
                /// Dimensione della struttura, in bytes.
                /// </summary>
                public uint Length;
                /// <summary>
                /// Percentuale di utilizzo della memoria.
                /// </summary>
                public uint MemoryLoad;
                /// <summary>
                /// Quantità di memoria fisica, in bytes.
                /// </summary>
                public ulong TotalPhys;
                /// <summary>
                /// Quantità di memoria fisica disponibile attualmente, in bytes.
                /// </summary>
                /// <remarks>Questo campo indica la quantità di memoria fisica che può essere immediatamente riutilizzata senza scriverne i contenuti su disco prima.<br/>
                /// Corrisponde al valore della somma delle liste standby, libera e azzerata.</remarks>
                public ulong AvailPhys;
                /// <summary>
                /// Attuale limite di memoria mappata per il sistema o il processo corrente, il minore tra i due, in bytes.
                /// </summary>
                public ulong TotalPageFile;
                /// <summary>
                /// Quantità di memoria massima che il processo corrente può mappare, in bytes.
                /// </summary>
                /// <remarks>Il valore di questo campo è uguale o minore della memoria massima mappabile dal sistema.</remarks>
                public ulong AvailPageFile;
                /// <summary>
                /// Dimensione della porzione in modalità utente dello spazio di indirizzamento virtuale del processo chiamante, in bytes.
                /// </summary>
                public ulong TotalVirtual;
                /// <summary>
                /// Quantità di memoria non riservata e non mappata attualemente nella porzione in modalità utente dello spazio di indirizzamento virtuale del processo chiamante, in bytes.
                /// </summary>
                public ulong AvailVirtual;
                /// <summary>
                /// Riservato, ha sempre valore 0.
                /// </summary>
                public ulong AvailExtendedVirtual;
            }

            /// <summary>
            /// Informazioni di performance.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PERFORMANCE_INFORMATION
            {
                /// <summary>
                /// Dimensione della struttura, in bytes.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Numero di pagine mappate dal sistema.
                /// </summary>
                public UIntPtr CommitTotal;
                /// <summary>
                /// Numero massimo di pagine che possono essere mappate dal sistema senza estendere il file di paging.
                /// </summary>
                public UIntPtr CommitLimit;
                /// <summary>
                /// Massimo numero di pagine che sono simultaneamente mappate dall'ultimo riavvio del sistema.
                /// </summary>
                public UIntPtr CommitPeak;
                /// <summary>
                /// Quantità di memoria fisica, in pagine.
                /// </summary>
                public UIntPtr PhysicalTotal;
                /// <summary>
                /// Quantità di memoria fisica disponibile, in pagine
                /// </summary>
                /// <remarks>Questo campo indica la quantità di memoria fisica che può essere immediatamente riutilizzata senza scriverne i contenuti su disco prima.<br/>
                /// Corrisponde al valore della somma delle liste standby, libera e azzerata.</remarks>
                public UIntPtr PhysicalAvailable;
                /// <summary>
                /// Qunatità di memoria cache del sistema, in pagine.
                /// </summary>
                /// <remarks>Questo campo rappresenta la dimensione della lista di standby e il working set del sistema.</remarks>
                public UIntPtr SystemCache;
                /// <summary>
                /// Somma della memoria attualmente nelle paged e nonpaged pools del kernel, in pagine.
                /// </summary>
                public UIntPtr KernelTotal;
                /// <summary>
                /// Memoria attualmente nel paged pool del kernel, in pagine.
                /// </summary>
                public UIntPtr KernelPaged;
                /// <summary>
                /// Memoria attualmente nel nonpaged pool del kernel, in pagine.
                /// </summary>
                public UIntPtr KernelNonpaged;
                /// <summary>
                /// Dimensione di una pagina, in bytes.
                /// </summary>
                public UIntPtr PageSize;
                /// <summary>
                /// Numero attuale di handle aperti.
                /// </summary>
                public uint HandleCount;
                /// <summary>
                /// Numero attuale di processi aperti.
                /// </summary>
                public uint ProcessCount;
                /// <summary>
                /// Numero attuale di thread.
                /// </summary>
                public uint ThreadCount;
            }
            #endregion
            #region Job Structures
            /// <summary>
            /// Informazioni sul controllo del tasso di utilizzo della CPU per un job.
            /// </summary>
            [StructLayout(LayoutKind.Explicit)]
            public struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
            {
                /// <summary>
                /// Modalità di controllo del tasso di utilizzo della CPU.
                /// </summary>
                [FieldOffset(0)]
                public Win32Enumerations.JobObjectCPURateControl ControlFlags;
                /// <summary>
                /// Porzione dei cicli processore che i thread del job possono usare durante ogni l'intervallo di scheduling espresso come il numero di cicli per 10000.
                /// </summary>
                /// <remarks>Se il campo <see cref="ControlFlags"/> specifica <see cref="Win32Enumerations.JobObjectCPURateControl.CpuRateControlWeightBased"/> oppure <see cref="Win32Enumerations.JobObjectCPURateControl.CpuRateControlMinMaxRate"/>, questo membro non è usato.<br/><br/>
                /// Il valore del campo deve essere impostato a un valore percentuale per 100, 0 non è un valore valido.</remarks>
                [FieldOffset(4)]
                public uint CpuRate;
                /// <summary>
                /// Peso del job, questo valore determina la quantità di tempo processore dato al job relativamente ad altre attività del processore.
                /// </summary>
                /// <remarks>Questo campo può avere un valore da 1 a 9 compresi, 5 è il valore di default.<br/><br/>
                /// Questo valore è usato se <see cref="ControlFlags"/> specifica <see cref="Win32Enumerations.JobObjectCPURateControl.CpuRateControlWeightBased"/>, se <see cref="ControlFlags"/> specifica <see cref="Win32Enumerations.JobObjectCPURateControl.CpuRateControlMinMaxRate"/> questo valore non è usato.</remarks>
                [FieldOffset(4)]
                public uint Weight;
                /// <summary>
                /// Specifica la porzione minima di cicli processore che i thread dek job possono riservare durante ogni intervallo di scheduling.
                /// </summary>
                /// <remarks>Questo valore deve essere specificato come una percentuale per 100, perché i tassi minimi funzionino correttamente la somma di tutti i tassi minimi per tutti i job del sistema non deve superare 10000.</remarks>
                [FieldOffset(4)]
                public ushort MinRate;
                /// <summary>
                /// Specifica la porzione massima di cicli processore che i thread dek job possono riservare durante ogni intervallo di scheduling.
                /// </summary>
                /// <remarks>Questo valore deve essere specificato come una percentuale per 100, una volta raggiunto questo limite per l'intervallo di scheduling, nessun thread associato al job potrà essere eseguito fino al prossimo intervallo.</remarks>
                [FieldOffset(6)]
                public ushort MaxRate;
            }

            /// <summary>
            /// Informazioni di contabilità per un job.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct JOBOBJECT_BASIC_ACCOUNTING_INFORMATION
            {
                /// <summary>
                /// Tempo di esecuzione totale in modalità utente per tutti i processi attivi associati al job, incluso quello dei processi terminati.
                /// </summary>
                /// <remarks>Questo campo è espresso in tick (100 nanosecondi).</remarks>
                public long TotalUserTime;
                /// <summary>
                /// Tempo di esecuzione totale in modalità kernel per tutti i processi attivi associati al job, incluso quello dei processi terminati.
                /// </summary>
                /// <remarks>Questo campo è espresso in tick (100 nanosecondi).</remarks>
                public long TotalKernelTime;
                /// <summary>
                /// Tempo di esecuzione, a partire dall'ultima impostazione di un limite relativo al tempo di esecuzione in modalità utente, per tutti i processi attivi associati al job, incluso quello dei processi terminati.
                /// </summary>
                /// <remarks>Questo campo è espresso in tick (100 nanosecondi).</remarks>
                public long ThisPeriodTotalUserTime;
                /// <summary>
                /// Tempo di esecuzione, a partire dall'ultima impostazione di un limite relativo al tempo di esecuzione in modalità kernel, per tutti i processi attivi associati al job, incluso quello dei processi terminati.
                /// </summary>
                /// <remarks>Questo campo è espresso in tick (100 nanosecondi).</remarks>
                public long ThisPeriodTotalKernelTime;
                /// <summary>
                /// Numero totale di page fault incontrati da tutti i processi attivi associati al job, inclusi quelli incontrati dai processi terminati.
                /// </summary>
                public uint TotalPageFaultCount;
                /// <summary>
                /// Numero totale di processi che sono stati associati al job a partire dalla sua creazione, inclusi i processi terminati.
                /// </summary>
                public uint TotalProcesses;
                /// <summary>
                /// Numero totale di processi attualmente associati al job.
                /// </summary>
                public uint ActiveProcesses;
                /// <summary>
                /// Numero totale di processi terminati a causa di una violazione di limite.
                /// </summary>
                public uint TotalTerminatedProcesses;
            }

            /// <summary>
            /// Informazioni di contabilità di base e I/O per un job.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION
            {
                /// <summary>
                /// Informazioni di contabilità di base.
                /// </summary>
                public JOBOBJECT_BASIC_ACCOUNTING_INFORMATION BasicInfo;
                /// <summary>
                /// Informazioni di contabilità I/O.
                /// </summary>
                public IO_COUNTERS IoInfo;
            }
            #endregion
            #region System Info Structures
            #region Accessibility
            /// <summary>
            /// Informazioni sul timeout per le funzionalità di accessibilità.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ACCESSTIMEOUT
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Impostazioni del timeout.
                /// </summary>
                public Win32Enumerations.AccessibilityTimeoutSettings Flags;
                /// <summary>
                /// Periodo di timeout, in millisecondi.
                /// </summary>
                public uint TimeoutMsec;
            }

            /// <summary>
            /// Informazioni sulla funzionalità Descrizioni Audio.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct AUDIODESCRIPTION
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Indica se la funzionalità è abilitata.
                /// </summary>
                [MarshalAs(UnmanagedType.Bool)]
                public bool Enabled;
                /// <summary>
                /// Lingua dell'audio.
                /// </summary>
                /// <remarks>Questo valore è un identificatore (LCID).</remarks>
                public uint Locale;
            }

            /// <summary>
            /// Informazioni sulla funzionalità Filtro Tasti.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct FILTERKEYS
            {
                /// <summary>
                /// DImensione, della struttura, in bytes.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Impostazioni della funzionalità.
                /// </summary>
                public Win32Enumerations.FilterKeysSettings Flags;
                /// <summary>
                /// Tempo, in millisecondi, prima che il computer accetti la pressione di un tasto.
                /// </summary>
                public uint WaitMsec;
                /// <summary>
                /// Tempo, in millisecondi, che l'utente deve tenere premuto un tasto prima che si ripeta.
                /// </summary>
                public uint DelayMsec;
                /// <summary>
                /// Tempo, in millisecondi, tra ogni ripetizione della pressione del tasto.
                /// </summary>
                public uint RepeatMsec;
                /// <summary>
                /// Tempo, in millisecondi, che deve passare dopo il rilascio di un tasto prima che il computer accetti un'altra pressione del tasto.
                /// </summary>
                public uint BounceMsec;
            }

            /// <summary>
            /// Informazioni sulla funzionalità Alto Contrasto.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct HIGHCONTRAST
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Impostazioni della funzionalità.
                /// </summary>
                public Win32Enumerations.HighContrastSettings Flags;
                /// <summary>
                /// Nome dello schema di colori che sarà impostato come schema di default.
                /// </summary>
                public string DefaultScheme;
            }

            /// <summary>
            /// Informazioni sulla funzionalità MouseKeys.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MOUSEKEYS
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Impostazioni della funzionalità.
                /// </summary>
                public Win32Enumerations.MouseKeysSettings Flags;
                /// <summary>
                /// Velocità massima del cursore del mouse quando una freccia direzionale viene tenuta premuta.
                /// </summary>
                public uint MaxSpeed;
                /// <summary>
                /// Tempo, in millisecondi, che il cursore richiede per raggiungere la velocità massima.
                /// </summary>
                /// <remarks>I valori validi sono tra 1000 e 5000.</remarks>
                public uint TimeToMaxSpeed;
                /// <summary>
                /// Moltiplicatore da applicare alla velocità del cursore del mouse quando il tasto CTRL è tenuto premuto mentre le frecce direzionali sono usate per muovere il cursore.
                /// </summary>
                public uint CtrlSpeed;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint Reserved1;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint Reserved2;
            }

            /// <summary>
            /// Informazioni sulla funzionalità SoundSentry.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SOUNDSENTRY
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Impostazioni della funzionalità.
                /// </summary>
                public Win32Enumerations.SoundSentrySettings Flags;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint TextEffect;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint TextEffectMsec;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint TextEffectColorBits;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint GrafEffect;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint GrafEffectMsec;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint GrafEffectColor;
                /// <summary>
                /// Segnale visuale da visualizzare quando un suono viene generato da un'applicazione basata su Windows o da un'applicazione MS-DOS in esecuzione in una finestra.
                /// </summary>
                public Win32Enumerations.SoundSentryWindowsEffect WindowsEffect;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint WindowsEffectMsec;
                /// <summary>
                /// Riservato.
                /// </summary>
                public string WindowsEffectDLL;
                /// <summary>
                /// Riservato.
                /// </summary>
                public uint WindowsEffectOrdinal;
            }

            /// <summary>
            /// Informazioni sulla funzionalità Tasti Permanenti.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct STICKYKEYS
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Impostazioni della funzionalità.
                /// </summary>
                public Win32Enumerations.StickyKeysSettings Flags;
            }

            /// <summary>
            /// Informazioni sulla funzionalità ToggleKeys.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TOGGLEKEYS
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Impostazioni della funzionalità.
                /// </summary>
                public Win32Enumerations.ToggleKeysSettings Flags;
            }
            #endregion
            #region Icon
            /// <summary>
            /// Attributi di un font.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct LOGFONT
            {
                /// <summary>
                /// Altezza, in unità logiche, di un carattere.
                /// </summary>
                public int Height;
                /// <summary>
                /// Larghezza media, in unità logiche, dei caratteri.
                /// </summary>
                public int Width;
                /// <summary>
                /// Angolo, in decimi di grado, tra il vettore di uscita e l'asse x del dispositivo.
                /// </summary>
                public int Escapement;
                /// <summary>
                /// Angolo, in decimi di grado, tra la base di ogni carattere e l'asse x del dispositivo.
                /// </summary>
                public int Orientation;
                /// <summary>
                /// Il peso del font.
                /// </summary>
                /// <remarks>I valori validi per questo campo vanno da 0 a 1000, se ha valore 0 viene usata un'impostazione di default.</remarks>
                public Win32Enumerations.FontWeight Weight;
                /// <summary>
                /// Indica se il font è corsivo.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool Italic;
                /// <summary>
                /// Indica se il font è sottolineato.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool Underline;
                /// <summary>
                /// Indica se il font è sbarrato.
                /// </summary>
                [MarshalAs(UnmanagedType.U1)]
                public bool StrikeOut;
                /// <summary>
                /// Set di caratteri.
                /// </summary>
                public Win32Enumerations.Charset Charset;
                /// <summary>
                /// Precisione dell'output.
                /// </summary>
                public Win32Enumerations.OutputPrecision OutputPrecision;
                /// <summary>
                /// Precisione del taglio.
                /// </summary>
                public Win32Enumerations.ClipPrecision ClippingPrecision;
                /// <summary>
                /// Qualità del font.
                /// </summary>
                public Win32Enumerations.FontQuality Quality;
                /// <summary>
                /// Inclinazione e famiglia del font.
                /// </summary>
                /// <remarks>I primi 2 bit di questo valore indicano l'inclinazione, i valori validi sono indicati nell'enumerazione <see cref="Win32Enumerations.FontPitch"/>.<br/>
                /// I bit dal 4 al 7 indicano la famiglia del font, i valori validi sono indicati nell'enumerazione <see cref="Win32Enumerations.FontFamily"/>.</remarks>
                public byte PitchAndFamily;
                /// <summary>
                /// Nome del font.
                /// </summary>
                /// <remarks>La lunghezza massima di questa stringa è <see cref="Win32Constants.LF_FACESIZE"/> (32), carattere nullo finale compreso.</remarks>
                public string FaceName;
            }

            /// <summary>
            /// Metriche di scalabilità associate con le icone.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ICONMETRICS
            {
                /// <summary>
                /// Dimensione della struttura, in byte.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Spazio orizzontale, in pixel, per ogni icona disposta.
                /// </summary>
                public int HorizontalSpacing;
                /// <summary>
                /// Spazio verticale, in pixel, per ogni icona disposta.
                /// </summary>
                public int VerticalSpacing;
                /// <summary>
                /// Indica se i titoli delle icone vanno a capo automaticamente (diverso da 0) o meno (0).
                /// </summary>
                public int TitleWrap;
                /// <summary>
                /// Il font da usare per i titoli delle icone.
                /// </summary>
                public LOGFONT Font;
            }
            #endregion
            #region Window Parameters
            /// <summary>
            /// Informazioni sulle animazioni delle finestre.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ANIMATIONINFO
            {
                /// <summary>
                /// Dimensione della struttura, in byte.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Indica se le animazioni di riduzione a icona e di ripristino sono abilitate (diverso da 0) o meno (0).
                /// </summary>
                public int MinAnimate;
            }

            /// <summary>
            /// Metriche di scalabilità associate con le finestre ridotte a icona.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MINIMIZEDMETRICS
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Larghezza delle finestre minimizzate, in pixel.
                /// </summary>
                public int Width;
                /// <summary>
                /// Spazio orizzontale tra finestre minimizzate disposte in fila, in pixel.
                /// </summary>
                public int HorizontalGap;
                /// <summary>
                /// Spazio verticale tra finestre minimizzate disposte in fila, in pixel.
                /// </summary>
                public int VerticalGap;
                /// <summary>
                /// Posizione di partenza e direzione di ordinamento delle finestre minimizzate.
                /// </summary>
                public Win32Enumerations.MinimizedWindowsStartingPositionsAndDirection Arrange;
            }

            /// <summary>
            /// Metriche di scalabilità associate con l'area non client di una finestra non minimizzata.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct NONCLIENTMETRICS
            {
                /// <summary>
                /// Dimensione, in byte, della struttura.
                /// </summary>
                public uint Size;
                /// <summary>
                /// Spessore del bordo di ridimensionamento, in pixel.
                /// </summary>
                public int BorderWidth;
                /// <summary>
                /// Larghezza di una barra di scorrimento verticale, in pixel.
                /// </summary>
                public int ScrollWidth;
                /// <summary>
                /// Altezza di una barra di scorrimento orizzontale, in pixel.
                /// </summary>
                public int ScrollHeight;
                /// <summary>
                /// Larghezza dei bottoni sulla barra del titolo, in pixel.
                /// </summary>
                public int CaptionWidth;
                /// <summary>
                /// Altezza dei bottoni sulla barra del titolo, in pixel.
                /// </summary>
                public int CaptionHeight;
                /// <summary>
                /// Font usato nei bottoni sulla barra del titolo.
                /// </summary>
                public LOGFONT CaptionFont;
                /// <summary>
                /// Larghezza dei bottoni piccoli sulla barra del titolo, in pixel.
                /// </summary>
                public int SmallCaptionWidth;
                /// <summary>
                /// Altezza dei bottoni piccoli sulla barra del titolo, in pixel.
                /// </summary>
                public int SmallCaptionHeight;
                /// <summary>
                /// Font usato sui bottoni piccoli sulla barra del titolo, in pixel.
                /// </summary>
                public LOGFONT SmallCaptionFont;
                /// <summary>
                /// Larghezza dei pulsanti della barra dei menù, in pixel.
                /// </summary>
                public int MenuWidth;
                /// <summary>
                /// Altezza dei pulsanti della barra dei menù, in pixel.
                /// </summary>
                public int MenuHeight;
                /// <summary>
                /// Font usato nelle barre dei menù.
                /// </summary>
                public LOGFONT MenuFont;
                /// <summary>
                /// Font usato nelle barre di stato e nei tooltip.
                /// </summary>
                public LOGFONT StatusFont;
                /// <summary>
                /// Font usato nelle finestre di messaggio.
                /// </summary>
                public LOGFONT MessageFont;
                /// <summary>
                /// Spessore del bordo imbottito, in pixel.
                /// </summary>
                public int PaddedBorderWidth;
            }
            #endregion
            #endregion
        }

        /// <summary>
        /// Funzioni Win32 per la gestione dei processi e dei loro thread.
        /// </summary>
        public static class Win32ProcessFunctions
        {
            #region Enumeration Functions
            /// <summary>
            /// Crea uno snapshot del sistema o di un processo che include le informazioni richieste.
            /// </summary>
            /// <param name="RequestedInformation">Informazione richiesta.</param>
            /// <param name="ProcessID">ID del processo.</param>
            /// <returns>Un handle allo snapshot eseguito.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "CreateToolhelp32Snapshot", SetLastError = true)]
            public static extern IntPtr CreateToolHelp32Snapshot(Win32Enumerations.SnapshotSystemPortions RequestedInformation, uint ProcessID);

            /// <summary>
            /// Recupera le informazioni dal primo processo trovato in uno snapshot del sistema.
            /// </summary>
            /// <param name="SnapshotHandle">Handle nativo dello snapshot.</param>
            /// <param name="ProcessInformationPointer">Struttura <see cref="Win32Structures.PROCESSENTRY32"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "Process32First", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool Process32First(IntPtr SnapshotHandle, ref Win32Structures.PROCESSENTRY32 ProcessInformation);

            /// <summary>
            /// Recupera le informazioni dal prossimo processo trovato in uno snapshot del sistema.
            /// </summary>
            /// <param name="SnapshotHandle">Handle nativo dello snapshot.</param>
            /// <param name="ProcessInformationPointer">Struttura <see cref="Win32Structures.PROCESSENTRY32"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "Process32Next", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool Process32Next(IntPtr SnapshotHandle, ref Win32Structures.PROCESSENTRY32 ProcessInformation);

            /// <summary>
            /// Recupera informazioni sul primo thread trovato in uno snapshot del sistema.
            /// </summary>
            /// <param name="SnapshotHandle">Handle nativo allo snapshot.</param>
            /// <param name="ThreadInformation">Struttura <see cref="Win32Structures.THREADENTRY32"/> contentente le informazioni sul thread.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "Thread32First", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool Thread32First(IntPtr SnapshotHandle, ref Win32Structures.THREADENTRY32 ThreadInformation);

            /// <summary>
            /// Recupera informazioni sul prossimo thread trovato in uno snapshot del sistema.
            /// </summary>
            /// <param name="SnapshotHandle">Handle nativo allo snapshot.</param>
            /// <param name="ThreadInformation">Struttura <see cref="Win32Structures.THREADENTRY32"/> contentente le informazioni sul thread.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "Thread32Next", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool Thread32Next(IntPtr SnapshotHandle, ref Win32Structures.THREADENTRY32 ThreadInformation);

            /// <summary>
            /// Recupera informazioni sul primo modulo trovato in uno snapshot del sistema.
            /// </summary>
            /// <param name="SnapshotHandle">Handle nativo allo snapshot.</param>
            /// <param name="ModuleInformation">Struttura <see cref="Win32Structures.MODULEENTRY32"/> contenente le informazioni sul modulo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "Module32First", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool Module32First(IntPtr SnapshotHandle, ref Win32Structures.MODULEENTRY32 ModuleInformation);

            /// <summary>
            /// Recupera informazioni sul prossimo modulo trovato in uno snapshot del sistema.
            /// </summary>
            /// <param name="SnapshotHandle">Handle nativo allo snapshot.</param>
            /// <param name="ModuleInformation">Struttura <see cref="Win32Structures.MODULEENTRY32"/> contenente le informazioni sul modulo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "Module32Next", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool Module32Next(IntPtr SnapshotHandle, ref Win32Structures.MODULEENTRY32 ModuleInformation);

            /// <summary>
            /// Apre un processo locale.
            /// </summary>
            /// <param name="DesiredAccess">Tipo di accesso al processo.</param>
            /// <param name="InheritHandle">Indica se i processi figlio del processo corrente possono ereditare l'handle.</param>
            /// <param name="ProcessID">ID del processo da aprire.</param>
            /// <returns>Handle nativo del processo, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
            public static extern IntPtr OpenProcess(Win32Enumerations.ProcessAccessRights DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool InheritHandle, uint ProcessID);
            #endregion
            #region Process Info Retrieval Functions
            #region NT API Functions
            /// <summary>
            /// Richiede informazioni su un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="InfoClass">Informazioni da recuperare.</param>
            /// <param name="ProcessInfo">Struttura che contiene l'informazione richiesta.</param>
            /// <param name="ProcessInfoSize">Dimensione del parametro <paramref name="ProcessInfo"/>, in bytes.</param>
            /// <param name="ReturnSize">Dimensione delle informazioni scritte nel buffer (<paramref name="ProcessInfo"/>).</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("ntdll.dll", EntryPoint = "NtQueryInformationProcess", SetLastError = true)]
            public static extern uint NtQueryInformationProcess(IntPtr ProcessHandle, Win32Enumerations.ProcessInformationClass InfoClass, IntPtr ProcessInfo, uint ProcessInfoSize, out uint ReturnSize);

            /// <summary>
            /// Richiede informazioni su un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <param name="RequestedInfo">Informazione da recuperare.</param>
            /// <param name="ThreadInfo">Struttura che contiene l'informazione richiesta.</param>
            /// <param name="InfoLength">Dimensione del parametro <paramref name="ThreadInfo"/>, in bytes.</param>
            /// <param name="ReturnLength">Dimensione delle informazioni scritte nel buffer (<paramref name="ThreadInfo"/>).</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("ntdll.dll", EntryPoint = "NtQueryInformationThread", SetLastError = true)]
            public static extern uint NtQueryInformationThread(IntPtr ThreadHandle, Win32Enumerations.ThreadInformationClass RequestedInfo, IntPtr ThreadInfo, uint InfoLength, out uint ReturnLength);
            #endregion
            #region Main Properties
            /// <summary>
            /// Recupera il PID di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo del processo.</param>
            /// <returns>Il PID del processo, 0 in caso di errore.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessId", SetLastError = true)]
            public static extern uint GetProcessID(IntPtr ProcessHandle);

            /// <summary>
            /// Recupera il l'ID del processo corrente.
            /// </summary>
            /// <returns>L'ID del processo corrente.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetCurrentProcessId", SetLastError = true)]
            public static extern uint GetCurrentProcessId();

            /// <summary>
            /// Recupera la priorità di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo del processo.</param>
            /// <returns>La priorità del processo, 0 in caso di errore.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetPriorityClass", SetLastError = true)]
            public static extern uint GetPriorityClass(IntPtr ProcessHandle);

            /// <summary>
            /// Recupera le informazioni relative all'utilizzo della memoria da parte di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo del processo.</param>
            /// <param name="CounterInfo">Struttura <see cref="PROCESS_MEMORY_COUNTERS"/> dove archiviare le informazioni.</param>
            /// <param name="StructureSize">Dimensione della struttura <see cref="PROCESS_MEMORY_COUNTERS"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Psapi.dll", EntryPoint = "GetProcessMemoryInfo", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetProcessMemoryInfo(IntPtr ProcessHandle, out Win32Structures.PROCESS_MEMORY_COUNTERS CounterInfo, uint StructureSize);

            /// <summary>
            /// Recupera informazioni sui tempi di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo del processo.</param>
            /// <param name="CreationTime">Data di creazione.</param>
            /// <param name="ExitTime">Data di uscita.</param>
            /// <param name="KernelTime">Tempo kernel.</param>
            /// <param name="UserTime">Tempo utente.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessTimes", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetProcessTimes(IntPtr ProcessHandle, out ulong CreationTime, out ulong ExitTime, out ulong KernelTime, out ulong UserTime);

            /// <summary>
            /// Recupera informazioni sui tempi di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo del processo.</param>
            /// <param name="CreationTime">Data di creazione.</param>
            /// <param name="ExitTime">Data di uscita.</param>
            /// <param name="KernelTime">Tempo kernel.</param>
            /// <param name="UserTime">Tempo utente.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessTimes", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetProcessTimes(IntPtr ProcessHandle, out Win32Structures.FILETIME CreationTime, out Win32Structures.FILETIME ExitTime, out Win32Structures.FILETIME KernelTime, out Win32Structures.FILETIME UserTime);

            /// <summary>
            /// Recupera informazioni sui tempi del sistema.
            /// </summary>
            /// <param name="IdleTime">Tempo di inattività.</param>
            /// <param name="KernelTime">Tempo kernel.</param>
            /// <param name="UserTime">Tempo utente.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetSystemTimes", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetSystemTimes(out ulong IdleTime, out ulong KernelTime, out ulong UserTime);

            /// <summary>
            /// Recupera informazioni sui tempi del sistema.
            /// </summary>
            /// <param name="IdleTime">Tempo di inattività.</param>
            /// <param name="KernelTime">Tempo kernel.</param>
            /// <param name="UserTime">Tempo utente.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetSystemTimes", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetSystemTimes(out Win32Structures.FILETIME IdleTime, out Win32Structures.FILETIME KernelTime, out Win32Structures.FILETIME UserTime);
            #endregion
            #region Detailed Properties (Generic)
            /// <summary>
            /// Recupera il percorso completo dell'eseguibile di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="PathType">Tipo di percorso (questo parametro ha sempre valore 0).</param>
            /// <param name="ExePath">Percorso dell'eseguibile.</param>
            /// <param name="Characters">Numero di caratteri del percorso, in input ha valore 1024.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "QueryFullProcessImageNameW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryFullProcessImageName(IntPtr ProcessHandle, uint PathType, StringBuilder ExePath, ref uint Characters);

            /// <summary>
            /// Recupera il percorso DOS dell'eseguibile di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle al processo.</param>
            /// <param name="FileName">Percorso.</param>
            /// <param name="Size">Dimensione di <paramref name="FileName"/>, in caratteri.</param>
            /// <returns>Lunghezza della stringa in <paramref name="FileName"/>, 0 in caso di errore.</returns>
            [DllImport("Psapi.dll", EntryPoint = "GetProcessImageFileNameW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint GetProcessImageFileName(IntPtr ProcessHandle, StringBuilder FileName, uint Size);

            /// <summary>
            /// Recupera il valore di affinità di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="ProcessAffinityMask">Affinità del processo.</param>
            /// <param name="SystemAffinityMask">Affinità del sistema.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessAffinityMask", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetProcessAffinityMask(IntPtr ProcessHandle, out IntPtr ProcessAffinityMask, out IntPtr SystemAffinityMask);

            /// <summary>
            /// Recupera il codice di uscita di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="ExitCode">Codice di uscita.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetExitCodeProcess", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetExitCodeProcess(IntPtr ProcessHandle, out uint ExitCode);

            /// <summary>
            /// Recupera la politica di mitigazione richiesta per un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="Policy">Politica.</param>
            /// <param name="Buffer">Puntatore a una struttura che contiene le informazioni sulla politica.</param>
            /// <param name="BufferSize">Dimensione del parametro <paramref name="Buffer"/>.</param>
            /// <returns>treu se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessMitigationPolicy", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetProcessMitigationPolicy(IntPtr ProcessHandle, Win32Enumerations.ProcessMitigationPolicy Policy, IntPtr Buffer, long BufferSize);
            #endregion
            #region Detailed Properties (Statistics)
            /// <summary>
            /// Recupera il numero di cicli CPU di esecuzione di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="CycleTime">Numero di cicli CPU di esecuzione.</param>
            /// <returns>true se l'operazione è riuscita, false altrimeni.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "QueryProcessCycleTime", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryProcessCycleTime(IntPtr ProcessHandle, out ulong CycleTime);

            /// <summary>
            /// Recupera informazioni dettagliate su ogni pagina presente nel working set di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="Buffer">Puntatore a una struttura <see cref="Win32Structures.PSAPI_WORKING_SET_INFORMATION"/> che conterrà le informazioni.</param>
            /// <param name="BufferSize">Dimensione di <paramref name="Buffer"/>, in bytes.</param>
            /// <returns>true, se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Se la dimensione del parametro <paramref name="Buffer"/> fornita non è sufficiente la funzione imposta come codice di errore <see cref="Win32Constants.ERROR_BAD_LENGTH"/>.</remarks>
            [DllImport("Psapi.dll", EntryPoint = "QueryWorkingSet", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryWorkingSet(IntPtr ProcessHandle, IntPtr Buffer, uint BufferSize);

            /// <summary>
            /// Recupera informazioni sull'attivita I/O di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="Counters">Struttura <see cref="Win32Structures.IO_COUNTERS"/> che conterrà le informazioni.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessIoCounters", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetProcessIOCounters(IntPtr ProcessHandle, out Win32Structures.IO_COUNTERS Counters);

            /// <summary>
            /// Recupera il numero di handle aperti da un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="HandleCount">Numero di handle aperti dal processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessHandleCount", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetProcessHandleCount(IntPtr ProcessHandle, out uint HandleCount);

            /// <summary>
            /// Recupera il numero di handle aperti agli oggetti GUI del tipo specificato di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="Flags">Tipo di oggetto GUI.</param>
            /// <returns>Il numero di handle aperti, 0 se l'operazione è fallita o se il processo non ha handle aperti del tipo indicato.</returns>
            [DllImport("User32.dll", EntryPoint = "GetGuiResources", SetLastError = true)]
            public static extern uint GetGuiResources(IntPtr ProcessHandle, Win32Enumerations.ProcessGUIObjectType Flags);
            #endregion
            #region Detailed Properties (Threads)
            /// <summary>
            /// Recupera un handle a un thread.
            /// </summary>
            /// <param name="DesiredAccess">Tipo di accesso al thread.</param>
            /// <param name="InheritHandle">Indica se i processi figli del processo richiedente erediteranno l'handle.</param>
            /// <param name="ThreadID">ID del thread.</param>
            /// <returns>Un handle a un thread, <see cref="IntPtr.Zero"/> se l'operazione è fallita.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "OpenThread", SetLastError = true)]
            public static extern IntPtr OpenThread(Win32Enumerations.ThreadAccessRights DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool InheritHandle, uint ThreadID);

            /// <summary>
            /// Recupera i tempi di un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <param name="CreationTime">Data e ora di creazione del thread.</param>
            /// <param name="ExitTime">Data e ora di uscita del thread.</param>
            /// <param name="KernelTime">Tempo che il thread ha passato in modalità kernel.</param>
            /// <param name="UserTime">Tempo che il thread ha passato in modalità utente.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetThreadTimes", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetThreadTimes(IntPtr ThreadHandle, out Win32Structures.FILETIME CreationTime, out Win32Structures.FILETIME ExitTime, out Win32Structures.FILETIME KernelTime, out Win32Structures.FILETIME UserTime);

            /// <summary>
            /// Recupera i cicli CPU di esecuzione di un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <param name="CycleTime">Numero di cicli CPU di esecuzione.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "QueryThreadCycleTime", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryThreadCycleTime(IntPtr ThreadHandle, out ulong CycleTime);

            /// <summary>
            /// Recupera il valore di priorità per un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <returns>Un valore che indica la priorità del thread.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetThreadPriority", SetLastError = true)]
            public static extern int GetThreadPriority(IntPtr ThreadHandle);

            /// <summary>
            /// Recupera il numero del processore ideale per un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <param name="IdealProcessor">Struttura <see cref="Win32Structures.PROCESSOR_NUMBER"/> che indica il processore.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetThreadIdealProcessorEx", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetThreadIdealProcessorEx(IntPtr ThreadHandle, out Win32Structures.PROCESSOR_NUMBER IdealProcessor);
            #endregion
            #region Detailed Properties (Modules)
            /// <summary>
            /// Recupera il percorso completo di un modulo caricato da un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="ModuleHandle">Handle nativo al modulo.</param>
            /// <param name="ModulePath">Percorso completo del modulo.</param>
            /// <param name="BufferSize">Dimensione del buffer, in caratteri.</param>
            /// <returns>Il numero di caratteri copiati nel buffer, 0 in caso di errore.</returns>
            [DllImport("Psapi.dll", CharSet = CharSet.Unicode, EntryPoint = "GetModuleFileNameExW", SetLastError = true)]
            public static extern uint GetModuleFileName(IntPtr ProcessHandle, IntPtr ModuleHandle, StringBuilder ModulePath, uint BufferSize);
            #endregion
            #endregion
            #region Process Manipulation Functions
            /// <summary>
            /// Cambia la classe di priorità di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="PriorityClass">Priorità del processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "SetPriorityClass", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetPriorityClass(IntPtr ProcessHandle, uint PriorityClass);

            /// <summary>
            /// Imposta il valore di affinità di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="ProcessAffinityMask">Nuovo valore di affinità del processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "SetProcessAffinityMask", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetProcessAffinityMask(IntPtr ProcessHandle, IntPtr ProcessAffinityMask);

            /// <summary>
            /// Determina se l'handle nativo fornito è relativo a un processo di sistema.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="IsCritical">Indica se il processo è un processo di sistema.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "IsProcessCritical", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsProcessCritical(IntPtr ProcessHandle, [MarshalAs(UnmanagedType.Bool)] out bool IsCritical);

            /// <summary>
            /// Termina un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="ExitCode">Codice di uscita del processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "TerminateProcess", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool TerminateProcess(IntPtr ProcessHandle, uint ExitCode);
            #endregion
            #region Process Debug Functions

            /// <summary>
            /// Determina se un processo è in corso di debug.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="DebuggerPresent">Indica se il processo è in corso di debug.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "CheckRemoteDebuggerPresent", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CheckRemoteDebuggerPresent(IntPtr ProcessHandle, [MarshalAs(UnmanagedType.Bool)] out bool DebuggerPresent);

            /// <summary>
            /// Determina se il processo chiamante è in corso di debug.
            /// </summary>
            /// <returns>true se il processo è in corso di debug, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "IsDebuggerPresent", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsDebuggerPresent();

            /// <summary>
            /// Causa un eccezione breakpoint in un processo permettendo al thread di segnalare il debugger di gestire l'eccezione.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "DebugBreakProcess", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DebugBreakProcess(IntPtr ProcessHandle);

            /// <summary>
            /// Interrompe il debugging di un processo.
            /// </summary>
            /// <param name="PID">ID del processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "DebugActiveProcessStop", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DebugActiveProcessStop(uint PID);
            #endregion
            #region Other Thread Functions
            /// <summary>
            /// Recupera l'ID di un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <returns>L'ID del thread, 0 se l'operazione è fallita.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetThreadId", SetLastError = true)]
            public static extern uint GetThreadID(IntPtr ThreadHandle);

            /// <summary>
            /// Recupera l'ID del processo associato a un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <returns>L'ID del processo associato, 0 se l'operazione è fallita.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessIdOfThread", SetLastError = true)]
            public static extern uint GetProcessIdOfThread(IntPtr ThreadHandle);

            /// <summary>
            /// Recupera informzioni sulla finestra attiva di un thread GUI specificato.
            /// </summary>
            /// <param name="ThreadID">ID del thread.</param>
            /// <param name="ThreadInfo">Struttura <see cref="Win32Structures.GUITHREADINFO"/> che riceve le informazioni.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "GetGUIThreadInfo", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetGuiThreadInfo(uint ThreadID, out Win32Structures.GUITHREADINFO ThreadInfo);

            /// <summary>
            /// Termina un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <param name="ExitCode">Codice di uscita per il thread.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "TerminateThread", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool TerminateThread(IntPtr ThreadHandle, uint ExitCode);

            /// <summary>
            /// Sospende un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <returns>Il conteggio di sospensione precedente del thread, <see cref="uint.MaxValue"/> in caso di errore.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "SuspendThread", SetLastError = true)]
            public static extern uint SuspendThread(IntPtr ThreadHandle);

            /// <summary>
            /// Sospende un thread WOW64.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <returns>Il conteggio di sospensione precedente del thread, <see cref="uint.MaxValue"/> in caso di errore.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "Wow64SuspendThread", SetLastError = true)]
            public static extern uint Wow64SuspendThread(IntPtr ThreadHandle);

            /// <summary>
            /// Riduce il conteggio di sospensione di un thread, quando il conteggio raggiunge 0, l'esecuzione del thread riprende.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <returns>Il conteggio di sospensione del thread prima dell'esecuzione della funzione, <see cref="uint.MaxValue"/> in caso di errore.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "ResumeThread", SetLastError = true)]
            public static extern uint ResumeThread(IntPtr ThreadHandle);

            /// <summary>
            /// Imposta la priorità di un thread.
            /// </summary>
            /// <param name="ThreadHandle">Handle nativo al thread.</param>
            /// <param name="Priority">Nuova priorità del thread.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "SetThreadPriority", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetThreadPriority(IntPtr ThreadHandle, Win32Enumerations.ThreadPriority Priority);

            /// <summary>
            /// Recupera l'identificatore del thread chiamante.
            /// </summary>
            /// <returns>L'ID del thread chiamante.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetCurrentThreadId", SetLastError = true)]
            public static extern uint GetCurrentThreadID();
            #endregion
            #region Process Windows Functions
            #region Windows Enumeration
            /// <summary>
            /// Esegue l'enumerazione di tutte le finestre principali associate a un thread.
            /// </summary>
            /// <param name="ThreadID">ID del thread.</param>
            /// <param name="Callback">Delegato che riceve l'handle di ogni finestra.</param>
            /// <param name="ParameterToCallback">Parametro da passare al delegato.</param>
            /// <remarks>Questa funzione chiama il delegato fornito per ogni finestra trovata, l'enumerazione continua se il delegato ritorna true, altrimenti si ferma.</remarks>
            /// <returns>true se l'enumerzione si conclude, false se l'enumerazione viene fermata dal delegato oppure se non ci sono finestre associate al thread.</returns>
            [DllImport("User32.dll", EntryPoint = "EnumThreadWindows", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumThreadWindows(uint ThreadID, [MarshalAs(UnmanagedType.FunctionPtr)] EnumWindowsCallback Callback, IntPtr ParameterToCallback);

            /// <summary>
            /// Esegue l'enumerazione di tutte le finestre figlio associate a una finestra padre.
            /// </summary>
            /// <param name="ParentWindowHandle">Handle nativo alla finestra padre.</param>
            /// <param name="Callback">Delegato che riceve l'handle di ogni finestra.</param>
            /// <param name="ParameterToCallback">Parametro da passare al delegato.</param>
            /// <remarks>Questa funzione chiama il delegato fornito per ogni finestra trovata, l'enumerazione continua se il delegato ritorna true, altrimenti si ferma.</remarks>
            /// <returns>Il valore restituito non è usato.</returns>
            [DllImport("User32.dll", EntryPoint = "EnumChildWindows", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumChildWindows(IntPtr ParentWindowHandle, [MarshalAs(UnmanagedType.FunctionPtr)] EnumWindowsCallback Callback, IntPtr ParameterToCallback);
            #endregion
            #region Windows Info Functions

            /// <summary>
            /// Recupera il nome della classe a cui una finestra appartiene.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="ClassName">Buffer che contiene il nome della classe.</param>
            /// <param name="ClassNameLength">Dimensione, in caratteri, del parametro <paramref name="ClassName"/>, il buffer deve essere abbastanza grande da contenere anche il carattere nullo finale.</param>
            /// <returns>Il numero di caratteri copiati nel buffer escluso il carattere nullo finale, 0 in caso di errore.</returns>
            [DllImport("User32.dll", EntryPoint = "GetClassNameW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern int GetClassName(IntPtr WindowHandle, StringBuilder ClassName, int ClassNameLength);

            /// <summary>
            /// Recupera la dimensione, in caratteri, del testo della barra del titolo di una finestra o del testo in un controllo.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra o al controllo.</param>
            /// <remarks>Quando questa funzione fallisce in seguito ad un errore il valore restituito è 0 e l'ultimo codice di errore Win32 è diverso da 0.<br/>
            /// Questa funzione non resetta il valore dell'ultimo codice di errore Win32.</remarks>
            /// <returns>La lunghezza, in caratteri, del testo, 0 in caso di fallimento o se non esiste testo.</returns>
            [DllImport("User32.dll", EntryPoint = "GetWindowTextLengthW", SetLastError = true)]
            public static extern int GetWindowTextLength(IntPtr WindowHandle);

            /// <summary>
            /// Recupera il testo della barra del titolo di una finestra oppure il testo di un controllo.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="Text">Buffer che contiene il testo della barra del titolo della finestra.</param>
            /// <param name="TextLength">Numero massimo di caratteri da copiare nel buffer incluso il carattere nullo finale, se il testo recuperato è troppo lungo viene troncato.</param>
            /// <remarks>Questa funzione non può recuperare il testo di un controllo in un altra applicazione.</remarks>
            /// <returns>La lunghezza, in caratteri, della stringa, escluso il carattere nullo finale, se la finestra non ha una barra del titolo, se ne ha una ma è vuota oppure se l'handle fornito non è valido restituisce 0.</returns>
            [DllImport("User32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetWindowText(IntPtr WindowHandle, StringBuilder Text, int TextLength);

            /// <summary>
            /// Recupera informazioni relative a una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="Info">Struttura <see cref="Win32Structures.WINDOWINFO"/> che contiene le informazioni, il membro <see cref="Win32Structures.WINDOWINFO.Size"/> deve essere impostato prima di chiamare questa funzione.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "GetWindowInfo", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowInfo(IntPtr WindowHandle, ref Win32Structures.WINDOWINFO Info);

            /// <summary>
            /// Recupera informazioni relative a una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="InfoIndex">Informazione da recuperare.</param>
            /// <returns>L'informazione richiesta in caso di successo, 0 altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
            public static extern IntPtr GetWindowLongPtr(IntPtr WindowHandle, int InfoIndex);

            /// <summary>
            /// Recupera informazioni su una classe di finestre.
            /// </summary>
            /// <param name="ApplicationInstance">Istanza dell'applicazione che ha creato la classe, <see cref="IntPtr.Zero"/> per recuperare informazioni sulle classi definite dal sistema.</param>
            /// <param name="ClassName">Nome della classe o valore dell'Atom.</param>
            /// <param name="ClassInfo">Struttura <see cref="Win32Structures.WNDCLASSEXW"/> che contiene le informazioni.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /*[DllImport("User32.dll", EntryPoint = "GetClassInfoExW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetClassInfoEx(IntPtr ApplicationInstance, string ClassName, ref Win32Structures.WNDCLASSEXW ClassInfo);*/

            /// <summary>
            /// Recupera informazioni sulla classe di una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="InfoIndex">Informazione da recuperare.</param>
            /// <returns>L'informazioni richiesta in caso di successo, 0 altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "GetClassLongPtrW", SetLastError = true)]
            public static extern IntPtr GetClassLongPtr(IntPtr WindowHandle, int InfoIndex);


            /// <summary>
            /// Determina se l'handle fornito corrisponde a una finestra Unicode nativa.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>true se l'handle corrisponde a una finestra Unicode, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "IsWindowUnicode", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindowUnicode(IntPtr WindowHandle);

            /// <summary>
            /// Recupera l'handle nativo al menù assegnato a un finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>L'handle nativo al menù, <see cref="IntPtr.Zero"/> se la finestra non ha un menù assegnato, se la finestra è una finestra figlia il valore di ritorno non è definito.</returns>
            [DllImport("User32.dll", EntryPoint = "GetMenu", SetLastError = true)]
            public static extern IntPtr GetMenu(IntPtr WindowHandle);

            /// <summary>
            /// Recupera l'identificatore di un controllo.
            /// </summary>
            /// <param name="ControlHandle">Handle nativo al controllo.</param>
            /// <returns>L'identificatore del controllo, 0 in caso di fallimento.</returns>
            /// <remarks>Questa funzione accetta sia un handle a una finestra figlia sia un handle a un controllo in un box di dialogo.<br/>
            /// Se l'handle è relativo a una finestra top-level il valore restituito non è mai valido.</remarks>
            [DllImport("User32.dll", EntryPoint = "GetDlgCtrlID", SetLastError = true)]
            public static extern int GetDlgCtrlID(IntPtr ControlHandle);

            /// <summary>
            /// Esegue l'enumerazione delle proprietà di una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="Callback">Delegato per l'elaborazione dei dati di una proprietà.</param>
            /// <param name="Parameter">Parametro fornito dall'applicazione.</param>
            /// <returns>L'ultimo valore restituto dal delegato, -1 se non sono state trovate proprietà.</returns>
            [DllImport("User32.dll", EntryPoint = "EnumPropsExW", SetLastError = true)]
            public static extern int EnumPropsEx(IntPtr WindowHandle, [MarshalAs(UnmanagedType.FunctionPtr)] EnumWindowPropsCallback Callback, IntPtr Parameter);

            /// <summary>
            /// Recupera il percorso completo del modulo associato a un handle relativo a una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="FullPath">Percorso completo.</param>
            /// <param name="FullPathMaxChars">Numero massimo di caratteri del parametro <paramref name="FullPath"/>.</param>
            /// <returns>Il numero di caratteri del parametro <paramref name="FullPath"/> fornito alla funzione.</returns>
            [DllImport("User32.dll", EntryPoint = "GetWindowModuleFileNameW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern uint GetWindowModuleFileName(IntPtr WindowHandle, StringBuilder FullPath, uint FullPathMaxChars);

            /// <summary>
            /// Recupera l'ID del thread e del processo che ha creato una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="PID">ID del processo.</param>
            /// <returns>ID del thread.</returns>
            [DllImport("User32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true)]
            public static extern uint GetWindowThreadProcessID(IntPtr WindowHandle, out uint PID);

            /// <summary>
            /// Determina se una finestra è attiva.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>true se la finestra è attiva, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "IsWindowEnabled", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindowEnabled(IntPtr WindowHandle);

            /// <summary>
            /// Determina se una finestra è visibile.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>true se la finestra è visibile, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "IsWindowVisible", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindowVisible(IntPtr WindowHandle);

            /// <summary>
            /// Recupera l'opacità e il colore di trasparenza di una finestra a strati.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="TransparencyKey">Colore di trasparenza.</param>
            /// <param name="OpacityLevel">Opacità.</param>
            /// <param name="Flags"></param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "GetLayeredWindowAttributes", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetLayeredWindowAttributes(IntPtr WindowHandle, out uint TransparencyKey, out byte OpacityLevel, out uint Flags);

            /// <summary>
            /// Determina se un'applicazione non risponde.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>true se la finestra non risponde, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "IsHungAppWindow", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsHungAppWindow(IntPtr WindowHandle);

            /// <summary>
            /// Determina se una finestra è ridotta a icona.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>true se la finestra è ridotta a icona, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "IsIconic", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsIconic(IntPtr WindowHandle);

            /// <summary>
            /// Determina se una finestra è ingrandita.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>true se la finestra è ingrandita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "IsZoomed", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsZoomed(IntPtr WindowHandle);
            #endregion
            #region Windows Manipulation Functions

            /// <summary>
            /// Cambia la dimensione, la posizione e l'ordine Z di una finestra figlia, di una finestra popup o di una finestra top-level.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="PrecedingWindowHandle">Handle nativo alla finestra che deve precedere la finestra rappresentata dal parametro <paramref name="WindowHandle"/>.</param>
            /// <param name="X">Nuova posizione del lato sinistro della finestra, in coordinate client.</param>
            /// <param name="Y">Nuova posizione del lato superiore della finestra, in coordinate client.</param>
            /// <param name="Width">Nuova larghezza della finestra, in pixels.</param>
            /// <param name="Height">Nuova altezza della finestra, in pixels.</param>
            /// <param name="Flags">Opzioni di ridimensionamento e posizionamento.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetWindowPos(IntPtr WindowHandle, IntPtr PrecedingWindowHandle, int X, int Y, int Width, int Height, uint Flags);

            /// <summary>
            /// Imposta lo stato di visibilità di una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="ShowState">Stato di visibilita.</param>
            /// <returns>true se la finestra era già visibile, false se la finestra era nascosta.</returns>
            [DllImport("User32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindow(IntPtr WindowHandle, Win32Enumerations.WindowShowState ShowState);

            /// <summary>
            /// Imposta lo stato di visibilità di una finestra senza aspettare il completamento dell'operazione.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="ShowState">Stato di visibilita.</param>
            /// <returns>true se l'operazione è iniziata, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "ShowWindowAsync", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindowAsync(IntPtr WindowHandle, Win32Enumerations.WindowShowState ShowState);

            /// <summary>
            /// Riduce a icona una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "CloseWindow", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseWindow(IntPtr WindowHandle);

            /// <summary>
            /// Ripristina e attiva una finestra ridotta a icona.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "OpenIcon", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenIcon(IntPtr WindowHandle);

            /// <summary>
            /// Abilita o disabilita l'input del mouse e della tastiera per una finestra.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="Enable">Indica se abilitare o disabilitare la finestra.</param>
            /// <returns>Se la finestra era disabilitata true, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "EnableWindow", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnableWindow(IntPtr WindowHandle, [MarshalAs(UnmanagedType.Bool)] bool Enable);

            /// <summary>
            /// Imposta l'opacità e il colore di trasparenza di una finestra a strati.
            /// </summary>
            /// <param name="WindowHandle">Handle nativo alla finestra.</param>
            /// <param name="TransparencyKey">Struttura COLORREF che definisce il colore di trasparenza.</param>
            /// <param name="AlphaValue">Valore alpha che descrive l'opacità.</param>
            /// <param name="Flags">Azione da eseguire.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "SetLayeredWindowAttributes", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetLayeredWindowAttributes(IntPtr WindowHandle, uint TransparencyKey, byte AlphaValue, Win32Enumerations.LayeredWindowAttributesFlags Flags);
            #endregion
            #endregion
            #region Process Start Functions
            /// <summary>
            /// Crea un nuovo processo e il suo thread primario.
            /// </summary>
            /// <param name="ApplicationName">Percorso completo o relativo del modulo da eseguire.</param>
            /// <param name="CommandLine">La linea di comando da eseguire.</param>
            /// <param name="ProcessAttributes">Puntatore a una struttura che determina se l'handle al nuovo processo può essere ereditato dai figli.</param>
            /// <param name="ThreadAttributes">Puntatore a una struttura che determina se l'handle al nuovo thread può essere ereditato dai figli.</param>
            /// <param name="InheritHandles">Indica se gli handle ereditabili del processo chiamante sono ereditati dal nuovo processo.</param>
            /// <param name="CreationFlags">Opzioni per la creazione del processo.</param>
            /// <param name="Environment">Puntatore a un blocco d'ambiente per il nuovo processo.</param>
            /// <param name="CurrentDirectory">Percorso completo della directory corrente per il processo, se è nullo il nuovo processo avrà lo stesso disco e directory corrente del chiamante.</param>
            /// <param name="StartupInfo">Struttura <see cref="Win32Structures.STARTUPINFO"/> con le informazioni di avvio.</param>
            /// <param name="ProcessInformation">Struttura <see cref="Win32Structures.PROCESS_INFORMATION"/> con informazioni di identificazione sul nuovo processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Se il parametro <paramref name="ApplicationName"/> specifica un percorso relativo, il sistema usa il disco e la directory corrente per completare il percorso, l'estensione deve essere inclusa.<br/>
            /// Questo parametro può essere nullo, in questo caso, il nome del modulo deve essere specificato nel primo token (deliminato da una spazio) nel parametro <paramref name="CommandLine"/>, se il nome contiene spazi utilizzare le virgolette per indicare la fine del percorso.<br/>
            /// Se il modulo eseguibile è a 16 bit, il parametro <paramref name="ApplicationName"/> deve essere nullo e il percorso del modulo deve essere specificato in <paramref name="CommandLine"/> insiemi ai suoi parametri.<br/><br/>
            /// La massima lunghezza della stringa indicata da <paramref name="CommandLine"/> è di 32.767 caratteri, se <paramref name="ApplicationName"/> è nullo, la porzione relativa al nome del modulo ha una massima lunghezza di <see cref="Win32Constants.MAX_PATH"/>.<br/>
            /// Il parametro <paramref name="CommandLine"/> può avere valore <see cref="IntPtr.Zero"/>, in questo caso, viene usata la stringa presente nel parametro <paramref name="ApplicationName"/>.<br/>
            /// Se il parametro <paramref name="CommandLine"/> include il percorso del modulo e l'estensione non è indicata, .exe viene aggiunto, se il percorso termina con un punto o se il percorso è completo, l'estensione non viene aggiunta.<br/>
            /// Se il percorso non contiene una directory, il sistema cerca l'eseguibile nei posti seguenti:<br/><br/>
            /// 1) la directory da cui l'applicazione è stata caricata<br/>
            /// 2) la directory corrente del processo padre<br/>
            /// 3) la directory a 32-bit di Windows<br/>
            /// 4) la directory a 16-bit di Windows<br/>
            /// 5) la directory di Windows<br/>
            /// 6) le directory indicate nella variabile d'ambiente PATH.<br/><br/>
            /// Se il sistema supporta Terminal Services, se <paramref name="InheritHandles"/> ha valore true, il processo deve essere creato nella stessa sessione del chiamante, gli handle non sono ereditabili tra sessioni.<br/>
            /// L'ereditarietà degli handle è bloccata se un processo protetto crea un processo non protetto, in quanto il permesso di accesso <see cref="Win32Enumerations.ProcessAccessRights.PROCESS_DUP_HANDLE"/> non è permesso da un processo non protetto a uno protetto.<br/><br/>
            /// Il parametro <paramref name="CreationFlags"/> controlla anche la priorità del nuovo processo oltre alle opzioni di creazione.<br/><br/>
            /// Il blocco d'ambiente del nuovo processo, indicato da <paramref name="Environment"/>, può avere valore <see cref="IntPtr.Zero"/>, in tal caso, il nuovo processo usa l'ambiente del chiamante.<br/><br/>
            /// Questa funzione restituisce prima della fine dell'inizializzazione del nuovo processo.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "CreateProcessW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CreateProcess(string ApplicationName, IntPtr CommandLine, IntPtr ProcessAttributes, IntPtr ThreadAttributes, [MarshalAs(UnmanagedType.Bool)] bool InheritHandles, Win32Enumerations.ProcessCreationOptions CreationFlags, IntPtr Environment, string CurrentDirectory, Win32Structures.STARTUPINFO StartupInfo, out Win32Structures.PROCESS_INFORMATION ProcessInformation);

            /// <summary>
            /// Crea un nuovo processo e il suo thread primario, il nuovo processo esegue l'eseguibile specificato nel contesto di sicurezza delle credenziali specificate.
            /// </summary>
            /// <param name="Username">Nome dell'utente.</param>
            /// <param name="Domain">Nome del dominio o del server il cui database degli account contiene il nome indicato da <paramref name="Username"/>.</param>
            /// <param name="Password">La password dell'account indicato da <paramref name="Username"/>.</param>
            /// <param name="LogonFlags">Opzioni di accesso.</param>
            /// <param name="ApplicationName">Percorso completo o relativo del modulo da eseguire.</param>
            /// <param name="CommandLine">La linea di comando da eseguire.</param>
            /// <param name="CreationFlags">Opzioni per la creazione del processo.</param>
            /// <param name="Environment">Puntatore a un blocco d'ambiente per il nuovo processo.</param>
            /// <param name="CurrentDirectory">Percorso completo della directory corrente per il processo, se è nullo il nuovo processo avrà lo stesso disco e directory corrente del chiamante.</param>
            /// <param name="StartupInfo">Struttura <see cref="Win32Structures.STARTUPINFO"/> con le informazioni di avvio.</param>
            /// <param name="ProcessInformation">Struttura <see cref="Win32Structures.PROCESS_INFORMATION"/> con informazioni di identificazione sul nuovo processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Se il parametro <paramref name="Username"/> usa il formato UPN (user@DNS_domain_name), <paramref name="Domain"/> deve essere nullo.<br/>
            /// L'account utente deve avere il permesso per l'accesso locale.<br/><br/>
            /// Se <paramref name="Domain"/> è nullo, <paramref name="Username"/> deve essere espresso in formato UPN.<br/><br/>
            /// Se il parametro <paramref name="ApplicationName"/> specifica un percorso relativo, il sistema usa il disco e la directory corrente per completare il percorso, l'estensione deve essere inclusa.<br/>
            /// Questo parametro può essere nullo, in questo caso, il nome del modulo deve essere specificato nel primo token (deliminato da una spazio) nel parametro <paramref name="CommandLine"/>, se il nome contiene spazi utilizzare le virgolette per indicare la fine del percorso.<br/>
            /// Se il modulo eseguibile è a 16 bit, il parametro <paramref name="ApplicationName"/> deve essere nullo e il percorso del modulo deve essere specificato in <paramref name="CommandLine"/> insiemi ai suoi parametri.<br/><br/>
            /// La massima lunghezza della stringa indicata da <paramref name="CommandLine"/> è di 32.767 caratteri, se <paramref name="ApplicationName"/> è nullo, la porzione relativa al nome del modulo ha una massima lunghezza di <see cref="Win32Constants.MAX_PATH"/>.<br/>
            /// Il parametro <paramref name="CommandLine"/> può avere valore <see cref="IntPtr.Zero"/>, in questo caso, viene usata la stringa presente nel parametro <paramref name="ApplicationName"/>.<br/>
            /// Se il parametro <paramref name="CommandLine"/> include il percorso del modulo e l'estensione non è indicata, .exe viene aggiunto, se il percorso termina con un punto o se il percorso è completo, l'estensione non viene aggiunta.<br/>
            /// Se il percorso non contiene una directory, il sistema cerca l'eseguibile nei posti seguenti:<br/><br/>
            /// 1) la directory da cui l'applicazione è stata caricata<br/>
            /// 2) la directory corrente del processo padre<br/>
            /// 3) la directory a 32-bit di Windows<br/>
            /// 4) la directory a 16-bit di Windows<br/>
            /// 5) la directory di Windows<br/>
            /// 6) le directory indicate nella variabile d'ambiente PATH.<br/><br/>
            /// Il parametro <paramref name="CreationFlags"/> controlla anche la priorità del nuovo processo oltre alle opzioni di creazione.<br/><br/>
            /// Il blocco d'ambiente del nuovo processo, indicato da <paramref name="Environment"/>, può avere valore <see cref="IntPtr.Zero"/>, in tal caso, il nuovo processo usa l'ambiente del chiamante.<br/><br/>
            /// Questa funzione restituisce prima della fine dell'inizializzazione del nuovo processo.</remarks>
            [DllImport("Advapi32.dll", EntryPoint = "CreateProcessWithLogonW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CreateProcessWithLogon(string Username, string Domain, IntPtr Password, Win32Enumerations.LogonOptions LogonFlags, string ApplicationName, IntPtr CommandLine, Win32Enumerations.ProcessCreationOptions CreationFlags, IntPtr Environment, string CurrentDirectory, Win32Structures.STARTUPINFO StartupInfo, out Win32Structures.PROCESS_INFORMATION ProcessInformation);
            #endregion
        }

        /// <summary>
        /// Altre funzioni.
        /// </summary>
        public static class Win32OtherFunctions
        {
            #region Generic Functions
            /// <summary>
            /// Imposta l'ultimo codice di errore Win32 per il thread chiamante.
            /// </summary>
            /// <param name="ErrorCode">Codice di errore.</param>
            [DllImport("Kernel32.dll", EntryPoint = "SetLastError")]
            public static extern void SetLastError(uint ErrorCode);

            /// <summary>
            /// Converte un insieme di coordinate riferite allo spazio occupato da una finestra in coordinate riferite allo spazio occupato da un'altra finestra.
            /// </summary>
            /// <param name="WindowHandleFrom">Handle nativo alla finestra al cui spazio occupato le coordinate si riferiscono.</param>
            /// <param name="WindowHandleTo">Handle nativo alla finestra al cui spazio occupato le coordinate vanno convertite.</param>
            /// <param name="Points">Puntatore a un array di strutture POINT che contengono l'insieme di coordinate da convertire, questo parametro può anche puntare a una struttura <see cref="Win32Structures.RECT"/>.</param>
            /// <param name="PointsCount">Numero di strutture POINT presenti nell'array puntato da <paramref name="Points"/>, se <paramref name="Points"/> punta a una struttura <see cref="Win32Structures.RECT"/> questo parametro deve avere valore 2.</param>
            /// <returns>I primi 4 byte sono il numero di pixel aggiunti alla coordinata orizzontale di ogni punto d'origine per ottenere la coordinata orizzontale di ogni punto di destinatione.<br/>
            /// Gli ultimi 4 byte sono il numero di pixel aggiunti alla coordinata verticale di ogni punto d'origine per ottenere la coordinata verticale di ogni punto di destinatione.<br/>
            /// Questa funzione ritorna 0 in caso di errore, questo valore può anche essere valido, controllare l'ultimo codice di errore Win32 per determinare il successo o il fallimento dell'operazione in questo caso.</returns>
            [DllImport("User32.dll", EntryPoint = "MapWindowPoints", SetLastError = true)]
            public static extern int MapWindowPoints(IntPtr WindowHandleFrom, IntPtr WindowHandleTo, IntPtr Points, uint PointsCount);

            /// <summary>
            /// Libera un oggetto locale e rende il suo handle nativo non valido.
            /// </summary>
            /// <param name="ObjectHandle">Handle nativo all'oggetto.</param>
            /// <returns><see cref="IntPtr.Zero"/> se la funzione ha successo, altrimenti il risultato è un handle nativo all'oggetto locale.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
            public static extern IntPtr LocalFree(IntPtr ObjectHandle);

            /// <summary>
            /// Recupera informazioni su un oggetto utente.
            /// </summary>
            /// <param name="ObjectHandle">Handle all'oggetto.</param>
            /// <param name="Information">Informazioni da recuperare.</param>
            /// <param name="Buffer">Buffer dove l'informazione verrà archiviata.</param>
            /// <param name="Length">Dimensione di <paramref name="Buffer"/>, in bytes.</param>
            /// <param name="LengthNeeded">Dimensione necessaria del buffer.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "GetUserObjectInformationW")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetUserObjectInformation(IntPtr ObjectHandle, Win32Enumerations.UserObjectInfo Information, IntPtr Buffer, uint Length, out uint LengthNeeded);

            /// <summary>
            /// Enumera tutte le finestre top-level sullo schermo.
            /// </summary>
            /// <param name="Callback">Puntatore al callback.</param>
            /// <param name="Param">Parametro da passare al callback.</param>
            /// <remarks>Questa funzione chiama il delegato fornito per ogni finestra trovata, l'enumerazione continua se il delegato ritorna true, altrimenti si ferma.</remarks>
            /// <returns>true se l'enumerazione si conclude, false se l'enumerazione viene fermata dal delegato oppure se si verifica un errore.</returns>
            [DllImport("User32.dll", EntryPoint = "EnumWindows", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumWindows([MarshalAs(UnmanagedType.FunctionPtr)] EnumWindowsCallback Callback, IntPtr Param);

            /// <summary>
            /// Notifica il chiamante riguardo cambiamenti agli attributi o ai contenuti di una chiave di registro.
            /// </summary>
            /// <param name="KeyHandle">Handle nativo alla chiave.</param>
            /// <param name="WatchSubtree">Indica se la funzione deve notifica di cambiamenti anche alle sottochiavi.</param>
            /// <param name="NotifyFilter">Indica quali cambiamenti devono essere notificati.</param>
            /// <param name="EventHandle">Handle nativo a un evento.</param>
            /// <param name="Asynchronous">Indica se notificare i cambiamenti attraverso l'evento specificato da <paramref name="EventHandle"/>.</param>
            /// <returns><see cref="Win32Constants.ERROR_SUCCESS"/> se l'operazione ha successo, un codice di errore altrimenti.</returns>
            /// <remarks>La funzione notifica di un singolo cambiamento.<br/><br/>
            /// A meno che il parametro <paramref name="NotifyFilter"/> non includa l'opzione <see cref="Win32Enumerations.RegistryNotificationFilters.REG_NOTIFY_THREAD_AGNOSTIC"/> l'evento a cui il parametro <paramref name="EventHandle"/> fa riferimento viene segnalato, questo succede anche se l'handle alla chiave viene chiuso.<br/>
            /// Per risultati accurati, senza l'utilizzo dell'opzione <see cref="Win32Enumerations.RegistryNotificationFilters.REG_NOTIFY_THREAD_AGNOSTIC"/>, chiamare questa funzione in un thread persistente.</remarks>
            [DllImport("Advapi32.dll", EntryPoint = "RegNotifyChangeKeyValue")]
            public static extern uint RegNotifyChangeKeyValue(IntPtr KeyHandle, [MarshalAs(UnmanagedType.Bool)] bool WatchSubtree, Win32Enumerations.RegistryNotificationFilters NotifyFilter, IntPtr EventHandle, [MarshalAs(UnmanagedType.Bool)] bool Asynchronous);

            /// <summary>
            /// Recupera la posizione del cursore, in coordinate dello schermo.
            /// </summary>
            /// <param name="Point">Struttura <see cref="Win32Structures.POINT"/> con le coordinate del punto.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("User32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetCursorPos(out Win32Structures.POINT Point);

            /// <summary>
            /// Recupera l'handle nativo alla finestra che contiene il punto specificato.
            /// </summary>
            /// <param name="Point">Il punto da controllare.</param>
            /// <returns>Handle nativo alla finestra che contiene il punto, se non esiste alcuna finestra nel punto indicato, la funzione restituisce <see cref="IntPtr.Zero"/>.<br/><br/>
            /// Questa funzione non resituisce handle a finestre nascoste o disabilitate.</returns>
            [DllImport("User32.dll", EntryPoint = "WindowFromPoint", SetLastError = true)]
            public static extern IntPtr WindowFromPoint(Win32Structures.POINT Point);

            /// <summary>
            /// Permette a un'applicazione di informare il sistema che è in uso, impedendo, così, ad esso di entrare in sospensione o di spegnere il display mentre l'applicazione è in esecuzione.
            /// </summary>
            /// <param name="Flags">Valore dell'enumerazione <see cref="Win32Enumerations.ExecutionState"/> che indica al sistema se esso o il display oppure entrambi devono rimanere in funzione.</param>
            /// <returns>Un valore dell'enumerazione <see cref="Win32Enumerations.ExecutionState"/> che indica le istruzioni date precedentemente al sistema dal thread.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "SetThreadExecutionState", SetLastError = true)]
            public static extern uint SetThreadExecutionState(Win32Enumerations.ExecutionState Flags);

            /// <summary>
            /// Converte un identificatore di località nel nome della località associata.
            /// </summary>
            /// <param name="Locale">Identificatore.</param>
            /// <param name="Name">Nome della località.</param>
            /// <param name="NameSize">Dimensione, in caratteri, del parametro <paramref name="Name"/>.</param>
            /// <param name="Flags">Indica se restituire il nome neutrale della località.</param>
            /// <returns>Il numero di caratteri, incluso il carattere nullo finale se l'operazione è riuscita, 0 altrimenti.</returns>
            /// <remarks>Se <paramref name="NameSize"/> ha valore 0 e l'operazione è riuscita, la funzione restituisce il numero di caratteri necessari, incluso il carattere nullo finale.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "LCIDToLocaleName", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern int LCIDToLocaleName(uint Locale, StringBuilder Name, int NameSize, uint Flags);
            #endregion
            #region Handle Functions
            /// <summary>
            /// Elimina un handle nativo.
            /// </summary>
            /// <param name="Handle">Handle nativo da eliminare.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr Handle);

            /// <summary>
            /// Duplica un handle nativo a un oggetto.
            /// </summary>
            /// <param name="SourceProcessHandle">Handle nativo al processo proprietario dell'handle da duplicare.</param>
            /// <param name="SourceHandle">Handle nativo all'oggetto da duplicare.</param>
            /// <param name="TargetProcessHandle">Handle nativo al processo che deve ricevere l'handle duplicato.</param>
            /// <param name="TargetHandle">Handle nativo duplicato all'oggetto.</param>
            /// <param name="DesiredAccess">Accesso desiderato all'handle.</param>
            /// <param name="InheritHandle">Indica se l'handle duplicato potrà essere ereditato da nuovi processi creati dal processo che riceverà l'handle.</param>
            /// <param name="Options">Azioni opzionali.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "DuplicateHandle", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DuplicateHandle(IntPtr SourceProcessHandle, IntPtr SourceHandle, IntPtr TargetProcessHandle, out IntPtr TargetHandle, uint DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool InheritHandle, Win32Enumerations.DuplicateHandleOptions Options);

            /// <summary>
            /// Confronta due handle nativi per determinare se si riferiscono allo stesso oggetto kernel.
            /// </summary>
            /// <param name="FirstHandle">Primo handle nativo da controllare.</param>
            /// <param name="SecondHandle">Secondo handle nativo da controllare.</param>
            /// <returns>true se gli handle si riferiscono allo stesso oggetto, false altrimenti.</returns>
            [DllImport("Kernelbase.dll", EntryPoint = "CompareObjectHandles", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CompareObjectHandles(IntPtr FirstHandle, IntPtr SecondHandle);
            #endregion
            #region Process Functions
            /// <summary>
            /// Recupera un pseudo handle nativo al processo corrente.
            /// </summary>
            /// <returns>Pseudo handle nativo relativo al processo corrente.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetCurrentProcess", SetLastError = true)]
            public static extern IntPtr GetCurrentProcess();

            /// <summary>
            /// Determina se un processo è in esecuzione sotto WOW64 oltre a restituire informazioni sulla macchina emulata per il processo e sull'architettura dell'host.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="ProcessMachine">Architettura emulata per il processo.</param>
            /// <param name="NativeMachine">Archittetura nativa dell'host.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "IsWow64Process2", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWow64Process(IntPtr ProcessHandle, out ushort ProcessMachine, out ushort NativeMachine);

            /// <summary>
            /// Recupera il nome completo del package per un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="PackageFullNameLength">In input la dimensione di <paramref name="PackageFullName"/>, in output la dimensione del nome restituito incluso il carattere nullo finale..</param>
            /// <param name="PackageFullName">Nome completo del package.</param>
            /// <returns><see cref="Win32Constants.ERROR_SUCCESS"/> se l'operazione è riuscita, un codice di errore Win32 altrimenti, i possibili codici includono:<br/><br/>
            /// 1) <see cref="Win32Constants.APPMODEL_ERROR_NO_PACKAGE"/><br/>
            /// 2) <see cref="Win32Constants.ERROR_INSUFFICIENT_BUFFER"/></returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetPackageFullName", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern int GetPackageFullName(IntPtr ProcessHandle, ref uint PackageFullNameLength, StringBuilder PackageFullName);
            #endregion
            #region WinEvents Hooking Functions
            /// <summary>
            /// Imposta una funzione di hook per una serie di eventi.
            /// </summary>
            /// <param name="EventMin">Valore minimo della serie di eventi.</param>
            /// <param name="EventMax">Valore massimo della serie di eventi.</param>
            /// <param name="DLLHandle">Handle nativo alla DLL che contiene la funzione hook indicata da <paramref name="Callback"/>.</param>
            /// <param name="Callback">Puntatore alla funzione hook.</param>
            /// <param name="PID">ID del processo da cui ricevere eventi.</param>
            /// <param name="TID">ID del thread da cui ricevere eventi.</param>
            /// <param name="Flags">Opzioni della funzione di hook.</param>
            /// <returns>Handle nativo all'istanza dell'hook eventi, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
            /// <remarks>Se il parametro <paramref name="Flags"/> specifica <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_OUTOFCONTEXT"/> o se la funzione di hook non si trova in una DLL, il parametro <paramref name="DLLHandle"/> ha valore <see cref="IntPtr.Zero"/>.<br/><br/>
            /// Se il parametro <paramref name="PID"/> ha valore 0 la funzione riceve eventi da tutti i processi nel desktop corrente.<br/><br/>
            /// Se il parametro <paramref name="TID"/> ha valore 0 la funzione riceve eventi da tutti i thread esistenti associati al desktop corrente.<br/><br/>
            /// Le combinazioni valide di valori per il parametro <paramref name="Flags"/> sono le seguenti:<br/><br/>
            /// <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_INCONTEXT"/> | <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_SKIPOWNPROCESS"/><br/>
            /// <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_INCONTEXT"/> | <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_SKIPOWNTHREAD"/><br/>
            /// <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_OUTOFCONTEXT"/> | <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_SKIPOWNPROCESS"/><br/>
            /// <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_OUTOFCONTEXT"/> | <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_SKIPOWNTHREAD"/><br/><br/>
            /// Se il parametro <paramref name="PID"/> ha un valore 0 e il parametro <paramref name="TID"/> ha valore diverso da 0, la funzione hook riceve eventi da tutti i thread del processo.<br/>
            /// Se il parametro <paramref name="PID"/> ha valore diverso da 0 e il parametro <paramref name="TID"/> ha valore 0, la funzione hook riceve eventi soltanto dal thread specificato.<br/>
            /// Se i parametri <paramref name="PID"/> e <paramref name="TID"/> hanno entrambi valore 0, la funzione hook riceve eventi da tutti i thread e tutti i processi.</remarks>
            [DllImport("User32.dll", EntryPoint = "SetWinEventHook", SetLastError = true)]
            public static extern IntPtr SetWinEventHook(uint EventMin, uint EventMax, IntPtr DLLHandle, [MarshalAs(UnmanagedType.FunctionPtr)] WindowEventCallback Callback, uint PID, uint TID, Win32Enumerations.EventHookingFlags Flags);

            /// <summary>
            /// Imposta una funzione di hook per una serie di eventi.
            /// </summary>
            /// <param name="EventMin">Valore minimo della serie di eventi.</param>
            /// <param name="EventMax">Valore massimo della serie di eventi.</param>
            /// <param name="DLLHandle">Handle nativo alla DLL che contiene la funzione hook indicata da <paramref name="Callback"/>.</param>
            /// <param name="Callback">Puntatore alla funzione hook.</param>
            /// <param name="PID">ID del processo da cui ricevere eventi.</param>
            /// <param name="TID">ID del thread da cui ricevere eventi.</param>
            /// <param name="Flags">Opzioni della funzione di hook.</param>
            /// <returns>Handle nativo all'istanza dell'hook eventi, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
            /// <remarks>Se il parametro <paramref name="Flags"/> specifica <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_OUTOFCONTEXT"/> o se la funzione di hook non si trova in una DLL, il parametro <paramref name="DLLHandle"/> ha valore <see cref="IntPtr.Zero"/>.<br/><br/>
            /// Se il parametro <paramref name="PID"/> ha valore 0 la funzione riceve eventi da tutti i processi nel desktop corrente.<br/><br/>
            /// Se il parametro <paramref name="TID"/> ha valore 0 la funzione riceve eventi da tutti i thread esistenti associati al desktop corrente.<br/><br/>
            /// Le combinazioni valide di valori per il parametro <paramref name="Flags"/> sono le seguenti:<br/><br/>
            /// <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_INCONTEXT"/> | <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_SKIPOWNPROCESS"/><br/>
            /// <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_INCONTEXT"/> | <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_SKIPOWNTHREAD"/><br/>
            /// <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_OUTOFCONTEXT"/> | <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_SKIPOWNPROCESS"/><br/>
            /// <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_OUTOFCONTEXT"/> | <see cref="Win32Enumerations.EventHookingFlags.WINEVENT_SKIPOWNTHREAD"/><br/><br/>
            /// Se il parametro <paramref name="PID"/> ha un valore diverso da 0 e il parametro <paramref name="TID"/> ha valore 0, la funzione hook riceve eventi da tutti i thread del processo.<br/>
            /// Se il parametro <paramref name="PID"/> ha valore 0 e il parametro <paramref name="TID"/> ha valore diverso da 0, la funzione hook riceve eventi soltanto dal thread specificato.<br/>
            /// Se i parametri <paramref name="PID"/> e <paramref name="TID"/> hanno entrambi valore 0, la funzione hook riceve eventi da tutti i thread e tutti i processi.</remarks>
            [DllImport("User32.dll", EntryPoint = "SetWinEventHook", SetLastError = true)]
            public static extern IntPtr SetWinEventHook(uint EventMin, uint EventMax, IntPtr DLLHandle, [MarshalAs(UnmanagedType.FunctionPtr)] WindowEventCallbackForeground Callback, uint PID, uint TID, Win32Enumerations.EventHookingFlags Flags);

            /// <summary>
            /// Rimuove una funzione di hook eventi.
            /// </summary>
            /// <param name="EventHookHandle">Handle nativo all'istanza dell'hook eventi.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>L'operazione può fallire nei seguenti casi, tra gli altri:<br/><br/>
            /// Il parametro <paramref name="EventHookHandle"/> ha valore <see cref="IntPtr.Zero"/> oppure non è valido<br/>
            /// L'hook eventi specificato da <paramref name="EventHookHandle"/> è già stato rimosso<br/>
            /// La funzione è stata chiamata da un thread diverso da quello che ha installato l'hook.</remarks>
            [DllImport("User32.dll", EntryPoint = "UnhookWinEvent", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWinEvent(IntPtr EventHookHandle);
            #endregion
            #region Job Functions
            /// <summary>
            /// Indica se il processo è in esecuzione in un job.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="JobHandle">Handle nativo al job.</param>
            /// <param name="Result">true se il processo è in esecuzione in un job, false altrimenti.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "IsProcessInJob", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsProcessInJob(IntPtr ProcessHandle, IntPtr JobHandle, out bool Result);

            /// <summary>
            /// Crea o apre un job.
            /// </summary>
            /// <param name="JobAttributes">Descrittore di sicurezza del job.</param>
            /// <param name="Name">Nome del job.</param>
            /// <returns>Handle nativo al job, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
            /// <remarks>Se <paramref name="JobAttributes"/> ha valore <see cref="IntPtr.Zero"/>, il job riceve un descrittore di sicurezza di default.<br/><br/>
            /// Se <paramref name="Name"/> è nullo, il job viene creato senza un nome, il nome può avere una lunghezza massima pari a <see cref="Win32Constants.MAX_PATH"/>.<br/><br/>
            /// Se il parametro <paramref name="Name"/> corrisponde al nome di un evento, semaforo, mutex, timer oppure oggetto di file mapping la funzione restituisce <see cref="Win32Constants.ERROR_INVALID_HANDLE"/>.<br/><br/>
            /// L'handle nativo restituito da questa funzione ha il diritto di accesso <see cref="Win32Enumerations.JobObjectAccessRights.JOB_OBJECT_ALL_ACCESS"/> applicato, se l'oggetto esisteva prima della chiamata alla funzione, essa restituisca un handle nativo al job esistente e restituisce il codice di errore <see cref="Win32Constants.ERROR_ALREADY_EXISTS"/>.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "CreateJobObjectW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr CreateJobObject(IntPtr JobAttributes, string Name);

            /// <summary>
            /// Imposta i limiti di un job.
            /// </summary>
            /// <param name="JobHandle">Handle nativo al job i cui limiti vanno impostati.</param>
            /// <param name="InfoClass">Tipo di limite da impostare.</param>
            /// <param name="Info">Struttura che contiene le informazioni sui nuovi limiti.</param>
            /// <param name="InfoLength">Dimensione della struttura indicata nel parametro <paramref name="Info"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "SetInformationJobObject", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetInformationJobObject(IntPtr JobHandle, Win32Enumerations.JobInformationClass InfoClass, IntPtr Info, uint InfoLength);

            /// <summary>
            /// Recupera i limiti e informazioni sullo stato di un job.
            /// </summary>
            /// <param name="JobHandle">Handle nativo al job.</param>
            /// <param name="InfoClass">Informazione da recuperare.</param>
            /// <param name="Info">Buffer dove archiviare l'informazione.</param>
            /// <param name="InfoLength">Dimensione, in bytes, del buffer.</param>
            /// <param name="ReturnLength">Dimensione, in bytes, dell'informazione.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "QueryInformationJobObject", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryInformationJobObject(IntPtr JobHandle, Win32Enumerations.JobInformationClass InfoClass, IntPtr Info, uint InfoLength, out uint ReturnLength);

            /// <summary>
            /// Assegna un processo a un job esistente.
            /// </summary>
            /// <param name="JobHandle">Handle nativo al job.</param>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Se il processo è già assegnato a un job, il job specificato da <paramref name="JobHandle"/> deve essere vuoto o appartenere alla gerarchia dei job annidati a cui il processo già appartiene e non può avere limiti UI impostati.<br/><br/>
            /// Tutti i processi nel job deve appartenere alla stessa sessione a cui esso appartiene.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "AssignProcessToJobObject", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AssignProcessToJobObject(IntPtr JobHandle, IntPtr ProcessHandle);
            #endregion
            #region SID Functions
            /// <summary>
            /// Converte un SID in una stringa.
            /// </summary>
            /// <param name="SID">Puntatore alla struttura SID da convertire.</param>
            /// <param name="StringSid">Stringa che rappresenta il SID.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "ConvertSidToStringSidW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ConvertSidToStringSid(IntPtr SID, out IntPtr StringSid);

            /// <summary>
            /// Converte una stringa che rappresenta un SID in un SID valido.
            /// </summary>
            /// <param name="StringSid">Stringa che rappresenta il SID.</param>
            /// <param name="Sid">SID risultato della conversione.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "ConvertStringSidToSidW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ConvertStringSidToSid(string StringSid, out IntPtr Sid);

            /// <summary>
            /// Determina se un SID è valido.
            /// </summary>
            /// <param name="SID">Puntatore alla struttura SID.</param>
            /// <returns>true se il SID è valido, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "IsValidSid", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsValidSID(IntPtr SID);

            /// <summary>
            /// Recupera il numero di sottoautorità presenti in un SID.
            /// </summary>
            /// <param name="SID">SID.</param>
            /// <returns>Un puntatore alla variabile (di tipo byte) che contiene il conteggio di sottoautorità.</returns>
            /// <remarks>Se questa funzione non riesce il valore di ritorno non è definito.</remarks>
            [DllImport("Advapi32.dll", EntryPoint = "GetSidSubAuthorityCount", SetLastError = true)]
            public static extern IntPtr GetSIDSubAuthorityCount(IntPtr SID);

            /// <summary>
            /// Recupera la sottoautorità di un SID.
            /// </summary>
            /// <param name="SID">SID.</param>
            /// <param name="SubAuthorityIndex">Indice dell'array di subautorità da recuperare.</param>
            /// <returns>Un puntatore alla variabile (di tipo uint) che contiene la sottoautorità.</returns>
            /// <remarks>Se questa funzione non riesce il valore di ritorno non è definito.</remarks>
            [DllImport("Advapi32.dll", EntryPoint = "GetSidSubAuthority", SetLastError = true)]
            public static extern IntPtr GetSIDSubAuthority(IntPtr SID, uint SubAuthorityIndex);

            /// <summary>
            /// Crea un SID per un alias predefinito.
            /// </summary>
            /// <param name="SidType">Alias da rappresentare.</param>
            /// <param name="DomainSID">SID del dominio da usare, <see cref="IntPtr.Zero"/> per il computer locale.</param>
            /// <param name="NewSID">Nuovo SID creato.</param>
            /// <param name="NewSIDBytes">Bytes utilizzati dal nuovo SID.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "CreateWellKnownSid", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CreateWellKnownSid(Win32Enumerations.WellKnownSid SidType, IntPtr DomainSID, IntPtr NewSID, ref uint NewSIDBytes);
            #endregion
            #region NT API Functions
            /// <summary>
            /// Recupera l'informazione richiesta sul sistema.
            /// </summary>
            /// <param name="Information">Informazione richiesta.</param>
            /// <param name="InformationStructure">Struttura che contiene i dati richiesti.</param>
            /// <param name="InformationSize">Dimensione del parametro <paramref name="InformationStructure"/>.</param>
            /// <param name="ReturnLength">Dimensione richiesta del parametro <paramref name="InformationStructure"/> per completare l'operazione.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtQuerySystemInformation")]
            public static extern uint NtQuerySystemInformation(Win32Enumerations.SystemInformationClass Information, IntPtr InformationStructure, uint InformationSize, out uint ReturnLength);

            /// <summary>
            /// Imposta l'informazione indicata nel sistema.
            /// </summary>
            /// <param name="Information">Informazione da impostare.</param>
            /// <param name="Buffer">Dato da impostare.</param>
            /// <param name="BufferLength">Dimensione di <paramref name="Buffer"/>.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtSetSystemInformation")]
            public static extern uint NtSetSystemInformation(Win32Enumerations.SystemInformationClass Information, IntPtr Buffer, uint BufferLength);

            /// <summary>
            /// Recupera l'informazione richiesta sull'oggetto.
            /// </summary>
            /// <param name="Handle">Handle nativo all'oggetto.</param>
            /// <param name="Information">Informazione richiesta.</param>
            /// <param name="InformationStructure">Struttura che riceve le informazioni.</param>
            /// <param name="InformationSize">Dimensione del parametro <paramref name="InformationStructure"/>.</param>
            /// <param name="ReturnLength">Dimensione richiesta del parametro <paramref name="InformationStructure"/> per completare l'operazione.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtQueryObject")]
            public static extern uint NtQueryObject(IntPtr Handle, Win32Enumerations.ObjectInformationClass Information, IntPtr InformationStructure, uint InformationSize, out uint ReturnLength);

            /// <summary>
            /// Duplica un handle a un oggetto.
            /// </summary>
            /// <param name="SourceProcessHandle">Handle nativo al processo a cui appartiene l'oggetto.</param>
            /// <param name="SourceHandle">Handle nativo all'oggetto nel contesto del processo indicato da <paramref name="SourceProcessHandle"/>.</param>
            /// <param name="TargetProcessHandle">Handle nativo al processo che deve ricevere l'handle nativo.</param>
            /// <param name="TargetHandle">Handle nativo all'oggetto nel contesto del processo indicato dal parametro <paramref name="TargetProcessHandle"/>.</param>
            /// <param name="DesiredAccess">Tipo di accesso all'handle.</param>
            /// <param name="Attributes">Attributi dell'handle.</param>
            /// <param name="Options">Opzioni.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtDuplicateObject")]
            public static extern uint NtDuplicateObject(IntPtr SourceProcessHandle, IntPtr SourceHandle, IntPtr TargetProcessHandle, out IntPtr TargetHandle, uint DesiredAccess, uint Attributes, uint Options);

            /// <summary>
            /// Converte un codice NTSTATUS in un codice di errore Win32.
            /// </summary>
            /// <param name="Status">Codice NTSTATUS.</param>
            /// <returns>Il codice di errore Win32 corrispondente.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "RtlNtStatusToDosError")]
            public static extern uint RtlNtStatusToDosError(uint Status);
            #region Handle Query Functions
            /// <summary>
            /// Recupera informazioni su un timer.
            /// </summary>
            /// <param name="TimerHandle">Handle nativo al timer.</param>
            /// <param name="InformationClass">Informazione da recuperare.</param>
            /// <param name="Info">Buffer dove archiviare le informazioni.</param>
            /// <param name="InfoLength">Dimensione di <paramref name="Info"/>, in bytes.</param>
            /// <param name="ReturnLength">Dimensione necessaria di <paramref name="Info"/>.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtQueryTimer", SetLastError = true)]
            public static extern uint NtQueryTimer(IntPtr TimerHandle, Win32Enumerations.TimerInformationClass InformationClass, IntPtr Info, uint InfoLength, out uint ReturnLength);

            /// <summary>
            /// Recupera informazioni su un semaforo.
            /// </summary>
            /// <param name="SemaphoreHandle">Handle nativo al semaforo.</param>
            /// <param name="InfoClass">Informazione da recuperare.</param>
            /// <param name="Info">Buffer dove archiviare le informazioni.</param>
            /// <param name="InfoLength">Dimensioni di <paramref name="Info"/>, in bytes.</param>
            /// <param name="ReturnLength">Dimensione necessaria di <paramref name="Info"/>.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtQuerySemaphore", SetLastError = true)]
            public static extern uint NtQuerySemaphore(IntPtr SemaphoreHandle, Win32Enumerations.SemaphoreInformationClass InfoClass, IntPtr Info, uint InfoLength, out uint ReturnLength);

            /// <summary>
            /// Recupera informazioni su una sezione.
            /// </summary>
            /// <param name="SectionHandle">Handle nativo alla sezione.</param>
            /// <param name="InfoClass">Informazione da recuperare.</param>
            /// <param name="Info">Buffer dove archiviare le informazioni.</param>
            /// <param name="InfoSize">Dimensione di <paramref name="Info"/>.</param>
            /// <param name="ResultLength">Dimensione necessaria di <paramref name="Info"/>.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtQuerySection", SetLastError = true)]
            public static extern uint NtQuerySection(IntPtr SectionHandle, Win32Enumerations.SectionInformationClass InfoClass, IntPtr Info, uint InfoSize, out uint ResultLength);

            /// <summary>
            /// Recupera informazioni su un mutante.
            /// </summary>
            /// <param name="MutantHandle">Handle nativo al mutante.</param>
            /// <param name="InfoClass">Informazione da recuperare.</param>
            /// <param name="Info">Buffer dove archiviare le informazioni.</param>
            /// <param name="InfoSize">Dimensione di <paramref name="Info"/>.</param>
            /// <param name="ResultLength">Dimensione necessaria di <paramref name="Info"/>.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtQueryMutant", SetLastError = true)]
            public static extern uint NtQueryMutant(IntPtr MutantHandle, Win32Enumerations.MutantInformationClass InfoClass, IntPtr Info, uint InfoSize, out uint ResultLength);

            /// <summary>
            /// Recupera informazioni su un evento.
            /// </summary>
            /// <param name="EventHandle">Handle nativo all'evento.</param>
            /// <param name="InfoClass">Informazione da recuperare.</param>
            /// <param name="Info">Buffer dove archiviare le informazioni.</param>
            /// <param name="InfoSize">Dimensione di <paramref name="Info"/>.</param>
            /// <param name="ResultLength">Dimensione necessaria di <paramref name="Info"/>.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtQueryEvent", SetLastError = true)]
            public static extern uint NtQueryEvent(IntPtr EventHandle, Win32Enumerations.EventInformationClass InfoClass, IntPtr Info, uint InfoSize, out uint ResultLength);

            /// <summary>
            /// Recupera informazioni su una chiave di registro.
            /// </summary>
            /// <param name="KeyHandle">Handle nativo alla chiave.</param>
            /// <param name="InfoClass">Informazione da recuperare.</param>
            /// <param name="Info">Buffer dove archiviare le informazioni.</param>
            /// <param name="InfoSize">Dimensione di <paramref name="Info"/>.</param>
            /// <param name="ResultLength">Dimensione necessaria di <paramref name="Info"/>.</param>
            /// <returns>Un valore che indica il risultato dell'operazione.</returns>
            [DllImport("Ntdll.dll", EntryPoint = "NtQueryKey", SetLastError = true)]
            public static extern uint NtQueryKey(IntPtr KeyHandle, Win32Enumerations.KeyinformationClass InfoClass, IntPtr Info, uint InfoSize, out uint ResultLength);
            #endregion
            #endregion
            #region Synchronization Functions
            /// <summary>
            /// Crea o apre un evento con o senza nome e restituisce un handle nativo all'oggetto.
            /// </summary>
            /// <param name="EventAttributes">Descrittore di sicurezza dell'evento.</param>
            /// <param name="Name">Nome dell'oggetto.</param>
            /// <param name="Flags">Opzioni di creazione.</param>
            /// <param name="DesiredAccess">Diritti di accesso richiesti.</param>
            /// <returns>Handle nativo all'oggetto, <see cref="IntPtr.Zero"/> in caso di errore, se l'oggetto era già esistente prima della chiamata alla funzione, essa restituisce un handle nativo all'oggetto e il codice di errore viene impostato a <see cref="Win32Constants.ERROR_ALREADY_EXISTS"/>.</returns>
            /// <remarks>Se <paramref name="EventAttributes"/> è nullo viene assegnato all'evento un descrittore di sicurezza di default.<br/>
            /// Se il nome dell'evento corrisponde a un oggetto già presente nello stesso spazio dei nomi la funzione termina con un errore, il codice di errore viene impostato a <see cref="Win32Constants.ERROR_INVALID_HANDLE"/>.<br/>
            /// Se <paramref name="Name"/> è nullo, l'oggetto viene creato senza un nome, la lunghezza del nome non deve superare <see cref="Win32Constants.MAX_PATH"/>.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "CreateEventExW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr CreateEvent(IntPtr EventAttributes, string Name, Win32Enumerations.EventFlags Flags, uint DesiredAccess);

            /// <summary>
            /// Imposta un evento come segnalato.
            /// </summary>
            /// <param name="EventHandle">Handle nativo all'evento.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "SetEvent", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetEvent(IntPtr EventHandle);

            /// <summary>
            /// Resta in attesa fino a quando un oggetto non è in stato segnalato oppure il tempo specificato è passato.
            /// </summary>
            /// <param name="Handle">Handle all'oggetto.</param>
            /// <param name="Milliseconds">Tempo di timeout.</param>
            /// <param name="Alertable">Indica se un APC o una routine di completamento può causare il ritorno della funzione.</param>
            /// <returns>Uno dei valori dell'enumerazione <see cref="Win32Enumerations.WaitResult"/> che indica il motivo del ritorno della funzione.</returns>
            /// <remarks>Se <paramref name="Milliseconds"/> ha valore 0 la funzione ritorna immediatamente, se invece ha valore <see cref="Win32Constants.INFINITE"/> la funzione ritorna solo quando l'oggetto è segnalato, un APC oppure una routine di completamento è stata messa in coda al thread.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "WaitForSingleObjectEx", SetLastError = true)]
            public static extern Win32Enumerations.WaitResult WaitForSingleObject(IntPtr Handle, uint Milliseconds, [MarshalAs(UnmanagedType.Bool)] bool Alertable);

            /// <summary>
            /// Resta in attesa fino a quando un oggetto non è in stato segnalato oppure il tempo specificato è passato.
            /// </summary>
            /// <param name="Handle">Handle all'oggetto.</param>
            /// <param name="Milliseconds">Tempo di timeout.</param>
            /// <returns>Uno dei valori dell'enumerazione <see cref="Win32Enumerations.WaitResult"/> che indica il motivo del ritorno della funzione.</returns>
            /// <remarks>Se <paramref name="Milliseconds"/> ha valore 0 la funzione ritorna immediatamente, se invece ha valore <see cref="Win32Constants.INFINITE"/> la funzione ritorna solo quando l'oggetto è segnalato.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "WaitForSingleObject", SetLastError = true)]
            public static extern Win32Enumerations.WaitResult WaitForSingleObject(IntPtr Handle, uint Milliseconds);

            /// <summary>
            /// Resta in attesa fino a quando uno o tutti gli oggetti non sono in stato segnalato oppure il tempo specificato è passato.
            /// </summary>
            /// <param name="HandleCount">Numero di handle nativi presenti nel parametro <paramref name="Handles"/>.</param>
            /// <param name="Handles">Puntatore a un array di handle nativi.</param>
            /// <param name="WaitAll">Indica se la funzione deve restituire il controllo solo quando tutti gli oggetti sono segnalati.</param>
            /// <param name="Milliseconds">Tempo di attesa.</param>
            /// <returns>La funzione restituisce un valore che indica la causa.</returns>
            /// <remarks>Se la funzione restiuisce a causa del passaggio di un oggetto allo stato segnalato, il valore di ritorno della funzione è compreso tra <see cref="Win32Enumerations.WaitResult.WAIT_OBJECT_0"/> e <see cref="Win32Enumerations.WaitResult.WAIT_OBJECT_0"/> + <paramref name="HandleCount"/> - 1.<br/>
            /// Quando <paramref name="WaitAll"/> ha valore true, se il valore di ritorno è compreso nel raggio significa che tutti gli oggetti sono segnalati, quando <paramref name="WaitAll"/> ha valore false il valore di ritorno indica quale oggetto nell'array è segnalato.<br/><br/>
            /// Se la funzione restiuisce un valore compreso tra <see cref="Win32Enumerations.WaitResult.WAIT_ABANDONED"/> e <see cref="Win32Enumerations.WaitResult.WAIT_ABANDONED"/> + <paramref name="HandleCount"/> - 1 almeno uno degli oggetti è un mutex.<br/>
            /// L'interpretazione del valore di ritorno rispetto al parametro <paramref name="WaitAll"/> è la stessa del caso in cui il valore di ritorno è <see cref="Win32Enumerations.WaitResult.WAIT_OBJECT_0"/>.<br/><br/>
            /// Se la funzione restituisce <see cref="Win32Enumerations.WaitResult.WAIT_TIMEOUT"/>, il tempo è scaduto.<br/><br/>
            /// Se la funzione restiuisce <see cref="Win32Enumerations.WaitResult.WAIT_FAILED"/>, l'operazione è fallita.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "WaitForMultipleObjects", SetLastError = true)]
            public static extern uint WaitForMultipleObjects(uint HandleCount, IntPtr[] Handles, [MarshalAs(UnmanagedType.Bool)] bool WaitAll, uint Milliseconds);

            /// <summary>
            /// Aumenta il conteggio di un semaforo.
            /// </summary>
            /// <param name="SemaphoreHandle">Handle nativo al semaforo.</param>
            /// <param name="ReleaseCount">Indica di quanto aumentare il conteggio del semaforo.</param>
            /// <param name="PreviousCount">Valore precedente del conteggio.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Se <paramref name="ReleaseCount"/> ha un valore tale che il conteggio del semaforo supera il massimo dopo l'esecuzione della funzione, non viene eseguita alcuna operazione e viene restituito false.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "ReleaseSemaphore", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ReleaseSemaphore(IntPtr SemaphoreHandle, int ReleaseCount, out int PreviousCount);

            /// <summary>
            /// Sospende il thread corrente fino a quando non si verifica la condizione specificata.
            /// </summary>
            /// <param name="Milliseconds">Tempo di sospensione, in millisecondi.</param>
            /// <param name="Alertable">Se true la funzione può restituire il controllo prima del timeout.</param>
            /// <returns>0 se il tempo è scaduto, <see cref="Win32Enumerations.WaitResult.WAIT_IO_COMPLETION"/> se è stato chiamato un callback per il completamento di un'operazione I/O.</returns>
            /// <remarks>Se <paramref name="Milliseconds"/> ha valore 0 e <paramref name="Alertable"/> ha valore false il thread dà possibilità a un altro thread pronto per l'esecuzione di effettuare le proprie operazioni.<br/>
            /// Se <paramref name="Milliseconds"/> ha un valore di <see cref="Win32Constants.INFINITE"/> il tempo di timeout non è impostato.<br/><br/>
            /// Se <paramref name="Alertable"/> è false la funzione restituisce il controllo solo quando il tempo è scaduto, se ha valore true al funzione può restituire il controllo se si verifica uno dei seguenti eventi:<br/><br/>
            /// 1) Un funzione I/O estesa (ReadFileEx oppure WriteFileEx) ha terminato l'esecuzione<br/>
            /// 2) Un APC è stata messa in coda al thread per l'esecuzione<br/>
            /// 3) Il tempo è scaduto</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "SleepEx", SetLastError = true)]
            public static extern uint Sleep(uint Milliseconds, [MarshalAs(UnmanagedType.Bool)] bool Alertable);
            #endregion
        }

        /// <summary>
        /// Funzioni per l'interazione con un token di accesso.
        /// </summary>
        public static class Win32TokenFunctions
        {
            #region Generic Token Functions
            /// <summary>
            /// Recupera l'handle nativo al token di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="DesiredAccess">Tipo di accesso desiderato.</param>
            /// <param name="TokenHandle">Handle nativo al token.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "OpenProcessToken", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenProcessToken(IntPtr ProcessHandle, Win32Enumerations.TokenAccessRights DesiredAccess, out IntPtr TokenHandle);
            #endregion
            #region Process Token Query Functions
            /// <summary>
            /// Recupera informazioni da un token di accesso.
            /// </summary>
            /// <param name="TokenHandle">Handle nativo al token.</param>
            /// <param name="RequestedInformation">Informazione richiesta.</param>
            /// <param name="InformationBuffer">Buffer contenente l'informazione richiesta.</param>
            /// <param name="BufferSize">Dimensione, in bytes, del parametro <paramref name="InformationBuffer"/>.</param>
            /// <param name="ReturnLength">Dimensione minima del buffer.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "GetTokenInformation", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetTokenInformation(IntPtr TokenHandle, Win32Enumerations.TokenInformationClass RequestedInformation, IntPtr InformationBuffer, uint BufferSize, out uint ReturnLength);
            #endregion
            #region Process Token Manipulation Functions
            /// <summary>
            /// Modifica i privilegi di un token.
            /// </summary>
            /// <param name="TokenHandle">Handle nativo al token.</param>
            /// <param name="DisableAllPrivileges">Indica se disattivare tutti i privilegi del token.</param>
            /// <param name="NewState">Nuovo stato del token.</param>
            /// <param name="BufferLength">Dimensione, in bytes, del buffer puntato da <paramref name="PreviousState"/>.</param>
            /// <param name="PreviousState">Puntatore a una struttura <see cref="Win32Structures.TOKEN_PRIVILEGES"/> che contiene lo stato precedente dei privilegi modificati dalla funzione..</param>
            /// <param name="ReturnLength">Dimensione minima, in bytes, del buffer puntato da <paramref name="PreviousState"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "AdjustTokenPrivileges", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges, ref Win32Structures.TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, out uint ReturnLength);

            /// <summary>
            /// Modifica i privilegi di un token.
            /// </summary>
            /// <param name="TokenHandle">Handle nativo al token.</param>
            /// <param name="DisableAllPrivileges">Indica se disattivare tutti i privilegi del token.</param>
            /// <param name="NewState">Nuovo stato del token.</param>
            /// <param name="BufferLength">Dimensione, in bytes, del buffer puntato da <paramref name="PreviousState"/>.</param>
            /// <param name="PreviousState">Puntatore a una struttura <see cref="Win32Structures.TOKEN_PRIVILEGES"/> che contiene lo stato precedente dei privilegi modificati dalla funzione..</param>
            /// <param name="ReturnLength">Dimensione minima, in bytes, del buffer puntato da <paramref name="PreviousState"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "AdjustTokenPrivileges", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges, ref Win32Structures.TOKEN_PRIVILEGES2 NewState, uint BufferLength, IntPtr PreviousState, out uint ReturnLength);

            /// <summary>
            /// Imposta informazioni di un token di accesso.
            /// </summary>
            /// <param name="TokenHandle">Handle nativo al token.</param>
            /// <param name="RequestedInformation">Informazione richiesta.</param>
            /// <param name="InformationBuffer">Buffer contenente l'informazione richiesta.</param>
            /// <param name="BufferSize">Dimensione, in bytes, del parametro <paramref name="InformationBuffer"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "SetTokenInformation", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetTokenInformation(IntPtr TokenHandle, Win32Enumerations.TokenInformationClass RequestedInformation, IntPtr InformationBuffer, uint BufferSize);
            #endregion
        }

        /// <summary>
        /// Funzioni per l'interazione con le funzionalità di sicurezza del sistema.
        /// </summary>
        public static class Win32SecurityFunctions
        {
            #region Generic Security Functions
            /// <summary>
            /// Recupera una copia del descrittore di sicurezza di un oggetto kernel.
            /// </summary>
            /// <param name="ObjectHandle">Handle nativo all'oggetto.</param>
            /// <param name="RequestedInformation">Informazione richiesta.</param>
            /// <param name="SecurityDescriptor"></param>
            /// <param name="BufferSize"></param>
            /// <param name="LengthNeeded"></param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "GetKernelObjectSecurity", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetKernelObjectSecurity(IntPtr ObjectHandle, Win32Enumerations.SecurityInformations RequestedInformation, IntPtr SecurityDescriptor, uint BufferSize, out uint LengthNeeded);
            #endregion
            #region Security Descriptor Functions
            /// <summary>
            /// Recupera le informazioni relative al proprietario da un descrittore di sicurezza.
            /// </summary>
            /// <param name="SecurityDescriptor">Puntatore al descrittore di sicurezza.</param>
            /// <param name="SID">Puntatore a una struttura SID che identifica il proprietario.</param>
            /// <param name="OwnerDefaulted">Indica se il proprietario è stato recuperato tramite un meccanismo di default.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Se il descrittore di sicurezza non contiene informazioni sul proprietario, <paramref name="SID"/> ha valore <see cref="IntPtr.Zero"/> e <paramref name="OwnerDefaulted"/> viene ignorato.</remarks>
            [DllImport("Advapi32.dll", EntryPoint = "GetSecurityDescriptorOwner", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetSecurityDescriptorOwner(IntPtr SecurityDescriptor, out IntPtr SID, [MarshalAs(UnmanagedType.Bool)] out bool OwnerDefaulted);

            /// <summary>
            /// Recupera le informazioni relative al gruppo primario da un descrittore di sicurezza.
            /// </summary>
            /// <param name="SecurityDescriptor">Puntatore al descrittore di sicurezza.</param>
            /// <param name="SID">Puntatore a una struttura SID che identifica il gruppo primario.</param>
            /// <param name="OwnerDefaulted">Indica se il gruppo primario è stato recuperato tramite un meccanismo di default.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Se il descrittore di sicurezza non contiene informazioni sul gruppo primario, <paramref name="SID"/> ha valore <see cref="IntPtr.Zero"/> e <paramref name="OwnerDefaulted"/> viene ignorato.</remarks>
            [DllImport("Advapi32.dll", EntryPoint = "GetSecurityDescriptorOwner", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetSecurityDescriptorGroup(IntPtr SecurityDescriptor, out IntPtr SID, [MarshalAs(UnmanagedType.Bool)] out bool OwnerDefaulted);
            #endregion
            #region Privilege Functions
            /// <summary>
            /// Recupera il nome che corrisponde al privilegio rappresentato da una struttura <see cref="Win32Structures.LUID"/>.
            /// </summary>
            /// <param name="SystemName">Nome del sistema, se è nullo la ricerca viene effettuata nel sistema locale.</param>
            /// <param name="LUID">Struttura <see cref="Win32Structures.LUID"/> che rappresenta il privilegio.</param>
            /// <param name="PrivilegeName">Buffer che conterrà una stringa che rappresenta il nome del privilegio.</param>
            /// <param name="NameLength">Lunghezza, in caratteri, di <paramref name="PrivilegeName"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "LookupPrivilegeNameW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool LookupPrivilegeName(string SystemName, ref Win32Structures.LUID LUID, StringBuilder PrivilegeName, ref uint NameLength);

            /// <summary>
            /// Recupera la struttura <see cref="LUID"/> associata al privilegio specificato.
            /// </summary>
            /// <param name="SystemName">Nome del sistema dal quale il nome del privilegio viene recuperato, se è nullo l'operazione viene eseguita nel sistema locale.</param>
            /// <param name="Name">Nome del privilegio.</param>
            /// <param name="Luid">Struttura <see cref="LUID"/> associata al privilegio.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "LookupPrivilegeValueW", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool LookupPrivilegeValue(string SystemName, string Name, out Win32Structures.LUID Luid);

            /// <summary>
            /// Recupera la descrizione di un privilegio.
            /// </summary>
            /// <param name="SystemName">Nome del sistema dal quale recuperare l'informazione, se è nullo l'operazione viene eseguita sul sistema locale.</param>
            /// <param name="PrivilegeName">Nome del privilegio.</param>
            /// <param name="DisplayName">Descrizione del privilegio.</param>
            /// <param name="DisplayNameLength">Lunghezza, in caratteri, del parametro <paramref name="DisplayName"/>.</param>
            /// <param name="LangID">ID della lingua della descrizione.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "LookupPrivilegeDisplayNameW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool LookupPrivilegeDisplayName(string SystemName, string PrivilegeName, StringBuilder DisplayName, ref uint DisplayNameLength, out uint LangID);
            #endregion
        }

        /// <summary>
        /// Funzioni per l'interazione con gli account utente.
        /// </summary>
        public static class Win32UserAccountFunctions
        {
            #region User Accounts Enumeration Functions
            /// <summary>
            /// Recupera i dati degli utenti presenti in un computer in base ai dati forniti.
            /// </summary>
            /// <param name="ServerName">Nome del computer.</param>
            /// <param name="Level">Tipo di informazioni.</param>
            /// <param name="Filter">Tipi di account da includere.</param>
            /// <param name="Buffer">Buffer dove sono archiviate le informazioni.</param>
            /// <param name="PrefMaxLen">Dimensione massima del buffer, in bytes, indicando -1 sarà il sistema a decidere la dimensione.</param>
            /// <param name="EntriesRead">Numero di elementi enumerati.</param>
            /// <param name="TotalEntries">Numero totale di elementi esistenti.</param>
            /// <param name="ResumeHandle">Valore da cui continuare una eventuale ricerca.</param>
            /// <returns>0 se l'operazione ha successo, altrimenti il codice dell'errore.</returns>
            [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "NetUserEnum", SetLastError = true)]
            public static extern uint NetUserEnum(string ServerName, Win32Enumerations.UserInfoDataType Level, uint Filter, out IntPtr Buffer, int PrefMaxLen, out uint EntriesRead, out uint TotalEntries, out uint ResumeHandle);

            /// <summary>
            /// Recupera una lista di gruppi locali a cui l'utente indicato appartiene.
            /// </summary>
            /// <param name="ServerName">Nome del server.</param>
            /// <param name="Username">Nome utente.</param>
            /// <param name="Level">Tipo di informazioni.</param>
            /// <param name="Flags"></param>
            /// <param name="Buffer">Buffer dove sono archiviate le informazioni.</param>
            /// <param name="PrefMaxLen">Dimensiona massima del buffer, in bytes, indicando -1 sarà il sistema a decidere la dimensione.</param>
            /// <param name="EntriesRead">Numero di elementi enumerati.</param>
            /// <param name="TotalEntries">Numero totale di elementi esistenti.</param>
            /// <returns>0 se l'operazione ha successo, altrimenti il codice dell'errore.</returns>
            [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "NetUserGetLocalGroups", SetLastError = true)]
            public static extern uint NetUserGetLocalGroups(string ServerName, string Username, uint Level, uint Flags, out IntPtr Buffer, int PrefMaxLen, out uint EntriesRead, out uint TotalEntries);

            /// <summary>
            /// Libera la memoria occupata dal buffer creato da una funzione di gestione rete.
            /// </summary>
            /// <param name="BufferPointer">Puntatore al buffer.</param>
            /// <returns>0 se l'operazione ha successo, altrimenti un codice di errore di sistema.</returns>
            [DllImport("Netapi32.dll", EntryPoint = "NetApiBufferFree", SetLastError = true)]
            public static extern uint NetApiBufferFree(IntPtr BufferPointer);
            #endregion
            #region User Accounts Information Functions

            /// <summary>
            /// Recupera il nome dell'account locale associato a un SID .
            /// </summary>
            /// <param name="SID">Puntatore alla struttura SID.</param>
            /// <param name="Name">Nome dell'account.</param>
            /// <param name="NameLength">Lunghezza, in caratteri, del parametro <paramref name="Name"/>.</param>
            /// <param name="DomainName">Nome del dominio dove è stato trovato l'account.</param>
            /// <param name="DomainNameLength">Lunghezza, in caratteri, del parametro <paramref name="DomainName"/>.</param>
            /// <param name="Use">Tipo di account.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "LookupAccountSidW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool LookupAccountSid(string SystemName, IntPtr SID, StringBuilder Name, ref uint NameLength, StringBuilder DomainName, ref uint DomainNameLength, out Win32Enumerations.SidNameUse Use);
            #endregion
        }

        /// <summary>
        /// Funzioni per l'interazione con file.
        /// </summary>
        public static class Win32FileFunctions
        {
            #region File/Directory Objects Functions
            /// <summary>
            /// Crea o apre un file o un dispositivo I/O.
            /// </summary>
            /// <param name="FileName">Nome del file o del dispositivo da aprire o creare.</param>
            /// <param name="DesiredAccess">Tipo di accesso desiderato (uno o più dei valori di <see cref="Win32Enumerations.FileDirectoryAccessRights"/> oppure di <see cref="Win32Enumerations.FileGenericAccessRights"/>).</param>
            /// <param name="ShareMode">Modalità di condivisione.</param>
            /// <param name="SecurityAttributes">Puntatore a un descrittore di sicurezza che determina se l'handle può essere ereditato dai processi figlio.</param>
            /// <param name="CreationDisposition">Azione da intraprendere su un file o un dispositivo.</param>
            /// <param name="FlagsAndAttributes">Attribute del file o del dispositivo.</param>
            /// <param name="TemplateFile">Handle al un file modello che fornisce gli attributi e gli attributi estesi per un nuovo file.</param>
            /// <returns>Handle nativo al file, oppure <see cref="Win32Constants.INVALID_HANDLE_VALUE"/> in caso di errore.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateFile(string FileName, uint DesiredAccess, Win32Enumerations.FileShareOptions ShareMode, IntPtr SecurityAttributes, Win32Enumerations.FileCreationDisposition CreationDisposition, Win32Enumerations.FileAttributesSQOSInfoAndFlags FlagsAndAttributes, IntPtr TemplateFile);

            /// <summary>
            /// Recupera il percorso finale per un file.
            /// </summary>
            /// <param name="FileHandle">Handle nativo al file.</param>
            /// <param name="FilePath">Percorso del file.</param>
            /// <param name="FilePathCount">Dimensione di <paramref name="FilePath"/>, in caratteri.</param>
            /// <param name="Flags">Tipo di risultato.</param>
            /// <returns>La lunghezza della stringa ricevuta da <paramref name="FilePath"/> in caso di successo, se <paramref name="FilePath"/> è troppo piccolo, il valore restituito è la dimensione richiesta, in caso di errore per qualunque altro motivo il valore restituito è 0.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint GetFinalPathNameByHandle(IntPtr FileHandle, StringBuilder FilePath, uint FilePathCount, Win32Enumerations.FileResultType Flags);

            /// <summary>
            /// Recupera la dimensione di un file.
            /// </summary>
            /// <param name="FileHandle">Handle nativo al file.</param>
            /// <param name="FileSize">Dimensione del file.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetFileSizeEx", SetLastError = true)]
            public static extern bool GetFileSize(IntPtr FileHandle, out long FileSize);

            /// <summary>
            /// Recupera il tipo di un file.
            /// </summary>
            /// <param name="FileHandle">Handle nativo al file.</param>
            /// <returns>Uno dei valori dell'enumerazione <see cref="Win32Enumerations.FileType2"/>.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetFileType", SetLastError = true)]
            public static extern Win32Enumerations.FileType2 GetFileType(IntPtr FileHandle);

            /// <summary>
            /// Recupera la dimensione effettiva di un file.
            /// </summary>
            /// <param name="FileName">Percorso del file.</param>
            /// <param name="FileSizeHigh">32 bit più significativi della dimensione del file.</param>
            /// <returns>I 32 bit meno significativi della dimensione del file.</returns>
            /// <remarks>Il parametro <paramref name="FileSizeHigh"/> è necessario solo se il file ha una dimensione maggiore di 4 GB.<br/>
            /// In caso di errore questa funzione restituisce <see cref="Win32Constants.INVALID_FILE_SIZE"/>, se la funzione ritorna questo valore e <paramref name="FileSizeHigh"/> è diverso da zero 
            /// è necessario verificare l'ultimo codice di errore Win32 per confermare la riuscita dell'operazione.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "GetCompressedFileSizeW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint GetCompressedFileSize(string FileName, out IntPtr FileSizeHigh);

            /// <summary>
            /// Recupera l'informazione richiesta su un file.
            /// </summary>
            /// <param name="FileHandle">Handle nativo al file.</param>
            /// <param name="FileInformationClass">Tipo di informazione da recuperare.</param>
            /// <param name="FileInformation">Buffer che riceve le informazioni.</param>
            /// <param name="BufferSize">Dimensione, in bytes, di <paramref name="FileInformation"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetFileInformationByHandle(IntPtr FileHandle, Win32Enumerations.FileHandleQueryClass FileInformationClass, IntPtr FileInformation, uint BufferSize);

            /// <summary>
            /// Recupera gli attributi di un file o di una directory.
            /// </summary>
            /// <param name="FileName">Percorso del file o della directory.</param>
            /// <param name="InfoLevelID">Informazione richiesta.</param>
            /// <param name="FileInformation">Buffer che riceve le informazioni.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetFileAttributesExW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetFileAttributes(string FileName, Win32Enumerations.FileAttributesInfoLevel InfoLevelID, IntPtr FileInformation);
            #endregion
            #region File Mapping Objects Functions
            /// <summary>
            /// Crea o apre un oggetto file mapping con o senza nome da un file.
            /// </summary>
            /// <param name="FileHandle">Handle nativo al file.</param>
            /// <param name="FileMappingAttributes">Puntatore a una struttura che indica se l'handle restituito può essere ereditato dai processi figlio.</param>
            /// <param name="PageProtection">Tipo di protezione delle pagine dell'oggetto file mapping.</param>
            /// <param name="MaximumSizeHigh">I 32 bit più significativi della dimensione massima dell'oggetto file mapping.</param>
            /// <param name="MaximumSizeLow">I 32bit meno significativi della dimensione massima dell'oggetto file mapping.</param>
            /// <param name="Name">Nome dell'oggetto file mapping.</param>
            /// <remarks>Se i parametri <paramref name="MaximumSizeHigh"/> e <paramref name="MaximumSizeLow"/> sono entrambi 0, la dimensione dell'oggetto è uguale alla dimensione del file, se il file è grande 0 bytes, viene restituito il codice di errore <see cref="Win32Constants.ERROR_FILE_INVALID"/>.<br/>
            /// Se il parametro <paramref name="Name"/> corrisponde a un oggetto file mapping esistente, questa funzione tenta di accedere a quell'oggetto con la protezione indicata, se  ha valore <see cref="IntPtr.Zero"/>, l'oggetto viene create senza un nome.<br/>
            /// Se <paramref name="Name"/> corrisponde al nome di un evento, semaforo, mutes, waitable timer od oggetto job esistente, viene restituito il codice di errore <see cref="Win32Constants.ERROR_INVALID_HANDLE"/>.</remarks>
            /// <returns>Un handle all'oggetto file mapping creato o esistente (con la sua dimensione attuale non quella indicata) in caso di successo, <see cref="IntPtr.Zero"/> altrimenti.<br/>
            /// Se l'oggetto esiste già la funzione ha successo e il codice di errore impostato sarà <see cref="Win32Constants.ERROR_ALREADY_EXISTS"/>.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "CreateFileMappingW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateFileMapping(IntPtr FileHandle, IntPtr FileMappingAttributes, uint PageProtection, uint MaximumSizeHigh, uint MaximumSizeLow, string Name);

            /// <summary>
            /// Mappa una vista di un'oggetto file mapping nello spazio di indirizzamento del processo chiamante.
            /// </summary>
            /// <param name="FileMappingObject">Handle nativo all'oggetto file mapping.</param>
            /// <param name="DesiredAccess">Tipo di accesso all'oggetto file mapping.</param>
            /// <param name="FileOffsetHigh">I 32 bit più significativi dell'offset dove la vista inizia.</param>
            /// <param name="FileOffsetLow">I 32 bit meno significativi dell'offset dove la vista inizia.</param>
            /// <param name="BytesToMapCount">Il numero di bytes del file mapping da mappare sulla vista.</param>
            /// <remarks><paramref name="DesiredAccess"/> non ha effetto se l'oggetto file mapping è stato creato con l'opzione <see cref="Win32Enumerations.FileMappingAttributes.SEC_IMAGE"/>.<br/>
            /// La combinazione di <paramref name="FileOffsetHigh"/> e <paramref name="FileOffsetLow"/> deve indicare una posizione all'interno del file mapping e deve rispettare la granularità di allocazione della memoria del sistema.<br/>
            /// Se <paramref name="BytesToMapCount"/> ha valore 0, la mappatura si estende dall'offset specificato fino al termine del file mapping.</remarks>
            /// <returns>L'indirizzo di partenza della vista mappata in caso di successo, <see cref="IntPtr.Zero"/> altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "MapViewOfFile", SetLastError = true)]
            public static extern IntPtr MapViewOfFile(IntPtr FileMappingObject, Win32Enumerations.FileMappingAccessRightsAndFlags DesiredAccess, uint FileOffsetHigh, uint FileOffsetLow, IntPtr BytesToMapCount);

            /// <summary>
            /// Rimuove dallo spazio di indirizzamento del processo chiamante una vista mappata di un file.
            /// </summary>
            /// <param name="BaseAddress">Indirizzo di base della vista.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "UnmapViewOfFile", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnmapViewOfFile(IntPtr BaseAddress);
            #endregion
            /// <summary>
            /// Calcola il checksum di un file immagine.
            /// </summary>
            /// <param name="BaseAddress">Indirizzo base del file mappato.</param>
            /// <param name="FileLength">Dimensione del file, in bytes.</param>
            /// <param name="HeaderChecksum">Checksum originale.</param>
            /// <param name="NewChecksum">Nuovo checksum.</param>
            /// <returns>Puntatore a una struttura <see cref="Win32Structures.IMAGE_NT_HEADERS32"/> oppure a una struttura <see cref="Win32Structures.IMAGE_NT_HEADERS64"/>.</returns>
            [DllImport("Imagehlp.dll", EntryPoint = "CheckSumMappedFile", SetLastError = true)]
            public static extern IntPtr ChecksumMappedFile(IntPtr BaseAddress, uint FileLength, out uint HeaderChecksum, out uint NewChecksum);
            #region File Version Functions
            /// <summary>
            /// Recupera la dimensione delle informazioni di versione di un file, se sono disponibili.
            /// </summary>
            /// <param name="Filename">Nome del file.</param>
            /// <param name="Handle">Variabile che la funzione imposta a 0.</param>
            /// <returns>Dimensione, in bytes, delle informazioni di versione del file, 0 in caso di errore.</returns>
            [DllImport("Api-ms-win-core-version-l1-1-0.dll", EntryPoint = "GetFileVersionInfoSizeW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern uint GetFileVersionInfoSize(string Filename, out uint Handle);

            /// <summary>
            /// Recupera le informazioni di versione di un file.
            /// </summary>
            /// <param name="Filename">Nome del file.</param>
            /// <param name="Handle">Parametro ignorato.</param>
            /// <param name="Length">Dimensione, in bytes, del buffer puntato da <paramref name="Data"/>.</param>
            /// <param name="Data">Puntatore a un buffer che riceve le informazioni di versione.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Api-ms-win-core-version-l1-1-0.dll", EntryPoint = "GetFileVersionInfoW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetFileVersionInfo(string Filename, uint Handle, uint Length, IntPtr Data);

            /// <summary>
            /// Recupera l'informazione di versione specificata dalla risorsa specificata.
            /// </summary>
            /// <param name="Block">Risorsa da cui recuperare l'informazione.</param>
            /// <param name="SubBlock">L'informazione da recuperare.</param>
            /// <param name="Buffer">Puntatore all'informazione richiesta.</param>
            /// <param name="Length">Dimensione dell'informazione puntata da <paramref name="Buffer"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti</returns>
            /// <remarks>Se <paramref name="Length"/> ha valore 0, l'informazione richiesta non è disponibile.<br/>
            /// Se il nome specificato o la risorsa fornita non sono validi, l'operazione fallisce.</remarks>
            [DllImport("Api-ms-win-core-version-l1-1-0.dll", EntryPoint = "VerQueryValueW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return:MarshalAs(UnmanagedType.Bool)]
            public static extern bool VerQueryValue(IntPtr Block, string SubBlock, out IntPtr Buffer, out uint Length);
            #endregion
        }

        /// <summary>
        /// Funzioni COM.
        /// </summary>
        public static class Win32COMFunctions
        {
            /// <summary>
            /// Esegue il comando "Proprietà" del menù contestuale di un oggetto della shell.
            /// </summary>
            /// <param name="WindowHandle"></param>
            /// <param name="ObjectType"></param>
            /// <param name="ObjectName"></param>
            /// <param name="PropertyPage"></param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Shell32.dll", EntryPoint = "SHObjectProperties", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SHObjectProperties(IntPtr WindowHandle, Win32Enumerations.PropertiesObjectType ObjectType, string ObjectName, string PropertyPage);
        }

        /// <summary>
        /// Funzioni per la gestione della memoria.
        /// </summary>
        public static class Win32MemoryFunctions
        {
            #region Virtual Memory Control
            /// <summary>
            /// Cambia la protezione di una regione di pagine mappate nello spazio di indirizzamento virtuale di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="BaseAddress">Indirizzo di base della regione la cui protezione deve essere cambiata.</param>
            /// <param name="Size">Dimensione della regione la cui protezione deve essere cambiata, in bytes.</param>
            /// <param name="NewProtect">Nuova protezione della memoria.</param>
            /// <param name="OldProtect">Vecchia protezione della memoria.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>La protezione può essere cambiata solo su regioni mappate.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "VirtualProtectEx", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool VirtualProtect(IntPtr ProcessHandle, IntPtr BaseAddress, IntPtr Size, Win32Enumerations.MemoryProtections NewProtect, out Win32Enumerations.MemoryProtections OldProtect);

            /// <summary>
            /// Rilascia, annulla la mappatura o esegue entrambe le operazioni su una regione di pagine nello spazio di indirizzamento virtuale di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="BaseAddress">Indirizzo di base della regione dal liberare.</param>
            /// <param name="Size">Dimensione della regione da liberare, in bytes.</param>
            /// <param name="FreeType">Tipo di operazione da eseguire.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Se <paramref name="FreeType"/> ha valore <see cref="Win32Enumerations.FreeOperationType.MEM_RELEASE"/>, <paramref name="Size"/> deve essere 0.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "VirtualFreeEx", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool VirtualFree(IntPtr ProcessHandle, IntPtr BaseAddress, IntPtr Size, Win32Enumerations.FreeOperationType FreeType);

            /// <summary>
            /// Recupera informazioni su un insieme di pagine nello spazio di indirizzamento virtuale di un processo (32 bit).
            /// </summary>
            /// <param name="ProcessHandle">Handle al processo.</param>
            /// <param name="Address">Puntatore all'indirizzo di base della regione di pagine da cui recuperare le informazioni, il valore viene arrotondato al limite della prossima pagina.</param>
            /// <param name="Buffer">Struttura <see cref="Win32Structures.MEMORY_BASIC_INFORMATION"/> con le informazioni sulle pagine.</param>
            /// <param name="Length">Dimensione, in bytes, del parametro <paramref name="Buffer"/>.</param>
            /// <returns>Il numero di byte restituiti nel buffer, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
            /// <remarks>Se il parametro <paramref name="Address"/> punta a un indirizzo di memoria oltre il limite massimo di indirizzi di memoria accessibili al processo la funzione restituisce il codice di errore <see cref="Win32Constants.ERROR_INVALID_PARAMETER"/>.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "VirtualQueryEx", SetLastError = true)]
            public static extern IntPtr VirtualQuery32(IntPtr ProcessHandle, IntPtr Address, out Win32Structures.MEMORY_BASIC_INFORMATION Buffer, ulong Length);

            /// <summary>
            /// Recupera informazioni su un insieme di pagine nello spazio di indirizzamento virtuale di un processo (64 bit).
            /// </summary>
            /// <param name="ProcessHandle">Handle al processo.</param>
            /// <param name="Address">Puntatore all'indirizzo di base della regione di pagine da cui recuperare le informazioni, il valore viene arrotondato al limite della prossima pagina.</param>
            /// <param name="Buffer">Struttura <see cref="Win32Structures.MEMORY_BASIC_INFORMATION64"/> con le informazioni sulle pagine.</param>
            /// <param name="Length">Dimensione, in bytes, del parametro <paramref name="Buffer"/>.</param>
            /// <returns>Il numero di byte restituiti nel buffer, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
            /// <remarks>Se il parametro <paramref name="Address"/> punta a un indirizzo di memoria oltre il limite massimo di indirizzi di memoria accessibili al processo la funzione restituisce il codice di errore <see cref="Win32Constants.ERROR_INVALID_PARAMETER"/>.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "VirtualQueryEx", SetLastError = true)]
            public static extern IntPtr VirtualQuery64(IntPtr ProcessHandle, IntPtr Address, out Win32Structures.MEMORY_BASIC_INFORMATION64 Buffer, ulong Length);
            #endregion
            /// <summary>
            /// Legge dati dalla memoria di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle al processo.</param>
            /// <param name="BaseAddress">Puntatore all'indirizzo di base nel processo da cui leggere.</param>
            /// <param name="Buffer">Buffer che riceve i contenuti dallo spazio di indirizzamento del processo.</param>
            /// <param name="Size">Numero di byte da leggere dal processo.</param>
            /// <param name="NumberOfBytesRead">Numero di bytes trasferiti nel buffer, se questo parametro ha valore <see cref="IntPtr.Zero"/> viene ignorato.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Prima di trasferire i dati, il sistema controlla che tutti i dati nella regione della dimensione indicata sono accessibili in lettura, se questo controllo ha esito negativo la funzione restituisce un errore.</remarks>
            [DllImport("Kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ReadProcessMemory(IntPtr ProcessHandle, IntPtr BaseAddress, IntPtr Buffer, IntPtr Size, out IntPtr NumberOfBytesRead);
            #region Working Set Functions
            /// <summary>
            /// Rimuove dal working set di un processo il maggior numero di pagine possibili.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Psapi.dll", EntryPoint = "EmptyWorkingSet", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EmptyWorkingSet(IntPtr ProcessHandle);

            /// <summary>
            /// Imposta la dimensione minina e massima del working set di un processo.
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="MinimumWorkingSetSize">Dimensione minima del working set.</param>
            /// <param name="MaximumWorkingSetSize">Dimensione massima del working set.</param>
            /// <param name="Flags">Opzioni.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "SetProcessWorkingSetSizeEx", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetProcessWorkingSetSize(IntPtr ProcessHandle, IntPtr MinimumWorkingSetSize, IntPtr MaximumWorkingSetSize, Win32Enumerations.ProcessWorkingSetQuotaLimitsOptions Flags);

            /// <summary>
            /// Recupera la dimensione minima e massima del working set di un processo (32 bit).
            /// </summary>
            /// <param name="ProcessHandle">Handle nativo al processo.</param>
            /// <param name="MinimumWorkingSetSize">Dimensione minima del working set.</param>
            /// <param name="MaximumWorkingSetSize">Dimensione massima del working set.</param>
            /// <param name="Flags">Opzioni.</param>
            /// <returns>Nessun valore di ritorno.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetProcessWorkingSetSizeEx", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetProcessWorkingSetSize(IntPtr ProcessHandle, IntPtr MinimumWorkingSetSize, IntPtr MaximumWorkingSetSize, out Win32Enumerations.ProcessWorkingSetQuotaLimitsOptions Flags);
            #endregion
            /// <summary>
            /// Crea un oggetto di notifica stato della memoria.
            /// </summary>
            /// <param name="NotificationType">Tipo di notifica.</param>
            /// <returns>Handle nativo all'oggetto di notifica, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "CreateMemoryResourceNotification", SetLastError = true)]
            public static extern IntPtr CreateMemoryResourceNotification(Win32Enumerations.MemoryResourceNotificationType NotificationType);

            /// <summary>
            /// Recupera lo stato dell'oggetto di notifica stato della memoria.
            /// </summary>
            /// <param name="ResourceNotificationHandle">Handle nativo all'oggetto di notifica.</param>
            /// <param name="ResourceState">Indica se la memoria si trova nello stato precendentemente indicato.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "QueryMemoryResourceNotification", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryMemoryResourceNotification(IntPtr ResourceNotificationHandle, [MarshalAs(UnmanagedType.Bool)] out bool ResourceState);
        }

        /// <summary>
        /// Funzioni per l'interazione con i servizi.
        /// </summary>
        public static class Win32ServiceFunctions
        {
            #region Generic Functions
            /// <summary>
            /// Stabilisce una connessione a Gestore controllo servizi sul computer specificato e apre il database specificato.
            /// </summary>
            /// <param name="MachineName">Nome del computer.</param>
            /// <param name="DatabaseName">Nome del database.</param>
            /// <param name="DesiredAccess">Tipo di accesso richiesto.</param>
            /// <returns>Handle nativo al database di Gestore controllo servizi, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
            /// <remarks>In caso di errore la funzione può impostare i seguenti codici di errore, tra gli altri:<br/><br/>
            /// 1) <see cref="Win32Constants.ERROR_ACCESS_DENIED"/>
            /// 2) <see cref="Win32Constants.ERROR_DATABASE_DOES_NOT_EXIST"/></remarks>
            [DllImport("Advapi32.dll", EntryPoint = "OpenSCManagerW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr OpenServiceControlManager(string MachineName, string DatabaseName, Win32Enumerations.ServiceControlManagerAccessRights DesiredAccess);

            /// <summary>
            /// Apre un servizio.
            /// </summary>
            /// <param name="ServiceControlManagerHandle">Handle al database di Gestione Controllo Servizi.</param>
            /// <param name="ServiceName">Nome del servizio.</param>
            /// <param name="DesiredAccess">Tipo di accesso richiesto.</param>
            /// <returns></returns>
            [DllImport("Advapi32.dll", EntryPoint = "OpenServiceW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr OpenService(IntPtr ServiceControlManagerHandle, string ServiceName, Win32Enumerations.ServiceAccessRights DesiredAccess);

            /// <summary>
            /// Chiude un handle a Gestione Controllo Servizi o a un servizio.
            /// </summary>
            /// <param name="Handle">Handle nativo da chiudere.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "CloseServiceHandle", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseServiceHandle(IntPtr Handle);
            #endregion
            #region Service Info Functions
            /// <summary>
            /// Enumera i servizi nel database specificato.
            /// </summary>
            /// <param name="ServiceControlManagerHandle">Handle nativo al database di Gestore controllo servizi.</param>
            /// <param name="InfoLevel">Livello di informazioni richiesto.</param>
            /// <param name="ServiceType">Tipo di servizio.</param>
            /// <param name="ServiceState">Stato del servizio.</param>
            /// <param name="Services">Buffer che riceve le informazioni.</param>
            /// <param name="BufferSize">Dimensione, in bytes, di <paramref name="Services"/>.</param>
            /// <param name="BytesNeeded">Dimensione necessaria di <paramref name="Services"/>.</param>
            /// <param name="ServicesReturned">Numero di servizi restituiti.</param>
            /// <param name="ResumeHandle">Punto di partenza dell'enumerazione.</param>
            /// <param name="GroupName">Nome del gruppo a cui i servizi appartengono.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>La dimensione massima di <paramref name="Services"/> è di 256 KB.<br/>
            /// <paramref name="ResumeHandle"/> deve avere valore 0 alla prima chiamata, la funzione imposta questo parametro se il buffer è troppo piccolo e ci sono altri dati.<br/>
            /// Se <paramref name="GroupName"/> ha un valore diverso dalla stringa vuota, la funzione enumera tutti i servizi che appartengono al gruppo specificato, se è una stringa vuota, enumera i servizi che non sono parte di un gruppo, se è nullo, enumera tutti i servizi.</remarks>
            [DllImport("Advapi32.dll", EntryPoint = "EnumServicesStatusExW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumServicesStatus(IntPtr ServiceControlManagerHandle, Win32Enumerations.ServiceEnumerationInfoLevel InfoLevel, Win32Enumerations.ServiceType ServiceType, Win32Enumerations.ServiceState2 ServiceState, IntPtr Services, uint BufferSize, out uint BytesNeeded, out uint ServicesReturned, ref uint ResumeHandle, string GroupName);

            /// <summary>
            /// Permette a un'applicazione di ricevere notifiche quando un servizio viene creato, eliminato o quando il suo stato cambia.
            /// </summary>
            /// <param name="ServiceHandle">Handle nativo al servizio o a Gestore Controllo Servizi.</param>
            /// <param name="NotifyMask">Eventi di cui ricevere notifiche.</param>
            /// <param name="NotifyBuffer">Puntatore a una struttura <see cref="Win32Structures.SERVICE_NOTIFY_2"/> che contiene informazioni sulla notifica</param>
            /// <returns><see cref="Win32Constants.ERROR_SUCCESS"/> se l'operazione è riuscita, <see cref="Win32Constants.ERROR_SERVICE_MARKED_FOR_DELETE"/> se il servizio deve essere eliminato, <see cref="Win32Constants.ERROR_SERVICE_NOTIFY_CLIENT_LAGGING"/> se le notifiche non riescono a tenere il passo con lo stato del sistema, uno dei codici di errore Win32 in caso di errore.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "NotifyServiceStatusChangeW", SetLastError = true)]
            public static extern uint NotifyServiceStatusChange(IntPtr ServiceHandle, Win32Enumerations.ServiceNotificationReasons NotifyMask, IntPtr NotifyBuffer);

            /// <summary>
            /// Recupera il nome descrittivo di un servizio.
            /// </summary>
            /// <param name="SCMHandle">Handle nativo a Gestione Controllo Servizi.</param>
            /// <param name="ServiceName">Nome del servizio nel database.</param>
            /// <param name="DisplayName">Parametro che riceve il nome descrittivo del servizio.</param>
            /// <param name="BufferSize">Dimensione di <paramref name="DisplayName"/>, in caratteri.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "GetServiceDisplayNameW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetServiceDisplayName(IntPtr SCMHandle, string ServiceName, StringBuilder DisplayName, ref uint BufferSize);

            /// <summary>
            /// Richiede lo stato di un servizio.
            /// </summary>
            /// <param name="ServiceHandle">Handle nativo al servizio.</param>
            /// <param name="InfoLevel">Livello di informazioni.</param>
            /// <param name="Buffer">Buffer che riceve i dati.</param>
            /// <param name="BufferSize">Dimensione di <paramref name="Buffer"/>, in bytes.</param>
            /// <param name="BytesNeeded">Dimensione necessario di <paramref name="Buffer"/>, in bytes.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "QueryServiceStatusEx", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryServiceStatus(IntPtr ServiceHandle, Win32Enumerations.ServiceEnumerationInfoLevel InfoLevel, IntPtr Buffer, uint BufferSize, out uint BytesNeeded);

            /// <summary>
            /// Recupera la configurazione base di un servizio.
            /// </summary>
            /// <param name="ServiceHandle">Handle nativo al servizio.</param>
            /// <param name="ServiceConfigBuffer">Buffer che riceve le informazioni.</param>
            /// <param name="BufferSize">Dimensione di <paramref name="ServiceConfigBuffer"/>, in bytes.</param>
            /// <param name="BytesNeeded">Dimensione necessaria di <paramref name="ServiceConfigBuffer"/>, in bytes.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "QueryServiceConfigW", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryServiceConfig(IntPtr ServiceHandle, IntPtr ServiceConfigBuffer, uint BufferSize, out uint BytesNeeded);

            /// <summary>
            /// Recupera le informazioni sulla configurazione opzionale di un servizio.
            /// </summary>
            /// <param name="ServiceHandle">Handle nativo al servizio.</param>
            /// <param name="InfoLevel">Informazione richiesta.</param>
            /// <param name="Buffer">Buffer che riceve le informazioni.</param>
            /// <param name="BufferSize">Dimensione, in bytes, di <paramref name="Buffer"/>.</param>
            /// <param name="BytesNeeded">Dimensione necessaria di <paramref name="Buffer"/>, in bytes.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [DllImport("Advapi32.dll", EntryPoint = "QueryServiceConfig2W", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool QueryServiceOptionalConfig(IntPtr ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel InfoLevel, IntPtr Buffer, uint BufferSize, out uint BytesNeeded);
            #endregion
        }

        /// <summary>
        /// Funzioni per lo spegnimento, la sospensione, l'ibernazione e il blocco del computer.
        /// </summary>
        public static class Win32ShutdownFunctions
        {
            /// <summary>
            /// Spegne, riavvia il computer e riavvia tutte le applicazioni che sono state registrate per il riavvio.
            /// </summary>
            /// <param name="MachineName">Nome del computer.</param>
            /// <param name="Message">Messaggio da mostrare nella finestra di dialogo dello spegnimento interattivo.</param>
            /// <param name="GracePeriod">Secondi di attesa prima dello spegnimento.</param>
            /// <param name="ShutdownFlags">Opzioni.</param>
            /// <param name="Reason">Motivazione dello spegnimento.</param>
            /// <returns><see cref="Win32Constants.ERROR_SUCCESS"/> se il sistema ha accettato la richiesta, in caso di errore il codice di errore impostato è uno dei seguenti:<br/><br/>
            /// <see cref="Win32Constants.ERROR_ACCESS_DENIED"/> (Privilegi insufficienti)<br/>
            /// <see cref="Win32Constants.ERROR_BAD_NETPATH"/> (Il computer indicato non esiste o non è accessibile)<br/>
            /// <see cref="Win32Constants.ERROR_INVALID_COMPUTERNAME"/> (Nome del computer non valido)<br/>
            /// <see cref="Win32Constants.ERROR_INVALID_FUNCTION"/> (Il computer non supporta un'interfaccia di spegnimento)<br/>
            /// <see cref="Win32Constants.ERROR_INVALID_PARAMETER"/> (Parametri forniti non validi)<br/>
            /// <see cref="Win32Constants.ERROR_SHUTDOWN_IN_PROGRESS"/> (Lo spegnimento è già iniziato)<br/>
            /// <see cref="Win32Constants.ERROR_SHUTDOWN_IS_SCHEDULED"/> (Lo spegnimento è stato programmato ma non è iniziato)<br/>
            /// <see cref="Win32Constants.ERROR_SHUTDOWN_USERS_LOGGED_ON"/> (Uno o più utenti oltre a quello attuale sono connessi al computer)</returns>
            /// <remarks>Questa funzione richiede il privilegio <see cref="Win32Constants.SE_SHUTDOWN_NAME"/> per spegnere il computer locale, <see cref="Win32Constants.SE_REMOTE_SHUTDOWN_NAME"/> per spegnere un computer remoto.<br/><br/>
            /// Il codice di errore <see cref="Win32Constants.ERROR_INVALID_PARAMETER"/> viene impostato nei seguenti casi:<br/><br/>
            /// <paramref name="MachineName"/> specifica un computer remoto e <paramref name="ShutdownFlags"/> non specifica <see cref="Win32Enumerations.ShutdownFlags.SHUTDOWN_FORCE_SELF"/><br/>
            /// <paramref name="GracePeriod"/> è maggiore di 0 e <paramref name="ShutdownFlags"/> non specifica <see cref="Win32Enumerations.ShutdownFlags.SHUTDOWN_FORCE_SELF"/> oppure specifica <see cref="Win32Enumerations.ShutdownFlags.SHUTDOWN_GRACE_OVERRIDE"/><br/><br/>
            /// Se lo spegnimento è stato programmato, <paramref name="ShutdownFlags"/> deve specificare <see cref="Win32Enumerations.ShutdownFlags.SHUTDOWN_GRACE_OVERRIDE"/>.<br/><br/>
            /// Se ci sono altri utenti connessi, <paramref name="ShutdownFlags"/> deve specificare <see cref="Win32Enumerations.ShutdownFlags.SHUTDOWN_FORCE_OTHERS"/>.</remarks>
            [DllImport("Advapi32.dll", EntryPoint = "InitiateShutdownW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern uint InitiateShutdown(string MachineName, string Message, uint GracePeriod, Win32Enumerations.ShutdownFlags ShutdownFlags, Win32Enumerations.ShutdownReasons Reason);

            /// <summary>
            /// Blocca il display della workstation.
            /// </summary>
            /// <returns>true se l'operazione è iniziata, false altrimenti.</returns>
            /// <remarks>Questa funzione può essere chiamata solo da applicazioni sul desktop interattivo.</remarks>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("User32.dll", EntryPoint = "LockWorkStation", SetLastError = true)]
            public static extern bool LockWorkstation();

            /// <summary>
            /// Sospende o iberna il sistema.
            /// </summary>
            /// <param name="Hibernate">Indica se ibernare il sistema.</param>
            /// <param name="Force">Nessun effetto.</param>
            /// <param name="WakeUpEventsDisabled">Indica se disabilitare gli eventi di risveglio.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>Questa funzione richiede il privilegio <see cref="Win32Constants.SE_SHUTDOWN_NAME"/> per cambiare lo stato energetico del computer.</remarks>
            [return: MarshalAs(UnmanagedType.U1)]
            [DllImport("PowrProf.dll", EntryPoint = "SetSuspendState", SetLastError = true)]
            public static extern bool SetSuspendState([MarshalAs(UnmanagedType.U1)] bool Hibernate, [MarshalAs(UnmanagedType.U1)] bool Force, [MarshalAs(UnmanagedType.U1)] bool WakeUpEventsDisabled);

            /// <summary>
            /// Disconnette l'utente, spegne o riavvia il computer.
            /// </summary>
            /// <param name="Flags">Tipo di spegnimento.</param>
            /// <param name="Reason">Motivazione dello spegnimento.</param>
            /// <returns>true se l'operazione è iniziata, false altrimenti.</returns>
            /// <remarks>Questa funzione invia il messaggio WM_QUERYENDSESSION alle applicazioni per determinare se possono essere terminate.<br/><br/>
            /// Le uniche opzioni definite per il parametro <paramref name="Flags"/> servono solo a disconnettere l'utente.</remarks>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("User32.dll", EntryPoint = "ExitWindowsEx", SetLastError = true)]
            public static extern bool ExitWindows(Win32Enumerations.LogOffFlags Flags, Win32Enumerations.ShutdownReasons Reason);
        }

        /// <summary>
        /// Contiene funzioni che permettono di recuperare informazioni sul computer.
        /// </summary>
        public static class Win32ComputerInfoFunctions
        {
            #region Hardware Info Functions
            /// <summary>
            /// Recupera informazioni sul sistema.
            /// </summary>
            /// <param name="SystemInfo">Struttura <see cref="Win32Structures.SYSTEM_INFO"/> che contiene le informazioni.</param>
            [DllImport("Kernel32.dll", EntryPoint = "GetNativeSystemInfo", SetLastError = true)]
            public static extern void GetNativeSystemInfo(out Win32Structures.SYSTEM_INFO SystemInfo);

            /// <summary>
            /// Recupera il nome NetBIOS o il nome DNS associato al computer locale.
            /// </summary>
            /// <param name="NameType">Formato del nome.</param>
            /// <param name="Buffer">Buffer che riceve il computer.</param>
            /// <param name="Size">Dimensione, in caratteri, del nome del computer.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", EntryPoint = "GetComputerNameExW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool GetComputerName(Win32Enumerations.ComputerNameFormat NameType, StringBuilder Buffer, ref uint Size);

            /// <summary>
            /// Recupera informazioni sul profilo hardware corrente per il computer locale.
            /// </summary>
            /// <param name="HwProfileInfo">Struttura <see cref="Win32Structures.HW_PROFILE_INFO"/> con le informazioni.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Advapi32.dll", EntryPoint = "GetCurrentHwProfileW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool GetCurrentHwProfile(out Win32Structures.HW_PROFILE_INFO HwProfileInfo);

            /// <summary>
            /// Recupera il tipo di firmware.
            /// </summary>
            /// <param name="FirmwareType">Tipo di firmware, uno dei valori dell'enumerazione <see cref="Win32Enumerations.FirmwareType"/>.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", EntryPoint = "GetFirmwareType", SetLastError = true)]
            public static extern bool GetFirmwareType(out Win32Enumerations.FirmwareType FirmwareType);

            /// <summary>
            /// Determina se il processore implementa una specifica funzionalita.
            /// </summary>
            /// <param name="ProcessorFeature">Funzionalità da controllare.</param>
            /// <returns>true se la funzionalità è supportata, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "IsProcessorFeaturePresent")]
            public static extern bool IsProcessorFeaturePresent(Win32Enumerations.ProcessorFeature ProcessorFeature);

            /// <summary>
            /// Recupera informazioni sulle relazioni tra i processori logici e hardware correlato.
            /// </summary>
            /// <param name="RelationshipType">Tipo di relazione.</param>
            /// <param name="Buffer">Buffer che riceve un array di strutture <see cref="Win32Structures.SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX"/>, in caso di errore i contenuti non sono definiti.</param>
            /// <param name="ReturnedLength">In input la dimensione, in bytes, di <paramref name="Buffer"/>, in caso di successo questo parametro viene impostato al numero di bytes restituiti, in caso di errore, viene impostato alla dimensione necessaria per contenere tutti i dati.</param>
            /// <returns>true se almeno una struttura <see cref="Win32Structures.SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX"/> è stata scritta nel buffer, false altrimenti.</returns>
            /// <remarks><paramref name="ReturnedLength"/> riceve la dimensione necessaria di <paramref name="Buffer"/> solo se il codice di errore viene impostato su <see cref="Win32Constants.ERROR_INSUFFICIENT_BUFFER"/>.<br/><br/>
            /// Se questa funzione viene chiamata da un processo a 32 bit su un sistema con più di 64 processori alcune delle maschere di affinità restituite potrebbero essere non corrette, inoltre i campi <see cref="Win32Structures.PROCESSOR_GROUP_INFO.ActiveProcessorCount"/> e <see cref="Win32Structures.PROCESSOR_GROUP_INFO.MaximumProcessorCount"/> potrebbe escludere alcuni processori logici attivi.<br/><br/>
            /// Quando questa funzione viene chiamata con <paramref name="RelationshipType"/> impostato su <see cref="Win32Enumerations.ProcessorRelationshipType.RelationProcessorCore"/> viene restituita una struttura <see cref="Win32Structures.PROCESSOR_RELATIONSHIP"/> per ogni core attivo in ogni gruppo del sistema.</remarks>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", EntryPoint = "GetLogicalProcessorInformationEx", SetLastError = true)]
            public static extern bool GetLogicalProcessorInformation(Win32Enumerations.ProcessorRelationshipType RelationshipType, IntPtr Buffer, ref uint ReturnedLength);

            /// <summary>
            /// Recupera la quantità di RAM che è installata fisicamente nel computer.
            /// </summary>
            /// <param name="TotalMemoryInKilobytes">Variabile che riceve la quantità di meoria RAM installata, in KB.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", EntryPoint = "GetPhysicallyInstalledSystemMemory", SetLastError = true)]
            public static extern bool GetPhysicallyInstalledSystemMemory(out ulong TotalMemoryInKilobytes);

            /// <summary>
            /// Recupera informazioni sull'attuale utilizzo della memoria virtuale e fisica da parte del sistema.
            /// </summary>
            /// <param name="Buffer">Struttura <see cref="Win32Structures.MEMORYSTATUSEX"/> con le informazioni.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", EntryPoint = "GlobalMemoryStatusEx", SetLastError = true)]
            public static extern bool GlobalMemoryStatus(IntPtr Buffer);

            /// <summary>
            /// Recupera informazioni sulla performance del sistema.
            /// </summary>
            /// <param name="PerformanceInformation">Struttura <see cref="Win32Structures.PERFORMANCE_INFORMATION"/> con le informazioni.</param>
            /// <param name="StructureSize">Dimensione della struttura, in bytes.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Psapi.dll", EntryPoint = "GetPerformanceInfo", SetLastError = true)]
            public static extern bool GetPerformanceInfo(out Win32Structures.PERFORMANCE_INFORMATION PerformanceInformation, uint StructureSize);
            #endregion
            #region OS Info Functions
            /// <summary>
            /// Recupera il numero di millisecondi passati dall'avvio del sistema.
            /// </summary>
            /// <returns>Il numero di millisecondi.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetTickCount64", SetLastError = true)]
            public static extern ulong GetTickCount();

            /// <summary>
            /// Recupera il percorso della cartella di Windows.
            /// </summary>
            /// <param name="Buffer">Array di caratteri che contiene il percorso.</param>
            /// <param name="Size">Dimensione del parametro <paramref name="Buffer"/>, in caratteri.</param>
            /// <returns></returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetWindowsDirectoryW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint GetWindowsDirectory(StringBuilder Buffer, uint Size);

            /// <summary>
            /// Recupera il percorso della directory di sistema.
            /// </summary>
            /// <param name="Buffer">Buffer che riceve il percorso, il percorso non termina con "\" a meno che la directory non si trova nella root dell'unità.</param>
            /// <param name="Size">Dimensione massima di <paramref name="Size"/>, in caratteri.</param>
            /// <returns>La lunghezza, in caratteri, della stringa scritta in <paramref name="Buffer"/> escluso il carattere nullo, se la dimensione di <paramref name="Buffer"/> non è sufficiente, viene restituita la dimensione necessaria incluso il carattere nullo, 0 in ogni altro caso.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetSystemDirectoryW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint GetSystemDirectory(StringBuilder Buffer, uint Size);

            /// <summary>
            /// Recupera il percorso della cartella di Windows condivisa in un sistema multi utente.
            /// </summary>
            /// <param name="Buffer">Buffer che riceve il percorso, il percorso non termina con "\" a meno che la directory non si trova nella root dell'unità.</param>
            /// <param name="Size">Dimensione massima di <paramref name="Buffer"/>, i caratteri.</param>
            /// <returns>La lunghezza, in caratteri, della stringa scritta in <paramref name="Buffer"/> escluso il carattere nullo, se la dimensione di <paramref name="Buffer"/> non è sufficiente, viene restituita la dimensione necessaria incluso il carattere nullo, 0 in ogni altro caso.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetSystemWindowsDirectoryW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint GetSystemWindowsDirectory(StringBuilder Buffer, uint Size);

            /// <summary>
            /// Recupera il percorso della directory di sistema usata da WOW64.
            /// </summary>
            /// <param name="Buffer">Buffer che riceve il percorso, il percorso non termina con "\".</param>
            /// <param name="Size">Dimensione massima di <paramref name="Buffer"/>, in caratteri.</param>
            /// <returns>La lunghezza, in caratteri, della stringa scritta in <paramref name="Buffer"/> escluso il carattere nullo, se la dimensione di <paramref name="Buffer"/> non è sufficiente, viene restituita la dimensione necessaria incluso il carattere nullo, 0 in ogni altro caso.</returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetSystemWow64DirectoryW", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint GetSystemWow64Directory(StringBuilder Buffer, uint Size);

            /// <summary>
            /// Recupera la dimensione corrente e quella massima del registro di sistema.
            /// </summary>
            /// <param name="QuotaAllowed">Dimensione massima del registro di sistema, in bytes.</param>
            /// <param name="QuotaUsed">Dimensione attuale del registro di sistema, in bytes.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", EntryPoint = "GetSystemRegistryQuota", SetLastError = true)]
            public static extern bool GetSystemRegistryQuota(out uint QuotaAllowed, out uint QuotaUsed);

            /// <summary>
            /// Indica se il sistema operativo è stato avviato da un contenitore VHD.
            /// </summary>
            /// <param name="NativeVhdBoot">Variabile che indica se l'avvio del sistema è avvenuto da VHD.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", EntryPoint = "IsNativeVhdBoot", SetLastError = true)]
            public static extern bool IsNativeVhdBoot([MarshalAs(UnmanagedType.Bool)] out bool NativeVhdBoot);

            /// <summary>
            /// Recupera il tipo di prodotto per il sistema operativo nel computer locale.
            /// </summary>
            /// <param name="OSMajorVersion">Versione maggiore del sistema operativo, valore minimo 6.</param>
            /// <param name="OSMinorVersion">Versione minore del sistema operativo, valore minino 0.</param>
            /// <param name="SpMajorVersion">Versione maggiore del service pack del sistema operativo, valore minimo 0.</param>
            /// <param name="SpMinorVersion">Versione minore del service pack del sistema operativo, valore minimo 0.</param>
            /// <param name="ReturnedProductType">Tipo di prodotto.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("Kernel32.dll", EntryPoint = "GetProductInfo", SetLastError = true)]
            public static extern bool GetProductInfo(uint OSMajorVersion, uint OSMinorVersion, uint SpMajorVersion, uint SpMinorVersion, out Win32Enumerations.ProductName ReturnedProductType);

            /// <summary>
            /// Recupera i numeri di versione di Windows da NTDLL.
            /// </summary>
            /// <param name="MajorVersion">Versione maggiore.</param>
            /// <param name="MinorVersion">Versione minore.</param>
            /// <param name="BuildNumber">Numero build.</param>
            /// <remarks>I primi 16 bit di <paramref name="BuildNumber"/> rappresenta il normale numero di build, gli ultimi quattro bit distingue tra versione free e checked.</remarks>
            [DllImport("Ntdll.dll", EntryPoint = "RtlGetNtVersionNumbers")]
            public static extern void RtlGetNtVersionNumbers(out uint MajorVersion, out uint MinorVersion, out uint BuildNumber);

            /// <summary>
            /// Recupera o imposta uno dei parametri di sistema, questa funzione può anche aggiornare il profilo utente dopo l'impostazione.
            /// </summary>
            /// <param name="Action">Parametro da recuperare o impostare.</param>
            /// <param name="uiParam">L'utilizzo di questo parametro cambia in base al valore del parametro <paramref name="Action"/>.</param>
            /// <param name="Param">L'utilizzo di questo parametro cambia in base al valore del parametro <paramref name="Action"/>.</param>
            /// <param name="WinIni">Indica se aggiornare o meno il profilo utente dopo l'impostazione di un parametro.</param>
            /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
            /// <remarks>I valori validi per il parametro <paramref name="Action"/> si trovano nelle seguenti enumerazioni:<br/><br/>
            /// <see cref="Win32Enumerations.SystemAccessibilityParameters"/><br/>
            /// <see cref="Win32Enumerations.SystemDesktopParameters"/><br/>
            /// <see cref="Win32Enumerations.SystemIconParameters"/><br/>
            /// <see cref="Win32Enumerations.SystemInputParameters"/><br/>
            /// <see cref="Win32Enumerations.SystemMenuParameters"/><br/>
            /// <see cref="Win32Enumerations.SystemScreenSaverParameters"/><br/>
            /// <see cref="Win32Enumerations.SystemTimeoutParameters"/><br/>
            /// <see cref="Win32Enumerations.SystemUIParameters"/><br/>
            /// <see cref="Win32Enumerations.SystemWindowParameters"/></remarks>
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("User32.dll", EntryPoint = "SystemParametersInfoW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool SystemParametersInfo(uint Action, uint uiParam, IntPtr Param, Win32Enumerations.SystemParametersUserProfileUpdateOptions WinIni);
            #endregion
        }
    }
}