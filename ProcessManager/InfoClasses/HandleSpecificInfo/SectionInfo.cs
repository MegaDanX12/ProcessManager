
namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioni su una sezione.
    /// </summary>
    public class SectionInfo
    {
        /// <summary>
        /// Informazioni di base.
        /// </summary>
        public SectionBasicInfo BasicInfo { get; }

        /// <summary>
        /// Informazioni sull'immagine a cui la sezione si riferisce.
        /// </summary>
        public SectionImageInfo ImageInfo { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SectionInfo"/>.
        /// </summary>
        /// <param name="BasicInfo">Informazioni di base.</param>
        /// <param name="ImageInfo">Informazioni sull'immagine associata.</param>
        public SectionInfo(SectionBasicInfo BasicInfo, SectionImageInfo ImageInfo)
        {
            this.BasicInfo = BasicInfo;
            this.ImageInfo = ImageInfo;
        }
    }
}