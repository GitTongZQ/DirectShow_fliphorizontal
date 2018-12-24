using DirectShowLib;
using UnmanagedMemory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace DirectShowCameraDemo
{
    public partial class Form1 : Form, ISampleGrabberCB
    {
        #region DirectShow Parameters
        private int _previewWidth = 640;
        private int _previewHeight = 480;
        private int _previewStride = 0;
        private int _previewFPS = 30;
        IVideoWindow videoWindow = null;
        IMediaControl mediaControl = null;
        IFilterGraph2 graphBuilder = null;
        ICaptureGraphBuilder2 captureGraphBuilder = null;
        DsROTEntry rot = null;
        #endregion

        public Form1()
        {
            InitializeComponent();

            ImgCbShow.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        #region interface implement for ISampleGrabberCB
        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            throw new NotImplementedException();
        }
        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            //For ImgCbShow
            Bitmap tmpCallBack = new Bitmap(_previewWidth, _previewHeight, _previewStride,PixelFormat.Format24bppRgb, pBuffer);
            tmpCallBack.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            ImgCbShow.Image = tmpCallBack.Clone(new Rectangle(0, 0, tmpCallBack.Width, tmpCallBack.Height), tmpCallBack.PixelFormat);

            //For ImgSrcShow (Can't used for rotating 90 or 270)
            Bitmap tmpSrc = new Bitmap(_previewWidth, _previewHeight, _previewStride, PixelFormat.Format24bppRgb, pBuffer);
            tmpSrc.RotateFlip(RotateFlipType.RotateNoneFlipX);
            int step;
            int iWidth = tmpSrc.Width;
            int iHeight = tmpSrc.Height;
            Rectangle rect = new Rectangle(0, 0, tmpSrc.Width, tmpSrc.Height);
            BitmapData bmpData = tmpSrc.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr iPtr = bmpData.Scan0;
            MemoryCtrl.MemoryCopy(pBuffer, BufferLen, iPtr);
            step = bmpData.Stride;
            int iBytes = step * iHeight;
            //byte[] PixelValues = new byte[iBytes];
            //Marshal.Copy(iPtr, PixelValues, 0, iBytes);
            //Marshal.Copy(PixelValues, 0, pBuffer, iBytes);
            tmpSrc.UnlockBits(bmpData);

            return 0;
        }
        #endregion

        #region DirectShow functions
        private void StartCamera()
        {
            // Get connected devices
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (devices.Length == 0)
            {
                MessageBox.Show("No USB webcam connected");
                CtrlCamera.Text = "StartCamera";
                return;
            }
            else
            {
                //DsDevice dev = devices[0] as DsDevice;
                //MessageBox.Show("Device: " + dev.Name);
                CaptureVideo();
            }
        }

        private void StopCamera()
        {
            CtrlCamera.Text = "StartCamera";
            CloseInterfaces();
            ImgCbShow.Image = null;
            ImgSrcShow.Image = null;
        }

        private void ReadBarcode(Bitmap bitmap)
        {
            // Read barcodes with Dynamsoft Barcode Reader
            //Stopwatch sw = Stopwatch.StartNew();
            //sw.Start();
            //BarcodeResult[] results = _barcodeReader.DecodeBitmap(bitmap);
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms");
            //bitmap.Dispose();

            //// Clear previous results
            //textBox1.Clear();

            //if (results == null)
            //{
            //    textBox1.Text = "No barcode detected!";
            //    return;
            //}

            //// Display barcode results
            //foreach (BarcodeResult result in results)
            //{
            //    textBox1.AppendText(result.BarcodeText + "\n");
            //    textBox1.AppendText("\n");
            //}
        }

        public void CaptureVideo()
        {
            ImgSrcShow.Image = null;
            int hr = 0;
            IBaseFilter sourceFilter = null;
            ISampleGrabber sampleGrabber = null;

            try
            {
                // Get DirectShow interfaces
                GetInterfaces();

                // Attach the filter graph to the capture graph
                hr = this.captureGraphBuilder.SetFiltergraph(this.graphBuilder);
                DsError.ThrowExceptionForHR(hr);

                // Use the system device enumerator and class enumerator to find
                // a video capture/preview device, such as a desktop USB video camera.
                sourceFilter = FindCaptureDevice();
                // Add Capture filter to graph.
                hr = this.graphBuilder.AddFilter(sourceFilter, "Video Capture");
                DsError.ThrowExceptionForHR(hr);

                // Initialize SampleGrabber.
                sampleGrabber = new SampleGrabber() as ISampleGrabber;
                // Configure SampleGrabber. Add preview callback.
                ConfigureSampleGrabber(sampleGrabber);
                // Add SampleGrabber to graph.
                hr = this.graphBuilder.AddFilter(sampleGrabber as IBaseFilter, "Frame Callback");
                DsError.ThrowExceptionForHR(hr);

                // Configure preview settings.
                SetConfigParams(this.captureGraphBuilder, sourceFilter, _previewFPS, _previewWidth, _previewHeight);

                // Render the preview
                hr = this.captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Video, sourceFilter, (sampleGrabber as IBaseFilter), null);
                DsError.ThrowExceptionForHR(hr);

                SaveSizeInfo(sampleGrabber);

                // Set video window style and position
                SetupVideoWindow();

                // Add our graph to the running object table, which will allow
                // the GraphEdit application to "spy" on our graph
                rot = new DsROTEntry(this.graphBuilder);

                // Start previewing video data
                hr = this.mediaControl.Run();
                DsError.ThrowExceptionForHR(hr);
            }
            catch
            {
                MessageBox.Show("An unrecoverable error has occurred.");
            }
            finally
            {
                if (sourceFilter != null)
                {
                    Marshal.ReleaseComObject(sourceFilter);
                    sourceFilter = null;
                }

                if (sampleGrabber != null)
                {
                    Marshal.ReleaseComObject(sampleGrabber);
                    sampleGrabber = null;
                }
            }
        }

        public IBaseFilter FindCaptureDevice()
        {
            int hr = 0;
#if USING_NET11
            UCOMIEnumMoniker classEnum = null;
            UCOMIMoniker[] moniker = new UCOMIMoniker[1];
#else
            IEnumMoniker classEnum = null;
            IMoniker[] moniker = new IMoniker[1];
#endif
            object source = null;

            // Create the system device enumerator
            ICreateDevEnum devEnum = (ICreateDevEnum)new CreateDevEnum();

            // Create an enumerator for the video capture devices
            hr = devEnum.CreateClassEnumerator(FilterCategory.VideoInputDevice, out classEnum, 0);
            DsError.ThrowExceptionForHR(hr);

            // The device enumerator is no more needed
            Marshal.ReleaseComObject(devEnum);

            // If there are no enumerators for the requested type, then 
            // CreateClassEnumerator will succeed, but classEnum will be NULL.
            if (classEnum == null)
            {
                throw new ApplicationException("No video capture device was detected.\r\n\r\n" +
                                               "This sample requires a video capture device, such as a USB WebCam,\r\n" +
                                               "to be installed and working properly.  The sample will now close.");
            }

            // Use the first video capture device on the device list.
            // Note that if the Next() call succeeds but there are no monikers,
            // it will return 1 (S_FALSE) (which is not a failure).  Therefore, we
            // check that the return code is 0 (S_OK).
#if USING_NET11
            int i;
            if (classEnum.Next (moniker.Length, moniker, IntPtr.Zero) == 0)
#else
            if (classEnum.Next(moniker.Length, moniker, IntPtr.Zero) == 0)
#endif
            {
                // Bind Moniker to a filter object
                Guid iid = typeof(IBaseFilter).GUID;
                moniker[0].BindToObject(null, null, ref iid, out source);
            }
            else
            {
                throw new ApplicationException("Unable to access video capture device!");
            }

            // Release COM objects
            Marshal.ReleaseComObject(moniker[0]);
            Marshal.ReleaseComObject(classEnum);

            // An exception is thrown if cast fail
            return (IBaseFilter)source;
        }

        public void GetInterfaces()
        {
            int hr = 0;

            // An exception is thrown if cast fail
            this.graphBuilder = (IFilterGraph2)new FilterGraph();
            this.captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
            this.mediaControl = (IMediaControl)this.graphBuilder;
            this.videoWindow = (IVideoWindow)this.graphBuilder;
            DsError.ThrowExceptionForHR(hr);
        }

        public void CloseInterfaces()
        {
            if (mediaControl != null)
            {
                int hr = mediaControl.StopWhenReady();
                DsError.ThrowExceptionForHR(hr);
            }

            if (videoWindow != null)
            {
                videoWindow.put_Visible(OABool.False);
                videoWindow.put_Owner(IntPtr.Zero);
            }

            // Remove filter graph from the running object table.
            if (rot != null)
            {
                rot.Dispose();
                rot = null;
            }

            // Release DirectShow interfaces.
            Marshal.ReleaseComObject(this.mediaControl); this.mediaControl = null;
            Marshal.ReleaseComObject(this.videoWindow); this.videoWindow = null;
            Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null;
            Marshal.ReleaseComObject(this.captureGraphBuilder); this.captureGraphBuilder = null;
        }

        public void SetupVideoWindow()
        {
            int hr = 0;

            // Set the video window to be a child of the PictureBox
            hr = this.videoWindow.put_Owner(ImgSrcShow.Handle);
            DsError.ThrowExceptionForHR(hr);

            hr = this.videoWindow.put_WindowStyle(WindowStyle.Child);
            DsError.ThrowExceptionForHR(hr);

            // Make the video window visible, now that it is properly positioned
            hr = this.videoWindow.put_Visible(OABool.True);
            DsError.ThrowExceptionForHR(hr);

            // Set the video position
            Rectangle rc = ImgSrcShow.ClientRectangle;
            hr = videoWindow.SetWindowPosition(0, 0, rc.Width, rc.Height);
            //hr = videoWindow.SetWindowPosition(rc.Width, rc.Height, -rc.Width, -rc.Height);
            //hr = videoWindow.SetWindowPosition(0, 0, _previewWidth, _previewHeight);
            DsError.ThrowExceptionForHR(hr);
        }

        private void SetConfigParams(ICaptureGraphBuilder2 capGraph, IBaseFilter capFilter, int iFrameRate, int iWidth, int iHeight)
        {
            int hr;
            object config;
            AMMediaType mediaType;
            // Find the stream config interface
            hr = capGraph.FindInterface(
                PinCategory.Capture, MediaType.Video, capFilter, typeof(IAMStreamConfig).GUID, out config);

            IAMStreamConfig videoStreamConfig = config as IAMStreamConfig;
            if (videoStreamConfig == null)
            {
                throw new Exception("Failed to get IAMStreamConfig");
            }

            // Get the existing format block
            hr = videoStreamConfig.GetFormat(out mediaType);
            DsError.ThrowExceptionForHR(hr);

            // copy out the videoinfoheader
            VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
            Marshal.PtrToStructure(mediaType.formatPtr, videoInfoHeader);

            // if overriding the framerate, set the frame rate
            if (iFrameRate > 0)
            {
                videoInfoHeader.AvgTimePerFrame = 10000000 / iFrameRate;
            }

            // if overriding the width, set the width
            if (iWidth > 0)
            {
                videoInfoHeader.BmiHeader.Width = iWidth;
            }

            // if overriding the Height, set the Height
            if (iHeight > 0)
            {
                videoInfoHeader.BmiHeader.Height = iHeight;
            }

            // Copy the media structure back
            Marshal.StructureToPtr(videoInfoHeader, mediaType.formatPtr, false);

            // Set the new format
            hr = videoStreamConfig.SetFormat(mediaType);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(mediaType);
            mediaType = null;
        }

        private void SaveSizeInfo(ISampleGrabber sampleGrabber)
        {
            int hr;

            // Get the media type from the SampleGrabber
            AMMediaType media = new AMMediaType();
            hr = sampleGrabber.GetConnectedMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("Unknown Grabber Media Format");
            }

            // Grab the size info
            VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
            _previewStride = _previewWidth * (videoInfoHeader.BmiHeader.BitCount / 8);

            DsUtils.FreeAMMediaType(media);
            media = null;
        }

        private void ConfigureSampleGrabber(ISampleGrabber sampleGrabber)
        {
            AMMediaType media;
            int hr;

            // Set the media type to Video/RBG24
            media = new AMMediaType();
            media.majorType = MediaType.Video;
            media.subType = MediaSubType.RGB24;
            media.formatType = FormatType.VideoInfo;

            hr = sampleGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);
            media = null;

            hr = sampleGrabber.SetCallback(this, 1);
            DsError.ThrowExceptionForHR(hr);
        }
        #endregion

        private void CtrlCamera_Click(object sender, EventArgs e)
        {
            string button_text = CtrlCamera.Text;
            if (button_text.Equals("StartCamera"))
            {
                CtrlCamera.Text = "StopCamera";
                StartCamera();
            }
            else
            {
                CtrlCamera.Text = "StopCamera";
                StopCamera();
            }
        }
    }
}
