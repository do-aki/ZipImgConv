using System.Collections.ObjectModel;
using System.Linq;

namespace ZipImgConv
{
    public class ConvertTargetList : ObservableCollection<ConvertTarget>
    {
        public void ClearDone()
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (this[i].Status == ConvertTarget.TargetStatus.Done)
                {
                    this.RemoveAt(i);
                }
            }
        }

        protected override void InsertItem(int index, ConvertTarget item)
        {
            if (detectSame(item))
            {
                return;
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ConvertTarget item)
        {
            if (detectSame(item))
            {
                return;
            }
            base.SetItem(index, item);
        }

        private bool detectSame(ConvertTarget t)
        {
            return this.Items.Any(
                i =>
                {
                    return i.FileName == t.FileName;
                }
            );
        }

    }
}
