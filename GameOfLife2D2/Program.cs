using System;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using Matrix;
using System.Threading.Tasks;

namespace GameOfLife
{
    internal static class Program
    {
        private const int MATRIX_SIZE_X = 1000;
        private const int MATRIX_SIZE_Y = 1000;
        public static Matrix2D<Cell> matrix;
        public static System.Drawing.Bitmap bmpWin = null;
        public static int LivePixels = 0;

        private static void IterateEnergyCalculations()
        {
            Parallel.For(0, matrix.XSize, x =>
            {
                for (int y = 0; y < matrix.YSize; y++)
                {
                    Cell pixel = matrix.GetNodeData(x, y);
                    List<Cell> neighbors = null;

                    if (pixel._neighborsCache == null)
                    {
                        neighbors = matrix.GetNeighbors(x, y);
                        pixel._neighborsCache = neighbors;
                    }
                    else
                    {
                        neighbors = pixel._neighborsCache;
                    }


                    int liveNeighborsCount = 0;


                    foreach (Cell _neighbor in neighbors) if (_neighbor.alive) liveNeighborsCount++;

                    if (pixel.alive)
                    {
                        if (liveNeighborsCount <= 1) pixel.die = true;
                        else if (liveNeighborsCount <= 3) pixel.die = false;
                        else if (liveNeighborsCount >= 4) pixel.die = true;
                    }
                    else if (liveNeighborsCount == 3)
                    {
                        pixel.die = false;
                        pixel.born = true;
                    }
                }
            });
        }

        private static void FinalizeEnergyCalculations()
        {
            LivePixels = 0;
            Parallel.For(0, matrix.XSize, x =>
           {
               for (int y = 0; y < matrix.YSize; y++)
               {
                   Cell pixel = matrix.GetNodeData(x, y);
                   pixel.UpdateTime();
                   LivePixels += pixel.alive == true ? 1 : 0;
               }
           });
        }


        [STAThread]
        private static void Main()
        {
            matrix = new Matrix2D<Cell>(Program.MATRIX_SIZE_X, Program.MATRIX_SIZE_Y);
            Parallel.For(0, matrix.XSize, x =>
           {
               for (int y = 0; y < matrix.YSize; y++)
               {
                   matrix.SetNodeData(new Cell(false), x, y);
               }
           });


            var form = new RenderForm("Game Of Life");
            form.Size = new System.Drawing.Size(MATRIX_SIZE_X, MATRIX_SIZE_Y);
            form.Icon = null;
            form.KeyDown += Form_KeyDown;

            SharpDxHelper sharpDxHelper = new SharpDxHelper();
            RenderTarget d2dRenderTarget = sharpDxHelper.CreateRenderTarget(form);
            SwapChain swapChain = sharpDxHelper.swapChain;
            var solidColorBrush = new SolidColorBrush(d2dRenderTarget, Color.White);
            Stopwatch stopwatch = new Stopwatch();

            d2dRenderTarget.AntialiasMode = AntialiasMode.Aliased;

            var _memory = new byte[MATRIX_SIZE_X * MATRIX_SIZE_Y * 4];
            var _backBufferBmp = new Bitmap(d2dRenderTarget, new Size2(MATRIX_SIZE_X, MATRIX_SIZE_Y), new BitmapProperties(d2dRenderTarget.PixelFormat));

            RenderLoop.Run(form, () =>
            {
                stopwatch.Restart();
                IterateEnergyCalculations();

                d2dRenderTarget.BeginDraw();

                stopwatch.Restart();
                Parallel.For(0, MATRIX_SIZE_X, x =>
                {
                    for (int y = 0; y < MATRIX_SIZE_Y; y++)
                    {
                        Color color = matrix.GetNodeData(x, y).ConvertToColor32();
                        var i = MATRIX_SIZE_X * 4 * y + x * 4;
                        _memory[i] = color.B;
                        _memory[i + 1] = color.G;
                        _memory[i + 2] = color.R;
                        _memory[i + 3] = color.A;
                    }
                });

                _backBufferBmp.CopyFromMemory(_memory, MATRIX_SIZE_X * 4);
                d2dRenderTarget.DrawBitmap(_backBufferBmp, 1f, BitmapInterpolationMode.Linear);

                d2dRenderTarget.EndDraw();
                swapChain.Present(0, PresentFlags.None);

                FinalizeEnergyCalculations();

                long calculationTime = stopwatch.ElapsedMilliseconds;
                form.Text = "Calculation Time: " + calculationTime.ToString() +"ms Live Cells: " + LivePixels.ToString();
            });


            sharpDxHelper.Destroy();
        }

        private static void Form_KeyDown(object sender, KeyEventArgs e)
        {
            Random rand = new Random();
            for (int x = 0; x < matrix.XSize; x++)
            {
                for (int y = 0; y < matrix.YSize; y++)
                {
                    Cell pixel = matrix.GetNodeData(x, y);
                    pixel.alive = rand.Next(0, 2) == 1 ? true : false;
                }
            }
        }
    }
}