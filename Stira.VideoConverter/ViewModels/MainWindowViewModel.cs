using Emgu.CV;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Stira.VideoConverter.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";

        public MainWindowViewModel()
        {
            OpenFileCommand = new DelegateCommand(OpenFile);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public int CurrentFile { get; set; }
        public int TotalFiles { get; set; }
        public int CurrentProgress { get; set; }
        public ICommand OpenFileCommand { get; set; }

        private async Task ConvertAndSave(string filePath)
        {
            VideoCapture videoCapture = new VideoCapture(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var filePathWithoutName = Path.GetDirectoryName(filePath);
            var width = (int)videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
            var height = (int)videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);
            Mat frame = new Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
            VideoWriter videoWriter = new VideoWriter(filePathWithoutName + "\\" + fileName + "_converted.mp4",
                                                      //VideoWriter.Fourcc('M', 'P', '4', '2'),
                                                      VideoWriter.Fourcc('H', 'E', 'V', 'C'),
                                                      30,
                                                      new System.Drawing.Size(width, height),
                                                      true);
            var totalFrames = (int)videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
            int frameCounter = 0;
            CurrentProgress = 0;
            int previousProgress = 0;
            await Task.Run(() =>
            {
                while (videoCapture.Read(frame))
                {
                    videoWriter.Write(frame);
                    //if (frameCounter++ % 100 == 0)
                    //{
                    //    CurrentProgress++;
                    //}
                    var currentProgress = ((float)frameCounter++ / totalFrames) * 100;
                    if (currentProgress > previousProgress + 1)
                    {
                        CurrentProgress = (int)((float)frameCounter / totalFrames) * 100;
                        previousProgress = CurrentProgress;
                    }
                }
            }).ConfigureAwait(false);
            videoWriter.Dispose();
        }

        private async void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true
            };
            CurrentFile = 0;
            if (openFileDialog.ShowDialog() == true)
            {
                TotalFiles = openFileDialog.FileNames.Length;
                foreach (var fileName in openFileDialog.FileNames)
                {
                    CurrentFile++;
                    await ConvertAndSave(fileName).ConfigureAwait(false);
                }
            }
        }
    }
}