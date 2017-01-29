using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ZipImgConv
{
    [DataContract]
    public class NotifyPropertyChangedImpl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void setProperty<Type>(ref Type propery, Type value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(propery, value))
            {
                return;
            }

            propery = value;
            onPropertyChanged(propertyName);
        }

        protected void onPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
