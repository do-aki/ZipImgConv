using ImageMagick;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Writer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZipImgConv
{
    public class Worker
    {
        private CompressionInfo compressionInfo;
        private MagickGeometry magickGeometry;
        private int quality;
        private string fileNameTemplate;
        private ConvertTargetList convertTargetList;
        private int concurrency;
        private ThreadPriority priority;

        public Worker(ConvertTargetList convertTargetList, Settings settings) 
        {
            compressionInfo = new CompressionInfo();
            compressionInfo.Type = SharpCompress.Common.CompressionType.Deflate;
            compressionInfo.DeflateCompressionLevel = settings.CompressionLevel;

            magickGeometry = new MagickGeometry(settings.Width, settings.Height);
            quality = settings.Quality;

            fileNameTemplate = settings.FileNameTemplate;

            concurrency = settings.Concurrency;
            priority = settings.Priority;

            this.convertTargetList = convertTargetList;
        }

        public async Task Convert(CancellationToken cancellationToken)
        {
            using (var semaphore = new SemaphoreSlim(initialCount: concurrency, maxCount: concurrency))
            {
                var tasks = new List<Task>();

                for (var i = 0; i < convertTargetList.Count; ++i)
                {
                    var target = convertTargetList[i];
                    if (target.Status == ConvertTarget.TargetStatus.Done)
                    {
                        continue;
                    }

                    try
                    {
                        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    var task = Task.Run(
                        _convert(target, cancellationToken)
                    ).ContinueWith(_t => semaphore.Release());

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private Action _convert(ConvertTarget t, CancellationToken cancellationToken)
        {
            return () =>
            {
                Thread.CurrentThread.Priority = priority;

                var write_file = buildWriteFileName(t.FileName, fileNameTemplate);
                if (File.Exists(write_file))
                {
                    t.Message = "書き込み先にファイルが存在するため中断しました";
                    return;
                }

                t.Status = ConvertTarget.TargetStatus.Prosessing;
                t.Message = String.Empty;

                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    if (this.convert(t, write_file, cancellationToken))
                    {
                        sw.Stop();
                        t.Status = ConvertTarget.TargetStatus.Done;
                        t.Message = processTimeFormat(sw.Elapsed);
                    }
                    else
                    {
                        this.cleanUp(write_file);
                        t.Status = ConvertTarget.TargetStatus.Ready;
                        t.Message = "中断されました";
                    }
                }
                catch (InvalidOperationException)
                {
                    t.Status = ConvertTarget.TargetStatus.Done;
                    t.Message = "サポートされないファイル形式です";
                }
                catch (FileNotFoundException)
                {
                    t.Status = ConvertTarget.TargetStatus.Ready;
                    t.Message = "ファイルが見つかりませんでした";
                }
                catch (Exception e)
                {
                    t.Status = ConvertTarget.TargetStatus.Ready;
                    t.Message = "想定外のエラーです (" + e.Message + ")";
                }
            };
        }

        private string processTimeFormat(TimeSpan ts)
        {
            return String.Format(
                "{0:00}:{1:00}",
                Math.Floor(ts.TotalMinutes), 
                ts.Seconds
            );
        }

        private void cleanUp(string write_file)
        {
            if (File.Exists(write_file))
            {
                File.Delete(write_file);
            }
        }

        private bool convert(ConvertTarget convertTarget, string write_file, CancellationToken cancellationToken)
        {
            int entry_count = countEntry(convertTarget.FileName);

            using (var wstream = File.OpenWrite(write_file))
            using (var writer = WriterFactory.Open(wstream, SharpCompress.Common.ArchiveType.Zip, compressionInfo))
            using (var rstream = File.OpenRead(convertTarget.FileName))
            using (var reader = ReaderFactory.Open(rstream))
            {
                int converted = 0;
                while (reader.MoveToNextEntry())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    if (reader.Entry.IsDirectory)
                    {
                        continue;
                    }

                    convertTarget.Progress = (int)((double)converted / entry_count * 100);
                    convertTarget.Message = string.Format("{0,4:D} / {1,4:D}", converted, entry_count);

                    convertFile(reader, writer);
                    ++converted;
                }
            }

            return true;
        }

        private void convertFile(IReader reader, IWriter writer)
        {
            using (var original = new MemoryStream())
            {
                using (var stm = reader.OpenEntryStream())
                {
                    stm.CopyTo(original);
                }

                original.Seek(0, SeekOrigin.Begin);
                if (original.Length == 0)
                {
                    // empty file
                }

                try
                {
                    using (var resized = new MemoryStream())
                    using (MagickImage image = new MagickImage(original))
                    {
                        var g = magickGeometry;

                        if ((g.Width < g.Height && image.Height < image.Width)
                            || (g.Height < g.Width && image.Width < image.Height))
                        {
                            g = new MagickGeometry(g.Height, g.Width);
                        }

                        image.Quality = quality;
                        image.Resize(g);
                        image.Write(resized);
                        image.Dispose();

                        resized.Seek(0, SeekOrigin.Begin);
                        writer.Write(reader.Entry.FilePath, resized);
                    }
                }
                catch (MagickException)
                {
                    original.Seek(0, SeekOrigin.Begin);
                    writer.Write(reader.Entry.FilePath, original);
                }
            }
        }

        private string buildWriteFileName(string filename, string template)
        {
            var d = Path.GetDirectoryName(filename);
            var f = Path.GetFileNameWithoutExtension(filename);
            
            return Path.Combine(d, template.Replace("%f", f));
        }

        private int countEntry(string filename)
        {
            int entry_count = 0;
            using (var stream = File.OpenRead(filename))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        ++entry_count;
                    }
                }
            }

            return entry_count;
        }

    }

}
