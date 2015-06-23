using System.Linq;
using System.Text;

namespace NuFridge.Shared.Server.Storage
{
    public class ReferenceCollectionReaderWriter : PropertyReaderWriterDecorator
    {
        public ReferenceCollectionReaderWriter(IPropertyReaderWriter<object> original)
            : base(original)
        {
        }

        public override object Read(object target)
        {
            ReferenceCollection referenceCollection = base.Read(target) as ReferenceCollection;
            if (referenceCollection == null || referenceCollection.Count == 0)
                return "";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("|");
            foreach (string str in referenceCollection)
            {
                stringBuilder.Append(str);
                stringBuilder.Append("|");
            }
            return stringBuilder.ToString();
        }

        public override void Write(object target, object value)
        {
            string[] strArray = (value ?? string.Empty).ToString().Split('|');
            ReferenceCollection referenceCollection = base.Read(target) as ReferenceCollection;
            if (referenceCollection == null)
                base.Write(target, referenceCollection = new ReferenceCollection());
            referenceCollection.ReplaceAll(strArray.Where(v => !string.IsNullOrWhiteSpace(v)));
        }
    }
}
