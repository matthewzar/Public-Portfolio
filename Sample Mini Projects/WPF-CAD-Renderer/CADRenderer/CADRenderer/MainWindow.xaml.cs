using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Drawing.Image;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Pen = System.Drawing.Pen;
using Rectangle = System.Drawing.Rectangle;

namespace CADRenderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CANVAS_WIDTH = 700;
        private const int CANVAS_HEIGHT = 600;

        public MainWindow()
        {
            InitializeComponent();
        }

        private static BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int xOffset, int yOffset)
        {
            var rect = new Rect(xOffset, yOffset, width - xOffset * 2, height - yOffset * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        private void DrawImageSourceToCanvas(ImageSource source)
        {
            imgPreviewImage.Source = source;
        }
        
        private void DisplayCSVToDataGrid(RectangleCollection source)
        {
            datagrid.ItemsSource = source.CADPoints;
        }

        private RectangleCollection processedRectangles;
        private void Btn_renderImage_Click(object sender, RoutedEventArgs e)
        {
            LoadAndDraw();
        }

        private void LoadAndDraw()
        {
            SetRendererSettingsFromUI();

            var csvToLoad = tb_importFileAddress.Text;
            processedRectangles = RectangleCollection.GetCollectionFromCSV(csvToLoad);

            DisplayCSVToDataGrid(processedRectangles);

            DrawCollection();
        }

        private void SetRendererSettingsFromUI()
        {
            RectangleCollection.PixelsPerPoint = int.Parse(tb_pixelsPerPoint.Text);
            RectangleCollection.Scaler = float.Parse(cb_zoomLevel.Text.Trim('%')) / 100;
            RectangleCollection.xShiftPercent = (float)(sb_xOffset.Value / 100);
            RectangleCollection.yShiftPercent = (float)(sb_yOffset.Value / 100);
        }

        private void Btn_restoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            sb_xOffset.Value = 0;
            sb_xOffset.Value = 0;
            cb_zoomLevel.SelectedIndex = 0;
            tb_pixelsPerPoint.Text = "10";
            LoadAndDraw();
        }

        private void DrawCollection()
        {
            SetRendererSettingsFromUI();
            var scalingFactor = 10f;

            var rectanglesToRender = processedRectangles.GetAllPositiveDimensionsClone(scalingFactor,
                invertXAxis: (bool)cb_invertXAxis.IsChecked,
                invertYAxis: (bool)cb_invertYAxis.IsChecked);

            var width = (int)(processedRectangles.width * scalingFactor) + 20;
            var height = (int)(processedRectangles.height * scalingFactor) + 20;
            var bitmapSource = ConvertRectanglesToBitmapSource(rectanglesToRender, width, height);

            DrawImageSourceToCanvas(CreateResizedImage(bitmapSource, CANVAS_WIDTH, CANVAS_HEIGHT, 0, 0));
        }

        private BitmapSource ConvertRectanglesToBitmapSource(RectangleF[] rectanglesToRender, int width, int height)
        {
            BitmapSource bitmapSource;
            using (var bitmap = new Bitmap(width, height))
            {
                using (Graphics gr = Graphics.FromImage(bitmap))
                {
                    gr.SmoothingMode = SmoothingMode.None;

                    //Add a big black background, then draw the sub-rectangles
                    gr.FillRectangle(System.Drawing.Brushes.Black, 0, 0, width, height);

                    //Draw ALL rectangles (even selected ones). This is more efficient that drawing each rectangle individually.
                    gr.FillRectangles(System.Drawing.Brushes.Green, rectanglesToRender);
                    
                    //Redraw the selected rectangle from the data-grid. Overwrites the green original value.
                    if(highlightPointIndex >= 0)
                        gr.FillRectangle(System.Drawing.Brushes.Yellow, rectanglesToRender[highlightPointIndex]);
                }

                bitmapSource = CreateBitmapSourceFromGdiBitmap(bitmap);
                SaveImageAsFile(bitmapSource, "testout.png");
            }

            return bitmapSource;
        }

        public static void SaveImageAsFile(BitmapSource source, string saveName)
        {
            using (var fileStream = new FileStream(saveName, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(fileStream);
            }
        }

        private void Tb_importFileAddress_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= Tb_importFileAddress_GotFocus;
        }

        private int highlightPointIndex = -1;
        private void Datagrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            highlightPointIndex = -1;
            var dg = sender as DataGrid;
            if (dg != null) highlightPointIndex = dg.SelectedIndex;

            DrawCollection();
        }
        
        private void ImgPreviewImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = Mouse.GetPosition(imgPreviewImage);
            Console.WriteLine(clickPoint);

            var result = processedRectangles.FindClosestPointIndexToCoordinates(
                (bool)cb_invertXAxis.IsChecked ? CANVAS_WIDTH - clickPoint.X + RectangleCollection.PixelsPerPoint : clickPoint.X + RectangleCollection.PixelsPerPoint + 10,
                (bool)cb_invertYAxis.IsChecked ? CANVAS_HEIGHT - clickPoint.Y + RectangleCollection.PixelsPerPoint : clickPoint.Y + RectangleCollection.PixelsPerPoint + 10,
                // (bool)cb_invertXAxis.IsChecked ? CANVAS_WIDTH - clickPoint.X + 10: clickPoint.X + 20,
                // (bool)cb_invertYAxis.IsChecked ? CANVAS_HEIGHT - clickPoint.Y + 10 : clickPoint.Y + 20, 
                CANVAS_WIDTH, CANVAS_HEIGHT);
            if (result.HasValue)
            {
                datagrid.SelectedIndex = result.Value;

                datagrid.ScrollIntoView(datagrid.SelectedItem);
                datagrid.Focus();

                DataGridRow row = datagrid.ItemContainerGenerator.ContainerFromIndex(result.Value) as DataGridRow;
                row.IsSelected = true;
                //row.Background = System.Windows.Media.Brushes.Red;
            }
            else datagrid.UnselectAll();
        }

    }
}
