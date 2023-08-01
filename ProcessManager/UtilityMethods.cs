using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ProcessManager
{
    /// <summary>
    /// Contiene metodi di utilità.
    /// </summary>
    public static class UtilityMethods
    {
        /// <summary>
        /// Descrizioni di valori di enumerazione.
        /// </summary>
        private static readonly Dictionary<Enum, string> EnumDescriptions = new();

        /// <summary>
        /// Recupera l'indice di una riga che l'utente ha cliccato.
        /// </summary>
        /// <param name="OriginalSource">Oggetto originale che ha generato l'evento.</param>
        /// <returns>L'indice della riga.</returns>
        public static int GetRowIndexFromMouseClick(object OriginalSource)
        {
            DependencyObject dep = (DependencyObject)OriginalSource;
            while (dep is not null and not DataGridCell and not DataGridColumnHeader)
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null)
            {
                return -1;
            }
            else if (dep is DataGridCell)
            {
                while (dep is not null and not DataGridRow)
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                DataGridRow Row = dep as DataGridRow;
                DataGrid Grid = ItemsControl.ItemsControlFromItemContainer(Row) as DataGrid;
                return Grid.ItemContainerGenerator.IndexFromContainer(Row);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Converte un valore che indica una dimensione (in byte) in stringa.
        /// </summary>
        /// <param name="Value">Valore da convertire.</param>
        /// <returns>Stringa con il valore convertito.</returns>
        public static string ConvertSizeValueToString(ulong Value)
        {
            double CalculatedValue;
            if (Value is >= 1048576 and < 1073741824)
            {
                CalculatedValue = (double)Value / 1024 / 1024;
                return CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
            }
            else if (Value >= 1073741824)
            {
                CalculatedValue = (double)Value / 1024 / 1024 / 1024;
                return CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
            }
            else if (Value is < 1048576 and >= 1024)
            {
                CalculatedValue = (double)Value / 1024;
                return CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
            }
            else
            {
                return Value.ToString("N2", CultureInfo.CurrentCulture) + " B";
            }
        }

        /// <summary>
        /// Converte un valore che indica una dimensione (in KB) in stringa.
        /// </summary>
        /// <param name="Value">Valore da convertire.</param>
        /// <returns>Stringa con il valore convertito.</returns>
        public static string ConvertSizeValueToString2(ulong Value)
        {
            double CalculatedValue;
            if (Value is >= 1024 and < 1048576)
            {
                CalculatedValue = (double)Value / 1024;
                return CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
            }
            else if (Value >= 1048576)
            {
                CalculatedValue = (double)Value / 1024 / 1024;
                return CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
            }
            else
            {
                return Value.ToString("N2", CultureInfo.CurrentCulture) + " B";
            }
        }

        /// <summary>
        /// Converte un conteggio di pagine in memoria in una stringa.
        /// </summary>
        /// <param name="PageSize">Dimensione di una singola pagina.</param>
        /// <param name="PageCount">Numero di pagine.</param>
        /// <returns>Una stringa che indica il numero di pagine seguito dalla loro dimensione totale.</returns>
        public static string PagesCountToString(ulong PageSize, ulong PageCount)
        {
            ulong PagesSize = PageSize * PageCount;
            string SizeString = ConvertSizeValueToString(PagesSize);
            return PageCount.ToString("N0", CultureInfo.CurrentCulture) + " (" + SizeString + ")";
        }

        /// <summary>
        /// Recupera la descrizione di un membro di una enumerazione.
        /// </summary>
        /// <param name="Enumeration">Membro di cui recuperare la descrizione.</param>
        /// <returns>La descrizione, il valore di ritorno è nullo se il valore enumerativo non ha una descrizione.</returns>
        public static string GetEnumDescription(Enum Enumeration)
        {
            if (EnumDescriptions.ContainsKey(Enumeration))
            {
                return EnumDescriptions[Enumeration];
            }
            else
            {
                Type EnumType = Enumeration.GetType();
                MemberInfo[] Members = EnumType.GetMember(Enumeration.ToString());
                if (Members.Length > 0)
                {
                    DescriptionAttribute Attribute = (DescriptionAttribute)Members[0].GetCustomAttribute(typeof(DescriptionAttribute));
                    if (Attribute != null)
                    {
                        EnumDescriptions.Add(Enumeration, Attribute.Description);
                        return Attribute.Description;
                    }
                    else
                    {
                        return Enum.GetName(EnumType, Enumeration);
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Costruisce una stringa che rappresenta l'affinità di un processo.
        /// </summary>
        /// <param name="AffinityValue">Valore di affinità.</param>
        /// <returns>La stringa di affinità derivante dal parametro <paramref name="AffinityValue"/>.</returns>
        public static string BuildAffinityString(ulong AffinityValue)
        {
            BitArray Bits = new(BitConverter.GetBytes(AffinityValue));
            StringBuilder sb = new();
            for (int i = 0; i < Bits.Count; i++)
            {
                if (Bits[i])
                {
                    _ = sb.Append(i.ToString("D0", CultureInfo.CurrentCulture) + ",");
                }
            }
            string AffinityString = sb.ToString();
            AffinityString = AffinityString.Remove(AffinityString.LastIndexOf(','));
            return AffinityString;
        }

        /// <summary>
        /// Costruisce il valore di affinità del processore.
        /// </summary>
        /// <param name="AffinityString">Stringa con le informazioni necessarie a costruire il valore di affinità.</param>
        /// <returns>Un valore a 64 bit che rappresenta l'affinità del processore.</returns>
        public static ulong GetAffinityValue(string AffinityString)
        {
            BitArray AffinityBits = new(64);
            string[] Cores = AffinityString.Split(',');
            foreach (string core in Cores)
            {
                if (core.Contains("-"))
                {
                    string[] CoreRangeComponents = core.Split('-');
                    uint CoreRangeStart = uint.Parse(CoreRangeComponents[0], CultureInfo.CurrentCulture);
                    uint CoreRangeEnd = uint.Parse(CoreRangeComponents[1], CultureInfo.CurrentCulture);
                    AffinityBits[(int)CoreRangeStart] = true;
                    AffinityBits[(int)CoreRangeEnd] = true;
                    for (int i = (int)CoreRangeStart + 1; i < CoreRangeEnd; i++)
                    {
                        AffinityBits[i] = true;
                    }
                }
                else
                {
                    uint Core = uint.Parse(core, CultureInfo.CurrentCulture);
                    AffinityBits[(int)Core] = true;
                }
            }
            byte[] AffinityValueBytes = new byte[8];
            AffinityBits.CopyTo(AffinityValueBytes, 0);
            return BitConverter.ToUInt64(AffinityValueBytes, 0);
        }
    }
}