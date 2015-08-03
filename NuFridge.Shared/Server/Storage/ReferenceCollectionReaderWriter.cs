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

        public override bool Read(object target, out object value)
        {
            object objReferenceCollection;

            if (!base.Read(target, out objReferenceCollection))
            {
                value = "";
                return false;
            }

            ReferenceCollection referenceCollection = objReferenceCollection as ReferenceCollection;
            if (referenceCollection == null || referenceCollection.Count == 0)
            {
                value = "";
                return false;
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("|");
            foreach (string str in referenceCollection)
            {
                stringBuilder.Append(str);
                stringBuilder.Append("|");
            }

            value = stringBuilder.ToString();
            return true;
        }

        public override void Write(object target, object value)
        {
            object objReferenceCollection;

            if (!base.Read(target, out objReferenceCollection))
            {
                objReferenceCollection = null;
            }

            string[] strArray = (value ?? string.Empty).ToString().Split('|');
            ReferenceCollection referenceCollection = objReferenceCollection as ReferenceCollection;
            if (referenceCollection == null)
                base.Write(target, referenceCollection = new ReferenceCollection());
            referenceCollection.ReplaceAll(strArray.Where(v => !string.IsNullOrWhiteSpace(v)));
        }
    }
}
