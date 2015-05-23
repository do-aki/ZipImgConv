using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipImgConv
{
    public class ConvertTargetList : ObservableCollection<ConvertTarget>
    {
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
