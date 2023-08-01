using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessManager
{
    #region Structure Definitions
    /// <summary>
    /// Informazioni sulla politica Data Execution Prevention.
    /// </summary>
    public struct DEPPolicyData
    {
        /// <summary>
        /// Indica se la politica è attiva.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// 
        /// </summary>
        public bool AtlThunkEmulationDisabled { get; }
        /// <summary>
        /// Indica se la politica è attiva in modo permanente.
        /// </summary>
        public bool Permanent { get; }

        public DEPPolicyData(bool Enabled, bool AtlThunkEmulationDisabled, bool Permanent)
        {
            this.Enabled = Enabled;
            this.AtlThunkEmulationDisabled = AtlThunkEmulationDisabled;
            this.Permanent = Permanent;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            if (Enabled)
            {
                if (AtlThunkEmulationDisabled)
                {
                    sb.Append("Atl Thunk Emulation Disabled");
                }
                if (Permanent)
                {
                    if (sb.Length == 0)
                    {
                        sb.Append("Permanent");
                    }
                    else
                    {
                        sb.Append(",Permanent");
                    }
                }
                EnabledSettings = sb.ToString();
                return true;
            }
            else
            {
                EnabledSettings = null;
                return false;
            }
        }
    }


    /// <summary>
    /// Informazioni sulla politica Address Space Randomization Layout.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct ASLRPolicyData
    {
        /// <summary>
        /// 
        /// </summary>
        public bool BottomUpRandomizationEnabled { get; }
        /// <summary>
        /// 
        /// </summary>
        public bool ForceRelocateImagesEnabled { get; }
        /// <summary>
        /// 
        /// </summary>
        public bool HighEntropyEnabled { get; }
        /// <summary>
        /// 
        /// </summary>
        public bool DisallowStrippedImages { get; }

        public ASLRPolicyData(bool BottomUpRandomizationEnabled, bool ForceRelocateImagesEnabled, bool HighEntropyEnabled, bool DisallowStrippedImages)
        {
            this.BottomUpRandomizationEnabled = BottomUpRandomizationEnabled;
            this.ForceRelocateImagesEnabled = ForceRelocateImagesEnabled;
            this.HighEntropyEnabled = HighEntropyEnabled;
            this.DisallowStrippedImages = DisallowStrippedImages;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (BottomUpRandomizationEnabled)
            {
                sb.Append("Bottom Up Randomization Enabled");
            }
            if (ForceRelocateImagesEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Force Relocate Images Enabled");
                }
                else
                {
                    sb.Append(",Force Relocate Images Enabled");
                }
            }
            if (HighEntropyEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("High Entropy Enabled");
                }
                else
                {
                    sb.Append(",High Entropy Enabled");
                }
            }
            if (DisallowStrippedImages)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Disallow Stripped Images");
                }
                else
                {
                    sb.Append(",Disallow Stripped Images");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica relativa alla generazione e alla modifica di codice dinamico.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct DynamicCodePolicyData
    {
        /// <summary>
        /// Indica se la generazione di codice dinamico o la modifica di codice eseguibile esistente è proibita.
        /// </summary>
        public bool DynamicCodeProhibited { get; }
        /// <summary>
        /// Indica se i thread del processo possono ignorare la politica del processo.
        /// </summary>
        public bool ThreadOptOutAllowed { get; }
        /// <summary>
        /// Indica se processi che non sono AppContainer hanno il permesso di modificare le impostazioni della politica del processo.
        /// </summary>
        public bool RemoteDowngradeAllowed { get; }
        /// <summary>
        /// Indica se viene generato un evento quando la generazione di codice dinamico o la modifica di codice eseguibile viene permessa o negata.
        /// </summary>
        public bool DynamicCodeProhibitionAuditEnabled { get; }

        public DynamicCodePolicyData(bool DynamicCodeProhibited, bool ThreadOptOutAllowed, bool RemoteDowngradeAllowed, bool DynamicCodeProhibitionAuditEnabled)
        {
            this.DynamicCodeProhibited = DynamicCodeProhibited;
            this.ThreadOptOutAllowed = ThreadOptOutAllowed;
            this.RemoteDowngradeAllowed = RemoteDowngradeAllowed;
            this.DynamicCodeProhibitionAuditEnabled = DynamicCodeProhibitionAuditEnabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (DynamicCodeProhibited)
            {
                sb.Append("Dynamic Code Prohibited");
            }
            if (ThreadOptOutAllowed)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Thread OptOut Allowed");
                }
                else
                {
                    sb.Append(",Thread OptOut Allowed");
                }
            }
            if (RemoteDowngradeAllowed)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Remote Downgrade Allowed");
                }
                else
                {
                    sb.Append(",Remote Downgrade Allowed");
                }
            }
            if (DynamicCodeProhibitionAuditEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Dynamic Code Prohibition Audit Enabled");
                }
                else
                {
                    sb.Append(",Dynamic Code Prohibition Audit Enabled");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica relativa al comportamento del processo con handle non validi.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct StrictHandleCheckPolicyData
    {
        /// <summary>
        /// Indica se viene generata un eccezione se il processo tenta di manipolare un handle non valido.
        /// </summary>
        public bool ExceptionOnInvalidHandleReferenceRaised { get; }
        /// <summary>
        /// Indica se la generazione delle eccezioni è attiva in modo permanente.
        /// </summary>
        public bool HandleExceptionsPermanentlyEnabled { get; }

        public StrictHandleCheckPolicyData(bool ExceptionOnInvalidHandleReferenceRaised, bool HandleExceptionsPermanentlyEnabled)
        {
            this.ExceptionOnInvalidHandleReferenceRaised = ExceptionOnInvalidHandleReferenceRaised;
            this.HandleExceptionsPermanentlyEnabled = HandleExceptionsPermanentlyEnabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (ExceptionOnInvalidHandleReferenceRaised)
            {
                sb.Append("Exception On Invalid Handle Reference Raised");
            }
            if (HandleExceptionsPermanentlyEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Handle Exceptions Permanently Enabled");
                }
                else
                {
                    sb.Append(",Handle Exceptions Permanently Enabled");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica relativa alle restrizioni di un processo su quali system calls possono essere eseguite.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct SystemCallDisablePolicyData
    {
        /// <summary>
        /// Indica se il processo può eseguire funzioni NTUser/GDI al livello più basso.
        /// </summary>
        public bool Win32kSystemCallsDisabled { get; }
        /// <summary>
        /// Indica se l'abilitazione o la disabilitazione della politica deve generare un evento.
        /// </summary>
        public bool Win32kSystemCallsDisablementAuditEnabled { get; }

        public SystemCallDisablePolicyData(bool Win32kSystemCallsDisabled, bool Win32kSystemCallsDisablementAuditEnabled)
        {
            this.Win32kSystemCallsDisabled = Win32kSystemCallsDisabled;
            this.Win32kSystemCallsDisablementAuditEnabled = Win32kSystemCallsDisablementAuditEnabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (Win32kSystemCallsDisabled)
            {
                sb.Append("Win32k System Calls Disabled");
            }
            if (Win32kSystemCallsDisablementAuditEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Win32k System Calls Disablement Audit Enabled");
                }
                else
                {
                    sb.Append(",Win32k System Calls Disablement Audit Enabled");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica di un processo relativa alle extension points di terze parti.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct ExtensionPointDisablePolicyData
    {
        /// <summary>
        /// Indica se le extension points di terze parti sono disabilitate.
        /// </summary>
        public bool ExtensionPointsDisabled { get; }

        public ExtensionPointDisablePolicyData(bool ExtensionPointsDisabled)
        {
            this.ExtensionPointsDisabled = ExtensionPointsDisabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (ExtensionPointsDisabled)
            {
                sb.Append("Extension Points Disabled");
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica Control Flow Guard (CFG) di un processo.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct CFGPolicyData
    {
        /// <summary>
        /// Indica se Control Flow Guard (CFG) è abilitato.
        /// </summary>
        public bool ControlFlowGuardEnabled { get; }
        /// <summary>
        /// Indica se le funzione esportate saranno trattate di default come bersagli non validi di chiamate indirette.
        /// </summary>
        public bool ExportSuppressionEnabled { get; }
        /// <summary>
        /// Indica se tutte le DLL che vengono caricate devono avere CFG abilitato.
        /// </summary>
        public bool StrictModeEnabled { get; }

        public CFGPolicyData(bool ControlFlowGuardEnabled, bool ExportSuppressionEnabled, bool StrictModeEnabled)
        {
            this.ControlFlowGuardEnabled = ControlFlowGuardEnabled;
            this.ExportSuppressionEnabled = ExportSuppressionEnabled;
            this.StrictModeEnabled = StrictModeEnabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (ControlFlowGuardEnabled)
            {
                sb.Append("Control Flow Guard Enabled");
            }
            if (ExportSuppressionEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Export Suppression Enabled");
                }
                else
                {
                    sb.Append(",Export Suppression Enabled");
                }
            }
            if (StrictModeEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Strict Mode Enabled");
                }
                else
                {
                    sb.Append(",Strict Mode Enabled");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica di un processo relativa al caricamento di immagini in base alla firma.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct BinarySignaturePolicyData
    {
        /// <summary>
        /// Indica se il processo può caricare solo immagini firmate da Microsoft.
        /// </summary>
        public bool MicrosoftSignedOnly { get; }
        /// <summary>
        /// Indica se il processo può caricare solo immagini firmate dal Windows Store.
        /// </summary>
        public bool StoreSignedOnly { get; }
        /// <summary>
        /// Indica se il processo può caricare solo immagini firmate da Microsoft, dal Windows Store e dai Windows Hardware Quality Labs (WHQL).
        /// </summary>
        public bool MitigationOptIn { get; }
        /// <summary>
        /// Indica se il caricamento di immagini firmate da Microsoft deve generare un evento.
        /// </summary>
        public bool MicrosoftSignedOnlyAuditEnabled { get; }
        /// <summary>
        /// Indica se il caricamento di immagini firamte dal Windows Store deve generare un evento.
        /// </summary>
        public bool StoreSignedOnlyAuditEnabled { get; }

        public BinarySignaturePolicyData(bool MicrosoftSignedOnly, bool StoreSignedOnly, bool MitigationOptIn, bool MicrosoftSignedOnlyAuditEnabled, bool StoreSignedOnlyAuditEnabled)
        {
            this.MicrosoftSignedOnly = MicrosoftSignedOnly;
            this.StoreSignedOnly = StoreSignedOnly;
            this.MitigationOptIn = MitigationOptIn;
            this.MicrosoftSignedOnlyAuditEnabled = MicrosoftSignedOnlyAuditEnabled;
            this.StoreSignedOnlyAuditEnabled = StoreSignedOnlyAuditEnabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (MicrosoftSignedOnly)
            {
                sb.Append("Microsoft Signed Only");
            }
            if (StoreSignedOnly)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Store Signed Only");
                }
                else
                {
                    sb.Append(",Store Signed Only");
                }
            }
            if (MitigationOptIn)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Mitigation OptIn");
                }
                else
                {
                    sb.Append(",Mitigation OptIn");
                }
            }
            if (MicrosoftSignedOnlyAuditEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Microsoft Signed Only Audit Enabled");
                }
                else
                {
                    sb.Append(",Microsoft Signed Only Audit Enabled");
                }
            }
            if (StoreSignedOnlyAuditEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Store Signed Only Audit Enabled");
                }
                else
                {
                    sb.Append(",Store Signed Only Audit Enabled");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica di un processo relativa al caricamento di font non di sistema.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct FontDisablePolicyData
    {
        /// <summary>
        /// Indica se il caricamento di font non di sistema è diabilitato.
        /// </summary>
        public bool NonSystemFontsDisabled { get; }
        /// <summary>
        /// Indica se il caricamento di un font non di sistema deve generare un evento.
        /// </summary>
        public bool NonSystemFontLoadingAuditEnabled { get; }

        public FontDisablePolicyData(bool NonSystemFontsDisabled, bool NonSystemFontLoadingAuditEnabled)
        {
            this.NonSystemFontsDisabled = NonSystemFontsDisabled;
            this.NonSystemFontLoadingAuditEnabled = NonSystemFontLoadingAuditEnabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (NonSystemFontsDisabled)
            {
                sb.Append("Non System Fonts Disabled");
            }
            if (NonSystemFontLoadingAuditEnabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Non System Font Loading Audit Enabled");
                }
                else
                {
                    sb.Append(",Non System Font Loading Audit Enabled");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica di un processo relativa al caricamento di immagini da un dispositivo remoto.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct ImageLoadPolicyData
    {
        /// <summary>
        /// Indica se il processo può caricare immagini da un dispositivo remoto.
        /// </summary>
        public bool NoRemoteImages { get; }
        /// <summary>
        /// Indica se il processo può caricare immagini che una Low mandatory label.
        /// </summary>
        public bool NoLowMandatoryLabelImages { get; }
        /// <summary>
        /// Indica se il processo deve preferire il caricamento di immagini dalla sottocartella System32 della cartella di installazione di Windows prima di ogni altra posizione.
        /// </summary>
        public bool PreferSystem32Images { get; }
        /// <summary>
        /// Indica se il caricamento di immagini da un dispositivo remoto deve generare un evento.
        /// </summary>
        public bool AuditNoRemoteImages { get; }
        /// <summary>
        /// Indica se il caricamento di immagini con una Low mandatory label deve generare un evento.
        /// </summary>
        public bool AuditNoLowMandatoryLabelImages { get; }

        public ImageLoadPolicyData(bool NoRemoteImages, bool NoLowMandatoryLabelImages, bool PreferSystem32Images, bool AuditNoRemoteImages, bool AuditNoLowMandatoryLabelImages)
        {
            this.NoRemoteImages = NoRemoteImages;
            this.NoLowMandatoryLabelImages = NoLowMandatoryLabelImages;
            this.PreferSystem32Images = PreferSystem32Images;
            this.AuditNoRemoteImages = AuditNoRemoteImages;
            this.AuditNoLowMandatoryLabelImages = AuditNoLowMandatoryLabelImages;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (NoRemoteImages)
            {
                sb.Append("No Remote Images");
            }
            if (NoLowMandatoryLabelImages)
            {
                if (sb.Length == 0)
                {
                    sb.Append("No Low Mandatory Label Images");
                }
                else
                {
                    sb.Append(",No Low Mandatory Label Images");
                }
            }
            if (PreferSystem32Images)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Prefer System32 Images");
                }
                else
                {
                    sb.Append(",Prefer System32 Images");
                }
            }
            if (AuditNoRemoteImages)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Audit No Remote Images");
                }
                else
                {
                    sb.Append(",Audit No Remote Images");
                }
            }
            if (AuditNoLowMandatoryLabelImages)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Audit No Low Mandatory Label Images");
                }
                else
                {
                    sb.Append(",Audit No Low Mandatory Label Images");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica di un processo relativa all'isolamento dei side channels.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct SideChannelIsolationPolicyData
    {
        /// <summary>
        /// 
        /// </summary>
        public bool SmtBranchTargetIsolation { get; }
        /// <summary>
        /// Indica se il processo deve essere isolato in un dominio di sicurezza distinto, anche da altri processi in esecuzione nello stesso contesto di sicurezza.
        /// </summary>
        public bool SecurityDomainIsolated { get; }
        /// <summary>
        /// Indica se la combinazione delle pagine per questo processo è disabilitata, questa impostazione non riguarda le pagine comuni.
        /// </summary>
        public bool PageCombineDisabled { get; }
        /// <summary>
        /// Indica se disabilitare la disambiguazione della memoria.
        /// </summary>
        public bool SpeculativeStoreBypassDisabled { get; }

        public SideChannelIsolationPolicyData(bool SmtBranchTargetIsolation, bool SecurityDomainIsolated, bool PageCombineDisabled, bool SpeculativeStoreBypassDisabled)
        {
            this.SmtBranchTargetIsolation = SmtBranchTargetIsolation;
            this.SecurityDomainIsolated = SecurityDomainIsolated;
            this.PageCombineDisabled = PageCombineDisabled;
            this.SpeculativeStoreBypassDisabled = SpeculativeStoreBypassDisabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (SmtBranchTargetIsolation)
            {
                sb.Append("Smt Branch Target Isolation");
            }
            if (SecurityDomainIsolated)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Security Domain Isolated");
                }
                else
                {
                    sb.Append(",Security Domain Isolated");
                }
            }
            if (PageCombineDisabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Page Combine Disabled");
                }
                else
                {
                    sb.Append(",Page Combine Disabled");
                }
            }
            if (SpeculativeStoreBypassDisabled)
            {
                if (sb.Length == 0)
                {
                    sb.Append("Speculative Store Bypass Disabled");
                }
                else
                {
                    sb.Append(",Speculative Store Bypass Disabled");
                }
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }

    /// <summary>
    /// Informazioni sulla politica di un processo relativa alla protezione dello stack eseguita dall'hardware in modalità utente.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Eseguire l'override di Equals e dell'operatore di uguaglianza sui tipi di valore", Justification = "<In sospeso>")]
    public struct UserShadowStackPolicyData
    {
        /// <summary>
        /// Indica se la protezione dello stack eseguita dall'hardware in modalità utente è abilitata.
        /// </summary>
        public bool UserShadowStackEnabled { get; }

        public UserShadowStackPolicyData(bool UserShadowStackEnabled)
        {
            this.UserShadowStackEnabled = UserShadowStackEnabled;
        }

        /// <summary>
        /// Indica se la politica è attiva e quali delle sue impostazioni sono attive.
        /// </summary>
        /// <param name="EnabledSettings">Una stringa con i nomi delle impostazioni attive, è nullo quando la politica non è attiva.</param>
        /// <returns>true se la politica è attiva, false altrimenti.</returns>
        public bool IsPolicyEnabled(out string EnabledSettings)
        {
            StringBuilder sb = new StringBuilder();
            EnabledSettings = null;
            if (UserShadowStackEnabled)
            {
                sb.Append("User Shadow Stack Enabled");
            }
            if (sb.Length > 0)
            {
                EnabledSettings = sb.ToString();
            }
            return !string.IsNullOrWhiteSpace(EnabledSettings);
        }
    }
    #endregion
    /// <summary>
    /// Contiene i dati relativi sui criteri di mitigazione di un processo.
    /// </summary>
    public class ProcessMitigationPoliciesData
    {
        /// <summary>
        /// Informazioni riguardo al criterio Data Execution Prevention.
        /// </summary>
        private readonly DEPPolicyData DEP;

        /// <summary>
        /// Informazioni riguardo al criterio Address Space Randomization Layout
        /// </summary>
        private readonly ASLRPolicyData ASLR;

        /// <summary>
        /// Informazioni rigurado al criterio relativo alla generazione di codice dinamico e alla modifica di codice eseguibile.
        /// </summary>
        private readonly DynamicCodePolicyData DynamicCode;

        /// <summary>
        /// Informazioni riguardo al criterio relativo al comportamento con handle non validi.
        /// </summary>
        private readonly StrictHandleCheckPolicyData StrictHandleCheck;

        /// <summary>
        /// Informazioni sul criterio relativo alle restrizioni su quali system calls possono essere eseguite.
        /// </summary>
        private readonly SystemCallDisablePolicyData SystemCallDisable;

        /// <summary>
        /// Informazioni sul criterio relativo alle extension points di terze parti.
        /// </summary>
        private readonly ExtensionPointDisablePolicyData ExtensionPointsDisable;

        /// <summary>
        /// Informazioni sul criterio Control Flow Guard (CFG).
        /// </summary>
        private readonly CFGPolicyData CFG;

        /// <summary>
        /// Informazioni sul criterio relativo al caricamento di immagini in base alla firma.
        /// </summary>
        private readonly BinarySignaturePolicyData BinarySignature;
        /// <summary>
        /// Informazioni sul criterio relativo al caricamento di font non di sistema.
        /// </summary>
        private readonly FontDisablePolicyData FontDisable;

        /// <summary>
        /// Informazioni sul criterio relativo al caricamento di immagini da un dispositivo remoto.
        /// </summary>
        private readonly ImageLoadPolicyData ImageLoad;

        /// <summary>
        /// Informazioni sul criterio relativo all'isolamento dei side channels.
        /// </summary>
        private readonly SideChannelIsolationPolicyData SideChannelIsolation;

        /// <summary>
        /// Informazioni sul criterio relativo alla protezione dello stack eseguita dall'hardware in modalità utente.
        /// </summary>
        private readonly UserShadowStackPolicyData UserShadowStack;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessMitigationPoliciesData"/> con le informazioni fornite.
        /// </summary>
        /// <param name="DEP">Informazioni sul criterio DEP.</param>
        /// <param name="ASLR">Informazioni sul criterio ASLR.</param>
        /// <param name="DynamicCode">Informazioni sul criterio relativo al codice dinamico.</param>
        /// <param name="StrictHandleCheck">Informazioni sul criterio relativo al trattamento degli handle non validi.</param>
        /// <param name="SystemCallDisable">Informazioni sul criterio relativo alle system call permesse.</param>
        /// <param name="ExtensionPointsDisable">Informazioni sul criterio relativo alle extension points.</param>
        /// <param name="CFG">Informazioni sul criterio CFG.</param>
        /// <param name="BinarySignature">Informazioni sul criterio relativo al caricamento di immagini firmate.</param>
        /// <param name="FontDisable">Informazioni sul criterio relativo al caricamento di font non di sistema.</param>
        /// <param name="ImageLoad">Informazioni sul criterio relativo al caricamento di immagini.</param>
        /// <param name="SideChannelIsolation">Informazioni sul criterio relativo all'isolamento dei side channels.</param>
        /// <param name="UserShadowStack">Informazioni sul criterio relativo alla protezione dello stack eseguita dall'hardware in modalità utente.</param>
        public ProcessMitigationPoliciesData(DEPPolicyData DEP, ASLRPolicyData ASLR, DynamicCodePolicyData DynamicCode, StrictHandleCheckPolicyData StrictHandleCheck, SystemCallDisablePolicyData SystemCallDisable, ExtensionPointDisablePolicyData ExtensionPointsDisable, CFGPolicyData CFG, BinarySignaturePolicyData BinarySignature, FontDisablePolicyData FontDisable, ImageLoadPolicyData ImageLoad, SideChannelIsolationPolicyData SideChannelIsolation, UserShadowStackPolicyData UserShadowStack)
        {
            this.DEP = DEP;
            this.ASLR = ASLR;
            this.DynamicCode = DynamicCode;
            this.StrictHandleCheck = StrictHandleCheck;
            this.SystemCallDisable = SystemCallDisable;
            this.ExtensionPointsDisable = ExtensionPointsDisable;
            this.CFG = CFG;
            this.BinarySignature = BinarySignature;
            this.FontDisable = FontDisable;
            this.ImageLoad = ImageLoad;
            this.SideChannelIsolation = SideChannelIsolation;
            this.UserShadowStack = UserShadowStack;
        }

        /// <summary>
        /// Recupera i criteri attivi e i dati su di essi.
        /// </summary>
        /// <param name="EnabledSettingsDictionary">Dizionazio che contiene informazioni dettagliate sui criteri.</param>
        /// <returns>Una stringa con le informazioni sui criteri attive.</returns>
        public string GetActivePolicies(out Dictionary<string, string> EnabledSettingsDictionary)
        {
            StringBuilder sb = new();
            EnabledSettingsDictionary = new Dictionary<string, string>();
            if (DEP.IsPolicyEnabled(out string EnabledSettings))
            {
                sb.Append("DEP");
                EnabledSettingsDictionary.Add("DEP", EnabledSettings);
            }
            if (ASLR.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("ASLR");
                }
                else
                {
                    sb.Append(", ASLR");
                }
                EnabledSettingsDictionary.Add("ASLR", EnabledSettings);
            }
            if (DynamicCode.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("Dynamic Code");
                }
                else
                {
                    sb.Append(", Dynamic Code");
                }
                EnabledSettingsDictionary.Add("DynamicCode", EnabledSettings);
            }
            if (StrictHandleCheck.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("Strict Handle Check");
                }
                else
                {
                    sb.Append(", Strict Handle Check");
                }
                EnabledSettingsDictionary.Add("StrictHandleCheck", EnabledSettings);
            }
            if (SystemCallDisable.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("System Call Disable");
                }
                else
                {
                    sb.Append(", System Call Disable");
                }
                EnabledSettingsDictionary.Add("SystemCallDisable", EnabledSettings);
            }
            if (ExtensionPointsDisable.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("Extension Points Disable");
                }
                else
                {
                    sb.Append(", Extension Points Disable");
                }
                EnabledSettingsDictionary.Add("ExtensionPointsDisable", EnabledSettings);
            }
            if (CFG.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("CFG");
                }
                else
                {
                    sb.Append(", CFG");
                }
                EnabledSettingsDictionary.Add("CFG", EnabledSettings);
            }
            if (BinarySignature.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("Binary Signature");
                }
                else
                {
                    sb.Append(", Binary Signature");
                }
                EnabledSettingsDictionary.Add("BinarySignature", EnabledSettings);
            }
            if (FontDisable.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("Font Disable");
                }
                else
                {
                    sb.Append(", Font Disable");
                }
                EnabledSettingsDictionary.Add("FontDisable", EnabledSettings);
            }
            if (ImageLoad.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("Image Load");
                }
                else
                {
                    sb.Append(", Image Load");
                }
                EnabledSettingsDictionary.Add("ImageLoad", EnabledSettings);
            }
            if (SideChannelIsolation.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("Side Channel Isolation");
                }
                else
                {
                    sb.Append(", Side Channel Isolation");
                }
                EnabledSettingsDictionary.Add("SideChannelIsolation", EnabledSettings);
            }
            if (UserShadowStack.IsPolicyEnabled(out EnabledSettings))
            {
                if (sb.Length == 0)
                {
                    sb.Append("User Shadow Stack");
                }
                else
                {
                    sb.Append(", User Shadow Stack");
                }
                EnabledSettingsDictionary.Add("UserShadowStack", EnabledSettings);
            }
            if (sb.Length > 0)
            {
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}