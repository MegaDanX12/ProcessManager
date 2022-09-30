namespace ProcessManager.InfoClasses.SystemParametersInfoStructures
{
    /// <summary>
    /// Informazioni sul timeout delle funzionalità di accessibilità.
    /// </summary>
    public struct AccessibilityTimeout
    {
        /// <summary>
        /// Indica se il timeout è abilitato.
        /// </summary>
        public bool TimeoutEnabled { get; }
        /// <summary>
        /// Indica se il feedback sonoro è attivato.
        /// </summary>
        public bool FeedbackEnabled { get; }
        /// <summary>
        /// Tempo di timeout, in millisecondi.
        /// </summary>
        public uint TimeoutMilliseconds { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="AccessibilityTimeout"/>.
        /// </summary>
        /// <param name="TimeoutEnabled">Indica se il timeout è abilitato.</param>
        /// <param name="FeedbackEnabled">Indica se il feedback sonoro è attivato.</param>
        public AccessibilityTimeout(bool TimeoutEnabled, bool FeedbackEnabled, uint TimeoutMilliseconds)
        {
            this.TimeoutEnabled = TimeoutEnabled;
            this.FeedbackEnabled = FeedbackEnabled;
            this.TimeoutMilliseconds = TimeoutMilliseconds;
        }
    }

    /// <summary>
    /// Informazioni sulla funzionalià Descrizione Audio.
    /// </summary>
    public struct AudioDescriptionInfo
    {
        /// <summary>
        /// Indica se la funzionalità è abilitata.
        /// </summary>
        public bool AudioDescriptionEnabled { get; }
        /// <summary>
        /// Lingua audio.
        /// </summary>
        public string Locale { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="AudioDescriptionInfo"/>.
        /// </summary>
        /// <param name="Enabled">Indica se la funzionalità è abilitata.</param>
        /// <param name="Locale">Lingua audio.</param>
        public AudioDescriptionInfo(bool Enabled, string Locale)
        {
            AudioDescriptionEnabled = Enabled;
            this.Locale = Locale;
        }
    }

    /// <summary>
    /// Informazioni sulla funzionalità Filtro Tasti.
    /// </summary>
    public struct FilterKeysFeatureInfo
    {
        /// <summary>
        /// Indica se la funzionalità è disponibile.
        /// </summary>
        public bool FilterKeysAvailable { get; }
        /// <summary>
        /// Indica se il computer emette un suono alla pressione di un tasto.
        /// </summary>
        public bool ClickSoundEnabled { get; }
        /// <summary>
        /// Indica se la funzionalità è abilitata.
        /// </summary>
        public bool FilterKeysEnabled { get; }
        /// <summary>
        /// Indica se è possibile attivare o disattivare la funzionalità con la hotkey.
        /// </summary>
        public bool HotkeyActive { get; }
        /// <summary>
        /// Indica se il sistema emette un suono all'attivazione della funzionalità con la hotkey.
        /// </summary>
        public bool HotkeySoundEnabled { get; }
        /// <summary>
        /// Tempo, in millisecondi, durante il quale l'utente deve tenere premuto un tasto prima dell'accettazione da parte del computer.
        /// </summary>
        public uint KeyDownWaitTimeMilliseconds { get; }
        /// <summary>
        /// Tempo, in millisecondi, che un tasto deve rimanere premuto perché esso si ripeta.
        /// </summary>
        public uint KeyDownRepeatDelayMilliseconds { get; }
        /// <summary>
        /// Tempo, in millisecondi, tra ripetizioni della pressione di un tasto.
        /// </summary>
        public uint KeyRepetitionDelayMilliseconds { get; }
        /// <summary>
        /// Tempo, in millisecondi, che deve passare prima che il sistema accetti un'ulteriore pressione dello stesso tasto.
        /// </summary>
        public uint NextKeyDownFromKeyUpMilliseconds { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="FilterKeysFeatureInfo"/>.
        /// </summary>
        /// <param name="FeatureAvailable">Indica se la funzionalità è disponibile.</param>
        /// <param name="ClickSoundEnabled">Indica se il computer emette un suono alla pressione di un tasto.</param>
        /// <param name="FeatureEnabled">Indica se la funzionalità è abilitata.</param>
        /// <param name="HotkeyActive">Indica se è possibile attivare o disattivare la funzionalità con la hotkey.</param>
        /// <param name="HotkeySoundEnabled">Indica se il sistema emette un suono all'attivazione della funzionalità con la hotkey.</param>
        /// <param name="WaitMSec">Tempo, in millisecondi, durante il quale l'utente deve tenere premuto un tasto prima dell'accettazione da parte del computer.</param>
        /// <param name="DelayMSec">Tempo, in millisecondi, che un tasto deve rimanere premuto perché esso si ripeta.</param>
        /// <param name="RepeatMSec">Tempo, in millisecondi, tra ripetizioni della pressione di un tasto.</param>
        /// <param name="BounceMSec">Tempo, in millisecondi, che deve passare prima che il sistema accetti un'ulteriore pressione dello stesso tasto.</param>
        public FilterKeysFeatureInfo(bool FeatureAvailable, bool ClickSoundEnabled, bool FeatureEnabled, bool HotkeyActive, bool HotkeySoundEnabled, uint WaitMSec, uint DelayMSec, uint RepeatMSec, uint BounceMSec)
        {
            FilterKeysAvailable = FeatureAvailable;
            this.ClickSoundEnabled = ClickSoundEnabled;
            FilterKeysEnabled = FeatureEnabled;
            this.HotkeyActive = HotkeyActive;
            this.HotkeySoundEnabled = HotkeySoundEnabled;
            KeyDownWaitTimeMilliseconds = WaitMSec;
            KeyDownRepeatDelayMilliseconds = DelayMSec;
            KeyRepetitionDelayMilliseconds = RepeatMSec;
            NextKeyDownFromKeyUpMilliseconds = BounceMSec;
        }
    }

    /// <summary>
    /// Dimensioni del rettangolo di focus.
    /// </summary>
    public struct FocusBorderData
    {
        /// <summary>
        /// Altezza del rettangolo.
        /// </summary>
        public int Height { get; }
        /// <summary>
        /// Larghezza del rettangolo.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="FocusBorderData"/>.
        /// </summary>
        /// <param name="Height">Altezza del rettangolo.</param>
        /// <param name="Width">Larghezza del rettangolo.</param>
        public FocusBorderData(int Height, int Width)
        {
            this.Height = Height;
            this.Width = Width;
        }
    }

    /// <summary>
    /// Informazioni sulla funzionalità Alto Contrasto.
    /// </summary>
    public struct HighContrastFeatureInfo
    {
        /// <summary>
        /// Indica se la funzionalità è attiva.
        /// </summary>
        public bool FeatureEnabled { get; }
        /// <summary>
        /// Indica se la funzionalità è disponibile.
        /// </summary>
        public bool FeatureAvailable { get; }
        /// <summary>
        /// Indica se la funzionalità può essere attivata o disattivata tramite hotkey.
        /// </summary>
        public bool HotkeyActive { get; }
        /// <summary>
        /// Indica se il sistema deve mostrare una finestra di conferma quando la funzionalità viene attivata tramite hotkey.
        /// </summary>
        public bool ConfirmationDialogEnabled { get; }
        /// <summary>
        /// Indica se il sistema deve emettere un suono quando la funzionalità viene attivata tramite hotkey.
        /// </summary>
        public bool SoundEnabled { get; }
        /// <summary>
        /// Indica se l'attivazione della funzionalità tramite hotkey è disponibile.
        /// </summary>
        public bool HotkeyAvailable { get; }
        /// <summary>
        /// Nome dello schema di colori che sarà impostato come default.
        /// </summary>
        public string DefaultSchemeName { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="HighContrastFeatureInfo"/>.
        /// </summary>
        /// <param name="Enabled">Indica se la funzionalità è attiva.</param>
        /// <param name="Available">Indica se la funzionalità è disponibile.</param>
        /// <param name="HotkeyActive">Indica se la funzionalità può essere attivata o disattivata tramite hotkey.</param>
        /// <param name="ConfirmationDialogEnabled">Indica se il sistema deve mostrare una finestra di conferma quando la funzionalità viene attivata tramite hotkey.</param>
        /// <param name="SoundEnabled">Indica se il sistema deve emettere un suono quando la funzionalità viene attivata tramite hotkey.</param>
        /// <param name="HotkeyAvailable">Indica se l'attivazione della funzionalità tramite hotkey è disponibile.</param>
        /// <param name="DefaultSchemeName">Nome dello schema di colori che sarà impostato come default.</param>
        public HighContrastFeatureInfo(bool Enabled, bool Available, bool HotkeyActive, bool ConfirmationDialogEnabled, bool SoundEnabled, bool HotkeyAvailable, string DefaultSchemeName)
        {
            FeatureEnabled = Enabled;
            FeatureAvailable = Available;
            this.HotkeyActive = HotkeyActive;
            this.ConfirmationDialogEnabled = ConfirmationDialogEnabled;
            this.SoundEnabled = SoundEnabled;
            this.HotkeyAvailable = HotkeyAvailable;
            this.DefaultSchemeName = DefaultSchemeName;
        }
    }

    /// <summary>
    /// Informazioni sulla funzionalità Mouse Click Lock.
    /// </summary>
    public struct MouseClickLockFeatureInfo
    {
        /// <summary>
        /// Indica se la funzionalità è abilitata.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// Tempo, in millisecondi, che deve trascorrere prima che il pulsante del mouse venga bloccato.
        /// </summary>
        public int DelayMSec { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="MouseClickLockFeatureInfo"/>.
        /// </summary>
        /// <param name="Enabled">Indica se la funzionalità è abilitata.</param>
        /// <param name="DelayMSec">Tempo, in millisecondi, che deve trascorrere prima che il pulsante del mouse venga bloccato.</param>
        public MouseClickLockFeatureInfo(bool? Enabled, uint? DelayMSec)
        {
            this.Enabled = Enabled ?? false;
            this.DelayMSec = DelayMSec.HasValue ? (int)DelayMSec.Value : -1;
        }
    }

    /// <summary>
    /// Informazioni sulla funzionalità MouseKeys.
    /// </summary>
    public struct MouseKeysInfo
    {
        /// <summary>
        /// Indica se la funzionalità è disponibile.
        /// </summary>
        public bool FeatureAvailable { get; }
        /// <summary>
        /// Indica se la funzionalità può essere attivata o disattivata tramite hotkey.
        /// </summary>
        public bool HotkeyActive { get; }
        /// <summary>
        /// Indica se il sistema emette un suono quando la funzionalità viene attivata.
        /// </summary>
        public bool SoundEnabled { get; }
        /// <summary>
        /// Indica se la funzionalità è abilitata.
        /// </summary>
        public bool FeatureEnabled { get; }
        /// <summary>
        /// Velocità massima del cursore.
        /// </summary>
        public uint MaximumCursorSpeed { get; }
        /// <summary>
        /// Tempo, in millisecondi, che il puntatore del mouse richiede per raggiungere la velocità massima.
        /// </summary>
        public uint TimeToMaxSpeed { get; }
        /// <summary>
        /// Moltiplicatore da applicare alla velocità del cursore del mouse quando l'utente tiene premuto il tasto CTRL.
        /// </summary>
        public uint CtrlMultiplier { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="MouseKeysInfo"/>.
        /// </summary>
        /// <param name="Available">Indica se la funzionalità è disponibile.</param>
        /// <param name="HotkeyActive">Indica se la funzionalità può essere attivata o disattivata tramite hotkey.</param>
        /// <param name="SoundEnabled">Indica se il sistema emette un suono quando la funzionalità viene attivata.</param>
        /// <param name="Enabled">Indica se la funzionalità è abilitata.</param>
        /// <param name="MaxSpeed">Velocità massima del cursore.</param>
        /// <param name="TimeToMaxSpeed">Tempo, in millisecondi, che il puntatore del mouse richiede per raggiungere la velocità massima.</param>
        /// <param name="CtrlSpeed">Moltiplicatore da applicare alla velocità del cursore del mouse quando l'utente tiene premuto il tasto CTRL.</param>
        public MouseKeysInfo(bool Available, bool HotkeyActive, bool SoundEnabled, bool Enabled, uint MaxSpeed, uint TimeToMaxSpeed, uint CtrlSpeed)
        {
            FeatureAvailable = Available;
            this.HotkeyActive = HotkeyActive;
            this.SoundEnabled = SoundEnabled;
            FeatureEnabled = Enabled;
            MaximumCursorSpeed = MaxSpeed;
            this.TimeToMaxSpeed = TimeToMaxSpeed;
            CtrlMultiplier = CtrlSpeed;
        }
    }

    /// <summary>
    /// Informazioni sulla funzionalità SoundSentry.
    /// </summary>
    public struct SoundSentryInfo
    {
        /// <summary>
        /// Indica se la funzionalità è disponibile.
        /// </summary>
        public bool FeatureAvailable { get; }
        /// <summary>
        /// Indica se la funzionalità è attiva.
        /// </summary>
        public bool FeatureEnabled { get; }
        /// <summary>
        /// Segnale visuale generato quando un suono viene generato da un'applicazione basata su Windows o da un'applicazione MS-DOS in esecuzione in una finestra.
        /// </summary>
        public SoundSentryWindowsEffect WindowsEffect { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="SoundSentryInfo"/>,
        /// </summary>
        /// <param name="Available">Indica se la funzionalità è disponibile.</param>
        /// <param name="Enabled">Indica se la funzionalità è attiva.</param>
        /// <param name="WindowsEffect">Segnale visuale generato quando un suono viene generato da un'applicazione basata su Windows o da un'applicazione MS-DOS in esecuzione in una finestra.</param>
        public SoundSentryInfo(bool Available, bool Enabled, SoundSentryWindowsEffect WindowsEffect)
        {
            FeatureAvailable = Available;
            FeatureEnabled = Enabled;
            this.WindowsEffect = WindowsEffect;
        }
    }

    /// <summary>
    /// Segnale visivo della funzionalità SoundSentry.
    /// </summary>
    public enum SoundSentryWindowsEffect
    {
        /// <summary>
        /// Nessun segnale visuale.
        /// </summary>
        None,
        /// <summary>
        /// La barra del titolo della finestra attiva lampeggia.
        /// </summary>
        FlashTitleBar,
        /// <summary>
        /// La finestra attiva lampeggia.
        /// </summary>
        FlashActiveWindow,
        /// <summary>
        /// Il display lampeggia.
        /// </summary>
        FlashDisplay,
        /// <summary>
        /// Segnale visuale personalizzato.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Informazioni sulla funzionalità Tasti Permanenti.
    /// </summary>
    public struct StickyKeysFeatureInfo
    {
        /// <summary>
        /// Indica se la funzionalità è disponibile.
        /// </summary>
        public bool FeatureAvailable { get; }
        /// <summary>
        /// Indica se la funzionalità è attiva.
        /// </summary>
        public bool FeatureEnabled { get; }
        /// <summary>
        /// Indica se il sistema deve emettere un suono quando l'utente aggancia, blocca o rilascia un tasto di controllo usando la funzionalità.
        /// </summary>
        public bool AudioFeedbackEnabled { get; }
        /// <summary>
        /// Indica se la funzionalità è attivabile o disattivabile tramite hotkey.
        /// </summary>
        public bool HotkeyActive { get; }
        /// <summary>
        /// Indica se il sistema deve emettere un suono quando la funzionalità viene attivata tramite hotkey.
        /// </summary>
        public bool HotkeySoundEnabled { get; }
        /// <summary>
        /// Indica se premere due volte un tasto di controllo blocca il tasto come premuto fino a una terza pressione.
        /// </summary>
        public bool ModifierKeyLockAfterDoublePress { get; }
        /// <summary>
        /// Indica se rilasciare un tasto di controllo premuto in combinazione con qualunque altro tasto disattiva la funzionalità.
        /// </summary>
        public bool ModifierKeyReleaseTurnsOffFeature { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="StickyKeysFeatureInfo"/>.
        /// </summary>
        /// <param name="Available">Indica se la funzionalità è disponibile.</param>
        /// <param name="Enabled">Indica se la funzionalità è attiva.</param>
        /// <param name="AudioFeedbackEnabled">Indica se il sistema deve emettere un suono quando l'utente aggancia, blocca o rilascia un tasto di controllo usando la funzionalità.</param>
        /// <param name="HotkeyActive">Indica se la funzionalità è attivabile o disattivabile tramite hotkey.</param>
        /// <param name="HotkeySoundEnabled">Indica se il sistema deve emettere un suono quando la funzionalità viene attivata tramite hotkey.</param>
        /// <param name="Tristate">Indica se premere due volte un tasto di controllo blocca il tasto come premuto fino a una terza pressione.</param>
        /// <param name="TwoKeysOff">Indica se rilasciare un tasto di controllo premuto in combinazione con qualunque altro tasto disattiva la funzionalità.</param>
        public StickyKeysFeatureInfo(bool Available, bool Enabled, bool AudioFeedbackEnabled, bool HotkeyActive, bool HotkeySoundEnabled, bool Tristate, bool TwoKeysOff)
        {
            FeatureAvailable = Available;
            FeatureEnabled = Enabled;
            this.AudioFeedbackEnabled = AudioFeedbackEnabled;
            this.HotkeyActive = HotkeyActive;
            this.HotkeySoundEnabled = HotkeySoundEnabled;
            ModifierKeyLockAfterDoublePress = Tristate;
            ModifierKeyReleaseTurnsOffFeature = TwoKeysOff;
        }
    }

    /// <summary>
    /// Informazioni sulla funzionalità ToggleKeys.
    /// </summary>
    public struct ToggleKeysFeatureInfo
    {
        /// <summary>
        /// Indica se la funzionalità è disponibile.
        /// </summary>
        public bool FeatureAvailable { get; }
        /// <summary>
        /// Indica se la funzionalità è abilitata.
        /// </summary>
        public bool FeatureEnabled { get; }
        /// <summary>
        /// Indica se è possibile attivare la funzionalità tramite hotkey.
        /// </summary>
        public bool HotkeyActive { get; }
        /// <summary>
        /// Indica se il sistema deve emettere un suono se la funzionalità viene attivata o disattivata tramite hotkey.
        /// </summary>
        public bool HotkeySound { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="ToggleKeysFeatureInfo"/>.
        /// </summary>
        /// <param name="Available">Indica se la funzionalità è disponibile.</param>
        /// <param name="Enabled">Indica se la funzionalità è abilitata.</param>
        /// <param name="HotkeyActive">Indica se è possibile attivare la funzionalità tramite hotkey.</param>
        /// <param name="HotkeySound">Indica se il sistema deve emettere un suono se la funzionalità viene attivata o disattivata tramite hotkey.</param>
        public ToggleKeysFeatureInfo(bool Available, bool Enabled, bool HotkeyActive, bool HotkeySound)
        {
            FeatureAvailable = Available;
            FeatureEnabled = Enabled;
            this.HotkeyActive = HotkeyActive;
            this.HotkeySound = HotkeySound;
        }
    }
}