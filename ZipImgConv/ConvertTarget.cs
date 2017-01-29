using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace ZipImgConv
{
    public class ConvertTarget : NotifyPropertyChangedImpl
    {
        public enum TargetStatus
        {
            Ready,
            Prosessing,
            Done,
        }

        private string fileName;
        public string FileName {
            get { return fileName; }
            set
            {
                setProperty(ref fileName, value);
            }
        }

        private TargetStatus status;
        public TargetStatus Status
        {
            get { return status; }
            set
            {
                setProperty(ref status, value);
            }
        }

        private int progress;
        public int Progress
        {
            get { return progress;  }
            set
            {
                setProperty(ref progress, value);
            }
        }

        private string message;
        public string Message {
            get { return message; }
            set
            {
                setProperty(ref message, value);
            }
        }
    }

    /// <summary>
    /// ConvertTarget.Status によって、ProgressBar を表示させるかどうかの boolean に変換するコンバータ
    /// </summary>
    public class ProgressBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as ConvertTarget.TargetStatus?;

            if (status.HasValue && status.Value == ConvertTarget.TargetStatus.Prosessing)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var file = value as string;
            if (string.IsNullOrEmpty(file))
            {
                return file;
            }
            else
            {
                return Path.GetFileName(file);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
