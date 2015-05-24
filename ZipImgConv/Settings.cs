using SharpCompress.Compressor.Deflate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZipImgConv
{
    [DataContract]
    public class Settings : NotifyPropertyChangedImpl
    {
        private int concurrency = 2;

        [DataMember]
        public int Concurrency
        {
            get { return concurrency; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("1以上を指定してください");
                }
                setProperty(ref concurrency, value);
            }
        }

        private ThreadPriority priority = ThreadPriority.Normal;

        [DataMember]
        public ThreadPriority Priority
        {
            get { return priority; }
            set { setProperty(ref priority, value); }
        }

        private int width = 600;

        [DataMember]
        public int Width
        {
            get { return width; }
            set { setProperty(ref width, value); }
        }

        private int height = 800;

        [DataMember]
        public int Height
        {
            get { return height; }
            set { setProperty(ref height, value); }
        }

        private int quality = 75;

        [DataMember]
        public int Quality
        {
            get { return quality; }
            set { setProperty(ref quality, value); }
        }

        private string fileNameTemplate = "new_%f.zip";

        [DataMember]
        public string FileNameTemplate
        {
            get { return fileNameTemplate; }
            set { setProperty(ref fileNameTemplate, value); }
        }

        private CompressionLevel compressionLevel = CompressionLevel.BestCompression;

        [DataMember]
        public CompressionLevel CompressionLevel
        {
            get { return compressionLevel; }
            set { setProperty(ref compressionLevel, value); }
        }

        public Settings()
        {
        }

        public Settings(Settings settings)
        {
            this.width = settings.width;
            this.height = settings.height;
            this.quality = settings.quality;
            this.fileNameTemplate = settings.fileNameTemplate;
            this.compressionLevel = settings.compressionLevel;
            this.concurrency = settings.concurrency;
            this.priority = settings.priority;
        }

        public void WriteAsJson(Stream stream)
        {
            var serializer = new DataContractJsonSerializer(typeof(Settings));
            serializer.WriteObject(stream, this);
        }

        public void WriteToJsonFile(string filename)
        {
            using (var f = new FileStream(filename, FileMode.Create))
            {
                this.WriteAsJson(f);
            }
        }

        public Array PriorityList
        {
            get
            {
                return Enum.GetValues(typeof(ThreadPriority));
            }
        }

        public Array CompressionLevelList
        {
            get
            {
                return new CompressionLevel[] {
                    CompressionLevel.Level0,
                    CompressionLevel.Level1,
                    CompressionLevel.Level2,
                    CompressionLevel.Level3,
                    CompressionLevel.Level4,
                    CompressionLevel.Level5,
                    CompressionLevel.Level6,
                    CompressionLevel.Level7,
                    CompressionLevel.Level8,
                    CompressionLevel.Level9,
                };
            }
        }

        public static Settings LoadFromJson(Stream stream)
        { 
            var serializer = new DataContractJsonSerializer(typeof(Settings));
            return (Settings)serializer.ReadObject(stream);        
        }


        public static Settings LoadFromJsonFile(string filename)
        {
            using (var f = new FileStream(filename, FileMode.Open))
            {
                return LoadFromJson(f);
            }
        }
    }
}
