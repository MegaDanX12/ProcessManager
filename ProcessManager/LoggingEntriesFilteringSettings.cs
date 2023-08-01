using System;
using System.Collections.ObjectModel;

namespace ProcessManager
{
    /// <summary>
    /// Contiene i filtri per la lista delle voci di log.
    /// </summary>
    public class LoggingEntriesFilteringSettings
    {
        /// <summary>
        /// Lista originale.
        /// </summary>
        private readonly ObservableCollection<LogEntry> OriginalCollection;

        /// <summary>
        /// Indica se il filtro del testo è attivo.
        /// </summary>
        public bool TextFilter { get; set; }

        /// <summary>
        /// Valore del filtro del testo.
        /// </summary>
        public string TextFilterValue { get; set; }

        /// <summary>
        /// Indica se il filtro della data è attivo.
        /// </summary>
        public bool DateFilter { get; set; }

        /// <summary>
        /// Valore del filtro della data.
        /// </summary>
        public DateTime? DateFilterValue { get; set; }

        /// <summary>
        /// Indica se il filtro dell'ora è attivo.
        /// </summary>
        public bool HourFilter { get; set; }

        /// <summary>
        /// Valore del filtro dell'ora (ora d'inizio).
        /// </summary>
        public DateTime? StartHourFilterValue { get; set; }

        /// <summary>
        /// Valore del filtro dell'ora (ora di fine).
        /// </summary>
        public DateTime? EndHourFilterValue { get; set; }

        /// <summary>
        /// Indica se il filtro della gravità dell'evento è attivo.
        /// </summary>
        public bool SeverityFilter { get; set; }

        /// <summary>
        /// Valore del filtro della gravità dell'evento.
        /// </summary>
        public EventSeverity? SeverityFilterValue { get; set; }

        /// <summary>
        /// Indica se il filtro della fonte dell'evento.
        /// </summary>
        public bool SourceFilter { get; set; }

        /// <summary>
        /// Valore del filtro della fonte dell'evento.
        /// </summary>
        public EventSource? EventSourceValue { get; set; }

        /// <summary>
        /// Indica se il filtro dell'azione indicata nell'evento è attivo.
        /// </summary>
        public bool ActionFilter { get; set; }

        /// <summary>
        /// Valore del filtro dell'azione indicata nell'evento.
        /// </summary>
        public EventAction? ActionFilterValue { get; set; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="LoggingEntriesFilteringSettings"/>.
        /// </summary>
        /// <param name="Entries">Lista con le voci.</param>
        public LoggingEntriesFilteringSettings(ObservableCollection<LogEntry> Entries)
        {
            OriginalCollection = Entries;
        }

        /// <summary>
        /// Applica i filtri alla lista.
        /// </summary>
        public ObservableCollection<LogEntry> ApplyFilters(ObservableCollection<LogEntry> Entries)
        {
            ObservableCollection<LogEntry> NewCollection = new();
            if (DateFilter)
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].Date.Date == DateFilterValue.Value.Date)
                    {
                        NewCollection.Add(Entries[i]);
                    }
                }
            }
            if (HourFilter)
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].Date.Hour >= StartHourFilterValue.Value.Hour && Entries[i].Date.Hour <= EndHourFilterValue.Value.Hour)
                    {
                        if (Entries[i].Date.Hour == StartHourFilterValue.Value.Hour && Entries[i].Date.Minute < StartHourFilterValue.Value.Minute)
                        {
                            NewCollection.Add(Entries[i]);
                        }
                        else if (Entries[i].Date.Hour == EndHourFilterValue.Value.Hour && Entries[i].Date.Minute > EndHourFilterValue.Value.Minute)
                        {
                            NewCollection.Add(Entries[i]);
                        }
                        else
                        {
                            NewCollection.Add(Entries[i]);
                        }
                    }
                }
            }
            if (SeverityFilter)
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].Severity == SeverityFilterValue.Value)
                    {
                        NewCollection.Add(Entries[i]);
                    }
                }
            }
            if (SourceFilter)
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].Source == EventSourceValue.Value)
                    {
                        NewCollection.Add(Entries[i]);
                    }
                }
            }
            if (ActionFilter)
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].Action == ActionFilterValue.Value)
                    {
                        NewCollection.Add(Entries[i]);
                    }
                }
            }
            if (TextFilter)
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].Text.Contains(TextFilterValue))
                    {
                        NewCollection.Add(Entries[i]);
                    }
                }
            }
            return NewCollection;
        }

        /// <summary>
        /// Resetta i filtri applicati sulla lista.
        /// </summary>
        public ObservableCollection<LogEntry> ResetFilters()
        {
            return OriginalCollection;
        }
    }
}